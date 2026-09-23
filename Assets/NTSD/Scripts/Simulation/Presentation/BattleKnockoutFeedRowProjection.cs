using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.App;

namespace NTSD.Simulation.Presentation
{
    public readonly struct BattleKnockoutFeedNameSnapshot
    {
        public BattleKnockoutFeedNameSnapshot(int slot, int battleGroup, string text,
            int screenLeft, int screenTop, bool teamColored, bool rightAligned)
        {
            Slot = slot;
            BattleGroup = battleGroup;
            Text = text;
            ScreenLeft = screenLeft;
            ScreenTop = screenTop;
            TeamColored = teamColored;
            RightAligned = rightAligned;
            NativeGlyphResourceSlot = teamColored && battleGroup >= 1 &&
                battleGroup <= 5 ? battleGroup : 0;
        }

        public int Slot { get; }
        public int BattleGroup { get; }
        public string Text { get; }
        public int ScreenLeft { get; }
        public int ScreenTop { get; }
        public bool TeamColored { get; }
        public bool RightAligned { get; }
        public int NativeGlyphResourceSlot { get; }
    }

    public readonly struct BattleKnockoutFeedRowSnapshot
    {
        public BattleKnockoutFeedRowSnapshot(int sequence, int battleTimeTick,
            int sourceObjectType, int sourceSlot, int victimSlot,
            int typeResourceIndex, string typeResourcePath, bool typeResourceAvailable,
            int imageScreenLeft, int imageScreenTop, int transparency, int respond,
            in BattleKnockoutFeedNameSnapshot attacker,
            in BattleKnockoutFeedNameSnapshot victim)
        {
            Sequence = sequence;
            BattleTimeTick = battleTimeTick;
            SourceObjectType = sourceObjectType;
            SourceSlot = sourceSlot;
            VictimSlot = victimSlot;
            TypeResourceIndex = typeResourceIndex;
            TypeResourcePath = typeResourcePath;
            TypeResourceAvailable = typeResourceAvailable;
            ImageScreenLeft = imageScreenLeft;
            ImageScreenTop = imageScreenTop;
            Transparency = transparency;
            Respond = respond;
            Attacker = attacker;
            Victim = victim;
        }

        public int Sequence { get; }
        public int BattleTimeTick { get; }
        public int SourceObjectType { get; }
        public int SourceSlot { get; }
        public int VictimSlot { get; }
        public int TypeResourceIndex { get; }
        public string TypeResourcePath { get; }
        public bool TypeResourceAvailable { get; }
        public int ImageScreenLeft { get; }
        public int ImageScreenTop { get; }
        public int Transparency { get; }
        public int Respond { get; }
        public BattleKnockoutFeedNameSnapshot Attacker { get; }
        public BattleKnockoutFeedNameSnapshot Victim { get; }
    }

