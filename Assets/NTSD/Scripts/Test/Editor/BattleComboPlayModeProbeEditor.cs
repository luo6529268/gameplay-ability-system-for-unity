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
        private const string Q07ResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-FORWARD-ATTACK-PHYSICAL-001";
        private const string Q07CloneSpriteResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-NARUTO-CLONE-SPRITE-BINDING-001";
        private const string Q07SasukeResultRoot =
            "artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001";
        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147";
        private const int ObservationTailTicks = 18;
        private const int TimeoutTicks = 90;
        private const int MaximumPressAttemptsPerStep = 8;
        private const int MinimumQueuedStateHoldTicks = 2;

        private static readonly List<TraceRow> Trace = new List<TraceRow>(128);
        private static readonly List<LF2Entity> ObservedEntities = new List<LF2Entity>(32);
        private static readonly HashSet<int> SasukeChildStableIds = new HashSet<int>();
        private static readonly List<LF2Entity> SasukeChildrenThisTick = new List<LF2Entity>(8);
        private static readonly List<SasukeChildBirth> SasukeChildBirths = new List<SasukeChildBirth>(4);
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
        private static int baselineObjectCount;
        private static int peakObjectCount;
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
        private static bool q07CloneSpriteAudit;
        private static bool q07SasukeNeedleAudit;
        private static bool cloneHiddenPicObserved;
        private static bool cloneVisiblePicObserved;
        private static bool sasukeFrame264Seen;
        private static bool sasukeChildPic0BindingObserved;
        private static int peakOid440Count;
        private static int sasukeBirthParentX;
        private static int sasukeBirthParentY;
        private static int sasukeBirthParentZ;
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
            string path = ProjectPath(Q07SasukeRequestPath);
            if (!File.Exists(path))
                return;
            Q07Request request;
            try
            {
                request = JsonUtility.FromJson<Q07Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || !request.requested)
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

            string sasukePath = ProjectPath(Q07SasukeRequestPath);
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
                    if (!IsValidQ07RunId(sasukeRequest.runId) ||
                        File.Exists(ProjectPath(Path.Combine(
                            Q07SasukeResultRoot, sasukeRequest.runId + ".json"))))
                    {
                        sasukeRequest.requested = false;
                        File.WriteAllText(sasukePath, JsonUtility.ToJson(sasukeRequest));
                        Debug.LogError("[BattleComboPlayModeProbe] Invalid or existing Sasuke Q07 result.");
                        return;
                    }
                    sasukeRequest.requested = false;
                    File.WriteAllText(sasukePath, JsonUtility.ToJson(sasukeRequest));
                    requestBatchActive = true;
                    RunProbe(ProbeKind.ForwardAttack, sasukeRequest.runId, false, true);
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

            if (!EditorApplication.isPlaying)
                return;

            if (!HasPendingRequest() || !IsLiveBattleReady())
                return;

            string downJump = ProjectPath(DownJumpRequestPath);
            if (File.Exists(downJump))
            {
                if (!CanStartProbe(ProbeKind.DownJump))
                    return;
                requestBatchActive = true;
                File.Delete(downJump);
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
            bool sasukeNeedleAudit = false)
        {
            activeKind = kind;
            q07FormalActive = q07RunId != null;
            q07CloneSpriteAudit = q07FormalActive && cloneSpriteAudit;
            q07SasukeNeedleAudit = q07FormalActive && sasukeNeedleAudit;
            resultRelativePath = q07FormalActive
                ? Path.Combine(
                    q07SasukeNeedleAudit ? Q07SasukeResultRoot :
                        q07CloneSpriteAudit ? Q07CloneSpriteResultRoot : Q07ResultRoot,
                    q07RunId + ".json")
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
                if (GameConfig.Instance?.BattleContentRuntimeRoot != FormalRoot ||
                    manager?.PublishedLoganContentIdentity?.SemanticFingerprint != FormalFingerprint ||
                    character.ObjectId != (q07SasukeNeedleAudit ? 11 : 2) ||
                    character.Frame.D.hit_Fa != (q07SasukeNeedleAudit ? 261 : 285) ||
                    string.IsNullOrEmpty(formalSourceKey))
                {
                    WriteFailure(q07SasukeNeedleAudit
                        ? "Q07 Sasuke preflight did not find formal root, fingerprint, OID11 and hit_Fa 261."
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

            int currentTick = driver.CurrentTickIndex;
            startTick = currentTick;
            step1Tick = -1;
            step2Tick = -1;
            step3Tick = -1;
            releaseTick = -1;
            baselineObjectCount = driver.World.ObjectCount;
            peakObjectCount = baselineObjectCount;
            peakOid33Count = 0;
            peakOid440Count = 0;
            sasukeFrame264Seen = false;
            sasukeChildPic0BindingObserved = false;
            SasukeChildStableIds.Clear();
            SasukeChildBirths.Clear();
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
            if (tick <= lastObservedTick)
                return;

            lastObservedTick = tick;
            int frame = character.Frame?.N ?? -1;
            if (q07SasukeNeedleAudit && frame == 264)
                sasukeFrame264Seen = true;
            byte combo = ResolveComboValue();
            int objectCount = driver.World.ObjectCount;
            peakObjectCount = Math.Max(peakObjectCount, objectCount);
            int oid33Count = 0;
            int oid440Count = 0;
            int cloneFrame = -1;
            int clonePic = -1;
            int cloneVisualDataId = -1;
            int cloneEffectivePic = -1;
            string cloneSourceSheet = string.Empty;
            float clonePixelWidth = 0f;
            float clonePixelHeight = 0f;
            bool cloneSpriteResolved = false;
            bool cloneCentralBindingValid = false;
            if (q07FormalActive)
            {
                ObservedEntities.Clear();
                SasukeChildrenThisTick.Clear();
                driver.World.GetAllEntities(ObservedEntities);
                foreach (LF2Entity entity in ObservedEntities)
                {
                    if (q07SasukeNeedleAudit && entity != null && entity.ObjectId == 440)
                    {
                        oid440Count++;
                        SasukeChildStableIds.Add(entity.Runtime.StableId);
                        SasukeChildrenThisTick.Add(entity);
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
                cdDefend = character.Runtime.CdDefend,
                cdDown = character.Runtime.CdDown,
                cdJump = character.Runtime.CdJump,
                cdAttack = character.Runtime.CdAttack,
                cdRight = character.Runtime.CdRight,
                cdLeft = character.Runtime.CdLeft,
                objectCount = objectCount,
                oid33Count = oid33Count,
                oid440Count = oid440Count,
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
            if (step2Seen && frame == expectedTargetFrame)
            {
                targetFrameSeen = true;
                if (step3Tick < 0)
                {
                    step3Tick = tick;
                    releaseTick = tick;
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

            if (targetFrameSeen && tick >= step3Tick + ObservationTailTicks)
            {
                bool passed = step1Seen && step2Seen &&
                    (!q07FormalActive || q07SasukeNeedleAudit || peakOid33Count > 0) &&
                    (!q07CloneSpriteAudit ||
                        cloneHiddenPicObserved && cloneVisiblePicObserved) &&
                    (!q07SasukeNeedleAudit ||
                        sasukeFrame264Seen && peakOid440Count >= 4 &&
                        SasukeChildStableIds.Count >= 4 && sasukeChildPic0BindingObserved);
                Finish(
                    passed,
                    passed
                        ? $"Live NTSD_Battle {comboLabel} reached the authored target frame" +
                          (q07SasukeNeedleAudit
                              ? " and observed frame264, four OID440 children and formal chi.png pic0."
                              : q07CloneSpriteAudit
                              ? " and bound OID33 pic999→pic1 to formal ncl.png."
                              : q07FormalActive ? " and spawned OID33." : ".")
                        : q07SasukeNeedleAudit
                            ? "Formal Sasuke did not complete frame264/four OID440/chi.png binding."
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
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressedKeys));
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
                cloneHiddenPicObserved = cloneHiddenPicObserved,
                cloneVisiblePicObserved = cloneVisiblePicObserved,
                formalSourceKey = q07FormalActive ? formalSourceKey : string.Empty,
                formalRootVerified = q07FormalActive,
                trace = Trace.ToArray(),
            };
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ResultPath()));
                File.WriteAllText(ResultPath(), JsonUtility.ToJson(result, true));
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
            };
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ResultPath()));
                File.WriteAllText(ResultPath(), JsonUtility.ToJson(result, true));
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
            string sasukePath = ProjectPath(Q07SasukeRequestPath);
            if (File.Exists(sasukePath))
            {
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
            if (keyboard != null)
                QueueKeyboardState();
            keyboard = null;
            running = false;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string message;
            public string probeKind;
            public string comboLabel;
            public string secondPhysicalKey;
            public string thirdPhysicalKey;
            public int expectedTargetFrame;
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
            public bool cloneHiddenPicObserved;
            public bool cloneVisiblePicObserved;
            public string formalSourceKey;
            public bool formalRootVerified;
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
            public int cdDefend;
            public int cdDown;
            public int cdJump;
            public int cdAttack;
            public int cdRight;
            public int cdLeft;
            public int objectCount;
            public int oid33Count;
            public int oid440Count;
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

        private enum ProbeKind
        {
            DownJump,
            ForwardAttack,
        }
    }
}
#endif
