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
                    TryReadIntField(trimmed, "id", out stage.Id);
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
                    TryReadIntField(trimmed, "bound", out phase.Bound);
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
                if (!TryReadIntField(trimmed, "id", out spawn.Id))
                    continue;
                TryReadIntField(trimmed, "act", out spawn.Act);
                TryReadIntField(trimmed, "hp", out spawn.Hp);
                TryReadIntField(trimmed, "times", out spawn.Times);
                TryReadIntField(trimmed, "x", out spawn.X);
                TryReadIntField(trimmed, "y", out spawn.Y);
                TryReadDoubleField(trimmed, "ratio", out spawn.Ratio);
                TryReadIntField(trimmed, "join", out spawn.Join);
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
