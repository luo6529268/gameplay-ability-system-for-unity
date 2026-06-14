using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
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

            if (!(taskBase is OPointCreateTask task))
            {
                Log.Error($"[{GetType().Name}] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();
            InitializeRuntimeIntPosition(task);

            Renderer = renderer;
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
            _lastState = -1;
            _holdObj = null;
            ShotCount = 0;
            PickerStableId = -1;
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
            Runtime.WeaponState = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            RunDiePhase();
        }

        protected void InitializeParent(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            bool inheritParentRelation = task.inheritParentRelation;
            if (task.useExplicitRelationIdentity)
            {
                Team = task.team;
                RelationTeam = task.relationTeam;
                HolderCopySlot = task.holderCopySlot;
            }
            else if (task.parent != null && inheritParentRelation)
            {
                Team = task.parent.Team;
                RelationTeam = task.parent.RelationTeam;
                HolderCopySlot = task.parent.HolderCopySlot;
            }
            else
            {
                Team = task.team;
                RelationTeam = task.relationTeam;
                if (RelationTeam == 0)
                    RelationTeam = task.team;
                HolderCopySlot = task.holderCopySlot;
            }
            Runtime.OwnerStableId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
        }

        protected void InitializePosition(OPointCreateTask task)
        {
            PS.x = task.pos.x;
            PS.y = task.pos.y;
            PS.z = task.z;
        }

        protected void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        protected void InitializeFrame(OPointCreateTask task)
        {
            int action = task.opoint.action;
            var wrapper = CharacterAnimtorManager.Instance.GetCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);
            Frame.D = FrameCache.GetFrameDataById(action);
            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = action;
            Frame.Prev2D = Frame.D;
            SetFrameDirect(action, 0);
        }

        protected void InitializeVelocity(OPointCreateTask task)
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

        protected void InitializeHealth()
        {
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            if (charData != null && charData.weapon_hp > 0)
            {
                Health.HP = charData.weapon_hp;
                WeaponDropHurt = charData.weapon_drop_hurt > 0 ? charData.weapon_drop_hurt : WeaponDropHurt;
            }
            else
            {
                Health.HP = 100;
            }

            OnHealthInitialized(charData);
        }

        protected void InitializeRuntimeIntPosition(OPointCreateTask task)
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
