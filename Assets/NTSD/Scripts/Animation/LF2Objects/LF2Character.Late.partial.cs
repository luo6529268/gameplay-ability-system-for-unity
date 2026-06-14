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
            RunLateState9996Special();
            RunLateN30InputTrigger();
        }

        private void RunLateState9996Special()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null || frame.state != 9996)
                return;

            // 中文注释：
            // C++ 当前 battle_logic.cpp 的生效实现里，
            // state==9996 的 late 特殊分支只在 facing==1 时触发。
            if (PS?.dir != "left")
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null || PS == null)
                return;

            int baseX = Runtime?.XInt ?? Mathf.RoundToInt(PS.x);
            int baseY = Runtime?.YInt ?? Mathf.RoundToInt(PS.y);
            int baseZ = Runtime?.ZInt ?? Mathf.RoundToInt(PS.z);

            for (int i = 0; i < 5; i++)
            {
                int spawnX = baseX + RandInt(0, 7) - 3;
                int spawnY = baseY + RandInt(0, 7) - 9;
                int spawnZ = baseZ + 1;
                int spawnOid = i == 4 ? 218 : 217;
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint { oid = spawnOid, kind = 0, action = RandInt(0, 4), facing = RandInt(0, 2) };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(spawnX, spawnY, spawnZ);
                task.z = spawnZ;
                task.dir = task.opoint.facing == 0 ? "right" : "left";
                task.useDirectVelocity = true;

                // 中文注释：
                // C++ release state=9996 late special:
                // vy = -(rand%15)/2 - 5
                // vx / vz 再按 v415 分支覆盖。
                task.directVx = 0f;
                task.directVy = -(RandInt(0, 15) / 2f) - 5f;
                task.directVz = 0f;
                task.spawnerEntityIndex = Runtime?.SlotIndex ?? -1;
                task.attackExempt = 6;
                task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;

                // 中文注释：
                // C++ release 的 state=9996 late special 是直接在 late loop 里创建对象，
                // 这里只保持“晚于本拍 RenderDispatch 出生”的自然结果，
                // 不再额外叠一层 Unity 自定义的 next-tick 展示强制延后。
                task.deferPresentationToNextTick = false;
                task.suppressLateFrameTickThisTick = false;
                task.deferFrameTickToNextTick = false;

                if (i == 1 || i == 3)
                {
                    task.directVz = -3f - RandInt(0, 2);
                }
                else if (i == 4)
                {
                    task.directVz = 1f;
                }
                else
                {
                    task.directVz = RandInt(0, 2) + 3f;
                }

                if (i >= 4)
                {
                    task.directVx = RandInt(0, 7) - 3f;
                }
                else if (i >= 2)
                {
                    task.directVx = RandInt(0, 3) + 10f;
                }
                else
                {
                    task.directVx = -10f - RandInt(0, 3);
                }

                factory.CreateObjectImmediate(task);
            }
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
                Runtime?.XInt ?? Mathf.RoundToInt(PS.x),
                0f,
                Runtime?.ZInt ?? Mathf.RoundToInt(PS.z));
            task.z = Runtime?.ZInt ?? Mathf.RoundToInt(PS.z);
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.spawnerEntityIndex = slotIndex;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = Runtime?.XInt ?? Mathf.RoundToInt(PS.x);
            task.initialRuntimeY = 0;
            task.initialRuntimeZ = Runtime?.ZInt ?? Mathf.RoundToInt(PS.z);
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;

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

        /// <summary>
        /// C++ release run_late_entity_update：角色死亡清理位于 opoint 生成之前。
        /// </summary>
        internal override void RunLateDeathOpointPreCleanupPhase()
        {
            if (Frame?.D?.state != LF2States.Lying)
                return;
            if (Health == null || Health.HP > 0)
                return;

            ForceDropHeldWeaponForLateDeath();

            int frameId = Frame.N;
            if (frameId < 12 || frameId == 110 || frameId == 111)
                EnterLateDeathLaunchFrame();

            if (PS != null &&
                Mathf.RoundToInt(PS.y) == 0 &&
                PS.y == 0f &&
                PS.vy == 0f &&
                KnockbackVy == 0f)
            {
                int currentFrame = Frame.N;
                bool groundDeathFrame = (currentFrame >= 180 && currentFrame <= 189 && currentFrame != 184) ||
                                        (currentFrame >= 212 && currentFrame <= 214);
                if (groundDeathFrame)
                    EnterLateDeathLaunchFrame();
            }
        }

        private void EnterLateDeathLaunchFrame()
        {
            ImmediateFrame(186);
            if (PS == null)
                return;

            PS.vy = -3f;
            KnockbackVy = -3f;
            PS.y = -1f;
        }

        private void ForceDropHeldWeaponForLateDeath()
        {
            LF2WeaponBase weapon = GetHeldWeaponBase();
            if (weapon == null)
                return;

            weapon.ForceClearHolder();
            if (weapon.PS != null)
                weapon.PS.vx *= 0.5f;

            _heldWeapon = null;
            GrabbedBy = 0;
            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
        }
    }
}
