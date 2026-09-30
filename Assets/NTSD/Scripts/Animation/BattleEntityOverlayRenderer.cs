using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// Materializes the published entity-overlay commands on the legacy sprite path.
    /// </summary>
    public sealed class BattleEntityOverlayRenderer : MonoBehaviour
    {
        private readonly List<SpriteRenderer> activeRenderers = new List<SpriteRenderer>(32);
        private readonly List<SpriteRenderer> preparedBleedMarkers = new List<SpriteRenderer>();
        private readonly BattlePresentationDisplayMotion bleedDisplayMotion =
            new BattlePresentationDisplayMotion();
        private Sprite bleedSprite;
        private int activeBleedMarkerCount;

        public int RejectedBleedMarkCountForDiagnostics { get; private set; }

        private void OnDisable()
        {
            StopForBattleShutdown();
        }

        private void OnDestroy()
        {
            StopForBattleShutdown();
            if (bleedSprite != null)
            {
                if (Application.isPlaying)
                    Destroy(bleedSprite);
                else
                    DestroyImmediate(bleedSprite);
                bleedSprite = null;
            }
        }

        internal void PrepareCapacity(int commandCapacity)
        {
            if (commandCapacity > activeRenderers.Capacity)
                activeRenderers.Capacity = commandCapacity;
        }

        internal void PrepareBattleCapacity(int runtimeSlotCapacity)
        {
            int capacity = Mathf.Max(0, runtimeSlotCapacity);
            bleedDisplayMotion.PrepareCapacity(capacity);
            if (bleedSprite == null && capacity > 0)
            {
                bleedSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0f, 1f),
                    SimulationConstants.PIXELS_PER_UNIT);
                if (bleedSprite == null)
                    throw new System.InvalidOperationException("Legacy bleed solid sprite preparation failed.");
            }

            while (preparedBleedMarkers.Count < capacity)
            {
                var markerObject = new GameObject("BattleBleedMark");
                markerObject.layer = LayerMask.NameToLayer("Battle");
                markerObject.transform.SetParent(transform, false);
                SpriteRenderer marker = markerObject.AddComponent<SpriteRenderer>();
                marker.sprite = bleedSprite;
                marker.sortingLayerName = "Object";
                markerObject.SetActive(false);
                preparedBleedMarkers.Add(marker);
            }
            RejectedBleedMarkCountForDiagnostics = 0;
        }

        internal void StopForBattleShutdown()
        {
            ReleaseActiveRenderers();
            HideActiveBleedMarkers();
        }

        public void RenderAll(SimulationWorld world)
        {
            ReleaseActiveRenderers();
            HideActiveBleedMarkers();
            if (world == null || BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(world))
                return;

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            if (frame == null)
                return;
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            bool bleedMotionPrepared = false;

            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type == BattleRenderCommandType.BleedMark)
                {
                    if (!world.TryResolveRuntimeHandle(command.Handle, out LF2Entity markEntity) ||
                        markEntity == null || markEntity.StableId != command.StableId)
                        continue;

                    if (activeBleedMarkerCount >= preparedBleedMarkers.Count)
                    {
                        RejectedBleedMarkCountForDiagnostics++;
                        if (RejectedBleedMarkCountForDiagnostics == 1)
                            Debug.LogError("Legacy bleed mark capacity was exceeded by the published content.");
                        continue;
                    }

                    if (!bleedMotionPrepared)
                    {
                        bleedDisplayMotion.Prepare(
                            frame,
                            BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world),
                            world.FixedViewRunDistanceScale,
                            world.FixedViewRunVerticalDistanceScale);
                        bleedMotionPrepared = true;
                    }

                    SpriteRenderer marker = preparedBleedMarkers[activeBleedMarkerCount++];
                    marker.color = command.Color;
                    marker.sortingLayerID = command.SortingLayerId;
                    marker.sortingOrder = command.SortOrder;
                    Vector3 position = command.Position;
                    if (bleedDisplayMotion.TryGet(command.Handle,
                            out BattlePresentationMotionDelta delta))
                        position += BattlePresentationDisplayMotion.ToWorldBody(delta);
                    marker.transform.position = position;
                    marker.transform.localScale = new Vector3(
                        command.Size.x * NTSDRenderSpace.BattleVisualScale,
                        command.Size.y * NTSDRenderSpace.BattleVisualScale,
                        1f);
                    marker.gameObject.SetActive(true);
                    continue;
                }

                if (command.Type != BattleRenderCommandType.OverlayGlyph ||
                    pool == null ||
                    !world.TryResolveRuntimeHandle(command.Handle, out LF2Entity entity) ||
                    entity == null || entity.StableId != command.StableId ||
                    !frame.CommonVisualCatalog.TryGetWordGlyph(
                        command.VisualDataId,
                        command.EffectivePic,
                        out BattleCommonVisualBinding binding) ||
                    !MatchesCommandBinding(in command, binding))
                {
                    continue;
                }

                SpriteRenderer renderer = pool.GetSprite();
                if (renderer == null)
                    continue;

                renderer.sprite = binding.Sprite;
                if (binding.Material != null)
                    renderer.sharedMaterial = binding.Material;
                renderer.color = binding.Color;
                renderer.flipX = binding.RenderState.FlipX;
                renderer.flipY = binding.RenderState.FlipY;
                renderer.maskInteraction = binding.RenderState.MaskInteraction;
                renderer.transform.position = command.Position;
                renderer.transform.localScale = NTSDRenderSpace.RenderScale;
                renderer.sortingLayerName = "Object";
                renderer.sortingOrder = command.SortOrder;
                activeRenderers.Add(renderer);
                world.BattlePresentation.RecordLegacyOverlayProbe(in command, renderer, binding);
            }
        }

        private void ReleaseActiveRenderers()
        {
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            for (int index = 0; index < activeRenderers.Count; index++)
            {
                SpriteRenderer renderer = activeRenderers[index];
                if (pool != null && pool.IsRuntimeStateValidForAcceptance)
                    pool.ReleaseSprite(renderer);
                else if (renderer != null)
                {
                    renderer.sprite = null;
                    renderer.gameObject.SetActive(false);
                }
            }

            activeRenderers.Clear();
        }

        private void HideActiveBleedMarkers()
        {
            for (int index = 0; index < activeBleedMarkerCount; index++)
            {
                SpriteRenderer marker = preparedBleedMarkers[index];
                if (marker != null)
                    marker.gameObject.SetActive(false);
            }
            activeBleedMarkerCount = 0;
        }

        private static bool MatchesCommandBinding(
            in BattleRenderCommand command,
            BattleCommonVisualBinding binding)
        {
            BattleSpriteValueDescriptor descriptor = command.SpriteDescriptor;
            return binding != null &&
                   descriptor.HasLogicalResourceKey &&
                   descriptor.LogicalResourceKey == binding.Key &&
                   binding.MatchesCommand(descriptor) &&
                   descriptor.MaterialInstanceId == binding.MaterialInstanceId;
        }
    }
}
