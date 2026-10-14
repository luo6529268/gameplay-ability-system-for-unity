using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using NTSD.Animation;

internal static class Program
{
    private static readonly List<object> Results = new List<object>();

    private static int Main(string[] args)
    {
        string output = Path.GetFullPath(args[0]);
        if (Directory.Exists(output)) throw new IOException("Test output must be new.");
        Directory.CreateDirectory(output);
        Check("block rounding and exact D0 payload", () =>
        {
            Assert(BattleAstcBlockData.PayloadBytes(1, 1) == 16);
            Assert(BattleAstcBlockData.PayloadBytes(5, 4) == 32);
            Assert(BattleAstcBlockData.PayloadBytes(532, 1100) == 585200);
        });
        Check("negative and overflow dimensions rejected", () =>
        {
            Reject(() => BattleAstcBlockData.PayloadBytes(0, 4));
            Reject(() => BattleAstcBlockData.PayloadBytes(0xFFFFFF, 0xFFFFFF));
        });
        byte[] valid = FileBytes(4, 4);
        Check("valid structural file accepted", () => BattleAstcBlockData.ValidateFile(valid, 4, 4, Hash(valid)));
        foreach (int offset in new[] { 0, 4, 5, 6, 7, 10, 13 })
        {
            int changed = offset;
            Check("invalid header byte " + changed + " rejected", () =>
            {
                byte[] bytes = (byte[])valid.Clone();
                bytes[changed]++;
                Reject(() => BattleAstcBlockData.ValidateHeader(bytes, bytes.Length, 4, 4));
            });
        }
        Check("truncated and trailing payload rejected", () =>
        {
            Reject(() => BattleAstcBlockData.ValidateHeader(valid.Take(15).ToArray(), 15, 4, 4));
            Reject(() => BattleAstcBlockData.ValidateHeader(valid, valid.Length - 1, 4, 4));
            Reject(() => BattleAstcBlockData.ValidateHeader(valid, valid.Length + 1, 4, 4));
        });
        Check("payload mutation rejected by SHA", () =>
        {
            byte[] changed = (byte[])valid.Clone();
            changed[16]++;
            Reject(() => BattleAstcBlockData.ValidateFile(changed, 4, 4, Hash(valid)));
        });
        Check("raw upload excludes 16 byte container and releases CPU readability", () =>
        {
            var entry = new BattleAstcTextures.Entry("source", "sourcehash", "file", Hash(valid), 4, 4);
            var texture = new UnityEngine.Texture2D(4, 4, UnityEngine.TextureFormat.ASTC_4x4, false);
            BattleAstcTextures.Upload(texture, entry, valid);
            Assert(texture.Raw.SequenceEqual(valid.Skip(16)));
            Assert(texture.Applied && !texture.Readable && !texture.Mips);
        });
        Check("compressed budget has no retained RGBA and accounts read/upload", () =>
        {
            var budget = new BattleImageMemoryBudget(0);
            budget.AddCompressedImage(1024, 1024, 1048576 + 16);
            budget.ReserveAtlas(0);
            Assert(budget.SourceTextureBytes == 1048576 && budget.RetainedPixelBytes == 0);
            Assert(budget.ResidentBytes == 1048576);
            Assert(budget.LoadingBytes == 1048576 + 1048576 + 2 * (1048576 + 16) + 65536);
        });
        Check("old publication counted and limit rejects before upload", () =>
        {
            var budget = new BattleImageMemoryBudget(1048576, 1048576, 10485760);
            budget.AddCompressedImage(4, 4, valid.Length);
            Reject(() => budget.ReserveAtlas(0));
        });
        Check("native clamp CPU retention is conditional", () =>
        {
            var budget = new BattleImageMemoryBudget(0);
            budget.AddClampedCell(4, 4, false);
            Assert(budget.SourceTextureBytes == 64 && budget.RetainedPixelBytes == 0);
        });
        Check("RGBA compatibility cells still allocate GPU in ASTC mode", () =>
        {
            var budget = new BattleImageMemoryBudget(0);
            budget.AddImage(new BMPLoader.VerifiedImageLayout { Width = 1, Height = 1, FileBytes = 20, IsPng = true }, false, false);
            budget.AddClampedCell(4, 4, false);
            budget.ReserveAtlas(0);
            Assert(budget.SourceTextureBytes == 68);
            Assert(budget.RetainedPixelBytes == 0);
            Assert(budget.LoadingBytes == 66308);
        });
        Check("manifest selection and stale container gate", () =>
        {
            string content = Path.Combine(output, "content");
            string derived = Path.Combine(content, "derived_astc");
            Directory.CreateDirectory(derived);
            WriteNew(Path.Combine(derived, "sample.astc"), valid);
            var manifest = new
            {
                version = 1, pixelContract = BattleAstcBlockData.PixelContract,
                entries = new[] { new { source = "vfs/sample.png", sourceSha256 = "originalhash",
                    astc = "sample.astc", astcSha256 = Hash(valid), width = 4, height = 4 } }
            };
            WriteNew(Path.Combine(derived, "manifest.json"), JsonSerializer.SerializeToUtf8Bytes(manifest));
            var source = new BattleContentSource(content);
            var images = new[] { new LoganVisualContentCandidate.ImageInput(Path.Combine(content, "vfs/sample.png"), "originalhash") };
            var captured = BattleAstcTextures.Capture(source, images);
            Assert(captured.TryGet(images[0].Path, "originalhash", out var entry));
            Assert(entry.ReadVerifiedFile().SequenceEqual(valid));
            Reject(() => captured.TryGet(images[0].Path, "wronghash", out _));
            captured.AssertInputsCurrent();
            byte[] mutated = (byte[])valid.Clone();
            mutated[16]++;
            string stale = Path.Combine(derived, "stale.astc");
            WriteNew(stale, mutated);
            var staleEntry = new BattleAstcTextures.Entry(images[0].Path, "originalhash", stale, Hash(valid), 4, 4);
            Reject(() => staleEntry.ReadVerifiedFile());
        });
        foreach (string badPath in new[] { "../escape.png", Path.GetFullPath(output) })
        {
            string bad = badPath;
            Check("manifest path confinement " + bad, () =>
            {
                string content = Path.Combine(output, "negative-" + Results.Count);
                string derived = Path.Combine(content, "derived_astc");
                Directory.CreateDirectory(derived);
                var manifest = new { version = 1, pixelContract = BattleAstcBlockData.PixelContract,
                    entries = new[] { new { source = bad, sourceSha256 = "hash", astc = "sample.astc",
                        astcSha256 = Hash(valid), width = 4, height = 4 } } };
                WriteNew(Path.Combine(derived, "manifest.json"), JsonSerializer.SerializeToUtf8Bytes(manifest));
                Reject(() => BattleAstcTextures.Capture(new BattleContentSource(content), Array.Empty<LoganVisualContentCandidate.ImageInput>()));
            });
        }
        if (args.Length > 1)
        {
            Check("all deployed ASTC files pass actual production header/SHA validator", () =>
            {
                string content = Path.GetFullPath(args[1]);
                using (JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(content, "derived_astc/manifest.json"))))
                {
                    int count = 0;
                    foreach (var entry in manifest.RootElement.GetProperty("entries").EnumerateArray())
                    {
                        byte[] bytes = File.ReadAllBytes(Path.Combine(content, "derived_astc", entry.GetProperty("astc").GetString()));
                        BattleAstcBlockData.ValidateFile(bytes, entry.GetProperty("width").GetInt32(),
                            entry.GetProperty("height").GetInt32(), entry.GetProperty("astcSha256").GetString());
                        count++;
                    }
                    Assert(count > 0);
                }
            });
        }
        var result = new { scope = "production C# structural/budget/upload-call tests with explicit Unity API shims; no GPU or Unity process",
            count = Results.Count, failed = Results.Count(r => !(bool)r.GetType().GetProperty("passed").GetValue(r)), cases = Results };
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, new JsonSerializerOptions { WriteIndented = true });
        WriteNew(Path.Combine(output, "result.json"), json);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(json));
        return result.failed == 0 ? 0 : 1;
    }

    private static void Check(string name, Action action)
    {
        try { action(); Results.Add(new { name, passed = true, error = (string)null }); }
        catch (Exception e) { Results.Add(new { name, passed = false, error = e.ToString() }); }
    }
    private static void Assert(bool value) { if (!value) throw new InvalidOperationException("Assertion failed."); }
    private static void Reject(Action action)
    {
        try { action(); } catch (Exception e) when (e is InvalidDataException || e is OverflowException || e is ArgumentException) { return; }
        throw new InvalidOperationException("Invalid input was accepted.");
    }
    private static byte[] FileBytes(int width, int height)
    {
        byte[] bytes = Enumerable.Repeat((byte)0x42, 16 + BattleAstcBlockData.PayloadBytes(width, height)).ToArray();
        new byte[] { 0x13, 0xAB, 0xA1, 0x5C, 4, 4, 1 }.CopyTo(bytes, 0);
        for (int i = 0; i < 3; i++) { bytes[7 + i] = (byte)(width >> (8 * i)); bytes[10 + i] = (byte)(height >> (8 * i)); bytes[13 + i] = (byte)(1 >> (8 * i)); }
        return bytes;
    }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static void WriteNew(string path, byte[] bytes) { using (var stream = new FileStream(path, FileMode.CreateNew)) stream.Write(bytes); }
}

