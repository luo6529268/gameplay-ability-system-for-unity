#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NUnit.Framework;

using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NativeFunctionKeyRouterEditorTests
    {
        [Test]
        public void RouteVirtualKey_ProjectsCompleteF1ToF12CommandTable()
        {
            NTSD28NativeFunctionKeyHostCommand[] hostCommands =
            {
                NTSD28NativeFunctionKeyHostCommand.TogglePause,
                NTSD28NativeFunctionKeyHostCommand.SingleStep,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.LeaveBattle,
                NTSD28NativeFunctionKeyHostCommand.ToggleFastMode,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.None,
                NTSD28NativeFunctionKeyHostCommand.VolumeDown,
                NTSD28NativeFunctionKeyHostCommand.VolumeUp,
            };
            NTSD28NativeFunctionKeySessionCommand[] sessionCommands =
            {
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.ToggleLock,
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.ToggleHitResource,
                NTSD28NativeFunctionKeySessionCommand.FillActiveMp,
                NTSD28NativeFunctionKeySessionCommand.DropModeObjects,
                NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects,
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.None,
                NTSD28NativeFunctionKeySessionCommand.None,
            };
            NTSD28NativeFunctionKeyDisposition[] dispositions =
            {
                NTSD28NativeFunctionKeyDisposition.HostCommand,
                NTSD28NativeFunctionKeyDisposition.HostCommand,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                NTSD28NativeFunctionKeyDisposition.HostCommand,
                NTSD28NativeFunctionKeyDisposition.HostCommand,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                NTSD28NativeFunctionKeyDisposition.NativeNoAction,
                NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand,
                NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand,
            };

            for (int index = 0; index < 12; index++)
            {
                NTSD28NativeFunctionKeyRouteResult result =
                    NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x70 + index, false);

                Assert.That(
                    result.Key,
                    Is.EqualTo((NTSD28NativeFunctionKey)(index + 1)));
                Assert.That(result.HostCommand, Is.EqualTo(hostCommands[index]));
                Assert.That(result.SessionCommand, Is.EqualTo(sessionCommands[index]));
                Assert.That(result.Disposition, Is.EqualTo(dispositions[index]));
                Assert.That(result.IsFunctionKey, Is.True);
            }
        }

        [Test]
        public void RouteVirtualKey_RejectsValuesOutsideFKeyRange()
        {
            NTSD28NativeFunctionKeyRouteResult before =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x6F, false);
            NTSD28NativeFunctionKeyRouteResult after =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x7C, false);

            Assert.That(before.Key, Is.EqualTo(NTSD28NativeFunctionKey.None));
            Assert.That(
                before.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.NotFunctionKey));
            Assert.That(before.IsFunctionKey, Is.False);
            Assert.That(after.Key, Is.EqualTo(NTSD28NativeFunctionKey.None));
            Assert.That(
                after.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.NotFunctionKey));
        }

        [Test]
        public void Route_RejectsOneShotRepeatBeforeMaintenance()
        {
            NTSD28NativeFunctionKeyRouteResult repeatedF1 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F1,
                    true,
                    NTSD28NativeFunctionKeyModifiers.None,
                    NTSD28NativeFunctionKeyRouteContext.Allowed);
            NTSD28NativeFunctionKeyRouteResult repeatedControlF9 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F9,
                    true,
                    new NTSD28NativeFunctionKeyModifiers(true),
                    NTSD28NativeFunctionKeyRouteContext.Allowed);
            NTSD28NativeFunctionKeyRouteResult repeatedF4 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F4,
                    true,
                    NTSD28NativeFunctionKeyModifiers.None,
                    NTSD28NativeFunctionKeyRouteContext.Allowed);

            Assert.That(
                repeatedF1.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.RejectedAutoRepeat));
            Assert.That(
                repeatedF1.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.AutoRepeat));
            Assert.That(
                repeatedF1.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.None));
            Assert.That(
                repeatedControlF9.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.RejectedAutoRepeat));
            Assert.That(
                repeatedControlF9.MaintenanceCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyMaintenanceCommand.None));
            Assert.That(
                repeatedF4.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.RejectedAutoRepeat));
            Assert.That(
                repeatedF4.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.None));
        }

        [Test]
        public void Route_F11F12RemainContinuousAndBypassRepeatAndContext()
        {
            var blocked = new NTSD28NativeFunctionKeyRouteContext(
                false,
                false,
                true,
                false);
            NTSD28NativeFunctionKeyRouteResult f11 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F11,
                    true,
                    new NTSD28NativeFunctionKeyModifiers(true),
                    blocked);
            NTSD28NativeFunctionKeyRouteResult f12 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F12,
                    true,
                    NTSD28NativeFunctionKeyModifiers.None,
                    blocked);

            Assert.That(
                f11.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand));
            Assert.That(
                f11.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.VolumeDown));
            Assert.That(f11.RejectReason, Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.None));
            Assert.That(f11.IsOneShotCommand, Is.False);
            Assert.That(
                f12.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand));
            Assert.That(
                f12.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.VolumeUp));
        }

        [Test]
        public void Route_ControlMaintenancePrecedesBattleContext()
        {
            var blocked = new NTSD28NativeFunctionKeyRouteContext(
                false,
                false,
                true,
                false);
            var control = new NTSD28NativeFunctionKeyModifiers(true);

            NTSD28NativeFunctionKeyRouteResult retry =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F9,
                    false,
                    control,
                    blocked);
            NTSD28NativeFunctionKeyRouteResult discard =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F10,
                    false,
                    control,
                    blocked);

            Assert.That(
                retry.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.MaintenanceCommand));
            Assert.That(
                retry.MaintenanceCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyMaintenanceCommand.RetryPendingRecording));
            Assert.That(
                discard.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.MaintenanceCommand));
            Assert.That(
                discard.MaintenanceCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording));
        }

        [Test]
        public void Route_AppliesContextGatesInAuthorityOrderAndScope()
        {
            NTSD28NativeFunctionKeyRouteResult outsideBattle =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F8,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(false, false, true, false));
            NTSD28NativeFunctionKeyRouteResult outsideBattleF4 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F4,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(false, false, true, false));
            NTSD28NativeFunctionKeyRouteResult wrongMainState =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F8,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(true, false, true, false));
            NTSD28NativeFunctionKeyRouteResult lockedF8 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F8,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(true, true, true, false));
            NTSD28NativeFunctionKeyRouteResult delayedF8 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F8,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(true, true, false, false));
            NTSD28NativeFunctionKeyRouteResult delayedF6 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F6,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(true, true, false, false));
            NTSD28NativeFunctionKeyRouteResult lockDoesNotBlockF3 =
                NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F3,
                    false,
                    NTSD28NativeFunctionKeyModifiers.None,
                    new NTSD28NativeFunctionKeyRouteContext(true, true, true, true));

            Assert.That(
                outsideBattle.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.NotInBattle));
            Assert.That(
                outsideBattleF4.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.NotInBattle));
            Assert.That(
                outsideBattleF4.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.None));
            Assert.That(
                wrongMainState.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.MainStateDisallows));
            Assert.That(
                lockedF8.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.F3Locked));
            Assert.That(
                delayedF8.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.GlobalDelayActive));
            Assert.That(
                delayedF6.SessionCommand,
                Is.EqualTo(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource));
            Assert.That(
                lockDoesNotBlockF3.SessionCommand,
                Is.EqualTo(NTSD28NativeFunctionKeySessionCommand.ToggleLock));
        }

        [Test]
        public void Route_PreservesPlainF10NoActionAndOneShotPredicate()
        {
            NTSD28NativeFunctionKeyRouteResult f1 =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x70, false);
            NTSD28NativeFunctionKeyRouteResult f3 =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x72, false);
            NTSD28NativeFunctionKeyRouteResult f10 =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x79, false);

            Assert.That(f1.IsOneShotCommand, Is.True);
            Assert.That(f3.IsOneShotCommand, Is.True);
            Assert.That(
                f10.Disposition,
                Is.EqualTo(NTSD28NativeFunctionKeyDisposition.NativeNoAction));
            Assert.That(f10.IsOneShotCommand, Is.False);
            Assert.That(
                f10.HostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.None));
            Assert.That(
                f10.SessionCommand,
                Is.EqualTo(NTSD28NativeFunctionKeySessionCommand.None));
        }

        [Test]
        public void Route_RemainsAllocationFreeAfterWarmup()
        {
            NTSD28NativeFunctionKeyRouteResult warm =
                NTSD28NativeFunctionKeyRouter.RouteVirtualKey(0x70, false);
            int checksum = (int)warm.Disposition;

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                NTSD28NativeFunctionKeyRouteResult result =
                    NTSD28NativeFunctionKeyRouter.RouteVirtualKey(
                        0x70 + (index % 12),
                        false);
                checksum ^= (int)result.HostCommand;
                checksum ^= (int)result.SessionCommand;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.EqualTo(int.MinValue));
        }
    }
}
#endif
