#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07ModeComboPublishedActivationEditorTests
    {
        private const string FormalRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void VersionedIdentityMatchesIndependentBytesAndAbsentV1Vector()
        {
            var components = new string[5];
            for (int component = 0; component < 5; component++)
            {
                var bytes = new byte[32];
                for (int index = 0; index < 32; index++) bytes[index] = (byte)(component * 32 + index);
                components[component] = BitConverter.ToString(bytes).Replace("-", "");
            }
            var absent = LoganContentIdentity.FromBattleComponents(components[0], components[1], components[2]);
            Assert.That(absent.RawDefinitionFingerprint, Is.EqualTo("C32F2109ABE09F390E5D13936B32E174AD704D64B1B56A9516D573BCF3BE55EF"));
            Assert.That(absent.SemanticFingerprint, Is.EqualTo("68EE16B9C9BCAB61B44C040C057C7686E642662F25DA2B0999FD7359729D0111"));
            var present = LoganContentIdentity.FromBattleComponents(components[0], components[1], components[2], components[3], components[4]);
            byte[] tag = Encoding.ASCII.GetBytes("NTSD28_LOGAN_BATTLE_INPUTS_V2\0");
            byte[] preimage = new byte[tag.Length + 160];
            Array.Copy(tag, preimage, tag.Length);
            for (int index = 0; index < 160; index++) preimage[tag.Length + index] = (byte)index;
            using (var sha = SHA256.Create())
                Assert.That(present.RawDefinitionFingerprint, Is.EqualTo(BitConverter.ToString(sha.ComputeHash(preimage)).Replace("-", "")));
            Assert.That(present.BattleInputContractTag, Is.EqualTo("NTSD28_LOGAN_BATTLE_INPUTS_V2"));
            Assert.That(present.ModeInputFingerprint, Is.EqualTo(components[3]));
            Assert.Throws<ArgumentException>(() => LoganContentIdentity.FromBattleComponents(components[0], components[1], components[2], components[3], null));
        }

        [Test]
        public void FormalAndStagedCatalogsCaptureSameModeAndV2Identity()
        {
            var projectMode = NTSD.App.ProjectBattleModeConfig.LoadDefault().Capture();
            var formal = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot), projectMode);
            var staged = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(Path.GetFullPath("Assets/NTSD/Content/LoganRuntime")), projectMode);
            Assert.That(staged.ModeComboInput, Is.Not.Null);
            Assert.That(staged.ModeComboInput.Bound, Is.EqualTo(1));
            Assert.That(staged.ModeComboInput.Facing, Is.EqualTo(1));
            Assert.That(staged.ModeComboInput.Respond, Is.EqualTo(50));
            Assert.That(staged.ModeComboInput.CaughtAct, Is.EqualTo(1));
            Assert.That(staged.ModeComboInput.SelectedChildVirtualPath,
                Is.EqualTo("unity:ProjectBattleModeConfig"));
            Assert.That(staged.ContentIdentity.BattleInputContractTag, Is.EqualTo("NTSD28_LOGAN_BATTLE_INPUTS_V3"));
            Assert.That(staged.ContentIdentity.SemanticFingerprint, Is.EqualTo(formal.ContentIdentity.SemanticFingerprint));
        }

        [Test]
        public void ModeAppearanceRemovalAndSameTupleByteChangesInvalidateCandidate()
        {
            string root = CreateRuntime();
            var source = BattleContentSource.ForLoganRuntime(root);
            var absent = LoganVisualContentCandidate.Capture(source);
            WriteMode(root);
            var present = LoganVisualContentCandidate.Capture(source);
            Assert.Throws<InvalidDataException>(() => absent.AssertInputsCurrent());
            Assert.That(present.SourceCacheKey, Is.Not.EqualTo(absent.SourceCacheKey));
            foreach (string path in new[] { "data/mode.dat", "data/mode/test.dat" })
            {
                File.AppendAllText(Path.Combine(root, "decoded_dat", path), "\n");
                var changed = LoganVisualContentCandidate.Capture(source);
                Assert.That(changed.ContentIdentity.ModeSemanticFingerprint, Is.EqualTo(present.ContentIdentity.ModeSemanticFingerprint));
                Assert.That(changed.SourceCacheKey, Is.Not.EqualTo(present.SourceCacheKey));
                Assert.Throws<InvalidDataException>(() => present.AssertInputsCurrent());
                present = changed;
            }
            File.Move(Path.Combine(root, "decoded_dat/data/mode.dat"), Path.Combine(root, "mode.dat.removed"));
            Assert.Throws<InvalidDataException>(() => present.AssertInputsCurrent());
            Assert.That(LoganVisualContentCandidate.Capture(source).SourceCacheKey, Is.EqualTo(absent.SourceCacheKey));
        }

        [Test]
        public void PreTickActivationUsesCapturedTupleAndPreservesRepeatedStateAndNewWorld()
        {
            string root = CreateRuntime();
            WriteMode(root);
            var catalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(root));
            var host = new GameObject("ModeComboActivationFixture");
            host.SetActive(false);
            var driver = host.AddComponent<SimulationTickDriver>();
            FieldInfo worldField = typeof(SimulationTickDriver).GetField("_world", PrivateInstance);
            MethodInfo apply = typeof(SimulationTickDriver).GetMethod("ApplyPublishedModeComboBeforeFirstTick", PrivateInstance);
            try
            {
                var world = new SimulationWorld();
                worldField.SetValue(driver, world);
                world.ResetRuntimeState();
                apply.Invoke(driver, new object[] { catalog });
                AssertTuple(world.Runtime.NativeCombo);
                world.Runtime.NativeCombo.Respond = 77;
                apply.Invoke(driver, new object[] { catalog });
                Assert.That(world.Runtime.NativeCombo.Respond, Is.EqualTo(77), "Repeated preparation must retain restored/live state.");
                var nextWorld = new SimulationWorld();
                worldField.SetValue(driver, nextWorld);
                apply.Invoke(driver, new object[] { catalog });
                AssertTuple(nextWorld.Runtime.NativeCombo);
            }
            finally
            {
                worldField.SetValue(driver, null);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [UnityTest]
        public IEnumerator PublishedModeFlowsThroughDirectSealMatchResetAndTickZeroRestore()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var publication = new NTSD28B11AtomicPublicationEditorTests();
                IDisposable driverScope = null;
                SimulationTickDriver driver = null;
                CharacterAnimtorManager manager = null;
                const BindingFlags staticPrivate = BindingFlags.Static | BindingFlags.NonPublic;
                FieldInfo managerInstance = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", staticPrivate);
                FieldInfo dataInstance = typeof(MoreMountains.Tools.MMSingleton<GameDataManager>)
                    .GetField("_instance", staticPrivate);
                object previousManager = managerInstance.GetValue(null);
                object previousData = dataInstance.GetValue(null);
                bool initialized = false;
                try
                {
                    publication.SetUp();
                    initialized = true;
                    Type fixtureType = typeof(NTSD28B11AtomicPublicationEditorTests);
                    manager = (CharacterAnimtorManager)fixtureType.GetField("manager", PrivateInstance).GetValue(publication);
                    var data = (GameDataManager)fixtureType.GetField("data", PrivateInstance).GetValue(publication);
                    var ui = (NTSD.UI.CharacterUIResourceManager)fixtureType.GetField("ui", PrivateInstance).GetValue(publication);
                    managerInstance.SetValue(null, manager);
                    dataInstance.SetValue(null, data);

                    string root = CreateRuntime();
                    WriteMode(root);
                    var candidate = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
                    var load = (UniTask<bool>)fixtureType.GetMethod("Load", PrivateInstance)
                        .Invoke(publication, new object[] { candidate, null });
                    Assert.That(await load, Is.True, "The production atomic publication must finish before host preparation.");
                    Assert.That(manager.PublishedLoganCatalog, Is.SameAs(candidate.Catalog));
                    Assert.That(manager.PublishedLoganContentIdentity, Is.SameAs(data.PublishedLoganContentIdentity));
                    Assert.That(ui.PublishedVisualContentKey, Is.EqualTo(candidate.SourceCacheKey));

                    Type scopeType = typeof(BattleSimulationWorkerBoundaryEditorTests)
                        .GetNestedType("DriverScope", BindingFlags.NonPublic);
                    driverScope = (IDisposable)Activator.CreateInstance(scopeType, true);
                    driver = (SimulationTickDriver)scopeType.GetProperty("Driver").GetValue(driverScope);
                    driver.ApplySettings(new LockstepSimulationSettings
                    {
                        driveMode = SimulationDriveMode.Manual,
                        requireInputFrameReady = false,
                    });
                    driver.BeginBattleAllocationSeal();
                    AssertTuple(driver.World.Runtime.NativeCombo);
                    Assert.That(driver.World.RuntimeDataCatalog.LoganContentIdentity, Is.SameAs(candidate.ContentIdentity));
                    Assert.That(driver.CurrentTickIndex, Is.Zero);
                    driver.World.Runtime.NativeCombo.Respond = 77;
                    driver.BeginBattleAllocationSeal();
                    Assert.That(driver.World.Runtime.NativeCombo.Respond, Is.EqualTo(77));

                    driver.ApplyMatchConfig(null);
                    Assert.That(driver.World.Runtime.NativeCombo.RecordPresent, Is.False, "Match preparation must reset the carrier.");
                    driver.World.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 0, 31, 1);
                    var identity = candidate.ContentIdentity.CreateLocalValidationSessionIdentity(1, 0, 1, new[] { 0 });
                    var snapshot = driver.World.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(driver.World.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                    driver.World.Runtime.NativeCombo.Reset();
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    driver.BeginBattleAllocationSeal();
                    Assert.That(driver.World.Runtime.NativeCombo.Facing, Is.Zero);
                    Assert.That(driver.World.Runtime.NativeCombo.Respond, Is.EqualTo(31), "An unsealed tick-zero restore must survive its first seal.");

                    driver.ApplyMatchConfig(null);
                    driver.BeginBattleAllocationSeal();
                    AssertTuple(driver.World.Runtime.NativeCombo);
                    Assert.That(driver.CurrentTickIndex, Is.Zero);
                }
                finally
                {
                    try
                    {
                        driver?.EndBattleAllocationSeal();
                        driverScope?.Dispose();
                        if (manager != null && !(bool)typeof(CharacterAnimtorManager)
                            .GetField("spritePrewarmDisposed", PrivateInstance).GetValue(manager))
                            typeof(CharacterAnimtorManager).GetMethod("OnDestroy", PrivateInstance).Invoke(manager, null);
                    }
                    finally
                    {
                        try { if (initialized) publication.TearDown(); }
                        finally
                        {
                            managerInstance.SetValue(null, previousManager);
                            dataInstance.SetValue(null, previousData);
                        }
                    }
                }
            });
        }

        private static void AssertTuple(NTSD28NativeComboRuntimeState state)
        {
            Assert.That(state.RecordPresent, Is.True);
            Assert.That(new[] { state.Bound, state.Facing, state.Respond, state.CaughtAct }, Is.EqualTo(new[] { 1, 1, 50, 1 }));
        }

        private static string CreateRuntime()
        {
            string root = Path.GetFullPath("Temp/Q07ModeCombo/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data/mode"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"), "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\n");
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin>\nname: mode\nhead: c/head.png\nsmall: c/small.png\n" +
                "file(20-19): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n" +
                "<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");
            var texture = new Texture2D(6, 2, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[12];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32(0, 255, 0, 255);
                texture.SetPixels32(pixels);
                texture.Apply();
                byte[] png = texture.EncodeToPNG();
                foreach (string name in new[] { "body", "head", "small" })
                    File.WriteAllBytes(Path.Combine(root, "vfs/c", name + ".png"), png);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
            return root;
        }

        private static void WriteMode(string root)
        {
            File.WriteAllText(Path.Combine(root, "decoded_dat/data/mode.dat"), "<mode_information>\nfile: data/mode/test.dat\n<mode_information_end>\n");
            File.WriteAllText(Path.Combine(root, "decoded_dat/data/mode/test.dat"), "<combo>\nbound: 1 respond: 50 offset_x: 0 offset_y: 0 facing: 1 times: 1 state: 0 caughtact: 1 effect: 0 w: 1 h: 1 pic: combo.png name: Combo\n<combo_end>\n");
        }
    }
}
#endif
