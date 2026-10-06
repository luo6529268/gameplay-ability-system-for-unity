#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Tools;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSDHitboxGizmoProjectionEditorTests
    {
        [Test]
        [Category("NTSD28_336B44")]
        public void DiagnosticsBodyVolume_IgnoresPoisonedSpriteOrigin_AndUsesCollisionFrame()
        {
            LF2FrameData collisionFrame = MakeFrame(
                0,
                11,
                7,
                new BattleBodyBoxValue(-20, -10, 30, 40));
            LF2FrameData currentFrame = MakeFrame(
                1,
                30,
                12,
                new BattleBodyBoxValue(70, 80, 10, 10));
            SimulationWorld world = new SimulationWorld();
            LF2Character character = CreateCharacter(
                world,
                new List<LF2FrameData> { collisionFrame, currentFrame },
                currentFrame);

            character.Runtime.SetPosition(250, -30, 19);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.Dir = "right";
            PoisonSpriteOrigin(character, 900f, 1800f, 2700f, "left");

            PhysicsState.BattleVolume first;
            Assert.That(
                NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    collisionFrame.bodies[0],
                    out first),
                Is.True);
            Assert.That(first.x, Is.EqualTo(219f));
            Assert.That(first.y, Is.EqualTo(-47f));
            Assert.That(first.z, Is.EqualTo(19f));
            Assert.That(first.w, Is.EqualTo(30f));
            Assert.That(first.h, Is.EqualTo(40f));

            PhysicsState.BattleVolume stale = character.PS.GetBodyVolumes(
                collisionFrame.bodies,
                collisionFrame.centerx,
                collisionFrame.centery,
                100f)[0];
            Assert.That(stale.x, Is.Not.EqualTo(first.x));
            Assert.That(stale.y, Is.Not.EqualTo(first.y));
            Assert.That(stale.z, Is.Not.EqualTo(first.z));

            character.Frame.Prev2 = 1;
            character.Frame.Prev2D = currentFrame;
            character.Runtime.SetPosition(410, 23, -14);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.Dir = "left";
            PoisonSpriteOrigin(character, -700f, -1600f, -2500f, "right");

            LF2FrameData actualCollisionFrame = character.GetCollisionFrameData();
            Assert.That(actualCollisionFrame, Is.SameAs(currentFrame));
            PhysicsState.BattleVolume second;
            Assert.That(
                NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(
                    character,
                    actualCollisionFrame,
                    currentFrame.bodies[0],
                    out second),
                Is.True);
            Assert.That(second.x, Is.EqualTo(360f));
            Assert.That(second.y, Is.EqualTo(91f));
            Assert.That(second.z, Is.EqualTo(-14f));
            Assert.That(second.w, Is.EqualTo(10f));
            Assert.That(second.h, Is.EqualTo(10f));

            PhysicsState.BattleVolume production;
            Assert.That(
                BruteForceSceneQuery.TryBuildBodyBattleVolume(
                    character,
                    actualCollisionFrame,
                    currentFrame.bodies[0],
                    out production),
                Is.True);
            AssertVolumeEqual(production, second);

            UnityEngine.Rect screenRect =
                NTSDHitboxGizmos.GetScreenRectForDiagnostics(character, second);
            Assert.That(screenRect.x, Is.EqualTo(second.x + second.vx));
            Assert.That(screenRect.y, Is.EqualTo(second.y + second.z + second.vy));
            Assert.That(screenRect.width, Is.EqualTo(second.w));
            Assert.That(screenRect.height, Is.EqualTo(second.h));
        }

        [Test]
        [Category("NTSD28_336B44")]
        public void DiagnosticsVolumes_ReuseProjectedRects_ForBodyItrAndPickup()
        {
            LF2FrameData collisionFrame = MakeFrame(
                0,
                9,
                6,
                new BattleBodyBoxValue(-25, -18, 40, 32));
            InteractionArea itr = new InteractionArea
            {
                kind = 0,
                x = -35,
                y = -22,
                w = 52,
                h = 27,
                zwidth = 17,
                hasGeometry = true,
            };
            collisionFrame.itrs.Add(itr);
            collisionFrame.wpoints.Add(new WeaponPoint
            {
                kind = 1,
                x = -12,
                y = -8,
                w = 18,
                h = 16,
            });

            SimulationWorld world = new SimulationWorld();
            LF2Character character = CreateCharacter(
                world,
                new List<LF2FrameData> { collisionFrame },
                collisionFrame);
            character.Runtime.SetPosition(188, -20, 48);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetSourceRulePosition(123, 48);
            character.Runtime.SyncSourceRuleIntegerPosition();
            character.Runtime.Dir = "right";
            PoisonSpriteOrigin(character, 3000f, 4000f, 5000f, "left");

            world.ConfigureFixedViewRunDistance(800, 550);
            Assert.That(
                NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    collisionFrame.bodies[0],
                    out PhysicsState.BattleVolume identityBody),
                Is.True);

            world.ConfigureFixedViewRunDistance(2048, 1152);
            Assert.That(
                NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    collisionFrame.bodies[0],
                    out PhysicsState.BattleVolume projectedBody),
                Is.True);
            Assert.That(projectedBody.w, Is.GreaterThan(identityBody.w));
            Assert.That(projectedBody.h, Is.GreaterThan(identityBody.h));
            Assert.That(projectedBody.zwidth, Is.GreaterThan(identityBody.zwidth));

            Assert.That(
                BruteForceSceneQuery.TryBuildItrVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    itr,
                    out PhysicsState.BattleVolume projectedItr),
                Is.True);
            Assert.That(projectedItr.w, Is.GreaterThan(itr.w));
            Assert.That(projectedItr.h, Is.GreaterThan(itr.h));
            Assert.That(
                projectedItr.zwidth,
                Is.EqualTo((float)(itr.zwidth * world.SpatialProjection.DepthScale))
                    .Within(0.001f));

            Assert.That(
                BruteForceSceneQuery.TryBuildPickupVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    collisionFrame.wpoints[0],
                    out PhysicsState.BattleVolume projectedPickup),
                Is.True);
            Assert.That(projectedPickup.w, Is.GreaterThan(collisionFrame.wpoints[0].w));
            Assert.That(projectedPickup.h, Is.GreaterThan(collisionFrame.wpoints[0].h));
            Assert.That(
                projectedPickup.zwidth,
                Is.EqualTo((float)(NTSDGlobal.Default.Itr.ZWidth *
                    world.SpatialProjection.DepthScale))
                    .Within(0.001f));

            UnityEngine.Rect screenRect =
                NTSDHitboxGizmos.GetScreenRectForDiagnostics(character, projectedBody);
            Assert.That(screenRect.x, Is.EqualTo(projectedBody.x + projectedBody.vx));
            Assert.That(
                screenRect.y,
                Is.EqualTo(projectedBody.y + projectedBody.z + projectedBody.vy));
            Assert.That(screenRect.width, Is.EqualTo(projectedBody.w));
            Assert.That(screenRect.height, Is.EqualTo(projectedBody.h));

            character.Runtime.SetPosition(276, 31, 72);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.Dir = "left";
            PoisonSpriteOrigin(character, -8000f, -9000f, -10000f, "right");
            Assert.That(
                NTSDHitboxGizmos.TryBuildBodyVolumeForDiagnostics(
                    character,
                    character.GetCollisionFrameData(),
                    collisionFrame.bodies[0],
                    out PhysicsState.BattleVolume movedBody),
                Is.True);
            Assert.That(movedBody.x, Is.Not.EqualTo(projectedBody.x));
            Assert.That(movedBody.y, Is.Not.EqualTo(projectedBody.y));
            Assert.That(movedBody.z, Is.EqualTo(72f));
        }

        private static LF2FrameData MakeFrame(
            int frameId,
            int centerx,
            int centery,
            BattleBodyBoxValue body)
        {
            var frame = new LF2FrameData
            {
                frameId = frameId,
                state = 0,
                wait = 1,
                next = frameId,
                centerx = centerx,
                centery = centery,
            };
            frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            List<LF2FrameData> frames,
            LF2FrameData currentFrame)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = "NTSDHitboxGizmoProjectionTest";
            character.ObjectId = 9810;
            character.Controller = NullLF2Controller.Instance;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(
                    character.ObjectId,
                    new LF2CharacterData
                    {
                        name = character.Name,
                        type_sub = (int)LF2ObjectType.Character,
                        frames = frames,
                    }));
            character.Frame.D = currentFrame;
            character.Frame.N = currentFrame.frameId;
            character.Frame.Prev2 = frames[0].frameId;
            character.Frame.Prev2D = frames[0];
            character.Runtime.PrevFrame2 = frames[0].frameId;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(0);
            world.Register(character);
            return character;
        }

        private static void PoisonSpriteOrigin(
            LF2Character character,
            float sx,
            float sy,
            float sz,
            string direction)
        {
            character.PS.sx = sx;
            character.PS.sy = sy;
            character.PS.sz = sz;
            character.PS.dir = direction;
        }

        private static void AssertVolumeEqual(
            PhysicsState.BattleVolume expected,
            PhysicsState.BattleVolume actual)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
            Assert.That(actual.vx, Is.EqualTo(expected.vx).Within(0.001f));
            Assert.That(actual.vy, Is.EqualTo(expected.vy).Within(0.001f));
            Assert.That(actual.w, Is.EqualTo(expected.w).Within(0.001f));
            Assert.That(actual.h, Is.EqualTo(expected.h).Within(0.001f));
            Assert.That(actual.zwidth, Is.EqualTo(expected.zwidth).Within(0.001f));
        }
    }
}
#endif
