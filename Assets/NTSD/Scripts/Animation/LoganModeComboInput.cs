using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.DatParser;

namespace NTSD.Animation
{
    public sealed class LoganModeKnockoutFeedInput
    {
        private readonly string[] typeResourcePaths = new string[7];

        public bool Enabled { get; }
        public int Respond { get; }
        public int LifetimeTicks { get; }
        public int RowSpacing { get; }
        public int Transparency { get; }
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
        public IReadOnlyList<int> AllowedBattleModes { get; }
        public IReadOnlyList<int> ExcludedVictimObjectIds { get; }
        public string StageTeam1DeathSoundPath { get; }
        public string StageTeam5DeathSoundPath { get; }

        private LoganModeKnockoutFeedInput(
            Dictionary<string, string> fields,
            List<int> modes,
            List<int> ids)
        {
            Enabled = Integer(fields, "bound") == 1;
            Respond = Integer(fields, "respond");
            LifetimeTicks = Integer(fields, "times");
            RowSpacing = Integer(fields, "loop");
            Transparency = Integer(fields, "transparency");
            AttackerTeamColor = Integer(fields, "arest") == 1;
            VictimTeamColor = Integer(fields, "vrest") == 1;
            ScreenTop = Integer(fields, "y");
            ImageScreenLeft = Integer(fields, "x");
            AttackerScreenLeft = Integer(fields, "dx");
            VictimScreenLeft = Integer(fields, "dvx");
            AttackerAppendCharacterName = Integer(fields, "caughtact") == 1;
            VictimAppendCharacterName = Integer(fields, "catchingact") == 1;
            AttackerRightAligned = Integer(fields, "pickedact") == 1;
            VictimRightAligned = Integer(fields, "pickingact") == 1;
            for (int type = 0; type < typeResourcePaths.Length; type++)
                fields.TryGetValue("pic_type" + type.ToString(CultureInfo.InvariantCulture),
                    out typeResourcePaths[type]);
            AllowedBattleModes = modes.AsReadOnly();
            ExcludedVictimObjectIds = ids.AsReadOnly();
            fields.TryGetValue("sound1", out string sound1);
            fields.TryGetValue("sound2", out string sound2);
            StageTeam1DeathSoundPath = sound1;
            StageTeam5DeathSoundPath = sound2;
        }

        public string TypeResourcePath(int type)
        {
            if (type < 0 || type >= typeResourcePaths.Length)
                throw new ArgumentOutOfRangeException(nameof(type));
            return typeResourcePaths[type];
        }

