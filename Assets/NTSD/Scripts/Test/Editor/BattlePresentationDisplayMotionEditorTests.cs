using System.Collections;
using System.IO;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class BattlePresentationDisplayMotionEditorTests
    {
        [Test]
        public void CapturedCommands_UseCategoryOffsets_WithoutChangingPublishedFrame()
        {
            BattlePresentationFrame published = FrameWithMotion(10, 11, 100, 120);
            published.AddCommand(Command(BattleRenderCommandType.Shadow));
            published.AddCommand(Command(BattleRenderCommandType.Entity, true));
            published.AddCommand(Command(BattleRenderCommandType.OverlayGlyph));
            published.AddCommand(Command(BattleRenderCommandType.BleedMark));
            published.AddCommand(Command(BattleRenderCommandType.HitRecord));
            var captured = new BattlePresentationFrame();
            captured.CopyFrom(published);
            var display = new BattlePresentationDisplayMotion();

            display.Prepare(captured, 0.5, 1.5, 2.0);
            Assert.That(display.SampledCount, Is.EqualTo(1));
            display.ApplyToCapturedCommands(captured);

            float unitsX = NTSD.Animation.NTSDRenderSpace.UnitsPerPixelX;
            float unitsY = NTSD.Animation.NTSDRenderSpace.UnitsPerPixelY;
            Assert.That(captured.GetCommand(0).Position.x,
                Is.EqualTo(-15f * unitsX).Within(1e-6));
            Assert.That(captured.GetCommand(0).Position.y,
                Is.EqualTo(10f * unitsY).Within(1e-6));
            Assert.That(captured.GetCommand(1).Position.y,
                Is.EqualTo(15f * unitsY).Within(1e-6));
            Assert.That(captured.GetCommand(2).Position.y,
                Is.EqualTo(10f * unitsY).Within(1e-6));
            Assert.That(captured.GetCommand(3).Position.y,
                Is.EqualTo(15f * unitsY).Within(1e-6));
            Assert.That(captured.GetCommand(4).Position, Is.EqualTo(Vector3.zero));
            Assert.That(captured.GetCommand(1).StableHealthAnchorWorld.y,
                Is.EqualTo(15f * unitsY).Within(1e-6));
            Assert.That(captured.GetCommand(1).StableFootAnchorWorld.y,
                Is.EqualTo(10f * unitsY).Within(1e-6));
            Assert.That(published.GetCommand(1).Position, Is.EqualTo(Vector3.zero));
            Assert.That(published.GetMotionState(0).PreciseX, Is.EqualTo(120));

            display.Prepare(captured, 1.0, 1.5, 2.0);
            Assert.That(display.SampledCount, Is.Zero);
            Assert.That(display.TryGet(new RuntimeEntityHandle(3, 1), out _), Is.False);
        }

        [Test]
        public void Lookup_RejectsGenerationRelationAndSkippedTick()
        {
            var display = new BattlePresentationDisplayMotion();
            BattlePresentationFrame adjacent = FrameWithMotion(10, 11, 100, 120);
            display.Prepare(adjacent, 0.5, 1.5, 2.0);
            Assert.That(display.TryGet(new RuntimeEntityHandle(3, 2), out _), Is.False);

            BattlePresentationFrame skipped = FrameWithMotion(10, 12, 100, 120);
            display.Prepare(skipped, 0.5, 1.5, 2.0);
            Assert.That(display.SampledCount, Is.Zero);
            Assert.That(display.TryGet(new RuntimeEntityHandle(3, 1), out _), Is.False);

            BattlePresentationFrame changedRelation = FrameWithMotion(
                10, 11, 100, 120, changeRelation: true);
            display.Prepare(changedRelation, 0.5, 1.5, 2.0);
            Assert.That(display.SampledCount, Is.Zero);
        }

        [Test]
        public void ReviveLivesCounterUsesHeightWhileNameplateUsesGroundMotion()
        {
            BattlePresentationFrame published = FrameWithMotion(10, 11, 100, 120);
            published.AddCommand(Command(BattleRenderCommandType.OverlayGlyph,
                motionAnchor: BattlePresentationMotionAnchor.Body));
            published.AddCommand(Command(BattleRenderCommandType.OverlayGlyph,
                motionAnchor: BattlePresentationMotionAnchor.Ground));
            var frame = new BattlePresentationFrame();
            frame.CopyFrom(published);
            var display = new BattlePresentationDisplayMotion();

            display.Prepare(frame, 0.5, 1.5, 2.0);
            display.ApplyToCapturedCommands(frame);

            float unitsY = NTSD.Animation.NTSDRenderSpace.UnitsPerPixelY;
            Assert.That(frame.GetCommand(0).MotionAnchor,
                Is.EqualTo(BattlePresentationMotionAnchor.Body));
            Assert.That(frame.GetCommand(0).Position.y,
                Is.EqualTo(15f * unitsY).Within(1e-6));
            Assert.That(frame.GetCommand(1).MotionAnchor,
                Is.EqualTo(BattlePresentationMotionAnchor.Ground));
            Assert.That(frame.GetCommand(1).Position.y,
                Is.EqualTo(10f * unitsY).Within(1e-6));
            Assert.That(published.GetCommand(0).Position, Is.EqualTo(Vector3.zero));
            Assert.That(published.GetCommand(1).Position, Is.EqualTo(Vector3.zero));
        }

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator OriginalBattleScene_RebuildsSameTickOnlyAboveThirtyFps()
        {
            string tracePath = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "artifacts", "diagnostics",
                "NTSD28-Q09-P02-VISIBLE-CONSUMERS-001", "play-trace.txt"));
            File.WriteAllText(tracePath, "enter-requested\n");
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();
            File.AppendAllText(tracePath, "entered-play\n");

            SimulationTickDriver driver = null;
            for (int second = 0; second < 120; second++)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World?.BattlePresentation.PublishedFrame != null &&
                    driver.CurrentTickIndex >= 2)
                    break;
                if (second % 10 == 0)
                    File.AppendAllText(tracePath,
                        $"wait={second} driver={(driver != null)} " +
                        $"state={driver?.LifecycleState} " +
                        $"tick={driver?.CurrentTickIndex} " +
                        $"frame={(driver?.World?.BattlePresentation.PublishedFrame != null)}\n");
                yield return new WaitForSecondsRealtime(1f);
            }

            File.AppendAllText(tracePath,
                $"ready driver={(driver != null)} tick={driver?.CurrentTickIndex}\n");
            Assert.That(driver?.World, Is.Not.Null);
            driver.SetPaused(true);
            yield return null;
            SimulationWorld world = driver.World;
            Assert.That(world.BattlePresentation.Mode,
                Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
            var actor = world.FindEntityByRuntimeSlotForQuery(0);
            Assert.That(actor, Is.Not.Null);
            Assert.That(actor.Runtime.SourceRulePositionInitialized, Is.True);
            int logicTick = driver.CurrentTickIndex;
            FieldInfo renderFpsField = typeof(SimulationTickDriver).GetField(
                "battleRenderFps", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(renderFpsField, Is.Not.Null);

            foreach (int fps in new[] { 30, 60, 120 })
            {
                renderFpsField.SetValue(driver, fps);
                world.ConfigureBattlePresentationDisplayPolicy(
                    fps, SimulationConstants.SIM_DT);
                BattlePresentationFrame prior = world.BattlePresentation.PublishedFrame;
                int nextTick = prior.TickIndex + 1;
                double sourceX = actor.Runtime.SourceRuleX;
                double viewX = actor.Runtime.X;
                actor.Runtime.SetSourceRulePosition(sourceX + 20.0,
                    actor.Runtime.SourceRuleZ);
                actor.Runtime.X = viewX +
                    20.0 * world.FixedViewRunDistanceScale;
                actor.Runtime.SyncIntegerPosition();
                actor.RefreshRuntimeSnapshot();
                world.BattlePresentation.BeginFrame(world, nextTick);
                Assert.That(world.BattlePresentation.PublishedFrame.PreviousMotionTickIndex,
                    Is.EqualTo(prior.TickIndex));

                BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
                BattlePixelFramePlan firstPlan = world.CurrentPixelFramePlan;
                File.AppendAllText(tracePath,
                    $"fps={fps} first owner={firstPlan.Owner} " +
                    $"reason={firstPlan.Reason} generation={firstPlan.Generation} " +
                    $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world)}\n");
                Assert.That(firstPlan.Owner, Is.EqualTo(BattlePixelFrameOwner.Central),
                    $"fps={fps}: central submission unavailable: {firstPlan.Reason}");
                Vector3 firstPosition = ActorPosition(firstPlan.CapturedFrame);
                int firstGeneration = firstPlan.Generation;

                yield return new WaitForSecondsRealtime(0.05f);
                BattlePixelFramePlan laterPlan = world.CurrentPixelFramePlan;
                Vector3 laterPosition = ActorPosition(laterPlan.CapturedFrame);
                File.AppendAllText(tracePath,
                    $"fps={fps} later generation={laterPlan.Generation} " +
                    $"x0={firstPosition.x:R} x1={laterPosition.x:R} " +
                    $"alpha={BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world)}\n");
                Assert.That(driver.CurrentTickIndex, Is.EqualTo(logicTick));
                Assert.That(actor.Runtime.SourceRuleX, Is.EqualTo(sourceX + 20.0));
                Assert.That(actor.Runtime.X,
                    Is.EqualTo(viewX + 20.0 * world.FixedViewRunDistanceScale));
                if (fps == 30)
                {
                    Assert.That(laterPlan.Generation, Is.EqualTo(firstGeneration));
                    Assert.That(laterPosition.x,
                        Is.EqualTo(firstPosition.x).Within(1e-6));
                }
                else
                {
                    Assert.That(laterPlan.Generation, Is.Not.EqualTo(firstGeneration),
                        $"fps={fps}: same-tick geometry was not rebuilt");
                    Assert.That(laterPosition.x, Is.GreaterThan(firstPosition.x));
                }
            }

            renderFpsField.SetValue(driver, 120);
            world.ConfigureBattlePresentationDisplayPolicy(
                120, SimulationConstants.SIM_DT);
            actor.HP2Orig = 2;
            BattlePresentationFrame beforeHeight = world.BattlePresentation.PublishedFrame;
            double sourceY = actor.Runtime.Y;
            actor.Runtime.Y = sourceY + 10;
            actor.Runtime.SyncIntegerPosition();
            actor.RefreshRuntimeSnapshot();
            world.BattlePresentation.BeginFrame(world, beforeHeight.TickIndex + 1);
            BattlePresentationFrame publishedHeight =
                world.BattlePresentation.PublishedFrame;
            Assert.That(publishedHeight.PreviousMotionTickIndex,
                Is.EqualTo(beforeHeight.TickIndex));
            Assert.That(PublishedActor(publishedHeight).HP2Orig,
                Is.EqualTo(2));

            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePixelFramePlan firstHeightPlan = world.CurrentPixelFramePlan;
            Assert.That(firstHeightPlan.Owner, Is.EqualTo(BattlePixelFrameOwner.Central));
            Assert.That(TryOverlayCommand(firstHeightPlan.CapturedFrame,
                BattlePresentationMotionAnchor.Body, out BattleRenderCommand firstCounter),
                Is.True);
            bool hasFirstLabel = TryOverlayCommand(firstHeightPlan.CapturedFrame,
                BattlePresentationMotionAnchor.Ground, out BattleRenderCommand firstLabel);

            yield return new WaitForSecondsRealtime(0.05f);
            BattlePixelFramePlan laterHeightPlan = world.CurrentPixelFramePlan;
            Assert.That(TryOverlayCommand(laterHeightPlan.CapturedFrame,
                BattlePresentationMotionAnchor.Body, out BattleRenderCommand laterCounter),
                Is.True);
            Assert.That(Mathf.Abs(laterCounter.Position.y - firstCounter.Position.y),
                Is.GreaterThan(1e-6f));
            if (hasFirstLabel)
            {
                Assert.That(TryOverlayCommand(laterHeightPlan.CapturedFrame,
                    BattlePresentationMotionAnchor.Ground, out BattleRenderCommand laterLabel),
                    Is.True);
                Assert.That(firstLabel.Position.y,
                    Is.EqualTo(laterLabel.Position.y).Within(1e-6));
            }

            File.AppendAllText(tracePath,
                $"counter firstY={firstCounter.Position.y:R} " +
                $"laterY={laterCounter.Position.y:R} " +
                $"label={hasFirstLabel}\n");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(logicTick));
            Assert.That(actor.Runtime.Y, Is.EqualTo(sourceY + 10));
            Assert.That(world.BattlePresentation.PublishedFrame,
                Is.SameAs(publishedHeight));
            Assert.That(PublishedActor(publishedHeight).HP2Orig,
                Is.EqualTo(2));
            File.AppendAllText(tracePath, "assertions-passed\n");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }

        private static Vector3 ActorPosition(BattlePresentationFrame frame)
        {
            Assert.That(frame, Is.Not.Null);
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type == BattleRenderCommandType.Entity &&
                    command.RuntimeSlot == 0)
                    return command.Position;
            }

            Assert.Fail("The original battle actor has no central body command.");
            return default;
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
                        command.RuntimeSlot == 0 && command.MotionAnchor == anchor)
                    {
                        result = command;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }

        private static BattlePresentationEntitySnapshot PublishedActor(
            BattlePresentationFrame frame)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(index);
                if (entity.RuntimeSlot == 0)
                    return entity;
            }

            Assert.Fail("The controlled actor is missing from the published logical frame.");
            return default;
        }

        private static BattlePresentationFrame FrameWithMotion(
            int previousTick,
            int currentTick,
            double previousX,
            double currentX,
            bool changeRelation = false)
        {
            var previous = new BattlePresentationFrame { TickIndex = previousTick };
            previous.AddMotionState(State(previousX, 0, 100, 0));
            var current = new BattlePresentationFrame { TickIndex = currentTick };
            current.AddMotionState(State(currentX, 10, 110,
                changeRelation ? 1 : 0));
            current.CopyPreviousMotionStatesFrom(previous);
            return current;
        }

        private static BattlePresentationMotionState State(
            double x,
            double y,
            double z,
            int ownerSlot)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(x, z);
            runtime.X = x * 1.5;
            runtime.Y = y;
            runtime.Z = z * 2.0;
            runtime.OwnerSlotIndex = ownerSlot;
            return new BattlePresentationMotionState(
                new RuntimeEntityHandle(3, 1), 2, runtime);
        }

        private static BattleRenderCommand Command(
            BattleRenderCommandType type,
            bool anchors = false,
            BattlePresentationMotionAnchor motionAnchor = BattlePresentationMotionAnchor.Ground)
        {
            return new BattleRenderCommand(
                type, new RuntimeEntityHandle(3, 1), 5, 2, 0,
                0, 3, 1, 0, 0, Vector3.zero,
                Vector2.one, Vector2.zero, Rect.zero,
                BattleSpriteRenderState.Default(false),
                default,
                showOverheadHealthBar: anchors,
                stableHealthAnchorWorld: Vector2.zero,
                hasStableHealthAnchor: anchors,
                stableFootAnchorWorld: Vector2.zero,
                hasStableFootAnchor: anchors,
                showSelfFootMarker: anchors,
                motionAnchor: motionAnchor);
        }
    }
}
