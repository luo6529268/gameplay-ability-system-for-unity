#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B5")]
    public sealed class NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests
    {
        [Test]
        public void Type3AndWeaponDamage_PreserveLegacyStatSentinels()
        {
            AssertDamageCase(LF2ObjectType.SpecialAttack);
            AssertDamageCase(LF2ObjectType.Other);
            AssertDamageCase(LF2ObjectType.LightWeapon);
            AssertDamageCase(LF2ObjectType.HeavyWeapon);
            AssertDamageCase(LF2ObjectType.ThrowWeapon);
        }

        [Test]
        public void FluteConcreteAndShared_PreserveLegacyStatSentinels()
        {
            AssertFluteCase(sharedResolver: false);
            AssertFluteCase(sharedResolver: true);
        }

        [Test]
        public void FluteHitPlan_PreservesLegacyStatSentinels()
        {
            new NTSD.Test.BattleHitExecutionPlanEditorTests()
                .ShadowCompare_Kind10CharacterWriterEffectMatchesAuthorityState();
        }

        [Test]
        public void ProductionSources_ContainNoScopedLegacyStatWriter()
        {
            string damage = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs");
            string hitPlan = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs");
            string concrete = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs");
            string shared = Source(
                "Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs");
            string cpoint = Source(
                "Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs");

            string type3Actual = Slice(
                damage,
                "private static void ApplyType3NormalVitalAndStatWrites(",
                "private static void ApplyWeaponNormalVitalAndStatWrites(");
            string weaponActual = Slice(
                damage,
                "private static void ApplyWeaponNormalVitalAndStatWrites(",
                "private static void ApplySpecialObjectHurtTail(");
            string type3Plan = Slice(
                hitPlan,
                "private static void ProjectType3NormalVitalAndStatWrites(",
                "private static void ProjectWeaponNormalVitalAndStatWrites(");
            string weaponPlan = Slice(
                hitPlan,
                "private static void ProjectWeaponNormalVitalAndStatWrites(",
                "private static void ProjectNativeStandardHitCreditAndConsume(");
            string flutePlan = Slice(
                hitPlan,
                "private static bool ProjectNativeImpactWriterEffect(",
                "private static bool ProjectKind15WriterEffect(");

            StringAssert.DoesNotContain("ComboCountVic", type3Actual);
            StringAssert.DoesNotContain("DamageStats", type3Actual);
            StringAssert.DoesNotContain("ComboCountVic", weaponActual);
            StringAssert.DoesNotContain("DamageStats", weaponActual);
            StringAssert.DoesNotContain("TargetComboCountVic", type3Plan);
            StringAssert.DoesNotContain("TargetDamageStat", type3Plan);
            StringAssert.DoesNotContain("TargetComboCountVic", weaponPlan);
            StringAssert.DoesNotContain("TargetDamageStat", weaponPlan);
            StringAssert.DoesNotContain("HolderComboCountAtk", flutePlan);
            StringAssert.DoesNotContain("TargetDamageStat", flutePlan);
            StringAssert.DoesNotContain("ComboCountAtk += 11", concrete);
            StringAssert.DoesNotContain("DamageStats[damageStatIndex] += 11", concrete);
            StringAssert.DoesNotContain("ComboCountAtk += 11", shared);
            StringAssert.DoesNotContain("DamageStats[damageStatIndex] += 11", shared);

            StringAssert.Contains("victim.ComboCountVic += injury", damage,
                "Standard legacy writer is outside this package.");
            StringAssert.DoesNotContain("victim.ComboCountVic", cpoint,
                "Held CPoint legacy victim combo writer must stay retired.");
            StringAssert.DoesNotContain("world.DamageStats", cpoint,
                "Held CPoint legacy world damage writer must stay retired.");
            StringAssert.DoesNotContain("holder.KillStat", cpoint,
                "Held CPoint legacy holder kill writer must stay retired.");
        }

        private static void AssertDamageCase(LF2ObjectType targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = Entity(world, 9940, 0, LF2ObjectType.Character);
            TypedCharacter target = Entity(world, 9941 + (int)targetType, 1, targetType);
            target.ComboCountVic = 31;
            target.Runtime.InputHpConsumedTotal34C = 5;
            target.Unk344 = 1;
            world.DamageStats[1] = 41;

            bool applied = targetType == LF2ObjectType.LightWeapon ||
                           targetType == LF2ObjectType.HeavyWeapon ||
                           targetType == LF2ObjectType.ThrowWeapon
                ? world.DamageWriter.ApplyWeaponDamage(
                    world,
                    attacker,
                    target,
                    DamageInteraction())
                : world.DamageWriter.ApplySpecialAttackDamage(
                    world,
                    attacker,
                    target,
                    DamageInteraction());

            Assert.That(applied, Is.True, targetType.ToString());
            Assert.That(target.Runtime.InputHpConsumedTotal34C,
                Is.GreaterThan(5), targetType.ToString());
            Assert.That(target.ComboCountVic, Is.EqualTo(31), targetType.ToString());
            Assert.That(world.DamageStats[1], Is.EqualTo(41), targetType.ToString());
        }

        private static void AssertFluteCase(bool sharedResolver)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = Entity(world, 9960, 0, LF2ObjectType.Character);
            TypedCharacter target = Entity(world, 9961, 1, LF2ObjectType.Character);
            TypedCharacter holder = Entity(world, 9962, 2, LF2ObjectType.Character);
            holder.ComboCountAtk = 41;
            target.KillCount = -1;
            target.Unk344 = 1;
            target.WeaponCount = 5;
            target.Runtime.SetVelocity(4.25, -2.5, -3.75);
            world.DamageStats[1] = 51;
            attacker.Runtime.OwnerSlotIndex = holder.Runtime.SlotIndex;
            holder.Runtime.OwnerSlotIndex = holder.Runtime.SlotIndex;
            var interaction = new InteractionArea { kind = 10 };

            bool applied = sharedResolver
                ? LF2CharacterDatHitResolver.TryResolveHit(
                    target,
                    interaction,
                    attacker,
                    Vector3.zero,
                    default)
                : target.Hit(interaction, attacker, Vector3.zero, default);

            Assert.That(applied, Is.True);
            Assert.That(target.WeaponCount, Is.EqualTo(5));
            Assert.That(target.Runtime.EnvironmentState320, Is.EqualTo(-20));
            Assert.That(target.Runtime.CatchSourceSlot90, Is.EqualTo(0x2000 + holder.Runtime.SlotIndex));
            Assert.That(target.Frame.N, Is.EqualTo(182));
            Assert.That(holder.ComboCountAtk, Is.EqualTo(41));
            Assert.That(world.DamageStats[1], Is.EqualTo(51));
        }

        private static TypedCharacter Entity(
            SimulationWorld world,
            int oid,
            int slot,
            LF2ObjectType type)
        {
            var frames = new List<LF2FrameData>(241);
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = LF2States.Standing,
                    wait = 100,
                    next = id,
                    pic = 999,
                });
            }
            var entity = new TypedCharacter(type)
            {
                Name = $"LegacyStatRetirement{oid}",
                ObjectId = oid,
            };
            entity.ModuleInitialize();
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                oid,
                new LF2CharacterData
                {
                    name = entity.Name,
                    type_sub = (int)type,
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Initialize(500, 500);
            entity.Team = slot + 1;
            entity.RelationTeam = slot + 1;
            entity.Unk344 = 1;
            entity.Runtime.SetPosition(slot * 20, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            world.Register(entity);
            return entity;
        }

        private static InteractionArea DamageInteraction()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 10,
                fall = 1,
                dvx = 1,
                arest = 10,
                vrest = 1,
            };
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
            int endIndex = source.IndexOf(end, startIndex + start.Length,
                StringComparison.Ordinal);
            Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), start);
            Assert.That(endIndex, Is.GreaterThan(startIndex), end);
            return source.Substring(startIndex, endIndex - startIndex);
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType type;

            internal TypedCharacter(LF2ObjectType type)
            {
                this.type = type;
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)type;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B5Type3WeaponFluteLegacyStatRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Type3WeaponFluteLegacyStat-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Type3WeaponFluteLegacyStat-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(4096);
        private static NTSD28B5Type3WeaponFluteLegacyStatRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B5Type3WeaponFluteLegacyStatRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
                return;
            if (File.Exists(ResultPath)) File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks = new NTSD28B5Type3WeaponFluteLegacyStatRequestRunner();
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
    internal static class NTSD28B5Type3WeaponFluteLegacyStatPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B5-Type3WeaponFluteLegacyStat-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B5-Type3WeaponFluteLegacyStat-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B5Type3WeaponFluteLegacyStatPlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
                return;
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
                var tests = new NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests();
                tests.Type3AndWeaponDamage_PreserveLegacyStatSentinels();
                tests.FluteConcreteAndShared_PreserveLegacyStatSentinels();
                tests.FluteHitPlan_PreservesLegacyStatSentinels();
                tests.ProductionSources_ContainNoScopedLegacyStatWriter();
                File.WriteAllText(ResultPath,
                    "state=Passed\ncases=4\n" +
                    "targets=type3,other,weapon1,weapon2,weapon4\n" +
                    "flute=concrete,shared,hitPlan\nlegacyStats=preserved\n" +
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
