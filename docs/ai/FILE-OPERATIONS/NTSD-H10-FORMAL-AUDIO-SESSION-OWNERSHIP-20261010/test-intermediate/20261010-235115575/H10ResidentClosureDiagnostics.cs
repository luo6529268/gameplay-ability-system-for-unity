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

                        long playsBeforeStage1 =
                            fixture.Player.PooledOneShotPlayCountForDiagnostics;
                        fixture.BeginShutdown();
                        fixture.Player.PresentSound(
                            new NTSD.Simulation.PendingSoundEvent(
                                fixture.FixedCueIds[0], 500, cycle + 10));
                        Assert.That(
                            fixture.Player.PooledOneShotPlayCountForDiagnostics,
                            Is.EqualTo(playsBeforeStage1));
                        await DrainFormalAudioAsync(
                            fixture.Player,
                            180,
                            "three-cycle stage1/stage5 drain " + cycle);
                        fixture.CompleteShutdown();
                        AssertShutdownReleased(fixture);
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
                        await DrainFormalAudioAsync(
                            fixture.Player,
                            180,
                            "cold reentry stage1/stage5 drain " + cycle);
                        fixture.CompleteShutdown();
                        AssertShutdownReleased(fixture);

                        evidence.Add(new
                        {
                            cycle,
                            controllerSameAcrossCycle =
                                ReferenceEquals(AudioController.Instance,
                                    fixture.PreviousAudioController),
                            generation,
                            cueEvidence,
                            stage5CueEvidence,
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
                                await DrainFormalAudioAsync(
                                    fixture.Player,
                                    180,
                                    "three-cycle final cleanup drain");
                                fixture.CompleteShutdown();
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
                    await AwaitWithTimeout(load, "local formal load cancellation");
                    await DrainFormalAudioAsync(
                        fixture.Player,
                        180,
                        "local formal load cancellation drain");
                    fixture.CompleteShutdown();
                    AssertShutdownReleased(fixture);
                    Assert.That(
                        fixture.Player.StaleFormalAudioReleaseCountForDiagnostics,
                        Is.GreaterThan(staleBefore));
                    Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(
                        cue, "Clips")), Is.True);
                    Assert.That(fixture.ReadCue<AudioClip>(
                        cue, "OwnedSource"), Is.Null);
                    evidence.Add(new
                    {
                        sourcePath,
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
                    await AwaitWithTimeout(load, "natural OnDestroy formal load");
                    Assert.That(fixture.Player == null, Is.True,
                        "The active host must reach the real Unity OnDestroy path.");
                    long staleAfter = ReadPrivateField<long>(fixture.Player,
                        "staleFormalAudioReleaseCount");
                    int pendingAfter = ReadPrivateField<int>(fixture.Player,
                        "pendingFormalAudioLoads");
                    Assert.That(staleAfter, Is.GreaterThan(staleBefore));
                    Assert.That(pendingAfter, Is.Zero);
                    Assert.That(IsEmptyOrNull(fixture.ReadCue<AudioClip[]>(
                        cue, "Clips")), Is.True);
                    Assert.That(fixture.ReadCue<AudioClip>(
                        cue, "OwnedSource"), Is.Null);
                    evidence.Add(new
                    {
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

        private static void AssertShutdownReleased(FormalAudioSessionFixture fixture)
        {
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
                    if (PreviousMMSoundManager == null && current is UnityEngine.Object currentObject &&
                        currentObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(currentObject);
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
    }
}
#endif
