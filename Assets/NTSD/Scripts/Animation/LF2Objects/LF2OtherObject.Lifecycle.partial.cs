using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2OtherObjectLifecycleModule
    {
        private readonly LF2OtherObject owner;
        private readonly LF2OtherObjectFrameModule frameModule;

        public long InvalidTaskTypeCountForDiagnostics { get; private set; }

        internal bool TryRestoreInvalidTaskTypeCountForSnapshot(long value)
        {
            if (value < 0)
                return false;
            InvalidTaskTypeCountForDiagnostics = value;
            return true;
        }

        public LF2OtherObjectLifecycleModule(
            LF2OtherObject owner,
            LF2OtherObjectFrameModule frameModule)
        {
            this.owner = owner;
            this.frameModule = frameModule;
        }

        public void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            owner.PS.BindRuntime(owner.Runtime);
            owner.Health.BindRuntime(owner.Runtime);
            owner.AssignRendererFromLifecycle(renderer);
            owner.GrabbedBy = 0;

            if (taskBase is not OPointCreateTask task)
            {
                InvalidTaskTypeCountForDiagnostics++;
                if (owner.Match?.RuntimeCapacity.IsSealed != true)
                    Log.Error("[LF2OtherObject] Invalid task type");
                return;
            }

            owner.Runtime.SpawnSemantic = (int)task.releaseSpawnSemantic;

            InitializeParent(task);
            owner.ApplyInitialRuntimePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();

            SimulationWorld world = task.targetWorld ??
                                    task.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            world?.Register(owner);
        }

        public void Reset()
        {
            owner.FrameCache.Clear();
            owner.ResetPooledEntityState();
            owner.Runtime.Reset();
            owner.ResetReusableRuntimeComponentsFromLifecycle();
            owner.ObjectId = 0;
            owner.Team = 0;
            owner.RelationTeam = 0;
            owner.Health.HP = 0;
            owner.Health.HPBound = 0;
            owner.Health.HP3 = 0;
            owner.Health.MP = 0;
            owner.Health.PP = 0;
            owner.Health.MaxPP = 0;
            owner.Health.PPBound = 0;
            owner.Health.MaxMP = 0;
            owner.GrabbedBy = 0;
            owner.HolderCopySlot = -1;
            owner.OwnerId = -1;
            owner.RelationOwnerSlot = -1;
            owner.OwnerEntityIndex = -1;
            owner.SpawnerEntityIndex = -1;
            owner.TrackerFlag = 0;
            owner.TrackerParent = null;
            owner.Runtime.LinkState = 0;
            owner.Runtime.TargetSlotIndex = -1;
            owner.Runtime.HeldWeaponStableId = -1;
            owner.Runtime.HolderStableId = -1;
            owner.Runtime.PickerStableId = -1;
            owner.ResetSparkFromLifecycle();
            owner.ResetStableIdFromLifecycle();
        }

        private void InitializeParent(OPointCreateTask task)
        {
            owner.ObjectId = task.opoint.oid;
            owner.Team = 0;
            owner.RelationTeam = 0;
            owner.HolderCopySlot = -1;
            owner.OwnerId = -1;
            owner.RelationOwnerSlot = -1;
            owner.OwnerEntityIndex = -1;
            owner.SpawnerEntityIndex = -1;
        }

        private void InitializeDirection(OPointCreateTask task)
        {
            string dir = owner.CalculateDirectionFromLifecycle(
                task.opoint.facing,
                task.dir);
            owner.SwitchDir(string.IsNullOrEmpty(dir) ? "right" : dir);
        }

        private void InitializeFrame(OPointCreateTask task)
        {
            LF2CharacterDataWrapper wrapper =
                owner.ResolveRuntimeCharacterConfig(owner.ObjectId);
            owner.FrameCache.Load(wrapper);

            int action = task.opoint.action;
            if (action == 0 && !task.preserveActionZero && !owner.FrameCache.HasFrame(0))
                action = 999;

            owner.Frame.PN = 0;
            owner.Frame.Prev = 0;
            owner.Frame.Prev2 = action;
            owner.Frame.D = owner.FrameCache.GetFrameDataById(action);
            owner.Frame.Prev2D = owner.Frame.D;
            frameModule.SetFrameDirect(action, 0);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                owner.PS.vx = task.directVx;
                owner.PS.vy = task.directVy;
                owner.PS.vz = task.directVz;
                return;
            }

            owner.PS.vx = owner.Dirh() * task.opoint.dvx;
            owner.PS.vy = task.opoint.dvy;
            owner.PS.vz = task.IsLateOpointSpawn ? 0f : task.dvz;
        }

        private void InitializeHealth()
        {
            LF2CharacterData charData =
                owner.ResolveRuntimeCharacterData(owner.ObjectId);
            int hp = charData?.weapon_hp > 0 ? charData.weapon_hp : NTSDGlobal.Default.Health.HpFull;
            owner.Health.HP = hp;
            owner.Health.HPBound = hp;
            owner.Health.HP3 = hp;
            owner.Health.MP = NTSDGlobal.Default.Health.MpFull;
            owner.Health.PP = NTSDGlobal.Default.Health.MpFull;
            owner.Health.MaxPP = NTSDGlobal.Default.Health.MpFull;
            owner.Health.PPBound = NTSDGlobal.Default.Health.MpFull;
            owner.Health.MaxMP = NTSDGlobal.Default.Health.MpFull;
        }

    }
}
