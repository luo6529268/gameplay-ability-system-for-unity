#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    [Category("NTSD28")]
    [Category("NTSD28_Q07")]
    public sealed class NTSD28Q07StagedPublicationEditorTests
    {
        [UnityTest]
        public IEnumerator StagedFormalCandidatePublishesAllOwnersAndRecyclesResources()
        {
            return UniTask.ToCoroutine(async () =>
            {
                string root = Path.GetFullPath(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime"));
                LoganVisualContentCandidate candidate = LoganVisualContentCandidate.Capture(
                    BattleContentSource.ForLoganRuntime(root),
                    ProjectBattleModeConfig.LoadDefault().Capture());
                Assert.That(candidate.Catalog.Entries.Count, Is.EqualTo(330));
                Assert.That(candidate.Images.Count, Is.EqualTo(906));

                var fixture = new NTSD28B11AtomicPublicationEditorTests();
                var tracked = new HashSet<UnityEngine.Object>();
                CharacterAnimtorManager managerRef = null;
                bool initialized = false;
                try
                {
                    fixture.SetUp();
                    initialized = true;
                    const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                    var type = typeof(NTSD28B11AtomicPublicationEditorTests);
                    var manager = (CharacterAnimtorManager)type.GetField("manager", flags).GetValue(fixture);
                    managerRef = manager;
                    Assert.That(manager != null && manager.gameObject != null, Is.True,
                        "The isolated publication manager was destroyed before loading.");
                    var data = (GameDataManager)type.GetField("data", flags).GetValue(fixture);
                    var ui = (NTSD.UI.CharacterUIResourceManager)type.GetField("ui", flags).GetValue(fixture);
                    var load = type.GetMethod("Load", flags);
                    Assert.That(load, Is.Not.Null);
                    Assert.That(await (UniTask<bool>)load.Invoke(fixture, new object[] { candidate, null }), Is.True);

                    string key = candidate.SourceCacheKey;
                    Assert.That(manager.IsPrewarmCompleted, Is.True);
                    Assert.That(manager.PublishedVisualContentKey, Is.EqualTo(key));
                    Assert.That(data.PublishedVisualContentKey, Is.EqualTo(key));
                    Assert.That(ui.PublishedVisualContentKey, Is.EqualTo(key));
                    Assert.That(manager.PublishedLoganContentIdentity.SemanticFingerprint,
                        Is.EqualTo(candidate.ContentIdentity.SemanticFingerprint));
                    BattleCommonVisualCatalog sparkCatalog = manager.CommonVisualCatalog;
                    Assert.That(sparkCatalog.IsNativeSpark, Is.True,
                        "Formal staged publication must use the verified native SPARK PNG.");
                    foreach (int id in new[] { 0, 4, 10, 20, 21, 24, 30, 34 })
                    {
                        Assert.That(sparkCatalog.TryGetSparkForAge(id, out int pic,
                            out BattleCommonVisualBinding binding), Is.True,
                            $"Native SPARK ID {id} was not published.");
                        Assert.That(pic, Is.EqualTo(id / 10 * 5 + id % 10));
                        Assert.That(binding.Texture.width, Is.EqualTo(500));
                        Assert.That(binding.Texture.height, Is.EqualTo(320));
                        Assert.That(binding.PixelRect,
                            Is.EqualTo(BattleCommonVisualCatalog.GetNativeSparkPixelRect(
                                pic, 500, 320, 99, 79)));
                        Assert.That(binding.CentralBinding.IsValid, Is.True);
                    }
                    foreach (int id in new[] { 5, 9, 25, 35, 99 })
                        Assert.That(sparkCatalog.TryGetSparkForAge(id, out _, out _),
                            Is.False, $"Native SPARK ID {id} must not be drawn.");
                    foreach (LoganObjectCatalog.Entry entry in candidate.Catalog.Entries)
                    {
                        var config = manager.GetCharacterConfig(entry.Id);
                        Assert.That(config?.characterData, Is.Not.Null, "Missing published config for " + entry.Id);
                        Assert.That(data.GetObjectById(entry.Id), Is.Not.Null, "Missing published data for " + entry.Id);
                        if (!string.IsNullOrEmpty(config.characterData.head))
                            Assert.That(ui.GetHeadSprite(entry.Id), Is.Not.Null, "Missing head for " + entry.Id);
                        if (!string.IsNullOrEmpty(config.characterData.small))
                            Assert.That(ui.GetSmallSprite(entry.Id), Is.Not.Null, "Missing small sprite for " + entry.Id);
                    }

                    foreach (string field in new[] { "publishedOwnedSprites", "publishedOwnedResources" })
                    {
                        var values = (IEnumerable)typeof(CharacterAnimtorManager).GetField(field, flags).GetValue(manager);
                        foreach (object value in values) tracked.Add((UnityEngine.Object)value);
                    }
                    Assert.That(tracked.Count, Is.GreaterThan(0), "No decoded publication resources were owned.");
                }
                finally
                {
                    try
                    {
                        if (managerRef != null)
                        {
                            const BindingFlags cleanupFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                            var disposedField = typeof(CharacterAnimtorManager).GetField("spritePrewarmDisposed", cleanupFlags);
                            if (!(bool)disposedField.GetValue(managerRef))
                            {
                                var destroy = typeof(CharacterAnimtorManager).GetMethod("OnDestroy", cleanupFlags);
                                Assert.That(destroy, Is.Not.Null);
                                destroy.Invoke(managerRef, null);
                            }
                        }
                    }
                    finally
                    {
                        if (initialized) fixture.TearDown();
                    }
                }
                var survivors = tracked.Where(value => value != null).ToArray();
                const BindingFlags diagnosticFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                bool disposed = !ReferenceEquals(managerRef, null) && (bool)typeof(CharacterAnimtorManager)
                    .GetField("spritePrewarmDisposed", diagnosticFlags).GetValue(managerRef);
                int remainingOwned = !ReferenceEquals(managerRef, null) ? ((IEnumerable)typeof(CharacterAnimtorManager)
                    .GetField("publishedOwnedSprites", diagnosticFlags).GetValue(managerRef)).Cast<object>().Count() : -1;
                Assert.That(survivors.Length, Is.Zero,
                    "Published resources survived fixture teardown: " + survivors.Length + "/" + tracked.Count +
                    "; managerDestroyed=" + (managerRef == null) + "; disposed=" + disposed +
                    "; remainingOwnedSprites=" + remainingOwned +
                    "; first=" + string.Join(", ", survivors.Take(8).Select(value =>
                        value.GetType().Name + ":" + value.name)));
            });
        }
    }
}
#endif
