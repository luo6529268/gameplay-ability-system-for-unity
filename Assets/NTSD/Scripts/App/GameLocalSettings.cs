using UnityEngine;

namespace NTSD.App
{
    /// <summary>
    /// 本地游戏设置管理器，使用 PlayerPrefs 持久化存储。设置变化时触发 GameSettingsChangedEvent。
    /// </summary>
    public static class GameLocalSettings
    {
        private const string Prefix = "NTSD";
        private const string PlayerNameKey = "PlayerName";
        private const string MasterVolumeKey = "MasterVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";

        private const int MaxPlayerCount = 4;
        private const float DefaultVolume = 1.0f;

        private static readonly string[] DefaultPlayerNames =
        {
            "Player1", "Player2", "Player3", "Player4"
        };

        #region Player Names

        public static string GetPlayerName(int index)
        {
            if (index < 0 || index >= MaxPlayerCount)
            {
                Debug.LogWarning($"[GameLocalSettings] Invalid player index: {index}");
                return $"Player{index + 1}";
            }

            string key = GetKey(PlayerNameKey, index);
            string defaultName = DefaultPlayerNames[index];
            return PlayerPrefs.GetString(key, defaultName);
        }

        public static void SetPlayerName(int index, string name)
        {
            if (index < 0 || index >= MaxPlayerCount)
            {
                Debug.LogWarning($"[GameLocalSettings] Invalid player index: {index}");
                return;
            }

            string key = GetKey(PlayerNameKey, index);
            string oldValue = GetPlayerName(index);
            string newValue = string.IsNullOrWhiteSpace(name) ? DefaultPlayerNames[index] : name;

            if (oldValue == newValue) return;

            PlayerPrefs.SetString(key, newValue);
            PlayerPrefs.Save();

            GameSettingsChangedEvent.Trigger(key, oldValue, newValue);
        }

        #endregion

        #region Audio Settings

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(GetKey(MasterVolumeKey), DefaultVolume);
            set => SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(GetKey(SfxVolumeKey), DefaultVolume);
            set => SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(GetKey(MusicVolumeKey), DefaultVolume);
            set => SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        }

        #endregion

        #region Utility Methods

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void ResetToDefaults()
        {
            for (int i = 0; i < MaxPlayerCount; i++)
            {
                SetPlayerName(i, DefaultPlayerNames[i]);
            }

            MasterVolume = DefaultVolume;
            SfxVolume = DefaultVolume;
            MusicVolume = DefaultVolume;

            PlayerPrefs.Save();
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetKey(key));
        }

        public static void DeleteAll()
        {
            for (int i = 0; i < MaxPlayerCount; i++)
            {
                PlayerPrefs.DeleteKey(GetKey(PlayerNameKey, i));
            }

            PlayerPrefs.DeleteKey(GetKey(MasterVolumeKey));
            PlayerPrefs.DeleteKey(GetKey(SfxVolumeKey));
            PlayerPrefs.DeleteKey(GetKey(MusicVolumeKey));

            PlayerPrefs.Save();
        }

        #endregion

        #region Private Helpers

        private static string GetKey(string key)
        {
            return $"{Prefix}_{key}";
        }

        private static string GetKey(string key, int index)
        {
            return $"{Prefix}_{key}_{index}";
        }

        private static void SetFloat(string key, float value)
        {
            string fullKey = GetKey(key);
            float oldValue = PlayerPrefs.GetFloat(fullKey, DefaultVolume);

            if (Mathf.Approximately(oldValue, value)) return;

            PlayerPrefs.SetFloat(fullKey, value);
            PlayerPrefs.Save();

            GameSettingsChangedEvent.Trigger(fullKey, oldValue, value);
        }

        #endregion
    }
}
