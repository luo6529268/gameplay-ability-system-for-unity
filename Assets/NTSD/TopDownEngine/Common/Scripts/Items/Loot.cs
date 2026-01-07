using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class meant to spawn objects (usually item pickers, but not necessarily)
    /// The spawn can be triggered by any script, at any time, and comes with automatic hooks
    /// to trigger loot on damage or death
    /// 一个用于生成对象（通常是物品拾取器，但不限于此）的类
    /// 可以通过任何脚本在任何时候触发生成，并自动挂钩
    /// 在受到伤害或死亡时触发战利品
    /// </summary>
    public class Loot : TopDownMonoBehaviour
	{
        /// the possible modes by which loot can be defined  <summary>
        /// 定义战利品的模式
		/// Unique 唯一性
        /// </summary>
        public enum LootModes { Unique, LootTable, LootTableScriptableObject }

		[Header("Loot Mode")]
        /// <summary>
        /// 选择的战利品模式：
        /// - 唯一：一个简单的对象
        /// - 战利品表：特定于此战利品对象的LootTable
        /// - 战利品定义：一个LootTable可脚本化对象（通过右键 > 创建 > MoreMountains > TopDown Engine > Loot Definition创建。然后可以在其他战利品对象中重用此战利品定义）。
        /// </summary>
        [Tooltip("选择的战利品模式：- " +
			"唯一：一个简单的对象 - 战利品表：特定于此战利品对象的LootTable - 战利品定义：一个LootTable可脚本化对象（通过右键 > 创建 > MoreMountains > TopDown Engine > Loot Definition创建。" +
			"然后可以在其他战利品对象中重用此战利品定义）。")]
        public LootModes LootMode = LootModes.Unique;

        /// <summary>
        /// 在LootMode下要拾取的对象
        /// </summary>
        [Tooltip("在LootMode下要拾取的对象")]
        [MMEnumCondition("LootMode", (int)LootModes.Unique)]
        public GameObject GameObjectToLoot;

        /// <summary>
        /// 定义要生成哪些对象的战利品表
        /// </summary>
        [Tooltip("定义要生成哪些对象的战利品表")]
        [MMEnumCondition("LootMode", (int)LootModes.LootTable)]
        public MMLootTableGameObject LootTable;

        /// <summary>
        /// 定义要生成哪些对象的战利品表可脚本化对象
        /// </summary>
        [Tooltip("定义要生成哪些对象的战利品表可脚本化对象")]
        [MMEnumCondition("LootMode", (int)LootModes.LootTableScriptableObject)]
        public MMLootTableGameObjectSO LootTableSO;

        [Header("条件")]
        /// <summary>
        /// 如果为true，则此对象死亡时会发生战利品生成
        /// </summary>
        [Tooltip("如果为true，则此对象死亡时会发生战利品生成")]
        public bool SpawnLootOnDeath = true;
        /// <summary>
        /// 如果为true，则此对象受到伤害时会发生战利品生成
        /// </summary>
        [Tooltip("如果为true，则此对象受到伤害时会发生战利品生成")]
        public bool SpawnLootOnDamage = false;

        [Header("池化")]
        /// <summary>
        /// 如果为true，则战利品将被池化
        /// </summary>
        [Tooltip("如果为true，则战利品将被池化")]
        public bool PoolLoot = false;
        /// <summary>
        /// 确定战利品表中每个对象的池大小
        /// </summary>
        [Tooltip("确定战利品表中每个对象的池大小")]
        [MMCondition("PoolLoot", true)]
        public int PoolSize = 20;
        /// <summary>
        /// 此池的唯一名称，如果你想要在共享相同战利品表的所有战利品对象之间共享它们的池，则必须在它们之间共享
        /// </summary>
        [Tooltip("此池的唯一名称，如果你想要在共享相同战利品表的所有战利品对象之间共享它们的池，则必须在它们之间共享")]
        [MMCondition("PoolLoot", true)]
        public string MutualizedPoolName = "";

        [Header("生成")]
        /// <summary>
        /// 如果为false，则不会发生生成
        /// </summary>
        [Tooltip("如果为false，则不会发生生成")]
        public bool CanSpawn = true;
        /// <summary>
        /// 在生成战利品前等待的延迟（以秒为单位）
        /// </summary>
        [Tooltip("在生成战利品前等待的延迟（以秒为单位）")]
        public float Delay = 0f;
        /// <summary>
        /// 生成对象的最小和最大数量
        /// </summary>
        [Tooltip("生成对象的最小和最大数量")]
        [MMVector("Min", "Max")]
        public Vector2 Quantity = Vector2.one;

        /// <summary>
        /// 对象应该生成的位置、旋转和缩放
        /// </summary>
        [Tooltip("对象应该生成的位置、旋转和缩放")]
        public MMSpawnAroundProperties SpawnProperties;
        /// <summary>
        /// 如果为true，则战利品将限制在最大数量，任何新的战利品尝试超出该数量将不会有结果。如果为false，则战利品是无限的，可以永远发生。
        /// </summary>
        [Tooltip("如果为true，则战利品将限制在最大数量，任何新的战利品尝试超出该数量将不会有结果。如果为false，则战利品是无限的，可以永远发生。")]
        public bool LimitedLootQuantity = true;
        /// <summary>
        /// 从此战利品对象可以拾取的最大对象数量
        /// </summary>
        [Tooltip("从此战利品对象可以拾取的最大对象数量")]
        [MMCondition("LimitedLootQuantity", true)]
        public int MaximumQuantity = 100;

        /// <summary>
        /// 从此战利品对象可以拾取的剩余对象数量，用于调试目的
        /// </summary>
        [Tooltip("从此战利品对象可以拾取的剩余对象数量，用于调试目的")]
        [MMReadOnly]
        public int RemainingQuantity = 100;

        [Header("碰撞")]
        /// <summary>
        /// 生成的对象是否应该尝试避开障碍物
        /// </summary>
        [Tooltip("生成的对象是否应该尝试避开障碍物")]
        public bool AvoidObstacles = false;
        /// <summary>
        /// 碰撞检测可以操作的模式
        /// </summary>
        public enum DimensionModes { TwoD, ThreeD }
        /// <summary>
        /// 碰撞检测应该在2D还是3D中发生
        /// </summary>
        [Tooltip("碰撞检测应该在2D还是3D中发生")]
        [MMCondition("AvoidObstacles", true)]
        public DimensionModes DimensionMode = DimensionModes.TwoD;

        /// <summary>
        /// 包含生成的对象不应该与之碰撞的层的层掩码
        /// </summary>
        [Tooltip("包含生成的对象不应该与之碰撞的层的层掩码")]
        [MMCondition("AvoidObstacles", true)]
        public LayerMask AvoidObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        /// <summary>
        /// 在对象周围没有障碍物的半径
        /// </summary>
        [Tooltip("在对象周围没有障碍物的半径")]
        [MMCondition("AvoidObstacles", true)]
        public float AvoidRadius = 0.25f;
        /// <summary>
        /// 如果最后一个位置在障碍物内，脚本应该尝试为战利品寻找另一个位置的次数。尝试次数越多：结果越好，成本越高
        /// </summary>
        [Tooltip("如果最后一个位置在障碍物内，脚本应该尝试为战利品寻找另一个位置的次数。尝试次数越多：结果越好，成本越高")]
        [MMCondition("AvoidObstacles", true)]
        public int MaxAvoidAttempts = 5;

        [Header("反馈")]
        /// <summary>
        /// 在生成战利品时播放的MMFeedbacks。只会播放一个反馈。如果你想每个物品一个，最好将其放在物品本身上，并在对象实例化时播放。
        /// </summary>
        [Tooltip("在生成战利品时播放的MMFeedbacks。只会播放一个反馈。如果你想每个物品一个，最好将其放在物品本身上，并在对象实例化时播放。")]
        public MMFeedbacks LootFeedback;

        [Header("调试")]
        /// <summary>
        /// 如果为true，则会绘制gizmo以显示战利品将生成的形状
        /// </summary>
        [Tooltip("如果为true，则会绘制gizmo以显示战利品将生成的形状")]
        public bool DrawGizmos = false;
        /// <summary>
        /// 绘制的gizmo数量
        /// </summary>
        [Tooltip("绘制的gizmo数量")]
        public int GizmosQuantity = 1000;
        /// <summary>
        /// 绘制gizmo的颜色
        /// </summary>
        [Tooltip("绘制gizmo的颜色")]
        public Color GizmosColor = MMColors.LightGray;
        /// <summary>
        /// 绘制gizmo的大小
        /// </summary>
        [Tooltip("绘制gizmo的大小")]
        public float GimosSize = 1f;
        /// <summary>
        /// 用于触发战利品的调试按钮
        /// </summary>
        [Tooltip("用于触发战利品的调试按钮")]
        [MMInspectorButton("SpawnLootDebug")]
        public bool SpawnLootButton;

        public static List<MMSimpleObjectPooler> SimplePoolers = new List<MMSimpleObjectPooler>();
		public static List<MMMultipleObjectPooler> MultiplePoolers = new List<MMMultipleObjectPooler>();
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			SimplePoolers = new List<MMSimpleObjectPooler>();
			MultiplePoolers = new List<MMMultipleObjectPooler>();
		}

		protected Health _health;
		protected GameObject _objectToSpawn;
		protected GameObject _spawnedObject;
		protected Vector3 _raycastOrigin;
		protected RaycastHit2D _raycastHit2D;
		protected Collider[] _overlapBox;
		protected MMSimpleObjectPooler _simplePooler;
		protected MMMultipleObjectPooler _multipleObjectPooler;
        
		/// <summary>
		/// On Awake we grab the health component if there's one, and initialize our loot table
		/// </summary>
		protected virtual void Awake()
		{
			_health = this.gameObject.GetComponentInParent<Health>();
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInChildren<Health>();
			}
			InitializeLootTable();
			InitializePools();
			ResetRemainingQuantity();
		}

		/// <summary>
		/// Resets the remaining quantity to the maximum quantity
		/// </summary>
		public virtual void ResetRemainingQuantity()
		{
			RemainingQuantity = MaximumQuantity;
		}

		/// <summary>
		/// Computes the associated loot table's weights
		/// </summary>
		public virtual void InitializeLootTable()
		{
			switch (LootMode)
			{
				case LootModes.LootTableScriptableObject:
					if (LootTableSO != null)
					{
						LootTableSO.ComputeWeights();
					}
					break;
				case LootModes.LootTable:
					LootTable.ComputeWeights();
					break;
			}
		}

		protected virtual void InitializePools()
		{
			if (!PoolLoot)
			{
				return;
			}

			switch (LootMode)
			{
				case LootModes.Unique:
					_simplePooler = FindSimplePooler();
					break;
				case LootModes.LootTable:
					_multipleObjectPooler = FindMultiplePooler();
					break;
				case LootModes.LootTableScriptableObject:
					_multipleObjectPooler = FindMultiplePooler();
					break;
			}
		}

		protected virtual MMSimpleObjectPooler FindSimplePooler()
		{
			foreach (MMSimpleObjectPooler simplePooler in SimplePoolers)
			{
				if (simplePooler.GameObjectToPool == GameObjectToLoot)
				{
					return simplePooler;
				}
			}
			// if we haven't found one, we create one
			GameObject newObject = new GameObject("[MMSimpleObjectPooler] "+GameObjectToLoot.name);
			MMSimpleObjectPooler pooler = newObject.AddComponent<MMSimpleObjectPooler>();
			pooler.GameObjectToPool = GameObjectToLoot;
			pooler.PoolSize = PoolSize;
			pooler.NestUnderThis = true;
			pooler.FillObjectPool();            
			pooler.Owner = SimplePoolers;
			SimplePoolers.Add(pooler);
			return pooler;
		}
        
		protected virtual MMMultipleObjectPooler FindMultiplePooler()
		{
			foreach (MMMultipleObjectPooler multiplePooler in MultiplePoolers)
			{
				if ((multiplePooler != null) && (multiplePooler.MutualizedPoolName == MutualizedPoolName)) 
				{
					return multiplePooler;
				}
			}
			// if we haven't found one, we create one
			GameObject newObject = new GameObject("[MMMultipleObjectPooler] "+MutualizedPoolName);
			MMMultipleObjectPooler pooler = newObject.AddComponent<MMMultipleObjectPooler>();
			pooler.MutualizeWaitingPools = true;
			pooler.MutualizedPoolName = MutualizedPoolName;
			pooler.NestUnderThis = true;
			pooler.Pool = new List<MMMultipleObjectPoolerObject>();
			if (LootMode == LootModes.LootTable)
			{
				foreach (MMLootGameObject loot in LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject();
					objectToPool.PoolSize = PoolSize * (int)loot.Weight;
					objectToPool.GameObjectToPool = loot.Loot;
					pooler.Pool.Add(objectToPool);
				}
			}
			else if (LootMode == LootModes.LootTableScriptableObject)
			{
				foreach (MMLootGameObject loot in LootTableSO.LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject
					{
						PoolSize = PoolSize * (int)loot.Weight,
						GameObjectToPool = loot.Loot
					};
					pooler.Pool.Add(objectToPool);
				}
			}
			pooler.FillObjectPool();
			pooler.Owner = MultiplePoolers;
			MultiplePoolers.Add(pooler);
			return pooler;
		}

		/// <summary>
		/// This method spawns the specified loot after applying a delay (if there's one)
		/// </summary>
		public virtual void SpawnLoot()
		{
			if (!CanSpawn)
			{
				return;
			}
			StartCoroutine(SpawnLootCo());
		}

		/// <summary>
		/// A debug method called by the inspector button
		/// </summary>
		protected virtual void SpawnLootDebug()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("This debug button is only meant to be used while in Play Mode.");
				return;
			}

			SpawnLoot();
		}

		/// <summary>
		/// A coroutine used to spawn loot after a delay
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpawnLootCo()
		{
			yield return MMCoroutine.WaitFor(Delay);
			int randomQuantity = Random.Range((int)Quantity.x, (int)Quantity.y + 1);
			for (int i = 0; i < randomQuantity; i++)
			{
				SpawnOneLoot();
			}
			LootFeedback?.PlayFeedbacks();
		}

		protected virtual void Spawn(GameObject gameObjectToSpawn)
		{
			if (PoolLoot)
			{
				switch (LootMode)
				{
					case LootModes.Unique:
						_spawnedObject = _simplePooler.GetPooledGameObject();
						break;
					case LootModes.LootTable: case LootModes.LootTableScriptableObject:
						_spawnedObject = _multipleObjectPooler.GetPooledGameObject();
						break;
				}
			}
			else
			{
				_spawnedObject = Instantiate(gameObjectToSpawn);    
			}
		}

		/// <summary>
		/// Spawns a single loot object, without delay, and regardless of the defined quantities 
		/// </summary>
		public virtual void SpawnOneLoot()
		{
			_objectToSpawn = GetObject();

			if (_objectToSpawn == null)
			{
				return;
			}

			if (LimitedLootQuantity && (RemainingQuantity <= 0))
			{
				return;
			}

			Spawn(_objectToSpawn);

			if (AvoidObstacles)
			{
				bool placementOK = false;
				int amountOfAttempts = 0;
				while (!placementOK && (amountOfAttempts < MaxAvoidAttempts))
				{
					MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);
                    
					if (DimensionMode == DimensionModes.TwoD)
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_raycastHit2D = Physics2D.BoxCast(_raycastOrigin + Vector3.right * AvoidRadius, AvoidRadius * Vector2.one, 0f, Vector2.left, AvoidRadius, AvoidObstaclesLayerMask);
						if (_raycastHit2D.collider == null)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
					else
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_overlapBox = Physics.OverlapBox(_raycastOrigin, Vector3.one * AvoidRadius, Quaternion.identity, AvoidObstaclesLayerMask);
                        
						if (_overlapBox.Length == 0)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
				}
			}
			else
			{
				MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);    
			}
			if (_spawnedObject != null)
			{
				_spawnedObject.gameObject.SetActive(true);
			}
			_spawnedObject.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

			if (LimitedLootQuantity)
			{
				RemainingQuantity--;	
			}
		}

		/// <summary>
		/// Gets the object that should be spawned
		/// </summary>
		/// <returns></returns>
		protected virtual GameObject GetObject()
		{
			_objectToSpawn = null;
			switch (LootMode)
			{
				case LootModes.Unique:
					_objectToSpawn = GameObjectToLoot;
					break;
				case LootModes.LootTableScriptableObject:
					if (LootTableSO == null)
					{
						_objectToSpawn = null;
						break;
					}
					_objectToSpawn = LootTableSO.GetLoot();
					break;
				case LootModes.LootTable:
					_objectToSpawn = LootTable.GetLoot()?.Loot;
					break;
			}

			return _objectToSpawn;
		}

		/// <summary>
		/// On hit, we spawn loot if needed
		/// </summary>
		protected virtual void OnHit()
		{
			if (!SpawnLootOnDamage)
			{
				return;
			}

			SpawnLoot();
		}
        
		/// <summary>
		/// On death, we spawn loot if needed
		/// </summary>
		protected virtual void OnDeath()
		{
			if (!SpawnLootOnDeath)
			{
				return;
			}

			SpawnLoot();
		}
        
		/// <summary>
		/// OnEnable we start listening for death and hit if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnHit += OnHit;
			}
		}

		/// <summary>
		/// OnDisable we stop listening for death and hit if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnHit -= OnHit;
			}
		}
        
		/// <summary>
		/// OnDrawGizmos, we display the shape at which objects will spawn when looted
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (DrawGizmos)
			{
				MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GimosSize, GizmosColor);    
			}
		}

	}
}