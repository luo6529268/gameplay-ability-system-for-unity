#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_Q09")]
    public sealed class NTSD28Q09KillIconPublicationEditorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator FormalSelectedIconsPublishSevenAliasesAndRetireWithCatalog()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var publication = new NTSD28B11AtomicPublicationEditorTests();
                Type managerType = typeof(CharacterAnimtorManager);
                var managerSingleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", PrivateStatic);
                var dataSingleton = typeof(MoreMountains.Tools.MMSingleton<GameDataManager>)
                    .GetField("_instance", PrivateStatic);
                object previousManager = managerSingleton.GetValue(null);
                object previousData = dataSingleton.GetValue(null);
                CharacterAnimtorManager manager = null;
                BattleSpriteCatalog heldCatalog = null;
                bool initialized = false;
                bool held = false;
                try
                {
                    publication.SetUp();
                    initialized = true;
                    Type fixtureType = typeof(NTSD28B11AtomicPublicationEditorTests);
                    manager = (CharacterAnimtorManager)fixtureType
                        .GetField("manager", PrivateInstance).GetValue(publication);
                    var data = (GameDataManager)fixtureType
                        .GetField("data", PrivateInstance).GetValue(publication);
                    managerSingleton.SetValue(null, manager);
                    dataSingleton.SetValue(null, data);

                    string root = CreateFixtureWithFormalIcons();
                    ProjectBattleModeConfig.Snapshot modeSnapshot =
                        ProjectBattleModeConfig.LoadDefault().Capture();
                    var candidate = LoganVisualContentCandidate.Capture(
                        BattleContentSource.ForLoganRuntime(root), modeSnapshot);
                    Assert.That(candidate.KillIconInput, Is.Not.Null);
                    var load = (UniTask<bool>)fixtureType.GetMethod("Load", PrivateInstance)
                        .Invoke(publication, new object[] { candidate, null });
                    Assert.That(await load, Is.True);

                    heldCatalog = manager.SpriteCatalog;
                    var unique = new HashSet<Sprite>();
                    for (int type = 0; type < BattleSpriteCatalog.NativeKillIconTypeCount; type++)
                    {
                        Assert.That(heldCatalog.TryGetNativeKillIcon(type, out Sprite icon), Is.True);
                        Assert.That(icon.rect.width, Is.EqualTo(40));
                        unique.Add(icon);
                    }
                    Assert.That(unique.Count, Is.EqualTo(3));
                    Assert.That(heldCatalog.TryGetNativeKillIcon(0, out Sprite common), Is.True);
                    Assert.That(heldCatalog.TryGetNativeKillIcon(3, out Sprite special), Is.True);
                    Assert.That(heldCatalog.TryGetNativeKillIcon(1, out Sprite shared), Is.True);
                    Assert.That(heldCatalog.TryGetNativeKillIcon(2, out Sprite alias), Is.True);
                    Assert.That(alias, Is.SameAs(shared));
                    Assert.That(common, Is.Not.SameAs(shared));
                    Assert.That(special, Is.Not.SameAs(shared));
                    Assert.That(common.rect.height, Is.EqualTo(44));
                    Assert.That(special.rect.height, Is.EqualTo(45));
                    Assert.That(shared.rect.height, Is.EqualTo(40));
                    Assert.That(heldCatalog.TryGetNativeKillIcon(7, out _), Is.False);

                    File.WriteAllBytes(Path.Combine(root, "vfs/sprite/kill/c.png"),
                        new byte[] { 1, 2, 3, 4 });
                    var malformed = LoganVisualContentCandidate.Capture(
                        BattleContentSource.ForLoganRuntime(root), modeSnapshot);
                    LogAssert.Expect(LogType.Error, new Regex(@"\[BMPLoader\]"));
                    try
                    {
                        var failedLoad = (UniTask<bool>)fixtureType.GetMethod("Load", PrivateInstance)
                            .Invoke(publication, new object[] { malformed, null });
                        await failedLoad;
                        Assert.Fail("A malformed selected PNG must fail before publication.");
                    }
                    catch (InvalidDataException)
                    {
                    }
                    Assert.That(manager.SpriteCatalog, Is.SameAs(heldCatalog));
                    Assert.That(common == null, Is.False);

                    manager.RegisterRendererCatalogBinding(heldCatalog);
                    held = true;
                    managerType.GetMethod("InvalidateSpriteCatalog", PrivateInstance)
                        .Invoke(manager, null);
                    Assert.That(common == null, Is.False,
                        "An old frame's catalog lease must retain icon Sprite resources.");
                    manager.UnregisterRendererCatalogBinding(heldCatalog);
                    held = false;
                    Assert.That(common == null, Is.True,
                        "The last old-generation catalog unbind must release its icon Sprite.");
                }
                finally
                {
                    try
                    {
                        if (held && manager != null)
                            manager.UnregisterRendererCatalogBinding(heldCatalog);
                        if (manager != null && !(bool)managerType
                            .GetField("spritePrewarmDisposed", PrivateInstance).GetValue(manager))
                            managerType.GetMethod("OnDestroy", PrivateInstance)
                                .Invoke(manager, null);
                    }
                    finally
                    {
                        try { if (initialized) publication.TearDown(); }
                        finally
                        {
                            managerSingleton.SetValue(null, previousManager);
                            dataSingleton.SetValue(null, previousData);
                        }
                    }
                }
            });
        }

        private static string CreateFixtureWithFormalIcons()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string formalRoot = Path.Combine(projectRoot, "Assets/NTSD/Content/LoganRuntime");
            string root = (string)typeof(NTSD28Q09WordsPublishedCatalogEditorTests)
                .GetMethod("CreateFixtureWithFormalWords", PrivateStatic)
                .Invoke(null, null);
            File.Copy(Path.Combine(formalRoot, "decoded_dat/data/system.dat"),
                Path.Combine(root, "decoded_dat/data/system.dat"));
            File.Copy(Path.Combine(formalRoot, "vfs/sprite/UI/SPARK.png"),
                Path.Combine(root, "vfs/sprite/UI/SPARK.png"));
            string iconFolder = Path.Combine(root, "vfs/sprite/kill");
            Directory.CreateDirectory(iconFolder);
            foreach (string name in new[] { "c", "sk1", "sk2" })
                File.Copy(Path.Combine(formalRoot, "vfs/sprite/kill", name + ".png"),
                    Path.Combine(iconFolder, name + ".png"));
            return root;
        }
    }
}
#endif
