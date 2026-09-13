#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B11")]
    public sealed class NTSD28B11ContentSourceEditorTests
    {
        private static string Root(string name)
        {
            return Path.GetFullPath(Path.Combine(Path.GetTempPath(), "NTSD content roots", name));
        }

        private static object Source(string factory, string root)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("NTSD.Animation.BattleContentSource"))
                .FirstOrDefault(value => value != null);
            Assert.That(type, Is.Not.Null, "Production content source contract does not exist yet.");
            return Invoke(type.GetMethod(factory), null, root);
        }

        private static object Invoke(MethodInfo method, object target, params object[] arguments)
        {
            Assert.That(method, Is.Not.Null);
            try
            {
                return method.Invoke(target, arguments);
            }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static string Get(object source, string property)
        {
            return (string)source.GetType().GetProperty(property).GetValue(source);
        }

        private static string Resolve(object source, string method, params object[] arguments)
        {
            return (string)Invoke(source.GetType().GetMethod(method), source, arguments);
        }

        [Test]
        public void LoganRootsKeepCatalogRegistryDatAndImagesDistinct()
        {
            string root = Root("Logan");
            object source = Source("ForLoganRuntime", root);
            Assert.That(Get(source, "CatalogPath"), Is.EqualTo(Path.Combine(root, "catalog.csv")));
            Assert.That(Get(source, "DataIndexPath"), Is.EqualTo(Path.Combine(root, "decoded_dat", "data", "data.txt")));
            Assert.That(Get(source, "DatRoot"), Is.EqualTo(Path.Combine(root, "decoded_dat")));
            Assert.That(Get(source, "ImageRoot"), Is.EqualTo(Path.Combine(root, "vfs")));
        }

        [Test]
        public void LoganDatKeyIsRelativeToDecodedRootRatherThanRegistryDirectory()
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.That(Resolve(source, "ResolveDatPath", "c\\nar\\nar.dat"),
                Is.EqualTo(Path.Combine(Root("Logan"), "decoded_dat", "c", "nar", "nar.dat")));
        }

        [TestCase("c\\nar\\nar.png")]
        [TestCase("sprite/face/naruto_f.png")]
        [TestCase("sprite/small/naruto_s.png")]
        public void LoganSheetHeadAndSmallUseTheSameVfsRoot(string key)
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            string result = Resolve(source, "ResolveImagePath", key, Root("unrelated-dat-directory"));
            Assert.That(result, Is.EqualTo(Path.GetFullPath(Path.Combine(Root("Logan"), "vfs", key.Replace('\\', '/')))));
        }

        [Test]
        public void SpriteExtensionIsNotSilentlyRewritten()
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.That(Resolve(source, "ResolveImagePath", "c/nar/nar.bmp", null), Does.EndWith("nar.bmp"));
        }

        [Test]
        public void SourceInstancesCannotChangeOneAnothersRoots()
        {
            object first = Source("ForLoganRuntime", Root("first"));
            string before = Resolve(first, "ResolveDatPath", "c/nar.dat");
            object second = Source("ForLoganRuntime", Root("second"));
            Assert.That(Resolve(second, "ResolveDatPath", "c/nar.dat"), Is.Not.EqualTo(before));
            Assert.That(Resolve(first, "ResolveDatPath", "c/nar.dat"), Is.EqualTo(before));
            Assert.That(first.GetType().GetProperties().All(property => property.SetMethod == null), Is.True);
        }

        [Test]
        public void SourceKindAndLegacyContainingImageRootAreExplicit()
        {
            object logan = Source("ForLoganRuntime", Root("Logan"));
            object unity = Source("ForUnityProject", Root("Unity"));
            Assert.That(logan.GetType().GetProperty("IsLoganRuntime").GetValue(logan), Is.True);
            Assert.That(unity.GetType().GetProperty("IsLoganRuntime").GetValue(unity), Is.False);
            Assert.That(Get(unity, "ImageRoot"), Is.EqualTo(Root("Unity")));
        }

        [Test]
        public void UnityDefaultRootsAndAssetsDatRemainProjectRelative()
        {
            object source = Source("ForUnityProject", Root("Unity"));
            Assert.That(Get(source, "CatalogPath"), Is.Null);
            Assert.That(Get(source, "DataIndexPath"), Is.EqualTo(Path.Combine(Root("Unity"), "Assets", "NTSD", "Config", "data.txt")));
            Assert.That(Resolve(source, "ResolveDatPath", "Assets/NTSD/Config/Character/naruto.dat"),
                Is.EqualTo(Path.Combine(Root("Unity"), "Assets", "NTSD", "Config", "Character", "naruto.dat")));
            Assert.That(Resolve(source, "ResolveDatPath", "Character/naruto.dat"),
                Is.EqualTo(Path.Combine(Root("Unity"), "Assets", "NTSD", "Config", "Character", "naruto.dat")));
        }

        [Test]
        public void UnityAssetsImageAndDatRelativeImageKeepTheirExistingBases()
        {
            object source = Source("ForUnityProject", Root("Unity"));
            string datDirectory = Path.Combine(Root("Unity"), "Assets", "NTSD", "Config", "Character");
            Assert.That(Resolve(source, "ResolveImagePath", "Assets/NTSD/Sprite/Character/nar.bmp", datDirectory),
                Is.EqualTo(Path.Combine(Root("Unity"), "Assets", "NTSD", "Sprite", "Character", "nar.bmp")));
            Assert.That(Resolve(source, "ResolveImagePath", "relative.bmp", datDirectory),
                Is.EqualTo(Path.Combine(datDirectory, "relative.bmp")));
        }

        [Test]
        public void UnityDatRelativeParentPathCanReachSiblingAssetsButNotLeaveProject()
        {
            object source = Source("ForUnityProject", Root("Unity"));
            string datDirectory = Path.Combine(Root("Unity"), "Assets", "NTSD", "Config", "Character");
            Assert.That(Resolve(source, "ResolveImagePath", "../../Sprite/Character/nar.bmp", datDirectory),
                Is.EqualTo(Path.Combine(Root("Unity"), "Assets", "NTSD", "Sprite", "Character", "nar.bmp")));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveImagePath", "../../../../../outside.bmp", datDirectory));
        }

        [Test]
        public void LegacyRelativeImageNeedsAnAbsoluteDirectoryWithinTheProject()
        {
            object source = Source("ForUnityProject", Root("Unity"));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveImagePath", "nar.bmp", "relative-directory"));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveImagePath", "nar.bmp", Root("other-project")));
        }

        [Test]
        public void NormalizationKeepsContainedParentSegmentsAndRejectsUncKeys()
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.That(Resolve(source, "ResolveDatPath", "c/./nar/../nar.dat"),
                Is.EqualTo(Path.Combine(Root("Logan"), "decoded_dat", "c", "nar.dat")));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveDatPath", "\\\\server\\share\\nar.dat"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void MissingOptionalImageStaysMissing(string key)
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.That(Resolve(source, "ResolveImagePath", key, null), Is.EqualTo(string.Empty));
        }

        [TestCase("../outside.dat")]
        [TestCase("/outside.dat")]
        [TestCase("C:/outside.dat")]
        public void CanonicalDatKeysCannotEscapeTheDeclaredRoot(string key)
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveDatPath", key));
        }

        [TestCase("../outside.png")]
        [TestCase("/outside.png")]
        [TestCase("C:/outside.png")]
        public void ImageKeysCannotEscapeTheDeclaredRoot(string key)
        {
            object source = Source("ForLoganRuntime", Root("Logan"));
            Assert.Throws<ArgumentException>(() => Resolve(source, "ResolveImagePath", key, null));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void MissingRootIsRejectedInsteadOfUsingTheWorkingDirectory(string root)
        {
            Assert.Throws<ArgumentException>(() => Source("ForLoganRuntime", root));
        }
    }
}
#endif
