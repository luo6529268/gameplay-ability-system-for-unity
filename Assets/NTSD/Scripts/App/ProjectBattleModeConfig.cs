using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NTSD.App
{
    [CreateAssetMenu(fileName = "ProjectBattleModeConfig", menuName = "NTSD/Battle Mode Config")]
    public sealed class ProjectBattleModeConfig : ScriptableObject
    {
        [Serializable]
        public sealed class ComboSettings
        {
            public bool enabled = true;
            public int facing = 1;
            public int respond = 50;
            public bool caughtAct = true;
        }

        [Serializable]
        public sealed class KnockoutSettings
        {
            public bool enabled = true;
            public int respond = 5;
            public int lifetimeTicks = 70;
            public int rowSpacing = 40;
            public int transparency = 1;
            public bool attackerTeamColor = true;
            public bool victimTeamColor = true;
            public int screenTop = 132;
            public int imageScreenLeft = 575;
            public int attackerScreenLeft = 560;
            public int victimScreenLeft = 650;
            public bool attackerAppendCharacterName = true;
            public bool victimAppendCharacterName = true;
            public bool attackerRightAligned = true;
            public bool victimRightAligned;
            public int[] allowedBattleModes = { 0, 1, 4 };
            public int[] excludedVictimObjectIds = Array.Empty<int>();
            public string[] typeImagePaths =
            {
                "sprite/kill/c.png", "sprite/kill/sk2.png", "sprite/kill/sk2.png",
                "sprite/kill/sk1.png", "sprite/kill/sk2.png", "sprite/kill/sk2.png",
                "sprite/kill/sk2.png"
            };
            public string stageTeam1DeathSoundPath = "data/m_ok.wav";
            public string stageTeam5DeathSoundPath = "data/m_join.wav";
        }

        [SerializeField] private ComboSettings combo = new ComboSettings();
        [SerializeField] private KnockoutSettings knockout = new KnockoutSettings();

        public ComboSettings Combo => combo;
        public KnockoutSettings Knockout => knockout;

        public static ProjectBattleModeConfig LoadDefault()
        {
            ProjectBattleModeConfig config = Resources.Load<ProjectBattleModeConfig>(
                "ProjectBattleModeConfig");
            if (config == null)
                throw new InvalidOperationException("ProjectBattleModeConfig asset is missing.");
            return config;
        }

        public Snapshot Capture()
        {
            if (combo == null || knockout == null || knockout.typeImagePaths == null ||
                knockout.typeImagePaths.Length != 7)
                throw new InvalidDataException("Project battle mode config is incomplete.");
            return new Snapshot(combo, knockout);
        }

        public sealed class Snapshot
        {
            private readonly int[] allowedBattleModes;
            private readonly int[] excludedVictimObjectIds;
            private readonly string[] typeImagePaths;

            public int ComboBound { get; }
            public int ComboFacing { get; }
            public int ComboRespond { get; }
            public int ComboCaughtAct { get; }
            public bool KnockoutEnabled { get; }
            public int KnockoutRespond { get; }
            public int KnockoutLifetimeTicks { get; }
            public int KnockoutRowSpacing { get; }
            public int KnockoutTransparency { get; }
            public bool AttackerTeamColor { get; }
            public bool VictimTeamColor { get; }
            public int ScreenTop { get; }
            public int ImageScreenLeft { get; }
            public int AttackerScreenLeft { get; }
            public int VictimScreenLeft { get; }
            public bool AttackerAppendCharacterName { get; }
            public bool VictimAppendCharacterName { get; }
            public bool AttackerRightAligned { get; }
            public bool VictimRightAligned { get; }
            public string StageTeam1DeathSoundPath { get; }
            public string StageTeam5DeathSoundPath { get; }
            public string Fingerprint { get; }

            public int[] AllowedBattleModes => (int[])allowedBattleModes.Clone();
            public int[] ExcludedVictimObjectIds => (int[])excludedVictimObjectIds.Clone();
            public string[] TypeImagePaths => (string[])typeImagePaths.Clone();

            internal Snapshot(ComboSettings combo, KnockoutSettings knockout)
            {
                ComboBound = combo.enabled ? 1 : 0;
                ComboFacing = combo.facing;
                ComboRespond = combo.respond;
                ComboCaughtAct = combo.caughtAct ? 1 : 0;
                KnockoutEnabled = knockout.enabled;
                KnockoutRespond = knockout.respond;
                KnockoutLifetimeTicks = knockout.lifetimeTicks;
                KnockoutRowSpacing = knockout.rowSpacing;
                KnockoutTransparency = knockout.transparency;
                AttackerTeamColor = knockout.attackerTeamColor;
                VictimTeamColor = knockout.victimTeamColor;
                ScreenTop = knockout.screenTop;
                ImageScreenLeft = knockout.imageScreenLeft;
                AttackerScreenLeft = knockout.attackerScreenLeft;
                VictimScreenLeft = knockout.victimScreenLeft;
                AttackerAppendCharacterName = knockout.attackerAppendCharacterName;
                VictimAppendCharacterName = knockout.victimAppendCharacterName;
                AttackerRightAligned = knockout.attackerRightAligned;
                VictimRightAligned = knockout.victimRightAligned;
                allowedBattleModes = (int[])(knockout.allowedBattleModes ?? Array.Empty<int>()).Clone();
                excludedVictimObjectIds = (int[])(knockout.excludedVictimObjectIds ?? Array.Empty<int>()).Clone();
                typeImagePaths = (string[])knockout.typeImagePaths.Clone();
                StageTeam1DeathSoundPath = knockout.stageTeam1DeathSoundPath ?? string.Empty;
                StageTeam5DeathSoundPath = knockout.stageTeam5DeathSoundPath ?? string.Empty;

                using (var bytes = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                    {
                        writer.Write("PROJECT_BATTLE_MODE_CONFIG_V1");
                        writer.Write(ComboBound);
                        writer.Write(ComboFacing);
                        writer.Write(ComboRespond);
                        writer.Write(ComboCaughtAct);
                        writer.Write(KnockoutEnabled);
                        writer.Write(KnockoutRespond);
                        writer.Write(KnockoutLifetimeTicks);
                        writer.Write(KnockoutRowSpacing);
                        writer.Write(KnockoutTransparency);
                        writer.Write(AttackerTeamColor);
                        writer.Write(VictimTeamColor);
                        writer.Write(ScreenTop);
                        writer.Write(ImageScreenLeft);
                        writer.Write(AttackerScreenLeft);
                        writer.Write(VictimScreenLeft);
                        writer.Write(AttackerAppendCharacterName);
                        writer.Write(VictimAppendCharacterName);
                        writer.Write(AttackerRightAligned);
                        writer.Write(VictimRightAligned);
                        WriteInts(writer, allowedBattleModes);
                        WriteInts(writer, excludedVictimObjectIds);
                        writer.Write(typeImagePaths.Length);
                        foreach (string path in typeImagePaths)
                            writer.Write(path ?? string.Empty);
                        writer.Write(StageTeam1DeathSoundPath);
                        writer.Write(StageTeam5DeathSoundPath);
                    }
                    using (var sha = SHA256.Create())
                        Fingerprint = BitConverter.ToString(sha.ComputeHash(bytes.ToArray()))
                            .Replace("-", string.Empty);
                }
            }

            private static void WriteInts(BinaryWriter writer, int[] values)
            {
                writer.Write(values.Length);
                foreach (int value in values)
                    writer.Write(value);
            }
        }
    }
}
