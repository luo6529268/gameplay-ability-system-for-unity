using System;
using System.Collections.Generic;
using System.Linq;
using NTSD.DatParser;
using NTSD.Simulation;

namespace NTSD.Animation
{
    internal static class BattleVisualDependencySelector
    {
        private static readonly int[] BuiltinWeaponFragmentSources =
        {
            100, 101, 120, 121, 122, 123, 124, 150, 151, 201, 213, 217, 218
        };

        private static readonly IReadOnlyDictionary<int, int[]> HitFaTargets =
            new Dictionary<int, int[]>
            {
                [5] = new[] { 219 },
                [6] = new[] { 220 },
                [8] = new[] { 225 },
                [9] = new[] { 221, 222 },
                [11] = new[] { 211, 212, 221 },
                [13] = new[] { 228 }
            };

        public static BattleImageLoadPlan Build(
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs,
            IReadOnlyDictionary<int, int> types,
            IReadOnlyCollection<int> roots,
            LoganFusionCatalog fusion,
            LoganKindCatalog kind,
            IReadOnlyCollection<int> additionalRoots = null,
            bool includeScenario = true,
            bool includeCompatibility = true)
        {
            if (configs == null)
                throw new ArgumentNullException(nameof(configs));
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (roots == null || roots.Count == 0)
                throw new ArgumentException("At least one image dependency root is required.", nameof(roots));

            var rootIds = new SortedSet<int>();
            AddValidatedRoots(roots, configs, rootIds, nameof(roots));
            if (additionalRoots != null)
                AddValidatedRoots(additionalRoots, configs, rootIds, nameof(additionalRoots));

            var definitionIds = new SortedSet<int>(rootIds);
            var pending = new SortedSet<int>(rootIds);
            var processed = new HashSet<int>();
            var missing = new HashSet<int>();
            var edges = new HashSet<BattleVisualDependencyEdge>();
            var fusionKeys = new HashSet<FusionKey>();

            void AddDependency(int? fromId, int toId, int? frameId, string rule)
            {
                edges.Add(new BattleVisualDependencyEdge(fromId, toId, frameId, rule));

                if (!configs.TryGetValue(toId, out LF2CharacterDataWrapper wrapper) ||
                    wrapper == null || wrapper.characterData == null)
                {
                    missing.Add(toId);
                    return;
                }

                if (definitionIds.Add(toId))
                    pending.Add(toId);
            }

            if (includeScenario)
            {
                AddDependency(
                    null,
                    998,
                    null,
                    "scenario: respawn effect OID998");

                foreach (int id in configs.Keys.Where(id => id >= 100 && id <= 199).OrderBy(id => id))
                {
                    AddDependency(
                        null,
                        id,
                        null,
                        "scenario: loaded random-weapon OID in [100,199]");
                }
            }

            if (includeCompatibility)
            {
                if (configs.ContainsKey(209))
                {
                    AddDependency(
                        null,
                        209,
                        null,
                        "compatibility: optional OID209 when loaded");
                }
            }

            bool fusionChanged;
            do
            {
                while (pending.Count > 0)
                {
                    int objectId = pending.Min;
                    pending.Remove(objectId);
                    if (!processed.Add(objectId))
                        continue;

                    if (!configs.TryGetValue(objectId, out LF2CharacterDataWrapper wrapper) ||
                        wrapper == null || wrapper.characterData == null)
                    {
                        missing.Add(objectId);
                        continue;
                    }

                    LF2CharacterData data = wrapper.characterData;
                    bool hasType = types.TryGetValue(objectId, out int objectType);
                    if (!hasType)
                        throw new ArgumentException("Missing catalog type for dependency OID " + objectId, nameof(types));
                    IReadOnlyList<LF2FrameData> frames = data.frames;
                    if (frames != null)
                    {
                        foreach (LF2FrameData frame in frames)
                        {
                            if (frame == null)
                                continue;

                            foreach (BattleObjectPointValue point in EnumerateObjectPoints(frame))
                            {
                                if ((point.Kind == 1 || point.Kind == 2) && point.Oid > 0)
                                {
                                    AddDependency(
                                        objectId,
                                        point.Oid,
                                        frame.frameId,
                                        $"opoint: legal kind {point.Kind} positive OID");
                                }
                            }

                            int state = frame.state;
                            if (state >= 8000 && state <= 8999)
                            {
                                AddDependency(
                                    objectId,
                                    state - 8000,
                                    frame.frameId,
                                    "native state transform: state-8000");
                            }

                            if (hasType && objectType == 0 && state == 9996)
                            {
                                AddDependency(
                                    objectId,
                                    217,
                                    frame.frameId,
                                    "state9996: type0 child OID217");
                                AddDependency(
                                    objectId,
                                    218,
                                    frame.frameId,
                                    "state9996: type0 child OID218");
                            }

                            if (hasType && objectType != 0 &&
                                HitFaTargets.TryGetValue(frame.hit_Fa, out int[] targets))
                            {
                                foreach (int target in targets)
                                {
                                    AddDependency(
                                        objectId,
                                        target,
                                        frame.frameId,
                                        $"hit_Fa: type!=0 hit_Fa {frame.hit_Fa}");
                                }
                            }

                            if (state == 18 || state == 19)
                            {
                                AddDependency(
                                    objectId,
                                    999,
                                    frame.frameId,
                                    "state18/19: native particle OID999");
                            }

                            if (includeCompatibility && hasType && objectType == 0 && state == 9995)
                            {
                                AddDependency(
                                    objectId,
                                    50,
                                    frame.frameId,
                                    "compatibility: type0 state9995 -> OID50");
                            }

                            if (includeCompatibility && state >= 4000 && state <= 4999)
                            {
                                AddDependency(
                                    objectId,
                                    state - 4000,
                                    frame.frameId,
                                    "compatibility: state4000..4999 -> state-4000");
                            }
                        }
                    }

                    AddWeaponPieceDependencies(objectId, data, AddDependency);

                    if (Array.IndexOf(BuiltinWeaponFragmentSources, objectId) >= 0)
                    {
                        AddDependency(
                            objectId,
                            999,
                            null,
                            "builtin weapon fragment: source OID -> OID999");
                    }
                }

                fusionChanged = false;
                if (fusion != null && fusion.Records != null)
                {
                    foreach (LoganFusionRecord record in fusion.Records
                                 .OrderBy(value => value.Id1)
                                 .ThenBy(value => value.Id2)
                                 .ThenBy(value => value.Id3)
                                 .ThenBy(value => value.SourceLine))
                    {
                        if (!definitionIds.Contains(record.Id1) ||
                            !definitionIds.Contains(record.Id2))
                        {
                            continue;
                        }

                        var key = new FusionKey(record.Id1, record.Id2, record.Id3);
                        if (!fusionKeys.Add(key))
                            continue;

                        AddDependency(
                            null,
                            record.Id3,
                            null,
                            $"fusion: both OID{record.Id1}+OID{record.Id2} -> OID{record.Id3}");
                        fusionChanged = true;
                    }
                }
            }
            while (pending.Count > 0 || fusionChanged);

            AddCpointContextEdges(rootIds, definitionIds, types, configs, edges);
            AddKindContextEdges(definitionIds, types, kind, edges);

            return BattleImageLoadPlan.Create(
                BattleImageLoadPhase.Match,
                definitionIds,
                rootIds,
                missing,
                edges);
        }

