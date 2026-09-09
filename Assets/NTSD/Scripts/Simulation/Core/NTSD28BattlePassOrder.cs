using System;

namespace NTSD.Simulation
{
    internal enum NTSD28BattlePassDomain : byte
    {
        SessionPreCore = 0,
        Core = 1,
        PhysicsSlot = 2,
        SlotTail = 3,
        SessionPostCore = 4,
        PresentationHandoff = 5,
    }

    internal enum NTSD28BattleTraversalKind : byte
    {
        Scalar = 0,
        AscendingLiveSlots = 1,
        AscendingLiveSlotsReacquireAfterMutation = 2,
        PairGeometry = 3,
        AscendingAttackersThenCandidates = 4,
        NestedAscendingLiveSlotStep = 5,
        CompletedTickSnapshot = 6,
        UserAcceptedException = 7,
        NestedAscendingPhysicsSlotStep = 8,
    }

    [Flags]
    internal enum NTSD28BattlePassFlags : byte
    {
        None = 0,
        GlobalBarrier = 1 << 0,
        ImmutableSnapshot = 1 << 1,
        MayMutateSlotSet = 1 << 2,
        NestedSlotTail = 1 << 3,
        UserAcceptedException = 1 << 4,
        CompletedTickHandoff = 1 << 5,
        SessionBoundary = 1 << 6,
        NestedPhysicsSlot = 1 << 7,
    }

    internal enum NTSD28BattlePassId : byte
    {
        SessionFunctionKeyDispatch = 0,
        SessionProjectGlobalsBeforeFlow = 1,
        SessionBattleFlowClassify = 2,
        SessionStoryPhaseAdvance = 3,
        SessionProjectGlobalsBeforeCore = 4,
        SessionApplyPendingInputs = 5,
        CoreInputPhaseAdvance = 6,
        CoreSparkAdvance = 7,
        CoreProducerSampleScan = 8,
        CoreProxyAndInputRouteScan = 9,
        CoreFrameMotion = 10,
        CoreTeleport = 11,
        SlotPhysics = 12,
        SlotDeadCharacterResourceNormalize = 13,
        CoreRevival = 14,
        CoreStageDepthClampBeforeGeometry = 15,
        CoreHeldRefillBeforeGeometry = 16,
        CoreCollisionActionSnapshot = 17,
        CoreCandidateBuild = 18,
        CoreFusion = 19,
        CoreActiveWeaponCount = 20,
        CoreTypeZeroHitConsume = 21,
        CoreRandomWeaponDrop = 22,
        CoreNonTypeZeroHitConsume = 23,
        CoreCatchRelationAdvance = 24,
        CoreCatchSettlement = 25,
        CoreStageDepthClampAfterHits = 26,
        CoreHeldRefillAfterHits = 27,
        CoreStageSettlement = 28,
        CoreHorizontalImpulseFinalize = 29,
        CoreBeginNativeResourceTick = 30,
        CoreBeginFrameTick = 31,
        SlotDefinitionTransition = 32,
        SlotSpecialStateCloneMaterialize = 33,
        SlotNativeResourcePreDisplay = 34,
        SlotDisplayValues = 35,
        SlotNativeResourcePostDisplay = 36,
        SlotComputerStateRefresh = 37,
        SlotFrameStep = 38,
        SlotReactionTimers = 39,
        SlotArmorRecovery = 40,
        SlotAttackerRestDecrement = 41,
        SlotFrameZeroOpoint = 42,
        SlotState18BrokenWeaponParticles = 43,
        SlotPreviousActionCommit = 44,
        SlotWeaponPieceFragments = 45,
        SlotPendingLifecycleResolve = 46,
        SlotHealing = 47,
        CoreComboExpire = 48,
        SessionFunctionKeyObjectEffect = 49,
        SessionFunctionKeyFullMpEffect = 50,
        SessionScoreboardTick = 51,
        SessionStoryResultAudioStop = 52,
        SessionKnockoutFeedAndPrune = 53,
        SessionEarthquake = 54,
        SessionCamera = 55,
        PresentationSnapshotBuild = 56,
    }

    internal readonly struct NTSD28BattlePassDescriptor
    {
        internal NTSD28BattlePassDescriptor(
            NTSD28BattlePassId id,
            NTSD28BattlePassDomain domain,
            NTSD28BattleTraversalKind traversal,
            NTSD28BattlePassFlags flags,
            int nestedSlotTailStep = -1)
        {
            Id = id;
            Domain = domain;
            Traversal = traversal;
            Flags = flags;
            NestedSlotTailStep = nestedSlotTailStep;
        }

        internal NTSD28BattlePassId Id { get; }
        internal NTSD28BattlePassDomain Domain { get; }
        internal NTSD28BattleTraversalKind Traversal { get; }
        internal NTSD28BattlePassFlags Flags { get; }
        internal int NestedSlotTailStep { get; }

        internal bool HasFlag(NTSD28BattlePassFlags flag)
        {
            return (Flags & flag) == flag;
        }
    }

