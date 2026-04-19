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
    /// SPARK.bmp 布局（反汇编 0x004287BD–0x004288FF 初始化数据严格验证）：
    ///   大 spark：src_y=0, w=102, h=80，初始化 fidx=1..4（src_x=102,204,306,408）
    ///   小 spark：src_y=80, w=61,  h=48，初始化 fidx=6..9（src_x=61,122,183,244）
    ///
    ///   注意：src_x=0 的帧（第1帧）从未被初始化写入 fidx 数组，因此从不渲染。
    ///
    /// timer → fidx 映射（反汇编严格对齐）：
    ///   timer 0      → fidx=0（未初始化）→ blank
    ///   timer 1-4    → fidx=1..4 → 大spark，src_x=102,204,306,408
    ///   timer 5-9    → 无渲染
    ///   timer 10     → fidx=5（未初始化）→ blank
    ///   timer 11-14  → fidx=6..9 → 小spark，src_x=61,122,183,244
    ///   timer 15-19  → 无渲染
    ///   timer 20-28  → fidx=(timer-20)/2+10（10..14，超出已初始化范围）→ blank
    ///   timer 30-38  → fidx=(timer-30)/2+15（15..19，超出已初始化范围）→ blank
    ///   timer >= 39  → 到期，slot 释放
    ///
    /// timer 初始值：
    ///   itr.fall > 60  → attacking*20（大spark起点，timer=0~4可见）
    ///   itr.fall <= 60 → attacking*4+10（小spark起点，timer=10~14可见）
    /// </summary>
    public class SparkRenderer : MonoBehaviour
    {
        // ========== 常量（对应反汇编 timer 边界）==========
        private const int BigEnd     = 5;   // timer 0-4   大spark渲染段
        private const int SmallStart = 10;  // timer 10    小spark起点
        private const int SmallEnd   = 15;  // timer 10-14 小spark段结束
        private const int Fade1Start = 20;  // timer 20-28 渐隐段1
        private const int Fade1End   = 29;
        private const int Fade2Start = 30;  // timer 30-38 渐隐段2
        private const int Fade2End   = 39;  // timer >= 39 到期

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
                Sprite sprite = GetSpriteForTimer(timer, out bool isExpired);

                if (isExpired)
                    continue;

                if (sprite == null)
                    continue;

                if (pool != null)
                {
                    // 存储约定：x=spark PS.x, y=edi(jump-height偏移,负数向上), z=attacker.PS.z(深度)
                    // Unity worldY = z/100 - y/100（与 LF2ObjectRenderer 公式一致：深度减跳跃高度）
                    var (wx, wy, wz) = obj.GetSparkWorldPos(j);
                    const float ppu = 100f;
                    float unityX = wx / ppu;
                    float unityY = wz / ppu - wy / ppu;

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

        private Sprite GetSpriteForTimer(int timer, out bool isExpired)
        {
            isExpired = false;

            // timer 0: blank（fidx=0 未初始化，反汇编严格对齐）
            // timer 1-4: fidx=1..4 → _bigSprites[0..3]（src_x=102,204,306,408）
            if (timer < BigEnd)
            {
                if (timer == 0) return null;
                return GetBig(timer - 1); // fidx=1..4 → index 0..3
            }

            // timer 5-9: blank
            if (timer < SmallStart)
                return null;

            // timer 10: blank（fidx=5 未初始化，反汇编严格对齐）
            // timer 11-14: fidx=6..9 → _smallSprites[0..3]（src_x=61,122,183,244）
            if (timer < SmallEnd)
            {
                if (timer == SmallStart) return null;
                return GetSmall(timer - SmallStart - 1); // timer=11→idx=0, ..., timer=14→idx=3
            }

            // timer 15-19: blank
            if (timer < Fade1Start)
                return null;

            // timer 20-28: fidx=(timer-20)/2+10（10..14），超出已初始化范围 → blank
            if (timer < Fade1End)
                return null;

            // timer 29: blank
            if (timer < Fade2Start)
                return null;

            // timer 30-38: fidx=(timer-30)/2+15（15..19），超出已初始化范围 → blank
            if (timer < Fade2End)
                return null;

            isExpired = true;
            return null;
        }
        

        private Sprite GetBig(int idx)
            => (_bigSprites != null && idx >= 0 && idx < _bigSprites.Length) ? _bigSprites[idx] : null;

        private Sprite GetSmall(int idx)
            => (_smallSprites != null && idx >= 0 && idx < _smallSprites.Length) ? _smallSprites[idx] : null;
    }
}
