using System.Reflection;
using NUnit.Framework;
using NTSD.App;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07ProjectBattleModeConfigEditorTests
    {
        [Test]
        public void ProjectModeAsset_CapturesSelectedEtcModeInFrozenIdentity()
        {
            ProjectBattleModeConfig asset = ProjectBattleModeConfig.LoadDefault();
            FieldInfo field = typeof(ProjectBattleModeConfig).GetField(
                "selectedModeEtcMode", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            ProjectBattleModeConfig.Snapshot original = asset.Capture();
            Assert.That(original.SelectedModeEtcMode, Is.EqualTo(1));

            ProjectBattleModeConfig clone = Object.Instantiate(asset);
            try
            {
                field.SetValue(clone, 0);
                ProjectBattleModeConfig.Snapshot changed = clone.Capture();
                Assert.That(changed.SelectedModeEtcMode, Is.Zero);
                Assert.That(changed.Fingerprint, Is.Not.EqualTo(original.Fingerprint));
                Assert.That(original.SelectedModeEtcMode, Is.EqualTo(1));
                field.SetValue(clone, -1);
                Assert.Throws<System.IO.InvalidDataException>(() => clone.Capture());
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        [Test]
        public void ProjectModeAsset_CapturesPresentationGateInFrozenIdentity()
        {
            ProjectBattleModeConfig asset = ProjectBattleModeConfig.LoadDefault();
            FieldInfo field = typeof(ProjectBattleModeConfig).GetField(
                "selectedModeReviveLivesGate54", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            ProjectBattleModeConfig.Snapshot original = asset.Capture();
            Assert.That(original.SelectedModeReviveLivesGate54, Is.Zero);

            ProjectBattleModeConfig clone = Object.Instantiate(asset);
            try
            {
                field.SetValue(clone, 3);
                ProjectBattleModeConfig.Snapshot changed = clone.Capture();
                Assert.That(changed.SelectedModeReviveLivesGate54, Is.EqualTo(3));
                Assert.That(changed.Fingerprint, Is.Not.EqualTo(original.Fingerprint));
                Assert.That(original.SelectedModeReviveLivesGate54, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        [Test]
        public void ProjectModeAsset_CapturesSelectedStageGateInFrozenIdentity()
        {
            ProjectBattleModeConfig asset = ProjectBattleModeConfig.LoadDefault();
            FieldInfo field = typeof(ProjectBattleModeConfig).GetField(
                "selectedStageGate50", BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo property = typeof(ProjectBattleModeConfig.Snapshot).GetProperty(
                "SelectedStageGate50", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            Assert.That(property, Is.Not.Null);
            ProjectBattleModeConfig.Snapshot original = asset.Capture();
            Assert.That(property.GetValue(original), Is.EqualTo(1));

            ProjectBattleModeConfig clone = Object.Instantiate(asset);
            try
            {
                field.SetValue(clone, 3);
                ProjectBattleModeConfig.Snapshot changed = clone.Capture();
                Assert.That(property.GetValue(changed), Is.EqualTo(3));
                Assert.That(original.Fingerprint, Is.Not.EqualTo(changed.Fingerprint));
                Assert.That(property.GetValue(original), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

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
