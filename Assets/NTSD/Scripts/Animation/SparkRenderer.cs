using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 命中闪光渲染器
    ///
    /// 对应 NTSD 反汇编 PostRender（0x41D6E5～0x41D7F8）的 SPARK blit 逻辑。
    ///
    /// timer 递增规则（反汇编严格对齐）：
    ///   timer 在渲染时递增（渲染和递增绑定），不在渲染窗口内时直接移除 slot。
    ///   timer 0      → blank，timer++
    ///   timer 1-4    → 大spark 渲染，timer++
    ///   timer 5-9    → 直接移除（不递增）
    ///   timer 10     → blank，timer++
    ///   timer 11-14  → 小spark 渲染，timer++
    ///   timer >= 15  → 直接移除（不递增）
    ///
    /// timer 初始值：
    ///   itr.fall > 60  → itrIdx*20（大spark）
    ///   itr.fall <= 60 → itrIdx*4+10（小spark）
    /// </summary>
    public class SparkRenderer : MonoBehaviour
    {
        // ========== 常量（对应反汇编 timer 边界）==========
        private const int BigEnd     = 5;   // timer 0-4   大spark渲染段
        private const int SmallStart = 10;  // timer 10    小spark起点
        private const int SmallEnd   = 15;  // timer 10-14 小spark段结束，timer>=15 移除

        // 大/小 spark 的 BMP 帧尺寸
        // 反汇编确认：初始化 fidx=1..4（大）和 fidx=6..9（小），src_x=0 的第1帧从不渲染
        // _bigSprites[0..3] 对应 src_x=102,204,306,408（跳过 src_x=0）
        // _smallSprites[0..3] 对应 src_x=61,122,183,244（跳过 src_x=0）
        private const int BigW = 102, BigH = 80, BigSrcY = 0,   BigSrcXStart = 102;
        private const int SmallW = 61, SmallH = 48, SmallSrcY = 80, SmallSrcXStart = 61;
        private const int BigFrameCount   = 4;
        private const int SmallFrameCount = 4;

        // ========== 内部状态 ==========
        private Texture2D _sparkTex;
        private Sprite[] _bigSprites;    // 4帧（fidx=1..4，src_x=102..408）
        private Sprite[] _smallSprites;  // 4帧（fidx=6..9，src_x=61..244）
        private bool _loaded = false;

        private readonly List<SpriteRenderer> _activeThisFrame = new List<SpriteRenderer>(32);
        private readonly List<LF2LivingObject> _objectScratch  = new List<LF2LivingObject>(128);

        // ========== Unity 生命周期 ==========
        private void Awake()
        {
            LoadSparkBmp();
        }

        // ========== 公共 API ==========

        public void RenderAll(SimulationWorld world)
        {
            var pool = LF2ObjectPool.Instance;
            if (pool != null)
            {
                for (int i = 0; i < _activeThisFrame.Count; i++)
                    pool.ReleaseSprite(_activeThisFrame[i]);
            }
            _activeThisFrame.Clear();

            if (world == null) { Debug.LogWarning("[SparkRenderer] world is null"); return; }
            if (!_loaded) { Debug.LogWarning("[SparkRenderer] not loaded"); return; }

            world.GetAllLivingObjects(_objectScratch);
            for (int i = 0; i < _objectScratch.Count; i++)
            {
                LF2LivingObject obj = _objectScratch[i];
                int slotCount = obj.SparkSlotCount;
                if (slotCount <= 0) continue;
                RenderObjectSlots(obj, slotCount, pool);
            }
        }

        // ========== 私有实现 ==========

        /// <summary>
        /// 使用 BMPLoader 加载 SPARK.bmp，黑色透明处理后切割成 Sprite 数组。
        /// </summary>
        private void LoadSparkBmp()
        {
            string sparkPath = System.IO.Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD", "Sprite", "UIPanels", "SPARK.bmp");

            var tex = BMPLoader.LoadBMP(sparkPath);
            if (tex == null)
            {
                Debug.LogWarning($"[SparkRenderer] SPARK.bmp not found at {sparkPath}. Spark will not render.");
                return;
            }

            var transparentData = new TransparentColorData
            {
                targetColor    = Color.black,
                colorTolerance = 0.1f
            };
            tex = RuntimeSpriteProcessor.MakeColorTransparent(tex, transparentData);

            _sparkTex     = tex;
            // 从 fidx=1 对应的第2帧开始切（src_x=BigSrcXStart），跳过从未被写入 fidx 数组的第1帧
            _bigSprites   = SliceSprites(tex, BigSrcY,   BigW,   BigH,   BigFrameCount, BigSrcXStart);
            _smallSprites = SliceSprites(tex, SmallSrcY, SmallW, SmallH, SmallFrameCount, SmallSrcXStart);
            _loaded       = true;
        }

        /// <summary>从 texture 切割 count 个 sprite，从 (srcXStart, srcY) 起，每帧宽 w、高 h</summary>
        private static Sprite[] SliceSprites(Texture2D tex, int srcY, int w, int h, int count, int srcXStart = 0)
        {
            // Unity Texture2D 坐标系：y=0 在底部，BMP y=0 在顶部，需要翻转
            int flippedY = tex.height - srcY - h;
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                var rect = new Rect(srcXStart + i * w, flippedY, w, h);
                sprites[i] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
            }
            return sprites;
        }

        private void RenderObjectSlots(LF2LivingObject obj, int slotCount, LF2ObjectPool pool)
        {
            for (int j = 0; j < slotCount; j++)
            {
                int timer = obj.GetSparkTimer(j);
                Sprite sprite = GetSpriteForTimer(timer);
                if (sprite == null) continue;

                Vector3 wpos = obj.GetSparkWorldPos(j);
                float wx = wpos.x;
                float wy = wpos.y;
                float wz = wpos.z;
                const float ppu = 100f;
                float unityX = wx / ppu;
                float unityY = wz / ppu - wy / ppu;

                if (pool != null)
                {
                    SpriteRenderer sr = pool.GetSprite();
                    if (sr != null)
                    {
                        sr.sprite = sprite;
                        sr.transform.position = new Vector3(unityX, unityY, 0f);
                        sr.sortingLayerName = "Object";
                        sr.sortingOrder = Mathf.Abs(Mathf.RoundToInt(wz)) + 1;
                        _activeThisFrame.Add(sr);
                    }
                }
            }
        }

        private Sprite GetSpriteForTimer(int timer)
        {
            if (timer < BigEnd)
            {
                if (timer == 0) return null;
                return GetBig(timer - 1);
            }
            if (timer < SmallStart) return null;
            if (timer < SmallEnd)
            {
                if (timer == SmallStart) return null;
                return GetSmall(timer - SmallStart - 1);
            }
            return null;
        }
        

        private Sprite GetBig(int idx)
            => (_bigSprites != null && idx >= 0 && idx < _bigSprites.Length) ? _bigSprites[idx] : null;

        private Sprite GetSmall(int idx)
            => (_smallSprites != null && idx >= 0 && idx < _smallSprites.Length) ? _smallSprites[idx] : null;
    }
}
