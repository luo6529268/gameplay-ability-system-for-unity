using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
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
                CheckCatchingJumpAction();
                CheckCatchingThrow();
                CheckCpointDirControlUsesRuntimeInput();
                CheckBeingCaughtPositionSync();
                CheckCpointNegativeActionMatrix();
                CheckCpointHeldSyncVactionMatrix();
                CheckCpointThrowRawAndTransformMatrix();
                CheckCpointDecreaseEscape();
                CheckCpointEscapeAndMismatchStillRunTail();
                CheckSharedDatCpointStep10StatsAndInputOrder();
                CheckBattleFlowToggleAndTeleportMatrix();
                CheckValidatePositiveLinksMatrix();
                CheckPreFrameXBoundsMatrix();
                CheckQueuedObjectPointPassBoundaries();
                CheckSimulationWorldLateMutation();
                CheckCollisionCandidateCapAndNewbornIsolation();
                CheckSpecialAttackStep4AndLateFrameTick();
                CheckFrameTickPpDisplayAndCurrentDatMatrix();
                CheckStateTransformLandingMatrix();
                CheckSerialTickInterleaveAndFrameEdgeMatrix();
                CheckState0BelowGroundFrame212PreservesAttackingCounter();
                CheckSimulationPassesImmediateFrameDoesNotZeroAttacking();
                CheckArestCooldownRule();
                CheckFrameTickDefendLockTail();
                CheckKind0HitRecords();
                CheckAlternateHurtTriggerMatrix();
                CheckAlternateDamageCoreSideEffects();
                CheckAlternateDamageMotionTailMatrix();
                CheckAlternateDamageCharacterEntry();
                CheckAlternateDamageSharedDatEntry();
                CheckAlternateDamageHeavyWeaponEntries();
                CheckAlternateDamageInteractionVrest();
                CheckSpecialAttackDamagePreprocess();
                CheckOid5152MergeSuccessAndDormantIsolation();
                CheckOid5152MergeCooldownOneTriggersSameTick();
                CheckOid5152SplitSuccessAndOddTruncate();
                CheckOid5152SplitFailurePartialRecovery();
                CheckOid5152DjaReleaseTriggersSameTickSplit();
                CheckRespawnPassWithoutStoredCount();
                CheckRespawnPassFreeEntityGate();
                CheckRespawnPassWithStoredCountAndEffectSpawn();
                CheckKind15CharacterWhirlwind();
                CheckKind16CharacterSideEffects();
                CheckLateDeathBounceFrame();
                CheckComboWrappersCharacterFrameJumps();
                CheckOid6DjaGuardComboHold();
                CheckStageWaveBootstrapAndSpawnContract();
                CheckStageWaveImmediateSpawnAndAdvance();
                CheckStageWavePositiveSpawnRefill();
                CheckAiTargetCacheCoordinateAndDeterminism();
                CheckAiHumanInputIsolation();
                CheckAiHeldInactiveSlotContract();
                CheckAiSharedCharacterDatShell();
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
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Attacker", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_Victim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Jump = true, Right = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 300;
            attacker.Runtime.KeyJump = 0;
            attacker.Runtime.CdAttack = 5;
            attacker.Runtime.KeyLeft = 0;
            attacker.Runtime.KeyRight = 0;
            attacker.Runtime.KeyUp = 0;
            attacker.Runtime.KeyDown = 0;
            attacker.Trans.SetWait(attacker.Frame.D.wait, 7);
            victim.Trans.SetWait(victim.Frame.D.wait, 8);
            world.CaptureCollisionFrameSnapshotsAll();

            Expect(attacker.Match == world && victim.Match == world,
                "catch self-check entities must resolve their registered SimulationWorld");
            Expect(attacker.Runtime.SlotIndex >= 0 && victim.Runtime.SlotIndex >= 0 &&
                   attacker.Runtime.SlotIndex != victim.Runtime.SlotIndex,
                "catch self-check entities must receive distinct runtime slots");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "catch self-check must establish both runtime cpoint links");
            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 100 && victim.CurrentFrameId == 130,
                "live Controller jump must not trigger aaction when Runtime.KeyJump is clear");

            controller.Jump = false;
            attacker.Runtime.KeyJump = 1;

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 120,
                "runtime aaction with no runtime direction must ignore conflicting live Controller direction");
            Expect(victim.CurrentFrameId == 131, "aaction 目标帧 cpoint.vaction 应直接写入被抓者帧 131");
            Expect(attacker.Trans.WaitCounter == 7 && victim.Trans.WaitCounter == 8,
                "aaction direct frame writes must preserve both wait counters");
            Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 0, "aaction 后双方 attacking 应清零");

            attacker.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            controller.Right = false;
            attacker.Runtime.KeyRight = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 121,
                "runtime direction must select taction even when the live Controller has no direction");
            Expect(victim.CurrentFrameId == 131,
                "taction must read vaction from the newly selected catcher frame");
        }

        private static void CheckCatchingJumpAction()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_JumpAction", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_JumpVictim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Defend = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(160);
            victim.ImmediateFrame(130);
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.KeyDefend = 0;
            attacker.Runtime.CdJump = 5;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 160 && victim.CurrentFrameId == 130,
                "live Controller defend must not trigger jaction when Runtime.KeyDefend is clear");

            controller.Defend = false;
            attacker.Runtime.KeyDefend = 1;
            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 120 && victim.CurrentFrameId == 131,
                "jaction must use Runtime.KeyDefend + Runtime.CdJump");
        }

        private static void CheckCatchingThrow()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Thrower", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_ThrowVictim", 2, BuildVictimFrames());
            var controller = new SelfCheckController { Up = true };
            attacker.Controller = controller;
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(110);
            victim.ImmediateFrame(130);
            attacker.SwitchDir("left");
            attacker.Runtime.SetPosition(100f, 20f, 7f);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(0f, 0f, 1f);
            victim.Runtime.SyncIntegerPosition();
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.Runtime.KeyUp = 0;
            attacker.Runtime.KeyDown = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 112, "throwvx 分支应让抓取者进入当前帧 next=112");
            Expect(victim.CurrentFrameId == 132, "throwvx 分支应无条件写入 victim vaction=132");
            Expect(Nearly(victim.Runtime.X, 124f) && Nearly(victim.Runtime.Y, -36f),
                "throwvx branch must place the victim from the catcher frame/cpoint geometry");
            Expect(Nearly(victim.Runtime.Vx, -8f), "左向投掷应反转 victim.vx");
            Expect(Nearly(victim.Runtime.Vy, -4f), "投掷应写入 victim.vy");
            Expect(Nearly(victim.Runtime.Vz, 3f), "按下方向投掷应写入正 throwvz");
            Expect(victim.WeaponCount == 25, "throwinjury>0 应写入 victim.WeaponCount");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "throw kind1 sub-pass must not invent runtime link cleanup");
        }

        private static void CheckCpointDirControlUsesRuntimeInput()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_DirControl", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_DirControlVictim", 2, BuildVictimFrames());
            attacker.Controller = new SelfCheckController { Left = true };
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(150);
            victim.ImmediateFrame(130);
            attacker.SwitchDir("left");
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            attacker.AttackingCounter = 2;
            attacker.Runtime.KeyLeft = 0;
            attacker.Runtime.KeyRight = 1;
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.Runtime.Dir == "right",
                "dircontrol must follow Runtime.KeyRight instead of conflicting live Controller left");
        }

        private static void CheckBeingCaughtPositionSync()
        {
            var world = new SimulationWorld();
            var catcher = CreateCharacter("SelfCheck_Catcher", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_BeingCaught", 2, BuildVictimFrames());
            world.Register(catcher);
            world.Register(victim);

            catcher.ImmediateFrame(100);
            victim.ImmediateFrame(130);
            catcher.SwitchDir("left");
            catcher.PS.dir = "right";
            victim.SwitchDir("right");
            catcher.Runtime.SetPosition(50f, 12f, 4f);
            catcher.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(0f, 0f, 0f);
            victim.Runtime.SyncIntegerPosition();
            catcher.Catching = victim;
            victim.Catching = catcher;
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            victim.FrameDelay = 0;
            victim.Trans.SetWait(victim.Frame.D.wait, 9);
            world.CaptureCollisionFrameSnapshotsAll();

            catcher.RunWeaponSyncHeldStep10();

            Expect(victim.CurrentFrameId == 131, "被抓位置同步应按 catcher cpoint.vaction 写入被抓者帧");
            Expect(victim.Trans.WaitCounter == 9,
                "being-caught vaction direct frame write must preserve the victim wait counter");
            Expect(Nearly(victim.Runtime.X, 94f),
                "held sync must use left-facing Runtime.Dir even when PS.dir is stale right");
            Expect(Nearly(victim.Runtime.Y, 20f), "被抓者 y 应按垂直坐标计算并应用 cover 修正");
            Expect(Nearly(victim.Runtime.Z, 3f), "被抓者 z 应复制 catcher 深度并应用 cover 修正");
            Expect(victim.Runtime.Dir == "left", "cover=10 应复制抓取者 Runtime.Dir");
            Expect(catcher.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == catcher.Runtime.SlotIndex,
                "position sync must preserve the established runtime cpoint links");
        }

        private static void CheckCpointNegativeActionMatrix()
        {
            string[] actionKinds = { "aaction", "taction", "jaction" };
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int actionIndex = 0; actionIndex < actionKinds.Length; actionIndex++)
                {
                    string actionKind = actionKinds[actionIndex];
                    CatchPoint sourceCpoint = new CatchPoint
                    {
                        kind = 1,
                        x = 20,
                        y = 30,
                        cover = 0,
                        hurtable = 1,
                    };
                    if (actionKind == "aaction") sourceCpoint.aaction = -120;
                    else if (actionKind == "taction") sourceCpoint.taction = -120;
                    else sourceCpoint.jaction = -120;

                    LF2CharacterData attackerData = new LF2CharacterData
                    {
                        name = $"SelfCheck_NegativeAction_{actionKind}",
                        frames = new List<LF2FrameData>
                        {
                            Frame(0, 0, 0, 0, 39, 79),
                            Frame(100, 9, 1, 100, 39, 79, sourceCpoint),
                            Frame(120, 9, 2, 120, 41, 81, new CatchPoint
                            {
                                kind = 1, vaction = -131, hurtable = 1
                            }),
                        },
                    };
                    LF2CharacterData victimData = BuildCpointMatrixVictimFrames();
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, $"NegativeAction_{actionKind}_Attacker", 1, attackerData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, $"NegativeAction_{actionKind}_Victim", 2, victimData);
                    world.Register(attacker);
                    world.Register(victim);
                    LinkCpointEntities(attacker, victim);
                    attacker.SetCpointRawFramePreserveWait(100);
                    victim.SetCpointRawFramePreserveWait(130);
                    attacker.SwitchDir("right");
                    victim.SwitchDir("right");
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 7);
                    victim.Trans.SetWait(victim.Frame.D.wait, 8);
                    attacker.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    attacker.Runtime.CdAttack = 5;
                    attacker.Runtime.CdJump = 5;
                    if (actionKind == "aaction") attacker.Runtime.KeyJump = 1;
                    else if (actionKind == "taction")
                    {
                        attacker.Runtime.KeyJump = 1;
                        attacker.Runtime.KeyRight = 1;
                    }
                    else attacker.Runtime.KeyDefend = 1;
                    world.CaptureCollisionFrameSnapshotsAll();

                    attacker.RunCpointCheckStep10();

                    string label = $"{(realCharacter ? "character" : "shared-DAT")} {actionKind}";
                    Expect(attacker.Frame.N == 120 && attacker.Runtime.Dir == "left",
                        $"{label}: negative action must flip attacker once and use the absolute frame");
                    Expect(victim.Frame.N == -131 && victim.Frame.D == null && victim.Runtime.Dir == "right",
                        $"{label}: action-produced victim vaction must remain a raw negative frame without flipping");
                    Expect(attacker.Trans.WaitCounter == 7 && victim.Trans.WaitCounter == 8,
                        $"{label}: action frame writes must preserve both wait counters");
                    Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 0,
                        $"{label}: action selection must explicitly clear both attacking counters");
                    Expect(attacker.Frame.Prev2 == 100 && victim.Frame.Prev2 == 130,
                        $"{label}: action selection must not overwrite collision prev_frame2 snapshots");
                }
            }
        }

        private static void CheckCpointHeldSyncVactionMatrix()
        {
            int[] vactions = { -131, 0, 131 };
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int i = 0; i < vactions.Length; i++)
                {
                    int vaction = vactions[i];
                    LF2CharacterData catcherData = new LF2CharacterData
                    {
                        name = $"SelfCheck_Held_{vaction}",
                        frames = new List<LF2FrameData>
                        {
                            Frame(0, 0, 0, 0, 39, 79),
                            Frame(100, 9, 1, 100, 39, 79, new CatchPoint
                            {
                                kind = 1, x = 20, y = 30, vaction = vaction, cover = 0, hurtable = 1
                            }),
                        },
                    };
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity catcher = CreateCpointMatrixEntity(realCharacter, $"Held_{vaction}_Catcher", 1, catcherData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, $"Held_{vaction}_Victim", 2, BuildCpointMatrixVictimFrames());
                    world.Register(catcher);
                    world.Register(victim);
                    LinkCpointEntities(catcher, victim);
                    catcher.SetCpointRawFramePreserveWait(100);
                    victim.SetCpointRawFramePreserveWait(130);
                    catcher.SwitchDir("right");
                    victim.SwitchDir("right");
                    catcher.Runtime.SetPosition(50, 12, 4);
                    catcher.Runtime.SyncIntegerPosition();
                    victim.Trans.SetWait(victim.Frame.D.wait, 9);
                    catcher.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    victim.FrameDelay = 0;

                    catcher.RunWeaponSyncHeldStep10();

                    int expectedFrame = vaction < 0 ? -vaction : vaction;
                    string expectedDirection = vaction < 0 ? "left" : "right";
                    float expectedX = vaction < 0 ? -3f : (vaction == 0 ? 58f : 56f);
                    float expectedY = vaction < 0 ? 33f : 20f;
                    string label = $"{(realCharacter ? "character" : "shared-DAT")} held vaction={vaction}";
                    Expect(victim.Frame.N == expectedFrame && victim.Runtime.Dir == expectedDirection,
                        $"{label}: held sync must raw-write, then flip/abs a negative vaction exactly once");
                    Expect(victim.Trans.WaitCounter == 9,
                        $"{label}: held sync must preserve the victim wait counter");
                    Expect(Nearly(victim.Runtime.X, expectedX) && Nearly(victim.Runtime.Y, expectedY) && Nearly(victim.Runtime.Z, 3f),
                        $"{label}: held position must use raw-vaction cpoint coordinates and resolved-frame centers");
                    Expect(catcher.AttackingCounter == 5 && victim.AttackingCounter == 6,
                        $"{label}: zero-injury held sync must preserve attacking counters");
                }
            }
        }

        private static void CheckCpointThrowRawAndTransformMatrix()
        {
            for (int shellIndex = 0; shellIndex < 2; shellIndex++)
            {
                bool realCharacter = shellIndex == 0;
                for (int directionMode = 0; directionMode < 2; directionMode++)
                {
                    LF2CharacterData attackerData = BuildCpointThrowFrames(-112, -132, 25);
                    SimulationWorld world = new SimulationWorld();
                    LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, "RawThrow_Attacker", 1, attackerData);
                    LF2Entity victim = CreateCpointMatrixEntity(realCharacter, "RawThrow_Victim", 2, BuildCpointMatrixVictimFrames());
                    world.Register(attacker);
                    world.Register(victim);
                    LinkCpointEntities(attacker, victim);
                    attacker.SetCpointRawFramePreserveWait(110);
                    victim.SetCpointRawFramePreserveWait(130);
                    attacker.SwitchDir("left");
                    victim.SwitchDir("right");
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 11);
                    victim.Trans.SetWait(victim.Frame.D.wait, 12);
                    attacker.AttackingCounter = 5;
                    victim.AttackingCounter = 6;
                    victim.Runtime.Vz = 6f;
                    if (directionMode == 1)
                    {
                        attacker.Runtime.KeyUp = 1;
                        attacker.Runtime.KeyDown = 1;
                    }
                    world.CaptureCollisionFrameSnapshotsAll();

                    attacker.RunCpointCheckStep10();

                    string label = $"{(realCharacter ? "character" : "shared-DAT")} raw throw mode={directionMode}";
                    Expect(attacker.Frame.N == -112 && attacker.Frame.D == null && attacker.Frame.Prev2 == -112,
                        $"{label}: attacker next must raw-write frame and prev_frame2");
                    Expect(victim.Frame.N == -132 && victim.Frame.D == null && victim.Frame.Prev2 == -132,
                        $"{label}: victim vaction must raw-write frame and prev_frame2");
                    Expect(attacker.Runtime.Dir == "left" && victim.Runtime.Dir == "right",
                        $"{label}: raw throw writes must not flip either entity");
                    Expect(attacker.Trans.WaitCounter == 11 && victim.Trans.WaitCounter == 12,
                        $"{label}: raw throw writes must preserve wait counters");
                    Expect(attacker.AttackingCounter == 0 && victim.AttackingCounter == 6,
                        $"{label}: throw clears only attacker attacking");
                    Expect(Nearly(victim.Runtime.Vz, 6f),
                        $"{label}: neither/both depth inputs must preserve the previous victim Vz");
                }

                CheckCpointThrowTransformUsesCurrentDat(realCharacter);
            }
        }

        private static void CheckCpointThrowTransformUsesCurrentDat(bool realCharacter)
        {
            LF2CharacterData sourceData = BuildCpointThrowFrames(112, -132, -1);
            sourceData.frames.Add(Frame(130, 10, 1, 130, 35, 70, new CatchPoint { kind = 2 }));
            LF2CharacterData targetData = new LF2CharacterData
            {
                name = "SelfCheck_ThrowTransformTarget",
                weapon_hp = 321,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 3, -7, 100, 200),
                    Frame(130, 10, 4, 130, 777, 778, new CatchPoint { kind = 2 }),
                    Frame(132, 10, 5, 132, 33, 68, new CatchPoint { kind = 2 }),
                },
            };
            LF2CharacterDataWrapper targetWrapper = new LF2CharacterDataWrapper(2, targetData);
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                int resolverCalls = 0;
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                {
                    resolverCalls++;
                    return oid == 2 ? targetWrapper : null;
                };
                SimulationWorld world = new SimulationWorld();
                LF2Entity attacker = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Attacker", 1, sourceData);
                LF2Entity victim = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Victim", 2, targetData);
                LF2Entity ownedChild = CreateCpointMatrixEntity(realCharacter, "TransformThrow_Child", 1, sourceData);
                world.Register(attacker);
                world.Register(victim);
                world.Register(ownedChild);
                LinkCpointEntities(attacker, victim);
                ownedChild.KillCount = attacker.Runtime.SlotIndex;
                attacker.SetCpointRawFramePreserveWait(110);
                victim.SetCpointRawFramePreserveWait(130);
                ownedChild.SetCpointRawFramePreserveWait(130);
                attacker.SwitchDir("right");
                victim.SwitchDir("right");
                attacker.Runtime.SetPosition(100, 20, 7);
                attacker.Runtime.SyncIntegerPosition();
                attacker.Trans.SetWait(attacker.Frame.D.wait, 11);
                victim.Trans.SetWait(victim.Frame.D.wait, 12);
                victim.Runtime.Vz = 6f;
                world.CaptureCollisionFrameSnapshotsAll();

                Expect(victim.ObjectId == 2 && attacker.HasStep10ThrowTransformVictimData(victim),
                    $"transform throw fixture must expose victim DAT; victimOid={victim.ObjectId}");
                Expect(attacker.GetCollisionFrameData()?.cpoint?.throwinjury == -1,
                    $"transform throw fixture must preserve throwinjury=-1; actual={attacker.GetCollisionFrameData()?.cpoint?.throwinjury}");

                attacker.RunCpointCheckStep10();

                string label = realCharacter ? "character transform throw" : "shared-DAT transform throw";
                Expect(attacker.ObjectId == 2,
                    $"{label}: throwinjury=-1 must replace attacker ObjectId; actual={attacker.ObjectId}, frame={attacker.Frame.N}, resolverCalls={resolverCalls}");
                Expect(attacker.FrameCache.Wrapper == targetWrapper,
                    $"{label}: throwinjury=-1 must load target DAT wrapper; actual={attacker.FrameCache.Wrapper?.characterId}");
                Expect(attacker.Frame.N == -7 && attacker.Frame.Prev2 == -7 && attacker.Trans.WaitCounter == 11,
                    $"{label}: throw next must come from transformed DAT frame 0 and raw-write without changing wait");
                Expect(Nearly(victim.Runtime.X, 16f) && Nearly(victim.Runtime.Y, -156f),
                    $"{label}: throw geometry must use transformed DAT frame 0 centers");
                Expect(ownedChild.ObjectId == 2 && ownedChild.FrameCache.Wrapper == targetWrapper &&
                       ownedChild.Frame.D != null && ownedChild.Frame.D.centerx == 777,
                    $"{label}: owned child must reload current Frame.D after DAT propagation");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckCpointDecreaseEscape()
        {
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_Decrease", 1, BuildCatchingFrames());
            var victim = CreateCharacter("SelfCheck_EscapeVictim", 2, BuildVictimFrames());
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(140);
            victim.ImmediateFrame(130);
            attacker.Runtime.SetPosition(30f, 0f, 0f);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(10f, 0f, 0f);
            victim.Runtime.SyncIntegerPosition();
            attacker.Catching = victim;
            victim.Catching = attacker;
            attacker.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = attacker.Runtime.SlotIndex;
            attacker.FrameDelay = 0;
            victim.FrameDelay = 0;
            attacker.Runtime.CaughtDuration = 3;
            attacker.Trans.SetWait(attacker.Frame.D.wait, 10);
            victim.Trans.SetWait(victim.Frame.D.wait, 11);
            world.CaptureCollisionFrameSnapshotsAll();

            attacker.RunCpointCheckStep10();

            Expect(attacker.CurrentFrameId == 0, "decrease<0 逃脱后抓取者应回 frame 0");
            Expect(victim.CurrentFrameId == 181, "decrease<0 逃脱后被抓者应进入 frame 181");
            Expect(attacker.Trans.WaitCounter == 10 && victim.Trans.WaitCounter == 11,
                "decrease escape raw frame writes must preserve both wait counters");
            Expect(attacker.Frame.D != null && attacker.Frame.D.frameId == 0 &&
                   victim.Frame.D != null && victim.Frame.D.frameId == 181,
                "decrease escape raw frame writes must keep Frame.D synchronized");
            Expect(attacker.HitCount == 1 && victim.HitCount == 1, "decrease<0 逃脱后双方 HitCount 应为 1");
            Expect(Nearly(victim.KnockbackVx, -4f), "抓取者在右侧时被抓者 knockback_vx 应为 -4");
            Expect(Nearly(victim.KnockbackVy, -3f), "逃脱后被抓者 knockback_vy 应为 -3");
            Expect(Nearly(victim.Runtime.Vx, 0f) && Nearly(victim.Runtime.Vy, 0f),
                "decrease escape must leave runtime velocity untouched before FramePostProcess");
            Expect(attacker.CaughtSlotIndex == victim.Runtime.SlotIndex &&
                   victim.CatcherSlotIndex == attacker.Runtime.SlotIndex,
                "decrease kind1 sub-pass must not invent runtime link cleanup");

            world.FramePostProcessAll();

            Expect(Nearly(victim.Runtime.Vx, -4f) && Nearly(victim.Runtime.Vy, -3f) && victim.HitCount == 0,
                $"FramePostProcess must consume escape knockback exactly once; vx={victim.Runtime.Vx}, vy={victim.Runtime.Vy}, hitCount={victim.HitCount}, frameDelay={victim.FrameDelay}");
        }

        private static void CheckCpointEscapeAndMismatchStillRunTail()
        {
            LF2CharacterData throwData = BuildCpointTailFrames(
                decrease: -5,
                throwVx: 8,
                dirControl: 0);
            var throwWorld = new SimulationWorld();
            LF2Entity thrower = CreateCpointMatrixEntity(false, "SelfCheck_EscapeThrow_Shared", 1, throwData);
            LF2Entity throwVictim = CreateCpointMatrixEntity(false, "SelfCheck_EscapeThrowVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            throwWorld.Register(thrower);
            throwWorld.Register(throwVictim);
            LinkCpointEntities(thrower, throwVictim);
            thrower.SetCpointRawFramePreserveWait(110);
            throwVictim.SetCpointRawFramePreserveWait(130);
            thrower.Runtime.CaughtDuration = 3;
            throwWorld.CaptureCollisionFrameSnapshotsAll();

            thrower.RunCpointCheckStep10();

            Expect(thrower.Frame.N == 0 && throwVictim.Frame.N == 132,
                "CaughtDuration<0 must skip actions but still execute the throw tail using source cpoint data");
            Expect(Nearly(throwVictim.Runtime.Vx, 8f) && Nearly(throwVictim.Runtime.Vy, -4f),
                "CaughtDuration<0 throw tail must still write victim velocity");

            LF2CharacterData dirData = BuildCpointTailFrames(
                decrease: -5,
                throwVx: 0,
                dirControl: 1);
            var dirWorld = new SimulationWorld();
            LF2Entity dirCatcher = CreateCpointMatrixEntity(false, "SelfCheck_EscapeDir_Shared", 1, dirData);
            LF2Entity dirVictim = CreateCpointMatrixEntity(false, "SelfCheck_EscapeDirVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            dirWorld.Register(dirCatcher);
            dirWorld.Register(dirVictim);
            LinkCpointEntities(dirCatcher, dirVictim);
            dirCatcher.SetCpointRawFramePreserveWait(110);
            dirVictim.SetCpointRawFramePreserveWait(130);
            dirCatcher.Runtime.CaughtDuration = 3;
            dirCatcher.AttackingCounter = 2;
            dirCatcher.SwitchDir("left");
            dirCatcher.Runtime.KeyRight = 1;
            dirWorld.CaptureCollisionFrameSnapshotsAll();

            dirCatcher.RunCpointCheckStep10();

            Expect(dirCatcher.Runtime.Dir == "right",
                "CaughtDuration<0 must still execute dircontrol after skipping action selection");

            var mismatchWorld = new SimulationWorld();
            LF2Entity mismatchCatcher = CreateCpointMatrixEntity(false, "SelfCheck_MismatchThrow_Shared", 1, BuildCpointTailFrames(0, 8, 0));
            LF2Entity mismatchVictim = CreateCpointMatrixEntity(false, "SelfCheck_MismatchThrowVictim_Shared", 2, BuildCpointMatrixVictimFrames());
            mismatchWorld.Register(mismatchCatcher);
            mismatchWorld.Register(mismatchVictim);
            mismatchCatcher.CaughtSlotIndex = mismatchVictim.Runtime.SlotIndex;
            mismatchVictim.CatcherSlotIndex = -1;
            mismatchCatcher.SetCpointRawFramePreserveWait(110);
            mismatchVictim.SetCpointRawFramePreserveWait(130);
            mismatchCatcher.Trans.SetWait(mismatchCatcher.Frame.D.wait, 9);
            mismatchWorld.CaptureCollisionFrameSnapshotsAll();

            mismatchCatcher.RunCpointCheckStep10();

            Expect(mismatchCatcher.Frame.N == 0 && mismatchVictim.Frame.N == 132,
                "cpoint mismatch must suppress actions but still run the source throw tail");
            Expect(mismatchCatcher.Frame.D != null && mismatchCatcher.Frame.D.frameId == 0 &&
                   mismatchCatcher.Trans.WaitCounter == 9,
                "cpoint mismatch frame0 fallback must preserve wait and synchronize Frame.D");
            Expect(Nearly(mismatchVictim.Runtime.Vx, 8f),
                "cpoint mismatch throw tail must still apply horizontal throw velocity");
        }

        private static void CheckSharedDatCpointStep10StatsAndInputOrder()
        {
            LF2CharacterData catcherData = BuildSharedCpointStatsFrames();
            LF2CharacterData victimData = BuildCpointMatrixVictimFrames();
            var world = new SimulationWorld();
            LF2Entity holder = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointHolder", 3, victimData);
            LF2Entity catcher = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointCatcher", 1, catcherData);
            LF2Entity victim = CreateCpointMatrixEntity(false, "SelfCheck_SharedCpointVictim", 2, victimData);
            world.Register(holder);
            world.Register(catcher);
            world.Register(victim);
            LinkCpointEntities(catcher, victim);
            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.Runtime.KeyJump = 1;
            catcher.Runtime.CdAttack = 5;
            catcher.Runtime.KeyDefend = 1;
            catcher.Runtime.CdJump = 5;
            catcher.Runtime.KeyRight = 1;
            catcher.SwitchDir("right");
            world.CaptureCollisionFrameSnapshotsAll();

            catcher.RunCpointCheckStep10();

            Expect(catcher.Frame.N == 122 && victim.Frame.N == 133,
                "simultaneous attack/direction/jump input must resolve taction before the final jaction");
            Expect(catcher.Runtime.Dir == "right",
                "positive simultaneous cpoint actions must preserve shared-DAT shell facing");

            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.AttackingCounter = 2;
            catcher.Runtime.KeyJump = 0;
            catcher.Runtime.KeyDefend = 0;
            catcher.Runtime.KeyRight = 0;
            catcher.Runtime.KeyLeft = 1;
            world.CaptureCollisionFrameSnapshotsAll();
            catcher.RunCpointCheckStep10();
            Expect(catcher.Runtime.Dir == "left",
                "shared-DAT non-character cpoint dircontrol must use runtime input without a CLR character gate");

            catcher.SetCpointRawFramePreserveWait(100);
            victim.SetCpointRawFramePreserveWait(130);
            catcher.AttackingCounter = 0;
            catcher.HolderCopySlot = holder.Runtime.SlotIndex;
            victim.Health.HP = 20;
            victim.Health.HPBound = 20;
            victim.Health.HPLost = 7;
            victim.KillCount = -1;
            victim.Unk344 = 1;
            world.KillStats[1] = 0;
            world.DamageStats[1] = 0;

            catcher.RunWeaponSyncHeldStep10();

            Expect(victim.Health.HP == -10 && victim.Health.HPBound == 10,
                "shared-DAT held cpoint injury must apply HP and HPBound damage");
            Expect(victim.Health.HPLost == 7,
                "held cpoint injury must not write the unrelated HPLost accumulator");
            Expect(holder.KillStat == 1 && world.KillStats[1] == 1,
                "lethal held cpoint injury must credit holder and indexed kill statistics");
            Expect(holder.ComboCountAtk == 30 && victim.ComboCountVic == 30 && world.DamageStats[1] == 30,
                "held cpoint injury must credit combo and indexed damage statistics exactly once");
        }

        private static LF2CharacterData BuildCpointTailFrames(int decrease, int throwVx, int dirControl)
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointTail",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(110, LF2States.Catching, 1, 0, 40, 80, new CatchPoint
                    {
                        kind = 1,
                        x = 16,
                        y = 24,
                        vaction = 132,
                        aaction = 120,
                        decrease = decrease,
                        throwvx = throwVx,
                        throwvy = -4,
                        throwvz = 3,
                        dircontrol = dirControl,
                        hurtable = 1,
                    }),
                    Frame(120, LF2States.Catching, 1, 120, 39, 79, new CatchPoint { kind = 1, vaction = 131, hurtable = 1 }),
                },
            };
        }

        private static LF2CharacterData BuildSharedCpointStatsFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_SharedCpointStats",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(100, LF2States.Catching, 1, 100, 39, 79, new CatchPoint
                    {
                        kind = 1,
                        vaction = 131,
                        aaction = 120,
                        taction = 121,
                        jaction = 122,
                        dircontrol = 1,
                        injury = 30,
                        hurtable = 1,
                    }),
                    Frame(120, LF2States.Catching, 1, 120, 39, 79, new CatchPoint { kind = 1, vaction = 131, hurtable = 1 }),
                    Frame(121, LF2States.Catching, 1, 121, 39, 79, new CatchPoint { kind = 1, vaction = 132, hurtable = 1 }),
                    Frame(122, LF2States.Catching, 1, 122, 39, 79, new CatchPoint { kind = 1, vaction = 133, hurtable = 1 }),
                },
            };
        }

        private static void CheckBattleFlowToggleAndTeleportMatrix()
        {
            var flowWorld = new SimulationWorld();
            for (int tick = 1; tick <= 13; tick++)
            {
                flowWorld.AdvanceBattleFlowTick(tick);
                if (tick <= 4 || tick >= 11)
                {
                    Expect(flowWorld.CurrentTickIndex == tick,
                        $"flow tick {tick}: CurrentTickIndex must advance at tick head");
                    Expect(flowWorld.InputPhase == (tick & 1),
                        $"flow tick {tick}: InputPhase parity mismatch");
                    Expect(flowWorld.FrameMod12 == tick % 12,
                        $"flow tick {tick}: FrameMod12 mismatch");
                    Expect(flowWorld.FrameToggle == (tick & 1),
                        $"flow tick {tick}: FrameToggle parity mismatch");
                }
            }

            flowWorld.ResetRuntimeState();
            Expect(flowWorld.CurrentTickIndex == 0 && flowWorld.InputPhase == 0 &&
                   flowWorld.FrameMod12 == 0 && flowWorld.FrameToggle == 0,
                "battle flow reset must clear tick, input phase, FrameMod12 and FrameToggle");

            var gatedWorld = new SimulationWorld();
            FlowSelfCheckEntity gatedSource = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportGate_Source", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 1, 10, 20, 0);
            FlowSelfCheckEntity gatedTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportGate_Target", LF2ObjectType.Character,
                LF2States.Standing, 2, 300, 40, 1);
            gatedWorld.Register(gatedSource);
            gatedWorld.Register(gatedTarget);

            gatedWorld.AdvanceBattleFlowTick(1);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(1);
            Expect(gatedSource.GetRuntimeXInt() == 10 && gatedSource.GetRenderZInt() == 20,
                "FrameToggle=1 on tick 1 must gate state 400 teleport");

            gatedWorld.AdvanceBattleFlowTick(2);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(gatedSource.GetRuntimeXInt() == 180 && gatedSource.GetRenderZInt() == 41,
                "FrameToggle=0 on tick 2 must run state 400 teleport");

            gatedSource.Runtime.SetPosition(25f, -8f, 30f);
            gatedSource.Runtime.SyncIntegerPosition();
            gatedWorld.AdvanceBattleFlowTick(3);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(3);
            Expect(gatedSource.GetRuntimeXInt() == 25 && gatedSource.GetRenderZInt() == 30,
                "FrameToggle=1 on tick 3 must gate state 400 teleport");

            gatedWorld.AdvanceBattleFlowTick(4);
            gatedWorld.EarlyFrameAdvanceSpecialsAll(4);
            Expect(gatedSource.GetRuntimeXInt() == 180 && gatedSource.GetRenderZInt() == 41,
                "FrameToggle=0 on tick 4 must run state 400 teleport");

            var selfWorld = new SimulationWorld();
            FlowSelfCheckEntity selfSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport401_Self", LF2ObjectType.Character,
                LF2States.TeleportToTeammate, 3, 100, 20, 5);
            selfWorld.Register(selfSource);
            AdvanceFlowToEvenToggle(selfWorld);
            selfWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(selfSource.GetRuntimeXInt() == 40 && selfSource.GetRenderZInt() == 21,
                "state 401 must be allowed to select self when no farther teammate exists");

            var sourceTypeWorld = new SimulationWorld();
            FlowSelfCheckEntity nonCharacterSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NonCharacterSource", LF2ObjectType.Other,
                LF2States.TeleportToEnemy, 1, 0, 0, 10);
            FlowSelfCheckEntity characterTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_CharacterTarget", LF2ObjectType.Character,
                LF2States.Standing, 2, 250, 50, 11);
            sourceTypeWorld.Register(nonCharacterSource);
            sourceTypeWorld.Register(characterTarget);
            AdvanceFlowToEvenToggle(sourceTypeWorld);
            sourceTypeWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(nonCharacterSource.GetRuntimeXInt() == 130 && nonCharacterSource.GetRenderZInt() == 51,
                "state 400 source must not require Character DAT when its target is Character DAT");

            var selectionWorld = new SimulationWorld();
            FlowSelfCheckEntity selectionSource = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Source", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 1, 0, 0, 20);
            FlowSelfCheckEntity farCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Far", LF2ObjectType.Character,
                LF2States.Standing, 2, 400, 0, 21);
            FlowSelfCheckEntity nearCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Near", LF2ObjectType.Character,
                LF2States.Standing, 2, 200, 0, 22);
            FlowSelfCheckEntity ignoredNonCharacter = CreateFlowSelfCheckEntity(
                "SelfCheck_TeleportSelection_Ignored", LF2ObjectType.Other,
                LF2States.Standing, 2, 20, 0, 23);
            selectionWorld.Register(selectionSource);
            selectionWorld.Register(farCharacter);
            selectionWorld.Register(nearCharacter);
            selectionWorld.Register(ignoredNonCharacter);
            AdvanceFlowToEvenToggle(selectionWorld);
            selectionWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(selectionSource.GetRuntimeXInt() == 80 && selectionSource.GetRenderZInt() == 1,
                "state 400 must select the nearest live Character DAT target and ignore non-character targets");

            var noTargetWorld = new SimulationWorld();
            FlowSelfCheckEntity noTargetSource = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NoTarget", LF2ObjectType.Character,
                LF2States.TeleportToEnemy, 4, 70, 30, 30);
            FlowSelfCheckEntity sameTeamOnly = CreateFlowSelfCheckEntity(
                "SelfCheck_Teleport_NoTarget_SameTeam", LF2ObjectType.Character,
                LF2States.Standing, 4, 200, 30, 31);
            noTargetSource.Runtime.Y = -12f;
            noTargetSource.Runtime.YInt = -12;
            noTargetSource.Runtime.Vx = 5f;
            noTargetSource.Runtime.Vy = -6f;
            noTargetSource.Runtime.Vz = 7f;
            noTargetWorld.Register(noTargetSource);
            noTargetWorld.Register(sameTeamOnly);
            AdvanceFlowToEvenToggle(noTargetWorld);
            noTargetWorld.EarlyFrameAdvanceSpecialsAll(2);
            Expect(noTargetSource.GetRuntimeXInt() == 70 && noTargetSource.GetRenderZInt() == 30 &&
                   noTargetSource.GetRuntimeYInt() == 0 && Nearly(noTargetSource.Runtime.Vx, 0f) &&
                   Nearly(noTargetSource.Runtime.Vy, 0f) && Nearly(noTargetSource.Runtime.Vz, 0f),
                "state 400 no-target branch must preserve X/Z and clear Y/velocity");
        }

        private static void CheckValidatePositiveLinksMatrix()
        {
            var world = new SimulationWorld();

            LF2Character characterHolder = CreateCharacter(
                "SelfCheck_PositiveLink_CharacterHolder", 1, BuildCatchingFrames());
            characterHolder.SetRuntimeSlotIndex(0);
            FlowSelfCheckEntity edgeTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_EdgeTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 399);
            characterHolder.Runtime.LinkState = 1;
            characterHolder.Runtime.TargetSlotIndex = 399;
            characterHolder.Runtime.HeldWeaponStableId = 77;
            edgeTarget.Runtime.HolderStableId = 0;
            edgeTarget.Runtime.LinkState = -2;

            FlowSelfCheckEntity nonCharacterHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NonCharacterHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 23);
            FlowSelfCheckEntity neutralTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NeutralTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 3);
            nonCharacterHolder.Runtime.LinkState = 2;
            nonCharacterHolder.Runtime.TargetSlotIndex = 3;
            nonCharacterHolder.Runtime.HeldWeaponStableId = 88;
            neutralTarget.Runtime.HolderStableId = 23;
            neutralTarget.Runtime.LinkState = 0;

            FlowSelfCheckEntity positiveTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTargetHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 31);
            FlowSelfCheckEntity positiveTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 32);
            FlowSelfCheckEntity positiveTargetChild = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_PositiveTargetChild", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 33);
            positiveTargetHolder.Runtime.LinkState = 3;
            positiveTargetHolder.Runtime.TargetSlotIndex = 32;
            positiveTarget.Runtime.HolderStableId = 31;
            positiveTarget.Runtime.LinkState = 5;
            positiveTarget.Runtime.TargetSlotIndex = 33;
            positiveTargetChild.Runtime.HolderStableId = 32;

            FlowSelfCheckEntity negativeTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_NegativeTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 7);
            negativeTargetHolder.Runtime.LinkState = 1;
            negativeTargetHolder.Runtime.TargetSlotIndex = -1;
            negativeTargetHolder.Runtime.HeldWeaponStableId = 101;

            FlowSelfCheckEntity highTargetHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_HighTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 8);
            highTargetHolder.Runtime.LinkState = 1;
            highTargetHolder.Runtime.TargetSlotIndex = 400;
            highTargetHolder.Runtime.HeldWeaponStableId = 102;

            FlowSelfCheckEntity mismatchHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_MismatchHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 200);
            FlowSelfCheckEntity mismatchTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_MismatchTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 201);
            mismatchHolder.Runtime.LinkState = 1;
            mismatchHolder.Runtime.TargetSlotIndex = 201;
            mismatchHolder.Runtime.HeldWeaponStableId = 103;
            mismatchTarget.Runtime.HolderStableId = 199;

            FlowSelfCheckEntity zeroLink = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_Zero", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 300);
            zeroLink.Runtime.LinkState = 0;
            zeroLink.Runtime.TargetSlotIndex = 400;
            zeroLink.Runtime.HeldWeaponStableId = 104;

            FlowSelfCheckEntity negativeLink = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_Negative", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 301);
            negativeLink.Runtime.LinkState = -2;
            negativeLink.Runtime.TargetSlotIndex = 400;
            negativeLink.Runtime.HeldWeaponStableId = 105;

            world.Register(nonCharacterHolder);
            world.Register(neutralTarget);
            world.Register(highTargetHolder);
            world.Register(negativeTargetHolder);
            world.Register(characterHolder);
            world.Register(edgeTarget);
            world.Register(mismatchTarget);
            world.Register(mismatchHolder);
            world.Register(positiveTarget);
            world.Register(positiveTargetChild);
            world.Register(positiveTargetHolder);
            world.Register(negativeLink);
            world.Register(zeroLink);

            world.ValidateHeldLinksAll(1);

            Expect(characterHolder.Runtime.LinkState == 1 &&
                   characterHolder.Runtime.TargetSlotIndex == 399 &&
                   characterHolder.Runtime.HeldWeaponStableId == 77,
                "positive link slot 0->399 must remain valid for a character holder");
            Expect(nonCharacterHolder.Runtime.LinkState == 2 &&
                   nonCharacterHolder.Runtime.TargetSlotIndex == 3 &&
                   nonCharacterHolder.Runtime.HeldWeaponStableId == 88,
                "positive link validation must include non-character holders");
            Expect(positiveTargetHolder.Runtime.LinkState == 3,
                "target positive LinkState must not invalidate an otherwise valid relation");
            Expect(edgeTarget.Runtime.LinkState == -2 && neutralTarget.Runtime.LinkState == 0 &&
                   positiveTarget.Runtime.LinkState == 5,
                "target LinkState sign must be irrelevant to positive holder validation");

            Expect(negativeTargetHolder.Runtime.LinkState == 0 &&
                   negativeTargetHolder.Runtime.TargetSlotIndex == -1 &&
                   negativeTargetHolder.Runtime.HeldWeaponStableId == 101,
                "target slot -1 must clear only holder LinkState");
            Expect(highTargetHolder.Runtime.LinkState == 0 &&
                   highTargetHolder.Runtime.TargetSlotIndex == 400 &&
                   highTargetHolder.Runtime.HeldWeaponStableId == 102,
                "target slot 400 must clear only holder LinkState");
            Expect(mismatchHolder.Runtime.LinkState == 0 &&
                   mismatchHolder.Runtime.TargetSlotIndex == 201 &&
                   mismatchHolder.Runtime.HeldWeaponStableId == 103 &&
                   mismatchTarget.Runtime.HolderStableId == 199,
                "holder mismatch must clear only holder LinkState and preserve both relation fields");
            Expect(zeroLink.Runtime.LinkState == 0 && zeroLink.Runtime.TargetSlotIndex == 400 &&
                   zeroLink.Runtime.HeldWeaponStableId == 104,
                "link==0 entities must not be processed");
            Expect(negativeLink.Runtime.LinkState == -2 && negativeLink.Runtime.TargetSlotIndex == 400 &&
                   negativeLink.Runtime.HeldWeaponStableId == 105,
                "link<0 entities must not be processed");

            var inactiveWorld = new SimulationWorld();
            FlowSelfCheckEntity inactiveHolder = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_InactiveHolder", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 12);
            FlowSelfCheckEntity inactiveTarget = CreateFlowSelfCheckEntity(
                "SelfCheck_PositiveLink_InactiveTarget", LF2ObjectType.Other,
                LF2States.Standing, 0, 0, 0, 13);
            inactiveHolder.Runtime.LinkState = 1;
            inactiveHolder.Runtime.TargetSlotIndex = 13;
            inactiveHolder.Runtime.HeldWeaponStableId = 106;
            inactiveTarget.Runtime.HolderStableId = 12;
            inactiveWorld.Register(inactiveTarget);
            inactiveWorld.Register(inactiveHolder);
            inactiveWorld.Unregister(inactiveTarget);
            inactiveWorld.ValidateHeldLinksAll(1);
            Expect(inactiveHolder.Runtime.LinkState == 0 &&
                   inactiveHolder.Runtime.TargetSlotIndex == 13 &&
                   inactiveHolder.Runtime.HeldWeaponStableId == 106,
                "inactive target must clear only holder LinkState");
        }

        private static void AdvanceFlowToEvenToggle(SimulationWorld world)
        {
            world.AdvanceBattleFlowTick(1);
            world.AdvanceBattleFlowTick(2);
        }

        private static void CheckPreFrameXBoundsMatrix()
        {
            const float baseStageWidth = 800f;
            const int xMaxOverride = 500;

            FlowSelfCheckEntity ordinary = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOrdinary", LF2ObjectType.Character, 0, 1, -1, 200, 0);
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   ordinary.Runtime.X == 0f && ordinary.Runtime.XInt == 0,
                "slot<20 ordinary character must clamp its lower X bound to zero");
            ordinary.Runtime.X = 700f;
            ordinary.Runtime.HitStop = 0;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 500f,
                "slot<20 ordinary character must apply the phase X override when hit stop is zero");
            ordinary.Runtime.X = 700f;
            ordinary.Runtime.HitStop = 1;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 700f,
                "slot<20 ordinary character must ignore the phase X override during hit stop");
            ordinary.Runtime.X = 900f;
            Expect(!ordinary.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && ordinary.Runtime.X == 800f,
                "base stage width clamp must still run while the phase override is hit-stop gated");

            FlowSelfCheckEntity relationFive = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsRelationFive", LF2ObjectType.Character, 0, 5, -301, 200, 1);
            Expect(!relationFive.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && relationFive.Runtime.X == -300f,
                "RelationTeam 5 character must use the -300 lower X bound");
            relationFive.Runtime.X = 700f;
            Expect(!relationFive.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && relationFive.Runtime.X == 700f,
                "RelationTeam 5 character must ignore the phase X override");

            FlowSelfCheckEntity reservedSlot = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsReserved", LF2ObjectType.Character, 0, 1, -101, 200, 20);
            Expect(!reservedSlot.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && reservedSlot.Runtime.X == -100f,
                "slot>=20 character must use the -100 lower X bound");
            reservedSlot.Runtime.X = 901f;
            Expect(!reservedSlot.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && reservedSlot.Runtime.X == 900f,
                "slot>=20 character must use base stage width plus 100 and ignore phase override");

            FlowSelfCheckEntity type3LowerEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3LowerEdge", LF2ObjectType.SpecialAttack, 0, 1, -300, 200, 2);
            FlowSelfCheckEntity type3UpperEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3UpperEdge", LF2ObjectType.SpecialAttack, 0, 1, 1100, 200, 3);
            Expect(!type3LowerEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   !type3UpperEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "type3 exact -300/base+300 edges must remain active");
            FlowSelfCheckEntity type3Outside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsType3Outside", LF2ObjectType.SpecialAttack, 0, 1, 1101, 200, 4);
            Expect(type3Outside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "type3 outside base stage width plus 300 must be freed");

            FlowSelfCheckEntity oid122 = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOid122", LF2ObjectType.LightWeapon, 0, 1, 0, 200, 5);
            oid122.ObjectId = 122;
            oid122.Unk344 = 1;
            Expect(!oid122.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && oid122.Runtime.X == 10f,
                "oid122 with Unk344>0 must clamp to the 10 lower X bound");
            FlowSelfCheckEntity oid123 = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsOid123", LF2ObjectType.Other, 0, 1, 800, 200, 6);
            oid123.ObjectId = 123;
            oid123.Unk344 = 2;
            Expect(!oid123.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && oid123.Runtime.X == 790f,
                "oid123 with Unk344>0 must clamp to base stage width minus 10");
            FlowSelfCheckEntity wrongWeaponField = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWrongWeaponField", LF2ObjectType.LightWeapon, 0, 1, 5, 200, 7);
            wrongWeaponField.ObjectId = 122;
            wrongWeaponField.Unk344 = 0;
            wrongWeaponField.Runtime.WeaponFlightCounter = 100;
            Expect(!wrongWeaponField.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && wrongWeaponField.Runtime.X == 5f,
                "oid122 bounds must use Unk344 rather than WeaponFlightCounter");

            FlowSelfCheckEntity groundedLowerEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedLowerEdge", LF2ObjectType.Other, 0, 1, 0, 200, 8);
            groundedLowerEdge.Runtime.YInt = 0;
            FlowSelfCheckEntity groundedUpperEdge = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedUpperEdge", LF2ObjectType.Other, 0, 1, 800, 200, 9);
            groundedUpperEdge.Runtime.YInt = 0;
            Expect(!groundedLowerEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) &&
                   !groundedUpperEdge.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "ordinary grounded non-character exact stage edges must remain active");
            FlowSelfCheckEntity groundedOutside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsGroundedOutside", LF2ObjectType.Other, 0, 1, -1, 200, 10);
            groundedOutside.Runtime.YInt = 0;
            Expect(groundedOutside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "ordinary grounded non-character outside the base stage must be freed");
            FlowSelfCheckEntity airborneOutside = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsAirborneOutside", LF2ObjectType.Other, 0, 1, 900, 200, 11);
            airborneOutside.Runtime.YInt = 1;
            Expect(!airborneOutside.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride) && airborneOutside.Runtime.X == 900f,
                "ordinary airborne non-character outside the base stage must remain active");

            FlowSelfCheckEntity truncation = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsXInt", LF2ObjectType.Character, 0, 1, 123, 200, 12);
            truncation.Runtime.X = 123.75f;
            Expect(!truncation.ApplyPreFrameXBounds(baseStageWidth, 0) && truncation.Runtime.XInt == 123,
                "surviving PreFrame X bounds must mirror truncated XInt");

            var transformedCharacter = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            transformedCharacter.BindData("SelfCheck_BoundsClrCharacterDatType3", 912, BuildCatchingFrames());
            transformedCharacter.SetRuntimeSlotIndex(13);
            transformedCharacter.Runtime.X = 1101f;
            Expect(transformedCharacter.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride),
                "current DAT type must select type3 bounds even for a character CLR shell");

            var world = new SimulationWorld();
            FlowSelfCheckEntity hitStopped = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWorldBaseWidth", LF2ObjectType.Character, 0, 1, 700, 200, 0);
            hitStopped.Runtime.HitStop = 1;
            world.Register(hitStopped);
            world.Runtime.Stage.ApplyPhaseBound(xMaxOverride);
            Expect(world.Runtime.Stage.StageWidthPx == xMaxOverride,
                "phase setup must retain the existing active StageWidthPx contract");
            world.ApplyPreFrameBoundsAll();
            Expect(hitStopped.Runtime.X == 700f && world.Runtime.Stage.BaseStageWidthPx >= 794,
                "PreFrame entity bounds must use base stage width separately from active phase width");

            FlowSelfCheckEntity worldFreed = CreateFlowSelfCheckEntity(
                "SelfCheck_BoundsWorldFree", LF2ObjectType.SpecialAttack, 0, 1, 5000, 200, 14);
            world.Register(worldFreed);
            world.ApplyPreFrameBoundsAll();
            var entities = new List<LF2Entity>();
            world.GetAllEntities(entities);
            Expect(!entities.Contains(worldFreed),
                "PreFrame out-of-bounds free must remove the entity through the world lifecycle");
        }

        private static FlowSelfCheckEntity CreateFlowSelfCheckEntity(
            string name,
            LF2ObjectType objectType,
            int state,
            int relationTeam,
            int x,
            int z,
            int slot)
        {
            var entity = new FlowSelfCheckEntity(objectType);
            entity.BindData(name, 900 + slot, new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state, 0, 0, 39, 79),
                },
            });
            entity.RelationTeam = relationTeam;
            entity.SetRuntimeSlotIndex(slot);
            entity.Runtime.SetPosition(x, 0f, z);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.Dir = "right";
            return entity;
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

        private static void CheckSimulationPassesImmediateFrameDoesNotZeroAttacking()
        {
            // BMD-023: SimulationWorld.Passes.partial.cs state=500/501 transform branches
            // used to call entity.ImmediateFrame(N), which zeros AttackingCounter as a side
            // effect (LF2Entity.cs:824). Baseline FrameTick.cs:67-76 SetFrameImmediate only
            // writes Frame + FrameWaitCounter. The fix routes through
            // DirectWriteFramePreserveWaitCounter, which delegates to SetFrameTickDirect and
            // leaves AttackingCounter alone.
            //
            // We test the replacement path end-to-end: build an entity, set Frame.N to a
            // state=500 frame, stash AttackingCounter, call the replacement, and assert
            // AttackingCounter survives while Frame advances. This covers all three call
            // sites (SimulationWorld.Passes.partial.cs:140/:168/:186) since they share the
            // same SetFrameTickDirect backing.
            var character = CreateCharacter("SelfCheck_PassesAttacking", 1, BuildCatchingFrames());
            character.ImmediateFrame(0);
            const int attackingBefore = 11;
            character.AttackingCounter = attackingBefore;

            character.DirectWriteFramePreserveWaitCounter(212);

            Expect(character.Frame != null && character.Frame.N == 212,
                "BMD-023: DirectWriteFramePreserveWaitCounter 必须把 Frame.N 写到目标帧 212");
            Expect(character.AttackingCounter == attackingBefore,
                "BMD-023: state=500/501 修复点必须保留 AttackingCounter，" +
                "ImmediateFrame 路径会在 LF2Entity.cs:824 将其清零，违反 baseline parity");
        }

        private static void CheckArestCooldownRule()
        {
            Expect(LF2Entity.ResolveArestCooldown(0, 0) == 4, "arest (0,0) must resolve to 4");
            Expect(LF2Entity.ResolveArestCooldown(3, 0) == 4, "arest (3,0) must resolve to 4");
            Expect(LF2Entity.ResolveArestCooldown(4, 0) == 4, "arest (4,0) must remain 4");
            Expect(LF2Entity.ResolveArestCooldown(15, 0) == 15, "arest (15,0) must remain 15");
            Expect(LF2Entity.ResolveArestCooldown(0, 1) == 0, "arest (0,1) must remain 0");
            Expect(LF2Entity.ResolveArestCooldown(2, 20) == 2, "arest (2,20) must remain 2");
            Expect(LF2Entity.ResolveArestCooldown(15, 20) == 15, "arest (15,20) must remain 15");
        }

        private static void CheckFrameTickDefendLockTail()
        {
            var character = CreateCharacter("SelfCheck_FrameTickDefendLock", 1, new LF2CharacterData
            {
                name = "SelfCheckFrameTickDefendLock",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 5, 0, 39, 79),
                    Frame(1, 0, 5, 1, 39, 79),
                    Frame(110, 0, 5, 110, 39, 79),
                    Frame(114, 0, 5, 114, 39, 79),
                }
            });

            character.ImmediateFrame(110);
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(1);
            Expect(character.Runtime.CdDefendLock == 3,
                "frame_tick tail must set CdDefendLock=3 on frame 110");

            character.ImmediateFrame(114);
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(2);
            Expect(character.Runtime.CdDefendLock == 3,
                "frame_tick tail must set CdDefendLock=3 on frame 114");

            character.ImmediateFrame(110);
            character.Frame.D.cpoint = new CatchPoint { kind = 2 };
            character.Runtime.CdDefendLock = 0;
            character.SimFrameTick(3);
            Expect(character.Runtime.CdDefendLock == 0,
                "frame_tick cpoint kind=2 early return must not set CdDefendLock on frame 110");
            character.Frame.D.cpoint = null;

            character.ImmediateFrame(1);
            character.Runtime.CdDefendLock = 7;
            character.SimFrameTick(4);
            Expect(character.Runtime.CdDefendLock == 7,
                "frame_tick tail must not change CdDefendLock on an ordinary frame");

            var world = new SimulationWorld();
            world.Register(character);
            character.Runtime.CdDefendLock = 3;
            world.VrestTickAll(5);
            Expect(character.Runtime.CdDefendLock == 2,
                "cooldowns pass must decrement CdDefendLock from 3 to 2");
            world.VrestTickAll(6);
            Expect(character.Runtime.CdDefendLock == 1,
                "cooldowns pass must decrement CdDefendLock from 2 to 1");
            world.VrestTickAll(7);
            Expect(character.Runtime.CdDefendLock == 0,
                "cooldowns pass must decrement CdDefendLock from 1 to 0");
            world.VrestTickAll(8);
            Expect(character.Runtime.CdDefendLock == 0,
                "cooldowns pass must keep CdDefendLock at zero");
        }

        private static void CheckKind0HitRecords()
        {
            LF2CharacterData frameData = new LF2CharacterData
            {
                name = "SelfCheckKind0HitRecord",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 40, 50),
                }
            };
            var attacker = CreateCharacter("SelfCheck_HitRecordAttacker", 1, frameData);
            var victim = CreateCharacter("SelfCheck_HitRecordVictim", 2, frameData);
            attacker.SwitchDir("right");
            attacker.Runtime.XInt = 100;
            attacker.Runtime.YInt = -20;
            victim.Runtime.XInt = 120;
            victim.Runtime.YInt = -10;

            attacker.Runtime.ZInt = 30;
            victim.Runtime.ZInt = 20;
            attacker.SetRuntimeSlotIndex(8);
            victim.SetRuntimeSlotIndex(3);
            victim.RecordKind0Hit(attacker, new InteractionArea
            {
                kind = 0, x = 5, y = 7, w = 30, h = 20, fall = 61, effect = 0
            });
            Expect(attacker.HitRecordCount == 1 && victim.HitRecordCount == 0,
                "kind0 hit record must use the entity with the larger ZInt as owner");
            Expect(attacker.GetHitRecordAge(0) == 0,
                "effect=0 and fall>60 must create timer 0");
            Expect(attacker.GetHitRecordX(0) >= 91 && attacker.GetHitRecordX(0) <= 99,
                "kind0 hit record X must use the integer frame/itr formula plus [-4,4] RNG");
            Expect(attacker.GetHitRecordZ(0) >= -27 && attacker.GetHitRecordZ(0) <= -19,
                "kind0 hit record Z must use the integer frame/itr formula plus [-4,4] RNG");

            attacker.Runtime.ZInt = 10;
            victim.Runtime.ZInt = 20;
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 0 });
            Expect(victim.HitRecordCount == 1 && victim.GetHitRecordAge(0) == 10,
                "effect=0 and fall<=60 must create timer 10 on the larger-Z victim");

            attacker.Runtime.ZInt = 15;
            victim.Runtime.ZInt = 15;
            attacker.SetRuntimeSlotIndex(9);
            victim.SetRuntimeSlotIndex(2);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 61, effect = 1 });
            Expect(attacker.HitRecordCount == 2 && attacker.GetHitRecordAge(1) == 20,
                "equal ZInt must use the larger runtime slot owner; effect=1/fall>60 timer must be 20");

            attacker.SetRuntimeSlotIndex(2);
            victim.SetRuntimeSlotIndex(9);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 1 });
            Expect(victim.HitRecordCount == 2 && victim.GetHitRecordAge(1) == 30,
                "equal ZInt must use the larger runtime slot owner; effect=1/fall<=60 timer must be 30");

            attacker.Runtime.ZInt = 10;
            victim.Runtime.ZInt = 20;
            for (int i = victim.HitRecordCount; i < LF2Entity.MaxHitRecordSlots; i++)
                victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 60, effect = 0 });

            int tailAge = victim.GetHitRecordAge(LF2Entity.MaxHitRecordSlots - 1);
            int tailX = victim.GetHitRecordX(LF2Entity.MaxHitRecordSlots - 1);
            int tailZ = victim.GetHitRecordZ(LF2Entity.MaxHitRecordSlots - 1);
            victim.RecordKind0Hit(attacker, new InteractionArea { kind = 0, fall = 61, effect = 1 });
            Expect(victim.HitRecordCount == LF2Entity.MaxHitRecordSlots,
                "kind0 hit records must not grow beyond 10 slots");
            Expect(victim.GetHitRecordAge(LF2Entity.MaxHitRecordSlots - 1) == tailAge &&
                   victim.GetHitRecordX(LF2Entity.MaxHitRecordSlots - 1) == tailX &&
                   victim.GetHitRecordZ(LF2Entity.MaxHitRecordSlots - 1) == tailZ,
                "a full kind0 hit-record owner must leave its tail record unchanged");
        }

        private static void CheckAlternateHurtTriggerMatrix()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHurtAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHurtVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(10, LF2States.Standing, 0, 10, 39, 79),
                    Frame(20, LF2States.Standing, 0, 20, 39, 79),
                    Frame(23, LF2States.Defending, 0, 23, 39, 79),
                    Frame(110, LF2States.Defending, 0, 110, 39, 79),
                }
            };
            var attacker = CreateCharacter("SelfCheck_AlternateHurtAttacker", 1, attackerData);
            var victim = CreateCharacter("SelfCheck_AlternateHurtVictim", 2, victimData);
            var itr = new InteractionArea
            {
                kind = 0,
                effect = 0,
                bdefend = 0,
                dvx = 5,
            };

            attacker.SwitchDir("right");
            victim.SwitchDir("right");
            victim.Health.HP = 500;
            victim.Runtime.PrevFrame2 = 0;
            victim.HitStateCount = 15;
            victim.ImmediateFrame(20);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid37 must use alternate hurt while HitStateCount is within 15");
            itr.effect = 6;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid37 heavy effects must reject alternate hurt");

            victimData.type_sub = 6;
            victim.HitStateCount = 1;
            itr.effect = 0;
            victim.ImmediateFrame(10);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 must use alternate hurt below frame 20");
            victim.ImmediateFrame(20);
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 frame 20 in a non-special state must reject alternate hurt");
            victim.ImmediateFrame(23);
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid6 state 7 must use alternate hurt at frame 20 or later");

            victimData.type_sub = 52;
            victim.HitStateCount = 15;
            victim.ImmediateFrame(20);
            attackerData.type_sub = 1;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "oid52 must use alternate hurt for an ordinary attacker within its hit window");
            attackerData.type_sub = 208;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "attacker oid208 must reject oid52 alternate hurt");

            victimData.type_sub = 1;
            victim.HitStateCount = 100;
            victim.Runtime.PrevFrame2 = 110;
            victim.Health.HP = 500;
            attackerData.type_sub = 1;
            itr.bdefend = 60;
            itr.dvx = 5;
            attacker.SwitchDir("right");
            victim.SwitchDir("left");
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 state 7 must allow alternate hurt when facings differ");
            victim.SwitchDir("right");
            itr.dvx = -1;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 state 7 must allow alternate hurt for negative dvx");
            itr.dvx = 5;
            attackerData.type_sub = 124;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "special defend attacker oid124 must allow alternate hurt with matching facings");
            attackerData.type_sub = 1;
            victim.SwitchDir("left");
            itr.bdefend = 61;
            Expect(!LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "PrevFrame2 defend alternate hurt must reject bdefend above 60");

            victimData.type_sub = 37;
            victim.HitStateCount = 0;
            victim.Runtime.PrevFrame2 = 0;
            itr.kind = 9;
            itr.effect = 0;
            itr.bdefend = 0;
            Expect(LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr),
                "raw kind9 fixture must otherwise satisfy alternate-hurt selection");
            Expect(!(itr.kind != 9 && LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, victim, itr)),
                "the caller gate must keep raw kind9 out of alternate hurt");
        }

        private static void CheckAlternateDamageCoreSideEffects()
        {
            var ordinaryData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageOrdinary",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                    Frame(110, LF2States.Defending, 5, 110, 39, 79),
                    Frame(112, LF2States.BrokenDefend, 5, 112, 39, 79),
                }
            };
            var world = new SimulationWorld();
            var holder = CreateCharacter("SelfCheck_AlternateDamageHolder", 3, ordinaryData);
            var attacker = CreateCharacter("SelfCheck_AlternateDamageAttacker", 1, ordinaryData);
            var victim = CreateCharacter("SelfCheck_AlternateDamageVictim", 37, victimData);
            world.Register(holder);
            world.Register(attacker);
            world.Register(victim);

            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = holder.Runtime.SlotIndex;
            attacker.SwitchDir("right");
            attacker.FrameDelay = -9;
            attacker.AttackExempt = 9;
            attacker.Runtime.ZInt = 10;

            holder.KillStat = 0;
            holder.ComboCountAtk = 0;
            holder.FrameDelay = 0;

            victim.ImmediateFrame(110);
            victim.Runtime.PrevFrame2 = 110;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;
            victim.Health.HP = 5;
            victim.Health.HPBound = 101;
            victim.Health.HPLost = 7;
            victim.FallDamageDiv = 200;
            victim.KillCount = -1;
            victim.Unk344 = 1;
            victim.ComboCountVic = 0;
            victim.FallCounter = 0;
            victim.AttackingCounter = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.AttackExempt = 13;
            victim.Trans.SetWait(victim.Frame.D.wait, 73);

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                bdefend = 31,
                dvx = 5,
                arest = 2,
                vrest = 15,
                effect = 0,
            };

            LF2AlternateDamageResolver.ApplyAlternateDamage(attacker, victim, victim.HitCounters, itr);
            victim.RecordKind0Hit(attacker, itr);

            Expect(victim.Health.HP == 0 && victim.Health.HPBound == 100,
                "alternate damage must apply adjusted injury 50, reduced to 5, with integer HPBound division");
            Expect(victim.Health.HPLost == 7,
                "alternate damage must leave HPLost unchanged");
            Expect(holder.KillStat == 1 && holder.ComboCountAtk == 5 && victim.ComboCountVic == 5,
                "lethal alternate damage must update holder kill/combo and victim combo stats once");
            Expect(world.KillStats[1] == 1 && world.DamageStats[1] == 5,
                "alternate damage must update world kill and damage stat slot Unk344=1");
            Expect(victim.FallCounter == 80 && victim.AttackingCounter == 0 &&
                   victim.HitStateCount == 31 && victim.HitCount == 1,
                "alternate damage must write lethal fall, attacking, hit-state, and hit-count fields");
            Expect(attacker.FrameDelay == 3 && victim.FrameDelay == -5,
                "alternate damage must overwrite both attacker and victim frame delays");
            Expect(victim.CurrentFrameId == 112 && victim.Trans.WaitCounter == 73,
                "grounded defended alternate damage must enter frame 112 without resetting wait_counter");
            Expect(Nearly(victim.KnockbackVx, 2f),
                "ground alternate knockback must use integer dvx/2 for dvx=5");
            Expect(attacker.AttackExempt == 2 && victim.AttackExempt == 13,
                "alternate damage must apply arest to the attacker only");
            Expect(holder.FrameDelay == 3,
                "a negative-link attacker must propagate its overwritten delay to the active holder");

            int attackerSlot = attacker.Runtime.SlotIndex;
            Expect(victim.ItrRest.HasVrest(attackerSlot),
                "alternate damage must create victim-side vrest for the attacker slot");
            for (int i = 0; i < 11; i++)
                victim.ItrRest.TickVrestForAttacker(attackerSlot);
            Expect(victim.ItrRest.HasVrest(attackerSlot),
                "vrest=15 must clamp to 12 rather than expire after 11 ticks");
            victim.ItrRest.TickVrestForAttacker(attackerSlot);
            Expect(!victim.ItrRest.HasVrest(attackerSlot),
                "vrest=15 must expire after the clamped twelfth tick");

            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "the alternate-damage caller must record exactly one kind0 hit");

            int[] killStats = world.KillStats;
            int[] damageStats = world.DamageStats;
            world.ResetRuntimeState();
            Expect(ReferenceEquals(killStats, world.KillStats) && ReferenceEquals(damageStats, world.DamageStats),
                "world reset must preserve alternate-damage stat array identity");
            for (int i = 0; i < killStats.Length; i++)
            {
                Expect(killStats[i] == 0 && damageStats[i] == 0,
                    "world reset must clear every alternate-damage stat slot");
            }
        }

        private static void CheckAlternateDamageMotionTailMatrix()
        {
            LF2CharacterData frameData = BuildAlternateDamageMotionFrames();

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 3.0),
                    "ground Fall80/dvx0 with a right-facing ordinary attacker must add +3 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.SwitchDir("left");
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -3.0),
                    "ground Fall80/dvx0 with a left-facing ordinary attacker must add -3 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 6.0),
                    "ground state2000 Fall80/dvx0 must add +6 when the attacker is left of the victim"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 20.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    victim.FallCounter = 80;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -6.0),
                    "ground state2000 Fall80/dvx0 must add -6 when the attacker is right of the victim"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 5.0),
                    "ground state2000 nonzero dvx must use attacker/victim X ordering"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 20.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.effect = 22;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 5.0),
                    "ground effect22 must add +dvx when victim X is not greater than attacker X"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    itr.effect = 23;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "ground effect23 must add -dvx when victim X is greater than attacker X"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    victim.FallCounter = 80;
                    victim.Runtime.Vx = 5.0;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, 6.0),
                    "air Fall80 with abs(Vx)<6 and dvx<6 must use right-facing +6 knockback"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    itr.effect = 23;
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "air effect23 must use victim/attacker X ordering"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.SwitchDir("left");
                    SetAlternateDamagePosition(victim, 10.0, -10.0);
                    itr.dvx = 5;
                },
                (attacker, victim, itr) => Expect(Nearly(victim.KnockbackVx, -5.0),
                    "air generic alternate knockback must use the full signed dvx"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    victim.ImmediateFrame(110);
                    victim.Runtime.PrevFrame2 = 0;
                    victim.HitStateCount = 0;
                    victim.Trans.SetWait(victim.Frame.D.wait, 47);
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    victim.CurrentFrameId == 111 && victim.Trans.WaitCounter == 47,
                    "ground frame110 with HitStateCount<=30 must enter frame111 and preserve wait_counter"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(20);
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 63);
                    attacker.Runtime.Vz = 6.0;
                    victim.KnockbackVx = 8.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    attacker.CurrentFrameId >= 0 && attacker.CurrentFrameId < 16 &&
                    Nearly(attacker.Runtime.Vx, -4.0) &&
                    Nearly(attacker.Runtime.Vy, -4.0) &&
                    Nearly(attacker.Runtime.Vz, -4.0) &&
                    attacker.Trans.WaitCounter == 63,
                    "state1002 tail must select frame0..15, apply reflected velocity, and preserve wait_counter"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    attacker.Runtime.Vx = 5.0;
                    attacker.Runtime.Vz = 10.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    Nearly(attacker.Runtime.Vx, 2.0) && Nearly(attacker.Runtime.Vz, 4.0),
                    "state2000 attacker moving toward the victim must damp Vx and Vz by 0.4"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(21);
                    SetAlternateDamagePosition(attacker, 0.0, 0.0);
                    SetAlternateDamagePosition(victim, 10.0, 0.0);
                    attacker.Runtime.Vx = -5.0;
                    attacker.Runtime.Vz = 10.0;
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    Nearly(attacker.Runtime.Vx, -5.0) && Nearly(attacker.Runtime.Vz, 10.0),
                    "state2000 attacker moving away from the victim must not damp velocity"));

            RunAlternateDamageMotionCase(
                frameData,
                (attacker, victim, itr) =>
                {
                    attacker.ImmediateFrame(22);
                    attacker.AttackingCounter = 7;
                    attacker.Runtime.Vx = 5.0;
                    attacker.Runtime.Vz = 9.0;
                    attacker.Trans.SetWait(attacker.Frame.D.wait, 71);
                    itr.dvx = 0;
                },
                (attacker, victim, itr) => Expect(
                    attacker.CurrentFrameId == 10 &&
                    attacker.AttackingCounter == 0 &&
                    Nearly(attacker.Runtime.Vx, 0.0) &&
                    Nearly(attacker.Runtime.Vz, 9.0) &&
                    attacker.Trans.WaitCounter == 71,
                    "state3000 tail must enter frame10, clear attacking/Vx, preserve Vz and wait_counter"));
        }

        private static void CheckAlternateDamageCharacterEntry()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageCharacterAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageCharacterVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var world = new SimulationWorld();
            var attacker = CreateCharacter("SelfCheck_AlternateDamageCharacterAttacker", 1, attackerData);
            var victim = CreateCharacter("SelfCheck_AlternateDamageCharacterVictim", 37, victimData);
            world.Register(attacker);
            world.Register(victim);

            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.ZInt = 10;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };
            var volume = new PhysicsState.BattleVolume(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            bool resolved = victim.Hit(itr, attacker, Vector3.zero, volume);

            Expect(resolved,
                "LF2Character.Hit must resolve the shared alternate-damage branch");
            Expect(victim.Health.HP == 90 && victim.Health.HPBound == 97 && victim.Health.HPLost == 7,
                "LF2Character.Hit alternate damage must apply reduced injury without changing HPLost");
            Expect(victim.FrameDelay == -5 && victim.HitCount == 1 && Nearly(victim.KnockbackVx, 2f),
                "LF2Character.Hit alternate damage must apply victim delay, hit count, and integer half-dvx");
            Expect(attacker.FrameDelay == 3 && attacker.AttackExempt == 4,
                "LF2Character.Hit alternate damage must apply attacker delay and arest");
            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "LF2Character.Hit alternate damage must record exactly one kind0 hit");
        }

        private static void CheckAlternateDamageSharedDatEntry()
        {
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageSharedAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageSharedVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "SelfCheck_AlternateDamageSharedAttacker",
                1,
                attackerData);
            var victim = new AlternateDamageSelfCheckEntity();
            victim.BindData(37, victimData);
            world.Register(attacker);
            world.Register(victim);

            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.ZInt = 10;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 7;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.Y = 0f;
            victim.Runtime.YInt = 0;
            victim.Runtime.Vx = 0f;
            victim.Runtime.ZInt = 20;
            victim.KnockbackVx = 0f;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            bool resolved = LF2CharacterDatHitResolver.TryResolveHit(
                victim,
                itr,
                attacker,
                Vector3.zero,
                default);

            Expect(resolved,
                "shared-DAT character entry must resolve the shared alternate-damage branch");
            Expect(victim.Health.HP == 90 && victim.Health.HPBound == 97 && victim.Health.HPLost == 7,
                "shared-DAT alternate damage must apply reduced injury without changing HPLost");
            Expect(victim.FrameDelay == -5 && victim.HitCount == 1 && Nearly(victim.KnockbackVx, 2f),
                "shared-DAT alternate damage must apply victim delay, hit count, and integer half-dvx");
            Expect(attacker.FrameDelay == 3 && attacker.AttackExempt == 4,
                "shared-DAT alternate damage must apply attacker delay and arest");
            Expect(attacker.HitRecordCount + victim.HitRecordCount == 1,
                "shared-DAT alternate damage must record exactly one kind0 hit");
        }

        private static void CheckAlternateDamageHeavyWeaponEntries()
        {
            LF2CharacterData weaponData = BuildAlternateDamageWeaponFrames();
            var victimData = new LF2CharacterData
            {
                name = "SelfCheckAlternateHeavyVictim",
                type_sub = 37,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 0, 39, 79),
                }
            };
            var itr = new InteractionArea
            {
                kind = 0,
                injury = 100,
                dvx = 5,
                bdefend = 0,
                arest = 4,
                vrest = 0,
                effect = 0,
            };

            var characterWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon characterAttacker = CreateSelfCheckWeapon(
                "SelfCheck_AlternateHeavyCharacterAttacker",
                1,
                2,
                weaponData,
                20);
            LF2Character characterVictim = CreateCharacter(
                "SelfCheck_AlternateHeavyCharacterVictim",
                37,
                victimData);
            characterWorld.Register(characterAttacker);
            characterWorld.Register(characterVictim);
            PrepareAlternateEntry(characterAttacker, characterVictim);
            characterAttacker.Runtime.WeaponState = LF2States.WeaponThrowing;
            characterAttacker.Runtime.Vz = 6.0;

            bool characterResolved = characterVictim.Hit(itr, characterAttacker, Vector3.zero, default);

            Expect(characterResolved && characterVictim.Health.HP == 90,
                "real-character alternate damage must use the heavy weapon's original injury");
            Expect(characterAttacker.Frame.N >= 0 && characterAttacker.Frame.N < 16 &&
                   Nearly(characterAttacker.Runtime.Vx, -1.0) &&
                   Nearly(characterAttacker.Runtime.Vy, -4.0) &&
                   Nearly(characterAttacker.Runtime.Vz, -4.0),
                "state1002 alternate tail must update frame and reflected velocity on a real weapon");
            Expect(characterAttacker.Runtime.WeaponState == LF2States.WeaponThrowing,
                "state1002 alternate tail must not rewrite the independent runtime weapon state");

            var sharedWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon sharedAttacker = CreateSelfCheckWeapon(
                "SelfCheck_AlternateHeavySharedAttacker",
                1,
                2,
                weaponData,
                0);
            var sharedVictim = new AlternateDamageSelfCheckEntity();
            sharedVictim.BindData(37, victimData);
            sharedWorld.Register(sharedAttacker);
            sharedWorld.Register(sharedVictim);
            PrepareAlternateEntry(sharedAttacker, sharedVictim);

            bool sharedResolved = LF2CharacterDatHitResolver.TryResolveHit(
                sharedVictim,
                itr,
                sharedAttacker,
                Vector3.zero,
                default);

            Expect(sharedResolved && sharedVictim.Health.HP == 90,
                "shared-DAT alternate damage must use the heavy weapon's original injury");

            var guardWorld = new SimulationWorld();
            LF2Character guardAttacker = CreateCharacter(
                "SelfCheck_AlternateGuardAttacker",
                1,
                BuildAlternateDamageMotionFrames());
            AlternateDamageSelfCheckWeapon guardVictim = CreateSelfCheckWeapon(
                "SelfCheck_AlternateGuardWeaponVictim",
                2,
                1,
                weaponData,
                0);
            guardWorld.Register(guardAttacker);
            guardWorld.Register(guardVictim);
            guardVictim.Health.HP = 100;
            guardVictim.Health.HPBound = 100;

            LF2AlternateDamageResolver.ApplyAlternateDamage(guardAttacker, guardVictim, null, itr);

            Expect(guardVictim.Health.HP == 100 && guardVictim.Health.HPBound == 100,
                "alternate damage must reject a non-character DAT victim");
        }

        private static void CheckAlternateDamageInteractionVrest()
        {
            var weaponItr = MakeInteractionItr(0, 1, 100, 4);
            var weaponData = new LF2CharacterData
            {
                name = "SelfCheckAlternateVrestWeapon",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(weaponItr) },
            };
            var victimData = BuildInteractionVictimData("SelfCheckAlternateVrestWeaponVictim", 37);
            var weaponWorld = new SimulationWorld();
            AlternateDamageSelfCheckWeapon weapon = CreateSelfCheckWeapon(
                "SelfCheck_AlternateVrestWeapon",
                1,
                1,
                weaponData,
                0);
            LF2Character weaponVictim = CreateInteractionCharacter(
                "SelfCheck_AlternateVrestWeaponVictim",
                37,
                victimData);
            RegisterInteractionPair(weaponWorld, weapon, weaponVictim);

            weaponWorld.CaptureCollisionFrameSnapshotsAll();
            weaponWorld.CollectCollisionCandidatesAll();
            weaponWorld.ObjectInteractionTickAll(1);
            weaponWorld.EndCollisionCandidateConsumption();

            int weaponSlot = weapon.Runtime.SlotIndex;
            Expect(weaponVictim.Health.HP == 90 && weaponVictim.ItrRest.GetVrest(weaponSlot) == 4,
                "weapon interaction must preserve alternate vrest clamp 1->4 after Hit returns");
            Expect(weaponItr.vrest == 1,
                "weapon interaction must not mutate authored raw vrest data");

            var sharedItr = MakeInteractionItr(0, 20, 100, 4);
            var sharedData = new LF2CharacterData
            {
                name = "SelfCheckAlternateVrestSharedAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(sharedItr) },
            };
            var sharedWorld = new SimulationWorld();
            var sharedAttacker = new AlternateDamageSelfCheckEntity();
            sharedAttacker.BindData(1, sharedData);
            LF2Character sharedVictim = CreateInteractionCharacter(
                "SelfCheck_AlternateVrestSharedVictim",
                37,
                BuildInteractionVictimData("SelfCheckAlternateVrestSharedVictim", 37));
            RegisterInteractionPair(sharedWorld, sharedAttacker, sharedVictim);

            sharedWorld.CaptureCollisionFrameSnapshotsAll();
            sharedWorld.CollectCollisionCandidatesAll();
            sharedWorld.PostInteractionTickAll(1);
            sharedWorld.EndCollisionCandidateConsumption();

            int sharedSlot = sharedAttacker.Runtime.SlotIndex;
            Expect(sharedVictim.Health.HP == 90 && sharedVictim.ItrRest.GetVrest(sharedSlot) == 12,
                "shared-DAT interaction must preserve alternate vrest clamp 20->12 after Hit returns");
            Expect(sharedItr.vrest == 20,
                "shared-DAT interaction must not mutate authored raw vrest data");
        }

        private static void CheckSpecialAttackDamagePreprocess()
        {
            RunSpecialAttackPreprocessCase(
                kind: 4,
                rawVrest: 1,
                arrange: special =>
                {
                    special.WeaponCount = 1;
                    special.SwitchDir("left");
                    special.Runtime.Vx = 1.0;
                },
                verify: (special, victim, sourceItr) =>
                {
                    Expect(special.Health.HP == 100,
                        "kind4 preprocessing must not zero special-attack HP");
                    Expect(Nearly(victim.KnockbackVx, 3.0),
                        "kind4 preprocessing must convert to kind0 and flip dvx for reverse travel");
                    Expect(victim.ItrRest.GetVrest(special.Runtime.SlotIndex) == 4,
                        "special-attack kind4 must preserve alternate vrest clamp 1->4");
                    Expect(sourceItr.kind == 4 && sourceItr.dvx == 6,
                        "kind4 preprocessing must not mutate authored itr data");
                });

            RunSpecialAttackPreprocessCase(
                kind: 9,
                rawVrest: 20,
                arrange: special => special.SwitchDir("right"),
                verify: (special, victim, sourceItr) =>
                {
                    Expect(special.Health.HP == 0,
                        "kind9 character preprocessing must zero special-attack HP before consume");
                    Expect(victim.Health.HP == 90,
                        "kind9 character preprocessing must convert to kind0 and enter alternate damage");
                    Expect(victim.ItrRest.GetVrest(special.Runtime.SlotIndex) == 12,
                        "special-attack kind9 must preserve alternate vrest clamp 20->12");
                    Expect(sourceItr.kind == 9,
                        "kind9 preprocessing must not mutate authored itr data");
                });
        }

        private static void CheckSimulationWorldLateMutation()
        {
            var world = new SimulationWorld();
            var spawner = new MutationSelfCheckEntity(1, registerDuringLate: true);
            var remover = new MutationSelfCheckEntity(2, unregisterDuringLate: true);

            world.Register(spawner);
            world.Register(remover);
            int removerSlot = remover.Runtime.SlotIndex;

            world.LateEntityUpdateAll(1);

            Expect(spawner.LateTickCount == 1,
                "LateEntityUpdateAll must execute the original spawner entity");
            Expect(remover.LateTickCount == 1,
                "LateEntityUpdateAll must allow an entity to request unregister during SimFrameTick");
            Expect(spawner.Spawned != null && spawner.Spawned.LateTickCount == 1,
                "an entity spawned into a later runtime slot must execute in the same late pass");
            Expect(spawner.Spawned.Runtime.SlotIndex > removerSlot,
                "the mutation fixture must place the spawned entity in a later runtime slot");
            Expect(world.FindEntityByRuntimeSlotForQuery(removerSlot) == null,
                "the unregistering entity must be removed when the late pass flushes mutations");
            Expect(world.ObjectCount == 2,
                "the late-pass mutation flush must leave only the spawner and spawned entity");

            world.LateEntityUpdateAll(2);

            Expect(spawner.LateTickCount == 2 && spawner.Spawned.LateTickCount == 2,
                "the remaining entities must each continue on the second late pass");
            Expect(remover.LateTickCount == 1 &&
                   world.FindEntityByRuntimeSlotForQuery(removerSlot) == null,
                "the removed entity must not execute or reappear on the second late pass");
            Expect(world.ObjectCount == 2,
                "the second late pass must preserve the two remaining entities");

            var lowSlotWorld = new SimulationWorld();
            var releasedLowSlot = new MutationSelfCheckEntity(10);
            var beforeSpawner = new MutationSelfCheckEntity(11);
            var highSlotSpawner = new MutationSelfCheckEntity(12, registerDuringLate: true);
            lowSlotWorld.Register(releasedLowSlot);
            lowSlotWorld.Register(beforeSpawner);
            lowSlotWorld.Register(highSlotSpawner);
            int lowSlot = releasedLowSlot.Runtime.SlotIndex;
            int highSlot = highSlotSpawner.Runtime.SlotIndex;
            lowSlotWorld.Unregister(releasedLowSlot);

            lowSlotWorld.LateEntityUpdateAll(3);

            Expect(highSlotSpawner.Spawned != null && highSlotSpawner.Spawned.Runtime.SlotIndex == lowSlot,
                "the low-slot mutation fixture must reuse the released runtime slot");
            Expect(highSlotSpawner.Spawned.Runtime.SlotIndex < highSlot,
                "the spawned low-slot entity must be behind the current dynamic late scan cursor");
            Expect(highSlotSpawner.Spawned.LateTickCount == 0,
                "an entity spawned into an already-scanned lower runtime slot must wait until the next late pass");

            lowSlotWorld.LateEntityUpdateAll(4);

            Expect(highSlotSpawner.Spawned.LateTickCount == 1,
                "the deferred lower-slot entity must execute exactly once on the next late pass");
        }

        private static void CheckQueuedObjectPointPassBoundaries()
        {
            var oid999Wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [999] = new LF2CharacterDataWrapper(999, BuildSelfCheckOpoint999Data()),
            };
            var oid999Types = new Dictionary<int, int>
            {
                [999] = (int)LF2ObjectType.Other,
            };
            using var oid999Config = new TemporaryRuntimeObjectConfigs(oid999Types, oid999Wrappers);
            using var objectPoolState = new TemporaryObjectPoolInitialization();
            using var sinkWorld = new TemporarySimulationDriverWorld(new SimulationWorld());

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            Expect(factory != null && referencePool != null,
                "queued opoint self-check requires the production factory and reference pool singletons");

            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "queued opoint self-check must start from an empty production queue");

            var frameLogicWorld = new SimulationWorld();
            var producer = new QueuedBoundarySelfCheckEntity(
                QueuedBoundarySelfCheckEntity.Phase.FrameLogic,
                ReleaseSpawnSemantic.ImmediateEffect);
            var observer = new QueuedBoundarySelfCheckEntity(
                QueuedBoundarySelfCheckEntity.Phase.ObserveFrameLogic,
                ReleaseSpawnSemantic.None);
            frameLogicWorld.Register(producer);
            frameLogicWorld.Register(observer);

            frameLogicWorld.FrameLogicBeforeAdvanceAll(1);

            Expect(producer.EnqueueCount == 1 && producer.LastTask != null,
                "the queued frame_logic fixture must publish exactly one task");
            Expect(observer.QueueCountObservedAtFrameLogic == 0,
                "frame_logic tasks must flush before the next runtime-slot entity enters the same pass");
            Expect(IsRecycledAndCleared(producer.LastTask),
                "the frame_logic task must be consumed and recycled at its production boundary");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the frame_logic boundary must leave no queued publication behind");

            frameLogicWorld.FlushQueuedObjectPointTasks();
            Expect(producer.EnqueueCount == 1 && !producer.LastTask.IsFromPool &&
                   GetQueuedObjectPointTaskCount(factory) == 0,
                "an outer safety flush must not publish or recycle an already-consumed frame_logic task twice");

            QueuedBoundarySelfCheckWeapon directBrokenWeapon = CreateQueuedBoundaryWeapon();
            bool completedBrokenCleanup = directBrokenWeapon.TryRunLatePostOpointCleanupPhase();

            Expect(completedBrokenCleanup && directBrokenWeapon.Runtime.PendingFlushDestroy,
                "the real weapon late cleanup phase must mark a depleted destroyable weapon for deferred destroy");
            Expect(GetQueuedObjectPointTaskCount(factory) == 5,
                "oid 100 real weapon cleanup must queue its five C++ broken-weapon fragments");

            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the broken-fragment production boundary must consume the real factory queue");

            directBrokenWeapon.TryRunLatePostOpointCleanupPhase();
            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "repeating cleanup and its safety flush must not publish broken fragments twice");

            var brokenWorld = new SimulationWorld();
            QueuedBoundarySelfCheckWeapon brokenWeapon = CreateQueuedBoundaryWeapon();
            brokenWorld.Register(brokenWeapon);
            int brokenSlot = brokenWeapon.Runtime.SlotIndex;

            brokenWorld.LateEntityUpdateAll(2);

            Expect(brokenWeapon.PendingDestroyObserved && brokenWeapon.TransitDestroyCount == 1,
                $"the full late pass must enter pending destroy and finalize the depleted weapon exactly once; " +
                $"pendingObserved={brokenWeapon.PendingDestroyObserved}, transitDestroyCount={brokenWeapon.TransitDestroyCount}, " +
                $"slotEntity={brokenWorld.FindEntityByRuntimeSlotIncludingPending(brokenSlot)?.Name ?? "null"}");
            Expect(brokenWorld.FindEntityByRuntimeSlotForQuery(brokenSlot) == null,
                "the depleted weapon must leave the world after its real fragment queue boundary");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the full broken-weapon late pass must not leak fragment tasks into a later pass");

            QueuedBoundaryTransitionSelfCheckEntity directTransition = CreateQueuedBoundaryTransitionEntity();
            directTransition.RunLateTailBeforePrevFrame();
            Expect(GetQueuedObjectPointTaskCount(factory) == 15,
                "leaving state 13 through the real late tail must queue the fifteen C++ transition effects");

            factory.FlushTasks();
            directTransition.MirrorLatePrevFrame();
            directTransition.RunLateTailBeforePrevFrame();
            factory.FlushTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "mirroring prev_frame and repeating the safety flush must not republish transition effects");

            var transitionWorld = new SimulationWorld();
            QueuedBoundaryTransitionSelfCheckEntity transition = CreateQueuedBoundaryTransitionEntity();
            transitionWorld.Register(transition);

            transitionWorld.LateEntityUpdateAll(3);

            Expect(transition.Frame.Prev == transition.Frame.N,
                "the full late pass must mirror prev_frame after the real transition-effect production phase");
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "the transition-effect late boundary must consume the real factory queue before the pass continues");

            transitionWorld.FlushQueuedObjectPointTasks();
            Expect(GetQueuedObjectPointTaskCount(factory) == 0,
                "a repeated late safety flush must not duplicate real transition-effect publication");

            CheckRealOpointSpawnSlotVisibility();
        }

        private static LF2CharacterData BuildSelfCheckOpoint999Data()
        {
            var frames = new List<LF2FrameData>(200);
            for (int frameId = 0; frameId < 200; frameId++)
                frames.Add(Frame(frameId, 9999, 100, frameId, 13, 27));

            return new LF2CharacterData
            {
                name = "SelfCheck_RealOpoint999",
                type_sub = 999,
                frames = frames,
            };
        }

        private static void CheckRealOpointSpawnSlotVisibility()
        {
            var highWorld = new SimulationWorld();
            using (new TemporarySimulationDriverWorld(highWorld))
            {
                var producer = new RealOpointProducerSelfCheckEntity("HighSlot");
                highWorld.Register(producer);
                int producerSlot = producer.Runtime.SlotIndex;

                highWorld.LateEntityUpdateAll(30);

                LF2Entity spawned = FindOidEntity(highWorld, 999);
                Expect(spawned is LF2OtherObject && spawned.Renderer != null,
                    "production oid999 opoint must create a real pooled LF2OtherObject with a renderer");
                Expect(spawned.Runtime.SlotIndex > producerSlot,
                    "production oid999 opoint must register into the next free high runtime slot");
                Expect(spawned.AttackingCounter == 1,
                    "an actual oid999 spawned into a later slot must execute frame_tick in the same late pass");
            }

            var lowWorld = new SimulationWorld();
            using (new TemporarySimulationDriverWorld(lowWorld))
            {
                var releasedLow = new DynamicSlotSelfCheckEntity(9001);
                var beforeProducer = new DynamicSlotSelfCheckEntity(9002);
                var producer = new RealOpointProducerSelfCheckEntity("LowSlot");
                lowWorld.Register(releasedLow);
                lowWorld.Register(beforeProducer);
                lowWorld.Register(producer);
                int lowSlot = releasedLow.Runtime.SlotIndex;
                int producerSlot = producer.Runtime.SlotIndex;
                lowWorld.Unregister(releasedLow);

                lowWorld.LateEntityUpdateAll(31);

                LF2Entity spawned = FindOidEntity(lowWorld, 999);
                Expect(spawned is LF2OtherObject && spawned.Renderer != null,
                    "low-slot production opoint must still create the actual oid999 pooled entity");
                Expect(spawned.Runtime.SlotIndex == lowSlot && spawned.Runtime.SlotIndex < producerSlot,
                    $"production oid999 opoint must reuse the released lower dynamic slot; " +
                    $"released={lowSlot}, spawned={spawned.Runtime.SlotIndex}, producer={producerSlot}");
                Expect(spawned.AttackingCounter == 0,
                    "an actual oid999 spawned behind the late scan cursor must not execute in the creation tick");

                lowWorld.LateEntityUpdateAll(32);

                Expect(spawned.AttackingCounter == 1,
                    "the deferred low-slot oid999 must execute exactly once on the next late pass");
                Expect(FindOidEntity(lowWorld, 999) == spawned,
                    "the producer must publish one actual oid999 task rather than duplicating it next tick");
            }
        }

        private static LF2Entity FindOidEntity(SimulationWorld world, int oid)
        {
            for (int slot = 0; slot < 400; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                if (entity != null && entity.ObjectId == oid)
                    return entity;
            }

            return null;
        }

        private static QueuedBoundarySelfCheckWeapon CreateQueuedBoundaryWeapon()
        {
            var weapon = new QueuedBoundarySelfCheckWeapon();
            weapon.BindData(new LF2CharacterData
            {
                name = "SelfCheck_QueuedBrokenWeapon",
                type_sub = 100,
                weapon_hp = 1,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                },
            });
            weapon.Runtime.WeaponFlightCounter = -1;
            weapon.Runtime.SetPosition(100, -20, 100);
            weapon.Runtime.SyncIntegerPosition();
            return weapon;
        }

        private static QueuedBoundaryTransitionSelfCheckEntity CreateQueuedBoundaryTransitionEntity()
        {
            return new QueuedBoundaryTransitionSelfCheckEntity(new LF2CharacterData
            {
                name = "SelfCheck_QueuedTransition",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 100, 0, 39, 79),
                    Frame(10, 13, 100, 10, 39, 79),
                },
            });
        }

        private static void CheckCollisionCandidateCapAndNewbornIsolation()
        {
            InteractionArea itr = MakeInteractionItr(kind: 0, vrest: 1, injury: 10, dvx: 1);
            var attackerData = new LF2CharacterData
            {
                name = "SelfCheck_CandidateCapAttacker",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(itr) },
            };
            var world = new SimulationWorld();
            LF2Character attacker = CreateInteractionCharacter("SelfCheck_CandidateCapAttacker", 1, attackerData);
            world.Register(attacker);
            attacker.Team = 1;
            attacker.RelationTeam = 1;
            attacker.Runtime.SetPosition(0, 0, 0);
            attacker.Runtime.SyncIntegerPosition();

            var targets = new List<LF2Character>();
            for (int i = 0; i < 21; i++)
            {
                LF2Character target = CreateInteractionCharacter(
                    $"SelfCheck_CandidateCapTarget_{i}",
                    100 + i,
                    BuildInteractionVictimData($"SelfCheckCandidateCapTarget{i}", 100 + i));
                world.Register(target);
                target.Team = 2;
                target.RelationTeam = 2;
                target.Runtime.SetPosition(0, 0, 0);
                target.Runtime.SyncIntegerPosition();
                targets.Add(target);
            }

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            var query = world.SceneQuery as BruteForceSceneQuery;
            Expect(query != null,
                "candidate cap self-check must use the production BruteForceSceneQuery");
            bool hasCandidateCarrier = query.TryGetCollisionCandidateSequence(attacker, out List<SceneQueryHit> candidates);
            Expect(hasCandidateCarrier,
                "candidate cap self-check must consume the production collision carrier");
            Expect(attacker.Runtime.HitCandidateCount == 20 && candidates.Count == 20,
                "collision collection must cap an ordinary attacker carrier at 20 candidates");
            for (int i = 0; i < 20; i++)
            {
                Expect(candidates[i].Target == targets[i],
                    $"collision candidate {i} must preserve runtime-slot scan order");
            }
            Expect(!candidates.Exists(hit => hit.Target == targets[20]),
                "the 21st runtime-slot target must be excluded by the 20-candidate cap");

            LF2Character newborn = CreateInteractionCharacter(
                "SelfCheck_Step8Newborn",
                999,
                BuildInteractionVictimData("SelfCheckStep8Newborn", 999));
            world.Register(newborn);
            newborn.Team = 2;
            newborn.RelationTeam = 2;
            newborn.Runtime.SetPosition(0, 0, 0);
            newborn.Runtime.SyncIntegerPosition();

            Expect(query.TryGetCollisionCandidateSequence(attacker, out List<SceneQueryHit> afterSpawn) &&
                   afterSpawn.Count == 20 && !afterSpawn.Exists(hit => hit.Target == newborn),
                "a step8 newborn must not enter an existing step6 collision candidate carrier");
            Expect(query.TryGetCollisionCandidateSequence(newborn, out List<SceneQueryHit> newbornCarrier) &&
                   newbornCarrier.Count == 0 && newborn.Runtime.HitCandidateCount == 0,
                "a step8 newborn must not receive a retroactive candidate carrier in the same tick");

            world.EndCollisionCandidateConsumption();
        }

        private static void CheckSpecialAttackStep4AndLateFrameTick()
        {
            LF2FrameData frame0 = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            frame0.hit_a = 5;
            var data = new LF2CharacterData
            {
                name = "SelfCheck_SpecialStep4Late",
                frames = new List<LF2FrameData> { frame0 },
            };
            var world = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData("SelfCheck_SpecialStep4Late", 200, data);
            world.Register(special);
            special.Health.HP = 20;
            special.AttackingCounter = 0;
            special.FrameDelay = 0;
            special.Trans.SyncDirectFrameData(frame0.wait, frame0.next, 0);

            world.SerialTickAll(1);

            Expect(special.Frame.N == 0 && special.AttackingCounter == 0,
                "SpecialAttack step4 must run TU only and must not advance frame_tick wait/next state");
            Expect(special.Health.HP == 20,
                "SpecialAttack step4 TU must not apply the type3 frame_tick hit_a drain");

            world.LateEntityUpdateAll(1);

            Expect(special.Health.HP == 15 && special.AttackingCounter == 1,
                "SpecialAttack late update must advance frame_tick once and apply hit_a exactly once");

            world.LateEntityUpdateAll(2);

            Expect(special.Health.HP == 10 && special.AttackingCounter == 2,
                "each subsequent late pass must apply one, and only one, type3 hit_a drain");
        }

        private static void CheckFrameTickPpDisplayAndCurrentDatMatrix()
        {
            LF2FrameData ppSource = Frame(0, LF2States.Standing, 0, 1, 39, 79);
            LF2FrameData ppCost = Frame(1, LF2States.Standing, 10, 1, 39, 79);
            ppCost.mp = -30;
            ppCost.hit_d = 2;
            var ppData = new LF2CharacterData
            {
                name = "SelfCheck_PpDisplayFrameTick",
                frames = new List<LF2FrameData>
                {
                    ppSource,
                    ppCost,
                    Frame(2, LF2States.Standing, 1, 2, 39, 79),
                },
            };
            var ppWorld = new SimulationWorld();
            LF2Character ppCharacter = CreateCharacter("SelfCheck_PpDisplayFrameTick", 1, ppData);
            ppWorld.Register(ppCharacter);
            ppCharacter.Health.PP = 100;
            ppCharacter.PpDisplay = 0;
            ppCharacter.AttackingCounter = 0;
            ppCharacter.Trans.SyncDirectFrameData(ppSource.wait, ppSource.next, 0);

            ppCharacter.SimFrameTick(1);

            Expect(ppCharacter.Frame.N == 1 && ppCharacter.Health.PP == 70,
                "negative frame mp must consume PP after a real wait/next transition");
            Expect(ppCharacter.PpDisplay == 30,
                "PP consumption must increase PpDisplay with a positive cost sign");

            LF2FrameData type3Frame = Frame(0, LF2States.ProjectileFlying, 0, 999, 39, 79);
            type3Frame.hit_a = 4;
            LF2FrameData caughtFrame = Frame(10, LF2States.ProjectileFlying, 0, 999, 39, 79, new CatchPoint { kind = 2 });
            caughtFrame.hit_a = 4;
            var type3Data = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatType3Shell",
                frames = new List<LF2FrameData>
                {
                    type3Frame,
                    caughtFrame,
                    Frame(212, LF2States.Jump, 1, 212, 39, 79),
                },
            };
            var type3World = new SimulationWorld();
            var characterShell = new BoundsSelfCheckCharacter(LF2ObjectType.SpecialAttack);
            characterShell.BindData("SelfCheck_CurrentDatType3Shell", 200, type3Data);
            type3World.Register(characterShell);
            characterShell.Health.HP = 20;
            characterShell.FrameDelay = 2;
            characterShell.Runtime.SetPosition(0, -10, 0);
            characterShell.Runtime.SyncIntegerPosition();
            characterShell.Trans.SyncDirectFrameData(type3Frame.wait, type3Frame.next, 0);

            characterShell.SimFrameTick(1);

            Expect(characterShell.Health.HP == 16 && characterShell.FrameDelay == 2,
                "a character CLR shell with current type3 DAT must run frame_tick despite nonzero FrameDelay");
            Expect(characterShell.Frame.N == 0,
                "current type3 DAT next=999 must resolve to frame0 even when the CLR shell is a character and airborne");

            characterShell.Health.HP = 20;
            characterShell.Runtime.LinkState = -1;
            characterShell.SimFrameTick(2);
            Expect(characterShell.Health.HP == 20,
                "current type3 DAT frame_tick must honor the shared negative-link gate before hit_a");

            characterShell.Runtime.LinkState = 0;
            characterShell.SetCpointRawFramePreserveWait(10);
            characterShell.SimFrameTick(3);
            Expect(characterShell.Health.HP == 20 && characterShell.Frame.N == 10,
                "current type3 DAT frame_tick must honor the caught cpoint kind2 gate before hit_a/next");

            var characterData = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatCharacterShell",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, 999, 39, 79),
                    Frame(10, LF2States.Standing, 0, 999, 39, 79, new CatchPoint { kind = 2 }),
                    Frame(212, LF2States.Jump, 1, 212, 39, 79),
                },
            };
            var characterWorld = new SimulationWorld();
            LF2Entity specialShell = CreateCpointMatrixEntity(false, "SelfCheck_CurrentDatCharacterShell", 1, characterData);
            characterWorld.Register(specialShell);
            specialShell.Runtime.SetPosition(0, -10, 0);
            specialShell.Runtime.SyncIntegerPosition();
            specialShell.FrameDelay = 1;
            specialShell.Trans.SyncDirectFrameData(0, 999, 0);

            specialShell.SimFrameTick(1);
            Expect(specialShell.Frame.N == 0,
                "a SpecialAttack CLR shell with current character DAT must honor the character FrameDelay gate");

            specialShell.SimTU(1);
            Expect(specialShell.FrameDelay == 0 && specialShell.Frame.N == 0,
                "current character DAT step4 must decay FrameDelay before returning without dynamics");
            specialShell.Runtime.LinkState = -1;
            double heldY = specialShell.Runtime.Y;
            specialShell.SimTU(2);
            Expect(Nearly(specialShell.Runtime.Y, heldY),
                "current character DAT step4 must honor the negative-link dynamics gate");

            specialShell.Runtime.LinkState = 0;
            specialShell.SimFrameTick(2);
            Expect(specialShell.Frame.N == 212,
                "current character DAT next=999 must resolve airborne shells to frame212");

            LF2FrameData heavyFrame = Frame(0, LF2States.HeavyWeaponInSky, 10, 0, 39, 79);
            var heavyData = new LF2CharacterData
            {
                name = "SelfCheck_CurrentDatHeavyShell",
                frames = new List<LF2FrameData>
                {
                    heavyFrame,
                    Frame(20, LF2States.HeavyWeaponOnGround, 1, 20, 39, 79),
                },
            };
            var heavyWorld = new SimulationWorld();
            var heavyShell = new BoundsSelfCheckCharacter(LF2ObjectType.HeavyWeapon);
            heavyShell.BindData("SelfCheck_CurrentDatHeavyShell", 150, heavyData);
            heavyWorld.Register(heavyShell);
            heavyShell.Runtime.SetPosition(0, 0, 0);
            heavyShell.Runtime.SetVelocity(0, 0, 0);
            heavyShell.Runtime.SyncIntegerPosition();
            heavyShell.Trans.SyncDirectFrameData(heavyFrame.wait, heavyFrame.next, 0);

            heavyShell.SimFrameTick(1);

            Expect(heavyShell.Frame.N == 20,
                "a character CLR shell with current type2 DAT must enter frame20 when grounded state2000 is stationary");

            var lyingData = new LF2CharacterData
            {
                name = "SelfCheck_State14FrameTick",
                frames = new List<LF2FrameData> { Frame(0, LF2States.Lying, 10, 0, 39, 79) },
            };
            var lyingWorld = new SimulationWorld();
            LF2Character lying = CreateCharacter("SelfCheck_State14FrameTick", 1, lyingData);
            lyingWorld.Register(lying);
            lying.Health.HP = 0;
            lying.KillCount = -1;
            lying.RelationTeam = 1;
            lying.HitStun = 0;

            lying.SimFrameTick(1);
            Expect(lying.HitStun == 0 && lying.AttackingCounter == 0,
                "state14 HP<=0 must not arm hit stop for an ordinary unowned low runtime slot");

            lying.KillCount = 0;
            lying.SimFrameTick(2);
            Expect(lying.HitStun == 30 && lying.AttackingCounter == 0,
                "state14 HP<=0 must arm mapped hit stop for an owned entity and keep attacking cleared");
        }

        private static void CheckStateTransformLandingMatrix()
        {
            const int lightOid = 741;
            const int heavyOid = 742;
            const int throwOid = 743;
            const int lightSkyOid = 745;
            const int drinkOid = 101;
            const int otherOid = 999;

            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [lightOid] = new LF2CharacterDataWrapper(lightOid,
                    BuildTransformedLandingData("SelfCheck_TransformLight", 31, 3, LF2States.WeaponThrowing, 70)),
                [heavyOid] = new LF2CharacterDataWrapper(heavyOid,
                    BuildTransformedLandingData("SelfCheck_TransformHeavy", 32, 4, LF2States.HeavyWeaponInSky, 20)),
                [throwOid] = new LF2CharacterDataWrapper(throwOid,
                    BuildTransformedLandingData("SelfCheck_TransformThrow", 33, 5, LF2States.WeaponInSky, 0)),
                [lightSkyOid] = new LF2CharacterDataWrapper(lightSkyOid,
                    BuildTransformedLandingData("SelfCheck_TransformLightSky", 35, 2, LF2States.WeaponInSky, 60)),
                [drinkOid] = new LF2CharacterDataWrapper(drinkOid,
                    BuildTransformedLandingData("SelfCheck_TransformDrink", 34, 6, LF2States.WeaponThrowing, 70)),
                [otherOid] = new LF2CharacterDataWrapper(otherOid,
                    BuildTransformedLandingData("SelfCheck_TransformOther999", 0, 0, 9999, 101)),
            };
            var types = new Dictionary<int, int>
            {
                [lightOid] = (int)LF2ObjectType.LightWeapon,
                [heavyOid] = (int)LF2ObjectType.HeavyWeapon,
                [throwOid] = (int)LF2ObjectType.ThrowWeapon,
                [lightSkyOid] = (int)LF2ObjectType.LightWeapon,
                [drinkOid] = (int)LF2ObjectType.Drink,
                [otherOid] = (int)LF2ObjectType.Other,
            };
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;

            using (new TemporaryRuntimeObjectConfigs(types, wrappers))
            {
                try
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                        wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                    TransformedLandingSelfCheckEntity light = CreateTransformedLandingShell(lightOid, false);
                    light.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(light, 5.0, 8.0, 1);
                    Expect(light.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.LightWeapon &&
                           light.Frame.N == 70 && light.Runtime.WeaponFlightCounter == 17,
                        "state4000 transform must dispatch type1 landing to frame70 and subtract weapon_drop_hurt durability");
                    Expect(Nearly(light.Runtime.Vx, 4.0) && Nearly(light.Runtime.Vy, 0.0) && light.WeaponCount == 0,
                        "type1 transformed landing must halve vx, stop vy, and clear WeaponCount outside state12");

                    TransformedLandingSelfCheckEntity lightStop = CreateTransformedLandingShell(lightSkyOid, false);
                    lightStop.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(lightStop, 5.0, 8.0, 11);
                    Expect(lightStop.Frame.N == 60 && lightStop.Runtime.WeaponFlightCounter == 18,
                        "transformed type1 non-throwing low-speed landing must enter frame60 and consume durability");

                    TransformedLandingSelfCheckEntity lightBounce = CreateTransformedLandingShell(lightOid, false);
                    lightBounce.Runtime.WeaponFlightCounter = 20;
                    lightBounce.SwitchDir("right");
                    RunTransformedLandingPasses(lightBounce, 10.0, 8.0, 12);
                    Expect(lightBounce.Frame.N == 7 && Nearly(lightBounce.Runtime.Vy, -8.0) &&
                           Nearly(lightBounce.Runtime.Vx, 4.0) && lightBounce.Runtime.Dir == "left" &&
                           lightBounce.Runtime.WeaponFlightCounter == 17,
                        "transformed type1 throwing high-speed landing must enter frame7, bounce -8, flip, and consume durability");

                    TransformedLandingSelfCheckEntity heavy = CreateTransformedLandingShell(heavyOid, false);
                    heavy.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(heavy, 5.0, 8.0, 2);
                    Expect(heavy.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.HeavyWeapon &&
                           heavy.Frame.N == 20 && heavy.Runtime.WeaponFlightCounter == 15,
                        "state4000 transform must dispatch low-speed type2 landing to frame20 and consume 1+drop durability");
                    Expect(Nearly(heavy.Runtime.Vx, 4.0) && Nearly(heavy.Runtime.Vy, 0.0) && heavy.WeaponCount == 0,
                        "type2 transformed landing must stop on frame20 and clear WeaponCount outside state12");

                    TransformedLandingSelfCheckEntity heavyBounce = CreateTransformedLandingShell(heavyOid, false);
                    heavyBounce.Runtime.WeaponFlightCounter = 20;
                    heavyBounce.SwitchDir("right");
                    RunTransformedLandingPasses(heavyBounce, 10.0, 8.0, 13);
                    Expect(heavyBounce.Frame.N == 0 && Nearly(heavyBounce.Runtime.Vy, -5.0) &&
                           Nearly(heavyBounce.Runtime.Vx, 4.0) && heavyBounce.Runtime.Dir == "right" &&
                           heavyBounce.Runtime.WeaponFlightCounter == 19,
                        $"transformed type2 high-speed landing must preserve frame0, bounce -5, consume one durability, " +
                        $"then let late state2000 face final vx; frame={heavyBounce.Frame.N}, vx={heavyBounce.Runtime.Vx}, " +
                        $"vy={heavyBounce.Runtime.Vy}, y={heavyBounce.Runtime.Y}, dir={heavyBounce.Runtime.Dir}, " +
                        $"durability={heavyBounce.Runtime.WeaponFlightCounter}, weaponCount={heavyBounce.WeaponCount}, " +
                        "inputLandingVy=10, inputVx=8");

                    TransformedLandingSelfCheckEntity thrown = CreateTransformedLandingShell(throwOid, false);
                    thrown.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(thrown, 10.0, 12.0, 3);
                    Expect(thrown.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon &&
                           thrown.Frame.N == 0 && thrown.Runtime.WeaponFlightCounter == 15,
                        "state4000 transform must dispatch high-speed type4 landing to frame0 and subtract drop durability");
                    Expect(Nearly(thrown.Runtime.Vx, 8.4) && Nearly(thrown.Runtime.Vy, -7.0) && thrown.WeaponCount == 0,
                        "type4 transformed landing must apply the release 0.7 bounce and clear WeaponCount");

                    TransformedLandingSelfCheckEntity thrownStop = CreateTransformedLandingShell(throwOid, false);
                    thrownStop.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(thrownStop, 5.0, 8.0, 14);
                    Expect(thrownStop.Frame.N == 60 && Nearly(thrownStop.Runtime.Vx, 5.6) &&
                           Nearly(thrownStop.Runtime.Vy, 0.0) && thrownStop.Runtime.WeaponFlightCounter == 15,
                        "transformed type4 low-speed landing must stop on frame60 with 0.7 vx and consume durability");

                    TransformedLandingSelfCheckEntity drinkBounce = CreateTransformedLandingShell(drinkOid, false);
                    drinkBounce.Runtime.WeaponFlightCounter = 20;
                    RunTransformedLandingPasses(drinkBounce, 10.0, 12.0, 15);
                    Expect(drinkBounce.Frame.N == 0 && Nearly(drinkBounce.Runtime.Vx, 8.4) &&
                           Nearly(drinkBounce.Runtime.Vy, -7.0) && drinkBounce.Runtime.WeaponFlightCounter == 14,
                        "oid101 transformed type6 high-speed landing must take the common 0.7 bounce branch");

                    TransformedLandingSelfCheckEntity drink = CreateTransformedLandingShell(drinkOid, true);
                    drink.Runtime.WeaponFlightCounter = 20;
                    drink.Health.HP = 0;
                    RunTransformedLandingPasses(drink, 5.0, 8.0, 4);
                    Expect(drink.HitStun == 139,
                        $"state8000 transform hit-stop must count down once in the following production late frame_tick; " +
                        $"actual={drink.HitStun}, immediate=140");
                    Expect(drink.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Drink &&
                           drink.Frame.N == 70 && drink.Runtime.WeaponFlightCounter == -1,
                        "state8000 transform must dispatch type6 landing and mark depleted drink durability -1");
                    Expect(Nearly(drink.Runtime.Vx, 5.6) && Nearly(drink.Runtime.Vy, 0.0) && drink.WeaponCount == 0,
                        "type6 transformed landing must use the 0.7 stop branch and clear WeaponCount");

                    TransformedLandingSelfCheckEntity other999 = CreateTransformedLandingShell(otherOid, false);
                    RunTransformedLandingPasses(other999, 5.0, 8.0, 16);
                    Expect(other999.Frame.N == 101 && Nearly(other999.Runtime.Vx, 0.0) &&
                           Nearly(other999.Runtime.Vy, 0.0),
                        "state4999 transform must dispatch oid999 default landing to frame101 and stop all planar motion");

                    CheckTransformedPendingDestroyCrossSimOrder(lightOid);
                }
                finally
                {
                    LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
                }
            }
        }

        private static LF2CharacterData BuildTransformedLandingData(
            string name,
            int weaponHp,
            int dropHurt,
            int sourceState,
            int landingFrame)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, sourceState, 100, 0, 39, 79),
            };
            int[] landingFrames = { 7, 20, 60, 70, 101 };
            for (int i = 0; i < landingFrames.Length; i++)
            {
                int frameId = landingFrames[i];
                int landingState = frameId == 20
                    ? LF2States.HeavyWeaponOnGround
                    : frameId == 7 ? LF2States.WeaponInSky
                    : frameId == 60 ? LF2States.WeaponOnGround
                    : frameId == 101 ? 9999
                    : LF2States.WeaponJustOnGround;
                frames.Add(Frame(frameId, landingState, 100, frameId, 39, 79));
            }

            return new LF2CharacterData
            {
                name = name,
                weapon_hp = weaponHp,
                weapon_drop_hurt = dropHurt,
                frames = frames,
            };
        }

        private static TransformedLandingSelfCheckEntity CreateTransformedLandingShell(int targetOid, bool hitStopTransform)
        {
            int transformState = (hitStopTransform ? 8000 : 4000) + targetOid;
            var shell = new TransformedLandingSelfCheckEntity();
            shell.BindSource(new LF2CharacterData
            {
                name = $"SelfCheck_TransformSource_{targetOid}",
                frames = new List<LF2FrameData>
                {
                    Frame(0, transformState, 100, 0, 39, 79),
                },
            });
            shell.RunStateSpecialPreCollision();

            Expect(shell.ObjectId == targetOid && shell.FrameCache.Wrapper?.characterId == targetOid &&
                   shell.Frame.N == 0 && shell.Frame.D != null,
                $"state transform must load oid {targetOid} wrapper and enter its frame0");
            Expect(shell.WeaponCount == 0,
                $"state transform must preserve the source WeaponCount for oid {targetOid}");
            Expect(shell.Runtime.WeaponFlightCounter == shell.FrameCache.Wrapper.characterData.weapon_hp,
                $"state transform must initialize oid {targetOid} durability from target weapon_hp");
            Expect(shell.HitStun == (hitStopTransform ? 140 : 0),
                $"state transform immediate hit-stop mismatch for oid {targetOid}; " +
                $"actual={shell.HitStun}, expected={(hitStopTransform ? 140 : 0)}");
            return shell;
        }

        private static void RunTransformedLandingPasses(
            TransformedLandingSelfCheckEntity shell,
            double landingVy,
            double vx,
            int tickIndex)
        {
            var world = new SimulationWorld();
            world.Register(shell);
            shell.Runtime.SetPosition(0, -1, 0);
            shell.Runtime.SetVelocity(vx, landingVy, 0);
            shell.Runtime.SyncIntegerPosition();

            world.SerialTickAll(tickIndex);
            int landingFrame = shell.Frame.N;
            world.LateEntityUpdateAll(tickIndex);

            Expect(shell.Frame.N == landingFrame && shell.AttackingCounter == 1,
                "transformed landing must remain visible through the production late frame_tick pass");
        }

        private static void CheckTransformedPendingDestroyCrossSimOrder(int targetOid)
        {
            var world = new SimulationWorld();
            var shell = new TransformingSimOrderSelfCheckEntity(targetOid);
            shell.BindSource(new LF2CharacterData
            {
                name = "SelfCheck_TransformPendingDestroy",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 4000 + targetOid, 100, 0, 39, 79),
                },
            });
            int originalOrder = shell.SimOrder;
            world.Register(shell);
            int slot = shell.Runtime.SlotIndex;

            shell.RunStateSpecialPreCollision();
            Expect(shell.SimOrder != originalOrder,
                "cross-SimOrder destroy fixture must change its exposed order after the current-DAT transform");
            shell.Runtime.PendingFlushDestroy = true;

            world.LateEntityUpdateAll(20);

            Expect(shell.TransitDestroyCount == 1,
                "transformed PendingFlushDestroy must finalize through OnTransitDestroy exactly once");
            Expect(world.FindEntityByRuntimeSlotIncludingPending(slot) == null && world.ObjectCount == 0,
                "transformed PendingFlushDestroy must release its runtime slot and remove every registry reference");
            Expect(!WorldContainsSimulationBucket(world, originalOrder),
                "transformed PendingFlushDestroy must remove the entity from its original registration bucket");

            world.LateEntityUpdateAll(21);
            Expect(shell.TransitDestroyCount == 1,
                "a later late pass must not finalize the transformed entity twice");
        }

        private static bool WorldContainsSimulationBucket(SimulationWorld world, int simOrder)
        {
            var field = typeof(SimulationWorld).GetField(
                "_buckets",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            object buckets = field?.GetValue(world);
            var containsKey = buckets?.GetType().GetMethod("ContainsKey");
            return containsKey != null && (bool)containsKey.Invoke(buckets, new object[] { simOrder });
        }

        private static void CheckSerialTickInterleaveAndFrameEdgeMatrix()
        {
            var serialEvents = new List<string>();
            var serialWorld = new SimulationWorld();
            var low = new SerialOrderSelfCheckEntity("low", serialEvents);
            var high = new SerialOrderSelfCheckEntity("high", serialEvents);
            serialWorld.Register(low);
            serialWorld.Register(high);

            serialWorld.SerialTickAll(1);

            Expect(serialEvents.Count == 4 &&
                   serialEvents[0] == "low:transit" &&
                   serialEvents[1] == "low:tu" &&
                   serialEvents[2] == "high:transit" &&
                   serialEvents[3] == "high:tu",
                "SerialTickAll must interleave Transit/TU per runtime slot instead of running two global loops");

            LF2FrameData singlePhysicsFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            singlePhysicsFrame.dvx = 3;
            var specialData = new LF2CharacterData
            {
                name = "SelfCheck_SpecialSinglePhysics",
                frames = new List<LF2FrameData> { singlePhysicsFrame },
            };
            var specialWorld = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData("SelfCheck_SpecialSinglePhysics", 200, specialData);
            specialWorld.Register(special);
            special.Runtime.SetPosition(0, -10, 0);
            special.Runtime.SetVelocity(0, 0, 0);
            special.Runtime.SyncIntegerPosition();

            specialWorld.SerialTickAll(1);

            Expect(Nearly(special.Runtime.X, 3.0) && Nearly(special.Runtime.Vx, 3.0),
                "SpecialAttack step4 must apply authored non-character velocity and horizontal physics exactly once");

            LF2FrameData weaponSpecialFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            weaponSpecialFrame.dvx = 2;
            var weaponSpecialData = new LF2CharacterData
            {
                name = "SelfCheck_WeaponShellSpecialDat",
                frames = new List<LF2FrameData> { weaponSpecialFrame },
            };
            var weaponSpecialWorld = new SimulationWorld();
            var weaponSpecialShell = new CurrentDatSelfCheckWeapon(LF2ObjectType.SpecialAttack);
            weaponSpecialShell.BindData("SelfCheck_WeaponShellSpecialDat", 200, 1, weaponSpecialData, 0);
            weaponSpecialWorld.Register(weaponSpecialShell);
            weaponSpecialShell.Runtime.SetPosition(0, -10, 0);
            weaponSpecialShell.Runtime.SetVelocity(0, 0, 0);
            weaponSpecialShell.Runtime.SyncIntegerPosition();

            weaponSpecialShell.SimTU(1);

            Expect(Nearly(weaponSpecialShell.Runtime.X, 2.0) && Nearly(weaponSpecialShell.Runtime.Vy, 0.0),
                "weapon CLR shell with current type3 DAT must use shared type3 physics with zero gravity");

            LF2FrameData weaponOtherFrame = Frame(0, LF2States.Standing, 10, 0, 0, 0);
            weaponOtherFrame.dvx = 2;
            var weaponOtherData = new LF2CharacterData
            {
                name = "SelfCheck_WeaponShellOtherDat",
                frames = new List<LF2FrameData> { weaponOtherFrame },
            };
            var weaponOtherWorld = new SimulationWorld();
            var weaponOtherShell = new CurrentDatSelfCheckWeapon(LF2ObjectType.Other);
            weaponOtherShell.BindData("SelfCheck_WeaponShellOtherDat", 300, 1, weaponOtherData, 0);
            weaponOtherWorld.Register(weaponOtherShell);
            weaponOtherShell.Runtime.SetPosition(0, -10, 0);
            weaponOtherShell.Runtime.SetVelocity(0, 0, 0);
            weaponOtherShell.Runtime.SyncIntegerPosition();

            weaponOtherShell.SimTU(1);

            Expect(Nearly(weaponOtherShell.Runtime.X, 2.0) &&
                   Nearly(weaponOtherShell.Runtime.Vy, NTSDGlobal.Gameplay.WeaponGravityDefault),
                "weapon CLR shell with current other DAT must use shared ordinary non-character gravity");

            LF2CharacterData negativeNextData = new LF2CharacterData
            {
                name = "SelfCheck_NegativeNext",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 0, -1, 39, 79),
                    Frame(1, LF2States.Standing, 10, 1, 39, 79),
                },
            };
            var negativeWeapon = new CurrentDatSelfCheckWeapon(LF2ObjectType.LightWeapon);
            negativeWeapon.BindData("SelfCheck_NegativeNextWeapon", 100, 1, negativeNextData, 0);
            negativeWeapon.SwitchDir("right");
            negativeWeapon.Trans.SyncDirectFrameData(0, -1, 0);
            negativeWeapon.SimFrameTick(1);
            Expect(negativeWeapon.Frame.N == 1 && negativeWeapon.Runtime.Dir == "left",
                "weapon negative next must enter the absolute frame and flip facing exactly once");

            var negativeSpecial = new AlternateDamageSelfCheckSpecialAttack();
            negativeSpecial.BindData("SelfCheck_NegativeNextSpecial", 200, negativeNextData);
            negativeSpecial.SwitchDir("right");
            negativeSpecial.Trans.SyncDirectFrameData(0, -1, 0);
            negativeSpecial.SimFrameTick(1);
            Expect(negativeSpecial.Frame.N == 1 && negativeSpecial.Runtime.Dir == "left",
                "SpecialAttack negative next must enter the absolute frame and flip facing exactly once");

            var caughtExitData = new LF2CharacterData
            {
                name = "SelfCheck_CaughtExitFrozen",
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Lying, 0, 1, 39, 79),
                    Frame(1, LF2States.Frozen, 10, 1, 39, 79),
                    Frame(2, LF2States.Standing, 10, 2, 39, 79),
                },
            };
            LF2Character frozenExit = CreateCharacter("SelfCheck_CaughtExitFrozen", 1, caughtExitData);
            frozenExit.HitStun = 0;
            frozenExit.Trans.SyncDirectFrameData(0, 1, 0);
            frozenExit.SimFrameTick(1);
            Expect(frozenExit.Frame.N == 1 && frozenExit.HitStun == 0,
                "leaving state14 into Frozen must not arm the generic caught-exit hit stop");

            caughtExitData.frames[0].next = 2;
            LF2Character ordinaryExit = CreateCharacter("SelfCheck_CaughtExitOrdinary", 1, caughtExitData);
            ordinaryExit.HitStun = 0;
            ordinaryExit.Trans.SyncDirectFrameData(0, 2, 0);
            ordinaryExit.SimFrameTick(1);
            Expect(ordinaryExit.Frame.N == 2 && ordinaryExit.HitStun == 15,
                "leaving state14 into an ordinary state must arm the mapped caught-exit hit stop");
        }

        private static bool IsRecycledAndCleared(OPointCreateTask task)
        {
            return task != null &&
                   !task.IsFromPool &&
                   task.opoint.oid == 0 &&
                   task.parent == null &&
                   task.releaseSpawnSemantic == ReleaseSpawnSemantic.None;
        }

        private static int GetQueuedObjectPointTaskCount(LF2ObjectPointFactory factory)
        {
            var queueField = typeof(LF2ObjectPointFactory).GetField(
                "_taskQueue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            object queue = queueField?.GetValue(factory);
            var countProperty = queue?.GetType().GetProperty("Count");
            return countProperty == null ? -1 : (int)countProperty.GetValue(queue);
        }

        private static LF2CharacterData BuildAlternateDamageWeaponFrames()
        {
            var frames = new List<LF2FrameData>();
            for (int frameId = 0; frameId < 16; frameId++)
                frames.Add(Frame(frameId, LF2States.Standing, 1, frameId, 39, 79));
            frames.Add(Frame(20, LF2States.WeaponThrowing, 1, 20, 39, 79));

            return new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageWeapon",
                type_sub = 1,
                frames = frames,
            };
        }

        private static void PrepareAlternateEntry(LF2Entity attacker, LF2Entity victim)
        {
            attacker.SwitchDir("right");
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.LinkState = 0;
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            SetAlternateDamagePosition(attacker, 0.0, 0.0);

            victim.SwitchDir("right");
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FallCounter = 0;
            victim.KillCount = 0;
            victim.FrameDelay = 0;
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            SetAlternateDamagePosition(victim, 10.0, 0.0);
        }

        private static InteractionArea MakeInteractionItr(int kind, int vrest, int injury, int dvx)
        {
            return new InteractionArea
            {
                kind = kind,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 20,
                injury = injury,
                dvx = dvx,
                bdefend = 0,
                arest = 4,
                vrest = vrest,
                effect = 0,
            };
        }

        private static LF2FrameData InteractionFrame(InteractionArea itr)
        {
            LF2FrameData frame = Frame(0, LF2States.Standing, 1, 0, 0, 0);
            frame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
            });
            if (itr != null)
                frame.itrs.Add(itr);
            return frame;
        }

        private static LF2CharacterData BuildInteractionVictimData(string name, int objectId)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = objectId,
                frames = new List<LF2FrameData> { InteractionFrame(null) },
            };
        }

        private static AlternateDamageSelfCheckWeapon CreateSelfCheckWeapon(
            string name,
            int objectId,
            int weaponType,
            LF2CharacterData data,
            int frameId)
        {
            var weapon = new AlternateDamageSelfCheckWeapon();
            weapon.BindData(name, objectId, weaponType, data, frameId);
            return weapon;
        }

        private static LF2Character CreateInteractionCharacter(string name, int objectId, LF2CharacterData data)
        {
            var character = new InteractionSelfCheckCharacter();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new SelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRuntimeSlotIndex(character.StableId);
            return character;
        }

        private static void RegisterInteractionPair(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Character victim)
        {
            world.Register(attacker);
            world.Register(victim);

            attacker.Team = 1;
            attacker.RelationTeam = 1;
            attacker.Health.HP = 100;
            attacker.Health.HPBound = 100;
            attacker.FrameDelay = 0;
            attacker.AttackExempt = 0;
            attacker.Runtime.LinkState = 0;
            attacker.ItrRest.Reset();
            attacker.Runtime.SetPosition(0.0, 0.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.SyncIntegerPosition();

            victim.Team = 2;
            victim.RelationTeam = 2;
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.FallCounter = 0;
            victim.KillCount = 0;
            victim.FrameDelay = 0;
            victim.ItrRest.Reset();
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            victim.Runtime.SetPosition(0.0, 0.0, 0.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
        }

        private static void RunSpecialAttackPreprocessCase(
            int kind,
            int rawVrest,
            Action<AlternateDamageSelfCheckSpecialAttack> arrange,
            Action<AlternateDamageSelfCheckSpecialAttack, LF2Character, InteractionArea> verify)
        {
            InteractionArea sourceItr = MakeInteractionItr(kind, rawVrest, 100, 6);
            var specialData = new LF2CharacterData
            {
                name = $"SelfCheckSpecialPreprocess{kind}",
                type_sub = 1,
                frames = new List<LF2FrameData> { InteractionFrame(sourceItr) },
            };
            var world = new SimulationWorld();
            var special = new AlternateDamageSelfCheckSpecialAttack();
            special.BindData($"SelfCheck_SpecialPreprocess{kind}", 1, specialData);
            LF2Character victim = CreateInteractionCharacter(
                $"SelfCheck_SpecialPreprocessVictim{kind}",
                37,
                BuildInteractionVictimData($"SelfCheckSpecialPreprocessVictim{kind}", 37));
            RegisterInteractionPair(world, special, victim);
            arrange(special);

            special.SimTU(1);
            Expect(victim.Health.HP == 100,
                "special-attack TU must not consume interaction candidates");

            special.FrameDelay = 0;
            special.AttackExempt = 0;
            special.Runtime.SetPosition(0.0, 0.0, 0.0);
            special.Runtime.SetVelocity(0.0, 0.0, 0.0);
            special.Runtime.SyncIntegerPosition();
            arrange(special);
            victim.Runtime.SetPosition(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.ObjectInteractionTickAll(2);
            world.EndCollisionCandidateConsumption();

            Expect(victim.Health.HP == 90,
                "special-attack object-interaction pass must resolve alternate damage");
            verify(special, victim, sourceItr);
        }

        private static void RunAlternateDamageMotionCase(
            LF2CharacterData frameData,
            Action<LF2Character, LF2Character, InteractionArea> arrange,
            Action<LF2Character, LF2Character, InteractionArea> verify)
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter("SelfCheck_AlternateMotionAttacker", 1, frameData);
            LF2Character victim = CreateCharacter("SelfCheck_AlternateMotionVictim", 2, frameData);
            world.Register(attacker);
            world.Register(victim);

            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            attacker.SwitchDir("right");
            victim.SwitchDir("right");
            SetAlternateDamagePosition(attacker, 0.0, 0.0);
            SetAlternateDamagePosition(victim, 10.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.LinkState = 0;
            attacker.Runtime.HolderStableId = -1;
            attacker.HolderCopySlot = -1;
            attacker.AttackExempt = 0;
            attacker.FrameDelay = 0;
            victim.Health.HP = 500;
            victim.Health.HPBound = 500;
            victim.Health.HPLost = 0;
            victim.FallDamageDiv = 0;
            victim.KillCount = 0;
            victim.Unk344 = 0;
            victim.ComboCountVic = 0;
            victim.FallCounter = 0;
            victim.AttackingCounter = 0;
            victim.HitStateCount = 0;
            victim.HitCount = 0;
            victim.KnockbackVx = 0.0;
            victim.KnockbackVy = 0.0;
            victim.KnockbackVz = 0.0;
            victim.FrameDelay = 0;
            victim.Runtime.PrevFrame2 = 0;

            var itr = new InteractionArea
            {
                kind = 0,
                injury = 0,
                bdefend = 0,
                dvx = 0,
                arest = 0,
                vrest = 0,
                effect = 0,
            };

            arrange(attacker, victim, itr);
            LF2AlternateDamageResolver.ApplyAlternateDamage(attacker, victim, victim.HitCounters, itr);
            verify(attacker, victim, itr);
        }

        private static void SetAlternateDamagePosition(LF2Entity entity, double x, double y)
        {
            entity.Runtime.SetPosition(x, y, 0.0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static LF2CharacterData BuildAlternateDamageMotionFrames()
        {
            var frames = new List<LF2FrameData>();
            for (int frameId = 0; frameId < 16; frameId++)
                frames.Add(Frame(frameId, LF2States.Standing, 1, frameId, 39, 79));

            frames.Add(Frame(20, LF2States.WeaponThrowing, 2, 20, 39, 79));
            frames.Add(Frame(21, LF2States.HeavyWeaponInSky, 2, 21, 39, 79));
            frames.Add(Frame(22, LF2States.ProjectileFlying, 2, 22, 39, 79));
            frames.Add(Frame(110, LF2States.Defending, 5, 111, 39, 79));
            frames.Add(Frame(111, LF2States.Defending, 6, 111, 39, 79));

            return new LF2CharacterData
            {
                name = "SelfCheckAlternateDamageMotion",
                type_sub = 1,
                frames = frames,
            };
        }

        private static void CheckOid5152MergeSuccessAndDormantIsolation()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid7", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(0);
                partner.SetRuntimeSlotIndex(11);
                world.Register(self);
                world.Register(partner);

                self.ImmediateFrame(10);
                partner.ImmediateFrame(10);
                self.Team = 1;
                partner.Team = 2;
                self.RelationTeam = 5;
                partner.RelationTeam = 5;
                self.Health.HP = 130;
                self.Health.HPBound = 150;
                self.Health.HP3 = 200;
                partner.Health.HP = 120;
                partner.Health.HPBound = 80;
                self.Health.PP = 10;
                partner.Health.PP = 20;
                self.Runtime.SetPosition(100f, 0f, 5f);
                partner.Runtime.SetPosition(121f, 0f, 12f);
                self.Runtime.SyncIntegerPosition();
                partner.Runtime.SyncIntegerPosition();
                self.Runtime.Vy = 7f;
                partner.Runtime.Vy = 3f;

                world.Oid5152RuntimeMaintenanceAll(1);

                Expect(self.ObjectId == 51 && self.CurrentFrameId == 290,
                    "oid 7/8 merge must convert self into oid 51 frame 290");
                Expect(self.Health.HPBound == 200 && self.Health.HP == 200,
                    "oid 7/8 merge must clamp aggregate HP/HPBound by self HP3");
                Expect(self.Health.PP == 500,
                    "oid 7/8 merge must set self PP to 500");
                Expect(self.GetRuntimeXInt() == 110 && self.GetRenderZInt() == 8,
                    "oid 7/8 merge must write integer midpoint X/Z");
                Expect(Nearly(self.Runtime.Vy, 7f) && Nearly(partner.Runtime.Vy, 0f),
                    "oid 7/8 merge must preserve self Vy and zero partner Vy");
                Expect(self.Runtime.Unk328 == 1 &&
                       self.Runtime.Unk32C == 11 &&
                       self.Runtime.Unk330 == 7 &&
                       self.Runtime.Unk334 == 8 &&
                       self.Runtime.Unk338 == 4500,
                    "oid 7/8 merge must write merge bookkeeping fields");
                Expect(partner.Runtime.OidMergeDormant,
                    "merged partner must become dormant instead of being unregistered");
                Expect(partner.Runtime.SlotIndex == 11 && partner.ObjectId == 8,
                    "merged partner must retain original slot and DAT identity");
                Expect(world.ObjectCount == 1,
                    "dormant merged partner must be excluded from ObjectCount");
                Expect(world.FindEntityByRuntimeSlotForQuery(11) == null,
                    "ordinary runtime-slot query must hide dormant merged partner");
                Expect(world.FindEntityByRuntimeSlotIncludingPending(11) == partner,
                    "including-pending runtime-slot query must still find dormant merged partner");

                var entities = new List<LF2Entity>();
                world.GetAllEntities(entities);
                Expect(entities.Count == 1 && entities[0] == self,
                    "ordinary entity enumeration must exclude dormant merged partner");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152MergeCooldownOneTriggersSameTick()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid7_Cooldown", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8_Cooldown", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(1);
                partner.SetRuntimeSlotIndex(12);
                world.Register(self);
                world.Register(partner);

                self.ImmediateFrame(10);
                partner.ImmediateFrame(10);
                self.RelationTeam = 1;
                partner.RelationTeam = 1;
                self.Health.HP = 100;
                self.Health.HPBound = 100;
                self.Health.HP3 = 200;
                partner.Health.HP = 90;
                partner.Health.HPBound = 90;
                self.Runtime.Unk338 = 1;
                self.Runtime.SetPosition(50f, 0f, 5f);
                partner.Runtime.SetPosition(80f, 0f, 8f);
                self.Runtime.SyncIntegerPosition();
                partner.Runtime.SyncIntegerPosition();

                world.Oid5152RuntimeMaintenanceAll(1);

                Expect(self.ObjectId == 51 && self.Runtime.Unk338 == 4500,
                    "merge cooldown 1 must decrement to 0 and still allow same-tick merge");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152SplitSuccessAndOddTruncate()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                SimulationWorld world = CreateOid5152MergedWorld(wrappers, out LF2Character self, out LF2Character partner);
                self.Health.HP = 201;
                self.Health.HPBound = 199;
                self.Runtime.Unk338 = 1;
                self.Runtime.Vy = 9f;

                world.Oid5152RuntimeMaintenanceAll(2);

                Expect(self.ObjectId == 7 && self.CurrentFrameId == 112,
                    "oid 51 split must restore self identity and enter frame 112");
                Expect(partner.ObjectId == 8 && partner.CurrentFrameId == 112,
                    "oid 51 split must revive dormant partner into frame 112");
                Expect(self.Health.HP == 100 && self.Health.HPBound == 99 &&
                       partner.Health.HP == 100 && partner.Health.HPBound == 99,
                    "oid 51 split must floor-divide odd HP and HPBound for both sides");
                Expect(self.Health.HP3 == 200 && partner.Health.HP3 == 500,
                    "oid 51 split must preserve self HP3 and keep partner Reset default HP3");
                Expect(self.Health.PP == 0 && partner.Health.PP == 0,
                    "oid 51 split must zero PP for both sides");
                Expect(self.Runtime.Unk328 == -1 && self.Runtime.Unk338 == 900,
                    "oid 51 split must clear merge flag and write 900 cooldown on self");
                Expect(!partner.Runtime.OidMergeDormant && world.ObjectCount == 2,
                    "split success must reactivate dormant partner and restore ObjectCount");
                Expect(partner.Team == 0 && partner.OwnerId == -1 && partner.Runtime.Unk328 == -1,
                    "split success partner must come from Reset defaults before contract overwrites");
                Expect(Nearly(self.Runtime.Vy, 9f) && Nearly(partner.Runtime.Vy, 0f) && Nearly(partner.Runtime.Vz, 0f),
                    "split success must preserve self Vy/Vz and keep partner Reset default vertical velocity");
                Expect(self.Runtime.Dir != partner.Runtime.Dir,
                    "split success must face revived partner opposite to self");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152SplitFailurePartialRecovery()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                SimulationWorld world = CreateOid5152MergedWorld(wrappers, out LF2Character self, out LF2Character partner);
                self.Health.PP = 123;
                self.Health.HP = 180;
                self.Health.HPBound = 180;
                self.Runtime.Unk32C = 399;
                self.Runtime.Unk338 = 0;

                world.Oid5152RuntimeMaintenanceAll(3);

                Expect(self.ObjectId == 7,
                    "split partial recovery must still restore self identity first");
                Expect(self.Runtime.Unk328 == -1 && self.Runtime.Unk338 == 900,
                    "split partial recovery must persist self cooldown writes");
                Expect(self.CurrentFrameId == 290 && self.Health.PP == 123 &&
                       self.Health.HP == 180 && self.Health.HPBound == 180,
                    "split partial recovery must not apply frame112, PP0 or HP halving");
                Expect(self.Frame.D == null,
                    "split partial recovery must leave self frame data reloaded against original DAT even when frame 290 is absent");
                Expect(partner.Runtime.OidMergeDormant && world.ObjectCount == 1,
                    "split partial recovery must not revive dormant partner or increment ObjectCount");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckOid5152DjaReleaseTriggersSameTickSplit()
        {
            Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildOid5152Wrappers();
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null;

                var world = new SimulationWorld();
                LF2Character self = CreateCharacter("SelfCheck_Oid51_Dja", 7, wrappers[7].characterData);
                LF2Character partner = CreateCharacter("SelfCheck_Oid8_Dormant", 8, wrappers[8].characterData);
                self.SetRuntimeSlotIndex(0);
                partner.SetRuntimeSlotIndex(11);
                world.Register(self);
                world.Register(partner);

                self.RelationTeam = 4;
                partner.RelationTeam = 4;
                self.TryApplyRuntimeIdentity(51, 290, true, out _);
                self.Runtime.Unk328 = 1;
                self.Runtime.Unk32C = 11;
                self.Runtime.Unk330 = 7;
                self.Runtime.Unk334 = 8;
                self.Runtime.Unk338 = 30;
                self.Health.HP = 180;
                self.Health.HPBound = 180;
                self.Health.HP3 = 200;
                self.Runtime.SetPosition(60f, 0f, 7f);
                self.Runtime.SyncIntegerPosition();

                partner.Runtime.OidMergeDormant = true;
                partner.Runtime.SetPosition(60f, 0f, 7f);
                partner.Runtime.SyncIntegerPosition();

                SetPrivateField(self.InputState, "_comboDJA", (byte)2);
                ((SelfCheckController)self.Controller).InputBuffer.EnqueueForTick(1, FuncKeyMask.jump, true);

                var tickSystem = new NTSDBattleTickSystem(world);
                tickSystem.RunReleaseTick(1);

                Expect(self.ObjectId == 7 && partner.ObjectId == 8 && world.ObjectCount == 2,
                    "DJA release in PostCooldownInput must reach M-1 on the same tick and trigger immediate split");
                Expect(self.Runtime.Unk338 == 900,
                    "same-tick DJA release split must end with split cooldown 900");

                LF2Character djaOnly = CreateCharacter("SelfCheck_Oid51_DjaOnly", 51, wrappers[51].characterData);
                djaOnly.ImmediateFrame(290);
                djaOnly.Runtime.Unk328 = 1;
                djaOnly.Runtime.Unk338 = 77;
                SetPrivateField(djaOnly.InputState, "_comboDJA", (byte)3);
                djaOnly.ApplyFrameInputFromLocalState();

                Expect(djaOnly.Runtime.Unk338 == 0,
                    "merged DJA branch must zero Unk338 when frame jump cannot be resolved");
                Expect((byte)GetPrivateField(djaOnly.InputState, "_comboDJA") == 3,
                    "merged DJA branch must not clear comboDJA state");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckRespawnPassWithoutStoredCount()
        {
            var world = new SimulationWorld();
            LF2Character dead = CreateCharacter("SelfCheck_Respawn_NoCount", 1, BuildRespawnCharacterData("SelfCheck_Respawn_NoCount"));
            LF2Character allyA = CreateCharacter("SelfCheck_Respawn_AllyA", 2, BuildRespawnCharacterData("SelfCheck_Respawn_AllyA"));
            LF2Character allyB = CreateCharacter("SelfCheck_Respawn_AllyB", 3, BuildRespawnCharacterData("SelfCheck_Respawn_AllyB"));
            dead.SetRuntimeSlotIndex(0);
            allyA.SetRuntimeSlotIndex(1);
            allyB.SetRuntimeSlotIndex(2);
            world.Register(dead);
            world.Register(allyA);
            world.Register(allyB);

            dead.RelationTeam = 5;
            allyA.RelationTeam = 5;
            allyB.RelationTeam = 5;
            dead.ImmediateFrame(14);
            dead.Health.HP = 0;
            dead.Health.HP3 = 180;
            dead.Health.HPBound = 60;
            dead.HP2Orig = 3;
            dead.Health.PP = 12;
            dead.HitStun = 3;
            dead.Runtime.SetPosition(40.0, 0.0, 5.0);
            dead.Runtime.SetVelocity(0.0, -7.0, 0.0);
            dead.Runtime.SyncIntegerPosition();

            allyA.Runtime.SetPosition(100.0, 0.0, 40.0);
            allyA.Runtime.SetVelocity(0.0, 0.0, 0.0);
            allyA.Runtime.SyncIntegerPosition();
            allyB.Runtime.SetPosition(160.0, 0.0, 20.0);
            allyB.Runtime.SetVelocity(0.0, 0.0, 0.0);
            allyB.Runtime.SyncIntegerPosition();

            DeterministicRng expectedRng = new DeterministicRng(0x4E545344u);
            int expectedX = 130 + expectedRng.NextInt(0, 51) - 26;
            int expectedZ = 30 + expectedRng.NextInt(0, 31) - 16;

            world.PostFrameAdvanceDeathCleanupAll(1);

            Expect(dead.HP2Orig == 2,
                "respawn no-count branch must decrement HP2 overlay by 1");
            Expect(dead.Health.HP == 180 && dead.Health.HPBound == 180,
                "respawn no-count branch must restore HP and HPBound from HP3");
            Expect(dead.Health.PP == 500,
                "respawn no-count branch must refill PP to 500");
            Expect(dead.CurrentFrameId == 212 && dead.HitStun == 20,
                "respawn no-count branch must enter frame 212 and arm 20 hit stop");
            Expect(dead.GetRuntimeYInt() == -300 && Nearly(dead.Runtime.Vy, 0.0),
                "respawn no-count branch must set y to -300 and zero Vy");
            Expect(dead.GetRuntimeXInt() == expectedX && dead.GetRenderZInt() == expectedZ,
                $"respawn no-count branch must respawn around same-relation teammates using release RNG offsets; " +
                $"expected=({expectedX},{expectedZ}) actual=({dead.GetRuntimeXInt()},{dead.GetRenderZInt()}) " +
                $"runtimeXZ=({dead.Runtime.X},{dead.Runtime.Z}) alliesXZ=({allyA.GetRuntimeXInt()},{allyA.GetRenderZInt()})/({allyB.GetRuntimeXInt()},{allyB.GetRenderZInt()})");
        }

        private static void CheckRespawnPassFreeEntityGate()
        {
            var world = new SimulationWorld();
            LF2Character freed = CreateCharacter("SelfCheck_Respawn_Free", 1, BuildRespawnCharacterData("SelfCheck_Respawn_Free"));
            LF2Character gated = CreateCharacter("SelfCheck_Respawn_Gated", 2, BuildRespawnCharacterData("SelfCheck_Respawn_Gated"));
            freed.SetRuntimeSlotIndex(0);
            gated.SetRuntimeSlotIndex(1);
            world.Register(freed);
            world.Register(gated);

            freed.RelationTeam = 5;
            freed.ImmediateFrame(14);
            freed.Health.HP = 0;
            freed.HP2Orig = 1;
            freed.HitStun = 2;

            gated.RelationTeam = 4;
            gated.ImmediateFrame(14);
            gated.Health.HP = 0;
            gated.HP2Orig = 5;
            gated.HitStun = 2;
            gated.Runtime.SetPosition(33.0, 0.0, 12.0);
            gated.Runtime.SetVelocity(0.0, 0.0, 0.0);
            gated.Runtime.SyncIntegerPosition();

            world.PostFrameAdvanceDeathCleanupAll(2);

            Expect(world.FindEntityByRuntimeSlotForQuery(0) == null,
                "respawn no-count branch must free entity immediately when HP2Orig < 2");
            Expect(gated.CurrentFrameId == 14 && gated.HP2Orig == 5 &&
                   gated.GetRuntimeXInt() == 33 && gated.GetRenderZInt() == 12,
                "respawn pass must respect slot<20 + relation/kill gate and leave gated lying entity unchanged");
        }

        private static void CheckRespawnPassWithStoredCountAndEffectSpawn()
        {
            System.Func<SimulationWorld, LF2Entity, LF2Entity> previousOverride = SimulationWorld.RespawnEffectSpawnOverride;
            RespawnSelfCheckEffectEntity spawned = null;
            try
            {
                SimulationWorld.RespawnEffectSpawnOverride = (world, source) =>
                {
                    spawned = new RespawnSelfCheckEffectEntity();
                    spawned.BindData(998, BuildRespawnEffectData());
                    spawned.RelationTeam = source.RelationTeam;
                    spawned.SpawnerEntityIndex = source.Runtime?.SlotIndex ?? -1;
                    spawned.Runtime.SetPosition(source.GetRuntimeXInt(), source.GetRuntimeYInt(), source.GetRenderZInt() + 1.0);
                    spawned.Runtime.SetVelocity(0.0, 0.0, 0.0);
                    spawned.Runtime.SyncIntegerPosition();
                    spawned.SetRuntimeSlotIndex(25);
                    world.Register(spawned);
                    return spawned;
                };

                var world = new SimulationWorld();
                LF2Character dead = CreateCharacter("SelfCheck_Respawn_WithCount", 0x1E, BuildRespawnCharacterData("SelfCheck_Respawn_WithCount"));
                dead.SetRuntimeSlotIndex(0);
                world.Register(dead);

                dead.RelationTeam = 3;
                dead.KillCount = 0;
                dead.ImmediateFrame(14);
                dead.Health.HP = 0;
                dead.Health.PP = 77;
                dead.Health.HPBound = 10;
                dead.Health.HP3 = 10;
                dead.HPOrig = 6;
                dead.HP2Orig = 4;
                dead.RespawnCount = 80;
                dead.AttackingCounter = 9;
                dead.HitStun = 4;
                dead.Runtime.SetPosition(77.0, -12.0, 19.0);
                dead.Runtime.SetVelocity(0.0, 0.0, 0.0);
                dead.Runtime.SyncIntegerPosition();

                world.PostFrameAdvanceDeathCleanupAll(3);

                Expect(dead.HP2Orig == 6 && dead.HPOrig == 0,
                    "respawn stored-count branch must copy HP overlay before clearing HPOrig");
                Expect(dead.Health.PP == 0,
                    "respawn stored-count branch must zero PP");
                Expect(dead.Health.HP == 80 && dead.Health.HPBound == 80 && dead.Health.HP3 == 80,
                    "respawn stored-count branch must restore HP/HPBound/HP3 from RespawnCount");
                Expect(dead.RespawnCount == 0 && dead.RelationTeam == 1,
                    "respawn stored-count branch must clear RespawnCount and reset relation identity to 1");
                Expect(dead.Runtime.RenderPicOffset == 0x8C,
                    "respawn stored-count branch must write render pic offset 0x8C for oid 0x1E..0x24");
                Expect(dead.CurrentFrameId == 0xDB && dead.FrameDelay == 0xA && dead.AttackingCounter == 0,
                    "respawn stored-count branch must enter frame 0xDB with frame delay 10 and clear attacking");
                Expect(spawned != null && world.ObjectCount == 2,
                    "respawn stored-count branch must spawn oid998 effect into the world");
                Expect(spawned.ObjectId == 998 && (spawned.Frame?.N ?? -1) == 6,
                    "respawn effect spawn must use oid998 frame 6");
                Expect(spawned.GetRuntimeXInt() == 77 &&
                       spawned.GetRuntimeYInt() == -12 &&
                       spawned.GetRenderZInt() == 20,
                    "respawn effect spawn must copy x/y and use z_int + 1");
                Expect(spawned.RelationTeam == 1 && spawned.SpawnerEntityIndex == dead.Runtime.SlotIndex,
                    "respawn effect spawn must inherit post-respawn relation identity and spawner slot");
            }
            finally
            {
                SimulationWorld.RespawnEffectSpawnOverride = previousOverride;
            }
        }

        private static void CheckKind15CharacterWhirlwind()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildKind1516CharacterData("SelfCheck_Kind1516");
            LF2Character attacker = CreateCharacter("SelfCheck_Kind15_Attacker", 1, data);
            LF2Character groundedVictim = CreateCharacter("SelfCheck_Kind15_Grounded", 2, data);
            LF2Character airVictim = CreateCharacter("SelfCheck_Kind15_Air", 3, data);

            world.Register(attacker);
            world.Register(groundedVictim);
            world.Register(airVictim);

            attacker.ImmediateFrame(0);
            groundedVictim.ImmediateFrame(0);
            airVictim.ImmediateFrame(0);

            attacker.Runtime.SetPosition(0.0, 0.0, 0.0);
            attacker.Runtime.SetVelocity(0.0, 0.0, 0.0);
            attacker.Runtime.SyncIntegerPosition();

            groundedVictim.Runtime.SetPosition(10.0, 0.0, 5.0);
            groundedVictim.Runtime.SetVelocity(2.0, -5.0, 1.0);
            groundedVictim.Runtime.SyncIntegerPosition();
            groundedVictim.KnockbackVx = 0.0;
            groundedVictim.KnockbackVy = 0.0;
            groundedVictim.KnockbackVz = 0.0;

            bool groundedResolved = groundedVictim.Hit(
                new InteractionArea { kind = 15 },
                attacker,
                Vector3.zero,
                default);

            Expect(groundedResolved, "kind15 should resolve on grounded character victim");
            Expect(Mathf.Approximately((float)groundedVictim.Runtime.Vx, 1f) &&
                   Mathf.Approximately((float)groundedVictim.KnockbackVx, 1f),
                "kind15 should rewrite victim vx from runtime vx ± 1");
            Expect(Mathf.Approximately((float)groundedVictim.Runtime.Vz, 0.5f) &&
                   Mathf.Approximately((float)groundedVictim.KnockbackVz, 0.5f),
                "kind15 should rewrite victim vz from runtime vz ± 0.5");
            Expect(groundedVictim.GetRuntimeYInt() == -2 &&
                   Mathf.Approximately((float)groundedVictim.Runtime.Y, -2f) &&
                   Mathf.Approximately((float)groundedVictim.Runtime.Vy, -6f),
                "kind15 grounded branch should clamp Y/YInt to -2 and set Vy=-6");

            airVictim.Runtime.SetPosition(10.0, -5.0, 5.0);
            airVictim.Runtime.SetVelocity(0.0, -5.0, 0.0);
            airVictim.Runtime.SyncIntegerPosition();
            airVictim.KnockbackVx = 0.0;
            airVictim.KnockbackVy = 0.0;
            airVictim.KnockbackVz = 0.0;

            bool airResolved = airVictim.Hit(
                new InteractionArea { kind = 15 },
                attacker,
                Vector3.zero,
                default);

            Expect(airResolved, "kind15 should resolve on airborne character victim");
            Expect(airVictim.GetRuntimeYInt() == -5, "kind15 airborne branch should preserve YInt below -2");
            Expect(Mathf.Approximately((float)airVictim.Runtime.Vy, -8f) &&
                   Mathf.Approximately((float)airVictim.KnockbackVy, -8f),
                "kind15 airborne branch should subtract vyStep=3.0 and mirror KnockbackVy");
        }

        private static void CheckKind16CharacterSideEffects()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildKind1516CharacterData("SelfCheck_Kind1516");
            LF2Character holder = CreateCharacter("SelfCheck_Kind16_Holder", 1, data);
            LF2Character attacker = CreateCharacter("SelfCheck_Kind16_Attacker", 2, data);
            LF2Character victim = CreateCharacter("SelfCheck_Kind16_Victim", 3, data);
            LF2Character heldTarget = CreateCharacter("SelfCheck_Kind16_HeldTarget", 4, data);

            world.Register(holder);
            world.Register(attacker);
            world.Register(victim);
            world.Register(heldTarget);

            holder.ImmediateFrame(0);
            attacker.ImmediateFrame(0);
            victim.ImmediateFrame(0);
            heldTarget.ImmediateFrame(10);

            attacker.HolderCopySlot = holder.Runtime.SlotIndex;
            victim.Health.HP = 70;
            victim.Health.HPBound = 100;
            victim.Health.HPLost = 0;
            victim.FallDamageDiv = 50;
            victim.KillCount = -1;
            victim.ComboCountVic = 0;
            victim.AttackingCounter = 5;
            victim.Runtime.LinkState = 2;
            victim.Runtime.TargetSlotIndex = heldTarget.Runtime.SlotIndex;

            heldTarget.Runtime.LinkState = -2;
            heldTarget.Runtime.HolderStableId = victim.Runtime.SlotIndex;
            heldTarget.Runtime.Vy = 0.0;

            bool resolved = victim.Hit(
                new InteractionArea
                {
                    kind = 16,
                    injury = 40,
                    vrest = 12,
                },
                attacker,
                Vector3.zero,
                default);

            Expect(resolved, "kind16 should resolve on character victim");
            Expect(victim.Health.HP == -10, "kind16 should scale injury by FallDamageDiv rather than MaxMP");
            Expect(victim.Health.HPBound == 74, "kind16 should reduce HPBound by adjustedInjury/3 with integer division");
            Expect(victim.Health.HPLost == 0, "kind16 should not accumulate HPLost via generic injury path");
            Expect(victim.ComboCountVic == 80, "kind16 should add adjusted injury to victim combo counter");
            Expect(holder.KillStat == 1, "kind16 lethal hit should increment holder KillStat");
            Expect(holder.ComboCountAtk == 80, "kind16 should add adjusted injury to holder ComboCountAtk");
            Expect(victim.Frame.N == LF2StandardFrames.MpDrain && victim.AttackingCounter == 0,
                "kind16 should jump victim to frame 200 and clear attacking counter");
            Expect(victim.ItrRest.GetVrest(attacker.Runtime.SlotIndex) == 45,
                "kind16 release path should overwrite attacker-side vrest to 45 when victim is holding a target");
            Expect(victim.ItrRest.GetVrest(heldTarget.Runtime.SlotIndex) == 30,
                "kind16 release path should write held-target vrest=30");
            Expect(victim.Runtime.LinkState == 0 && heldTarget.Runtime.LinkState == 0,
                "kind16 should break 2/-2 hold links");
            Expect(Mathf.Approximately((float)heldTarget.Runtime.Vy, -1f),
                "kind16 should launch released held target with Vy=-1");
        }

        private static void CheckLateDeathBounceFrame()
        {
            var world = new SimulationWorld();
            LF2CharacterData data = BuildDeathBounceCharacterData("SelfCheck_DeathBounce");
            LF2Character victim = CreateCharacter("SelfCheck_DeathBounceVictim", 1, data);
            world.Register(victim);

            victim.ImmediateFrame(5);
            victim.Health.HP = 0;
            victim.Runtime.SetPosition(12.0, 0.0, 3.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            victim.KnockbackVy = 0f;

            victim.RunLateDeathOpointPreCleanupPhase();

            Expect(victim.Frame.N == 186,
                "late death bounce should force frame 186 for dead lying character in frame<12");
            Expect(victim.GetRuntimeYInt() == -1 &&
                   Mathf.Approximately((float)victim.Runtime.Y, -1f) &&
                   Mathf.Approximately((float)victim.Runtime.Vy, -3f) &&
                   Mathf.Approximately((float)victim.KnockbackVy, -3f),
                "late death bounce should set y/yInt to -1 and vy/knockbackVy to -3");

            victim.ImmediateFrame(212);
            victim.Health.HP = 0;
            victim.Runtime.SetPosition(12.0, 0.0, 3.0);
            victim.Runtime.SetVelocity(0.0, 0.0, 0.0);
            victim.Runtime.SyncIntegerPosition();
            victim.KnockbackVy = 0f;

            victim.RunLateDeathOpointPreCleanupPhase();

            Expect(victim.Frame.N == 186,
                "late death bounce should re-launch grounded death frame 212");
        }

        private static void CheckComboWrappersCharacterFrameJumps()
        {
            AssertComboFrameJump(
                "SelfCheck_DRA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DRA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.right, FuncKeyMask.jump },
                100,
                "right",
                verifyCooldownClear: true);

            AssertComboFrameJump(
                "SelfCheck_DLA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DLA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.left, FuncKeyMask.jump },
                100,
                "left");

            AssertComboFrameJump(
                "SelfCheck_DUA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DUA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.up, FuncKeyMask.jump },
                101,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DDA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DDA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.down, FuncKeyMask.jump },
                102,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DRJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DRJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.right, FuncKeyMask.def },
                103,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DLJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DLJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.left, FuncKeyMask.def },
                103,
                "left");

            AssertComboFrameJump(
                "SelfCheck_DUJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DUJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.up, FuncKeyMask.def },
                104,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DDJ",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DDJ", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.down, FuncKeyMask.def },
                105,
                "right");

            AssertComboFrameJump(
                "SelfCheck_DJA",
                1,
                BuildComboWrapperCharacterData("SelfCheck_DJA", 180),
                new[] { FuncKeyMask.att, FuncKeyMask.def, FuncKeyMask.jump },
                180,
                "right");
        }

        private static void CheckOid6DjaGuardComboHold()
        {
            var world = new SimulationWorld();
            LF2Character guarded = CreateCharacter("SelfCheck_Oid6_DjaGuard", 6, BuildComboWrapperCharacterData("SelfCheck_Oid6_DjaGuard", 300));
            guarded.SwitchDir("right");
            guarded.Health.HP = 200;
            world.Register(guarded);
            world.Runtime.Flow.DjaGuardGlobal44F224 = 0;

            EnqueueComboTicks((SelfCheckController)guarded.Controller, 1, FuncKeyMask.att, FuncKeyMask.def, FuncKeyMask.jump);
            RunComboTicks(guarded, 1, 3);

            Expect(guarded.Frame.N == 0,
                "oid6 DjaGuard must block DJA frame jump when hit_ja=300 and guard flag is active");
            Expect((byte)GetPrivateField(guarded.InputState, "_comboDJA") == 3,
                "oid6 DjaGuard must preserve comboDJA state while the guard blocks frame jump");

            LF2Character released = CreateCharacter("SelfCheck_Oid6_DjaRelease", 6, BuildComboWrapperCharacterData("SelfCheck_Oid6_DjaRelease", 300));
            released.SwitchDir("right");
            released.Health.HP = 200;
            world.Register(released);
            world.Runtime.Flow.DjaGuardGlobal44F224 = 1;

            EnqueueComboTicks((SelfCheckController)released.Controller, 1, FuncKeyMask.att, FuncKeyMask.def, FuncKeyMask.jump);
            RunComboTicks(released, 1, 3);

            Expect(released.Frame.N == 300,
                "oid6 DJA must frame jump once DjaGuardGlobal44F224 no longer blocks it");
            Expect((byte)GetPrivateField(released.InputState, "_comboDJA") == 0,
                "successful oid6 DJA must clear comboDJA state");
        }

        private static void CheckStageWaveImmediateSpawnAndAdvance()
        {
            const int stageOid = 201;
            LF2CharacterDataWrapper stageWrapper = BuildStageSpawnWrapper(stageOid, "SelfCheck_StageImmediate");
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid => oid == stageOid ? stageWrapper : null;

                var world = new SimulationWorld();
                world.Runtime.Match.BattleGameModeId = 1;
                world.Runtime.Stage.SetSceneSnapshot(1000, 180, 350, 0, 0);
                world.StageCampaigns.Add(new BattleStageCampaignData
                {
                    Id = 9,
                    Phases = new List<BattleStagePhaseData>
                    {
                        new BattleStagePhaseData
                        {
                            Spawns = new List<BattleStageSpawnData>
                            {
                                new BattleStageSpawnData
                                {
                                    Id = stageOid,
                                    Act = 0,
                                    Hp = 321,
                                    Times = 1,
                                    X = 100,
                                    Y = -20,
                                    Ratio = 0.0,
                                },
                            },
                        },
                        new BattleStagePhaseData
                        {
                            Bound = 1400,
                        },
                    },
                });
                world.StageProgression.StageSeriesIdx = 9;
                world.StageProgression.WaveIdx = 0;
                world.SetStageProgressionValid(true);

                world.CurrentWaveStageTickAll();

                var entities = new List<LF2Entity>();
                world.GetAllEntities(entities);
                Expect(entities.Count == 1,
                    "stage immediate spawn must create exactly one entity for one immediate entry");
                LF2Entity spawned = entities[0];
                int spawnedSlot = spawned.Runtime?.SlotIndex ?? -1;
                Expect(spawned.ObjectId == stageOid && spawnedSlot >= 50,
                    "stage immediate spawn must use a dynamic runtime slot");
                Expect(spawned.Frame.N == 0 && spawned.FrameDelay == 0,
                    "stage immediate spawn must preserve configured action zero with zero frame delay");
                Expect(spawned.Health.HP == 321 && spawned.Health.HPBound == 321 && spawned.Health.HP3 == 321,
                    "stage immediate spawn must apply configured HP to HP, HPBound and HP3");
                Expect(spawned.Team == 2 && spawned.RelationTeam == 2 &&
                       spawned.Unk344 == 2 && spawned.HitStun == 20 &&
                       spawned.HolderCopySlot == spawnedSlot,
                    "stage immediate character spawn must apply team, Unk344, init and self-holder contracts");
                Expect(spawned.AiControlled,
                    "stage/opoint character spawns must be AI-controlled by default");
                Expect(world.StageSpawnWaveApplied == 0 && world.StageProgression.WaveIdx == 0,
                    "stage immediate producer must initialize once without advancing while its entity is alive");

                world.CurrentWaveStageTickAll();
                Expect(world.StageProgression.WaveIdx == 0,
                    "stage wave must not advance while a configured stage entity remains active");

                world.Unregister(spawned);
                LF2Character reservedSlotEntity = CreateCharacter(
                    "SelfCheck_StageReservedSlot",
                    stageOid,
                    stageWrapper.characterData);
                reservedSlotEntity.SetRuntimeSlotIndex(20);
                world.Register(reservedSlotEntity);
                world.CurrentWaveStageTickAll();

                Expect(world.StageProgression.WaveIdx == 1,
                    "stage wave must ignore matching non-stage entities below the Unity dynamic slot range");
                Expect(world.Runtime.Stage.XMaxOverride == 1400 &&
                       world.Runtime.Stage.CameraMaxOverride == 606 &&
                       world.Runtime.Stage.StageWidthPx == 1400,
                    "stage phase advance must apply bound and camera bound overrides");
                Expect(world.StageSpawnWaveApplied == 1 && world.StageSpawnWaveDeferredEntryApplied == 1,
                    "empty next phase must initialize both stage spawn producer markers");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckStageWaveBootstrapAndSpawnContract()
        {
            const string stageText =
                "<stage> id: 12 #self-check\n" +
                "<phase> bound: 900\n" +
                "id: 205 act: 0 hp: 275 times: 3 x: -100 y: -20 ratio: 1.5 join: 8\n" +
                "<phase_end>\n" +
                "<stage_end>\n";

            List<BattleStageCampaignData> campaigns = BattleStageCampaignLoader.ParseText(stageText);
            Expect(campaigns.Count == 1 && campaigns[0].Id == 12 && campaigns[0].Comment == "self-check",
                "stage campaign parser must load stage identity and comment");
            BattleStagePhaseData phase = campaigns[0].Phases[0];
            BattleStageSpawnData spawn = phase.Spawns[0];
            Expect(phase.Bound == 900 && spawn.Id == 205 && spawn.Act == 0 && spawn.Hp == 275 &&
                   spawn.Times == 3 && spawn.X == -100 && spawn.Y == -20 &&
                   Nearly(spawn.Ratio, 1.5) && spawn.Join == 8,
                "stage campaign parser must map all phase and spawn fields");

            string tempStagePath = Path.Combine(Application.temporaryCachePath, "ntsd_stage_campaign_self_check.dat");
            try
            {
                File.WriteAllText(tempStagePath, stageText);
                List<BattleStageCampaignData> loadedCampaigns =
                    BattleStageCampaignLoader.LoadFromFile(tempStagePath);
                Expect(loadedCampaigns.Count == 1 && loadedCampaigns[0].Id == 12,
                    "stage campaign production loader must read an explicit plaintext DAT path");
            }
            finally
            {
                if (File.Exists(tempStagePath))
                    File.Delete(tempStagePath);
            }

            var world = new SimulationWorld();
            world.ConfigureStageCampaigns(campaigns, 12, -1);
            Expect(world.StageProgressionValid && world.StageProgression.StageSeriesIdx == 12 &&
                   world.StageProgression.WaveIdx == -1,
                "stage production bootstrap must retain authority pre-wave state after data load");
            Expect(world.StartInitialStageWave() && world.StageProgression.WaveIdx == 0 &&
                   world.Runtime.Stage.XMaxOverride == 900,
                "stage production bootstrap must advance pre-wave to wave zero and apply its bound");

            OPointCreateTask task = SimulationWorld.BuildStageSpawnTask(spawn, 10, -20, 200, "right");
            Expect(task.preserveActionZero && task.opoint.action == 0,
                "stage factory task must preserve authored action zero");

            var character = new StageSpawnContractSelfCheckEntity(LF2ObjectType.Character);
            character.SetRuntimeSlotIndex(50);
            SimulationWorld.ApplyStageSpawnRuntimeContract(character, 300);
            Expect(character.Team == 2 && character.RelationTeam == 2 &&
                   character.Unk344 == 2 && character.HitStun == 20 && character.HolderCopySlot == 50,
                "stage character contract must map Unk364 to RelationTeam=2 and use character init semantics");

            var type5 = new StageSpawnContractSelfCheckEntity(LF2ObjectType.Other);
            type5.SetRuntimeSlotIndex(51);
            SimulationWorld.ApplyStageSpawnRuntimeContract(type5, 301);
            Expect(type5.RelationTeam == 2 && type5.HitStun == 20 && type5.Unk344 == 2,
                "stage DAT type 5 contract must use authority character-init semantics");

            var projectile = new StageSpawnContractSelfCheckEntity(LF2ObjectType.SpecialAttack);
            projectile.SetRuntimeSlotIndex(52);
            SimulationWorld.ApplyStageSpawnRuntimeContract(projectile, 302);
            Expect(projectile.Team == 2 && projectile.RelationTeam == 0 &&
                   projectile.HitStun == 0 && projectile.Unk344 == 2,
                "stage non-character contract must preserve Team=2 but clear RelationTeam/Unk364");
        }

        private static void CheckStageWavePositiveSpawnRefill()
        {
            const int stageOid = 202;
            LF2CharacterDataWrapper stageWrapper = BuildStageSpawnWrapper(stageOid, "SelfCheck_StagePositive");
            System.Func<int, LF2CharacterDataWrapper> previousResolver = LF2Entity.RuntimeCharacterConfigResolverOverride;
            try
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = oid => oid == stageOid ? stageWrapper : null;

                var world = new SimulationWorld();
                world.Runtime.Match.BattleGameModeId = 2;
                LF2Character factorCharacter = CreateCharacter(
                    "SelfCheck_StageFactor",
                    1,
                    BuildStageSpawnCharacterData("SelfCheck_StageFactor"));
                factorCharacter.SetRuntimeSlotIndex(0);
                world.Register(factorCharacter);
                world.StageCampaigns.Add(new BattleStageCampaignData
                {
                    Id = 10,
                    Phases = new List<BattleStagePhaseData>
                    {
                        new BattleStagePhaseData
                        {
                            Spawns = new List<BattleStageSpawnData>
                            {
                                new BattleStageSpawnData
                                {
                                    Id = stageOid,
                                    Act = 7,
                                    Hp = 250,
                                    Times = 2,
                                    X = 200,
                                    Ratio = 1.0,
                                },
                            },
                        },
                    },
                });
                world.StageProgression.StageSeriesIdx = 10;
                world.StageProgression.WaveIdx = 0;
                world.SetStageProgressionValid(true);

                world.CurrentWaveStageTickAll();

                Expect(world.StageSpawnWaveDeferredEntryApplied == 0 && world.StageSpawnRuntimeWave == 0,
                    "positive stage producer must initialize its deferred marker and runtime wave");
                Expect(world.StageSpawnRuntimeEntryCount.Count == 1 &&
                       world.StageSpawnRuntimeEntryCount[0] == 1 &&
                       world.StageSpawnRuntimeTargetTotal[0] == 2 &&
                       world.StageSpawnRuntimeSpawnedTotal[0] == 1,
                    "positive stage runtime must derive one concurrent entry and two total spawns from factor 1");
                int firstSlot = world.StageSpawnRuntimeSlots[0][0];
                LF2Entity firstSpawn = world.FindEntityByRuntimeSlotForQuery(firstSlot);
                Expect(firstSlot >= 50 && firstSpawn != null && firstSpawn.ObjectId == stageOid,
                    "positive stage producer must track its active spawned entity by dynamic runtime slot");

                world.CurrentWaveStageTickAll();
                Expect(world.StageSpawnRuntimeSpawnedTotal[0] == 1 &&
                       world.StageSpawnRuntimeSlots[0][0] == firstSlot,
                    "positive stage producer must not exceed its concurrent entry count while the slot is alive");

                world.Unregister(firstSpawn);
                world.CurrentWaveStageTickAll();

                int replacementSlot = world.StageSpawnRuntimeSlots[0][0];
                LF2Entity replacement = world.FindEntityByRuntimeSlotForQuery(replacementSlot);
                Expect(replacement != null && replacement.ObjectId == stageOid,
                    "positive stage producer must refill a cleared concurrent slot");
                Expect(world.StageSpawnRuntimeSpawnedTotal[0] == 2,
                    "positive stage producer must increment total spawned count on refill");

                world.Unregister(replacement);
                world.CurrentWaveStageTickAll();
                Expect(world.StageSpawnRuntimeSlots[0][0] == -1 &&
                       world.StageSpawnRuntimeSpawnedTotal[0] == 2,
                    "positive stage producer must stop refilling after reaching target total");
            }
            finally
            {
                LF2Entity.RuntimeCharacterConfigResolverOverride = previousResolver;
            }
        }

        private static void CheckAiTargetCacheCoordinateAndDeterminism()
        {
            LF2CharacterData data = BuildComboWrapperCharacterData("SelfCheck_AI", 180);
            SimulationWorld firstWorld = BuildAiSelfCheckWorld(data, 12345, out LF2Character firstAi, out LF2Character firstTarget);
            firstWorld.AiInputAndComboAll(2);

            Expect(firstAi.Runtime.Unk360 == firstTarget.Runtime.SlotIndex,
                "AI must cache the selected target by runtime slot");
            Expect(firstAi.Runtime.KeyRight != 0 || firstAi.Runtime.KeyLeft != 0 ||
                   firstAi.Runtime.KeyUp != 0 || firstAi.Runtime.KeyDown != 0 ||
                   firstAi.Runtime.KeyAttack != 0 || firstAi.Runtime.KeyJump != 0 || firstAi.Runtime.KeyDefend != 0 ||
                   firstAi.Runtime.ComboDra != 0 || firstAi.Runtime.ComboDla != 0 || firstAi.Runtime.ComboDua != 0 ||
                   firstAi.Runtime.ComboDda != 0 || firstAi.Runtime.ComboDrj != 0 || firstAi.Runtime.ComboDlj != 0 ||
                   firstAi.Runtime.ComboDuj != 0 || firstAi.Runtime.ComboDdj != 0 || firstAi.Runtime.ComboDja != 0,
                "AI target pass must produce movement, action, or combo intent");

            SimulationWorld secondWorld = BuildAiSelfCheckWorld(data, 12345, out LF2Character secondAi, out _);
            secondWorld.AiInputAndComboAll(2);
            Expect(AiInputSignature(firstAi.Runtime) == AiInputSignature(secondAi.Runtime),
                "AI decisions must be deterministic for the same seed and runtime-slot world state");

            SimulationWorld coordinateWorld = new SimulationWorld();
            LF2Character coordinateAi = CreateCharacter("SelfCheck_AI_Coordinate", 33, data);
            coordinateAi.SetRuntimeSlotIndex(3);
            coordinateAi.AiControlled = true;
            coordinateAi.RelationTeam = 1;
            coordinateAi.Runtime.SetPosition(100, 0, 100);
            coordinateAi.Runtime.SyncIntegerPosition();
            coordinateAi.Runtime.Unk3FC = 500;
            coordinateAi.Runtime.Unk400 = 300;
            coordinateAi.Runtime.KeyRight = 1;
            coordinateWorld.Runtime.Flow.AiRand3 = 5;
            coordinateWorld.Runtime.Match.Difficulty = 2;
            coordinateWorld.Rng.Seed(3);
            coordinateWorld.Register(coordinateAi);
            coordinateWorld.AiInputAndComboAll(2);
            Expect(coordinateAi.Runtime.KeyRight == 1 && coordinateAi.Runtime.KeyDown == 1,
                "AI coordinate mode must move toward Unk3FC/Unk400 without requiring a target");
            Expect(coordinateAi.Runtime.Unk360 == -1,
                "AI coordinate mode must not mutate the cached combat target");
            Expect(coordinateWorld.Runtime.Flow.AiRand3 == 5 && coordinateAi.Runtime.PrevRight == 0,
                "AI coordinate mode must reuse the previous world AiRand3 before normal-path globals are recomputed");
            Expect(coordinateAi.Runtime.CdRight == 5 &&
                   coordinateAi.Runtime.InputHistory[4] == 6 &&
                   coordinateAi.Runtime.InputHistory[5] == 2,
                "AI coordinate movement must apply right then down edges in authority history order");
        }

        private static void CheckAiHeldInactiveSlotContract()
        {
            LF2CharacterData data = new LF2CharacterData
            {
                name = "SelfCheck_AI_HeldInactive",
                frames = new List<LF2FrameData> { Frame(0, 2, 1, 0, 39, 79) },
            };
            SimulationWorld world = new SimulationWorld();
            world.Runtime.Match.Difficulty = 0;
            world.Rng.Seed(3);
            LF2Character ai = CreateCharacter("SelfCheck_AI_HeldInactive_Source", 40, data);
            LF2Character target = CreateCharacter("SelfCheck_AI_HeldInactive_Target", 41, data);
            ai.SetRuntimeSlotIndex(0);
            target.SetRuntimeSlotIndex(1);
            ai.AiControlled = true;
            ai.RelationTeam = 1;
            target.RelationTeam = 2;
            ai.Runtime.LinkState = 1;
            ai.Runtime.TargetSlotIndex = 5;
            ai.Runtime.SetPosition(100, 0, 200);
            target.Runtime.SetPosition(140, 0, 200);
            ai.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            world.Register(ai);
            world.Register(target);
            world.AiInputAndComboAll(2);
            int nextRng = world.Rng.NextRaw();
            Expect(AiInputSignature(ai.Runtime) == "1:0100000:0000000:000000000",
                "a valid but inactive held slot must continue through the authority self-state branch before returning");
            Expect(nextRng == 12168,
                "a valid but inactive held slot must preserve the authority RNG consumption count");
        }

        private static void CheckAiSharedCharacterDatShell()
        {
            LF2CharacterData data = new LF2CharacterData
            {
                name = "SelfCheck_AI_SharedShell",
                frames = new List<LF2FrameData> { Frame(0, 3, 1, 0, 39, 79) },
            };
            SimulationWorld world = new SimulationWorld();
            var shell = new SelfCheckCharacterDatShell();
            shell.ObjectId = 40;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(40, data));
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Frame.N = 0;
            shell.Runtime.HP = 500;
            shell.SetRuntimeSlotIndex(4);
            shell.AiControlled = true;
            shell.RelationTeam = 1;
            shell.Runtime.Unk3FC = 400;
            shell.Runtime.Unk400 = 200;
            shell.Runtime.SetPosition(100, 0, 100);
            shell.Runtime.SyncIntegerPosition();
            world.Register(shell);
            world.AiInputAndComboAll(2);
            Expect(shell.Runtime.KeyRight == 1 && shell.Runtime.KeyDown == 1,
                "current character-DAT entities must run AI even when their CLR shell is not LF2Character");
        }

        private static void CheckAiHumanInputIsolation()
        {
            LF2CharacterData data = BuildComboWrapperCharacterData("SelfCheck_AI_Human", 180);
            SimulationWorld world = new SimulationWorld();
            LF2Character human = CreateCharacter("SelfCheck_HumanIsolation", 1, data);
            human.SetRuntimeSlotIndex(0);
            human.AiControlled = false;
            ((SelfCheckController)human.Controller).InputBuffer.EnqueueForTick(2, FuncKeyMask.right, true);
            world.Register(human);

            world.PostCooldownHumanInputAll(2);
            byte humanRight = human.Runtime.KeyRight;
            world.AiInputAndComboAll(2);

            Expect(humanRight == 1 && human.Runtime.KeyRight == 1,
                "human input must be consumed before M1 and remain untouched by the AI pass");
            Expect(human.Runtime.Unk360 == -1,
                "human-controlled characters must not run AI target selection");
        }

        private static SimulationWorld BuildAiSelfCheckWorld(
            LF2CharacterData data,
            int seed,
            out LF2Character ai,
            out LF2Character target)
        {
            SimulationWorld world = new SimulationWorld();
            world.Rng.Seed(seed);
            world.Runtime.Match.Difficulty = 2;
            ai = CreateCharacter("SelfCheck_AI_Source", 33, data);
            target = CreateCharacter("SelfCheck_AI_Target", 4, data);
            ai.SetRuntimeSlotIndex(0);
            target.SetRuntimeSlotIndex(1);
            ai.AiControlled = true;
            target.AiControlled = false;
            ai.RelationTeam = 1;
            target.RelationTeam = 2;
            ai.Runtime.SetPosition(100, 0, 200);
            target.Runtime.SetPosition(260, 0, 210);
            ai.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            world.Register(ai);
            world.Register(target);
            return world;
        }

        private static string AiInputSignature(NTSDEntityRuntime r)
        {
            return $"{r.Unk360}:{r.KeyRight}{r.KeyLeft}{r.KeyUp}{r.KeyDown}{r.KeyAttack}{r.KeyJump}{r.KeyDefend}:" +
                   $"{r.PrevRight}{r.PrevLeft}{r.PrevUp}{r.PrevDown}{r.PrevAttack}{r.PrevJump}{r.PrevDefend}:" +
                   $"{r.ComboDra}{r.ComboDla}{r.ComboDua}{r.ComboDda}{r.ComboDrj}{r.ComboDlj}{r.ComboDuj}{r.ComboDdj}{r.ComboDja}";
        }

        private static LF2CharacterDataWrapper BuildStageSpawnWrapper(int objectId, string name)
        {
            return new LF2CharacterDataWrapper(objectId, BuildStageSpawnCharacterData(name));
        }

        private static LF2CharacterData BuildStageSpawnCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing, 1, 0, 39, 79),
                    Frame(7, LF2States.Standing, 1, 7, 39, 79),
                },
            };
        }

        private static Dictionary<int, LF2CharacterDataWrapper> BuildOid5152Wrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(7, BuildOid5152BaseData("SelfCheck_Oid7")),
                [8] = new LF2CharacterDataWrapper(8, BuildOid5152BaseData("SelfCheck_Oid8")),
                [51] = new LF2CharacterDataWrapper(51, BuildOid5152MergedData()),
            };
        }

        private static LF2CharacterData BuildOid5152BaseData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(10, 2, 1, 10, 39, 79),
                    Frame(112, 0, 1, 112, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildOid5152MergedData()
        {
            LF2FrameData frame290 = Frame(290, 2, 1, 290, 39, 79);
            frame290.hit_ja = 300;

            return new LF2CharacterData
            {
                name = "SelfCheck_Oid51",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(112, 0, 1, 112, 39, 79),
                    frame290,
                },
            };
        }

        private static LF2CharacterData BuildRespawnCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(14, 14, 1, 14, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                    Frame(0xDB, 0, 1, 0xDB, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildKind1516CharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 1, 0, 39, 79),
                    Frame(10, 0, 1, 10, 39, 79),
                    Frame(LF2StandardFrames.MpDrain, 18, 1, LF2StandardFrames.MpDrain, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildDeathBounceCharacterData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(5, LF2States.Lying, 1, 5, 39, 79),
                    Frame(14, LF2States.Lying, 1, 14, 39, 79),
                    Frame(186, LF2States.Lying, 1, 186, 39, 79),
                    Frame(212, LF2States.Lying, 1, 212, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildComboWrapperCharacterData(string name, int djaTargetFrame)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        frameName = "self_check_combo_root",
                        state = 0,
                        wait = 1,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                        hit_Fa = 100,
                        hit_Ua = 101,
                        hit_Da = 102,
                        hit_Fj = 103,
                        hit_Uj = 104,
                        hit_Dj = 105,
                        hit_ja = djaTargetFrame,
                    },
                    Frame(100, 0, 1, 100, 39, 79),
                    Frame(101, 0, 1, 101, 39, 79),
                    Frame(102, 0, 1, 102, 39, 79),
                    Frame(103, 0, 1, 103, 39, 79),
                    Frame(104, 0, 1, 104, 39, 79),
                    Frame(105, 0, 1, 105, 39, 79),
                    Frame(180, 0, 1, 180, 39, 79),
                    Frame(300, 0, 1, 300, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildComboWrapperData(string name, int djaTargetFrame)
        {
            return BuildComboWrapperCharacterData(name, djaTargetFrame);
        }

        private static void AssertComboFrameJump(
            string name,
            int objectId,
            LF2CharacterData data,
            FuncKeyMask[] sequence,
            int expectedFrame,
            string expectedDir,
            bool verifyCooldownClear = false)
        {
            LF2Character character = CreateCharacter(name, objectId, data);
            character.SwitchDir("right");

            EnqueueComboTicks((SelfCheckController)character.Controller, 1, sequence);
            RunComboTicks(character, 1, sequence.Length);

            Expect(character.Frame.N == expectedFrame,
                $"{name} should jump to frame {expectedFrame} after combo wrapper input");
            Expect(character.Runtime.Dir == expectedDir,
                $"{name} should face {expectedDir} after combo wrapper input");

            if (verifyCooldownClear)
            {
                Expect(character.Runtime.CdRight == 0 &&
                       character.Runtime.CdLeft == 0 &&
                       character.Runtime.CdUp == 0 &&
                       character.Runtime.CdDown == 0 &&
                       character.Runtime.CdAttack == 0 &&
                       character.Runtime.CdJump == 0 &&
                       character.Runtime.CdDefend == 0,
                    $"{name} should clear action and direction cooldowns after successful combo frame jump");
            }
        }

        private static void EnqueueComboTicks(SelfCheckController controller, int startTick, params FuncKeyMask[] sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
                controller.InputBuffer.EnqueueForTick(startTick + i, sequence[i], true);
        }

        private static void RunComboTicks(LF2Character character, int startTick, int count)
        {
            for (int i = 0; i < count; i++)
                character.RunPostCooldownInputPhase(startTick + i);
        }

        private static LF2CharacterData BuildRespawnEffectData()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_RespawnEffect998",
                frames = new List<LF2FrameData>
                {
                    Frame(6, 9998, 1, 1000, 39, 79),
                },
            };
        }

        private static SimulationWorld CreateOid5152MergedWorld(
            Dictionary<int, LF2CharacterDataWrapper> wrappers,
            out LF2Character self,
            out LF2Character partner)
        {
            var world = new SimulationWorld();
            self = CreateCharacter("SelfCheck_Oid7_Merged", 7, wrappers[7].characterData);
            partner = CreateCharacter("SelfCheck_Oid8_Merged", 8, wrappers[8].characterData);
            self.SetRuntimeSlotIndex(0);
            partner.SetRuntimeSlotIndex(11);
            world.Register(self);
            world.Register(partner);

            self.ImmediateFrame(10);
            partner.ImmediateFrame(10);
            self.RelationTeam = 3;
            partner.RelationTeam = 3;
            self.Health.HP = 100;
            self.Health.HPBound = 100;
            self.Health.HP3 = 200;
            partner.Health.HP = 100;
            partner.Health.HPBound = 100;
            self.Runtime.SetPosition(90f, 0f, 6f);
            partner.Runtime.SetPosition(120f, 0f, 9f);
            self.Runtime.SyncIntegerPosition();
            partner.Runtime.SyncIntegerPosition();

            world.Oid5152RuntimeMaintenanceAll(1);
            return world;
        }

        private static object GetPrivateField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            return field?.GetValue(instance);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(instance, value);
        }

        private static LF2Entity CreateCpointMatrixEntity(
            bool realCharacter,
            string name,
            int objectId,
            LF2CharacterData data)
        {
            if (realCharacter)
                return CreateCharacter(name, objectId, data);

            var shell = new SelfCheckCharacterDatShell();
            shell.InitializeForCpoint();
            shell.Name = name;
            shell.ObjectId = objectId;
            shell.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            shell.Frame.N = 0;
            shell.Frame.D = shell.FrameCache.GetFrameDataById(0);
            shell.Frame.PN = 0;
            shell.Frame.Prev2 = 0;
            shell.Frame.Prev2D = shell.Frame.D;
            shell.Runtime.HP = 500;
            shell.Runtime.HPBound = 500;
            shell.Runtime.PP = 500;
            shell.SetRuntimeSlotIndex(shell.StableId);
            shell.RefreshRuntimeSnapshot();
            return shell;
        }

        private static void LinkCpointEntities(LF2Entity catcher, LF2Entity victim)
        {
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            catcher.FrameDelay = 0;
            victim.FrameDelay = 0;
            catcher.Runtime.CaughtDuration = 300;
        }

        private static LF2CharacterData BuildCpointMatrixVictimFrames()
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointMatrixVictim",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 10, 2, 0, 30, 60, new CatchPoint
                    {
                        kind = 2, x = 3, y = 4, hurtable = 1
                    }),
                    Frame(130, 10, 3, 130, 35, 70, new CatchPoint
                    {
                        kind = 2, x = 8, y = 12, hurtable = 1
                    }),
                    Frame(131, 10, 4, 131, 34, 69, new CatchPoint
                    {
                        kind = 2, x = 9, y = 13, hurtable = 1
                    }),
                    Frame(132, 10, 5, 132, 33, 68, new CatchPoint
                    {
                        kind = 2, x = 6, y = 10, hurtable = 1
                    }),
                    Frame(181, 11, 1, 181, 39, 79),
                    Frame(212, 5, 1, 212, 39, 79),
                },
            };
        }

        private static LF2CharacterData BuildCpointThrowFrames(int nextFrame, int victimAction, int throwInjury)
        {
            return new LF2CharacterData
            {
                name = "SelfCheck_CpointThrow",
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0, 0, 0, 39, 79),
                    Frame(110, 9, 1, nextFrame, 40, 80, new CatchPoint
                    {
                        kind = 1,
                        x = 16,
                        y = 24,
                        vaction = victimAction,
                        throwvx = 8,
                        throwvy = -4,
                        throwvz = 3,
                        throwinjury = throwInjury,
                        cover = 0,
                        hurtable = 1,
                    }),
                    Frame(112, 0, 1, 112, 39, 79),
                },
            };
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
                    Frame(212, 5, 1, 212, 39, 79),
                    Frame(100, 9, 1, 100, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, aaction = 120, taction = 121, cover = 10, hurtable = 1
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
                    Frame(121, 9, 1, 121, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, cover = 10, hurtable = 1
                    }),
                    Frame(140, 9, 1, 140, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, decrease = -5, cover = 10, hurtable = 1
                    }),
                    Frame(150, 9, 1, 150, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, dircontrol = 1, cover = 10, hurtable = 1
                    }),
                    Frame(160, 9, 1, 160, 39, 79, new CatchPoint
                    {
                        kind = 1, x = 20, y = 30, vaction = 131, jaction = 120, cover = 10, hurtable = 1
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

        private static bool Nearly(double actual, double expected)
        {
            return System.Math.Abs(actual - expected) <= 0.001;
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

        private sealed class InteractionSelfCheckCharacter : LF2Character
        {
            public override float GetSpriteWidthPxForCollision() => 100f;
        }

        private sealed class FlowSelfCheckEntity : LF2Entity
        {
            private readonly LF2ObjectType objectType;

            public override LF2ObjectType ObjectTypeEnum => objectType;

            public FlowSelfCheckEntity(LF2ObjectType objectType)
            {
                this.objectType = objectType;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)objectType;

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class BoundsSelfCheckCharacter : LF2Character
        {
            private readonly LF2ObjectType currentDataType;

            public BoundsSelfCheckCharacter(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
                ModuleInitialize();
                Controller = new SelfCheckController();
            }

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Initialize(500, 500);
                FrameDelay = 0;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)currentDataType;
        }

        private sealed class AlternateDamageSelfCheckWeapon : LF2Weapon
        {
            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(
                string name,
                int objectId,
                int weaponType,
                LF2CharacterData data,
                int frameId)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(frameId);
                Frame.PN = frameId;
                Frame.N = frameId;
                Runtime.Frame = frameId;
                Runtime.PrevFrame2 = frameId;
                Health.HP = 500;
                Health.HPBound = 500;
            }
        }

        private sealed class StageSpawnContractSelfCheckEntity : LF2Entity
        {
            private readonly LF2ObjectType objectType;

            public override LF2ObjectType ObjectTypeEnum => objectType;

            public StageSpawnContractSelfCheckEntity(LF2ObjectType objectType)
            {
                this.objectType = objectType;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class AlternateDamageSelfCheckSpecialAttack : LF2SpecialAttack
        {
            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(string name, int objectId, LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 100;
                Health.HPBound = 100;
            }
        }

        private sealed class AlternateDamageSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;
            public override float GetSpriteWidthPxForCollision() => 100f;

            public AlternateDamageSelfCheckEntity()
            {
                Name = "SelfCheck_AlternateDamageSharedVictim";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
            }

            public void BindData(int objectId, LF2CharacterData data)
            {
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class RespawnSelfCheckEffectEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            public override float GetSpriteWidthPxForCollision() => 100f;

            public RespawnSelfCheckEffectEntity()
            {
                Name = "SelfCheck_RespawnEffect998";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public void BindData(int objectId, LF2CharacterData data)
            {
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(6);
                Frame.PN = 6;
                Frame.N = 6;
                Runtime.Frame = 6;
                Runtime.PrevFrame2 = 6;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class SerialOrderSelfCheckEntity : LF2Entity
        {
            private readonly string label;
            private readonly List<string> events;

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public SerialOrderSelfCheckEntity(string label, List<string> events)
            {
                this.label = label;
                this.events = events;
                Name = $"SelfCheck_SerialOrder_{label}";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override void SimTransit(int tickIndex)
            {
                events.Add($"{label}:transit");
            }

            public override void SimTU(int tickIndex)
            {
                events.Add($"{label}:tu");
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private class TransformedLandingSelfCheckEntity : LF2OtherObject
        {
            public void BindSource(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 740;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 500;
                Health.HPBound = 500;
            }
        }

        private sealed class TransformingSimOrderSelfCheckEntity : TransformedLandingSelfCheckEntity
        {
            private readonly int targetOid;

            public int TransitDestroyCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                ObjectId == targetOid ? LF2ObjectType.LightWeapon : LF2ObjectType.Other;

            public TransformingSimOrderSelfCheckEntity(int targetOid)
            {
                this.targetOid = targetOid;
            }

            public override void OnTransitDestroy()
            {
                TransitDestroyCount++;
                UnregisterFromWorld();
            }
        }

        private sealed class TemporaryRuntimeObjectConfigs : IDisposable
        {
            private readonly GameDataManager dataManager;
            private readonly CharacterAnimtorManager animatorManager;
            private readonly System.Reflection.FieldInfo objectLookupField;
            private readonly System.Reflection.FieldInfo cachedConfigField;
            private readonly System.Reflection.FieldInfo frameConfigField;
            private readonly Dictionary<int, ObjectDefinition> originalObjectLookup;
            private readonly GameDataConfig originalCachedConfig;
            private readonly Dictionary<int, LF2CharacterDataWrapper> originalFrameConfigs;
            private readonly Dictionary<int, ObjectDefinition> replacedDefinitions = new Dictionary<int, ObjectDefinition>();
            private readonly Dictionary<int, LF2CharacterDataWrapper> replacedWrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            private readonly HashSet<int> addedDefinitions = new HashSet<int>();
            private readonly HashSet<int> addedWrappers = new HashSet<int>();

            public TemporaryRuntimeObjectConfigs(
                Dictionary<int, int> objectTypes,
                Dictionary<int, LF2CharacterDataWrapper> wrappers)
            {
                dataManager = GameDataManager.Instance;
                animatorManager = CharacterAnimtorManager.Instance;
                Expect(dataManager != null && animatorManager != null,
                    "runtime config fixture requires GameDataManager and CharacterAnimtorManager singletons");

                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                Expect(objectLookupField != null && cachedConfigField != null && frameConfigField != null,
                    "runtime config fixture reflection contract changed");

                originalObjectLookup = objectLookupField.GetValue(dataManager) as Dictionary<int, ObjectDefinition>;
                originalCachedConfig = cachedConfigField.GetValue(dataManager) as GameDataConfig;
                originalFrameConfigs = frameConfigField.GetValue(animatorManager) as Dictionary<int, LF2CharacterDataWrapper>;

                Dictionary<int, ObjectDefinition> objectLookup = originalObjectLookup ?? new Dictionary<int, ObjectDefinition>();
                Dictionary<int, LF2CharacterDataWrapper> frameConfigs =
                    originalFrameConfigs ?? new Dictionary<int, LF2CharacterDataWrapper>();
                if (originalObjectLookup == null)
                    objectLookupField.SetValue(dataManager, objectLookup);
                if (originalCachedConfig == null)
                    cachedConfigField.SetValue(dataManager, new GameDataConfig());
                if (originalFrameConfigs == null)
                    frameConfigField.SetValue(animatorManager, frameConfigs);

                foreach (KeyValuePair<int, int> pair in objectTypes)
                {
                    if (objectLookup.TryGetValue(pair.Key, out ObjectDefinition existing))
                        replacedDefinitions[pair.Key] = existing;
                    else
                        addedDefinitions.Add(pair.Key);
                    objectLookup[pair.Key] = new ObjectDefinition(pair.Key, pair.Value, "self-check.dat");
                }

                foreach (KeyValuePair<int, LF2CharacterDataWrapper> pair in wrappers)
                {
                    if (frameConfigs.TryGetValue(pair.Key, out LF2CharacterDataWrapper existing))
                        replacedWrappers[pair.Key] = existing;
                    else
                        addedWrappers.Add(pair.Key);
                    frameConfigs[pair.Key] = pair.Value;
                }
            }

            public void Dispose()
            {
                if (originalObjectLookup == null)
                {
                    objectLookupField.SetValue(dataManager, null);
                }
                else
                {
                    foreach (int oid in addedDefinitions)
                        originalObjectLookup.Remove(oid);
                    foreach (KeyValuePair<int, ObjectDefinition> pair in replacedDefinitions)
                        originalObjectLookup[pair.Key] = pair.Value;
                }
                cachedConfigField.SetValue(dataManager, originalCachedConfig);

                if (originalFrameConfigs == null)
                {
                    frameConfigField.SetValue(animatorManager, null);
                }
                else
                {
                    foreach (int oid in addedWrappers)
                        originalFrameConfigs.Remove(oid);
                    foreach (KeyValuePair<int, LF2CharacterDataWrapper> pair in replacedWrappers)
                        originalFrameConfigs[pair.Key] = pair.Value;
                }
            }
        }

        private sealed class TemporarySimulationDriverWorld : IDisposable
        {
            private readonly SimulationTickDriver driver;
            private readonly GameObject temporaryDriverObject;
            private readonly System.Reflection.FieldInfo instanceField;
            private readonly SimulationTickDriver originalDriverInstance;
            private readonly System.Reflection.FieldInfo worldField;
            private readonly SimulationWorld originalWorld;
            private readonly SimulationWorld temporaryWorld;

            public TemporarySimulationDriverWorld(SimulationWorld world)
            {
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Static |
                            System.Reflection.BindingFlags.NonPublic;
                Type singletonBaseType = typeof(SimulationTickDriver).BaseType;
                instanceField = singletonBaseType?.GetField("<Instance>k__BackingField", flags);
                originalDriverInstance = instanceField?.GetValue(null) as SimulationTickDriver;
                SimulationTickDriver resolvedDriver = SimulationTickDriver.Instance;
                if (resolvedDriver == null)
                {
                    temporaryDriverObject = new GameObject("SelfCheck_TemporarySimulationTickDriver");
                    resolvedDriver = temporaryDriverObject.AddComponent<SimulationTickDriver>();
                    var awake = singletonBaseType?.GetMethod("Awake", flags);
                    awake?.Invoke(resolvedDriver, null);
                    if (SimulationTickDriver.Instance == null)
                        instanceField?.SetValue(null, resolvedDriver);
                }
                driver = resolvedDriver;
                Expect(driver != null && SimulationTickDriver.Instance == driver,
                    "real opoint fixture failed to create its temporary SimulationTickDriver singleton");
                worldField = typeof(SimulationTickDriver).GetField(
                    "_world",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Expect(worldField != null, "real opoint fixture SimulationTickDriver._world contract changed");
                originalWorld = worldField.GetValue(driver) as SimulationWorld;
                temporaryWorld = world;
                worldField.SetValue(driver, temporaryWorld);
            }

            public void Dispose()
            {
                try
                {
                    var spawned = new List<LF2Entity>();
                    for (int slot = 0; slot < 400; slot++)
                    {
                        LF2Entity entity = temporaryWorld.FindEntityByRuntimeSlotIncludingPending(slot);
                        if (entity != null && entity.ObjectId == 999 && entity.Renderer != null)
                            spawned.Add(entity);
                    }

                    for (int i = 0; i < spawned.Count; i++)
                        spawned[i].FreeEntityLikeExe();
                }
                finally
                {
                    worldField.SetValue(driver, originalWorld);
                    if (temporaryDriverObject != null)
                    {
                        if (Application.isPlaying)
                            UnityEngine.Object.Destroy(temporaryDriverObject);
                        else
                            UnityEngine.Object.DestroyImmediate(temporaryDriverObject);
                    }
                    instanceField?.SetValue(null, originalDriverInstance);
                }
            }
        }

        private sealed class TemporaryObjectPoolInitialization : IDisposable
        {
            private readonly LF2ObjectPool pool;
            private readonly GameObject temporaryPoolObject;
            private readonly System.Reflection.FieldInfo availableField;
            private readonly System.Reflection.FieldInfo activeField;
            private readonly System.Reflection.FieldInfo releaseMapField;
            private readonly System.Reflection.FieldInfo spritePoolField;
            private readonly System.Reflection.FieldInfo cachedPrefabField;
            private readonly object originalAvailable;
            private readonly object originalActive;
            private readonly object originalReleaseMap;
            private readonly object originalSpritePool;
            private readonly object originalCachedPrefab;
            private readonly bool ownsState;

            public TemporaryObjectPoolInitialization()
            {
                LF2ObjectPool resolvedPool = LF2ObjectPool.Instance;
                if (resolvedPool == null)
                {
                    temporaryPoolObject = new GameObject("SelfCheck_TemporaryLF2ObjectPool");
                    resolvedPool = temporaryPoolObject.AddComponent<LF2ObjectPool>();
                }
                pool = resolvedPool;
                Expect(pool != null, "real opoint fixture requires an LF2ObjectPool singleton");

                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                Type type = typeof(LF2ObjectPool);
                availableField = type.GetField("_availableObjects", flags);
                activeField = type.GetField("_activeObjects", flags);
                releaseMapField = type.GetField("_releaseTimeMap", flags);
                spritePoolField = type.GetField("_spritePool", flags);
                cachedPrefabField = type.GetField("_cachedLF2ObjectPrefab", flags);
                Expect(availableField != null && activeField != null && releaseMapField != null &&
                       spritePoolField != null && cachedPrefabField != null,
                    "real opoint fixture LF2ObjectPool field contract changed");

                originalAvailable = availableField.GetValue(pool);
                originalActive = activeField.GetValue(pool);
                originalReleaseMap = releaseMapField.GetValue(pool);
                originalSpritePool = spritePoolField.GetValue(pool);
                originalCachedPrefab = cachedPrefabField.GetValue(pool);
                ownsState = originalAvailable == null || originalActive == null || originalReleaseMap == null;
                if (!ownsState)
                    return;

                availableField.SetValue(pool, new LinkedList<GameObject>());
                activeField.SetValue(pool, new HashSet<GameObject>());
                releaseMapField.SetValue(pool, new Dictionary<GameObject, float>());
                spritePoolField.SetValue(pool, new Stack<SpriteRenderer>());
                cachedPrefabField.SetValue(pool, null);
            }

            public void Dispose()
            {
                try
                {
                    if (ownsState)
                    {
                        var objects = new HashSet<GameObject>();
                        if (availableField.GetValue(pool) is LinkedList<GameObject> available)
                        {
                            foreach (GameObject item in available)
                                if (item != null) objects.Add(item);
                        }
                        if (activeField.GetValue(pool) is HashSet<GameObject> active)
                        {
                            foreach (GameObject item in active)
                                if (item != null) objects.Add(item);
                        }

                        foreach (GameObject item in objects)
                        {
                            if (Application.isPlaying)
                                UnityEngine.Object.Destroy(item);
                            else
                                UnityEngine.Object.DestroyImmediate(item);
                        }

                        availableField.SetValue(pool, originalAvailable);
                        activeField.SetValue(pool, originalActive);
                        releaseMapField.SetValue(pool, originalReleaseMap);
                        spritePoolField.SetValue(pool, originalSpritePool);
                        cachedPrefabField.SetValue(pool, originalCachedPrefab);
                    }
                }
                finally
                {
                    if (temporaryPoolObject != null)
                    {
                        if (Application.isPlaying)
                            UnityEngine.Object.Destroy(temporaryPoolObject);
                        else
                            UnityEngine.Object.DestroyImmediate(temporaryPoolObject);
                    }
                }
            }
        }

        private sealed class CurrentDatSelfCheckWeapon : LF2Weapon
        {
            private readonly LF2ObjectType currentDataType;

            public CurrentDatSelfCheckWeapon(LF2ObjectType currentDataType)
            {
                this.currentDataType = currentDataType;
            }

            public override float GetSpriteWidthPxForCollision() => 100f;

            public void BindData(
                string name,
                int objectId,
                int weaponType,
                LF2CharacterData data,
                int frameId)
            {
                Name = name;
                ObjectId = objectId;
                SetWeaponType(weaponType);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                Frame.D = FrameCache.GetFrameDataById(frameId);
                Frame.PN = frameId;
                Frame.N = frameId;
                Runtime.Frame = frameId;
                Runtime.PrevFrame2 = frameId;
                Health.HP = 500;
                Health.HPBound = 500;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)currentDataType;
        }

        private sealed class RealOpointProducerSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            internal override bool UsesDynamicRuntimeSlot() => true;

            public RealOpointProducerSelfCheckEntity(string label)
            {
                Name = $"SelfCheck_RealOpointProducer_{label}";
                ObjectId = 739;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);

                LF2FrameData frame = Frame(0, LF2States.Standing, 100, 0, 39, 79);
                frame.opoint = new ObjectPoint { kind = 1, oid = 999, action = 0, facing = 0 };
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, new LF2CharacterData
                {
                    name = Name,
                    frames = new List<LF2FrameData> { frame },
                }));
                Frame.D = frame;
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                AttackingCounter = 1;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class DynamicSlotSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            internal override bool UsesDynamicRuntimeSlot() => true;

            public DynamicSlotSelfCheckEntity(int stableId)
            {
                StableId = stableId;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class QueuedBoundarySelfCheckEntity : LF2Entity
        {
            public enum Phase
            {
                FrameLogic,
                ObserveFrameLogic,
            }

            private readonly Phase phase;
            private readonly ReleaseSpawnSemantic[] semantics;

            public int EnqueueCount { get; private set; }
            public int QueueCountObservedAtFrameLogic { get; private set; } = -1;
            public OPointCreateTask LastTask { get; private set; }
            public List<OPointCreateTask> PublishedTasks { get; } = new List<OPointCreateTask>();
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public QueuedBoundarySelfCheckEntity(Phase phase, params ReleaseSpawnSemantic[] semantics)
            {
                this.phase = phase;
                this.semantics = semantics ?? Array.Empty<ReleaseSpawnSemantic>();
                Name = $"SelfCheck_QueuedBoundary_{phase}";
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);

                var data = new LF2CharacterData
                {
                    name = Name,
                    frames = new List<LF2FrameData>
                    {
                        new LF2FrameData
                        {
                            frameId = 0,
                            state = LF2States.Standing,
                            wait = 1,
                            next = 0,
                            hit_Fa = 5,
                        },
                    },
                };
                FrameCache.Load(new LF2CharacterDataWrapper(0, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
            {
                return phase == Phase.FrameLogic || phase == Phase.ObserveFrameLogic;
            }

            public override void RunFrameLogicBeforeAdvance()
            {
                if (phase == Phase.ObserveFrameLogic)
                {
                    QueueCountObservedAtFrameLogic = GetQueuedObjectPointTaskCount(LF2ObjectPointFactory.Instance);
                    return;
                }

                if (phase == Phase.FrameLogic)
                    Publish(semantics.Length > 0 ? semantics[0] : ReleaseSpawnSemantic.ImmediateEffect);
            }

            private void Publish(ReleaseSpawnSemantic semantic)
            {
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint { oid = -700 - EnqueueCount, kind = 0, action = 0 };
                task.parent = this;
                task.team = Team;
                task.releaseSpawnSemantic = semantic;
                LF2ObjectPointFactory.Instance.EnqueueCreateObject(task);
                LastTask = task;
                PublishedTasks.Add(task);
                EnqueueCount++;
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class QueuedBoundarySelfCheckWeapon : LF2Weapon
        {
            public int TransitDestroyCount { get; private set; }
            public bool PendingDestroyObserved { get; private set; }

            public void BindData(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 100;
                SetWeaponType((int)LF2ObjectType.LightWeapon);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 1;
                Health.HPBound = 1;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                bool completed = base.TryRunLatePostOpointCleanupPhase();
                PendingDestroyObserved |= Runtime.PendingFlushDestroy;
                return completed;
            }

            public override void OnTransitDestroy()
            {
                TransitDestroyCount++;
                UnregisterFromWorld();
            }
        }

        private sealed class QueuedBoundaryTransitionSelfCheckEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public QueuedBoundaryTransitionSelfCheckEntity(LF2CharacterData data)
            {
                Name = data.name;
                ObjectId = 700;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(ObjectId, data));
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 10;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.SetPosition(100, -20, 100);
                Runtime.SyncIntegerPosition();
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class MutationSelfCheckEntity : LF2Entity
        {
            private readonly bool _registerDuringLate;
            private readonly bool _unregisterDuringLate;

            public MutationSelfCheckEntity Spawned { get; private set; }
            public int LateTickCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            public MutationSelfCheckEntity(int stableId, bool registerDuringLate = false, bool unregisterDuringLate = false)
            {
                StableId = stableId;
                _registerDuringLate = registerDuringLate;
                _unregisterDuringLate = unregisterDuringLate;
            }

            public override void SimFrameTick(int tickIndex)
            {
                LateTickCount++;

                if (_registerDuringLate && Spawned == null)
                {
                    Spawned = new MutationSelfCheckEntity(1000 + StableId);
                    Match.Register(Spawned);
                }

                if (_unregisterDuringLate && LateTickCount == 1)
                    Match.Unregister(this);
            }

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class SelfCheckCharacterDatShell : LF2SpecialAttack
        {
            public void InitializeForCpoint()
            {
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
            }

            public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Character;
            public override void Reset() { }
        }
    }
}
