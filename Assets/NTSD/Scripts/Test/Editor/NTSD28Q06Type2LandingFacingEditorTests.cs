#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06Type2LandingFacingEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-TYPE2-LANDING-FACING-SOURCE-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-TYPE2-LANDING-FACING-FIXTURE-001/";

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        [TestCase(false, 1)]
        [TestCase(true, 1)]
        public void BoundType2MatchesNativeLandingAndLate(bool sharedShell, int phase)
        {
            int cases = 0;
            var differences = new List<string>();
            foreach (string line in File.ReadLines(Witness))
            {
                var row = JObject.Parse(line);
                if ((int)row["phase"] != phase) continue;
                var data = new LF2CharacterData { weapon_hp = 32, weapon_drop_hurt = 4 };
                data.frames.Add(new LF2FrameData { frameId = 0, state = (int)row["state"], wait = 100, next = 0, centerx = 39, centery = 79 });
                data.frames.Add(new LF2FrameData { frameId = 20, state = 2004, wait = 100, next = 20, centerx = 39, centery = 79 });
                var wrapper = new LF2CharacterDataWrapper(742, data);
                var world = new SimulationWorld();
                world.SetLogicOnlyEntityMaterialization(true);
                world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(742, 2, "type2-facing.dat") }, _ => wrapper);
                LF2Entity entity = sharedShell ? new LF2OtherObject() : new LF2Weapon();
                if (entity is LF2Weapon weapon) weapon.SetWeaponType(2);
                try
                {
                    entity.ObjectId = 742;
                    entity.FrameCache.Load(wrapper);
                    entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(0);
                    entity.Trans.SyncDirectFrameData(100, 0, 0);
                    entity.SetRequiredRuntimeSlot(70);
                    world.Register(entity);
                    entity.Health.HP = 500;
                    entity.Runtime.WeaponFlightCounter = 20;
                    entity.Runtime.CollisionYReference = (int)row["reference"];
                    entity.Runtime.SetPosition(0, (double)row["y"], 0);
                    entity.Runtime.SyncIntegerPosition();
                    entity.Runtime.SetVelocity((double)row["vx"], (double)row["vy"], 0);
                    entity.Runtime.Dir = (int)row["facing"] == 0 ? "right" : "left";
                    world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                    if (phase == 1) world.LateEntityUpdateAll(1);
                    var expected = row["entity"];
                    if (entity.Frame.N != (int)expected["frame"]["action"] ||
                        entity.Runtime.IsFacingLeft != (bool)expected["frame"]["facingLeft"] ||
                        entity.Runtime.WeaponFlightCounter != (int)expected["combat"]["weaponHp"] ||
                        entity.Runtime.YInt != (int)expected["position"]["y"] ||
                        entity.Runtime.X != (double)expected["position"]["preciseX"] ||
                        entity.Runtime.Y != (double)expected["position"]["preciseY"] ||
                        entity.Runtime.Vx != (double)expected["motion"]["x"] ||
                        entity.Runtime.Vy != (double)expected["motion"]["y"])
                        differences.Add("case " + cases + " input=" + row["state"] + "/" + row["facing"] + "/" + row["vx"] + "/" + row["vy"] +
                            "/" + row["y"] + "/" + row["reference"] + " actual=" + entity.Frame.N + "/" + entity.Runtime.Dir + "/" +
                            entity.Runtime.Y + "/" + entity.Runtime.Vx + "/" + entity.Runtime.Vy + "/" + entity.Runtime.WeaponFlightCounter);
                }
                finally
                {
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                cases++;
            }
            File.WriteAllText(Output + sharedShell + "-" + phase + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(648));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(8)));
        }
    }
}
#endif
