#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q09P20LoganGridCapacity")]
    public sealed class NTSD28Q09P20LoganGridCapacityEditorTests
    {
        [Test]
        public void HidanRuntimeSheetRanges_AccumulateCapacityAndMapPic119ToHid6()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var source = BattleContentSource.ForLoganRuntime(
                Path.Combine(projectRoot, "Assets", "NTSD", "Content", "LoganRuntime"));
            string datPath = source.ResolveDatPath("c/hid/hid.dat");
            Lf2DatFile dat = new Lf2DatParserV2().ParseLoganContent(
                File.ReadAllText(datPath), datPath);
            List<SpriteFileInfo> files = CharacterAnimtorManager.BuildSpriteFilesForSource(
                dat, Path.GetDirectoryName(datPath), source);
            Assert.That(dat.Bmp.Files[3].StartIndex, Is.EqualTo(62));
            Assert.That(dat.Bmp.Files[3].EndIndex, Is.EqualTo(126));
            Assert.That(dat.Bmp.Files[4].StartIndex, Is.EqualTo(117));
            Assert.That(dat.Bmp.Files[4].EndIndex, Is.EqualTo(128));
            Assert.That(files[3].startFrame, Is.EqualTo(62));
            Assert.That(files[3].endFrame, Is.EqualTo(116));
            Assert.That(files[4].startFrame, Is.EqualTo(117));
            Assert.That(files[4].endFrame, Is.EqualTo(128));

            Rect?[] rects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                files[3], 580, 1221, files[3].col, files[3].row);
            Rect?[] followingRects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                files[4], 1083, 1160, files[4].col, files[4].row);
            List<HashSet<int>> ownership =
                CharacterAnimtorManager.BuildFirstDeclaredSpriteOwnership(files);

            Assert.That(rects.Length, Is.EqualTo(55),
                "The runtime hid2 range ends at pic116 after its 55 cells.");
            Assert.That(rects[54].HasValue, Is.True);
            Assert.That(followingRects.Length, Is.EqualTo(12));
            Assert.That(followingRects[119 - files[4].startFrame].HasValue, Is.True);
            Assert.That(ownership[3].Contains(119), Is.False);
            Assert.That(ownership[4].Contains(119), Is.True,
                "Formal frame430 pic119 belongs to cumulative hid6, not textual hid2.");
        }

        [Test]
        public void SyntheticDeclaredRange_StillStopsAtGridCapacity()
        {
            var file = new SpriteFileInfo("synthetic.png", 62, 126, 115, 110, 5, 11);
            Rect?[] rects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                file, 580, 1221, file.col, file.row);
            Rect?[] legacyRects = CharacterAnimtorManager.BuildIndexedSpriteRects(
                file, 580, 1221, file.col, file.row,
                allowBeyondGridCapacity: true);

            Assert.That(rects.Length, Is.EqualTo(55));
            Assert.That(legacyRects.Length, Is.EqualTo(65));
            Assert.That(legacyRects[57].HasValue, Is.False);
        }
    }
}
#endif
