using UnityEngine;

namespace NTSD.App
{
    [System.Serializable]
    public class AudioItem
    {
        public string name;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 1f)] public float randomVolume = 0f;
        [Range(0f, 1f)] public float randomPitch = 0f;
        public float minTimeBetweenCall = 0.1f;

        // 为 0 时关闭距离衰减，始终直接播放音效。
        public float range = 0f;

        public bool loop;
        public string streamingFolder;
        public AudioClip[] clip;
        [HideInInspector] public float lastTimePlayed = 0f;
    }
}
