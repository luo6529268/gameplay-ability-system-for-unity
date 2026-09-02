using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NTSD.DatParser;
using UnityEngine;

namespace NTSD.Simulation
{
    public static class BattleStageCampaignLoader
    {
        private const string DatEncryptionKey = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private static string DefaultStageDatPath =>
            Path.Combine(Application.streamingAssetsPath, "NTSD", "data", "stage.dat");

        public static List<BattleStageCampaignData> LoadFromFile(string filePath)
        {
            try
            {
                string path = string.IsNullOrWhiteSpace(filePath) ? DefaultStageDatPath : filePath;
                string fullPath = ResolvePath(path);
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[BattleStageCampaignLoader] stage wave disabled; campaign file not found: {fullPath}");
                    return new List<BattleStageCampaignData>();
                }

                string text = Lf2DatDecryptor.DecryptFile(fullPath, DatEncryptionKey);
                List<BattleStageCampaignData> campaigns = ParseText(text);
                if (campaigns.Count == 0)
                    Debug.LogWarning($"[BattleStageCampaignLoader] stage wave disabled; no campaigns parsed from: {fullPath}");
                return campaigns;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BattleStageCampaignLoader] stage wave disabled; failed to load campaign data: {ex.Message}");
                return new List<BattleStageCampaignData>();
            }
        }

        private static string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            return Path.GetFullPath(Path.Combine(projectRoot, path));
        }

        public static List<BattleStageCampaignData> ParseText(string text)
        {
            var stages = new List<BattleStageCampaignData>();
            if (string.IsNullOrEmpty(text))
                return stages;

            BattleStageCampaignData currentStage = null;
            BattleStagePhaseData currentPhase = null;
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r', ' ', '\t');
                string trimmed = line.TrimStart();
                if (trimmed.Length == 0)
                    continue;

                if (trimmed.StartsWith("<stage>", StringComparison.Ordinal))
                {
                    var stage = new BattleStageCampaignData();
                    if (TryReadIntField(trimmed, "id", out int stageId))
                        stage.Id = stageId;
                    int commentIndex = trimmed.IndexOf('#');
                    if (commentIndex >= 0)
                        stage.Comment = trimmed.Substring(commentIndex + 1).Trim();
                    stages.Add(stage);
                    currentStage = stage;
                    currentPhase = null;
                    continue;
                }

                if (trimmed.StartsWith("<phase>", StringComparison.Ordinal))
                {
                    if (currentStage == null)
                        continue;
                    var phase = new BattleStagePhaseData();
                    if (TryReadIntField(trimmed, "bound", out int phaseBound))
                        phase.Bound = phaseBound;
                    currentStage.Phases.Add(phase);
                    currentPhase = phase;
                    continue;
                }

                if (trimmed.StartsWith("<phase_end>", StringComparison.Ordinal))
                {
                    currentPhase = null;
                    continue;
                }

                if (trimmed.StartsWith("<stage_end>", StringComparison.Ordinal))
                {
                    currentStage = null;
                    currentPhase = null;
                    continue;
                }

                if (currentPhase == null || !ContainsField(trimmed, "id"))
                    continue;

                var spawn = new BattleStageSpawnData();
                if (!TryReadIntField(trimmed, "id", out int spawnId))
                    continue;
                spawn.Id = spawnId;
                if (TryReadIntField(trimmed, "act", out int spawnAct))
                    spawn.Act = spawnAct;
                if (TryReadIntField(trimmed, "hp", out int spawnHp))
                    spawn.Hp = spawnHp;
                if (TryReadIntField(trimmed, "times", out int spawnTimes))
                    spawn.Times = spawnTimes;
                if (TryReadIntField(trimmed, "x", out int spawnX))
                    spawn.X = spawnX;
                if (TryReadIntField(trimmed, "y", out int spawnY))
                    spawn.Y = spawnY;
                if (TryReadDoubleField(trimmed, "ratio", out double spawnRatio))
                    spawn.Ratio = spawnRatio;
                if (TryReadIntField(trimmed, "join", out int spawnJoin))
                    spawn.Join = spawnJoin;
                currentPhase.Spawns.Add(spawn);
            }

            return stages;
        }

        private static bool ContainsField(string line, string field)
        {
            return Regex.IsMatch(line, $@"(?:^|\s){Regex.Escape(field)}\s*:", RegexOptions.CultureInvariant);
        }

        private static bool TryReadIntField(string line, string field, out int value)
        {
            value = 0;
            Match match = MatchField(line, field);
            return match.Success && int.TryParse(
                match.Groups[1].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryReadDoubleField(string line, string field, out double value)
        {
            value = 0.0;
            Match match = MatchField(line, field);
            return match.Success && double.TryParse(
                match.Groups[1].Value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        private static Match MatchField(string line, string field)
        {
            return Regex.Match(
                line,
                $@"(?:^|\s){Regex.Escape(field)}\s*:\s*([^\s#]+)",
                RegexOptions.CultureInvariant);
        }
    }
}
