using NtsdReleaseCSharp.BattleCore.Simulation;
using NtsdReleaseCSharp.BattleCore.Entities;

namespace NTSDParity;

internal sealed class StructuralWitnessEvent
{
    public int Tick;
    public string Pass = string.Empty;
    public string Action = string.Empty;
    public int CursorSlot = -1;
    public int ActorSlot = -1;
    public int Slot = -1;
    public int SearchStart = -1;
    public int SearchEndExclusive = -1;
    public string Before = string.Empty;
    public string After = string.Empty;
    public int LifecycleEpoch;
    public string SourceKind = string.Empty;
    public int BeforeLinkState;
    public int BeforeTargetSlot = -1;
    public int BeforeHeldWeaponSlot = -1;
    public int AfterLinkState;
    public int AfterTargetSlot = -1;
    public int AfterHeldWeaponSlot = -1;
    public bool TargetActive;
    public int ObservedHolderSlot = -1;
    public string Outcome = string.Empty;
    public string Reason = string.Empty;
    public int TargetBeforeHolderSlot = -1;
    public int TargetBeforeLinkState;
    public int TargetAfterHolderSlot = -1;
    public int TargetAfterLinkState;

    public object ToCanonicalObject()
    {
        SortedDictionary<string, object?> result = new(StringComparer.Ordinal)
        {
            ["action"] = Action,
            ["actorSlot"] = ActorSlot,
            ["after"] = After,
            ["before"] = Before,
            ["cursorSlot"] = CursorSlot,
            ["lifecycleEpoch"] = LifecycleEpoch,
            ["pass"] = Pass,
            ["searchEndExclusive"] = SearchEndExclusive,
            ["searchStart"] = SearchStart,
            ["slot"] = Slot,
            ["sourceKind"] = SourceKind,
            ["tick"] = Tick,
        };
        if (Action == "link-validation")
        {
            result["afterHeldWeaponSlot"] = AfterHeldWeaponSlot;
            result["afterLinkState"] = AfterLinkState;
            result["afterTargetSlot"] = AfterTargetSlot;
            result["beforeHeldWeaponSlot"] = BeforeHeldWeaponSlot;
            result["beforeLinkState"] = BeforeLinkState;
            result["beforeTargetSlot"] = BeforeTargetSlot;
            result["observedHolderSlot"] = ObservedHolderSlot;
            result["outcome"] = Outcome;
            result["reason"] = Reason;
            result["targetActive"] = TargetActive;
            result["targetAfterHolderSlot"] = TargetAfterHolderSlot;
            result["targetAfterLinkState"] = TargetAfterLinkState;
            result["targetBeforeHolderSlot"] = TargetBeforeHolderSlot;
            result["targetBeforeLinkState"] = TargetBeforeLinkState;
        }
        return result;
    }
}

internal sealed class StructuralWitnessEventBuffer
{
    private readonly int[] lifecycleEpochs = new int[400];
    private readonly List<StructuralWitnessEvent> events = [];

    public void Record(StructuralWitnessEvent structuralEvent)
    {
        if (structuralEvent.Slot is >= 400)
            throw new InvalidOperationException("Authority400 structural fixture emitted a slot above 399.");
        if (structuralEvent.Slot >= 0 && structuralEvent.Action == "allocate")
            lifecycleEpochs[structuralEvent.Slot]++;
        structuralEvent.LifecycleEpoch = structuralEvent.Slot >= 0
            ? lifecycleEpochs[structuralEvent.Slot]
            : 0;
        events.Add(structuralEvent);
    }

    public object[] CaptureTick(int tick) => events
        .Where(value => value.Tick == tick)
        .Select(value => value.ToCanonicalObject())
        .ToArray();
}

internal static class StructuralWitnessFixture
{
    public static StructuralWitnessEventBuffer? Run(string? witnessId)
    {
        if (string.IsNullOrWhiteSpace(witnessId))
            return null;
        return witnessId switch
        {
            "W03" => RunW03(),
            "W04" => RunW04(),
            "W07" => RunW07(),
            _ => throw new ArgumentException(
                $"Unsupported structuralWitness '{witnessId}'. Expected W03, W04, or W07."),
        };
    }

