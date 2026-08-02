#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class AiSensingSoAShadowEditorTests
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        [Test]
        public void Modes_DefaultLegacy_ShadowIsPure_AndAuthoritativeSoAFailsFast()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(world, 2, 1, 2, 30, 0, 4, 0, 500, false);
            SeedSentinelInput(self.Runtime);
            world.Rng.Seed(0x51A0u);

            Assert.That(
                world.AiSensingMode,
                Is.EqualTo(AiSensingMode.LegacyAiSensing));
            ulong inputBefore = InputChecksum(self.Runtime);
            uint rngBefore = world.Rng.State;
            ulong callsBefore = world.Rng.CallCount;

            world.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            object[] nearestArgs = { self, 1, -1, 10000, false };
            Assert.That(
                Invoke(world, "CaptureAiSoASensingNearestForSelfCheck", nearestArgs),
                Is.True);
            Assert.That(InputChecksum(self.Runtime), Is.EqualTo(inputBefore));
            Assert.That(world.Rng.State, Is.EqualTo(rngBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(callsBefore));

            world.AiSensingMode = AiSensingMode.LegacyAiSensing;
            Assert.That(
                () => world.AiSensingMode = AiSensingMode.SoAAiSensing,
                Throws.TypeOf<NotSupportedException>());
            Assert.That(
                world.AiSensingMode,
                Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(InputChecksum(self.Runtime), Is.EqualTo(inputBefore));
            Assert.That(world.Rng.State, Is.EqualTo(rngBefore));
            Assert.That(world.Rng.CallCount, Is.EqualTo(callsBefore));
        }

        [Test]
        public void Nearest_GroundTieFiltersAndAirBoundaries_PublishExactParityTrace()
        {
            SimulationWorld ground = CreateGroundTieWorld(out LF2Character groundSelf);
            RunShadowInputPass(ground, 2);
            AssertCleanPublishedComparison(ground, 1);
            AssertTrace(ground, "InitialSelectedSlot", 2);
            AssertTrace(ground, "InitialBestDist", 10);
            AssertTrace(ground, "InitialSameZLane", true);

            var air = new SimulationWorld();
            air.Runtime.Flow.InputPhase = 2;
            LF2Character airSelf = RegisterCharacter(
                air, 0, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(air, 5, 1, 2, 100, 0, 0, 0, 500, false);
            RegisterCharacter(air, 9, 1, 2, -249, 3, -39, 0, 500, false);
            RegisterCharacter(air, 2, 1, 2, 249, 3, 39, 0, 500, false);
            RegisterCharacter(air, 1, 1, 2, 250, 3, 0, 0, 500, false);
            RegisterCharacter(air, 3, 1, 2, 0, 3, 40, 0, 500, false);

            RunShadowInputPass(air, 2);
            AssertCleanPublishedComparison(air, 1);
            AssertTrace(air, "InitialSelectedSlot", 2);
            AssertTrace(air, "InitialBestDist", 100);
            AssertTrace(air, "InitialSameZLane", true);
            Assert.That(airSelf.Runtime.Unk360, Is.EqualTo(2));
        }

        [Test]
        public void SpecialScan_NotFound_SelectableD5_AndC8ThreatRestoreMatchLegacyTrace()
        {
            SimulationWorld notFound = CreateSpecialWorld(
                includeD5: false,
                includeC8Threat: false,
                out _);
            RunShadowInputPass(notFound, 2);
            AssertCleanPublishedComparison(notFound, 1);
            AssertTrace(notFound, "InitialSelectedSlot", 2);
            AssertTrace(notFound, "PostSpecialSelectedSlot", 2);
            AssertTrace(notFound, "SpecialBestDist", 10000);
            Assert.That(TraceInt(notFound, "SpecialFlags") & (1 << 8), Is.Zero);

            SimulationWorld selectable = CreateSpecialWorld(
                includeD5: true,
                includeC8Threat: false,
                out LF2Character selectableSelf);
            RunShadowInputPass(selectable, 2);
            AssertCleanPublishedComparison(selectable, 1);
            AssertTrace(selectable, "InitialSelectedSlot", 2);
            AssertTrace(selectable, "PostSpecialSelectedSlot", 20);
            AssertTrace(selectable, "SpecialBestDist", 20);
            Assert.That(selectableSelf.Runtime.Unk360, Is.EqualTo(20));

            SimulationWorld restored = CreateSpecialWorld(
                includeD5: true,
                includeC8Threat: true,
                out LF2Character restoredSelf);
            RunShadowInputPass(restored, 2);
            AssertCleanPublishedComparison(restored, 1);
            AssertTrace(restored, "InitialSelectedSlot", 2);
            AssertTrace(restored, "PostSpecialSelectedSlot", 2);
            AssertTrace(restored, "SpecialBestDist", 20);
            Assert.That(TraceInt(restored, "SpecialFlags") & (1 << 8), Is.Not.Zero);
            Assert.That(restoredSelf.Runtime.Unk360, Is.EqualTo(2));
        }

        [Test]
        public void EarlierSlotRefresh_UpdatesCurrentCharacterDatNonCharacterShellRow()
        {
            var world = new SimulationWorld();
            world.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            CharacterDatShell shell = RegisterShell(
                world, 0, 1, 2, 100, 0, 0, 0, 500);
            LF2Character self = RegisterCharacter(
                world, 1, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(world, 2, 1, 2, 50, 0, 0, 0, 500, false);

            Invoke(world, "BuildAiInputSlotSnapshot");
            try
            {
                shell.Runtime.SetPosition(10, 0, 0);
                shell.Runtime.SyncIntegerPosition();
                Invoke(world, "RefreshAiSoASensingShadowRowAfterCharacterInput", shell);

                object rows = GetField(world, "aiSoASensingRows");
                int[] xRows = (int[])GetField(rows, "X");
                Assert.That(xRows[0], Is.EqualTo(10));
                Assert.That(shell, Is.Not.InstanceOf<LF2Character>());
                Assert.That(
                    shell.GetCurrentDataObjectTypeForSimulation(),
                    Is.EqualTo((int)LF2ObjectType.Character));

                object result = RunPureShadowQuery(world, self, 2);
                Assert.That(ResultInt(result, "InitialSelectedSlot"), Is.EqualTo(0));
                Assert.That(ResultInt(result, "InitialBestDist"), Is.EqualTo(10));
            }
            finally
            {
                Invoke(world, "ClearAiInputSlotSnapshot");
            }
        }

        [TestCase(DriftKind.Epoch)]
        [TestCase(DriftKind.Generation)]
        [TestCase(DriftKind.Identity)]
        public void SnapshotDrift_FailsClosedWithoutStalePublication_AndLegacyRunsOnce(
            DriftKind driftKind)
        {
            SimulationWorld shadow = CreateParityWorld(out LF2Character shadowSelf);
            SimulationWorld legacy = CreateParityWorld(out LF2Character legacySelf);
            shadow.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            shadow.ResetAiSoASensingShadowDiagnostics();
            shadow.Rng.Seed(0xD12Fu);
            legacy.Rng.Seed(0xD12Fu);

            Invoke(shadow, "BuildAiInputSlotSnapshot");
            Invoke(legacy, "BuildAiInputSlotSnapshot");
            try
            {
                CorruptShadowSnapshot(shadow, driftKind);
                ulong shadowCallsBefore = shadow.Rng.CallCount;
                ulong legacyCallsBefore = legacy.Rng.CallCount;
                int shadowInvocationCount = 0;
                int legacyInvocationCount = 0;

                Invoke(shadow, "PrepareAiInputBasic", shadowSelf, 2);
                shadowInvocationCount++;
                Invoke(legacy, "PrepareAiInputBasic", legacySelf, 2);
                legacyInvocationCount++;

                Assert.That(shadowInvocationCount, Is.EqualTo(1));
                Assert.That(legacyInvocationCount, Is.EqualTo(1));
                Assert.That(
                    shadow.Rng.CallCount - shadowCallsBefore,
                    Is.EqualTo(legacy.Rng.CallCount - legacyCallsBefore));
                Assert.That(shadow.Rng.CallCount - shadowCallsBefore, Is.GreaterThan(0));
                Assert.That(shadow.Rng.State, Is.EqualTo(legacy.Rng.State));
                Assert.That(InputChecksum(shadowSelf.Runtime), Is.EqualTo(InputChecksum(legacySelf.Runtime)));
                Assert.That(OverallChecksum(shadow, shadowSelf), Is.EqualTo(OverallChecksum(legacy, legacySelf)));
                Assert.That(shadow.AiSoASensingShadowInvalidationCountForDiagnostics, Is.EqualTo(1));
                Assert.That(shadow.AiSoASensingShadowQueryCountForDiagnostics, Is.Zero);
                Assert.That(shadow.AiSoASensingShadowComparisonPublishedForDiagnostics, Is.False);
                Assert.That(shadow.AiSoASensingShadowMismatchMaskForDiagnostics, Is.Zero);
                Assert.That(shadow.AiSoASensingShadowPurityMismatchCountForDiagnostics, Is.Zero);
                Assert.That(shadow.AiSoASensingShadowInitialMismatchCountForDiagnostics, Is.Zero);
                Assert.That(shadow.AiSoASensingShadowCachedMismatchCountForDiagnostics, Is.Zero);
                Assert.That(shadow.AiSoASensingShadowPostSpecialMismatchCountForDiagnostics, Is.Zero);
            }
            finally
            {
                Invoke(shadow, "ClearAiInputSlotSnapshot");
                Invoke(legacy, "ClearAiInputSlotSnapshot");
            }
        }

        [Test]
        public void LegacyAndShadow_SameSeedPreserveRngInputAndOverallChecksum()
        {
            SimulationWorld legacy = CreateParityWorld(out LF2Character legacySelf);
            SimulationWorld shadow = CreateParityWorld(out LF2Character shadowSelf);
            legacy.Rng.Seed(0x5EEDu);
            shadow.Rng.Seed(0x5EEDu);

            legacy.CharacterInputAll(2);
            RunShadowInputPass(shadow, 2);

            Assert.That(shadow.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(shadow.Rng.CallCount, Is.EqualTo(legacy.Rng.CallCount));
            Assert.That(InputChecksum(shadowSelf.Runtime), Is.EqualTo(InputChecksum(legacySelf.Runtime)));
            Assert.That(OverallChecksum(shadow, shadowSelf), Is.EqualTo(OverallChecksum(legacy, legacySelf)));
            AssertCleanPublishedComparison(shadow, 1);
        }

        [Test]
        public void PureShadowQuery_Warmed512IterationsAllocateZeroManagedBytes()
        {
            var world = new SimulationWorld();
            LF2Character self = RegisterCharacter(
                world, 0, 1, 1, 0, 0, 0, 0, 500, false);
            for (int slot = 1; slot < 160; slot++)
            {
                RegisterCharacter(
                    world,
                    slot,
                    1,
                    slot % 3 == 0 ? 1 : 2,
                    (slot % 20) * 17,
                    slot % 7 == 0 ? 3 : 0,
                    (slot / 20) * 13,
                    slot % 11 == 0 ? 14 : 0,
                    500,
                    false);
            }

            object allocated = Invoke(
                world,
                "MeasureAiSoASensingShadowAllocationsForSelfCheck",
                self,
                2,
                512);
            Assert.That((long)allocated, Is.Zero);
        }

        private static SimulationWorld CreateGroundTieWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 9, 1, 2, -5, 0, 5, 0, 500, false);
            RegisterCharacter(world, 2, 1, 2, 5, 0, -5, 0, 500, false);
            RegisterCharacter(world, 1, 1, 1, 1, 0, 0, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, 1, 0, 0, 0, 0, false);
            RegisterCharacter(world, 4, 1, 2, 1, 0, 0, 14, 500, false);
            RegisterCharacter(world, 5, 1, 2, 1, 3, 0, 0, 500, false);
            return world;
        }

        private static SimulationWorld CreateSpecialWorld(
            bool includeD5,
            bool includeC8Threat,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 9, 500, true);
            RegisterCharacter(world, 2, 1, 2, 100, 0, 50, 0, 500, false);
            if (includeD5)
            {
                RegisterCharacter(
                    world, 20, 0xD5, 1, 20, 0, 0, 0x3EC, 500, false);
            }
            if (includeC8Threat)
            {
                RegisterCharacter(
                    world, 21, 0xC8, 2, 10, 3, 0, 0, 500, false, 60);
            }
            return world;
        }

        private static SimulationWorld CreateParityWorld(out LF2Character self)
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.InputPhase = 2;
            self = RegisterCharacter(world, 0, 1, 1, 0, 0, 0, 0, 500, true);
            RegisterCharacter(world, 2, 1, 2, 80, 0, 6, 0, 500, false);
            RegisterCharacter(world, 3, 1, 2, -120, 0, -20, 0, 500, false);
            RegisterCharacter(world, 4, 1, 1, 30, 0, 2, 0, 450, false);
            RegisterCharacter(world, 5, 1, 2, 40, 3, 30, 14, 500, false);
            self.Runtime.Unk360 = 2;
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp,
            bool aiControlled,
            int frameId = 0)
        {
            LF2Character character = CreateCharacter(
                slot,
                objectId,
                team,
                x,
                y,
                z,
                state,
                hp,
                aiControlled,
                frameId);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(
            int runtimeSlot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp,
            bool aiControlled,
            int frameId)
        {
            var character = new LF2Character();
            InitializeEntity(
                character,
                runtimeSlot,
                objectId,
                team,
                x,
                y,
                z,
                state,
                hp,
                frameId);
            character.Controller = new EmptyController();
            character.AiControlled = aiControlled;
            return character;
        }

        private static CharacterDatShell RegisterShell(
            SimulationWorld world,
            int slot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp)
        {
            var shell = new CharacterDatShell();
            InitializeEntity(shell, slot, objectId, team, x, y, z, state, hp, 0);
            world.Register(shell);
            return shell;
        }

        private static void InitializeEntity(
            LF2Entity entity,
            int runtimeSlot,
            int objectId,
            int team,
            int x,
            int y,
            int z,
            int state,
            int hp,
            int frameId)
        {
            var frame = new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"AiSoAShadow_{runtimeSlot}_{objectId}",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            LF2Character character = entity as LF2Character;
            character?.ModuleInitialize();
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(frameId);
            entity.Frame.PN = frameId;
            entity.Frame.N = frameId;
            character?.Initialize(500, 500);
            entity.FrameDelay = 0;
            entity.SetRequiredRuntimeSlot(runtimeSlot);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.HP = hp;
            entity.Runtime.HP3 = 500;
            entity.Runtime.HPBound = 500;
            entity.Runtime.PP = 0;
            entity.Runtime.KillCount = -1;
            entity.Runtime.Unk3FC = -1001;
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void RunShadowInputPass(SimulationWorld world, int tickIndex)
        {
            world.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            world.ResetAiSoASensingShadowDiagnostics();
            world.CharacterInputAll(tickIndex);
        }

        private static void AssertCleanPublishedComparison(
            SimulationWorld world,
            int expectedQueryCount)
        {
            Assert.That(
                world.AiSoASensingShadowQueryCountForDiagnostics,
                Is.EqualTo(expectedQueryCount));
            Assert.That(
                world.AiSoASensingShadowComparisonPublishedForDiagnostics,
                Is.True);
            Assert.That(world.AiSoASensingShadowMismatchMaskForDiagnostics, Is.Zero);
            Assert.That(world.AiSoASensingShadowPurityMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoASensingShadowInitialMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoASensingShadowCachedMismatchCountForDiagnostics, Is.Zero);
            Assert.That(world.AiSoASensingShadowPostSpecialMismatchCountForDiagnostics, Is.Zero);
            Assert.That(
                world.AiSoASensingShadowFirstMismatchForDiagnostics.Kind,
                Is.EqualTo(AiSoASensingShadowMismatchKind.None));
        }

        private static void AssertTrace(SimulationWorld world, string fieldName, int expected)
        {
            Assert.That(TraceInt(world, fieldName), Is.EqualTo(expected), fieldName);
        }

        private static void AssertTrace(SimulationWorld world, string fieldName, bool expected)
        {
            object trace = GetField(world, "aiSoASensingExpected");
            Assert.That((bool)GetField(trace, fieldName), Is.EqualTo(expected), fieldName);
        }

        private static int TraceInt(SimulationWorld world, string fieldName)
        {
            object trace = GetField(world, "aiSoASensingExpected");
            return (int)GetField(trace, fieldName);
        }

        private static object RunPureShadowQuery(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            object[] arguments =
            {
                self.Runtime.SlotIndex,
                inputPhase,
                world.Rng.State,
                false,
                null,
            };
            Assert.That(
                Invoke(world, "TryRunAiSoASensingShadowQuery", arguments),
                Is.True);
            Assert.That(arguments[4], Is.Not.Null);
            return arguments[4];
        }

        private static int ResultInt(object result, string fieldName) =>
            (int)GetField(result, fieldName);

        private static void CorruptShadowSnapshot(
            SimulationWorld world,
            DriftKind driftKind)
        {
            if (driftKind == DriftKind.Epoch)
            {
                FieldInfo epoch = RequireField(
                    typeof(SimulationWorld),
                    "aiSoASensingSnapshotEpoch");
                epoch.SetValue(world, (ulong)epoch.GetValue(world) + 1UL);
                return;
            }

            object rows = GetField(world, "aiSoASensingRows");
            bool[] included = (bool[])GetField(rows, "Included");
            int slot = Array.FindIndex(included, value => value);
            Assert.That(slot, Is.GreaterThanOrEqualTo(0));
            if (driftKind == DriftKind.Generation)
            {
                uint[] generations = (uint[])GetField(rows, "Generation");
                generations[slot]++;
            }
            else
            {
                int[] identities = (int[])GetField(rows, "Identity");
                identities[slot]++;
            }
        }

        private static void SeedSentinelInput(NTSDEntityRuntime runtime)
        {
            runtime.KeyUp = 1;
            runtime.KeyLeft = 1;
            runtime.KeyAttack = 1;
            runtime.PrevDown = 1;
            runtime.PrevRight = 1;
            runtime.PrevJump = 1;
            runtime.CdDefend = 7;
            runtime.Unk360 = 2;
        }

        private static ulong InputChecksum(NTSDEntityRuntime input)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            unchecked
            {
                hash = (hash ^ input.KeyUp) * prime;
                hash = (hash ^ input.KeyDown) * prime;
                hash = (hash ^ input.KeyLeft) * prime;
                hash = (hash ^ input.KeyRight) * prime;
                hash = (hash ^ input.KeyAttack) * prime;
                hash = (hash ^ input.KeyJump) * prime;
                hash = (hash ^ input.KeyDefend) * prime;
                hash = (hash ^ input.PrevUp) * prime;
                hash = (hash ^ input.PrevDown) * prime;
                hash = (hash ^ input.PrevLeft) * prime;
                hash = (hash ^ input.PrevRight) * prime;
                hash = (hash ^ input.PrevAttack) * prime;
                hash = (hash ^ input.PrevJump) * prime;
                hash = (hash ^ input.PrevDefend) * prime;
                hash = (hash ^ input.CdUp) * prime;
                hash = (hash ^ input.CdDown) * prime;
                hash = (hash ^ input.CdLeft) * prime;
                hash = (hash ^ input.CdRight) * prime;
                hash = (hash ^ input.CdAttack) * prime;
                hash = (hash ^ input.CdJump) * prime;
                hash = (hash ^ input.CdDefend) * prime;
                hash = (hash ^ (uint)input.Unk360) * prime;
                if (input.InputHistory != null)
                {
                    for (int index = 0; index < input.InputHistory.Length; index++)
                        hash = (hash ^ (uint)input.InputHistory[index]) * prime;
                }
            }
            return hash;
        }

        private static ulong OverallChecksum(
            SimulationWorld world,
            LF2Entity self)
        {
            unchecked
            {
                ulong hash = InputChecksum(self.Runtime);
                hash = hash * 1099511628211UL ^ world.Rng.State;
                hash = hash * 1099511628211UL ^ world.Rng.CallCount;
                hash = hash * 1099511628211UL ^ (uint)self.Runtime.Unk360;
                return hash;
            }
        }

        private static object Invoke(
            SimulationWorld world,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                InstanceMembers);
            Assert.That(method, Is.Not.Null, methodName);
            return method.Invoke(world, arguments);
        }

        private static object GetField(object instance, string fieldName)
        {
            Assert.That(instance, Is.Not.Null, fieldName);
            FieldInfo field = RequireField(instance.GetType(), fieldName);
            return field.GetValue(instance);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, InstanceMembers);
            Assert.That(field, Is.Not.Null, $"{type.FullName}.{fieldName}");
            return field;
        }

        public enum DriftKind
        {
            Epoch,
            Generation,
            Identity,
        }

        private sealed class CharacterDatShell : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
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
