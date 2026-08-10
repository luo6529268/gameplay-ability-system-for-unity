using System;
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    /// <summary>
    /// Tracks active presentation mounts and assigns their generation-aware runtime identity.
    /// </summary>
    internal static class BattleCentralPresentationMountRegistry
    {
        private static readonly BattleCentralPresentationMountRegistryState State =
            new BattleCentralPresentationMountRegistryState();

        internal static long RejectedMountRegistrationCount =>
            State.RejectedMountRegistrationCount;
        internal static long RejectedOwnerBindingCount =>
            State.RejectedOwnerBindingCount;
        internal static bool IsCapacitySealed => State.IsCapacitySealed;
        internal static int OwnerRuntimeBindingCountForSelfCheck =>
            State.OwnerRuntimeBindingCountForSelfCheck;

        internal static void PrepareCapacity(int ownerCapacity) =>
            State.PrepareCapacity(ownerCapacity);
        internal static void SealCapacity() => State.SealCapacity();
        internal static void UnsealCapacity() => State.UnsealCapacity();
        internal static void Register(BattleCentralPresentationMount mount) =>
            State.Register(mount);
        internal static void Unregister(BattleCentralPresentationMount mount) =>
            State.Unregister(mount);
        internal static void BindOwnerRuntime(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle) =>
            State.BindOwnerRuntime(ownerRenderer, runtimeHandle);
        internal static void ResetOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer) =>
            State.ResetOwnerRuntimeBinding(ownerRenderer);
        internal static void RemoveOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer) =>
            State.RemoveOwnerRuntimeBinding(ownerRenderer);
        internal static bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId) =>
            State.HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        internal static bool HasValidOwnerRuntimeBindingForAcceptance(int ownerInstanceId) =>
            State.HasValidOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        internal static bool HasOwnerRuntimeBindingForSelfCheck(int ownerInstanceId) =>
            State.HasOwnerRuntimeBindingForSelfCheck(ownerInstanceId);
        internal static void ResetAllRuntimeBindings() =>
            State.ResetAllRuntimeBindings();
        internal static void ValidateActiveMounts() => State.ValidateActiveMounts();
        internal static bool ValidateActiveMounts(out string error) =>
            State.ValidateActiveMounts(out error);
    }

    /// <summary>
    /// Owns the mutable mount registry state. The static compatibility surface above
    /// contains no mutable collections and can be removed once all Unity callbacks
    /// receive an explicit central-render runtime owner.
    /// </summary>
    internal sealed class BattleCentralPresentationMountRegistryState
    {
        private readonly List<BattleCentralPresentationMount> activeMounts =
            new List<BattleCentralPresentationMount>(32);

        private readonly Dictionary<int, OwnerRuntimeBinding> ownerRuntimeBindings =
            new Dictionary<int, OwnerRuntimeBinding>(32);

        private readonly List<int> ownerInstanceIds = new List<int>(32);

        private int preparedMountCapacity = 32;
        private int preparedOwnerCapacity = 32;
        private bool capacitySealed;

        internal long RejectedMountRegistrationCount { get; private set; }
        internal long RejectedOwnerBindingCount { get; private set; }
        internal bool IsCapacitySealed => capacitySealed;

        internal void PrepareCapacity(int ownerCapacity)
        {
            if (capacitySealed)
                return;

            int normalizedOwnerCapacity = Math.Max(ownerCapacity, ownerRuntimeBindings.Count);
            int normalizedMountCapacity = Math.Max(normalizedOwnerCapacity * 2, activeMounts.Count);
            ownerRuntimeBindings.EnsureCapacity(normalizedOwnerCapacity);
            if (ownerInstanceIds.Capacity < normalizedOwnerCapacity)
                ownerInstanceIds.Capacity = normalizedOwnerCapacity;
            if (activeMounts.Capacity < normalizedMountCapacity)
                activeMounts.Capacity = normalizedMountCapacity;
            preparedOwnerCapacity = Math.Max(preparedOwnerCapacity, normalizedOwnerCapacity);
            preparedMountCapacity = Math.Max(preparedMountCapacity, normalizedMountCapacity);
        }

        internal void SealCapacity()
        {
            capacitySealed = true;
        }

        internal void UnsealCapacity()
        {
            capacitySealed = false;
        }

        internal void Register(BattleCentralPresentationMount mount)
        {
            if (mount == null || activeMounts.Contains(mount))
                return;

            PruneDestroyedEntries();
            if (capacitySealed && activeMounts.Count >= preparedMountCapacity)
            {
                RejectedMountRegistrationCount++;
                return;
            }

            activeMounts.Add(mount);
            if (IsConfigurationValid(mount) &&
                TryGetOwnerRuntimeBinding(mount.OwnerRenderer, out RuntimeEntityHandle runtimeHandle))
            {
                mount.SetRuntimeHandle(runtimeHandle);
            }
        }

        internal void Unregister(BattleCentralPresentationMount mount)
        {
            if (ReferenceEquals(mount, null))
                return;

            mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
            for (int index = activeMounts.Count - 1; index >= 0; index--)
            {
                if (ReferenceEquals(activeMounts[index], mount))
                    activeMounts.RemoveAt(index);
            }
        }

        internal void BindOwnerRuntime(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            if (ownerRenderer == null)
                return;

            PruneDestroyedEntries();
            if (!CacheOwnerRuntimeBinding(ownerRenderer, runtimeHandle))
                return;

            // EntityModel is mounted on the renderer itself. Bind it directly as well as
            // through ActiveMounts so a pool activation that misses OnEnable registration
            // cannot retain an invalid handle after the logic entity is registered.
            BattleCentralPresentationMount ownerMount =
                ownerRenderer.GetComponent<BattleCentralPresentationMount>();
            if (ownerMount != null)
            {
                ownerMount.SetRuntimeHandle(ownerMount.isActiveAndEnabled &&
                                            IsConfigurationValid(ownerMount)
                    ? runtimeHandle
                    : RuntimeEntityHandle.Invalid);
            }

            for (int index = 0; index < activeMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = activeMounts[index];
                if (mount.OwnerRenderer == ownerRenderer)
                    mount.SetRuntimeHandle(IsConfigurationValid(mount)
                        ? runtimeHandle
                        : RuntimeEntityHandle.Invalid);
            }
        }

        internal void ResetOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            BindOwnerRuntime(ownerRenderer, RuntimeEntityHandle.Invalid);
        }

        internal void RemoveOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            if (ReferenceEquals(ownerRenderer, null))
                return;

            int instanceId = ownerRenderer.GetInstanceID();
            if (ownerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) &&
                ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                ownerRuntimeBindings.Remove(instanceId);
                RemoveOwnerInstanceId(instanceId);
            }

            for (int index = activeMounts.Count - 1; index >= 0; index--)
            {
                BattleCentralPresentationMount mount = activeMounts[index];
                if (mount == null)
                {
                    activeMounts.RemoveAt(index);
                    continue;
                }

                if (ReferenceEquals(mount.OwnerRenderer, ownerRenderer))
                {
                    mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
                }
            }
        }

        internal int OwnerRuntimeBindingCountForSelfCheck => ownerRuntimeBindings.Count;

        internal bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return ownerRuntimeBindings.ContainsKey(ownerInstanceId);
        }

        internal bool HasValidOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return ownerRuntimeBindings.TryGetValue(ownerInstanceId, out OwnerRuntimeBinding binding) &&
                   binding.OwnerRenderer != null &&
                   binding.Handle.IsValid;
        }

        internal bool HasOwnerRuntimeBindingForSelfCheck(int ownerInstanceId)
        {
            return HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }

        internal void ResetAllRuntimeBindings()
        {
            PruneDestroyedEntries();
            for (int index = 0; index < ownerInstanceIds.Count; index++)
            {
                int ownerInstanceId = ownerInstanceIds[index];
                if (!ownerRuntimeBindings.TryGetValue(
                        ownerInstanceId,
                        out OwnerRuntimeBinding binding))
                {
                    continue;
                }

                binding.Handle = RuntimeEntityHandle.Invalid;
                ownerRuntimeBindings[ownerInstanceId] = binding;
            }
            for (int index = 0; index < activeMounts.Count; index++)
                activeMounts[index].SetRuntimeHandle(RuntimeEntityHandle.Invalid);
        }

        internal void ValidateActiveMounts()
        {
            if (!ValidateActiveMounts(out string error))
                throw new InvalidOperationException(error);
        }

        internal bool ValidateActiveMounts(out string error)
        {
            PruneDestroyedEntries();
            var seenOwnerPurposes = new HashSet<OwnerPurposeKey>();
            for (int index = 0; index < activeMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = activeMounts[index];
                if (!IsConfigurationValid(mount))
                {
                    error = DescribeInvalidConfiguration(mount);
                    return false;
                }

                var key = new OwnerPurposeKey(mount.OwnerRenderer, mount.Purpose);
                if (!seenOwnerPurposes.Add(key))
                {
                    error = Describe(mount, "duplicates another active owner/purpose mount.");
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsRolePurposePairValid(
            BattleCentralPresentationMountRole role,
            BattleCentralPresentationMountPurpose purpose)
        {
            return (role == BattleCentralPresentationMountRole.EntityModel &&
                    purpose == BattleCentralPresentationMountPurpose.EntitySprite) ||
                   (role == BattleCentralPresentationMountRole.Shadow &&
                    purpose == BattleCentralPresentationMountPurpose.CommonShadow);
        }

        private static bool IsMountedOnExpectedNode(BattleCentralPresentationMount mount)
        {
            if (mount.Role == BattleCentralPresentationMountRole.EntityModel)
                return mount.gameObject == mount.OwnerRenderer.gameObject;

            return mount.transform.parent != null &&
                   mount.transform.parent == mount.OwnerRenderer.transform.parent;
        }

        private static bool IsConfigurationValid(BattleCentralPresentationMount mount)
        {
            return mount != null && mount.OwnerRenderer != null &&
                   IsRolePurposePairValid(mount.Role, mount.Purpose) &&
                   IsMountedOnExpectedNode(mount);
        }

        private static string DescribeInvalidConfiguration(BattleCentralPresentationMount mount)
        {
            if (mount.OwnerRenderer == null)
                return Describe(mount, "has no owner LF2ObjectRenderer.");
            if (!IsRolePurposePairValid(mount.Role, mount.Purpose))
                return Describe(mount, "has an invalid role/purpose pairing.");
            return Describe(mount, "is mounted on the wrong node for its role.");
        }

        private static string Describe(BattleCentralPresentationMount mount, string detail)
        {
            string name = mount != null ? mount.name : "<destroyed mount>";
            return $"Battle central presentation mount '{name}' {detail}";
        }

        private bool CacheOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            int instanceId = ownerRenderer.GetInstanceID();
            bool hasExistingBinding = ownerRuntimeBindings.TryGetValue(
                instanceId,
                out OwnerRuntimeBinding binding);
            if (hasExistingBinding && ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                binding.Handle = runtimeHandle;
                ownerRuntimeBindings[instanceId] = binding;
                return true;
            }

            if (hasExistingBinding)
            {
                ownerRuntimeBindings[instanceId] =
                    new OwnerRuntimeBinding(ownerRenderer, runtimeHandle);
                return true;
            }

            if (capacitySealed && ownerRuntimeBindings.Count >= preparedOwnerCapacity)
            {
                RejectedOwnerBindingCount++;
                return false;
            }

            ownerRuntimeBindings[instanceId] =
                new OwnerRuntimeBinding(ownerRenderer, runtimeHandle);
            ownerInstanceIds.Add(instanceId);
            return true;
        }

        private bool TryGetOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            out RuntimeEntityHandle runtimeHandle)
        {
            runtimeHandle = RuntimeEntityHandle.Invalid;
            if (ownerRenderer == null)
                return false;

            int instanceId = ownerRenderer.GetInstanceID();
            if (!ownerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) ||
                !ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                return false;
            }

            runtimeHandle = binding.Handle;
            return runtimeHandle.IsValid;
        }

        private void PruneDestroyedEntries()
        {
            for (int index = activeMounts.Count - 1; index >= 0; index--)
            {
                if (activeMounts[index] == null)
                    activeMounts.RemoveAt(index);
            }

            for (int index = ownerInstanceIds.Count - 1; index >= 0; index--)
            {
                int instanceId = ownerInstanceIds[index];
                if (ownerRuntimeBindings.TryGetValue(
                        instanceId,
                        out OwnerRuntimeBinding binding) &&
                    binding.OwnerRenderer != null)
                {
                    continue;
                }

                ownerRuntimeBindings.Remove(instanceId);
                RemoveOwnerInstanceIdAt(index);
            }
        }

        private void RemoveOwnerInstanceId(int instanceId)
        {
            for (int index = ownerInstanceIds.Count - 1; index >= 0; index--)
            {
                if (ownerInstanceIds[index] != instanceId)
                    continue;

                RemoveOwnerInstanceIdAt(index);
                return;
            }
        }

        private void RemoveOwnerInstanceIdAt(int index)
        {
            int lastIndex = ownerInstanceIds.Count - 1;
            ownerInstanceIds[index] = ownerInstanceIds[lastIndex];
            ownerInstanceIds.RemoveAt(lastIndex);
        }

        private struct OwnerRuntimeBinding
        {
            public OwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer, RuntimeEntityHandle handle)
            {
                OwnerRenderer = ownerRenderer;
                Handle = handle;
            }

            public LF2ObjectRenderer OwnerRenderer { get; private set; }
            public RuntimeEntityHandle Handle { get; set; }
        }

        private readonly struct OwnerPurposeKey : IEquatable<OwnerPurposeKey>
        {
            private readonly LF2ObjectRenderer ownerRenderer;
            private readonly BattleCentralPresentationMountPurpose purpose;

            public OwnerPurposeKey(
                LF2ObjectRenderer ownerRenderer,
                BattleCentralPresentationMountPurpose purpose)
            {
                this.ownerRenderer = ownerRenderer;
                this.purpose = purpose;
            }

            public bool Equals(OwnerPurposeKey other)
            {
                return ownerRenderer == other.ownerRenderer && purpose == other.purpose;
            }

            public override bool Equals(object obj)
            {
                return obj is OwnerPurposeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((ownerRenderer != null ? ownerRenderer.GetInstanceID() : 0) * 397) ^
                           (int)purpose;
                }
            }
        }
    }

    /// <summary>Read-only bridge for editor acceptance evidence.</summary>
    public static class BattleCentralPresentationMountDiagnostics
    {
        public static bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return BattleCentralPresentationMountRegistry
                .HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }

        public static bool HasValidOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return BattleCentralPresentationMountRegistry
                .HasValidOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }
    }
}
