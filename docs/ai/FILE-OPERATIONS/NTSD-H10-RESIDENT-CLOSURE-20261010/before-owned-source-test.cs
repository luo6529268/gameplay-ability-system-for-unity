#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class H10ResidentClosureDiagnostics
    {
        private const string Output = "artifacts/diagnostics/NTSD-H10-RESIDENT-CLOSURE-20261010";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

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