    private static StructuralWitnessEventBuffer RunW03()
    {
        SimulationWorld world = new();
        StructuralWitnessEventBuffer buffer = new();
        SpawnRequired(world, buffer, 0, 0, "fixture-setup", "general", -1);
        SpawnRequired(world, buffer, 0, 1, "fixture-setup", "general", -1);
        SpawnRequired(world, buffer, 0, 2, "fixture-setup", "general", -1);

        for (int tick = 1; tick <= 2; tick++)
        {
            for (int cursor = 0; cursor < world.Objects.Length; cursor++)
            {
                if (!world.Objects[cursor].Active)
                    continue;

                Record(buffer, tick, "late-entity-update", "scan", cursor, cursor,
                    cursor, 0, 400, "active", "visited", "general");
                if (tick != 1 || cursor != 0)
                    continue;

                world.FreeEntity(0);
                Record(buffer, tick, "late-entity-update", "free", cursor, cursor,
                    0, -1, -1, "active", "free", "general");
                Record(buffer, tick, "late-entity-update", "unregister-deferred", cursor, cursor,
                    0, -1, -1, "active", "pending", "general");

                int lowSlot = FindLowest(world, 0, 400);
                Record(buffer, tick, "late-entity-update", "search", cursor, cursor,
                    lowSlot, 0, 400, "free", "selected", "general");
                world.SpawnAt(lowSlot, 0, 0, 0, 0, 0);
                Record(buffer, tick, "late-entity-update", "allocate", cursor, cursor,
                    lowSlot, -1, -1, "free", "active", "general");

                SpawnRequired(world, buffer, tick, 3, "late-entity-update", "general", cursor);
            }

            if (tick == 1)
            {
                Record(buffer, tick, "late-entity-update", "unregister-flush", -1, -1,
                    0, -1, -1, "pending", "free", "general");
            }
        }

        object[] tick1 = buffer.CaptureTick(1);
        object[] tick2 = buffer.CaptureTick(2);
        if (tick1.Length == 0 || tick2.Length == 0 ||
            !world.Objects[0].Active || !world.Objects[3].Active)
        {
            throw new InvalidOperationException("Authority W03 structural fixture did not execute.");
        }
        return buffer;
    }

    private static StructuralWitnessEventBuffer RunW04()
    {
        SimulationWorld world = new();
        StructuralWitnessEventBuffer buffer = new();

        int general = FindLowest(world, 0, 400);
        SpawnSearched(world, buffer, 1, general, 0, 400, "allocator", "general");
        int stage = FindLowest(world, 20, 400);
        SpawnSearched(world, buffer, 1, stage, 20, 400, "allocator", "stage");
        int dynamicSlot = FindLowest(world, 50, 400);
        SpawnSearched(world, buffer, 1, dynamicSlot, 50, 400, "allocator", "dynamic");
        SpawnRequired(world, buffer, 1, 399, "allocator", "general", -1);

        if (general != 0 || stage != 20 || dynamicSlot != 50 ||
            !world.Objects[399].Active)
        {
            throw new InvalidOperationException(
                "Authority W04 structural fixture did not hit allocator starts 0/20/50 and slot 399.");
        }
        return buffer;
    }

    private static StructuralWitnessEventBuffer RunW07()
    {
        SimulationWorld world = new();
        StructuralWitnessEventBuffer buffer = new();
        SpawnRequired(world, buffer, 0, 0, "fixture-setup", "positive-link", -1);
        SpawnRequired(world, buffer, 0, 1, "fixture-setup", "positive-link", -1);

        Entity holder = world.Objects[0];
        Entity target = world.Objects[1];
        GameTick.Run(world);

        holder.LinkState = 1;
        holder.TargetIdx = 1;
        holder.HeldWeaponSlot = 1;
        target.LinkState = 0;
        target.HolderIdx = 0;
        RunPositiveLinkValidationTick(world, buffer, 2, holder, target);

        target.HolderIdx = 2;
        RunPositiveLinkValidationTick(world, buffer, 3, holder, target);

        if (world.GameTick != 3 ||
            holder.LinkState != 0 || holder.TargetIdx != -1 || holder.HeldWeaponSlot != -1 ||
            target.HolderIdx != 2 || target.LinkState != 0)
        {
            throw new InvalidOperationException(
                "Authority W07 GameTick fixture failed: " +
                $"tick={world.GameTick}, holder={holder.LinkState}/{holder.TargetIdx}/{holder.HeldWeaponSlot}, " +
                $"target={target.HolderIdx}/{target.LinkState}.");
        }
        return buffer;
    }