        public static LoganModeKnockoutFeedInput Parse(string text)
        {
            string[] tokens = Lf2DatTokenizer.Tokenize(text);
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            var modes = new List<int>();
            var ids = new List<int>();
            bool inBmp = false;
            bool seenBmp = false;
            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index];
                if (token == "<bmp_begin>")
                {
                    if (seenBmp || inBmp)
                        throw new InvalidDataException("Mode child contains multiple knockout-feed blocks.");
                    seenBmp = inBmp = true;
                    continue;
                }
                if (token == "<bmp_end>")
                {
                    if (!inBmp)
                        throw new InvalidDataException("Mode child has an unexpected knockout-feed end.");
                    inBmp = false;
                    continue;
                }
                if (!inBmp)
                    continue;
                if (!token.EndsWith(":", StringComparison.Ordinal) ||
                    index + 1 >= tokens.Length ||
                    tokens[index + 1].StartsWith("<", StringComparison.Ordinal) ||
                    tokens[index + 1].EndsWith(":", StringComparison.Ordinal))
                    throw new InvalidDataException("Mode knockout-feed field is malformed: " + token);
                string key = token.Substring(0, token.Length - 1);
                string value = tokens[++index];
                fields[key] = value;
                if (key == "mode" || key == "id")
                {
                    if (!int.TryParse(value, NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture, out int parsed))
                        throw new InvalidDataException("Mode knockout-feed list value is invalid: " + key);
                    if (key == "mode") modes.Add(parsed);
                    else ids.Add(parsed);
                }
            }
            if (inBmp)
                throw new InvalidDataException("Mode knockout-feed block is not closed.");
            return seenBmp && fields.Count > 0
                ? new LoganModeKnockoutFeedInput(fields, modes, ids)
                : null;
        }

        private static int Integer(Dictionary<string, string> fields, string key)
        {
            if (!fields.TryGetValue(key, out string value))
                return 0;
            if (!int.TryParse(value, NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out int parsed))
                throw new InvalidDataException("Mode knockout-feed integer is invalid: " + key);
            return parsed;
        }
    }

    public sealed class LoganModeComboInput
    {
        private static readonly string[] RequiredFields =
        {
            "bound", "respond", "offset_x", "offset_y", "facing", "times",
            "state", "caughtact", "effect", "w", "h", "pic", "name"
        };

        private static readonly HashSet<string> TextFields =
            new HashSet<string>(StringComparer.Ordinal) { "pic", "name" };

        private readonly string modePath;
        private readonly string childPath;
        private readonly string modeSha256;
        private readonly string childSha256;

        public string SelectedChildVirtualPath { get; }
        public string InputFingerprint { get; }
        public string SemanticFingerprint { get; }
        public int Bound { get; }
        public int Facing { get; }
        public int Respond { get; }
        public int CaughtAct { get; }
        public LoganModeKnockoutFeedInput KnockoutFeed { get; }

        private LoganModeComboInput(string modePath, string childPath,
            string childVirtualPath, byte[] modeBytes, byte[] childBytes,
            Dictionary<string, string> fields)
        {
            this.modePath = modePath;
            this.childPath = childPath;
            SelectedChildVirtualPath = childVirtualPath;
            modeSha256 = Hash(modeBytes);
            childSha256 = Hash(childBytes);
            Bound = Integer(fields, "bound");
            Facing = Integer(fields, "facing");
            Respond = Integer(fields, "respond");
            CaughtAct = Integer(fields, "caughtact");
            KnockoutFeed = LoganModeKnockoutFeedInput.Parse(
                Encoding.UTF8.GetString(childBytes));
            InputFingerprint = HashContract("NTSD28-MODE-COMBO-INPUT-v1",
                childVirtualPath, modeSha256, childSha256);
            SemanticFingerprint = HashContract("NTSD28-MODE-COMBO-SEMANTIC-v1",
                Bound, Facing, Respond, CaughtAct);
        }

        public static LoganModeComboInput Capture(BattleContentSource source)
        {
            if (source == null || !source.IsLoganRuntime)
                throw new ArgumentException("A Logan runtime content source is required.", nameof(source));

            string modePath = Path.Combine(source.DatRoot, "data", "mode.dat");
            if (!File.Exists(modePath))
                return null;

            byte[] modeBytes = File.ReadAllBytes(modePath);
            string childVirtualPath = FirstModeChildPath(Encoding.UTF8.GetString(modeBytes));
            string childPath = ResolveChildPath(source.DatRoot, childVirtualPath);
            if (!File.Exists(childPath))
                throw new InvalidDataException("Selected Logan mode child is missing: " + childVirtualPath);

            byte[] childBytes = File.ReadAllBytes(childPath);
            Dictionary<string, string> fields = ParseCompleteCombo(Encoding.UTF8.GetString(childBytes));
            return new LoganModeComboInput(modePath, childPath, childVirtualPath,
                modeBytes, childBytes, fields);
        }

        public void AssertInputsCurrent()
        {
            if (!File.Exists(modePath) || !File.Exists(childPath) ||
                !string.Equals(Hash(File.ReadAllBytes(modePath)), modeSha256, StringComparison.Ordinal) ||
                !string.Equals(Hash(File.ReadAllBytes(childPath)), childSha256, StringComparison.Ordinal))
                throw new InvalidDataException("Logan mode combo inputs changed after capture.");
        }

        internal static string FirstModeChildPath(string text)
        {
            string[] tokens = Lf2DatTokenizer.Tokenize(text);
            bool inRecord = false;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (token == "<mode_information>")
                {
                    inRecord = true;
                    continue;
                }
                if (token == "<mode_information_end>")
                {
                    inRecord = false;
                    continue;
                }
                if (!inRecord || token != "file:")
                    continue;

                // Keep the formal first-record/first-file selection order.
                if (i + 1 < tokens.Length && tokens[i + 1].Length != 0 &&
                    tokens[i + 1][0] != '<' && !tokens[i + 1].EndsWith(":", StringComparison.Ordinal))
                    return tokens[i + 1];
                throw new InvalidDataException("Selected mode file has no path.");
            }
            throw new InvalidDataException("data/mode.dat has no selected mode child.");
        }

        internal static Dictionary<string, string> ParseCompleteCombo(string text)
        {
            string[] tokens = Lf2DatTokenizer.Tokenize(text);
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            bool inCombo = false;
            int comboCount = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (token == "<combo>")
                {
                    if (inCombo || ++comboCount != 1)
                        throw new InvalidDataException("Mode child must contain one combo block.");
                    inCombo = true;
                    continue;
                }
                if (token == "<combo_end>")
                {
                    if (!inCombo)
                        throw new InvalidDataException("Unexpected combo end marker.");
                    inCombo = false;
                    continue;
                }
                if (!inCombo)
                    continue;
                if (!token.EndsWith(":", StringComparison.Ordinal) ||
                    token.StartsWith("<", StringComparison.Ordinal))
                    throw new InvalidDataException("Invalid token in mode combo block: " + token);

                string key = token.Substring(0, token.Length - 1);
                if (Array.IndexOf(RequiredFields, key) < 0)
                    throw new InvalidDataException("Unknown mode combo field: " + key);
                if (i + 1 >= tokens.Length || tokens[i + 1].StartsWith("<", StringComparison.Ordinal) ||
                    tokens[i + 1].EndsWith(":", StringComparison.Ordinal))
                    throw new InvalidDataException("Mode combo field has no value: " + key);

                string value = tokens[++i];
                if (TextFields.Contains(key))
                {
                    if (value.Length == 0)
                        throw new InvalidDataException("Mode combo text field is empty: " + key);
                }
                else if (!int.TryParse(value, NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out _))
                {
                    throw new InvalidDataException("Mode combo integer is invalid: " + key);
                }
                fields[key] = value;
            }

            if (inCombo || comboCount != 1)
                throw new InvalidDataException("Mode child must contain one closed combo block.");
            foreach (string key in RequiredFields)
                if (!fields.ContainsKey(key))
                    throw new InvalidDataException("Mode combo is missing: " + key);
            return fields;
        }

        private static string ResolveChildPath(string datRoot, string virtualPath)
        {
            string relative = virtualPath.Replace('\\', '/');
            if (Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0 ||
                !relative.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Invalid selected mode child path.");

            string root = Path.GetFullPath(datRoot).TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath = Path.GetFullPath(Path.Combine(datRoot,
                relative.Replace('/', Path.DirectorySeparatorChar)));
            StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!fullPath.StartsWith(root, comparison))
                throw new InvalidDataException("Selected mode child escapes the DAT root.");
            return fullPath;
        }

        private static int Integer(Dictionary<string, string> fields, string key)
        {
            return int.Parse(fields[key], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
        }

        private static string HashContract(string tag, params object[] values)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(tag);
                    foreach (object value in values)
                    {
                        if (value is int integer) writer.Write(integer);
                        else writer.Write((string)value);
                    }
                }
                return Hash(stream.ToArray());
            }
        }
    }
}
