using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This component allows the definition of a level that can then be accessed and loaded. Used mostly in the level map scene.
    /// 这个组件允许定义一个关卡，然后可以访问并加载这个关卡。它主要用于关卡地图场景中。
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Level Selector")]
	public class LevelSelector : TopDownMonoBehaviour
	{
        /// 目标关卡的确切名称
        [Tooltip("目标关卡的确切名称")]
        public string LevelName;

        /// 如果设置为真，GoToLevel将忽略关卡管理器并直接调用
        [Tooltip("如果设置为真，GoToLevel将忽略关卡管理器并直接调用")]
        public bool DoNotUseLevelManager = false;

        /// 如果设置为真，在加载新关卡时将销毁任何持久性角色
        [Tooltip("如果设置为真，在加载新关卡时将销毁任何持久性角色")]
        public bool DestroyPersistentCharacter = false;

        /// <summary>
        /// Loads the level specified in the inspector
        /// </summary>
        public virtual void GoToLevel()
		{
			LoadScene(LevelName);
		}

		public void OnQuitGame() 
		{
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
			    Application.Quit();
#endif
        }

        /// <summary>
        /// Loads a new scene, either via the LevelManager or not
        /// </summary>
        /// <param name="newSceneName"></param>
        protected virtual void LoadScene(string newSceneName)
		{
			if (DoNotUseLevelManager)
			{
				MMAdditiveSceneLoadingManager.LoadScene(newSceneName);    
			}
		}


		/// <summary>
		/// Reloads the current level
		/// </summary>
		public virtual void ReloadLevel()
		{
			// we trigger an unPause event for the GameManager (and potentially other classes)
			LoadScene(SceneManager.GetActiveScene().name);
		}
		
	}
}