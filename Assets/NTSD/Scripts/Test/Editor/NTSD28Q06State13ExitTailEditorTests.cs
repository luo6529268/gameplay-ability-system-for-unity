#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06State13ExitTailEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-STATE13-EXIT-TAIL-RETIREMENT-001/";
        private const string Source = "artifacts/diagnostics/NTSD28-Q06-STATE13-EXIT-TAIL-SOURCE-WITNESS-001/source/first.jsonl";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void FullTickExitMatchesSource(int index) => Verify(index, false);

        internal static void VerifyRendererForPlay(int index) => Verify(index, true);

        private static void Verify(int index, bool renderer)
        {
            using (var stream = File.OpenRead(Source))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""),
                    Is.EqualTo("E7C238072075C441584EDB94FD86B326E842310A672009C52081687092C2C7E8"));
            var row = JObject.Parse(File.ReadLines(Source).Where(line => !string.IsNullOrWhiteSpace(line)).ElementAt(index));
            var world = new SimulationWorld();
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(!renderer);
                var source = Definition(7201, (string)row["dat"]);
                var particle = Definition(999, (string)row["particleDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(7201, 0, "exit.dat"), new ObjectDefinition(999, 3, "particle.dat")
                }, id => id == 7201 ? source : id == 999 ? particle : null);
                world.SetRuntimeCharacterConfigResolverForSelfCheck(id => id == 999 ? particle : id == 7201 ? source : null);
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = 21, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = 0, preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 7201, action = (int)row["current"] }
                };
                BattleLogicEntityCreationFailure failure = BattleLogicEntityCreationFailure.None;
                var entity = renderer ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                    : world.LogicEntityFactory.Create(task, out failure);
                if (renderer) Assert.That(entity?.Renderer, Is.Not.Null);
                Assert.That(entity, Is.Not.Null, failure.ToString());
                entity.AiControlled = false;
                entity.Runtime.SetPosition(300, -10, 250);
                entity.Runtime.SyncIntegerPosition();
                entity.Frame.Prev = (int)row["previous"];
                entity.Trans.SyncDirectFrameData(entity.Frame.D.wait, entity.Frame.D.next, (int)row["current"]);
                Assert.That(entity.Frame.N, Is.EqualTo((int)row["current"]));
                Assert.That(entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev).state, Is.EqualTo((int)row["previousState"]));
                world.NativeRandom.ResetFromSeed(42);
                world.Runtime.FunctionKeys.ResetForBattle(true);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                ulong legacy = world.Rng.CallCount;
                long audio = world.QueuedSoundEventCountForDiagnostics;
                var projectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
                var beforeRandom = JObject.FromObject(projectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() }));
                Assert.That(JToken.DeepEquals(beforeRandom, row["beforeRandom"]), Is.True, "initial RNG");
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                int count = 0;
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    if (world.FindEntityByRuntimeSlotForQuery(slot) != null) count++;
                var afterRandom = JObject.FromObject(projectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() }));
                Directory.CreateDirectory(Root);
                File.WriteAllText(Root + (renderer ? "renderer-" : "unity-") + index + ".json", new JObject
                {
                    ["index"] = index, ["count"] = count,
                    ["audioCount"] = world.QueuedSoundEventCountForDiagnostics - audio,
                    ["legacyCalls"] = world.Rng.CallCount - legacy, ["afterRandom"] = afterRandom,
                    ["expectedCount"] = row["count"].DeepClone()
                }.ToString());
                Assert.That(count, Is.EqualTo((int)row["count"]), "entity count");
                Assert.That(world.QueuedSoundEventCountForDiagnostics - audio, Is.EqualTo((int)row["audioCount"]), "audio");
                // Approved sparse C17 random-weapon exception; exit particles may add no calls.
                Assert.That(world.Rng.CallCount, Is.EqualTo(legacy + 1), "legacy RNG");
                Assert.That(JToken.DeepEquals(afterRandom, row["afterRandom"]), Is.True, "native RNG");
            }
            finally
            {
                if (renderer)
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            }
        }

        private static LF2CharacterDataWrapper Definition(int oid, string dat)
        {
            string root = Path.GetFullPath(Root + "fixture-runtime");
            return new LF2CharacterDataWrapper(oid, CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root)));
        }
    }
}
#endif
