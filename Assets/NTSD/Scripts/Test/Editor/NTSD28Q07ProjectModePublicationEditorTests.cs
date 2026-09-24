using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07ProjectModePublicationEditorTests
    {
        [Test]
        public void ProjectSnapshot_ReplacesMissingNativeModeDatOnWorkerAndVersionsIdentity()
        {
            ProjectBattleModeConfig asset = ProjectBattleModeConfig.LoadDefault();
            ProjectBattleModeConfig.Snapshot first = asset.Capture();
            ProjectBattleModeConfig clone = UnityEngine.Object.Instantiate(asset);
            ProjectBattleModeConfig.Snapshot second;
            try
            {
                clone.Combo.respond++;
                second = clone.Capture();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clone);
            }

            string root = Path.GetFullPath("Temp/Q07ProjectModePublication/" +
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\n" +
                "object,0,56,0,a.dat,missing\n");
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin> name: test file(0-0): c/test.png w: 1 h: 1 row: 1 col: 1 <bmp_end> " +
                "<frame> 0 standing pic: 0 state: 0 wait: 1 next: 0 <frame_end>");

            BattleContentSource source = BattleContentSource.ForLoganRuntime(root);
            LoganObjectCatalog catalog = Task.Run(() =>
                LoganObjectCatalog.Read(source, first)).GetAwaiter().GetResult();
            Assert.That(File.Exists(Path.Combine(root, "decoded_dat/data/mode.dat")), Is.False);
            Assert.That(catalog.ProjectModeSnapshot.Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(catalog.ModeComboInput.SelectedChildVirtualPath,
                Is.EqualTo("unity:ProjectBattleModeConfig"));
            Assert.That(catalog.ModeComboInput.Respond, Is.EqualTo(50));
            Assert.That(catalog.ModeComboInput.KnockoutFeed.LifetimeTicks, Is.EqualTo(70));
            Assert.That(catalog.ModeComboInput.KnockoutFeed.TypeResourcePath(0),
                Is.EqualTo("sprite/kill/c.png"));
            Assert.That(catalog.ModeComboInput.KnockoutFeed.StageTeam5DeathSoundPath,
                Is.EqualTo("data/m_join.wav"));

            LoganObjectCatalog changed = Task.Run(() =>
                LoganObjectCatalog.Read(source, second)).GetAwaiter().GetResult();
            Assert.That(changed.ModeComboInput.Respond, Is.EqualTo(51));
            Assert.That(changed.ContentIdentity.SemanticFingerprint,
                Is.Not.EqualTo(catalog.ContentIdentity.SemanticFingerprint));

            var host = new GameObject("ProjectModeFirstTickFixture");
            host.SetActive(false);
            var driver = host.AddComponent<SimulationTickDriver>();
            FieldInfo worldField = typeof(SimulationTickDriver).GetField("_world",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo apply = typeof(SimulationTickDriver).GetMethod(
                "ApplyPublishedModeComboBeforeFirstTick",
                BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                var world = new SimulationWorld();
                worldField.SetValue(driver, world);
                apply.Invoke(driver, new object[] { catalog });
                Assert.That(world.Runtime.NativeCombo.RecordPresent, Is.True);
                Assert.That(world.Runtime.NativeCombo.Bound, Is.EqualTo(1));
                Assert.That(world.Runtime.NativeCombo.Facing, Is.EqualTo(1));
                Assert.That(world.Runtime.NativeCombo.Respond, Is.EqualTo(50));
                Assert.That(world.Runtime.NativeCombo.CaughtAct, Is.EqualTo(1));
            }
            finally
            {
                worldField.SetValue(driver, null);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }
    }
}
