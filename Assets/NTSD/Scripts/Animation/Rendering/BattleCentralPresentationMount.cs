using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    public enum BattleCentralPresentationMountRole : byte
    {
        EntityModel = 0,
        Shadow = 1,
    }

    public enum BattleCentralPresentationMountPurpose : byte
    {
        EntitySprite = 0,
        CommonShadow = 1,
    }

    /// <summary>
    /// Declares which pooled presentation node belongs to an LF2 runtime entity.
    /// It deliberately contains no rendering or simulation state beyond the runtime handle.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class BattleCentralPresentationMount : MonoBehaviour
    {
        [SerializeField] private BattleCentralPresentationMountRole role;
        [SerializeField] private BattleCentralPresentationMountPurpose purpose;
        [SerializeField] private LF2ObjectRenderer ownerRenderer;

        private RuntimeEntityHandle runtimeHandle = RuntimeEntityHandle.Invalid;
        private readonly List<LF2ObjectRenderer> ownerResolveScratch =
            new List<LF2ObjectRenderer>(2);

        public BattleCentralPresentationMountRole Role => role;
        public BattleCentralPresentationMountPurpose Purpose => purpose;
        public LF2ObjectRenderer OwnerRenderer
        {
            get
            {
                if (ownerRenderer == null && gameObject.scene.IsValid())
                    ResolveOwnerRendererIfMissing();

                return ownerRenderer;
            }
        }
        public RuntimeEntityHandle RuntimeHandle => runtimeHandle;

        private void Awake()
        {
            if (gameObject.scene.IsValid())
                ResolveOwnerRendererIfMissing();
        }

        private void OnEnable()
        {
            if (!gameObject.scene.IsValid())
                return;

            ResolveOwnerRendererIfMissing();
            BattleCentralPresentationMountRegistry.Register(this);
        }

        private void OnDisable()
        {
            if (!gameObject.scene.IsValid())
                return;

            BattleCentralPresentationMountRegistry.Unregister(this);
        }

        private void OnDestroy()
        {
            LF2ObjectRenderer cachedOwner = ownerRenderer;
            BattleCentralPresentationMountRegistry.Unregister(this);
            if (role == BattleCentralPresentationMountRole.EntityModel &&
                !ReferenceEquals(cachedOwner, null))
            {
                BattleCentralPresentationMountRegistry.RemoveOwnerRuntimeBinding(cachedOwner);
            }
        }

        internal void SetRuntimeHandle(RuntimeEntityHandle handle)
        {
            runtimeHandle = handle;
        }

        private void ResolveOwnerRendererIfMissing()
        {
            if (ownerRenderer != null)
                return;

            if (role == BattleCentralPresentationMountRole.EntityModel)
            {
                ownerRenderer = GetComponent<LF2ObjectRenderer>();
                return;
            }

            if (role != BattleCentralPresentationMountRole.Shadow || transform.parent == null)
                return;

            LF2ObjectRenderer candidate = null;
            Transform directParent = transform.parent;
            for (int childIndex = 0; childIndex < directParent.childCount; childIndex++)
            {
                ownerResolveScratch.Clear();
                directParent.GetChild(childIndex).GetComponents(ownerResolveScratch);
                for (int rendererIndex = 0; rendererIndex < ownerResolveScratch.Count; rendererIndex++)
                {
                    if (candidate != null)
                    {
                        ownerResolveScratch.Clear();
                        return;
                    }

                    candidate = ownerResolveScratch[rendererIndex];
                }
            }

            ownerResolveScratch.Clear();
            ownerRenderer = candidate;
        }

        internal void ConfigureForSelfCheck(
            BattleCentralPresentationMountRole configuredRole,
            BattleCentralPresentationMountPurpose configuredPurpose,
            LF2ObjectRenderer configuredOwner)
        {
            role = configuredRole;
            purpose = configuredPurpose;
            ownerRenderer = configuredOwner;
        }

        internal void ConfigureRuntimeFallback(
            BattleCentralPresentationMountRole configuredRole,
            BattleCentralPresentationMountPurpose configuredPurpose,
            LF2ObjectRenderer configuredOwner)
        {
            role = configuredRole;
            purpose = configuredPurpose;
            ownerRenderer = configuredOwner;
        }
    }
}