// Standalone tests link production logic and replace only external Unity APIs.
namespace UnityEngine
{
    public enum TextureFormat { RGBA32, ASTC_4x4 }
    public sealed class Texture2D
    {
        public TextureFormat format;
        public byte[] Raw;
        public bool Applied, Readable = true, Mips;
        public Texture2D(int width, int height, TextureFormat value, bool mips) { format = value; Mips = mips; }
        public void LoadRawTextureData(IntPtr data, int size) { Raw = new byte[size]; Marshal.Copy(data, Raw, 0, size); }
        public void Apply(bool updateMipmaps, bool makeNoLongerReadable) { Applied = true; Mips = updateMipmaps; Readable = !makeNoLongerReadable; }
    }
    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { IncludeFields = true });
    }
}
namespace NTSD.Animation
{
    internal sealed class BattleContentSource
    {
        public readonly string RuntimeRoot;
        public bool IsLoganRuntime => true;
        public BattleContentSource(string root) { RuntimeRoot = root; }
    }
    internal sealed class LoganVisualContentCandidate
    {
        internal sealed class ImageInput
        {
            public readonly string Path, Sha256;
            public ImageInput(string path, string hash) { Path = path; Sha256 = hash; }
        }
    }
    internal static class BattleAtlasLayoutPlanner { internal const int PageSize = 2048; }
    internal static class BMPLoader
    {
        internal struct VerifiedImageLayout { internal int Width, Height; internal long FileBytes; internal bool IsPng; }
    }
}
