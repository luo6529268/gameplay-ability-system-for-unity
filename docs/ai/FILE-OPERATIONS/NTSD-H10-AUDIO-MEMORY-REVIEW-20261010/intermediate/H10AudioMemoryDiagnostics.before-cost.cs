#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            int frames = Math.Min(source.samples, BlockFrames);
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
            Assert.That(input.GetData(mono, offset), Is.True);
            for (int i = 0; i < mono.Length; i++) stereo[i * 2] = stereo[i * 2 + 1] = mono[i];
            Assert.That(output.SetData(stereo, offset), Is.True);
        }

        private static void Save(string name, object data)
        {
            Directory.CreateDirectory(Output);
            using var stream = new FileStream(Path.Combine(Output, name), FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
#endif
