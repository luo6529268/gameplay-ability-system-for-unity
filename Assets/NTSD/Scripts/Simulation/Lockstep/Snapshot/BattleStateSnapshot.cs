using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    /// <summary>
    /// Preallocated aggregate of every U7 battle-state capture domain. A capture
    /// is visible only after all component buffers agree on tick, protocol schema,
    /// and session identity. Restore is intentionally added only after its rebuild
    /// contract is complete.
    /// </summary>
    public sealed class BattleStateSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        internal BattleStateSnapshotBuffer(
            BattleWorldRosterResultsSnapshotBuffer rosterResults,
            BattleWorldStageSpawnSnapshotBuffer stageSpawn,
            BattleWorldRuntimeSlotSnapshotBuffer runtimeSlots,
            BattleWorldEntityRuntimeSnapshotBuffer entityRuntime,
            BattleWorldEntityBaseShellSnapshotBuffer entityBaseShell,
            BattleWorldLivingShellSnapshotBuffer livingShell,
            BattleWorldCharacterShellSnapshotBuffer characterShell,
            BattleWorldWeaponShellSnapshotBuffer weaponShell,
            BattleWorldSpecialOtherShellSnapshotBuffer specialOtherShell,
            BattleWorldPendingEventSnapshotBuffer pendingEvents,
            BattleWorldRestSnapshotBuffer rest)
        {
            RosterResults = rosterResults ??
                throw new ArgumentNullException(nameof(rosterResults));
            StageSpawn = stageSpawn ??
                throw new ArgumentNullException(nameof(stageSpawn));
            RuntimeSlots = runtimeSlots ??
                throw new ArgumentNullException(nameof(runtimeSlots));
            EntityRuntime = entityRuntime ??
                throw new ArgumentNullException(nameof(entityRuntime));
            EntityBaseShell = entityBaseShell ??
                throw new ArgumentNullException(nameof(entityBaseShell));
            LivingShell = livingShell ??
                throw new ArgumentNullException(nameof(livingShell));
            CharacterShell = characterShell ??
                throw new ArgumentNullException(nameof(characterShell));
            WeaponShell = weaponShell ??
                throw new ArgumentNullException(nameof(weaponShell));
            SpecialOtherShell = specialOtherShell ??
                throw new ArgumentNullException(nameof(specialOtherShell));
            PendingEvents = pendingEvents ??
                throw new ArgumentNullException(nameof(pendingEvents));
            Rest = rest ?? throw new ArgumentNullException(nameof(rest));
        }

        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }
        public bool IsValid => SchemaVersion == CurrentSchemaVersion;

        public BattleWorldCoreScalarSnapshot Core { get; private set; }
        public BattleWorldRosterResultsSnapshotBuffer RosterResults { get; }
        public BattleWorldStageSpawnSnapshotBuffer StageSpawn { get; }
        public BattleWorldRuntimeSlotSnapshotBuffer RuntimeSlots { get; }
        public BattleWorldEntityRuntimeSnapshotBuffer EntityRuntime { get; }
        public BattleWorldEntityBaseShellSnapshotBuffer EntityBaseShell { get; }
        public BattleWorldLivingShellSnapshotBuffer LivingShell { get; }
        public BattleWorldCharacterShellSnapshotBuffer CharacterShell { get; }
        public BattleWorldWeaponShellSnapshotBuffer WeaponShell { get; }
        public BattleWorldSpecialOtherShellSnapshotBuffer SpecialOtherShell { get; }
        public BattleWorldPendingEventSnapshotBuffer PendingEvents { get; }
        public BattleWorldRestSnapshotBuffer Rest { get; }

        internal void Invalidate()
        {
            SchemaVersion = 0;
            ProtocolSchemaVersion = 0;
            IdentityFingerprint = 0UL;
            CapturedTick = 0;
            Core = default;
        }

        internal bool TryPublish(
            in BattleWorldCoreScalarSnapshot core,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            int protocolSchema = identity.SchemaVersion;
            ulong fingerprint = identity.IdentityFingerprint;
            if (core.SchemaVersion != BattleWorldCoreScalarSnapshot.CurrentSchemaVersion ||
                core.ProtocolSchemaVersion != protocolSchema ||
                core.IdentityFingerprint != fingerprint ||
                core.Flow.CurrentTickIndex != tick ||
                !Matches(RosterResults.SchemaVersion,
                    BattleWorldRosterResultsSnapshotBuffer.CurrentSchemaVersion,
                    RosterResults.ProtocolSchemaVersion,
                    RosterResults.IdentityFingerprint,
                    RosterResults.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(StageSpawn.SchemaVersion,
                    BattleWorldStageSpawnSnapshotBuffer.CurrentSchemaVersion,
                    StageSpawn.ProtocolSchemaVersion,
                    StageSpawn.IdentityFingerprint,
                    StageSpawn.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(RuntimeSlots.SchemaVersion,
                    BattleWorldRuntimeSlotSnapshotBuffer.CurrentSchemaVersion,
                    RuntimeSlots.ProtocolSchemaVersion,
                    RuntimeSlots.IdentityFingerprint,
                    RuntimeSlots.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(EntityRuntime.SchemaVersion,
                    BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                    EntityRuntime.ProtocolSchemaVersion,
                    EntityRuntime.IdentityFingerprint,
                    EntityRuntime.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(EntityBaseShell.SchemaVersion,
                    BattleWorldEntityBaseShellSnapshotBuffer.CurrentSchemaVersion,
                    EntityBaseShell.ProtocolSchemaVersion,
                    EntityBaseShell.IdentityFingerprint,
                    EntityBaseShell.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(LivingShell.SchemaVersion,
                    BattleWorldLivingShellSnapshotBuffer.CurrentSchemaVersion,
                    LivingShell.ProtocolSchemaVersion,
                    LivingShell.IdentityFingerprint,
                    LivingShell.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(CharacterShell.SchemaVersion,
                    BattleWorldCharacterShellSnapshotBuffer.CurrentSchemaVersion,
                    CharacterShell.ProtocolSchemaVersion,
                    CharacterShell.IdentityFingerprint,
                    CharacterShell.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(WeaponShell.SchemaVersion,
                    BattleWorldWeaponShellSnapshotBuffer.CurrentSchemaVersion,
                    WeaponShell.ProtocolSchemaVersion,
                    WeaponShell.IdentityFingerprint,
                    WeaponShell.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(SpecialOtherShell.SchemaVersion,
                    BattleWorldSpecialOtherShellSnapshotBuffer.CurrentSchemaVersion,
                    SpecialOtherShell.ProtocolSchemaVersion,
                    SpecialOtherShell.IdentityFingerprint,
                    SpecialOtherShell.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(PendingEvents.SchemaVersion,
                    BattleWorldPendingEventSnapshotBuffer.CurrentSchemaVersion,
                    PendingEvents.ProtocolSchemaVersion,
                    PendingEvents.IdentityFingerprint,
                    PendingEvents.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick) ||
                !Matches(Rest.SchemaVersion,
                    BattleWorldRestSnapshotBuffer.CurrentSchemaVersion,
                    Rest.ProtocolSchemaVersion,
                    Rest.IdentityFingerprint,
                    Rest.CapturedTick,
                    protocolSchema,
                    fingerprint,
                    tick))
            {
                Invalidate();
                return false;
            }

            Core = core;
            ProtocolSchemaVersion = protocolSchema;
            IdentityFingerprint = fingerprint;
            CapturedTick = tick;
            SchemaVersion = CurrentSchemaVersion;
            return true;
        }

        private static bool Matches(
            int actualSchema,
            int expectedSchema,
            int actualProtocolSchema,
            ulong actualFingerprint,
            int actualTick,
            int expectedProtocolSchema,
            ulong expectedFingerprint,
            int expectedTick)
        {
            return actualSchema == expectedSchema &&
                   actualProtocolSchema == expectedProtocolSchema &&
                   actualFingerprint == expectedFingerprint &&
                   actualTick == expectedTick;
        }
    }
}
