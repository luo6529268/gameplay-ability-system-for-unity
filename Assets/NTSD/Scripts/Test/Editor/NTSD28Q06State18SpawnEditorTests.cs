#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06State18SpawnEditorTests
    {
        internal const string Witness = "artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001/";

        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameTickPassMode.Legacy, 0)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 0)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 1)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.DataOriented, 1)]
        public void BirthAndFullDriverMatchOriginal(BattleRuntimeProfile profile, BattleEcsCharacterFrameTickPassMode mode, int phase)
        {
            int cases = 0;
            var differences = new List<string>();
            foreach (string line in File.ReadLines(Witness))
            {
                var row = JObject.Parse(line);
                if ((int)row["phase"] != phase || (int)row["delay"] != 0 ||
                    (profile == BattleRuntimeProfile.Authority400 && (int)row["source"] >= 400)) continue;
                var world = MakeWorld(row, profile, out LF2Entity source);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                    var observer = new Observer();
                    world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacyBefore = world.Rng.CallCount;
                    if (phase == 0)
                    {
                        source.RunNativeC25State18BrokenWeaponParticles();
                        world.ResolveLateObjectPointStructuralMaterializerForModule().FlushTasks();
                    }
                    else new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    string label = "case " + cases + " prev=" + row["previous"] + " current=" + row["current"] +
                        " source=" + row["source"] + " seed=" + row["seed"] + " composite=" + row["composite"];
                    if (world.Rng.CallCount != legacyBefore) differences.Add(label + " legacy RNG consumed");
                    if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add(label + " native RNG sequence");
                    if (observer.CrtCalls != (int)row["crtCalls"]) differences.Add(label + " CRT calls");
                    CompareChildren(world, row, label, differences);
                    if ((world.FindEntityByRuntimeSlotForQuery((int)row["source"]) != null) != (row["sourceAfter"] is JObject))
                        differences.Add(label + " source lifetime");
                    if (world.LogicReferencePool.AvailableCreateTaskCount != 24) differences.Add(label + " task pool not restored");
                }
                finally { Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + mode + "-" + phase + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(profile == BattleRuntimeProfile.Authority400 ? 750 : 757));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(14)));
        }

        internal static void CompareChildren(SimulationWorld world, JObject row, string label, List<string> differences)
        {
            var actual = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"]
                .Where(e => (int)e["identity"]["objectId"] == 999 || (int)e["identity"]["objectId"] == 777).ToArray();
            if (actual.Length != row["children"].Count()) differences.Add(label + " children=" + actual.Length + " expected " + row["children"].Count());
            foreach (var child in row["children"])
            {
                var expected = child["raw"];
                var found = actual.FirstOrDefault(e => (int)e["slot"] == (int)expected["slot"]);
                if (found == null) { differences.Add(label + " missing slot " + expected["slot"]); continue; }
                foreach (var field in Flatten(expected, ""))
                {
                    if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                    var value = found.SelectToken(field.Key);
                    bool equal = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float
                        ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value
                        : JToken.DeepEquals(value, field.Value);
                    if (!equal) differences.Add(label + " slot " + expected["slot"] + " " + field.Key + "=" + value + " expected " + field.Value);
                }
                var entity = world.FindEntityByRuntimeSlotForQuery((int)expected["slot"]);
                if (entity.Runtime.NativeSoundActionLatch != (int)child["soundLatch"]) differences.Add(label + " sound latch");
            }
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity source)
        {
            int I(string key) => (int)row[key];
            bool composite = I("composite") != 0;
            int oid = composite ? 151 : 888;
            string text = "<bmp_begin>\nname: Parent weapon_hp: 17\n<bmp_end>\n<frame> 0 current\nstate: " + I("current") +
                " wait: " + (composite ? 0 : 100) + " next: 0\n";
            if (composite) text += "opoint:\nkind: 1 oid: 777 action: 0\nopoint_end:\n";
            text += "<frame_end>\n<frame> 1 previous\nstate: " + I("previous") + " wait: 100 next: 1\n<frame_end>\n";
            if (I("declared999") != 0) text += "<frame> 999 declared_terminal\nstate: " + I("current") + " wait: 100 next: 0\n<frame_end>\n";
            if (composite) text += "<weapon_piece>\nteam: 1\npiece: 1\namount: 1 oid: 777 act: 0 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n<weapon_piece_end>\n";
            const string childText = "<bmp_begin>\nname: Particle weapon_hp: 17\n<bmp_end>\n<stats> ohp: 25 omp: 50 max_mp: 700 <stats_end>\n" +
                "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n<frame> 140 particle\nstate: 0 wait: 100 next: 140\n<frame_end>\n";
            var parent = Wrapper(oid, 1, text);
            var child = Wrapper(999, I("type"), childText);
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper> { [oid] = parent };
            if (I("present") != 0) wrappers[999] = child;
            if (composite) wrappers[777] = Wrapper(777, 3, childText);
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(p => new ObjectDefinition(p.Key, p.Value.characterData.type_sub, "state18.dat")).ToArray(), id => wrappers[id]);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(24);
            source = new LF2Weapon { ObjectId = oid };
            source.FrameCache.Load(parent);
            source.WriteCurrentFrameId(I("action"));
            source.Frame.D = source.FrameCache.GetNativeFrameDataById(I("action"));
            source.Frame.PN = 0; source.Frame.Prev = 1;
            source.Trans.SyncDirectFrameData(source.Frame.D?.wait ?? 0, source.Frame.D?.next ?? 0, 0);
            source.SetRequiredRuntimeSlot(I("source")); world.Register(source);
            source.Health.HP = 500; source.Health.HPBound = 500; source.Health.HP3 = 500; source.Health.PP = 500;
            source.Runtime.SetPosition(100.25, -20.5, 200.75);
            source.Runtime.XInt = 100; source.Runtime.YInt = -20; source.Runtime.ZInt = 200;
            source.Runtime.SetVelocity(3.25, -1.25, 0.75); source.Runtime.Dir = "left";
            source.Runtime.WeaponFlightCounter = composite ? -1 : 17;
            source.OwnerEntityIndex = 17; source.RelationTeam = 9;
            source.Runtime.NativeLifecycleResolutionPending = I("pending") != 0;
            source.Runtime.NativeLifecycleCode = I("pending") != 0 ? 1101 : 0;
            if (I("freeSlots") >= 0)
            {
                int remaining = I("freeSlots");
                for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    if (slot == I("source")) continue;
                    if (remaining-- > 0) continue;
                    var blocker = new LF2Weapon { ObjectId = 4444 };
                    blocker.SetWeaponType(4); blocker.FrameCache.Load(child);
                    blocker.Frame.D = blocker.FrameCache.GetNativeFrameDataById(0);
                    blocker.Trans.SyncDirectFrameData(100, 0, 0);
                    blocker.Runtime.WeaponFlightCounter = 17;
                    blocker.SetRequiredRuntimeSlot(slot); world.Register(blocker);
                }
            }
            return world;
        }

        private static LF2CharacterDataWrapper Wrapper(int oid, int type, string text)
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(text);
            var bmp = LoganDefinitionMetadata.CopyFields(dat.Bmp.Properties);
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = bmp.Int32OrDefault("weapon_hp", 0),
                NativeMetadata = new LoganDefinitionMetadata(bmp, LoganDefinitionMetadata.CopyFields(dat.LoganStats?.Properties), null,
                    dat.LoganWeaponPiece == null ? null : new LoganWeaponPieceDefinition(dat.LoganWeaponPiece)),
            };
            foreach (var frame in dat.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            return new LF2CharacterDataWrapper(oid, data);
        }

        private static IEnumerable<KeyValuePair<string, JToken>> Flatten(JToken token, string prefix)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                    foreach (var value in Flatten(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name)) yield return value;
            }
            else yield return new KeyValuePair<string, JToken>(prefix, token);
        }

        internal static void Shutdown(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        internal sealed class Observer : INTSD28NativeRandomCallObserver
        {
            internal readonly List<long[]> Calls = new List<long[]>();
            internal int CrtCalls;
            public void OnCrtNext(NTSD28NativeCrtCall call) { CrtCalls++; }
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                Calls.Add(new[] { (long)call.CallSite, call.UpperBound, call.Result, call.CounterAfter, call.IndexAfter, (long)call.TotalCalls });
            }
        }
    }
}
#endif