    private static void RunPositiveLinkValidationTick(
        SimulationWorld world,
        StructuralWitnessEventBuffer buffer,
        int tick,
        Entity holder,
        Entity target)
    {
        int beforeLinkState = 0;
        int beforeTargetSlot = -1;
        int beforeHeldWeaponSlot = -1;
        bool targetActive = false;
        int observedHolderSlot = -1;
        int targetBeforeLinkState = 0;

        GameTick.Run(
            world,
            beforeCollectCandidates: () =>
            {
                beforeLinkState = holder.LinkState;
                beforeTargetSlot = holder.TargetIdx;
                beforeHeldWeaponSlot = holder.HeldWeaponSlot;
                targetActive = target.Active;
                observedHolderSlot = targetActive ? target.HolderIdx : -1;
                targetBeforeLinkState = targetActive ? target.LinkState : 0;
            },
            afterCollectCandidates: () =>
            {
                if (world.GameTick != tick)
                    throw new InvalidOperationException("Authority W07 observation tick drifted from GameTick.Run.");
                bool kept = holder.LinkState > 0;
                buffer.Record(new StructuralWitnessEvent
                {
                    Tick = tick,
                    Pass = "positive-link-validation",
                    Action = "link-validation",
                    CursorSlot = 0,
                    ActorSlot = 0,
                    Slot = 0,
                    Before = $"{beforeLinkState}/{beforeTargetSlot}/{beforeHeldWeaponSlot}",
                    After = $"{holder.LinkState}/{holder.TargetIdx}/{holder.HeldWeaponSlot}",
                    SourceKind = "positive-link",
                    BeforeLinkState = beforeLinkState,
                    BeforeTargetSlot = beforeTargetSlot,
                    BeforeHeldWeaponSlot = beforeHeldWeaponSlot,
                    AfterLinkState = holder.LinkState,
                    AfterTargetSlot = holder.TargetIdx,
                    AfterHeldWeaponSlot = holder.HeldWeaponSlot,
                    TargetActive = targetActive,
                    ObservedHolderSlot = observedHolderSlot,
                    Outcome = kept ? "kept" : "cleared",
                    Reason = kept ? "reciprocal" : targetActive ? "holder-mismatch" : "target-inactive",
                    TargetBeforeHolderSlot = observedHolderSlot,
                    TargetBeforeLinkState = targetBeforeLinkState,
                    TargetAfterHolderSlot = target.Active ? target.HolderIdx : -1,
                    TargetAfterLinkState = target.Active ? target.LinkState : 0,
                });
            });
    }

    private static void SpawnSearched(
        SimulationWorld world,
        StructuralWitnessEventBuffer buffer,
        int tick,
        int slot,
        int searchStart,
        int searchEndExclusive,
        string pass,
        string sourceKind)
    {
        Record(buffer, tick, pass, "search", -1, -1, slot, searchStart,
            searchEndExclusive, "free", "selected", sourceKind);
        world.SpawnAt(slot, 0, 0, 0, 0, 0);
        Record(buffer, tick, pass, "allocate", -1, -1, slot, -1, -1,
            "free", "active", sourceKind);
    }

    private static void SpawnRequired(
        SimulationWorld world,
        StructuralWitnessEventBuffer buffer,
        int tick,
        int slot,
        string pass,
        string sourceKind,
        int cursor)
    {
        Record(buffer, tick, pass, "search", cursor, cursor, slot,
            sourceKind == "stage" ? 20 : slot,
            sourceKind == "stage" ? 400 : slot + 1,
            "free", "selected", sourceKind);
        world.SpawnAt(slot, 0, 0, 0, 0, 0);
        Record(buffer, tick, pass, "allocate", cursor, cursor, slot, -1, -1,
            "free", "active", sourceKind);
    }

    private static int FindLowest(SimulationWorld world, int start, int endExclusive)
    {
        for (int slot = start; slot < endExclusive; slot++)
        {
            if (!world.Objects[slot].Active)
                return slot;
        }
        return -1;
    }

    private static void Record(
        StructuralWitnessEventBuffer buffer,
        int tick,
        string pass,
        string action,
        int cursor,
        int actor,
        int slot,
        int searchStart,
        int searchEndExclusive,
        string before,
        string after,
        string sourceKind)
    {
        buffer.Record(new StructuralWitnessEvent
        {
            Tick = tick,
            Pass = pass,
            Action = action,
            CursorSlot = cursor,
            ActorSlot = actor,
            Slot = slot,
            SearchStart = searchStart,
            SearchEndExclusive = searchEndExclusive,
            Before = before,
            After = after,
            SourceKind = sourceKind,
        });
    }
}
