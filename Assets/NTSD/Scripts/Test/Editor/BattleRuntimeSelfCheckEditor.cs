#if UNITY_EDITOR
using NTSD.Test;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NTSD.EditorTools
{
    /// <summary>
    /// 战斗运行时自检的编辑器入口。
    /// 支持菜单执行，也支持 Unity batchmode 的 -executeMethod 调用。
    /// </summary>
    [InitializeOnLoad]
    public static class BattleRuntimeSelfCheckEditor
    {
        private const string RequestFile = "Temp/NTSD_BattleRuntimeSelfCheck.request";
        private const string ResultFile = "Temp/NTSD_BattleRuntimeSelfCheck.result";
        private const string SimulationWorldM1RequestFile =
            "Temp/NTSD_SimulationWorldM1Focused.request";
        private const string SimulationWorldM1ResultFile =
            "Temp/NTSD_SimulationWorldM1Focused.result";
        private const string SimulationWorldM2RequestFile =
            "Temp/NTSD_SimulationWorldM2Focused.request";
        private const string SimulationWorldM2ResultFile =
            "Temp/NTSD_SimulationWorldM2Focused.result";
        private static bool requestRunInProgress;
        private static bool staleResultDeleteWarningLogged;
        private static readonly string RequestAbsolutePath = ProjectPath(RequestFile);
        private static readonly string ResultAbsolutePath = ProjectPath(ResultFile);
        private static readonly string SimulationWorldM1RequestAbsolutePath =
            ProjectPath(SimulationWorldM1RequestFile);
        private static readonly string SimulationWorldM1ResultAbsolutePath =
            ProjectPath(SimulationWorldM1ResultFile);
        private static readonly string SimulationWorldM2RequestAbsolutePath =
            ProjectPath(SimulationWorldM2RequestFile);
        private static readonly string SimulationWorldM2ResultAbsolutePath =
            ProjectPath(SimulationWorldM2ResultFile);

        static BattleRuntimeSelfCheckEditor()
        {
            EditorApplication.update += PollRequest;
        }

        [MenuItem("NTSD/验证/运行战斗运行时自检")]
        public static void RunFromMenu()
        {
            RunAndWriteResult(exitBatchmode: false);
        }

        public static void RunForBatchmode()
        {
            RunAndWriteResult(exitBatchmode: true);
        }

        private static void PollRequest()
        {
            if (TryRunSimulationWorldM2FocusedRequest())
                return;
            if (TryRunSimulationWorldM1FocusedRequest())
                return;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (requestRunInProgress)
                return;

            if (!File.Exists(RequestAbsolutePath))
            {
                staleResultDeleteWarningLogged = false;
                return;
            }

            // A pending request invalidates any result left by an earlier run.
            try
            {
                if (File.Exists(ResultAbsolutePath))
                    File.Delete(ResultAbsolutePath);
                staleResultDeleteWarningLogged = false;
            }
            catch (Exception ex)
            {
                if (!staleResultDeleteWarningLogged)
                {
                    Debug.LogWarning($"[BattleRuntimeSelfCheckEditor] Failed to delete stale result file: {ex.Message}");
                    staleResultDeleteWarningLogged = true;
                }
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            requestRunInProgress = true;
            try
            {
                try
                {
                    File.Delete(RequestAbsolutePath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BattleRuntimeSelfCheckEditor] Failed to delete request file: {ex.Message}");
                    return;
                }

                RunAndWriteResult(exitBatchmode: false);
            }
            finally
            {
                requestRunInProgress = false;
            }
        }

        private static bool TryRunSimulationWorldM1FocusedRequest()
        {
            if (!File.Exists(SimulationWorldM1RequestAbsolutePath))
                return false;
            if (EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return true;
            }

            File.Delete(SimulationWorldM1RequestAbsolutePath);
            try
            {
                RunSimulationWorldM1FocusedChecks();
                File.WriteAllText(
                    SimulationWorldM1ResultAbsolutePath,
                    "PASS\narchitecture=4\noid5152=7\nrespawn=4\ntotal=15");
                Debug.Log(
                    "[SimulationWorldM1Focused] PASS: architecture=4, " +
                    "oid5152=7, respawn=4, total=15.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    SimulationWorldM1ResultAbsolutePath,
                    "FAIL\n" + exception);
                Debug.LogException(exception);
            }
            return true;
        }

        private static bool TryRunSimulationWorldM2FocusedRequest()
        {
            if (!File.Exists(SimulationWorldM2RequestAbsolutePath))
                return false;
            if (EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return true;
            }

            File.Delete(SimulationWorldM2RequestAbsolutePath);
            try
            {
                RunSimulationWorldM2FocusedChecks();
                File.WriteAllText(
                    SimulationWorldM2ResultAbsolutePath,
                    "PASS\narchitecture=4\nearly=6\nflow=1\ntotal=11");
                Debug.Log(
                    "[SimulationWorldM2Focused] PASS: architecture=4, " +
                    "early=6, flow=1, total=11.");
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    SimulationWorldM2ResultAbsolutePath,
                    "FAIL\n" + exception);
                Debug.LogException(exception);
            }
            return true;
        }

        private static void RunSimulationWorldM2FocusedChecks()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string simulationRoot = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Scripts",
                "Simulation");
            string modulePath = Path.Combine(
                simulationRoot,
                "Passes",
                "EarlyFrameAdvance",
                "BattleEarlyFrameAdvanceModule.cs");
            string worldPath = Path.Combine(
                simulationRoot,
                "Core",
                "SimulationWorld.cs");
            Require(File.Exists(modulePath),
                "BattleEarlyFrameAdvanceModule must have a dedicated file.");
            Require(
                File.ReadAllText(modulePath).Contains(
                    "class BattleEarlyFrameAdvanceModule"),
                "BattleEarlyFrameAdvanceModule dedicated file must declare the module.");

            string worldSource = File.ReadAllText(worldPath);
            Require(
                !Regex.IsMatch(
                    worldSource,
                    @"\bclass\s+BattleEarlyFrameAdvanceModule\b",
                    RegexOptions.CultureInvariant),
                "SimulationWorld.cs must not declare BattleEarlyFrameAdvanceModule.");
            AssertReadonlyModuleField(
                "passPipeline",
                "SimulationPassPipeline");

            string[] earlyChecks =
            {
                "NeutralExactCharacters_SkipSnapshotsAndMatchForcedLegacy",
                "ToggleGateAndTeleportTieSelection_MatchForcedLegacyAndRng",
                "State500Branches_MatchForcedLegacy",
                "State501OwnerChildrenDeadAndMissingReplacement_MatchLegacy",
                "ReusedState500Slot_UsesCurrentGeneration",
                "WarmedNeutralFastPath_AllocatesNoManagedMemory",
            };
            for (int index = 0; index < earlyChecks.Length; index++)
            {
                InvokeEditorFixtureMethod(
                    "NTSD.Test.EarlyFrameAdvanceOptimizationEditorTests",
                    earlyChecks[index]);
            }

            InvokeExistingSelfCheck("CheckBattleFlowToggleAndTeleportMatrix");
        }

        private static void InvokeEditorFixtureMethod(
            string typeName,
            string methodName)
        {
            Type fixtureType = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                fixtureType = assemblies[index].GetType(typeName, false);
                if (fixtureType != null)
                    break;
            }

            Require(fixtureType != null,
                $"Focused fixture type {typeName} must be loaded.");
            MethodInfo method = fixtureType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Require(method != null,
                $"Focused fixture {typeName} must retain {methodName}.");

            object fixture = Activator.CreateInstance(fixtureType);
            try
            {
                method.Invoke(fixture, null);
            }
            catch (TargetInvocationException exception)
                when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw new InvalidOperationException(
                    "Unreachable after rethrowing the focused fixture failure.");
            }
        }

        private static void RunSimulationWorldM1FocusedChecks()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string scriptsRoot = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Scripts");
            string simulationRoot = Path.Combine(scriptsRoot, "Simulation");
            string[] sourceFiles = Directory.GetFiles(
                scriptsRoot,
                "*.cs",
                SearchOption.AllDirectories);
            for (int index = 0; index < sourceFiles.Length; index++)
            {
                string source = File.ReadAllText(sourceFiles[index]);
                Require(
                    !Regex.IsMatch(
                        source,
                        @"\bpartial\s+class\s+SimulationWorld\b",
                        RegexOptions.CultureInvariant),
                    "SimulationWorld partial declaration remains in " +
                    sourceFiles[index]);
            }

            string[] historicalPartialFiles = Directory.GetFiles(
                simulationRoot,
                "SimulationWorld*.partial.cs",
                SearchOption.TopDirectoryOnly);
            Require(
                historicalPartialFiles.Length == 0,
                "SimulationWorld historical partial files must be removed.");

            string worldPath = Path.Combine(
                simulationRoot,
                "Core",
                "SimulationWorld.cs");
            string worldSource = File.ReadAllText(worldPath);
            string[,] moduleContracts =
            {
                { "SimulationRegistryModule", "Runtime/SimulationRegistryModule.cs" },
                { "SimulationAiRuntime", "Ai/Runtime/SimulationAiRuntime.cs" },
                { "SimulationAiInputModule", "Ai/Runtime/SimulationAiInputModule.cs" },
                { "SimulationAiSensingModule", "Ai/Runtime/SimulationAiSensingModule.cs" },
                { "SimulationAiDecisionModule", "Ai/Runtime/SimulationAiDecisionModule.cs" },
                { "SimulationStageWaveModule", "Stage/SimulationStageWaveModule.cs" },
                { "SimulationStageRenderModule", "Stage/SimulationStageRenderModule.cs" },
                { "BattleOid5152RuntimeModule", "Passes/Oid5152/BattleOid5152RuntimeModule.cs" },
                { "BattleRespawnModule", "Passes/Respawn/BattleRespawnModule.cs" },
                { "BattleEarlyFrameAdvanceModule", "Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs" },
                { "BattleLateEntityLifecycleModule", "Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs" },
                { "BattleInteractionPipeline", "Passes/Interaction/BattleInteractionPipeline.cs" },
                { "BattleRandomWeaponDropModule", "Passes/RandomWeapon/BattleRandomWeaponDropModule.cs" },
                { "SimulationPassPipeline", "Core/SimulationPassPipeline.cs" },
            };
            for (int index = 0; index < moduleContracts.GetLength(0); index++)
            {
                string typeName = moduleContracts[index, 0];
                string relativePath = moduleContracts[index, 1];
                string modulePath = Path.Combine(
                    simulationRoot,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                Require(File.Exists(modulePath),
                    $"Module {typeName} must have dedicated file {relativePath}.");
                Require(
                    File.ReadAllText(modulePath).Contains("class " + typeName),
                    $"Dedicated file {relativePath} must declare {typeName}.");
                Require(
                    !Regex.IsMatch(
                        worldSource,
                        $@"\bclass\s+{Regex.Escape(typeName)}\b",
                        RegexOptions.CultureInvariant),
                    $"SimulationWorld.cs must not declare child module {typeName}.");
            }

            AssertReadonlyModuleField("registryModule", "SimulationRegistryModule");
            AssertReadonlyModuleField("aiRuntime", "SimulationAiRuntime");
            AssertReadonlyModuleField("passPipeline", "SimulationPassPipeline");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "oid5152RuntimeModule",
                "BattleOid5152RuntimeModule");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "respawnModule",
                "BattleRespawnModule");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "earlyFrameAdvanceModule",
                "BattleEarlyFrameAdvanceModule");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "lateEntityLifecycleModule",
                "BattleLateEntityLifecycleModule");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "interactionPipeline",
                "BattleInteractionPipeline");
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationPassPipeline),
                "randomWeaponDropModule",
                "BattleRandomWeaponDropModule");
            AssertReadonlyModuleField("stageWaveModule", "SimulationStageWaveModule");
            AssertReadonlyModuleField("stageRenderModule", "SimulationStageRenderModule");

            string[] oidChecks =
            {
                "CheckOid5152MergeSuccessAndDormantIsolation",
                "CheckOid5152MergeCooldownOneTriggersSameTick",
                "CheckOid5152AuthorityGateMatrix",
                "CheckOid5152MirrorIdentityAndPresentation",
                "CheckOid5152SplitSuccessAndOddTruncate",
                "CheckOid5152SplitFailurePartialRecovery",
                "CheckOid5152DjaReleaseTriggersSameTickSplit",
            };
            for (int index = 0; index < oidChecks.Length; index++)
                InvokeExistingSelfCheck(oidChecks[index]);

            string[] respawnChecks =
            {
                "CheckRespawnPassWithoutStoredCount",
                "CheckRespawnReadsPhysicsTailIntegerCoordinates",
                "CheckRespawnPassFreeEntityGate",
                "CheckRespawnPassWithStoredCountAndEffectSpawn",
            };
            for (int index = 0; index < respawnChecks.Length; index++)
                InvokeExistingSelfCheck(respawnChecks[index]);
        }

        private static void AssertReadonlyModuleField(
            string fieldName,
            string expectedTypeName)
        {
            AssertReadonlyModuleField(
                typeof(NTSD.Simulation.SimulationWorld),
                fieldName,
                expectedTypeName);
        }

        private static void AssertReadonlyModuleField(
            Type ownerType,
            string fieldName,
            string expectedTypeName)
        {
            FieldInfo field = ownerType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null,
                $"{ownerType.Name} must own module field {fieldName}.");
            Require(field.IsInitOnly,
                $"{ownerType.Name} module field {fieldName} must be readonly.");
            Require(field.FieldType.Name == expectedTypeName,
                $"{ownerType.Name} field {fieldName} must use {expectedTypeName}.");
        }

        private static void InvokeExistingSelfCheck(string methodName)
        {
            MethodInfo method = typeof(BattleRuntimeSelfCheck).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Require(method != null,
                $"BattleRuntimeSelfCheck must retain focused check {methodName}.");
            try
            {
                method.Invoke(null, null);
            }
            catch (TargetInvocationException exception)
                when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw new InvalidOperationException(
                    "Unreachable after rethrowing the self-check failure.");
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void RunAndWriteResult(bool exitBatchmode)
        {
            try
            {
                BattleRuntimeSelfCheck.RunAllChecksStatic();
                WriteResult("PASS");
                Debug.Log("[BattleRuntimeSelfCheckEditor] 自检完成。");
            }
            catch (Exception ex)
            {
                WriteResult("FAIL\n" + ex);
                if (exitBatchmode && Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }

            if (exitBatchmode && Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        private static void WriteResult(string content)
        {
            File.WriteAllText(ResultAbsolutePath, content);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }
    }
}
#endif