        private static void AddValidatedRoots(
            IEnumerable<int> roots,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs,
            ISet<int> destination,
            string argumentName)
        {
            foreach (int root in roots)
            {
                if (!configs.TryGetValue(root, out LF2CharacterDataWrapper wrapper) ||
                    wrapper == null || wrapper.characterData == null)
                {
                    throw new ArgumentException(
                        $"Image dependency root {root} is empty or unknown.",
                        argumentName);
                }

                destination.Add(root);
            }
        }

        private static IEnumerable<BattleObjectPointValue> EnumerateObjectPoints(LF2FrameData frame)
        {
            if (frame.opoint.HasValue)
                yield return frame.opoint.Value;

            if (frame.opoints == null)
                yield break;

            foreach (BattleObjectPointValue point in frame.opoints)
                yield return point;
        }

        private static void AddWeaponPieceDependencies(
            int sourceId,
            LF2CharacterData data,
            Action<int?, int, int?, string> addDependency)
        {
            LoganWeaponPieceDefinition weaponPiece = data.NativeMetadata?.WeaponPiece;
            if (weaponPiece?.Groups == null)
                return;

            foreach (LoganWeaponPieceGroup group in weaponPiece.Groups)
            {
                if (group == null || group.Fields.Int32OrDefault("amount", 0) <= 0)
                    continue;

                if (group.Variants == null)
                    continue;

                foreach (LoganWeaponPieceVariant variant in group.Variants)
                {
                    if (variant == null)
                        continue;

                    int authoredTarget = variant.Fields.Int32OrDefault("oid", -1);
                    int target = authoredTarget == -1 ? 999 : authoredTarget;
                    addDependency(
                        sourceId,
                        target,
                        null,
                        $"weapon_piece: amount>0 all variants (oid {authoredTarget})");
                }
            }
        }

