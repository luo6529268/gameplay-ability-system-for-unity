using System;
using System.IO;
using BeatEmUpTemplate2D;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.Load;
using UnityEngine;

namespace NTSD.App
{
    public sealed class NTSDSoundPlayer : MonoBehaviour
    {
        [SerializeField] private string soundRootFolder = "NTSD/Sound";

        public void PlaySfx(string soundId, Vector3? position = null, Transform parent = null)
        {
            PlaySfxAsync(soundId, position, parent).Forget();
        }

        private async UniTaskVoid PlaySfxAsync(string soundId, Vector3? position, Transform parent)
        {
            if (string.IsNullOrEmpty(soundId))
            {
                return;
            }

            Debug.Log($"[SoundPlayer] PlaySfx: {soundId}");

            // 确保 MMSoundManager 已初始化（访问 Instance 会触发自动创建 + OnEnable/Register）
            _ = MMSoundManager.Instance;

            AudioItem audioItem = FindAudioItem(soundId) ?? CreateFallbackAudioItem(soundId);
            AudioClip[] clips = await LoadClipsAsync(soundId, audioItem);
            AudioClip clip = PickClip(clips);
            Debug.Log($"[SoundPlayer] clip={clip?.name ?? "NULL"} for {soundId}");
            if (clip == null)
            {
                Debug.LogWarning($"[NTSD][Audio] AudioClip is missing for: {soundId}, folder: {ResolveRelativeFolder(soundId, audioItem)}");
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

            var src = MMSoundManager.Instance?.PlaySound(clip, options);
            Debug.Log($"[SoundPlayer] Trigger: {clip.name} vol={options.Volume} src={src?.name} isPlaying={src?.isPlaying}");
        }

        private AudioItem FindAudioItem(string soundId)
        {
            AudioController controller = AudioController.Instance;
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

        private async UniTask<AudioClip[]> LoadClipsAsync(string soundId, AudioItem audioItem)
        {
            string relativeFolder = ResolveRelativeFolder(soundId, audioItem);
            string normalizedRelativeFolder = NormalizeRelativeFolder(relativeFolder);

            // 单文件模式：soundId 以音频扩展名结尾（如 data\003.wav）
            if (IsSingleFilePath(normalizedRelativeFolder))
            {
                string cacheKey = $"NTSD.AudioFile::{normalizedRelativeFolder}";
                string filePath = Path.Combine(Application.dataPath, soundRootFolder, normalizedRelativeFolder);
                AudioClip clip = await NTSD_ResourceLoader.Instance.LoadSingleAudioClipAsync(cacheKey, filePath);
                return clip != null ? new[] { clip } : Array.Empty<AudioClip>();
            }

            // 目录模式：soundId 是目录路径，加载目录下所有音频文件
            string dirCacheKey = $"NTSD.Audio::{normalizedRelativeFolder}";
            string directoryPath = Path.Combine(Application.dataPath, soundRootFolder, normalizedRelativeFolder);
            AudioClip[] loadedClips = await NTSD_ResourceLoader.Instance.LoadAudioClipsAsync(dirCacheKey, directoryPath);
            if (loadedClips != null && loadedClips.Length > 0)
            {
                return loadedClips;
            }

            return audioItem?.clip;
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
