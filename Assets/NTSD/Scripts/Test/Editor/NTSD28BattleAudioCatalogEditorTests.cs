#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.App;
using NTSD.Load;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28BattleAudioCatalogEditorTests
    {
        [UnityTest]
        public IEnumerator StagedBattleWavsDecodeAndPreparedAliasesPlayWithoutAllocation()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FieldInfo instance = typeof(GameConfig).GetField(
                    "_instance", BindingFlags.Static | BindingFlags.NonPublic);
                var previous = (GameConfig)instance.GetValue(null);
                var config = ScriptableObject.CreateInstance<GameConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.BattleContentRuntimeRoot = "Assets/NTSD/Content/LoganRuntime";
                instance.SetValue(null, config);
                var host = new GameObject("BattleAudioCatalogTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                var owned = new Dictionary<string, AudioClip>();
                try
                {
                    var player = host.AddComponent<NTSDSoundPlayer>();
                    MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod(
                        "GetOrPrepareCue", BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo loadCue = typeof(NTSDSoundPlayer).GetMethod(
                        "EnsurePreparedCueLoadedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
                    string root = Path.GetFullPath(Path.Combine(
                        Application.dataPath, "NTSD/Content/LoganRuntime/vfs"));
                    string[] files = Directory.GetFiles(root, "*.wav", SearchOption.AllDirectories);
                    Assert.That(files.Length, Is.GreaterThanOrEqualTo(978));
                    int decoded = 0;
                    foreach (string file in files)
                    {
                        string cue = file.Substring(root.Length + 1);
                        object prepared = getCue.Invoke(player, new object[] { cue, true });
                        string key = Read<string>(prepared, "CacheKey");
                        bool cached = NTSD_ResourceLoader.Instance.TryGetCache(key, out _);
                        await (UniTask)loadCue.Invoke(player, new[] { prepared });
                        AudioClip[] clips = Read<AudioClip[]>(prepared, "Clips");
                        if (!cached && clips[0] != null)
                            owned.Add(key, clips[0]);
                        Assert.That(Read<bool>(prepared, "IsFormalBattleFile"), Is.True, cue);
                        Assert.That(clips[0], Is.Not.Null, cue);
                        var expected = ReadWaveFormat(file);
                        Assert.That(clips[0].channels, Is.EqualTo(expected.Channels), cue);
                        Assert.That(clips[0].frequency, Is.EqualTo(expected.Frequency), cue);
                        Assert.That(clips[0].samples, Is.EqualTo(expected.Samples), cue);
                        decoded++;
                    }

                    await player.PrepareBattleCuesAsync(null);
                    Assert.That(player.BattleCatalogSealedForDiagnostics, Is.True);
                    foreach (string cue in new[]
                    {
                        "SFX_001", "SFX_006", "SFX_011", "SFX_032", "SFX_033",
                        @"c\nar\w\p3.wav", @"c\nar\w\a7.wav", @"c\saku\w\tra.wav",
                    })
                    {
                        long before = player.PooledOneShotPlayCountForDiagnostics;
                        player.PresentSound(new PendingSoundEvent(cue, 500, 1));
                        Assert.That(player.PooledOneShotPlayCountForDiagnostics,
                            Is.EqualTo(before + 1), cue);
                    }
                    long allocations = GC.GetAllocatedBytesForCurrentThread();
                    for (int index = 0; index < 128; index++)
                        player.PresentSound(new PendingSoundEvent("SFX_001", 500, 2));
                    long allocated = GC.GetAllocatedBytesForCurrentThread() - allocations;
                    Assert.That(allocated, Is.Zero);
                    Assert.That(player.FailedPreparedCueLoadCountForDiagnostics, Is.Zero);
                    Assert.That(player.RejectedUnpreparedCueCountForDiagnostics, Is.Zero);
                    TestContext.WriteLine($"Decoded WAV files: {decoded}; representative voices: 8; alias hot-path allocations: {allocated}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(host);
                    foreach (var entry in owned)
                    {
                        NTSD_ResourceLoader.Instance.RemoveCache(entry.Key);
                        UnityEngine.Object.DestroyImmediate(entry.Value);
                    }
                    instance.SetValue(null, previous);
                    UnityEngine.Object.DestroyImmediate(config);
                }
            });
        }

        private static T Read<T>(object cue, string field)
        {
            return (T)cue.GetType().GetField(field).GetValue(cue);
        }

        private static (int Channels, int Frequency, int Samples) ReadWaveFormat(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("RIFF"), path);
            reader.ReadUInt32();
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("WAVE"), path);
            int channels = 0, frequency = 0, blockAlign = 0, dataBytes = 0;
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
                    dataBytes = checked((int)size);
                stream.Position = end + (size & 1);
            }
            Assert.That(blockAlign, Is.GreaterThan(0), path);
            return (channels, frequency, dataBytes / blockAlign);
        }
    }
}
#endif
