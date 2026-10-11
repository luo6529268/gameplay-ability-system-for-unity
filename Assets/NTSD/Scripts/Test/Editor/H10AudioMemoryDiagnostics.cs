#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class H10AudioMemoryDiagnostics
    {
        private const int BlockFrames = 4096;
        private const string Output = "artifacts/diagnostics/NTSD-H10-AUDIO-MEMORY-REVIEW-20261010";
        private static readonly string Invocation = Guid.NewGuid().ToString("N");
        private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        private sealed class Row
        {
            public string name;
            public int samples, channels, frequency, blockArrays;
            public long originalTemporaryFloatPayload, blockTemporaryFloatPayload;
            public long sourceObjectMemory, originalPlaybackObjectMemory, blockPlaybackObjectMemory;
            public bool sampleBitsEqual, inputUnchanged, borrowedSourceSurvived, ownedClipsReleased;
        }

        [Test]
        public void BlockMirrorMatchesActualPrivateConversionAtBoundaries()
        {
            var rows = new List<Row>();
            foreach (int frames in new[] { 1, 2, 4095, 4096, 4097, 8191, 8192, 8193, 65537 })
            {
                AudioClip source = AudioClip.Create("H10Boundary" + frames, frames, 1, 44100, false);
                try
                {
                    var input = new float[frames];
                    for (int i = 0; i < input.Length; i++)
                        input[i] = ((i * 37 % 2047) - 1023) / 1024f;
                    Assert.That(source.SetData(input, 0), Is.True);
                    rows.Add(Compare(source, source.name));
                }
                finally { UnityEngine.Object.DestroyImmediate(source); }
            }
            AudioClip stereo = AudioClip.Create("H10StereoBorrowed", 4097, 2, 48000, false);
            try { rows.Add(Compare(stereo, stereo.name)); }
            finally { UnityEngine.Object.DestroyImmediate(stereo); }
            Save("boundaries.json", rows);
        }

        [UnityTest]
        public IEnumerator RealWavsMatchActualPrivateConversionAndBoundedBuffers()
        {
            var rows = new List<Row>();
            foreach (string relative in new[] { "c/jira/w/e3.wav", "c/kar/w/t.wav", "c/dan/w/t.wav", "data/001.wav" })
            {
                string path = Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/vfs/" + relative);
                Assert.That(File.Exists(path), Is.True, path);
                AudioClip source = null;
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri, AudioType.WAV))
                {
                    yield return request.SendWebRequest();
                    try
                    {
                        Assert.That(request.result, Is.EqualTo(UnityWebRequest.Result.Success), request.error);
                        source = DownloadHandlerAudioClip.GetContent(request);
                        Assert.That(source, Is.Not.Null);
                        rows.Add(Compare(source, relative));
                    }
                    finally { if (source != null) UnityEngine.Object.DestroyImmediate(source); }
                }
            }
            Save("real-wavs.json", rows);
        }

        [Test]
        public void BoundedColdConversionCostGroup()
        {
            var rows = new List<object>();
            foreach (int frames in new[] { 65537, 329244 })
            {
                AudioClip source = AudioClip.Create("H10Cost" + frames, frames, 1, 44100, false);
                GameObject host = new GameObject("H10CostOwner") { hideFlags = HideFlags.HideAndDontSave };
                host.SetActive(false);
                var player = host.AddComponent<NTSDSoundPlayer>();
                Type cueType = typeof(NTSDSoundPlayer).GetNestedType("PreparedSoundCue", BindingFlags.NonPublic);
                MethodInfo method = typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips", Hidden);
                MethodInfo releaseMethod = typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Hidden);
                var param = Expression.Parameter(typeof(object), "cue");
                Action<object> convert = Expression.Lambda<Action<object>>(Expression.Call(Expression.Constant(player), method, Expression.Convert(param, cueType)), param).Compile();
                Action release = (Action)Delegate.CreateDelegate(typeof(Action), player, releaseMethod);
                var input = new float[frames];
                for (int i = 0; i < input.Length; i++) input[i] = ((i * 37 % 2047) - 1023) / 1024f;
                Assert.That(source.SetData(input, 0), Is.True);
                try
                {
                    foreach (bool blocks in new[] { false, true, true, false })
                    {
                        var ticks = new long[16];
                        for (int iteration = -4; iteration < ticks.Length; iteration++)
                        {
                            object cue = Activator.CreateInstance(cueType, true);
                            cueType.GetField("Clips").SetValue(cue, new[] { source });
                            cueType.GetField("IsLoaded").SetValue(cue, true);
                            AudioClip result = null;
                            try
                            {
                                long start = System.Diagnostics.Stopwatch.GetTimestamp();
                                if (blocks) result = ConvertBlocks(source, out _, out _);
                                else convert(cue);
                                long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - start;
                                if (iteration >= 0) ticks[iteration] = elapsed;
                                if (!blocks) result = ((AudioClip[])cueType.GetField("BattlePlaybackClips").GetValue(cue))[0];
                                Assert.That(result.samples, Is.EqualTo(frames));
                                var actual = new float[frames * 2];
                                Assert.That(result.GetData(actual, 0), Is.True);
                                for (int i = 0; i < actual.Length; i++)
                                    if (BitConverter.SingleToInt32Bits(actual[i]) != BitConverter.SingleToInt32Bits(input[i / 2])) Assert.Fail("Cost PCM mismatch");
                            }
                            finally
                            {
                                release();
                                if (blocks && result != null) UnityEngine.Object.DestroyImmediate(result);
                            }
                        }
                        rows.Add(new { frames, blocks, ticks, frequency = System.Diagnostics.Stopwatch.Frequency,
                            meanMs = ticks.Average() * 1000 / System.Diagnostics.Stopwatch.Frequency,
                            warm = 4, sample = 16, scope = "cold conversion with AudioClip creation; no readback/assertion/cleanup, allocations expected" });
                    }
                }
                finally { release(); UnityEngine.Object.DestroyImmediate(host); UnityEngine.Object.DestroyImmediate(source); }
            }
            Save("conversion-cost.json", rows);
        }

        private static Row Compare(AudioClip source, string name)
        {
            var row = new Row { name = name, samples = source.samples, channels = source.channels, frequency = source.frequency };
            GameObject host = new GameObject("H10IsolatedPcmReview") { hideFlags = HideFlags.HideAndDontSave };
            host.SetActive(false);
            NTSDSoundPlayer player = host.AddComponent<NTSDSoundPlayer>();
            AudioClip block = null;
            AudioClip original = null;
            var before = new float[source.samples * source.channels];
            Assert.That(source.GetData(before, 0), Is.True);
            try
            {
                Type cueType = typeof(NTSDSoundPlayer).GetNestedType("PreparedSoundCue", BindingFlags.NonPublic);
                object cue = Activator.CreateInstance(cueType, true);
                cueType.GetField("Clips").SetValue(cue, new[] { source });
                cueType.GetField("IsLoaded").SetValue(cue, true);
                typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips", Hidden).Invoke(player, new[] { cue });
                original = ((AudioClip[])cueType.GetField("BattlePlaybackClips").GetValue(cue))[0];
                block = ConvertBlocks(source, out row.blockTemporaryFloatPayload, out row.blockArrays);
                row.originalTemporaryFloatPayload = source.channels == 1 ? (long)source.samples * 12 : 0;
                row.sourceObjectMemory = Profiler.GetRuntimeMemorySizeLong(source);
                row.originalPlaybackObjectMemory = Profiler.GetRuntimeMemorySizeLong(original);
                row.blockPlaybackObjectMemory = Profiler.GetRuntimeMemorySizeLong(block);
                Assert.That(block.samples, Is.EqualTo(original.samples));
                Assert.That(block.channels, Is.EqualTo(original.channels));
                Assert.That(block.frequency, Is.EqualTo(original.frequency));
                var left = new float[original.samples * original.channels];
                var right = new float[block.samples * block.channels];
                Assert.That(original.GetData(left, 0), Is.True);
                Assert.That(block.GetData(right, 0), Is.True);
                for (int i = 0; i < left.Length; i++)
                {
                    if (BitConverter.SingleToInt32Bits(left[i]) != BitConverter.SingleToInt32Bits(right[i]))
                        Assert.Fail(name + " first PCM bit mismatch at " + i);
                    if (source.channels == 1 && BitConverter.SingleToInt32Bits(right[i]) != BitConverter.SingleToInt32Bits(before[i / 2]))
                        Assert.Fail(name + " stereo channel mismatch at " + i);
                }
                row.sampleBitsEqual = true;
                var after = new float[before.Length];
                Assert.That(source.GetData(after, 0), Is.True);
                CollectionAssert.AreEqual(before, after);
                row.inputUnchanged = true;
                Assert.That(row.blockTemporaryFloatPayload, Is.LessThanOrEqualTo(row.originalTemporaryFloatPayload));
                if (source.channels == 1 && source.samples >= BlockFrames * 2)
                    Assert.That(row.blockTemporaryFloatPayload, Is.LessThan(row.originalTemporaryFloatPayload));
                Assert.That(row.blockTemporaryFloatPayload, Is.LessThan(BlockFrames * 24L));
                if (source.channels != 1)
                {
                    Assert.That(ReferenceEquals(block, source), Is.True);
                    Assert.That(ReferenceEquals(original, source), Is.True);
                }
            }
            finally
            {
                // Explicit original owner cleanup here is a diagnostic cleanup, not a natural lifecycle certificate.
                typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Hidden).Invoke(player, null);
                if (block != null && !ReferenceEquals(block, source)) UnityEngine.Object.DestroyImmediate(block);
                row.borrowedSourceSurvived = source != null;
                row.ownedClipsReleased = (original == null || ReferenceEquals(original, source)) && (block == null || ReferenceEquals(block, source));
                UnityEngine.Object.DestroyImmediate(host);
            }
            Assert.That(row.borrowedSourceSurvived, Is.True);
            Assert.That(row.ownedClipsReleased, Is.True);
            return row;
        }

        private static AudioClip ConvertBlocks(AudioClip source, out long temporaryPayload, out int arrays)
        {
            temporaryPayload = 0;
            arrays = 0;
            if (source.channels != 1) return source;
            int frames = source.samples < BlockFrames * 2 ? source.samples : BlockFrames;
            var mono = new float[frames];
            var stereo = new float[frames * 2];
            arrays = 2;
            temporaryPayload = frames * 12L;
            AudioClip output = AudioClip.Create(source.name, source.samples, 2, source.frequency, false);
            try
            {
                int offset = 0;
                while (offset + frames <= source.samples)
                {
                    CopyBlock(source, output, mono, stereo, offset);
                    offset += frames;
                }
                int remaining = source.samples - offset;
                if (remaining > 0)
                {
                    // Exact tail lengths prevent SetData wrapping the tail over the beginning of the clip.
                    var tailMono = new float[remaining];
                    var tailStereo = new float[remaining * 2];
                    arrays += 2;
                    temporaryPayload += remaining * 12L;
                    CopyBlock(source, output, tailMono, tailStereo, offset);
                }
                return output;
            }
            catch { UnityEngine.Object.DestroyImmediate(output); throw; }
        }

        private static void CopyBlock(AudioClip input, AudioClip output, float[] mono, float[] stereo, int offset)
        {
            if (!input.GetData(mono, offset)) throw new InvalidOperationException("Cannot read block PCM");
            for (int i = 0; i < mono.Length; i++) stereo[i * 2] = stereo[i * 2 + 1] = mono[i];
            if (!output.SetData(stereo, offset)) throw new InvalidOperationException("Cannot write block PCM");
        }


        private const string ProductionOutput = "artifacts/diagnostics/NTSD-H10-BOUNDED-PCM-CONVERSION-20261010";

        private sealed class FrozenCue
        {
            public bool IsLoaded;
            public AudioClip[] Clips;
            public AudioClip[] BattlePlaybackClips;
        }

        private sealed class FrozenOwner
        {
            private readonly NTSDSoundPlayer owner;
            internal readonly List<AudioClip> ownedBattlePlaybackClips = new List<AudioClip>();
            internal FrozenOwner(NTSDSoundPlayer owner) { this.owner = owner; }

            internal void PrepareBattlePlaybackClips(FrozenCue cue)
            {
                if (owner == null || cue == null || !cue.IsLoaded || cue.BattlePlaybackClips != null)
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


            internal void Release()
            {
                foreach (var clip in ownedBattlePlaybackClips)
                    if (clip != null) UnityEngine.Object.DestroyImmediate(clip);
                ownedBattlePlaybackClips.Clear();
            }
        }

        private sealed class ProductionFixture : IDisposable
        {
            internal readonly GameObject host;
            internal readonly NTSDSoundPlayer player;
            internal readonly FrozenOwner frozen;
            internal readonly Action<object> convert;
            internal readonly Action release;
            private readonly Type cueType;
            private readonly List<AudioClip> observed = new List<AudioClip>(192);
            private readonly List<object> identities = new List<object>(192);
            private readonly HashSet<int> baseline;
            private readonly string id = Guid.NewGuid().ToString("N");

            internal ProductionFixture()
            {
                baseline = new HashSet<int>(Resources.FindObjectsOfTypeAll<AudioClip>().Select(c => c.GetInstanceID()));
                host = new GameObject("H10ProductionOwner_" + id) { hideFlags = HideFlags.HideAndDontSave };
                host.SetActive(false);
                player = host.AddComponent<NTSDSoundPlayer>();
                frozen = new FrozenOwner(player);
                cueType = typeof(NTSDSoundPlayer).GetNestedType("PreparedSoundCue", BindingFlags.NonPublic);
                var param = Expression.Parameter(typeof(object), "cue");
                convert = Expression.Lambda<Action<object>>(Expression.Call(Expression.Constant(player),
                    typeof(NTSDSoundPlayer).GetMethod("PrepareBattlePlaybackClips", Hidden),
                    Expression.Convert(param, cueType)), param).Compile();
                release = (Action)Delegate.CreateDelegate(typeof(Action), player,
                    typeof(NTSDSoundPlayer).GetMethod("ReleaseBattlePlaybackClips", Hidden));
            }

            internal List<AudioClip> Owned => (List<AudioClip>)typeof(NTSDSoundPlayer)
                .GetField("ownedBattlePlaybackClips", Hidden).GetValue(player);

            internal object Cue(AudioClip[] clips, bool loaded = true, AudioClip[] ready = null)
            {
                object cue = Activator.CreateInstance(cueType, true);
                cueType.GetField("Clips").SetValue(cue, clips);
                cueType.GetField("IsLoaded").SetValue(cue, loaded);
                cueType.GetField("BattlePlaybackClips").SetValue(cue, ready);
                return cue;
            }

            internal AudioClip[] Results(object cue) => (AudioClip[])cueType.GetField("BattlePlaybackClips").GetValue(cue);

            internal void Observe()
            {
                foreach (var clip in Owned.Concat(frozen.ownedBattlePlaybackClips))
                {
                    if (clip == null || observed.Any(v => ReferenceEquals(v, clip))) continue;
                    Assert.That(baseline.Contains(clip.GetInstanceID()), Is.False);
                    identities.Add(new { instanceId = clip.GetInstanceID(), clip.name, clip.samples, clip.channels, clip.frequency });
                    observed.Add(clip);
                }
            }

            internal void Release()
            {
                Observe();
                release();
                frozen.Release();
                Assert.That(Owned.Count, Is.Zero);
                Assert.That(frozen.ownedBattlePlaybackClips.Count, Is.Zero);
                Assert.That(observed.All(v => v == null), Is.True);
            }

            public void Dispose()
            {
                bool released = false;
                try { Release(); released = true; }
                finally
                {
                    if (host != null) UnityEngine.Object.DestroyImmediate(host);
                    SaveProduction("owner-" + id, new
                    {
                        test = TestContext.CurrentContext.Test.FullName, released, explicitOwnerCleanup = true,
                        naturalCallbackCertified = false, baselineCount = baseline.Count,
                        allObservedDestroyed = observed.All(v => v == null), resources = identities
                    });
                }
            }
        }

        private static void SaveProduction(string name, object data)
        {
            Directory.CreateDirectory(ProductionOutput);
            using var stream = new FileStream(Path.Combine(ProductionOutput, name + "-" + Invocation + ".json"),
                FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        private static float[] InputSamples(int frames, int channels = 1)
        {
            var result = new float[checked(frames * channels)];
            for (int i = 0; i < result.Length; i++) result[i] = ((i * 37 % 2047) - 1023) / 1024f;
            return result;
        }

        private static void AssertBits(float[] expected, float[] actual, string label)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length), label);
            for (int i = 0; i < expected.Length; i++)
                if (BitConverter.SingleToInt32Bits(expected[i]) != BitConverter.SingleToInt32Bits(actual[i]))
                    Assert.Fail(label + " first bit mismatch at " + i);
        }

        private static float[] ReadSamples(AudioClip clip)
        {
            var data = new float[checked(clip.samples * clip.channels)];
            Assert.That(clip.GetData(data, 0), Is.True);
            return data;
        }

        private static object CompareProduction(AudioClip source, string label)
        {
            using var fixture = new ProductionFixture();
            float[] input = ReadSamples(source);
            var frozenCue = new FrozenCue { IsLoaded = true, Clips = new[] { source } };
            object actualCue = fixture.Cue(new[] { source });
            fixture.frozen.PrepareBattlePlaybackClips(frozenCue);
            fixture.convert(actualCue);
            fixture.Observe();
            AudioClip before = frozenCue.BattlePlaybackClips[0], after = fixture.Results(actualCue)[0];
            Assert.That(after.name, Is.EqualTo(before.name).And.EqualTo(source.name));
            Assert.That(after.samples, Is.EqualTo(before.samples).And.EqualTo(source.samples));
            Assert.That(after.channels, Is.EqualTo(before.channels).And.EqualTo(source.channels == 1 ? 2 : source.channels));
            Assert.That(after.frequency, Is.EqualTo(before.frequency).And.EqualTo(source.frequency));
            float[] original = ReadSamples(before), actual = ReadSamples(after);
            AssertBits(original, actual, label);
            for (int i = 0; i < actual.Length; i++)
                if (BitConverter.SingleToInt32Bits(actual[i]) != BitConverter.SingleToInt32Bits(input[source.channels == 1 ? i / 2 : i]))
                    Assert.Fail(label + " channel mapping differs at " + i);
            AssertBits(input, ReadSamples(source), label + " source");
            long oldMemory = Profiler.GetRuntimeMemorySizeLong(before), newMemory = Profiler.GetRuntimeMemorySizeLong(after);
            Assert.That(newMemory, Is.EqualTo(oldMemory));
            int tail = source.samples % BlockFrames;
            bool bounded = source.channels == 1 && source.samples >= BlockFrames * 2;
            long originalPayload = source.channels == 1 ? source.samples * 12L : 0;
            long payload = bounded ? (BlockFrames + tail) * 12L : originalPayload;
            int arrays = source.channels != 1 ? 0 : bounded && tail != 0 ? 4 : 2;
            int calls = source.channels != 1 ? 0 : bounded ? (source.samples + BlockFrames - 1) / BlockFrames : 1;
            if (bounded)
                Assert.That(payload, Is.LessThan(98304L).And.LessThan(originalPayload));
            if (source.channels != 1)
            {
                Assert.That(before, Is.SameAs(source));
                Assert.That(after, Is.SameAs(source));
            }
            object evidence = new
            {
                label, source.name, source.samples, source.channels, source.frequency,
                originalFloatPayloadBytes = originalPayload, productionFloatPayloadBytes = payload,
                productionFloatArrayCount = arrays, getDataCalls = calls, setDataCalls = calls,
                floatCapacitySource = "source-derived; excludes array headers and test readback buffers",
                originalPlaybackObjectBytes = oldMemory, productionPlaybackObjectBytes = newMemory,
                exactMetadataAndPcmBits = true, sourceBitsUnchanged = true
            };
            fixture.Release();
            Assert.That(source != null, Is.True);
            return evidence;
        }

        [Test]
        public void ProductionPcmMatchesFrozenOriginalAtBoundaries()
        {
            var rows = new List<object>();
            foreach (int frames in new[] { 1, 2, 4095, 4096, 4097, 8191, 8192, 8193, 16384, 65537 })
            {
                AudioClip source = AudioClip.Create("H10ProductionBoundary" + frames, frames, 1, 44100, false);
                try
                {
                    Assert.That(source.SetData(InputSamples(frames), 0), Is.True);
                    rows.Add(CompareProduction(source, source.name));
                }
                finally { UnityEngine.Object.DestroyImmediate(source); }
            }
            AudioClip stereo = AudioClip.Create("H10ProductionStereo", 4097, 2, 48000, false);
            try
            {
                Assert.That(stereo.SetData(InputSamples(stereo.samples, 2), 0), Is.True);
                rows.Add(CompareProduction(stereo, stereo.name));
            }
            finally { UnityEngine.Object.DestroyImmediate(stereo); }
            SaveProduction("production-boundaries", rows);
        }

        [UnityTest]
        public IEnumerator ProductionPcmMatchesFrozenOriginalForFourWavs()
        {
            var rows = new List<object>();
            foreach (string relative in new[] { "c/jira/w/e3.wav", "c/kar/w/t.wav", "c/dan/w/t.wav", "data/001.wav" })
            {
                string path = Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/vfs/" + relative);
                using var request = UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri, AudioType.WAV);
                yield return request.SendWebRequest();
                AudioClip source = null;
                try
                {
                    Assert.That(request.result, Is.EqualTo(UnityWebRequest.Result.Success), request.error);
                    source = DownloadHandlerAudioClip.GetContent(request);
                    rows.Add(CompareProduction(source, relative));
                }
                finally { if (source != null) UnityEngine.Object.DestroyImmediate(source); }
            }
            SaveProduction("production-wavs", rows);
        }

        [Test]
        public void ProductionCueGuardsBorrowingOrderAndDestroyedOwner()
        {
            using var fixture = new ProductionFixture();
            var sources = new[]
            {
                AudioClip.Create("H10OrderShort", 4097, 1, 44100, false),
                AudioClip.Create("H10OrderStereo", 8193, 2, 48000, false),
                AudioClip.Create("H10OrderLong", 8193, 1, 22050, false)
            };
            try
            {
                foreach (var source in sources) Assert.That(source.SetData(InputSamples(source.samples, source.channels), 0), Is.True);
                fixture.convert(null);
                object notLoaded = fixture.Cue(sources, false);
                fixture.convert(notLoaded);
                Assert.That(fixture.Results(notLoaded), Is.Null);
                var ready = new[] { sources[1] };
                object prepared = fixture.Cue(sources, true, ready);
                fixture.convert(prepared);
                Assert.That(fixture.Results(prepared), Is.SameAs(ready));
                Assert.That(fixture.Owned, Is.Empty);
                object empty = fixture.Cue(null);
                fixture.convert(empty);
                Assert.That(fixture.Results(empty), Is.Empty);
                object cue = fixture.Cue(new[] { sources[0], null, sources[1], sources[2] });
                fixture.convert(cue);
                fixture.Observe();
                var result = fixture.Results(cue);
                Assert.That(result.Length, Is.EqualTo(4));
                Assert.That(result[1], Is.Null);
                Assert.That(result[2], Is.SameAs(sources[1]));
                Assert.That(result[0].name, Is.EqualTo(sources[0].name));
                Assert.That(result[3].name, Is.EqualTo(sources[2].name));
                Assert.That(result[0], Is.Not.SameAs(sources[0]));
                Assert.That(result[3], Is.Not.SameAs(sources[2]));
                Assert.That(fixture.Owned.Count, Is.EqualTo(2));
                fixture.convert(cue);
                Assert.That(fixture.Results(cue), Is.SameAs(result));
                Assert.That(fixture.Owned.Count, Is.EqualTo(2));
                fixture.Release();
                Assert.That(sources.All(v => v != null), Is.True);
                UnityEngine.Object.DestroyImmediate(fixture.host);
                object late = fixture.Cue(sources);
                fixture.convert(late);
                Assert.That(fixture.Results(late), Is.Null);
                Assert.That(fixture.Owned, Is.Empty);
                SaveProduction("production-cue-guards", new
                {
                    nullCue = true, notLoaded = true, preparedIdentity = true, empty = true, nullBorrowed = true,
                    stereoBorrowed = true, order = true, idempotent = true, borrowedSurvived = true,
                    destroyedOwnerCannotRecreate = true, explicitReleaseBeforeDestroy = true
                });
            }
            finally { foreach (var source in sources) if (source != null) UnityEngine.Object.DestroyImmediate(source); }
        }

        private static void ExpectStreamingReadError()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(
                "(?i)(stream|GetData|sample data)"));
        }

        [Test]
        public void ProductionStreamingReadFailureQualification()
        {
            AudioClip source = AudioClip.Create("H10StreamingQualification", 8193, 1, 44100, true);
            var errors = new List<string>();
            Application.LogCallback observer = (message, stack, type) => { if (type == LogType.Error) errors.Add(message); };
            Application.logMessageReceived += observer;
            try
            {
                ExpectStreamingReadError();
                bool result = source.GetData(new float[4096], 0);
                SaveProduction("streaming-read-qualification", new { result, errors, source.samples, source.channels });
                Assert.That(result, Is.False);
                Assert.That(errors.Count, Is.EqualTo(1));
            }
            finally
            {
                Application.logMessageReceived -= observer;
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void ProductionFailedReadRollsBackOnlyThisCueWithoutPublication()
        {
            using var fixture = new ProductionFixture();
            string prefix = "H10Failure_" + Guid.NewGuid().ToString("N");
            AudioClip priorSource = AudioClip.Create(prefix + "_prior", 8193, 1, 44100, false);
            AudioClip validSource = AudioClip.Create(prefix + "_valid", 16384, 1, 44100, false);
            AudioClip invalidSource = AudioClip.Create(prefix + "_stream", 8193, 1, 44100, true);
            try
            {
                object prior = fixture.Cue(new[] { priorSource });
                fixture.convert(prior);
                fixture.Observe();
                AudioClip retained = fixture.Results(prior)[0];
                var baselineIds = Resources.FindObjectsOfTypeAll<AudioClip>()
                    .Where(v => v.name.StartsWith(prefix, StringComparison.Ordinal)).Select(v => v.GetInstanceID()).OrderBy(v => v).ToArray();
                var rows = new List<object>();
                foreach (bool laterClip in new[] { false, true })
                {
                    object cue = fixture.Cue(laterClip ? new[] { validSource, invalidSource } : new[] { invalidSource });
                    ExpectStreamingReadError();
                    var error = Assert.Throws<InvalidOperationException>(() => fixture.convert(cue));
                    Assert.That(error.Message, Is.EqualTo("Cannot prepare battle PCM: " + invalidSource.name));
                    Assert.That(fixture.Results(cue), Is.Null);
                    Assert.That(fixture.Owned.Count, Is.EqualTo(1));
                    Assert.That(fixture.Owned[0], Is.SameAs(retained));
                    Assert.That(retained != null && priorSource != null && validSource != null && invalidSource != null, Is.True);
                    var afterIds = Resources.FindObjectsOfTypeAll<AudioClip>()
                        .Where(v => v.name.StartsWith(prefix, StringComparison.Ordinal)).Select(v => v.GetInstanceID()).OrderBy(v => v).ToArray();
                    CollectionAssert.AreEqual(baselineIds, afterIds, "No partial playback clip may survive rollback.");
                    rows.Add(new { laterClip, error = error.Message, unchangedNativeIds = afterIds, ownedCount = fixture.Owned.Count });
                }
                SaveProduction("production-failure-rollback", new
                {
                    actualStreamingGetDataFailure = true, rows,
                    laterBlockReadFailure = "source-proof only; valid readable clip does not expose deterministic later-block failure",
                    targetSetDataFailure = "source-proof only; no production fault injection"
                });
            }
            finally
            {
                fixture.Release();
                UnityEngine.Object.DestroyImmediate(priorSource);
                UnityEngine.Object.DestroyImmediate(validSource);
                UnityEngine.Object.DestroyImmediate(invalidSource);
            }
        }

        [Test]
        public void ProductionBoundedColdConversionCostGroupOnce()
        {
            Directory.CreateDirectory(ProductionOutput);
            using (var marker = new FileStream(Path.Combine(ProductionOutput, "production-cost.started"), FileMode.CreateNew, FileAccess.Write))
                marker.WriteByte(1);
            var rows = new List<object>();
            bool allMeanGates = true;
            foreach (int frames in new[] { 65537, 329244 })
            {
                AudioClip source = AudioClip.Create("H10ProductionCost" + frames, frames, 1, 44100, false);
                using var fixture = new ProductionFixture();
                var input = InputSamples(frames);
                Assert.That(source.SetData(input, 0), Is.True);
                var means = new double[4];
                var maxima = new double[4];
                try
                {
                    for (int window = 0; window < 4; window++)
                    {
                        bool production = window == 1 || window == 2;
                        var ticks = new long[16];
                        long objectMemory = 0;
                        for (int iteration = -4; iteration < ticks.Length; iteration++)
                        {
                            object cue = fixture.Cue(new[] { source });
                            var originalCue = new FrozenCue { IsLoaded = true, Clips = new[] { source } };
                            try
                            {
                                long start = System.Diagnostics.Stopwatch.GetTimestamp();
                                if (production) fixture.convert(cue);
                                else fixture.frozen.PrepareBattlePlaybackClips(originalCue);
                                long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - start;
                                if (iteration >= 0) ticks[iteration] = elapsed;
                                AudioClip result = production ? fixture.Results(cue)[0] : originalCue.BattlePlaybackClips[0];
                                Assert.That(result.samples, Is.EqualTo(frames));
                                Assert.That(result.channels, Is.EqualTo(2));
                                Assert.That(result.frequency, Is.EqualTo(source.frequency));
                                Assert.That(result.name, Is.EqualTo(source.name));
                                float[] actual = ReadSamples(result);
                                for (int i = 0; i < actual.Length; i++)
                                    if (BitConverter.SingleToInt32Bits(actual[i]) != BitConverter.SingleToInt32Bits(input[i / 2]))
                                        Assert.Fail("Cost PCM mismatch " + i);
                                long memory = Profiler.GetRuntimeMemorySizeLong(result);
                                if (objectMemory != 0) Assert.That(memory, Is.EqualTo(objectMemory));
                                objectMemory = memory;
                            }
                            finally { fixture.Release(); }
                        }
                        double factor = 1000d / System.Diagnostics.Stopwatch.Frequency;
                        means[window] = ticks.Average() * factor;
                        maxima[window] = ticks.Max() * factor;
                        rows.Add(new
                        {
                            frames, window, production, ticks, frequency = System.Diagnostics.Stopwatch.Frequency,
                            meanMs = means[window], p95Ms = maxima[window], p99Ms = maxima[window], maxMs = maxima[window],
                            warm = 4, sample = 16, playbackObjectBytes = objectMemory,
                            floatArrays = production ? 4 : 2,
                            temporaryFloatPayloadBytes = production ? (BlockFrames + frames % BlockFrames) * 12L : frames * 12L,
                            scope = "actual conversion and AudioClip creation; assertion/readback/reflection/input/cleanup outside; cold allocations allowed"
                        });
                    }
                    double originalLimit = Math.Max(means[0], means[3]);
                    bool pass = means[1] <= originalLimit && means[2] <= originalLimit;
                    allMeanGates &= pass;
                    SaveProduction("production-cost-" + frames, new
                    {
                        frames, originalLimit, means, observedMaxMs = maxima, meanGate = pass,
                        p95p99AreObservedMaxOf16 = true
                    });
                }
                finally { fixture.Release(); UnityEngine.Object.DestroyImmediate(source); }
            }
            SaveProduction("production-cost", new { allMeanGates, rows });
            Assert.That(allMeanGates, Is.True, "Production means must not exceed the larger original mean for either size.");
        }

        private static void Save(string name, object data)
        {
            Directory.CreateDirectory(Output);
            using var stream = new FileStream(Path.Combine(Output, Path.GetFileNameWithoutExtension(name) + "-" + Invocation + ".json"), FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
#endif
