using NUnit.Framework;
using NTSD.App;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07ProjectBattleModeConfigEditorTests
    {
        [Test]
        public void ProjectModeAsset_LoadsSerializedFieldsAndCapturesIndependentSnapshot()
        {
            ProjectBattleModeConfig asset = ProjectBattleModeConfig.LoadDefault();
            Assert.That(asset, Is.Not.Null);
            ProjectBattleModeConfig.Snapshot saved = asset.Capture();
            Assert.That(saved.ComboBound, Is.EqualTo(1));
            Assert.That(saved.ComboFacing, Is.EqualTo(1));
            Assert.That(saved.ComboRespond, Is.EqualTo(50));
            Assert.That(saved.ComboCaughtAct, Is.EqualTo(1));
            Assert.That(saved.KnockoutLifetimeTicks, Is.EqualTo(70));
            Assert.That(saved.AllowedBattleModes, Is.EqualTo(new[] { 0, 1, 4 }));
            Assert.That(saved.TypeImagePaths, Has.Length.EqualTo(7));
            Assert.That(saved.StageTeam5DeathSoundPath, Is.EqualTo("data/m_join.wav"));
            Assert.That(saved.Fingerprint, Has.Length.EqualTo(64));

            ProjectBattleModeConfig clone = Object.Instantiate(asset);
            try
            {
                clone.Combo.respond++;
                clone.Knockout.allowedBattleModes[0] = 3;
                clone.Knockout.typeImagePaths[0] = "project/icon.png";
                ProjectBattleModeConfig.Snapshot changed = clone.Capture();
                Assert.That(changed.Fingerprint, Is.Not.EqualTo(saved.Fingerprint));
                Assert.That(saved.ComboRespond, Is.EqualTo(50));
                Assert.That(saved.AllowedBattleModes, Is.EqualTo(new[] { 0, 1, 4 }));
                Assert.That(saved.TypeImagePaths[0], Is.EqualTo("sprite/kill/c.png"));
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }
    }
}
