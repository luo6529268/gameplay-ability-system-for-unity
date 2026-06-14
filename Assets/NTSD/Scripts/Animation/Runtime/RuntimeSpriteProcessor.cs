using UnityEngine;
using System.Collections.Generic;

namespace NTSD.Animation
{
    /// <summary>
    /// 透明色处理配置数据，可在运行时使用
    /// </summary>
    [System.Serializable]
    public class TransparentColorData
    {
        public Color targetColor = Color.black;        // 目标透明色（黑色）
        public float colorTolerance = 0.1f;            // 颜色容差
        public bool preserveEdgeColors = true;         // 保留边缘颜色
        public float edgeSmoothing = 0.5f;             // 边缘平滑强度

        public Color borderColor = Color.black;        // 边框颜色
        public float borderTolerance = 0.12f;          // 边框容差
        public int searchRadius = 6;                   // 搜索半径

        // 边缘检测参数
        public bool useEdgeDetection = true;           // 启用边缘检测（保护轮廓黑边）
        public int edgeDetectionRadius = 1;            // 边缘检测半径
        public float edgeThreshold = 0.15f;             // 边缘阈值（周围非黑色像素比例）
    }

    /// <summary>
    /// 精灵处理器 - 运行时版本
    /// 用于处理 BMP 图片的黑色透明和精灵切割
    /// </summary>
    public static class RuntimeSpriteProcessor
    {
        public sealed class SpritePixelData
        {
            public int Width;
            public int Height;
            public Color[] Pixels;
            public string Name;
        }

        public readonly struct SpriteRectData
        {
            public readonly Rect Rect;
            public readonly string Name;

            public SpriteRectData(Rect rect, string name)
            {
                Rect = rect;
                Name = name;
            }
        }

        /// <summary>
        /// 基础透明处理
        /// </summary>
        public static Texture2D MakeColorTransparent(Texture2D sourceTexture, TransparentColorData data)
        {
            if (sourceTexture == null) return null;

            Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

            // ✅ 设置为像素艺术过滤模式
            resultTexture.filterMode = FilterMode.Point;
            resultTexture.wrapMode = TextureWrapMode.Clamp;

            resultTexture.SetPixels(sourceTexture.GetPixels());
            resultTexture.Apply();

            Color[] pixels = resultTexture.GetPixels();
            int transparentPixels = 0;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];

                // 计算颜色差异
                float colorDiff = Mathf.Abs(pixel.r - data.targetColor.r) +
                                 Mathf.Abs(pixel.g - data.targetColor.g) +
                                 Mathf.Abs(pixel.b - data.targetColor.b);

                // 如果在容差范围内，设置为透明
                if (colorDiff <= data.colorTolerance)
                {
                    float alpha = 0f;

                    // 如果启用边缘平滑，根据颜色差异计算alpha值
                    if (data.preserveEdgeColors && data.edgeSmoothing > 0)
                    {
                        alpha = Mathf.Clamp01((colorDiff - data.colorTolerance * 0.5f) / (data.colorTolerance * 0.5f));
                        alpha = Mathf.Pow(alpha, 1f / (data.edgeSmoothing + 0.1f));

                        if (alpha > 0f && alpha < 1f)
                        {
                            float blendFactor = 1f - alpha;
                            float r = Mathf.Lerp(pixel.r, data.targetColor.r, blendFactor * 0.5f);
                            float g = Mathf.Lerp(pixel.g, data.targetColor.g, blendFactor * 0.5f);
                            float b = Mathf.Lerp(pixel.b, data.targetColor.b, blendFactor * 0.5f);

                            pixels[i] = new Color(r, g, b, alpha);
                        }
                        else
                        {
                            pixels[i] = new Color(pixel.r, pixel.g, pixel.b, alpha);
                        }
                    }
                    else
                    {
                        pixels[i] = new Color(data.targetColor.r, data.targetColor.g, data.targetColor.b, alpha);
                    }

                    if (alpha == 0f) transparentPixels++;
                }
            }

