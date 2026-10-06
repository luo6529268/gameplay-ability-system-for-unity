#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.EditorTools;
using NTSD.Load;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28RasenganCommonRegressionEditorTests
    {
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/";

        [TestCase("charge-clone-near", 16)]
        [TestCase("charge-body-near", 16)]
        [TestCase("prepared-hold-sound-far", 64)]
        public void AuthoredRasenganCommonConsumerMatchesFormalWitness(string scenario, int ticks)
        {
            RunFormalComparison(scenario, ticks, false);
        }

        [TestCase("held-attack-clone", false)]
        [TestCase("held-attack-body", false)]
        [TestCase("held-attack-clone", true)]
        [TestCase("held-attack-body", true)]
        public void HeldRasenganAttackAndTerminationMatchesFormalWitness(string scenario, bool projected)
        {
            RunFormalComparison(scenario, 64, projected);
        }

        [UnityTest]
        public IEnumerator Rasengan053DecodedSamplesAndAssignedVoicesMatchAuthoredPcm()
        {
            return UniTask.ToCoroutine(async () =>
            {
                FieldInfo instance = typeof(GameConfig).GetField("_instance",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var previous = (GameConfig)instance.GetValue(null);
                var config = ScriptableObject.CreateInstance<GameConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.BattleContentRuntimeRoot = "Assets/NTSD/Content/LoganRuntime";
                instance.SetValue(null, config);
                var host = new GameObject("Rasengan053PcmDiagnostic")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                AudioClip ownedClip = null;
                string ownedKey = null;
                try
                {
                    var player = host.AddComponent<NTSDSoundPlayer>();
                    MethodInfo getCue = typeof(NTSDSoundPlayer).GetMethod("GetOrPrepareCue",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo loadCue = typeof(NTSDSoundPlayer).GetMethod("EnsurePreparedCueLoadedAsync",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    object cue = getCue.Invoke(player, new object[] { @"data\053.wav", true });
                    Type cueType = cue.GetType();
                    string key = (string)cueType.GetField("CacheKey").GetValue(cue);
                    bool cached = NTSD_ResourceLoader.Instance.TryGetCache(key, out _);
                    await (UniTask)loadCue.Invoke(player, new[] { cue });
                    AudioClip clip = ((AudioClip[])cueType.GetField("Clips").GetValue(cue))[0];
                    Assert.That(clip, Is.Not.Null);
                    if (!cached)
                    {
                        ownedClip = clip;
                        ownedKey = key;
                    }
                    Assert.That(clip.channels, Is.EqualTo(1));
                    Assert.That(clip.frequency, Is.EqualTo(11025));
                    Assert.That(clip.samples, Is.EqualTo(5746));
                    string path = Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime/vfs/data/053.wav");
                    byte[] pcm = ReadPcm8(path);
                    var samples = new float[clip.samples];
                    Assert.That(clip.GetData(samples, 0), Is.True);
                    Assert.That(samples, Has.Length.EqualTo(pcm.Length));
                    double maxError = 0;
                    for (int index = 0; index < samples.Length; index++)
                        maxError = Math.Max(maxError, Math.Abs(samples[index] - (pcm[index] - 128) / 128f));
                    Assert.That(maxError, Is.LessThanOrEqualTo(1e-6), "PCM decode must preserve authored samples");
                    player.PresentSound(new PendingSoundEvent(@"data\053.wav", 500, 1));
                    player.PresentSound(new PendingSoundEvent(@"data\053.wav", 500, 2));
                    AudioSource[] assigned = host.GetComponentsInChildren<AudioSource>()
                        .Where(value => value.clip != null).ToArray();
                    Assert.That(assigned, Has.Length.EqualTo(1));
                    Assert.That(assigned[0].clip.channels, Is.EqualTo(2));
                    var playbackSamples = new float[clip.samples * 2];
                    Assert.That(assigned[0].clip.GetData(playbackSamples, 0), Is.True);
                    for (int index = 0; index < samples.Length; index++)
                    {
                        Assert.That(playbackSamples[index * 2], Is.EqualTo(samples[index]));
                        Assert.That(playbackSamples[index * 2 + 1], Is.EqualTo(samples[index]));
                    }
                    foreach (AudioSource voice in assigned)
                    {
                        Assert.That(voice.loop, Is.False);
                        Assert.That(voice.pitch, Is.EqualTo(1f));
                        Assert.That(voice.spatialBlend, Is.Zero);
                    }
                    WriteNew("053-pcm-voices", new
                    {
                        sampleCount = samples.Length,
                        maxError,
                        assigned = assigned.Select(value => new
                        {
                            value.volume, value.panStereo, value.pitch, value.loop,
                            mixer = value.outputAudioMixerGroup != null ? value.outputAudioMixerGroup.name : null
                        }).ToArray()
                    });
                }
                finally
                {
                    NTSDSoundPlayer player = host.GetComponent<NTSDSoundPlayer>();
                    if (player != null)
                        typeof(NTSDSoundPlayer).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(player, null);
                    UnityEngine.Object.DestroyImmediate(host);
                    if (ownedClip != null)
                    {
                        NTSD_ResourceLoader.Instance.RemoveCache(ownedKey);
                        UnityEngine.Object.DestroyImmediate(ownedClip);
                    }
                    instance.SetValue(null, previous);
                    UnityEngine.Object.DestroyImmediate(config);
                }
            });
        }

        private static byte[] ReadPcm8(string path)
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("RIFF"));
            reader.ReadUInt32();
            Assert.That(new string(reader.ReadChars(4)), Is.EqualTo("WAVE"));
            while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
            {
                string chunk = new string(reader.ReadChars(4));
                uint length = reader.ReadUInt32();
                long end = reader.BaseStream.Position + length;
                if (chunk == "fmt ")
                {
                    Assert.That(reader.ReadUInt16(), Is.EqualTo(1));
                    Assert.That(reader.ReadUInt16(), Is.EqualTo(1));
                    Assert.That(reader.ReadUInt32(), Is.EqualTo(11025));
                    reader.ReadUInt32();
                    reader.ReadUInt16();
                    Assert.That(reader.ReadUInt16(), Is.EqualTo(8));
                }
                else if (chunk == "data")
                    return reader.ReadBytes(checked((int)length));
                reader.BaseStream.Position = end + (length & 1);
            }
            throw new InvalidDataException("Missing PCM data chunk: " + path);
        }

        private static void RunFormalComparison(string scenario, int ticks, bool projected)
        {
            var rows = new List<JObject>();
            string tracePath = OutputRoot + scenario + "/root-trace.jsonl";
            JObject[] formal = File.ReadLines(tracePath).Select(JObject.Parse).ToArray();
            JObject observation = JObject.Parse(File.ReadAllText(OutputRoot + scenario + "/summary.json"));
            JArray audio = (JArray)observation["events"];
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    "Assets/NTSD/Content/LoganRuntime", OutputRoot + scenario + "-unity-scenario.json",
                    BattleRuntimeProfile.Authority400, ticks, (driver, unused, identity) =>
                    {
                        SimulationWorld world = driver.World;
                        world.Runtime.FunctionKeys.ResetForBattle(true);
                        world.Runtime.Flow.FrameToggle = 0;
                        world.Runtime.Flow.InputPhase = 0;
                        var entities = new List<LF2Entity>();
                        if (projected)
                        {
                            world.ConfigureFixedViewRunDistance(2048, 1152);
                            world.GetAllEntities(entities);
                            foreach (LF2Entity entity in entities)
                            {
                                entity.Runtime.Z = 450;
                                entity.Runtime.SetSourceRulePosition(entity.Runtime.X, entity.Runtime.Z);
                                entity.Runtime.X = world.SpatialProjection.SourceToViewX(entity.Runtime.X);
                                entity.Runtime.Z = world.SpatialProjection.SourceToViewZ(entity.Runtime.Z);
                                entity.Runtime.SyncIntegerPosition();
                                entity.Runtime.SyncSourceRuleIntegerPosition();
                            }
                        }
                        for (int tick = 1; tick <= ticks; tick++)
                        {
                            SimulationInputButtons buttons = scenario.StartsWith("held-attack", StringComparison.Ordinal) &&
                                (tick == 29 || tick == 30)
                                ? SimulationInputButtons.Jump : SimulationInputButtons.None;
                            Assert.That(driver.StepOneTick(new FrameInputSet(tick,
                                new[]
                                {
                                    new SimulationPlayerInput(0, buttons),
                                    new SimulationPlayerInput(1, SimulationInputButtons.None)
                                }), ignorePaused: true, buildPresentation: false), Is.True);
                            world.GetAllEntities(entities);
                            rows.Add(JObject.FromObject(new
                            {
                                tick,
                                actors = entities.Select(value => new
                                {
                                    slot = value.Runtime.SlotIndex,
                                    oid = value.ObjectId,
                                    action = value.Frame.N,
                                    state = value.Frame.D?.state ?? -1,
                                    counter = value.Runtime.AttackingCounter,
                                    owner = value.Runtime.OwnerSlotIndex,
                                    parent = value.Runtime.LinkState,
                                    catchTarget = value.Runtime.TargetSlotIndex,
                                    catchSource = value.Runtime.CatchSourceSlot90,
                                    caught = value.Runtime.CaughtSlotIndex,
                                    x = value.Runtime.SourceRulePositionInitialized ? value.Runtime.SourceRuleX : value.Runtime.X,
                                    z = value.Runtime.SourceRulePositionInitialized ? value.Runtime.SourceRuleZ : value.Runtime.Z,
                                    hp = value.Runtime.HP
                                }).ToArray(),
                                sounds = world.PendingSounds.Select(value => new
                                {
                                    path = value.Cue,
                                    worldX = value.WorldX
                                }).ToArray()
                            }));
                        }
                    }, useProjectMode: true);
            }
            finally
            {
                WriteNew(scenario, new { scenario, ticks, projected, rows });
            }

            Assert.That(rows, Has.Count.EqualTo(ticks));
            for (int tick = 1; tick <= ticks; tick++)
            {
                JObject expected = formal.Single(value => (int)value["tick"] == tick);
                JObject actual = rows[tick - 1];
                foreach (int slot in new[] { 0, 1 })
                {
                    JToken expectedActor = expected["entities"].FirstOrDefault(value => (int)value["slot"] == slot);
                    JToken actualActor = actual["actors"].FirstOrDefault(value => (int)value["slot"] == slot);
                    Assert.That(actualActor != null, Is.EqualTo(expectedActor != null), "actor existence tick" + tick + " slot" + slot);
                    if (expectedActor == null)
                        continue;
                    foreach (string field in new[] { "oid", "action", "state", "hp" })
                        Assert.That((int)actualActor[field], Is.EqualTo((int)expectedActor[field]),
                            "formal tick" + tick + " slot" + slot + " " + field);
                }
                if (tick > 1)
                {
                    JToken expectedAudio = audio.Single(value => (int)value["sequence"] == tick);
                    string[] expected053 = expectedAudio["events"].Where(value =>
                        NormalizePath((string)value["path"]) == "data/053.wav")
                        .Select(value => NormalizePath((string)value["path"])).ToArray();
                    string[] actual053 = actual["sounds"].Where(value =>
                        NormalizePath((string)value["path"]) == "data/053.wav")
                        .Select(value => NormalizePath((string)value["path"])).ToArray();
                    Assert.That(actual053, Is.EqualTo(expected053), "formal053 event cadence tick" + tick);
                }
                if (scenario.StartsWith("held-attack", StringComparison.Ordinal))
                {
                    JToken[] expectedHeld = expected["entities"].Where(value => (int)value["oid"] == 434)
                        .OrderBy(value => (int)value["slot"]).ToArray();
                    JToken[] actualHeld = actual["actors"].Where(value => (int)value["oid"] == 434)
                        .OrderBy(value => (int)value["slot"]).ToArray();
                    Assert.That(actualHeld, Has.Length.EqualTo(expectedHeld.Length), "formal434 existence tick" + tick);
                    for (int index = 0; index < actualHeld.Length; index++)
                    {
                        Assert.That((int)actualHeld[index]["action"], Is.EqualTo((int)expectedHeld[index]["action"]),
                            "formal434 action tick" + tick + " index" + index);
                        Assert.That((int)actualHeld[index]["parent"], Is.EqualTo((int)expectedHeld[index]["interaction"]),
                            "formal434 relation tick" + tick + " index" + index);
                    }
                }
            }
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/').ToLowerInvariant();
        }

        private static void WriteNew(string label, object value)
        {
            string path = OutputRoot + label + "-unity-" + Guid.NewGuid().ToString("N") + ".json";
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }
    }
}
#endif
