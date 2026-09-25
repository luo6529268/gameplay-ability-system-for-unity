#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Input;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Explicit Editor-only probe for the real NTSD_Battle scene. It queues the
    /// physical combo sequences through the Input System and records the
    /// resulting fixed-tick combo/frame progression.
    /// </summary>
    public static class BattleComboPlayModeProbeEditor
    {
        private const string DownJumpMenuPath = "NTSD/验证/运行组合键PlayMode探针";
        private const string ForwardAttackMenuPath = "NTSD/验证/运行组合键PlayMode探针-防前攻";
        private const string DownJumpResultPath = "Temp/NTSD_R3_COMBO_PLAY.result.json";
        private const string ForwardAttackResultPath = "Temp/NTSD_R3_COMBO_PLAY.forward-attack.result.json";
        private const string DownJumpRequestPath =
            "Temp/NTSD_R3_COMBO_PLAY.request";
        private const string ForwardAttackRequestPath =
            "Temp/NTSD_R3_COMBO_PLAY.forward-attack.request";
        private const string Q07RequestPath =
            "Temp/NTSD28_Q07_NarutoForwardAttack.request.json";
        private const string Q07SasukeRequestPath =
            "Temp/NTSD28_Q07_SasukeNeedle.request.json";
        private const string Q07SasukeMotionRequestPath =
            "Temp/NTSD28_Q07_SasukeState15Motion.request.json";
        private const string Q07DdjStateRequestPath =
            "Temp/NTSD28_Q07_DdjState.request.json";
        private const string Q07DdjContinuousRequestPath =
            "Temp/NTSD28_Q07_DdjContinuous.request.json";
        private const string Q07ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001";
        private const string Q07CloneSpriteResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001";
        private const string Q07SasukeResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001";
        private const string Q07SasukeMotionResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-SASUKE-NATURAL-STATE15-MOTION-001";
        private const string Q07DdjResultPath =
            "artifacts/diagnostics/NTSD28-Q07-FORMAL-DDJ-PROBE-001/formal-ddj-play-3.json";
        private const string Q07DdjStateResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-DDJ-SAME-STATE-WITNESS-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "B8B13894088DDE96D71771C9110FBD84E2C03AE712FE32222B99C5DFE8155A45";
        private const string ProjectModeFormalFingerprint =
            "27CAE01489909C46A5145A5867988CCD8C10E20FEF165D847B7AC7A6DE2DE02D";
        private const int ObservationTailTicks = 18;
        private const int TimeoutTicks = 90;
        private const int MaximumPressAttemptsPerStep = 8;
        private const int MinimumQueuedStateHoldTicks = 2;

        private static readonly List<TraceRow> Trace = new List<TraceRow>(128);
        private static readonly List<LF2Entity> ObservedEntities = new List<LF2Entity>(32);
        private static readonly HashSet<int> SasukeChildStableIds = new HashSet<int>();
        private static readonly List<LF2Entity> SasukeChildrenThisTick = new List<LF2Entity>(8);
        private static readonly List<SasukeChildBirth> SasukeChildBirths = new List<SasukeChildBirth>(4);
        private static readonly List<SasukeChildMotion> SasukeMotionRows =
            new List<SasukeChildMotion>(24);
        private static readonly HashSet<int> SasukeAction12StableIds = new HashSet<int>();
        private static readonly List<QueuedInputRow> QueuedInputs = new List<QueuedInputRow>(8);
        private static readonly FieldInfo FirstPlayerField =
            typeof(BattleTestBootstrap).GetField(
                "firstPlayerLf2",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static LF2Character character;
        private static SimulationTickDriver driver;
        private static Keyboard keyboard;
        private static int startTick;
        private static int step1Tick;
        private static int step2Tick;
        private static int step3Tick;
        private static int releaseTick;
        private static int expectedTargetFrame;
        private static int expectedResolvedAction;
        private static int expectedFullTickFrame;
        private static int observedResolvedAction;
        private static int baselinePP;
        private static int resolvedPP;
        private static int baselineObjectCount;
        private static int peakObjectCount;
        private static int peakOid518Count;
        private static int firstOid518Tick;
        private static int peakOid33Count;
        private static int lastObservedTick;
        private static int lastInputPulseTick;
        private static int step1PressAttempts;
        private static int step2PressAttempts;
        private static int step3PressAttempts;
        private static bool step1Seen;
        private static bool step2Seen;
        private static bool targetFrameSeen;
        private static bool retryReleaseQueued;
        private static bool running;
        private static bool requestBatchActive;
        private static ProbeKind activeKind;
        private static Key secondPhysicalKey;
        private static Key thirdPhysicalKey;
        private static string comboLabel;
        private static string resultRelativePath;
        private static bool q07FormalActive;
        private static bool q07FormalDdjAudit;
        private static bool q07CloneSpriteAudit;
        private static bool q07SasukeNeedleAudit;
        private static bool q07SasukeState15MotionAudit;
        private static bool q07DdjStateAudit;
        private static bool controlledTickStepping;
        private static bool initialPaused;
        private static InitialState ddjInitialState;
        private static int missedObservedTicks;
        private static bool cloneHiddenPicObserved;
        private static bool cloneVisiblePicObserved;
        private static bool sasukeFrame264Seen;
        private static bool sasukeChildPic0BindingObserved;
        private static int peakOid440Count;
        private static int sasukeBirthParentX;
        private static int sasukeBirthParentY;
        private static int sasukeBirthParentZ;
        private static bool sasukeAction12VxZero;
        private static string formalSourceKey;

        [Serializable]
        private sealed class Q07Request
        {
            public bool requested;
            public bool cloneSpriteAudit;
            public string runId;
        }

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
            EditorApplication.playModeStateChanged -= ConfigureSasukePlayClone;
            EditorApplication.playModeStateChanged += ConfigureSasukePlayClone;
        }

        private static void ConfigureSasukePlayClone(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;
            bool sasukeRequested = false;
            foreach (string relativePath in new[]
            {
                Q07SasukeMotionRequestPath, Q07SasukeRequestPath
            })
            {
                string path = ProjectPath(relativePath);
                if (!File.Exists(path))
                    continue;
                try
                {
                    Q07Request request = JsonUtility.FromJson<Q07Request>(File.ReadAllText(path));
                    if (request != null && request.requested)
                    {
                        sasukeRequested = true;
                        break;
                    }
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (!sasukeRequested)
                return;
            BattleTestBootstrap bootstrap =
                UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            FieldInfo overrideField = typeof(BattleTestBootstrap).GetField(
                "overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic);
            if (bootstrap != null && overrideField != null)
                overrideField.SetValue(bootstrap, new[] { 11 });
        }

        private static void PollRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || running)
            {
                return;
            }

            string sasukeMotionPath = ProjectPath(Q07SasukeMotionRequestPath);
            string sasukePath = ProjectPath(Q07SasukeRequestPath);
            if (File.Exists(sasukeMotionPath))
            {
                try
                {
                    Q07Request motionRequest = JsonUtility.FromJson<Q07Request>(
                        File.ReadAllText(sasukeMotionPath));
                    if (motionRequest != null && motionRequest.requested)
                        sasukePath = sasukeMotionPath;
                }
                catch (IOException)
                {
                    return;
                }
            }
            if (File.Exists(sasukePath))
            {
                Q07Request sasukeRequest;
                try
                {
                    sasukeRequest = JsonUtility.FromJson<Q07Request>(File.ReadAllText(sasukePath));
                }
                catch (IOException)
                {
                    return;
                }
                if (sasukeRequest != null && sasukeRequest.requested)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        if (!EditorApplication.isPlayingOrWillChangePlaymode)
                            EditorApplication.EnterPlaymode();
                        return;
                    }
                    if (!IsLiveBattleReady() || !CanStartProbe(ProbeKind.ForwardAttack))
                        return;
                    bool motionAudit = sasukePath == sasukeMotionPath;
                    if (!IsValidQ07RunId(sasukeRequest.runId) ||
                        File.Exists(ProjectPath(Path.Combine(
                            motionAudit ? Q07SasukeMotionResultRoot : Q07SasukeResultRoot,
                            sasukeRequest.runId + ".json"))))
                    {
                        sasukeRequest.requested = false;
                        File.WriteAllText(sasukePath, JsonUtility.ToJson(sasukeRequest));
                        Debug.LogError("[BattleComboPlayModeProbe] Invalid or existing Sasuke Q07 result.");
                        return;
                    }
                    sasukeRequest.requested = false;
                    File.WriteAllText(sasukePath, JsonUtility.ToJson(sasukeRequest));
                    requestBatchActive = true;
                    RunProbe(ProbeKind.ForwardAttack, sasukeRequest.runId, false, true,
                        sasukeState15MotionAudit: motionAudit);
                    return;
                }
            }

            string q07Path = ProjectPath(Q07RequestPath);
            if (File.Exists(q07Path))
            {
                Q07Request q07Request;
                try
                {
                    q07Request = JsonUtility.FromJson<Q07Request>(File.ReadAllText(q07Path));
                }
                catch (IOException)
                {
                    return;
                }
                if (q07Request != null && q07Request.requested)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        if (!EditorApplication.isPlayingOrWillChangePlaymode)
                            EditorApplication.EnterPlaymode();
                        return;
                    }
                    if (!IsLiveBattleReady() || !CanStartProbe(ProbeKind.ForwardAttack))
                        return;
                    if (!IsValidQ07RunId(q07Request.runId))
                    {
                        q07Request.requested = false;
                        File.WriteAllText(q07Path, JsonUtility.ToJson(q07Request));
                        Debug.LogError("[BattleComboPlayModeProbe] Invalid Q07 run ID.");
                        return;
                    }
                    string output = ProjectPath(Path.Combine(
                        q07Request.cloneSpriteAudit ? Q07CloneSpriteResultRoot : Q07ResultRoot,
                        q07Request.runId + ".json"));
                    if (File.Exists(output))
                    {
                        q07Request.requested = false;
                        File.WriteAllText(q07Path, JsonUtility.ToJson(q07Request));
                        Debug.LogError("[BattleComboPlayModeProbe] Q07 output already exists: " + output);
                        return;
                    }
                    q07Request.requested = false;
                    File.WriteAllText(q07Path, JsonUtility.ToJson(q07Request));
                    requestBatchActive = true;
                    RunProbe(ProbeKind.ForwardAttack, q07Request.runId, q07Request.cloneSpriteAudit);
                    return;
                }
            }

            foreach (string relativePath in new[]
            {
                Q07DdjStateRequestPath, Q07DdjContinuousRequestPath
            })
            {
                string ddjStatePath = ProjectPath(relativePath);
                if (!File.Exists(ddjStatePath))
                    continue;
                Q07Request ddjStateRequest;
                try
                {
                    ddjStateRequest = JsonUtility.FromJson<Q07Request>(File.ReadAllText(ddjStatePath));
                }
                catch (IOException)
                {
                    return;
                }
                if (ddjStateRequest != null && ddjStateRequest.requested)
                {
                    string output = IsValidQ07RunId(ddjStateRequest.runId)
                        ? ProjectPath(Path.Combine(Q07DdjStateResultRoot,
                            ddjStateRequest.runId + ".json"))
                        : null;
                    if (output == null || File.Exists(output))
                    {
                        ddjStateRequest.requested = false;
                        File.WriteAllText(ddjStatePath, JsonUtility.ToJson(ddjStateRequest));
                        Debug.LogError("[BattleComboPlayModeProbe] Invalid or existing DDJ state result.");
                        return;
                    }
                    if (!EditorApplication.isPlaying)
                    {
                        if (!EditorApplication.isPlayingOrWillChangePlaymode)
                            EditorApplication.EnterPlaymode();
                        return;
                    }
                    if (!IsLiveBattleReady() || !CanStartProbe(ProbeKind.DownJump))
                        return;
                    ddjStateRequest.requested = false;
                    File.WriteAllText(ddjStatePath, JsonUtility.ToJson(ddjStateRequest));
                    requestBatchActive = true;
                    RunProbe(ProbeKind.DownJump, ddjStateRequest.runId,
                        ddjStateAudit: true);
                    return;
                }
            }

            if (!EditorApplication.isPlaying)
                return;

            if (!HasPendingRequest() || !IsLiveBattleReady())
                return;

            string downJump = ProjectPath(DownJumpRequestPath);
            if (File.Exists(downJump))
            {
                if (!CanStartProbe(ProbeKind.DownJump))
                    return;
                if (GameConfig.Instance?.BattleContentRuntimeRoot == FormalRoot &&
                    File.Exists(ProjectPath(Q07DdjResultPath)))
                {
                    File.Delete(downJump);
                    Debug.LogError("[BattleComboPlayModeProbe] Formal DDJ output already exists.");
                    return;
                }
                requestBatchActive = true;
                File.Delete(downJump);
                if (GameConfig.Instance?.BattleContentRuntimeRoot != FormalRoot)
                    DeletePriorResult(DownJumpResultPath);
                RunProbe(ProbeKind.DownJump);
                return;
            }

            string forwardAttack = ProjectPath(ForwardAttackRequestPath);
            if (File.Exists(forwardAttack))
            {
                if (!CanStartProbe(ProbeKind.ForwardAttack))
                    return;
                requestBatchActive = true;
                File.Delete(forwardAttack);
                DeletePriorResult(ForwardAttackResultPath);
                RunProbe(ProbeKind.ForwardAttack);
            }
        }

        private static bool IsValidQ07RunId(string runId)
        {
            return !string.IsNullOrEmpty(runId) &&
                System.Linq.Enumerable.All(runId,
                    value => char.IsLetterOrDigit(value) || value == '-');
        }

        [MenuItem(DownJumpMenuPath)]
        public static void RunFromMenu()
        {
            RunProbe(ProbeKind.DownJump);
        }

        [MenuItem(ForwardAttackMenuPath)]
        public static void RunForwardAttackFromMenu()
        {
            RunProbe(ProbeKind.ForwardAttack);
        }

        private static void RunProbe(
            ProbeKind kind,
            string q07RunId = null,
            bool cloneSpriteAudit = false,
            bool sasukeNeedleAudit = false,
            bool ddjStateAudit = false,
            bool sasukeState15MotionAudit = false)
        {
            activeKind = kind;
            q07FormalActive = q07RunId != null;
            q07FormalDdjAudit = false;
            q07CloneSpriteAudit = q07FormalActive && cloneSpriteAudit;
            q07SasukeNeedleAudit = q07FormalActive && sasukeNeedleAudit;
            q07SasukeState15MotionAudit = q07SasukeNeedleAudit &&
                sasukeState15MotionAudit;
            q07DdjStateAudit = q07FormalActive && ddjStateAudit;
            resultRelativePath = q07FormalActive
                ? Path.Combine(
                    q07DdjStateAudit ? Q07DdjStateResultRoot :
                        q07SasukeState15MotionAudit ? Q07SasukeMotionResultRoot :
                        q07SasukeNeedleAudit ? Q07SasukeResultRoot :
                        q07CloneSpriteAudit ? Q07CloneSpriteResultRoot : Q07ResultRoot,
                    q07RunId + ".json")
                : kind == ProbeKind.DownJump &&
                  GameConfig.Instance?.BattleContentRuntimeRoot == FormalRoot
                    ? Q07DdjResultPath
                    : kind == ProbeKind.DownJump ? DownJumpResultPath : ForwardAttackResultPath;
            StopObservation();
            if (!EditorApplication.isPlaying)
            {
                WriteFailure("Play Mode is not active.");
                return;
            }

            BattleTestBootstrap bootstrap = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            character = FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            driver = SimulationTickDriver.Instance;
            if (bootstrap == null || character == null || driver?.World == null ||
                character.Controller?.InputBuffer == null || character.Runtime == null ||
                character.Frame?.D == null)
            {
                WriteFailure("The live battle player/input/runtime is not ready.");
                return;
            }

            if (q07FormalActive)
            {
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                formalSourceKey = manager?.PublishedVisualContentKey;
                bool authoredTargetMatches = q07DdjStateAudit
                    ? character.Frame.D.hit_Dj == 271
                    : character.Frame.D.hit_Fa == (q07SasukeNeedleAudit ? 261 : 285);
                if (GameConfig.Instance?.BattleContentRuntimeRoot != FormalRoot ||
                    manager?.PublishedLoganContentIdentity?.SemanticFingerprint !=
                        (q07DdjStateAudit || q07SasukeNeedleAudit
                            ? ProjectModeFormalFingerprint : FormalFingerprint) ||
                    character.ObjectId != (q07SasukeNeedleAudit ? 11 : 2) ||
                    !authoredTargetMatches ||
                    string.IsNullOrEmpty(formalSourceKey))
                {
                    WriteFailure(q07SasukeNeedleAudit
                        ? $"Q07 Sasuke preflight: root={GameConfig.Instance?.BattleContentRuntimeRoot}, " +
                          $"fingerprint={manager?.PublishedLoganContentIdentity?.SemanticFingerprint}, " +
                          $"oid={character.ObjectId}, hit_Fa={character.Frame.D.hit_Fa}, " +
                          $"visualKeyPresent={!string.IsNullOrEmpty(formalSourceKey)}."
                        : q07DdjStateAudit
                            ? $"Q07 Naruto DDJ preflight: root={GameConfig.Instance?.BattleContentRuntimeRoot}, " +
                              $"fingerprint={manager?.PublishedLoganContentIdentity?.SemanticFingerprint}, " +
                              $"oid={character.ObjectId}, hit_Dj={character.Frame.D.hit_Dj}, " +
                              $"visualKeyPresent={!string.IsNullOrEmpty(formalSourceKey)}."
                            : "Q07 Naruto preflight did not find formal root, fingerprint, OID2 and hit_Fa 285.");
                    return;
                }
            }

            keyboard = Keyboard.current;
            if (keyboard == null)
            {
                WriteFailure("Unity Input System has no current Keyboard device.");
                return;
            }

            bool facingLeft = string.Equals(
                character.Runtime.Dir,
                "left",
                StringComparison.OrdinalIgnoreCase);
            if (kind == ProbeKind.DownJump)
            {
                expectedTargetFrame = character.Frame.D.hit_Dj;
                secondPhysicalKey = Key.S;
                thirdPhysicalKey = Key.K;
                comboLabel = "DDJ";
            }
            else
            {
                expectedTargetFrame = character.Frame.D.hit_Fa;
                secondPhysicalKey = facingLeft ? Key.A : Key.D;
                thirdPhysicalKey = Key.J;
                comboLabel = facingLeft ? "DLA" : "DRA";
            }
            if (expectedTargetFrame == 0)
            {
                WriteFailure(
                    $"Current frame {character.Frame.N} has no authored target for {comboLabel}.");
                return;
            }

            expectedResolvedAction = expectedTargetFrame;
            expectedFullTickFrame = expectedTargetFrame;
            if (kind == ProbeKind.DownJump &&
                GameConfig.Instance?.BattleContentRuntimeRoot == FormalRoot)
            {
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                LF2FrameData requested = character.FrameCache?.GetNativeFrameDataById(
                    expectedTargetFrame);
                LF2FrameData resolved = character.FrameCache?.GetNativeFrameDataById(272);
                if (manager?.PublishedLoganContentIdentity == null ||
                    character.ObjectId != 2 || expectedTargetFrame != 271 ||
                    requested?.state != 1150272 || requested.mp != 350 ||
                    resolved?.wait != 0 || resolved.next != 495 ||
                    character.Health.HP <= 150 || character.Health.PP < 350)
                {
                    WriteFailure(
                        $"Q07 formal DDJ preflight: fingerprint={manager?.PublishedLoganContentIdentity?.SemanticFingerprint}, " +
                        $"oid={character.ObjectId}, requested={expectedTargetFrame}, " +
                        $"state={requested?.state}, mp={requested?.mp}, " +
                        $"resolvedWait={resolved?.wait}, resolvedNext={resolved?.next}, " +
                        $"hp={character.Health.HP}, pp={character.Health.PP}.");
                    return;
                }
                q07FormalDdjAudit = true;
                expectedResolvedAction = 272;
                expectedFullTickFrame = 495;
                formalSourceKey = manager.PublishedVisualContentKey;
            }

            int currentTick = driver.CurrentTickIndex;
            startTick = currentTick;
            step1Tick = -1;
            step2Tick = -1;
            step3Tick = -1;
            releaseTick = -1;
            baselineObjectCount = driver.World.ObjectCount;
            peakObjectCount = baselineObjectCount;
            peakOid518Count = 0;
            firstOid518Tick = -1;
            observedResolvedAction = -1;
            baselinePP = character.Health.PP;
            resolvedPP = -1;
            peakOid33Count = 0;
            peakOid440Count = 0;
            sasukeFrame264Seen = false;
            sasukeChildPic0BindingObserved = false;
            SasukeChildStableIds.Clear();
            SasukeChildBirths.Clear();
            SasukeMotionRows.Clear();
            SasukeAction12StableIds.Clear();
            sasukeAction12VxZero = true;
            sasukeBirthParentX = 0;
            sasukeBirthParentY = 0;
            sasukeBirthParentZ = 0;
            cloneHiddenPicObserved = false;
            cloneVisiblePicObserved = false;
            lastObservedTick = currentTick;
            lastInputPulseTick = currentTick;
            step1PressAttempts = 1;
            step2PressAttempts = 0;
            step3PressAttempts = 0;
            step1Seen = false;
            step2Seen = false;
            targetFrameSeen = false;
            retryReleaseQueued = false;
            Trace.Clear();
            QueuedInputs.Clear();
            missedObservedTicks = 0;
            ddjInitialState = q07DdjStateAudit ? CaptureInitialState() : null;

            if (q07DdjStateAudit)
            {
                initialPaused = driver.IsPaused;
                driver.SetPaused(true);
                controlledTickStepping = true;
            }

            running = true;
            EditorApplication.update += Observe;
            // Alignment contract: R3-COMBO-001. Queue physical device states so
            // CharacterInputModule and canonical FrameInputSet remain in the path.
            QueueKeyboardState(Key.L);
            Debug.Log(
                $"[BattleComboPlayModeProbe] queued {activeKind} physical L at tick {startTick}; " +
                $"combo={comboLabel},target={expectedTargetFrame}.");
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || character == null || driver?.World == null)
            {
                Finish(false, "Play Mode or live character ended before observation completed.");
                return;
            }

            int tick = driver.CurrentTickIndex;
            if (controlledTickStepping && tick == lastObservedTick)
            {
                try
                {
                    if (!driver.StepOneTick(ignorePaused: true))
                    {
                        Finish(false, "Controlled single tick could not consume the normal local input provider.");
                        return;
                    }
                    tick = driver.CurrentTickIndex;
                }
                catch (Exception exception)
                {
                    Finish(false, "Controlled single tick failed: " + exception);
                    return;
                }
            }
            if (tick <= lastObservedTick)
                return;

            if ((q07DdjStateAudit || q07SasukeState15MotionAudit) &&
                tick > lastObservedTick + 1)
                missedObservedTicks += tick - lastObservedTick - 1;

            lastObservedTick = tick;
            int frame = character.Frame?.N ?? -1;
            if (q07SasukeNeedleAudit && frame == 264)
                sasukeFrame264Seen = true;
            byte combo = ResolveComboValue();
            int objectCount = driver.World.ObjectCount;
            peakObjectCount = Math.Max(peakObjectCount, objectCount);
            int oid33Count = 0;
            int oid440Count = 0;
            int oid518Count = 0;
            int cloneFrame = -1;
            int clonePic = -1;
            int cloneVisualDataId = -1;
            int cloneEffectivePic = -1;
            string cloneSourceSheet = string.Empty;
            float clonePixelWidth = 0f;
            float clonePixelHeight = 0f;
            bool cloneSpriteResolved = false;
            bool cloneCentralBindingValid = false;
            if (q07FormalActive || q07FormalDdjAudit)
            {
                ObservedEntities.Clear();
                SasukeChildrenThisTick.Clear();
                driver.World.GetAllEntities(ObservedEntities);
                foreach (LF2Entity entity in ObservedEntities)
                {
                    if (q07FormalDdjAudit && entity != null && entity.ObjectId == 518)
                        oid518Count++;
                    if (q07SasukeNeedleAudit && entity != null && entity.ObjectId == 440)
                    {
                        oid440Count++;
                        SasukeChildStableIds.Add(entity.Runtime.StableId);
                        SasukeChildrenThisTick.Add(entity);
                        int childFrame = entity.Frame?.N ?? -1;
                        if (q07SasukeState15MotionAudit &&
                            childFrame >= 12 && childFrame <= 14)
                        {
                            SasukeMotionRows.Add(new SasukeChildMotion
                            {
                                tick = tick,
                                stableId = entity.Runtime.StableId,
                                slot = entity.Runtime.SlotIndex,
                                frame = childFrame,
                                x = entity.Runtime.XInt,
                                y = entity.Runtime.YInt,
                                z = entity.Runtime.ZInt,
                                vx = entity.PS.vx,
                                vy = entity.PS.vy,
                                vz = entity.PS.vz,
                            });
                            if (childFrame == 12)
                            {
                                SasukeAction12StableIds.Add(entity.Runtime.StableId);
                                if (Math.Abs(entity.PS.vx) > 0.00001)
                                    sasukeAction12VxZero = false;
                            }
                        }
                        if (entity.Frame?.N == 1 && entity.GetRenderPicIndex() == 0 &&
                            entity.TryResolveCurrentSpriteEntry(out BattleSpriteEntry sasukeEntry) &&
                            sasukeEntry != null && sasukeEntry.Key.VisualDataId == 440 &&
                            sasukeEntry.Key.EffectivePic == 0 &&
                            sasukeEntry.SourceSheetPath.Replace('\\', '/').EndsWith(
                                "c/sasu/a/chi.png", StringComparison.OrdinalIgnoreCase) &&
                            Math.Abs(sasukeEntry.PixelWidth - 81f) < 0.01f &&
                            Math.Abs(sasukeEntry.PixelHeight - 82f) < 0.01f &&
                            sasukeEntry.CentralBinding.IsValid)
                            sasukeChildPic0BindingObserved = true;
                    }
                    if (entity != null && entity.ObjectId == 33)
                    {
                        oid33Count++;
                        if (!q07CloneSpriteAudit || cloneFrame >= 0)
                            continue;
                        cloneFrame = entity.Frame?.N ?? -1;
                        clonePic = entity.GetRenderPicIndex();
                        cloneSpriteResolved = entity.TryResolveCurrentSpriteEntry(
                            out BattleSpriteEntry cloneEntry);
                        if (cloneSpriteResolved && cloneEntry != null)
                        {
                            cloneVisualDataId = cloneEntry.Key.VisualDataId;
                            cloneEffectivePic = cloneEntry.Key.EffectivePic;
                            cloneSourceSheet = cloneEntry.SourceSheetPath;
                            clonePixelWidth = cloneEntry.PixelWidth;
                            clonePixelHeight = cloneEntry.PixelHeight;
                            cloneCentralBindingValid = cloneEntry.CentralBinding.IsValid;
                        }
                        if ((cloneFrame == 240 || cloneFrame == 241) &&
                            clonePic == 999 && !cloneSpriteResolved)
                            cloneHiddenPicObserved = true;
                        if (cloneFrame == 242 && clonePic == 1 && cloneSpriteResolved &&
                            cloneVisualDataId == 33 && cloneEffectivePic == 1 &&
                            cloneSourceSheet.Replace('\\', '/').EndsWith(
                                "c/nar/ncl.png", StringComparison.OrdinalIgnoreCase) &&
                            Math.Abs(clonePixelWidth - 79f) < 0.01f &&
                            Math.Abs(clonePixelHeight - 79f) < 0.01f &&
                            cloneCentralBindingValid)
                        {
                            cloneVisiblePicObserved = true;
                        }
                    }
                }
                peakOid33Count = Math.Max(peakOid33Count, oid33Count);
                peakOid440Count = Math.Max(peakOid440Count, oid440Count);
                peakOid518Count = Math.Max(peakOid518Count, oid518Count);
                if (oid518Count > 0 && firstOid518Tick < 0)
                    firstOid518Tick = tick;
                if (q07SasukeNeedleAudit && oid440Count == 4 && SasukeChildBirths.Count == 0)
                {
                    sasukeBirthParentX = character.Runtime.XInt;
                    sasukeBirthParentY = character.Runtime.YInt;
                    sasukeBirthParentZ = character.Runtime.ZInt;
                    SasukeChildrenThisTick.Sort((left, right) =>
                        left.Runtime.StableId.CompareTo(right.Runtime.StableId));
                    foreach (LF2Entity child in SasukeChildrenThisTick)
                    {
                        SasukeChildBirths.Add(new SasukeChildBirth
                        {
                            stableId = child.Runtime.StableId,
                            slot = child.Runtime.SlotIndex,
                            frame = child.Frame?.N ?? -1,
                            pic = child.GetRenderPicIndex(),
                            x = child.Runtime.XInt,
                            y = child.Runtime.YInt,
                            z = child.Runtime.ZInt,
                            vx = child.PS.vx,
                            vy = child.PS.vy,
                            vz = child.PS.vz,
                            facingLeft = child.Runtime.IsFacingLeft,
                        });
                    }
                }
            }
            Trace.Add(new TraceRow
            {
                tick = tick,
                frame = frame,
                comboValue = combo,
                comboDdj = character.Runtime.ComboDdj,
                comboDra = character.Runtime.ComboDra,
                comboDla = character.Runtime.ComboDla,
                exactDdj = character.Runtime.NativeInputProxy.ComboState[5],
                exactHorizontal = character.Runtime.NativeInputProxy.ComboState[0],
                lastAction144 = character.Runtime.InputLastAction144,
                pp = character.Health.PP,
                cdDefend = character.Runtime.CdDefend,
                cdDown = character.Runtime.CdDown,
                cdJump = character.Runtime.CdJump,
                cdAttack = character.Runtime.CdAttack,
                cdRight = character.Runtime.CdRight,
                cdLeft = character.Runtime.CdLeft,
                inputPhase = q07DdjStateAudit ? driver.World.InputPhase : -1,
                proxyCurrentBits = q07DdjStateAudit
                    ? RawProxyBitset(character.Runtime.NativeInputProxy.Current) : 0,
                proxyPreviousBits = q07DdjStateAudit
                    ? RawProxyBitset(character.Runtime.NativeInputProxy.Previous) : 0,
                proxyEdgeBits = q07DdjStateAudit
                    ? RawProxyBitset(character.Runtime.NativeInputProxy.EdgeWindow) : 0,
                proxyCurrent = q07DdjStateAudit
                    ? CopyProxyBytes(character.Runtime.NativeInputProxy.Current) : null,
                proxyPrevious = q07DdjStateAudit
                    ? CopyProxyBytes(character.Runtime.NativeInputProxy.Previous) : null,
                proxyEdgeWindow = q07DdjStateAudit
                    ? CopyProxyBytes(character.Runtime.NativeInputProxy.EdgeWindow) : null,
                sharedRngState = q07DdjStateAudit
                    ? driver.World.Rng?.State.ToString() : null,
                nativeRng = q07DdjStateAudit
                    ? CaptureNativeRngState(driver.World) : null,
                objectCount = objectCount,
                oid33Count = oid33Count,
                oid440Count = oid440Count,
                oid518Count = oid518Count,
                sasukeFrame264Seen = sasukeFrame264Seen,
                cloneFrame = cloneFrame,
                clonePic = clonePic,
                cloneSpriteResolved = cloneSpriteResolved,
                cloneVisualDataId = cloneVisualDataId,
                cloneEffectivePic = cloneEffectivePic,
                cloneSourceSheet = cloneSourceSheet,
                clonePixelWidth = clonePixelWidth,
                clonePixelHeight = clonePixelHeight,
                cloneCentralBindingValid = cloneCentralBindingValid,
            });

            if (!step1Seen && combo == 1)
            {
                step1Seen = true;
                step1Tick = tick;
                QueueKeyboardState(secondPhysicalKey);
                step2PressAttempts = 1;
                lastInputPulseTick = tick;
                retryReleaseQueued = false;
            }
            else if (step1Seen && !step2Seen && IsDirectionStep(combo))
            {
                step2Seen = true;
                step2Tick = tick;
                QueueKeyboardState(thirdPhysicalKey);
                step3PressAttempts = 1;
                lastInputPulseTick = tick;
                retryReleaseQueued = false;
            }
            bool formalDdjResolved = q07FormalDdjAudit &&
                character.Runtime.InputLastAction144 == expectedResolvedAction &&
                frame == expectedFullTickFrame;
            if (step2Seen && (formalDdjResolved ||
                (!q07FormalDdjAudit && frame == expectedTargetFrame)))
            {
                targetFrameSeen = true;
                if (step3Tick < 0)
                {
                    step3Tick = tick;
                    releaseTick = tick;
                    observedResolvedAction = character.Runtime.InputLastAction144;
                    resolvedPP = character.Health.PP;
                    QueueKeyboardState();
                }
            }

            if (running && !RetryPendingPhysicalInput(tick, combo))
            {
                Finish(
                    false,
                    $"Physical {comboLabel} input did not reach canonical FrameInputSet " +
                    $"within {MaximumPressAttemptsPerStep} press attempts.");
                return;
            }

            if (targetFrameSeen && tick >= step3Tick +
                (q07DdjStateAudit ? 20 :
                    q07SasukeState15MotionAudit ? ObservationTailTicks + 4 :
                    ObservationTailTicks) &&
                (!q07DdjStateAudit || tick >= startTick + 26))
            {
                bool ddjContinuous = !q07DdjStateAudit ||
                    (missedObservedTicks == 0 && Trace.Count >= 26 &&
                     Trace[0].tick == startTick + 1 &&
                     Trace[Trace.Count - 1].tick >= startTick + 26);
                bool passed = step1Seen && step2Seen &&
                    ddjContinuous &&
                    (!q07FormalDdjAudit ||
                        (observedResolvedAction == expectedResolvedAction &&
                         baselinePP - resolvedPP == 350 && peakOid518Count > 0)) &&
                    (!q07FormalActive || q07SasukeNeedleAudit ||
                        q07DdjStateAudit || peakOid33Count > 0) &&
                    (!q07CloneSpriteAudit ||
                        cloneHiddenPicObserved && cloneVisiblePicObserved) &&
                    (!q07SasukeNeedleAudit ||
                        sasukeFrame264Seen && peakOid440Count >= 4 &&
                        SasukeChildStableIds.Count >= 4 && sasukeChildPic0BindingObserved) &&
                    (!q07SasukeState15MotionAudit ||
                        (SasukeAction12StableIds.Count == 4 && sasukeAction12VxZero));
                Finish(
                    passed,
                    passed
                        ? q07FormalDdjAudit
                            ? "Formal Naruto DDJ requested 271, resolved 272, reached full-tick 495 and spawned OID518."
                            : $"Live NTSD_Battle {comboLabel} reached the authored target frame" +
                          (q07SasukeNeedleAudit
                              ? q07SasukeState15MotionAudit
                                  ? " and observed four OID440 action12 children with Vx0."
                                  : " and observed frame264, four OID440 children and formal chi.png pic0."
                              : q07CloneSpriteAudit
                              ? " and bound OID33 pic999→pic1 to formal ncl.png."
                              : q07FormalActive ? " and spawned OID33." : ".")
                        : q07DdjStateAudit && !ddjContinuous
                            ? "Formal Naruto DDJ did not capture 26 consecutive ticks."
                        : q07FormalDdjAudit
                            ? "Formal Naruto DDJ lacked resolved action, source MP350 or OID518 birth."
                        : q07SasukeNeedleAudit
                            ? q07SasukeState15MotionAudit &&
                              SasukeAction12StableIds.Count != 4
                                ? "Natural Sasuke action12 coverage missed one or more OID440 children."
                                : q07SasukeState15MotionAudit && !sasukeAction12VxZero
                                    ? "Natural Sasuke OID440 action12 Vx was nonzero."
                                    : "Formal Sasuke did not complete frame264/four OID440/chi.png binding."
                        : q07CloneSpriteAudit && peakOid33Count > 0 &&
                          (!cloneHiddenPicObserved || !cloneVisiblePicObserved)
                            ? "Formal Naruto spawned OID33 without the complete hidden-to-visible ncl.png binding."
                        : q07FormalActive && peakOid33Count == 0
                            ? "Formal Naruto reached the authored target but no OID33 spawn was observed."
                            : "The target frame was reached without observing both persisted combo steps.");
                return;
            }

            if (tick > startTick + TimeoutTicks)
            {
                Finish(false, $"Timed out before the authored {comboLabel} target frame was observed.");
            }
        }

        private static byte ResolveComboValue()
        {
            byte[] exact = character.Runtime.NativeInputProxy.ComboState;
            return activeKind == ProbeKind.DownJump ? exact[5] : exact[0];
        }

        private static bool IsDirectionStep(byte combo)
        {
            if (activeKind == ProbeKind.DownJump || comboLabel != "DLA")
                return combo == 2;
            return combo == 3;
        }

        private static bool RetryPendingPhysicalInput(int tick, byte combo)
        {
            if (!step1Seen)
            {
                return PulsePhysicalState(
                    tick,
                    Key.L,
                    ref step1PressAttempts);
            }

            if (!step2Seen)
            {
                return PulsePhysicalState(
                    tick,
                    secondPhysicalKey,
                    ref step2PressAttempts);
            }

            if (!targetFrameSeen && IsDirectionStep(combo))
            {
                return PulsePhysicalState(
                    tick,
                    thirdPhysicalKey,
                    ref step3PressAttempts);
            }

            return true;
        }

        private static bool PulsePhysicalState(
            int tick,
            Key pressedKey,
            ref int pressAttempts)
        {
            // NTSD 2.8 2tu polls human input every other logic tick. Keep both
            // pressed and released device states alive long enough to cross a poll.
            if (tick - lastInputPulseTick < MinimumQueuedStateHoldTicks)
                return true;

            if (!retryReleaseQueued)
            {
                if (pressAttempts >= MaximumPressAttemptsPerStep)
                    return false;
                QueueKeyboardState();
                retryReleaseQueued = true;
            }
            else
            {
                QueueKeyboardState(pressedKey);
                pressAttempts++;
                retryReleaseQueued = false;
            }

            lastInputPulseTick = tick;
            return true;
        }

        private static void QueueKeyboardState(params Key[] pressedKeys)
        {
            if (keyboard != null)
            {
                if (q07DdjStateAudit && running)
                {
                    QueuedInputs.Add(new QueuedInputRow
                    {
                        tickBeforeQueue = driver?.CurrentTickIndex ?? -1,
                        keys = string.Join(",", pressedKeys),
                    });
                }
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressedKeys));
            }
        }

        private static int RawProxyBitset(byte[] keys)
        {
            int mask = 0;
            if (keys == null)
                return mask;
            for (int index = 0; index < Math.Min(7, keys.Length); index++)
            {
                if (keys[index] != 0)
                    mask |= 1 << index;
            }
            return mask;
        }

        private static byte[] CopyProxyBytes(byte[] source)
        {
            if (source == null)
                return null;
            var copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        private static NativeRngState CaptureNativeRngState(SimulationWorld world)
        {
            if (world?.NativeRandom == null)
                return null;
            NTSD28NativeRandomScalarState value = world.NativeRandom.CaptureScalarState();
            return new NativeRngState
            {
                crtState = value.CrtState.ToString(),
                crtCalls = value.CrtCalls.ToString(),
                tableSeed = value.TableSeed.ToString(),
                synchronizedCounter = value.SynchronizedCounter,
                synchronizedIndex = value.SynchronizedIndex,
                synchronizedCalls = value.SynchronizedCalls.ToString(),
                lastSynchronizedCallSite = value.LastSynchronizedCallSite.ToString(),
                synchronizedTableHash = value.SynchronizedTableHash.ToString("X16"),
                synchronizedGeneration = value.SynchronizedGeneration.ToString(),
            };
        }

        private static InitialState CaptureInitialState()
        {
            SimulationWorld world = driver.World;
            ObservedEntities.Clear();
            world.GetAllEntities(ObservedEntities);
            ObservedEntities.Sort((left, right) =>
                (left?.Runtime?.SlotIndex ?? -1).CompareTo(
                    right?.Runtime?.SlotIndex ?? -1));
            var entities = new List<EntityInitialState>(ObservedEntities.Count);
            foreach (LF2Entity entity in ObservedEntities)
            {
                if (entity?.Runtime == null)
                    continue;
                NTSDEntityRuntime runtime = entity.Runtime;
                entities.Add(new EntityInitialState
                {
                    slot = runtime.SlotIndex,
                    stableId = runtime.StableId,
                    oid = entity.ObjectId,
                    team = entity.RelationTeam,
                    action = entity.Frame?.N ?? -1,
                    frameWaitCounter = runtime.FrameWaitCounter,
                    transWaitCounter = entity.Trans?.WaitCounter ?? -1,
                    hp = entity.Health?.HP ?? runtime.HP,
                    pp = entity.Health?.PP ?? runtime.PP,
                    x = runtime.X,
                    y = runtime.Y,
                    z = runtime.Z,
                    vx = runtime.Vx,
                    vy = runtime.Vy,
                    vz = runtime.Vz,
                    facingLeft = runtime.IsFacingLeft,
                    sourceRuleInitialized = runtime.SourceRulePositionInitialized,
                    sourceRuleX = runtime.SourceRuleX,
                    sourceRuleZ = runtime.SourceRuleZ,
                    proxyCurrentBits = RawProxyBitset(runtime.NativeInputProxy.Current),
                    proxyPreviousBits = RawProxyBitset(runtime.NativeInputProxy.Previous),
                    proxyCurrent = CopyProxyBytes(runtime.NativeInputProxy.Current),
                    proxyPrevious = CopyProxyBytes(runtime.NativeInputProxy.Previous),
                });
            }
            return new InitialState
            {
                tick = driver.CurrentTickIndex,
                objectCount = world.ObjectCount,
                battleMode = world.BattleGameModeId,
                backgroundId = world.BackgroundId,
                matchSeed = world.MatchSeed,
                inputPhase = world.InputPhase,
                sharedRngState = world.Rng?.State.ToString(),
                nativeRng = CaptureNativeRngState(world),
                entities = entities.ToArray(),
            };
        }

        private static void Finish(bool passed, string message)
        {
            ProbeResult result = new ProbeResult
            {
                status = passed ? "PASS" : "FAIL",
                message = message,
                probeKind = activeKind.ToString(),
                comboLabel = comboLabel,
                secondPhysicalKey = secondPhysicalKey.ToString(),
                thirdPhysicalKey = thirdPhysicalKey.ToString(),
                expectedTargetFrame = expectedTargetFrame,
                expectedResolvedAction = expectedResolvedAction,
                expectedFullTickFrame = expectedFullTickFrame,
                observedResolvedAction = observedResolvedAction,
                baselinePP = baselinePP,
                resolvedPP = resolvedPP,
                formalDdjAudit = q07FormalDdjAudit,
                peakOid518Count = peakOid518Count,
                firstOid518Tick = firstOid518Tick,
                step1Tick = step1Tick,
                step2Tick = step2Tick,
                step3Tick = step3Tick,
                step1Seen = step1Seen,
                step2Seen = step2Seen,
                targetFrameSeen = targetFrameSeen,
                step1PressAttempts = step1PressAttempts,
                step2PressAttempts = step2PressAttempts,
                step3PressAttempts = step3PressAttempts,
                baselineObjectCount = baselineObjectCount,
                peakObjectCount = peakObjectCount,
                peakOid33Count = peakOid33Count,
                peakOid440Count = peakOid440Count,
                sasukeChildStableIdCount = SasukeChildStableIds.Count,
                sasukeFrame264Seen = sasukeFrame264Seen,
                sasukeChildPic0BindingObserved = sasukeChildPic0BindingObserved,
                sasukeBirthParentX = sasukeBirthParentX,
                sasukeBirthParentY = sasukeBirthParentY,
                sasukeBirthParentZ = sasukeBirthParentZ,
                sasukeChildBirths = SasukeChildBirths.ToArray(),
                sasukeState15MotionAudit = q07SasukeState15MotionAudit,
                sasukeAction12StableIdCount = SasukeAction12StableIds.Count,
                sasukeAction12VxZero = sasukeAction12VxZero,
                sasukeMotionMissedObservedTicks = q07SasukeState15MotionAudit
                    ? missedObservedTicks : -1,
                sasukeMotionRows = q07SasukeState15MotionAudit
                    ? SasukeMotionRows.ToArray() : null,
                cloneHiddenPicObserved = cloneHiddenPicObserved,
                cloneVisiblePicObserved = cloneVisiblePicObserved,
                formalSourceKey = q07FormalActive || q07FormalDdjAudit
                    ? formalSourceKey : string.Empty,
                formalRootVerified = q07FormalActive || q07FormalDdjAudit,
                proxyBitsetSchema = q07DdjStateAudit
                    ? "raw NTSD28InputProxyBlock array index bitset; no formal mask normalization; EdgeWindow has a separate index order and values are countdowns"
                    : null,
                initialState = q07DdjStateAudit ? ddjInitialState : null,
                controlledTickStepping = controlledTickStepping,
                missedObservedTicks = q07DdjStateAudit ? missedObservedTicks : -1,
                continuousTickCapture = q07DdjStateAudit &&
                    missedObservedTicks == 0 && Trace.Count >= 26 &&
                    Trace[0].tick == startTick + 1 &&
                    Trace[Trace.Count - 1].tick >= startTick + 26,
                queuedInputs = q07DdjStateAudit
                    ? QueuedInputs.ToArray() : Array.Empty<QueuedInputRow>(),
                trace = Trace.ToArray(),
            };
            try
            {
                WriteProbeResult(result);
                Debug.Log($"[BattleComboPlayModeProbe] {result.status}: {message}");
            }
            finally
            {
                StopObservation();
                CompleteRequestBatchIfFinished();
            }
        }

        private static void WriteFailure(string message)
        {
            ProbeResult result = new ProbeResult
            {
                status = "FAIL",
                message = message,
                probeKind = activeKind.ToString(),
                comboLabel = comboLabel,
                trace = Array.Empty<TraceRow>(),
                controlledTickStepping = controlledTickStepping,
            };
            try
            {
                WriteProbeResult(result);
                Debug.LogError($"[BattleComboPlayModeProbe] FAIL: {message}");
            }
            finally
            {
                StopObservation();
                CompleteRequestBatchIfFinished();
            }
        }

        private static string ResultPath()
        {
            return ProjectPath(resultRelativePath);
        }

        private static void WriteProbeResult(ProbeResult result)
        {
            string path = ResultPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonUtility.ToJson(result, true);
            if (!q07DdjStateAudit)
            {
                File.WriteAllText(path, json);
                return;
            }

            using (var stream = new FileStream(path, FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(json);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void CompleteRequestBatchIfFinished()
        {
            if (!requestBatchActive || HasPendingRequest())
                return;

            EditorApplication.delayCall += ExitPlayModeAfterRequestBatch;
        }

        private static void ExitPlayModeAfterRequestBatch()
        {
            if (!requestBatchActive || running || HasPendingRequest())
                return;

            requestBatchActive = false;
            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;
        }

        private static bool HasPendingRequest()
        {
            foreach (string relativePath in new[]
            {
                Q07SasukeMotionRequestPath, Q07SasukeRequestPath
            })
            {
                string sasukePath = ProjectPath(relativePath);
                if (!File.Exists(sasukePath))
                    continue;
                try
                {
                    Q07Request request = JsonUtility.FromJson<Q07Request>(File.ReadAllText(sasukePath));
                    if (request != null && request.requested)
                        return true;
                }
                catch (IOException)
                {
                    return true;
                }
            }
            string q07Path = ProjectPath(Q07RequestPath);
            if (File.Exists(q07Path))
            {
                try
                {
                    Q07Request request = JsonUtility.FromJson<Q07Request>(File.ReadAllText(q07Path));
                    if (request != null && request.requested)
                        return true;
                }
                catch (IOException)
                {
                    return true;
                }
            }
            foreach (string relativePath in new[]
            {
                Q07DdjStateRequestPath, Q07DdjContinuousRequestPath
            })
            {
                string ddjStatePath = ProjectPath(relativePath);
                if (!File.Exists(ddjStatePath))
                    continue;
                try
                {
                    Q07Request request = JsonUtility.FromJson<Q07Request>(
                        File.ReadAllText(ddjStatePath));
                    if (request != null && request.requested)
                        return true;
                }
                catch (IOException)
                {
                    return true;
                }
            }
            return File.Exists(ProjectPath(DownJumpRequestPath)) ||
                File.Exists(ProjectPath(ForwardAttackRequestPath));
        }

        private static bool IsLiveBattleReady()
        {
            BattleTestBootstrap bootstrap =
                UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            LF2Character player =
                FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            SimulationTickDriver liveDriver = SimulationTickDriver.Instance;
            return bootstrap != null && player != null &&
                liveDriver?.World != null &&
                player.Controller?.InputBuffer != null &&
                player.Runtime != null && player.Frame?.D != null;
        }

        private static bool CanStartProbe(ProbeKind kind)
        {
            BattleTestBootstrap bootstrap =
                UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            LF2Character player =
                FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            if (player?.Frame?.D == null)
                return false;

            return kind == ProbeKind.DownJump
                ? player.Frame.D.hit_Dj != 0
                : player.Frame.D.hit_Fa != 0;
        }

        private static void DeletePriorResult(string relativePath)
        {
            string path = ProjectPath(relativePath);
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            try
            {
                if (keyboard != null)
                    QueueKeyboardState();
            }
            finally
            {
                keyboard = null;
                running = false;
                if (controlledTickStepping)
                {
                    controlledTickStepping = false;
                    if (driver != null)
                        driver.SetPaused(initialPaused);
                }
            }
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public bool controlledTickStepping;
            public string status;
            public string message;
            public string probeKind;
            public string comboLabel;
            public string secondPhysicalKey;
            public string thirdPhysicalKey;
            public int expectedTargetFrame;
            public int expectedResolvedAction;
            public int expectedFullTickFrame;
            public int observedResolvedAction;
            public int baselinePP;
            public int resolvedPP;
            public bool formalDdjAudit;
            public int peakOid518Count;
            public int firstOid518Tick;
            public int step1Tick;
            public int step2Tick;
            public int step3Tick;
            public bool step1Seen;
            public bool step2Seen;
            public bool targetFrameSeen;
            public int step1PressAttempts;
            public int step2PressAttempts;
            public int step3PressAttempts;
            public int baselineObjectCount;
            public int peakObjectCount;
            public int peakOid33Count;
            public int peakOid440Count;
            public int sasukeChildStableIdCount;
            public bool sasukeFrame264Seen;
            public bool sasukeChildPic0BindingObserved;
            public int sasukeBirthParentX;
            public int sasukeBirthParentY;
            public int sasukeBirthParentZ;
            public SasukeChildBirth[] sasukeChildBirths;
            public bool sasukeState15MotionAudit;
            public int sasukeAction12StableIdCount;
            public bool sasukeAction12VxZero;
            public int sasukeMotionMissedObservedTicks;
            public SasukeChildMotion[] sasukeMotionRows;
            public bool cloneHiddenPicObserved;
            public bool cloneVisiblePicObserved;
            public string formalSourceKey;
            public bool formalRootVerified;
            public string proxyBitsetSchema;
            public InitialState initialState;
            public int missedObservedTicks;
            public bool continuousTickCapture;
            public QueuedInputRow[] queuedInputs;
            public TraceRow[] trace;
        }

        [Serializable]
        private sealed class TraceRow
        {
            public int tick;
            public int frame;
            public int comboValue;
            public int comboDdj;
            public int comboDra;
            public int comboDla;
            public int exactDdj;
            public int exactHorizontal;
            public int lastAction144;
            public int pp;
            public int cdDefend;
            public int cdDown;
            public int cdJump;
            public int cdAttack;
            public int cdRight;
            public int cdLeft;
            public int inputPhase;
            public int proxyCurrentBits;
            public int proxyPreviousBits;
            public int proxyEdgeBits;
            public byte[] proxyCurrent;
            public byte[] proxyPrevious;
            public byte[] proxyEdgeWindow;
            public string sharedRngState;
            public NativeRngState nativeRng;
            public int objectCount;
            public int oid33Count;
            public int oid440Count;
            public int oid518Count;
            public bool sasukeFrame264Seen;
            public int cloneFrame;
            public int clonePic;
            public bool cloneSpriteResolved;
            public int cloneVisualDataId;
            public int cloneEffectivePic;
            public string cloneSourceSheet;
            public float clonePixelWidth;
            public float clonePixelHeight;
            public bool cloneCentralBindingValid;
        }

        [Serializable]
        private sealed class InitialState
        {
            public int tick;
            public int objectCount;
            public int battleMode;
            public int backgroundId;
            public int matchSeed;
            public int inputPhase;
            public string sharedRngState;
            public NativeRngState nativeRng;
            public EntityInitialState[] entities;
        }

        [Serializable]
        private sealed class EntityInitialState
        {
            public int slot;
            public int stableId;
            public int oid;
            public int team;
            public int action;
            public int frameWaitCounter;
            public int transWaitCounter;
            public int hp;
            public int pp;
            public double x;
            public double y;
            public double z;
            public double vx;
            public double vy;
            public double vz;
            public bool facingLeft;
            public bool sourceRuleInitialized;
            public double sourceRuleX;
            public double sourceRuleZ;
            public int proxyCurrentBits;
            public int proxyPreviousBits;
            public byte[] proxyCurrent;
            public byte[] proxyPrevious;
        }

        [Serializable]
        private sealed class NativeRngState
        {
            public string crtState;
            public string crtCalls;
            public string tableSeed;
            public int synchronizedCounter;
            public int synchronizedIndex;
            public string synchronizedCalls;
            public string lastSynchronizedCallSite;
            public string synchronizedTableHash;
            public string synchronizedGeneration;
        }

        [Serializable]
        private sealed class QueuedInputRow
        {
            public int tickBeforeQueue;
            public string keys;
        }

        [Serializable]
        private sealed class SasukeChildBirth
        {
            public int stableId;
            public int slot;
            public int frame;
            public int pic;
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
            public bool facingLeft;
        }

        [Serializable]
        private sealed class SasukeChildMotion
        {
            public int tick;
            public int stableId;
            public int slot;
            public int frame;
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
        }

        private enum ProbeKind
        {
            DownJump,
            ForwardAttack,
        }
    }
}
#endif
