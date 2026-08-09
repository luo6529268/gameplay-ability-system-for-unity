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

        public int PreparedCueCountForDiagnostics => preparedCues.Count;
        public long PreparedCueBuildCountForDiagnostics => preparedCueBuildCount;
        public int OneShotVoiceCountForDiagnostics => oneShotVoices.Length;
        public long PooledOneShotPlayCountForDiagnostics => pooledOneShotPlayCount;
        public long OneShotVoiceLimitDropCountForDiagnostics => oneShotVoiceLimitDropCount;
        public long CoalescedLoadRequestCountForDiagnostics => coalescedLoadRequestCount;
        public long LoopFallbackPlayCountForDiagnostics => loopFallbackPlayCount;

        private void Start()
        {
            EnsureOneShotVoicePool();
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
                preparedCues.Clear();
                preparedAudioController = controller;
            }

            if (preparedCues.TryGetValue(soundId, out PreparedSoundCue preparedCue))
                return preparedCue;

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

            MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
            options.Location = playbackPosition;
            options.AttachToTransform = attachTarget;
            options.Loop = audioItem.loop;
            options.Volume = Mathf.Clamp(audioItem.volume + UnityEngine.Random.Range(-audioItem.randomVolume, audioItem.randomVolume), 0f, 2f);
            options.Pitch = Mathf.Clamp(1f + UnityEngine.Random.Range(-audioItem.randomPitch, audioItem.randomPitch), -3f, 3f);
            options.SpatialBlend = audioItem.range > 0f ? 1f : 0f;
            options.RolloffMode = AudioRolloffMode.Custom;

            if (audioItem.range > 0f)
            {
                options.MaxDistance = audioItem.range;
                options.MinDistance = audioItem.range > 3f ? audioItem.range - 3f : 0f;
            }

            if (audioItem.loop)
            {
                loopFallbackPlayCount++;
                MMSoundManager.Instance?.PlaySound(clip, options);
                return;
            }

            MMSoundManager soundManager = MMSoundManager.Instance;
            if (soundManager == null)
            {
                return;
            }

            AudioSource voice = AcquireOneShotVoice(out int voiceIndex);
            if (voice == null)
            {
                oneShotVoiceLimitDropCount++;
                return;
            }

            options.RecycleAudioSource = voice;
            AudioSource playingVoice = soundManager.PlaySound(clip, options);
            if (playingVoice == null)
            {
                return;
            }

            float absolutePitch = Mathf.Max(0.01f, Mathf.Abs(options.Pitch));
            oneShotVoiceAvailableDspTimes[voiceIndex] =
                AudioSettings.dspTime + clip.length / absolutePitch;
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

        private static Transform ResolveListenerTransform()
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

            // 单文件模式：soundId 以音频扩展名结尾（如 data\003.wav）
            if (preparedCue.IsSingleFile)
            {
                preparedCue.Clips[0] = await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(
                    preparedCue.CacheKey,
                    preparedCue.SourcePath);
                preparedCue.IsLoaded = true;
                return;
            }

            // 目录模式：soundId 是目录路径，加载目录下所有音频文件
            AudioClip[] loadedClips = await NTSD_ResourceLoader.Instance.LoadAudioClipsAsync(
                preparedCue.CacheKey,
                preparedCue.SourcePath);
            if (loadedClips != null && loadedClips.Length > 0)
            {
                preparedCue.Clips = loadedClips;
            }
            else
            {
                preparedCue.Clips = preparedCue.AudioItem?.clip ?? Array.Empty<AudioClip>();
            }
            preparedCue.IsLoaded = true;
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
