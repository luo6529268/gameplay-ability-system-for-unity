#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06PlatformMixedCandidateEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001/";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void MixedCandidateOrderMatchesSource(int index)
        {
            string source = Root + "source-final/first.jsonl";
            using (var stream = File.OpenRead(source))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""),
                    Is.EqualTo("62D0258D7DE8061625D0BE85F764BCF58CA878062F1F94E5C70C25F627DEB77E"));
            var row = JObject.Parse(File.ReadAllLines(source)[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                var platformData = Definition(31980, 3, (string)row["platformDat"]);
                var riderData = Definition(31981, 0, (string)row["riderDat"]);
                var attackData = Definition(31982, 0, (string)row["attackDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(31980, 3, "platform.dat"),
                    new ObjectDefinition(31981, 0, "rider.dat"),
                    new ObjectDefinition(31982, 0, "attack.dat")
                }, id => id == 31980 ? platformData : id == 31981 ? riderData : id == 31982 ? attackData : null);
                var platform = Spawn(31980, 20, 100, -20, 1);
                var rider = Spawn(31981, 21, 100, -10, 1);
                var attacker = Spawn(31982, (int)row["attackSlot"], 120, 0, 2);
                platform.Runtime.NativePreviousY104 = 0;
                rider.Runtime.NativePreviousY104 = -10;
                Assert.That(rider.Runtime.YInt, Is.EqualTo(-10));
                Assert.That(rider.Runtime.Y, Is.EqualTo(-10.0));
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
                int rangeCount = world.SceneQuery.TryGetCollisionCandidateRange(attacker, out var range) ? range.Count : 0;
                var actual = new JObject
                {
                    ["candidateCount"] = attacker.Runtime.HitCandidateCount,
                    ["rangeCount"] = rangeCount,
                    ["targetY"] = rider.Runtime.YInt,
                    ["targetPreciseY"] = rider.Runtime.Y,
                    ["reference"] = rider.Runtime.CollisionYReference,
                    ["platformSlot"] = rider.Runtime.PlatformSourceSlotF4,
                    ["attackerY"] = attacker.Runtime.YInt
                };
                File.WriteAllText(Root + "unity-" + index + ".json", actual.ToString());
                foreach (string key in new[] { "candidateCount", "targetY", "targetPreciseY", "reference", "platformSlot" })
                    Assert.That((double)actual[key], Is.EqualTo((double)row[key]), key);
                Assert.That(rangeCount, Is.EqualTo((int)row["candidateCount"]));
                Assert.That(attacker.Runtime.YInt, Is.Zero, "Attacker stays outside platform");

                LF2Entity Spawn(int oid, int slot, int x, int y, int group)
                {
                    var entity = world.LogicEntityFactory.Create(new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right",
                        preserveActionZero = true, relationTeam = group,
                        opoint = new ObjectPoint { oid = oid, action = 0 }
                    }, out var failure);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    entity.Runtime.SetPosition(x, y, 250);
                    entity.Runtime.SyncIntegerPosition();
                    entity.Runtime.RelationTeam = group;
                    return entity;
                }
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var failure), Is.True, failure);
            }
        }

        private static LF2CharacterDataWrapper Definition(int oid, int type, string dat)
        {
            string root = Path.GetFullPath(Root + "fixture-runtime");
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root));
            data.type_sub = type;
            return new LF2CharacterDataWrapper(oid, data);
        }
    }
}
#endif
