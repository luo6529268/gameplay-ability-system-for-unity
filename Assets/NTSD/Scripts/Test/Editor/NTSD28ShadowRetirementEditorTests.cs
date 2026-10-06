#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28ShadowRetirementEditorTests
    {
        private const string OutputRoot = "artifacts/diagnostics/NTSD28-BATTLE-SHADOW-RETIREMENT-20261006/";

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        [TestCase(5, false)]
        [TestCase(6, false)]
        public void ViewportCleanupCoversAllNonCharacterTypesAndPreservesCharacters(int type, bool expected)
        {
            var cameraObject = new GameObject("shared shadow viewport camera");
            var previousCamera = NTSDRenderSpace.BoundWorldCameraForSelfCheck;
            try
            {
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                NTSDRenderSpace.BindWorldCamera(camera);
                Assert.That(LF2ObjectRenderer.ShouldDrawShadowForBodyViewport(type, 2000, 300, 28, 0, 1.0),
                    Is.EqualTo(expected));
                Assert.That(LF2ObjectRenderer.ShouldDrawShadowForBodyViewport(type, 0, 300, 28, 0, 1.0), Is.True);
                Assert.That(LF2ObjectRenderer.ShouldDrawShadowForBodyViewport(type, -2000, 300, 28, 0, 1.0), Is.True);
            }
            finally
            {
                NTSDRenderSpace.BindWorldCamera(previousCamera);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void LegacyShadowDisappearsWithBodyBelowViewportAndReturnsAboveIt()
        {
            var cameraObject = new GameObject("shadow viewport test camera");
            var shadowObject = new GameObject("shadow viewport test renderer");
            var previousCamera = NTSDRenderSpace.BoundWorldCameraForSelfCheck;
            var entity = new LF2OtherObject { ObjectId = 31981 };
            var frame = new LF2FrameData { frameId = 0, pic = 0, state = 9999, centery = 28 };
            var data = new LF2CharacterData { type_sub = 5 };
            data.frames.Add(frame);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(31981, data));
            entity.Frame.D = frame;
            entity.Runtime.HitStop = 0;
            entity.Runtime.LinkState = 0;
            entity.Runtime.ZInt = 300;
            try
            {
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                NTSDRenderSpace.BindWorldCamera(camera);
                var renderer = shadowObject.AddComponent<SpriteRenderer>();
                entity.SetShadowRenderer(renderer);
                entity.UpdateShadow();
                Assert.That(renderer.enabled, Is.True, "Grounded object keeps its shadow.");
                entity.Runtime.YInt = 2000;
                entity.UpdateShadow();
                Assert.That(renderer.enabled, Is.False, "Entire body below viewport must not leave ground shadow.");
                entity.Runtime.YInt = -2000;
                entity.UpdateShadow();
                Assert.That(renderer.enabled, Is.True, "Above-screen airborne shadow remains intentional.");
                entity.Runtime.YInt = 0;
                entity.UpdateShadow();
                Assert.That(renderer.enabled, Is.True, "Visibility must not be permanently latched off.");
            }
            finally
            {
                entity.SetShadowRenderer(null);
                NTSDRenderSpace.BindWorldCamera(previousCamera);
                UnityEngine.Object.DestroyImmediate(shadowObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CaptureReportedSageThrowAndStoneBreak(bool stone)
        {
            var rows = new List<object>();
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "Assets/NTSD/Content/LoganRuntime", NTSD28UnityRawCaptureEditor.DefaultScenario,
                BattleRuntimeProfile.Authority400, 120, (driver, inputs, identity) =>
                {
                    var world = driver.World;
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    world.Runtime.Flow.FrameToggle = 0;
                    world.Runtime.Flow.InputPhase = 0;
                    var p1 = world.FindEntityByRuntimeSlotForQuery(0);
                    var p2 = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(p1.TryApplyRuntimeIdentity(99, stone ? 0 : 278, true, out _), Is.True);
                    p1.Runtime.X = 500; p1.Runtime.Y = 0; p1.Runtime.Z = 600;
                    p1.Runtime.Dir = "right"; p1.Runtime.SyncIntegerPosition();
                    p2.Runtime.X = 700; p2.Runtime.Y = 0; p2.Runtime.Z = 600;
                    p2.Runtime.SyncIntegerPosition();
                    if (stone)
                    {
                        var task = new OPointCreateTask
                        {
                            targetWorld = world, requiredRuntimeSlot = 50, preserveActionZero = true,
                            nativeWeaponPieceSpawn = true, dir = "right", relationTeam = 1,
                            useDirectRuntimePosition = true, directX = 800, directY = -80, directZ = 600,
                            opoint = new ObjectPoint { oid = 150, action = 0 }
                        };
                        Assert.That(world.LogicEntityFactory.Create(task, out var failure), Is.Not.Null, failure.ToString());
                    }
                    for (int tick = 1; tick <= 240; tick++)
                    {
                        if (stone && tick == 20)
                        {
                            var rock = world.FindEntityByRuntimeSlotForQuery(50);
                            Assert.That(rock, Is.Not.Null);
                            rock.Runtime.WeaponFlightCounter = -1;
                        }
                        Assert.That(driver.StepOneTick(new FrameInputSet(tick, new[]
                        {
                            new SimulationPlayerInput(0, SimulationInputButtons.None),
                            new SimulationPlayerInput(1, SimulationInputButtons.None)
                        }), ignorePaused: true, buildPresentation: false), Is.True);
                        var entities = new List<LF2Entity>();
                        world.GetAllEntities(entities);
                        rows.Add(new
                        {
                            tick, hp = p2.Health.HP,
                            entities = entities.OfType<LF2Entity>().Select(entity => new
                            {
                                slot = entity.Runtime.SlotIndex, oid = entity.ObjectId,
                                action = entity.Frame.N, state = entity.Frame.D?.state, pic = entity.Frame.D?.pic,
                                entity.Runtime.X, entity.Runtime.Y, entity.Runtime.Z,
                                entity.Runtime.Vy, entity.Runtime.NativeLifecycleResolutionPending,
                                shadowEligible = !LF2Entity.ShouldHideShadowForPresentation(entity.Frame.D,
                                    entity.Runtime.LinkState, LF2Entity.ResolveCurrentDataObjectId(entity),
                                    entity.Runtime.HitStop) && !entity.IsNativeShadowSuppressedForPresentation(entity.Frame.D)
                            }).ToArray()
                        });
                    }
                }, useProjectMode: true);
            using var writer = new StreamWriter(new FileStream(OutputRoot + (stone ? "stone-" : "sage-") +
                Guid.NewGuid().ToString("N") + ".json", FileMode.CreateNew));
            writer.Write(JsonConvert.SerializeObject(rows, Formatting.Indented));
            Assert.That(rows.Count, Is.EqualTo(240));
        }

        [Test]
        public void CaptureActualDatEndToEndLifetimes()
        {
            var rows = new List<object>();
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "Assets/NTSD/Content/LoganRuntime", NTSD28UnityRawCaptureEditor.DefaultScenario,
                BattleRuntimeProfile.Authority400, 120, (driver, inputs, identity) =>
                {
                    var world = driver.World;
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    world.Runtime.Flow.FrameToggle = 0;
                    world.Runtime.Flow.InputPhase = 0;
                    for (int slot = 0; slot < 2; slot++)
                    {
                        var actor = world.FindEntityByRuntimeSlotForQuery(slot);
                        actor.Runtime.X = slot == 0 ? 300 : 1400;
                        actor.Runtime.Z = 450;
                        actor.Runtime.SyncIntegerPosition();
                    }
                    int[] oids = { 999, 999, 999, 999, 150, 434, 223 };
                    int[] actions = { 0, 4, 109, 101, 0, 0, 0 };
                    for (int index = 0; index < oids.Length; index++)
                    {
                        var task = new OPointCreateTask
                        {
                            targetWorld = world,
                            requiredRuntimeSlot = 50 + index,
                            preserveActionZero = true,
                            nativeWeaponPieceSpawn = true,
                            dir = "right",
                            relationTeam = 1,
                            useDirectRuntimePosition = true,
                            directX = 500 + 100 * index,
                            directY = -20,
                            directZ = 450,
                            useDirectVelocity = true,
                            directVx = 0,
                            directVy = -2,
                            directVz = 0,
                            opoint = new ObjectPoint { oid = oids[index], action = actions[index] }
                        };
                        var entity = world.LogicEntityFactory.Create(task, out var failure);
                        Assert.That(entity, Is.Not.Null, failure.ToString());
                        entity.Runtime.HitStop = 0;
                        entity.Runtime.LinkState = 0;
                    }
                    for (int tick = 1; tick <= 240; tick++)
                    {
                        Assert.That(driver.StepOneTick(new FrameInputSet(tick, new[]
                        {
                            new SimulationPlayerInput(0, SimulationInputButtons.None),
                            new SimulationPlayerInput(1, SimulationInputButtons.None)
                        }), ignorePaused: true, buildPresentation: false), Is.True);
                        for (int slot = 50; slot < 57; slot++)
                        {
                            var entity = world.FindEntityByRuntimeSlotForQuery(slot);
                            if (entity == null)
                            {
                                rows.Add(new { tick, slot, active = false });
                                continue;
                            }
                            var frame = entity.Frame.D;
                            rows.Add(new
                            {
                                tick, slot, active = true, oid = entity.ObjectId,
                                action = entity.Frame.N, frameId = frame?.frameId,
                                state = frame?.state, pic = frame?.pic,
                                wait = entity.Trans.WaitCounter,
                                entity.Runtime.X, entity.Runtime.Y, entity.Runtime.Z,
                                entity.Runtime.Vy, entity.Runtime.HitStop,
                                entity.Runtime.NativeLifecycleResolutionPending,
                                entity.Runtime.NativeLifecycleCode,
                                entity.Runtime.WeaponFlightCounter,
                                shadowEligible = !LF2Entity.ShouldHideShadowForPresentation(
                                    frame, entity.Runtime.LinkState, LF2Entity.ResolveCurrentDataObjectId(entity),
                                    entity.Runtime.HitStop) && !entity.IsNativeShadowSuppressedForPresentation(frame)
                            });
                        }
                    }
                }, useProjectMode: true);
            string output = OutputRoot + "full-driver-lifetimes-" + Guid.NewGuid().ToString("N") + ".json";
            using var writer = new StreamWriter(new FileStream(output, FileMode.CreateNew));
            writer.Write(JsonConvert.SerializeObject(rows, Formatting.Indented));
            Assert.That(rows.Count, Is.EqualTo(1680));
        }
    }
}
#endif
