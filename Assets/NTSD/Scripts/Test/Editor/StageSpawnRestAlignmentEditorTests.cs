#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("StageSpawnRestAlignment")]
    public sealed class StageSpawnRestAlignmentEditorTests
    {
        [Test]
        public void ReusedSlotResetTransaction_ClearsARestVictimRowAndAttackerColumn()
        {
            var store = new RuntimeRestStore(64);
            Assert.That(store.SetARest(20, 7), Is.True);
            Assert.That(store.SetARest(21, 5), Is.True);
            Assert.That(store.SetVRest(20, 21, 9), Is.True);
            Assert.That(store.SetVRest(21, 20, 11), Is.True);
            Assert.That(store.SetVRest(22, 23, 13), Is.True);

            Assert.That(
                store.TryResetSlotAndAcquireBinding(
                    20,
                    out RuntimeRestBindingHandle stageSpawnLease),
                Is.True);

            Assert.That(store.IsBindingValid(stageSpawnLease), Is.True);
            Assert.That(store.GetARest(20), Is.Zero);
            Assert.That(store.GetVRest(20, 21), Is.Zero);
            Assert.That(store.GetVRest(21, 20), Is.Zero);
            Assert.That(store.GetARest(21), Is.EqualTo(5));
            Assert.That(store.GetVRest(22, 23), Is.EqualTo(13));
            Assert.That(store.ReleaseBinding(stageSpawnLease), Is.True);
        }

        [Test]
        public void ConflictingLeaseResetTransaction_RejectsWithoutMutationOrLeaseInvalidation()
        {
            var store = new RuntimeRestStore(64);
            Assert.That(store.SetARest(20, 7), Is.True);
            Assert.That(store.SetVRest(20, 21, 9), Is.True);
            Assert.That(store.SetVRest(21, 20, 11), Is.True);
            Assert.That(
                store.TryAcquireBinding(20, out RuntimeRestBindingHandle foreignLease),
                Is.True);

            Assert.That(
                store.TryResetSlotAndAcquireBinding(20, out _),
                Is.False);

            Assert.That(store.IsBindingValid(foreignLease), Is.True);
            Assert.That(store.GetARest(20), Is.EqualTo(7));
            Assert.That(store.GetVRest(20, 21), Is.EqualTo(9));
            Assert.That(store.GetVRest(21, 20), Is.EqualTo(11));
            Assert.That(store.ReleaseBinding(foreignLease), Is.True);
        }
    }
}
#endif
