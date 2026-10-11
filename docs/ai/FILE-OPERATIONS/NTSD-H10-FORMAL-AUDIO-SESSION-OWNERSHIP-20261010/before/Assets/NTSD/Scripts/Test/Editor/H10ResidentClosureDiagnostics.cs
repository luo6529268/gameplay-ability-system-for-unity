#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.App;
using NTSD.Load;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class H10ResidentClosureDiagnostics
    {
        private const string Output = "artifacts/diagnostics/NTSD-H10-RESIDENT-CLOSURE-20261010";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator UncachedOwnedSourceCanRetireAfterExactProductionConversion()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var loader = (NTSD_ResourceLoader)Activator.CreateInstance(typeof(NTSD_ResourceLoader), true);
                var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(loader);
                var globalCache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(NTSD_ResourceLoader.Instance);
                var before = globalCache.Keys.Cast<string>().ToDictionary(k => k, k => globalCache[k]);
                Type cueType = typeof(NTSDSoundPlayer).GetNestedType("PreparedSoundCue", BindingFlags.NonPublic);
                MethodInfo convert = typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips", Private);
                MethodInfo release = typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Private);
                var rows = new List<object>();
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    foreach (string relative in new[] { "c/jira/w/e3.wav", "c/kar/w/t.wav", "c/dan/w/t.wav", "data/001.wav" })
                    {
                        var host = new GameObject("H10OwnedSource") { hideFlags = HideFlags.HideAndDontSave };
                        host.SetActive(false);
                        var player = host.AddComponent<NTSDSoundPlayer>();
                        AudioClip source = null;
                        AudioClip playback = null;
                        try
                        {
                            source = await loader.LoadSingleAudioClipAsync(string.Empty,
                                Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/vfs/" + relative));
                            Assert.That(source, Is.Not.Null);
                            Assert.That(source.channels, Is.EqualTo(1));
                            long decodedObjectBytes = Profiler.GetRuntimeMemorySizeLong(source);
                            var input = new float[source.samples];
                            Assert.That(source.GetData(input, 0), Is.True);
                            object cue = Activator.CreateInstance(cueType);
                            cueType.GetField("IsLoaded").SetValue(cue, true);
                            cueType.GetField("Clips").SetValue(cue, new[] { source });
                            convert.Invoke(player, new[] { cue });
                            playback = ((AudioClip[])cueType.GetField("BattlePlaybackClips").GetValue(cue))[0];
                            Assert.That(playback, Is.Not.SameAs(source));
                            long sourceBytesAfterConversion = Profiler.GetRuntimeMemorySizeLong(source);
                            long playbackBytes = Profiler.GetRuntimeMemorySizeLong(playback);
                            // Prototype only: do not leave the cue referring to a retired native source.
                            cueType.GetField("Clips").SetValue(cue, new[] { playback });
                            UnityEngine.Object.DestroyImmediate(source);
                            Assert.That(source == null, Is.True);
                            var actual = new float[playback.samples * playback.channels];
                            Assert.That(playback.GetData(actual, 0), Is.True);
                            for (int i = 0; i < actual.Length; i++)
                                if (BitConverter.SingleToInt32Bits(actual[i]) != BitConverter.SingleToInt32Bits(input[i / 2]))
                                    Assert.Fail("Retired-source PCM mismatch: " + relative + ":" + i);
                            convert.Invoke(player, new[] { cue });
                            Assert.That(((AudioClip[])cueType.GetField("BattlePlaybackClips").GetValue(cue))[0], Is.SameAs(playback));
                            release.Invoke(player, null);
                            Assert.That(playback == null, Is.True);
                            Assert.That(cache.Count, Is.Zero);
                            rows.Add(new { cycle, relative, decodedObjectBytes, sourceBytesAfterConversion, playbackBytes,
                                retainedBeforeBytes = sourceBytesAfterConversion + playbackBytes, retainedAfterBytes = playbackBytes,
                                sourceRetired = source == null, playbackReleased = playback == null, pcmEqual = true });
                        }
                        finally
                        {
                            release.Invoke(player, null);
                            if (source != null) UnityEngine.Object.DestroyImmediate(source);
                            UnityEngine.Object.DestroyImmediate(host);
                        }
                    }
                }
                Assert.That(globalCache.Count, Is.EqualTo(before.Count));
                foreach (var entry in before) Assert.That(globalCache[entry.Key], Is.SameAs(entry.Value));
                Directory.CreateDirectory(Output);
                string file = Path.Combine(Output, "owned-source-" + Guid.NewGuid().ToString("N") + ".json");
                using var writer = new StreamWriter(new FileStream(file, FileMode.CreateNew, FileAccess.Write));
                writer.Write(JsonConvert.SerializeObject(new { rows, globalCacheUnchanged = true, isolatedCacheCount = cache.Count,
                    scope = "Actual existing uncached loader and production PCM conversion, prototype source retirement only; no production loading change, whole-game peak, playback waveform or natural lifecycle certificate." }, Formatting.Indented));
                TestContext.WriteLine(file);
            });
        }

        [Test]
        public void CurrentProductionCueInventoryAndVisualClosureCoverage()
        {
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset");
            Assert.That(config, Is.Not.Null);
            var source = BattleContentSource.ForLoganRuntime(Path.GetFullPath(config.BattleContentRuntimeRoot));
            var candidate = LoganVisualContentCandidate.Capture(source, ProjectBattleModeConfig.LoadDefault().Capture());
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(candidate.Catalog);
            FieldInfo configInstance = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            object previousConfig = configInstance.GetValue(null);
            var host = new GameObject("H10ClosureInventory") { hideFlags = HideFlags.HideAndDontSave };
            host.SetActive(false);
            try
            {
                configInstance.SetValue(null, config);
                var manager = host.AddComponent<CharacterAnimtorManager>();
                var player = host.AddComponent<NTSDSoundPlayer>();
                FieldInfo definitions = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private);
                MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue", Private);
                var builtIn = (string[])typeof(NTSDSoundPlayer).GetField("BuiltInBattleSoundIds", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var common = new HashSet<string>(builtIn, StringComparer.Ordinal);
                var feed = candidate.Catalog.ModeComboInput?.KnockoutFeed;
                foreach (string cue in new[] { feed?.StageTeam1DeathSoundPath, feed?.StageTeam5DeathSoundPath })
                    if (!string.IsNullOrWhiteSpace(cue)) common.Add(cue);
                var perDefinition = new Dictionary<int, string[]>();
                foreach (var pair in configs)
                {
                    definitions.SetValue(manager, new Dictionary<int, LF2CharacterDataWrapper> { [pair.Key] = pair.Value });
                    var cues = new HashSet<string>(StringComparer.Ordinal);
                    manager.CollectBattleSoundIds(cues);
                    perDefinition.Add(pair.Key, cues.OrderBy(x => x, StringComparer.Ordinal).ToArray());
                }
                definitions.SetValue(manager, configs);
                var all = new HashSet<string>(common, StringComparer.Ordinal);
                manager.CollectBattleSoundIds(all);
                object Describe(string cue, bool battle)
                {
                    object prepared = getCue.Invoke(player, new object[] { cue, battle });
                    if (prepared == null) return new { cue, battle, missingPrepared = true };
                    object Read(string name) => prepared.GetType().GetField(name).GetValue(prepared);
                    string path = (string)Read("SourcePath");
                    return new { cue, battle, path, exists = File.Exists(path) || Directory.Exists(path),
                        key = Read("CacheKey"), formal = Read("IsFormalBattleFile"), singleFile = Read("IsSingleFile") };
                }
                var resolved = all.OrderBy(x => x, StringComparer.Ordinal)
                    .Select(c => new { cue = c, formal = Describe(c, true), generic = Describe(c, false) }).ToArray();
                var closures = configs.Keys.OrderBy(x => x).Select(id =>
                {
                    var ids = CharacterAnimtorManager.CollectBattleVisualDependencyIds(configs, new[] { id });
                    var cues = new HashSet<string>(common, StringComparer.Ordinal);
                    foreach (int dependency in ids) cues.UnionWith(perDefinition[dependency]);
                    Assert.That(cues.IsSubsetOf(all), Is.True);
                    return new { id, definitions = ids.OrderBy(x => x).ToArray(), cues = cues.OrderBy(x => x, StringComparer.Ordinal).ToArray() };
                }).ToArray();
                candidate.AssertInputsCurrent();
                Directory.CreateDirectory(Output);
                string output = Path.Combine(Output, "cue-inventory-" + Guid.NewGuid().ToString("N") + ".json");
                using var writer = new StreamWriter(new FileStream(output, FileMode.CreateNew, FileAccess.Write));
                writer.Write(JsonConvert.SerializeObject(new { definitionCount = configs.Count, cueCount = all.Count,
                    common = common.OrderBy(x => x).ToArray(), perDefinition, resolved, closures,
                    scope = "Actual production collection and resolution; visual dependency closure is a candidate subset only, not certified audio reachability. No audio decode, playback, memory measurement or production filtering." }, Formatting.Indented));
                TestContext.WriteLine(output);
                typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Private).Invoke(player, null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                configInstance.SetValue(null, previousConfig);
            }
            Assert.That(configInstance.GetValue(null), Is.SameAs(previousConfig));
        }
    }
}
#endif
