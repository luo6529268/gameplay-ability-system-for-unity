#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B5")]
    public sealed class NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests
    {
        [TestCase(BattleEcsCharacterRecoveryPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterRecoveryPassMode.Legacy)]
        public void RegisteredProfiles_UseRuleScaleTwoHopsAndIgnoreStepWait(
            BattleEcsCharacterRecoveryPassMode mode)
        {
            SimulationWorld world = World(mode);
            LF2Character victim = Character(world, 0, 9970);
            LF2Character root = Character(world, 1, 9971);
            LF2Character firstOwner = Character(world, 2, 9972);
            LF2Character secondOwner = Character(world, 3, 9973);
            LF2Character thirdOwner = Character(world, 4, 9974);
            root.Runtime.OwnerSlotIndex = 2;
            firstOwner.Runtime.OwnerSlotIndex = 3;
            secondOwner.Runtime.OwnerSlotIndex = 4;
            ArrangeVictim(victim, hp: 120, hpBound: 120);
            victim.Runtime.CatchSourceSlot90 = 0x2001;
            victim.Runtime.IncomingDamageScale340 = 150;
            victim.Runtime.InputHpConsumedTotal34C = 5;
            victim.ComboCountVic = 31;
            victim.KillStat = 33;
            secondOwner.Runtime.InputScoreTotal348 = 7;
            secondOwner.Runtime.KnockoutCount358 = 11;
            thirdOwner.Runtime.InputScoreTotal348 = 13;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 12;
            world.Runtime.Flow.BattleStepMode = 1;
            world.Runtime.Flow.BattleStepGate = 0;
            world.DamageStats[1] = 41;
            world.KillStats[1] = 43;

            world.LateEntityUpdateAll(1);

            Assert.That(victim.Health.HP, Is.EqualTo(114), mode.ToString());
            Assert.That(victim.Health.HPBound, Is.EqualTo(118), mode.ToString());
            Assert.That(victim.Runtime.InputHpConsumedTotal34C,
                Is.EqualTo(17), mode.ToString());
            Assert.That(secondOwner.Runtime.InputScoreTotal348,
                Is.EqualTo(13), mode.ToString());
            Assert.That(secondOwner.Runtime.KnockoutCount358,
                Is.EqualTo(11), mode.ToString());
            Assert.That(thirdOwner.Runtime.InputScoreTotal348,
                Is.EqualTo(13), mode.ToString());
            AssertPreservedSentinels(victim, world, mode.ToString());
        }

        [Test]
        public void DerivedCompatibility_UsesNativePhaseNotTickOrStepWait()
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            DerivedCharacter victim = Character<DerivedCharacter>(
                world, 0, 9975);
            LF2Character credit = Character(world, 1, 9976);
            ArrangeVictim(victim, hp: 30, hpBound: 30);
            victim.Runtime.CatchSourceSlot90 = 1;
            victim.Runtime.InputHpConsumedTotal34C = 4;
            credit.Runtime.InputScoreTotal348 = 6;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 7;
            world.Runtime.Flow.BattleStepMode = 1;
            world.Runtime.Flow.BattleStepGate = 0;

            world.LateEntityUpdateAll(1);

            Assert.That(victim.Health.HP, Is.EqualTo(23));
            Assert.That(victim.Health.HPBound, Is.EqualTo(28));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(11));
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(13));
            Assert.That(
                world.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(-4)]
        public void NonPositiveRuleFallsBackToNineAndMissingCreditStillDamages(
            int rawRule)
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character victim = Character(world, 0, 9977);
            ArrangeVictim(victim, hp: 5, hpBound: 5);
            victim.Runtime.CatchSourceSlot90 = -1;
            victim.Runtime.InputHpConsumedTotal34C = 3;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 =
                rawRule;

            world.LateEntityUpdateAll(1);

            Assert.That(victim.Health.HP, Is.Zero);
            Assert.That(victim.Health.HPBound, Is.EqualTo(2));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(12));
            Assert.That(victim.Runtime.EnvironmentState320, Is.EqualTo(-1));
        }

        [Test]
        public void LethalSlotReuseCreditsCurrentOccupantAndClampsAfterAccounting()
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character victim = Character(world, 0, 9978);
            LF2Character released = Character(world, 1, 9979);
            released.Runtime.InputScoreTotal348 = 101;
            released.Runtime.KnockoutCount358 = 103;
            world.Unregister(released);
            LF2Character replacement = Character(world, 1, 9980);
            LF2Character owner = Character(world, 2, 9981);
            replacement.Runtime.OwnerSlotIndex = 2;
            replacement.Runtime.InputScoreTotal348 = 11;
            replacement.Runtime.KnockoutCount358 = 13;
            owner.Runtime.InputScoreTotal348 = 17;
            owner.Runtime.KnockoutCount358 = 19;
            ArrangeVictim(victim, hp: 5, hpBound: 5);
            victim.Runtime.CatchSourceSlot90 = 0x2001;
            victim.Runtime.ImpactSourceSlot164 = 2;
            victim.Runtime.InputHpConsumedTotal34C = 23;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 9;

            world.LateEntityUpdateAll(1);

            Assert.That(victim.Health.HP, Is.Zero);
            Assert.That(victim.Health.HPBound, Is.EqualTo(2));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(32));
            Assert.That(owner.Runtime.InputScoreTotal348, Is.EqualTo(26));
            Assert.That(owner.Runtime.KnockoutCount358, Is.EqualTo(20));
            Assert.That(replacement.Runtime.InputScoreTotal348, Is.EqualTo(11));
            Assert.That(replacement.Runtime.KnockoutCount358, Is.EqualTo(13));
            Assert.That(released.Runtime.InputScoreTotal348, Is.EqualTo(101));
            Assert.That(released.Runtime.KnockoutCount358, Is.EqualTo(103));
            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1));
            NativeKnockoutEvent knockout = world.NativeKnockoutEvents[0];
            Assert.That(knockout.VictimSlot, Is.EqualTo(0));
            Assert.That(knockout.SourceSlot, Is.EqualTo(2));
            Assert.That(knockout.CreditSlot, Is.EqualTo(2));
            Assert.That(knockout.FourOwnerSlot, Is.EqualTo(2));
        }

        [Test]
        public void LethalMissingImpactSourceStillRecordsCreditedEvent()
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character victim = Character(world, 0, 9988);
            LF2Character credit = Character(world, 1, 9989);
            ArrangeVictim(victim, hp: 5, hpBound: 5);
            victim.Runtime.CatchSourceSlot90 = 1;
            victim.Runtime.ImpactSourceSlot164 = -1;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 9;

            world.LateEntityUpdateAll(1);

            Assert.That(credit.Runtime.KnockoutCount358, Is.EqualTo(1));
            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1));
            NativeKnockoutEvent knockout = world.NativeKnockoutEvents[0];
            Assert.That(knockout.SourceSlot, Is.EqualTo(1000));
            Assert.That(knockout.SourceObjectType, Is.EqualTo(-1));
            Assert.That(knockout.FourOwnerSlot, Is.EqualTo(1000));
            Assert.That(knockout.VictimSlot, Is.EqualTo(0));
            Assert.That(knockout.CreditSlot, Is.EqualTo(1));
        }

        [Test]
        public void ScaleCanProduceZeroActualWhileRuleAccountingStillAdvances()
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character victim = Character(world, 0, 9982);
            LF2Character credit = Character(world, 1, 9983);
            ArrangeVictim(victim, hp: 50, hpBound: 50);
            victim.Runtime.CatchSourceSlot90 = 1;
            victim.Runtime.IncomingDamageScale340 = 1800;
            victim.Runtime.InputHpConsumedTotal34C = 2;
            credit.Runtime.InputScoreTotal348 = 3;
            credit.Runtime.KnockoutCount358 = 5;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 12;

            world.LateEntityUpdateAll(1);

            Assert.That(victim.Health.HP, Is.EqualTo(50));
            Assert.That(victim.Health.HPBound, Is.EqualTo(50));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(14));
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(3));
            Assert.That(credit.Runtime.KnockoutCount358, Is.EqualTo(5));
            Assert.That(world.NativeKnockoutEvents, Is.Empty);
        }

        [TestCase(BattleEcsCharacterRecoveryPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterRecoveryPassMode.Legacy)]
        public void FluteWeaponCountWithoutNegativeEnvironmentIsInert(
            BattleEcsCharacterRecoveryPassMode mode)
        {
            SimulationWorld world = World(mode);
            LF2Character victim = Character(world, 0, 9984);
            victim.Health.HP = 100;
            victim.Health.HPBound = 100;
            victim.Health.PP = 500;
            victim.WeaponCount = -20;
            victim.FallDamageDiv = 25;
            victim.Runtime.EnvironmentState320 = 0;
            victim.Runtime.InputHpConsumedTotal34C = 7;
            victim.ComboCountVic = 29;

            world.LateEntityUpdateAll(12);

            Assert.That(victim.Health.HP, Is.EqualTo(100), mode.ToString());
            Assert.That(victim.Health.HPBound, Is.EqualTo(100), mode.ToString());
            Assert.That(victim.Runtime.InputHpConsumedTotal34C,
                Is.EqualTo(7), mode.ToString());
            Assert.That(victim.ComboCountVic, Is.EqualTo(29), mode.ToString());
            Assert.That(victim.WeaponCount, Is.EqualTo(-20), mode.ToString());
            Assert.That(victim.FallDamageDiv, Is.EqualTo(25), mode.ToString());
        }

        [Test]
        public void PositiveEnvironmentOrNonzeroPhaseIsInert()
        {
            SimulationWorld positiveWorld = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character positive = Character(positiveWorld, 0, 9985);
            ArrangeVictim(positive, hp: 50, hpBound: 50);
            positive.Runtime.EnvironmentState320 = 1;
            positiveWorld.LateEntityUpdateAll(1);

            SimulationWorld phaseWorld = World(
                BattleEcsCharacterRecoveryPassMode.DataOriented);
            LF2Character wrongPhase = Character(phaseWorld, 0, 9986);
            ArrangeVictim(wrongPhase, hp: 50, hpBound: 50);
            phaseWorld.Runtime.NativeWorldClock.ResourcePhase12 = 1;
            phaseWorld.Runtime.NativeWorldClock.ResourcePhase3 = 1;
            phaseWorld.LateEntityUpdateAll(1);

            Assert.That(positive.Health.HP, Is.EqualTo(50));
            Assert.That(wrongPhase.Health.HP, Is.EqualTo(50));
            Assert.That(
                phaseWorld.BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics
                    .ProvenNoOpCount,
                Is.EqualTo(1));
        }

        [Test]
        public void ProductionSourcesContainOneSharedTransactionAndNoOldBranches()
        {
            string writer = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNegativeEnvironmentRecoveryWriter.cs");
            string entity = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs");
            string recovery = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs");
            string entityMethod = Slice(
                entity,
                "internal virtual void RunPreCollisionRecoveryPhase(int tickIndex)",
                "internal virtual void RunHumanInputPollPhase(int tickIndex)");
            string recoveryMethod = Slice(
                recovery,
                "private void ApplyAuthorityRecovery(",
                "private void ResetDiagnostics()");

            Assert.That(Count(writer, "runtime.HP = Math.Max"), Is.EqualTo(1));
            Assert.That(Count(writer,
                "runtime.InputHpConsumedTotal34C = unchecked("), Is.EqualTo(1));
            Assert.That(Count(writer,
                "credit.InputScoreTotal348 = unchecked("), Is.EqualTo(1));
            Assert.That(Count(entityMethod,
                "BattleNegativeEnvironmentRecoveryWriter.Apply"), Is.EqualTo(1));
            Assert.That(Count(recoveryMethod,
                "BattleNegativeEnvironmentRecoveryWriter.Apply"), Is.EqualTo(1));
            StringAssert.DoesNotContain("WeaponCount < 0", entityMethod);
            StringAssert.DoesNotContain("FallDamageDiv", entityMethod);
            StringAssert.DoesNotContain("ComboCountVic += 9", entityMethod);
            StringAssert.DoesNotContain("WeaponCount < 0", recoveryMethod);
            StringAssert.DoesNotContain("FallDamageDiv", recoveryMethod);
            StringAssert.DoesNotContain("ComboCountVic += 9", recoveryMethod);
        }

        [Test]
        public void WarmLegacyTransactionAllocatesZeroManagedBytes()
        {
            SimulationWorld world = World(
                BattleEcsCharacterRecoveryPassMode.Legacy);
            LF2Character victim = Character(world, 0, 9987);
            LF2Character credit = Character(world, 1, 9988);
            victim.Runtime.CatchSourceSlot90 = 1;
            victim.Runtime.EnvironmentState320 = -1;
            victim.Health.PP = 500;
            world.Runtime.NativeHitResourceRules.NegativeEnvironmentDamage90 = 9;
            victim.RunPreCollisionRecoveryPhase(1);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                victim.Health.HP = 100;
                victim.Health.HPBound = 100;
                victim.Runtime.InputHpConsumedTotal34C = 0;
                credit.Runtime.InputScoreTotal348 = 0;
                victim.RunPreCollisionRecoveryPhase(1);
                checksum ^= victim.Health.HP;
                checksum ^= victim.Runtime.InputHpConsumedTotal34C;
                checksum ^= credit.Runtime.InputScoreTotal348;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Zero);
            Assert.That(victim.Health.HP, Is.EqualTo(91));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(9));
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(9));
            Assert.That(allocated, Is.Zero);
        }

        private static SimulationWorld World(
            BattleEcsCharacterRecoveryPassMode mode)
        {
            var world = new SimulationWorld();
            world.ConfigureBattleEcsCharacterRecoveryPassForDiagnostics(mode);
            world.Runtime.NativeWorldClock.ResourcePhase12 = 0;
            return world;
        }

        private static LF2Character Character(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            return Character<LF2Character>(world, slot, objectId);
        }

        private static T Character<T>(
            SimulationWorld world,
            int slot,
            int objectId)
            where T : LF2Character, new()
        {
            var data = new LF2CharacterData
            {
                name = $"NegativeEnvironment_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            var entity = new T
            {
                Name = data.name,
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Initialize(500, 500);
            entity.Team = 1;
            entity.RelationTeam = 1;
            entity.Runtime.SetPosition(slot * 10.0, 0, 200);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SuppressLateFrameTickUntilTick = int.MaxValue;
            world.Register(entity);
            return entity;
        }

        private static void ArrangeVictim(
            LF2Character victim,
            int hp,
            int hpBound)
        {
            victim.Health.HP = hp;
            victim.Health.HPBound = hpBound;
            victim.Health.PP = 500;
            victim.Runtime.EnvironmentState320 = -1;
            victim.WeaponCount = -20;
            victim.FallDamageDiv = 37;
        }

        private static void AssertPreservedSentinels(
            LF2Character victim,
            SimulationWorld world,
            string label)
        {
            Assert.That(victim.Runtime.EnvironmentState320,
                Is.EqualTo(-1), label);
            Assert.That(victim.WeaponCount, Is.EqualTo(-20), label);
            Assert.That(victim.FallDamageDiv, Is.EqualTo(37), label);
            Assert.That(victim.ComboCountVic, Is.EqualTo(31), label);
            Assert.That(victim.KillStat, Is.EqualTo(33), label);
            Assert.That(world.DamageStats[1], Is.EqualTo(41), label);
            Assert.That(world.KillStats[1], Is.EqualTo(43), label);
        }

        private static string Source(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return File.ReadAllText(Path.GetFullPath(
                Path.Combine(root ?? string.Empty, relativePath)));
        }

        private static string Slice(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start, StringComparison.Ordinal);
            int endIndex = source.IndexOf(
                end,
                startIndex + start.Length,
                StringComparison.Ordinal);
            Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), start);
            Assert.That(endIndex, Is.GreaterThan(startIndex), end);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(
                       token,
                       offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }
            return count;
        }

        private sealed class DerivedCharacter : LF2Character
        {
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B5NegativeEnvironmentRecoveryRequestRunner :
        ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-NegativeEnvironmentRecovery-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-NegativeEnvironmentRecovery-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B5NegativeEnvironmentRecoveryRequestRunner
            activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B5NegativeEnvironmentRecoveryRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks =
                new NTSD28B5NegativeEnvironmentRecoveryRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[] { FocusedTestClass },
            }) { runSynchronously = false });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text = $"state={result.ResultState}\npassed={result.PassCount}\n" +
                $"failed={result.FailCount}\nskipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\nmessage={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));
            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;
            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28B5NegativeEnvironmentRecoveryPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-NegativeEnvironmentRecovery-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-NegativeEnvironmentRecovery-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B5NegativeEnvironmentRecoveryPlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            running = true;
            File.Delete(RequestPath);
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            try
            {
                var tests =
                    new NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests();
                tests.RegisteredProfiles_UseRuleScaleTwoHopsAndIgnoreStepWait(
                    BattleEcsCharacterRecoveryPassMode.DataOriented);
                tests.RegisteredProfiles_UseRuleScaleTwoHopsAndIgnoreStepWait(
                    BattleEcsCharacterRecoveryPassMode.Legacy);
                tests.DerivedCompatibility_UsesNativePhaseNotTickOrStepWait();
                tests.NonPositiveRuleFallsBackToNineAndMissingCreditStillDamages(0);
                tests.NonPositiveRuleFallsBackToNineAndMissingCreditStillDamages(-4);
                tests.LethalSlotReuseCreditsCurrentOccupantAndClampsAfterAccounting();
                tests.ScaleCanProduceZeroActualWhileRuleAccountingStillAdvances();
                tests.FluteWeaponCountWithoutNegativeEnvironmentIsInert(
                    BattleEcsCharacterRecoveryPassMode.DataOriented);
                tests.FluteWeaponCountWithoutNegativeEnvironmentIsInert(
                    BattleEcsCharacterRecoveryPassMode.Legacy);
                tests.PositiveEnvironmentOrNonzeroPhaseIsInert();
                tests.ProductionSourcesContainOneSharedTransactionAndNoOldBranches();
                tests.WarmLegacyTransactionAllocatesZeroManagedBytes();
                File.WriteAllText(ResultPath,
                    "state=Passed\ncases=12\n" +
                    "profiles=DataOriented,Legacy,derived\n" +
                    "ruleScaleOwnerClamp=exact\nfluteFalsePositive=absent\n" +
                    "sceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(ResultPath, "state=Failed\n" + exception,
                    new UTF8Encoding(false));
            }
            finally
            {
                EditorApplication.delayCall += ExitPlayMode;
            }
        }

        private static void ExitPlayMode()
        {
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            running = false;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
