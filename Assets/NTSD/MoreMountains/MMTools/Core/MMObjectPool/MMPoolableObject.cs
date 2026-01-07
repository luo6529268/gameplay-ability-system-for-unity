using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this class to an object that you expect to pool from an objectPooler. 
    /// Note that these objects can't be destroyed by calling Destroy(), they'll just be set inactive (that's the whole point).
    /// 
    ///  将这个类添加到你期望从对象池（objectPooler）中池化的对象上。
	///  注意，这些对象不能通过调用Destroy()来销毁，它们只会被设置为非活动状态（这就是全部目的）。
    /// </summary>
    [AddComponentMenu("ThirdParty/More Mountains/Tools/Object Pool/MM Poolable Object")]
	public class MMPoolableObject : MMObjectBounds
	{
		[Header("Events")]
		public UnityEvent ExecuteOnEnable;
		public UnityEvent ExecuteOnDisable;
		
		public delegate void Events();
		public event Events OnSpawnComplete;

		[Header("Poolable Object")]
        /// 对象的生命周期，以秒为单位。如果设置为0，则对象将永远存在；如果设置为任何正值，则在该时间后对象将被设置为非活动状态
        public float LifeTime = 0f;

		/// <summary>
		/// Turns the instance inactive, in order to eventually reuse it.
		/// </summary>
		public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Called every frame
		/// </summary>
		protected virtual void Update()
		{

		}

		/// <summary>
		/// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
		/// </summary>
		protected virtual void OnEnable()
		{
			Size = GetBounds().extents * 2;
			if (LifeTime > 0f)
			{
				Invoke("Destroy", LifeTime);	
			}
			ExecuteOnEnable?.Invoke();
		}

		/// <summary>
		/// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
		/// </summary>
		protected virtual void OnDisable()
		{
			ExecuteOnDisable?.Invoke();
			CancelInvoke();
		}

		/// <summary>
		/// Triggers the on spawn complete event
		/// </summary>
		public virtual void TriggerOnSpawnComplete()
		{
			OnSpawnComplete?.Invoke();
		}
	}
}