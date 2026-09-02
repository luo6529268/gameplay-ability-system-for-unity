using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation
{
    /// <summary>
    /// Pure managed logic/task pool. One simulation owner may mutate it after
    /// the battle seal; Unity objects and MonoBehaviour lifetime are not part of
    /// this state.
    /// </summary>
    public sealed class BattleLogicReferencePool
    {
        private const int DefaultAvailableQueueCapacity =
            BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;

        private readonly Dictionary<LF2ObjectType, Queue<ILF2Object>> availablePools =
            new Dictionary<LF2ObjectType, Queue<ILF2Object>>(7);
        private readonly HashSet<ILF2Object> activeObjects =
            new HashSet<ILF2Object>(DefaultAvailableQueueCapacity);
        private Stack<OPointCreateTask> createTaskPool;
        private Stack<OPointCreateMultipleTask> createMultipleTaskPool;
        private bool battleCapacitySealed;
        private long rejectedLogicObjectFetchCount;
        private readonly long[] rejectedLogicObjectFetchCountByType = new long[7];
        private long rejectedTaskFetchCount;
        private long rejectedUnknownTaskRecycleCount;

        public BattleLogicReferencePool()
        {
            EnsureObjectTypeQueues();
        }

        public bool IsBattleCapacitySealed => battleCapacitySealed;
        public long RejectedLogicObjectFetchCount => rejectedLogicObjectFetchCount;
        public long RejectedTaskFetchCount => rejectedTaskFetchCount;
        public long RejectedUnknownTaskRecycleCount => rejectedUnknownTaskRecycleCount;
        public int ActiveCount => activeObjects.Count;
        public int AvailableCreateTaskCount => createTaskPool?.Count ?? 0;
        public int AvailableCreateMultipleTaskCount => createMultipleTaskPool?.Count ?? 0;

        public long GetRejectedLogicObjectFetchCount(LF2ObjectType objectType)
        {
            int typeIndex = (int)objectType;
            return (uint)typeIndex < (uint)rejectedLogicObjectFetchCountByType.Length
                ? rejectedLogicObjectFetchCountByType[typeIndex]
                : 0L;
        }

        public ILF2Object Get(LF2ObjectType objectType, int objectId)
        {
            return Get(objectType, objectId, null);
        }

        internal ILF2Object Get(
            LF2ObjectType objectType,
            int objectId,
            SimulationWorld resetWorld)
        {
            ILF2Object value = TakeAvailableObject(objectType);
            if (value == null)
            {
                if (battleCapacitySealed)
                {
                    rejectedLogicObjectFetchCount++;
                    int typeIndex = (int)objectType;
                    if ((uint)typeIndex < (uint)rejectedLogicObjectFetchCountByType.Length)
                        rejectedLogicObjectFetchCountByType[typeIndex]++;
                    return null;
                }

                value = CreateNewObject(objectType);
            }

            if (value == null)
                return null;

            if (value is LF2Weapon pooledWeapon && IsWeaponType(objectType))
                pooledWeapon.SetWeaponType((int)objectType);

            if (value is LF2Entity entity && resetWorld != null)
                entity.BindRegisteredWorldForSnapshotRestore(resetWorld);
            value.Reset();
            value.ObjectId = objectId;
            activeObjects.Add(value);
            return value;
        }

        internal ILF2Object GetSnapshotShell(
            BattleRuntimeEntityKind entityKind,
            int objectId,
            int poolWeaponType,
            SimulationWorld resetWorld)
        {
            LF2ObjectType objectType;
            switch (entityKind)
            {
                case BattleRuntimeEntityKind.Character:
                    objectType = LF2ObjectType.Character;
                    break;
                case BattleRuntimeEntityKind.Weapon:
                    objectType = poolWeaponType switch
                    {
                        2 => LF2ObjectType.HeavyWeapon,
                        4 => LF2ObjectType.ThrowWeapon,
                        6 => LF2ObjectType.Drink,
                        _ => LF2ObjectType.LightWeapon,
                    };
                    break;
                case BattleRuntimeEntityKind.SpecialAttack:
                    objectType = LF2ObjectType.SpecialAttack;
                    break;
                case BattleRuntimeEntityKind.Other:
                    objectType = LF2ObjectType.Other;
                    break;
                default:
                    return null;
            }

            return Get(objectType, objectId, resetWorld);
        }

        public void Release(ILF2Object value)
        {
            if (value == null || !activeObjects.Remove(value))
                return;

            if (availablePools.TryGetValue(
                    value.ObjectTypeEnum,
                    out Queue<ILF2Object> queue))
            {
                queue.Enqueue(value);
            }
        }

        public void PrewarmDefaults(int initialPoolSize)
        {
            if (battleCapacitySealed)
                return;

            for (int index = 0; index < initialPoolSize / 3; index++)
                AddToPool(LF2ObjectType.LightWeapon);
            for (int index = 0; index < initialPoolSize / 3; index++)
                AddToPool(LF2ObjectType.HeavyWeapon);
            for (int index = 0; index < initialPoolSize / 3; index++)
                AddToPool(LF2ObjectType.SpecialAttack);
            for (int index = 0; index < initialPoolSize / 6; index++)
                AddToPool(LF2ObjectType.ThrowWeapon);
            for (int index = 0; index < initialPoolSize / 6; index++)
                AddToPool(LF2ObjectType.Other);
            for (int index = 0; index < 10; index++)
                AddToPool(LF2ObjectType.Character);
        }

        public void Prewarm(LF2ObjectType objectType, int count)
        {
            if (battleCapacitySealed || count <= 0)
                return;

            for (int index = 0; index < count; index++)
                AddToPool(objectType);
        }

        public void PrepareObjectCapacity(
            LF2ObjectType objectType,
            int targetTotalCount)
        {
            if (battleCapacitySealed || targetTotalCount <= 0)
                return;

            int totalCount = GetAvailableCount(objectType);
            foreach (ILF2Object activeObject in activeObjects)
            {
                if (activeObject != null && activeObject.ObjectTypeEnum == objectType)
                    totalCount++;
            }

            int missing = targetTotalCount - totalCount;
            for (int index = 0; index < missing; index++)
                AddToPool(objectType);
            activeObjects.EnsureCapacity(targetTotalCount);
        }

        public void PrepareBattleEntityShellCapacity(int targetTotalCount)
        {
            if (battleCapacitySealed || targetTotalCount <= 0)
                return;

            PrepareObjectCapacity(LF2ObjectType.Character, targetTotalCount);
            PrepareObjectCapacity(LF2ObjectType.SpecialAttack, targetTotalCount);
            PrepareObjectCapacity(LF2ObjectType.Other, targetTotalCount);
            PrepareWeaponFamilyCapacity(targetTotalCount);
        }

        public int GetAvailableCount(LF2ObjectType objectType)
        {
            return availablePools.TryGetValue(
                objectType,
                out Queue<ILF2Object> queue)
                ? queue.Count
                : 0;
        }

        public void PrewarmTasks<T>(int count)
            where T : class, ILF2Recyclable, new()
        {
            if (battleCapacitySealed || count <= 0)
                return;

            if (typeof(T) == typeof(OPointCreateTask))
            {
                createTaskPool ??= new Stack<OPointCreateTask>(count);
                int missing = count - createTaskPool.Count;
                for (int index = 0; index < missing; index++)
                {
                    var task = new OPointCreateTask();
                    task.Clear();
                    createTaskPool.Push(task);
                }
                return;
            }

            if (typeof(T) == typeof(OPointCreateMultipleTask))
            {
                createMultipleTaskPool ??= new Stack<OPointCreateMultipleTask>(count);
                int missing = count - createMultipleTaskPool.Count;
                for (int index = 0; index < missing; index++)
                {
                    var task = new OPointCreateMultipleTask();
                    task.Clear();
                    createMultipleTaskPool.Push(task);
                }
            }
        }

        public T Fetch<T>() where T : class, ILF2Recyclable, new()
        {
            if (typeof(T) == typeof(OPointCreateTask) &&
                createTaskPool != null &&
                createTaskPool.Count > 0)
            {
                T value = createTaskPool.Pop() as T;
                value.IsFromPool = true;
                return value;
            }

            if (typeof(T) == typeof(OPointCreateMultipleTask) &&
                createMultipleTaskPool != null &&
                createMultipleTaskPool.Count > 0)
            {
                T value = createMultipleTaskPool.Pop() as T;
                value.IsFromPool = true;
                return value;
            }

            if (battleCapacitySealed)
            {
                rejectedTaskFetchCount++;
                return null;
            }

            return new T { IsFromPool = true };
        }

        public void Recycle(ILF2Recyclable value)
        {
            if (value == null || !value.IsFromPool)
                return;

            value.IsFromPool = false;
            value.Clear();
            if (value is OPointCreateTask createTask)
            {
                if (createTaskPool == null)
                {
                    if (battleCapacitySealed)
                    {
                        rejectedUnknownTaskRecycleCount++;
                        return;
                    }
                    createTaskPool = new Stack<OPointCreateTask>();
                }
                createTaskPool.Push(createTask);
                return;
            }

            if (value is OPointCreateMultipleTask createMultipleTask)
            {
                if (createMultipleTaskPool == null)
                {
                    if (battleCapacitySealed)
                    {
                        rejectedUnknownTaskRecycleCount++;
                        return;
                    }
                    createMultipleTaskPool = new Stack<OPointCreateMultipleTask>();
                }
                createMultipleTaskPool.Push(createMultipleTask);
                return;
            }

            rejectedUnknownTaskRecycleCount++;
        }

        public void SealBattleCapacity()
        {
            battleCapacitySealed = true;
        }

        public void UnsealBattleCapacity()
        {
            battleCapacitySealed = false;
        }

        private void EnsureObjectTypeQueues()
        {
            AddQueue(LF2ObjectType.LightWeapon);
            AddQueue(LF2ObjectType.HeavyWeapon);
            AddQueue(LF2ObjectType.SpecialAttack);
            AddQueue(LF2ObjectType.ThrowWeapon);
            AddQueue(LF2ObjectType.Drink);
            AddQueue(LF2ObjectType.Character);
            AddQueue(LF2ObjectType.Other);
        }

        private void AddQueue(LF2ObjectType objectType)
        {
            availablePools[objectType] =
                new Queue<ILF2Object>(DefaultAvailableQueueCapacity);
        }

        private void AddToPool(LF2ObjectType objectType)
        {
            ILF2Object value = CreateNewObject(objectType);
            if (value != null &&
                availablePools.TryGetValue(
                    objectType,
                    out Queue<ILF2Object> queue))
            {
                queue.Enqueue(value);
            }
        }

        private ILF2Object TakeAvailableObject(LF2ObjectType objectType)
        {
            if (availablePools.TryGetValue(objectType, out Queue<ILF2Object> queue) &&
                queue.Count > 0)
            {
                return queue.Dequeue();
            }

            if (!IsWeaponType(objectType))
                return null;

            return TryTakeAvailableWeapon(LF2ObjectType.LightWeapon) ??
                   TryTakeAvailableWeapon(LF2ObjectType.HeavyWeapon) ??
                   TryTakeAvailableWeapon(LF2ObjectType.ThrowWeapon) ??
                   TryTakeAvailableWeapon(LF2ObjectType.Drink);
        }

        private ILF2Object TryTakeAvailableWeapon(LF2ObjectType objectType)
        {
            return availablePools.TryGetValue(
                       objectType,
                       out Queue<ILF2Object> queue) &&
                   queue.Count > 0
                ? queue.Dequeue()
                : null;
        }

        private void PrepareWeaponFamilyCapacity(int targetTotalCount)
        {
            int totalCount = 0;
            foreach (KeyValuePair<LF2ObjectType, Queue<ILF2Object>> pair in availablePools)
            {
                if (IsWeaponType(pair.Key))
                    totalCount += pair.Value.Count;
            }
            foreach (ILF2Object activeObject in activeObjects)
            {
                if (activeObject != null && IsWeaponType(activeObject.ObjectTypeEnum))
                    totalCount++;
            }

            int missing = targetTotalCount - totalCount;
            for (int index = 0; index < missing; index++)
                AddToPool(LF2ObjectType.LightWeapon);
            activeObjects.EnsureCapacity(targetTotalCount);
        }

        private static bool IsWeaponType(LF2ObjectType objectType)
        {
            return objectType == LF2ObjectType.LightWeapon ||
                   objectType == LF2ObjectType.HeavyWeapon ||
                   objectType == LF2ObjectType.ThrowWeapon ||
                   objectType == LF2ObjectType.Drink;
        }

        private static ILF2Object CreateNewObject(LF2ObjectType objectType)
        {
            switch (objectType)
            {
                case LF2ObjectType.LightWeapon:
                    var lightWeapon = new LF2Weapon();
                    lightWeapon.SetWeaponType(1);
                    return lightWeapon;
                case LF2ObjectType.HeavyWeapon:
                    var heavyWeapon = new LF2Weapon();
                    heavyWeapon.SetWeaponType(2);
                    return heavyWeapon;
                case LF2ObjectType.ThrowWeapon:
                    var throwWeapon = new LF2Weapon();
                    throwWeapon.SetWeaponType(4);
                    return throwWeapon;
                case LF2ObjectType.SpecialAttack:
                    return new LF2SpecialAttack();
                case LF2ObjectType.Drink:
                    var drinkWeapon = new LF2Weapon();
                    drinkWeapon.SetWeaponType(6);
                    return drinkWeapon;
                case LF2ObjectType.Character:
                    return new LF2Character();
                case LF2ObjectType.Other:
                    return new LF2OtherObject();
                default:
                    return null;
            }
        }
    }
}
