using UnityEngine;  // 引入Unity引擎的核心命名空间，提供Unity引擎的基础类和功能
using System;      // 引入System命名空间，提供基础类和基类，如Console、DateTime等

namespace MoreMountains.Tools  // 定义MoreMountains.Tools命名空间，用于组织工具类代码，避免命名冲突
{
	/// <summary>
	///持久的谦虚单例，基本上是一个经典的单例，但会破坏它在唤醒时发现的相同类型的任何其他旧组件
	/// </summary>
	public class MMPersistentHumbleSingleton<T> : MonoBehaviour	where T : Component
	{
		/// whether or not this singleton already has an instance 
		public static bool HasInstance => _instance != null;
		public static T Current => _instance;
		
		protected static T _instance;
		
		/// the timestamp at which this singleton got initialized
		[MMReadOnly]
		public float InitializationTime;

		/// <summary>
		/// Singleton design pattern
		/// </summary>
		/// <value>The instance.</value>
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T> ();
					if (_instance == null)
					{
						GameObject obj = new GameObject ();
						obj.hideFlags = HideFlags.HideAndDontSave;
						obj.name = typeof(T).Name + "_AutoCreated";
						_instance = obj.AddComponent<T> ();
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
		/// </summary>
		protected virtual void Awake ()
		{
			InitializeSingleton();			
		}

		/// <summary>
		/// Initializes the singleton.
		/// </summary>
		protected virtual void InitializeSingleton()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			
			InitializationTime = Time.time;

			DontDestroyOnLoad (this.gameObject);
			// we check for existing objects of the same type
			T[] check = FindObjectsOfType<T>();
			foreach (T searched in check)
			{
				if (searched!=this)
				{
					// if we find another object of the same type (not this), and if it's older than our current object, we destroy it.
					if (searched.GetComponent<MMPersistentHumbleSingleton<T>>().InitializationTime < InitializationTime)
					{
						Destroy (searched.gameObject);
					}
				}
			}

			if (_instance == null)
			{
				_instance = this as T;
			}
		}
	}
}