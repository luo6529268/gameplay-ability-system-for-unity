using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Add this class to a GameObject to have it play a background music when instanciated.
    /// 将这个类添加到一个GameObject上，使其在实例化时播放背景音乐。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/Background Music")]
	public class BackgroundMusic : TopDownMonoBehaviour
    {
        /// 背景音乐
        [Tooltip("用作背景音乐的音频剪辑")]
        public AudioClip RoomBGM_Zero;
        public AudioClip RoomBGM_One;

        /// 音乐是否应该循环播放
        [Tooltip("音乐是否应该循环播放")]
        public bool Loop = true;
        /// 创建此背景音乐所用的ID
        [Tooltip("创建此背景音乐所用的ID")]
        public int ID = 255;

        private bool _IsCanSwitchBGM;
        AudioSource _abilityInProgressSfx;

        void OnEnable() 
        {
        }

        void OnDisable() 
        {
        }

        

        void OnHandleRoomBGM(AudioClip bgm) 
        {
            if (_abilityInProgressSfx != null)
            {
                MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, 0, _abilityInProgressSfx);
                _abilityInProgressSfx = null;
            }

            if (_abilityInProgressSfx == null)
            {
                _abilityInProgressSfx = MMSoundManagerSoundPlayEvent.Trigger(bgm, MMSoundManager.MMSoundManagerTracks.Music, this.transform.position, true);
            }
        }

        /// <summary>
        /// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
        /// </summary>
        protected virtual void Start()
		{
		
		}
	}
}