        private static void AddCpointContextEdges(
            IReadOnlyCollection<int> rootIds,
            IReadOnlyCollection<int> definitionIds,
            IReadOnlyDictionary<int, int> types,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs,
            ISet<BattleVisualDependencyEdge> edges)
        {
            int[] reachableType0 = definitionIds
                .Where(id => types.TryGetValue(id, out int type) && type == 0)
                .OrderBy(id => id)
                .ToArray();

            if (reachableType0.Length == 0)
                return;

            foreach (int sourceId in definitionIds.OrderBy(id => id))
            {
                if (!configs.TryGetValue(sourceId, out LF2CharacterDataWrapper wrapper) ||
                    wrapper?.characterData?.frames == null)
                {
                    continue;
                }

                foreach (LF2FrameData frame in wrapper.characterData.frames)
                {
                    if (frame == null || !HasCpointCopyContext(frame))
                        continue;

                    foreach (int partnerId in reachableType0)
                    {
                        if (partnerId == sourceId || !rootIds.Contains(partnerId))
                            continue;

                        edges.Add(new BattleVisualDependencyEdge(
                            sourceId,
                            partnerId,
                            frame.frameId,
                            "cpoint context: throwinjury=-1 and type0 partner is copresent"));
                    }
                }
            }
        }

        private static bool HasCpointCopyContext(LF2FrameData frame)
        {
            if (frame.CatchPoints != null && frame.CatchPoints.Count > 0)
            {
                foreach (BattleCatchPointValue point in frame.CatchPoints)
                {
                    if (point.ThrowInjury == -1)
                        return true;
                }

                return false;
            }

            return frame.cpoint != null && frame.cpoint.throwinjury == -1;
        }

        private static void AddKindContextEdges(
            IReadOnlyCollection<int> definitionIds,
            IReadOnlyDictionary<int, int> types,
            LoganKindCatalog kind,
            ISet<BattleVisualDependencyEdge> edges)
        {
            if (kind?.Records == null)
                return;

            foreach (int attackerId in definitionIds.OrderBy(id => id))
            {
                if (!types.TryGetValue(attackerId, out int attackerType) || attackerType != 3)
                    continue;

                foreach (LoganKindRecord record in kind.Records
                             .OrderBy(value => value.Effect)
                             .ThenBy(value => value.Frame)
                             .ThenBy(value => value.SourceLine))
                {
                    if (record == null || !record.Binds(attackerId) || record.RespondIds == null)
                        continue;

                    foreach (int partnerId in record.RespondIds.OrderBy(value => value))
                    {
                        if (!definitionIds.Contains(partnerId))
                            continue;

                        edges.Add(new BattleVisualDependencyEdge(
                            attackerId,
                            partnerId,
                            record.Frame,
                            $"kind context: type3 bound OID{attackerId} responds-to copresent OID{partnerId}"));
                    }
                }
            }
        }

        private readonly struct FusionKey : IEquatable<FusionKey>
        {
            public FusionKey(int first, int second, int result)
            {
                First = first;
                Second = second;
                Result = result;
            }

            private int First { get; }
            private int Second { get; }
            private int Result { get; }

            public bool Equals(FusionKey other)
            {
                return First == other.First && Second == other.Second && Result == other.Result;
            }

            public override bool Equals(object obj)
            {
                return obj is FusionKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = First;
                    hash = hash * 31 + Second;
                    return hash * 31 + Result;
                }
            }
        }
    }
}
