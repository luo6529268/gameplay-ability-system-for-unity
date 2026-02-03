using MoreMountains.Tools;
using UnityEngine;

namespace NTSD.App
{
    public class AudioSettingsBridge : MonoBehaviour, MMEventListener<GameSettingsChangedEvent>
    {
        private void OnEnable()
        {
            this.MMEventStartListening<GameSettingsChangedEvent>();
            ApplyAllVolumeSettings();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<GameSettingsChangedEvent>();
        }

        public void OnMMEvent(GameSettingsChangedEvent evt)
        {
            if (evt.SettingName.Contains("Volume"))
            {
                ApplyVolumeSettingByName(evt.SettingName);
            }
        }

        private void ApplyAllVolumeSettings()
        {
            ApplyMasterVolume();
            ApplySfxVolume();
            ApplyMusicVolume();
        }

        private void ApplyVolumeSettingByName(string settingName)
        {
            if (settingName.Contains("MasterVolume"))
                ApplyMasterVolume();
            else if (settingName.Contains("SfxVolume"))
                ApplySfxVolume();
            else if (settingName.Contains("MusicVolume"))
                ApplyMusicVolume();
        }

        private void ApplyMasterVolume()
        {
            float volume = GameLocalSettings.MasterVolume;
            if (MMSoundManager.Instance != null && MMSoundManager.Instance.settingsSo != null)
            {
                MMSoundManager.Instance.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, volume);
            }
        }

        private void ApplySfxVolume()
        {
            float volume = GameLocalSettings.SfxVolume;
            if (MMSoundManager.Instance != null && MMSoundManager.Instance.settingsSo != null)
            {
                MMSoundManager.Instance.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, volume);
            }
        }

        private void ApplyMusicVolume()
        {
            float volume = GameLocalSettings.MusicVolume;
            if (MMSoundManager.Instance != null && MMSoundManager.Instance.settingsSo != null)
            {
                MMSoundManager.Instance.settingsSo.SetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, volume);
            }
        }
    }
}
