using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2OtherObject
    {
        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            AllocateStableId();

            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();
            Renderer = renderer;
            GrabbedBy = 0;

            if (taskBase is not OPointCreateTask task)
            {
                Log.Error("[LF2OtherObject] Invalid task type");
                return;
            }

            Runtime.SpawnSemantic = (int)task.releaseSpawnSemantic;

            InitializeParent(task);
            InitializePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();
            InitializeRuntimeIntPosition(task);

            SimulationTickDriver.Instance?.World?.Register(this);
        }

        public override void Reset()
        {
            FrameCache.Clear();
            ResetPooledEntityState();
            Runtime.Reset();
            ObjectId = 0;
            Team = 0;
            RelationTeam = 0;
            Health.HP = 0;
            Health.HPBound = 0;
            Health.HP3 = 0;
            Health.MP = 0;
            Health.PP = 0;
            Health.MaxPP = 0;
            Health.PPBound = 0;
            Health.MaxMP = 0;
            GrabbedBy = 0;
            HolderCopySlot = -1;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
            TrackerFlag = 0;
            TrackerParent = null;
            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
            Runtime.HolderStableId = -1;
            Runtime.PickerStableId = -1;
            ResetSpark();
            ResetStableId();
        }

        private void InitializeParent(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            Team = 0;
            RelationTeam = 0;
            HolderCopySlot = -1;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
        }

        private void InitializePosition(OPointCreateTask task)
        {
            if (task.useDirectRuntimePosition)
            {
                PS.x = task.directX;
                PS.y = task.directY;
                PS.z = task.directZ;
                return;
            }

            PS.x = task.pos.x;
            PS.y = task.pos.y;
            PS.z = task.z;
        }

        private void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(string.IsNullOrEmpty(dir) ? "right" : dir);
        }

        private void InitializeFrame(OPointCreateTask task)
        {
            var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);

            int action = task.opoint.action;
            if (action == 0 && !task.preserveActionZero && !FrameCache.HasFrame(0))
                action = 999;

            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = action;
            Frame.D = FrameCache.GetFrameDataById(action);
            Frame.Prev2D = Frame.D;
            SetFrameDirect(action, 0);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;
            PS.vz = task.IsLateOpointSpawn ? 0f : task.dvz;
        }

        private void InitializeHealth()
        {
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int hp = charData?.weapon_hp > 0 ? charData.weapon_hp : NTSDGlobal.Default.Health.HpFull;
            Health.HP = hp;
            Health.HPBound = hp;
            Health.HP3 = hp;
            Health.MP = NTSDGlobal.Default.Health.MpFull;
            Health.PP = NTSDGlobal.Default.Health.MpFull;
            Health.MaxPP = NTSDGlobal.Default.Health.MpFull;
            Health.PPBound = NTSDGlobal.Default.Health.MpFull;
            Health.MaxMP = NTSDGlobal.Default.Health.MpFull;
        }

        private void InitializeRuntimeIntPosition(OPointCreateTask task)
        {
            if (task == null)
                return;

            if (task.useInitialRuntimeIntPosition)
            {
                ApplyForcedRuntimeIntPosition(task.initialRuntimeX, task.initialRuntimeY, task.initialRuntimeZ);
                return;
            }

            ClearForcedRuntimeIntPosition();
        }
    }
}
