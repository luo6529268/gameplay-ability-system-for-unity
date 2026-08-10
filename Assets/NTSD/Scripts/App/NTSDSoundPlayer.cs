using System;
using System.Collections.Generic;
using System.IO;
using BeatEmUpTemplate2D;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Load;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.App
{
    public sealed class NTSDSoundPlayer : MonoBehaviour, ISimulationSoundPresentationSink
    {
        [SerializeField] private string soundRootFolder = "NTSD/Sound";
        [SerializeField, Min(1)] private int desktopOneShotVoiceLimit = 48;
        [SerializeField, Min(1)] private int mobileOneShotVoiceLimit = 24;

        private static readonly string[] BuiltInBattleSoundIds =
        {
            "SFX_001",
            "SFX_002",
            "SFX_004",
            "SFX_006",
            "SFX_010",
            "SFX_011",
            "SFX_017",
            "SFX_032",
            "SFX_033",
            "SFX_039",
            "SFX_065",
            "SFX_066",
            "SFX_068",
        };

        private sealed class PreparedSoundCue
        {
            public AudioItem AudioItem;
            public string CacheKey;
            public string SourcePath;
            public bool IsSingleFile;
            public bool IsLoaded;
            public bool IsLoading;
            public AudioClip[] Clips;
        }

        private readonly Dictionary<string, PreparedSoundCue> preparedCues =
            new Dictionary<string, PreparedSoundCue>(StringComparer.Ordinal);
        private AudioController preparedAudioController;
        private long preparedCueBuildCount;
        private AudioSource[] oneShotVoices = Array.Empty<AudioSource>();
        private MMFollowTarget[] oneShotVoiceFollowers = Array.Empty<MMFollowTarget>();
        private double[] oneShotVoiceAvailableDspTimes = Array.Empty<double>();
        private int nextOneShotVoiceIndex;
        private long pooledOneShotPlayCount;
        private long oneShotVoiceLimitDropCount;
        private long coalescedLoadRequestCount;
        private long loopFallbackPlayCount;
        private bool battleCatalogSealed;
        private long rejectedUnpreparedCueCount;
        private long failedPreparedCueLoadCount;
        private Transform cachedListenerTransform;
        private UnityEngine.Audio.AudioMixerGroup cachedSfxMixerGroup;

        public int PreparedCueCountForDiagnostics => preparedCues.Count;
        public long PreparedCueBuildCountForDiagnostics => preparedCueBuildCount;
        public int OneShotVoiceCountForDiagnostics => oneShotVoices.Length;
        public long PooledOneShotPlayCountForDiagnostics => pooledOneShotPlayCount;
        public long OneShotVoiceLimitDropCountForDiagnostics => oneShotVoiceLimitDropCount;
        public long CoalescedLoadRequestCountForDiagnostics => coalescedLoadRequestCount;
        public long LoopFallbackPlayCountForDiagnostics => loopFallbackPlayCount;
        public bool BattleCatalogSealedForDiagnostics => battleCatalogSealed;
        public long RejectedUnpreparedCueCountForDiagnostics =>
            rejectedUnpreparedCueCount;
        public long FailedPreparedCueLoadCountForDiagnostics =>
            failedPreparedCueLoadCount;

        private void Awake()
        {
            PrepareBattlePresentationHotPath();
        }

        private void Start()
        {
            EnsureOneShotVoicePool();
        }

        internal void PrepareBattlePresentationHotPath()
        {
            EnsureOneShotVoicePool();
            cachedListenerTransform = ResolveListenerTransformUncached();
            MMSoundManager soundManager = MMSoundManager.Instance;
            cachedSfxMixerGroup =
                soundManager != null && soundManager.settingsSo != null
                    ? soundManager.settingsSo.SfxAudioMixerGroup
                    : null;
        }

        public async UniTask PrepareBattleCuesAsync(
            CharacterAnimtorManager characterManager)
        {
            battleCatalogSealed = false;
            PrepareBattlePresentationHotPath();

            AudioController controller = AudioController.Instance;
            if (!ReferenceEquals(controller, preparedAudioController))
            {
                preparedCues.Clear();
                preparedAudioController = controller;
            }

            var soundIds = new HashSet<string>(StringComparer.Ordinal);
            characterManager?.CollectBattleSoundIds(soundIds);
            for (int index = 0; index < BuiltInBattleSoundIds.Length; index++)
                soundIds.Add(BuiltInBattleSoundIds[index]);

            preparedCues.EnsureCapacity(soundIds.Count);
            foreach (string soundId in soundIds)
            {
                PreparedSoundCue preparedCue = GetOrPrepareCue(soundId);
                if (preparedCue == null || preparedCue.IsLoaded)
                    continue;

                preparedCue.IsLoading = true;
                try
                {
                    await LoadPreparedClipsAsync(preparedCue);
                }
                finally
                {
                    preparedCue.IsLoading = false;
                }
            }

            battleCatalogSealed = true;
        }

        public void PlaySfx(string soundId, Vector3? position = null, Transform parent = null)
        {
            PlaySfx(soundId, position, parent, ResolveListenerTransform());
        }

        private void PlaySfx(
            string soundId,
            Vector3? position,
            Transform parent,
            Transform listenerTransform)
        {
            PreparedSoundCue preparedCue = GetOrPrepareCue(soundId);
            if (preparedCue == null)
                return;

            if (preparedCue.IsLoaded)
            {
                PlayPreparedCue(preparedCue, position, parent, listenerTransform);
                return;
            }

            if (preparedCue.IsLoading)
            {
                coalescedLoadRequestCount++;
                return;
            }

            preparedCue.IsLoading = true;
            LoadAndPlayPreparedCueAsync(
                preparedCue,
                position,
                parent,
                listenerTransform).Forget();
        }

        public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
        {
            if (sounds == null)
                return;

            Transform listenerTransform = ResolveListenerTransform();
            for (int i = 0; i < sounds.Count; i++)
                PresentSound(sounds[i], listenerTransform);
        }

        public void PresentSound(PendingSoundEvent sound)
        {
            PresentSound(sound, ResolveListenerTransform());
        }

        private void PresentSound(PendingSoundEvent sound, Transform listenerTransform)
        {
            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld(sound.WorldX, 0f);
            PlaySfx(
                sound.Cue,
                new Vector3(groundPoint.x, groundPoint.y, 0f),
                null,
                listenerTransform);
        }

        public bool TryGetPreparedSingleFileWrapperForDiagnostics(
            string soundId,
            out AudioClip[] clips)
        {
            PreparedSoundCue preparedCue = GetOrPrepareCue(soundId);
            if (preparedCue == null || !preparedCue.IsSingleFile)
            {
                clips = null;
                return false;
            }

            clips = preparedCue.Clips;
            return true;
        }

        private PreparedSoundCue GetOrPrepareCue(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return null;

            AudioController controller = AudioController.Instance;
            if (!ReferenceEquals(controller, preparedAudioController))
            {
                if (battleCatalogSealed)
                {
                    rejectedUnpreparedCueCount++;
                    return null;
                }
                preparedCues.Clear();
                preparedAudioController = controller;
            }

            if (preparedCues.TryGetValue(soundId, out PreparedSoundCue preparedCue))
                return preparedCue;

            if (battleCatalogSealed)
            {
                rejectedUnpreparedCueCount++;
                return null;
            }

            AudioItem audioItem = FindAudioItem(controller, soundId) ??
                                  CreateFallbackAudioItem(soundId);
            string relativeFolder = ResolveRelativeFolder(soundId, audioItem);
            string normalizedRelativeFolder = NormalizeRelativeFolder(relativeFolder);
            bool isSingleFile = IsSingleFilePath(normalizedRelativeFolder);
            preparedCue = new PreparedSoundCue
            {
                AudioItem = audioItem,
                IsSingleFile = isSingleFile,
                CacheKey = isSingleFile
                    ? $"NTSD.AudioFile::{normalizedRelativeFolder}"
                    : $"NTSD.Audio::{normalizedRelativeFolder}",
                SourcePath = Path.Combine(
                    Application.dataPath,
                    soundRootFolder,
                    normalizedRelativeFolder),
                Clips = isSingleFile ? new AudioClip[1] : null,
            };
            preparedCues.Add(soundId, preparedCue);
            preparedCueBuildCount++;
            return preparedCue;
        }

        private async UniTaskVoid LoadAndPlayPreparedCueAsync(
            PreparedSoundCue preparedCue,
            Vector3? position,
            Transform parent,
            Transform listenerTransform)
        {
            try
            {
                await LoadPreparedClipsAsync(preparedCue);
                PlayPreparedCue(preparedCue, position, parent, listenerTransform);
            }
            finally
            {
                preparedCue.IsLoading = false;
            }
        }

        private void PlayPreparedCue(
            PreparedSoundCue preparedCue,
            Vector3? position,
            Transform parent,
            Transform listenerTransform)
        {
            AudioItem audioItem = preparedCue.AudioItem;

            AudioClip clip = PickClip(preparedCue.Clips);
            if (clip == null)
            {
                return;
            }

            if (Time.time - audioItem.lastTimePlayed < audioItem.minTimeBetweenCall)
            {
                return;
            }

            audioItem.lastTimePlayed = Time.time;

            Vector3 playbackPosition = position ?? (listenerTransform != null ? listenerTransform.position : Vector3.zero);
            Transform attachTarget = audioItem.range > 0f ? parent : null;
            float volume = Mathf.Clamp(
                audioItem.volume +
                UnityEngine.Random.Range(
                    -audioItem.randomVolume,
                    audioItem.randomVolume),
                0f,
                2f);
            float pitch = Mathf.Clamp(
                1f +
                UnityEngine.Random.Range(
                    -audioItem.randomPitch,
                    audioItem.randomPitch),
                -3f,
                3f);

            AudioSource voice = AcquireOneShotVoice(out int voiceIndex);
            if (voice == null)
            {
                oneShotVoiceLimitDropCount++;
                return;
            }

            MMFollowTarget follower = oneShotVoiceFollowers[voiceIndex];
            voice.Stop();
            voice.transform.position = playbackPosition;
            voice.clip = clip;
            voice.pitch = pitch;
            voice.volume = volume;
            voice.spatialBlend = audioItem.range > 0f ? 1f : 0f;
            voice.rolloffMode = AudioRolloffMode.Custom;
            voice.minDistance = audioItem.range > 3f
                ? audioItem.range - 3f
                : 0f;
            voice.maxDistance = audioItem.range > 0f ? audioItem.range : 500f;
            voice.loop = audioItem.loop;
            voice.panStereo = 0f;
            voice.bypassEffects = false;
            voice.bypassListenerEffects = false;
            voice.bypassReverbZones = false;
            voice.priority = 128;
            voice.reverbZoneMix = 1f;
            voice.dopplerLevel = 1f;
            voice.spread = 0f;
            voice.time = 0f;

            voice.outputAudioMixerGroup = cachedSfxMixerGroup;

            if (follower != null && attachTarget != null)
            {
                follower.Target = attachTarget;
                follower.enabled = true;
            }

            voice.Play();
            float absolutePitch = Mathf.Max(0.01f, Mathf.Abs(pitch));
            oneShotVoiceAvailableDspTimes[voiceIndex] =
                audioItem.loop
                    ? double.PositiveInfinity
                    : AudioSettings.dspTime + clip.length / absolutePitch;
            if (audioItem.loop)
                loopFallbackPlayCount++;
            else
                pooledOneShotPlayCount++;
        }

        private AudioSource AcquireOneShotVoice(out int voiceIndex)
        {
            EnsureOneShotVoicePool();
            voiceIndex = -1;
            int voiceCount = oneShotVoices.Length;
            double currentDspTime = AudioSettings.dspTime;
            for (int offset = 0; offset < voiceCount; offset++)
            {
                int index = (nextOneShotVoiceIndex + offset) % voiceCount;
                AudioSource voice = oneShotVoices[index];
                if (voice == null || oneShotVoiceAvailableDspTimes[index] > currentDspTime)
                {
                    continue;
                }

                MMFollowTarget follower = oneShotVoiceFollowers[index];
                if (follower != null)
                {
                    follower.Target = null;
                    follower.enabled = false;
                }

                voice.transform.SetParent(transform, false);
                nextOneShotVoiceIndex = (index + 1) % voiceCount;
                voiceIndex = index;
                return voice;
            }

            return null;
        }

        private void EnsureOneShotVoicePool()
        {
            int voiceLimit = ResolveOneShotVoiceLimit();
            if (oneShotVoices.Length == voiceLimit)
            {
                return;
            }

            oneShotVoices = new AudioSource[voiceLimit];
            oneShotVoiceFollowers = new MMFollowTarget[voiceLimit];
            oneShotVoiceAvailableDspTimes = new double[voiceLimit];
            nextOneShotVoiceIndex = 0;
            for (int i = 0; i < voiceLimit; i++)
            {
                var voiceHost = new GameObject($"NTSD SFX Voice {i}");
                voiceHost.transform.SetParent(transform, false);

                AudioSource voice = voiceHost.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.loop = false;

                MMFollowTarget follower = voiceHost.AddComponent<MMFollowTarget>();
                follower.Target = null;
                follower.InterpolatePosition = false;
                follower.InterpolateRotation = false;
                follower.InterpolateScale = false;
                follower.FollowRotation = false;
                follower.FollowScale = false;
                follower.enabled = false;

                oneShotVoices[i] = voice;
                oneShotVoiceFollowers[i] = follower;
            }
        }

        private int ResolveOneShotVoiceLimit()
        {
#if UNITY_ANDROID || UNITY_IOS
            return Mathf.Max(1, mobileOneShotVoiceLimit);
#else
            return Mathf.Max(1, desktopOneShotVoiceLimit);
#endif
        }

        private Transform ResolveListenerTransform()
        {
            if (cachedListenerTransform == null)
                cachedListenerTransform = ResolveListenerTransformUncached();
            return cachedListenerTransform;
        }

        private static Transform ResolveListenerTransformUncached()
        {
            Camera listenerCamera = Camera.main;
            return listenerCamera != null ? listenerCamera.transform : null;
        }

        private static AudioItem FindAudioItem(AudioController controller, string soundId)
        {
            if (controller == null || controller.AudioList == null)
            {
                return null;
            }

            foreach (AudioItem audioItem in controller.AudioList)
            {
                if (audioItem != null && audioItem.name == soundId)
                {
                    return audioItem;
                }
            }

            return null;
        }

        private async UniTask LoadPreparedClipsAsync(PreparedSoundCue preparedCue)
        {
            if (preparedCue.IsLoaded)
                return;

            try
            {
                // 单文件模式：soundId 以音频扩展名结尾（如 data\003.wav）
                if (preparedCue.IsSingleFile)
                {
                    preparedCue.Clips[0] =
                        await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(
                            preparedCue.CacheKey,
                            preparedCue.SourcePath);
                    return;
                }

                // 目录模式：soundId 是目录路径，加载目录下所有音频文件
                AudioClip[] loadedClips =
                    await NTSD_ResourceLoader.Instance.LoadAudioClipsAsync(
                        preparedCue.CacheKey,
                        preparedCue.SourcePath);
                if (loadedClips != null && loadedClips.Length > 0)
                {
                    preparedCue.Clips = loadedClips;
                }
                else
                {
                    UseConfiguredClipOrEmpty(preparedCue);
                }
            }
            catch (Exception ex)
            {
                failedPreparedCueLoadCount++;
                UseConfiguredClipOrEmpty(preparedCue);
                Debug.LogWarning(
                    $"[NTSDSoundPlayer] Battle cue prewarm failed and was sealed without a streamed clip: " +
                    $"{preparedCue.AudioItem?.name ?? preparedCue.SourcePath}; {ex.Message}");
            }
            finally
            {
                preparedCue.IsLoaded = true;
            }
        }

        private static void UseConfiguredClipOrEmpty(PreparedSoundCue preparedCue)
        {
            AudioClip[] configuredClips = preparedCue.AudioItem?.clip;
            preparedCue.Clips = configuredClips ?? Array.Empty<AudioClip>();
        }

        private static bool IsSingleFilePath(string path)
        {
            string ext = Path.GetExtension(path);
            return string.Equals(ext, ".wav", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".ogg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".mp3", StringComparison.OrdinalIgnoreCase);
        }

        private AudioClip PickClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            int startIndex = UnityEngine.Random.Range(0, clips.Length);
            for (int i = 0; i < clips.Length; i++)
            {
                AudioClip clip = clips[(startIndex + i) % clips.Length];
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private AudioItem CreateFallbackAudioItem(string soundId)
        {
            return new AudioItem
            {
                name = soundId,
                volume = 1f,
                randomVolume = 0f,
                randomPitch = 0f,
                minTimeBetweenCall = 0f,
                range = 0f,
                loop = false,
                streamingFolder = soundId
            };
        }

        private string ResolveRelativeFolder(string soundId, AudioItem audioItem)
        {
            if (audioItem != null && !string.IsNullOrWhiteSpace(audioItem.streamingFolder))
            {
                return audioItem.streamingFolder;
            }

            return soundId;
        }

        private string NormalizeRelativeFolder(string relativeFolder)
        {
            string normalized = relativeFolder.Replace('\\', '/').Trim('/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            string[] parts = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Replace(':', '_');
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }
    }
}
