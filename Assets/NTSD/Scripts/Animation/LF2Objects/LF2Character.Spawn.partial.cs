using NTSD.Animation.LF2Tasks;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        private void InitializeFromOpoint(OPointCreateTask task)
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

            // 初始化阶段先清空，正式的 release opoint 归属字段
            // 由工厂 PostInitLiving 按 C++ release 回写 owner slot。
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
            KillCount = -1;
            HitStun = 0;

            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(string.IsNullOrEmpty(dir) ? "right" : dir);

            int action = task.opoint.action;
            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = action;
            Frame.N = action;
            Frame.D = FrameCache?.GetFrameDataById(action);
            Frame.Prev2D = Frame.D;
            if (Frame.D != null)
            {
                // C++ release spawn_from_opoint/new entity init：frame=action，但 wait_counter=0，
                // 不能把 wait_counter 初始化成当前 frame，否则同 tick frame_tick 语义会提前漂移。
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, 0);
            }

            SetOpointPosition(task);
            SetOpointVelocity(task);
            InitializeRuntimeIntPosition(task);

            FrameDelay = task.frameDelay;
            AttackExempt = task.attackExempt;

            AiControlled = false;
            Controller = NullLF2Controller.Instance;
            _initializedFromOpoint = true;
        }

        private void SetOpointPosition(OPointCreateTask task)
        {
            if (PS == null)
                return;

            PS.x = task.pos.x;
            PS.y = task.pos.y;
            PS.z = task.z;
        }

        private void SetOpointVelocity(OPointCreateTask task)
        {
            if (PS == null)
                return;

            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;
            PS.vz = 0f;
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
