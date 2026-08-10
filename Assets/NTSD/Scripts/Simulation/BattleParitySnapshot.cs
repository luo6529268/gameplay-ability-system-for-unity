using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public sealed class BattleParityStructuralEvent
    {
        public int Tick;
        public string Pass;
        public string Action;
        public int CursorSlot = -1;
        public int ActorSlot = -1;
        public int Slot = -1;
        public int SearchStart = -1;
        public int SearchEndExclusive = -1;
        public string Before;
        public string After;
        public int LifecycleEpoch;
        public string SourceKind;
        public int BeforeLinkState;
        public int BeforeTargetSlot = -1;
        public int BeforeHeldWeaponSlot = -1;
        public int AfterLinkState;
        public int AfterTargetSlot = -1;
        public int AfterHeldWeaponSlot = -1;
        public bool TargetActive;
        public int ObservedHolderSlot = -1;
        public string Outcome;
        public string Reason;
        public int TargetBeforeHolderSlot = -1;
        public int TargetBeforeLinkState;
        public int TargetAfterHolderSlot = -1;
        public int TargetAfterLinkState;

        internal object ToCanonicalObject()
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["action"] = Action ?? string.Empty,
                ["actorSlot"] = ActorSlot,
                ["after"] = After ?? string.Empty,
                ["before"] = Before ?? string.Empty,
                ["cursorSlot"] = CursorSlot,
                ["lifecycleEpoch"] = LifecycleEpoch,
                ["pass"] = Pass ?? string.Empty,
                ["searchEndExclusive"] = SearchEndExclusive,
                ["searchStart"] = SearchStart,
                ["slot"] = Slot,
                ["sourceKind"] = SourceKind ?? string.Empty,
                ["tick"] = Tick,
            };
            if (string.Equals(Action, "link-validation", StringComparison.Ordinal))
            {
                result["afterHeldWeaponSlot"] = AfterHeldWeaponSlot;
                result["afterLinkState"] = AfterLinkState;
                result["afterTargetSlot"] = AfterTargetSlot;
                result["beforeHeldWeaponSlot"] = BeforeHeldWeaponSlot;
                result["beforeLinkState"] = BeforeLinkState;
                result["beforeTargetSlot"] = BeforeTargetSlot;
                result["observedHolderSlot"] = ObservedHolderSlot;
                result["outcome"] = Outcome ?? string.Empty;
                result["reason"] = Reason ?? string.Empty;
                result["targetActive"] = TargetActive;
                result["targetAfterHolderSlot"] = TargetAfterHolderSlot;
                result["targetAfterLinkState"] = TargetAfterLinkState;
                result["targetBeforeHolderSlot"] = TargetBeforeHolderSlot;
                result["targetBeforeLinkState"] = TargetBeforeLinkState;
            }
            return result;
        }
    }

    public interface IBattleParityStructuralEventSink
    {
        void Record(BattleParityStructuralEvent structuralEvent);
    }

    /// <summary>
    /// Diagnostic-only structural event collector. Lifecycle epochs are derived from
    /// observed allocations per slot; RuntimeSlotTable generations never cross the
    /// Authority/Unity trace boundary.
    /// </summary>
    public sealed class BattleParityStructuralEventBuffer : IBattleParityStructuralEventSink
    {
        private readonly int[] lifecycleEpochs;
        private readonly List<BattleParityStructuralEvent> events =
            new List<BattleParityStructuralEvent>();

        public BattleParityStructuralEventBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            lifecycleEpochs = new int[slotCapacity];
        }

        public IReadOnlyList<BattleParityStructuralEvent> Events => events;

        public void Record(BattleParityStructuralEvent structuralEvent)
        {
            if (structuralEvent == null)
                return;

            int slot = structuralEvent.Slot;
            if (slot >= lifecycleEpochs.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(structuralEvent),
                    $"Structural event slot {slot} exceeds Authority400 capacity.");
            }

            if (slot >= 0 &&
                string.Equals(structuralEvent.Action, "allocate", StringComparison.Ordinal))
            {
                lifecycleEpochs[slot]++;
            }

            structuralEvent.LifecycleEpoch = slot >= 0 ? lifecycleEpochs[slot] : 0;
            events.Add(structuralEvent);
        }

        public IReadOnlyList<BattleParityStructuralEvent> CaptureTick(int tick)
        {
            var result = new List<BattleParityStructuralEvent>();
            for (int index = 0; index < events.Count; index++)
            {
                BattleParityStructuralEvent structuralEvent = events[index];
                if (structuralEvent.Tick == tick)
                    result.Add(structuralEvent);
            }
            return result;
        }
    }

    public sealed class BattleParityHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Overall;
        public string Rng;
        public string Slots;
        public string Stats;
        public string VRest;
        public string World;

        internal SortedDictionary<string, object> ToCanonicalObject(bool includeOverall)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARest,
                ["events"] = Events,
                ["input"] = Input,
                ["rng"] = Rng,
                ["slots"] = Slots,
                ["stats"] = Stats,
                ["vRest"] = VRest,
                ["world"] = World,
            };
            if (includeOverall)
                result["overall"] = Overall;
            return result;
        }
    }

    public interface IBattleChecksumSnapshot
    {
        string Schema { get; }
        int Tick { get; }
        int ObjectCount { get; }
        string OverallChecksum { get; }
        string ToJson();
    }

    public sealed class BattleParityFrameSnapshot : IBattleChecksumSnapshot
    {
        public const string SchemaId = "ntsd-battle-trace-v3";
        internal object InputDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object[] AllSlotsDomain;
        internal object[] CompactSlotsDomain;
        internal string[] SlotCommitments;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object FullARestDomain;
        internal object FullVRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public int Tick { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleParityHashes Hashes { get; internal set; }
        public string Schema => SchemaId;
        public string OverallChecksum => Hashes?.Overall ?? string.Empty;

        public string ToJson()
        {
            return ToJson(full: false);
        }

        public string ToJson(bool full)
        {
            var tick = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = full && FullARestDomain != null ? FullARestDomain : ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "tick",
                ["objectCount"] = ObjectCount,
                ["rng"] = RngDomain,
                ["slots"] = full ? AllSlotsDomain : CompactSlotsDomain,
                ["slotCommitments"] = SlotCommitments,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = full && FullVRestDomain != null ? FullVRestDomain : VRestDomain,
                ["world"] = WorldDomain,
            };
            return BattleCanonicalJson.Serialize(tick);
        }
    }

    public sealed class BattleExtendedChecksumHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Metadata;
        public string Overall;
        public string Rng;
        public string Slots;
        public string Stats;
        public string VRest;
        public string World;

        internal SortedDictionary<string, object> ToCanonicalObject(bool includeOverall)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARest,
                ["events"] = Events,
                ["input"] = Input,
                ["metadata"] = Metadata,
                ["rng"] = Rng,
                ["slots"] = Slots,
                ["stats"] = Stats,
                ["vRest"] = VRest,
                ["world"] = World,
            };
            if (includeOverall)
                result["overall"] = Overall;
            return result;
        }
    }

    /// <summary>
    /// Capacity-aware checksum for Extended runtime profiles. This is deliberately
    /// independent from the frozen Authority400 v3 parity/trace representation.
    /// </summary>
    public sealed class BattleExtendedChecksumSnapshot : IBattleChecksumSnapshot
    {
        public const string SchemaId = "ntsd-unity-extended-battle-checksum-v1";

        internal object InputDomain;
        internal object MetadataDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object SlotsDomain;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public string Schema => SchemaId;
        public string Profile { get; internal set; }
        public int Tick { get; internal set; }
        public int LogicalCapacity { get; internal set; }
        public int ClaimedCount { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleExtendedChecksumHashes Hashes { get; internal set; }
        public string OverallChecksum => Hashes?.Overall ?? string.Empty;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "extended-checksum",
                ["metadata"] = MetadataDomain,
                ["rng"] = RngDomain,
                ["schema"] = Schema,
                ["slots"] = SlotsDomain,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = VRestDomain,
                ["world"] = WorldDomain,
            });
        }
    }

    public sealed class BattleLockstepChecksumHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Metadata;
        public string Overall;
        public string Rng;
        public string Slots;
        public string Stats;
        public string VRest;
        public string World;

        internal SortedDictionary<string, object> ToCanonicalObject(bool includeOverall)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARest,
                ["events"] = Events,
                ["input"] = Input,
                ["metadata"] = Metadata,
                ["rng"] = Rng,
                ["slots"] = Slots,
                ["stats"] = Stats,
                ["vRest"] = VRest,
                ["world"] = World,
            };
            if (includeOverall)
                result["overall"] = Overall;
            return result;
        }
    }

    /// <summary>
    /// Deterministic lockstep projection. Presentation-owned hit-record samples are
    /// deliberately excluded because their retirement follows Unity presentation
    /// finalization rather than the fixed simulation tick.
    /// </summary>
    public sealed class BattleLockstepChecksumSnapshot : IBattleChecksumSnapshot
    {
        public const string SchemaId = "ntsd-lockstep-core-checksum-v1";

        internal object InputDomain;
        internal object MetadataDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object SlotsDomain;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public string Schema => SchemaId;
        public string Profile { get; internal set; }
        public int Tick { get; internal set; }
        public int LogicalCapacity { get; internal set; }
        public int ClaimedCount { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleLockstepChecksumHashes Hashes { get; internal set; }
        public string OverallChecksum => Hashes?.Overall ?? string.Empty;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "lockstep-core-checksum",
                ["metadata"] = MetadataDomain,
                ["rng"] = RngDomain,
                ["schema"] = Schema,
                ["slots"] = SlotsDomain,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = VRestDomain,
                ["world"] = WorldDomain,
            });
        }
    }

    public static class BattleCanonicalJson
    {
        public static string Serialize(object value)
        {
            var builder = new StringBuilder(4096);
            WriteValue(builder, value);
            return builder.ToString();
        }

        public static string Sha256(object value)
        {
            byte[] payload = Encoding.UTF8.GetBytes(Serialize(value));
            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(payload);
            var builder = new StringBuilder(digest.Length * 2);
            for (int i = 0; i < digest.Length; i++)
                builder.Append(digest[i].ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static void WriteValue(StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            switch (value)
            {
                case string text:
                    WriteString(builder, text);
                    return;
                case char character:
                    WriteString(builder, character.ToString());
                    return;
                case bool boolean:
                    builder.Append(boolean ? "true" : "false");
                    return;
                case byte byteValue:
                    builder.Append(byteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case sbyte signedByteValue:
                    builder.Append(signedByteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case short shortValue:
                    builder.Append(shortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ushort unsignedShortValue:
                    builder.Append(unsignedShortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case int intValue:
                    builder.Append(intValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case uint unsignedIntValue:
                    builder.Append(unsignedIntValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case long longValue:
                    builder.Append(longValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ulong unsignedLongValue:
                    builder.Append(unsignedLongValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case float floatValue:
                    WriteFloatingPoint(builder, floatValue);
                    return;
                case double doubleValue:
                    WriteFloatingPoint(builder, doubleValue);
                    return;
                case decimal decimalValue:
                    builder.Append(decimalValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case IDictionary dictionary:
                    WriteDictionary(builder, dictionary);
                    return;
                case IEnumerable enumerable:
                    WriteArray(builder, enumerable);
                    return;
            }

            if (value.GetType().IsEnum)
            {
                builder.Append(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported canonical JSON value type: {value.GetType().FullName}");
        }

        private static void WriteDictionary(StringBuilder builder, IDictionary dictionary)
        {
            var keys = new List<string>(dictionary.Count);
            foreach (object key in dictionary.Keys)
                keys.Add(Convert.ToString(key, CultureInfo.InvariantCulture));
            keys.Sort(StringComparer.Ordinal);

            builder.Append('{');
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0)
                    builder.Append(',');
                string key = keys[i];
                WriteString(builder, key);
                builder.Append(':');
                WriteValue(builder, dictionary[key]);
            }
            builder.Append('}');
        }

        private static void WriteArray(StringBuilder builder, IEnumerable values)
        {
            builder.Append('[');
            bool first = true;
            foreach (object value in values)
            {
                if (!first)
                    builder.Append(',');
                first = false;
                WriteValue(builder, value);
            }
            builder.Append(']');
        }

        private static void WriteFloatingPoint(StringBuilder builder, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidOperationException("Canonical battle snapshots cannot contain NaN or Infinity.");
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void WriteString(StringBuilder builder, string value)
        {
            builder.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c > 0x7E)
                            builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
        }
    }

    internal sealed class BattleParitySnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleParitySnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        private const int AuthorityRuntimeSlotCapacity =
            SimulationWorld.AuthorityRuntimeSlotCapacity;

        private BattleRuntimeProfile RuntimeProfileForServices =>
            world.RuntimeProfileForServices;
        private int RuntimeSlotCapacity => world.RuntimeSlotCapacityForDiagnostics;
        private DeterministicRng Rng => world.Rng;
        private int[] DamageStats => world.DamageStats;
        private int[] KillStats => world.KillStats;
        private int ObjectCount => world.ObjectCount;
        private RuntimeSlotTable _runtimeSlots => world.RuntimeSlotTableForModules;
        private RuntimeRestStore _runtimeRestStore => world.RuntimeRestStoreForServices;
        private BattleRuntimeState Runtime => world.Runtime;
        private int _cameraVel => world.ReleaseCameraVelocityForServices;
        private int _cameraX => world.ReleaseCameraX;
        private bool PpMode => world.PpMode;
        private List<PendingSoundEvent> PendingSounds => world.PendingSounds;

        private bool IsActiveForCurrentPass(ISimObject value)
        {
            return world.IsActiveForCurrentPassInternal(value);
        }

        private LF2Entity FindEntityByRuntimeSlotIncludingDormant(int runtimeSlot)
        {
            return world.FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
        }

        private NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return world.GetRawRuntimeSlotState(runtimeSlot);
        }

        private int GetRawRestArest(int attackerSlot)
        {
            return _runtimeRestStore.GetARest(attackerSlot);
        }

        private int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            return _runtimeRestStore.GetVRest(victimSlot, attackerSlot);
        }

        internal BattleParityFrameSnapshot CaptureParityFrameSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            bool includeFullDomains = false,
            IReadOnlyList<BattleParityStructuralEvent> structuralEvents = null)
        {
            if (RuntimeProfileForServices != BattleRuntimeProfile.Authority400 ||
                RuntimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new InvalidOperationException(
                    $"Parity snapshots require an Authority400 world ({AuthorityRuntimeSlotCapacity} slots); " +
                    $"actual profile is {RuntimeProfileForServices} with capacity {RuntimeSlotCapacity}.");
            }

            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object[] allSlots = ProjectAllRuntimeSlots(includePresentationHitRecords: true);
            var slotCommitments = new string[allSlots.Length];
            for (int slot = 0; slot < allSlots.Length; slot++)
                slotCommitments[slot] = BattleCanonicalJson.Sha256(allSlots[slot]);
            object slotCommitmentDomain = DictionaryOf(
                ("commitments", (object)slotCommitments),
                ("count", allSlots.Length));
            object aRestDomain = ProjectARestDomain();
            object vRestDomain = ProjectVRestDomain();
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain(structuralEvents);

            var hashes = new BattleParityHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(slotCommitmentDomain),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(hashes.ToCanonicalObject(includeOverall: false));

            var compactSlots = new List<object>();
            for (int slot = 0; slot < allSlots.Length; slot++)
            {
                object baseline = ProjectDefaultRuntimeSlot(
                    slot,
                    includePresentationHitRecords: true);
                if (!string.Equals(
                        BattleCanonicalJson.Sha256(allSlots[slot]),
                        BattleCanonicalJson.Sha256(baseline),
                        StringComparison.Ordinal))
                {
                    compactSlots.Add(allSlots[slot]);
                }
            }

            return new BattleParityFrameSnapshot
            {
                Tick = tickIndex,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                AllSlotsDomain = allSlots,
                CompactSlotsDomain = compactSlots.ToArray(),
                SlotCommitments = slotCommitments,
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                FullARestDomain = includeFullDomains ? ProjectFullARestDomain() : null,
                FullVRestDomain = includeFullDomains ? ProjectFullVRestDomain() : null,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        /// <summary>
        /// Captures the capacity-aware checksum used by MobileExtended and
        /// DesktopExtended worlds. It must not be used as a v3 parity trace.
        /// </summary>
        internal BattleExtendedChecksumSnapshot CaptureExtendedChecksumSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null)
        {
            if (RuntimeProfileForServices != BattleRuntimeProfile.MobileExtended &&
                RuntimeProfileForServices != BattleRuntimeProfile.DesktopExtended)
            {
                throw new InvalidOperationException(
                    "Extended checksums require a MobileExtended or DesktopExtended world.");
            }

            int logicalCapacity = RuntimeSlotCapacity;
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object metadataDomain = DictionaryOf(
                ("claimedCount", (object)_runtimeSlots.ClaimedCount),
                ("logicalCapacity", logicalCapacity),
                ("objectCount", ObjectCount),
                ("profile", RuntimeProfileForServices.ToString()),
                ("schema", BattleExtendedChecksumSnapshot.SchemaId),
                ("tick", tickIndex));
            object slotsDomain = ProjectExtendedRuntimeSlots(
                logicalCapacity,
                includePresentationHitRecords: true);
            RuntimeRestStore.DiagnosticSnapshot restSnapshot =
                _runtimeRestStore.CaptureSparseSnapshot();
            object aRestDomain = ProjectExtendedARestDomain(restSnapshot);
            object vRestDomain = ProjectExtendedVRestDomain(restSnapshot);
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain();

            var hashes = new BattleExtendedChecksumHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Metadata = BattleCanonicalJson.Sha256(metadataDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(slotsDomain),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(
                hashes.ToCanonicalObject(includeOverall: false));

            return new BattleExtendedChecksumSnapshot
            {
                Profile = RuntimeProfileForServices.ToString(),
                Tick = tickIndex,
                LogicalCapacity = logicalCapacity,
                ClaimedCount = _runtimeSlots.ClaimedCount,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                MetadataDomain = metadataDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                SlotsDomain = slotsDomain,
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        /// <summary>
        /// Captures the versioned deterministic core used for lockstep comparison.
        /// Unlike the diagnostic parity snapshots, this projection excludes only the
        /// four presentation-finalized hit-record fields.
        /// </summary>
        internal BattleLockstepChecksumSnapshot CaptureLockstepChecksumSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            IReadOnlyList<BattleParityStructuralEvent> structuralEvents = null)
        {
            int logicalCapacity = RuntimeSlotCapacity;
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object metadataDomain = DictionaryOf(
                ("claimedCount", (object)_runtimeSlots.ClaimedCount),
                ("logicalCapacity", logicalCapacity),
                ("objectCount", ObjectCount),
                ("profile", RuntimeProfileForServices.ToString()),
                ("schema", BattleLockstepChecksumSnapshot.SchemaId),
                ("tick", tickIndex));
            object slotsDomain = ProjectExtendedRuntimeSlots(
                logicalCapacity,
                includePresentationHitRecords: false);
            RuntimeRestStore.DiagnosticSnapshot restSnapshot =
                _runtimeRestStore.CaptureSparseSnapshot();
            object aRestDomain = ProjectExtendedARestDomain(restSnapshot);
            object vRestDomain = ProjectExtendedVRestDomain(restSnapshot);
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain(structuralEvents);

            var hashes = new BattleLockstepChecksumHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Metadata = BattleCanonicalJson.Sha256(metadataDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(slotsDomain),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(
                hashes.ToCanonicalObject(includeOverall: false));

            return new BattleLockstepChecksumSnapshot
            {
                Profile = RuntimeProfileForServices.ToString(),
                Tick = tickIndex,
                LogicalCapacity = logicalCapacity,
                ClaimedCount = _runtimeSlots.ClaimedCount,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                MetadataDomain = metadataDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                SlotsDomain = slotsDomain,
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        private object ProjectExtendedRuntimeSlots(
            int logicalCapacity,
            bool includePresentationHitRecords)
        {
            var slots = new object[logicalCapacity];
            for (int runtimeSlot = 0; runtimeSlot < logicalCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
                LF2Entity entity = view.Entity;
                if (view.Claimed &&
                    (entity?.ItrRest == null ||
                     !entity.ItrRest.IsBoundTo(_runtimeRestStore, runtimeSlot)))
                {
                    throw new InvalidOperationException(
                        $"Extended checksum requires slot {runtimeSlot} to be bound to the current world's rest store.");
                }
                int? currentDataOid = entity?.FrameCache?.Wrapper != null
                    ? entity.FrameCache.Wrapper.characterId
                    : entity?.ObjectId;
                int stableId = entity?.Runtime?.StableId ?? view.RawRuntime?.StableId ?? 0;
                slots[runtimeSlot] = DictionaryOf(
                    ("claimed", (object)view.Claimed),
                    ("currentDataOid", currentDataOid),
                    ("generation", view.Generation),
                    ("runtime", entity == null
                        ? ProjectEntityRuntime(
                            null,
                            runtimeSlot,
                            false,
                            view.RawRuntime,
                            projectRawState: view.RawRuntime != null,
                            includePresentationHitRecords: includePresentationHitRecords)
                        : ProjectEntityRuntime(
                            entity,
                            runtimeSlot,
                            IsActiveForCurrentPass(entity),
                            includePresentationHitRecords: includePresentationHitRecords)),
                    ("runtimeSlot", runtimeSlot),
                    ("stableId", stableId));
            }

            return DictionaryOf(
                ("encoding", "capacity-ordered-runtime-slots"),
                ("logicalCapacity", logicalCapacity),
                ("slots", slots));
        }

        private static object ProjectExtendedARestDomain(RuntimeRestStore.DiagnosticSnapshot snapshot)
        {
            var entries = new object[snapshot.ARestEntries.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                RuntimeRestStore.ARestEntry entry = snapshot.ARestEntries[i];
                entries[i] = DictionaryOf(
                    ("slot", (object)entry.AttackerSlot),
                    ("value", entry.Value));
            }

            return DictionaryOf(
                ("encoding", "sparse-nonzero"),
                ("logicalCapacity", snapshot.LogicalCapacity),
                ("entries", entries));
        }

        private static object ProjectExtendedVRestDomain(RuntimeRestStore.DiagnosticSnapshot snapshot)
        {
            var entries = new object[snapshot.VRestEntries.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                RuntimeRestStore.VRestEntry entry = snapshot.VRestEntries[i];
                entries[i] = DictionaryOf(
                    ("attackerSlot", (object)entry.AttackerSlot),
                    ("value", entry.Value),
                    ("victimSlot", entry.VictimSlot));
            }

            return DictionaryOf(
                ("encoding", "sparse-nonzero"),
                ("logicalCapacity", snapshot.LogicalCapacity),
                ("entries", entries));
        }

        private object ProjectFrameInput(FrameInputSet frameInput)
        {
            var players = new object[frameInput.Players?.Count ?? 0];
            for (int i = 0; i < players.Length; i++)
            {
                SimulationPlayerInput player = frameInput.Players[i];
                players[i] = DictionaryOf(
                    ("buttons", (object)(byte)player.Buttons),
                    ("playerSlot", player.PlayerSlot));
            }
            return DictionaryOf(("players", (object)players), ("tickIndex", frameInput.TickIndex));
        }

        private object[] ProjectAllRuntimeSlots(bool includePresentationHitRecords)
        {
            var result = new object[AuthorityRuntimeSlotCapacity];
            for (int runtimeSlot = 0; runtimeSlot < result.Length; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                result[runtimeSlot] = entity == null
                    ? ProjectDefaultRuntimeSlot(
                        runtimeSlot,
                        GetRawRuntimeSlotState(runtimeSlot),
                        includePresentationHitRecords)
                    : ProjectRuntimeSlot(entity, runtimeSlot, includePresentationHitRecords);
            }
            return result;
        }

        private object ProjectDefaultRuntimeSlot(
            int runtimeSlot,
            NTSDEntityRuntime runtime = null,
            bool includePresentationHitRecords = true)
        {
            return DictionaryOf(
                ("currentDataOid", null),
                ("runtime", ProjectEntityRuntime(
                    null,
                    runtimeSlot,
                    false,
                    runtime,
                    includePresentationHitRecords: includePresentationHitRecords)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectRuntimeSlot(
            LF2Entity entity,
            int runtimeSlot,
            bool includePresentationHitRecords)
        {
            bool active = IsActiveForCurrentPass(entity);
            int? currentDataOid = entity.FrameCache?.Wrapper != null
                ? entity.FrameCache.Wrapper.characterId
                : entity.ObjectId;
            return DictionaryOf(
                ("currentDataOid", (object)currentDataOid),
                ("runtime", ProjectEntityRuntime(
                    entity,
                    runtimeSlot,
                    active,
                    includePresentationHitRecords: includePresentationHitRecords)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectEntityRuntime(
            LF2Entity entity,
            int runtimeSlot,
            bool active,
            NTSDEntityRuntime runtimeOverride = null,
            bool projectRawState = false,
            bool includePresentationHitRecords = true)
        {
            NTSDEntityRuntime runtime = entity?.Runtime ?? runtimeOverride;
            bool isDefault = entity == null && !projectRawState;
            int[] hitRecordDamage = null;
            int[] hitRecordX = null;
            int[] hitRecordZ = null;
            if (includePresentationHitRecords)
            {
                hitRecordDamage = new int[LF2Entity.MaxHitRecordSlots];
                hitRecordX = new int[LF2Entity.MaxHitRecordSlots];
                hitRecordZ = new int[LF2Entity.MaxHitRecordSlots];
                if (entity != null)
                {
                    for (int i = 0; i < hitRecordDamage.Length; i++)
                    {
                        hitRecordDamage[i] = entity.GetHitRecordAge(i);
                        hitRecordX[i] = entity.GetHitRecordX(i);
                        hitRecordZ[i] = entity.GetHitRecordZ(i);
                    }
                }
            }

            int currentDataType = entity?.GetCurrentDataObjectTypeForSimulation() ?? -1;
            int category = ResolveTraceCategory(currentDataType);
            object identity = DictionaryOf(
                ("active", active),
                ("aiControlled", runtime?.AiControlled ?? false),
                ("category", category),
                ("charId", isDefault ? -1 : runtime.ObjectId),
                ("entityType", isDefault ? 0 : runtime.EntityType),
                ("objType", isDefault ? 0 : runtime.ObjType),
                ("ownerId", runtime?.OwnerStableId ?? -1),
                ("slot", runtimeSlot),
                ("team", isDefault ? 0 : runtime.Team),
                ("unk364", runtime?.RelationTeam ?? 0));

            object transform = DictionaryOf(
                ("facing", (object)(isDefault || runtime.Dir == "right" ? 0 : 1)),
                ("renderOffsetX", isDefault ? 0 : (int)runtime.RenderOffsetX),
                ("type3VisualZOffset", isDefault ? 0.0 : runtime.Type3VisualZOffset),
                ("x", isDefault ? 0.0 : runtime.X),
                ("xInt", isDefault ? 0 : runtime.XInt),
                ("y", isDefault ? 0.0 : runtime.Y),
                ("yInt", isDefault ? 0 : runtime.YInt),
                ("z", isDefault ? 0.0 : runtime.Z),
                ("zInt", isDefault ? 0 : runtime.ZInt));

            object motion = DictionaryOf(
                ("fall", isDefault ? 0 : runtime.Fall),
                ("hitCount", isDefault ? 0 : runtime.HitCount),
                ("knockbackVx", isDefault ? 0.1 : runtime.KnockbackVx),
                ("knockbackVy", isDefault ? 0.1 : runtime.KnockbackVy),
                ("knockbackVz", isDefault ? 0.1 : runtime.KnockbackVz),
                ("vx", isDefault ? 0.0 : runtime.Vx),
                ("vy", isDefault ? 0.0 : runtime.Vy),
                ("vz", isDefault ? 0.0 : runtime.Vz));

            object frame = DictionaryOf(
                ("animCounter", isDefault ? 0 : runtime.AnimCounter),
                ("animSub", isDefault ? 0 : runtime.AnimSub),
                ("attacking", isDefault ? 0 : runtime.AttackingCounter),
                ("frame", isDefault ? 0 : runtime.Frame),
                ("frameDelay", isDefault ? 0 : runtime.FrameDelay),
                ("frameWaitCounter", isDefault ? 0 : runtime.FrameWaitCounter),
                ("hitStateCount", isDefault ? 0 : runtime.HitStateCount),
                ("hitStop", isDefault ? 0 : runtime.HitStop),
                ("jumpInitPending", false),
                ("prevFrame", isDefault ? 0 : entity?.Frame?.Prev ?? 0),
                ("prevFrame2", isDefault ? 0 : runtime.PrevFrame2),
                ("suppressJumpInit", false),
                ("waitCounter", isDefault ? 0 : runtime.WaitCounter));

            object links = DictionaryOf(
                ("catcherIdx", isDefault ? -1 : runtime.CatcherSlotIndex),
                ("caughtDuration", isDefault ? 0 : runtime.CaughtDuration),
                ("caughtIdx", isDefault ? -1 : runtime.CaughtSlotIndex),
                ("escapeCounter", isDefault ? 0 : runtime.CatchingStateTU),
                ("grabbedTimer", 0),
                ("heldWeaponSlot", isDefault ? -1 : runtime.HeldWeaponStableId),
                ("holderCopy", isDefault ? 99 : runtime.HolderCopySlotIndex),
                ("holderIdx", isDefault ? -1 : runtime.HolderStableId),
                ("linkState", isDefault ? 0 : runtime.LinkState),
                ("pickerIdx", isDefault ? -1 : runtime.PickerStableId),
                ("pickupCount", isDefault ? 0 : runtime.PickupCount),
                ("releaseTick", runtime?.ReleaseTick ?? -1),
                ("stuckVictimSlot", -1),
                ("targetIdx", isDefault ? -1 : runtime.TargetSlotIndex),
                ("throwFrameGuard", isDefault ? -1 : runtime.ThrowFrameGuard));

            object transient = DictionaryOf(
                ("hitCandidateItrIndices", (object)new sbyte[20]),
                ("hitCandidateSlots", new int[20]),
                ("mp", isDefault ? 0 : runtime.TransientMp),
                ("mp2", isDefault ? 1000 : runtime.TransientMp2),
                ("mp3", isDefault ? 1000 : runtime.TransientMp3),
                ("mp4", isDefault ? 1000 : runtime.TransientMp4));

            object stats = DictionaryOf(
                ("comboCountAtk", isDefault ? 0 : runtime.ComboCountAtk),
                ("comboCountVic", isDefault ? 0 : runtime.ComboCountVic),
                ("fallDamageDiv", isDefault ? 0 : runtime.FallDamageDiv),
                ("hp", isDefault ? 500 : runtime.HP),
                ("hp3", isDefault ? 500 : runtime.HP3),
                ("hpMax", isDefault ? 500 : runtime.HPBound),
                ("killCount", isDefault ? -1 : runtime.KillCount),
                ("killStat", isDefault ? 0 : runtime.KillStat),
                ("pp", isDefault ? 500 : runtime.PP),
                ("respawnCount", isDefault ? 0 : runtime.RespawnCount),
                ("spawnerSlot", isDefault ? -1 : runtime.SpawnerSlotIndex),
                ("unk344", isDefault ? 0 : runtime.Unk344),
                ("weaponCount", isDefault ? 0 : runtime.WeaponCount));

            object input = DictionaryOf(
                ("cdAttack", (object)(runtime?.CdAttack ?? 0)),
                ("cdDefend", runtime?.CdDefend ?? 0),
                ("cdDefendLock", runtime?.CdDefendLock ?? 0),
                ("cdDown", runtime?.CdDown ?? 0),
                ("cdJump", runtime?.CdJump ?? 0),
                ("cdLeft", runtime?.CdLeft ?? 0),
                ("cdRight", runtime?.CdRight ?? 0),
                ("cdUp", runtime?.CdUp ?? 0),
                ("comboDda", runtime?.ComboDda ?? 0),
                ("comboDdj", runtime?.ComboDdj ?? 0),
                ("comboDja", runtime?.ComboDja ?? 0),
                ("comboDla", runtime?.ComboDla ?? 0),
                ("comboDlj", runtime?.ComboDlj ?? 0),
                ("comboDra", runtime?.ComboDra ?? 0),
                ("comboDrj", runtime?.ComboDrj ?? 0),
                ("comboDua", runtime?.ComboDua ?? 0),
                ("comboDuj", runtime?.ComboDuj ?? 0),
                ("inputHistory", isDefault ? new int[6] : CloneArray(runtime.InputHistory)),
                ("keyAttack", runtime?.KeyAttack ?? 0),
                ("keyDefend", runtime?.KeyDefend ?? 0),
                ("keyDown", runtime?.KeyDown ?? 0),
                ("keyJump", runtime?.KeyJump ?? 0),
                ("keyLeft", runtime?.KeyLeft ?? 0),
                ("keyRight", runtime?.KeyRight ?? 0),
                ("keyUp", runtime?.KeyUp ?? 0),
                ("prevAttack", runtime?.PrevAttack ?? 0),
                ("prevDefend", runtime?.PrevDefend ?? 0),
                ("prevDown", runtime?.PrevDown ?? 0),
                ("prevJump", runtime?.PrevJump ?? 0),
                ("prevLeft", runtime?.PrevLeft ?? 0),
                ("prevRight", runtime?.PrevRight ?? 0),
                ("prevUp", runtime?.PrevUp ?? 0));

            SortedDictionary<string, object> presentation = DictionaryOf(
                ("blink", isDefault ? 0 : runtime.Blink),
                ("hp2Orig", isDefault ? 0 : runtime.HP2Orig),
                ("hpOrig", isDefault ? 0 : runtime.HPOrig),
                ("ppDisplay", isDefault ? 0 : runtime.PpDisplay));
            if (includePresentationHitRecords)
            {
                presentation["hitRecordCount"] = entity?.HitRecordCount ?? 0;
                presentation["hitRecordDamage"] = hitRecordDamage;
                presentation["hitRecordX"] = hitRecordX;
                presentation["hitRecordZ"] = hitRecordZ;
            }

            object residual = DictionaryOf(
                ("abortRemainingHitPairs", false),
                ("attackExempt", isDefault ? 0 : runtime.AttackExempt),
                ("blockBackZ", runtime?.ZBoundNegative == true ? 1 : 0),
                ("blockFwdZ", runtime?.ZBoundPositive == true ? 1 : 0),
                ("blockLeft", runtime?.XBoundNegative == true ? 1 : 0),
                ("blockRight", runtime?.XBoundPositive == true ? 1 : 0),
                ("catchTimer", runtime?.CatchTimer ?? 0),
                ("healTimer", isDefault ? 0 : runtime.HealTimer),
                ("hitConfirm", isDefault ? 0 : runtime.HitConfirmEa),
                ("hitConfirm2", isDefault ? 0 : runtime.HitConfirm2),
                ("unk318", runtime?.RenderPicOffset ?? 0),
                ("unk31C", runtime?.WeaponFlightCounter ?? 0),
                ("unk324", runtime?.TransformOriginalObjectId ?? -1),
                ("unk328", isDefault ? -1 : runtime.Unk328),
                ("unk32C", isDefault ? -1 : runtime.Unk32C),
                ("unk330", isDefault ? 0 : runtime.Unk330),
                ("unk334", isDefault ? 0 : runtime.Unk334),
                ("unk338", isDefault ? 0 : runtime.Unk338),
                ("unk33C", runtime?.TransformTargetObjectId ?? -1),
                ("unk360", isDefault ? -1 : runtime.Unk360),
                ("unk3FC", isDefault ? -1000 : runtime.Unk3FC),
                ("unk400", isDefault ? -1000 : runtime.Unk400),
                ("weaponState", isDefault ? 0 : runtime.WeaponState));

            return DictionaryOf(
                ("frame", frame),
                ("identity", identity),
                ("input", input),
                ("links", links),
                ("motion", motion),
                ("presentation", presentation),
                ("residual", residual),
                ("stats", stats),
                ("transform", transform),
                ("transient", transient));
        }

        private object ProjectWorldDomain()
        {
            BattleRuntimeState battle = Runtime ?? new BattleRuntimeState();
            BattleMatchRuntimeState match = battle.Match ?? new BattleMatchRuntimeState();
            BattleStageRuntimeState stage = battle.Stage ?? new BattleStageRuntimeState();
            BattleFlowRuntimeState flow = battle.Flow ?? new BattleFlowRuntimeState();
            BattleResultsRuntimeState results = battle.Results ?? new BattleResultsRuntimeState();
            BattleRosterRuntimeState roster = battle.Roster ?? new BattleRosterRuntimeState();
            BattleStageProgressionState progression = battle.StageProgression ?? new BattleStageProgressionState();

            int slotCount = roster.Slots?.Length ?? 0;
            var battleSlotEntity = FilledArray(8, -1);
            var battleSlotOid = FilledArray(8, -1);
            var battleSlotState = new int[8];
            var battleSlotTeam = FilledArray(8, 1);
            var rosterSlots = new object[8];
            for (int i = 0; i < rosterSlots.Length; i++)
            {
                BattleSlotRuntimeState slot = i < slotCount ? roster.Slots[i] : null;
                bool active = slot?.Active ?? false;
                int oid = active ? slot.CharacterId : -1;
                int entitySlot = active ? slot.RuntimeSlotIndex : -1;
                int team = active ? slot.Team : 1;
                battleSlotEntity[i] = entitySlot;
                battleSlotOid[i] = oid;
                battleSlotState[i] = active ? 3 : 0;
                battleSlotTeam[i] = team;
                rosterSlots[i] = DictionaryOf(
                    ("active", (object)active),
                    ("ai", active && !slot.IsHuman),
                    ("entitySlot", entitySlot),
                    ("oid", oid),
                    ("state", battleSlotState[i]),
                    ("team", team));
            }

            object runtimeDomain = DictionaryOf(
                ("flow", DictionaryOf(
                    ("aiPhaseGate", (object)flow.AiPhaseGate),
                    ("battlePauseOverlay", 0),
                    ("battleStepEarlyReturned", 0),
                    ("battleStepFlag", 0),
                    ("battleStepGate", flow.BattleStepGate),
                    ("battleStepMode", flow.BattleStepMode),
                    ("frameMod12", flow.FrameMod12),
                    ("frameToggle", flow.FrameToggle),
                    ("gameTick", flow.CurrentTickIndex),
                    ("inputPhase", flow.InputPhase),
                    ("needClearInput", flow.NeedClearInput),
                    ("paused", false))),
                ("match", DictionaryOf(
                    ("difficulty", (object)match.Difficulty),
                    ("gameMode", match.BattleGameModeId),
                    ("randomStage", match.BackgroundId),
                    ("seed", match.Seed),
                    ("stageIdx", progression.StageSeriesIdx))),
                ("roster", DictionaryOf(
                    ("activeSlotCount", (object)roster.ActiveSlotCount),
                    ("slots", rosterSlots))),
                ("stage", DictionaryOf(
                    ("boundLeft", (object)0),
                    ("boundRight", stage.BoundRight),
                    ("cameraMaxOverride", stage.CameraMaxOverride),
                    ("cameraVel", _cameraVel),
                    ("cameraX", _cameraX),
                    ("width", stage.StageWidthPx),
                    ("xMaxOverride", stage.XMaxOverride),
                    ("zMax", stage.ZMax),
                    ("zMin", stage.ZMin))));

            return DictionaryOf(
                ("aiDifficulty", (object)flow.AiDifficulty),
                ("aiMoveMode", flow.AiMoveMode),
                ("aiPhaseGate", flow.AiPhaseGate),
                ("aiRand15", flow.AiRand15),
                ("aiRand20", flow.AiRand20),
                ("aiRand3", flow.AiRand3),
                ("aiRand5", flow.AiRand5),
                ("aiStageTargetX", flow.AiStageTargetX),
                ("battlePauseOverlay", 0),
                ("battleSlotCount", roster.ActiveSlotCount),
                ("battleSlotEntity", battleSlotEntity),
                ("battleSlotOid", battleSlotOid),
                ("battleSlotState", battleSlotState),
                ("battleSlotTeam", battleSlotTeam),
                ("battleStepEarlyReturned", 0),
                ("battleStepFlag449048", 0),
                ("battleStepGate44905C", flow.BattleStepGate),
                ("battleStepMode", flow.BattleStepMode),
                ("boundLeft", 0),
                ("boundRight", stage.BoundRight),
                ("cameraMaxOverride", stage.CameraMaxOverride),
                ("cameraVel", _cameraVel),
                ("cameraX", _cameraX),
                ("difficulty", match.Difficulty),
                ("djaGuardGlobal44F224", flow.DjaGuardGlobal44F224),
                ("f8Pressed", false),
                ("frameMod12", flow.FrameMod12),
                ("frameToggle", flow.FrameToggle),
                ("gameMode", match.BattleGameModeId),
                ("gameMode2", match.LocalGameModeId),
                ("gameTick", flow.CurrentTickIndex),
                ("humanInputPolledExternally", flow.HumanInputPolledExternally),
                ("initStats", 0),
                ("inputPhase", flow.InputPhase),
                ("needClearInput", flow.NeedClearInput),
                ("objectCount", ObjectCount),
                ("paused", false),
                ("ppMode", PpMode),
                ("randomStage", match.BackgroundId),
                ("reserveCommittedHp", ZeroMatrix(2, 11)),
                ("reserveCommittedTotal", ZeroMatrix(2, 11)),
                ("reserveLiveCount", ZeroMatrix(2, 11)),
                ("reserveMissingCount", ZeroMatrix(2, 11)),
                ("reserveOidTable", new[] { 30, 31, 33, 34, 39, 32, 35, 36, 37, 122, 123 }),
                ("reserveOwnerValid", false),
                ("results", DictionaryOf(
                    ("battleEndPhase", (object)results.BattleEndPhase),
                    ("hadBoth", results.HadBoth),
                    ("pendingHostAction", results.PendingHostAction),
                    ("pendingWinner", results.PendingWinner),
                    ("phase", results.Phase),
                    ("teamCount", results.TeamCount),
                    ("teamIds", CloneArray(results.TeamIds)),
                    ("timer", results.Timer),
                    ("winner", results.Winner))),
                ("runtime", runtimeDomain),
                ("stageAiInputCarrier", 0),
                ("stageIdx", progression.StageSeriesIdx),
                ("stageProgression", DictionaryOf(
                    ("round", (object)progression.Round),
                    ("roundMax", progression.RoundMax),
                    ("stageSeriesIdx", progression.StageSeriesIdx),
                    ("waveIdx", progression.WaveIdx))),
                ("stageProgressionValid", battle.StageProgressionValid),
                ("stageSpawnRuntimeEntryCount", CloneList(battle.StageSpawnRuntimeEntryCount)),
                ("stageSpawnRuntimeSlots", CloneNestedList(battle.StageSpawnRuntimeSlots)),
                ("stageSpawnRuntimeSpawnedTotal", CloneList(battle.StageSpawnRuntimeSpawnedTotal)),
                ("stageSpawnRuntimeTargetTotal", CloneList(battle.StageSpawnRuntimeTargetTotal)),
                ("stageSpawnRuntimeWave", battle.StageSpawnRuntimeWave),
                ("stageSpawnWaveApplied", battle.StageSpawnWaveApplied),
                ("stageSpawnWaveDeferredEntryApplied", battle.StageSpawnWaveDeferredEntryApplied),
                ("xMaxOverride", stage.XMaxOverride));
        }

        private object ProjectARestDomain()
        {
            var entries = new List<object>();
            for (int slot = 0; slot < AuthorityRuntimeSlotCapacity; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                int value = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
                if (value != 0)
                    entries.Add(DictionaryOf(("slot", (object)slot), ("value", value)));
            }
            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectVRestDomain()
        {
            var entries = new List<object>();
            var victims = new LF2Entity[AuthorityRuntimeSlotCapacity];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < AuthorityRuntimeSlotCapacity; first++)
            {
                for (int second = 0; second < AuthorityRuntimeSlotCapacity; second++)
                {
                    // v3 preserves the authority matrix byte order. Its historical labels
                    // call the first (actual victim) index attackerSlot.
                    int value = victims[first]?.ItrRest?.GetVrest(second) ??
                                GetRawRestVrest(first, second);
                    if (value == 0)
                        continue;
                    entries.Add(DictionaryOf(
                        ("attackerSlot", (object)first),
                        ("value", value),
                        ("victimSlot", second)));
                }
            }
            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectFullARestDomain()
        {
            var values = new int[AuthorityRuntimeSlotCapacity];
            for (int slot = 0; slot < values.Length; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                values[slot] = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
            }

            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "full"),
                ("values", values));
        }

        private object ProjectFullVRestDomain()
        {
            var values = new int[AuthorityRuntimeSlotCapacity][];
            var victims = new LF2Entity[AuthorityRuntimeSlotCapacity];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < AuthorityRuntimeSlotCapacity; first++)
            {
                var row = new int[AuthorityRuntimeSlotCapacity];
                for (int second = 0; second < AuthorityRuntimeSlotCapacity; second++)
                {
                    row[second] = victims[first]?.ItrRest?.GetVrest(second) ??
                                  GetRawRestVrest(first, second);
                }
                values[first] = row;
            }

            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "full-row-major"),
                ("values", values));
        }

        private object ProjectEventsDomain(
            IReadOnlyList<BattleParityStructuralEvent> structuralEvents = null)
        {
            var sounds = new object[PendingSounds?.Count ?? 0];
            for (int i = 0; i < sounds.Length; i++)
            {
                PendingSoundEvent sound = PendingSounds[i];
                sounds[i] = DictionaryOf(
                    ("cue", (object)NormalizeTraceAssetCue(sound.Cue)),
                    ("tick", sound.Tick),
                    ("worldX", sound.WorldX));
            }
            if (structuralEvents == null)
                return DictionaryOf(("pendingSounds", (object)sounds));

            var structural = new object[structuralEvents.Count];
            for (int i = 0; i < structural.Length; i++)
                structural[i] = structuralEvents[i]?.ToCanonicalObject();
            return DictionaryOf(
                ("pendingSounds", (object)sounds),
                ("structural", structural));
        }

        internal static string NormalizeTraceAssetCue(string value)
        {
            string normalized = (value ?? string.Empty).Trim().Replace('\\', '/');
            while (normalized.StartsWith("./", StringComparison.Ordinal))
                normalized = normalized.Substring(2);
            while (normalized.IndexOf("//", StringComparison.Ordinal) >= 0)
                normalized = normalized.Replace("//", "/");

            normalized = normalized.ToLowerInvariant();
            int separator = normalized.LastIndexOf('/');
            string identifier = separator >= 0 ? normalized.Substring(separator + 1) : normalized;
            return identifier.StartsWith("snddata_", StringComparison.Ordinal)
                ? identifier.Substring("snddata_".Length)
                : identifier;
        }

        private static int ResolveTraceCategory(int dataType)
        {
            return dataType switch
            {
                0 => 0,
                1 or 2 or 4 or 6 => 1,
                3 => 2,
                _ => 3,
            };
        }

        private static int[] CloneArray(int[] values)
        {
            return values == null ? Array.Empty<int>() : (int[])values.Clone();
        }

        private static int[] FilledArray(int count, int value)
        {
            var result = new int[count];
            if (value != 0)
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = value;
            }
            return result;
        }

        private static object[] ZeroMatrix(int rows, int columns)
        {
            var result = new object[rows];
            for (int i = 0; i < rows; i++)
                result[i] = new int[columns];
            return result;
        }

        private static int[] CloneList(List<int> values)
        {
            return values == null ? Array.Empty<int>() : values.ToArray();
        }

        private static object[] CloneNestedList(List<int[]> values)
        {
            if (values == null)
                return Array.Empty<object>();
            var result = new object[values.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = CloneArray(values[i]);
            return result;
        }

        private static SortedDictionary<string, object> DictionaryOf(
            params (string key, object value)[] values)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            for (int i = 0; i < values.Length; i++)
                result[values[i].key] = values[i].value;
            return result;
        }
    }
}
