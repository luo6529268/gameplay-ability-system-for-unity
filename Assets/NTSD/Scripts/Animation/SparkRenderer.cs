using System.Collections.Generic;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.Animation.Rendering;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 命中闪光渲染器
    ///
    /// 对应权威 C# host renderer 的 SPARK blit 逻辑。
    ///
    /// C# Host DrawHitRecords 按 HitRecordDamage 选取 SPARK.bmp 的 20 个图块，
    /// 成功绘制后立即递增 age；无效 age 只在该 slot 是最后一个时回收。
    /// </summary>
    public class SparkRenderer : MonoBehaviour
    {
        private const int SparkFrameCount = 20;

        // ========== 内部状态 ==========
        private Texture2D _sparkTex;
        private Sprite[] _sparkSprites = new Sprite[SparkFrameCount];
        private BattleCommonVisualCatalog _boundCommonCatalog = BattleCommonVisualCatalog.Empty;
        private bool _loaded = false;

        private readonly List<SpriteRenderer> _activeThisFrame = new List<SpriteRenderer>(32);
        private readonly List<LF2ObjectPool> _activePools = new List<LF2ObjectPool>(32);

        // ========== Unity 生命周期 ==========
        private void Awake()
        {
            _loaded = false;
        }

        private void OnDisable()
        {
            ReleaseActiveRenderers();
            ReleaseCatalogBinding();
        }

        private void OnDestroy()
        {
            ReleaseActiveRenderers();
            ReleaseCatalogBinding();
        }

        // ========== 公共 API ==========

        public void RenderAll(SimulationWorld world)
        {
            ReleaseActiveRenderers();

            if (world == null) { Debug.LogWarning("[SparkRenderer] world is null"); return; }
            if (BattleCentralRenderSystem.ShouldUseCentralPixels(world))
                return;

            var pool = LF2ObjectPool.Instance;
            BattleHitRecordPresentationCycle cycle =
                world.BattlePresentation.PublishedHitRecordCycle;
            if (cycle == null)
            {
                world.BattlePresentation.CompleteLegacyFrame();
                return;
            }

            RefreshCommonPublication(cycle.CommonVisualCatalog);
            if (!_loaded)
            {
                Debug.LogWarning("[SparkRenderer] not loaded");
                world.BattlePresentation.CompleteLegacyFrame();
                return;
            }

            for (int ownerIndex = 0; ownerIndex < cycle.OwnerCount; ownerIndex++)
            {
                BattleHitRecordOwnerSnapshot owner = cycle.GetOwner(ownerIndex);
                RenderObjectSlots(cycle, owner, pool, world);
            }
            world.BattlePresentation.CompleteLegacyFrame();
        }

        // ========== 私有实现 ==========

        /// <summary>
        /// Binds the manager's immutable common publication. No BMP decode or
        /// Sprite.Create occurs on this renderer path.
        /// </summary>
        private void RefreshCommonPublication(BattleCommonVisualCatalog catalog)
        {
            catalog ??= BattleCommonVisualCatalog.Empty;
            if (catalog.IsSparkValid)
            {
                if (ReferenceEquals(catalog, _boundCommonCatalog) &&
                    _sparkSprites != null && _sparkSprites.Length == SparkFrameCount)
                {
                    _loaded = true;
                    return;
                }

                ReleaseCatalogBinding();
                _boundCommonCatalog = catalog;
                for (int pic = 0; pic < SparkFrameCount; pic++)
                {
                    catalog.TryGetSpark(pic, out BattleCommonVisualBinding binding);
                    _sparkSprites[pic] = binding?.Sprite;
                    if (pic == 0)
                        _sparkTex = binding?.Texture;
                }
                _loaded = _sparkTex != null;
                return;
            }

            ReleaseCatalogBinding();
        }

        private void ReleaseCatalogBinding()
        {
            _boundCommonCatalog = BattleCommonVisualCatalog.Empty;
            _sparkTex = null;
            if (_sparkSprites == null || _sparkSprites.Length != SparkFrameCount)
                _sparkSprites = new Sprite[SparkFrameCount];
            else
                System.Array.Clear(_sparkSprites, 0, _sparkSprites.Length);
            _loaded = false;

        }

        private void RenderObjectSlots(
            BattleHitRecordPresentationCycle cycle,
            in BattleHitRecordOwnerSnapshot owner,
            LF2ObjectPool pool,
            SimulationWorld world)
        {
            for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
            {
                BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                    owner.HitRecordStart + hitIndex);
                if (!BattleCommonVisualCatalog.TryResolveSparkAge(hit.Age, out int pic) ||
                    !cycle.CommonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding binding))
                {
                    continue;
                }

                Sprite sprite = GetSpriteForAge(hit.Age);
                if (sprite == null)
                    continue;

                float screenX = hit.AnchorX + owner.RenderOffsetX - owner.CameraX;
                float screenY = hit.AnchorZ;
                Vector3 unityPos = NTSDRenderSpace.ScreenPixelToWorld(screenX, screenY, 0f);

                SpriteRenderer sr = pool?.GetSprite();
                if (sr == null)
                    continue;

                sr.sprite = sprite;
                if (binding.Material != null)
                    sr.sharedMaterial = binding.Material;
                sr.color = binding.Color;
                sr.flipX = binding.RenderState.FlipX;
                sr.flipY = binding.RenderState.FlipY;
                sr.maskInteraction = binding.RenderState.MaskInteraction;
                sr.transform.position = unityPos;
                sr.transform.localScale = NTSDRenderSpace.RenderScale;
                sr.sortingLayerName = "Object";
                sr.sortingOrder = owner.PresentationBaseOrder +
                                  SimulationWorld.PresentationHitRecordSubOrder;
                _activeThisFrame.Add(sr);
                _activePools.Add(pool);
                world.BattlePresentation.RecordLegacyHitRecordProbe(
                    owner,
                    sr,
                    hitIndex,
                    binding);
            }
        }

        private void ReleaseActiveRenderers()
        {
            for (int i = 0; i < _activeThisFrame.Count; i++)
            {
                SpriteRenderer renderer = _activeThisFrame[i];
                LF2ObjectPool ownerPool = i < _activePools.Count ? _activePools[i] : null;
                if (ownerPool != null)
                {
                    ownerPool.ReleaseSprite(renderer);
                }
                else if (renderer != null)
                {
                    renderer.sprite = null;
                    renderer.gameObject.SetActive(false);
                }
            }

            _activeThisFrame.Clear();
            _activePools.Clear();
        }

        private Sprite GetSpriteForAge(int age)
        {
            if (!BattleCommonVisualCatalog.TryResolveSparkAge(age, out int pic))
                return null;

            if (pic >= 0 && _sparkSprites != null && pic < _sparkSprites.Length)
                return _sparkSprites[pic];
            return null;
        }
    }
}
