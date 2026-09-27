#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class BattlePresentationDisplayMotionPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q09/Run Display Motion Live Probe";
        private const string CounterMenuPath =
            "NTSD/Battle Diagnostics/Q09/Run Revive Lives Anchor Probe";
        private static readonly int[] RenderFps = { 30, 60, 120 };
        private static readonly string ResultPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", "artifacts", "diagnostics",
            "NTSD28-Q09-P02-VISIBLE-CONSUMERS-001", "live-play-result.txt"));

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Entity actor;
        private static int actorSlot;
        private static FieldInfo renderFpsField;
        private static int phase;
        private static int fpsIndex;
        private static int fixedTick;
        private static int firstGeneration;
        private static double firstPositionX;
        private static double expectedSourceX;
        private static double expectedViewX;
        private static double dueTime;
        private static double deadline;
        private static double lastStatusLogTime;
        private static bool pausedByProbe;
        private static bool switchedBackend;
        private static BattlePresentationBackendMode originalBackend;
        private static LF2ObjectRenderer legacyRenderer;
        private static SpriteRenderer legacyShadow;
        private static double legacyFirstBodyX;
        private static double legacyFirstShadowX;
        private static bool counterOnly;
        private static double originalY;
        private static int originalHp2Orig;
        private static bool counterStateChanged;
        private static BattlePresentationFrame counterPublishedFrame;
        private static bool hasFirstLabel;
        private static float firstCounterY;
        private static float firstLabelY;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            Start(false);
        }

        [MenuItem(CounterMenuPath)]
        public static void RunCounterFromMenu()
        {
            Start(true);
        }

        private static void Start(bool runCounterOnly)
        {
            EditorApplication.update -= Observe;
            File.WriteAllText(ResultPath,
                runCounterOnly ? "RUNNING counter-only\n" : "RUNNING\n");
            if (!EditorApplication.isPlaying)
            {
                Finish("FAIL Play Mode is not active.");
                return;
            }

            driver = null;
            world = null;
            actor = null;
            phase = 0;
            fpsIndex = 0;
            pausedByProbe = false;
            switchedBackend = false;
            counterOnly = runCounterOnly;
            counterStateChanged = false;
            deadline = EditorApplication.timeSinceStartup + 180.0;
            lastStatusLogTime = 0.0;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            try
            {
                if (!EditorApplication.isPlaying)
                    throw new InvalidOperationException("Play Mode ended during probe.");
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("Battle runtime did not reach the next probe phase.");

                if (phase == 0)
                {
                    driver = SimulationTickDriver.Instance;
                    if (EditorApplication.timeSinceStartup - lastStatusLogTime >= 5.0)
                    {
                        lastStatusLogTime = EditorApplication.timeSinceStartup;
                        File.AppendAllText(ResultPath,
                            $"waiting driver={(driver != null)} " +
                            $"state={driver?.LifecycleState} " +
                            $"tick={driver?.CurrentTickIndex} " +
                            $"objects={driver?.World?.ObjectCount} " +
                            $"frame={(driver?.World?.BattlePresentation.PublishedFrame != null)} " +
                            PrewarmStatus() + "\n");
                    }
                    if (driver == null ||
                        driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                        driver.CurrentTickIndex < 2 ||
                        driver.World?.BattlePresentation.PublishedFrame == null)
                        return;

                    world = driver.World;
                    Check(world.BattlePresentation.Mode ==
                        BattlePresentationBackendMode.CentralOnly,
                        "Battle Scene is not CentralOnly.");
                    var entities = new List<LF2Entity>();
                    world.GetAllEntities(entities);
                    foreach (LF2Entity candidate in entities)
                    {
                        File.AppendAllText(ResultPath,
                            $"candidate slot={candidate.Runtime.SlotIndex} " +
                            $"source={candidate.Runtime.SourceRulePositionInitialized}\n");
                        if (actor == null &&
                            candidate.Runtime.SourceRulePositionInitialized)
                            actor = candidate;
                    }
                    Check(actor != null,
                        "Original actor/source coordinates are unavailable.");
                    actorSlot = actor.Runtime.SlotIndex;
                    renderFpsField = typeof(SimulationTickDriver).GetField(
                        "battleRenderFps", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(renderFpsField != null, "Battle render FPS field is unavailable.");
                    driver.SetPaused(true);
                    pausedByProbe = true;
                    fixedTick = driver.CurrentTickIndex;
                    phase = 1;
                    File.AppendAllText(ResultPath, $"ready tick={fixedTick}\n");
                    return;
                }

                if (phase == 1)
                {
                    if (!driver.IsPaused ||
                        driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                        return;
                    fixedTick = driver.CurrentTickIndex;
                    if (counterOnly)
                        BeginCounterCase();
                    else
                        BeginFpsCase();
                    return;
                }

                if (phase == 5)
                {
                    ObserveCounterCase();
                    return;
                }

                if (phase == 3 || phase == 4)
                {
                    ObserveLegacyCase();
                    return;
                }

                if (EditorApplication.timeSinceStartup < dueTime)
                    return;

                int fps = RenderFps[fpsIndex];
                BattlePixelFramePlan later = world.CurrentPixelFramePlan;
                double laterX = ActorPositionX(later.CapturedFrame);
                File.AppendAllText(ResultPath,
                    $"fps={fps} laterGeneration={later.Generation} " +
                    $"firstX={firstPositionX:R} laterX={laterX:R} " +
                    $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
                Check(driver.CurrentTickIndex == fixedTick,
                    "Logic advanced while the display case was paused.");
                Check(Math.Abs(actor.Runtime.SourceRuleX - expectedSourceX) < 1e-6 &&
                      Math.Abs(actor.Runtime.X - expectedViewX) < 1e-6,
                    "Presentation changed source/view logic coordinates.");
                if (fps == 30)
                {
                    Check(later.Generation == firstGeneration &&
                          Math.Abs(laterX - firstPositionX) < 1e-6,
                        "30 FPS should remain discrete on the same tick.");
                }
                else
                {
                    Check(later.Generation != firstGeneration && laterX > firstPositionX,
                        $"{fps} FPS did not rebuild a sampled same-tick body.");
                }

                fpsIndex++;
                if (fpsIndex == RenderFps.Length)
                    BeginLegacyCase();
                else
                    BeginFpsCase();
            }
            catch (Exception exception)
            {
                Finish("FAIL " + exception);
            }
        }

        private static void BeginFpsCase()
        {
            int fps = RenderFps[fpsIndex];
            renderFpsField.SetValue(driver, fps);
            world.ConfigureBattlePresentationDisplayPolicy(
                fps, SimulationConstants.SIM_DT);
            BattlePresentationFrame previous = world.BattlePresentation.PublishedFrame;
            int nextTick = previous.TickIndex + 1;
            double sourceX = actor.Runtime.SourceRuleX;
            double viewX = actor.Runtime.X;
            actor.Runtime.SetSourceRulePosition(sourceX + 20.0,
                actor.Runtime.SourceRuleZ);
            actor.Runtime.X = viewX + 20.0 * world.FixedViewRunDistanceScale;
            actor.Runtime.SyncIntegerPosition();
            actor.RefreshRuntimeSnapshot();
            expectedSourceX = sourceX + 20.0;
            expectedViewX = viewX + 20.0 * world.FixedViewRunDistanceScale;
            world.BattlePresentation.BeginFrame(world, nextTick);
            Check(world.BattlePresentation.PublishedFrame.PreviousMotionTickIndex ==
                previous.TickIndex, "The display motion pair is not adjacent.");

            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePixelFramePlan first = world.CurrentPixelFramePlan;
            Check(first.Owner == BattlePixelFrameOwner.Central,
                $"Central submission unavailable: {first.Reason}");
            firstGeneration = first.Generation;
            firstPositionX = ActorPositionX(first.CapturedFrame);
            File.AppendAllText(ResultPath,
                $"fps={fps} firstGeneration={firstGeneration} " +
                $"firstX={firstPositionX:R} " +
                $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
            dueTime = EditorApplication.timeSinceStartup + 0.05;
            deadline = EditorApplication.timeSinceStartup + 5.0;
            phase = 2;
        }

        private static double ActorPositionX(BattlePresentationFrame frame)
        {
            Check(frame != null, "The captured central frame is missing.");
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type == BattleRenderCommandType.Entity &&
                    command.RuntimeSlot == actorSlot)
                    return command.Position.x;
            }

            throw new InvalidOperationException("The actor has no captured body command.");
        }

        private static void BeginCounterCase()
        {
            renderFpsField.SetValue(driver, 120);
            world.ConfigureBattlePresentationDisplayPolicy(
                120, SimulationConstants.SIM_DT);
            originalY = actor.Runtime.Y;
            originalHp2Orig = actor.HP2Orig;
            BattlePresentationFrame previous = world.BattlePresentation.PublishedFrame;
            actor.HP2Orig = 2;
            actor.Runtime.Y = originalY + 10;
            counterStateChanged = true;
            actor.Runtime.SyncIntegerPosition();
            actor.RefreshRuntimeSnapshot();
            world.BattlePresentation.BeginFrame(world, previous.TickIndex + 1);
            counterPublishedFrame = world.BattlePresentation.PublishedFrame;
            Check(counterPublishedFrame.PreviousMotionTickIndex == previous.TickIndex,
                "Counter motion history is not adjacent.");
            Check(TryPublishedActor(counterPublishedFrame,
                    out BattlePresentationEntitySnapshot publishedActor) &&
                  publishedActor.HP2Orig == 2,
                "The controlled revive-lives value is missing from the logical snapshot.");

            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePixelFramePlan first = world.CurrentPixelFramePlan;
            Check(first.Owner == BattlePixelFrameOwner.Central,
                $"Counter central submission unavailable: {first.Reason}");
            Check(TryOverlayCommand(first.CapturedFrame,
                BattlePresentationMotionAnchor.Body, out BattleRenderCommand firstCounter),
                "The first captured counter is missing.");
            firstCounterY = firstCounter.Position.y;
            hasFirstLabel = TryOverlayCommand(first.CapturedFrame,
                BattlePresentationMotionAnchor.Ground, out BattleRenderCommand firstLabel);
            if (hasFirstLabel)
            {
                firstLabelY = firstLabel.Position.y;
            }

            File.AppendAllText(ResultPath,
                $"counter firstY={firstCounterY:R} " +
                $"label={hasFirstLabel} " +
                $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
            dueTime = EditorApplication.timeSinceStartup + 0.05;
            deadline = EditorApplication.timeSinceStartup + 5.0;
            phase = 5;
        }

        private static void ObserveCounterCase()
        {
            if (EditorApplication.timeSinceStartup < dueTime)
                return;

            BattlePixelFramePlan later = world.CurrentPixelFramePlan;
            Check(TryOverlayCommand(later.CapturedFrame,
                BattlePresentationMotionAnchor.Body, out BattleRenderCommand laterCounter),
                "The later captured counter is missing.");
            Check(Math.Abs(laterCounter.Position.y - firstCounterY) > 1e-6,
                "Counter did not move across display alpha.");
            if (hasFirstLabel)
            {
                Check(TryOverlayCommand(later.CapturedFrame,
                    BattlePresentationMotionAnchor.Ground, out BattleRenderCommand laterLabel),
                    "The later captured ground label is missing.");
                Check(Math.Abs(laterLabel.Position.y - firstLabelY) < 1e-6,
                    "Ground label moved with the actor height.");
            }
            Check(ReferenceEquals(world.BattlePresentation.PublishedFrame,
                    counterPublishedFrame) &&
                  TryPublishedActor(counterPublishedFrame,
                      out BattlePresentationEntitySnapshot unchangedActor) &&
                  unchangedActor.HP2Orig == 2,
                "Display sampling changed the published logical snapshot.");
            Check(driver.CurrentTickIndex == fixedTick &&
                  Math.Abs(actor.Runtime.Y - (originalY + 10)) < 1e-6,
                "Display sampling changed the logic tick or actor height.");
            File.AppendAllText(ResultPath,
                $"counter laterY={laterCounter.Position.y:R} " +
                $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
            Finish("PASS counter Body Y sampled; Label Ground Y unchanged when present; published and logic truth unchanged.");
        }

        private static bool TryOverlayCommand(
            BattlePresentationFrame frame,
            BattlePresentationMotionAnchor anchor,
            out BattleRenderCommand result)
        {
            if (frame != null)
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type == BattleRenderCommandType.OverlayGlyph &&
                        command.RuntimeSlot == actorSlot &&
                        command.MotionAnchor == anchor)
                    {
                        result = command;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private static bool TryPublishedActor(
            BattlePresentationFrame frame,
            out BattlePresentationEntitySnapshot result)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(index);
                if (entity.RuntimeSlot == actorSlot)
                {
                    result = entity;
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static void BeginLegacyCase()
        {
            originalBackend = world.BattlePresentation.Mode;
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.LegacyOnly);
            switchedBackend = true;
            renderFpsField.SetValue(driver, 120);
            world.ConfigureBattlePresentationDisplayPolicy(120,
                SimulationConstants.SIM_DT);
            legacyRenderer = actor.Renderer;
            Check(legacyRenderer != null, "The actor has no Legacy renderer.");
            legacyShadow = (SpriteRenderer)typeof(LF2ObjectRenderer).GetField(
                "_shadowRenderer", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(legacyRenderer);
            File.AppendAllText(ResultPath,
                $"legacy bornInCentralOnly shadowRenderer={(legacyShadow != null)}\n");

            BattlePresentationFrame previous = world.BattlePresentation.PublishedFrame;
            double sourceX = actor.Runtime.SourceRuleX;
            double viewX = actor.Runtime.X;
            actor.Runtime.SetSourceRulePosition(sourceX + 20.0,
                actor.Runtime.SourceRuleZ);
            actor.Runtime.X = viewX + 20.0 * world.FixedViewRunDistanceScale;
            actor.Runtime.SyncIntegerPosition();
            actor.RefreshRuntimeSnapshot();
            expectedSourceX = sourceX + 20.0;
            expectedViewX = viewX + 20.0 * world.FixedViewRunDistanceScale;
            world.BattlePresentation.BeginFrame(world, previous.TickIndex + 1);
            Check(world.BattlePresentation.PublishedFrame.PreviousMotionTickIndex ==
                previous.TickIndex, "Legacy motion history is not adjacent.");
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            dueTime = EditorApplication.timeSinceStartup + 0.01;
            deadline = EditorApplication.timeSinceStartup + 5.0;
            phase = 3;
        }

        private static void ObserveLegacyCase()
        {
            if (EditorApplication.timeSinceStartup < dueTime)
                return;

            Transform root = legacyRenderer.transform.parent != null
                ? legacyRenderer.transform.parent
                : legacyRenderer.transform;
            if (phase == 3)
            {
                legacyFirstBodyX = root.position.x;
                legacyFirstShadowX = legacyShadow != null
                    ? legacyShadow.transform.position.x
                    : double.NaN;
                File.AppendAllText(ResultPath,
                    $"legacy firstBodyX={legacyFirstBodyX:R} " +
                    $"firstShadowX={legacyFirstShadowX:R} " +
                    $"shadowEnabled={legacyShadow?.enabled} " +
                    $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
                dueTime = EditorApplication.timeSinceStartup + 0.05;
                phase = 4;
                return;
            }

            double laterBodyX = root.position.x;
            double laterShadowX = legacyShadow != null
                ? legacyShadow.transform.position.x
                : double.NaN;
            File.AppendAllText(ResultPath,
                $"legacy laterBodyX={laterBodyX:R} " +
                $"laterShadowX={laterShadowX:R} " +
                $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world):R}\n");
            Check(driver.CurrentTickIndex == fixedTick,
                "Logic advanced during Legacy presentation sampling.");
            Check(Math.Abs(actor.Runtime.SourceRuleX - expectedSourceX) < 1e-6 &&
                  Math.Abs(actor.Runtime.X - expectedViewX) < 1e-6,
                "Legacy presentation changed logic coordinates.");
            Check(laterBodyX > legacyFirstBodyX,
                "Legacy body did not move across display alpha.");
            if (legacyShadow != null && legacyShadow.enabled)
                Check(laterShadowX > legacyFirstShadowX,
                    "Legacy shadow did not move across display alpha.");
            Finish("PASS central 30 discrete; 60/120 sampled; Legacy 120 body sampled; logic coordinates unchanged. Shadow requires a Legacy-born actor.");
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string PrewarmStatus()
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
            if (manager == null)
                return "manager=null";
            Type type = typeof(CharacterAnimtorManager);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            int generation = (int)type.GetField("configuredContentGeneration", flags)
                .GetValue(manager);
            bool disposed = (bool)type.GetField("spritePrewarmDisposed", flags)
                .GetValue(manager);
            bool running = (bool)type.GetField("configuredPrewarmRunning", flags)
                .GetValue(manager);
            object selectedConfig = type.GetField("configuredPrewarmConfig", flags)
                .GetValue(manager);
            string selectedRoot = (string)type.GetField("configuredPrewarmRoot", flags)
                .GetValue(manager);
            bool boundaryOpen = (bool)type.GetMethod("NativeContentBoundaryIsOpen",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            string currentRoot = GameConfig.Instance?.BattleContentRuntimeRoot?.Trim();
            return $"prewarmGen={generation} running={running} disposed={disposed} " +
                $"configSame={ReferenceEquals(selectedConfig, GameConfig.Instance)} " +
                $"rootSame={string.Equals(selectedRoot, currentRoot, StringComparison.Ordinal)} " +
                $"boundaryOpen={boundaryOpen} app={AppManager.Instance?.State}";
        }

        private static void Finish(string result)
        {
            EditorApplication.update -= Observe;
            File.AppendAllText(ResultPath, result + "\n");
            if (counterStateChanged && actor != null && EditorApplication.isPlaying)
            {
                actor.HP2Orig = originalHp2Orig;
                actor.Runtime.Y = originalY;
                actor.Runtime.SyncIntegerPosition();
                actor.RefreshRuntimeSnapshot();
            }
            counterStateChanged = false;
            if (switchedBackend && world != null && EditorApplication.isPlaying)
                world.SetBattlePresentationBackend(originalBackend);
            switchedBackend = false;
            if (pausedByProbe && driver != null &&
                EditorApplication.isPlaying &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Running)
                driver.SetPaused(false);
            pausedByProbe = false;
            driver = null;
            world = null;
            actor = null;
        }
    }
}
#endif
