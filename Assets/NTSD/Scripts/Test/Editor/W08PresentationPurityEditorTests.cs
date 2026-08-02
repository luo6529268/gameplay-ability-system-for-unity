#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class W08PresentationPurityEditorTests
    {
        private static readonly FieldInfo RendererLogicObjectField =
            typeof(LF2ObjectRenderer).GetField(
                "_logicObject",
                BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void SpawnPositionContract_PreparesOrdinaryLateStageAndRespawnCoordinatesBeforeInit()
        {
            OPointCreateTask ordinary = CreatePositionTask(10.75, -2.5, 30.25, false);
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(ordinary);
            AssertPositionTask(ordinary, 10.75, -2.5, 31.25, 10, -2, 31);

            OPointCreateTask late = CreatePositionTask(20.5, -8.75, 40.75, true);
            late.useInitialRuntimeIntPosition = true;
            late.initialRuntimeX = 20;
            late.initialRuntimeY = -11;
            late.initialRuntimeZ = 40;
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(late);
            AssertPositionTask(late, 20.5, -8.75, 40.75, 20, -11, 40);

            OPointCreateTask stage = CreatePositionTask(300.0, -100.0, 220.0, true);
            stage.useInitialRuntimeIntPosition = true;
            stage.initialRuntimeX = 300;
            stage.initialRuntimeY = -100;
            stage.initialRuntimeZ = 220;
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(stage);
            AssertPositionTask(stage, 300.0, -100.0, 220.0, 300, -100, 220);

            OPointCreateTask respawn = CreatePositionTask(90.0, -300.0, 150.0, false);
            respawn.useInitialRuntimeIntPosition = true;
            respawn.initialRuntimeX = 90;
            respawn.initialRuntimeY = -300;
            respawn.initialRuntimeZ = 151;
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(respawn);
            AssertPositionTask(respawn, 90.0, -300.0, 151.0, 90, -300, 151);
        }

        [Test]
        public void SpawnPositionContract_FourEntityKindsPreserveExplicitFloatAndIntegerValuesAcrossSnapshot()
        {
            LF2Entity[] entities =
            {
                new LF2Character(),
                new LF2Weapon(),
                new LF2SpecialAttack(),
                new LF2OtherObject(),
            };
            OPointCreateTask task = CreatePositionTask(12.75, -5.25, 9.875, true);
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = 12;
            task.initialRuntimeY = -8;
            task.initialRuntimeZ = 9;

            foreach (LF2Entity entity in entities)
            {
                entity.ApplyInitialRuntimePosition(task);
                entity.RefreshRuntimeSnapshot();
                AssertRuntimePosition(entity.Runtime, 12.75, -5.25, 9.875, 12, -8, 9);
            }
        }

        [Test]
        public void HitFa13DivergentY_SurvivesRegistrationAndForceRefreshUntilPhysicsSync()
        {
            using var logging = new DisabledLoggingScope();
            var world = new SimulationWorld();
            var entity = new PositionProbeEntity(701);
            OPointCreateTask task = CreatePositionTask(120.75, -14.25, 205.5, true);
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = 120;
            task.initialRuntimeY = -17;
            task.initialRuntimeZ = 205;
            entity.ApplyInitialRuntimePosition(task);
            entity.SetRequiredRuntimeSlot(50);
            world.Register(entity);

            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(50));
            AssertRuntimePosition(entity.Runtime, 120.75, -14.25, 205.5, 120, -17, 205);
            string noRendererChecksum = world.CaptureParityFrameSnapshot(7).OverallChecksum;

            var rendererObject = new GameObject("W08PresentationOnlyRenderer");
            rendererObject.SetActive(false);
            rendererObject.AddComponent<SpriteRenderer>();
            LF2ObjectRenderer renderer = rendererObject.AddComponent<LF2ObjectRenderer>();
            Assert.That(RendererLogicObjectField, Is.Not.Null);
            RendererLogicObjectField.SetValue(renderer, entity);

            try
            {
                renderer.ForceRefreshPresentation();
                string oneRefreshChecksum = world.CaptureParityFrameSnapshot(7).OverallChecksum;
                for (int i = 0; i < 8; i++)
                    renderer.ForceRefreshPresentation();
                string manyRefreshChecksum = world.CaptureParityFrameSnapshot(7).OverallChecksum;

                Assert.That(oneRefreshChecksum, Is.EqualTo(noRendererChecksum));
                Assert.That(manyRefreshChecksum, Is.EqualTo(noRendererChecksum));
                AssertRuntimePosition(entity.Runtime, 120.75, -14.25, 205.5, 120, -17, 205);

                entity.Runtime.SyncIntegerPosition();
                AssertRuntimePosition(entity.Runtime, 120.75, -14.25, 205.5, 120, -14, 205);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rendererObject);
            }
        }

        [Test]
        public void ExplicitLogicPositionCommits_CpointHeldAndThrowWriteRequiredIntegerFields()
        {
            var catcher = new PositionProbeEntity(801);
            var victim = new PositionProbeEntity(802);
            catcher.Runtime.SetPosition(100.75, -20.5, 50.75);
            catcher.Runtime.XInt = 100;
            catcher.Runtime.YInt = -20;
            catcher.Runtime.ZInt = 50;
            catcher.PS.dir = "right";
            victim.PS.dir = "right";
            victim.SetFrame(new LF2FrameData
            {
                frameId = 0,
                centerx = 20,
                centery = 20,
                cpoint = new CatchPoint { kind = 2, x = 5, y = 6 },
            });
            LF2FrameData catcherFrame = new LF2FrameData
            {
                frameId = 0,
                centerx = 30,
                centery = 40,
            };
            CatchPoint held = new CatchPoint { kind = 1, x = 10, y = 15, cover = 1 };

            catcher.ApplyHeldPosition(victim, catcherFrame, held);
            AssertRuntimePosition(victim.Runtime, 95.0, -32.0, 51.0, 95, -32, 51);

            victim.Runtime.Z = 70.75;
            victim.Runtime.ZInt = 69;
            CatchPoint thrown = new CatchPoint
            {
                kind = 1,
                x = 10,
                y = 15,
                throwvx = 4,
                throwvy = -3,
                vaction = 0,
            };
            catcher.ApplyThrowPosition(victim, thrown, catcherFrame);
            Assert.That(victim.Runtime.X, Is.EqualTo(80.0));
            Assert.That(victim.Runtime.Y, Is.EqualTo(-45.0));
            Assert.That(victim.Runtime.XInt, Is.EqualTo(80));
            Assert.That(victim.Runtime.YInt, Is.EqualTo(-45));
            Assert.That(victim.Runtime.Z, Is.EqualTo(70.75));
            Assert.That(victim.Runtime.ZInt, Is.EqualTo(69),
                "C# CPoint throw commits X/Y only and must not opportunistically sync Z");
        }

        [Test]
        public void PositionTaskClear_RemovesAllSpawnCoordinatesForPoolReuse()
        {
            OPointCreateTask task = CreatePositionTask(8.5, -7.5, 6.5, true);
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = 8;
            task.initialRuntimeY = -7;
            task.initialRuntimeZ = 6;

            task.Clear();

            Assert.That(task.useDirectRuntimePosition, Is.False);
            Assert.That(task.useInitialRuntimeIntPosition, Is.False);
            Assert.That(task.directX, Is.Zero);
            Assert.That(task.directY, Is.Zero);
            Assert.That(task.directZ, Is.Zero);
            Assert.That(task.initialRuntimeX, Is.Zero);
            Assert.That(task.initialRuntimeY, Is.Zero);
            Assert.That(task.initialRuntimeZ, Is.Zero);
            Assert.That(typeof(OPointCreateTask).GetField("initialRuntimeHoldMode"), Is.Null);
        }

        private static OPointCreateTask CreatePositionTask(
            double x,
            double y,
            double z,
            bool skipFactoryZOffset)
        {
            return new OPointCreateTask
            {
                pos = new Vector3((float)x, (float)y, (float)z),
                z = (float)z,
                useDirectRuntimePosition = true,
                directX = x,
                directY = y,
                directZ = z,
                skipPostInitZOffset = skipFactoryZOffset,
            };
        }

        private static void AssertPositionTask(
            OPointCreateTask task,
            double x,
            double y,
            double z,
            int xInt,
            int yInt,
            int zInt)
        {
            Assert.That(task.useDirectRuntimePosition, Is.True);
            Assert.That(task.useInitialRuntimeIntPosition, Is.True);
            Assert.That(task.directX, Is.EqualTo(x));
            Assert.That(task.directY, Is.EqualTo(y));
            Assert.That(task.directZ, Is.EqualTo(z));
            Assert.That(task.initialRuntimeX, Is.EqualTo(xInt));
            Assert.That(task.initialRuntimeY, Is.EqualTo(yInt));
            Assert.That(task.initialRuntimeZ, Is.EqualTo(zInt));
            Assert.That(task.skipPostInitZOffset, Is.True);
        }

        private static void AssertRuntimePosition(
            NTSDEntityRuntime runtime,
            double x,
            double y,
            double z,
            int xInt,
            int yInt,
            int zInt)
        {
            Assert.That(runtime.X, Is.EqualTo(x));
            Assert.That(runtime.Y, Is.EqualTo(y));
            Assert.That(runtime.Z, Is.EqualTo(z));
            Assert.That(runtime.XInt, Is.EqualTo(xInt));
            Assert.That(runtime.YInt, Is.EqualTo(yInt));
            Assert.That(runtime.ZInt, Is.EqualTo(zInt));
        }

        private sealed class PositionProbeEntity : LF2OtherObject
        {
            internal PositionProbeEntity(int stableId)
            {
                StableId = stableId;
                Runtime.StableId = stableId;
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
            }

            internal void SetFrame(LF2FrameData frame)
            {
                Frame.D = frame;
                Frame.N = frame?.frameId ?? 0;
            }

            internal void ApplyHeldPosition(
                LF2Entity victim,
                LF2FrameData catcherFrame,
                CatchPoint catcherCpoint)
            {
                SyncCpointHeldPositionStep10(victim, catcherFrame, catcherCpoint);
            }

            internal void ApplyThrowPosition(
                LF2Entity victim,
                CatchPoint cpoint,
                LF2FrameData throwFrame)
            {
                ApplyCpointThrowStep10(cpoint, victim, throwFrame);
            }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool previous;

            internal DisabledLoggingScope()
            {
                previous = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = previous;
            }
        }
    }
}
#endif
