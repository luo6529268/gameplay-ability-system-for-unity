#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28B11VisualContentCandidateEditorTests
    {
        private string root;
        private byte[] imageBytes;
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        [SetUp]
        public void PrepareOwnedSource()
        {
            imageBytes = File.ReadAllBytes(Path.Combine(ProjectRoot, "Temp/NTSD28PngAlpha/fixture.dat"));
            root = CreateRuntime();
        }

        private string CreateRuntime()
        {
            string runtime = Path.Combine(ProjectRoot, "Temp/NTSD28VisualCandidate", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(runtime, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(runtime, "vfs/c"));
            File.WriteAllText(Path.Combine(runtime, "catalog.csv"), "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(runtime, "decoded_dat/a.dat"), "<bmp_begin>\nname: Test\nhead: c/head.png\nsmall: c/small.png\nfile(20-19): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n", new UTF8Encoding(false));
            foreach (string name in new[] { "body", "head", "small" })
                File.WriteAllBytes(Path.Combine(runtime, "vfs/c/" + name + ".png"), imageBytes);
            return runtime;
        }

        private static object Invoke(MethodInfo method, object target, params object[] arguments)
        {
            Assert.That(method, Is.Not.Null, "Required candidate/verified image API is missing.");
            try { return method.Invoke(target, arguments); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static object Capture(string runtime)
        {
            Type type = typeof(BattleContentSource).Assembly.GetType("NTSD.Animation.LoganVisualContentCandidate");
            Assert.That(type, Is.Not.Null, "Bound visual content candidate is missing.");
            return Invoke(type.GetMethod("Capture"), null, BattleContentSource.ForLoganRuntime(runtime), null);
        }

        private static object Property(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
        private static void Verify(object candidate) => Invoke(candidate.GetType().GetMethod("AssertInputsCurrent"), candidate);
        private static string Hash(byte[] bytes)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
        }

        [Test]
        public void Candidate_BindsDefinitionsSheetsHeadAndSmall()
        {
            object candidate = Capture(root);
            var images = ((IEnumerable)Property(candidate, "Images")).Cast<object>().ToArray();
            Assert.That(images.Length, Is.EqualTo(3));
            Assert.That(((IList)Property(candidate, "Images")).IsReadOnly, Is.True);
            CollectionAssert.AreEquivalent(new[] { "body.png", "head.png", "small.png" }, images.Select(i => Path.GetFileName((string)Property(i, "Path"))));
            foreach (object image in images) Assert.That(Property(image, "Sha256"), Is.EqualTo(Hash(imageBytes)));
            Verify(candidate);
        }

        [Test]
        public void PngOnlyChange_ChangesVisualIdentity_WithoutChangingDefinitionIdentity()
        {
            object before = Capture(root);
            string file = Path.Combine(root, "vfs/c/head.png");
            File.WriteAllBytes(file, File.ReadAllBytes(Path.Combine(ProjectRoot, "Temp/NTSD28PngDecode/generated/palette-8-filter-0.png")));
            object after = Capture(root);
            Assert.That(Property(Property(before, "Catalog"), "DefinitionFingerprint"), Is.EqualTo(Property(Property(after, "Catalog"), "DefinitionFingerprint")));
            Assert.That(Property(before, "VisualFingerprint"), Is.Not.EqualTo(Property(after, "VisualFingerprint")));
            Assert.That(Property(before, "SourceCacheKey"), Is.Not.EqualTo(Property(after, "SourceCacheKey")));
            Assert.Throws<InvalidDataException>(() => Verify(before));
        }

        [Test]
        public void DatChange_RejectsCapturedCandidate()
        {
            object candidate = Capture(root);
            string dat = Path.Combine(root, "decoded_dat/a.dat");
            File.AppendAllText(dat, "\n# changed content\n");
            Assert.Throws<InvalidDataException>(() => Verify(candidate));
        }

        [Test]
        public void SameAssetsInDifferentRoots_DoNotShareSourceCacheKey()
        {
            object first = Capture(root);
            object second = Capture(CreateRuntime());
            Assert.That(Property(first, "VisualFingerprint"), Is.EqualTo(Property(second, "VisualFingerprint")));
            Assert.That(Property(first, "SourceCacheKey"), Is.Not.EqualTo(Property(second, "SourceCacheKey")));
        }

        [Test]
        public void MissingRequiredImage_DoesNotCreateCandidate()
        {
            string dat = Path.Combine(root, "decoded_dat/a.dat");
            File.WriteAllText(dat, File.ReadAllText(dat).Replace("c/body.png", "c/missing.png"));
            Assert.Throws<FileNotFoundException>(() => Capture(root));
        }

        [Test]
        public void VerifiedDecode_AcceptsExactBytes_RejectsStaleOrEmptyHash()
        {
            MethodInfo method = typeof(BMPLoader).GetMethod("LoadVerifiedImageData");
            Assert.That(method, Is.Not.Null);
            string file = Path.Combine(root, "vfs/c/body.png");
            var data = Task.Run(() => (BMPLoader.BmpData)Invoke(method, null, file, Hash(imageBytes))).GetAwaiter().GetResult();
            var legacy = Task.Run(() => BMPLoader.LoadBmpData(file)).GetAwaiter().GetResult();
            Assert.That(data.Pixels, Is.EqualTo(legacy.Pixels));
            Assert.Throws<InvalidDataException>(() => Invoke(method, null, file, new string('0', 64)));
            Assert.Throws<ArgumentException>(() => Invoke(method, null, file, ""));
        }

        [Test]
        public void FormalCandidate_Captures330DefinitionsAnd906ReferencedImages()
        {
            var candidate = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(@"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime"),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Assert.That(candidate.Catalog.Entries.Count, Is.EqualTo(330));
            Assert.That(candidate.Images.Count, Is.EqualTo(906));
            Assert.That(candidate.SparkInput, Is.Not.Null);
            Assert.That(candidate.SparkInput.Width, Is.EqualTo(99));
            Assert.That(candidate.SparkInput.Height, Is.EqualTo(79));
            Assert.That(candidate.SparkInput.Image.Path.Replace('\\', '/'),
                Does.EndWith("/sprite/UI/SPARK.png").IgnoreCase);
            Assert.That(candidate.SparkInput.Image.Sha256,
                Is.EqualTo("15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B"));
            Verify(candidate);
        }

        [Test]
        public void NativeSparkInputs_ChangeCandidateIdentityAndRejectStaleCapture()
        {
            string datDirectory = Path.Combine(root, "decoded_dat/data");
            string spriteDirectory = Path.Combine(root, "vfs/sprite/UI");
            Directory.CreateDirectory(datDirectory);
            Directory.CreateDirectory(spriteDirectory);
            string[] resourceRows = Enumerable.Range(0, 44)
                .Select(index => "pic: " + (index == 43
                    ? @"sprite\UI\SPARK.png" :
                    index >= 16 && index <= 21 ? "c/body.png" :
                    "unused/" + index + ".png"))
                .ToArray();
            File.WriteAllText(Path.Combine(datDirectory, "resource.dat"),
                "<bmp_begin>\n" + string.Join("\n", resourceRows) + "\n<bmp_end>\n");
            string systemPath = Path.Combine(datDirectory, "system.dat");
            File.WriteAllText(systemPath, "spark_w: 99\nspark_h: 79\n");
            string imagePath = Path.Combine(spriteDirectory, "SPARK.png");
            File.Copy(Path.Combine(ProjectRoot,
                "Assets/NTSD/Content/LoganRuntime/vfs/sprite/UI/SPARK.png"), imagePath);

            var before = (LoganVisualContentCandidate)Capture(root);
            Assert.That(before.Images.Count, Is.EqualTo(3));
            Assert.That(before.SparkInput.Width, Is.EqualTo(99));
            Assert.That(before.SparkInput.Height, Is.EqualTo(79));
            Assert.That(before.SparkInput.Image.Sha256, Is.EqualTo(Hash(File.ReadAllBytes(imagePath))));

            File.WriteAllText(systemPath, "spark_w: 98\nspark_h: 79\n");
            var changedDimensions = (LoganVisualContentCandidate)Capture(root);
            Assert.That(changedDimensions.VisualFingerprint, Is.Not.EqualTo(before.VisualFingerprint));
            Assert.That(changedDimensions.SourceCacheKey, Is.Not.EqualTo(before.SourceCacheKey));
            Assert.Throws<InvalidDataException>(() => before.AssertInputsCurrent());

            File.WriteAllBytes(imagePath, imageBytes);
            var changedImage = (LoganVisualContentCandidate)Capture(root);
            Assert.That(changedImage.VisualFingerprint,
                Is.Not.EqualTo(changedDimensions.VisualFingerprint));
            Assert.Throws<InvalidDataException>(() => changedDimensions.AssertInputsCurrent());

            string secondImagePath = Path.Combine(spriteDirectory, "SPARK2.png");
            File.Copy(imagePath, secondImagePath);
            resourceRows[43] = @"pic: sprite\UI\SPARK2.png";
            File.WriteAllText(Path.Combine(datDirectory, "resource.dat"),
                "<bmp_begin>\n" + string.Join("\n", resourceRows) + "\n<bmp_end>\n");
            var changedSelection = (LoganVisualContentCandidate)Capture(root);
            Assert.That(changedSelection.VisualFingerprint,
                Is.Not.EqualTo(changedImage.VisualFingerprint));
            Assert.Throws<InvalidDataException>(() => changedImage.AssertInputsCurrent());

            File.Delete(systemPath);
            Assert.Throws<InvalidDataException>(() => Capture(root));
        }
    }
}
#endif
