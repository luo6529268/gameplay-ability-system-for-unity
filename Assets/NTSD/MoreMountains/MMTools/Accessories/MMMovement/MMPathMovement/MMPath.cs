using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	[System.Serializable]
	/// <summary>
	/// This class describes a node on an MMPath
	/// </summary>
	public class MMPathMovementElement
	{
		/// the point that make up the path the object will follow
		public Vector3 PathElementPosition;
		/// a delay (in seconds) associated to each node
		public float Delay;
	}

    /// <summary>
    /// Add this component to an object and you'll be able to define a path, that can then be used by another component
    /// 将这个组件添加到一个对象上，你将能够定义一条路径，之后其他组件可以使用这条路径。
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Movement/MM Path")]
	public class MMPath : MonoBehaviour 
	{
		/// the possible cycle options
		public enum CycleOptions
		{
			BackAndForth,
			Loop,
			OnlyOnce
		}

		/// the possible movement directions
		public enum MovementDirection
		{
            //提升,
            //下行
            Ascending,
			Descending
		}

        [Header("路径")]
        [MMInformation("<size=15>在这里，你可以选择'<b>循环选项</b>'。</size>" +
            " <size=15> BackAndForth 将使你的对象沿着路径行走直到终点，然后返回原点。</size>" +
            "<size=15>如果你选择 Loop ，路径将被闭合，对象将沿着它一直移动，直到被告知停止。</size>" +
            "<size=15>如果你选择 OnlyOnce ，对象将沿着路径从第一个点移动到最后一个点，并永远停留在那里。</size>", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        public CycleOptions CycleOption;

        [MMInformation("<size=15>将点添加到<b>路径</b>（首先设置路径的大小），</size>" +
            "<size=15>然后使用 inspector 或直接在 scene 中移动手柄来定位这些点。</size>" +
            "<size=15>对于每个路径元素，你可以指定一个延迟（以秒为单位）。点的顺序将决定对象跟随的顺序。\n对于循环路径，</size>" +
            "<size=15>你可以决定对象是按升序（1, 2, 3...）还是降序（最后，倒数第二，倒数第三...）通过路径中的点。</size>", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 初始移动方向：升序 > 将从点0到1，2等；降序 > 将从最后一个点到最后-1，最后-2等
        public MovementDirection LoopInitialMovementDirection = MovementDirection.Ascending;
        /// 构成对象将跟随的路径的点
        public List<MMPathMovementElement> PathElements;
        /// 另一个MMPath的引用。如果设置了，引用的MMPath的数据将替换这个MMPath的
        public MMPath ReferenceMMPath;
        /// 如果为真，这个对象将移动到参考路径的0位置
        public bool AbsoluteReferencePath = false;
        /// 到达点的最小距离，我们将任意决定已经到达了点
        public float MinDistanceToGoal = .1f;
        /// 如果为真，路径已经到达了它的终点
        public bool EndReached => _endReached;

        [Header(" Gizmos")]
        public bool LockHandlesOnXAxis = false;
        public bool LockHandlesOnYAxis = false;
        public bool LockHandlesOnZAxis = false;

        /// 变换的原始位置，隐藏且不应被访问
        protected Vector3 _originalTransformPosition;
        /// 内部标志，隐藏且不应被访问
        protected bool _originalTransformPositionStatus = false;
        /// 如果为真，对象可以沿着路径移动
        public virtual bool CanMove { get; set; }
        /// 如果为真，这个路径已经通过了它的初始化方法
        public virtual bool Initialized { get; set; }

        public virtual int Direction => _direction;

		protected bool _active=false;
		protected IEnumerator<Vector3> _currentPoint;
		protected int _direction = 1;
		protected Vector3 _initialPosition;
		protected Vector3 _initialPositionThisFrame;
		protected Vector3 _finalPosition;
		protected Vector3 _previousPoint = Vector3.zero;
		protected int _currentIndex;
		protected float _distanceToNextPoint;
		protected bool _endReached = false;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start ()
		{
			if (!Initialized)
			{
				Initialization ();	
			}
		}

		/// <summary>
		/// Flag inits, initial movement determination, and object positioning
		/// </summary>
		public virtual void Initialization()
		{
			// on Start, we set our active flag to true
			_active=true;
			_endReached = false;
			CanMove = true;

			// we copy our reference if needed
			if ((ReferenceMMPath != null) && (ReferenceMMPath.PathElements != null || ReferenceMMPath.PathElements.Count > 0))
			{
				if (AbsoluteReferencePath)
				{
					this.transform.position = ReferenceMMPath.transform.position;
				}
				PathElements = ReferenceMMPath.PathElements;
			}

			// if the path is null we exit
			if (PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			// if the first path element isn't at 0, we offset everything
			if (PathElements[0].PathElementPosition != Vector3.zero)
			{
				Vector3 path0Position = PathElements[0].PathElementPosition;
				this.transform.position += path0Position;
				
				foreach (MMPathMovementElement element in PathElements)
				{
					element.PathElementPosition -= path0Position;
				}
			}

			// we set our initial direction based on the settings
			if (LoopInitialMovementDirection == MovementDirection.Ascending)
			{
				_direction=1;
			}
			else
			{
				_direction=-1;
			}

			// we initialize our path enumerator
			_initialPosition = this.transform.position;
			
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();

			// initial positioning
			if (!_originalTransformPositionStatus)
			{
				_originalTransformPositionStatus = true;
				_originalTransformPosition = transform.position;
			}
			transform.position = _originalTransformPosition + _currentPoint.Current;
		}

		public int CurrentIndex()
		{
			return _currentIndex;
		}

		public Vector3 CurrentPoint()
		{
			return _initialPosition + _currentPoint.Current;
		}

		public Vector3 CurrentPositionRelative()
		{
			return _currentPoint.Current;
		}

		/// <summary>
		/// On update we keep moving along the path
		/// </summary>
		protected virtual void Update () 
		{
			// if the path is null we exit, if we only go once and have reached the end we exit, if we can't move we exit
			if(PathElements == null 
			   || PathElements.Count < 1
			   || _endReached
			   || !CanMove
			  )
			{
				return;
			}

			ComputePath ();
		}

		/// <summary>
		/// Moves the object and determines when a point has been reached
		/// </summary>
		protected virtual void ComputePath()
		{
			// we store our initial position to compute the current speed at the end of the udpate	
			_initialPositionThisFrame = this.transform.position;
            
			// we decide if we've reached our next destination or not, if yes, we move our destination to the next point 
			_distanceToNextPoint = (this.transform.position - (_originalTransformPosition + _currentPoint.Current)).magnitude;
			if(_distanceToNextPoint < MinDistanceToGoal)
			{
				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}

			// we determine the current speed		
			_finalPosition = this.transform.position;
		}

		/// <summary>
		/// Returns the current target point in the path
		/// </summary>
		/// <returns>The path enumerator.</returns>
		public virtual IEnumerator<Vector3> GetPathEnumerator()
		{

			// if the path is null we exit
			if(PathElements == null || PathElements.Count < 1)
			{
				yield break;
			}

			int index = 0;
			_currentIndex = index;
			while (true)
			{
				_currentIndex = index;
				yield return PathElements[index].PathElementPosition;
				
				if(PathElements.Count <= 1)
				{
					continue;
				}

				// if the path is looping
				if (CycleOption == CycleOptions.Loop)
				{
					index = index + _direction;
					if(index < 0)
					{
						index = PathElements.Count-1;
					}
					else if(index > PathElements.Count - 1)
					{
						index = 0;
					}
				}

				if (CycleOption == CycleOptions.BackAndForth)
				{
					if(index <= 0)
					{
						_direction = 1;
					}
					else if(index >= PathElements.Count - 1)
					{
						_direction = -1;
					}
					index = index + _direction;
				}

				if (CycleOption == CycleOptions.OnlyOnce)
				{
					if(index <= 0)
					{
						_direction = 1;
					}
					else if(index >= PathElements.Count - 1)
					{
						_direction = 0;
						_endReached = true;
					}
					index = index + _direction;
				}
			}
		}

		/// <summary>
		/// Call this method to force a change in direction at any time
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = - _direction;
			_currentPoint.MoveNext();
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (PathElements==null)
			{
				return;
			}

			if (PathElements.Count==0)
			{
				return;
			}
							
			// if we haven't stored the object's original position yet, we do it
			if (_originalTransformPositionStatus==false)
			{
				_originalTransformPosition = this.transform.position;
				_originalTransformPositionStatus=true;
			}
			// if we're not in runtime mode and the transform has changed, we update our position
			if (transform.hasChanged && (_active == false))
			{
				_originalTransformPosition = this.transform.position;
			}
			// for each point in the path
			for (int i=0;i<PathElements.Count;i++)
			{
				// we draw a green point 
				MMDebug.DrawGizmoPoint(_originalTransformPosition+PathElements[i].PathElementPosition,0.2f,Color.green);

				// we draw a line towards the next point in the path
				if ((i+1)<PathElements.Count)
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[i].PathElementPosition,_originalTransformPosition+PathElements[i+1].PathElementPosition);
				}
				// we draw a line from the first to the last point if we're looping
				if ( (i == PathElements.Count-1) && (CycleOption == CycleOptions.Loop) )
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[0].PathElementPosition,_originalTransformPosition+PathElements[i].PathElementPosition);
				}
			}

			// if the game is playing, we add a blue point to the destination, and a red point to the last visited point
			if (Application.isPlaying)
			{
				if (_currentPoint != null)
				{
					MMDebug.DrawGizmoPoint(_originalTransformPosition + _currentPoint.Current,0.2f,Color.blue);
					MMDebug.DrawGizmoPoint(_originalTransformPosition + _previousPoint,0.2f,Color.red);	
				}
			}
			#endif


		}

		/// <summary>
		/// Updates the original transform position.
		/// </summary>
		/// <param name="newOriginalTransformPosition">New original transform position.</param>
		public virtual void UpdateOriginalTransformPosition(Vector3 newOriginalTransformPosition)
		{
			_originalTransformPosition = newOriginalTransformPosition;
		}

		/// <summary>
		/// Gets the original transform position.
		/// </summary>
		/// <returns>The original transform position.</returns>
		public virtual Vector3 GetOriginalTransformPosition()
		{
			return _originalTransformPosition;
		}

		/// <summary>
		/// Sets the original transform position status.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void SetOriginalTransformPositionStatus(bool status)
		{
			_originalTransformPositionStatus = status;
		}

		/// <summary>
		/// Gets the original transform position status.
		/// </summary>
		/// <returns><c>true</c>, if original transform position status was gotten, <c>false</c> otherwise.</returns>
		public virtual bool GetOriginalTransformPositionStatus()
		{
			return _originalTransformPositionStatus ;
		}

		/// <summary>
		/// A data structure 
		/// </summary>
		[System.Serializable] public struct Data
		{
			public static Data ForwardLoopingPath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.Loop, Direction = MovementDirection.Ascending
				};
			public static Data ForwardBackAndForthPath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.BackAndForth, Direction = MovementDirection.Ascending
				};
			public static Data ForwardOnlyOncePath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.OnlyOnce, Direction = MovementDirection.Ascending
				};

			public Vector3 Center;
			public Vector3[] Offsets;
			public float Delay;
			public CycleOptions Cycle;
			public MovementDirection Direction;
		}
		
		/// <summary>
		/// Replaces this MMPath's settings with the ones passed in parameters
		/// </summary>
		/// <param name="configuration"></param>
		public void SetPath(in Data configuration)
		{
			if (configuration.Offsets == null) return;

			// same as on Start, we set our active flag to true
			_active = true;
			_endReached = false;
			CanMove = true;

			PathElements = PathElements ?? new List<MMPathMovementElement>(configuration.Offsets.Length);
			PathElements.Clear();

			foreach (var offset in configuration.Offsets)
			{
				PathElements.Add(new MMPathMovementElement() {Delay = configuration.Delay, PathElementPosition = offset});
			}

			// if the path is null we exit
			if (PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			CycleOption = configuration.Cycle;

			// we set our initial direction based on the settings
			if (configuration.Direction == MovementDirection.Ascending)
			{
				_direction = 1;
			}
			else
			{
				_direction = -1;
			}

			_initialPosition = configuration.Center;
			_originalTransformPosition = configuration.Center;
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();
		}
	}
}