#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28Q07FormalFixtureProjection")]
    public sealed class NTSD28Q07FormalFixtureProjectionEditorTests
    {
        private const string FormalRoot =
            @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";

        [Test]
        public void IndexedFormalFixturesHaveMeasuredUnityProjection()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            LoganObjectCatalog formal = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            LoganObjectCatalog staged = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(stagedRoot), mode);
            var formalEntries = formal.Entries.ToDictionary(entry => entry.Id);
            var stagedEntries = staged.Entries.ToDictionary(entry => entry.Id);
            Assert.That(formalEntries.ContainsKey(205), Is.False, "retired poison oid is absent");
            Assert.That(stagedEntries.ContainsKey(205), Is.False, "staged poison oid is absent");

            Dictionary<int, LF2CharacterDataWrapper> formalConfigs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(formal);
            Dictionary<int, LF2CharacterDataWrapper> stagedConfigs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(staged);
            int[] objectIds = { 214, 2, 3, 1, 11, 33, 120, 204 };
            foreach (int id in objectIds)
            {
                Assert.That(stagedEntries[id].DatSha256,
                    Is.EqualTo(formalEntries[id].DatSha256), "DAT byte identity oid=" + id);
                Assert.That(stagedEntries[id].SourcePath,
                    Is.EqualTo(formalEntries[id].SourcePath), "catalog path oid=" + id);
                Assert.That(Describe(stagedConfigs[id].characterData),
                    Is.EqualTo(Describe(formalConfigs[id].characterData)), "converted oid=" + id);
            }

            LF2CharacterData flash = formalConfigs[214].characterData;
            Assert.That(flash.files, Has.Count.EqualTo(1));
            Assert.That(flash.files[0].startFrame, Is.Zero);
            Assert.That(flash.files[0].endFrame, Is.EqualTo(13));
            Assert.That(formalConfigs[2].characterData.running_speed, Is.EqualTo(16f));
            Assert.That(formalConfigs[3].characterData.running_speed, Is.EqualTo(18f));
            Assert.That(formalConfigs[1].characterData.running_speed, Is.EqualTo(17f));
            Assert.That(formalConfigs[11].characterData.running_speed, Is.EqualTo(20f));
            Assert.That(formalConfigs[33].characterData.running_speed, Is.EqualTo(14f));
            Assert.That(formalConfigs[120].characterData.weapon_hp, Is.EqualTo(200));
            Assert.That(formalConfigs[120].characterData.weapon_drop_hurt, Is.EqualTo(35));
            Assert.That(formalConfigs[120].characterData.weapon_hit_sound,
                Is.EqualTo("data\\027.wav"));

            var naruto = new LF2FrameCache();
            naruto.Load(formalConfigs[2]);
            Assert.That(naruto.GetFrameDataById(0).hit_Dj, Is.EqualTo(271));
            LF2FrameData frame272 = naruto.GetFrameDataById(272);
            Assert.That(frame272.opoints, Is.Empty);
            Assert.That(frame272.opoint.HasValue, Is.False);
            TestContext.WriteLine("Q07 formal fixture: flash 0..13; speeds 16/18/17/20/14; oid205 absent; Naruto frame272 no opoint");
        }

        [Test]
        public void FormalFlashSpriteRangeSelfCheckUsesCurrentCatalog()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(stagedRoot), mode);
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckSpriteFileRangeParsingContracts", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(Dictionary<int, LF2CharacterDataWrapper>) }, null);
            Assert.That(check, Is.Not.Null);
            Assert.DoesNotThrow(() => check.Invoke(null, new object[] { configs }));
        }

        private static string Describe(LF2CharacterData data)
        {
            return string.Join("|", data.files.Select(file =>
                file.startFrame + "-" + file.endFrame)) + ";" +
                data.walking_frame_rate + ";" + data.walking_speed + ";" +
                data.running_frame_rate + ";" + data.running_speed + ";" +
                data.running_speedz + ";" + data.heavy_walking_speed + ";" +
                data.jump_height + ";" + data.dash_height + ";" +
                data.rowing_distance + ";" + data.weapon_hp + ";" +
                data.weapon_drop_hurt + ";" + data.weapon_hit_sound;
        }
    }
}
#endif
