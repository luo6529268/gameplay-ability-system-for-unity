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
        private string _dir = "right";

        private SpriteRenderer _shadowRenderer;
        private bool _hasShadow;

        // SortingGroup 用于角色根节点，优先控制层级；武器/SA 无 SortingGroup 则回退到 SpriteRenderer.sortingOrder
        private SortingGroup _sortingGroup;

        /// <summary>
        /// 当前方向
        /// </summary>
        public string Dir => _dir;

        private int _startFrame;

        /// <summary>
        /// 初始化精灵模块
        /// </summary>
        /// <param name="renderer">SpriteRenderer 组件引用</param>
        /// <param name="sprites">精灵列表</param>
        /// <param name="startFrame">精灵列表中的起始偏移（对应 SpriteFileInfo.startFrame）</param>
        public void Initialize(SpriteRenderer renderer, List<Sprite> sprites, int startFrame = 0)
        {
            _renderer = renderer;
            _sprites = sprites;
            _startFrame = startFrame;
            _dir = "right";

            // 从根节点查找 SortingGroup（角色有，武器/SA 无）
            _sortingGroup = renderer != null
                ? renderer.GetComponentInParent<SortingGroup>()
                : null;

            if (_renderer != null)
            {
                _renderer.sortingLayerName = "Object";
                _renderer.enabled = true;
                _renderer.color = Color.white;
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
        }

        public bool HasShadow => _hasShadow;

        /// <summary>
        /// 更新精灵列表（用于运行时切换角色）
        /// </summary>
        public void SetSprites(List<Sprite> sprites)
        {
            _sprites = sprites;
        }

        /// <summary>
        /// 显示指定图片。
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        public bool HasRenderer => _renderer != null;

        public void ShowPic(int picIndex)
        {
            if (_renderer == null || _sprites == null) return;
            if (picIndex == 999)
            {
                _renderer.enabled = false;
                return;
            }

            int actualIndex = _startFrame + picIndex;
            if (actualIndex < 0 || actualIndex >= _sprites.Count)
            {
                _renderer.enabled = false;
                return;
            }
            if (_sprites[actualIndex] == null)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;
            _renderer.sprite = _sprites[actualIndex];
        }

        /// <summary>
        /// 切换左右方向。
        /// </summary>
        /// <param name="dir">"left" 或 "right"</param>
        public void SwitchLR(string dir)
        {
            if (dir == _dir) return;
            _dir = dir;
            if (_renderer != null)
            {
                _renderer.flipX = (dir == "left");
            }
        }

        /// <summary>
        /// 设置本地显示位置。
        /// </summary>
        public void SetXY(float x, float y)
        {
            if (_renderer == null) return;
            _renderer.transform.localPosition = new Vector3(x, y, _renderer.transform.localPosition.z);
        }

        /// <summary>
        /// 设置 Z 排序。
        /// 角色有 SortingGroup → 改 SortingGroup.sortingOrder（控制整个角色层级）
        /// 武器/SA 无 SortingGroup → 回退改 SpriteRenderer.sortingOrder
        /// </summary>
        public void SetZ(float z)
        {
            if (_sortingGroup != null)
                _sortingGroup.sortingOrder = Mathf.Abs((int)z);
            else if (_renderer != null)
                _renderer.sortingOrder = Mathf.Abs((int)z);
        }

        /// <summary>
        /// 显示精灵。
        /// </summary>
        public void Show()
        {
            if (_renderer != null)
                _renderer.enabled = true;
        }

        /// <summary>
        /// 隐藏精灵。
        /// </summary>
        public void Hide()
        {
            if (_renderer != null)
                _renderer.enabled = false;
        }

        /// <summary>
        /// 显示阴影
        /// </summary>
        public void ShowShadow()
        {
            if (_shadowRenderer != null)
                _shadowRenderer.enabled = true;
        }

        /// <summary>
        /// 隐藏阴影
        /// </summary>
        public void HideShadow()
        {
            if (_shadowRenderer != null)
                _shadowRenderer.enabled = false;
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
            Hide();
            HideShadow();
        }

        /// <summary>
        /// 获取当前精灵宽度（像素）
        /// </summary>
        public float GetWidthPx()
        {
            if (_renderer == null || _renderer.sprite == null) return 0f;
            return _renderer.sprite.textureRect.width;
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
            if (_renderer == null || _renderer.sprite == null) return 0f;
            return _renderer.sprite.textureRect.height;
        }
    }
}