    /// <summary>
    /// Immutable normal-completed-tick order recovered from the NTSD 2.8-Logan
    /// playable GameSession and SimulationTickDriver live path. This is the
    /// expected contract for B3 migration; it does not execute Unity passes.
    /// </summary>
    internal static class NTSD28BattlePassOrder
    {
        private static readonly NTSD28BattlePassDescriptor[] descriptors =
            CreateDescriptors();

        internal static int Count => descriptors.Length;

        internal static NTSD28BattlePassDescriptor GetAt(int index)
        {
            if ((uint)index >= (uint)descriptors.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return descriptors[index];
        }

        internal static bool TryGet(
            int index,
            out NTSD28BattlePassDescriptor descriptor)
        {
            if ((uint)index >= (uint)descriptors.Length)
            {
                descriptor = default;
                return false;
            }

            descriptor = descriptors[index];
            return true;
        }

        internal static int IndexOf(NTSD28BattlePassId id)
        {
            int index = (int)id;
            return (uint)index < (uint)descriptors.Length &&
                   descriptors[index].Id == id
                ? index
                : -1;
        }

        internal static bool IsBefore(
            NTSD28BattlePassId first,
            NTSD28BattlePassId second)
        {
            int firstIndex = IndexOf(first);
            int secondIndex = IndexOf(second);
            return firstIndex >= 0 && secondIndex >= 0 && firstIndex < secondIndex;
        }

        private static NTSD28BattlePassDescriptor[] CreateDescriptors()
        {
            const NTSD28BattlePassFlags session =
                NTSD28BattlePassFlags.SessionBoundary;
            const NTSD28BattlePassFlags barrier =
                NTSD28BattlePassFlags.GlobalBarrier;
            const NTSD28BattlePassFlags nested =
                NTSD28BattlePassFlags.NestedSlotTail;

            return new[]
            {
                Pass(NTSD28BattlePassId.SessionFunctionKeyDispatch,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.SessionProjectGlobalsBeforeFlow,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, session),
                Pass(NTSD28BattlePassId.SessionBattleFlowClassify,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.SessionStoryPhaseAdvance,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.Scalar,
                    session | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.SessionProjectGlobalsBeforeCore,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, session),
                Pass(NTSD28BattlePassId.SessionApplyPendingInputs,
                    NTSD28BattlePassDomain.SessionPreCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, session),
                Pass(NTSD28BattlePassId.CoreInputPhaseAdvance,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.Scalar, barrier),
                Pass(NTSD28BattlePassId.CoreSparkAdvance,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.Scalar, barrier),
                Pass(NTSD28BattlePassId.CoreProducerSampleScan,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlotsReacquireAfterMutation,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.CoreProxyAndInputRouteScan,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreFrameMotion,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreTeleport,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                PhysicsSlotPass(NTSD28BattlePassId.SlotPhysics, 0),
                PhysicsSlotPass(
                    NTSD28BattlePassId.SlotDeadCharacterResourceNormalize,
                    1),
                Pass(NTSD28BattlePassId.CoreRevival,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.CoreStageDepthClampBeforeGeometry,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreHeldRefillBeforeGeometry,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreCollisionActionSnapshot,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots,
                    barrier | NTSD28BattlePassFlags.ImmutableSnapshot),
                Pass(NTSD28BattlePassId.CoreCandidateBuild,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.PairGeometry, barrier),
                Pass(NTSD28BattlePassId.CoreFusion,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.PairGeometry,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.CoreActiveWeaponCount,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreTypeZeroHitConsume,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingAttackersThenCandidates,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.CoreRandomWeaponDrop,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.UserAcceptedException,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet |
                    NTSD28BattlePassFlags.UserAcceptedException),
                Pass(NTSD28BattlePassId.CoreNonTypeZeroHitConsume,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingAttackersThenCandidates,
                    barrier | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.CoreCatchRelationAdvance,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreCatchSettlement,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreStageDepthClampAfterHits,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreHeldRefillAfterHits,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreStageSettlement,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreHorizontalImpulseFinalize,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, barrier),
                Pass(NTSD28BattlePassId.CoreBeginNativeResourceTick,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.Scalar, barrier),
                Pass(NTSD28BattlePassId.CoreBeginFrameTick,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.Scalar, barrier),
                SlotPass(NTSD28BattlePassId.SlotDefinitionTransition, 0, nested),
                SlotPass(
                    NTSD28BattlePassId.SlotSpecialStateCloneMaterialize,
                    1,
                    nested | NTSD28BattlePassFlags.MayMutateSlotSet),
                SlotPass(
                    NTSD28BattlePassId.SlotNativeResourcePreDisplay,
                    2,
                    nested),
                SlotPass(NTSD28BattlePassId.SlotDisplayValues, 3, nested),
                SlotPass(
                    NTSD28BattlePassId.SlotNativeResourcePostDisplay,
                    4,
                    nested),
                SlotPass(NTSD28BattlePassId.SlotComputerStateRefresh, 5, nested),
                SlotPass(NTSD28BattlePassId.SlotFrameStep, 6, nested),
                SlotPass(NTSD28BattlePassId.SlotReactionTimers, 7, nested),
                SlotPass(NTSD28BattlePassId.SlotArmorRecovery, 8, nested),
                SlotPass(
                    NTSD28BattlePassId.SlotAttackerRestDecrement,
                    9,
                    nested),
                SlotPass(
                    NTSD28BattlePassId.SlotFrameZeroOpoint,
                    10,
                    nested | NTSD28BattlePassFlags.MayMutateSlotSet),
                SlotPass(
                    NTSD28BattlePassId.SlotState18BrokenWeaponParticles,
                    11,
                    nested | NTSD28BattlePassFlags.MayMutateSlotSet),
                SlotPass(NTSD28BattlePassId.SlotPreviousActionCommit, 12, nested),
                SlotPass(
                    NTSD28BattlePassId.SlotWeaponPieceFragments,
                    13,
                    nested | NTSD28BattlePassFlags.MayMutateSlotSet),
                SlotPass(
                    NTSD28BattlePassId.SlotPendingLifecycleResolve,
                    14,
                    nested | NTSD28BattlePassFlags.MayMutateSlotSet),
                SlotPass(NTSD28BattlePassId.SlotHealing, 15, nested),
                Pass(NTSD28BattlePassId.CoreComboExpire,
                    NTSD28BattlePassDomain.Core,
                    NTSD28BattleTraversalKind.Scalar, barrier),
                Pass(NTSD28BattlePassId.SessionFunctionKeyObjectEffect,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots,
                    session | NTSD28BattlePassFlags.MayMutateSlotSet),
                Pass(NTSD28BattlePassId.SessionFunctionKeyFullMpEffect,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, session),
                Pass(NTSD28BattlePassId.SessionScoreboardTick,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.SessionStoryResultAudioStop,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.SessionKnockoutFeedAndPrune,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.AscendingLiveSlots, session),
                Pass(NTSD28BattlePassId.SessionEarthquake,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.SessionCamera,
                    NTSD28BattlePassDomain.SessionPostCore,
                    NTSD28BattleTraversalKind.Scalar, session),
                Pass(NTSD28BattlePassId.PresentationSnapshotBuild,
                    NTSD28BattlePassDomain.PresentationHandoff,
                    NTSD28BattleTraversalKind.CompletedTickSnapshot,
                    session | NTSD28BattlePassFlags.ImmutableSnapshot |
                    NTSD28BattlePassFlags.CompletedTickHandoff),
            };
        }

        private static NTSD28BattlePassDescriptor Pass(
            NTSD28BattlePassId id,
            NTSD28BattlePassDomain domain,
            NTSD28BattleTraversalKind traversal,
            NTSD28BattlePassFlags flags)
        {
            return new NTSD28BattlePassDescriptor(id, domain, traversal, flags);
        }

        private static NTSD28BattlePassDescriptor SlotPass(
            NTSD28BattlePassId id,
            int nestedSlotTailStep,
            NTSD28BattlePassFlags flags)
        {
            return new NTSD28BattlePassDescriptor(
                id,
                NTSD28BattlePassDomain.SlotTail,
                NTSD28BattleTraversalKind.NestedAscendingLiveSlotStep,
                flags,
                nestedSlotTailStep);
        }

        private static NTSD28BattlePassDescriptor PhysicsSlotPass(
            NTSD28BattlePassId id,
            int nestedPhysicsStep)
        {
            return new NTSD28BattlePassDescriptor(
                id,
                NTSD28BattlePassDomain.PhysicsSlot,
                NTSD28BattleTraversalKind.NestedAscendingPhysicsSlotStep,
                NTSD28BattlePassFlags.GlobalBarrier |
                NTSD28BattlePassFlags.NestedPhysicsSlot,
                nestedPhysicsStep);
        }
    }
}
