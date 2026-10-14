using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NTSD.Animation
{
    internal enum BattleImageLoadPhase
    {
        Menu,
        Match,
        FullCatalogue
    }

    internal readonly struct BattleVisualDependencyEdge : IEquatable<BattleVisualDependencyEdge>
    {
        public BattleVisualDependencyEdge(
            int? fromId,
            int toId,
            int? frameId,
            string rule)
        {
            if (string.IsNullOrWhiteSpace(rule))
                throw new ArgumentException("A dependency edge requires a rule.", nameof(rule));

            FromId = fromId;
            ToId = toId;
            FrameId = frameId;
            Rule = rule;
        }

        public int? FromId { get; }
        public int ToId { get; }
        public int? FrameId { get; }
        public string Rule { get; }

        public bool Equals(BattleVisualDependencyEdge other)
        {
            return FromId == other.FromId &&
                   ToId == other.ToId &&
                   FrameId == other.FrameId &&
                   string.Equals(Rule, other.Rule, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is BattleVisualDependencyEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (FromId.HasValue ? FromId.Value : int.MinValue);
                hash = hash * 31 + ToId;
                hash = hash * 31 + (FrameId.HasValue ? FrameId.Value : int.MinValue);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Rule ?? string.Empty);
                return hash;
            }
        }

        public static bool operator ==(
            BattleVisualDependencyEdge left,
            BattleVisualDependencyEdge right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleVisualDependencyEdge left,
            BattleVisualDependencyEdge right)
        {
            return !left.Equals(right);
        }
    }

    internal sealed class BattleImageLoadPlan
    {
        private const string FingerprintVersion = "NTSD28-BATTLE-IMAGE-LOAD-PLAN-v1";

        private readonly ReadOnlyCollection<int> definitionIds;
        private readonly ReadOnlyCollection<int> rootIds;
        private readonly ReadOnlyCollection<int> missingDependencies;
        private readonly ReadOnlyCollection<BattleVisualDependencyEdge> edges;
        private readonly HashSet<int> definitionLookup;

        private BattleImageLoadPlan(
            BattleImageLoadPhase phase,
            IEnumerable<int> definitionIds,
            IEnumerable<int> rootIds,
            IEnumerable<int> missingDependencies,
            IEnumerable<BattleVisualDependencyEdge> edges,
            string fallbackReason)
        {
            Phase = phase;
            this.definitionIds = NormalizeIds(definitionIds);
            this.rootIds = NormalizeIds(rootIds);
            this.missingDependencies = NormalizeIds(missingDependencies);
            this.edges = NormalizeEdges(edges);
            this.definitionLookup = new HashSet<int>(this.definitionIds);
            FallbackReason = string.IsNullOrWhiteSpace(fallbackReason)
                ? null
                : fallbackReason;
            Fingerprint = ComputeFingerprint(Phase, this.rootIds, this.definitionIds);
        }

        public BattleImageLoadPhase Phase { get; }
        public IReadOnlyList<int> DefinitionIds => definitionIds;
        public IReadOnlyList<int> RootIds => rootIds;
        public IReadOnlyList<int> MissingDependencies => missingDependencies;
        public IReadOnlyList<BattleVisualDependencyEdge> Edges => edges;
        public string Fingerprint { get; }
        public string FallbackReason { get; }

        public bool IncludesDefinition(int definitionId)
        {
            return definitionLookup.Contains(definitionId);
        }

        public IEnumerable<LF2CharacterDataWrapper> SelectBodyConfigs(
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs)
        {
            if (Phase == BattleImageLoadPhase.Menu)
                return Array.Empty<LF2CharacterDataWrapper>();

            if (configs == null)
                throw new ArgumentNullException(nameof(configs));

            var selected = new List<LF2CharacterDataWrapper>(definitionIds.Count);
            foreach (int definitionId in definitionIds)
            {
                if (!configs.TryGetValue(definitionId, out LF2CharacterDataWrapper wrapper))
                {
                    throw new InvalidOperationException(
                        $"Image load plan {Phase} declares missing definition {definitionId}.");
                }

                if (wrapper == null || wrapper.characterData == null)
                {
                    throw new InvalidOperationException(
                        $"Image load plan {Phase} declares an empty definition {definitionId}.");
                }

                selected.Add(wrapper);
            }

            return new ReadOnlyCollection<LF2CharacterDataWrapper>(selected);
        }

        public static BattleImageLoadPlan Menu()
        {
            return new BattleImageLoadPlan(
                BattleImageLoadPhase.Menu,
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<BattleVisualDependencyEdge>(),
                null);
        }

        public static BattleImageLoadPlan FullCatalogue(
            IEnumerable<int> ids,
            string reason = null)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            int[] normalized = NormalizeIds(ids).ToArray();
            return new BattleImageLoadPlan(
                BattleImageLoadPhase.FullCatalogue,
                normalized,
                normalized,
                Array.Empty<int>(),
                Array.Empty<BattleVisualDependencyEdge>(),
                reason);
        }

        internal static BattleImageLoadPlan Create(
            BattleImageLoadPhase phase,
            IEnumerable<int> definitionIds,
            IEnumerable<int> rootIds,
            IEnumerable<int> missingDependencies,
            IEnumerable<BattleVisualDependencyEdge> edges,
            string fallbackReason = null)
        {
            return new BattleImageLoadPlan(
                phase,
                definitionIds,
                rootIds,
                missingDependencies,
                edges,
                fallbackReason);
        }

        private static ReadOnlyCollection<int> NormalizeIds(IEnumerable<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source
                .Distinct()
                .OrderBy(value => value)
                .ToList()
                .AsReadOnly();
        }

        private static ReadOnlyCollection<BattleVisualDependencyEdge> NormalizeEdges(
            IEnumerable<BattleVisualDependencyEdge> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source
                .Distinct()
                .OrderBy(edge => edge.FromId.HasValue ? edge.FromId.Value : int.MinValue)
                .ThenBy(edge => edge.ToId)
                .ThenBy(edge => edge.FrameId.HasValue ? edge.FrameId.Value : int.MinValue)
                .ThenBy(edge => edge.Rule, StringComparer.Ordinal)
                .ToList()
                .AsReadOnly();
        }

        private static string ComputeFingerprint(
            BattleImageLoadPhase phase,
            IReadOnlyList<int> roots,
            IReadOnlyList<int> ids)
        {
            string canonical = string.Join(
                "\n",
                FingerprintVersion,
                "phase=" + phase,
                "roots=" + JoinIds(roots),
                "ids=" + JoinIds(ids));

            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] digest = algorithm.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                return BitConverter.ToString(digest).Replace("-", string.Empty);
            }
        }

        private static string JoinIds(IReadOnlyList<int> ids)
        {
            return string.Join(
                ",",
                ids.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
