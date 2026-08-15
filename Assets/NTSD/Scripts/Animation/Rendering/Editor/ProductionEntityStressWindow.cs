#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Simulation;
using NTSD.Test;
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressWindow : EditorWindow
    {
        private int warmupTicks = 30;
        private int sampleTicks = 300;
        private int aiSimulationSmokeSampleTicks = 30;
        private int spawnBatchSize = 25;
        private int maxCatchUpTicksPerFrame = 1;
        private int maxBacklogTicks = 8;
        private float catchUpCpuBudgetMs = 1000f / 30f;
        private int maxSaturationDrainTicks = 300;
        private int formalCollectorModeIndex;
        private int aiExecutionProfileIndex;
        private int lateRuntimeSnapshotModeIndex = 1;
        private int soundPresentationModeIndex;
        private bool enableAiDecisionSoAShadow;
        private bool enableAiDecisionSharedShadow;
        private uint seed = 0x4E545344u;
        private bool simulationOnly;
        private bool skipLateRendererUpdate;
        private bool autoStopWhenSampled;
        private bool enablePhaseTiming;
        private bool enablePresentationTiming;
        private bool enableDetailPhaseTiming;
        private bool enableFrameTiming;
        private string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
        private string status = "就绪。启动请求将进入播放模式，并让 1000 实体压力测试保持可见，直到执行清理。";
        private static readonly string[] FormalCollectorModes =
        {
            "configured",
            "legacy",
            "role",
            "brute",
        };
        private static readonly string[] FormalCollectorModeLabels =
        {
            "按配置",
            "旧版",
            "角色感知",
            "暴力遍历",
        };
        private static readonly string[] AiExecutionProfiles =
        {
            "legacy",
            "data-oriented-canonical",
        };
        private static readonly string[] AiExecutionProfileLabels =
        {
            "旧版规范模式",
            "数据导向规范模式",
        };
        private static readonly string[] LateRuntimeSnapshotModes =
        {
            "legacy-three",
            "consolidated-final",
        };
        private static readonly string[] LateRuntimeSnapshotModeLabels =
        {
            "旧版三阶段快照",
            "合并最终快照",
        };
        private static readonly string[] SoundPresentationModes =
        {
            "inherit",
            "suppress",
            "dispatch",
        };
        private static readonly string[] SoundPresentationModeLabels =
        {
            "跟随仅模拟设置",
            "抑制",
            "派发",
        };

        [MenuItem("NTSD/战斗诊断/生产实体压力测试")]
        public static void Open()
        {
            GetWindow<ProductionEntityStressWindow>("生产实体压力测试");
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 50 实体冒烟测试")]
        public static void RunSmokeFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("smoke", "Temp/NTSD_ProductionEntityStress.smoke.json"));
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 1000 实体分散测试")]
        public static void RunDispersedFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("dispersed", "Temp/NTSD_ProductionEntityStress.dispersed.json"));
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 1000 AI 分组近战零 GC 测试")]
        public static void RunCombatFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateCombatZeroGcRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.zero-gc.json"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Capacity Pressure Smoke")]
        public static void RunCombatCapacityPressureSmokeFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.capacity-pressure-smoke.json",
                    "legacy"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Capacity Pressure Smoke")]
        public static void RunCombatDataOrientedCapacityPressureSmokeFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json",
                    "data-oriented-canonical"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Legacy Formal Slot Map Capacity Pressure A-B")]
        public static void RunCombatDataOrientedLegacyFormalSlotMapCapacityPressureFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-legacy-formal-slot-map-capacity-pressure.json",
                    "data-oriented-canonical");
            request.forceLegacyFormalSlotMap = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Performance Smoke")]
        public static void RunCombatDataOrientedPerformanceSmokeFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-performance-smoke.json",
                    "data-oriented-canonical"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Phase Timing Smoke")]
        public static void RunCombatDataOrientedPhaseTimingSmokeFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-phase-timing-smoke.json",
                    "data-oriented-canonical");
            request.enablePhaseTiming = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Late Common NoOp Candidate")]
        public static void RunCombatU6LateCommonNoOpCandidateFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-late-common-noop-candidate.json",
                    "data-oriented-canonical");
            request.forceLegacyLateCommonNoOpGates = false;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Late Common NoOp Legacy A-B")]
        public static void RunCombatU6LateCommonNoOpLegacyFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-late-common-noop-legacy.json",
                    "data-oriented-canonical");
            request.forceLegacyLateCommonNoOpGates = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Force Role-Aware Sweep A-B")]
        public static void RunCombatU6ForceRoleAwareSweepFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-force-role-aware-sweep.json",
                    "data-oriented-canonical");
            request.forceRoleAwareSweepDirect = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Force Role-Aware Tree A-B")]
        public static void RunCombatU6ForceRoleAwareTreeFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-force-role-aware-tree.json",
                    "data-oriented-canonical");
            request.forceRoleAwareTree = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Positive Link Index Candidate")]
        public static void RunCombatU6PositiveLinkIndexCandidateFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-positive-link-index-candidate.json",
                    "data-oriented-canonical");
            request.positiveLinkValidationMode = "data-oriented";
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI U6 Positive Link Index Legacy A-B")]
        public static void RunCombatU6PositiveLinkIndexLegacyFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.u6-positive-link-index-legacy.json",
                    "data-oriented-canonical");
            request.positiveLinkValidationMode = "legacy";
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Completed Frame Timing Diagnostic")]
        public static void RunCombatCompletedFrameTimingDiagnosticFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.completed-frame-timing.json",
                    "data-oriented-canonical");
            request.sampleTicks = 180;
            request.enableFrameTiming = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Completed Frame Timing Sound Suppressed A-B")]
        public static void RunCombatCompletedFrameTimingSoundSuppressedFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.completed-frame-sound-suppressed.json",
                    "data-oriented-canonical");
            request.sampleTicks = 180;
            request.enableFrameTiming = true;
            request.soundPresentationMode = "suppress";
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Full Character Input Refresh A-B")]
        public static void RunCombatDataOrientedFullCharacterInputRefreshFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-full-character-input-refresh.json",
                    "data-oriented-canonical");
            request.forceFullCharacterInputPostRefresh = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Full Unified Snapshot Rebuild A-B")]
        public static void RunCombatDataOrientedFullUnifiedSnapshotRebuildFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-full-unified-snapshot-rebuild.json",
                    "data-oriented-canonical");
            request.forceFullAiUnifiedSnapshotRebuild = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Legacy Formal Slot Map A-B")]
        public static void RunCombatDataOrientedLegacyFormalSlotMapFromMenu()
        {
            ProductionEntityStressRequest request =
                CreateCombatPerformanceSmokeRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-legacy-formal-slot-map.json",
                    "data-oriented-canonical");
            request.forceLegacyFormalSlotMap = true;
            ProductionEntityStressRequestProcessor.WriteRequest(request);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 AI Data Oriented Steady State Gate")]
        public static void RunCombatDataOrientedSteadyStateGateFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateCombatSteadyStateRequest(
                    "Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-steady-state.json",
                    "data-oriented-canonical"));
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 1000 实体集中测试")]
        public static void RunConcentratedFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("concentrated", "Temp/NTSD_ProductionEntityStress.concentrated.json"));
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 AI 纯模拟冒烟测试/100 实体分散")]
        public static void RunDispersed100AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(100);
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 AI 纯模拟冒烟测试/300 实体分散")]
        public static void RunDispersed300AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(300);
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 AI 纯模拟冒烟测试/500 实体分散")]
        public static void RunDispersed500AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(500);
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/运行 AI 纯模拟冒烟测试/1000 实体分散")]
        public static void RunDispersed1000AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(1000);
        }

        [MenuItem("NTSD/战斗诊断/生产实体压力测试/停止并清理")]
        public static void StopFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteStopRequest();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("真实生产实体压力测试", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "配置",
                "移动扩展（MobileExtended，1050 个槽位），松散四叉树（LooseQuadtree）");
            warmupTicks = EditorGUILayout.IntField("预热逻辑帧数", warmupTicks);
            sampleTicks = EditorGUILayout.IntField("目标采样帧数", sampleTicks);
            aiSimulationSmokeSampleTicks = EditorGUILayout.IntSlider(
                "AI 纯模拟冒烟采样数",
                aiSimulationSmokeSampleTicks,
                10,
                30);
            spawnBatchSize = EditorGUILayout.IntSlider("每批生成数量", spawnBatchSize, 1, 100);
            maxCatchUpTicksPerFrame = EditorGUILayout.IntSlider(
                "每帧最大追赶帧数",
                maxCatchUpTicksPerFrame,
                1,
                12);
            maxBacklogTicks = EditorGUILayout.IntSlider(
                "最大积压帧数",
                maxBacklogTicks,
                maxCatchUpTicksPerFrame,
                30);
            catchUpCpuBudgetMs = Mathf.Max(
                0f,
                EditorGUILayout.FloatField(
                    "追帧 CPU 预算(ms，0=关闭)",
                    catchUpCpuBudgetMs));
            EditorGUILayout.HelpBox(
                "启用预算后，首个逻辑 tick 一定执行；仅当预计下一 tick 仍在预算内时才继续追帧。" +
                "显式提高每帧最大 tick 数时，它能限制单个 Unity Update 的停顿放大，但不会掩盖 backlog 和 dropped tick。",
                MessageType.Info);
            maxSaturationDrainTicks = EditorGUILayout.IntField(
                "最大饱和排空帧数",
                Math.Max(1, maxSaturationDrainTicks));
            formalCollectorModeIndex = EditorGUILayout.Popup(
                "正式收集器",
                formalCollectorModeIndex,
                FormalCollectorModeLabels);
            aiExecutionProfileIndex = EditorGUILayout.Popup(
                "AI 执行模式",
                aiExecutionProfileIndex,
                AiExecutionProfileLabels);
            string selectedAiExecutionProfile =
                AiExecutionProfiles[aiExecutionProfileIndex];
            NormalizeDecisionShadowModesForProfile(
                selectedAiExecutionProfile,
                ref enableAiDecisionSoAShadow,
                ref enableAiDecisionSharedShadow);
            lateRuntimeSnapshotModeIndex = EditorGUILayout.Popup(
                "延迟运行时快照",
                lateRuntimeSnapshotModeIndex,
                LateRuntimeSnapshotModeLabels);
            using (new EditorGUI.DisabledScope(
                       !SupportsDecisionShadowModes(selectedAiExecutionProfile)))
            {
                bool requestedDeepShadow = EditorGUILayout.Toggle(
                    "AI 决策 SoA 影子验证",
                    enableAiDecisionSoAShadow);
                if (requestedDeepShadow != enableAiDecisionSoAShadow)
                {
                    enableAiDecisionSoAShadow = requestedDeepShadow;
                    if (enableAiDecisionSoAShadow)
                        enableAiDecisionSharedShadow = false;
                }
                bool requestedSharedShadow = EditorGUILayout.Toggle(
                    "AI 决策共享影子验证",
                    enableAiDecisionSharedShadow);
                if (requestedSharedShadow != enableAiDecisionSharedShadow)
                {
                    enableAiDecisionSharedShadow = requestedSharedShadow;
                    if (enableAiDecisionSharedShadow)
                        enableAiDecisionSoAShadow = false;
                }
            }
            if (selectedAiExecutionProfile == "data-oriented-canonical")
            {
                EditorGUILayout.HelpBox(
                    "使用原子化 SoA 感知、索引化决策与统一权威的生产配置。" +
                    "此模式自带采样完整预言机，不能同时启用决策影子验证。",
                    MessageType.Info);
            }
            long editedSeed = EditorGUILayout.LongField("确定性种子", seed);
            seed = (uint)Math.Max(uint.MinValue, Math.Min(uint.MaxValue, editedSeed));
            simulationOnly = EditorGUILayout.Toggle("仅模拟", simulationOnly);
            soundPresentationModeIndex = EditorGUILayout.Popup(
                "声音表现",
                soundPresentationModeIndex,
                SoundPresentationModeLabels);
            using (new EditorGUI.DisabledScope(!simulationOnly))
            {
                skipLateRendererUpdate = EditorGUILayout.Toggle(
                    "跳过延迟渲染器更新",
                    skipLateRendererUpdate);
            }
            if (skipLateRendererUpdate && !simulationOnly)
            {
                EditorGUILayout.HelpBox(
                    "“跳过延迟渲染器更新”要求启用“仅模拟”。",
                    MessageType.Error);
            }
            autoStopWhenSampled = EditorGUILayout.Toggle(
                "采样完成后自动停止",
                autoStopWhenSampled);
            enablePhaseTiming = EditorGUILayout.Toggle("粗粒度阶段计时", enablePhaseTiming);
            enablePresentationTiming = EditorGUILayout.Toggle(
                "表现层计时",
                enablePresentationTiming);
            enableDetailPhaseTiming = EditorGUILayout.Toggle(
                "详细阶段计时",
                enableDetailPhaseTiming);
            enableFrameTiming = EditorGUILayout.Toggle(
                "完整帧 CPU/渲染/GPU 计时",
                enableFrameTiming);
            outputPath = EditorGUILayout.TextField("报告路径", outputPath);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("50 实体冒烟测试"))
                    WriteStart("smoke", "Temp/NTSD_ProductionEntityStress.smoke.json");
                if (GUILayout.Button("1000 实体分散测试"))
                    WriteStart("dispersed", "Temp/NTSD_ProductionEntityStress.dispersed.json");
                if (GUILayout.Button("1000 AI 分组近战"))
                    WriteStart("combat", "Temp/NTSD_ProductionEntityStress.combat1000.zero-gc.json");
                if (GUILayout.Button("1000 实体集中测试"))
                    WriteStart("concentrated", "Temp/NTSD_ProductionEntityStress.concentrated.json");
            }
            if (GUILayout.Button("停止并清理"))
                StopFromMenu();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("分散式 AI 纯模拟冒烟测试", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "固定阶梯：预热 30 个逻辑帧，采样 10 到 30 次，自动停止，使用确定性 AI 输入且禁用表现。",
                MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("100 实体 AI 模拟"))
                    WriteDispersedAiSimulationSmoke(100);
                if (GUILayout.Button("300 实体 AI 模拟"))
                    WriteDispersedAiSimulationSmoke(300);
                if (GUILayout.Button("500 实体 AI 模拟"))
                    WriteDispersedAiSimulationSmoke(500);
                if (GUILayout.Button("1000 实体 AI 模拟"))
                    WriteDispersedAiSimulationSmoke(1000);
            }

            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            if (runner != null && runner.Report != null)
            {
                ProductionEntityStressReport report = runner.Report;
                status = $"{LocalizeRunStatus(report.status)}：活动对象={report.activeGameObjectCount}，" +
                         $"世界对象={report.worldObjectCount}，运行时槽位={report.claimedRuntimeSlotCount}，" +
                         $"已采样逻辑帧={report.sampledLogicTicks}，" +
                         $"收集器={LocalizeFormalCollectorMode(report.formalCollectorMode)}，" +
                         $"bdy 条目={report.formalCollectorBodyEntries}，" +
                         $"itr 查询={report.formalCollectorItrQueries}";
                Repaint();
            }
            EditorGUILayout.HelpBox(status, MessageType.Info);
        }

        internal static bool SupportsDecisionShadowModes(string aiExecutionProfile)
        {
            return string.Equals(
                aiExecutionProfile,
                "legacy",
                StringComparison.Ordinal);
        }

        internal static void NormalizeDecisionShadowModesForProfile(
            string aiExecutionProfile,
            ref bool enableSoAShadow,
            ref bool enableSharedShadow)
        {
            if (SupportsDecisionShadowModes(aiExecutionProfile))
                return;

            enableSoAShadow = false;
            enableSharedShadow = false;
        }

        private void WriteStart(string action, string defaultOutputPath)
        {
            try
            {
                string selectedOutput = string.IsNullOrWhiteSpace(outputPath)
                    ? defaultOutputPath
                    : outputPath;
                var request = new ProductionEntityStressRequest
                {
                    action = action,
                    warmupTicks = warmupTicks,
                    sampleTicks = sampleTicks,
                    spawnBatchSize = spawnBatchSize,
                    maxCatchUpTicksPerFrame = maxCatchUpTicksPerFrame,
                    maxBacklogTicks = maxBacklogTicks,
                    catchUpCpuBudgetMs = catchUpCpuBudgetMs,
                    maxSaturationDrainTicks = maxSaturationDrainTicks,
                    enablePhaseTiming = enablePhaseTiming,
                    enablePresentationTiming = enablePresentationTiming,
                    enableDetailPhaseTiming = enableDetailPhaseTiming,
                    enableFrameTiming = enableFrameTiming,
                    simulationOnly = simulationOnly,
                    soundPresentationMode =
                        SoundPresentationModes[soundPresentationModeIndex],
                    skipLateRendererUpdate = skipLateRendererUpdate,
                    autoStopWhenSampled = autoStopWhenSampled,
                    seed = seed,
                    aiExecutionProfile = AiExecutionProfiles[aiExecutionProfileIndex],
                    lateRuntimeSnapshotMode =
                        LateRuntimeSnapshotModes[lateRuntimeSnapshotModeIndex],
                    enableAiDecisionSoAShadow = enableAiDecisionSoAShadow,
                    enableAiDecisionSharedShadow = enableAiDecisionSharedShadow,
                    formalCollectorMode = FormalCollectorModes[formalCollectorModeIndex],
                    outputPath = selectedOutput,
                };
                ProductionEntityStressRequestProcessor.WriteRequest(request);
                status = "请求已写入，正在等待播放模式下的生产服务。";
            }
            catch (Exception exception)
            {
                status = $"错误：{exception.Message}";
                Debug.LogError($"[ProductionEntityStress] Request write failed: {exception}");
            }
        }

        private void WriteDispersedAiSimulationSmoke(int entityCount)
        {
            try
            {
                string defaultOutputPath =
                    $"Temp/NTSD_ProductionEntityStress.dispersed{entityCount}.ai-sim-smoke.json";
                string selectedOutput = string.IsNullOrWhiteSpace(outputPath)
                    ? defaultOutputPath
                    : outputPath;
                ProductionEntityStressRequestProcessor.WriteRequest(
                    CreateDispersedAiSimulationOnlySmokeRequest(
                        entityCount,
                        selectedOutput,
                        aiSimulationSmokeSampleTicks,
                        LateRuntimeSnapshotModes[lateRuntimeSnapshotModeIndex],
                        skipLateRendererUpdate));
                status = "AI 纯模拟冒烟测试请求已写入，正在等待播放模式下的生产服务。";
            }
            catch (Exception exception)
            {
                status = $"错误：{exception.Message}";
                Debug.LogError($"[ProductionEntityStress] Request write failed: {exception}");
            }
        }

        private static string LocalizeRunStatus(string value)
        {
            switch (value)
            {
                case "Starting":
                    return "正在启动";
                case "Running":
                    return "正在运行";
                case "Failed":
                    return "失败";
                case "SmokePassed":
                    return "冒烟测试通过";
                case "SmokeFailed":
                    return "冒烟测试失败";
                case "StoppedCleanly":
                    return "已正常停止";
                case "StoppedWithResidue":
                    return "已停止但存在残留";
                case "InterruptedCleanly":
                    return "中断后已清理";
                case "InterruptedWithResidue":
                    return "中断后仍有残留";
                case "SaturationBlockedReplenishment":
                    return "因容量饱和而无法补充实体";
                default:
                    return value;
            }
        }

        private static string LocalizeFormalCollectorMode(string value)
        {
            switch (value)
            {
                case "configured":
                    return "按配置";
                case "legacy":
                    return "旧版";
                case "role":
                    return "角色感知";
                case "brute":
                    return "暴力遍历";
                default:
                    return value;
            }
        }

        internal static ProductionEntityStressRequest CreateDefaultRequest(
            string action,
            string reportPath)
        {
            bool smoke = string.Equals(action, "smoke", StringComparison.Ordinal);
            return new ProductionEntityStressRequest
            {
                action = action,
                warmupTicks = smoke ? 2 : 30,
                sampleTicks = smoke ? 10 : 300,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 1,
                maxBacklogTicks = 8,
                maxSaturationDrainTicks = 300,
                aiExecutionProfile = "legacy",
                lateRuntimeSnapshotMode = "consolidated-final",
                forceLegacyLateTailNoOp = true,
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }

        internal static ProductionEntityStressRequest CreateCombatZeroGcRequest(
            string reportPath)
        {
            return new ProductionEntityStressRequest
            {
                action = "combat1000",
                inputMode = "ai",
                warmupTicks = 120,
                sampleTicks = 1800,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 1,
                maxBacklogTicks = 8,
                maxSaturationDrainTicks = 300,
                autoStopWhenSampled = true,
                requireZeroGcAfterWarmup = true,
                seed = 0x4E545344u,
                aiExecutionProfile = "legacy",
                lateRuntimeSnapshotMode = "consolidated-final",
                forceLegacyLateTailNoOp = true,
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }

        internal static ProductionEntityStressRequest CreateCombatCapacityPressureSmokeRequest(
            string reportPath,
            string aiExecutionProfile)
        {
            ProductionEntityStressRequest request = CreateCombatZeroGcRequest(reportPath);
            request.warmupTicks = 30;
            request.sampleTicks = 180;
            request.spawnBatchSize = 100;
            request.aiExecutionProfile = aiExecutionProfile;
            request.enablePhaseTiming = true;
            request.enablePresentationTiming = true;
            request.enableDetailPhaseTiming = true;
            return request;
        }

        internal static ProductionEntityStressRequest CreateCombatPerformanceSmokeRequest(
            string reportPath,
            string aiExecutionProfile)
        {
            ProductionEntityStressRequest request =
                CreateCombatCapacityPressureSmokeRequest(
                    reportPath,
                    aiExecutionProfile);
            request.enablePhaseTiming = false;
            request.enablePresentationTiming = false;
            request.enableDetailPhaseTiming = false;
            return request;
        }

        internal static ProductionEntityStressRequest CreateCombatSteadyStateRequest(
            string reportPath,
            string aiExecutionProfile)
        {
            ProductionEntityStressRequest request = CreateCombatZeroGcRequest(reportPath);
            request.spawnBatchSize = 100;
            request.aiExecutionProfile = aiExecutionProfile;
            request.enablePhaseTiming = false;
            request.enablePresentationTiming = false;
            request.enableDetailPhaseTiming = false;
            return request;
        }

        internal static ProductionEntityStressRequest CreateDispersedAiSimulationOnlySmokeRequest(
            int entityCount,
            string reportPath,
            int sampleTicks = 30,
            string lateRuntimeSnapshotMode = "consolidated-final",
            bool skipLateRendererUpdate = false)
        {
            if (sampleTicks < 10 || sampleTicks > 30)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleTicks),
                    sampleTicks,
                    "AI simulation smoke samples must be in the inclusive range 10..30.");
            }

            return new ProductionEntityStressRequest
            {
                action = GetDispersedAction(entityCount),
                entityCount = entityCount,
                inputMode = "ai",
                warmupTicks = 30,
                sampleTicks = sampleTicks,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 1,
                maxBacklogTicks = 8,
                maxSaturationDrainTicks = 300,
                simulationOnly = true,
                skipLateRendererUpdate = skipLateRendererUpdate,
                autoStopWhenSampled = true,
                seed = 0x4E545344u,
                aiExecutionProfile = "legacy",
                lateRuntimeSnapshotMode = lateRuntimeSnapshotMode,
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }

        private static void WriteDispersedAiSimulationSmokeRequest(int entityCount)
        {
            string outputPath =
                $"Temp/NTSD_ProductionEntityStress.dispersed{entityCount}.ai-sim-smoke.json";
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDispersedAiSimulationOnlySmokeRequest(entityCount, outputPath));
        }

        private static string GetDispersedAction(int entityCount)
        {
            switch (entityCount)
            {
                case 100:
                    return "dispersed100";
                case 300:
                    return "dispersed300";
                case 500:
                    return "dispersed500";
                case 1000:
                    return "dispersed1000";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(entityCount),
                        entityCount,
                        "Dispersed AI simulation smoke supports only 100, 300, 500, or 1000 entities.");
            }
        }
    }

    internal enum ProductionEntityStressPlayRestartDecision
    {
        None = 0,
        WaitForInitialServices = 1,
        RecordHealthyRuntime = 2,
        WaitForRestartTransition = 3,
        RestartPlayMode = 4,
        RetryLimitExceeded = 5,
    }

    internal enum ProductionEntityStressReloadRecoveryDecision
    {
        None = 0,
        RetryAfterCleanExit = 1,
        TerminalFailure = 2,
    }

    internal enum ProductionEntityStressReloadRecoveryTransition
    {
        None = 0,
        Wait = 1,
        ExitPlayMode = 2,
        EnterPlayMode = 3,
    }

    [InitializeOnLoad]
    internal static class ProductionEntityStressRequestProcessor
    {
        private const string SessionRequestJsonKey =
            "NTSD.ProductionEntityStress.RequestJson";
        private const string SessionServiceWaitDeadlineKey =
            "NTSD.ProductionEntityStress.ServiceWaitDeadlineRealtime";
        private const string SessionManagedRuntimeObservedKey =
            "NTSD.ProductionEntityStress.ManagedRuntimeObserved";
        private const string SessionPlayRestartPendingKey =
            "NTSD.ProductionEntityStress.PlayRestartPending";
        private const string SessionPlayRestartCountKey =
            "NTSD.ProductionEntityStress.PlayRestartCount";
        private const string SessionActiveRequestJsonKey =
            "NTSD.ProductionEntityStress.ActiveRequestJson";
        private const string SessionActiveConfigJsonKey =
            "NTSD.ProductionEntityStress.ActiveConfigJson";
        private const string SessionActiveStatePresentKey =
            "NTSD.ProductionEntityStress.ActiveStatePresent";
        private const string SessionReloadRecoveryPendingKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryPending";
        private const string SessionReloadRecoveryDispatchedKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryDispatched";
        private const string SessionReloadRecoveryTransitionKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryTransition";
        private const string SessionReloadRecoveryCountKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryCount";
        internal const int PlayRestartLimitForDiagnostics = 1;
        internal const int ReloadRecoveryLimitForDiagnostics = 1;
        internal const double ServiceWaitTimeoutSecondsForDiagnostics = 120d;
        private static bool processing;
        private static bool requestPending =
            File.Exists(ProductionEntityStressPaths.RequestAbsolutePath);
        private static bool polling;

        static ProductionEntityStressRequestProcessor()
        {
            ProductionEntityStressEditorBridge.NotifyRunStoppedAction =
                NotifyRunStopped;
            ConfigureBootstrapSuppressionFromPendingRequest();
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (requestPending || HasPendingReloadRecovery())
                StartPolling();
        }

        internal static void WriteRequest(ProductionEntityStressRequest request)
        {
            ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            WriteRequestJson(JsonUtility.ToJson(request));
        }

        internal static void WriteStopRequest()
        {
            ProductionEntityStressRunner activeRunner = ProductionEntityStressRunner.Active;
            if (activeRunner != null)
            {
                activeRunner.StopAndCleanup("stop-request");
            }
            else
            {
                ProductionEntityStressPaths.WriteTerminalResult(
                    true,
                    string.Empty,
                    "No active production entity stress runner required cleanup.");
            }

            NotifyRunStopped();
            CompleteRequest(ProductionEntityStressPaths.RequestAbsolutePath);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Process Pending Request")]
        internal static void ProcessPendingRequestFromMenu()
        {
            requestPending = File.Exists(ProductionEntityStressPaths.RequestAbsolutePath);
            if (!requestPending)
            {
                Debug.LogWarning(
                    "[ProductionEntityStress] No pending request file was found.");
                return;
            }

            StartPolling();
            PollRequest();
        }

        internal static bool ShouldEnterPlayMode(string action, bool isPlaying)
        {
            return !isPlaying && !string.Equals(
                action,
                "stop",
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldSuppressBattleTestBootstrap(string action)
        {
            return !string.Equals(
                action,
                "stop",
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsReadyToStart(
            bool hasActiveBattleTestBootstrap,
            bool bootstrapReportedReady,
            bool productionServicesReady)
        {
            return productionServicesReady &&
                   (!hasActiveBattleTestBootstrap || bootstrapReportedReady);
        }

        internal static ProductionEntityStressPlayRestartDecision EvaluatePlayRestartDecision(
            bool pendingStartRequest,
            bool isPlaying,
            bool managedRuntimeWasValid,
            bool managedRuntimeExpected,
            bool managedRuntimeIsValid,
            bool restartTransitionPending,
            int restartCount)
        {
            if (!pendingStartRequest || !isPlaying)
                return ProductionEntityStressPlayRestartDecision.None;
            if (managedRuntimeIsValid)
                return ProductionEntityStressPlayRestartDecision.RecordHealthyRuntime;
            if (restartTransitionPending)
                return ProductionEntityStressPlayRestartDecision.WaitForRestartTransition;
            if (!managedRuntimeWasValid && !managedRuntimeExpected && restartCount == 0)
                return ProductionEntityStressPlayRestartDecision.WaitForInitialServices;
            return restartCount < PlayRestartLimitForDiagnostics
                ? ProductionEntityStressPlayRestartDecision.RestartPlayMode
                : ProductionEntityStressPlayRestartDecision.RetryLimitExceeded;
        }

        internal static double ResolveServiceWaitDeadline(
            double realtimeNow,
            string persistedDeadline)
        {
            if (double.TryParse(
                    persistedDeadline,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double deadline) &&
                !double.IsNaN(deadline) &&
                !double.IsInfinity(deadline))
            {
                return deadline;
            }

            return realtimeNow + ServiceWaitTimeoutSecondsForDiagnostics;
        }

        internal static bool HasServiceWaitTimedOut(double realtimeNow, double deadline)
        {
            return realtimeNow >= deadline;
        }

        internal static ProductionEntityStressReloadRecoveryDecision
            EvaluateReloadRecoveryDecision(
                bool hasCompleteActiveState,
                bool runnerActive,
                int recoveryCount)
        {
            if (!hasCompleteActiveState && !runnerActive)
                return ProductionEntityStressReloadRecoveryDecision.None;
            if (!hasCompleteActiveState || !runnerActive ||
                recoveryCount >= ReloadRecoveryLimitForDiagnostics)
            {
                return ProductionEntityStressReloadRecoveryDecision.TerminalFailure;
            }
            return ProductionEntityStressReloadRecoveryDecision.RetryAfterCleanExit;
        }

        internal static ProductionEntityStressReloadRecoveryTransition
            ResolveReloadRecoveryTransition(
                bool recoveryPending,
                bool isPlaying,
                bool isPlayingOrWillChangePlaymode)
        {
            if (!recoveryPending)
                return ProductionEntityStressReloadRecoveryTransition.None;
            if (isPlaying)
                return ProductionEntityStressReloadRecoveryTransition.ExitPlayMode;
            if (isPlayingOrWillChangePlaymode)
                return ProductionEntityStressReloadRecoveryTransition.Wait;
            return ProductionEntityStressReloadRecoveryTransition.EnterPlayMode;
        }

        internal static string BuildActiveConfigJson(
            string requestJson,
            ProductionEntityStressConfig config)
        {
            var state = new ActiveRunConfigState
            {
                schemaVersion = 1,
                requestHash = BattleCanonicalJson.Sha256(requestJson ?? string.Empty),
                workloadFingerprint = ProductionEntityStressFingerprint.BuildWorkload(config),
                implementationConfigFingerprint =
                    ProductionEntityStressFingerprint.BuildImplementationConfig(config),
                mode = config.Mode.ToString(),
                inputMode = config.InputMode.ToString(),
                entityCount = config.EntityCount,
                warmupTicks = config.WarmupTicks,
                sampleTicks = config.SampleTicks,
                simulationOnly = config.SimulationOnly,
                skipLateRendererUpdate = config.SkipLateRendererUpdate,
                autoStopWhenSampled = config.ShouldAutoStopWhenSampled,
                seed = config.Seed.ToString(CultureInfo.InvariantCulture),
                outputPath = config.OutputPath,
            };
            return JsonUtility.ToJson(state);
        }

        internal static bool IsCompleteActiveRunState(
            string requestJson,
            string configJson)
        {
            if (string.IsNullOrWhiteSpace(requestJson) ||
                string.IsNullOrWhiteSpace(configJson))
            {
                return false;
            }

            try
            {
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(requestJson);
                ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot);
                ActiveRunConfigState state =
                    JsonUtility.FromJson<ActiveRunConfigState>(configJson);
                return state != null &&
                       state.schemaVersion == 1 &&
                       string.Equals(
                           state.requestHash,
                           BattleCanonicalJson.Sha256(requestJson),
                           StringComparison.Ordinal) &&
                       string.Equals(state.mode, config.Mode.ToString(), StringComparison.Ordinal) &&
                       string.Equals(
                           state.inputMode,
                           config.InputMode.ToString(),
                           StringComparison.Ordinal) &&
                       state.entityCount == config.EntityCount &&
                       state.warmupTicks == config.WarmupTicks &&
                       state.sampleTicks == config.SampleTicks &&
                       state.simulationOnly == config.SimulationOnly &&
                       state.skipLateRendererUpdate == config.SkipLateRendererUpdate &&
                       state.autoStopWhenSampled == config.ShouldAutoStopWhenSampled &&
                       string.Equals(
                           state.seed,
                           config.Seed.ToString(CultureInfo.InvariantCulture),
                           StringComparison.Ordinal) &&
                       string.Equals(
                           state.outputPath,
                           config.OutputPath,
                           StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        internal static void PollRequestForTests()
        {
            PollRequest();
        }

        private static void WriteRequestJson(string json)
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(json);
            if (ShouldSuppressBattleTestBootstrap(request?.action))
                ClearActiveRunRecoveryState(clearCount: true);
            ClearPlayRestartGuard();
            string requestPath = ProductionEntityStressPaths.RequestAbsolutePath;
            Directory.CreateDirectory(
                Path.GetDirectoryName(requestPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(requestPath, json, new UTF8Encoding(false));
            requestPending = true;
            StartPolling();
            SessionState.SetString(SessionRequestJsonKey, json);
            SessionState.SetString(
                SessionServiceWaitDeadlineKey,
                (Time.realtimeSinceStartupAsDouble + ServiceWaitTimeoutSecondsForDiagnostics)
                .ToString("R", CultureInfo.InvariantCulture));
            BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                ShouldSuppressBattleTestBootstrap(request?.action);

            string resultPath = ProductionEntityStressPaths.ResultAbsolutePath;
            if (File.Exists(resultPath))
                File.Delete(resultPath);
        }

        private static void PollRequest()
        {
            if (processing || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (ProductionEntityStressRunner.Active != null)
            {
                StopPolling();
                return;
            }
            if (ProcessReloadRecovery())
                return;
            if (!requestPending)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                requestPending = File.Exists(ProductionEntityStressPaths.RequestAbsolutePath);
                if (!requestPending)
                {
                    SessionState.EraseString(SessionRequestJsonKey);
                    ClearPlayRestartGuard();
                    return;
                }
            }

            string requestPath = ProductionEntityStressPaths.RequestAbsolutePath;

            processing = true;
            try
            {
                string json = File.ReadAllText(requestPath, Encoding.UTF8);
                SessionState.SetString(SessionRequestJsonKey, json);
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(json);
                string action = (request?.action ?? string.Empty).Trim().ToLowerInvariant();
                BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    ShouldSuppressBattleTestBootstrap(action);

                if (string.Equals(action, "stop", StringComparison.OrdinalIgnoreCase))
                {
                    if (EditorApplication.isPlaying && ProductionEntityStressRunner.Active != null)
                    {
                        ProductionEntityStressRunner.Active.StopAndCleanup("stop-request");
                    }
                    else
                    {
                        ProductionEntityStressPaths.WriteTerminalResult(
                            true,
                            string.Empty,
                            "No active production entity stress runner required cleanup.");
                    }
                    NotifyRunStopped();
                    CompleteRequest(requestPath);
                    return;
                }

                ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot);
                if (ShouldEnterPlayMode(action, EditorApplication.isPlaying))
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        if (SessionState.GetBool(SessionPlayRestartPendingKey, false))
                        {
                            ResetServiceWaitDeadline();
                            SessionState.SetBool(SessionPlayRestartPendingKey, false);
                        }
                        EditorApplication.EnterPlaymode();
                    }
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (ProductionEntityStressRunner.Active != null)
                    throw new InvalidOperationException(
                        "Stop the active production stress run before starting another one.");

                bool productionServicesReady =
                    ProductionEntityStressRunner.AreProductionServicesReady();
                ManagedRuntimeState managedRuntime = CaptureManagedRuntimeState();
                ProductionEntityStressPlayRestartDecision restartDecision =
                    EvaluatePlayRestartDecision(
                        true,
                        true,
                        SessionState.GetBool(SessionManagedRuntimeObservedKey, false),
                        managedRuntime.HasServiceFootprint,
                        managedRuntime.IsValid,
                        SessionState.GetBool(SessionPlayRestartPendingKey, false),
                        SessionState.GetInt(SessionPlayRestartCountKey, 0));
                switch (restartDecision)
                {
                    case ProductionEntityStressPlayRestartDecision.RecordHealthyRuntime:
                        SessionState.SetBool(SessionManagedRuntimeObservedKey, true);
                        SessionState.SetBool(SessionPlayRestartPendingKey, false);
                        break;
                    case ProductionEntityStressPlayRestartDecision.RestartPlayMode:
                        RequestCleanPlayRestart(managedRuntime);
                        return;
                    case ProductionEntityStressPlayRestartDecision.RetryLimitExceeded:
                        throw new InvalidOperationException(
                            "Production managed runtime was invalidated again after the single " +
                            $"clean Play Mode restart: {managedRuntime.Describe()}.");
                }

                bool hasActiveBootstrap = HasActiveBattleTestBootstrap();
                bool servicesReady = managedRuntime.IsValid && productionServicesReady;
                if (!IsReadyToStart(
                        hasActiveBootstrap,
                        BattleTestBootstrap.ProductionStressServicesReady,
                        servicesReady))
                {
                    string persistedDeadline = SessionState.GetString(
                        SessionServiceWaitDeadlineKey,
                        string.Empty);
                    double realtimeNow = Time.realtimeSinceStartupAsDouble;
                    double deadline = ResolveServiceWaitDeadline(
                        realtimeNow,
                        persistedDeadline);
                    string normalizedDeadline = deadline.ToString(
                        "R",
                        CultureInfo.InvariantCulture);
                    if (!string.Equals(
                            persistedDeadline,
                            normalizedDeadline,
                            StringComparison.Ordinal))
                    {
                        SessionState.SetString(
                            SessionServiceWaitDeadlineKey,
                            normalizedDeadline);
                    }
                    if (HasServiceWaitTimedOut(realtimeNow, deadline))
                    {
                        throw new TimeoutException(
                            "Timed out after 120 seconds waiting for " +
                            "BattleTestBootstrap-suppressed production services.");
                    }
                    return;
                }

                ProductionEntityStressRunner.StartRun(config);
                PersistActiveRun(json, config);
                WriteRunningResult(config.OutputPath);
                CompleteRequest(requestPath);
            }
            catch (Exception exception)
            {
                if (ProductionEntityStressRunner.Active != null)
                {
                    try
                    {
                        ProductionEntityStressRunner.Active.StopAndCleanup("request-failed");
                    }
                    catch (Exception cleanupException)
                    {
                        Debug.LogError(
                            "[ProductionEntityStress] Request failure cleanup also failed: " +
                            cleanupException);
                    }
                }
                ClearActiveRunRecoveryState(clearCount: true);
                ProductionEntityStressPaths.WriteTerminalResult(
                    false,
                    string.Empty,
                    exception.ToString());
                Debug.LogError($"[ProductionEntityStress] Request failed: {exception}");
                CompleteRequest(requestPath);
            }
            finally
            {
                processing = false;
            }
        }

        private static void WriteRunningResult(string reportPath)
        {
            string resultPath = ProductionEntityStressPaths.ResultAbsolutePath;
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(
                resultPath,
                "RUNNING\n" + reportPath,
                new UTF8Encoding(false));
        }

        private static void CompleteRequest(string requestPath)
        {
            try
            {
                if (File.Exists(requestPath))
                    File.Delete(requestPath);
            }
            finally
            {
                requestPending = false;
                SessionState.EraseString(SessionRequestJsonKey);
                SessionState.EraseString(SessionServiceWaitDeadlineKey);
                ClearPlayRestartGuard();
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
                if (!HasPendingReloadRecovery())
                    StopPolling();
            }
        }

        internal static void NotifyRunStopped()
        {
            ClearActiveRunRecoveryState(clearCount: true);
            ClearPlayRestartGuard();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
            if (!requestPending)
                StopPolling();
        }

        private static bool HasPendingReloadRecovery()
        {
            return SessionState.GetBool(SessionReloadRecoveryPendingKey, false) ||
                   SessionState.GetBool(SessionReloadRecoveryTransitionKey, false);
        }

        private static void StartPolling()
        {
            if (polling)
                return;

            EditorApplication.update += PollRequest;
            polling = true;
        }

        private static void StopPolling()
        {
            if (!polling)
                return;

            EditorApplication.update -= PollRequest;
            polling = false;
        }

        private static void PersistActiveRun(
            string requestJson,
            ProductionEntityStressConfig config)
        {
            SessionState.SetString(SessionActiveRequestJsonKey, requestJson);
            SessionState.SetString(
                SessionActiveConfigJsonKey,
                BuildActiveConfigJson(requestJson, config));
            SessionState.SetBool(SessionActiveStatePresentKey, true);
            SessionState.SetBool(SessionReloadRecoveryPendingKey, false);
            SessionState.SetBool(SessionReloadRecoveryDispatchedKey, false);
            SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
        }

        private static bool ProcessReloadRecovery()
        {
            bool pending = SessionState.GetBool(SessionReloadRecoveryPendingKey, false);
            bool transition = SessionState.GetBool(
                SessionReloadRecoveryTransitionKey,
                false);
            if (!pending && !transition)
            {
                if (ProductionEntityStressRunner.Active != null ||
                    !SessionState.GetBool(SessionActiveStatePresentKey, false))
                {
                    return false;
                }
            }

            string requestJson = SessionState.GetString(
                SessionActiveRequestJsonKey,
                string.Empty);
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            bool hasAnyActiveState = !string.IsNullOrWhiteSpace(requestJson) ||
                                     !string.IsNullOrWhiteSpace(configJson);

            if (!pending)
            {
                if (hasAnyActiveState && ProductionEntityStressRunner.Active == null && !transition)
                {
                    FailReloadRecovery(
                        "Persisted active-run state had no live runner after reload.");
                    return true;
                }
                return false;
            }

            if (transition)
                return true;

            if (!IsCompleteActiveRunState(requestJson, configJson) ||
                SessionState.GetInt(SessionReloadRecoveryCountKey, 0) !=
                ReloadRecoveryLimitForDiagnostics)
            {
                FailReloadRecovery(
                    "Reload recovery state was incomplete or exceeded its single retry contract.");
                return true;
            }

            bool dispatched = SessionState.GetBool(
                SessionReloadRecoveryDispatchedKey,
                false);
            if (dispatched)
                return !EditorApplication.isPlaying;

            ProductionEntityStressReloadRecoveryTransition action =
                ResolveReloadRecoveryTransition(
                    recoveryPending: true,
                    EditorApplication.isPlaying,
                    EditorApplication.isPlayingOrWillChangePlaymode);
            switch (action)
            {
                case ProductionEntityStressReloadRecoveryTransition.ExitPlayMode:
                    SessionState.SetBool(SessionReloadRecoveryTransitionKey, true);
                    EditorApplication.ExitPlaymode();
                    return true;
                case ProductionEntityStressReloadRecoveryTransition.EnterPlayMode:
                    PrepareReloadRecoveryRequest(requestJson);
                    SessionState.SetBool(SessionReloadRecoveryDispatchedKey, true);
                    SessionState.SetBool(SessionReloadRecoveryTransitionKey, true);
                    EditorApplication.EnterPlaymode();
                    return true;
                case ProductionEntityStressReloadRecoveryTransition.Wait:
                    return true;
                default:
                    return false;
            }
        }

        private static void PrepareReloadRecoveryRequest(string requestJson)
        {
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(requestPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(requestPath, requestJson, new UTF8Encoding(false));
            SessionState.SetString(SessionRequestJsonKey, requestJson);
            ResetServiceWaitDeadline();
            ClearPlayRestartGuard();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;

            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
        }

        private static void BeforeAssemblyReload()
        {
            if (SessionState.GetBool(SessionReloadRecoveryTransitionKey, false))
                return;

            string requestJson = SessionState.GetString(
                SessionActiveRequestJsonKey,
                string.Empty);
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            bool complete = IsCompleteActiveRunState(requestJson, configJson);
            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            int recoveryCount = SessionState.GetInt(SessionReloadRecoveryCountKey, 0);
            ProductionEntityStressReloadRecoveryDecision decision =
                EvaluateReloadRecoveryDecision(complete, runner != null, recoveryCount);
            if (decision == ProductionEntityStressReloadRecoveryDecision.None)
                return;
            if (decision == ProductionEntityStressReloadRecoveryDecision.TerminalFailure)
            {
                FailReloadRecovery(
                    recoveryCount >= ReloadRecoveryLimitForDiagnostics
                        ? "A second assembly reload interrupted the one allowed recovery run."
                        : "Assembly reload found incomplete active-run recovery state.");
                DestroyRunnerBeforeReload(runner);
                return;
            }

            SessionState.SetInt(SessionReloadRecoveryCountKey, recoveryCount + 1);
            SessionState.SetBool(SessionReloadRecoveryPendingKey, true);
            SessionState.SetBool(SessionReloadRecoveryDispatchedKey, false);
            SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            try
            {
                runner.StopAndCleanup(
                    "assembly-reload-recovery",
                    preserveRequestProcessorState: true);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[ProductionEntityStress] Best-effort assembly reload cleanup failed: " +
                    exception);
            }
            DestroyRunnerBeforeReload(runner);
            WriteReloadRecoveryPendingResult(
                TryReadActiveOutputPath(configJson),
                "Assembly reload interrupted the visible run; cleanup completed and one clean " +
                "Play Mode retry is pending.");
        }

        private static void WriteReloadRecoveryPendingResult(
            string reportPath,
            string evidence)
        {
            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(
                resultPath,
                "RESTARTING\n" + (reportPath ?? string.Empty) + "\n" +
                (evidence ?? string.Empty),
                new UTF8Encoding(false));
        }

        private static void DestroyRunnerBeforeReload(ProductionEntityStressRunner runner)
        {
            if (runner == null)
                return;
            try
            {
                UnityEngine.Object.DestroyImmediate(runner.gameObject);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[ProductionEntityStress] Could not destroy the cleaned runner before " +
                    "assembly reload: " + exception.Message);
            }
        }

        private static void FailReloadRecovery(string reason)
        {
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            if (runner != null)
            {
                try
                {
                    runner.StopAndCleanup(
                        "assembly-reload-terminal-failure",
                        preserveRequestProcessorState: true);
                }
                catch (Exception exception)
                {
                    reason += " Cleanup failure: " + exception.Message;
                }
            }

            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            if (File.Exists(requestPath))
                File.Delete(requestPath);
            SessionState.EraseString(SessionRequestJsonKey);
            SessionState.EraseString(SessionServiceWaitDeadlineKey);
            ClearPlayRestartGuard();
            ClearActiveRunRecoveryState(clearCount: true);
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
            ProductionEntityStressPaths.WriteTerminalResult(
                false,
                TryReadActiveOutputPath(configJson),
                reason);
            Debug.LogError("[ProductionEntityStress] " + reason);
        }

        private static string TryReadActiveOutputPath(string configJson)
        {
            try
            {
                return JsonUtility.FromJson<ActiveRunConfigState>(configJson)?.outputPath ??
                       string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
            }
        }

        private static void ClearActiveRunRecoveryState(bool clearCount)
        {
            SessionState.EraseString(SessionActiveRequestJsonKey);
            SessionState.EraseString(SessionActiveConfigJsonKey);
            SessionState.EraseBool(SessionActiveStatePresentKey);
            SessionState.EraseBool(SessionReloadRecoveryPendingKey);
            SessionState.EraseBool(SessionReloadRecoveryDispatchedKey);
            SessionState.EraseBool(SessionReloadRecoveryTransitionKey);
            if (clearCount)
                SessionState.EraseInt(SessionReloadRecoveryCountKey);
        }

        private static ManagedRuntimeState CaptureManagedRuntimeState()
        {
            bool driverComponentPresent = HasActiveSceneComponent<SimulationTickDriver>();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            bool driverSingletonPresent = driver != null;
            bool worldPresent = driver?.World != null;
            bool poolComponentPresent = HasActiveSceneComponent<LF2ObjectPool>();
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            bool poolSingletonPresent = pool != null;
            bool poolRuntimeStateValid = pool != null &&
                                         pool.IsRuntimeStateValidForAcceptance;
            return new ManagedRuntimeState(
                driverComponentPresent,
                driverSingletonPresent,
                worldPresent,
                poolComponentPresent,
                poolSingletonPresent,
                poolRuntimeStateValid);
        }

        private static bool HasActiveSceneComponent<T>() where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int index = 0; index < components.Length; index++)
            {
                T component = components[index];
                if (component != null && component.gameObject.scene.IsValid() &&
                    component.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        private static void RequestCleanPlayRestart(ManagedRuntimeState state)
        {
            int restartCount = SessionState.GetInt(SessionPlayRestartCountKey, 0) + 1;
            SessionState.SetInt(SessionPlayRestartCountKey, restartCount);
            SessionState.SetBool(SessionManagedRuntimeObservedKey, false);
            SessionState.SetBool(SessionPlayRestartPendingKey, true);
            ResetServiceWaitDeadline();
            Debug.LogWarning(
                "[ProductionEntityStress] Managed runtime invalidated during a pending request; " +
                $"exiting Play Mode for clean restart {restartCount}/" +
                $"{PlayRestartLimitForDiagnostics}. {state.Describe()}");
            EditorApplication.ExitPlaymode();
        }

        private static void ResetServiceWaitDeadline()
        {
            SessionState.SetString(
                SessionServiceWaitDeadlineKey,
                (Time.realtimeSinceStartupAsDouble + ServiceWaitTimeoutSecondsForDiagnostics)
                .ToString("R", CultureInfo.InvariantCulture));
        }

        private static void ClearPlayRestartGuard()
        {
            SessionState.EraseBool(SessionManagedRuntimeObservedKey);
            SessionState.EraseBool(SessionPlayRestartPendingKey);
            SessionState.EraseInt(SessionPlayRestartCountKey);
        }

        [Serializable]
        private sealed class ActiveRunConfigState
        {
            public int schemaVersion;
            public string requestHash;
            public string workloadFingerprint;
            public string implementationConfigFingerprint;
            public string mode;
            public string inputMode;
            public int entityCount;
            public int warmupTicks;
            public int sampleTicks;
            public bool simulationOnly;
            public bool skipLateRendererUpdate;
            public bool autoStopWhenSampled;
            public string seed;
            public string outputPath;
        }

        private readonly struct ManagedRuntimeState
        {
            internal ManagedRuntimeState(
                bool driverComponentPresent,
                bool driverSingletonPresent,
                bool worldPresent,
                bool poolComponentPresent,
                bool poolSingletonPresent,
                bool poolRuntimeStateValid)
            {
                DriverComponentPresent = driverComponentPresent;
                DriverSingletonPresent = driverSingletonPresent;
                WorldPresent = worldPresent;
                PoolComponentPresent = poolComponentPresent;
                PoolSingletonPresent = poolSingletonPresent;
                PoolRuntimeStateValid = poolRuntimeStateValid;
            }

            internal bool DriverComponentPresent { get; }
            internal bool DriverSingletonPresent { get; }
            internal bool WorldPresent { get; }
            internal bool PoolComponentPresent { get; }
            internal bool PoolSingletonPresent { get; }
            internal bool PoolRuntimeStateValid { get; }
            internal bool HasServiceFootprint =>
                DriverComponentPresent || PoolComponentPresent;
            internal bool IsValid =>
                DriverComponentPresent &&
                DriverSingletonPresent &&
                WorldPresent &&
                PoolComponentPresent &&
                PoolSingletonPresent &&
                PoolRuntimeStateValid;

            internal string Describe()
            {
                return $"driverComponent={DriverComponentPresent}, " +
                       $"driverSingleton={DriverSingletonPresent}, " +
                       $"world={WorldPresent}, poolComponent={PoolComponentPresent}, " +
                       $"poolSingleton={PoolSingletonPresent}, " +
                       $"poolRuntime={PoolRuntimeStateValid}";
            }
        }

        private static bool HasActiveBattleTestBootstrap()
        {
            BattleTestBootstrap[] bootstraps =
                Resources.FindObjectsOfTypeAll<BattleTestBootstrap>();
            for (int i = 0; i < bootstraps.Length; i++)
            {
                BattleTestBootstrap bootstrap = bootstraps[i];
                if (bootstrap != null && bootstrap.gameObject.scene.IsValid() &&
                    bootstrap.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ConfigureBootstrapSuppressionFromPendingRequest()
        {
            try
            {
                if (SessionState.GetBool(SessionReloadRecoveryPendingKey, false))
                {
                    BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                    return;
                }
                string requestPath = ProductionEntityStressPaths.ProjectPath(
                    ProductionEntityStressPaths.RequestFile);
                if (!File.Exists(requestPath))
                    return;
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(
                        File.ReadAllText(requestPath, Encoding.UTF8));
                BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    ShouldSuppressBattleTestBootstrap(request?.action);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[ProductionEntityStress] Could not initialize bootstrap suppression: {exception.Message}");
            }
        }
    }
}
#endif
