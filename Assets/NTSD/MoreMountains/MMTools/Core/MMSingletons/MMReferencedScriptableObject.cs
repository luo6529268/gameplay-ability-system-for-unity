using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
/// <summary>
	/// MMReferencedScriptableObject类：自动引用T类型的ScriptableObject实例
	/// 这是一个ReferenceHolder<T>的示例用法，可以用于任何类类型
	/// </summary>
	public class MMReferencedScriptableObject<T> : ScriptableObject where T : ScriptableObject
	{
		// 存储T类型实例的引用持有者
		private MMReferenceHolder<T> _instances;
		// 类型化属性，延迟初始化
		protected virtual T Typed => _typed = _typed ?? this as T; private T _typed;

		// 当被引用时的虚方法，子类可重写
		protected virtual void OnReferenced() {}
		// 启用时的虚方法
		protected virtual void OnEnable()
		{
			// 将当前实例添加到引用持有者中
			_instances.Reference(Typed);
			// 调用引用回调
			OnReferenced();
			// MMDebug.DebugLogInfo(ReferenceHolder<T>.Any != null, this);
		}

		// 销毁时的虚方法，子类可重写
		protected virtual void OnDisposed() {}
		// 禁用时的虚方法
		protected virtual void OnDisable()
		{
			// 释放引用
			_instances.Dispose();
			// 调用销毁回调
			OnDisposed();
			// MMDebug.DebugLogInfo(ReferenceHolder<T>.Any != null);
		}
	}

	// MMReferenceHolder结构体：使用弱引用来管理对象引用
	// 当引擎不再使用这些对象时，允许GC回收它们
	public struct MMReferenceHolder<T> : IDisposable where T : class
	{
		// 存储所有弱引用的静态列表
		private static List<WeakReference<T>> _instances = new List<WeakReference<T>>(2);

		// 当前实例的弱引用
		private WeakReference<T> _instance;
		
		// 引用方法：添加新的弱引用
		public void Reference(T instance, bool cleanUp = false)
		{
			_instances = _instances ?? new List<WeakReference<T>>(1);
			// 如果需要，先清理无效引用
			if(cleanUp) CleanUp();
			if (instance != null)
			{
				// 创建新的弱引用并添加到列表
				_instance = new WeakReference<T>(instance);
				_instances.Add(_instance); // 总是在末尾添加以保持高效，需要时调用CleanUp
			}
		}
		
		// 释放方法：移除当前实例的引用
		public void Dispose()
		{
			if (_instance != null) _instances?.Remove(_instance);
		}

		// 清理方法：移除所有无效的弱引用
		public static void CleanUp() => RepackNonNullReferences();
		
		// 重新打包非空引用：遍历并移除已被GC回收的引用
		static void RepackNonNullReferences()
		{
			if (_instances == null) return;
			for(int n=_instances.Count-1; n >=0; --n)
			{
				// 如果无法获取目标对象，说明已被GC回收，从列表中移除
				if (!_instances[n].TryGetTarget(out T target))
				{
					_instances.RemoveAt(n);
				}
			}
		}

		// 获取任意一个有效的实例
		public static T Any => _instances != null && _instances.Count > 0 && _instances[0].TryGetTarget(out T target) ? target : null;
		
		// 获取所有有效实例的枚举器
		public static IEnumerator<T> All
		{
			get
			{
				if (_instances == null) yield break;
				foreach (var inst in _instances)
				{
					// 只返回仍然有效的实例
					if (inst.TryGetTarget(out T target))
					{
						yield return target;
					}
				}
			}
		}
		
		// 根据选择器函数查找第一个匹配的实例
		public static T First(System.Func<T,bool> selector)
		{
			if (_instances == null) return null;
			if (selector == null) return Any;
			foreach (var inst in _instances)
			{
				// 返回第一个满足选择器条件的有效实例
				if (inst.TryGetTarget(out T target) && selector(target))
				{
					return target;
				}
			}
			return null;
		}
	}

}