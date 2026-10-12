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

        [UnityTest]
        public IEnumerator FormalAudioSourceRetirement_UsesProductionLoadConversionAndShutdown()
        {
            return UniTask.ToCoroutine(async () =>
            {
                const string gameConfigPath = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
                const string sessionOutput =
                    "artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010";
                const string relative = "c/jira/w/e3.wav";
                var evidence = new List<object>();
                string failure = null;
                GameConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(gameConfigPath);
                Assert.That(config, Is.Not.Null, gameConfigPath);

                FieldInfo configInstance = typeof(GameConfig).GetField(
                    "_instance", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(configInstance, Is.Not.Null);
                object previousConfig = configInstance.GetValue(null);

                var globalCache = (IDictionary)typeof(NTSD_ResourceLoader)
                    .GetField("cache", Private)
                    .GetValue(NTSD_ResourceLoader.Instance);
                Dictionary<string, object> cacheBefore = SnapshotCache(globalCache);
                var host = new GameObject("H10FormalSourceRetirement")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                host.SetActive(false);
                NTSDSoundPlayer player = null;
                object cue = null;
                AudioClip source = null;
                AudioClip playback = null;
                bool shutdownStarted = false;
                FieldInfo soundRootField = null;
                string previousSoundRoot = null;
                MethodInfo completeShutdown = null;

                try
                {
                    configInstance.SetValue(null, config);
                    player = host.AddComponent<NTSDSoundPlayer>();
                    player.runInEditMode = true;
                    soundRootField = typeof(NTSDSoundPlayer).GetField("soundRootFolder", Private);
                    Assert.That(soundRootField, Is.Not.Null);
                    previousSoundRoot = (string)soundRootField.GetValue(player);
                    soundRootField.SetValue(
                        player,
                        "H10_Diagnostic_NeverCreate_" + Guid.NewGuid().ToString("N"));

                    Type cueType = typeof(NTSDSoundPlayer).GetNestedType(
                        "PreparedSoundCue", BindingFlags.NonPublic);
                    Assert.That(cueType, Is.Not.Null);
                    MethodInfo getCue = RequireProductionMethod("GetOrPrepareCue", 2);
                    MethodInfo load = RequireProductionMethod("LoadPreparedClipsAsync", 1);
                    MethodInfo convert = RequireProductionMethod("PrepareBattlePlaybackClips", 1);
                    MethodInfo beginShutdown = RequireProductionMethod("BeginBattleSoundShutdown", 0);
                    completeShutdown = RequireProductionMethod("CompleteBattleSoundShutdown", 0);
                    RequireDiagnosticProperty("PendingFormalAudioLoadsForDiagnostics");
                    RequireDiagnosticProperty("OwnedFormalAudioSourcesForDiagnostics");
                    RequireDiagnosticProperty("FormalAudioLoadCountForDiagnostics");
                    RequireDiagnosticProperty("StaleFormalAudioReleaseCountForDiagnostics");
                    RequireDiagnosticProperty("BattleSoundShutdownCompletedForDiagnostics");
                    RequireDiagnosticProperty("BattleAudioPreparationInProgressForDiagnostics");

                    FieldInfo clipsField = RequireCueField(cueType, "Clips");
                    FieldInfo playbackField = RequireCueField(cueType, "BattlePlaybackClips");
                    FieldInfo ownedSourceField = RequireCueField(cueType, "OwnedSource");
                    FieldInfo generationField = RequireCueField(cueType, "CatalogGeneration");
                    FieldInfo identityField = RequireCueField(cueType, "FormalIdentity");

                    cue = getCue.Invoke(player, new object[] { relative, true });
                    Assert.That(cue, Is.Not.Null, relative);
                    Assert.That((bool)RequireCueField(cueType, "IsFormalBattleFile").GetValue(cue), Is.True);
                    string sourcePath = (string)RequireCueField(cueType, "SourcePath").GetValue(cue);
                    Assert.That(File.Exists(sourcePath), Is.True, sourcePath);

                    await (UniTask)load.Invoke(player, new[] { cue });
                    AudioClip[] loaded = (AudioClip[])clipsField.GetValue(cue);
                    Assert.That(loaded, Has.Length.EqualTo(1));
                    source = loaded[0];
                    Assert.That(source, Is.Not.Null);
                    Assert.That(source.channels, Is.EqualTo(1));
                    float[] sourceSamples = ReadSamples(source);
                    string sourceHash = SampleBitHash(sourceSamples);
                    int sourceInstanceId = source.GetInstanceID();
                    int sourceFrequency = source.frequency;
                    Assert.That(ownedSourceField.GetValue(cue), Is.SameAs(source));

                    convert.Invoke(player, new[] { cue });
                    AudioClip[] prepared = (AudioClip[])playbackField.GetValue(cue);
                    Assert.That(prepared, Has.Length.EqualTo(1));
                    playback = prepared[0];
                    Assert.That(playback, Is.Not.Null);
                    Assert.That(playback, Is.Not.SameAs(source));
                    Assert.That(source == null, Is.True,
                        "A converted formal mono source must be retired by production.");
                    Assert.That(ownedSourceField.GetValue(cue), Is.Null);
                    Assert.That(clipsField.GetValue(cue),
                        Is.SameAs(playbackField.GetValue(cue)));
                    Assert.That(((AudioClip[])clipsField.GetValue(cue))[0], Is.SameAs(playback));
                    float[] playbackSamples = ReadSamples(playback);
                    int playbackInstanceId = playback.GetInstanceID();
                    int playbackSampleCount = playback.samples;
                    int playbackChannels = playback.channels;
                    int playbackFrequency = playback.frequency;
                    string playbackHash = SampleBitHash(playbackSamples);
                    Assert.That(playback.channels, Is.EqualTo(2));
                    Assert.That(playback.samples, Is.EqualTo(sourceSamples.Length));
                    AssertStereoMirror(sourceSamples, playbackSamples, relative);

                    beginShutdown.Invoke(player, null);
                    shutdownStarted = true;
                    completeShutdown.Invoke(player, null);
                    Assert.That(ReadDiagnostic<bool>(player,
                        "BattleSoundShutdownCompletedForDiagnostics"), Is.True);
                    Assert.That(ReadDiagnostic<int>(player,
                        "OwnedFormalAudioSourcesForDiagnostics"), Is.Zero);
                    Assert.That(ownedSourceField.GetValue(cue), Is.Null);
                    Assert.That(IsEmptyOrNull((AudioClip[])clipsField.GetValue(cue)), Is.True);
                    Assert.That(IsEmptyOrNull((AudioClip[])playbackField.GetValue(cue)), Is.True);

                    evidence.Add(new
                    {
                        relative,
                        sourcePath,
                        sourceInstanceId,
                        sourceSamples = sourceSamples.Length,
                        sourceChannels = 1,
                        sourceFrequency,
                        sourceHash,
                        playbackInstanceId,
                        playbackSamples = playbackSampleCount,
                        playbackChannels,
                        playbackFrequency,
                        playbackHash,
                        generation = generationField.GetValue(cue),
                        formalIdentity = identityField.GetValue(cue),
                        sourceRetired = source == null,
                        cueSourceCleared = IsEmptyOrNull((AudioClip[])clipsField.GetValue(cue)),
                        playbackReleased = playback == null,
                        pendingFormalLoads = ReadDiagnostic<int>(player,
                            "PendingFormalAudioLoadsForDiagnostics"),
                        ownedFormalSources = ReadDiagnostic<int>(player,
                            "OwnedFormalAudioSourcesForDiagnostics"),
                        formalAudioLoadCount = ReadDiagnostic<long>(player,
                            "FormalAudioLoadCountForDiagnostics"),
                        staleFormalReleases = ReadDiagnostic<long>(player,
                            "StaleFormalAudioReleaseCountForDiagnostics"),
                        shutdownCompleted = ReadDiagnostic<bool>(player,
                            "BattleSoundShutdownCompletedForDiagnostics"),
                        preparationInProgress = ReadDiagnostic<bool>(player,
                            "BattleAudioPreparationInProgressForDiagnostics"),
                        globalCacheUnchanged = CacheMatches(globalCache, cacheBefore),
                        scope = "One real formal mono WAV; production load, conversion, stage1/stage5 shutdown, source/output ownership only. Cold allocation is allowed; no public whole-catalog prewarm, cost group, Player or device claim."
                    });
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (shutdownStarted && completeShutdown != null && player != null)
                        completeShutdown.Invoke(player, null);

                    if (soundRootField != null && player != null)
                        soundRootField.SetValue(player, previousSoundRoot);
                    if (source != null)
                        UnityEngine.Object.DestroyImmediate(source);
                    if (playback != null)
                        UnityEngine.Object.DestroyImmediate(playback);
                    if (host != null)
                        UnityEngine.Object.DestroyImmediate(host);
                    configInstance.SetValue(null, previousConfig);

                    evidence.Add(new
                    {
                        failure,
                        globalCacheUnchanged = CacheMatches(globalCache, cacheBefore),
                        configRestored = ReferenceEquals(configInstance.GetValue(null), previousConfig),
                        outputKind = "CreateNew GUID JSON",
                    });
                    Directory.CreateDirectory(sessionOutput);
                    string output = Path.Combine(
                        sessionOutput,
                        "formal-source-retirement-" + Guid.NewGuid().ToString("N") + ".json");
                    using var writer = new StreamWriter(new FileStream(
                        output, FileMode.CreateNew, FileAccess.Write));
                    writer.Write(JsonConvert.SerializeObject(new
                    {
                        test = TestContext.CurrentContext.Test.FullName,
                        evidence,
                        cacheBefore = cacheBefore.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                    }, Formatting.Indented));
                    TestContext.WriteLine(output);
                }

                Assert.That(CacheMatches(globalCache, cacheBefore), Is.True,
                    "Formal session must leave the global loader cache unchanged.");
                Assert.That(configInstance.GetValue(null), Is.SameAs(previousConfig),
                    "The temporary GameConfig static binding must be restored.");
            });
        }

        private static MethodInfo RequireProductionMethod(string name, int parameterCount)
        {
            MethodInfo method = typeof(NTSDSoundPlayer).GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate => candidate.Name == name &&
                    candidate.GetParameters().Length == parameterCount);
            Assert.That(method, Is.Not.Null,
                "Missing H10 production method contract: " + name + "/" + parameterCount);
            return method;
        }

        private static PropertyInfo RequireDiagnosticProperty(string name)
        {
            PropertyInfo property = typeof(NTSDSoundPlayer).GetProperty(
                name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, "Missing H10 diagnostic property: " + name);
            return property;
        }

        private static FieldInfo RequireCueField(Type cueType, string name)
        {
            FieldInfo field = cueType.GetField(
                name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing PreparedSoundCue field: " + name);
            return field;
        }

        private static T ReadDiagnostic<T>(NTSDSoundPlayer player, string name)
        {
            PropertyInfo property = RequireDiagnosticProperty(name);
            return (T)property.GetValue(player);
        }

        private static Dictionary<string, object> SnapshotCache(IDictionary cache)
        {
            var snapshot = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in cache)
                snapshot[(string)entry.Key] = entry.Value;
            return snapshot;
        }

        private static bool CacheMatches(IDictionary cache, Dictionary<string, object> snapshot)
        {
            if (cache.Count != snapshot.Count)
                return false;
            foreach (var entry in snapshot)
            {
                if (!cache.Contains(entry.Key) || !ReferenceEquals(cache[entry.Key], entry.Value))
                    return false;
            }
            return true;
        }

        private static bool IsEmptyOrNull(AudioClip[] clips)
        {
            return clips == null || clips.Length == 0 || clips.All(clip => clip == null);
        }

        private static float[] ReadSamples(AudioClip clip)
        {
            var data = new float[checked(clip.samples * clip.channels)];
            Assert.That(clip.GetData(data, 0), Is.True, clip.name);
            return data;
        }

        private static string SampleBitHash(float[] samples)
        {
            var bits = new int[samples.Length];
            for (int index = 0; index < samples.Length; index++)
                bits[index] = BitConverter.SingleToInt32Bits(samples[index]);
            byte[] bytes = new byte[checked(bits.Length * sizeof(int))];
            Buffer.BlockCopy(bits, 0, bytes, 0, bytes.Length);
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        private static void AssertStereoMirror(
            float[] mono, float[] stereo, string label)
        {
            Assert.That(stereo.Length, Is.EqualTo(mono.Length * 2), label);
            for (int index = 0; index < mono.Length; index++)
            {
                int left = index * 2;
                if (BitConverter.SingleToInt32Bits(stereo[left]) !=
                    BitConverter.SingleToInt32Bits(mono[index]) ||
                    BitConverter.SingleToInt32Bits(stereo[left + 1]) !=
                    BitConverter.SingleToInt32Bits(mono[index]))
                {
                    Assert.Fail(label + " stereo PCM mismatch at frame " + index);
                }
            }
        }

        [UnityTest]
        public IEnumerator FormalAudioSession_ThreeCyclesPublicPreparePlayShutdownAndColdReentry()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FormalAudioSessionFixture fixture = null;
                var evidence = new List<object>();
                string failure = null;
                string cleanupFailure = null;
                int previousGeneration = 0;
                try
                {
                    fixture = new FormalAudioSessionFixture(
                        "H10FormalThreeCycleSession", false, true);
                    Assert.That(fixture.CollectedCueIds,
                        Is.EquivalentTo(fixture.FixedCueIds));
                    Assert.That(fixture.BuiltInIds, Has.Length.EqualTo(19));
                    Assert.That(
                        ReferenceEquals(AudioController.Instance,
                            fixture.PreviousAudioController),
                        Is.True,
                        "All three rounds must use the same AudioController instance.");
                    int expectedFormalLoads = fixture.ExpectedUniqueFormalCount;
                    Assert.That(expectedFormalLoads, Is.GreaterThan(0));

                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        long formalLoadsBefore =
                            fixture.Player.FormalAudioLoadCountForDiagnostics;
                        long rejectedBefore =
                            fixture.Player.RejectedUnpreparedCueCountForDiagnostics;
                        UniTask prepare = fixture.Player.PrepareBattleCuesAsync(
                            fixture.Manager);
                        await AwaitWithTimeout(
                            prepare,
                            "formal three-cycle public prepare " + cycle);
                        Assert.That(fixture.Player.BattleCatalogSealedForDiagnostics,
                            Is.True);
                        Assert.That(fixture.Player.BattleAudioPreparationInProgressForDiagnostics,
                            Is.False);
                        Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                            Is.Zero);
                        Assert.That(fixture.Player.BattleAudioTailDrainedForDiagnostics,
                            Is.True);
                        Assert.That(
                            fixture.Player.FormalAudioLoadCountForDiagnostics -
                            formalLoadsBefore,
                            Is.EqualTo(expectedFormalLoads),
                            "Physical formal loads must count unique formal identities only.");
                        Assert.That(
                            fixture.Player.RejectedUnpreparedCueCountForDiagnostics,
                            Is.EqualTo(rejectedBefore));

                        object[] cycleCues = fixture.FixedCueIds
                            .Select(fixture.GetBattleCue)
                            .ToArray();
                        int generation = fixture.ReadCue<int>(cycleCues[0],
                            "CatalogGeneration");
                        Assert.That(generation, Is.GreaterThan(previousGeneration));
                        previousGeneration = generation;

                        object alias = fixture.GetBattleCue("SFX_001");
                        object dataAlias = fixture.GetBattleCue("data/001.wav");
                        Assert.That(alias, Is.SameAs(dataAlias));
                        Assert.That(
                            fixture.ReadCue<string>(alias, "FormalIdentity"),
                            Is.EqualTo(fixture.ReadCue<string>(dataAlias, "FormalIdentity")));

                        long cueBuildsBeforePlayback =
                            fixture.Player.PreparedCueBuildCountForDiagnostics;
                        long formalLoadsBeforePlayback =
                            fixture.Player.FormalAudioLoadCountForDiagnostics;
                        var cueEvidence = new List<object>();
                        foreach (string cueId in fixture.FixedCueIds)
                        {
                            object cue = fixture.GetBattleCue(cueId);
                            AudioClip[] playback = fixture.ReadCue<AudioClip[]>(
                                cue, "BattlePlaybackClips");
                            Assert.That(playback, Is.Not.Null, cueId);
                            Assert.That(playback, Is.Not.Empty, cueId);
                            Assert.That(playback.Any(clip => clip != null), Is.True, cueId);
                            if (string.Equals(cueId, "c/kim/w/a1.wav",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                AudioClip owned = fixture.ReadCue<AudioClip>(
                                    cue, "OwnedSource");
                                AudioClip[] sourceClips = fixture.ReadCue<AudioClip[]>(
                                    cue, "Clips");
                                Assert.That(owned, Is.Not.Null, cueId);
                                Assert.That(owned.channels, Is.Not.EqualTo(1), cueId);
                                Assert.That(sourceClips, Is.Not.Null, cueId);
                                Assert.That(sourceClips[0], Is.SameAs(owned), cueId);
                                Assert.That(playback[0], Is.SameAs(owned), cueId);
                            }
                            else
                            {
                                Assert.That(fixture.ReadCue<AudioClip>(
                                    cue, "OwnedSource"), Is.Null, cueId);
                                Assert.That(fixture.ReadCue<AudioClip[]>(
                                    cue, "Clips"), Is.SameAs(playback), cueId);
                                Assert.That(playback[0].channels, Is.EqualTo(2), cueId);
                            }

                            ResetAudioItemThrottle(cue);
                            long playedBefore =
                                fixture.Player.PooledOneShotPlayCountForDiagnostics;
                            fixture.Player.PresentSound(
                                new NTSD.Simulation.PendingSoundEvent(cueId, 500, cycle + 1));
                            Assert.That(
                                fixture.Player.PooledOneShotPlayCountForDiagnostics,
                                Is.EqualTo(playedBefore + 1), cueId);
                            AudioClip expectedClip = playback.FirstOrDefault(
                                clip => clip != null);
                            Assert.That(FindVoiceWithClip(fixture.Host, expectedClip),
                                Is.Not.Null, cueId);
                            cueEvidence.Add(DescribeCue(fixture, cueId, cue));
                        }

                        Assert.That(
                            fixture.Player.PreparedCueBuildCountForDiagnostics,
                            Is.EqualTo(cueBuildsBeforePlayback),
                            "Playing prepared battle cues must not build another cue.");
                        Assert.That(
                            fixture.Player.FormalAudioLoadCountForDiagnostics,
                            Is.EqualTo(formalLoadsBeforePlayback),
                            "Playing prepared battle cues must not perform another formal load.");

                        long playsBeforeStage1 =
                            fixture.Player.PooledOneShotPlayCountForDiagnostics;
                        fixture.BeginShutdown();
                        fixture.Player.PresentSound(
                            new NTSD.Simulation.PendingSoundEvent(
                                fixture.FixedCueIds[0], 500, cycle + 10));
                        Assert.That(
                            fixture.Player.PooledOneShotPlayCountForDiagnostics,
                            Is.EqualTo(playsBeforeStage1));
                        OwnedAudioReleaseSnapshot managerStage5Release =
                            CaptureOwnedAudioSnapshot(
                                fixture.Player,
                                "manager-cycle-" + (cycle + 1) + "-stage5");
                        fixture.CompleteShutdown();
                        AssertShutdownReleased(fixture, managerStage5Release);
                        await DrainFormalAudioAsync(
                            fixture.Player,
                            180,
                            "three-cycle stage1/stage5 drain " + cycle);
                        var stage5CueEvidence = cycleCues
                            .Select(cue => DescribeCue(
                                fixture,
                                fixture.ReadCue<string>(cue, "FormalIdentity"),
                                cue))
                            .ToArray();
                        foreach (object cue in cycleCues)
                        {
                            Assert.That(fixture.ReadCue<AudioClip>(
                                cue, "OwnedSource"), Is.Null);
                            Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(
                                cue, "Clips")), Is.True);
                            Assert.That(fixture.ReadCue<AudioClip[]>(
                                cue, "BattlePlaybackClips"), Is.Null);
                        }

                        long coldFormalLoadsBefore =
                            fixture.Player.FormalAudioLoadCountForDiagnostics;
                        UniTask coldReentry = fixture.Player.PrepareBattleCuesAsync(null);
                        await AwaitWithTimeout(
                            coldReentry,
                            "formal three-cycle public cold reentry " + cycle);
                        Assert.That(fixture.Player.BattleCatalogSealedForDiagnostics,
                            Is.True);
                        Assert.That(fixture.Player.BattleAudioPreparationInProgressForDiagnostics,
                            Is.False);
                        Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                            Is.Zero);
                        Assert.That(fixture.Player.BattleAudioTailDrainedForDiagnostics,
                            Is.True);
                        Assert.That(
                            fixture.Player.FormalAudioLoadCountForDiagnostics -
                            coldFormalLoadsBefore,
                            Is.EqualTo(fixture.ExpectedBuiltInFormalCount));
                        foreach (string builtIn in fixture.BuiltInIds)
                        {
                            object builtInCue = fixture.GetBattleCue(builtIn);
                            Assert.That(builtInCue, Is.Not.Null, builtIn);
                            Assert.That(fixture.ReadCue<bool>(
                                builtInCue, "IsLoaded"), Is.True, builtIn);
                            object genericCue = fixture.GetGenericCue(builtIn);
                            if (genericCue != null)
                            {
                                string genericSourcePath = fixture.ReadCue<string>(
                                    genericCue, "SourcePath");
                                Assert.That(
                                    File.Exists(genericSourcePath) ||
                                    Directory.Exists(genericSourcePath),
                                    Is.False,
                                    "The diagnostic generic root must remain absent: " +
                                    genericSourcePath);
                            }
                        }

                        fixture.BeginShutdown();
                        OwnedAudioReleaseSnapshot coldStage5Release =
                            CaptureOwnedAudioSnapshot(
                                fixture.Player,
                                "builtin-cold-cycle-" + (cycle + 1) + "-stage5");
                        fixture.CompleteShutdown();
                        AssertShutdownReleased(fixture, coldStage5Release);
                        await DrainFormalAudioAsync(
                            fixture.Player,
                            180,
                            "cold reentry stage1/stage5 drain " + cycle);

                        evidence.Add(new
                        {
                            cycle,
                            managerPrepareRound = cycle + 1,
                            builtinColdReentry = true,
                            controllerSameAcrossCycle =
                                ReferenceEquals(AudioController.Instance,
                                    fixture.PreviousAudioController),
                            generation,
                            cueEvidence,
                            stage5CueEvidence,
                            managerStage5Release = managerStage5Release.ToEvidence(),
                            coldStage5Release = coldStage5Release.ToEvidence(),
                            cueBuildsBeforePlayback,
                            cueBuildsAfterPlayback =
                                fixture.Player.PreparedCueBuildCountForDiagnostics,
                            formalLoadsBeforePlayback,
                            formalLoadsAfterPlayback =
                                fixture.Player.FormalAudioLoadCountForDiagnostics,
                            uniqueFormalLoadCount = expectedFormalLoads,
                            builtInCount = fixture.BuiltInIds.Length,
                            builtInFormalLoadCount = fixture.ExpectedBuiltInFormalCount,
                            genericRoot = fixture.GenericRoot,
                            genericRootExists = Directory.Exists(fixture.GenericRootPath),
                            stage5Released = fixture.Player.OwnedFormalAudioSourcesForDiagnostics == 0,
                            pendingAfterDrain = fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                            preparationAfterDrain = fixture.Player.BattleAudioPreparationInProgressForDiagnostics,
                            shutdownCompleted = fixture.Player.BattleSoundShutdownCompletedForDiagnostics,
                        });
                    }
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (fixture != null)
                    {
                        if (fixture.Player != null)
                        {
                            try
                            {
                                fixture.BeginShutdown();
                                fixture.CompleteShutdown();
                                await DrainFormalAudioAsync(
                                    fixture.Player,
                                    180,
                                    "three-cycle final cleanup drain");
                            }
                            catch (Exception ex)
                            {
                                cleanupFailure = ex.ToString();
                            }
                        }

                        fixture.Dispose();
                        WriteH10Evidence("formal-three-cycle", new
                        {
                            failure,
                            cleanupFailure,
                            evidence,
                            managerPrepareRounds = 3,
                            builtinColdReentries = 3,
                            cacheRestored = fixture.CacheRestored,
                            configRestored = fixture.ConfigRestored,
                            sceneRestored = fixture.SceneRestored,
                            audioControllerRestored = fixture.AudioControllerRestored,
                            mmSoundManagerRestored = fixture.MMSoundManagerRestored,
                            loaderPendingRequestsRestored = fixture.LoaderPendingRequestsRestored,
                            loaderQueuedTasksRestored = fixture.LoaderQueuedTasksRestored,
                            scope = "Three public manager-bounded formal prewarm cycles with real play, stage1/stage5 drain, and public null cold reentry; fixed five-cue fixture plus the existing nineteen builtins only."
                        });
                        Assert.That(fixture.CacheRestored, Is.True);
                        Assert.That(fixture.ConfigRestored, Is.True);
                        Assert.That(fixture.SceneRestored, Is.True);
                        Assert.That(fixture.AudioControllerRestored, Is.True);
                        Assert.That(fixture.MMSoundManagerRestored, Is.True);
                        Assert.That(fixture.LoaderPendingRequestsRestored, Is.True);
                        Assert.That(fixture.LoaderQueuedTasksRestored, Is.True);
                        if (cleanupFailure != null && failure == null)
                            Assert.Fail(cleanupFailure);
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator FormalAudioSession_LocalLoadAwaitCancellationReleasesStaleResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FormalAudioSessionFixture fixture = null;
                var evidence = new List<object>();
                string failure = null;
                string cleanupFailure = null;
                try
                {
                    fixture = new FormalAudioSessionFixture(
                        "H10FormalLoadCancellation", false, false);
                    object cue = fixture.GetBattleCue("c/kim/w/a1.wav");
                    string sourcePath = fixture.ReadCue<string>(cue, "SourcePath");
                    long staleBefore =
                        fixture.Player.StaleFormalAudioReleaseCountForDiagnostics;
                    UniTask load = fixture.LoadCue(cue);
                    await WaitForConditionAsync(
                        () => fixture.Player.PendingFormalAudioLoadsForDiagnostics > 0,
                        180,
                        "real local formal load did not expose a pending await");
                    Assert.That(fixture.Player.FormalAudioLoadCountForDiagnostics,
                        Is.GreaterThan(0));
                    fixture.BeginShutdown();
                    fixture.CompleteShutdown();
                    int pendingAtStage5 = fixture.Player.PendingFormalAudioLoadsForDiagnostics;
                    Assert.That(pendingAtStage5, Is.EqualTo(1));
                    Assert.That(fixture.Player.BattleSoundShutdownCompletedForDiagnostics, Is.True);
                    Assert.That(fixture.Player.BattleAudioTailDrainedForDiagnostics, Is.False);
                    await AwaitWithTimeout(load, "local formal load cancellation");
                    await DrainFormalAudioAsync(
                        fixture.Player,
                        180,
                        "local formal load cancellation drain");
                    fixture.CompleteShutdown();
                    AssertShutdownReleased(fixture);
                    int staleId = fixture.Player.LastStaleFormalAudioInstanceIdForDiagnostics;
                    Assert.That(staleId, Is.Not.Zero);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(c => c.GetInstanceID() == staleId), Is.False);
                    Assert.That(
                        fixture.Player.StaleFormalAudioReleaseCountForDiagnostics,
                        Is.GreaterThan(staleBefore));
                    Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(
                        cue, "Clips")), Is.True);
                    Assert.That(fixture.ReadCue<AudioClip>(
                        cue, "OwnedSource"), Is.Null);
                    evidence.Add(new
                    {
                        sourcePath, staleId, staleNativeGone = true, pendingAtStage5,
                        formalIdentity = fixture.ReadCue<string>(cue, "FormalIdentity"),
                        generation = fixture.ReadCue<int>(cue, "CatalogGeneration"),
                        staleBefore,
                        staleAfter = fixture.Player.StaleFormalAudioReleaseCountForDiagnostics,
                        formalLoads = fixture.Player.FormalAudioLoadCountForDiagnostics,
                        pending = fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                        ownedSources = fixture.Player.OwnedFormalAudioSourcesForDiagnostics,
                        sourceFile = ReadWaveMetadata(sourcePath),
                        globalCacheUnchanged = CacheMatches(
                            fixture.GlobalCache, fixture.CacheBefore),
                        scope = "One actual local formal await invalidated by production BeginBattleSoundShutdown; stale result release and stage5 cleanup."
                    });
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (fixture != null)
                    {
                        if (fixture.Player != null)
                        {
                            try
                            {
                                fixture.BeginShutdown();
                                await DrainFormalAudioAsync(
                                    fixture.Player,
                                    180,
                                    "load cancellation final cleanup drain");
                                fixture.CompleteShutdown();
                            }
                            catch (Exception ex)
                            {
                                cleanupFailure = ex.ToString();
                            }
                        }

                        fixture.Dispose();
                        WriteH10Evidence("formal-load-cancel", new
                        {
                            failure,
                            cleanupFailure,
                            evidence,
                            cacheRestored = fixture.CacheRestored,
                            configRestored = fixture.ConfigRestored,
                            sceneRestored = fixture.SceneRestored,
                            audioControllerRestored = fixture.AudioControllerRestored,
                            mmSoundManagerRestored = fixture.MMSoundManagerRestored,
                            loaderPendingRequestsRestored = fixture.LoaderPendingRequestsRestored,
                            loaderQueuedTasksRestored = fixture.LoaderQueuedTasksRestored,
                            scope = "Actual local-load cancellation only; no injected production fault or cache cleanup."
                        });
                        Assert.That(fixture.CacheRestored, Is.True);
                        Assert.That(fixture.ConfigRestored, Is.True);
                        Assert.That(fixture.SceneRestored, Is.True);
                        Assert.That(fixture.AudioControllerRestored, Is.True);
                        Assert.That(fixture.MMSoundManagerRestored, Is.True);
                        Assert.That(fixture.LoaderPendingRequestsRestored, Is.True);
                        Assert.That(fixture.LoaderQueuedTasksRestored, Is.True);
                        if (cleanupFailure != null && failure == null)
                            Assert.Fail(cleanupFailure);
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator FormalAudioSession_NaturalOnDestroyDuringAwaitReleasesStaleResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FormalAudioSessionFixture fixture = null;
                var evidence = new List<object>();
                string failure = null;
                string cleanupFailure = null;
                try
                {
                    fixture = new FormalAudioSessionFixture(
                        "H10FormalNaturalOnDestroy", true, false);
                    Assert.That(fixture.Host.activeSelf, Is.True);
                    Assert.That(fixture.Player.runInEditMode, Is.True);
                    object cue = fixture.GetBattleCue("c/kim/w/a1.wav");
                    string sourcePath = fixture.ReadCue<string>(cue, "SourcePath");
                    long staleBefore =
                        ReadPrivateField<long>(fixture.Player,
                            "staleFormalAudioReleaseCount");
                    UniTask load = fixture.LoadCue(cue);
                    await WaitForConditionAsync(
                        () => ReadPrivateField<int>(fixture.Player,
                            "pendingFormalAudioLoads") > 0,
                        180,
                        "natural OnDestroy load did not expose a pending await");
                    Assert.That(fixture.Player.runInEditMode, Is.True);
                    UnityEngine.Object.DestroyImmediate(fixture.Host);
                    int pendingAtDestroy = ReadPrivateField<int>(fixture.Player, "pendingFormalAudioLoads");
                    Assert.That(pendingAtDestroy, Is.EqualTo(1));
                    Assert.That(ReadPrivateField<bool>(fixture.Player, "battleSoundShutdownCompleted"), Is.True);
                    Assert.That(ReadPrivateField<bool>(fixture.Player, "soundOwnerDisposed"), Is.True);
                    await AwaitWithTimeout(load, "natural OnDestroy formal load");
                    Assert.That(fixture.Player == null, Is.True,
                        "The active host must reach the real Unity OnDestroy path.");
                    long staleAfter = ReadPrivateField<long>(fixture.Player,
                        "staleFormalAudioReleaseCount");
                    int pendingAfter = ReadPrivateField<int>(fixture.Player,
                        "pendingFormalAudioLoads");
                    Assert.That(staleAfter, Is.GreaterThan(staleBefore));
                    Assert.That(pendingAfter, Is.Zero);
                    int staleId = ReadPrivateField<int>(fixture.Player, "lastStaleFormalAudioInstanceId");
                    Assert.That(staleId, Is.Not.Zero);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(c => c.GetInstanceID() == staleId), Is.False);
                    Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(
                        cue, "Clips")), Is.True);
                    Assert.That(fixture.ReadCue<AudioClip>(
                        cue, "OwnedSource"), Is.Null);
                    evidence.Add(new
                    {
                        staleId, staleNativeGone = true, pendingAtDestroy,
                        activeHost = true,
                        runInEditMode = true,
                        sourcePath,
                        formalIdentity = fixture.ReadCue<string>(cue, "FormalIdentity"),
                        generation = fixture.ReadCue<int>(cue, "CatalogGeneration"),
                        staleBefore,
                        staleAfter,
                        pendingAfter,
                        sourceFile = ReadWaveMetadata(sourcePath),
                        globalCacheUnchanged = CacheMatches(
                            fixture.GlobalCache, fixture.CacheBefore),
                        scope = "Real active-host DestroyImmediate during a local formal await; production OnDestroy only, with no reflected lifecycle call."
                    });
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (fixture != null)
                    {
                        if (fixture.Player != null)
                        {
                            try
                            {
                                fixture.BeginShutdown();
                                await DrainFormalAudioAsync(
                                    fixture.Player,
                                    180,
                                    "natural OnDestroy final cleanup drain");
                                fixture.CompleteShutdown();
                            }
                            catch (Exception ex)
                            {
                                cleanupFailure = ex.ToString();
                            }
                        }

                        fixture.Dispose();
                        WriteH10Evidence("formal-natural-ondestroy", new
                        {
                            failure,
                            cleanupFailure,
                            evidence,
                            cacheRestored = fixture.CacheRestored,
                            configRestored = fixture.ConfigRestored,
                            sceneRestored = fixture.SceneRestored,
                            audioControllerRestored = fixture.AudioControllerRestored,
                            mmSoundManagerRestored = fixture.MMSoundManagerRestored,
                            loaderPendingRequestsRestored = fixture.LoaderPendingRequestsRestored,
                            loaderQueuedTasksRestored = fixture.LoaderQueuedTasksRestored,
                            scope = "Natural OnDestroy during real local load await; active host and runInEditMode are required preconditions."
                        });
                        Assert.That(fixture.CacheRestored, Is.True);
                        Assert.That(fixture.ConfigRestored, Is.True);
                        Assert.That(fixture.SceneRestored, Is.True);
                        Assert.That(fixture.AudioControllerRestored, Is.True);
                        Assert.That(fixture.MMSoundManagerRestored, Is.True);
                        Assert.That(fixture.LoaderPendingRequestsRestored, Is.True);
                        Assert.That(fixture.LoaderQueuedTasksRestored, Is.True);
                        if (cleanupFailure != null && failure == null)
                            Assert.Fail(cleanupFailure);
                    }
                }
            });
        }

        [UnityTest]
        public IEnumerator FormalAudioSession_ConcurrentPrepareKeepsNewestGeneration()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FormalAudioSessionFixture fixture = null;
                var evidence = new List<object>();
                string failure = null;
                string cleanupFailure = null;
                try
                {
                    fixture = new FormalAudioSessionFixture(
                        "H10FormalConcurrentPrepare", false, true);
                    long staleBefore =
                        fixture.Player.StaleFormalAudioReleaseCountForDiagnostics;
                    long formalLoadsBefore =
                        fixture.Player.FormalAudioLoadCountForDiagnostics;
                    UniTask first = fixture.Player.PrepareBattleCuesAsync(
                        fixture.Manager);
                    await WaitForConditionAsync(
                        () => fixture.Player.BattleAudioPreparationInProgressForDiagnostics &&
                            fixture.Player.PendingFormalAudioLoadsForDiagnostics > 0,
                        240,
                        "first concurrent public prepare did not enter a local await");
                    object firstCue = fixture.GetBattleCue(fixture.FixedCueIds[0]);
                    int firstGeneration = fixture.ReadCue<int>(
                        firstCue, "CatalogGeneration");
                    UniTask second = fixture.Player.PrepareBattleCuesAsync(
                        fixture.Manager);
                    int maximumPending = 0;
                    int frameBudget = 360;
                    for (int frame = 0; frame < frameBudget; frame++)
                    {
                        maximumPending = Math.Max(
                            maximumPending,
                            fixture.Player.PendingFormalAudioLoadsForDiagnostics);
                        if (first.Status.IsCompleted() && second.Status.IsCompleted())
                            break;
                        await UniTask.Yield();
                    }
                    Assert.That(first.Status.IsCompleted(), Is.True,
                        "The superseded preparation did not drain.");
                    Assert.That(second.Status.IsCompleted(), Is.True,
                        "The newest preparation did not complete.");
                    await AwaitWithTimeout(
                        UniTask.WhenAll(first, second),
                        "concurrent public prepare completion");
                    Assert.That(maximumPending, Is.EqualTo(1),
                        "Old formal pending work must drain before the new generation loads.");
                    Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                        Is.Zero);
                    Assert.That(fixture.Player.BattleAudioPreparationInProgressForDiagnostics,
                        Is.False);
                    Assert.That(fixture.Player.BattleCatalogSealedForDiagnostics,
                        Is.True);
                    object finalCue = fixture.GetBattleCue(fixture.FixedCueIds[0]);
                    int finalGeneration = fixture.ReadCue<int>(
                        finalCue, "CatalogGeneration");
                    Assert.That(finalCue, Is.Not.SameAs(firstCue));
                    Assert.That(finalGeneration, Is.GreaterThan(firstGeneration));
                    Assert.That(fixture.ReadCue<bool>(finalCue, "IsLoaded"), Is.True);
                    Assert.That(fixture.ReadCue<AudioClip[]>(
                        finalCue, "BattlePlaybackClips"), Is.Not.Null);
                    Assert.That(
                        fixture.Player.StaleFormalAudioReleaseCountForDiagnostics,
                        Is.GreaterThan(staleBefore));
                    Assert.That(
                        fixture.Player.FormalAudioLoadCountForDiagnostics - formalLoadsBefore,
                        Is.GreaterThanOrEqualTo(fixture.ExpectedUniqueFormalCount + 1));
                    evidence.Add(new
                    {
                        firstGeneration,
                        finalGeneration,
                        firstCueRetired = fixture.ReadCue<AudioClip[]>(
                            firstCue, "BattlePlaybackClips") == null,
                        maximumPending,
                        staleBefore,
                        staleAfter = fixture.Player.StaleFormalAudioReleaseCountForDiagnostics,
                        formalLoadsBefore,
                        formalLoadsAfter = fixture.Player.FormalAudioLoadCountForDiagnostics,
                        finalCue = DescribeCue(
                            fixture, fixture.FixedCueIds[0], finalCue),
                        globalCacheUnchanged = CacheMatches(
                            fixture.GlobalCache, fixture.CacheBefore),
                        scope = "Two real public manager-bounded preparations on one player; the first local result is stale and cannot publish over the second generation."
                    });
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (fixture != null)
                    {
                        if (fixture.Player != null)
                        {
                            try
                            {
                                fixture.BeginShutdown();
                                await DrainFormalAudioAsync(
                                    fixture.Player,
                                    240,
                                    "concurrent prepare final cleanup drain");
                                fixture.CompleteShutdown();
                            }
                            catch (Exception ex)
                            {
                                cleanupFailure = ex.ToString();
                            }
                        }

                        fixture.Dispose();
                        WriteH10Evidence("formal-concurrent-prepare", new
                        {
                            failure,
                            cleanupFailure,
                            evidence,
                            cacheRestored = fixture.CacheRestored,
                            configRestored = fixture.ConfigRestored,
                            sceneRestored = fixture.SceneRestored,
                            audioControllerRestored = fixture.AudioControllerRestored,
                            mmSoundManagerRestored = fixture.MMSoundManagerRestored,
                            loaderPendingRequestsRestored = fixture.LoaderPendingRequestsRestored,
                            loaderQueuedTasksRestored = fixture.LoaderQueuedTasksRestored,
                            scope = "Concurrent public PrepareBattleCuesAsync only; no reflected unseal or production fault injection."
                        });
                        Assert.That(fixture.CacheRestored, Is.True);
                        Assert.That(fixture.ConfigRestored, Is.True);
                        Assert.That(fixture.SceneRestored, Is.True);
                        Assert.That(fixture.AudioControllerRestored, Is.True);
                        Assert.That(fixture.MMSoundManagerRestored, Is.True);
                        Assert.That(fixture.LoaderPendingRequestsRestored, Is.True);
                        Assert.That(fixture.LoaderQueuedTasksRestored, Is.True);
                        if (cleanupFailure != null && failure == null)
                            Assert.Fail(cleanupFailure);
                    }
                }
            });
        }

        private static async UniTask AwaitWithTimeout(UniTask task, string label)
        {
            try
            {
                await task.Timeout(TimeSpan.FromSeconds(20), DelayType.Realtime);
            }
            catch (TimeoutException ex)
            {
                throw new AssertionException(label + " timed out: " + ex.Message);
            }
        }

        private static async UniTask WaitForConditionAsync(
            Func<bool> condition, int maxFrames, string label)
        {
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (condition())
                    return;
                await UniTask.Yield();
            }
            Assert.Fail(label + " within " + maxFrames + " frames.");
        }

        private static async UniTask DrainFormalAudioAsync(
            NTSDSoundPlayer player, int maxFrames, string label)
        {
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (player == null ||
                    (player.PendingFormalAudioLoadsForDiagnostics == 0 &&
                     !player.BattleAudioPreparationInProgressForDiagnostics))
                    return;
                await UniTask.Yield();
            }
            Assert.Fail(label + " within " + maxFrames + " frames.");
        }

        private static void AssertShutdownReleased(FormalAudioSessionFixture fixture,
            OwnedAudioReleaseSnapshot snapshot = null)
        {
            snapshot?.AssertReleased();
            Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics,
                Is.Zero);
            Assert.That(fixture.Player.OwnedFormalAudioSourcesForDiagnostics,
                Is.Zero);
            Assert.That(fixture.Player.BattleAudioPreparationInProgressForDiagnostics,
                Is.False);
            Assert.That(fixture.Player.BattleAudioTailDrainedForDiagnostics,
                Is.True);
            Assert.That(fixture.Player.BattleSoundShutdownCompletedForDiagnostics,
                Is.True);
        }

        private sealed class OwnedAudioReleaseSnapshot
        {
            internal string Label;
            internal AudioClip[] Clips;
            internal int[] Ids;
            internal AudioSource[] Voices;

            internal void AssertReleased()
            {
                Assert.That(Clips.All(c => c == null), Is.True, Label + " native clips");
                Assert.That(Voices.All(v => v == null || v.clip == null), Is.True, Label + " voice unbind");
            }

            internal object ToEvidence() => new
            {
                Label, Ids, trackedOwnedCount = Clips.Length,
                nativeAliveAfterStage5 = Clips.Count(c => c != null),
                boundVoicesAfterStage5 = Voices.Count(v => v != null && v.clip != null)
            };
        }

        private static OwnedAudioReleaseSnapshot CaptureOwnedAudioSnapshot(NTSDSoundPlayer player, string label)
        {
            var outputs = ReadPrivateField<List<AudioClip>>(player, "ownedBattlePlaybackClips");
            var sources = ReadPrivateField<List<AudioClip>>(player, "ownedFormalAudioSources");
            AudioClip[] clips = outputs.Concat(sources).Where(c => c != null).Distinct().ToArray();
            return new OwnedAudioReleaseSnapshot
            {
                Label = label, Clips = clips, Ids = clips.Select(c => c.GetInstanceID()).ToArray(),
                Voices = player.GetComponentsInChildren<AudioSource>(true)
            };
        }

        private static void ResetAudioItemThrottle(object cue)
        {
            AudioItem audioItem = cue.GetType().GetField(
                "AudioItem", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(cue) as AudioItem;
            Assert.That(audioItem, Is.Not.Null);
            audioItem.minTimeBetweenCall = 0f;
            audioItem.lastTimePlayed = -1000f;
        }

        private static AudioSource FindVoiceWithClip(GameObject host, AudioClip clip)
        {
            if (host == null || clip == null)
                return null;
            foreach (AudioSource voice in host.GetComponentsInChildren<AudioSource>(true))
            {
                if (voice != null && voice.clip == clip)
                    return voice;
            }
            return null;
        }

        private static object DescribeCue(
            FormalAudioSessionFixture fixture, string cueId, object cue)
        {
            string sourcePath = fixture.ReadCue<string>(cue, "SourcePath");
            AudioClip owned = fixture.ReadCue<AudioClip>(cue, "OwnedSource");
            AudioClip[] clips = fixture.ReadCue<AudioClip[]>(cue, "Clips");
            AudioClip[] playback = fixture.ReadCue<AudioClip[]>(
                cue, "BattlePlaybackClips");
            return new
            {
                cue = cueId,
                formalIdentity = fixture.ReadCue<string>(cue, "FormalIdentity"),
                catalogGeneration = fixture.ReadCue<int>(cue, "CatalogGeneration"),
                sourcePath,
                sourceExists = File.Exists(sourcePath),
                sourceFile = File.Exists(sourcePath) ? ReadWaveMetadata(sourcePath) : null,
                isLoaded = fixture.ReadCue<bool>(cue, "IsLoaded"),
                ownedSource = DescribeClip(owned),
                clips = DescribeClips(clips),
                battlePlaybackClips = DescribeClips(playback),
                clipsSameAsBattlePlayback = ReferenceEquals(clips, playback),
                ownedSourceSameAsSourceArray = owned != null && clips != null &&
                    clips.Length > 0 && ReferenceEquals(owned, clips[0]),
            };
        }

        private static object[] DescribeClips(AudioClip[] clips)
        {
            if (clips == null)
                return null;
            return clips.Select(DescribeClip).ToArray();
        }

        private static object DescribeClip(AudioClip clip)
        {
            if (clip == null)
                return null;
            return new
            {
                instanceId = clip.GetInstanceID(),
                name = clip.name,
                samples = clip.samples,
                channels = clip.channels,
                frequency = clip.frequency,
                runtimeBytes = Profiler.GetRuntimeMemorySizeLong(clip),
            };
        }

        private static WaveMetadata ReadWaveMetadata(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("RIFF"), path);
            reader.ReadUInt32();
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("WAVE"), path);
            int channels = 0;
            int frequency = 0;
            int blockAlign = 0;
            int dataBytes = 0;
            while (stream.Position + 8 <= stream.Length)
            {
                string chunk = new string(reader.ReadChars(4));
                uint size = reader.ReadUInt32();
                long end = stream.Position + size;
                if (chunk == "fmt ")
                {
                    Assert.That(reader.ReadUInt16(), Is.EqualTo(1), path);
                    channels = reader.ReadUInt16();
                    frequency = reader.ReadInt32();
                    reader.ReadUInt32();
                    blockAlign = reader.ReadUInt16();
                }
                else if (chunk == "data")
                {
                    dataBytes = checked((int)size);
                }
                stream.Position = end + (size & 1);
            }
            Assert.That(blockAlign, Is.GreaterThan(0), path);
            return new WaveMetadata
            {
                Channels = channels,
                Frequency = frequency,
                Samples = dataBytes / blockAlign,
            };
        }

        private static T ReadPrivateField<T>(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(name, Private);
            Assert.That(field, Is.Not.Null, name);
            return (T)field.GetValue(target);
        }

        private static void WriteH10Evidence(string stem, object payload)
        {
            const string sessionOutput =
                "artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010";
            Directory.CreateDirectory(sessionOutput);
            string output = Path.Combine(
                sessionOutput,
                stem + "-" + Guid.NewGuid().ToString("N") + ".json");
            using var writer = new StreamWriter(new FileStream(
                output, FileMode.CreateNew, FileAccess.Write));
            writer.Write(JsonConvert.SerializeObject(payload, Formatting.Indented));
            TestContext.WriteLine(output);
        }

        private sealed class WaveMetadata
        {
            public int Channels;
            public int Frequency;
            public int Samples;
        }

        private sealed class FormalAudioSessionFixture : IDisposable
        {
            private const string GameConfigPath =
                "Assets/NTSD/Config/GameConfig/GameConfig.asset";
            private static readonly string[] FixedFormalCueIds =
            {
                "c/jira/w/e3.wav",
                "c/kar/w/t.wav",
                "c/dan/w/t.wav",
                "data/001.wav",
                "c/kim/w/a1.wav",
            };

            private readonly FieldInfo configInstance;
            private readonly FieldInfo soundRootField;
            private readonly string previousSoundRoot;
            private readonly MethodInfo beginShutdown;
            private readonly MethodInfo completeShutdown;
            private readonly MethodInfo loadPreparedCue;
            private readonly MethodInfo getCue;
            private readonly Type cueType;
            private readonly FieldInfo builtInIdsField;
            private readonly FieldInfo singletonInstanceField;
            private readonly int[] sceneSoundPlayersBefore;
            private readonly int[] sceneManagersBefore;
            private readonly int[] sceneMMSoundManagersBefore;
            private bool disposed;

            public readonly GameConfig Config;
            public readonly object PreviousConfig;
            public readonly object PreviousAudioController;
            public readonly object PreviousMMSoundManager;
            public readonly IDictionary GlobalCache;
            public readonly Dictionary<string, object> CacheBefore;
            public readonly int LoaderPendingRequestsBefore;
            public readonly int LoaderQueuedTasksBefore;
            public readonly GameObject Host;
            public readonly CharacterAnimtorManager Manager;
            public readonly NTSDSoundPlayer Player;
            public readonly string GenericRoot;
            public readonly string GenericRootPath;
            public readonly string[] FixedCueIds = FixedFormalCueIds.ToArray();
            public readonly string[] CollectedCueIds;
            public readonly string[] BuiltInIds;
            public readonly int ExpectedUniqueFormalCount;
            public readonly int ExpectedBuiltInFormalCount;
            public bool CacheRestored { get; private set; }
            public bool ConfigRestored { get; private set; }
            public bool SceneRestored { get; private set; }
            public bool AudioControllerRestored { get; private set; }
            public bool MMSoundManagerRestored { get; private set; }
            public bool LoaderPendingRequestsRestored { get; private set; }
            public bool LoaderQueuedTasksRestored { get; private set; }

            public FormalAudioSessionFixture(
                string name, bool active, bool includeManager)
            {
                configInstance = typeof(GameConfig).GetField(
                    "_instance", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(configInstance, Is.Not.Null);
                Config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(
                    GameConfigPath);
                Assert.That(Config, Is.Not.Null, GameConfigPath);
                PreviousConfig = configInstance.GetValue(null);
                PreviousAudioController = AudioController.Instance;
                PreviousMMSoundManager =
                    MoreMountains.Tools.MMSoundManager.Current;
                singletonInstanceField = typeof(MoreMountains.Tools.MMPersistentSingleton<
                    MoreMountains.Tools.MMSoundManager>).GetField(
                        "_instance",
                        BindingFlags.Static | BindingFlags.NonPublic);
                sceneSoundPlayersBefore = CaptureSceneIds<NTSDSoundPlayer>();
                sceneManagersBefore = CaptureSceneIds<CharacterAnimtorManager>();
                sceneMMSoundManagersBefore = CaptureSceneIds<
                    MoreMountains.Tools.MMSoundManager>();
                GlobalCache = (IDictionary)typeof(NTSD_ResourceLoader)
                    .GetField("cache", Private)
                    .GetValue(NTSD_ResourceLoader.Instance);
                CacheBefore = SnapshotCache(GlobalCache);
                LoaderPendingRequestsBefore =
                    NTSD_ResourceLoader.Instance.GetPendingRequestCount();
                LoaderQueuedTasksBefore =
                    NTSD_ResourceLoader.Instance.GetQueuedTaskCount();
                configInstance.SetValue(null, Config);

                Host = new GameObject(name)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Host.SetActive(false);
                try
                {
                    Manager = null;
                    if (includeManager)
                    {
                        Manager = Host.AddComponent<CharacterAnimtorManager>();
                        BuildMinimalManagerCatalog(Manager);
                    }

                    Player = Host.AddComponent<NTSDSoundPlayer>();
                    Player.runInEditMode = true;
                    soundRootField = typeof(NTSDSoundPlayer).GetField(
                        "soundRootFolder", Private);
                    Assert.That(soundRootField, Is.Not.Null);
                    previousSoundRoot = (string)soundRootField.GetValue(Player);
                    GenericRoot = "H10_Diagnostic_NeverCreate_" +
                        Guid.NewGuid().ToString("N");
                    soundRootField.SetValue(Player, GenericRoot);
                    GenericRootPath = Path.Combine(Application.dataPath, GenericRoot);
                    Assert.That(Directory.Exists(GenericRootPath), Is.False);

                    cueType = typeof(NTSDSoundPlayer).GetNestedType(
                        "PreparedSoundCue", BindingFlags.NonPublic);
                    Assert.That(cueType, Is.Not.Null);
                    getCue = RequireProductionMethod("GetOrPrepareCue", 2);
                    loadPreparedCue = RequireProductionMethod(
                        "LoadPreparedClipsAsync", 1);
                    beginShutdown = RequireProductionMethod(
                        "BeginBattleSoundShutdown", 0);
                    completeShutdown = RequireProductionMethod(
                        "CompleteBattleSoundShutdown", 0);
                    builtInIdsField = typeof(NTSDSoundPlayer).GetField(
                        "BuiltInBattleSoundIds",
                        BindingFlags.Static | BindingFlags.NonPublic);
                    Assert.That(builtInIdsField, Is.Not.Null);
                    BuiltInIds = ((string[])builtInIdsField.GetValue(null)).ToArray();
                    if (active)
                        Host.SetActive(true);

                    if (Manager != null)
                    {
                        var collected = new HashSet<string>(StringComparer.Ordinal);
                        Manager.CollectBattleSoundIds(collected);
                        CollectedCueIds = collected.OrderBy(
                            cue => cue, StringComparer.Ordinal).ToArray();
                    }
                    else
                    {
                        CollectedCueIds = Array.Empty<string>();
                    }

                    var allFormalIds = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);
                    if (Manager != null)
                    {
                        foreach (string cue in CollectedCueIds)
                            AddExistingFormalIdentity(allFormalIds, cue);
                    }
                    foreach (string builtIn in BuiltInIds)
                        AddExistingFormalIdentity(allFormalIds, builtIn);
                    ExpectedUniqueFormalCount = allFormalIds.Count;

                    var builtInFormalIds = new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);
                    foreach (string builtIn in BuiltInIds)
                        AddExistingFormalIdentity(builtInFormalIds, builtIn);
                    ExpectedBuiltInFormalCount = builtInFormalIds.Count;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public object GetBattleCue(string soundId)
            {
                object cue = getCue.Invoke(Player, new object[] { soundId, true });
                Assert.That(cue, Is.Not.Null, soundId);
                return cue;
            }

            public object GetGenericCue(string soundId)
            {
                return getCue.Invoke(Player, new object[] { soundId, false });
            }

            public UniTask LoadCue(object cue)
            {
                return (UniTask)loadPreparedCue.Invoke(Player, new[] { cue });
            }

            public T ReadCue<T>(object cue, string fieldName)
            {
                FieldInfo field = cueType.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, fieldName);
                return (T)field.GetValue(cue);
            }

            public void BeginShutdown()
            {
                if (Player != null)
                    beginShutdown.Invoke(Player, null);
            }

            public void CompleteShutdown()
            {
                if (Player != null)
                    completeShutdown.Invoke(Player, null);
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                if (Player != null)
                {
                    if (soundRootField != null)
                        soundRootField.SetValue(Player, previousSoundRoot);
                    if (completeShutdown != null)
                        completeShutdown.Invoke(Player, null);
                }
                if (Host != null)
                    UnityEngine.Object.DestroyImmediate(Host);
                RestoreMMSoundManager();
                configInstance.SetValue(null, PreviousConfig);
                CacheRestored = CacheMatches(GlobalCache, CacheBefore);
                ConfigRestored = ReferenceEquals(
                    configInstance.GetValue(null), PreviousConfig);
                SceneRestored = ArraysEqual(
                    sceneSoundPlayersBefore, CaptureSceneIds<NTSDSoundPlayer>()) &&
                    ArraysEqual(
                        sceneManagersBefore,
                        CaptureSceneIds<CharacterAnimtorManager>()) &&
                    ArraysEqual(
                        sceneMMSoundManagersBefore,
                        CaptureSceneIds<MoreMountains.Tools.MMSoundManager>());
                AudioControllerRestored = ReferenceEquals(
                    AudioController.Instance, PreviousAudioController);
                MMSoundManagerRestored = ReferenceEquals(
                    MoreMountains.Tools.MMSoundManager.Current,
                    PreviousMMSoundManager);
                LoaderPendingRequestsRestored =
                    NTSD_ResourceLoader.Instance.GetPendingRequestCount() ==
                    LoaderPendingRequestsBefore;
                LoaderQueuedTasksRestored =
                    NTSD_ResourceLoader.Instance.GetQueuedTaskCount() ==
                    LoaderQueuedTasksBefore;
            }

            private void BuildMinimalManagerCatalog(
                CharacterAnimtorManager manager)
            {
                var frame = new LF2FrameData();
                typeof(LF2FrameData).GetMethod(
                    "SealFrameSounds",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(frame, new object[] { FixedCueIds });
                var data = new LF2CharacterData
                {
                    frames = new List<LF2FrameData> { frame },
                };
                var catalog = new Dictionary<int, LF2CharacterDataWrapper>
                {
                    [2710] = new LF2CharacterDataWrapper(2710, data),
                };
                typeof(CharacterAnimtorManager).GetField(
                    "TotalCharacterFrameConfig", Private)
                    .SetValue(manager, catalog);
            }

            private void AddExistingFormalIdentity(
                ISet<string> identities, string soundId)
            {
                string normalized = NormalizeFormalIdentity(soundId);
                if (string.IsNullOrEmpty(normalized))
                    return;
                string path = Path.GetFullPath(Path.Combine(
                    Application.dataPath,
                    "..",
                    Config.BattleContentRuntimeRoot,
                    "vfs",
                    normalized.Replace('/', Path.DirectorySeparatorChar)));
                if (File.Exists(path))
                    identities.Add(normalized);
            }

            private static string NormalizeFormalIdentity(string soundId)
            {
                if (string.IsNullOrWhiteSpace(soundId))
                    return null;
                string normalized = soundId.Replace('\\', '/').Trim('/');
                if (normalized.Length == 7 &&
                    normalized.StartsWith("SFX_", StringComparison.Ordinal) &&
                    normalized[4] >= '0' && normalized[4] <= '9' &&
                    normalized[5] >= '0' && normalized[5] <= '9' &&
                    normalized[6] >= '0' && normalized[6] <= '9')
                {
                    return "data/" + normalized.Substring(4) + ".wav";
                }
                return normalized;
            }

            private void RestoreMMSoundManager()
            {
                object current = MoreMountains.Tools.MMSoundManager.Current;
                if (!ReferenceEquals(current, PreviousMMSoundManager))
                {
                    if (PreviousMMSoundManager == null && current is Component currentObject &&
                        currentObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(currentObject.gameObject);
                    }
                    if (singletonInstanceField != null)
                        singletonInstanceField.SetValue(null, PreviousMMSoundManager);
                }
            }

            private static int[] CaptureSceneIds<T>() where T : UnityEngine.Object
            {
                return UnityEngine.Object.FindObjectsOfType<T>(true)
                    .Where(item => item != null)
                    .Select(item => item.GetInstanceID())
                    .OrderBy(id => id)
                    .ToArray();
            }

            private static bool ArraysEqual(int[] left, int[] right)
            {
                return left.SequenceEqual(right);
            }
        }

        private const string SessionStageOutput =
            "artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010";

        private sealed class RetainedStageCue
        {
            public bool IsLoaded = true;
            public AudioClip[] Clips;
            public AudioClip[] BattlePlaybackClips;
        }

        private sealed class RetainedStageOwner
        {
            private readonly List<AudioClip> ownedBattlePlaybackClips = new List<AudioClip>();
            public AudioClip Convert(AudioClip source)
            {
                var cue = new RetainedStageCue { Clips = new[] { source } };
                PrepareBattlePlaybackClips(cue);
                return cue.BattlePlaybackClips[0];
            }

            public void Release()
            {
                foreach (AudioClip clip in ownedBattlePlaybackClips)
                    if (clip != null) UnityEngine.Object.DestroyImmediate(clip);
                ownedBattlePlaybackClips.Clear();
            }

        private void PrepareBattlePlaybackClips(RetainedStageCue cue)
        {
            if (this == null || cue == null || !cue.IsLoaded || cue.BattlePlaybackClips != null)
                return;
            AudioClip[] sourceClips = cue.Clips ?? Array.Empty<AudioClip>();
            var playbackClips = new AudioClip[sourceClips.Length];
            int firstOwned = ownedBattlePlaybackClips.Count;
            try
            {
                for (int index = 0; index < sourceClips.Length; index++)
                {
                    AudioClip source = sourceClips[index];
                    if (source == null || source.channels != 1)
                    {
                        playbackClips[index] = source;
                        continue;
                    }

                    if (source.samples >= 8192)
                    {
                        playbackClips[index] = CreateBoundedBattlePlaybackClip(source);
                        continue;
                    }

                    // Unity's mono pan law attenuates the center and caps volume at1.
                    // Duplicate PCM into stereo to preserve native per-channel gain.
                    var mono = new float[source.samples];
                    if (!source.GetData(mono, 0))
                        throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
                    var stereo = new float[checked(source.samples * 2)];
                    for (int sample = 0; sample < mono.Length; sample++)
                        stereo[sample * 2] = stereo[sample * 2 + 1] = mono[sample];
                    AudioClip playback = AudioClip.Create(source.name, source.samples, 2, source.frequency, false);
                    ownedBattlePlaybackClips.Add(playback);
                    if (!playback.SetData(stereo, 0))
                        throw new InvalidOperationException("Cannot initialize battle PCM: " + source.name);
                    playbackClips[index] = playback;
                }
                cue.BattlePlaybackClips = playbackClips;
            }
            catch
            {
                for (int index = ownedBattlePlaybackClips.Count - 1; index >= firstOwned; index--)
                {
                    AudioClip clip = ownedBattlePlaybackClips[index];
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(clip);
                    else
                        UnityEngine.Object.DestroyImmediate(clip);
                    ownedBattlePlaybackClips.RemoveAt(index);
                }
                throw;
            }
        }

        private AudioClip CreateBoundedBattlePlaybackClip(AudioClip source)
        {
            const int blockFrames = 4096;
            int totalFrames = source.samples;
            var mono = new float[blockFrames];
            if (!source.GetData(mono, 0))
                throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
            _ = checked(totalFrames * 2);
            var stereo = new float[blockFrames * 2];
            AudioClip playback = AudioClip.Create(source.name, totalFrames, 2, source.frequency, false);
            ownedBattlePlaybackClips.Add(playback);
            int offset = 0;
            while (true)
            {
                for (int sample = 0; sample < mono.Length; sample++)
                    stereo[sample * 2] = stereo[sample * 2 + 1] = mono[sample];
                if (!playback.SetData(stereo, offset))
                    throw new InvalidOperationException("Cannot initialize battle PCM: " + source.name);
                offset += mono.Length;
                if (offset == totalFrames)
                    return playback;
                int remaining = totalFrames - offset;
                if (remaining < blockFrames)
                {
                    // Exact tail buffers prevent Unity SetData from wrapping over the clip's beginning.
                    mono = new float[remaining];
                    stereo = new float[remaining * 2];
                }
                if (!source.GetData(mono, offset))
                    throw new InvalidOperationException("Cannot prepare battle PCM: " + source.name);
            }
        }


        }

        private static object StageResourceSnapshot(string stage, List<AudioClip> sources,
            List<AudioClip> outputs, int pending, long temporaryFloatPayload)
        {
            AudioClip[] liveSources = sources.Where(c => c != null).ToArray();
            AudioClip[] liveOutputs = outputs.Where(c => c != null).ToArray();
            AudioClip[] unique = liveSources.Concat(liveOutputs)
                .GroupBy(c => c.GetInstanceID()).Select(g => g.First()).ToArray();
            return new
            {
                stage,
                sourceIds = liveSources.Select(c => c.GetInstanceID()).ToArray(),
                outputIds = liveOutputs.Select(c => c.GetInstanceID()).ToArray(),
                sourceObjectBytes = liveSources.Sum(c => Profiler.GetRuntimeMemorySizeLong(c)),
                outputObjectBytes = liveOutputs.Sum(c => Profiler.GetRuntimeMemorySizeLong(c)),
                distinctObjectBytes = unique.Sum(c => Profiler.GetRuntimeMemorySizeLong(c)),
                temporaryFloatPayload,
                pendingLocalLoads = pending,
                destroyRequestedButAliveCount = 0,
                scope = "EditMode immediate destruction, actual native clip objects; float payload analytic, not RSS/decode peak"
            };
        }

        private static string StagePcmHash(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            Assert.That(clip.GetData(samples, 0), Is.True);
            var bytes = new byte[samples.Length * sizeof(float)];
            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
            using var sha = System.Security.Cryptography.SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        [UnityTest]
        public IEnumerator FormalAudioSessionBoundedResourceStages()
        {
            return UniTask.ToCoroutine(async () =>
            {
                Assert.That(Application.isPlaying, Is.False);
                Directory.CreateDirectory(SessionStageOutput);
                using (var marker = new FileStream(Path.Combine(SessionStageOutput,
                    "bounded-resource-group.started"), FileMode.CreateNew, FileAccess.Write))
                {
                    byte[] note = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));
                    marker.Write(note, 0, note.Length);
                }
                string[] paths = { "c/jira/w/e3.wav", "c/kar/w/t.wav", "c/dan/w/t.wav",
                    "data/001.wav", "c/kim/w/a1.wav" };
                var instance = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previousConfig = instance.GetValue(null);
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(
                    "Assets/NTSD/Config/GameConfig/GameConfig.asset");
                var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private)
                    .GetValue(NTSD_ResourceLoader.Instance);
                var previousCache = cache.Keys.Cast<string>().ToDictionary(k => k, k => cache[k]);
                var rows = new List<object>();
                var expectedHashes = new Dictionary<string, string>();
                var expectedMetadata = new Dictionary<string, string>();
                var allOwned = new List<AudioClip>();
                var allIds = new List<int>();
                try
                {
                    instance.SetValue(null, config);
                    for (int group = 0; group < 2; group++)
                    {
                        Assert.That(allOwned.All(c => c == null), Is.True, "groups cannot co-reside");
                        bool candidate = group == 1;
                        var host = new GameObject("H10StageMeasurement") { hideFlags = HideFlags.HideAndDontSave };
                        host.SetActive(false);
                        NTSDSoundPlayer player = host.AddComponent<NTSDSoundPlayer>();
                        var frozen = new RetainedStageOwner();
                        var loader = (NTSD_ResourceLoader)Activator.CreateInstance(typeof(NTSD_ResourceLoader), true);
                        var sources = new List<AudioClip>();
                        var outputs = new List<AudioClip>();
                        var stages = new List<object>();
                        var cues = new List<object>();
                        try
                        {
                            stages.Add(StageResourceSnapshot("before", sources, outputs, 0, 0));
                            foreach (string relative in paths)
                            {
                                string full = Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/vfs/" + relative);
                                object cue = null;
                                AudioClip source;
                                var clock = new System.Diagnostics.Stopwatch();
                                if (candidate)
                                {
                                    cue = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue", Private)
                                        .Invoke(player, new object[] { relative, true });
                                    Assert.That(cue, Is.Not.Null);
                                    clock.Start();
                                    await (UniTask)typeof(NTSDSoundPlayer).GetMethod("EnsurePreparedCueLoadedAsync", Private)
                                        .Invoke(player, new[] { cue });
                                    clock.Stop();
                                    source = ((AudioClip[])cue.GetType().GetField("Clips").GetValue(cue))[0];
                                }
                                else
                                {
                                    clock.Start();
                                    source = await loader.LoadSingleAudioClipAsync(
                                        "NTSD.FormalBattleAudioFile::" + relative, full);
                                    clock.Stop();
                                }
                                Assert.That(source, Is.Not.Null);
                                int sourceId = source.GetInstanceID();
                                int frames = source.samples, channels = source.channels, hz = source.frequency;
                                double loadElapsedMs = clock.Elapsed.TotalMilliseconds;
                                long sourceObjectBytes = Profiler.GetRuntimeMemorySizeLong(source);
                                sources.Add(source); allOwned.Add(source); allIds.Add(sourceId);
                                long temp = channels != 1 ? 0 : frames < 8192 ? frames * 12L :
                                    (4096L + frames % 4096) * 12L;
                                stages.Add(StageResourceSnapshot("loaded:" + relative, sources, outputs, 0, 0));
                                clock.Restart();
                                AudioClip playback;
                                if (candidate)
                                {
                                    typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips", Private)
                                        .Invoke(player, new[] { cue });
                                    clock.Stop();
                                    playback = ((AudioClip[])cue.GetType().GetField("BattlePlaybackClips").GetValue(cue))[0];
                                }
                                else
                                {
                                    playback = frozen.Convert(source);
                                    clock.Stop();
                                }
                                double conversionElapsedMs = clock.Elapsed.TotalMilliseconds;
                                Assert.That(playback, Is.Not.Null);
                                outputs.Add(playback); allOwned.Add(playback); allIds.Add(playback.GetInstanceID());
                                string hash = StagePcmHash(playback);
                                string metadata = playback.name + ":" + playback.samples + ":" + playback.channels + ":" + playback.frequency;
                                if (candidate)
                                {
                                    Assert.That(hash, Is.EqualTo(expectedHashes[relative]));
                                    Assert.That(metadata, Is.EqualTo(expectedMetadata[relative]));
                                    Assert.That(source == null, Is.EqualTo(channels == 1));
                                }
                                else
                                {
                                    expectedHashes.Add(relative, hash);
                                    expectedMetadata.Add(relative, metadata);
                                }
                                stages.Add(StageResourceSnapshot("converted:" + relative, sources, outputs, 0, temp));
                                cues.Add(new { relative, sourceId, outputId = playback.GetInstanceID(), frames, channels, hz,
                                    sourceObjectBytes, outputObjectBytes = Profiler.GetRuntimeMemorySizeLong(playback),
                                    tempFloatPayload = temp, hash, metadata, loadElapsedMs, conversionElapsedMs,
                                    sourceRetired = source == null });
                            }
                            stages.Add(StageResourceSnapshot("steady", sources, outputs, 0, 0));
                            if (candidate)
                            {
                                typeof(NTSDSoundPlayer).GetMethod("BeginBattleSoundShutdown", Private).Invoke(player, null);
                                typeof(NTSDSoundPlayer).GetMethod("CompleteBattleSoundShutdown", Private).Invoke(player, null);
                            }
                            else
                            {
                                frozen.Release();
                                foreach (AudioClip source in sources)
                                    if (source != null) UnityEngine.Object.DestroyImmediate(source);
                            }
                            stages.Add(StageResourceSnapshot("released-and-drained", sources, outputs, 0, 0));
                            Assert.That(sources.All(c => c == null) && outputs.All(c => c == null), Is.True);
                            rows.Add(new { group = candidate ? "actual-production" : "frozen-retained-formal",
                                cues, stages, ownedReleased = true });
                        }
                        finally
                        {
                            frozen.Release();
                            typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Private).Invoke(player, null);
                            foreach (AudioClip c in sources) if (c != null) UnityEngine.Object.DestroyImmediate(c);
                            UnityEngine.Object.DestroyImmediate(host);
                        }
                    }
                    Assert.That(cache.Count, Is.EqualTo(previousCache.Count));
                    foreach (var entry in previousCache) Assert.That(cache[entry.Key], Is.SameAs(entry.Value));
                    Assert.That(allOwned.All(c => c == null), Is.True);
                    string file = Path.Combine(SessionStageOutput, "bounded-resource-stages.json");
                    using var writer = new StreamWriter(new FileStream(file, FileMode.CreateNew, FileAccess.Write));
                    writer.Write(JsonConvert.SerializeObject(new { rows, allResourceIds = allIds,
                        globalCacheUnchanged = true, finalOwnedZero = true,
                        scope = "One fixed serial five-file resource group. No cost ABBA, full-catalog decode, Driver GC, RSS, device or full H10 certification." },
                        Formatting.Indented));
                    TestContext.WriteLine(file);
                }
                finally { instance.SetValue(null, previousConfig); }
            });
        }


        [UnityTest]
        public IEnumerator FormalAudioDriverCapturesBothPreparationOrdersAndNoDriverShutdown()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var appSingleton = typeof(AppManager).BaseType.GetField("<Instance>k__BackingField",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var driverSingleton = typeof(NTSD.Simulation.SimulationTickDriver).BaseType
                    .GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
                var configSingleton = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object oldApp = appSingleton.GetValue(null), oldDriver = driverSingleton.GetValue(null),
                    oldConfig = configSingleton.GetValue(null);
                var rows = new List<object>();
                try
                {
                    configSingleton.SetValue(null, UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(
                        "Assets/NTSD/Config/GameConfig/GameConfig.asset"));
                    for (int order = 0; order < 2; order++)
                    {
                        appSingleton.SetValue(null, null);
                        driverSingleton.SetValue(null, null);
                        var appHost = new GameObject("H10CapturedApp") { hideFlags = HideFlags.HideAndDontSave };
                        var driverHost = new GameObject("H10CapturedDriver") { hideFlags = HideFlags.HideAndDontSave };
                        appHost.SetActive(false); driverHost.SetActive(false);
                        var app = appHost.AddComponent<AppManager>();
                        var driver = driverHost.AddComponent<NTSD.Simulation.SimulationTickDriver>();
                        var appModules = typeof(AppManager).GetMethod("EnsureRuntimeModules", Private);
                        var enter = driver.GetType().GetMethod("EnterPreparingState", Private);
                        var captured = driver.GetType().GetField("_battleSoundPlayer", Private);
                        try
                        {
                            if (order == 0)
                            {
                                appSingleton.SetValue(null, app);
                                appModules.Invoke(app, null);
                                driverSingleton.SetValue(null, driver);
                                enter.Invoke(driver, null);
                            }
                            else
                            {
                                driverSingleton.SetValue(null, driver);
                                enter.Invoke(driver, null);
                                Assert.That(captured.GetValue(driver), Is.Null);
                                appSingleton.SetValue(null, app);
                                appModules.Invoke(app, null);
                            }
                            NTSDSoundPlayer player = app.SoundPlayer;
                            Assert.That(captured.GetValue(driver), Is.SameAs(player));
                            var other = appHost.AddComponent<NTSDSoundPlayer>();
                            Assert.Throws<InvalidOperationException>(() => driver.SetSoundPresentationSinkForDiagnostics(other));
                            Assert.That(captured.GetValue(driver), Is.SameAs(player));
                            driver.SetSoundPresentationSinkForDiagnostics(null);
                            Assert.That(captured.GetValue(driver), Is.SameAs(player));
                            Assert.That(driver.LifecycleState,
                                Is.EqualTo(NTSD.Simulation.BattleRuntimeLifecycleState.Preparing));
                            object cue = RequireProductionMethod("GetOrPrepareCue", 2)
                                .Invoke(player, new object[] { "data/001.wav", true });
                            await (UniTask)RequireProductionMethod("EnsurePreparedCueLoadedAsync", 1)
                                .Invoke(player, new[] { cue });
                            RequireProductionMethod("PrepareBattlePlaybackClips", 1).Invoke(player, new[] { cue });
                            AudioClip output = ((AudioClip[])cue.GetType().GetField("BattlePlaybackClips").GetValue(cue))[0];
                            int outputId = output.GetInstanceID();
                            if (order == 0)
                            {
                                var report = driver.ShutdownBattleRuntime();
                                Assert.That(report.RuntimeStagesCompleted, Is.True);
                                Assert.That(ReadDiagnostic<bool>(player, "BattleSoundShutdownCompletedForDiagnostics"), Is.True);
                                Assert.That(output == null, Is.True);
                                Assert.That(driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true).IsComplete, Is.True);
                                Assert.That(captured.GetValue(driver), Is.Null);
                            }
                            else
                            {
                                // Exercise the existing application fallback with the exact owned player.
                                driverSingleton.SetValue(null, null);
                                Assert.That(app.TryShutdownBattleRuntimeBeforeSceneDestroy(out var report), Is.True);
                                Assert.That(output == null, Is.True);
                                Assert.That(ReadDiagnostic<bool>(player, "BattleSoundShutdownCompletedForDiagnostics"), Is.True);
                            }
                            rows.Add(new { order, capturedBeforeAudioAwait = true, outputId, outputReleased = output == null });
                        }
                        finally
                        {
                            if (app.SoundPlayer != null)
                            {
                                RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(app.SoundPlayer, null);
                                RequireProductionMethod("CompleteBattleSoundShutdown", 0).Invoke(app.SoundPlayer, null);
                            }
                            object input = typeof(AppManager).GetField("inputModule", Private).GetValue(app);
                            if (input != null)
                            {
                                object actions = input.GetType().GetField("_NTSDInputConfig", Private).GetValue(input);
                                if (actions != null)
                                {
                                    var asset = actions.GetType().GetProperty("asset").GetValue(actions) as UnityEngine.Object;
                                    if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
                                }
                            }
                            appSingleton.SetValue(null, null);
                            driverSingleton.SetValue(null, null);
                            UnityEngine.Object.DestroyImmediate(driverHost);
                            UnityEngine.Object.DestroyImmediate(appHost);
                        }
                    }
                    Directory.CreateDirectory(SessionStageOutput);
                    using var writer = new StreamWriter(new FileStream(Path.Combine(SessionStageOutput,
                        "driver-owner-" + Guid.NewGuid().ToString("N") + ".json"), FileMode.CreateNew, FileAccess.Write));
                    writer.Write(JsonConvert.SerializeObject(new { rows }, Formatting.Indented));
                }
                finally
                {
                    configSingleton.SetValue(null, oldConfig);
                    appSingleton.SetValue(null, oldApp);
                    driverSingleton.SetValue(null, oldDriver);
                }
                Assert.That(appSingleton.GetValue(null), Is.SameAs(oldApp));
                Assert.That(driverSingleton.GetValue(null), Is.SameAs(oldDriver));
                Assert.That(configSingleton.GetValue(null), Is.SameAs(oldConfig));
            });
        }

        [UnityTest]
        public IEnumerator FormalAudioMissingFallbackKeepsBorrowedClipAndGenericVoice()
        {
            return UniTask.ToCoroutine(async () =>
            {
                AudioClip borrowed = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/NTSD/Content/LoganRuntime/vfs/c/kim/w/a1.wav");
                Assert.That(borrowed, Is.Not.Null);
                int borrowedId = borrowed.GetInstanceID();
                var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private)
                    .GetValue(NTSD_ResourceLoader.Instance);
                var before = SnapshotCache(cache);
                var host = new GameObject("H10BorrowedFallback") { hideFlags = HideFlags.HideAndDontSave };
                var player = host.AddComponent<NTSDSoundPlayer>();
                player.runInEditMode = true;
                try
                {
                    typeof(NTSDSoundPlayer).GetField("soundRootFolder", Private)
                        .SetValue(player, "H10_Diagnostic_NeverCreate_Borrowed");
                    const string id = "H10_MissingBorrowed.wav";
                    object cue = RequireProductionMethod("GetOrPrepareCue", 2)
                        .Invoke(player, new object[] { id, false });
                    var item = (AudioItem)cue.GetType().GetField("AudioItem").GetValue(cue);
                    item.clip = new[] { borrowed };
                    await (UniTask)RequireProductionMethod("EnsurePreparedCueLoadedAsync", 1)
                        .Invoke(player, new[] { cue });
                    Assert.That(cue.GetType().GetField("OwnedSource").GetValue(cue), Is.Null);
                    Assert.That(((AudioClip[])cue.GetType().GetField("Clips").GetValue(cue))[0], Is.SameAs(borrowed));
                    player.PresentSound(new NTSD.Simulation.PendingSoundEvent(id, 500, 1));
                    item.loop = true;
                    player.PlaySfx(id);
                    var voices = host.GetComponentsInChildren<AudioSource>(true);
                    AudioSource generic = voices.First(v => v.clip == borrowed && v.loop);
                    AudioSource battle = voices.First(v => v.clip == borrowed && !v.loop);
                    long count = ReadDiagnostic<long>(player, "FormalAudioLoadCountForDiagnostics");
                    long builds = player.PreparedCueBuildCountForDiagnostics;
                    RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(player, null);
                    player.PresentSound(new NTSD.Simulation.PendingSoundEvent(id, 500, 2));
                    Assert.That(player.PreparedCueBuildCountForDiagnostics, Is.EqualTo(builds));
                    Assert.That(ReadDiagnostic<long>(player, "FormalAudioLoadCountForDiagnostics"), Is.EqualTo(count));
                    Assert.That(battle.clip, Is.SameAs(borrowed), "stage1 closes intake, stage5 retires");
                    RequireProductionMethod("CompleteBattleSoundShutdown", 0).Invoke(player, null);
                    Assert.That(battle.clip, Is.Null);
                    Assert.That(generic.clip, Is.SameAs(borrowed));
                    Assert.That(generic.loop, Is.True);
                    Assert.That(borrowed != null && borrowed.GetInstanceID() == borrowedId, Is.True);
                    Assert.That(ReadDiagnostic<int>(player, "OwnedFormalAudioSourcesForDiagnostics"), Is.Zero);
                    Assert.That(CacheMatches(cache, before), Is.True);
                }
                finally { UnityEngine.Object.DestroyImmediate(host); }
                Assert.That(borrowed != null && borrowed.GetInstanceID() == borrowedId, Is.True);
            });
        }


        [UnityTest]
        public IEnumerator FormalAudioControllerChangeFailsCurrentPreparationAndAllowsNewGeneration()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var fixture = new FormalAudioSessionFixture("H10ControllerChange", false, true);
                FieldInfo controllerInstance = typeof(AudioController).GetField("<Instance>k__BackingField",
                    BindingFlags.Static | BindingFlags.NonPublic);
                object original = controllerInstance.GetValue(null);
                var host = new GameObject("H10ReplacementController") { hideFlags = HideFlags.HideAndDontSave };
                host.SetActive(false);
                var replacement = host.AddComponent<AudioController>();
                try
                {
                    UniTask first = fixture.Player.PrepareBattleCuesAsync(fixture.Manager);
                    Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics, Is.EqualTo(1),
                        "actual local file await required for controller-change qualification");
                    object oldCue = fixture.GetBattleCue(fixture.FixedCueIds[0]);
                    controllerInstance.SetValue(null, replacement);
                    bool failed = false;
                    try { await AwaitWithTimeout(first, "controller-change current preparation"); }
                    catch (InvalidOperationException error)
                    {
                        Assert.That(error.Message, Does.Contain("without sealing"));
                        failed = true;
                    }
                    Assert.That(failed, Is.True);
                    Assert.That(fixture.Player.BattleCatalogSealedForDiagnostics, Is.False);
                    Assert.That(fixture.Player.BattleSoundShutdownCompletedForDiagnostics, Is.True);
                    Assert.That(fixture.Player.OwnedFormalAudioSourcesForDiagnostics, Is.Zero);
                    Assert.That(fixture.Player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                    int staleId = fixture.Player.LastStaleFormalAudioInstanceIdForDiagnostics;
                    Assert.That(staleId, Is.Not.Zero);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(c => c.GetInstanceID() == staleId), Is.False);
                    await AwaitWithTimeout(fixture.Player.PrepareBattleCuesAsync(fixture.Manager),
                        "new-controller preparation");
                    Assert.That(fixture.Player.BattleCatalogSealedForDiagnostics, Is.True);
                    object newCue = fixture.GetBattleCue(fixture.FixedCueIds[0]);
                    Assert.That(newCue, Is.Not.SameAs(oldCue));
                    WriteH10Evidence("formal-controller-change", new
                    {
                        oldGeneration = fixture.ReadCue<int>(oldCue, "CatalogGeneration"),
                        newGeneration = fixture.ReadCue<int>(newCue, "CatalogGeneration"),
                        oldRequestFailed = failed, staleId, staleNativeGone = true, newSealed = true
                    });
                }
                finally
                {
                    fixture.BeginShutdown();
                    fixture.CompleteShutdown();
                    controllerInstance.SetValue(null, original);
                    UnityEngine.Object.DestroyImmediate(host);
                }
            });
        }


        private const string SessionPlayRestoreKey = "NTSD.H10.FormalAudioSession.PlayRestore";

        [Serializable]
        private sealed class H10SceneSetup
        {
            public UnityEditor.SceneManagement.SceneSetup[] Scenes;
        }

        [UnityTearDown]
        public IEnumerator RestoreOwnedSessionPlayScene()
        {
            if (string.IsNullOrEmpty(UnityEditor.SessionState.GetString(SessionPlayRestoreKey, "")))
                yield break;
            if (Application.isPlaying)
                yield return new ExitPlayMode();
            var setup = JsonConvert.DeserializeObject<H10SceneSetup>(
                UnityEditor.SessionState.GetString(SessionPlayRestoreKey, ""));
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(setup.Scenes);
            UnityEditor.SessionState.EraseString(SessionPlayRestoreKey);
            Assert.That(UnityEngine.SceneManagement.SceneManager.sceneCount, Is.EqualTo(setup.Scenes.Length));
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty, Is.False);
        }

        [UnityTest]
        public IEnumerator FormalAudioOneIsolatedPlayDeferredDestroy()
        {
            Assert.That(Application.isPlaying, Is.False);
            Assert.That(UnityEditor.SessionState.GetString(SessionPlayRestoreKey, ""), Is.Empty);
            var current = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
                "Assets/NTSD/Scene/NTSD_Menu.unity");
            Assert.That(current.IsValid() && current.isLoaded, Is.True);
            Assert.That(current.isDirty, Is.False);
            Assert.That(current.rootCount, Is.EqualTo(8));
            for (int index = 0; index < UnityEngine.SceneManagement.SceneManager.sceneCount; index++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
                if (scene == current)
                    continue;
                // Installed EditModeLauncher adds an active empty, untitled test scene.
                Assert.That(scene.path, Is.Empty);
                Assert.That(scene.rootCount, Is.Zero);
                Assert.That(scene.isDirty, Is.False);
            }
            var editorCache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private)
                .GetValue(NTSD_ResourceLoader.Instance);
            Assert.That(editorCache.Count, Is.Zero, "No existing global cache contents may be discarded by Play reload");
            Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
            Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
            var setup = new H10SceneSetup
            {
                Scenes = new[] { new UnityEditor.SceneManagement.SceneSetup
                {
                    path = current.path, isLoaded = true, isActive = true
                } }
            };
            Directory.CreateDirectory(SessionStageOutput);
            using (var marker = new FileStream(Path.Combine(SessionStageOutput,
                "isolated-play.started"), FileMode.CreateNew, FileAccess.Write))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));
                marker.Write(data, 0, data.Length);
            }
            string setupJson = JsonConvert.SerializeObject(setup);
            using (var writer = new StreamWriter(new FileStream(Path.Combine(SessionStageOutput,
                "isolated-play-scene-before.json"), FileMode.CreateNew, FileAccess.Write)))
                writer.Write(setupJson);
            UnityEditor.SessionState.SetString(SessionPlayRestoreKey, setupJson);
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return UniTask.ToCoroutine(async () =>
            {
                Assert.That(Application.isPlaying, Is.True);
                Assert.That(AppManager.Instance == null, Is.True, "isolated scene must not start Menu/AppManager");
                Assert.That(NTSD.Simulation.SimulationTickDriver.Instance == null, Is.True);
                var configInstance = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previous = configInstance.GetValue(null);
                var host = new GameObject("H10DeferredAudioOnly");
                NTSDSoundPlayer player = host.AddComponent<NTSDSoundPlayer>();
                var all = new List<AudioClip>();
                var rows = new List<object>();
                var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private)
                    .GetValue(NTSD_ResourceLoader.Instance);
                var before = SnapshotCache(cache);
                try
                {
                    configInstance.SetValue(null, UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>(
                        "Assets/NTSD/Config/GameConfig/GameConfig.asset"));
                    foreach (string relative in new[] { "data/001.wav", "c/kim/w/a1.wav" })
                    {
                        object cue = RequireProductionMethod("GetOrPrepareCue", 2)
                            .Invoke(player, new object[] { relative, true });
                        await (UniTask)RequireProductionMethod("EnsurePreparedCueLoadedAsync", 1)
                            .Invoke(player, new[] { cue });
                        AudioClip source = ((AudioClip[])cue.GetType().GetField("Clips").GetValue(cue))[0];
                        int sourceId = source.GetInstanceID();
                        int channels = source.channels;
                        all.Add(source);
                        RequireProductionMethod("PrepareBattlePlaybackClips", 1).Invoke(player, new[] { cue });
                        AudioClip output = ((AudioClip[])cue.GetType().GetField("BattlePlaybackClips").GetValue(cue))[0];
                        int outputId = output.GetInstanceID();
                        all.Add(output);
                        bool sourceAliveWhenDestroyRequested = source != null;
                        await UniTask.NextFrame();
                        Assert.That(source == null, Is.EqualTo(channels == 1));
                        player.PresentSound(new NTSD.Simulation.PendingSoundEvent(relative, 500, 1));
                        rows.Add(new { relative, sourceId, outputId, sourceAliveWhenDestroyRequested,
                            sourceRetiredAfterFrame = source == null, outputAlive = output != null });
                    }
                    long loadsBeforeColdReentry = player.FormalAudioLoadCountForDiagnostics;
                    UniTask coldReentry = player.PrepareBattleCuesAsync(null);
                    Assert.That(player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                    Assert.That(player.FormalAudioLoadCountForDiagnostics, Is.EqualTo(loadsBeforeColdReentry));
                    int aliveAtStage5Return = all.Distinct().Count(c => c != null);
                    // Cancel during the real cold reentry's deferred-destruction wait; no extra builtin files load.
                    RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(player, null);
                    Assert.That(ReadDiagnostic<bool>(player, "BattleSoundShutdownCompletedForDiagnostics"), Is.True);
                    Assert.That(host.GetComponentsInChildren<AudioSource>(true).All(v => v.clip == null), Is.True);
                    await coldReentry;
                    Assert.That(player.FormalAudioLoadCountForDiagnostics, Is.EqualTo(loadsBeforeColdReentry));
                    Assert.That(all.All(c => c == null), Is.True);
                    Assert.That(CacheMatches(cache, before), Is.True);
                    UnityEngine.Object.Destroy(host);
                    await UniTask.NextFrame();
                    Assert.That(host == null && player == null, Is.True);
                    using var writer = new StreamWriter(new FileStream(Path.Combine(SessionStageOutput,
                        "isolated-play-deferred.json"), FileMode.CreateNew, FileAccess.Write));
                    writer.Write(JsonConvert.SerializeObject(new { rows, aliveAtStage5Return,
                        allTrackedOwnedDestroyedAfterFrame = true, voiceBindingsCleared = true,
                        coldReentryIssuedReleaseBeforeWait = true, newLoadBlockedUntilOldDestroy = true,
                        coldReentryCancelledBeforeBuiltinLoads = true,
                        globalCacheUnchanged = true, noAppOrBattleStarted = true }, Formatting.Indented));
                }
                finally
                {
                    configInstance.SetValue(null, previous);
                    if (player != null)
                    {
                        RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(player, null);
                        RequireProductionMethod("CompleteBattleSoundShutdown", 0).Invoke(player, null);
                    }
                    if (host != null) UnityEngine.Object.Destroy(host);
                }
            });
            yield return new ExitPlayMode();
            // UnityTearDown restores the exact saved clean scene setup, including failure paths.
        }



        [UnityTest]
        public IEnumerator FormalAudioNullFallbackAndOwnedConversionFailureKeepBorrowedSource()
        {
            return UniTask.ToCoroutine(async () =>
            {
                using var fixture = new FormalAudioSessionFixture("H10FailureOwnership", false, false);
                object empty = fixture.GetGenericCue("H10_Missing_NoConfiguredClip.wav");
                await fixture.LoadCue(empty);
                Assert.That(fixture.ReadCue<bool>(empty, "IsLoaded"), Is.True);
                Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(empty, "Clips")), Is.True);
                Assert.That(fixture.ReadCue<AudioClip>(empty, "OwnedSource"), Is.Null);
                RequireProductionMethod("PrepareBattlePlaybackClips", 1).Invoke(fixture.Player, new[] { empty });
                Assert.That(fixture.ReadCue<AudioClip[]>(empty, "BattlePlaybackClips"), Is.Empty);

                AudioClip borrowed = AudioClip.Create("H10BorrowedMono", 17, 1, 44100, false);
                AudioClip failing = AudioClip.Create("H10OwnedStreaming", 8193, 1, 44100, true);
                AudioClip retained = null;
                int failingId = failing.GetInstanceID();
                try
                {
                    object prior = fixture.GetGenericCue("H10_Missing_BorrowedMono.wav");
                    var priorItem = (AudioItem)prior.GetType().GetField("AudioItem").GetValue(prior);
                    priorItem.clip = new[] { borrowed };
                    await fixture.LoadCue(prior);
                    RequireProductionMethod("PrepareBattlePlaybackClips", 1).Invoke(fixture.Player, new[] { prior });
                    retained = fixture.ReadCue<AudioClip[]>(prior, "BattlePlaybackClips")[0];
                    Assert.That(retained, Is.Not.SameAs(borrowed));
                    Assert.That(borrowed != null, Is.True);

                    object failedCue = fixture.GetBattleCue("data/001.wav");
                    Type type = failedCue.GetType();
                    type.GetField("Clips").SetValue(failedCue, new[] { failing });
                    type.GetField("IsLoaded").SetValue(failedCue, true);
                    type.GetField("OwnedSource").SetValue(failedCue, failing);
                    ReadPrivateField<List<AudioClip>>(fixture.Player, "ownedFormalAudioSources").Add(failing);
                    LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
                        "(?i)(stream|GetData|sample data)"));
                    var failure = Assert.Throws<TargetInvocationException>(() =>
                        RequireProductionMethod("PrepareBattlePlaybackClips", 1)
                            .Invoke(fixture.Player, new[] { failedCue }));
                    Assert.That(failure.InnerException, Is.TypeOf<InvalidOperationException>());
                    Assert.That(fixture.ReadCue<AudioClip[]>(failedCue, "BattlePlaybackClips"), Is.Null);
                    Assert.That(fixture.ReadCue<bool>(failedCue, "IsLoaded"), Is.False);
                    Assert.That(failing == null, Is.True);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(c => c.GetInstanceID() == failingId), Is.False);
                    Assert.That(retained != null && borrowed != null, Is.True);
                    Assert.That(ReadPrivateField<List<AudioClip>>(fixture.Player, "ownedBattlePlaybackClips"),
                        Has.Count.EqualTo(1));
                    fixture.BeginShutdown();
                    fixture.CompleteShutdown();
                    Assert.That(retained == null, Is.True);
                    Assert.That(borrowed != null, Is.True);
                    WriteH10Evidence("owned-conversion-failure", new
                    {
                        failingId, ownedInputNativeGone = true, earlierPlaybackKeptUntilStage5 = true,
                        borrowedMonoSurvives = true, nullMissingFallbackEmpty = true,
                        scope = "Real Unity streaming GetData failure with explicitly owned test input; not a decoder/async failure injection."
                    });
                }
                finally
                {
                    fixture.BeginShutdown(); fixture.CompleteShutdown();
                    if (failing != null) UnityEngine.Object.DestroyImmediate(failing);
                    if (borrowed != null) UnityEngine.Object.DestroyImmediate(borrowed);
                }
            });
        }


        private const string EnvelopeOutput = "artifacts/diagnostics/NTSD-H10-FULL-CATALOG-AUDIO-ENVELOPE-20261011";
        private static bool envelopeRunning;

        private static void SaveEnvelope(string name, object value)
        {
            Directory.CreateDirectory(EnvelopeOutput);
            using var writer = new StreamWriter(new FileStream(
                Path.Combine(EnvelopeOutput, name + ".json"), FileMode.CreateNew, FileAccess.Write));
            writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static T EnvelopeField<T>(object instance, string name)
        {
            return (T)instance.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance);
        }

        private static string EnvelopeSha(string path)
        {
            using var hash = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(path);
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private sealed class EnvelopeRoute
        {
            public string path, key, identity;
            public bool formal, single, exists;
            public string[] files;
            public int[] configuredBorrowed;
        }

        private sealed class EnvelopeCue
        {
            public string soundId;
            public EnvelopeRoute battle, generic;
        }

        private sealed class EnvelopeFixture
        {
            public readonly GameObject Host;
            public readonly NTSDSoundPlayer Player;
            public readonly CharacterAnimtorManager Manager;
            public readonly LoganVisualContentCandidate Candidate;
            public readonly Dictionary<int, LF2CharacterDataWrapper> Configs;
            public readonly List<EnvelopeCue> Cues = new List<EnvelopeCue>();
            public readonly IDictionary Cache;
            public readonly Dictionary<string, object> BeforeCache;
            public readonly Dictionary<string, object> AddedCache = new Dictionary<string, object>(StringComparer.Ordinal);
            public readonly HashSet<string> AllowedKeys = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<object> Stages = new List<object>();
            private readonly Dictionary<string, int[]> beforeCacheClipIds;
            private readonly HashSet<int> initialClipIds;
            private readonly AudioSource[] oldVoices;
            private readonly int[] oldVoiceClipIds;
            private readonly NTSDSoundPlayer[] oldPlayers;
            private readonly string oldPlayerBorrowSnapshot;
            private readonly object previousConfig;
            private readonly AudioController previousController, boundController;
            private readonly string controllerJson;
            private readonly object previousSoundManager, previousCharacterManager;
            private readonly FieldInfo configField, controllerField, soundManagerField, managerField;
            private readonly string sceneSnapshot;
            private readonly int[] beforeSceneObjects;
            private readonly AudioClip[] configuredClips;
            private bool finished;
            private Newtonsoft.Json.Linq.JObject headers;
            private readonly List<UniTask> operations = new List<UniTask>();
            private MoreMountains.Tools.MMSoundManager resolvedExistingSoundManager;
            private readonly MoreMountains.Tools.MMSoundManager[] beforeSoundManagers;
            private readonly List<string> operationErrors = new List<string>();

            public UniTask Track(UniTask operation)
            {
                UniTask preserved = operation.Preserve();
                operations.Add(preserved);
                RecordResolvedExistingSoundManager(MoreMountains.Tools.MMSoundManager.Current);
                return preserved;
            }

            private bool FullyDrained => operations.All(x => x.Status != UniTaskStatus.Pending)
                && Player.BattleAudioTailDrainedForDiagnostics
                && NTSD_ResourceLoader.Instance.GetPendingRequestCount() == 0
                && NTSD_ResourceLoader.Instance.GetQueuedTaskCount() == 0;

            private async UniTask Drain()
            {
                double deadline = UnityEditor.EditorApplication.timeSinceStartup + 90;
                while (!FullyDrained)
                {
                    Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(deadline),
                        "Unconfirmed async tail: fixture and lock retained; do not run full group");
                    await NTSD_ResourceLoader.Instance.ProcessFrame();
                    await UniTask.Yield();
                }
                foreach (UniTask operation in operations)
                {
                    try { await operation; }
                    catch (Exception error) { operationErrors.Add(error.ToString()); }
                }
                operations.Clear();
            }

            private void RecordResolvedExistingSoundManager(MoreMountains.Tools.MMSoundManager current)
            {
                if (current == null) return;
                Assert.That(beforeSoundManagers.Contains(current)
                    && beforeSceneObjects.Contains(current.gameObject.GetInstanceID()), Is.True,
                    "Unrecognized manager retained; this fixture only borrows original scene managers");
                if (resolvedExistingSoundManager != null)
                    Assert.That(ReferenceEquals(current, resolvedExistingSoundManager), Is.True,
                        "Resolved manager changed; retain foreign pointer and report conflict");
                resolvedExistingSoundManager = current;
            }

            private void RestoreOwnedSoundManager()
            {
                var current = MoreMountains.Tools.MMSoundManager.Current;
                if (!ReferenceEquals(current, previousSoundManager))
                {
                    Assert.That(current != null && ReferenceEquals(current, resolvedExistingSoundManager)
                        && beforeSoundManagers.Contains(current) && beforeSceneObjects.Contains(current.gameObject.GetInstanceID()),
                        Is.True, "Only restore the exact manager this fixture borrowed; preserve all others");
                    soundManagerField.SetValue(null, previousSoundManager);
                }
                Assert.That(ReferenceEquals(MoreMountains.Tools.MMSoundManager.Current, previousSoundManager), Is.True);
            }

            public EnvelopeFixture()
            {
                Assert.That(envelopeRunning, Is.False, "Only one envelope fixture may run");
                Assert.That(Application.isPlaying, Is.False);
                Assert.That(UnityEditor.EditorApplication.isCompiling, Is.False);
                Assert.That(UnityEditor.EditorApplication.isUpdating, Is.False);
                Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
                Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
                var menu = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/NTSD/Scene/NTSD_Menu.unity");
                Assert.That(menu.IsValid() && menu.isLoaded && !menu.isDirty && menu.rootCount == 8, Is.True);
                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                {
                    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                    if (scene == menu) continue;
                    Assert.That(scene.path, Is.Empty);
                    Assert.That(scene.isDirty, Is.False);
                    Assert.That(scene.rootCount, Is.Zero);
                }
                sceneSnapshot = SceneState();
                beforeSceneObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true)
                    .Select(x => x.GetInstanceID()).OrderBy(x => x).ToArray();
                configField = typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                controllerField = typeof(AudioController).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
                soundManagerField = typeof(MoreMountains.Tools.MMPersistentSingleton<MoreMountains.Tools.MMSoundManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                managerField = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                previousConfig = configField.GetValue(null);
                previousController = AudioController.Instance;
                previousSoundManager = MoreMountains.Tools.MMSoundManager.Current;
                beforeSoundManagers = Resources.FindObjectsOfTypeAll<MoreMountains.Tools.MMSoundManager>();
                previousCharacterManager = managerField.GetValue(null);
                oldPlayers = Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>();
                foreach (var player in oldPlayers)
                {
                    Assert.That(player.BattleAudioPreparationInProgressForDiagnostics, Is.False);
                    Assert.That(player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                }
                oldPlayerBorrowSnapshot = PlayerBorrowState(oldPlayers);
                oldVoices = Resources.FindObjectsOfTypeAll<AudioSource>();
                oldVoiceClipIds = oldVoices.Select(x => x.clip != null ? x.clip.GetInstanceID() : 0).ToArray();
                Cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(NTSD_ResourceLoader.Instance);
                BeforeCache = SnapshotCache(Cache);
                beforeCacheClipIds = BeforeCache.ToDictionary(x => x.Key, x => ClipIds(CacheClips(x.Value)));
                initialClipIds = Resources.FindObjectsOfTypeAll<AudioClip>().Select(x => x.GetInstanceID()).ToHashSet();
                var controllers = UnityEngine.Object.FindObjectsOfType<AudioController>(true)
                    .Where(x => x.gameObject.scene.IsValid()).ToArray();
                Assert.That(previousController != null || controllers.Length <= 1, Is.True, "Ambiguous real controller");
                boundController = previousController != null ? previousController : controllers.SingleOrDefault();
                if (boundController != null)
                    Assert.That(boundController.gameObject.scene.IsValid() && boundController.gameObject.scene.isLoaded
                        && controllers.Contains(boundController), Is.True);
                else
                {
                    // Verified actual Battle scene has an empty AudioList; null selects the same existing resolver fallback.
                    var proof = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(EnvelopeOutput, "controller-evidence.json")));
                    Assert.That((int)proof["configuredAudioItems"], Is.Zero);
                    foreach (var property in ((Newtonsoft.Json.Linq.JObject)proof["files"]).Properties())
                        Assert.That(EnvelopeSha(property.Name), Is.EqualTo((string)property.Value));
                }
                controllerJson = boundController != null ? UnityEditor.EditorJsonUtility.ToJson(boundController) : null;
                configuredClips = Resources.FindObjectsOfTypeAll<AudioController>()
                    .SelectMany(x => x.AudioList ?? Array.Empty<AudioItem>())
                    .Where(x => x != null).SelectMany(x => x.clip ?? Array.Empty<AudioClip>())
                    .Where(x => x != null).Distinct().ToArray();
                var config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset");
                Assert.That(config, Is.Not.Null);
                Candidate = LoganVisualContentCandidate.Capture(
                    BattleContentSource.ForLoganRuntime(Path.GetFullPath(config.BattleContentRuntimeRoot)),
                    ProjectBattleModeConfig.LoadDefault().Capture());
                Configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(Candidate.Catalog);
                var definitionsField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private);
                var publicationField = typeof(CharacterAnimtorManager).GetField("publishedLoganCandidate", Private);
                foreach (FieldInfo field in new[] { configField, controllerField, soundManagerField, managerField, definitionsField, publicationField })
                    Assert.That(field, Is.Not.Null);
                // Mutations start only after pure catalog construction/preflight succeed.
                try
                {
                configField.SetValue(null, config);
                controllerField.SetValue(null, boundController);
                envelopeRunning = true;
                Host = new GameObject("H10FullCatalogAudioEnvelope") { hideFlags = HideFlags.HideAndDontSave };
                Host.SetActive(false);
                Manager = Host.AddComponent<CharacterAnimtorManager>();
                Player = Host.AddComponent<NTSDSoundPlayer>();
                Player.runInEditMode = true;
                typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private).SetValue(Manager, Configs);
                publicationField.SetValue(Manager, Candidate);
                }
                catch
                {
                    if (Host != null) UnityEngine.Object.DestroyImmediate(Host);
                    configField.SetValue(null, previousConfig);
                    controllerField.SetValue(null, previousController);
                    envelopeRunning = false;
                    throw;
                }
            }

            public static IEnumerable<AudioClip> CacheClips(object value)
            {
                if (value is AudioClip clip) return new[] { clip };
                if (value is AudioClip[] clips) return clips;
                return Array.Empty<AudioClip>();
            }

            private static int[] ClipIds(IEnumerable<AudioClip> clips)
            {
                return clips.Select(x => x != null ? x.GetInstanceID() : 0).ToArray();
            }

            private static string SceneState()
            {
                return JsonConvert.SerializeObject(Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount)
                    .Select(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i))
                    .Where(s => !string.IsNullOrEmpty(s.path))
                    .Select(s => new { s.path, s.isLoaded, s.isDirty, roots = s.GetRootGameObjects()
                        .Select(x => x.GetInstanceID()).OrderBy(x => x).ToArray() }));
            }

            private static IEnumerable<AudioClip> PlayerBorrowed(NTSDSoundPlayer player)
            {
                foreach (string field in new[] { "preparedCues", "preparedFormalBattleCues" })
                {
                    var map = EnvelopeField<IDictionary>(player, field);
                    foreach (object cue in map.Values)
                    {
                        foreach (AudioClip clip in EnvelopeField<AudioClip[]>(cue, "Clips") ?? Array.Empty<AudioClip>())
                            if (clip != null) yield return clip;
                        foreach (AudioClip clip in EnvelopeField<AudioClip[]>(cue, "BattlePlaybackClips") ?? Array.Empty<AudioClip>())
                            if (clip != null) yield return clip;
                    }
                }
            }

            private static string PlayerBorrowState(IEnumerable<NTSDSoundPlayer> players)
            {
                return JsonConvert.SerializeObject(players.OrderBy(p => p.GetInstanceID())
                    .Select(p => new { id = p.GetInstanceID(), clips = ClipIds(PlayerBorrowed(p)) }));
            }

            public object GetCue(string soundId, bool battle)
            {
                return RequireProductionMethod("GetOrPrepareCue", 2).Invoke(Player, new object[] { soundId, battle });
            }

            private EnvelopeRoute Describe(object cue)
            {
                Assert.That(cue, Is.Not.Null);
                string path = EnvelopeField<string>(cue, "SourcePath");
                bool single = EnvelopeField<bool>(cue, "IsSingleFile");
                bool exists = single ? File.Exists(path) : Directory.Exists(path);
                string[] files = !exists ? Array.Empty<string>() : single ? new[] { path } :
                    Directory.EnumerateFiles(path)
                        .Where(p => (bool)typeof(NTSD_ResourceLoader).GetMethod("IsSupportedAudioFile", Private)
                            .Invoke(NTSD_ResourceLoader.Instance, new object[] { p }))
                        .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
                return new EnvelopeRoute
                {
                    path = path, key = EnvelopeField<string>(cue, "CacheKey"),
                    identity = EnvelopeField<string>(cue, "FormalIdentity"),
                    formal = EnvelopeField<bool>(cue, "IsFormalBattleFile"), single = single, exists = exists,
                    files = files, configuredBorrowed = ClipIds(EnvelopeField<AudioItem>(cue, "AudioItem")?.clip ?? Array.Empty<AudioClip>())
                };
            }

            public object BuildInventory()
            {
                var builtIn = (string[])typeof(NTSDSoundPlayer).GetField("BuiltInBattleSoundIds",
                    BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var feed = Candidate.Catalog.ModeComboInput?.KnockoutFeed;
                string[] mode = new[] { feed?.StageTeam1DeathSoundPath, feed?.StageTeam5DeathSoundPath };
                var perDefinition = new List<object>();
                foreach (var pair in Configs)
                {
                    typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private)
                        .SetValue(Manager, new Dictionary<int, LF2CharacterDataWrapper> { [pair.Key] = pair.Value });
                    var cues = new HashSet<string>(StringComparer.Ordinal);
                    Manager.CollectBattleSoundIds(cues);
                    var data = pair.Value.characterData;
                    perDefinition.Add(new
                    {
                        id = pair.Key, type = Candidate.Catalog.Entries.Single(x => x.Id == pair.Key).Type, includesPublishedModeCues = true,
                        cues = cues.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                        weapon = new[] { data.weapon_hit_sound, data.weapon_drop_sound, data.weapon_broken_sound },
                        frames = data.frames.Where(x => x != null && (!string.IsNullOrWhiteSpace(x.sound) || x.FrameSounds.Count > 0))
                            .Select(x => new { x.frameId, x.UsesLoganFrameNumbers, x.sound, frameSounds = x.FrameSounds.ToArray() }).ToArray()
                    });
                }
                typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private).SetValue(Manager, Configs);
                var all = new HashSet<string>(StringComparer.Ordinal);
                Manager.CollectBattleSoundIds(all);
                foreach (string cue in mode.Where(x => !string.IsNullOrWhiteSpace(x)))
                    Assert.That(all.Contains(cue), Is.True, "Published mode KO must enter real collector");
                all.UnionWith(builtIn);
                foreach (string cue in all.OrderBy(x => x, StringComparer.Ordinal))
                {
                    var row = new EnvelopeCue { soundId = cue, battle = Describe(GetCue(cue, true)), generic = Describe(GetCue(cue, false)) };
                    Cues.Add(row);
                    if (!row.battle.formal) AllowedKeys.Add(row.battle.key);
                    if (row.battle.formal && row.generic.exists) AllowedKeys.Add(row.generic.key);
                }
                var files = Cues.SelectMany(x => x.battle.files.Concat(x.generic.files)).Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(p => new { path = p, bytes = new FileInfo(p).Length, sha256 = EnvelopeSha(p) }).ToArray();
                Candidate.AssertInputsCurrent();
                return new
                {
                    definitionCount = Configs.Count, cueCount = Cues.Count,
                    catalog = Candidate.Catalog.Entries.Select(x => new { x.Id, x.Type, x.SourcePath, x.DatSha256 }).ToArray(),
                    Candidate.Catalog.CatalogSha256, Candidate.Catalog.DefinitionFingerprint,
                    Candidate.Catalog.BattleDefinitionFingerprint,
                    mode, builtIn, perDefinition, routes = Cues, files,
                    formalUniqueIdentities = Cues.Where(x => x.battle.formal).Select(x => x.battle.identity).Distinct().Count(),
                    genericExistingRoutes = Cues.Count(x => x.generic.exists),
                    allowedGenericKeys = AllowedKeys.OrderBy(x => x).ToArray(),
                    actualSoundRoot = EnvelopeField<string>(Player, "soundRootFolder"),
                    actualControllerId = boundController != null ? boundController.GetInstanceID() : 0,
                    controllerWasTemporarilyBound = !ReferenceEquals(boundController, previousController),
                    configuredBorrowed = configuredClips.Select(ClipDescription).ToArray(),
                    originalCache = BeforeCache.Select(x => new { key = x.Key, clipIds = ClipIds(CacheClips(x.Value)) }).ToArray(),
                    originalOtherPlayers = oldPlayerBorrowSnapshot,
                    originalVoiceClipIds = oldVoiceClipIds,
                    baselineEditorAudioClips = initialClipIds.Count,
                    scope = "All formal definitions are current fallback; visual closure is NOT a certified audio closure. No audio/visual decoding in inventory."
                };
            }

            public async UniTask LoadSmall(string formalId, EnvelopeCue generic)
            {
                object first = GetCue(formalId, true);
                Assert.That(EnvelopeField<bool>(first, "IsFormalBattleFile"), Is.True);
                Assert.That(File.Exists(EnvelopeField<string>(first, "SourcePath")), Is.True);
                long started = Player.FormalAudioLoadCountForDiagnostics, failed = Player.FailedPreparedCueLoadCountForDiagnostics;
                RequireProductionMethod("PrepareBattlePresentationHotPath", 0).Invoke(Player, null);
                await Track((UniTask)RequireProductionMethod("EnsurePreparedCueLoadedAsync", 1).Invoke(Player, new[] { first }));
                Assert.That(Player.FormalAudioLoadCountForDiagnostics, Is.EqualTo(started + 1));
                Assert.That(Player.FailedPreparedCueLoadCountForDiagnostics, Is.EqualTo(failed));
                Assert.That(EnvelopeField<AudioClip>(first, "OwnedSource") != null, Is.True);
                RequireProductionMethod("PrepareBattlePlaybackClips", 1).Invoke(Player, new[] { first });
                object second = GetCue(generic.soundId, false);
                Assert.That(EnvelopeField<AudioClip[]>(first, "BattlePlaybackClips").Count(x => x != null), Is.EqualTo(1));
                Assert.That(Owned().Length, Is.GreaterThan(0));
                UniTask load = Track((UniTask)RequireProductionMethod("EnsurePreparedCueLoadedAsync", 1).Invoke(Player, new[] { second }));
                await Pump(load);
                Assert.That(EnvelopeField<AudioClip[]>(second, "Clips").Count(x => x != null), Is.EqualTo(1));
                ObserveCache();
            }

            public static async UniTask Pump(UniTask task)
            {
                double deadline = UnityEditor.EditorApplication.timeSinceStartup + 240;
                while (task.Status == UniTaskStatus.Pending)
                {
                    Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(deadline), "bounded fixture timeout");
                    await NTSD_ResourceLoader.Instance.ProcessFrame();
                    await UniTask.Yield();
                }
                await task;
            }

            private void AssertProtected()
            {
                foreach (var pair in BeforeCache)
                {
                    Assert.That(Cache.Contains(pair.Key), Is.True, pair.Key);
                    Assert.That(ReferenceEquals(Cache[pair.Key], pair.Value), Is.True, pair.Key);
                    CollectionAssert.AreEqual(beforeCacheClipIds[pair.Key], ClipIds(CacheClips(pair.Value)), pair.Key);
                }
                Assert.That(ReferenceEquals(AudioController.Instance, boundController), Is.True);
                Assert.That(boundController == null ? null : UnityEditor.EditorJsonUtility.ToJson(boundController), Is.EqualTo(controllerJson));
                Assert.That(PlayerBorrowState(oldPlayers), Is.EqualTo(oldPlayerBorrowSnapshot));
                for (int i = 0; i < oldVoices.Length; i++)
                {
                    Assert.That(oldVoices[i] != null, Is.True);
                    Assert.That(oldVoices[i].clip != null ? oldVoices[i].clip.GetInstanceID() : 0, Is.EqualTo(oldVoiceClipIds[i]));
                }
                Assert.That(SceneState(), Is.EqualTo(sceneSnapshot));
            }

            private void ObserveCache()
            {
                AssertProtected();
                foreach (string key in Cache.Keys)
                {
                    if (BeforeCache.ContainsKey(key)) continue;
                    Assert.That(AllowedKeys.Contains(key), Is.True, "Unknown cache delta retained: " + key);
                    if (AddedCache.TryGetValue(key, out object prior))
                        Assert.That(ReferenceEquals(prior, Cache[key]), Is.True, "Changed delta retained: " + key);
                    else
                        AddedCache.Add(key, Cache[key]);
                }
                foreach (var pair in AddedCache)
                    Assert.That(Cache.Contains(pair.Key) && ReferenceEquals(Cache[pair.Key], pair.Value), Is.True);
            }

            private static object ClipDescription(AudioClip clip)
            {
                return new
                {
                    id = clip.GetInstanceID(), clip.name, clip.samples, clip.channels, clip.frequency,
                    theoreticalFloatPcmBytes = (long)clip.samples * clip.channels * sizeof(float),
                    objectBytes = Profiler.GetRuntimeMemorySizeLong(clip)
                };
            }

            public AudioClip[] Owned()
            {
                return EnvelopeField<List<AudioClip>>(Player, "ownedBattlePlaybackClips")
                    .Concat(EnvelopeField<List<AudioClip>>(Player, "ownedFormalAudioSources")).Where(x => x != null).Distinct().ToArray();
            }

            public void Snapshot(string stage)
            {
                ObserveCache();
                var output = EnvelopeField<IDictionary>(Player, "preparedFormalBattleCues").Values.Cast<object>()
                    .SelectMany(x => EnvelopeField<AudioClip[]>(x, "BattlePlaybackClips") ?? Array.Empty<AudioClip>()).Where(x => x != null).Distinct().ToArray();
                var sources = EnvelopeField<List<AudioClip>>(Player, "ownedFormalAudioSources").Where(x => x != null).ToArray();
                var oldCache = BeforeCache.Values.SelectMany(CacheClips).Where(x => x != null).Distinct().ToArray();
                var delta = AddedCache.Values.SelectMany(CacheClips).Where(x => x != null).Distinct().ToArray();
                var voices = Resources.FindObjectsOfTypeAll<AudioSource>().Where(x => x.clip != null).Select(x => x.clip).Distinct().ToArray();
                var groups = new Dictionary<string, AudioClip[]>
                {
                    ["formalOutputOrNonmono"] = output,
                    ["ownedMonoSource"] = sources.Where(x => x.channels == 1).ToArray(),
                    ["ownedNonmonoInput"] = sources.Where(x => x.channels != 1).ToArray(),
                    ["genericCacheExisting"] = oldCache, ["genericCacheNew"] = delta,
                    ["configuredBorrowed"] = configuredClips.Where(x => x != null).ToArray(), ["voiceBindings"] = voices
                };
                var distinct = groups.Values.SelectMany(x => x).Distinct().ToArray();
                using var process = System.Diagnostics.Process.GetCurrentProcess();
                Stages.Add(new
                {
                    stage, time = DateTime.UtcNow.ToString("O"),
                    groups = groups.ToDictionary(x => x.Key, x => ClipIds(x.Value)),
                    clips = distinct.Select(ClipDescription).ToArray(),
                    distinctObjectBytes = distinct.Sum(x => Profiler.GetRuntimeMemorySizeLong(x)),
                    distinctTheoreticalPcmBytes = distinct.Sum(x => (long)x.samples * x.channels * 4),
                    pendingLocal = Player.PendingFormalAudioLoadsForDiagnostics,
                    formalStarted = Player.FormalAudioLoadCountForDiagnostics,
                    formalCompleted = Player.FormalAudioLoadCountForDiagnostics - Player.PendingFormalAudioLoadsForDiagnostics,
                    preparedSealed = Player.BattleCatalogSealedForDiagnostics,
                    stage5Complete = Player.BattleSoundShutdownCompletedForDiagnostics,
                    tailDrained = Player.BattleAudioTailDrainedForDiagnostics,
                    cacheKeys = Cache.Keys.Cast<string>().OrderBy(x => x).ToArray(),
                    editorAudioClipCount = Resources.FindObjectsOfTypeAll<AudioClip>().Length,
                    destroyPending = 0, destroyScope = "EditMode DestroyImmediate; NOT Player deferred-destroy measurement",
                    decoderStagingBytes = "UNKNOWN: loader-private transient not observed",
                    actualTemporaryPcmBytes = "UNKNOWN: synchronous converter not intercepted",
                    boundedConverterTemporaryPayloadUpperBound = 98304,
                    privateMemoryBytes = process.PrivateMemorySize64 > 0 ? (long?)process.PrivateMemorySize64 : null,
                    workingSetBytes = process.WorkingSet64 > 0 ? (long?)process.WorkingSet64 : null,
                    processScope = "Environmental only; cannot add to object bytes or call sampled max a whole-game/device peak"
                });
            }

            public async UniTask Close(string phase)
            {
                RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(Player, null);
                RequireProductionMethod("CompleteBattleSoundShutdown", 0).Invoke(Player, null);
                Exception observationFailure = null;
                try { Snapshot(phase + "-stage5-return"); }
                catch (Exception error) { observationFailure = error; }
                await Drain();
                if (observationFailure != null) throw observationFailure;
                Snapshot(phase + "-tail-drained");
                Assert.That(Player.OwnedFormalAudioSourcesForDiagnostics, Is.Zero);
                Assert.That(Owned(), Is.Empty);
            }

            private void VerifyHeader(AudioClip clip, string path, bool converted)
            {
                if (headers == null)
                    headers = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(EnvelopeOutput, "headers.json")));
                var header = headers["files"][path];
                Assert.That(header, Is.Not.Null, path);
                Assert.That((bool)header["knownFrames"], Is.True, path);
                Assert.That(clip.samples, Is.EqualTo((int)header["frames"]), path);
                int channels = (int)header["channels"];
                Assert.That(clip.channels, Is.EqualTo(converted && channels == 1 ? 2 : channels), path);
                Assert.That(clip.frequency, Is.EqualTo((int)header["frequency"]), path);
                Assert.That(clip.name, Is.EqualTo(Path.GetFileNameWithoutExtension(path)), path);
            }

            public object VerifySealed(int session)
            {
                Assert.That(Player.BattleCatalogSealedForDiagnostics, Is.True);
                Assert.That(Player.RejectedUnpreparedCueCountForDiagnostics, Is.Zero);
                var rows = new List<object>();
                var identityObjects = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (EnvelopeCue row in Cues)
                {
                    object cue = GetCue(row.soundId, true);
                    Assert.That(cue, Is.Not.Null, row.soundId);
                    if (row.battle.formal)
                    {
                        if (identityObjects.TryGetValue(row.battle.identity, out object earlier))
                            Assert.That(ReferenceEquals(cue, earlier), Is.True, row.soundId);
                        else identityObjects.Add(row.battle.identity, cue);
                    }
                    Assert.That(EnvelopeField<bool>(cue, "IsLoaded"), Is.True, row.soundId);
                    AudioClip[] outputs = EnvelopeField<AudioClip[]>(cue, "BattlePlaybackClips") ?? Array.Empty<AudioClip>();
                    if (row.battle.exists)
                    {
                        Assert.That(outputs.Count(x => x != null), Is.EqualTo(row.battle.files.Length), row.soundId);
                        for (int index = 0; index < outputs.Length; index++) VerifyHeader(outputs[index], row.battle.files[index], true);
                    }
                    AudioClip[] genericClips = Array.Empty<AudioClip>();
                    if (row.battle.formal && row.generic.exists)
                    {
                        object generic = GetCue(row.soundId, false);
                        Assert.That(EnvelopeField<bool>(generic, "IsLoaded"), Is.True, row.soundId + " generic");
                        genericClips = EnvelopeField<AudioClip[]>(generic, "Clips") ?? Array.Empty<AudioClip>();
                        Assert.That(genericClips.Count(x => x != null), Is.EqualTo(row.generic.files.Length));
                        for (int index = 0; index < genericClips.Length; index++) VerifyHeader(genericClips[index], row.generic.files[index], false);
                    }
                    rows.Add(new
                    {
                        row.soundId, row.battle.identity, row.battle.path,
                        sourceExists = row.battle.exists,
                        missingReason = row.battle.exists ? null : row.battle.configuredBorrowed.Any(x => x != 0) ? "configured fallback" : "missing local, empty fallback",
                        outputs = outputs.Where(x => x != null).Select(x => new { x.name, x.samples, x.channels, x.frequency }).ToArray(),
                        generic = genericClips.Where(x => x != null).Select(x => new { x.name, x.samples, x.channels, x.frequency }).ToArray()
                    });
                }
                long expectedFormal = Cues.Where(x => x.battle.formal && x.battle.exists)
                    .Select(x => x.battle.identity).Distinct().LongCount();
                Assert.That(Player.FormalAudioLoadCountForDiagnostics, Is.EqualTo(expectedFormal * session));
                Assert.That(Player.RejectedUnpreparedCueCountForDiagnostics, Is.Zero);
                return new { rows, expectedFormal, genericDeltaKeyCount = AddedCache.Count,
                    failedLoads = Player.FailedPreparedCueLoadCountForDiagnostics,
                    missingFiles = Player.SkippedMissingBattleCueFileCountForDiagnostics };
            }

            public async UniTask Finish(string label)
            {
                if (finished) return;
                finished = true;
                var cleanup = new List<object>();
                string failure = null;
                try
                {
                    Exception observationFailure = null;
                    try { await Close(label); }
                    catch (Exception error) { observationFailure = error; }
                    Assert.That(FullyDrained, Is.True, "Tail unconfirmed: preserve fixture and lock");
                    // Destroy the fixture first. Its production generic dictionaries intentionally retain borrowed clips at stage5.
                    var genericReferences = EnvelopeField<IDictionary>(Player, "preparedCues");
                    var formalReferences = EnvelopeField<IDictionary>(Player, "preparedFormalBattleCues");
                    UnityEngine.Object.DestroyImmediate(Host);
                    genericReferences.Clear();
                    formalReferences.Clear();
                    RestoreOwnedSoundManager();
                    if (observationFailure != null) throw observationFailure;
                    ObserveCache();
                    Assert.That(Host == null && Player == null, Is.True);
                    AssertProtected();
                    var newObjects = AddedCache.Values.SelectMany(CacheClips).Where(x => x != null).Distinct().ToArray();
                    var protectedIds = new HashSet<int>(initialClipIds);
                    protectedIds.UnionWith(configuredClips.Where(x => x != null).Select(x => x.GetInstanceID()));
                    protectedIds.UnionWith(Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>()
                        .SelectMany(PlayerBorrowed).Select(x => x.GetInstanceID()));
                    protectedIds.UnionWith(Resources.FindObjectsOfTypeAll<AudioSource>().Where(x => x.clip != null).Select(x => x.clip.GetInstanceID()));
                    foreach (string key in Cache.Keys)
                        if (!AddedCache.ContainsKey(key))
                            protectedIds.UnionWith(CacheClips(Cache[key]).Where(x => x != null).Select(x => x.GetInstanceID()));
                    Assert.That(newObjects.All(x => !protectedIds.Contains(x.GetInstanceID())), Is.True,
                        "Shared/old cache resource detected; retain delta, no destructive cleanup");
                    foreach (var pair in AddedCache)
                    {
                        Assert.That(!BeforeCache.ContainsKey(pair.Key) && AllowedKeys.Contains(pair.Key)
                            && ReferenceEquals(Cache[pair.Key], pair.Value), Is.True);
                        cleanup.Add(new { key = pair.Key, ids = ClipIds(CacheClips(pair.Value)), identityQualified = true });
                    }
                    foreach (var pair in AddedCache)
                        Assert.That(NTSD_ResourceLoader.Instance.RemoveCache(pair.Key), Is.True);
                    int[] destroyed = newObjects.Select(x => x.GetInstanceID()).ToArray();
                    foreach (AudioClip clip in newObjects) UnityEngine.Object.DestroyImmediate(clip);
                    Assert.That(newObjects.All(x => x == null), Is.True);
                    var survivingIds = Resources.FindObjectsOfTypeAll<AudioClip>().Select(x => x.GetInstanceID()).ToHashSet();
                    Assert.That(destroyed.Any(survivingIds.Contains), Is.False);
                    AddedCache.Clear();
                    Assert.That(CacheMatches(Cache, BeforeCache), Is.True);
                    Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
                    Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
                    RestoreOwnedSoundManager();
                    Assert.That(ReferenceEquals(managerField.GetValue(null), previousCharacterManager), Is.True);
                    AssertProtected();
                    CollectionAssert.AreEqual(beforeSceneObjects, UnityEngine.Object.FindObjectsOfType<GameObject>(true)
                        .Select(x => x.GetInstanceID()).OrderBy(x => x).ToArray());
                    Snapshot(label + "-final-cleanup");
                    SaveEnvelope(label + "-cleanup", new { passed = true, cleanup, destroyed,
                        cacheRestored = true, oldNativeProtected = true, otherPlayersAndCrossKeyBorrowersChecked = true,
                        hostDestroyedBeforeCacheRelease = true, sceneRestored = true, queuesRestored = true });
                }
                catch (Exception error)
                {
                    failure = error.ToString();
                    SaveEnvelope(label + "-cleanup-failure", new { failure, cleanup, cacheKeys = Cache.Keys.Cast<string>().ToArray(),
                        policy = "Unknown cache/resource delta retained. No global cleanup or follow-on full session." });
                    throw;
                }
                finally
                {
                    bool drained = FullyDrained;
                    if (drained)
                    {
                        if (Host != null) UnityEngine.Object.DestroyImmediate(Host);
                        RestoreOwnedSoundManager();
                        configField.SetValue(null, previousConfig);
                        controllerField.SetValue(null, previousController);
                        envelopeRunning = false;
                    }
                    SaveEnvelope(label + "-stages", new { Stages, failure, operationErrors, drained,
                        configRestored = ReferenceEquals(configField.GetValue(null), previousConfig),
                        controllerRestored = ReferenceEquals(AudioController.Instance, previousController),
                        mmSoundManagerRestored = ReferenceEquals(MoreMountains.Tools.MMSoundManager.Current, previousSoundManager) });
                }
            }
        }


        private static void AssertEnvelopeInventoryStable(object current)
        {
            var expected = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(EnvelopeOutput, "inventory.json")));
            var actual = Newtonsoft.Json.Linq.JObject.FromObject(current);
            foreach (string key in new[] { "definitionCount", "cueCount", "catalog", "CatalogSha256",
                "DefinitionFingerprint", "BattleDefinitionFingerprint", "mode", "builtIn",
                "perDefinition", "files", "allowedGenericKeys", "actualSoundRoot" })
                Assert.That(Newtonsoft.Json.Linq.JToken.DeepEquals(expected[key], actual[key]), Is.True, "Inventory changed: " + key);
            foreach (var root in new[] { expected, actual })
                foreach (var route in root["routes"])
                {
                    ((Newtonsoft.Json.Linq.JObject)route["battle"]).Remove("configuredBorrowed");
                    ((Newtonsoft.Json.Linq.JObject)route["generic"]).Remove("configuredBorrowed");
                }
            Assert.That(Newtonsoft.Json.Linq.JToken.DeepEquals(expected["routes"], actual["routes"]), Is.True, "Resolver route drift");
        }

        [UnityTest]
        public IEnumerator CatalogAudioEnvelopeInventory()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var fixture = new EnvelopeFixture();
                try
                {
                    SaveEnvelope("inventory", fixture.BuildInventory());
                    fixture.Snapshot("inventory-no-decode");
                    Assert.That(fixture.Player.FormalAudioLoadCountForDiagnostics, Is.Zero);
                }
                finally { await fixture.Finish("inventory"); }
            });
        }

        [UnityTest]
        public IEnumerator CatalogAudioEnvelopeFixtureQualification()
        {
            return UniTask.ToCoroutine(async () =>
            {
                Assert.That(File.Exists(Path.Combine(EnvelopeOutput, "inventory.json")), Is.True);
                var fixture = new EnvelopeFixture();
                bool passed = false;
                try
                {
                    AssertEnvelopeInventoryStable(fixture.BuildInventory());
                    EnvelopeCue generic = fixture.Cues.First(x => x.generic.exists && x.generic.files.Length == 1
                        && !fixture.BeforeCache.ContainsKey(x.generic.key));
                    fixture.Snapshot("small-before");
                    await fixture.LoadSmall("data/001.wav", generic);
                    fixture.Snapshot("small-loaded");
                    Assert.That(fixture.AddedCache.Count, Is.EqualTo(1));
                    AudioClip[] owned = fixture.Owned();
                    Assert.That(owned.Length, Is.GreaterThan(0));
                    int[] ids = owned.Select(x => x.GetInstanceID()).ToArray();
                    await fixture.Close("small");
                    Assert.That(owned.All(x => x == null), Is.True);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(x => ids.Contains(x.GetInstanceID())), Is.False);
                    SaveEnvelope("small-selection-v2", new { formal = "data/001.wav", generic.soundId, generic.generic,
                        actualNewCacheKeys = fixture.AddedCache.Keys.ToArray(), ownedDestroyed = ids });
                    passed = true;
                }
                finally { await fixture.Finish("qualification-v2"); }
                Assert.That(passed, Is.True);
                SaveEnvelope("fixture-qualified", new { passed = true, time = DateTime.UtcNow.ToString("O"),
                    sourceSha = EnvelopeSha("Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs") });
            });
        }

        [UnityTest]
        public IEnumerator CatalogAudioEnvelopeTwoSessionMeasurement()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var qualification = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(EnvelopeOutput, "fixture-qualified.json")));
                Assert.That((bool)qualification["passed"], Is.True);
                Assert.That((string)qualification["sourceSha"], Is.EqualTo(EnvelopeSha("Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs")));
                var preflight = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(EnvelopeOutput, "capacity-preflight.json")));
                Assert.That((bool)preflight["safeToExecute"], Is.True);
                Assert.That((string)preflight["inventorySha256"], Is.EqualTo(EnvelopeSha(Path.Combine(EnvelopeOutput, "inventory.json"))));
                Assert.That(DateTime.UtcNow - DateTime.Parse((string)preflight["utcTime"]).ToUniversalTime(), Is.LessThan(TimeSpan.FromMinutes(10)));
                var fixture = new EnvelopeFixture();
                try
                {
                    AssertEnvelopeInventoryStable(fixture.BuildInventory());
                    using (var marker = new FileStream(Path.Combine(EnvelopeOutput, "full-group.started"), FileMode.CreateNew, FileAccess.Write))
                    {
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));
                        marker.Write(data, 0, data.Length);
                    }
                    object firstSemantic = null;
                    var previousOwned = Array.Empty<AudioClip>();
                    var previousIds = Array.Empty<int>();
                    for (int session = 1; session <= 2; session++)
                    {
                        Assert.That(previousOwned.All(x => x == null), Is.True);
                        Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(x => previousIds.Contains(x.GetInstanceID())), Is.False);
                        fixture.Snapshot("session" + session + "-before");
                        long startCount = fixture.Player.FormalAudioLoadCountForDiagnostics;
                        long nextSnapshot = 100;
                        int midSnapshots = 0;
                        double start = UnityEditor.EditorApplication.timeSinceStartup;
                        UniTask preparation = fixture.Track(fixture.Player.PrepareBattleCuesAsync(fixture.Manager));
                        while (preparation.Status == UniTaskStatus.Pending)
                        {
                            Assert.That(UnityEditor.EditorApplication.timeSinceStartup - start, Is.LessThan(240));
                            await NTSD_ResourceLoader.Instance.ProcessFrame();
                            long completed = fixture.Player.FormalAudioLoadCountForDiagnostics - startCount - fixture.Player.PendingFormalAudioLoadsForDiagnostics;
                            if (completed >= nextSnapshot && midSnapshots < 16)
                            {
                                fixture.Snapshot("session" + session + "-preparing-" + completed);
                                midSnapshots++;
                                nextSnapshot = completed + 100;
                            }
                            await UniTask.Yield();
                        }
                        await preparation;
                        fixture.Snapshot("session" + session + "-sealed");
                        object semantics = fixture.VerifySealed(session);
                        // Cumulative failure counters are reported separately, not compared as per-session state.
                        string currentRows = Newtonsoft.Json.Linq.JObject.FromObject(semantics)["rows"].ToString(Formatting.None);
                        if (session == 1) firstSemantic = currentRows;
                        else Assert.That(currentRows, Is.EqualTo(firstSemantic));
                        previousOwned = fixture.Owned();
                        Assert.That(previousOwned.Any(x => previousIds.Contains(x.GetInstanceID())), Is.False, "New session must not retain old formal identity");
                        previousIds = previousOwned.Select(x => x.GetInstanceID()).ToArray();
                        SaveEnvelope("session" + session + "-sealed", new { semantics, ownedIds = previousIds, midSnapshots,
                            elapsedWithObserverSeconds = UnityEditor.EditorApplication.timeSinceStartup - start,
                            scope = "Actual public full prepare; observer allocation/time included; not CPU benchmark" });
                        await fixture.Close("session" + session);
                        Assert.That(previousOwned.All(x => x == null), Is.True);
                        Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(x => previousIds.Contains(x.GetInstanceID())), Is.False);
                    }
                    fixture.Candidate.AssertInputsCurrent();
                }
                finally { await fixture.Finish("full"); }
                SaveEnvelope("full-result", new { status = "FULL_CATALOG_AUDIO_ENVELOPE_MEASURED", sessions = 2,
                    metadataEqual = true, exactRestoration = true, budgetStatus = "UNDECIDED: measurement is not product budget acceptance" });
            });
        }

        [Test]
        public void CatalogAudioEnvelopeRecoverExactQualificationDelta()
        {
            Assert.That(Application.isPlaying, Is.False);
            Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
            Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
            Assert.That(System.Diagnostics.Process.GetCurrentProcess().Id, Is.EqualTo(39108), "Original Editor process required for native ID proof");
            const int expectedId = -1243288;
            const string expectedKey = @"NTSD.AudioFile::data\015.wav";
            var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(NTSD_ResourceLoader.Instance);
            var before = SnapshotCache(cache);
            var clips = Resources.FindObjectsOfTypeAll<AudioClip>();
            var otherBefore = clips.Where(x => x.GetInstanceID() != expectedId).ToArray();
            AudioClip target = clips.SingleOrDefault(x => x.GetInstanceID() == expectedId);
            object capturedValue = cache.Contains(expectedKey) ? cache[expectedKey] : null;
            Assert.That(cache.Keys.Cast<string>().All(x => x == expectedKey), Is.True, "Unexpected cache preserved");
            if (target != null)
            {
                Assert.That(target.name, Is.EqualTo("015"));
                Assert.That(target.samples, Is.EqualTo(7153));
                Assert.That(target.channels, Is.EqualTo(1));
                Assert.That(target.frequency, Is.EqualTo(44100));
                if (capturedValue != null) Assert.That(ReferenceEquals(capturedValue, target), Is.True);
                foreach (AudioSource voice in Resources.FindObjectsOfTypeAll<AudioSource>())
                    Assert.That(voice.clip == target, Is.False);
                foreach (AudioController controller in Resources.FindObjectsOfTypeAll<AudioController>())
                    Assert.That((controller.AudioList ?? Array.Empty<AudioItem>()).Where(x => x != null)
                        .SelectMany(x => x.clip ?? Array.Empty<AudioClip>()).Contains(target), Is.False);
                foreach (NTSDSoundPlayer player in Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>())
                {
                    Assert.That(player.BattleAudioPreparationInProgressForDiagnostics, Is.False);
                    Assert.That(player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                    foreach (string ownedField in new[] { "ownedBattlePlaybackClips", "ownedFormalAudioSources" })
                        Assert.That(EnvelopeField<List<AudioClip>>(player, ownedField).Contains(target), Is.False);
                    foreach (string field in new[] { "preparedCues", "preparedFormalBattleCues" })
                        foreach (object cue in EnvelopeField<IDictionary>(player, field).Values)
                            foreach (string clipsField in new[] { "Clips", "BattlePlaybackClips" })
                                Assert.That((EnvelopeField<AudioClip[]>(cue, clipsField) ?? Array.Empty<AudioClip>()).Contains(target), Is.False);
                }
                foreach (string key in cache.Keys)
                    if (key != expectedKey)
                        Assert.That(EnvelopeFixture.CacheClips(cache[key]).Contains(target), Is.False);
                if (capturedValue != null)
                {
                    Assert.That(ReferenceEquals(cache[expectedKey], capturedValue), Is.True);
                    Assert.That(NTSD_ResourceLoader.Instance.RemoveCache(expectedKey), Is.True);
                }
                UnityEngine.Object.DestroyImmediate(target);
                Assert.That(target == null, Is.True);
            }
            else
                Assert.That(capturedValue, Is.Null, "Missing native with unexpected cache mapping: retain");
            Assert.That(otherBefore.All(x => x != null), Is.True);
            Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Any(x => x.GetInstanceID() == expectedId), Is.False);
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/NTSD/Scene/NTSD_Menu.unity");
            Assert.That(scene.isLoaded && !scene.isDirty && scene.rootCount == 8, Is.True);
            SaveEnvelope("qualification-delta-recovery", new
            {
                expectedKey, expectedId, cacheMappingPresentBefore = capturedValue != null,
                targetPresentBefore = clips.Any(x => ReferenceEquals(x, target)),
                originalOtherClips = otherBefore.Select(x => new { id = x.GetInstanceID(), x.name, x.samples, x.channels, x.frequency }).ToArray(),
                cacheEmptyAfter = cache.Count == 0, targetGone = true, sceneRestoredByFramework = true,
                scope = "Exact task-owned failed-qualification native ID only. Domain reload may discard new mapping; not used as release proof. No other native cleanup."
            });
        }

        [Test]
        public void CatalogAudioEnvelopeUnknownManagerGuardPreservesObjects()
        {
            var singletonBefore = MoreMountains.Tools.MMSoundManager.Current;
            var hosts = new[] { new GameObject("H10KnownGuard"), new GameObject("H10OtherKnownGuard"), new GameObject("H10UnknownGuard") };
            foreach (var host in hosts) { host.hideFlags = HideFlags.HideAndDontSave; host.SetActive(false); }
            try
            {
                var managers = hosts.Select(x => x.AddComponent<MoreMountains.Tools.MMSoundManager>()).ToArray();
                var fixture = (EnvelopeFixture)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(EnvelopeFixture));
                typeof(EnvelopeFixture).GetField("beforeSoundManagers", Private).SetValue(fixture, managers.Take(2).ToArray());
                typeof(EnvelopeFixture).GetField("beforeSceneObjects", Private).SetValue(fixture, hosts.Take(2).Select(x => x.GetInstanceID()).ToArray());
                MethodInfo guard = typeof(EnvelopeFixture).GetMethod("RecordResolvedExistingSoundManager", Private);
                Assert.That(Assert.Throws<TargetInvocationException>(() => guard.Invoke(fixture, new object[] { managers[2] })).InnerException,
                    Is.InstanceOf<NUnit.Framework.AssertionException>());
                Assert.That(hosts.All(x => x != null), Is.True, "Unknown object must not be destroyed");
                guard.Invoke(fixture, new object[] { managers[0] });
                Assert.That(Assert.Throws<TargetInvocationException>(() => guard.Invoke(fixture, new object[] { managers[1] })).InnerException,
                    Is.InstanceOf<NUnit.Framework.AssertionException>());
                Assert.That(hosts.All(x => x != null), Is.True);
                Assert.That(ReferenceEquals(MoreMountains.Tools.MMSoundManager.Current, singletonBefore), Is.True);
                SaveEnvelope("unknown-manager-guard", new { unknownPreserved = true, changedKnownRejected = true,
                    originalAccepted = true, singletonUnchanged = true, scope = "No audio decode, isolated real-component ownership guard only" });
            }
            finally
            {
                foreach (var host in hosts) if (host != null) UnityEngine.Object.DestroyImmediate(host);
            }
        }

        // Alignment contract: NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001.
        private const string PcBudgetOutput = "artifacts/diagnostics/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011";
        private const string PcBudgetRestoreKey = "NTSD.H10.PcBudget.SceneRestore.20261011";
        private static readonly string[] PcBudgetCueIds =
        {
            "c/jira/w/e3.wav", "c/kar/w/t.wav", "c/dan/w/t.wav", "data/001.wav", "c/kim/w/a1.wav"
        };

        private static void PcBudgetSave(string name, object value)
        {
            Directory.CreateDirectory(PcBudgetOutput);
            using var writer = new StreamWriter(new FileStream(Path.Combine(PcBudgetOutput, name + ".json"),
                FileMode.CreateNew, FileAccess.Write));
            writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static FieldInfo PcBudgetStatic(Type type, string name)
        {
            var field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, type.FullName + "." + name);
            return field;
        }

        private static UnityEditor.SceneManagement.SceneSetup[] PcBudgetPreflight()
        {
            Assert.That(Application.isPlaying, Is.False);
            Assert.That(UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating, Is.False);
            Assert.That(UnityEditor.SessionState.GetString(PcBudgetRestoreKey, ""), Is.Empty);
            var initial = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(PcBudgetOutput, "initial-scenes.json")));
            var original = (Newtonsoft.Json.Linq.JArray)initial["result"]["data"]["scenes"];
            Assert.That(original.Count, Is.EqualTo(1));
            Assert.That((string)original[0]["path"], Is.EqualTo("Assets/NTSD/Scene/NTSD_Menu.unity"));
            Assert.That((bool)original[0]["isDirty"], Is.False);
            var menu = UnityEngine.SceneManagement.SceneManager.GetSceneByPath((string)original[0]["path"]);
            Assert.That(menu.IsValid() && menu.isLoaded && !menu.isDirty && menu.rootCount == 8, Is.True);
            // The installed EditModeLauncher creates exactly one active empty additive scene before a test.
            Assert.That(UnityEngine.SceneManagement.SceneManager.sceneCount, Is.EqualTo(2));
            var frameworkScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(1);
            Assert.That(frameworkScene != menu && frameworkScene == UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Is.True);
            Assert.That(frameworkScene.path, Is.Empty);
            Assert.That(frameworkScene.rootCount, Is.Zero);
            Assert.That(frameworkScene.isDirty, Is.False);
            Assert.That(EnvelopeSha("Library/PackageCache/com.unity.test-framework@1.1.33/UnityEditor.TestRunner/TestLaunchers/EditModeLauncher.cs"),
                Is.EqualTo((string)Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(
                    Path.Combine(PcBudgetOutput, "framework-input.json")))["sha256"]));
            var loader = NTSD_ResourceLoader.Instance;
            Assert.That(loader.GetPendingRequestCount(), Is.Zero);
            Assert.That(loader.GetQueuedTaskCount(), Is.Zero);
            var cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(loader);
            Assert.That(cache.Count, Is.Zero, "Existing cache cannot be discarded by domain reload");
            foreach (var player in Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>())
            {
                Assert.That(player.BattleAudioPreparationInProgressForDiagnostics, Is.False);
                Assert.That(player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                Assert.That(player.OwnedFormalAudioSourcesForDiagnostics, Is.Zero);
                Assert.That(ReadPrivateField<List<AudioClip>>(player, "ownedBattlePlaybackClips").Count, Is.Zero);
                Assert.That(ReadPrivateField<IDictionary>(player, "preparedCues").Count, Is.Zero);
            }
            foreach (var voice in Resources.FindObjectsOfTypeAll<AudioSource>())
                if (voice.gameObject.scene.IsValid()) Assert.That(voice.isPlaying, Is.False, voice.name);
            var setups = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            return setups.Where(x => x.path == menu.path).Select(x => new UnityEditor.SceneManagement.SceneSetup
            {
                path = x.path, isLoaded = x.isLoaded, isActive = (bool)original[0]["isActive"]
            }).ToArray();
        }

        [Test]
        public void PcBudgetRuntimeQualification()
        {
            var setup = PcBudgetPreflight();
            var correspondence = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(
                Path.Combine(PcBudgetOutput, "catalog-input-correspondence-resolved.json")));
            Assert.That((string)correspondence["status"], Is.EqualTo("INPUT_FINGERPRINTS_MATCH"));
            var controllerProof = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(
                EnvelopeOutput, "controller-evidence.json")));
            foreach (var pair in ((Newtonsoft.Json.Linq.JObject)controllerProof["files"]).Properties())
                Assert.That(EnvelopeSha(pair.Name), Is.EqualTo((string)pair.Value));
            Assert.That((int)controllerProof["configuredAudioItems"], Is.Zero);
            Assert.That(Resources.FindObjectsOfTypeAll<AudioController>().Any(x => x.gameObject.scene.IsValid()), Is.False,
                "This fixture supports the recorded empty Battle AudioList fallback only");
            foreach (string name in new[] { "PublishPendingSoundsAfterChecksum", "DispatchPublishedSounds", "PreparePresentationHotPathCapacity" })
                Assert.That(typeof(NTSD.Simulation.SimulationTickDriver).GetMethod(name, Private), Is.Not.Null);
            foreach (string cue in PcBudgetCueIds)
                Assert.That(File.Exists(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime/vfs", cue)), Is.True, cue);
            PcBudgetSave("qualification", new
            {
                setup, cacheEmpty = true, loaderIdle = true, noActiveAudio = true,
                originalManagers = Resources.FindObjectsOfTypeAll<MoreMountains.Tools.MMSoundManager>()
                    .Where(x => x.gameObject.scene.IsValid()).Select(x => new { id = x.GetInstanceID(), json = UnityEditor.EditorJsonUtility.ToJson(x) }).ToArray(),
                controller = "Recorded Menu null / Battle empty AudioList fallback, not controller lifecycle equivalence",
                editorSettings = new { UnityEditor.EditorSettings.enterPlayModeOptionsEnabled, UnityEditor.EditorSettings.enterPlayModeOptions },
                scope = "No audio decode or scene switch; precise original-state and seam prerequisite qualification"
            });
        }

        [UnityTearDown]
        public IEnumerator RestorePcBudgetRuntimeScene()
        {
            string saved = UnityEditor.SessionState.GetString(PcBudgetRestoreKey, "");
            if (string.IsNullOrEmpty(saved)) yield break;
            if (Application.isPlaying) yield return new ExitPlayMode();
            var setup = JsonConvert.DeserializeObject<H10SceneSetup>(saved);
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(setup.Scenes);
            UnityEditor.SessionState.EraseString(PcBudgetRestoreKey);
            var after = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            Assert.That(after.Length, Is.EqualTo(setup.Scenes.Length));
            for (int i = 0; i < after.Length; i++)
            {
                Assert.That(after[i].path, Is.EqualTo(setup.Scenes[i].path));
                Assert.That(after[i].isActive, Is.EqualTo(setup.Scenes[i].isActive));
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty, Is.False);
            }
            Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).rootCount, Is.EqualTo(8));
            PcBudgetSave("scene-restored", new { setup = after, menuClean8 = true, sceneAssetsSaved = false });
        }

        [UnityTest]
        public IEnumerator PcBudgetRuntimeThreeSessionsPlay()
        {
            var setup = PcBudgetPreflight();
            Assert.That(File.Exists(Path.Combine(PcBudgetOutput, "qualification.json")), Is.True);
            PcBudgetSave("unique-play-started", new { utc = DateTime.UtcNow, setup });
            string saved = JsonConvert.SerializeObject(new H10SceneSetup { Scenes = setup });
            UnityEditor.SessionState.SetString(PcBudgetRestoreKey, saved);
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return UniTask.ToCoroutine(async () =>
            {
                PcBudgetFixture fixture = null;
                string failure = null;
                string cleanupFailure = null;
                try
                {
                    fixture = new PcBudgetFixture();
                    fixture.Initialize();
                    await fixture.RunThreeSessions();
                    await fixture.RunPendingBoundaries();
                }
                catch (Exception ex)
                {
                    failure = ex.ToString();
                    throw;
                }
                finally
                {
                    if (fixture != null)
                    {
                        try { await fixture.Finish(); }
                        catch (Exception ex) { cleanupFailure = ex.ToString(); }
                    }
                    PcBudgetSave("play-result", new
                    {
                        failure, cleanupFailure, rows = fixture?.Rows,
                        scope = "One isolated Play; diagnostic World and audio requests, not real1000 combat entities/Player/8GB certification",
                        success = failure == null && cleanupFailure == null,
                        hearingVerified = false, fullEngineNativeAllocationClaimed = false
                    });
                    if (failure == null && cleanupFailure != null) Assert.Fail(cleanupFailure);
                }
            });
            yield return new ExitPlayMode();
        }

        private struct PcBudgetCounters
        {
            public long Plays, Evictions, Rejections, Loads, Builds, Failures;
            public static PcBudgetCounters Read(NTSDSoundPlayer player) => new PcBudgetCounters
            {
                Plays = player.PooledOneShotPlayCountForDiagnostics,
                Evictions = player.OneShotVoiceLimitDropCountForDiagnostics,
                Rejections = player.RejectedUnpreparedCueCountForDiagnostics,
                Loads = player.FormalAudioLoadCountForDiagnostics,
                Builds = player.PreparedCueBuildCountForDiagnostics,
                Failures = player.FailedPreparedCueLoadCountForDiagnostics
            };
        }

        private struct PcBudgetHotRow
        {
            public int Frame, Tick, Requests;
            public long Allocated, QueueAllocated;
            public PcBudgetCounters Before, After;
        }

        private sealed class PcBudgetSink : NTSD.Simulation.ISimulationSoundPresentationSink
        {
            private readonly NTSDSoundPlayer player;
            public readonly PcBudgetHotRow[] Samples = new PcBudgetHotRow[128];
            public int Count;
            public PcBudgetSink(NTSDSoundPlayer value) { player = value; }
            public void PresentSounds(IReadOnlyList<NTSD.Simulation.PendingSoundEvent> sounds)
            {
                int index = Count++;
                var before = PcBudgetCounters.Read(player);
                long bytes = GC.GetAllocatedBytesForCurrentThread();
                player.PresentSounds(sounds);
                long allocated = GC.GetAllocatedBytesForCurrentThread() - bytes;
                Samples[index] = new PcBudgetHotRow
                {
                    Frame = Time.frameCount, Tick = sounds.Count > 0 ? sounds[0].Tick : -1,
                    Requests = sounds.Count, Allocated = allocated, Before = before, After = PcBudgetCounters.Read(player)
                };
            }
        }

        private sealed class PcBudgetEmitter : NTSD.Animation.LF2Objects.LF2Entity
        {
            private readonly NTSD.Simulation.SimulationWorld world;
            public int Count = 1, CueCount = 1;
            public long QueueAllocated;
            public override NTSD.Animation.LF2Objects.LF2ObjectType ObjectTypeEnum => NTSD.Animation.LF2Objects.LF2ObjectType.Other;
            public PcBudgetEmitter(NTSD.Simulation.SimulationWorld value)
            {
                world = value;
                Name = "H10PcBudgetDiagnosticSoundEmitter";
                FrameDelay = 4;
                Health = new NTSD.Animation.LF2Objects.LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }
            public override void SimTransit(int tickIndex)
            {
                long bytes = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < Count; i++) world.QueueSound(PcBudgetCueIds[i % CueCount], 500);
                QueueAllocated = GC.GetAllocatedBytesForCurrentThread() - bytes;
            }
            public override void SimTU(int tickIndex) { }
            public override void Reset() { }
            public override void Init(NTSD.Animation.LF2Tasks.LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        private sealed class PcBudgetFixture
        {
            public readonly List<object> Rows = new List<object>(96);
            private readonly List<GameObject> hosts = new List<GameObject>(12);
            private readonly List<UniTask> operations = new List<UniTask>(8);
            private readonly Dictionary<string, object> addedCache = new Dictionary<string, object>(StringComparer.Ordinal);
            private readonly HashSet<string> allowedKeys = new HashSet<string>(StringComparer.Ordinal);
            private readonly IDictionary cache;
            private readonly Dictionary<string, object> beforeCache;
            private readonly HashSet<int> initialClipIds;
            private readonly FieldInfo configField, managerField, mmField, driverField;
            private readonly object oldConfig, oldManager, oldMm, oldDriver;
            private readonly UnityEngine.Random.State randomBefore;
            private GameObject audioHost;
            private CharacterAnimtorManager manager;
            private AudioListener listener;
            private NTSDSoundPlayer player;
            private readonly float[] outputSamples = new float[512];
            private readonly List<AudioClip> allTrackedOwned = new List<AudioClip>(160);
            private NTSD.Simulation.SimulationTickDriver driver;
            private GameObject driverHost;
            private int lastGeneration;
            private int firstGenericId;
            private bool finished;

            public PcBudgetFixture()
            {
                Assert.That(Application.isPlaying, Is.True);
                Assert.That(AppManager.Instance == null && NTSD.Simulation.SimulationTickDriver.Instance == null, Is.True);
                Assert.That(Resources.FindObjectsOfTypeAll<AudioController>().Any(x => x.gameObject.scene.IsValid()), Is.False);
                Assert.That(Resources.FindObjectsOfTypeAll<AudioSource>().Any(x => x.gameObject.scene.IsValid()), Is.False);
                Assert.That(Resources.FindObjectsOfTypeAll<MoreMountains.Tools.MMSoundManager>().Any(x => x.gameObject.scene.IsValid()), Is.False);
                configField = PcBudgetStatic(typeof(GameConfig), "_instance");
                managerField = PcBudgetStatic(typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>), "_instance");
                mmField = PcBudgetStatic(typeof(MoreMountains.Tools.MMPersistentSingleton<MoreMountains.Tools.MMSoundManager>), "_instance");
                driverField = PcBudgetStatic(typeof(NTSD.Simulation.SimulationTickDriver).BaseType, "<Instance>k__BackingField");
                oldConfig = configField.GetValue(null); oldManager = managerField.GetValue(null);
                oldMm = mmField.GetValue(null); oldDriver = driverField.GetValue(null);
                Assert.That(oldManager == null && oldMm == null && oldDriver == null, Is.True);
                randomBefore = UnityEngine.Random.state;
                cache = (IDictionary)typeof(NTSD_ResourceLoader).GetField("cache", Private).GetValue(NTSD_ResourceLoader.Instance);
                beforeCache = SnapshotCache(cache);
                Assert.That(beforeCache.Count, Is.Zero);
                Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
                Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
                initialClipIds = Resources.FindObjectsOfTypeAll<AudioClip>().Select(x => x.GetInstanceID()).ToHashSet();
            }

            public void Initialize()
            {
                configField.SetValue(null, UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset"));
                var mmHost = OwnHost("H10PcBudgetOwnedMM", false);
                var mm = mmHost.AddComponent<MoreMountains.Tools.MMSoundManager>();
                mm.enabled = false; // Own manager supplies its mixer reference; do not load or mutate shared settings on Start.
                mm.settingsSo = UnityEditor.AssetDatabase.LoadAssetAtPath<MoreMountains.Tools.MMSoundManagerSettingsSO>(
                    "Assets/NTSD/MoreMountains/MMTools/Core/MMAudio/MMSoundManager/Settings/MMSoundManagerSettings.asset");
                mmHost.SetActive(true);
                Assert.That(ReferenceEquals(MoreMountains.Tools.MMSoundManager.Current, mm), Is.True);
                var managerHost = OwnHost("H10PcBudgetMetadata", false);
                manager = managerHost.AddComponent<CharacterAnimtorManager>();
                var frame = new LF2FrameData();
                typeof(LF2FrameData).GetMethod("SealFrameSounds", Private).Invoke(frame, new object[] { PcBudgetCueIds });
                typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", Private).SetValue(manager,
                    new Dictionary<int, LF2CharacterDataWrapper>
                    {
                        [2710] = new LF2CharacterDataWrapper(2710, new LF2CharacterData { frames = new List<LF2FrameData> { frame } })
                    });
                audioHost = OwnHost("H10PcBudgetAudio", false);
                listener = audioHost.AddComponent<AudioListener>();
                player = audioHost.AddComponent<NTSDSoundPlayer>();
                audioHost.SetActive(true);
                Assert.That(listener.enabled && listener.gameObject.activeInHierarchy, Is.True);
                Assert.That(ReadPrivateField<string>(player, "soundRootFolder"), Is.EqualTo("NTSD/Sound"));
                var all = new HashSet<string>(PcBudgetCueIds, StringComparer.Ordinal);
                foreach (string cue in (string[])typeof(NTSDSoundPlayer).GetField("BuiltInBattleSoundIds",
                    BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)) all.Add(cue);
                var routes = new List<object>();
                foreach (string cue in all)
                {
                    object generic = Cue(player, cue, false);
                    string path = ReadPrivateCue<string>(generic, "SourcePath");
                    string key = ReadPrivateCue<string>(generic, "CacheKey");
                    bool exists = ReadPrivateCue<bool>(generic, "IsSingleFile") ? File.Exists(path) : Directory.Exists(path);
                    if (exists) allowedKeys.Add(key);
                    routes.Add(new { cue, path, key, exists });
                }
                Rows.Add(new { stage = "fixture-created", routes, allowedKeys = allowedKeys.ToArray(),
                    soundRoot = ReadPrivateField<string>(player, "soundRootFolder"), controller = "null; verified real empty AudioList fallback",
                    noFullCatalog = true, builtinsAddedByPublicPrepare = true });
                PcBudgetSave("predeclared-generic-routes", Rows[Rows.Count - 1]);
            }

            private GameObject OwnHost(string name, bool active)
            {
                var host = new GameObject(name);
                hosts.Add(host); // Ownership is recorded at construction, before any component/getter runs.
                host.SetActive(active);
                return host;
            }

            private static object Cue(NTSDSoundPlayer value, string id, bool battle) =>
                RequireProductionMethod("GetOrPrepareCue", 2).Invoke(value, new object[] { id, battle });
            private static T ReadPrivateCue<T>(object cue, string name) => (T)RequireCueField(cue.GetType(), name).GetValue(cue);
            private static AudioClip[] Owned(NTSDSoundPlayer value) => ReadPrivateField<List<AudioClip>>(value, "ownedFormalAudioSources")
                .Concat(ReadPrivateField<List<AudioClip>>(value, "ownedBattlePlaybackClips")).Where(x => x != null).Distinct().ToArray();

            private async UniTask Await(UniTask operation)
            {
                UniTask tracked = operation.Preserve();
                operations.Add(tracked);
                double deadline = UnityEditor.EditorApplication.timeSinceStartup + 120;
                while (tracked.Status == UniTaskStatus.Pending)
                {
                    if (UnityEditor.EditorApplication.timeSinceStartup > deadline)
                        throw new TimeoutException("H10 PC fixture async tail is still pending; no new session allowed");
                    await UniTask.NextFrame();
                }
                await tracked;
            }

            private void ObserveCache()
            {
                foreach (DictionaryEntry entry in cache)
                {
                    string key = (string)entry.Key;
                    if (beforeCache.TryGetValue(key, out object old)) { Assert.That(entry.Value, Is.SameAs(old)); continue; }
                    Assert.That(allowedKeys.Contains(key), Is.True, key);
                    if (addedCache.TryGetValue(key, out object previous)) Assert.That(entry.Value, Is.SameAs(previous), key);
                    else addedCache.Add(key, entry.Value);
                }
            }

            private void CreateDriver(NTSDSoundPlayer owner)
            {
                Assert.That(driver == null && driverField.GetValue(null) == null, Is.True);
                driverHost = OwnHost("H10PcBudgetDriver", false);
                driver = driverHost.AddComponent<NTSD.Simulation.SimulationTickDriver>();
                driver.enabled = false;
                driverHost.SetActive(true);
                Assert.That(driver.TryConfigureEmptyDiagnosticWorld(new NTSD.Simulation.BattleRuntimeWorldSettings(
                    NTSD.Simulation.BattleRuntimeProfile.Authority400,
                    NTSD.Simulation.BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity,
                    NTSD.Simulation.BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity), out string reason), Is.True, reason);
                driver.ApplySettings(new NTSD.Simulation.LockstepSimulationSettings
                {
                    driveMode = NTSD.Simulation.SimulationDriveMode.Manual, requireInputFrameReady = false,
                    enableFrameChecksum = true, captureFullFrameSnapshotForDiagnostics = false
                });
                driver.SetSoundPresentationSinkForDiagnostics(owner);
                Assert.That(ReadPrivateField<NTSDSoundPlayer>(driver, "_battleSoundPlayer"), Is.SameAs(owner),
                    "The real diagnostic seam must capture owner before the first await");
                int capacity = driver.World.RuntimeSlotCapacity;
                driver.World.BattleBuffersForServices.Prepare(capacity, 0);
                typeof(NTSD.Simulation.SimulationTickDriver).GetMethod("PreparePresentationHotPathCapacity", Private)
                    .Invoke(driver, new object[] { capacity });
                Assert.That(driver.World.PendingSounds.Capacity, Is.GreaterThanOrEqualTo(1000));
                Assert.That(ReadPrivateField<int>(driver, "_publishedSoundEventLimit"), Is.GreaterThanOrEqualTo(1000));
                Rows.Add(new { stage = "diagnostic-owner-before-await", capacity,
                    publishedCapacity = ReadPrivateField<int>(driver, "_publishedSoundEventLimit"), profile = "Authority400; audio requests only" });
            }

            private static void AssertNoHotWork(PcBudgetCounters before, PcBudgetCounters after)
            {
                Assert.That(after.Loads, Is.EqualTo(before.Loads));
                Assert.That(after.Builds, Is.EqualTo(before.Builds));
                Assert.That(after.Failures, Is.EqualTo(before.Failures));
                Assert.That(after.Rejections, Is.EqualTo(before.Rejections));
            }

            private async UniTask ExerciseHot(int cycle)
            {
                var sink = new PcBudgetSink(player);
                driver.SetSoundPresentationSinkForDiagnostics(sink);
                var emitter = new PcBudgetEmitter(driver.World);
                driver.World.Register(emitter);
                driver.SetPaused(false);
                driver.SetPaused(true);
                var publish = (Action)Delegate.CreateDelegate(typeof(Action), driver,
                    typeof(NTSD.Simulation.SimulationTickDriver).GetMethod("PublishPendingSoundsAfterChecksum", Private));
                var dispatch = (Action)Delegate.CreateDelegate(typeof(Action), driver,
                    typeof(NTSD.Simulation.SimulationTickDriver).GetMethod("DispatchPublishedSounds", Private));
                var stages = new List<object>(16);
                _ = GC.GetAllocatedBytesForCurrentThread();
                long begin = GC.GetAllocatedBytesForCurrentThread();
                long empty = GC.GetAllocatedBytesForCurrentThread() - begin;
                begin = GC.GetAllocatedBytesForCurrentThread();
                byte[] positive = new byte[97];
                long positiveBytes = GC.GetAllocatedBytesForCurrentThread() - begin;
                GC.KeepAlive(positive);
                Assert.That(empty, Is.Zero);
                Assert.That(positiveBytes, Is.GreaterThanOrEqualTo(97));
                try
                {
                for (int tick = 1; tick <= 10; tick++)
                {
                    bool warm = tick <= 3;
                    emitter.Count = tick == 1 || tick == 4 ? 1 : 1000;
                    emitter.CueCount = tick == 2 || tick == 5 ? 1 : PcBudgetCueIds.Length;
                    int beforeSamples = sink.Count;
                    var before = PcBudgetCounters.Read(player);
                    Assert.That(driver.StepOneTick(NTSD.Simulation.FrameInputSet.Empty(tick),
                        ignorePaused: true, buildPresentation: false), Is.True);
                    Assert.That(sink.Count, Is.EqualTo(beforeSamples + 1));
                    var sample = sink.Samples[beforeSamples];
                    int expectedPlays = Math.Min(emitter.Count, emitter.CueCount);
                    // Same-frame replay of a pre-existing diagnostic publication; measured separately from StepOneTick.
                    before = PcBudgetCounters.Read(player);
                    begin = GC.GetAllocatedBytesForCurrentThread();
                    publish();
                    dispatch();
                    long pipelineBytes = GC.GetAllocatedBytesForCurrentThread() - begin;
                    var after = PcBudgetCounters.Read(player);
                    stages.Add(new { tick, frame = Time.frameCount, warm, requests = emitter.Count,
                        cueCount = emitter.CueCount, aggregated = emitter.Count - expectedPlays,
                        sample, queueAllocated = emitter.QueueAllocated, pipelineBytes, before, after });
                    if (!warm)
                    {
                        Assert.That(sample.Requests, Is.EqualTo(emitter.Count));
                        Assert.That(sample.After.Plays - sample.Before.Plays, Is.EqualTo(expectedPlays));
                        AssertNoHotWork(sample.Before, sample.After);
                        Assert.That(sample.Allocated, Is.Zero, "Prepared NTSDSoundPlayer.PresentSounds allocation");
                        Assert.That(emitter.QueueAllocated, Is.Zero, "World.QueueSound loop allocation");
                        Assert.That(pipelineBytes, Is.Zero, "Publish+dispatch+prepared player managed scope");
                        Assert.That(after.Plays - before.Plays, Is.EqualTo(expectedPlays));
                        AssertNoHotWork(before, after);
                    }
                    Assert.That(driver.RejectedPublishedSoundEventCountForDiagnostics, Is.Zero);
                    Assert.That(driver.World.RejectedSoundEventCountForDiagnostics, Is.Zero);
                    await UniTask.NextFrame();
                }
                Assert.That(player.OneShotVoiceCountForDiagnostics, Is.EqualTo(64));
                }
                finally
                {
                PcBudgetSave("hot-cycle-" + cycle, new { empty, positiveBytes, stages, recordedSinkSamples = sink.Samples.Take(sink.Count).ToArray(),
                    scope = "World queue; prepared PresentSounds; publication+dispatch managed current thread, each separate. Not whole tick/engine GC.",
                    saturation = "Representative real cues exercise retrigger; 65-identity saturation is unchanged prior evidence, not newly claimed here" });
                }
                Rows.Add(new { stage = "hot-complete", cycle, measuredTicks = 7, actualFrames = 10,
                    sameFrameBatchReplay = true, expectedVoiceCapacity = 64 });
            }

            private async UniTask CloseDriver(NTSDSoundPlayer owner, string label,
                AudioSource preserveGenericVoice = null, AudioClip preserveGenericClip = null)
            {
                AudioClip[] owned = Owned(owner);
                int[] ids = owned.Select(x => x.GetInstanceID()).ToArray();
                allTrackedOwned.AddRange(owned);
                var world = driver.World;
                var report = driver.ShutdownBattleRuntime();
                int pendingAtStage5Return = owner.PendingFormalAudioLoadsForDiagnostics;
                int aliveAtReturn = owned.Count(x => x != null);
                Rows.Add(new { stage = "public-shutdown-return", label, report, ids,
                    pendingAtStage5Return, aliveAtReturn, tailDrained = owner.BattleAudioTailDrainedForDiagnostics });
                Assert.That(report.RuntimeStagesCompleted, Is.True, report.FailureReason);
                Assert.That(report.CompletedStage, Is.EqualTo(NTSD.Simulation.BattleRuntimeShutdownStage.ObjectPoolQuiesced));
                Assert.That(driver.World, Is.Null);
                Assert.That(report.RemainingWorldObjects + report.RemainingRuntimeSlots + report.RemainingPoolBorrowers, Is.Zero);
                Assert.That(owner.OwnedFormalAudioSourcesForDiagnostics, Is.Zero);
                Assert.That(Owned(owner), Is.Empty);
                Assert.That(owner.BattleSoundShutdownCompletedForDiagnostics, Is.True);
                if (preserveGenericVoice != null)
                    Assert.That(preserveGenericClip != null && preserveGenericVoice.clip == preserveGenericClip
                        && preserveGenericVoice.isPlaying, Is.True, "Stage5 must not stop the borrowed generic voice");
                // This diagnostic world never creates a map carrier; check the actual map owner before reporting stage11.
                Assert.That(ReadPrivateField<object>(driver, "_battleObjectPool"), Is.Null);
                Assert.That(Resources.FindObjectsOfTypeAll<NTSD.Test.BattleTestBootstrap>()
                    .Where(x => x.gameObject.scene.IsValid()).All(x => x.IsRuntimeMapCleared), Is.True);
                var complete = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true);
                Assert.That(complete.IsComplete, Is.True, complete.FailureReason);
                Assert.That(complete.CompletedStage, Is.EqualTo(NTSD.Simulation.BattleRuntimeShutdownStage.RuntimeMapCleared));
                await WaitForConditionAsync(() => owner.BattleAudioTailDrainedForDiagnostics, 600, label + " drain");
                await UniTask.NextFrame();
                Assert.That(owned.All(x => x == null), Is.True, label + " deferred owned destruction");
                Assert.That(Resources.FindObjectsOfTypeAll<AudioSource>().Any(v => v.clip != null && ids.Contains(v.clip.GetInstanceID())), Is.False);
                Rows.Add(new { stage = "drained-next-frame", label, complete, ownedGone = true,
                    mapBoundary = "Isolated empty diagnostic world; no scene map carrier. Not loaded Battle map teardown." });
                UnityEngine.Object.Destroy(driverHost);
                await UniTask.NextFrame();
                Assert.That(driver == null && driverHost == null, Is.True);
                Assert.That(driverField.GetValue(null) == null, Is.True);
                driver = null; driverHost = null;
            }

            public async UniTask RunThreeSessions()
            {
                for (int cycle = 1; cycle <= 3; cycle++)
                {
                    Assert.That(allTrackedOwned.All(x => x == null), Is.True, "Old generation must retire before new load");
                    CreateDriver(player);
                    await Await(player.PrepareBattleCuesAsync(manager));
                    ObserveCache();
                    Assert.That(player.BattleCatalogSealedForDiagnostics, Is.True);
                    Assert.That(player.PendingFormalAudioLoadsForDiagnostics, Is.Zero);
                    Assert.That(player.FailedPreparedCueLoadCountForDiagnostics, Is.Zero);
                    object first = Cue(player, PcBudgetCueIds[0], true);
                    int generation = ReadPrivateCue<int>(first, "CatalogGeneration");
                    Assert.That(generation, Is.GreaterThan(lastGeneration)); lastGeneration = generation;
                    object nonmono = Cue(player, "c/kim/w/a1.wav", true);
                    var source = ReadPrivateCue<AudioClip>(nonmono, "OwnedSource");
                    Assert.That(source, Is.Not.Null);
                    Assert.That(source.channels, Is.Not.EqualTo(1));
                    Assert.That(ReadPrivateCue<AudioClip[]>(nonmono, "BattlePlaybackClips")[0], Is.SameAs(source));
                    var owned = Owned(player);
                    Rows.Add(new { stage = "prepared", cycle, generation, ownedIds = owned.Select(x => x.GetInstanceID()).ToArray(),
                        ownedBytes = owned.Sum(x => Profiler.GetRuntimeMemorySizeLong(x)),
                        nonmonoCountedOnce = true, cacheCount = cache.Count,
                        privateBytes = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64,
                        processScope = "Unity Editor diagnostic sample; not game-process budget verdict" });
                    await ExerciseHot(cycle);
                    AudioClip playback = ReadPrivateCue<AudioClip[]>(Cue(player, PcBudgetCueIds[0], true), "BattlePlaybackClips")[0];
                    player.PresentSound(new NTSD.Simulation.PendingSoundEvent(PcBudgetCueIds[0], 500, 20));
                    var voice = FindVoiceWithClip(audioHost, playback);
                    Assert.That(voice != null && voice.enabled && voice.gameObject.activeInHierarchy && voice.isPlaying, Is.True);
                    float outputMax = 0f;
                    for (int i = 0; i < 8; i++)
                    {
                        await UniTask.NextFrame();
                        AudioListener.GetOutputData(outputSamples, 0);
                        for (int n = 0; n < outputSamples.Length; n++) outputMax = Mathf.Max(outputMax, Mathf.Abs(outputSamples[n]));
                    }
                    object generic = Cue(player, "data/001.wav", false);
                    AudioClip genericClip = ReadPrivateCue<AudioClip[]>(generic, "Clips")[0];
                    Assert.That(genericClip, Is.Not.Null);
                    int genericId = genericClip.GetInstanceID();
                    if (firstGenericId == 0) firstGenericId = genericId;
                    Assert.That(genericId, Is.EqualTo(firstGenericId));
                    player.PlaySfx("data/001.wav");
                    var genericVoice = FindVoiceWithClip(audioHost, genericClip);
                    Assert.That(genericVoice != null && genericVoice.isPlaying, Is.True);
                    Rows.Add(new { stage = "play-evidence", cycle, voiceEnabled = voice.enabled,
                        voicePlayingObserved = true, listenerEnabled = listener.enabled, outputMax, outputSamples = outputSamples.Length * 8,
                        hearingVerified = false, genericId });
                    await CloseDriver(player, "cycle-" + cycle, genericVoice, genericClip);
                    Assert.That(genericClip, Is.Not.Null);
                    ObserveCache();
                    PcBudgetSave("cycle-" + cycle, new { cycle, generation, genericId, ownedGone = true, rows = Rows.ToArray() });
                }
            }

            public async UniTask RunPendingBoundaries()
            {
                CreateDriver(player);
                long staleBefore = player.StaleFormalAudioReleaseCountForDiagnostics;
                UniTask pending = player.PrepareBattleCuesAsync(manager).Preserve();
                operations.Add(pending);
                await WaitForConditionAsync(() => player.PendingFormalAudioLoadsForDiagnostics > 0, 600, "real public prepare pending load");
                await CloseDriver(player, "pending-public-prepare-cancel");
                await pending;
                Assert.That(player.StaleFormalAudioReleaseCountForDiagnostics, Is.GreaterThan(staleBefore));
                ObserveCache();
                Rows.Add(new { stage = "pending-cancel", staleBefore, staleAfter = player.StaleFormalAudioReleaseCountForDiagnostics,
                    staleId = player.LastStaleFormalAudioInstanceIdForDiagnostics, drained = player.BattleAudioTailDrainedForDiagnostics });
                var naturalHost = OwnHost("H10PcBudgetNaturalDestroy", false);
                var natural = naturalHost.AddComponent<NTSDSoundPlayer>();
                naturalHost.SetActive(true);
                UniTask naturalPending = natural.PrepareBattleCuesAsync(manager).Preserve();
                operations.Add(naturalPending);
                await WaitForConditionAsync(() => natural.PendingFormalAudioLoadsForDiagnostics > 0, 600, "natural destroy pending load");
                int hostId = naturalHost.GetInstanceID();
                UnityEngine.Object.Destroy(naturalHost);
                await UniTask.NextFrame();
                await Await(naturalPending);
                await UniTask.NextFrame();
                Assert.That(naturalHost == null && natural == null, Is.True);
                ObserveCache();
                Rows.Add(new { stage = "natural-destroy-await", hostId, destroyedByUnityCallback = true, pendingOperationCompleted = true });
            }

            public async UniTask Finish()
            {
                if (finished) return;
                if (driver != null)
                {
                    await CloseDriver(player, "failure-cleanup");
                }
                if (player != null)
                {
                    RequireProductionMethod("BeginBattleSoundShutdown", 0).Invoke(player, null);
                    RequireProductionMethod("CompleteBattleSoundShutdown", 0).Invoke(player, null);
                }
                foreach (UniTask operation in operations)
                {
                    double deadline = UnityEditor.EditorApplication.timeSinceStartup + 120;
                    while (operation.Status == UniTaskStatus.Pending)
                    {
                        if (UnityEditor.EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Pending owner retained");
                        await UniTask.NextFrame();
                    }
                }
                ObserveCache();
                foreach (GameObject host in hosts) if (host != null) UnityEngine.Object.Destroy(host);
                await UniTask.NextFrame();
                Assert.That(hosts.All(x => x == null), Is.True);
                Assert.That(Resources.FindObjectsOfTypeAll<NTSDSoundPlayer>().Any(x => x.gameObject.scene.IsValid()), Is.False);
                var candidates = addedCache.Values.SelectMany(EnvelopeFixture.CacheClips).Where(x => x != null).Distinct().ToArray();
                foreach (AudioClip clip in candidates)
                {
                    Assert.That(initialClipIds.Contains(clip.GetInstanceID()), Is.False, "Pre-existing native clip cannot be released");
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioSource>().Any(x => x.clip == clip), Is.False);
                    Assert.That(Resources.FindObjectsOfTypeAll<AudioController>().SelectMany(x => x.AudioList ?? Array.Empty<AudioItem>())
                        .Where(x => x != null).SelectMany(x => x.clip ?? Array.Empty<AudioClip>()).Any(x => x == clip), Is.False);
                    foreach (DictionaryEntry entry in cache)
                        if (!addedCache.ContainsKey((string)entry.Key)) Assert.That(EnvelopeFixture.CacheClips(entry.Value).Any(x => x == clip), Is.False);
                }
                foreach (var entry in addedCache)
                {
                    Assert.That(cache.Contains(entry.Key) && ReferenceEquals(cache[entry.Key], entry.Value), Is.True, entry.Key);
                    NTSD_ResourceLoader.Instance.RemoveCache(entry.Key);
                }
                int[] ids = candidates.Select(x => x.GetInstanceID()).ToArray();
                foreach (AudioClip clip in candidates) UnityEngine.Object.Destroy(clip);
                await UniTask.NextFrame();
                Assert.That(candidates.All(x => x == null), Is.True);
                Assert.That(CacheMatches(cache, beforeCache), Is.True);
                Assert.That(allTrackedOwned.All(x => x == null), Is.True);
                var remainingClips = Resources.FindObjectsOfTypeAll<AudioClip>();
                Assert.That(remainingClips.Select(x => x.GetInstanceID()), Is.EquivalentTo(initialClipIds),
                    "The fixture must leave no new native AudioClip and preserve all pre-existing ones");
                Assert.That(NTSD_ResourceLoader.Instance.GetPendingRequestCount(), Is.Zero);
                Assert.That(NTSD_ResourceLoader.Instance.GetQueuedTaskCount(), Is.Zero);
                configField.SetValue(null, oldConfig); managerField.SetValue(null, oldManager);
                mmField.SetValue(null, oldMm); driverField.SetValue(null, oldDriver);
                UnityEngine.Random.state = randomBefore;
                finished = true;
                PcBudgetSave("fixture-cleanup", new { exactRemovedKeys = addedCache.Keys.ToArray(), ids, cacheRestored = true,
                    allOwnedHostsGone = true, allTrackedOwnedGone = true, loaderIdle = true, singletonBindingsRestored = true });
            }
        }

    }
}
#endif
