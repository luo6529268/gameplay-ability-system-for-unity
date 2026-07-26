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
        private static readonly List<BattleCentralPresentationMount> ActiveMounts =
            new List<BattleCentralPresentationMount>(32);

        private static readonly Dictionary<int, OwnerRuntimeBinding> OwnerRuntimeBindings =
            new Dictionary<int, OwnerRuntimeBinding>(32);

        private static readonly List<int> StaleOwnerInstanceIds = new List<int>(8);

        internal static void Register(BattleCentralPresentationMount mount)
        {
            if (mount == null || ActiveMounts.Contains(mount))
                return;

            PruneDestroyedEntries();
            ActiveMounts.Add(mount);
            if (IsConfigurationValid(mount) &&
                TryGetOwnerRuntimeBinding(mount.OwnerRenderer, out RuntimeEntityHandle runtimeHandle))
            {
                mount.SetRuntimeHandle(runtimeHandle);
            }
        }

        internal static void Unregister(BattleCentralPresentationMount mount)
        {
            if (ReferenceEquals(mount, null))
                return;

            mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                if (ReferenceEquals(ActiveMounts[index], mount))
                    ActiveMounts.RemoveAt(index);
            }
        }

        internal static void BindOwnerRuntime(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            if (ownerRenderer == null)
                return;

            PruneDestroyedEntries();
            CacheOwnerRuntimeBinding(ownerRenderer, runtimeHandle);

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

            for (int index = 0; index < ActiveMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
                if (mount.OwnerRenderer == ownerRenderer)
                    mount.SetRuntimeHandle(IsConfigurationValid(mount)
                        ? runtimeHandle
                        : RuntimeEntityHandle.Invalid);
            }
        }

        internal static void ResetOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            BindOwnerRuntime(ownerRenderer, RuntimeEntityHandle.Invalid);
        }

        internal static void RemoveOwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer)
        {
            if (ReferenceEquals(ownerRenderer, null))
                return;

            int instanceId = ownerRenderer.GetInstanceID();
            if (OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) &&
                ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                OwnerRuntimeBindings.Remove(instanceId);
            }

            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
                if (mount == null)
                {
                    ActiveMounts.RemoveAt(index);
                    continue;
                }

                if (ReferenceEquals(mount.OwnerRenderer, ownerRenderer))
                {
                    mount.SetRuntimeHandle(RuntimeEntityHandle.Invalid);
                }
            }
        }

        internal static int OwnerRuntimeBindingCountForSelfCheck => OwnerRuntimeBindings.Count;

        internal static bool HasOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return OwnerRuntimeBindings.ContainsKey(ownerInstanceId);
        }

        internal static bool HasValidOwnerRuntimeBindingForAcceptance(int ownerInstanceId)
        {
            return OwnerRuntimeBindings.TryGetValue(ownerInstanceId, out OwnerRuntimeBinding binding) &&
                   binding.OwnerRenderer != null &&
                   binding.Handle.IsValid;
        }

        internal static bool HasOwnerRuntimeBindingForSelfCheck(int ownerInstanceId)
        {
            return HasOwnerRuntimeBindingForAcceptance(ownerInstanceId);
        }

        internal static void ResetAllRuntimeBindings()
        {
            PruneDestroyedEntries();
            foreach (int ownerInstanceId in OwnerRuntimeBindings.Keys)
                OwnerRuntimeBindings[ownerInstanceId].Handle = RuntimeEntityHandle.Invalid;
            for (int index = 0; index < ActiveMounts.Count; index++)
                ActiveMounts[index].SetRuntimeHandle(RuntimeEntityHandle.Invalid);
        }

        internal static void ValidateActiveMounts()
        {
            if (!ValidateActiveMounts(out string error))
                throw new InvalidOperationException(error);
        }

        internal static bool ValidateActiveMounts(out string error)
        {
            PruneDestroyedEntries();
            var seenOwnerPurposes = new HashSet<OwnerPurposeKey>();
            for (int index = 0; index < ActiveMounts.Count; index++)
            {
                BattleCentralPresentationMount mount = ActiveMounts[index];
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

        private static void CacheOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            RuntimeEntityHandle runtimeHandle)
        {
            int instanceId = ownerRenderer.GetInstanceID();
            if (OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) &&
                ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                binding.Handle = runtimeHandle;
                return;
            }

            OwnerRuntimeBindings[instanceId] = new OwnerRuntimeBinding(ownerRenderer, runtimeHandle);
        }

        private static bool TryGetOwnerRuntimeBinding(
            LF2ObjectRenderer ownerRenderer,
            out RuntimeEntityHandle runtimeHandle)
        {
            runtimeHandle = RuntimeEntityHandle.Invalid;
            if (ownerRenderer == null)
                return false;

            int instanceId = ownerRenderer.GetInstanceID();
            if (!OwnerRuntimeBindings.TryGetValue(instanceId, out OwnerRuntimeBinding binding) ||
                !ReferenceEquals(binding.OwnerRenderer, ownerRenderer))
            {
                return false;
            }

            runtimeHandle = binding.Handle;
            return runtimeHandle.IsValid;
        }

        private static void PruneDestroyedEntries()
        {
            for (int index = ActiveMounts.Count - 1; index >= 0; index--)
            {
                if (ActiveMounts[index] == null)
                    ActiveMounts.RemoveAt(index);
            }

            StaleOwnerInstanceIds.Clear();
            foreach (KeyValuePair<int, OwnerRuntimeBinding> pair in OwnerRuntimeBindings)
            {
                if (pair.Value.OwnerRenderer == null)
                    StaleOwnerInstanceIds.Add(pair.Key);
            }

            for (int index = 0; index < StaleOwnerInstanceIds.Count; index++)
                OwnerRuntimeBindings.Remove(StaleOwnerInstanceIds[index]);
        }

        private sealed class OwnerRuntimeBinding
        {
            public OwnerRuntimeBinding(LF2ObjectRenderer ownerRenderer, RuntimeEntityHandle handle)
            {
                OwnerRenderer = ownerRenderer;
                Handle = handle;
            }

            public LF2ObjectRenderer OwnerRenderer { get; }
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
