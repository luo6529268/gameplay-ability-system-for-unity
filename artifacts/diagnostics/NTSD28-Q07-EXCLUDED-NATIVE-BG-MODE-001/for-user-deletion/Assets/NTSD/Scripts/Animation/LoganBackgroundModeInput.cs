using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.DatParser;

namespace NTSD.Animation
{
    /// <summary>Immutable capture of the formal background-mode parent and ordered child records.</summary>
    public sealed class LoganBackgroundModeInput
    {
        public sealed class Record
        {
            private readonly IReadOnlyDictionary<string, int> fields;

            public string Name { get; }
            public IReadOnlyList<int> DropCandidateIds { get; }
            public int SelectionCondition => Value("hidden");
            public int Hurtable => Value("hurtable");
            public int Attacking => Value("attacking");
            public int RegenHp => Value("regen_hp");
            public int RegenMp => Value("regen_mp");
            public int RecMp => Value("recmp");
            public int GainMpAttacker => Value("gain_mp_a");
            public int GainMpVictim => Value("gain_mp_v");
            public int Spark => Value("spark");
            public int Effect => Value("effect");
            public int WeaponDrop => Value("weapon_drop");
            public int Type => Value("type");
            public int Kind => Value("kind");
            public int Reserve => Value("reserve");
            public int State => Value("state");
            public int EtcMode => Value("etc_mode");
            public int EtcWidth => Value("etc_width");
            public int DirControl => Value("dircontrol");
            public int CaughtAct => Value("caughtact");

            internal Record(string name, Dictionary<string, int> values, List<int> ids)
            {
                Name = name ?? string.Empty;
                fields = new ReadOnlyDictionary<string, int>(
                    new Dictionary<string, int>(values, StringComparer.Ordinal));
                DropCandidateIds = Array.AsReadOnly(ids.ToArray());
            }

            private int Value(string key)
            {
                return fields.TryGetValue(key, out int value) ? value : 0;
            }
        }

        public sealed class Group
        {
            public int BackgroundId { get; }
            public string ChildVirtualPath { get; }
            public IReadOnlyList<Record> Records { get; }

            internal Group(int backgroundId, string childVirtualPath, List<Record> records)
            {
                BackgroundId = backgroundId;
                ChildVirtualPath = childVirtualPath;
                Records = records.AsReadOnly();
            }
        }

        private static readonly HashSet<string> NumericFields = new HashSet<string>(StringComparer.Ordinal)
        {
            "hidden", "hurtable", "attacking", "regen_hp", "regen_mp", "recmp",
            "gain_mp_a", "gain_mp_v", "spark", "effect", "weapon_drop",
            "type", "kind", "reserve", "state", "etc_mode", "etc_width",
            "dircontrol", "caughtact"
        };

        private readonly string[] paths;
        private readonly string[] hashes;
        private readonly Dictionary<int, Group> byBackgroundId;

        public IReadOnlyList<Group> Groups { get; }
        public int RecordCount { get; }
        public string InputFingerprint { get; }
        public string SemanticFingerprint { get; }

        private LoganBackgroundModeInput(List<Group> groups, List<string> inputPaths,
            List<string> inputHashes)
        {
            Groups = groups.AsReadOnly();
            paths = inputPaths.ToArray();
            hashes = inputHashes.ToArray();
            byBackgroundId = new Dictionary<int, Group>(groups.Count);
            int recordCount = 0;
            foreach (Group group in groups)
            {
                byBackgroundId.Add(group.BackgroundId, group);
                recordCount += group.Records.Count;
            }
            RecordCount = recordCount;
            InputFingerprint = HashContract("NTSD28-BG-MODE-INPUT-v1", writer =>
            {
                for (int index = 0; index < hashes.Length; index++)
                {
                    writer.Write(index == 0 ? "data/bg_mode.dat" : groups[index - 1].ChildVirtualPath);
                    writer.Write(hashes[index]);
                }
            });
            SemanticFingerprint = HashContract("NTSD28-BG-MODE-SEMANTIC-v1", writer =>
            {
                foreach (Group group in groups)
                {
                    writer.Write(group.BackgroundId);
                    writer.Write(group.Records.Count);
                    foreach (Record record in group.Records)
                    {
                        writer.Write(record.SelectionCondition);
                        writer.Write(record.Hurtable);
                        writer.Write(record.Attacking);
                        writer.Write(record.RegenHp);
                        writer.Write(record.RegenMp);
                        writer.Write(record.RecMp);
                        writer.Write(record.GainMpAttacker);
                        writer.Write(record.GainMpVictim);
                        writer.Write(record.Spark);
                        writer.Write(record.Effect);
                        writer.Write(record.WeaponDrop);
                        writer.Write(record.Type);
                        writer.Write(record.Kind);
                        writer.Write(record.Reserve);
                        writer.Write(record.State);
                        writer.Write(record.EtcMode);
                        writer.Write(record.EtcWidth);
                        writer.Write(record.DirControl);
                        writer.Write(record.CaughtAct);
                        writer.Write(record.DropCandidateIds.Count);
                        foreach (int id in record.DropCandidateIds)
                            writer.Write(id);
                    }
                }
            });
        }

