using System;
using UnityEngine;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        /// <summary>
        /// C++ release run_late_entity_update 尾段的角色清理和状态转场副作用。
        /// </summary>
        internal void RunLateCharacterCleanup()
        {
            RunLateN30InputTrigger();
        }

        private void RunLateN30InputTrigger()
        {
            int slotIndex = Runtime?.SlotIndex ?? -1;
            if (slotIndex < 0 || slotIndex >= 10)
                return;
            if (Health == null || Health.HP <= 0)
                return;

            int[] history = Runtime?.InputHistory;
            if (history == null || history.Length < 4)
                return;

            int frameVal = 0;
            int a = history[0];
            int b = history[1];
            int c = history[2];
            int d = history[3];
            if (a == 9 && b == 0 && c == 9 && d == 0) frameVal = 100;
            else if (a == 9 && b == 9 && c == 9 && d == 9) frameVal = 102;
            else if (a == 9 && b == 5 && c == 9 && d == 5) frameVal = 104;
            if (frameVal == 0)
                return;

            Array.Clear(history, 0, history.Length);

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null || PS == null)
                return;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = frameVal, facing = 0 };
            task.parent = null;

            // 中文注释：
            // C++ release 的 N-30 特殊 998 创建链只显式写：
            // 1. spawner_slot = e.slot
            // 2. unk_364 = e.unk_364
            // 并不会把 team 当成正式战斗身份继承给 998。
            //
            // 因此这里不能继续沿用来源角色的 Team。
            // Unity 侧只显式带 RelationTeam，对应 C++ 的 unk_364。
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = RelationTeam;
            task.holderCopySlot = -1;
            task.pos = new Vector3(
                Runtime?.XInt ?? Mathf.RoundToInt((float)PS.x),
                0f,
                Runtime?.ZInt ?? Mathf.RoundToInt((float)PS.z));
            task.z = Runtime?.ZInt ?? Mathf.RoundToInt((float)PS.z);
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.spawnerEntityIndex = slotIndex;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime?.XInt ?? Mathf.RoundToInt((float)PS.x);
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = Runtime?.ZInt ?? Mathf.RoundToInt((float)PS.z);
            // 中文注释：
            // C++ release 的 N-30 触发 998 也是 late special 里的直接创建链。
            // 这里同样不再额外强制 next-tick 展示，避免首帧表现再被人为推迟。
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;
            factory.CreateObjectImmediate(task);
        }

        internal override void RunLateTailBeforePrevFrame()
        {
            RunLateCharacterCleanup();
            base.RunLateTailBeforePrevFrame();
        }
    }
}
