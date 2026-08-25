using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Lockstep
{
    /// <summary>
    /// Immutable session-wide inputs that every in-process S0 world must accept before
    /// the first authority frame is consumed.
    /// </summary>
    public sealed class LockstepStartBarrier
    {
        public const int MaximumBattleRosterSlots = 8;

        private readonly int[] canonicalPlayerSlots;
        private readonly IReadOnlyList<int> canonicalPlayerSlotView;

        public LockstepStartBarrier(
            LockstepSessionIdentity identity,
            ulong ruleFingerprint,
            int policyVersion,
            BattleRuntimeWorldSettings worldSettings)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (ruleFingerprint == 0)
                throw new ArgumentOutOfRangeException(nameof(ruleFingerprint));
            if (identity.CatalogFingerprint == 0)
                throw new ArgumentException(
                    "The start barrier requires a non-zero catalog fingerprint.",
                    nameof(identity));
            if (identity.StageFingerprint == 0)
                throw new ArgumentException(
                    "The start barrier requires a non-zero stage fingerprint.",
                    nameof(identity));
            if (policyVersion <= 0)
                throw new ArgumentOutOfRangeException(nameof(policyVersion));
            if (worldSettings.InitialRuntimeSlotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(worldSettings),
                    "The start barrier requires a positive runtime slot capacity.");
            }

            canonicalPlayerSlots = new int[identity.PlayerCount];
            for (int index = 0; index < canonicalPlayerSlots.Length; index++)
            {
                int playerSlot = identity.CanonicalPlayerSlots[index];
                if ((uint)playerSlot >= MaximumBattleRosterSlots)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(identity),
                        $"Battle player slot {playerSlot} is outside the 0-7 roster range.");
                }

                canonicalPlayerSlots[index] = playerSlot;
            }

            canonicalPlayerSlotView = Array.AsReadOnly(canonicalPlayerSlots);
            RuleFingerprint = ruleFingerprint;
            PolicyVersion = policyVersion;
            WorldSettings = worldSettings;
            BarrierFingerprint = ComputeBarrierFingerprint();
        }

        public LockstepSessionIdentity Identity { get; }
        public ulong RuleFingerprint { get; }
        public int PolicyVersion { get; }
        public BattleRuntimeWorldSettings WorldSettings { get; }
        public ulong BarrierFingerprint { get; }
        public IReadOnlyList<int> CanonicalPlayerSlots => canonicalPlayerSlotView;
        public int PlayerCount => canonicalPlayerSlots.Length;

        public bool Matches(LockstepStartBarrier other)
        {
            return other != null &&
                   BarrierFingerprint == other.BarrierFingerprint;
        }

        internal bool IsCanonicalFrame(FrameInputSet frame)
        {
            return frame != null &&
                   frame.IsCanonicalFor(frame.TickIndex, canonicalPlayerSlotView);
        }

        private ulong ComputeBarrierFingerprint()
        {
            ulong hash = CanonicalHash.Offset;
            CanonicalHash.AddUlong(ref hash, Identity.IdentityFingerprint);
            CanonicalHash.AddUlong(ref hash, RuleFingerprint);
            CanonicalHash.AddInt(ref hash, PolicyVersion);
            CanonicalHash.AddInt(ref hash, (int)WorldSettings.Profile);
            CanonicalHash.AddInt(ref hash, WorldSettings.InitialRuntimeSlotCapacity);
            CanonicalHash.AddInt(ref hash, WorldSettings.MaxActiveRuntimeEntities);
            CanonicalHash.AddInt(ref hash, (int)WorldSettings.CollisionBroadphase);
            return hash;
        }
    }
}
