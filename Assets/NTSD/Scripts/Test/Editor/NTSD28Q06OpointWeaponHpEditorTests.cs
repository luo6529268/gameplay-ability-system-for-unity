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
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointWeaponHpEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001/";

        [TestCase(false)]
        [TestCase(true)]
        public void BothPostInitCallersMatchOriginalWeaponHp(bool presentation)
        {
            var differences = new List<string>();
            int cases = 0;
            GameObject host = null;
            LF2ObjectPointFactory factory = null;
            var method = typeof(LF2ObjectPointFactory).GetMethod("PostInitLiving", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                if (presentation)
                {
                    host = new GameObject("WeaponHpPostInitFixture") { hideFlags = HideFlags.HideAndDontSave };
                    host.SetActive(false);
                    factory = host.AddComponent<LF2ObjectPointFactory>();
                }
                foreach (string line in File.ReadLines(Output + "native.jsonl"))
                {
                    var row = JObject.Parse(line);
                    int type = (int)row["type"];
                    var child = NewEntity(type);
                    child.FrameCache.Load(Wrapper(type, (bool)row["weaponHpPresent"], (int)row["weaponHp"]));
                    child.Runtime.WeaponFlightCounter = 12345;
                    var parent = new LF2Character();
                    var point = new ObjectPoint { oid = 777, kind = (int)row["kind"], hp = (int)row["pointHp"], action = 0 };
                    if (presentation) method.Invoke(factory, new object[] { child, parent, point, type, 0f, false });
                    else BattleLogicEntityFactory.PostInitLiving(child, parent, point, type, 0f, false);
                    int expected = (int)row["raw"]["combat"]["weaponHp"];
                    if (child.Runtime.WeaponFlightCounter != expected)
                        differences.Add(cases + " type=" + type + " kind=" + point.kind + " weaponHp=" + child.Runtime.WeaponFlightCounter + " expected " + expected);
                    if (child.Health.HP != (int)row["raw"]["vitals"]["currentHp"])
                        differences.Add(cases + " HP changed independently of native vitals");
                    if (child.Runtime.LinkState != (int)row["childLink"] || parent.Runtime.LinkState != (int)row["parentLink"])
                        differences.Add(cases + " kind2 link mismatch");
                    cases++;
                }
            }
            finally { if (host != null) UnityEngine.Object.DestroyImmediate(host); }
            File.WriteAllText(Output + "post-init-" + presentation + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(210));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(8)));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void LogicFactoryReinitializesWeaponHpAcrossPoolReuse(int type)
        {
            var wrapper = Wrapper(type, true, 17);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(_ => wrapper));
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(777, type, "weapon-hp.dat") }, _ => wrapper);
            try
            {
                for (int birth = 0; birth < 2; birth++)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, dir = "right", preserveActionZero = true,
                        opoint = new ObjectPoint { oid = 777, kind = 1, action = 0, hp = 41 }
                    };
                    var child = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(child, Is.Not.Null, failure.ToString());
                    Assert.That(child.Runtime.WeaponFlightCounter, Is.EqualTo(17));
                    child.Runtime.WeaponFlightCounter = -999;
                    child.FreeEntityLikeExe();
                    Assert.That(world.ObjectCount, Is.Zero);
                }
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void ManualWrapperWithoutNativeMetadataKeepsItsWeaponHp()
        {
            var child = new LF2SpecialAttack();
            child.FrameCache.Load(new LF2CharacterDataWrapper(777, new LF2CharacterData { type_sub = 3, weapon_hp = -9 }));
            child.Runtime.WeaponFlightCounter = 12345;
            BattleSpawnVitalsWriter.Apply(child, new ObjectPoint { oid = 777, kind = 1, hp = 41 });
            Assert.That(child.Runtime.WeaponFlightCounter, Is.EqualTo(-9));
        }

        private static LF2Entity NewEntity(int type)
        {
            LF2Entity entity = type == 0 ? new LF2Character() : type == 3 ? new LF2SpecialAttack() : type == 5 ? new LF2OtherObject() : new LF2Weapon();
            if (entity is LF2Weapon weapon) weapon.SetWeaponType(type);
            entity.ObjectId = 777;
            return entity;
        }

        internal static LF2CharacterDataWrapper Wrapper(int type, bool present, int weaponHp)
        {
            var fields = new List<KeyValuePair<string, string>>();
            if (present) fields.Add(new KeyValuePair<string, string>("weapon_hp", weaponHp.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = 333,
                NativeMetadata = new LoganDefinitionMetadata(new LoganDefinitionFieldSet(fields), new LoganDefinitionFieldSet(Array.Empty<KeyValuePair<string, string>>()))
            };
            data.frames.Add(new LF2FrameData { frameId = 0, wait = 100, next = 0 });
            return new LF2CharacterDataWrapper(777, data);
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06OpointWeaponHpPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_OpointWeaponHpPlay.request";

        static NTSD28Q06OpointWeaponHpPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            string status = "FAIL", error = null;
            int cases = 0;
            try
            {
                foreach (bool logicOnly in new[] { true, false })
                foreach (string line in File.ReadLines("artifacts/diagnostics/NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001/native.jsonl"))
                {
                    var row = JObject.Parse(line);
                    if (!(bool)row["weaponHpPresent"] || (int)row["weaponHp"] != 17) continue;
                    int type = (int)row["type"];
                    var wrapper = NTSD28Q06OpointWeaponHpEditorTests.Wrapper(type, true, 17);
                    var world = new SimulationWorld(new RuntimeCharacterConfigResolver(_ => wrapper));
                    world.SetLogicOnlyEntityMaterialization(logicOnly);
                    world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(777, type, "weapon-hp.dat") }, _ => wrapper);
                    try
                    {
                        var parent = new LF2Character { ObjectId = 888 };
                        parent.FrameCache.Load(NTSD28Q06OpointWeaponHpEditorTests.Wrapper(0, true, 17));
                        parent.Frame.D = parent.FrameCache.GetNativeFrameDataById(0);
                        parent.Trans.SyncDirectFrameData(100, 0, 0);
                        parent.Health.HP = 500;
                        parent.Health.PP = 500;
                        parent.SetRequiredRuntimeSlot(20); world.Register(parent);
                        var task = new OPointCreateTask
                        {
                            targetWorld = world, parent = parent, requiredRuntimeSlot = 50, dir = "right", preserveActionZero = true,
                            opoint = new ObjectPoint { oid = 777, kind = (int)row["kind"], hp = (int)row["pointHp"], action = 0 }
                        };
                        var child = logicOnly ? world.LogicEntityFactory.Create(task, out _) : LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
                        Assert.That(child, Is.Not.Null, "spawn " + cases);
                        Assert.That(child.Runtime.WeaponFlightCounter, Is.EqualTo((int)row["raw"]["combat"]["weaponHp"]), "weapon HP " + cases);
                        Assert.That(child.Health.HP, Is.EqualTo((int)row["raw"]["vitals"]["currentHp"]), "HP " + cases);
                        Assert.That(child.Runtime.LinkState, Is.EqualTo((int)row["childLink"]));
                        Assert.That(child.Renderer != null, Is.EqualTo(!logicOnly));
                        cases++;
                    }
                    finally
                    {
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                        NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                        Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                    }
                }
                Assert.That(cases, Is.EqualTo(84));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_OpointWeaponHpPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, both actual factories, synthetic DAT weapon_hp and kind2 override only; image and full OPoint field parity not certified."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
