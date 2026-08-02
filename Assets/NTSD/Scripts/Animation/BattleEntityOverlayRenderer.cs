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

        private void OnDisable()
        {
            ReleaseActiveRenderers();
        }

        private void OnDestroy()
        {
            ReleaseActiveRenderers();
        }

        public void RenderAll(SimulationWorld world)
        {
            ReleaseActiveRenderers();
            if (world == null || BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(world))
                return;

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            LF2ObjectPool pool = LF2ObjectPool.Instance;
            if (frame == null || pool == null)
                return;

            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.OverlayGlyph ||
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
