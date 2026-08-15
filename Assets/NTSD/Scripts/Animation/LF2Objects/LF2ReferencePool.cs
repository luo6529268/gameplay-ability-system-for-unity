using MoreMountains.Tools;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Unity loading host for the pure managed battle reference pool. Runtime
    /// simulation receives <see cref="SimulationCore"/> before the battle seal
    /// and does not access this MonoBehaviour from its worker thread.
    /// </summary>
    public class LF2ReferencePool : MMSingleton<LF2ReferencePool>
    {
        [Header("预热配置")]
        [SerializeField] private int _initialPoolSize = 50;

        private BattleLogicReferencePool simulationCore;

        public BattleLogicReferencePool SimulationCore => EnsureCore();
        public bool IsBattleCapacitySealed => EnsureCore().IsBattleCapacitySealed;
        public long RejectedLogicObjectFetchCount =>
            EnsureCore().RejectedLogicObjectFetchCount;
        public long RejectedTaskFetchCount => EnsureCore().RejectedTaskFetchCount;
        public long RejectedUnknownTaskRecycleCount =>
            EnsureCore().RejectedUnknownTaskRecycleCount;
        public int ActiveCount => EnsureCore().ActiveCount;
        public int AvailableCreateTaskCountForDiagnostics =>
            EnsureCore().AvailableCreateTaskCount;
        public int AvailableCreateMultipleTaskCountForDiagnostics =>
            EnsureCore().AvailableCreateMultipleTaskCount;

        public long GetRejectedLogicObjectFetchCountForDiagnostics(
            LF2ObjectType objectType)
        {
            return EnsureCore().GetRejectedLogicObjectFetchCount(objectType);
        }

        protected override void Awake()
        {
            base.Awake();
            BattleLogicReferencePool core = EnsureCore();
            core.PrewarmDefaults(_initialPoolSize);
            Log.Info(
                "[LF2ReferencePool] Prewarmed: {0} logic objects",
                _initialPoolSize + 10);
        }

        public ILF2Object Get(LF2ObjectType objectType, int objectId)
        {
            return EnsureCore().Get(objectType, objectId);
        }

        public void Release(ILF2Object value)
        {
            EnsureCore().Release(value);
        }

        public void Prewarm(LF2ObjectType objectType, int count)
        {
            EnsureCore().Prewarm(objectType, count);
        }

        public void PrepareObjectCapacity(
            LF2ObjectType objectType,
            int targetTotalCount)
        {
            EnsureCore().PrepareObjectCapacity(objectType, targetTotalCount);
        }

        public int GetAvailableCount(LF2ObjectType objectType)
        {
            return EnsureCore().GetAvailableCount(objectType);
        }

        public void PrewarmTasks<T>(int count)
            where T : class, ILF2Recyclable, new()
        {
            EnsureCore().PrewarmTasks<T>(count);
        }

        public T Fetch<T>() where T : class, ILF2Recyclable, new()
        {
            return EnsureCore().Fetch<T>();
        }

        public void Recycle(ILF2Recyclable value)
        {
            EnsureCore().Recycle(value);
        }

        public void SealBattleCapacity()
        {
            EnsureCore().SealBattleCapacity();
        }

        public void UnsealBattleCapacity()
        {
            EnsureCore().UnsealBattleCapacity();
        }

        private BattleLogicReferencePool EnsureCore()
        {
            simulationCore ??= new BattleLogicReferencePool();
            return simulationCore;
        }
    }
}
