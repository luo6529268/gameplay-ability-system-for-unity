using UnityEngine;
using System.IO;

namespace NTSD.Animation
{
    /// <summary>
    /// BMP 文件加载器
    /// 支持 24位 和 32位 BMP 格式的手动解析
    /// </summary>
    public static class BMPLoader
    {
        public sealed class BmpData
        {
            public int Width;
            public int Height;
            public Color[] Pixels;
        }

        /// <summary>
        /// 加载 BMP 文件为 Texture2D
        /// 自动尝试多种加载方式
        /// </summary>
        public static Texture2D LoadBMP(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[BMPLoader] 文件不存在: {filePath}");
                return null;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            Debug.Log($"[BMPLoader] 开始加载: {Path.GetFileName(filePath)} (大小: {fileInfo.Length / 1024f:F2} KB)");

            byte[] fileData = File.ReadAllBytes(filePath);

            // 方法1: 尝试使用 Unity 的 LoadImage（支持 PNG, JPG, 部分 BMP）
            Texture2D texture = TryLoadWithUnity(fileData, filePath);
            if (texture != null)
            {
                Debug.Log($"<color=green>[BMPLoader] ✅ Unity LoadImage 成功</color>");
                return texture;
            }

            // 方法2: 手动解析 BMP 格式
            Debug.Log($"<color=yellow>[BMPLoader] Unity LoadImage 失败，尝试手动解析 BMP...</color>");
            texture = LoadBMPManual(fileData);
            if (texture != null)
            {
                Debug.Log($"<color=green>[BMPLoader] ✅ 手动解析 BMP 成功</color>");
                return texture;
            }

            Debug.LogError($"<color=red>[BMPLoader] ❌ 所有加载方法失败: {filePath}</color>");
            return null;
        }

        public static BmpData LoadBmpData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[BMPLoader] 文件不存在: {filePath}");
                return null;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            Debug.Log($"[BMPLoader] 开始加载: {Path.GetFileName(filePath)} (大小: {fileInfo.Length / 1024f:F2} KB)");

            byte[] fileData = File.ReadAllBytes(filePath);
            var data = TryLoadWithUnityData(fileData, filePath);
            if (data != null)
            {
                Debug.Log($"<color=green>[BMPLoader] ✅ Unity LoadImage 成功</color>");
                return data;
            }

            Debug.Log($"<color=yellow>[BMPLoader] Unity LoadImage 失败，尝试手动解析 BMP...</color>");
            data = LoadBmpDataManual(fileData);
            if (data != null)
            {
                Debug.Log($"<color=green>[BMPLoader] ✅ 手动解析 BMP 成功</color>");
                return data;
            }

            Debug.LogError($"<color=red>[BMPLoader] ❌ 所有加载方法失败: {filePath}</color>");
            return null;
        }

