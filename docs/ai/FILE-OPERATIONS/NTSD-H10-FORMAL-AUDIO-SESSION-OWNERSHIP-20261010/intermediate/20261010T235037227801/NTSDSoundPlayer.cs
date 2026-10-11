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
        private const int NativeBattleVoiceLimit = 64;
        [SerializeField] private string soundRootFolder = "NTSD/Sound";
        [SerializeField, Min(1)] private int desktopOneShotVoiceLimit = 48;
        [SerializeField, Min(1)] private int mobileOneShotVoiceLimit = 24;

        private static readonly string[] BuiltInBattleSoundIds =
        {
            "SFX_001",
            "SFX_002",
            "SFX_004",
            "SFX_006",
            @"data\016.wav",
            "SFX_016",
            "SFX_010",
            "SFX_011",
            "SFX_017",
            "SFX_020",
            "SFX_021",
            "SFX_025",
            "SFX_032",
            "SFX_033",
            "SFX_039",
            "SFX_065",
            "SFX_066",
            "SFX_068",
            "SFX_085",
        };

        private sealed class PreparedSoundCue
        {
            public AudioItem AudioItem;
            public string CacheKey;
            public string SourcePath;
            public bool IsSingleFile;
            public bool IsLoaded;
            public bool IsLoading;
            public bool IsFormalBattleFile;
            public int CatalogGeneration;
            public string FormalIdentity;
            public AudioClip OwnedSource;
            public AudioClip[] Clips;
            public AudioClip[] BattlePlaybackClips;
        }

        private readonly Dictionary<string, PreparedSoundCue> preparedCues =
            new Dictionary<string, PreparedSoundCue>(StringComparer.Ordinal);
        private readonly Dictionary<string, PreparedSoundCue> preparedFormalBattleCues =
            new Dictionary<string, PreparedSoundCue>(StringComparer.OrdinalIgnoreCase);
        private AudioController preparedAudioController;
        private int preparedCatalogGeneration;
        private readonly List<AudioClip> ownedBattlePlaybackClips = new List<AudioClip>();
        private readonly List<AudioClip> ownedFormalAudioSources = new List<AudioClip>();
        private int battlePrepareRequest;
        private int pendingFormalAudioLoads;
        private bool battleAudioPreparationInProgress;
        private bool battleSoundIntakeClosed;
        private bool battleSoundShutdownCompleted;
        private bool soundOwnerDisposed;
        private long formalAudioLoadCount;
        private long staleFormalAudioReleaseCount;
        private long preparedCueBuildCount;
        private AudioSource[] oneShotVoices = Array.Empty<AudioSource>();
        private MMFollowTarget[] oneShotVoiceFollowers = Array.Empty<MMFollowTarget>();
        private double[] oneShotVoiceAvailableDspTimes = Array.Empty<double>();
        private float[] oneShotVoiceBaseVolumes = Array.Empty<float>();
        private long[] oneShotVoiceStartSequences = Array.Empty<long>();
        private string[] battleVoiceIdentities = Array.Empty<string>();
        private int[] battleVoicePanHundredthDb = Array.Empty<int>();
        private long nextOneShotVoiceStartSequence;
        private int nativeBattleVolumePercent = 100;
        private float nativeBattleSfxGain = 1f;
        private int nextOneShotVoiceIndex;
        private long pooledOneShotPlayCount;
        private long oneShotVoiceLimitDropCount;
        private long coalescedLoadRequestCount;
        private long loopFallbackPlayCount;
        private bool battleCatalogSealed;
        private long rejectedUnpreparedCueCount;
        private long failedPreparedCueLoadCount;
        private long skippedMissingBattleCueFileCount;
        private Transform cachedListenerTransform;
        private UnityEngine.Audio.AudioMixerGroup cachedSfxMixerGroup;

        public int PreparedCueCountForDiagnostics =>
            preparedCues.Count + preparedFormalBattleCues.Count;
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
        public long SkippedMissingBattleCueFileCountForDiagnostics =>
            skippedMissingBattleCueFileCount;
        public int NativeBattleVolumePercentForDiagnostics => nativeBattleVolumePercent;
        public int PendingFormalAudioLoadsForDiagnostics => pendingFormalAudioLoads;
        public int OwnedFormalAudioSourcesForDiagnostics => ownedFormalAudioSources.Count;
        public long FormalAudioLoadCountForDiagnostics => formalAudioLoadCount;
        public long StaleFormalAudioReleaseCountForDiagnostics => staleFormalAudioReleaseCount;
        public bool BattleSoundShutdownCompletedForDiagnostics => battleSoundShutdownCompleted;
        public bool BattleAudioPreparationInProgressForDiagnostics => battleAudioPreparationInProgress;
        public bool BattleAudioTailDrainedForDiagnostics =>
            !battleAudioPreparationInProgress && pendingFormalAudioLoads == 0;

        internal void ApplyNativeBattleVolumeHostTick(
            NTSD28NativeFunctionKeyHostCommand command)
        {
            int delta = command == NTSD28NativeFunctionKeyHostCommand.VolumeDown
                ? -1
                : command == NTSD28NativeFunctionKeyHostCommand.VolumeUp
                    ? 1
                    : 0;
            if (delta == 0)
                return;

            int nextPercent = Mathf.Clamp(nativeBattleVolumePercent + delta, 0, 100);
            if (nextPercent == nativeBattleVolumePercent)
                return;

            nativeBattleVolumePercent = nextPercent;
            nativeBattleSfxGain = ComputeNativeBattleSfxGain(nextPercent);
            for (int index = 0; index < oneShotVoices.Length; index++)
            {
                AudioSource voice = oneShotVoices[index];
                if (voice != null)
                    voice.volume = oneShotVoiceBaseVolumes[index] * nativeBattleSfxGain;
            }
        }

        private static float ComputeNativeBattleSfxGain(int volumePercent)
        {
            if (volumePercent == 0)
                return 0f;
            int hundredthDb = ((volumePercent - 100) * 0xED8) / 100;
            return Mathf.Pow(10f, hundredthDb / 2000f);
        }

        private void Awake()
        {
            PrepareBattlePresentationHotPath();
        }

        private void Start()
        {
            EnsureOneShotVoicePool();
        }

        private void OnDestroy()
        {
            soundOwnerDisposed = true;
            BeginBattleSoundShutdown();
            CompleteBattleSoundShutdown();
        }

        internal void BeginBattleSoundShutdown()
        {
            battleSoundIntakeClosed = true;
            battlePrepareRequest++;
            preparedCatalogGeneration++;
        }

        internal void CompleteBattleSoundShutdown()
        {
            if (!battleSoundIntakeClosed)
                BeginBattleSoundShutdown();
            ReleaseBattlePlaybackClips();
            battleCatalogSealed = false;
            battleSoundShutdownCompleted = true;
        }

        private static void DestroyOwnedAudioClip(AudioClip clip)
        {
            if (clip == null)
                return;
            if (Application.isPlaying)
                Destroy(clip);
            else
                DestroyImmediate(clip);
        }

        private void ReleaseOwnedFormalSource(PreparedSoundCue cue)
        {
            AudioClip source = cue.OwnedSource;
            cue.OwnedSource = null;
            if (ReferenceEquals(source, null))
                return;
            ownedFormalAudioSources.Remove(source);
            DestroyOwnedAudioClip(source);
        }

        private void ReleaseBattlePlaybackClips()
        {
            preparedCatalogGeneration++;
            for (int index = 0; index < oneShotVoices.Length; index++)
            {
                AudioSource voice = oneShotVoices[index];
                if (voice == null ||
                    (string.IsNullOrEmpty(battleVoiceIdentities[index]) &&
                     !ownedBattlePlaybackClips.Contains(voice.clip) &&
                     !ownedFormalAudioSources.Contains(voice.clip)))
                    continue;
                voice.Stop();
                voice.clip = null;
                battleVoiceIdentities[index] = null;
                oneShotVoiceAvailableDspTimes[index] = 0;
                oneShotVoiceStartSequences[index] = 0;
                MMFollowTarget follower = oneShotVoiceFollowers[index];
                if (follower != null)
                {
                    follower.Target = null;
                    follower.enabled = false;
                }
            }
            foreach (AudioClip clip in ownedBattlePlaybackClips)
                DestroyOwnedAudioClip(clip);
            ownedBattlePlaybackClips.Clear();
            foreach (AudioClip clip in ownedFormalAudioSources)
                DestroyOwnedAudioClip(clip);
            ownedFormalAudioSources.Clear();
            foreach (PreparedSoundCue cue in preparedFormalBattleCues.Values)
            {
                cue.OwnedSource = null;
                cue.Clips = Array.Empty<AudioClip>();
                cue.BattlePlaybackClips = null;
                cue.IsLoaded = false;
            }
            preparedFormalBattleCues.Clear();
            foreach (PreparedSoundCue cue in preparedCues.Values)
                cue.BattlePlaybackClips = null;
        }

        private bool IsCurrentFormalCue(PreparedSoundCue cue)
        {
            return this != null && !soundOwnerDisposed && !battleSoundIntakeClosed &&
                cue.CatalogGeneration == preparedCatalogGeneration &&
                ReferenceEquals(AudioController.Instance, preparedAudioController) &&
                cue.FormalIdentity != null &&
                preparedFormalBattleCues.TryGetValue(cue.FormalIdentity, out PreparedSoundCue current) &&
                ReferenceEquals(current, cue);
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
            if (this == null || soundOwnerDisposed)
                return;
            BeginBattleSoundShutdown();
            int request = battlePrepareRequest;
            while (battleAudioPreparationInProgress || pendingFormalAudioLoads != 0)
            {
                await UniTask.Yield();
                if (this == null || soundOwnerDisposed || request != battlePrepareRequest)
                    return;
            }
            // Old local results have drained; let Player deferred Destroy finish before re-entry.
            if (Application.isPlaying)
                await UniTask.NextFrame();
            if (this == null || soundOwnerDisposed || request != battlePrepareRequest)
                return;

            CompleteBattleSoundShutdown();
            battleSoundShutdownCompleted = false;
            battleSoundIntakeClosed = false;
            battleAudioPreparationInProgress = true;
            try
            {
                await PrepareBattleCuesCoreAsync(characterManager);
            }
            finally
            {
                if (!battleCatalogSealed && request == battlePrepareRequest)
                {
                    BeginBattleSoundShutdown();
                    CompleteBattleSoundShutdown();
                }
                battleAudioPreparationInProgress = false;
            }
        }

        private async UniTask PrepareBattleCuesCoreAsync(
            CharacterAnimtorManager characterManager)
        {
            battleCatalogSealed = false;
            PrepareBattlePresentationHotPath();
            EnsureOneShotVoiceCapacity(NativeBattleVoiceLimit);
            long skippedMissingBefore = skippedMissingBattleCueFileCount;

            AudioController controller = AudioController.Instance;
            if (!ReferenceEquals(controller, preparedAudioController))
            {
                preparedCues.Clear();
                preparedAudioController = controller;
            }

            int generation = preparedCatalogGeneration;

            var soundIds = new HashSet<string>(StringComparer.Ordinal);
            characterManager?.CollectBattleSoundIds(soundIds);
            for (int index = 0; index < BuiltInBattleSoundIds.Length; index++)
                soundIds.Add(BuiltInBattleSoundIds[index]);

            preparedCues.EnsureCapacity(soundIds.Count);
            if (ownedFormalAudioSources.Capacity < soundIds.Count)
                ownedFormalAudioSources.Capacity = soundIds.Count;
            foreach (string soundId in soundIds)
            {
                PreparedSoundCue battleCue = GetOrPrepareCue(soundId, true);
                await EnsurePreparedCueLoadedAsync(battleCue);
                if (this == null || soundOwnerDisposed || battleSoundIntakeClosed ||
                    generation != preparedCatalogGeneration ||
                    !ReferenceEquals(AudioController.Instance, preparedAudioController))
                    return;
                PrepareBattlePlaybackClips(battleCue);
                if (battleCue != null && battleCue.IsFormalBattleFile)
                {
                    PreparedSoundCue genericCue = GetOrPrepareCue(soundId);
                    if (genericCue != null && (genericCue.IsSingleFile
                        ? File.Exists(genericCue.SourcePath)
                        : Directory.Exists(genericCue.SourcePath)))
                    {
                        await EnsurePreparedCueLoadedAsync(genericCue);
                        if (this == null || soundOwnerDisposed || battleSoundIntakeClosed ||
                    generation != preparedCatalogGeneration ||
                    !ReferenceEquals(AudioController.Instance, preparedAudioController))
                            return;
                    }
                }
            }

            long skippedMissing = skippedMissingBattleCueFileCount -
                skippedMissingBefore;
            if (skippedMissing > 0)
            {
                Debug.LogWarning(
                    $"[NTSDSoundPlayer] Battle cue prewarm used configured fallback for " +
                    $"{skippedMissing} absent local audio files.");
            }
            battleCatalogSealed = true;
        }

        public void PlaySfx(string soundId, Vector3? position = null, Transform parent = null)
        {
            PlaySfx(soundId, position, parent, ResolveListenerTransform(), false, 0);
        }

        private void PlaySfx(
            string soundId,
            Vector3? position,
            Transform parent,
            Transform listenerTransform,
            bool isBattleEvent,
            int battleSourceWorldX,
            Vector2Int? battleMix = null)
        {
            if (this == null || soundOwnerDisposed ||
                (isBattleEvent && (battleSoundIntakeClosed || battleAudioPreparationInProgress)))
                return;
            PreparedSoundCue preparedCue = GetOrPrepareCue(soundId, isBattleEvent);
            if (preparedCue == null)
                return;

            if (preparedCue.IsLoaded)
            {
                PlayPreparedCue(preparedCue, position, parent, listenerTransform,
                    isBattleEvent, battleSourceWorldX, soundId, battleMix);
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
                listenerTransform,
                isBattleEvent,
                battleSourceWorldX,
                soundId,
                battleMix).Forget();
        }

        public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
        {
            if (sounds == null)
                return;

            Transform listenerTransform = ResolveListenerTransform();
            // Alignment contract: NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001.
            for (int start = 0; start < sounds.Count;)
            {
                int end = start + 1;
                while (end < sounds.Count && sounds[end].Tick == sounds[start].Tick)
                    end++;
                for (int index = start; index < end; index++)
                {
                    PendingSoundEvent sound = sounds[index];
                    bool alreadyConsumed = false;
                    for (int previous = start; previous < index; previous++)
                    {
                        if (!string.Equals(sounds[previous].Cue, sound.Cue, StringComparison.Ordinal))
                            continue;
                        alreadyConsumed = true;
                        break;
                    }
                    if (alreadyConsumed)
                        continue;

                    Vector2Int mix = Vector2Int.zero;
                    for (int contribution = index; contribution < end; contribution++)
                    {
                        if (string.Equals(sounds[contribution].Cue, sound.Cue, StringComparison.Ordinal))
                            mix += ComputeNativeBattleStereoPercentages(sounds[contribution].WorldX, 0);
                    }
                    PresentSound(sound, listenerTransform, mix);
                }
                start = end;
            }
        }

        public void PresentSound(PendingSoundEvent sound)
        {
            PresentSound(sound, ResolveListenerTransform());
        }

        private void PresentSound(PendingSoundEvent sound, Transform listenerTransform,
            Vector2Int? battleMix = null)
        {
            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld(sound.WorldX, 0f);
            PlaySfx(
                sound.Cue,
                new Vector3(groundPoint.x, groundPoint.y, 0f),
                null,
                listenerTransform,
                true,
                sound.WorldX,
                battleMix);
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

        private PreparedSoundCue GetOrPrepareCue(
            string soundId, bool isBattleEvent = false)
        {
            if (this == null || soundOwnerDisposed || string.IsNullOrEmpty(soundId) ||
                (isBattleEvent && battleSoundIntakeClosed))
                return null;

            AudioController controller = AudioController.Instance;
            if (!ReferenceEquals(controller, preparedAudioController))
            {
                if (battleCatalogSealed)
                {
                    rejectedUnpreparedCueCount++;
                    return null;
                }
                ReleaseBattlePlaybackClips();
                preparedCues.Clear();
                preparedFormalBattleCues.Clear();
                preparedAudioController = controller;
            }

            if (isBattleEvent &&
                preparedFormalBattleCues.TryGetValue(soundId, out PreparedSoundCue preparedCue))
                return preparedCue;

            if (!isBattleEvent &&
                preparedCues.TryGetValue(soundId, out preparedCue))
                return preparedCue;

            if (battleCatalogSealed)
            {
                if (isBattleEvent &&
                    preparedCues.TryGetValue(soundId, out preparedCue))
                    return preparedCue;
                rejectedUnpreparedCueCount++;
                return null;
            }

            AudioItem audioItem = FindAudioItem(controller, soundId) ??
                                  CreateFallbackAudioItem(soundId);
            string relativeFolder = ResolveRelativeFolder(soundId, audioItem);
            if (isBattleEvent && soundId.Length == 7 &&
                soundId.StartsWith("SFX_", StringComparison.Ordinal) &&
                soundId[4] >= '0' && soundId[4] <= '9' &&
                soundId[5] >= '0' && soundId[5] <= '9' &&
                soundId[6] >= '0' && soundId[6] <= '9')
            {
                relativeFolder = "data/" + soundId.Substring(4) + ".wav";
            }
            string normalizedRelativeFolder = NormalizeRelativeFolder(relativeFolder);
            bool isSingleFile = IsSingleFilePath(normalizedRelativeFolder);
            string formalSourcePath = isBattleEvent && isSingleFile
                ? ResolveFormalBattleSoundSourcePath(normalizedRelativeFolder)
                : null;
            if (formalSourcePath == null &&
                preparedCues.TryGetValue(soundId, out preparedCue))
                return preparedCue;
            bool isFormalBattleFile = formalSourcePath != null;
            if (isFormalBattleFile && preparedFormalBattleCues.TryGetValue(
                normalizedRelativeFolder, out preparedCue))
            {
                preparedFormalBattleCues.TryAdd(soundId, preparedCue);
                return preparedCue;
            }
            preparedCue = new PreparedSoundCue
            {
                AudioItem = audioItem,
                IsSingleFile = isSingleFile,
                IsFormalBattleFile = isFormalBattleFile,
                CatalogGeneration = preparedCatalogGeneration,
                FormalIdentity = normalizedRelativeFolder,
                CacheKey = isSingleFile
                    ? $"{(isFormalBattleFile ? "NTSD.FormalBattleAudioFile::" : "NTSD.AudioFile::")}{normalizedRelativeFolder}"
                    : $"NTSD.Audio::{normalizedRelativeFolder}",
                SourcePath = formalSourcePath ?? Path.Combine(
                    Application.dataPath, soundRootFolder, normalizedRelativeFolder),
                Clips = isSingleFile ? new AudioClip[1] : null,
            };
            if (isFormalBattleFile)
            {
                preparedFormalBattleCues.TryAdd(normalizedRelativeFolder, preparedCue);
                preparedFormalBattleCues.TryAdd(soundId, preparedCue);
            }
            else
                preparedCues.Add(soundId, preparedCue);
            preparedCueBuildCount++;
            return preparedCue;
        }

        private async UniTask EnsurePreparedCueLoadedAsync(PreparedSoundCue preparedCue)
        {
            if (preparedCue == null)
                return;
            while (preparedCue.IsLoading)
                await UniTask.Yield();
            if (this == null || preparedCue.IsLoaded ||
                (preparedCue.IsFormalBattleFile && !IsCurrentFormalCue(preparedCue)))
                return;

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

        private static string ResolveFormalBattleSoundSourcePath(string relativePath)
        {
            string configuredRoot = GameConfig.Instance?.BattleContentRuntimeRoot?.Trim();
            if (string.IsNullOrWhiteSpace(relativePath) ||
                string.IsNullOrWhiteSpace(configuredRoot))
                return null;

            try
            {
                string root = Path.GetFullPath(Path.Combine(
                    Application.dataPath, "..", configuredRoot, "vfs"));
                string candidate = Path.GetFullPath(Path.Combine(root, relativePath));
                string rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) +
                    Path.DirectorySeparatorChar;
                if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                    return null;
                return File.Exists(candidate) ? candidate : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (PathTooLongException)
            {
                return null;
            }
        }

        private async UniTaskVoid LoadAndPlayPreparedCueAsync(
            PreparedSoundCue preparedCue,
            Vector3? position,
            Transform parent,
            Transform listenerTransform,
            bool isBattleEvent,
            int battleSourceWorldX,
            string battleIdentity,
            Vector2Int? battleMix)
        {
            int generation = preparedCatalogGeneration;
            try
            {
                await LoadPreparedClipsAsync(preparedCue);
                if (this == null || generation != preparedCatalogGeneration)
                    return;
                PlayPreparedCue(preparedCue, position, parent, listenerTransform,
                    isBattleEvent, battleSourceWorldX, battleIdentity, battleMix);
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
            Transform listenerTransform,
            bool isBattleEvent,
            int battleSourceWorldX,
            string battleIdentity,
            Vector2Int? battleMix)
        {
            if (this == null || soundOwnerDisposed ||
                (isBattleEvent && battleSoundIntakeClosed))
                return;
            AudioItem audioItem = preparedCue.AudioItem;

            if (isBattleEvent && preparedCue.BattlePlaybackClips == null)
            {
                if (battleCatalogSealed)
                {
                    rejectedUnpreparedCueCount++;
                    return;
                }
                PrepareBattlePlaybackClips(preparedCue);
            }
            AudioClip clip = PickClip(isBattleEvent ? preparedCue.BattlePlaybackClips : preparedCue.Clips);
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
            Transform attachTarget = !isBattleEvent && audioItem.range > 0f
                ? parent
                : null;
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

            float matrixPan = 0f;
            float matrixGain = 1f;
            Vector2Int mix = battleMix ?? ComputeNativeBattleStereoPercentages(battleSourceWorldX, 0);
            if (isBattleEvent && mix.x + mix.y <= 0)
                return;

            AudioSource voice = AcquireOneShotVoice(out int voiceIndex, isBattleEvent, battleIdentity);
            if (voice == null)
            {
                oneShotVoiceLimitDropCount++;
                return;
            }
            if (isBattleEvent)
            {
                // Original DirectSound keeps the previous pan if SetPan is out of range.
                long pan = ((long)mix.y - mix.x) * 15;
                if (pan >= -10000 && pan <= 10000)
                    battleVoicePanHundredthDb[voiceIndex] = (int)pan;
                int appliedPan = battleVoicePanHundredthDb[voiceIndex];
                float spatialGain = Mathf.Pow(10f, (Math.Min((long)mix.x + mix.y, 100) - 100) * 0.01f);
                float left = spatialGain * Mathf.Pow(10f, -Math.Max(0, appliedPan) / 2000f);
                float right = spatialGain * Mathf.Pow(10f, Math.Min(0, appliedPan) / 2000f);
                if (clip.channels == 1)
                {
                    float leftPower = left * left;
                    float rightPower = right * right;
                    float totalPower = leftPower + rightPower;
                    matrixGain = Mathf.Sqrt(totalPower);
                    matrixPan = totalPower > 0f
                        ? (rightPower - leftPower) / totalPower
                        : 0f;
                }
                else
                {
                    matrixGain = Mathf.Max(left, right);
                    matrixPan = matrixGain > 0f
                        ? left >= right
                            ? right / matrixGain - 1f
                            : 1f - left / matrixGain
                        : 0f;
                }
            }

            MMFollowTarget follower = oneShotVoiceFollowers[voiceIndex];
            voice.Stop();
            voice.transform.position = playbackPosition;
            voice.clip = clip;
            voice.pitch = pitch;
            oneShotVoiceBaseVolumes[voiceIndex] = volume * matrixGain;
            voice.volume = oneShotVoiceBaseVolumes[voiceIndex] * nativeBattleSfxGain;
            voice.spatialBlend = !isBattleEvent && audioItem.range > 0f ? 1f : 0f;
            voice.rolloffMode = AudioRolloffMode.Custom;
            voice.minDistance = audioItem.range > 3f
                ? audioItem.range - 3f
                : 0f;
            voice.maxDistance = audioItem.range > 0f ? audioItem.range : 500f;
            voice.loop = !isBattleEvent && audioItem.loop;
            voice.panStereo = matrixPan;
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
            oneShotVoiceStartSequences[voiceIndex] =
                ++nextOneShotVoiceStartSequence;
            float absolutePitch = Mathf.Max(0.01f, Mathf.Abs(pitch));
            oneShotVoiceAvailableDspTimes[voiceIndex] =
                voice.loop
                    ? double.PositiveInfinity
                    : AudioSettings.dspTime + clip.length / absolutePitch;
            if (voice.loop)
                loopFallbackPlayCount++;
            else
                pooledOneShotPlayCount++;
        }

        private static Vector2Int ComputeNativeBattleStereoPercentages(
            int sourceWorldX,
            int audioCameraX)
        {
            long relative = (long)sourceWorldX - audioCameraX;
            if (relative < -333)
                return Vector2Int.zero;
            if (relative < 0)
                return new Vector2Int((int)((333 + relative) * 100 / 333), 0);

            relative -= 333;
            if (relative < 0)
                return new Vector2Int(100, 0);
            relative -= 666;
            if (relative < 0)
            {
                int right = (int)((relative + 666) * 100 / 666);
                return new Vector2Int(100 - right, right);
            }
            relative -= 333;
            if (relative < 0)
                return new Vector2Int(0, 100);
            relative -= 333;
            return relative < 0
                ? new Vector2Int(0, (int)(-relative * 100 / 333))
                : Vector2Int.zero;
        }

        private void PrepareBattlePlaybackClips(PreparedSoundCue cue)
        {
            if (this == null || cue == null || !cue.IsLoaded || cue.BattlePlaybackClips != null)
                return;
            AudioClip[] sourceClips = cue.Clips ?? Array.Empty<AudioClip>();
            var playbackClips = new AudioClip[sourceClips.Length];
            int firstOwned = ownedBattlePlaybackClips.Count;
            try
            {
                for (int index = 0; index < sourceClips.Length; index++)
                {
                    AudioClip source = sourceClips[index];
                    if (source == null || source.channels != 1)
                    {
                        playbackClips[index] = source;
                        continue;
                    }

                    if (source.samples >= 8192)
                    {
                        playbackClips[index] = CreateBoundedBattlePlaybackClip(source);
                        continue;
                    }

                    // Unity's mono pan law attenuates the center and caps volume at1.
                    // Duplicate PCM into stereo to preserve native per-channel gain.
                    var mono = new float[source.samples];
                    if (!source.GetData(mono, 0))
                        throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
                    var stereo = new float[checked(source.samples * 2)];
                    for (int sample = 0; sample < mono.Length; sample++)
                        stereo[sample * 2] = stereo[sample * 2 + 1] = mono[sample];
                    AudioClip playback = AudioClip.Create(source.name, source.samples, 2, source.frequency, false);
                    ownedBattlePlaybackClips.Add(playback);
                    if (!playback.SetData(stereo, 0))
                        throw new InvalidOperationException("Cannot initialize battle PCM: " + source.name);
                    playbackClips[index] = playback;
                }
                cue.BattlePlaybackClips = playbackClips;
                if (cue.OwnedSource != null && cue.OwnedSource.channels == 1)
                {
                    cue.Clips = playbackClips;
                    ReleaseOwnedFormalSource(cue);
                }
            }
            catch
            {
                for (int index = ownedBattlePlaybackClips.Count - 1; index >= firstOwned; index--)
                {
                    AudioClip clip = ownedBattlePlaybackClips[index];
                    if (Application.isPlaying)
                        Destroy(clip);
                    else
                        DestroyImmediate(clip);
                    ownedBattlePlaybackClips.RemoveAt(index);
                }
                if (cue.OwnedSource != null)
                {
                    ReleaseOwnedFormalSource(cue);
                    cue.Clips = Array.Empty<AudioClip>();
                    cue.IsLoaded = false;
                }
                throw;
            }
        }

        private AudioClip CreateBoundedBattlePlaybackClip(AudioClip source)
        {
            const int blockFrames = 4096;
            int totalFrames = source.samples;
            var mono = new float[blockFrames];
            if (!source.GetData(mono, 0))
                throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
            _ = checked(totalFrames * 2);
            var stereo = new float[blockFrames * 2];
            AudioClip playback = AudioClip.Create(source.name, totalFrames, 2, source.frequency, false);
            ownedBattlePlaybackClips.Add(playback);
            int offset = 0;
            while (true)
            {
                for (int sample = 0; sample < mono.Length; sample++)
                    stereo[sample * 2] = stereo[sample * 2 + 1] = mono[sample];
                if (!playback.SetData(stereo, offset))
                    throw new InvalidOperationException("Cannot initialize battle PCM: " + source.name);
                offset += mono.Length;
                if (offset == totalFrames)
                    return playback;
                int remaining = totalFrames - offset;
                if (remaining < blockFrames)
                {
                    // Exact tail buffers prevent Unity SetData from wrapping over the clip's beginning.
                    mono = new float[remaining];
                    stereo = new float[remaining * 2];
                }
                if (!source.GetData(mono, offset))
                    throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
            }
        }

        private AudioSource AcquireOneShotVoice(
            out int voiceIndex, bool isBattleEvent, string battleIdentity)
        {
            int voiceLimit = isBattleEvent
                ? NativeBattleVoiceLimit
                : ResolveOneShotVoiceLimit();
            EnsureOneShotVoiceCapacity(voiceLimit);
            voiceIndex = -1;
            if (isBattleEvent)
            {
                for (int index = 0; index < voiceLimit; index++)
                {
                    if (oneShotVoices[index] != null &&
                        string.Equals(battleVoiceIdentities[index], battleIdentity, StringComparison.Ordinal))
                        return PrepareOneShotVoice(index, voiceLimit, out voiceIndex);
                }
            }
            double currentDspTime = AudioSettings.dspTime;
            for (int offset = 0; offset < voiceLimit; offset++)
            {
                int index = (nextOneShotVoiceIndex + offset) % voiceLimit;
                AudioSource voice = oneShotVoices[index];
                if (voice == null || oneShotVoiceAvailableDspTimes[index] > currentDspTime)
                {
                    continue;
                }
                BindBattleVoice(index, isBattleEvent ? battleIdentity : null);
                return PrepareOneShotVoice(index, voiceLimit, out voiceIndex);
            }

            if (!isBattleEvent)
                return null;

            int oldestIndex = -1;
            long oldestSequence = long.MaxValue;
            for (int index = 0; index < voiceLimit; index++)
            {
                if (oneShotVoices[index] == null ||
                    oneShotVoiceStartSequences[index] >= oldestSequence)
                    continue;
                oldestIndex = index;
                oldestSequence = oneShotVoiceStartSequences[index];
            }
            if (oldestIndex < 0)
                return null;

            // Alignment contract: NTSD28-Q10-BATTLE-VOICE-CAP-001.
            oneShotVoiceLimitDropCount++;
            BindBattleVoice(oldestIndex, battleIdentity);
            return PrepareOneShotVoice(oldestIndex, voiceLimit, out voiceIndex);
        }

        private void BindBattleVoice(int index, string identity)
        {
            battleVoiceIdentities[index] = identity;
            battleVoicePanHundredthDb[index] = 0;
        }

        private AudioSource PrepareOneShotVoice(
            int index, int voiceLimit, out int voiceIndex)
        {
            MMFollowTarget follower = oneShotVoiceFollowers[index];
            if (follower != null)
            {
                follower.Target = null;
                follower.enabled = false;
            }

            AudioSource voice = oneShotVoices[index];
            voice.transform.SetParent(transform, false);
            nextOneShotVoiceIndex = (index + 1) % voiceLimit;
            voiceIndex = index;
            return voice;
        }

        private void EnsureOneShotVoicePool()
        {
            EnsureOneShotVoiceCapacity(ResolveOneShotVoiceLimit());
        }

        private void EnsureOneShotVoiceCapacity(int voiceLimit)
        {
            int previousCount = oneShotVoices.Length;
            if (previousCount >= voiceLimit)
                return;

            Array.Resize(ref oneShotVoices, voiceLimit);
            Array.Resize(ref oneShotVoiceFollowers, voiceLimit);
            Array.Resize(ref oneShotVoiceAvailableDspTimes, voiceLimit);
            Array.Resize(ref oneShotVoiceBaseVolumes, voiceLimit);
            Array.Resize(ref oneShotVoiceStartSequences, voiceLimit);
            Array.Resize(ref battleVoiceIdentities, voiceLimit);
            Array.Resize(ref battleVoicePanHundredthDb, voiceLimit);
            for (int i = previousCount; i < voiceLimit; i++)
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
            if (preparedCue.IsLoaded ||
                (preparedCue.IsFormalBattleFile && !IsCurrentFormalCue(preparedCue)))
                return;
            int generation = preparedCatalogGeneration;

            try
            {
                if (preparedCue.IsSingleFile && !File.Exists(preparedCue.SourcePath))
                {
                    failedPreparedCueLoadCount++;
                    skippedMissingBattleCueFileCount++;
                    UseConfiguredClipOrEmpty(preparedCue);
                    return;
                }

                // 单文件模式：soundId 以音频扩展名结尾（如 data\003.wav）
                if (preparedCue.IsSingleFile)
                {
                    if (preparedCue.IsFormalBattleFile)
                    {
                        pendingFormalAudioLoads++;
                        formalAudioLoadCount++;
                        try
                        {
                            AudioClip loaded = await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(
                                string.Empty, preparedCue.SourcePath);
                            if (!IsCurrentFormalCue(preparedCue))
                            {
                                if (loaded != null)
                                    staleFormalAudioReleaseCount++;
                                DestroyOwnedAudioClip(loaded);
                                return;
                            }
                            preparedCue.OwnedSource = loaded;
                            if (loaded != null)
                                ownedFormalAudioSources.Add(loaded);
                            preparedCue.Clips[0] = loaded;
                        }
                        finally
                        {
                            pendingFormalAudioLoads--;
                        }
                    }
                    else
                    {
                        AudioClip loaded = await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(
                            preparedCue.CacheKey, preparedCue.SourcePath);
                        if (this == null || generation != preparedCatalogGeneration)
                            return;
                        preparedCue.Clips[0] = loaded;
                    }
                    return;
                }

                // 目录模式：soundId 是目录路径，加载目录下所有音频文件
                AudioClip[] loadedClips =
                    await NTSD_ResourceLoader.Instance.LoadAudioClipsAsync(
                        preparedCue.CacheKey,
                        preparedCue.SourcePath);
                if (this == null || generation != preparedCatalogGeneration)
                    return;
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
                if (this == null || generation != preparedCatalogGeneration ||
                    (preparedCue.IsFormalBattleFile && !IsCurrentFormalCue(preparedCue)))
                    return;
                failedPreparedCueLoadCount++;
                UseConfiguredClipOrEmpty(preparedCue);
                Debug.LogWarning(
                    $"[NTSDSoundPlayer] Battle cue prewarm failed and was sealed without a streamed clip: " +
                    $"{preparedCue.AudioItem?.name ?? preparedCue.SourcePath}; {ex.Message}");
            }
            finally
            {
                if (this != null && generation == preparedCatalogGeneration &&
                    (!preparedCue.IsFormalBattleFile || IsCurrentFormalCue(preparedCue)))
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
