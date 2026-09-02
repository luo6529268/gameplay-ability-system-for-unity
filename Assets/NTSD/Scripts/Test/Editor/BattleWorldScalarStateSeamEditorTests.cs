#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace NTSD.Test
{
    [Category("BattleWorldScalarStateSeam")]
    public sealed class BattleWorldScalarStateSeamEditorTests
    {
        private static readonly string[] SelectedTypeNames =
        {
            nameof(BattleMatchRuntimeState),
            nameof(BattleStageRuntimeState),
            nameof(BattleStageProgressionState),
            nameof(BattleSlotRuntimeState),
            nameof(BattleFlowRuntimeState),
        };

        [Test]
        public void SelectedTypesHaveOnePureSharedPackageOwnerSource()
        {
            string simulationPath = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Simulation");
            string oldOwnerPath = Path.Combine(simulationPath, "BattleWorldScalarState.cs");
            string mixedPath = Path.Combine(
                simulationPath,
                "Runtime",
                "BattleRuntimeState.cs");
            PackageInfo packageInfo = PackageInfo.FindForAssembly(
                typeof(BattleMatchRuntimeState).Assembly);
            Assert.That(packageInfo, Is.Not.Null);
            Assert.That(packageInfo.name, Is.EqualTo("com.ntsd.battle-kernel"));
            Assert.That(packageInfo.version, Is.EqualTo("0.6.0"));
            string ownerPath = Path.Combine(
                packageInfo.resolvedPath,
                "Runtime",
                "Core",
                "BattleWorldScalarState.cs");
            string rosterLabelPath = Path.Combine(
                packageInfo.resolvedPath,
                "Runtime",
                "Core",
                "BattleRosterLabelState.cs");

            Assert.That(File.Exists(oldOwnerPath), Is.False,
                "The superseded Client scalar owner source still exists.");
            Assert.That(File.Exists(ownerPath), Is.True,
                "The shared package scalar owner source does not exist.");
            Assert.That(
                typeof(BattleMatchRuntimeState).Assembly.GetName().Name,
                Is.EqualTo("NTSD.Battle.Kernel"));
            string owner = File.ReadAllText(ownerPath);
            string mixed = File.ReadAllText(mixedPath);
            string rosterLabel = File.ReadAllText(rosterLabelPath);
            for (int index = 0; index < SelectedTypeNames.Length; index++)
            {
                string declaration = "public sealed class " + SelectedTypeNames[index];
                Assert.That(owner, Does.Contain(declaration));
                Assert.That(mixed, Does.Not.Contain(declaration));
            }

            Assert.That(owner, Does.Not.Contain("using NTSD.App"));
            Assert.That(owner, Does.Not.Contain("UnityEngine"));
            Assert.That(owner, Does.Not.Contain("MatchConfig"));
            Assert.That(owner, Does.Not.Contain("StageSpawnRuntimeBufferPool"));
            Assert.That(rosterLabel, Does.Contain("public sealed class BattleRosterRuntimeState"));
            Assert.That(mixed, Does.Not.Contain("public sealed class BattleRosterRuntimeState"));
            Assert.That(mixed, Does.Contain("public sealed class BattleResultsRuntimeState"));
            Assert.That(mixed, Does.Contain("public sealed class BattleRuntimeState"));
        }

        [Test]
        public void MatchProgressionAndSlotDefaultsAndResetRemainFrozen()
        {
            var match = new BattleMatchRuntimeState();
            Assert.That(match.BackgroundId, Is.EqualTo(-1));
            Assert.That(match.Difficulty, Is.EqualTo(2));
            Assert.That(match.PpMode, Is.True);
            match.LocalGameModeId = 7;
            match.BattleGameModeId = 8;
            match.BackgroundId = 9;
            match.Difficulty = -1;
            match.StageIdx = 4;
            match.RandomStage = 1;
            match.RuntimeStageCount = 12;
            match.Seed = 99;
            match.PpMode = false;
            match.Reset();
            Assert.That(match.LocalGameModeId, Is.Zero);
            Assert.That(match.BattleGameModeId, Is.Zero);
            Assert.That(match.BackgroundId, Is.EqualTo(-1));
            Assert.That(match.Difficulty, Is.EqualTo(2));
            Assert.That(match.StageIdx, Is.Zero);
            Assert.That(match.RandomStage, Is.Zero);
            Assert.That(match.RuntimeStageCount, Is.Zero);
            Assert.That(match.Seed, Is.Zero);
            Assert.That(match.PpMode, Is.True);

            var progression = new BattleStageProgressionState
            {
                StageSeriesIdx = 41,
                WaveIdx = 3,
                Round = 2,
                RoundMax = 9,
            };
            progression.Reset();
            Assert.That(progression.StageSeriesIdx, Is.Zero);
            Assert.That(progression.WaveIdx, Is.EqualTo(-1));
            Assert.That(progression.Round, Is.Zero);
            Assert.That(progression.RoundMax, Is.Zero);

            var slot = new BattleSlotRuntimeState
            {
                Active = true,
                IsHuman = true,
                CharacterId = 12,
                Team = 3,
                InputId = 4,
                AiId = 5,
                RuntimeSlotIndex = 20,
                StableId = 77,
            };
            slot.Reset();
            Assert.That(slot.Active, Is.False);
            Assert.That(slot.IsHuman, Is.False);
            Assert.That(slot.CharacterId, Is.EqualTo(-1));
            Assert.That(slot.Team, Is.Zero);
            Assert.That(slot.InputId, Is.Zero);
            Assert.That(slot.AiId, Is.EqualTo(-1));
            Assert.That(slot.RuntimeSlotIndex, Is.EqualTo(-1));
            Assert.That(slot.StableId, Is.EqualTo(-1));
        }

        [Test]
        public void StageSnapshotAndPhaseBoundMathRemainFrozen()
        {
            var stage = new BattleStageRuntimeState();
            Assert.That(stage.BaseStageWidthPx, Is.EqualTo(800));
            Assert.That(stage.StageWidthPx, Is.EqualTo(800));
            Assert.That(stage.ZMin, Is.EqualTo(180));
            Assert.That(stage.ZMax, Is.EqualTo(350));
            Assert.That(stage.BoundRight, Is.EqualTo(800));

            stage.SetSceneSnapshot(0, 10, 10, 3, 4);
            Assert.That(stage.BaseStageWidthPx, Is.EqualTo(1));
            Assert.That(stage.StageWidthPx, Is.EqualTo(1));
            Assert.That(stage.ZMin, Is.EqualTo(10));
            Assert.That(stage.ZMax, Is.EqualTo(11));
            Assert.That(stage.PerspectiveNear, Is.EqualTo(3));
            Assert.That(stage.PerspectiveFar, Is.EqualTo(4));

            stage.ApplyPhaseBound(900);
            Assert.That(stage.XMaxOverride, Is.EqualTo(900));
            Assert.That(stage.CameraMaxOverride, Is.EqualTo(106));
            Assert.That(stage.StageWidthPx, Is.EqualTo(900));
            stage.ClearPhaseBound();
            Assert.That(stage.XMaxOverride, Is.Zero);
            Assert.That(stage.CameraMaxOverride, Is.Zero);
            Assert.That(stage.StageWidthPx, Is.EqualTo(1));
            stage.Reset();
            Assert.That(stage.BaseStageWidthPx, Is.EqualTo(800));
            Assert.That(stage.StageWidthPx, Is.EqualTo(800));
            Assert.That(stage.ZMin, Is.EqualTo(180));
            Assert.That(stage.ZMax, Is.EqualTo(350));
        }

        [Test]
        public void FlowResetClearsEveryPublicScalarField()
        {
            var flow = new BattleFlowRuntimeState();
            FieldInfo[] fields = typeof(BattleFlowRuntimeState).GetFields(
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(fields.Length, Is.GreaterThan(0));
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                if (field.FieldType == typeof(int))
                    field.SetValue(flow, 17);
                else if (field.FieldType == typeof(bool))
                    field.SetValue(flow, true);
                else
                    Assert.Fail($"Unexpected flow scalar field type: {field.Name}={field.FieldType}");
            }

            flow.Reset();
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                object expected = field.FieldType == typeof(bool) ? (object)false : 0;
                Assert.That(field.GetValue(flow), Is.EqualTo(expected), field.Name);
            }
        }

        [Test]
        public void AuthorityConfirmedAndUnityMetadataFieldsStayExplicitlySeparated()
        {
            AssertFieldsExist<BattleMatchRuntimeState>(
                "BattleGameModeId", "Difficulty", "StageIdx", "RandomStage");
            AssertFieldsExist<BattleStageRuntimeState>(
                "BaseStageWidthPx", "StageWidthPx", "ZMin", "ZMax",
                "PerspectiveNear", "PerspectiveFar", "BoundLeft", "BoundRight",
                "XMaxOverride", "CameraMaxOverride");
            AssertFieldsExist<BattleStageProgressionState>(
                "StageSeriesIdx", "WaveIdx", "Round", "RoundMax");
            AssertFieldsExist<BattleFlowRuntimeState>(
                "CurrentTickIndex", "InputPhase", "FrameToggle", "AiPhaseGate",
                "AiDifficulty", "AiRand3", "AiRand5", "AiRand15", "AiRand20",
                "AiMoveMode", "AiStageTargetX", "BattleExitCountdown",
                "RouteOutRequest", "InitStatsRequest", "Mode2Request",
                "BattleStepMode", "BattleStepGate", "NeedClearInput");

            AssertFieldsExist<BattleMatchRuntimeState>(
                "LocalGameModeId", "BackgroundId", "RuntimeStageCount", "Seed", "PpMode");
            AssertFieldsExist<BattleSlotRuntimeState>(
                "IsHuman", "InputId", "AiId", "RuntimeSlotIndex", "StableId");
            AssertFieldsExist<BattleFlowRuntimeState>(
                "SparkRenderFrame", "HumanInputPolledExternally");
        }

        private static void AssertFieldsExist<T>(params string[] names)
        {
            Type type = typeof(T);
            for (int index = 0; index < names.Length; index++)
            {
                Assert.That(
                    type.GetField(names[index], BindingFlags.Instance | BindingFlags.Public),
                    Is.Not.Null,
                    $"{type.Name}.{names[index]}");
            }
        }
    }
}
#endif
