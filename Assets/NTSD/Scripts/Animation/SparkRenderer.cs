using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 命中闪光渲染器
    ///
    /// 对应 C++ release 的 SPARK blit 逻辑。
    ///
    /// C++ 在 draw_hit_records 中按 hit_record_damage 选取 SPARK.bmp 的 20 个图块，
    /// 成功绘制后立即递增 age；无效 age 只在该 slot 是最后一个时回收。
    /// </summary>
    public class SparkRenderer : MonoBehaviour
    {
        private const int SparkFrameCount = 20;

        // ========== 内部状态 ==========
        private Texture2D _sparkTex;
        private Sprite[] _sparkSprites;
        private bool _loaded = false;

        private readonly List<SpriteRenderer> _activeThisFrame = new List<SpriteRenderer>(32);
        private readonly List<LF2Entity> _objectScratch  = new List<LF2Entity>(128);

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

            world.GetAllEntities(_objectScratch);
            for (int i = 0; i < _objectScratch.Count; i++)
            {
                LF2Entity obj = _objectScratch[i];
                int slotCount = obj.HitRecordCount;
                if (slotCount <= 0) continue;
                RenderObjectSlots(obj, pool, world);
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

            _sparkTex = tex;
            _sparkSprites = new Sprite[SparkFrameCount];
            for (int pic = 0; pic < SparkFrameCount; pic++)
            {
                GetSparkSourceRect(pic, out int srcX, out int srcY, out int w, out int h);
                GetSparkOffset(pic, out int xoff, out int yoff);
                _sparkSprites[pic] = SliceSprite(tex, srcX, srcY, w, h, xoff, yoff);
            }
            _loaded = true;
        }

        private static Sprite SliceSprite(Texture2D tex, int srcX, int srcY, int w, int h, int xoff, int yoff)
        {
            int flippedY = tex.height - srcY - h;
            var rect = new Rect(srcX, flippedY, w, h);
            var pivot = new Vector2(xoff / (float)w, (h - yoff) / (float)h);
            return Sprite.Create(tex, rect, pivot, 100f);
        }

        private void RenderObjectSlots(LF2Entity obj, LF2ObjectPool pool, SimulationWorld world)
        {
            int j = 0;
            while (j < obj.HitRecordCount)
            {
                int age = obj.GetHitRecordAge(j);
                Sprite sprite = GetSpriteForAge(age);
                if (sprite == null)
                {
                    if (!obj.RemoveHitRecordIfTail(j))
                        j++;
                    continue;
                }

                float screenX = obj.GetHitRecordX(j) + obj.GetRenderOffsetX() - world.ReleaseCameraX;
                float screenY = obj.GetHitRecordZ(j);
                Vector3 unityPos = NTSDRenderSpace.ScreenPixelToWorld(screenX, screenY, 0f);

                SpriteRenderer sr = pool?.GetSprite();
                if (sr == null)
                {
                    j++;
                    continue;
                }

                sr.sprite = sprite;
                sr.transform.position = unityPos;
                sr.transform.localScale = NTSDRenderSpace.RenderScale;
                sr.sortingLayerName = "Object";
                // Keep the hit spark immediately above its source entity but
                // inside that entity's reserved slot sub-order.
                sr.sortingOrder = obj.GetRenderSortingOrder() + 1;
                _activeThisFrame.Add(sr);
                obj.AdvanceHitRecord(j, world.SparkRenderFrame);
                j++;
            }
        }

        private Sprite GetSpriteForAge(int age)
        {
            int pic = -1;
            if (age < 5)
                pic = age;
            else if (age >= 10 && age < 15)
                pic = age - 5;
            else if (age >= 20 && age < 29)
                pic = (age - 20) / 2 + 10;
            else if (age >= 30 && age < 39)
                pic = (age - 30) / 2 + 15;

            if (pic >= 0 && _sparkSprites != null && pic < _sparkSprites.Length)
                return _sparkSprites[pic];
            return null;
        }

        private static void GetSparkSourceRect(int pic, out int x, out int y, out int w, out int h)
        {
            if (pic < 5)
            {
                x = pic * 102; y = 0; w = 102; h = 80;
                return;
            }
            if (pic < 10)
            {
                x = (pic - 5) * 61; y = 80; w = 61; h = 48;
                return;
            }
            if (pic < 15)
            {
                x = (pic - 10) * 102; y = 128; w = 102; h = 80;
                return;
            }

            x = (pic - 15) * 61; y = 208; w = 61; h = 48;
        }

        private static void GetSparkOffset(int pic, out int xoff, out int yoff)
        {
            if (pic < 5 || (pic >= 10 && pic < 15))
            {
                xoff = 51; yoff = 40;
            }
            else
            {
                xoff = 30; yoff = 24;
            }
        }
    }
}
