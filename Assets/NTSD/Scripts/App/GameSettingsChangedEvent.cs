using MoreMountains.Tools;

namespace NTSD.App
{
    /// <summary>
    /// GameLocalSettings 中任意设置发生变化时触发的事件。
    /// </summary>
    public struct GameSettingsChangedEvent
    {
        public string SettingName;
        public object OldValue;
        public object NewValue;

        private static GameSettingsChangedEvent e;

        public static void Trigger(string settingName, object oldValue, object newValue)
        {
            e.SettingName = settingName;
            e.OldValue = oldValue;
            e.NewValue = newValue;
            MMEventManager.TriggerEvent(e);
        }
    }
}
