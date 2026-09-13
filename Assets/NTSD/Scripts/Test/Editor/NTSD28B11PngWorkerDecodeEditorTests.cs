#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using NTSD.Animation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B11")]
    public sealed class NTSD28B11PngWorkerDecodeEditorTests
    {
        [OneTimeSetUp]
        public void RecordCurrentPipelineWithoutChangingSettings()
        {
            string pipeline = DetectPipeline();
            string outputRoot = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp/NTSD28PngDecode");
            Directory.CreateDirectory(outputRoot);
            File.WriteAllText(Path.Combine(outputRoot, "pipeline.txt"), pipeline);
            TestContext.WriteLine("PNG decode validation pipeline: " + pipeline);
        }

        private static string DetectPipeline()
        {
            var asset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (asset == null)
                return "BuiltIn";
            string name = asset.GetType().FullName ?? string.Empty;
            if (name.Contains("Universal"))
                return "URP";
            if (name.Contains("HighDefinition"))
                return "HDRP";
            return "Custom";
        }

        [Serializable]
        private sealed class Entry
        {
            public string path;
            public string rgbaPath;
            public int width;
            public int height;
            public string category;
        }

        [Serializable]
        private sealed class Manifest
        {
            public Entry[] valid;
            public Entry[] invalid;
        }

        private static Manifest Fixtures()
        {
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "Temp/NTSD28PngDecode/generated/manifest.json");
            Assert.That(File.Exists(path), Is.True, "Run Tools/NTSD28PngDecode/Generate-Fixtures.py first.");
            return JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
        }

        private static BMPLoader.BmpData OnWorker(string path)
        {
            int callerThread = Thread.CurrentThread.ManagedThreadId;
            bool previous = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                return Task.Run(() =>
                {
                    Assert.That(Thread.CurrentThread.ManagedThreadId, Is.Not.EqualTo(callerThread));
                    return BMPLoader.LoadBmpData(path);
                }).GetAwaiter().GetResult();
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previous;
            }
        }

        private static void AssertPixels(BMPLoader.BmpData actual, byte[] expected, string context)
        {
            Assert.That(actual, Is.Not.Null, context);
            Assert.That(actual.Pixels.Length * 4, Is.EqualTo(expected.Length), context);
            for (int i = 0; i < actual.Pixels.Length; i++)
            {
                Color32 value = actual.Pixels[i];
                int offset = i * 4;
                if (value.r != expected[offset] || value.g != expected[offset + 1] ||
                    value.b != expected[offset + 2] || value.a != expected[offset + 3])
                    Assert.Fail(context + " pixel " + i + " differs from expected RGBA.");
            }
        }

        [TestCase("palette")]
        [TestCase("rgba")]
        [TestCase("bmp")]
        public void ActualWorkerMatchesIndependentFixturePixels(string category)
        {
            int checkedFiles = 0;
            foreach (Entry entry in Fixtures().valid)
            {
                if (entry.category != category)
                    continue;
                BMPLoader.BmpData actual = OnWorker(entry.path);
                AssertPixels(actual, File.ReadAllBytes(entry.rgbaPath), entry.path);
                Assert.That(actual.Width, Is.EqualTo(entry.width));
                Assert.That(actual.Height, Is.EqualTo(entry.height));
                checkedFiles++;
            }
            Assert.That(checkedFiles, Is.GreaterThan(0));
        }

        [Test]
        public void MalformedOrUnsupportedPngDoesNotReturnPixels()
        {
            foreach (Entry entry in Fixtures().invalid)
                Assert.That(OnWorker(entry.path), Is.Null, entry.path);
        }

        [Test]
        public void DecoderFailureHasNoPartialImageState()
        {
            foreach (Entry entry in Fixtures().invalid)
            {
                Assert.That(PngPixelDecoder.TryDecode(File.ReadAllBytes(entry.path),
                    out int width, out int height, out byte[] pixels, out string error), Is.False, entry.path);
                Assert.That(width, Is.Zero, entry.path);
                Assert.That(height, Is.Zero, entry.path);
                Assert.That(pixels, Is.Null, entry.path);
                Assert.That(error, Is.Not.Null.And.Not.Empty, entry.path);
            }
            Assert.That(PngPixelDecoder.HasSignature(null), Is.False);
            Assert.That(PngPixelDecoder.TryDecode(null, out int emptyWidth, out int emptyHeight,
                out byte[] emptyPixels, out string emptyError), Is.False);
            Assert.That(emptyWidth, Is.Zero);
            Assert.That(emptyHeight, Is.Zero);
            Assert.That(emptyPixels, Is.Null);
            Assert.That(emptyError, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void LargestFormalSheetLoadsOnWorkerAndMatchesReferenceHash()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string evidence = Path.Combine(project, "artifacts/diagnostics/NTSD28-B11-PNG-WORKER-DECODE-001");
            string row = null;
            foreach (string candidate in File.ReadLines(Path.Combine(evidence, "corpus-reference.tsv")))
            {
                if (candidate.Split('\t')[0].Replace('\\', '/').EndsWith("/c/dan/a/win2.png", StringComparison.Ordinal))
                {
                    row = candidate;
                    break;
                }
            }
            Assert.That(row, Is.Not.Null, "Generate the formal corpus reference first.");
            string[] fields = row.Split('\t');
            long beforeManaged = GC.GetTotalMemory(false);
            long sampledManagedPeak = beforeManaged;
            int beforeGc = GC.CollectionCount(0);
            long beforeWorkingSet;
            long sampledPeak;
            BMPLoader.BmpData actual;
            using (Process process = Process.GetCurrentProcess())
            {
                beforeWorkingSet = process.WorkingSet64;
                sampledPeak = beforeWorkingSet;
                int callerThread = Thread.CurrentThread.ManagedThreadId;
                Task<BMPLoader.BmpData> load = Task.Run(() =>
                {
                    Assert.That(Thread.CurrentThread.ManagedThreadId, Is.Not.EqualTo(callerThread));
                    return BMPLoader.LoadBmpData(fields[0]);
                });
                while (!load.IsCompleted)
                {
                    process.Refresh();
                    sampledPeak = Math.Max(sampledPeak, process.WorkingSet64);
                    sampledManagedPeak = Math.Max(sampledManagedPeak, GC.GetTotalMemory(false));
                    Thread.Sleep(10);
                }
                actual = load.GetAwaiter().GetResult();
                process.Refresh();
                sampledPeak = Math.Max(sampledPeak, process.WorkingSet64);
                sampledManagedPeak = Math.Max(sampledManagedPeak, GC.GetTotalMemory(false));
            }
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Width, Is.EqualTo(2001));
            Assert.That(actual.Height, Is.EqualTo(8768));
            byte[] buffer = new byte[32768];
            using (SHA256 hash = SHA256.Create())
            {
                int used = 0;
                for (int i = 0; i < actual.Pixels.Length; i++)
                {
                    Color32 pixel = actual.Pixels[i];
                    buffer[used++] = pixel.r;
                    buffer[used++] = pixel.g;
                    buffer[used++] = pixel.b;
                    buffer[used++] = pixel.a;
                    if (used == buffer.Length)
                    {
                        hash.TransformBlock(buffer, 0, used, buffer, 0);
                        used = 0;
                    }
                }
                hash.TransformFinalBlock(buffer, 0, used);
                Assert.That(BitConverter.ToString(hash.Hash).Replace("-", string.Empty), Is.EqualTo(fields[4]));
            }
            File.WriteAllText(Path.Combine(evidence, "largest-sheet-worker-memory.txt"),
                "width=2001\nheight=8768\npixels=" + actual.Pixels.Length +
                "\nprocessWorkingSetBefore=" + beforeWorkingSet +
                "\nsampledProcessPeakWorkingSet=" + sampledPeak +
                "\nprocessWorkingSetAvailable=" + (sampledPeak > 0) +
                "\nsampledManagedHeapPeak=" + sampledManagedPeak +
                "\nmanagedBefore=" + beforeManaged + "\nmanagedAfter=" + GC.GetTotalMemory(false) +
                "\ngen0CollectionsDuringTest=" + (GC.CollectionCount(0) - beforeGc) +
                "\nrgbaHashMatches=true\nmeasurement=10ms heap/process samples; Mono process values of zero mean unavailable; not exact allocation or concurrent-prewarm peak\n");
        }

        [TestCase("c/nar/nar.png")]
        [TestCase("sprite/face/naruto_f.png")]
        [TestCase("sprite/small/naruto_s.png")]
        [TestCase("c/oro/a/atk.png")]
        [TestCase("w/5.png")]
        [TestCase("s/c.png")]
        [TestCase("data/pixel.png")]
        public void FormalPngWorkerPixelsMatchUnityMainThreadDecode(string relative)
        {
            string root = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\vfs";
            string path = Path.Combine(root, relative);
            Assert.That(File.Exists(path), Is.True, "Formal resource precondition: " + path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(texture.LoadImage(File.ReadAllBytes(path)), Is.True);
                Color32[] reference = texture.GetPixels32();
                byte[] expected = new byte[reference.Length * 4];
                for (int i = 0; i < reference.Length; i++)
                {
                    expected[i * 4] = reference[i].r;
                    expected[i * 4 + 1] = reference[i].g;
                    expected[i * 4 + 2] = reference[i].b;
                    expected[i * 4 + 3] = reference[i].a;
                }
                BMPLoader.BmpData actual = OnWorker(path);
                AssertPixels(actual, expected, relative);
                Assert.That(actual.Width, Is.EqualTo(texture.width));
                Assert.That(actual.Height, Is.EqualTo(texture.height));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
