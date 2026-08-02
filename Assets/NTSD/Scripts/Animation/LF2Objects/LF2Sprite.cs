using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 精灵动画模块，封装 Unity SpriteRenderer 操作。
    /// 当前模块只负责 Unity 渲染适配，不作为战斗逻辑复刻依据。
    /// </summary>
    public class LF2Sprite
    {
        private SpriteRenderer _renderer;
        private List<Sprite> _sprites;
        private BattleSpriteCatalog _catalog;
        private int _visualDataId = int.MinValue;
        private BattleSpriteEntry _currentEntry;
        private int _currentPic = 999;
        private string _dir = "right";

        private SpriteRenderer _shadowRenderer;
        private bool _hasShadow;
        private bool _entityVisible = true;
        private bool _shadowVisible = true;
        private bool _presentationSuppressed;
        private bool _legacyRendererSuppressed;
        private bool _legacyEntityVisible = true;
        private Vector2 _localOffsetPixels;

        // SortingGroup 用于角色根节点，优先控制层级；武器/SA 无 SortingGroup 则回退到 SpriteRenderer.sortingOrder
        private SortingGroup _sortingGroup;

        /// <summary>
        /// 当前方向
        /// </summary>
        public string Dir => _dir;

        public bool EntityVisible => _entityVisible;

        public bool ShadowVisible => _shadowVisible;

        public bool PresentationSuppressed => _presentationSuppressed;

        public Vector2 LocalOffsetPixels => _localOffsetPixels;

        public int CurrentPic => _currentPic;

        private int _startFrame;

        /// <summary>
        /// 初始化精灵模块
        /// </summary>
        /// <param name="renderer">SpriteRenderer 组件引用</param>
        /// <param name="sprites">精灵列表</param>
        /// <param name="startFrame">精灵列表中的起始偏移（对应 SpriteFileInfo.startFrame）</param>
        public void Initialize(
            SpriteRenderer renderer,
            List<Sprite> sprites,
            int startFrame = 0,
            BattleSpriteCatalog catalog = null,
            int visualDataId = int.MinValue)
        {
            _renderer = renderer;
            _sprites = sprites;
            _startFrame = startFrame;
            _catalog = catalog;
            _visualDataId = visualDataId;
            _currentEntry = null;
            _currentPic = 999;
            _dir = "right";
            _entityVisible = true;
            _shadowVisible = true;
            _presentationSuppressed = false;
            _legacyRendererSuppressed = false;
            _legacyEntityVisible = true;
            _localOffsetPixels = Vector2.zero;

            // 从根节点查找 SortingGroup（角色有，武器/SA 无）
            _sortingGroup = renderer != null
                ? renderer.GetComponentInParent<SortingGroup>()
                : null;

            if (_renderer != null)
            {
                _renderer.sortingLayerName = "Object";
                _renderer.color = Color.white;
                _renderer.sprite = null;
                _renderer.flipX = false;
                Vector3 localPosition = _renderer.transform.localPosition;
                _renderer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
                ApplyEntityRendererVisibility();
            }
            if(_sortingGroup != null)
                _sortingGroup.sortingLayerName = "Object";

        }

        /// <summary>
        /// 初始化阴影渲染器。
        /// </summary>
        public void InitializeShadow(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _hasShadow = shadowRenderer != null;
            if (_shadowRenderer != null)
            {
                _shadowRenderer.sortingLayerName = "Object";
                ApplyShadowRendererVisibility();
            }
        }

        public bool HasShadow => _hasShadow;

        /// <summary>
        /// 更新精灵列表（用于运行时切换角色）
        /// </summary>
        public void SetSprites(List<Sprite> sprites, int startFrame = 0)
        {
            SetSpritesManagedOnly(sprites, startFrame);

            if (sprites == null)
                ClearResolvedRendererSprite();
        }

        internal void SetSpritesManagedOnly(List<Sprite> sprites, int startFrame = 0)
        {
            _sprites = sprites;
            _startFrame = startFrame;

            if (sprites == null)
                ClearCurrentSpriteManagedOnly();
        }

        public void SetCatalogBinding(BattleSpriteCatalog catalog, int visualDataId)
        {
            SetCatalogBindingManagedOnly(catalog, visualDataId);
            ClearResolvedRendererSprite();
        }

        internal void SetCatalogBindingManagedOnly(BattleSpriteCatalog catalog, int visualDataId)
        {
            _catalog = catalog;
            _visualDataId = visualDataId;
            _currentEntry = null;
            ClearCurrentSpriteManagedOnly();
        }

        /// <summary>
        /// 显示指定图片。
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        public bool HasRenderer => _renderer != null;

        public void ShowPic(int picIndex)
        {
            Sprite resolvedSprite = ResolvePicManagedOnly(picIndex);
            if (_renderer == null)
                return;

            _renderer.sprite = resolvedSprite;
            if (resolvedSprite == null)
            {
                _renderer.enabled = false;
                return;
            }

            ApplyEntityRendererVisibility();
        }

        internal void ShowPicManagedOnly(int picIndex)
        {
            ResolvePicManagedOnly(picIndex);
        }

        private Sprite ResolvePicManagedOnly(int picIndex)
        {
            _currentPic = picIndex;
            if (picIndex == 999)
            {
                ClearCurrentSpriteManagedOnly();
                return null;
            }

            if (_catalog != null)
            {
                if (!_catalog.TryGet(_visualDataId, picIndex, out BattleSpriteEntry entry) ||
                    entry == null)
                {
                    ClearResolvedSpriteManagedOnly();
                    return null;
                }

                _currentEntry = entry;
                _entityVisible = true;
                return entry.LegacySprite;
            }

            // Editor previews and isolated legacy tests may still bind only a
            // sprite list. Production battle renderers always bind the catalog.
            if (_sprites == null)
            {
                ClearResolvedSpriteManagedOnly();
                return null;
            }

            // 运行时 MergedSprites 已按绝对 pic 编号展开；正常路径直接用 picIndex 取图。
            // 仅在传入的是局部表索引时，才回退到 startFrame 偏移，避免把 oid=999 等多文件对象二次偏移。
            int actualIndex = picIndex;
            if ((actualIndex < 0 || actualIndex >= _sprites.Count || _sprites[actualIndex] == null) &&
                _startFrame != 0)
            {
                actualIndex = _startFrame + picIndex;
            }

            if (actualIndex < 0 || actualIndex >= _sprites.Count)
            {
                ClearResolvedSpriteManagedOnly();
                return null;
            }
            if (_sprites[actualIndex] == null)
            {
                ClearResolvedSpriteManagedOnly();
                return null;
            }

            _currentEntry = null;
            _entityVisible = true;
            return _sprites[actualIndex];
        }

        public void ClearCurrentSprite()
        {
            ClearCurrentSpriteManagedOnly();
            ClearResolvedRendererSprite();
        }

        internal void ClearCurrentSpriteManagedOnly()
        {
            _currentPic = 999;
            ClearResolvedSpriteManagedOnly();
        }

        private void ClearResolvedSpriteManagedOnly()
        {
            _currentEntry = null;
        }

        private void ClearResolvedRendererSprite()
        {
            if (_renderer == null)
                return;

            _renderer.sprite = null;
            _renderer.enabled = false;
        }

        /// <summary>
        /// 切换左右方向。
        /// </summary>
        /// <param name="dir">"left" 或 "right"</param>
        public void SwitchLR(string dir)
        {
            SwitchLRManagedOnly(dir);
            if (_renderer != null)
            {
                _renderer.flipX = (dir == "left");
            }
        }

        internal void SwitchLRManagedOnly(string dir)
        {
            _dir = dir;
        }

        /// <summary>
        /// 设置本地显示位置。
        /// </summary>
        public void SetXY(float x, float y)
        {
            _localOffsetPixels = new Vector2(x, y);
            if (_renderer == null) return;
            const float ppu = 100f;
            _renderer.transform.localPosition = new Vector3(x / ppu, -y / ppu, _renderer.transform.localPosition.z);
        }

        /// <summary>
        /// 设置 Z 排序。
        /// 角色有 SortingGroup → 改 SortingGroup.sortingOrder（控制整个角色层级）
        /// 武器/SA 无 SortingGroup → 回退改 SpriteRenderer.sortingOrder
        /// </summary>
        public void SetZ(int order)
        {
            if (_sortingGroup != null)
            {
                _sortingGroup.sortingLayerName = "Object";
                _sortingGroup.sortingOrder = order;
            }

            if (_renderer != null)
            {
                _renderer.sortingLayerName = "Object";
                _renderer.sortingOrder = order;
            }
        }

        public void SetZ(float z)
        {
            SetZ((int)z);
        }

        /// <summary>
        /// 显示精灵。
        /// </summary>
        public void Show()
        {
            SetEntityVisibleManagedOnly(true);
            ApplyEntityRendererVisibility();
        }

        /// <summary>
        /// 隐藏精灵。
        /// </summary>
        public void Hide()
        {
            SetEntityVisibleManagedOnly(false);
            ApplyEntityRendererVisibility();
        }

        internal void SetEntityVisibleManagedOnly(bool visible)
        {
            _entityVisible = visible;
        }

        public void SetPresentationSuppressed(bool suppressed)
        {
            if (_presentationSuppressed == suppressed)
                return;

            _presentationSuppressed = suppressed;
            if (_legacyRendererSuppressed)
                return;

            ApplyEntityRendererVisibility();
            ApplyShadowRendererVisibility();
        }

        public void SetLegacyRendererSuppressed(bool suppressed)
        {
            if (_legacyRendererSuppressed == suppressed)
                return;

            _legacyRendererSuppressed = suppressed;
            ApplyEntityRendererVisibility();
            ApplyShadowRendererVisibility();
        }

        public void SetLegacyEntityVisible(bool visible)
        {
            SetLegacyEntityVisibleManagedOnly(visible);
            ApplyEntityRendererVisibility();
        }

        internal void SetLegacyEntityVisibleManagedOnly(bool visible)
        {
            _legacyEntityVisible = visible;
        }

        /// <summary>
        /// 显示阴影
        /// </summary>
        public void ShowShadow()
        {
            SetShadowVisibleManagedOnly(true);
            ApplyShadowRendererVisibility();
        }

        /// <summary>
        /// 隐藏阴影
        /// </summary>
        public void HideShadow()
        {
            SetShadowVisibleManagedOnly(false);
            ApplyShadowRendererVisibility();
        }

        internal void SetShadowVisibleManagedOnly(bool visible)
        {
            _shadowVisible = visible;
        }

        /// <summary>
        /// 更新阴影位置（阴影始终在地面）
        /// </summary>
        public void UpdateShadowPosition(float groundX, float groundZ)
        {
            if (_shadowRenderer == null) return;
            var t = _shadowRenderer.transform;
            t.localPosition = new Vector3(groundX, groundZ, t.localPosition.z);
        }

        /// <summary>
        /// 隐藏精灵和阴影。
        /// </summary>
        public void Destroy()
        {
            ClearCurrentSprite();
            Hide();
            HideShadow();
        }

        public void Reset()
        {
            _sprites = null;
            _catalog = null;
            _visualDataId = int.MinValue;
            _startFrame = 0;
            _currentPic = 999;
            _currentEntry = null;
            _dir = "right";
            _entityVisible = false;
            _shadowVisible = false;
            _presentationSuppressed = false;
            _legacyRendererSuppressed = false;
            _legacyEntityVisible = true;
            _localOffsetPixels = Vector2.zero;
            _sortingGroup = null;

            if (_renderer != null)
            {
                _renderer.sprite = null;
                _renderer.flipX = false;
                Vector3 localPosition = _renderer.transform.localPosition;
                _renderer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
                _renderer.enabled = false;
            }
            if (_shadowRenderer != null)
                _shadowRenderer.enabled = false;

            _renderer = null;
            _shadowRenderer = null;
            _hasShadow = false;
        }

        private void ApplyEntityRendererVisibility()
        {
            if (_renderer == null)
                return;

            _renderer.enabled = _entityVisible &&
                                _legacyEntityVisible &&
                                !_presentationSuppressed &&
                                !_legacyRendererSuppressed &&
                                _renderer.sprite != null;
        }

        private void ApplyShadowRendererVisibility()
        {
            if (_shadowRenderer == null)
                return;

            _shadowRenderer.enabled = _shadowVisible &&
                                      !_presentationSuppressed &&
                                      !_legacyRendererSuppressed;
        }

        /// <summary>
        /// 获取当前精灵宽度（像素）
        /// </summary>
        public float GetWidthPx()
        {
            return _currentEntry?.PixelWidth ?? 0f;
        }

        /// <summary>
        /// 获取当前精灵宽度（像素）- 别名，用于碰撞检测
        /// </summary>
        public float GetCurrentSpriteWidthPx() => GetWidthPx();

        /// <summary>
        /// 获取当前精灵高度（像素）
        /// </summary>
        public float GetHeightPx()
        {
            return _currentEntry?.PixelHeight ?? 0f;
        }

        public BattleSpriteEntry CurrentEntry => _currentEntry;
    }
}
