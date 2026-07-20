using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
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

    public sealed class BattleParityFrameSnapshot
    {
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

    public partial class SimulationWorld
    {
        public BattleParityFrameSnapshot CaptureParityFrameSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            bool includeFullDomains = false)
        {
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object[] allSlots = ProjectAllRuntimeSlots();
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
            object eventsDomain = ProjectEventsDomain();

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
                object baseline = ProjectDefaultRuntimeSlot(slot);
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

        private object[] ProjectAllRuntimeSlots()
        {
            var result = new object[MaxRuntimeSlots];
            for (int runtimeSlot = 0; runtimeSlot < result.Length; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                result[runtimeSlot] = entity == null
                    ? ProjectDefaultRuntimeSlot(runtimeSlot, GetRawRuntimeSlotState(runtimeSlot))
                    : ProjectRuntimeSlot(entity, runtimeSlot);
            }
            return result;
        }

        private object ProjectDefaultRuntimeSlot(int runtimeSlot, NTSDEntityRuntime runtime = null)
        {
            return DictionaryOf(
                ("currentDataOid", null),
                ("runtime", ProjectEntityRuntime(null, runtimeSlot, false, runtime)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectRuntimeSlot(LF2Entity entity, int runtimeSlot)
        {
            bool active = IsActiveForCurrentPass(entity);
            int? currentDataOid = entity.FrameCache?.Wrapper != null
                ? entity.FrameCache.Wrapper.characterId
                : entity.ObjectId;
            return DictionaryOf(
                ("currentDataOid", (object)currentDataOid),
                ("runtime", ProjectEntityRuntime(entity, runtimeSlot, active)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectEntityRuntime(
            LF2Entity entity,
            int runtimeSlot,
            bool active,
            NTSDEntityRuntime runtimeOverride = null)
        {
            NTSDEntityRuntime runtime = entity?.Runtime ?? runtimeOverride;
            bool isDefault = entity == null;
            int[] hitRecordDamage = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordX = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordZ = new int[LF2Entity.MaxHitRecordSlots];
            if (entity != null)
            {
                for (int i = 0; i < hitRecordDamage.Length; i++)
                {
                    hitRecordDamage[i] = entity.GetHitRecordAge(i);
                    hitRecordX[i] = entity.GetHitRecordX(i);
                    hitRecordZ[i] = entity.GetHitRecordZ(i);
                }
            }

            int currentDataType = entity?.GetCurrentDataObjectTypeForSimulation() ?? -1;
            int category = ResolveTraceCategory(currentDataType);
            object identity = DictionaryOf(
                ("active", active),
                ("aiControlled", runtime?.AiControlled ?? false),
                ("category", isDefault ? 0 : category),
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
                ("prevFrame", isDefault ? 0 : entity.Frame?.Prev ?? 0),
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

            object presentation = DictionaryOf(
                ("blink", isDefault ? 0 : runtime.Blink),
                ("hitRecordCount", entity?.HitRecordCount ?? 0),
                ("hitRecordDamage", hitRecordDamage),
                ("hitRecordX", hitRecordX),
                ("hitRecordZ", hitRecordZ),
                ("hp2Orig", isDefault ? 0 : runtime.HP2Orig),
                ("hpOrig", isDefault ? 0 : runtime.HPOrig),
                ("ppDisplay", isDefault ? 0 : runtime.PpDisplay));

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
            for (int slot = 0; slot < MaxRuntimeSlots; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                int value = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
                if (value != 0)
                    entries.Add(DictionaryOf(("slot", (object)slot), ("value", value)));
            }
            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectVRestDomain()
        {
            var entries = new List<object>();
            var victims = new LF2Entity[MaxRuntimeSlots];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < MaxRuntimeSlots; first++)
            {
                for (int second = 0; second < MaxRuntimeSlots; second++)
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
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectFullARestDomain()
        {
            var values = new int[MaxRuntimeSlots];
            for (int slot = 0; slot < values.Length; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                values[slot] = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
            }

            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "full"),
                ("values", values));
        }

        private object ProjectFullVRestDomain()
        {
            var values = new int[MaxRuntimeSlots][];
            var victims = new LF2Entity[MaxRuntimeSlots];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < MaxRuntimeSlots; first++)
            {
                var row = new int[MaxRuntimeSlots];
                for (int second = 0; second < MaxRuntimeSlots; second++)
                {
                    row[second] = victims[first]?.ItrRest?.GetVrest(second) ??
                                  GetRawRestVrest(first, second);
                }
                values[first] = row;
            }

            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "full-row-major"),
                ("values", values));
        }

        private object ProjectEventsDomain()
        {
            var sounds = new object[PendingSounds?.Count ?? 0];
            for (int i = 0; i < sounds.Length; i++)
            {
                PendingSoundEvent sound = PendingSounds[i];
                sounds[i] = DictionaryOf(
                    ("cue", (object)NormalizeTraceAssetCue(sound?.Cue)),
                    ("tick", sound?.Tick ?? 0),
                    ("worldX", sound?.WorldX ?? 0));
            }
            return DictionaryOf(("pendingSounds", (object)sounds));
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