        /// <summary>
        /// 尝试使用 Unity 的 LoadImage
        /// </summary>
        private static Texture2D TryLoadWithUnity(byte[] fileData, string filePath)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(fileData))
                {
                    // ⭐ 诊断日志：检查Unity加载的像素数据
                    Color[] pixels = texture.GetPixels();
                    int sampleCount = Mathf.Min(10, pixels.Length);
                    bool allBlack = true;
                    bool allTransparent = true;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        if (pixels[i].r > 0.01f || pixels[i].g > 0.01f || pixels[i].b > 0.01f)
                            allBlack = false;
                        if (pixels[i].a > 0.01f)
                            allTransparent = false;
                    }

                    if (allBlack)
                        Debug.LogWarning($"<color=yellow>[BMPLoader-Unity] ⚠️ 前{sampleCount}个像素全黑！文件={Path.GetFileName(filePath)}</color>");
                    if (allTransparent)
                        Debug.LogWarning($"<color=yellow>[BMPLoader-Unity] ⚠️ 前{sampleCount}个像素全透明！文件={Path.GetFileName(filePath)}</color>");

                    Debug.Log($"<color=green>[BMPLoader] ✅ Unity LoadImage 成功: {texture.width}x{texture.height}, 像素样本: " +
                             $"[0]={pixels[0]}, [1]={pixels[1]}, [2]={pixels[2]}</color>");
                    return texture;
                }
                Object.DestroyImmediate(texture);
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BMPLoader] Unity LoadImage 异常: {e.Message}");
                return null;
            }
        }

        private static BmpData TryLoadWithUnityData(byte[] fileData, string filePath)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(fileData))
                {
                    Color[] pixels = texture.GetPixels();
                    var data = new BmpData
                    {
                        Width = texture.width,
                        Height = texture.height,
                        Pixels = pixels
                    };
                    Object.DestroyImmediate(texture);
                    return data;
                }

                Object.DestroyImmediate(texture);
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BMPLoader] Unity LoadImage 异常: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 手动解析 BMP 文件
        /// 支持 4位、8位、24位 和 32位 BMP（未压缩）
        /// </summary>
        private static Texture2D LoadBMPManual(byte[] fileData)
        {
            try
            {
                // 检查 BMP 文件头 (BM)
                if (fileData.Length < 54 || fileData[0] != 'B' || fileData[1] != 'M')
                {
                    Debug.LogError("[BMPLoader] 不是有效的 BMP 文件（缺少 'BM' 标识）");
                    return null;
                }

                // 读取 BMP 头信息
                int pixelDataOffset = System.BitConverter.ToInt32(fileData, 10);
                int headerSize = System.BitConverter.ToInt32(fileData, 14);
                int width = System.BitConverter.ToInt32(fileData, 18);
                int height = System.BitConverter.ToInt32(fileData, 22);
                int bitsPerPixel = System.BitConverter.ToInt16(fileData, 28);
                int compression = System.BitConverter.ToInt32(fileData, 30);
                int colorsUsed = System.BitConverter.ToInt32(fileData, 46); // 实际使用的调色板颜色数

                Debug.Log($"[BMPLoader] BMP 信息: {width}x{height}, {bitsPerPixel}位, 压缩={compression}, 偏移={pixelDataOffset}, 调色板={colorsUsed}");

                // 检查是否支持
                if (compression != 0)
                {
                    Debug.LogError($"[BMPLoader] 不支持压缩的 BMP 文件（压缩类型: {compression}）");
                    return null;
                }

                if (bitsPerPixel != 4 && bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
                {
                    Debug.LogError($"[BMPLoader] 只支持 4位、8位、24位 或 32位 BMP（当前: {bitsPerPixel}位）");
                    return null;
                }

                // 读取调色板（对于 4位 和 8位 BMP）
                Color[] palette = null;
                if (bitsPerPixel <= 8)
                {
                    palette = LoadPalette(fileData, bitsPerPixel, colorsUsed);
                    if (palette == null)
                    {
                        Debug.LogError("[BMPLoader] 调色板加载失败");
                        return null;
                    }
                    Debug.Log($"<color=cyan>[BMPLoader] 调色板加载成功: {palette.Length} 种颜色</color>");
                }

                // 创建纹理
                Texture2D texture = new Texture2D(width, Mathf.Abs(height), TextureFormat.RGBA32, false);
                Color[] pixels = new Color[width * Mathf.Abs(height)];

                // BMP 行数据需要对齐到 4 字节
                int rowSize = ((width * bitsPerPixel + 31) / 32) * 4;

                // BMP 存储顺序：从下到上，从左到右
                bool isBottomUp = height > 0;
                int absHeight = Mathf.Abs(height);

                Debug.Log($"[BMPLoader] 位深度: {bitsPerPixel}位, 行大小: {rowSize}, 从下到上: {isBottomUp}");

                // 解析像素数据
                if (bitsPerPixel <= 8)
                {
                    // 使用调色板索引模式
                    ParseIndexedPixels(fileData, pixelDataOffset, width, absHeight, bitsPerPixel, rowSize, isBottomUp, palette, pixels);
                }
                else
                {
                    // 24位 / 32位 直接颜色模式
                    ParseDirectPixels(fileData, pixelDataOffset, width, absHeight, bitsPerPixel, rowSize, isBottomUp, pixels);
                }

                texture.SetPixels(pixels);
                texture.Apply();

                // ⭐ 诊断日志：检查加载的像素数据
                int sampleCount = Mathf.Min(10, pixels.Length);
                bool allBlack = true;
                bool allTransparent = true;
                for (int i = 0; i < sampleCount; i++)
                {
                    if (pixels[i].r > 0.01f || pixels[i].g > 0.01f || pixels[i].b > 0.01f)
                        allBlack = false;
                    if (pixels[i].a > 0.01f)
                        allTransparent = false;
                }

                if (allBlack)
                    Debug.LogWarning($"<color=yellow>[BMPLoader] ⚠️ 前{sampleCount}个像素全黑！这可能导致后续透明处理后精灵变空白</color>");
                if (allTransparent)
                    Debug.LogWarning($"<color=yellow>[BMPLoader] ⚠️ 前{sampleCount}个像素全透明！</color>");

                Debug.Log($"<color=green>[BMPLoader] BMP 解析成功: {width}x{absHeight}, 像素样本: " +
                         $"[0]={pixels[0]}, [1]={pixels[1]}, [2]={pixels[2]}</color>");
                return texture;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BMPLoader] 手动解析 BMP 失败: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private static BmpData LoadBmpDataManual(byte[] fileData)
        {
            try
            {
                if (fileData.Length < 54 || fileData[0] != 'B' || fileData[1] != 'M')
                {
                    Debug.LogError("[BMPLoader] 不是有效的 BMP 文件（缺少 'BM' 标识）");
                    return null;
                }

                int pixelDataOffset = System.BitConverter.ToInt32(fileData, 10);
                int width = System.BitConverter.ToInt32(fileData, 18);
                int height = System.BitConverter.ToInt32(fileData, 22);
                int bitsPerPixel = System.BitConverter.ToInt16(fileData, 28);
                int compression = System.BitConverter.ToInt32(fileData, 30);
                int colorsUsed = System.BitConverter.ToInt32(fileData, 46);

                if (compression != 0)
                {
                    Debug.LogError($"[BMPLoader] 不支持压缩的 BMP 文件（压缩类型: {compression}）");
                    return null;
                }

                if (bitsPerPixel != 4 && bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
                {
                    Debug.LogError($"[BMPLoader] 只支持 4位、8位、24位 或 32位 BMP（当前: {bitsPerPixel}位）");
                    return null;
                }

                Color[] palette = null;
                if (bitsPerPixel <= 8)
                {
                    palette = LoadPalette(fileData, bitsPerPixel, colorsUsed);
                    if (palette == null)
                    {
                        Debug.LogError("[BMPLoader] 调色板加载失败");
                        return null;
                    }
                }

                int absHeight = Mathf.Abs(height);
                Color[] pixels = new Color[width * absHeight];
                int rowSize = ((width * bitsPerPixel + 31) / 32) * 4;
                bool isBottomUp = height > 0;

                if (bitsPerPixel <= 8)
                {
                    ParseIndexedPixels(fileData, pixelDataOffset, width, absHeight, bitsPerPixel, rowSize, isBottomUp, palette, pixels);
                }
                else
                {
                    ParseDirectPixels(fileData, pixelDataOffset, width, absHeight, bitsPerPixel, rowSize, isBottomUp, pixels);
                }

                return new BmpData
                {
                    Width = width,
                    Height = absHeight,
                    Pixels = pixels
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BMPLoader] 手动解析 BMP 失败: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 加载调色板（对于 4位 和 8位 BMP）
        /// </summary>
        private static Color[] LoadPalette(byte[] fileData, int bitsPerPixel, int colorsUsed)
        {
            try
            {
                // 调色板位置：紧跟在信息头之后（文件偏移 54）
                int paletteOffset = 54;

                // 计算调色板大小
                int maxColors = (bitsPerPixel == 4) ? 16 : 256;
                int actualColors = (colorsUsed > 0 && colorsUsed <= maxColors) ? colorsUsed : maxColors;

                Debug.Log($"[BMPLoader] 调色板偏移: {paletteOffset}, 最大颜色: {maxColors}, 实际颜色: {actualColors}");

                // 检查数据长度
                if (fileData.Length < paletteOffset + actualColors * 4)
                {
                    Debug.LogError($"[BMPLoader] 调色板数据不足: 需要 {paletteOffset + actualColors * 4} 字节, 实际 {fileData.Length} 字节");
                    return null;
                }

                // 读取调色板（每个条目 4 字节：BGR + Reserved）
                // ⭐ 注意：BMP调色板格式是 BGR + Reserved（保留字节），不是BGRA！
                // 第4个字节应该被忽略，alpha默认为1.0（不透明）
                Color[] palette = new Color[actualColors];
                for (int i = 0; i < actualColors; i++)
                {
                    int offset = paletteOffset + i * 4;
                    float b = fileData[offset] / 255f;
                    float g = fileData[offset + 1] / 255f;
                    float r = fileData[offset + 2] / 255f;
                    // float a = fileData[offset + 3] / 255f;  // ❌ 错误！第4字节是Reserved，不是alpha
                    float a = 1.0f;  // ✅ BMP调色板中的颜色默认不透明
                    palette[i] = new Color(r, g, b, a);
                }

                return palette;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BMPLoader] 加载调色板失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析索引像素（4位 / 8位 BMP）
        /// </summary>
        private static void ParseIndexedPixels(byte[] fileData, int pixelDataOffset, int width, int height,
            int bitsPerPixel, int rowSize, bool isBottomUp, Color[] palette, Color[] pixels)
        {
            for (int y = 0; y < height; y++)
            {
                int rowIndex = isBottomUp ? y : (height - 1 - y);
                int rowOffset = pixelDataOffset + y * rowSize;

                for (int x = 0; x < width; x++)
                {
                    int paletteIndex = 0;

                    if (bitsPerPixel == 8)
                    {
                        // 8位：每个字节一个像素
                        paletteIndex = fileData[rowOffset + x];
                    }
                    else if (bitsPerPixel == 4)
                    {
                        // 4位：每个字节两个像素（高4位是左边像素，低4位是右边像素）
                        int byteIndex = rowOffset + x / 2;
                        if (x % 2 == 0)
                        {
                            // 左边像素（高4位）
                            paletteIndex = (fileData[byteIndex] >> 4) & 0x0F;
                        }
                        else
                        {
                            // 右边像素（低4位）
                            paletteIndex = fileData[byteIndex] & 0x0F;
                        }
                    }

                    // 边界检查
                    if (paletteIndex < 0 || paletteIndex >= palette.Length)
                    {
                        Debug.LogWarning($"[BMPLoader] 调色板索引越界: index={paletteIndex}, palette.Length={palette.Length}, 位置=({x},{y})");
                        paletteIndex = 0; // 使用默认颜色
                    }

                    pixels[rowIndex * width + x] = palette[paletteIndex];
                }
            }
        }

        /// <summary>
        /// 解析直接颜色像素（24位 / 32位 BMP）
        /// </summary>
        private static void ParseDirectPixels(byte[] fileData, int pixelDataOffset, int width, int height,
            int bitsPerPixel, int rowSize, bool isBottomUp, Color[] pixels)
        {
            int bytesPerPixel = bitsPerPixel / 8;

            for (int y = 0; y < height; y++)
            {
                int rowIndex = isBottomUp ? y : (height - 1 - y);
                int rowOffset = pixelDataOffset + y * rowSize;

                for (int x = 0; x < width; x++)
                {
                    int pixelOffset = rowOffset + x * bytesPerPixel;

                    if (pixelOffset + bytesPerPixel > fileData.Length)
                    {
                        Debug.LogError($"[BMPLoader] 像素数据越界: offset={pixelOffset}, fileSize={fileData.Length}");
                        return;
                    }

                    // BMP 格式: BGR 或 BGRA
                    float b = fileData[pixelOffset] / 255f;
                    float g = fileData[pixelOffset + 1] / 255f;
                    float r = fileData[pixelOffset + 2] / 255f;
                    float a = (bitsPerPixel == 32) ? (fileData[pixelOffset + 3] / 255f) : 1f;

                    pixels[rowIndex * width + x] = new Color(r, g, b, a);
                }
            }
        }
    }
}
