using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test
{
    /// <summary>
    /// 战斗运行时自检工具。
    /// 只在测试场景或编辑器菜单中手动启用，用最小帧数据验证 C++ release 对齐过的关键战斗分支。
    /// </summary>
    public sealed class BattleRuntimeSelfCheck : MonoBehaviour
    {
        [Header("启动设置")]
        [Tooltip("进入 Play 后自动执行自检")]
        [SerializeField] private bool runOnStart = false;

        [Tooltip("全部通过后销毁该 GameObject")]
        [SerializeField] private bool destroyWhenPassed = false;

        private void Start()
        {
            if (runOnStart)
                RunAllChecks();
        }

        [ContextMenu("运行战斗运行时自检")]
        public void RunAllChecks()
        {
            RunAllChecksStatic();

            if (destroyWhenPassed)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        public static void RunAllChecksStatic()
        {
            try
            {
                BattleRuntimeSelfCheckCore.RunAllChecks();
                CheckCatchingAttackAction();
                CheckCatchingThrow();
                CheckBeingCaughtPositionSync();
                CheckCpointDecreaseEscape();
                CheckSimulationWorldLateMutation();
                CheckState0BelowGroundFrame212PreservesAttackingCounter();
                Debug.Log("[BattleRuntimeSelfCheck] 战斗运行时自检通过。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BattleRuntimeSelfCheck] 自检失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static void CheckCatchingAttackAction()
        {
            var attacker = CreateCharacter("SelfCheck_Attacker", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_Victim", 2, BuildVictimFrames());
            var controller = new SelfCheckController();
            attacker.Controller = controller;

            attacker.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 300;

            controller.Attack = true;
            controller.InputBuffer.EnqueueForTick(1, FuncKeyMask.att, down: true);
            attacker.InputState.UpdateFromBuffer(controller.InputBuffer, 1, attacker);

            attacker.RunTuCoreForSelfCheck();

            Expect(attacker.CurrentFrameId == 120, "aaction 应直接写入抓取者帧 120");
            Expect(victim.CurrentFrameId == 131, "aaction 目标帧 cpoint.vaction 应直接写入被抓者帧 131");
            Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 0, "aaction 后双方 attacking 应清零");
        }

        private static void CheckCatchingThrow()
        {
            var attacker = CreateCharacter("SelfCheck_Thrower", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_ThrowVictim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Down = true };
            attacker.Controller = controller;

            attacker.ImmediateFrame(110);
            victim.ImmediateFrame(130);
            attacker.SwitchDir("left");
            attacker.PS.x = 100f;
            attacker.PS.y = 20f;
            attacker.PS.z = 7f;
            victim.PS.x = 0f;
            victim.PS.y = 0f;
            victim.PS.z = 1f;
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.FrameDelay = 0;

            attacker.RunTuCoreForSelfCheck();

            Expect(attacker.CurrentFrameId == 112, "throwvx 分支应让抓取者进入当前帧 next=112");
            Expect(victim.CurrentFrameId == 132, "throwvx 分支应无条件写入 victim vaction=132");
            Expect(Nearly(victim.PS.vx, -8f), "左向投掷应反转 victim.vx");
            Expect(Nearly(victim.PS.vy, -4f), "投掷应写入 victim.vy");
            Expect(Nearly(victim.PS.vz, 3f), "按下方向投掷应写入正 throwvz");
            Expect(victim.WeaponCount == 25, "throwinjury>0 应写入 victim.WeaponCount");
            Expect(attacker.Catching == null && victim.Catching == null, "投掷后双方抓取关系应清空");
        }

        private static void CheckBeingCaughtPositionSync()
        {
            var catcher = CreateCharacter("SelfCheck_Catcher", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_BeingCaught", 2, BuildVictimFrames());

            catcher.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            catcher.SwitchDir("right");
            victim.SwitchDir("right");
            catcher.PS.x = 50f;
            catcher.PS.y = 12f;
            catcher.PS.z = 4f;
            catcher.Catching = victim;
            victim.Catching = catcher;
            victim.FrameDelay = 0;

            victim.RunTuCoreForSelfCheck();

            Expect(victim.CurrentFrameId == 131, "被抓位置同步应按 catcher cpoint.vaction 写入被抓者帧");
            Expect(Nearly(victim.PS.x, 56f), "被抓者 x 应按 catcher/vaction cpoint 组合计算");
            Expect(Nearly(victim.PS.y, 20f), "被抓者 y 应按垂直坐标计算并应用 cover 修正");
            Expect(Nearly(victim.PS.z, 3f), "被抓者 z 应复制 catcher 深度并应用 cover 修正");
            Expect(victim.PS.dir == "right", "cover=10 应复制抓取者方向");
        }

        private static void CheckCpointDecreaseEscape()
        {
            var attacker = CreateCharacter("SelfCheck_Decrease", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_EscapeVictim", 2, BuildVictimFrames());

            attacker.ImmediateFrame(140);
            victim.ImmediateFrame(130);
            attacker.PS.x = 30f;
            victim.PS.x = 10f;
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 3;

            attacker.RunTuCoreForSelfCheck();

            Expect(attacker.CurrentFrameId == 0, "decrease<0 逃脱后抓取者应回 frame 0");
            Expect(victim.CurrentFrameId == 181, "decrease<0 逃脱后被抓者应进入 frame 181");
            Expect(attacker.HitCount == 1 && victim.HitCount == 1, "decrease<0 逃脱后双方 HitCount 应为 1");
            Expect(Nearly(victim.KnockbackVx, -4f), "抓取者在右侧时被抓者 knockback_vx 应为 -4");
            Expect(Nearly(victim.KnockbackVy, -3f), "逃脱后被抓者 knockback_vy 应为 -3");
            Expect(attacker.Catching == null && victim.Catching == null, "逃脱后双方抓取关系应清空");
        }

        private static void CheckState0BelowGroundFrame212PreservesAttackingCounter()
        {
            // BMD-023-extended: standing-state-but-below-ground branch
            // (LF2Character.ApplyObjectSpecificFrameTickBeforeWaitAdvance, frame state 0 + Y < 0)
            // must mirror baseline FrameTick.cs:67-76 (SetFrameImmediate(entity, 212)).
            // Baseline's SetFrameImmediate writes Frame + FrameWaitCounter only, never Attacking.
            // Unity's old ImmediateFrame path zeroed AttackingCounter as a side effect
            // (LF2Entity.cs:824). Verify the replacement path preserves AttackingCounter.
            var character = CreateCharacter("SelfCheck_State0BelowGround", 1, BuildCatchingFrames());

            // BuildCatchingFrames frame 0 is state=0, wait=0, next=0 — standing on ground.
            // Drive it to frame 0 explicitly so Frame.D.state resolves correctly.
            character.ImmediateFrame(0);

            // Below-ground runtime state: standing frame, but Y < 0 (drops through ground).
            character.Runtime.YInt = -10;

            // Stash an arbitrary AttackingCounter; the fix must preserve this through the tick.
            const int attackingBefore = 7;
            character.AttackingCounter = attackingBefore;

            // OnFrameTickBeforeWaitAdvance is the public entry that routes through
            // ApplyObjectSpecificFrameTickBeforeWaitAdvance on LF2Character.
            character.OnFrameTickBeforeWaitAdvance(0);

            Expect(character.Frame != null && character.Frame.N == 212,
                "state=0 + Y<0 分支应强制切到 212 空中跳跃帧");
            Expect(character.AttackingCounter == attackingBefore,
                "BMD-023-extended: 切帧必须保留 AttackingCounter，" +
                "ImmediateFrame 路径会在 LF2Entity.cs:824 将其清零，违反 baseline parity");
        }

        private static void CheckSimulationWorldLateMutation()
        {
            var world = new SimulationWorld();
            var spawner = new MutationSelfCheckObject(1, 0, registerDuringLate: true);
            var remover = new MutationSelfCheckObject(2, 1, unregisterDuringLate: true);

            world.Register(spawner);
            world.Register(remover);

            world.LateEntityUpdateAll(1);

            Expect(spawner.LateTickCount == 1, "LateEntityUpdateAll 应执行原始对象后期更新");
            Expect(remover.LateTickCount == 1, "LateEntityUpdateAll 应允许对象在后期更新中请求注销");
            Expect(world.ObjectCount == 2, "LateEntityUpdateAll 应延迟注销并允许新对象注册，不能破坏桶遍历");

            world.LateEntityUpdateAll(2);

            Expect(spawner.Spawned != null && spawner.Spawned.LateTickCount == 1,
                "LateEntityUpdateAll 新注册对象应从下一次后期 pass 开始参与遍历");
        }

        private static LF2Character CreateCharacter(string name, int objectId, LF2CharacterData data)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new SelfCheckController();
            // 自检只验证纯战斗逻辑，不注册到 SimulationWorld，避免批处理验证污染场景运行时。
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRuntimeSlotIndex(character.StableId);
            return character;
        }

        private static LF2CharacterData BuildCatchingFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheckCatcher",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(100, 9, 1, 100, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, aaction = 120, cover = 10, hurtable = 1
                    }),
                    Frame(110, 9, 1, 112, 40, 80, new CatchPoint
                    {
                        kind = 1, x = 16, y = 24, vaction = 132, throwvx = 8, throwvy = -4, throwvz = 3,
                        throwinjury = 25, cover = 10, hurtable = 1
                    }),
                    Frame(112, 0, 0, 0, 39, 79),
                    Frame(120, 9, 1, 120, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, cover = 10, hurtable = 1
                    }),
                    Frame(140, 9, 1, 140, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, decrease = -5, cover = 10, hurtable = 1
                    }),
                }
            };
        }

        private static LF2CharacterData BuildVictimFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheckVictim",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(130, 10, 99, 130, 35, 70, new CatchPoint
                    {
                        kind = 2, x = 8, y = 12, hurtable = 1
                    }),
                    Frame(131, 10, 99, 131, 34, 69, new CatchPoint
                    {
                        kind = 2, x = 9, y = 13, hurtable = 1
                    }),
                    Frame(132, 10, 99, 132, 33, 68, new CatchPoint
                    {
                        kind = 2, x = 6, y = 10, hurtable = 1
                    }),
                    Frame(181, 11, 1, 181, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                }
            };
        }

        private static LF2FrameData Frame(
            int id,
            int state,
            int wait,
            int next,
            int centerx,
            int centery,
            CatchPoint cpoint = null)
        {
            return new LF2FrameData
            {
                frameId = id,
                frameName = $"self_check_{id}",
                state = state,
                wait = wait,
                next = next,
                centerx = centerx,
                centery = centery,
                cpoint = cpoint
            };
        }

        private static void Expect(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static bool Nearly(float actual, float expected)
        {
            return Mathf.Abs(actual - expected) <= 0.001f;
        }

        private sealed class SelfCheckController : ILF2Controller
        {
            public bool Up { get; set; }
            public bool Down { get; set; }
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Attack { get; set; }
            public bool Jump { get; set; }
            public bool Defend { get; set; }

            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();

            bool ILF2Controller.IsUp => Up;
            bool ILF2Controller.IsDown => Down;
            bool ILF2Controller.IsLeft => Left;
            bool ILF2Controller.IsRight => Right;
            bool ILF2Controller.IsAttack => Attack;
            bool ILF2Controller.IsJump => Jump;
            bool ILF2Controller.IsDefend => Defend;

            public int Dirv()
            {
                if (Up && !Down) return -1;
                if (Down && !Up) return 1;
                return 0;
            }

            public (int dx, int dz) GetMoveInput()
            {
                int dx = Right == Left ? 0 : Right ? 1 : -1;
                int dz = Down == Up ? 0 : Down ? 1 : -1;
                return (dx, dz);
            }

            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class MutationSelfCheckObject : ISimObject
        {
            private readonly bool _registerDuringLate;
            private readonly bool _unregisterDuringLate;
            private SimContext _ctx;

            public MutationSelfCheckObject Spawned { get; private set; }
            public int LateTickCount { get; private set; }
            public int SimOrder { get; }
            public int StableId { get; }

            public MutationSelfCheckObject(int stableId, int simOrder, bool registerDuringLate = false, bool unregisterDuringLate = false)
            {
                StableId = stableId;
                SimOrder = simOrder;
                _registerDuringLate = registerDuringLate;
                _unregisterDuringLate = unregisterDuringLate;
            }

            public void OnAdded(SimContext ctx)
            {
                _ctx = ctx;
            }

            public void SimLateTick(int tickIndex)
            {
                LateTickCount++;

                if (_registerDuringLate && Spawned == null)
                {
                    Spawned = new MutationSelfCheckObject(1000 + StableId, SimOrder + 10);
                    _ctx.World.Register(Spawned);
                }

                if (_unregisterDuringLate && LateTickCount == 1)
                    _ctx.World.Unregister(this);
            }
        }
    }
}
