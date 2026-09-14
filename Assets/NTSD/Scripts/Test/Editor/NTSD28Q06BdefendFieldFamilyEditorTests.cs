#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed class NTSD28Q06BdefendFieldFamilyEditorTests
    {
        private const string Source = "artifacts/diagnostics/NTSD28-Q06-BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001/first.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();
        private static readonly MethodInfo timer = typeof(BattleLateEntityLifecycleModule).GetMethod(
            "AdvanceNativeReactionAndStatusTail", BindingFlags.Instance | BindingFlags.NonPublic);

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void NativeFieldWritesAndRecoveryMatch(BattleRuntimeProfile profile, bool shadow)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse))
            {
                var definitions = new[] { Definition(row, true), Definition(row, false) };
                var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                world.SetLogicOnlyEntityMaterialization(true);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "bdefend-a.dat"),
                    new ObjectDefinition(78, (int)row["type"], "bdefend-t.dat")
                }, id => definitions[id - 77]);
                try
                {
                    var pair = new LF2Entity[2];
                    for (int slot = 0; slot < 2; slot++)
                    {
                        var task = new OPointCreateTask
                        {
                            targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                            relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                        };
                        pair[slot] = world.LogicEntityFactory.Create(task, out _);
                        Assert.That(pair[slot], Is.Not.Null);
                        pair[slot].Team = slot + 1;
                        pair[slot].Runtime.SetPosition(100 + slot * 10, 0, 200);
                        pair[slot].Runtime.SyncIntegerPosition();
                    }
                    var attacker = pair[0];
                    var target = pair[1];
                    target.Runtime.Bdefend = (int)row["initial"];
                    target.Runtime.HitStateCount = 241;
                    target.Runtime.RuntimeArmorHp118 = (int)row["armorHp"];
                    world.NativeRandom.ResetFromSeed(42);
                    world.CaptureCollisionFrameSnapshotsAll();
                    world.CollectCollisionCandidatesAll();
                    string label = "case " + row["index"];
                    NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, row["before"], label + " before", differences);
                    if (attacker.Runtime.HitCandidateCount != (int)row["candidates"]) differences.Add(label + " candidates differ");
                    if (shadow)
                    {
                        world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                        world.PostInteractionTickAll(1);
                        var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                        if (!plan.CurrentTickPlanValid || plan.ObservedWriterEffectCount != 1 || plan.LastWriterEffectDifferenceMask != 0)
                            differences.Add(label + " Shadow plan/effect differs mask=" + plan.LastWriterEffectDifferenceMask);
                    }
                    else if (!world.DamageWriter.TryApplyCurrentDatTargetHit(world, attacker, target, attacker.Frame.D.itrs[0], default))
                        differences.Add(label + " actual writer rejected");
                    NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, row["after"], label + " after", differences);
                    if (target.Runtime.HitStateCount != 241) differences.Add(label + " legacy HitStateCount overwritten=" + target.Runtime.HitStateCount);
                    int after = target.Runtime.Bdefend;
                    target.FrameDelay = 2;
                    timer.Invoke(new BattleLateEntityLifecycleModule(world), new object[] { target, 1, false });
                    if (target.Runtime.Bdefend != (int)row["afterHeldTimer"]) differences.Add(label + " held timer=" + target.Runtime.Bdefend);
                    target.Runtime.Bdefend = after;
                    target.FrameDelay = 0;
                    timer.Invoke(new BattleLateEntityLifecycleModule(world), new object[] { target, 1, false });
                    if (target.Runtime.Bdefend != (int)row["afterFreeTimer"]) differences.Add(label + " free timer=" + target.Runtime.Bdefend);
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + shadow + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(256));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker)
        {
            int armor = (int)row["armorType"], raw = (int)row["rawBdefend"], type = attacker ? 0 : (int)row["type"];
            string key = attacker + "/" + type + "/" + armor + "/" + raw;
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: BdefendFamily\nweapon_hp: 20\n<bmp_end>\n";
            if (!attacker && armor >= 0) text += "<armor>\ntype: " + armor +
                " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4\n<armor_end>\n";
            text += "<frame> 0 active\nstate: " + (!attacker && armor == 1 ? 4 : 0) + " wait: 100 next: 0\n";
            text += attacker ? "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1 fall: 0 vrest: 1 bdefend: " + raw + "\nitr_end:\n" :
                "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
            text += "<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = 20,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            Lf2DatConverter.ApplyNativeArmorDefinitionData(parsed, data, true);
            var wrapper = new LF2CharacterDataWrapper(attacker ? 77 : 78, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }
    }
}
#endif
