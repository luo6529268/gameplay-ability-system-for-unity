using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 精灵动画模块（纯 C# 类，对应 FLF sprite.js）
    /// 封装 SpriteRenderer 操作，被 Character Hub 持有
    /// 
    /// 渐进式迁移策略：
    /// - 当前阶段：可以独立使用，也可以从 LF2CharacterAnimator 获取 SpriteRenderer
    /// - 后续阶段：完全接管精灵动画逻辑
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\sprite.js
    /// </summary>
    public class LF2Sprite
    {
        private SpriteRenderer _renderer;
        private List<Sprite> _sprites;
        private string _dir = "right";

        /// <summary>
        /// 当前方向
        /// </summary>
        public string Dir => _dir;

        /// <summary>
        /// 初始化精灵模块
        /// </summary>
        /// <param name="renderer">SpriteRenderer 组件引用</param>
        /// <param name="sprites">精灵列表</param>
        public void Initialize(SpriteRenderer renderer, List<Sprite> sprites)
        {
            _renderer = renderer;
            _sprites = sprites;
            _dir = "right";
        }

        /// <summary>
        /// 更新精灵列表（用于运行时切换角色）
        /// </summary>
        public void SetSprites(List<Sprite> sprites)
        {
            _sprites = sprites;
        }

        /// <summary>
        /// 显示指定图片（对应 FLF sp.show_pic）
        /// 参考：FLF sprite.js:111-131
        /// </summary>
        /// <param name="picIndex">图片索引</param>
        public void ShowPic(int picIndex)
        {
            if (_renderer == null || _sprites == null) return;
            if (picIndex < 0 || picIndex >= _sprites.Count) return;
            _renderer.sprite = _sprites[picIndex];
        }

        /// <summary>
        /// 切换左右方向（对应 FLF sp.switch_lr）
        /// 参考：FLF sprite.js:137-144
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
        /// 设置位置（对应 FLF sp.set_x_y）
        /// 参考：FLF sprite.js:150-153
        /// </summary>
        public void SetXY(float x, float y)
        {
            if (_renderer == null) return;
            _renderer.transform.localPosition = new Vector3(x, y, _renderer.transform.localPosition.z);
        }

        /// <summary>
        /// 设置 Z 排序（对应 FLF sp.set_z）
        /// 参考：FLF sprite.js:159-162
        /// </summary>
        public void SetZ(float z)
        {
            if (_renderer == null) return;
            _renderer.sortingOrder = (int)z;
        }

        /// <summary>
        /// 显示精灵（对应 FLF sp.show）
        /// 参考：FLF sprite.js:167-170
        /// </summary>
        public void Show()
        {
            if (_renderer != null)
                _renderer.enabled = true;
        }

        /// <summary>
        /// 隐藏精灵（对应 FLF sp.hide）
        /// 参考：FLF sprite.js:175-178
        /// </summary>
        public void Hide()
        {
            if (_renderer != null)
                _renderer.enabled = false;
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