            resultTexture.SetPixels(pixels);
            resultTexture.Apply();

            Debug.Log($"透明处理完成：透明像素数: {transparentPixels}/{pixels.Length} ({((float)transparentPixels / pixels.Length) * 100f:F1}%)");
            return resultTexture;
        }

        /// <summary>
        /// 带防色彩渗透的透明处理（推荐使用）
        /// </summary>
        public static Texture2D MakeColorTransparent_Debleeding_AvoidBorder(Texture2D sourceTexture, TransparentColorData data)
        {
            if (sourceTexture == null) return null;
            int w = sourceTexture.width;
            int h = sourceTexture.height;

            Color[] src = sourceTexture.GetPixels();
            Color[] dst = new Color[src.Length];
            System.Array.Copy(src, dst, src.Length);

            bool[] isTransparent = new bool[src.Length];

            // 边缘检测函数：判断像素是否在轮廓边缘上
            bool IsEdgePixel(int x, int y)
            {
                if (!data.useEdgeDetection) return false;

                int radius = data.edgeDetectionRadius;
                int nonBlackCount = 0;
                int totalCount = 0;

                // 检查周围像素
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

                        totalCount++;
                        int nIdx = ny * w + nx;
                        Color neighbor = src[nIdx];

                        // 计算邻居颜色与目标色的差异
                        float nDiff = Mathf.Abs(neighbor.r - data.targetColor.r) +
                                     Mathf.Abs(neighbor.g - data.targetColor.g) +
                                     Mathf.Abs(neighbor.b - data.targetColor.b);

                        // 如果邻居不是黑色（差异较大），计数+1
                        if (nDiff > data.colorTolerance)
                        {
                            nonBlackCount++;
                        }
                    }
                }

                // 如果周围非黑色像素比例超过阈值，认为是边缘
                if (totalCount > 0)
                {
                    float ratio = (float)nonBlackCount / totalCount;
                    return ratio >= data.edgeThreshold;
                }
                return false;
            }

            // 找到需要变透明的像素（根据 targetColor 和 colorTolerance）
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    Color p = src[i];
                    float diff = Mathf.Abs(p.r - data.targetColor.r) +
                                 Mathf.Abs(p.g - data.targetColor.g) +
                                 Mathf.Abs(p.b - data.targetColor.b);

                    if (diff <= data.colorTolerance)
                    {
                        // 检查是否是边缘像素（轮廓黑边）
                        if (IsEdgePixel(x, y))
                        {
                            // 是边缘，保留不处理
                            continue;
                        }

                        // 不是边缘，设为透明
                        isTransparent[i] = true;
                        dst[i].a = 0f;
                    }
                }
            }

            // 内部函数：判断颜色是否接近边框色
            bool IsBorderColor(Color c)
            {
                float d = Mathf.Abs(c.r - data.borderColor.r) +
                          Mathf.Abs(c.g - data.borderColor.g) +
                          Mathf.Abs(c.b - data.borderColor.b);
                return d <= data.borderTolerance;
            }

            // 生成邻域偏移
            int maxRadius = Mathf.Max(1, data.searchRadius);
            List<(int dx, int dy)> neighborOffsets = new List<(int, int)>();
            for (int r = 1; r <= maxRadius; r++)
            {
                for (int yy = -r; yy <= r; yy++)
                {
                    for (int xx = -r; xx <= r; xx++)
                    {
                        if (Mathf.Abs(xx) == r || Mathf.Abs(yy) == r)
                            neighborOffsets.Add((xx, yy));
                    }
                }
            }

            // 处理每个透明像素
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (!isTransparent[idx]) continue;

                    Color chosen = new Color(0, 0, 0, 0);
                    bool found = false;

                    // 优先寻找非透明且非边框的颜色
                    foreach (var off in neighborOffsets)
                    {
                        int nx = x + off.dx;
                        int ny = y + off.dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        int nIdx = ny * w + nx;
                        if (!isTransparent[nIdx] && !IsBorderColor(src[nIdx]))
                        {
                            chosen = src[nIdx];
                            found = true;
                            break;
                        }
                    }

                    // 如果没找到，寻找任何非透明像素
                    if (!found)
                    {
                        foreach (var off in neighborOffsets)
                        {
                            int nx = x + off.dx;
                            int ny = y + off.dy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                            int nIdx = ny * w + nx;
                            if (!isTransparent[nIdx])
                            {
                                chosen = src[nIdx];
                                found = true;
                                break;
                            }
                        }
                    }

                    // 设置透明像素的 RGB 为邻近颜色
                    if (found)
                    {
                        dst[idx].r = chosen.r;
                        dst[idx].g = chosen.g;
                        dst[idx].b = chosen.b;
                        dst[idx].a = 0f;
                    }
                    else
                    {
                        dst[idx] = new Color(data.targetColor.r, data.targetColor.g, data.targetColor.b, 0f);
                    }
                }
            }

            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);

            // ✅ 设置为像素艺术过滤模式
            result.filterMode = FilterMode.Point;
            result.wrapMode = TextureWrapMode.Clamp;

            result.SetPixels(dst);
            result.Apply();
            Debug.Log("防色彩渗透透明处理完成");
            return result;
        }

        /// <summary>
        /// 从纹理切割精灵（从左上角开始）
        /// 遵循 LF2 格式：每个精灵格子实际尺寸为 (width+1)x(height+1)，
        /// 右边和底部各有 1 像素绿色边框线需要排除
        /// </summary>
        /// <param name="texture">源纹理</param>
        /// <param name="width">单个精灵宽度（配置表中的值，不包含边框）</param>
        /// <param name="height">单个精灵高度（配置表中的值，不包含边框）</param>
        /// <param name="row">行数</param>
        /// <param name="col">列数</param>
        /// <returns>切割后的精灵列表</returns>
        public static List<Sprite> SliceTextureFromTopLeft(Texture2D texture, int width, int height, int row, int col)
        {
            List<Sprite> sprites = new List<Sprite>();

            if (texture == null)
            {
                Debug.LogError("源纹理为空，无法切割精灵");
                return sprites;
            }

            // LF2 格式：每个格子实际尺寸为 (width+1) x (height+1)
            // 包含右边和底部各 1 像素的绿色边框线
            int cellWidth = width + 1;   // 格子宽度（包含右边框）
            int cellHeight = height + 1; // 格子高度（包含底边框）

            int skipped = 0;

            // 从左上角开始，按行列顺序切割
            // r 是行索引（从上到下），c 是列索引（从左到右）
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    // 计算格子起始位置（包含边框的格子）
                    // Unity 纹理坐标原点在左下角，所以需要转换
                    int x = c * cellWidth;  // 列索引 * 格子宽度
                    int y = texture.height - (r + 1) * cellHeight;  // 从上往下数第 r 行

                    // 跳过底部 1 像素的绿色边框线
                    // Rect 的 (x,y) 是左下角，向上延伸 height 像素
                    // 绿色线在格子最底部，所以 y 要 +1
                    y += 1;

                    // 确保起始位置在纹理范围内
                    if (x < 0 || y < 0)
                    {
                        skipped++;
                        Debug.LogWarning($"[切割] 跳过精灵 ({r},{c}): 起始位置超出边界 x={x}, y={y}");
                        continue;
                    }

                    // ✅ 强制使用统一尺寸，确保所有帧 Rect 高度一致，避免序列帧播放时位移
                    // 检查纹理范围是否足够容纳完整的精灵尺寸
                    if (x + width > texture.width || y + height > texture.height)
                    {
                        skipped++;
                        Debug.LogWarning($"[切割] 跳过精灵 ({r},{c}): 纹理范围不足 " +
                                       $"需要位置=({x},{y}), 尺寸=({width}x{height}), 纹理=({texture.width}x{texture.height})");
                        continue;
                    }

                    // 创建精灵（强制使用配置的统一尺寸）
                    Rect spriteRect = new Rect(x, y, width, height);
                    // ✅ 设置锚点为底部中心（0.5, 0），防止序列帧播放时上下漂移
                    Vector2 pivot = new Vector2(0.5f, 0f);

                    Sprite sprite = Sprite.Create(texture, spriteRect, pivot, 100f);
                    sprite.name = $"sprite_{r}_{c}";
                    sprites.Add(sprite);
                }
            }

            if (skipped > 0)
            {
                Debug.LogWarning($"精灵切割警告：期望 {row}x{col}={row*col} 个精灵，实际得到 {sprites.Count} 个，跳过 {skipped} 个");
            }

            Debug.Log($"精灵切割完成：从 {row}x{col} 网格（每格 {cellWidth}x{cellHeight}）切割出 {sprites.Count} 个精灵（每个 {width}x{height}）");
            return sprites;
        }

        public static List<SpritePixelData> SliceAndProcessPixels(Color[] sourcePixels, int textureWidth, int textureHeight,
            int width, int height, int row, int col, TransparentColorData data)
        {
            var slices = SliceTextureFromTopLeftPixels(sourcePixels, textureWidth, textureHeight, width, height, row, col);
            for (int i = 0; i < slices.Count; i++)
            {
                var slice = slices[i];
                slice.Pixels = MakeColorTransparent_Debleeding_AvoidBorderPixels(slice.Pixels, slice.Width, slice.Height, data);
            }

            return slices;
        }

        public static Color[] ProcessSheetPixels(Color[] sourcePixels, int textureWidth, int textureHeight, TransparentColorData data)
        {
            return MakeColorTransparent_Debleeding_AvoidBorderPixels(sourcePixels, textureWidth, textureHeight, data);
        }

        public static Color32[] ProcessSheetPixelsFast(Color32[] sourcePixels)
        {
            if (sourcePixels == null || sourcePixels.Length == 0)
            {
                return sourcePixels;
            }

            for (int i = 0; i < sourcePixels.Length; i++)
            {
                Color32 pixel = sourcePixels[i];
                if (pixel.r == 0 && pixel.g == 0 && pixel.b == 0)
                {
                    pixel.a = 0;
                }
                else
                {
                    pixel.a = 255;
                }

                sourcePixels[i] = pixel;
            }

            return sourcePixels;
        }

        public static List<SpriteRectData> BuildSpriteRectsFromTopLeft(int textureWidth, int textureHeight,
            int width, int height, int row, int col)
        {
            var rects = new List<SpriteRectData>();
            int cellWidth = width + 1;
            int cellHeight = height + 1;

            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    int x = c * cellWidth;
                    int y = textureHeight - (r + 1) * cellHeight;
                    y += 1;

                    if (x < 0 || y < 0)
                    {
                        continue;
                    }

                    if (x + width > textureWidth || y + height > textureHeight)
                    {
                        continue;
                    }

                    rects.Add(new SpriteRectData(
                        new Rect(x, y, width, height),
                        $"sprite_{r}_{c}"
                    ));
                }
            }

            return rects;
        }

        public static List<SpritePixelData> SliceTextureFromTopLeftPixels(Color[] sourcePixels, int textureWidth, int textureHeight,
            int width, int height, int row, int col)
        {
            List<SpritePixelData> sprites = new List<SpritePixelData>();
            if (sourcePixels == null || sourcePixels.Length == 0)
            {
                return sprites;
            }

            int cellWidth = width + 1;
            int cellHeight = height + 1;

            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < col; c++)
                {
                    int x = c * cellWidth;
                    int y = textureHeight - (r + 1) * cellHeight;
                    y += 1;

                    if (x < 0 || y < 0)
                    {
                        continue;
                    }

                    if (x + width > textureWidth || y + height > textureHeight)
                    {
                        continue;
                    }

                    var pixels = new Color[width * height];
                    for (int yy = 0; yy < height; yy++)
                    {
                        int srcY = y + yy;
                        int srcRow = srcY * textureWidth;
                        int dstRow = yy * width;
                        for (int xx = 0; xx < width; xx++)
                        {
                            pixels[dstRow + xx] = sourcePixels[srcRow + x + xx];
                        }
                    }

                    sprites.Add(new SpritePixelData
                    {
                        Width = width,
                        Height = height,
                        Pixels = pixels,
                        Name = $"sprite_{r}_{c}"
                    });
                }
            }

            return sprites;
        }

        public static Color[] MakeColorTransparent_Debleeding_AvoidBorderPixels(Color[] src, int w, int h, TransparentColorData data)
        {
            if (src == null || src.Length == 0)
            {
                return src;
            }

            Color[] dst = new Color[src.Length];
            System.Array.Copy(src, dst, src.Length);
            bool[] isTransparent = new bool[src.Length];

            bool IsEdgePixel(int x, int y)
            {
                if (!data.useEdgeDetection) return false;

                int radius = data.edgeDetectionRadius;
                int nonBlackCount = 0;
                int totalCount = 0;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

                        totalCount++;
                        int nIdx = ny * w + nx;
                        Color neighbor = src[nIdx];

                        float nDiff = Mathf.Abs(neighbor.r - data.targetColor.r) +
                                      Mathf.Abs(neighbor.g - data.targetColor.g) +
                                      Mathf.Abs(neighbor.b - data.targetColor.b);

                        if (nDiff > data.colorTolerance)
                        {
                            nonBlackCount++;
                        }
                    }
                }

                if (totalCount > 0)
                {
                    float ratio = (float)nonBlackCount / totalCount;
                    return ratio >= data.edgeThreshold;
                }

                return false;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    Color p = src[i];
                    float diff = Mathf.Abs(p.r - data.targetColor.r) +
                                 Mathf.Abs(p.g - data.targetColor.g) +
                                 Mathf.Abs(p.b - data.targetColor.b);

                    if (diff <= data.colorTolerance)
                    {
                        if (IsEdgePixel(x, y))
                        {
                            continue;
                        }

                        isTransparent[i] = true;
                        dst[i].a = 0f;
                    }
                }
            }

            bool IsBorderColor(Color c)
            {
                float d = Mathf.Abs(c.r - data.borderColor.r) +
                          Mathf.Abs(c.g - data.borderColor.g) +
                          Mathf.Abs(c.b - data.borderColor.b);
                return d <= data.borderTolerance;
            }

            int maxRadius = Mathf.Max(1, data.searchRadius);
            List<(int dx, int dy)> neighborOffsets = new List<(int, int)>();
            for (int r = 1; r <= maxRadius; r++)
            {
                for (int yy = -r; yy <= r; yy++)
                {
                    for (int xx = -r; xx <= r; xx++)
                    {
                        if (Mathf.Abs(xx) == r || Mathf.Abs(yy) == r)
                            neighborOffsets.Add((xx, yy));
                    }
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (!isTransparent[idx]) continue;

                    Color chosen = new Color(0, 0, 0, 0);
                    bool found = false;

                    foreach (var off in neighborOffsets)
                    {
                        int nx = x + off.dx;
                        int ny = y + off.dy;
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        int nIdx = ny * w + nx;
                        if (!isTransparent[nIdx] && !IsBorderColor(src[nIdx]))
                        {
                            chosen = src[nIdx];
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        foreach (var off in neighborOffsets)
                        {
                            int nx = x + off.dx;
                            int ny = y + off.dy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                            int nIdx = ny * w + nx;
                            if (!isTransparent[nIdx])
                            {
                                chosen = src[nIdx];
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        dst[idx].r = chosen.r;
                        dst[idx].g = chosen.g;
                        dst[idx].b = chosen.b;
                        dst[idx].a = 0f;
                    }
                    else
                    {
                        dst[idx] = new Color(data.targetColor.r, data.targetColor.g, data.targetColor.b, 0f);
                    }
                }
            }

            return dst;
        }
    }
}
