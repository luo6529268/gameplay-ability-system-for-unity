#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleSinglePlayerRuntimeValidationEditorTests
    {
        [Test]
        public void EditorMonoPassesPureValueTransferAndRestoreReplayGate()
        {
            BattleSinglePlayerRuntimeValidationReport report =
                BattleSinglePlayerRuntimeValidation.Run("editor-mono-gate");

            Assert.That(report.status, Is.EqualTo("Passed"), report.failure);
            Assert.That(report.scriptingBackend, Is.EqualTo("Mono"));
            Assert.That(report.pureValueTransferPassed, Is.True);
            Assert.That(report.restoreReplayPassed, Is.True);
            Assert.That(report.sourceChecksum, Is.EqualTo(report.restoredChecksum));
            Assert.That(report.restoredSlot, Is.EqualTo(3));
            Assert.That(report.restoredStableId, Is.GreaterThan(0));
            Assert.That(report.restoredGeneration, Is.GreaterThan(0));
            Assert.That(report.replayChecksum, Is.Not.Empty);
        }
    }
}
#endif
