#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiNearestSpecializedFilterEditorTests
    {
        [Test]
        public void SpecializedSnapshotFilter_DefaultsOnAndMatchesLegacyOracle()
        {
            var world = new SimulationWorld();
            LF2Character groundSelf = RegisterCharacter(
                world, 0, 1, 0, 0, 0, 9);
            RegisterCharacter(world, 1, 2, 40, 0, 5, 0);
            RegisterCharacter(world, 2, 2, -40, 0, 5, 0);
            RegisterCharacter(world, 3, 5, 20, 0, 0, 0);
            LF2Character airSelf = RegisterCharacter(
                world, 4, 1, 100, 0, 0, 0);
            RegisterCharacter(world, 5, 2, 110, 0, 0, 0);
            RegisterCharacter(world, 6, 2, 130, 3, 0, 14);

            PropertyInfo switchProperty = RequireProperty(
                "ForceLegacyAiNearestFilterForDiagnostics");
            Assert.That((bool)switchProperty.GetValue(world), Is.False);

            foreach (int inputPhase in new[] { 0, 1, 4 })
            {
                AssertParity(world, groundSelf, inputPhase);
                AssertParity(world, airSelf, inputPhase);
            }

            Assert.That((bool)switchProperty.GetValue(world), Is.False);
        }

        [Test]
        public void SpecializedSnapshotFilter_PreservesFactValidationFallbacks()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 0, 0, 0, 9);
            LF2Character candidate = RegisterCharacter(
                world, 1, 2, 40, 0, 0, 0);
            RegisterCharacter(world, 2, 2, 80, 0, 0, 0);

            for (int invalidationKind = 0;
                 invalidationKind <= 4;
                 invalidationKind++)
            {
                object[] arguments =
                {
                    self,
                    candidate,
                    0,
                    invalidationKind,
                    false,
                };
                bool matches = (bool)RequireMethod(
                        "AiNearestFactsValidationFallbackForSelfCheck")
                    .Invoke(world, arguments);
                Assert.That(matches, Is.True, $"kind={invalidationKind}");
                Assert.That(
                    (bool)arguments[4],
                    Is.True,
                    $"kind={invalidationKind}: fast path must abort");
            }
        }

        [Test]
        public void SnapshotStamp_RejectsEveryQueryLevelMutation()
        {
            var world = new SimulationWorld();
            RegisterCharacter(world, 0, 1, 0, 0, 0, 9);
            RegisterCharacter(world, 1, 2, 40, 0, 0, 0);

            for (int mutationKind = 0; mutationKind <= 4; mutationKind++)
            {
                bool rejected = (bool)RequireMethod(
                        "AiNearestSnapshotStampRejectsMutationForSelfCheck")
                    .Invoke(world, new object[] { mutationKind });
                Assert.That(rejected, Is.True, $"kind={mutationKind}");
            }
        }

        private static void AssertParity(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            uint rngStateBefore = world.Rng.State;
            ulong rngCallsBefore = world.Rng.CallCount;
            NearestResult specialized = Capture(
                world,
                self,
                inputPhase,
                false);
            Assert.That(world.Rng.State, Is.EqualTo(rngStateBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(rngCallsBefore));

            NearestResult legacy = Capture(
                world,
                self,
                inputPhase,
                true);
            Assert.That(world.Rng.State, Is.EqualTo(rngStateBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(rngCallsBefore));
            Assert.That(
                specialized.Slot,
                Is.EqualTo(legacy.Slot),
                $"phase={inputPhase}");
            Assert.That(
                specialized.Distance,
                Is.EqualTo(legacy.Distance),
                $"phase={inputPhase}");
            Assert.That(
                specialized.SameZ,
                Is.EqualTo(legacy.SameZ),
                $"phase={inputPhase}");
        }

        private static NearestResult Capture(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            bool forceLegacyFilter)
        {
            PropertyInfo switchProperty = RequireProperty(
                "ForceLegacyAiNearestFilterForDiagnostics");
            bool previous = (bool)switchProperty.GetValue(world);
            try
            {
                switchProperty.SetValue(world, forceLegacyFilter);
                object[] arguments =
                {
                    self,
                    inputPhase,
                    false,
                    false,
                    -1,
                    10000,
                    false,
                };
                RequireMethod("CaptureAiNearestFactsTargetForSelfCheck")
                    .Invoke(world, arguments);
                return new NearestResult
                {
                    Slot = (int)arguments[4],
                    Distance = (int)arguments[5],
                    SameZ = (bool)arguments[6],
                };
            }
            finally
            {
                switchProperty.SetValue(world, previous);
            }
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int team,
            int x,
            int y,
            int z,
            int state)
        {
            LF2Character character = CreateCharacter(
                $"AiNearestSpecialized_{slot}",
                slot,
                team,
                x,
                y,
                z,
                state);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(
            string name,
            int runtimeSlot,
            int team,
            int x,
            int y,
            int z,
            int state)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = 1;
            character.Controller = new NullController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.SetPosition(x, y, z);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static MethodInfo RequireMethod(string methodName)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            Assert.That(method, Is.Not.Null, methodName);
            return method;
        }

        private static PropertyInfo RequireProperty(string propertyName)
        {
            PropertyInfo property = typeof(SimulationWorld).GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            Assert.That(property, Is.Not.Null, propertyName);
            return property;
        }

        private struct NearestResult
        {
            public int Slot;
            public int Distance;
            public bool SameZ;
        }

        private sealed class NullController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } =
                new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