        public static LoganBackgroundModeInput Capture(BattleContentSource source)
        {
            if (source == null || !source.IsLoganRuntime)
                throw new ArgumentException("A Logan runtime content source is required.", nameof(source));

            string parentPath = Path.Combine(source.DatRoot, "data", "bg_mode.dat");
            if (!File.Exists(parentPath))
                return null;

            byte[] parentBytes = File.ReadAllBytes(parentPath);
            List<(int id, string child)> descriptors = ParseDescriptors(
                Encoding.UTF8.GetString(parentBytes));
            var groups = new List<Group>(descriptors.Count);
            var inputPaths = new List<string>(descriptors.Count + 1) { parentPath };
            var inputHashes = new List<string>(descriptors.Count + 1) { Hash(parentBytes) };
            foreach ((int id, string child) descriptor in descriptors)
            {
                string childPath = source.ResolveDatPath(descriptor.child);
                if (!File.Exists(childPath))
                    throw new InvalidDataException("Background-mode child is missing: " + descriptor.child);
                byte[] bytes = File.ReadAllBytes(childPath);
                List<Record> records = ParseRecords(Encoding.UTF8.GetString(bytes));
                groups.Add(new Group(descriptor.id, descriptor.child, records));
                inputPaths.Add(childPath);
                inputHashes.Add(Hash(bytes));
            }
            return new LoganBackgroundModeInput(groups, inputPaths, inputHashes);
        }

        public bool TryGetRecord(int backgroundId, int recordIndex, out Record record)
        {
            record = null;
            if (!byBackgroundId.TryGetValue(backgroundId, out Group group) ||
                recordIndex < 0 || recordIndex >= group.Records.Count)
                return false;
            record = group.Records[recordIndex];
            return true;
        }

        public void AssertInputsCurrent()
        {
            for (int index = 0; index < paths.Length; index++)
            {
                if (!File.Exists(paths[index]) ||
                    !string.Equals(Hash(File.ReadAllBytes(paths[index])), hashes[index], StringComparison.Ordinal))
                    throw new InvalidDataException("Background-mode input changed after capture: " + paths[index]);
            }
        }

        private static List<(int id, string child)> ParseDescriptors(string text)
        {
            string[] tokens = Lf2DatTokenizer.Tokenize(text);
            var result = new List<(int id, string child)>();
            var seen = new HashSet<int>();
            bool inGroup = false;
            int id = -1;
            string child = null;
            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index];
                if (token == "<bg_information>")
                {
                    if (inGroup)
                        throw new InvalidDataException("Nested background-mode descriptor.");
                    inGroup = true;
                    id = -1;
                    child = null;
                    continue;
                }
                if (token == "<bg_information_end>")
                {
                    if (!inGroup || id < 0 || string.IsNullOrWhiteSpace(child) || !seen.Add(id))
                        throw new InvalidDataException("Invalid or duplicate background-mode descriptor.");
                    result.Add((id, child));
                    if (result.Count > 600)
                        throw new InvalidDataException("Background-mode descriptor count exceeds native capacity.");
                    inGroup = false;
                    continue;
                }
                if (!inGroup || (token != "bg:" && token != "file:"))
                    continue;
                string value = NextValue(tokens, ref index);
                if (token == "file:")
                    child = value;
                else if (!int.TryParse(value, NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out id) || id < 0)
                    throw new InvalidDataException("Invalid background-mode id.");
            }
            if (inGroup || result.Count == 0)
                throw new InvalidDataException("Background-mode parent is incomplete or empty.");
            return result;
        }

        private static List<Record> ParseRecords(string text)
        {
            string[] tokens = Lf2DatTokenizer.Tokenize(text);
            var result = new List<Record>();
            Dictionary<string, int> fields = null;
            List<int> ids = null;
            string name = null;
            for (int index = 0; index < tokens.Length; index++)
            {
                string token = tokens[index];
                if (token == "<mode>")
                {
                    if (fields != null)
                        throw new InvalidDataException("Nested background-mode record.");
                    fields = new Dictionary<string, int>(StringComparer.Ordinal);
                    ids = new List<int>();
                    name = null;
                    continue;
                }
                if (token == "<mode_end>")
                {
                    if (fields == null)
                        throw new InvalidDataException("Unexpected background-mode record end.");
                    result.Add(new Record(name, fields, ids));
                    if (result.Count > 100)
                        throw new InvalidDataException("Background-mode record count exceeds native capacity.");
                    fields = null;
                    continue;
                }
                if (fields == null || !token.EndsWith(":", StringComparison.Ordinal))
                    continue;
                string key = token.Substring(0, token.Length - 1);
                string value = NextValue(tokens, ref index);
                if (key == "name")
                    name = value;
                else if (key == "id")
                {
                    if (int.TryParse(value, NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture, out int dropId))
                        ids.Add(dropId);
                }
                else if (NumericFields.Contains(key) && int.TryParse(value,
                    NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int number))
                    fields[key] = number;
            }
            if (fields != null || result.Count == 0)
                throw new InvalidDataException("Background-mode child is incomplete or empty.");
            return result;
        }

        private static string NextValue(string[] tokens, ref int index)
        {
            if (index + 1 >= tokens.Length || tokens[index + 1].StartsWith("<", StringComparison.Ordinal) ||
                tokens[index + 1].EndsWith(":", StringComparison.Ordinal))
                throw new InvalidDataException("Background-mode field has no value: " + tokens[index]);
            return tokens[++index];
        }

        private static string HashContract(string tag, Action<BinaryWriter> write)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(tag);
                write(writer);
                writer.Flush();
                return Hash(stream.ToArray());
            }
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