    internal sealed class BattleKnockoutFeedRowProjection
    {
        private readonly struct LabelKey : IEquatable<LabelKey>
        {
            public LabelKey(int slot, int objectId, bool appendCharacterName)
            {
                Slot = slot;
                ObjectId = objectId;
                AppendCharacterName = appendCharacterName;
            }

            public int Slot { get; }
            public int ObjectId { get; }
            public bool AppendCharacterName { get; }

            public bool Equals(LabelKey other) =>
                Slot == other.Slot && ObjectId == other.ObjectId &&
                AppendCharacterName == other.AppendCharacterName;

            public override bool Equals(object obj) =>
                obj is LabelKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Slot * 397) ^ ObjectId) * 397 ^
                        (AppendCharacterName ? 1 : 0);
                }
            }
        }

        private static readonly string[] DefaultPlayerNames =
        {
            "Remie", "Kezeal", "John", "Herkato", "5",
            "6", "7", "8", "<No name>", "",
        };

        private readonly string[] playerNames = new string[10];
        private readonly bool[] bracketPlayerNames = new bool[8];
        private readonly bool[] typeResourceAvailable = new bool[7];
        private readonly Dictionary<LabelKey, string> labelCache =
            new Dictionary<LabelKey, string>(32);
        private LoganModeKnockoutFeedInput feed;
        private bool runtimeDisplayEnabled = true;

        internal BattleKnockoutFeedRowProjection()
        {
            SetNames(null);
        }

        internal void SetNames(MatchConfig config)
        {
            List<string> overrides = config?.nativeBattlePlayerNames;
            if (overrides != null && overrides.Count > 0)
            {
                if (overrides.Count != playerNames.Length)
                    throw new ArgumentException(
                        "Native battle player names require exactly ten slots.",
                        nameof(config));
                for (int slot = 0; slot < overrides.Count; slot++)
                {
                    string value = overrides[slot];
                    if (value == null || value.IndexOf('\0') >= 0 ||
                        Encoding.UTF8.GetByteCount(value) > 10)
                        throw new ArgumentException(
                            "A native battle player name exceeds its ten-byte payload.",
                            nameof(config));
                }
            }
            labelCache.Clear();
            runtimeDisplayEnabled = config?.nativeKnockoutFeedRuntimeDisplayEnabled ?? true;
            Array.Copy(DefaultPlayerNames, playerNames, playerNames.Length);
            Array.Clear(bracketPlayerNames, 0, bracketPlayerNames.Length);
            if (overrides != null)
            {
                for (int slot = 0; slot < overrides.Count; slot++)
                    playerNames[slot] = overrides[slot];
            }
            if (config?.nativeBracketPlayerNames != null)
            {
                int count = Math.Min(bracketPlayerNames.Length,
                    config.nativeBracketPlayerNames.Count);
                for (int slot = 0; slot < count; slot++)
                    bracketPlayerNames[slot] = config.nativeBracketPlayerNames[slot];
            }
            feed = null;
            Array.Clear(typeResourceAvailable, 0, typeResourceAvailable.Length);
        }

        internal void SetFeed(LoganObjectCatalog catalog)
        {
            SetFeed(catalog?.ModeComboInput?.KnockoutFeed, catalog?.Source);
        }

        internal void SetFeed(LoganModeKnockoutFeedInput input,
            BattleContentSource source)
        {
            feed = input;
            Array.Clear(typeResourceAvailable, 0, typeResourceAvailable.Length);
            if (feed == null)
                return;
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            for (int type = 0; type < typeResourceAvailable.Length; type++)
            {
                string virtualPath = feed.TypeResourcePath(type);
                if (string.IsNullOrEmpty(virtualPath))
                    continue;
                string path = source.ResolveImagePath(virtualPath, null);
                typeResourceAvailable[type] = File.Exists(path);
            }
        }

        internal void Project(SimulationWorld world, int tick, BattlePresentationFrame frame)
        {
            if (world == null || frame == null || feed == null)
                return;

            IReadOnlyList<NativeKnockoutEvent> events = world.NativeKnockoutEvents;
            int activeCount = events.Count;
            while (activeCount > 0)
            {
                NativeKnockoutEvent tail = events[activeCount - 1];
                if (tail.BattleTimeTick == 0 ||
                    (long)tail.BattleTimeTick + feed.LifetimeTicks >= tick)
                    break;
                activeCount--;
            }
            frame.KnockoutFeedNativeRecordCount = activeCount;
            if (!feed.Enabled || !runtimeDisplayEnabled ||
                !AllowsMode(world.BattleGameModeId))
                return;

            int rowTop = feed.ScreenTop;
            for (int index = 0; index < activeCount; index++)
            {
                NativeKnockoutEvent value = events[index];
                long endTick = (long)value.BattleTimeTick + 30;
                bool withinWindow = value.BattleTimeTick > 0 && tick < endTick;
                bool consumesSpacing = value.BattleTimeTick <= 0 || tick <= endTick;

                bool hasVictim = TryActor(world, value.VictimSlot, out NTSDEntityRuntime victim);
                bool excluded = hasVictim && ExcludesVictim(victim.ObjectId);
                if (withinWindow && !excluded)
                {
                    bool hasAttacker = TryActor(world, value.FourOwnerSlot,
                        out NTSDEntityRuntime attacker);
                    if (!hasAttacker || !hasVictim)
                    {
                        frame.KnockoutFeedMissingActorCount++;
                    }
                    else
                    {
                        int type = value.SourceObjectType >= 1 && value.SourceObjectType <= 6
                            ? value.SourceObjectType
                            : 0;
                        frame.AddKnockoutFeedRow(new BattleKnockoutFeedRowSnapshot(
                            value.BattleTimeTick,
                            value.BattleTimeTick,
                            value.SourceObjectType,
                            value.FourOwnerSlot,
                            value.VictimSlot,
                            type,
                            feed.TypeResourcePath(type) ?? string.Empty,
                            typeResourceAvailable[type],
                            feed.ImageScreenLeft,
                            rowTop,
                            feed.Transparency,
                            feed.Respond,
                            BuildName(world, value.FourOwnerSlot, attacker, rowTop,
                                feed.AttackerScreenLeft, feed.AttackerTeamColor,
                                feed.AttackerRightAligned,
                                feed.AttackerAppendCharacterName),
                            BuildName(world, value.VictimSlot, victim, rowTop,
                                feed.VictimScreenLeft, feed.VictimTeamColor,
                                feed.VictimRightAligned,
                                feed.VictimAppendCharacterName)));
                    }
                }
                if (consumesSpacing)
                    rowTop += feed.RowSpacing;
            }
        }

        private BattleKnockoutFeedNameSnapshot BuildName(SimulationWorld world,
            int slot, NTSDEntityRuntime runtime, int top, int left,
            bool teamColored, bool rightAligned, bool appendCharacterName)
        {
            var key = new LabelKey(slot, runtime.ObjectId, appendCharacterName);
            if (!labelCache.TryGetValue(key, out string text))
            {
                text = slot < playerNames.Length ? playerNames[slot] : "Com";
                if (slot < bracketPlayerNames.Length && bracketPlayerNames[slot])
                    text = "[" + text + "]";
                if (appendCharacterName)
                {
                    string characterName = world.RuntimeDataCatalog?
                        .GetCharacterData(runtime.ObjectId)?.name;
                    text += " [" + (string.IsNullOrEmpty(characterName)
                        ? "none"
                        : characterName) + "]";
                }
                labelCache.Add(key, text);
            }
            if (rightAligned)
                left -= Encoding.UTF8.GetByteCount(text) * 9;
            return new BattleKnockoutFeedNameSnapshot(slot, runtime.RelationTeam,
                text, left, top, teamColored, rightAligned);
        }

        private bool AllowsMode(int mode)
        {
            IReadOnlyList<int> modes = feed.AllowedBattleModes;
            if (modes.Count == 0)
                return true;
            for (int index = 0; index < modes.Count; index++)
                if (modes[index] == mode)
                    return true;
            return false;
        }

        private bool ExcludesVictim(int objectId)
        {
            IReadOnlyList<int> ids = feed.ExcludedVictimObjectIds;
            for (int index = 0; index < ids.Count; index++)
                if (ids[index] == objectId)
                    return true;
            return false;
        }

        private static bool TryActor(SimulationWorld world, int slot,
            out NTSDEntityRuntime runtime)
        {
            runtime = null;
            if (slot < 0 || slot >= 1000 ||
                !world.TryGetRuntimeSlotReadOnlyView(slot,
                    out RuntimeSlotTable.ReadOnlySlotView view) || !view.Claimed)
                return false;
            runtime = view.Entity?.Runtime ?? view.RawRuntime;
            return runtime != null;
        }
    }
}
