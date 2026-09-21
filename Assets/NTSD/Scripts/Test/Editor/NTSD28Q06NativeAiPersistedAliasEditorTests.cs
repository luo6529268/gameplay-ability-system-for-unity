#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;
using AiRows = NTSD.Simulation.SimulationAiSensingModule.AiSoASensingRows;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28Q06NativeAiPersistedAliasEditorTests
    {
        private const string EvidenceRoot = "artifacts/diagnostics/NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001/";

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        [TestCase(8)] [TestCase(9)] [TestCase(10)] [TestCase(11)] [TestCase(12)]
        public void ProfileAndSpecialDecisionMatchSource(int index)
        {
            JObject row = ReadSource(index);
            var rows = new AiRows(2);
            PopulateRows(rows, row);
            var input = new AiDecisionInputState { KeyUp = 1 };
            var owner = new NTSD28NativeRandom((uint)row["params"]["seed"]);
            var sites = new uint[32];
            var bounds = new int[32];
            var values = new int[32];
            var rng = new AiDecisionRandomStream(owner.CaptureSynchronizedCursor(), true,
                sites, bounds, new int[32], values);
            bool stopped = false;
            if ((string)row["params"]["api"] == "profile")
            {
                AiDecisionKernel.ProcessNativeProfiledCombat(rows, 0, 1,
                    (int)row["params"]["level61c"], (int)row["params"]["level618"],
                    false, false, ref input, ref rng);
            }
            else
            {
                stopped = AiDecisionKernel.TryApplyNativeSpecialProfile(rows, 0, 1,
                    (int)row["params"]["level618"], ref input, ref rng);
            }
            var differences = new List<string>();
            if ((string)row["params"]["api"] == "special")
                Compare(differences, "stopped", (int)row["result"]["returnValue"] != 0 ||
                    (bool)row["result"]["unsupported_custom_profile"], stopped);
            CompareInputAndCalls(row, input, rng.DrawCount, sites, bounds, values, differences);
            Compare(differences, "counter", (int)row["after"]["random"]["synchronized"]["counter"], rng.SynchronizedCounter);
            Compare(differences, "index", (int)row["after"]["random"]["synchronized"]["index"], rng.SynchronizedIndex);
            Assert.That(differences, Is.Empty, string.Join("\n", differences));
        }

        [TestCase(14)]
        [TestCase(15)]
        public void MainDecisionStopsOrContinuesAtSourceBoundary(int index)
        {
            JObject row = ReadSource(index);
            AiDecisionSnapshot snapshot = CreateMainSnapshot(row);
            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);
            var differences = new List<string>();
            CompareInputAndCalls(row, witness.Input, witness.RngDrawCount,
                snapshot.RngTraceCallSites, snapshot.RngTraceModuli, snapshot.RngTraceValues, differences);
            Assert.That(differences, Is.Empty, string.Join("\n", differences));
        }

        [Test]
        public void PositiveSpecialGateContinuesOrdinaryThroughMainKernel()
        {
            AiDecisionSnapshot snapshot = CreateMainSnapshot(ReadSource(14));
            snapshot.World.Difficulty = 3;
            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);
            Assert.That(snapshot.Rows.NativeAiProfileObjectId[0], Is.EqualTo(-1));
            Assert.That(snapshot.RngTraceCallSites[1], Is.EqualTo(0x3Cu));
            Assert.That(snapshot.RngTraceValues[1], Is.GreaterThan(0),
                "This representative must actually reject the special gate before alias inspection.");
            Assert.That(witness.RngDrawCount, Is.GreaterThan(2));
            Assert.That(snapshot.RngTraceCallSites[2], Is.EqualTo(0x1Cu),
                "The existing caller must continue ordinary movement after a positive special gate.");
        }

        private static AiDecisionSnapshot CreateMainSnapshot(JObject row)
        {
            var snapshot = new AiDecisionSnapshot(2);
            snapshot.Reset(7);
            PopulateRows(snapshot.Rows, row);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.OccupancyEpoch = 7;
            snapshot.Input.PrevUp = 1;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = 180;
            snapshot.World.StageZMax = 350;
            var owner = new NTSD28NativeRandom(42);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());
            return snapshot;
        }

        private static JObject ReadSource(int index)
        {
            return JObject.Parse(File.ReadAllLines(EvidenceRoot + "source/first.jsonl")[index]);
        }

        private static void PopulateRows(AiSensingSnapshot rows, JObject row)
        {
            rows.Reset(7);
            for (int slot = 0; slot < 2; slot++)
            {
                JToken entity = row["before"]["entities"][slot];
                JToken raw = entity["raw"];
                rows.Included[slot] = true;
                rows.Generation[slot] = 1;
                rows.Identity[slot] = 1000 + slot;
                rows.ObjectId[slot] = (int)raw["identity"]["objectId"];
                rows.NativeAiProfileObjectId[slot] = (int)entity["alias"];
                rows.DataObjectType[slot] = (int)raw["identity"]["objectType"];
                rows.X[slot] = (int)raw["position"]["x"];
                rows.Y[slot] = (int)raw["position"]["y"];
                rows.Z[slot] = (int)raw["position"]["z"];
                rows.Vx[slot] = (double)raw["motion"]["x"];
                rows.State[slot] = (int)entity["state"];
                rows.Frame[slot] = (int)raw["frame"]["action"];
                rows.Facing[slot] = (bool)raw["frame"]["facingLeft"] ? 1 : 0;
                rows.Hp[slot] = (int)raw["vitals"]["currentHp"];
                rows.Hp3[slot] = (int)raw["vitals"]["effectiveMaxHp"];
                rows.HpMax[slot] = (int)raw["vitals"]["baseMaxHp"];
                rows.Pp[slot] = (int)raw["vitals"]["currentMp"];
                rows.Team[slot] = (int)raw["identity"]["battleGroup"];
                rows.CachedTargetSlot[slot] = -1;
                rows.CoordinateTargetX[slot] = -1000;
            }
        }

        private static void CompareInputAndCalls(JObject source, AiDecisionInputState input,
            int count, uint[] sites, int[] bounds, int[] values, List<string> differences)
        {
            // Native button order, preserving the established Unity legacy button-name rotation.
            int[] pending = { input.KeyUp, input.KeyDown, input.KeyLeft, input.KeyRight,
                input.KeyJump, input.KeyDefend, input.KeyAttack };
            JToken expected = source["after"]["entities"][0];
            for (int key = 0; key < pending.Length; key++)
                Compare(differences, "pending[" + key + "]", (int)expected["pending"][key], pending[key]);
            Compare(differences, "combo2", (int)expected["input"]["input"]["comboState"][2], (int)input.ComboDua);
            var calls = (JArray)source["calls"]["synchronized"];
            Compare(differences, "drawCount", calls.Count, count);
            for (int call = 0; call < Math.Min(count, calls.Count); call++)
            {
                Compare(differences, "site[" + call + "]", (uint)calls[call]["callSite"], sites[call]);
                Compare(differences, "bound[" + call + "]", (int)calls[call]["upperBound"], bounds[call]);
                Compare(differences, "value[" + call + "]", (int)calls[call]["result"], values[call]);
            }
        }

        private static void Compare<T>(List<string> differences, string name, T expected, T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                differences.Add(name + ": source=" + expected + " Unity=" + actual);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ActualRowProducerCapturesPersistedAliasInsteadOfCurrentObjectId(int producer)
        {
            var world = new SimulationWorld();
            LF2Character character = Register(world);
            try
            {
                character.Runtime.NativeAiProfileObjectId = 34;
                AiSensingSnapshot rows = Capture(world, character, producer);
                Assert.That(rows.ObjectId[0], Is.EqualTo(77));
                Assert.That(Aliases(rows)[0], Is.EqualTo(34));

                character.Runtime.NativeAiProfileObjectId = -1;
                rows = Capture(world, character, producer);
                Assert.That(rows.ObjectId[0], Is.EqualTo(77));
                Assert.That(Aliases(rows)[0], Is.EqualTo(-1),
                    "An alias-only change must be visible without changing DAT or object ID.");
            }
            finally
            {
                world.Unregister(character);
            }
        }

        [Test]
        public void DerivedRowGrowthPreservesIndependentAliasAndActualIdentity()
        {
            var rows = new AiRows(2);
            rows.Reset(11);
            rows.Included[0] = true;
            rows.ObjectId[0] = 77;
            Aliases(rows)[0] = 33;
            rows.Included[1] = true;
            rows.ObjectId[1] = 34;
            Aliases(rows)[1] = -1;
            AiRows grown = rows.GrowTo(4);
            Assert.That(grown.ObjectId[0], Is.EqualTo(77));
            Assert.That(grown.ObjectId[1], Is.EqualTo(34));
            Assert.That(Aliases(grown)[0], Is.EqualTo(33));
            Assert.That(Aliases(grown)[1], Is.EqualTo(-1));
        }

        private static int[] Aliases(AiSensingSnapshot rows)
        {
            FieldInfo field = typeof(AiSensingSnapshot).GetField("NativeAiProfileObjectId");
            Assert.That(field, Is.Not.Null,
                "The derived AI snapshot must carry the persisted alias independently of ObjectId.");
            return (int[])field.GetValue(rows);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SnapshotComparisonReportsAliasOnlyDifference(bool fullComparison)
        {
            var world = new SimulationWorld();
            var runtime = (SimulationAiRuntime)typeof(SimulationWorld).GetField(
                "aiRuntime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
            var production = new AiRows(2);
            production.Reset(7);
            production.Included[0] = true;
            production.ObjectId[0] = 77;
            Aliases(production)[0] = 34;
            AiRows unified = production.GrowTo(2);
            runtime.Decision.UnifiedSnapshotRows = unified;
            // This fixture exercises row comparison; candidate products have a separate owner.
            runtime.Decision.UnifiedSnapshotProductsComparedThisPass = true;

            for (int attempt = 0; attempt < 2; attempt++)
            {
                if (attempt == 1)
                    Aliases(unified)[0] = -1;
                object[] arguments = fullComparison
                    ? new object[] { production, new int[2], AiUnifiedSnapshotConsumer.IndexedDecision, default(AiUnifiedSnapshotMismatch) }
                    : new object[] { production, unified, new int[2], AiUnifiedSnapshotConsumer.IndexedDecision, 0, default(AiUnifiedSnapshotMismatch) };
                MethodInfo method = typeof(SimulationAiDecisionModule).GetMethod(
                    fullComparison ? "TryCompareAiUnifiedSnapshotRows" : "TryCompareUnifiedSnapshotRow",
                    BindingFlags.NonPublic | (fullComparison ? BindingFlags.Instance : BindingFlags.Static));
                bool matched = (bool)method.Invoke(fullComparison ? runtime.Decision : null, arguments);
                Assert.That(matched, Is.EqualTo(attempt == 0));
                if (attempt == 1)
                {
                    var mismatch = (AiUnifiedSnapshotMismatch)arguments[arguments.Length - 1];
                    Assert.That(mismatch.Field.ToString(), Is.EqualTo("NativeAiProfileObjectId"));
                    Assert.That(mismatch.ExpectedValue, Is.EqualTo(34));
                    Assert.That(mismatch.ActualValue, Is.EqualTo(-1));
                }
            }
        }

        private static AiSensingSnapshot Capture(SimulationWorld world, LF2Character character, int producer)
        {
            if (producer == 2)
            {
                var snapshot = new AiDecisionSnapshot(world.RuntimeSlotCapacityForDiagnostics);
                Assert.That(world.CaptureAiDecisionShadowSnapshotForModule(character, snapshot),
                    Is.EqualTo(AiDecisionAvailability.Available));
                return snapshot.Rows;
            }

            var rows = new AiRows(world.RuntimeSlotCapacityForDiagnostics);
            rows.Reset(1);
            if (producer == 0)
            {
                Assert.That(SimulationAiSensingModule.TryCaptureRow(rows, character, 0, 1, false), Is.True);
            }
            else
            {
                Assert.That(world.TryCaptureAiUnifiedAuthorityRowForDecisionModule(
                    rows, character, 0, 1, false, out _), Is.True);
            }
            return rows;
        }

        [Test]
        public void PublishedRowGuardRejectsAliasOnlyStalenessAndAcceptsRecapture()
        {
            var world = new SimulationWorld();
            LF2Character character = Register(world);
            try
            {
                character.Runtime.NativeAiProfileObjectId = 34;
                var state = new SimulationAiDecisionModule.AiUnifiedSnapshotExecutionState(
                    world.RuntimeSlotCapacityForDiagnostics);
                Assert.That(world.TryCaptureAiUnifiedAuthorityRowForDecisionModule(
                    state.Rows, character, 0, 1, false, out int flags), Is.True);
                state.SoASensingBoundaryFlags[0] = state.Rows.BoundaryFlags[0];
                state.DecisionBoundaryFlags[0] = flags;
                var module = ((SimulationAiRuntime)WorldField(world, "aiRuntime")).Decision;
                MethodInfo guard = typeof(SimulationAiDecisionModule).GetMethod(
                    "ValidateUnifiedExecutionRowAfterCharacterInput", BindingFlags.Instance | BindingFlags.NonPublic);
                object[] arguments =
                {
                    WorldField(world, "battleIdentityWriter"), WorldField(world, "battleFrameMotionWriter"),
                    WorldField(world, "battleCharacterInputWriter"), WorldField(world, "battleRelationLinkWriter"),
                    WorldField(world, "battleVitalWriter"), state, state.Rows, character, 0, 1u
                };
                Assert.That((bool)guard.Invoke(module, arguments), Is.True);
                character.Runtime.NativeAiProfileObjectId = -1;
                Assert.That((bool)guard.Invoke(module, arguments), Is.False,
                    "The actual post-input guard must reject stale alias even when ObjectId is unchanged.");
                Assert.That(world.TryCaptureAiUnifiedAuthorityRowForDecisionModule(
                    state.Rows, character, 0, 1, false, out _), Is.True);
                Assert.That((bool)guard.Invoke(module, arguments), Is.True);
            }
            finally
            {
                world.Unregister(character);
            }
        }

        private static object WorldField(SimulationWorld world, string name)
        {
            return typeof(SimulationWorld).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
        }

        private static LF2Character Register(SimulationWorld world)
        {
            var data = new LF2CharacterData
            {
                name = "AliasCarrier77",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0 }
                }
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = 77;
            character.FrameCache.Load(new LF2CharacterDataWrapper(77, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Initialize(500, 500);
            character.SetRequiredRuntimeSlot(0);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.SetPosition(0, 0, 0);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = true;
            world.Register(character);
            return character;
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId) { }
        }
    }
}
#endif
