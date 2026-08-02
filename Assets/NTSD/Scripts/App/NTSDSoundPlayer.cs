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

        private sealed class PreparedSoundCue
        {
            public AudioItem AudioItem;
            public string CacheKey;
            public string SourcePath;
            public bool IsSingleFile;
            public bool IsLoaded;
            public AudioClip[] Clips;
        }

        private readonly Dictionary<string, PreparedSoundCue> preparedCues =
            new Dictionary<string, PreparedSoundCue>(StringComparer.Ordinal);
        private AudioController preparedAudioController;
        private long preparedCueBuildCount;

        public int PreparedCueCountForDiagnostics => preparedCues.Count;
        public long PreparedCueBuildCountForDiagnostics => preparedCueBuildCount;

        public void PlaySfx(string soundId, Vector3? position = null, Transform parent = null)
        {
            PreparedSoundCue preparedCue = GetOrPrepareCue(soundId);
            if (preparedCue == null)
                return;

            if (preparedCue.IsLoaded)
            {
                PlayPreparedCue(preparedCue, position, parent);
                return;
            }

            LoadAndPlayPreparedCueAsync(preparedCue, position, parent).Forget();
        }

        public void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds)
        {
            if (sounds == null)
                return;

            for (int i = 0; i < sounds.Count; i++)
                PresentSound(sounds[i]);
        }

        public void PresentSound(PendingSoundEvent sound)
        {
            Vector2 groundPoint = NTSDRenderSpace.GroundPixelToWorld(sound.WorldX, 0f);
            PlaySfx(sound.Cue, new Vector3(groundPoint.x, groundPoint.y, 0f));
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
            Transform parent)
        {
            await LoadPreparedClipsAsync(preparedCue);
            PlayPreparedCue(preparedCue, position, parent);
        }

        private void PlayPreparedCue(
            PreparedSoundCue preparedCue,
            Vector3? position,
            Transform parent)
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

            Transform listenerTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 playbackPosition = position ?? (listenerTransform != null ? listenerTransform.position : Vector3.zero);
            Transform attachTarget = audioItem.range <= 0f ? listenerTransform : parent;

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

            MMSoundManager.Instance?.PlaySound(clip, options);
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
