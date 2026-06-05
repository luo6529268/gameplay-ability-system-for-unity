using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// type=5 其他对象。当前正式战斗逻辑主要覆盖 broken_weapon(oid=999) 碎片。
    /// </summary>
    public class LF2OtherObject : LF2Entity
    {
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

        public override LF2ItrRestTracker ItrRest { get; protected set; }

        public override LF2Health Health { get; protected set; } = new LF2Health();

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
            GrabbedBy = -1;

            if (taskBase is not OPointCreateTask task)
            {
                Log.Error("[LF2OtherObject] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializePosition(task);
            InitializeVelocity(task);
            InitializeHealth();

            SimulationTickDriver.Instance?.World?.Register(this);
        }

        public override void Reset()
        {
            FrameCache.Clear();
            Runtime.Reset();
            ObjectId = 0;
            Team = 0;
            Health.HP = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            Frame.D = targetFrame;
            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);

            if (switchDirAfterTrans)
                SwitchDir(PS.dir == "right" ? "left" : "right");
        }

        public override void SimTransit(int tickIndex)
        {
            if (FrameDelay < 0)
            {
                FrameDelay++;
                return;
            }

            if (FrameDelay > 0)
            {
                FrameDelay--;
                return;
            }

            Trans?.Trans();
        }

        public override void SimTU(int tickIndex)
        {
            ApplyFrameVelocity();
            RunPhysics();
        }

        public override void SimLateTick(int tickIndex)
        {
            if (Frame?.D != null && Sprite != null)
                Sprite.ShowPic(Frame.D.pic);

            base.SimLateTick(tickIndex);
        }

        protected override bool FrameEvent()
        {
            var frame = Frame?.D;
            if (frame == null) return false;

            if (!string.IsNullOrEmpty(frame.sound))
                PlaySound(frame.sound);

            return true;
        }

        private void InitializeParent(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            Team = task.team;
            Runtime.OwnerStableId = task.parent?.StableId ?? -1;
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
            if (action == 0 && !task.preserveActionZero && FrameCache.GetFrameDataById(0) == null)
                action = 999;

            Frame.D = FrameCache.GetFrameDataById(action);
            SetFrameDirect(action);
        }

        private void InitializePosition(OPointCreateTask task)
        {
            float x = task.pos.x;
            float y = task.parent != null ? task.pos.y - task.z : task.pos.y;
            float z = task.z;

            PS.x = x;
            PS.y = y;
            PS.z = z;
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
            PS.vz = task.opoint.dvz != 0 ? task.opoint.dvz : task.dvz;
        }

        private void InitializeHealth()
        {
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int hp = charData?.weapon_hp > 0 ? charData.weapon_hp : NTSDGlobal.Default.Health.HpFull;
            Health.HP = hp;
            Health.HPBound = hp;
            Health.MP = NTSDGlobal.Default.Health.MpFull;
            Health.PP = NTSDGlobal.Default.Health.MpFull;
            Health.MaxPP = NTSDGlobal.Default.Health.MpFull;
            Health.PPBound = NTSDGlobal.Default.Health.MpFull;
        }

        private void ApplyFrameVelocity()
        {
            var frame = Frame?.D;
            if (frame == null || PS == null) return;

            float vx = PS.vx;
            ApplyFrameAxisVelocity(frame.dvx, ref vx, Dirh());
            PS.vx = vx;

            if (frame.dvy > 500)
                PS.vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                PS.vy += frame.dvy;

            if (frame.dvz > 500)
                PS.vz = frame.dvz - 550;
            else if (frame.dvz != 0)
                PS.vz += frame.dvz;
        }

        private static void ApplyFrameAxisVelocity(int value, ref float velocity, int direction)
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 550)
            {
                velocity = 0f;
                return;
            }

            if (value > 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity < target) velocity = target;
                }
                else
                {
                    if (velocity > target) velocity = target;
                }
                return;
            }

            if (value < 0)
            {
                float target = value * direction;
                if (direction >= 0)
                {
                    if (velocity > target) velocity = target;
                }
                else
                {
                    if (velocity < target) velocity = target;
                }
            }
        }

        private void RunPhysics()
        {
            if (PS == null) return;

            float predictedY = PS.y + PS.vy;
            CharacterMechanics.WeaponDynamics(PS, NTSDGlobal.Gameplay.WeaponGravityDefault);

            if (ObjectId == 999 && predictedY >= -0.0001f)
            {
                PS.y = 0f;
                PS.vx = 0f;
                PS.vy = 0f;
                PS.vz = 0f;
                SetFrameDirect(101);
                AttackingCounter = 0;
            }

            if (Frame?.D != null)
                PS.UpdateSpriteOrigin(Frame.D.centerx, Frame.D.centery, Sprite?.GetWidthPx() ?? 0f);
        }

        private void SetFrameDirect(int frameId)
        {
            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;

            if (Frame.D != null)
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        private void CreateObject(ObjectPoint opoint)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null || PS == null) return;

            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = opoint;
            task.parent = this;
            task.team = Team;
            task.pos = MakeObjectPoint(opoint);
            task.z = PS.z;
            task.dir = PS.dir;
            task.dvz = 0f;
            factory.EnqueueCreateObject(task);
        }

        private Vector3 MakeObjectPoint(ObjectPoint opoint)
        {
            var frame = Frame?.D;
            if (frame == null)
                return new Vector3(PS.x, PS.y, PS.z);

            float x = PS.dir == "right"
                ? PS.x - frame.centerx + opoint.x
                : PS.x + frame.centerx - opoint.x;

            float y = PS.y + PS.z - frame.centery + opoint.y;
            float z = PS.z + opoint.y;
            return new Vector3(x, y, z);
        }

        private static void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }
    }
}
