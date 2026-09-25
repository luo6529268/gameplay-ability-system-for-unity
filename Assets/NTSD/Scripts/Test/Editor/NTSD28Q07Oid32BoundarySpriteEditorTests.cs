#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q07Oid32BoundarySprite")]
    public sealed class NTSD28Q07Oid32BoundarySpriteEditorTests
    {
        [Test]
        public void FormalHunterPic64CurrentIndexedRectResultIsMeasured()
        {
            string root = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            ProjectBattleModeConfig.Snapshot mode =
                ProjectBattleModeConfig.LoadDefault().Capture();
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(root), mode);
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs.TryGetValue(32, out LF2CharacterDataWrapper wrapper), Is.True);
            Assert.That(wrapper.characterData.files.Count, Is.GreaterThan(0));
            SpriteFileInfo file = wrapper.characterData.files[0];
            Assert.That(file.filePath.Replace('\\', '/').EndsWith(
                "m/nin/hun.png", StringComparison.OrdinalIgnoreCase), Is.True);
            Assert.That(File.Exists(file.filePath), Is.True, file.filePath);
            Assert.That(file.startFrame, Is.EqualTo(0));
            Assert.That(file.endFrame, Is.EqualTo(139));
            Assert.That(file.width, Is.EqualTo(79));
            Assert.That(file.height, Is.EqualTo(79));
            Assert.That(file.row, Is.EqualTo(10));
            Assert.That(file.col, Is.EqualTo(14));

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(
                    texture, File.ReadAllBytes(file.filePath)), Is.True);
                Assert.That(texture.width, Is.EqualTo(799));
                Assert.That(texture.height, Is.EqualTo(480));
                Rect?[] rects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                    file, texture.width, texture.height, file.col, file.row);
                Assert.That(rects.Length, Is.EqualTo(140));
                TestContext.Progress.WriteLine(
                    $"oid=32 pic=64 source={file.filePath} size={texture.width}x{texture.height} " +
                    $"pic54={(rects[54].HasValue ? rects[54].Value.ToString() : "null")} " +
                    $"pic64={(rects[64].HasValue ? rects[64].Value.ToString() : "null")}");
                Assert.That(rects[54].HasValue, Is.True);
                Assert.That(rects[64].HasValue, Is.False,
                    "Diagnostic of current indexed-rect omission; this is not parity acceptance.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
