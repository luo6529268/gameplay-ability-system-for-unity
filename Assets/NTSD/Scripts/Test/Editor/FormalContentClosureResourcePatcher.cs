#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    internal static class FormalContentClosureResourcePatcher
    {
        private const int HeaderLength = 123;
        private const int InitialExpectedPatchCount = 60;
        private const int InitialExpectedFileCount = 23;
        private const int ResidualExpectedPatchCount = 4;
        private const int ResidualExpectedFileCount = 1;
        private const string DatPassword =
            "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        [MenuItem("Tools/NTSD/Tests/Apply CAP-S0-1 Frame Scalar Resource Patch")]
        private static void ApplyFromMenu()
        {
            PatchPlan plan = BuildPlan();
            ApplyPlan(plan);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log(
                $"[S0-FORMAL-CONTENT-CLOSURE-001] Applied " +
                $"{plan.PatchCount} declared scalar tokens across " +
                $"{plan.Files.Count} DAT files.");
        }

        [MenuItem("Tools/NTSD/Tests/Apply CAP-S0-1 Frame Scalar Residual Patch")]
        private static void ApplyResidualFromMenu()
        {
            PatchPlan plan = BuildResidualPlan();
            ApplyPlan(plan);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log(
                $"[S0-FORMAL-CONTENT-CLOSURE-001] Applied " +
                $"{plan.PatchCount} residual scalar tokens across " +
                $"{plan.Files.Count} DAT file.");
        }

        internal static PatchPlan BuildPlan()
        {
            return BuildPlan(
                DeclaredPatches(),
                InitialExpectedPatchCount,
                InitialExpectedFileCount);
        }

        internal static PatchPlan BuildResidualPlan()
        {
            return BuildPlan(
                ResidualPatches(),
                ResidualExpectedPatchCount,
                ResidualExpectedFileCount);
        }

        private static PatchPlan BuildPlan(
            IEnumerable<FieldPatch> declaredPatches,
            int expectedPatchCount,
            int expectedFileCount)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                throw new InvalidOperationException("Unity project root is unavailable.");
            }

            FieldPatch[] patches = declaredPatches.ToArray();
            if (patches.Length != expectedPatchCount)
            {
                throw new InvalidOperationException(
                    $"Declared patch count is {patches.Length}; expected {expectedPatchCount}.");
            }

            string duplicate = patches
                .GroupBy(patch => patch.Identity, StringComparer.Ordinal)
                .Where(group => group.Count() != 1)
                .Select(group => group.Key)
                .FirstOrDefault();
            if (duplicate != null)
            {
                throw new InvalidOperationException($"Duplicate patch identity: {duplicate}");
            }

            IGrouping<string, FieldPatch>[] groups = patches
                .GroupBy(patch => patch.RelativePath, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .ToArray();
            if (groups.Length != expectedFileCount)
            {
                throw new InvalidOperationException(
                    $"Declared resource count is {groups.Length}; expected {expectedFileCount}.");
            }

            var files = new List<FilePatchPlan>(groups.Length);
            var preflightErrors = new List<string>();
            foreach (IGrouping<string, FieldPatch> group in groups)
            {
                try
                {
                    string absolutePath = Path.GetFullPath(Path.Combine(
                        projectRoot,
                        group.Key.Replace('/', Path.DirectorySeparatorChar)));
                    string expectedRoot = Path.GetFullPath(projectRoot) + Path.DirectorySeparatorChar;
                    if (!absolutePath.StartsWith(expectedRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Declared path escapes project root: {group.Key}");
                    }

                    byte[] originalBytes = File.ReadAllBytes(absolutePath);
                    DecodedDat decoded = DecodeFile(originalBytes, group.Key);
                    TextPatchResult result = ApplyDeclaredTextPatches(
                        decoded.Text,
                        group.OrderBy(patch => patch.Identity, StringComparer.Ordinal).ToArray());
                    if (!decoded.IsPlaintext)
                    {
                        EnsureAscii(result.PatchedText, group.Key);
                    }

                    byte[] patchedBytes = EncodeFile(originalBytes, decoded, result.PatchedText);
                    DecodedDat roundTrip = DecodeFile(patchedBytes, group.Key);
                    if (roundTrip.IsPlaintext != decoded.IsPlaintext ||
                        !string.Equals(roundTrip.Text, result.PatchedText, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"DAT round-trip changed format or plaintext: {group.Key}");
                    }

                    files.Add(new FilePatchPlan(
                        group.Key,
                        absolutePath,
                        originalBytes,
                        patchedBytes,
                        result.PatchCount,
                        decoded.IsPlaintext));
                }
                catch (Exception exception)
                {
                    preflightErrors.Add($"{group.Key}: {exception.Message}");
                }
            }

            if (preflightErrors.Count != 0)
            {
                throw new InvalidOperationException(
                    "DAT patch preflight differences:\n" +
                    string.Join("\n", preflightErrors));
            }

            int patchCount = files.Sum(file => file.PatchCount);
            if (patchCount != expectedPatchCount)
            {
                throw new InvalidOperationException(
                    $"Preflight resolved {patchCount} tokens; expected {expectedPatchCount}.");
            }

            return new PatchPlan(
                files,
                patchCount,
                expectedPatchCount,
                expectedFileCount);
        }

        internal static void ApplyPlan(PatchPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (plan.PatchCount != plan.ExpectedPatchCount ||
                plan.Files.Count != plan.ExpectedFileCount)
            {
                throw new InvalidOperationException("Patch plan is incomplete.");
            }

            var tempPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var replacedFiles = new List<FilePatchPlan>();
            try
            {
                foreach (FilePatchPlan file in plan.Files)
                {
                    byte[] currentBytes = File.ReadAllBytes(file.AbsolutePath);
                    if (!currentBytes.SequenceEqual(file.OriginalBytes))
                    {
                        throw new InvalidOperationException(
                            $"Resource changed after preflight: {file.RelativePath}");
                    }

                    string tempPath = file.AbsolutePath +
                        $".cap-s0-1-{Guid.NewGuid():N}.tmp";
                    File.WriteAllBytes(tempPath, file.PatchedBytes);
                    if (!File.ReadAllBytes(tempPath).SequenceEqual(file.PatchedBytes))
                    {
                        throw new IOException(
                            $"Temporary resource verification failed: {file.RelativePath}");
                    }

                    tempPaths.Add(file.AbsolutePath, tempPath);
                }

                foreach (FilePatchPlan file in plan.Files)
                {
                    string tempPath = tempPaths[file.AbsolutePath];
                    File.Replace(tempPath, file.AbsolutePath, null);
                    replacedFiles.Add(file);
                }

                foreach (FilePatchPlan file in plan.Files)
                {
                    byte[] actual = File.ReadAllBytes(file.AbsolutePath);
                    if (!actual.SequenceEqual(file.PatchedBytes))
                    {
                        throw new IOException(
                            $"Atomic replacement verification failed: {file.RelativePath}");
                    }
                }
            }
            catch
            {
                RollBackReplacedFiles(replacedFiles);
                throw;
            }
            finally
            {
                foreach (string tempPath in tempPaths.Values)
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
        }

        private static void RollBackReplacedFiles(IEnumerable<FilePatchPlan> files)
        {
            foreach (FilePatchPlan file in files.Reverse())
            {
                string rollbackPath = file.AbsolutePath +
                    $".cap-s0-1-rollback-{Guid.NewGuid():N}.tmp";
                try
                {
                    File.WriteAllBytes(rollbackPath, file.OriginalBytes);
                    File.Replace(rollbackPath, file.AbsolutePath, null);
                }
                finally
                {
                    if (File.Exists(rollbackPath))
                    {
                        File.Delete(rollbackPath);
                    }
                }
            }
        }

        private static TextPatchResult ApplyDeclaredTextPatches(
            string text,
            IReadOnlyList<FieldPatch> patches)
        {
            var replacements = new List<TextReplacement>(patches.Count);
            var errors = new List<string>();
            foreach (FieldPatch patch in patches)
            {
                MatchCollection frameMatches = Regex.Matches(
                    text,
                    @"(?ms)^[ \t]*<frame>[ \t]+" + patch.FrameId +
                    @"(?:[ \t]+[^\r\n]*)?\r?\n(?<body>.*?^[ \t]*<frame_end>[ \t]*\r?$)");
                if (frameMatches.Count == 0)
                {
                    errors.Add($"Frame {patch.FrameId} is missing");
                    continue;
                }

                Group body = frameMatches[frameMatches.Count - 1].Groups["body"];
                string bodyText = body.Value;
                Match firstContentLine = Regex.Match(bodyText, @"(?m)^[ \t]*\S.*$");
                if (!firstContentLine.Success)
                {
                    errors.Add($"Frame {patch.FrameId} has no scalar line");
                    continue;
                }

                MatchCollection tokenMatches = Regex.Matches(
                    firstContentLine.Value,
                    @"(?<![A-Za-z0-9_])" + Regex.Escape(patch.Field) +
                    @"(?<separator>[ \t]*:[ \t]*)(?<value>-?\d*)(?![A-Za-z0-9_])");
                if (tokenMatches.Count == 0)
                {
                    errors.Add(
                        $"Frame{patch.FrameId} top-level {patch.Field} token is missing");
                    continue;
                }

                Group valueMatch = tokenMatches[tokenMatches.Count - 1].Groups["value"];
                if (!string.Equals(
                    valueMatch.Value,
                    patch.Before,
                    StringComparison.Ordinal))
                {
                    errors.Add(
                        $"Frame{patch.FrameId} {patch.Field}: expected before " +
                        $"'{patch.Before}', actual '{valueMatch.Value}'");
                    continue;
                }

                replacements.Add(new TextReplacement(
                    body.Index + firstContentLine.Index + valueMatch.Index,
                    valueMatch.Length,
                    patch.After,
                    patch.Identity));
            }

            if (errors.Count != 0)
            {
                throw new InvalidOperationException(string.Join("; ", errors));
            }

            string duplicateOffset = replacements
                .GroupBy(replacement => replacement.Start)
                .Where(group => group.Count() != 1)
                .Select(group => string.Join(", ", group.Select(item => item.Identity)))
                .FirstOrDefault();
            if (duplicateOffset != null)
            {
                throw new InvalidOperationException(
                    $"Multiple patches target the same token: {duplicateOffset}");
            }

            var builder = new StringBuilder(text);
            foreach (TextReplacement replacement in replacements
                .OrderByDescending(item => item.Start))
            {
                builder.Remove(replacement.Start, replacement.Length);
                builder.Insert(replacement.Start, replacement.Value);
            }

            return new TextPatchResult(builder.ToString(), replacements.Count);
        }

        private static DecodedDat DecodeFile(byte[] bytes, string relativePath)
        {
            int plainStart = bytes.Length >= 3 &&
                bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf
                    ? 3
                    : 0;
            string head = Encoding.ASCII.GetString(
                bytes,
                plainStart,
                Math.Min(64, bytes.Length - plainStart));
            string trimmedHead = head.TrimStart();
            bool isPlaintext = trimmedHead.StartsWith("<bmp_begin>", StringComparison.Ordinal) ||
                trimmedHead.StartsWith("<frame>", StringComparison.Ordinal) ||
                trimmedHead.StartsWith("<stage>", StringComparison.Ordinal);
            if (!isPlaintext)
            {
                if (bytes.Length <= HeaderLength)
                {
                    throw new InvalidOperationException(
                        $"Encrypted DAT is shorter than its header: {relativePath}");
                }

                return new DecodedDat(DecryptPayload(bytes), false, HeaderLength);
            }

            string text = StrictUtf8.GetString(bytes, plainStart, bytes.Length - plainStart);
            byte[] roundTrip = StrictUtf8.GetBytes(text);
            if (!BytesEqual(bytes, plainStart, roundTrip))
            {
                throw new InvalidOperationException(
                    $"Plaintext DAT is not strict UTF-8 round-trippable: {relativePath}");
            }

            return new DecodedDat(text, true, plainStart);
        }

        private static string DecryptPayload(byte[] bytes)
        {
            int payloadLength = bytes.Length - HeaderLength;
            var payload = new byte[payloadLength];
            for (int i = 0; i < payloadLength; i++)
            {
                unchecked
                {
                    payload[i] = (byte)(bytes[i + HeaderLength] -
                        (byte)DatPassword[i % DatPassword.Length]);
                }
            }

            return Encoding.ASCII.GetString(payload);
        }

        private static byte[] EncodeFile(
            byte[] originalBytes,
            DecodedDat decoded,
            string text)
        {
            if (!decoded.IsPlaintext)
            {
                return EncryptPayload(originalBytes, text);
            }

            byte[] payload = StrictUtf8.GetBytes(text);
            var bytes = new byte[decoded.PrefixLength + payload.Length];
            Buffer.BlockCopy(originalBytes, 0, bytes, 0, decoded.PrefixLength);
            Buffer.BlockCopy(payload, 0, bytes, decoded.PrefixLength, payload.Length);
            return bytes;
        }

        private static byte[] EncryptPayload(byte[] originalBytes, string text)
        {
            byte[] payload = Encoding.ASCII.GetBytes(text);
            var bytes = new byte[HeaderLength + payload.Length];
            Buffer.BlockCopy(originalBytes, 0, bytes, 0, HeaderLength);
            for (int i = 0; i < payload.Length; i++)
            {
                unchecked
                {
                    bytes[i + HeaderLength] = (byte)(payload[i] +
                        (byte)DatPassword[i % DatPassword.Length]);
                }
            }

            return bytes;
        }

        private static bool BytesEqual(byte[] source, int sourceOffset, byte[] expected)
        {
            if (source.Length - sourceOffset != expected.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (source[sourceOffset + i] != expected[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureAscii(string text, string relativePath)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] > 0x7f)
                {
                    throw new InvalidOperationException(
                        $"Non-ASCII plaintext at offset {i}: {relativePath}");
                }
            }
        }

        private static IEnumerable<FieldPatch> DeclaredPatches()
        {
            yield return P(Effect("heart0.dat"), 399, "state", 0, 3005);
            yield return P(Effect("heart0.dat"), 399, "next", 0, 1000);
            yield return P(Special("wind.dat"), 207, "next", 205, 208);
            yield return P(Special("wind.dat"), 211, "next", 212, 312);
            yield return PBlank(Special("clay_bird2.dat"), 134, "hit_j", "0 ");
            yield return PBlank(Special("water.dat"), 45, "dvz", "0 ");
            yield return PBlank(Special("water.dat"), 46, "dvz", "0 ");
            yield return P(Special("4TK_ball.dat"), 211, "next", 212, 213);
            yield return P(Special("4TK_ball.dat"), 213, "pic", 68, 67);
            yield return P(Special("4TK_ball.dat"), 214, "pic", 69, 68);
            yield return P(Special("4TK_ball.dat"), 214, "next", 1000, 219);
            yield return P(Special("earth_creature.dat"), 68, "pic", 4, 3);
            yield return P(Special("earth_creature.dat"), 68, "next", 65, 69);
            yield return P(Special("shadow.dat"), 211, "next", 212, 213);
            yield return P(Special("shadow.dat"), 213, "pic", 33, 32);
            yield return P(Special("shadow.dat"), 213, "wait", 200, 1);
            yield return P(Special("shadow.dat"), 213, "next", 999, 214);
            yield return P(Special("katon_kakuzu.dat"), 13, "pic", 27, 26);
            yield return P(Character("sasori.dat"), 45, "pic", 40, 29);
            yield return P(Character("sasori.dat"), 45, "wait", 0, 15);
            yield return P(Character("sasori.dat"), 45, "next", 36, 999);
            yield return P(Character("sasori.dat"), 45, "centerx", 39, 19);
            yield return P(Character("sasori.dat"), 45, "centery", 79, 91);
            yield return P(Character("sasori.dat"), 390, "next", 391, 3910);
            yield return P(Character("naruto_clone.dat"), 243, "pic", 63, 37);
            yield return P(Character("naruto_clone.dat"), 243, "next", 244, 235);
            yield return P(Character("naruto_clone.dat"), 243, "dvx", 15, 0);
            yield return P(Character("naruto_clone.dat"), 243, "dvy", -7, 0);
            yield return P(Character("naruto.dat"), 123, "dvy", 0, 550);
            yield return P(Character("naruto.dat"), 123, "hit_d", 0, 370);
            yield return P(Character("yamato.dat"), 102, "centery", 79, 790);
            yield return P(Character("rock_lee.dat"), 299, "next", 300, 294);
            yield return PBlank(Character("rock_lee.dat"), 332, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 334, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 336, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 338, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 340, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 342, "dvx", "0 ");
            yield return PBlank(Character("rock_lee.dat"), 344, "dvx", "0 ");
            yield return P(Character("neji.dat"), 102, "centery", 79, 790);
            yield return P(Character("shikamaru.dat"), 102, "centery", 79, 790);
            yield return P(Character("kankuro.dat"), 102, "centery", 79, 790);
            yield return P(Character("temari.dat"), 102, "centery", 79, 790);
            yield return P(Character("temari.dat"), 318, "pic", 123, 122);
            yield return P(Character("temari.dat"), 318, "next", 319, 349);
            yield return P(Character("deidara.dat"), 7, "hit_Dj", 265, 0);
            yield return P(Character("deidara.dat"), 8, "hit_Dj", 265, 0);
            yield return P("Assets/NTSD/Config/chars/weapon3.dat", 399, "state", 0, 3005);
            yield return P("Assets/NTSD/Config/chars/weapon3.dat", 399, "next", 0, 1000);
            yield return P("Assets/NTSD/Config/chars/weapon3.dat", 399, "dvx", 0, 550);
            yield return P("Assets/NTSD/Config/chars/weapon3.dat", 399, "dvy", 0, 550);
            yield return P("Assets/NTSD/Config/FrameConfig/weapon8.dat", 399, "next", 0, 1000);
            yield return P("Assets/NTSD/Config/FrameConfig/weapon8.dat", 399, "dvx", 0, 550);
            yield return P("Assets/NTSD/Config/FrameConfig/weapon8.dat", 399, "dvy", 0, 550);
            yield return P(Effect("heart.dat"), 399, "state", 0, 3005);
            yield return P(Effect("heart.dat"), 399, "next", 0, 1000);
            yield return P(Effect("heart2.dat"), 399, "state", 0, 3005);
            yield return P(Effect("heart2.dat"), 399, "next", 0, 1000);
            yield return P(Effect("heart3.dat"), 399, "state", 0, 3005);
            yield return P(Effect("heart3.dat"), 399, "next", 0, 1000);
        }

        private static IEnumerable<FieldPatch> ResidualPatches()
        {
            yield return P(Character("rock_lee.dat"), 338, "dvy", 0, 550);
            yield return P(Character("rock_lee.dat"), 340, "dvy", 0, 550);
            yield return P(Character("rock_lee.dat"), 342, "dvy", 0, 550);
            yield return P(Character("rock_lee.dat"), 344, "dvy", 0, 550);
        }

        private static FieldPatch P(
            string path,
            int frameId,
            string field,
            int before,
            int after)
        {
            return new FieldPatch(
                path,
                frameId,
                field,
                before.ToString(),
                after.ToString());
        }

        private static FieldPatch PBlank(
            string path,
            int frameId,
            string field,
            string after)
        {
            return new FieldPatch(path, frameId, field, string.Empty, after);
        }

        private static string Character(string file) =>
            $"Assets/NTSD/Config/Character/{file}";

        private static string Effect(string file) =>
            $"Assets/NTSD/Config/effect/{file}";

        private static string Special(string file) =>
            $"Assets/NTSD/Config/specialattack/{file}";

        internal sealed class PatchPlan
        {
            internal PatchPlan(
                IReadOnlyList<FilePatchPlan> files,
                int patchCount,
                int expectedPatchCount,
                int expectedFileCount)
            {
                Files = files;
                PatchCount = patchCount;
                ExpectedPatchCount = expectedPatchCount;
                ExpectedFileCount = expectedFileCount;
            }

            internal IReadOnlyList<FilePatchPlan> Files { get; }
            internal int PatchCount { get; }
            internal int ExpectedPatchCount { get; }
            internal int ExpectedFileCount { get; }
        }

        internal sealed class FilePatchPlan
        {
            internal FilePatchPlan(
                string relativePath,
                string absolutePath,
                byte[] originalBytes,
                byte[] patchedBytes,
                int patchCount,
                bool isPlaintext)
            {
                RelativePath = relativePath;
                AbsolutePath = absolutePath;
                OriginalBytes = originalBytes;
                PatchedBytes = patchedBytes;
                PatchCount = patchCount;
                IsPlaintext = isPlaintext;
            }

            internal string RelativePath { get; }
            internal string AbsolutePath { get; }
            internal byte[] OriginalBytes { get; }
            internal byte[] PatchedBytes { get; }
            internal int PatchCount { get; }
            internal bool IsPlaintext { get; }
        }

        private sealed class DecodedDat
        {
            internal DecodedDat(string text, bool isPlaintext, int prefixLength)
            {
                Text = text;
                IsPlaintext = isPlaintext;
                PrefixLength = prefixLength;
            }

            internal string Text { get; }
            internal bool IsPlaintext { get; }
            internal int PrefixLength { get; }
        }

        private sealed class FieldPatch
        {
            internal FieldPatch(
                string relativePath,
                int frameId,
                string field,
                string before,
                string after)
            {
                RelativePath = relativePath;
                FrameId = frameId;
                Field = field;
                Before = before;
                After = after;
            }

            internal string RelativePath { get; }
            internal int FrameId { get; }
            internal string Field { get; }
            internal string Before { get; }
            internal string After { get; }
            internal string Identity =>
                $"{RelativePath}|Frame{FrameId}|{Field}";
        }

        private sealed class TextReplacement
        {
            internal TextReplacement(
                int start,
                int length,
                string value,
                string identity)
            {
                Start = start;
                Length = length;
                Value = value;
                Identity = identity;
            }

            internal int Start { get; }
            internal int Length { get; }
            internal string Value { get; }
            internal string Identity { get; }
        }

        private sealed class TextPatchResult
        {
            internal TextPatchResult(string patchedText, int patchCount)
            {
                PatchedText = patchedText;
                PatchCount = patchCount;
            }

            internal string PatchedText { get; }
            internal int PatchCount { get; }
        }
    }

    [Category("FormalContentClosure")]
    [Category("FrameScalarPatchPreflight")]
    public sealed class FormalContentClosureResourcePatcherEditorTests
    {
        [Test]
        [Explicit("Pre-change-only witness; passed as Unity job 1ffcf2087f94467da8a9ba8e9ccf344e before the declared resource apply action.")]
        public void DeclaredMixedFormatDatFrameScalarPatchPreflightIsExactAndReadOnly()
        {
            FormalContentClosureResourcePatcher.PatchPlan plan =
                FormalContentClosureResourcePatcher.BuildPlan();

            Assert.That(plan.Files.Count, Is.EqualTo(23));
            Assert.That(plan.PatchCount, Is.EqualTo(60));
            Assert.That(plan.Files.Count(file => file.IsPlaintext), Is.EqualTo(3));
            Assert.That(plan.Files.Count(file => !file.IsPlaintext), Is.EqualTo(20));
            foreach (FormalContentClosureResourcePatcher.FilePatchPlan file in plan.Files)
            {
                Assert.That(File.ReadAllBytes(file.AbsolutePath),
                    Is.EqualTo(file.OriginalBytes),
                    $"Preflight must not mutate {file.RelativePath}");
            }
        }

        [Test]
        [Explicit("Pre-change-only residual witness; passed as Unity job 91a405ebeb0d4fa3bef0795400404e45 before the declared residual apply action.")]
        public void DeclaredRockLeeResidualPatchPreflightIsExactAndReadOnly()
        {
            FormalContentClosureResourcePatcher.PatchPlan plan =
                FormalContentClosureResourcePatcher.BuildResidualPlan();

            Assert.That(plan.Files.Count, Is.EqualTo(1));
            Assert.That(plan.PatchCount, Is.EqualTo(4));
            foreach (FormalContentClosureResourcePatcher.FilePatchPlan file in plan.Files)
            {
                Assert.That(File.ReadAllBytes(file.AbsolutePath),
                    Is.EqualTo(file.OriginalBytes),
                    $"Residual preflight must not mutate {file.RelativePath}");
            }
        }
    }
}
#endif
