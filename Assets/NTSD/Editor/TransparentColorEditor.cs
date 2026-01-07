using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 透明色处理配置数据（编辑器版本）
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
    public float edgeThreshold = 0.15f;            // 边缘阈值（周围非黑色像素比例）
}

public class TransparentColorEditor : EditorWindow
{
    private TransparentColorData processingData = new TransparentColorData();
    private Texture2D originalTexture;
    private Texture2D processedTexture;
    private Vector2 scrollPosition;
    private bool showPreview = true;
    private float previewSize = 300f;

    // Ԥ����ɫ
    private List<Color> presetColors = new List<Color>
    {
        Color.green,
        Color.magenta,
        Color.blue,
        Color.black,
        Color.white,
        new Color(1f, 1f, 0f) // 黄色
    };

    [MenuItem("Tools/精灵透明颜色处理器")]
    public static void ShowWindow()
    {
        TransparentColorEditor window = GetWindow<TransparentColorEditor>("透明颜色处理器");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawTextureSelection();
        DrawProcessingSettings();
        DrawPresetColors();
        DrawActionButtons();
        DrawPreview();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTextureSelection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("精灵图选择", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        originalTexture = (Texture2D)EditorGUILayout.ObjectField("原始纹理", originalTexture, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            processedTexture = null;
        }

        if (originalTexture != null)
        {
            EditorGUILayout.HelpBox($"纹理尺寸: {originalTexture.width} x {originalTexture.height}", MessageType.Info);

            // 检查纹理是否可读
            string assetPath = AssetDatabase.GetAssetPath(originalTexture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                EditorGUILayout.HelpBox("纹理未启用读写(Read/Write Enabled)! 处理前请启用。", MessageType.Warning);
                if (GUILayout.Button("启用纹理读写"))
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                }
            }
        }

        EditorGUILayout.Space();
    }

    private void DrawProcessingSettings()
    {
        EditorGUILayout.LabelField("处理设置", EditorStyles.boldLabel);

        processingData.targetColor = EditorGUILayout.ColorField("目标透明色", processingData.targetColor);
        processingData.colorTolerance = EditorGUILayout.Slider("颜色容差", processingData.colorTolerance, 0f, 1f);
        processingData.preserveEdgeColors = EditorGUILayout.Toggle("边缘平滑", processingData.preserveEdgeColors);

        if (processingData.preserveEdgeColors)
        {
            processingData.edgeSmoothing = EditorGUILayout.Slider("平滑强度", processingData.edgeSmoothing, 0.1f, 2f);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("启用抗颜色污染", EditorStyles.boldLabel);

        processingData.borderColor = EditorGUILayout.ColorField("边框颜色", processingData.borderColor);
        processingData.borderTolerance = EditorGUILayout.Slider("边框容差", processingData.borderTolerance, 0f, 1f);
        processingData.searchRadius = EditorGUILayout.IntSlider("搜索半径", processingData.searchRadius, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("边缘检测设置（保护轮廓黑边）", EditorStyles.boldLabel);

        processingData.useEdgeDetection = EditorGUILayout.Toggle("启用边缘检测", processingData.useEdgeDetection);
        if (processingData.useEdgeDetection)
        {
            processingData.edgeDetectionRadius = EditorGUILayout.IntSlider("检测半径", processingData.edgeDetectionRadius, 1, 5);
            processingData.edgeThreshold = EditorGUILayout.Slider("边缘阈值", processingData.edgeThreshold, 0f, 1f);
            EditorGUILayout.HelpBox("边缘阈值：周围非黑色像素比例超过此值时，保留该黑色像素（不变透明）。\n推荐值：0.3-0.5", MessageType.Info);
        }

        EditorGUILayout.Space();
    }

    private void ProcessTexture()
    {
        if (originalTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择要处理的纹理", "确定");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("处理纹理", "正在处理中...", 0.5f);

            // 使用与运行时相同的处理算法
            processedTexture = ProcessTransparentTexture(originalTexture, processingData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"处理纹理时出错: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"处理失败: {e.Message}", "确定");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Repaint();
    }

    /// <summary>
    /// 处理透明纹理 - 与运行时逻辑相同
    /// </summary>
    private Texture2D ProcessTransparentTexture(Texture2D sourceTexture, TransparentColorData data)
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

        // 找到需要变透明的像素
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
                    if (!isTransparent[nIdx] && !IsBorderColor(src[nIdx], data))
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
        result.SetPixels(dst);
        result.Apply();
        Debug.Log("防色彩渗透透明处理完成");
        return result;
    }

    /// <summary>
    /// 判断颜色是否接近边框色
    /// </summary>
    private bool IsBorderColor(Color c, TransparentColorData data)
    {
        float d = Mathf.Abs(c.r - data.borderColor.r) +
                  Mathf.Abs(c.g - data.borderColor.g) +
                  Mathf.Abs(c.b - data.borderColor.b);
        return d <= data.borderTolerance;
    }

    private void DrawPresetColors()
    {
        EditorGUILayout.LabelField("预设颜色", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < presetColors.Count; i++)
        {
            if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
            {
                processingData.targetColor = presetColors[i];
            }
            // 绘制颜色预览
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(rect, presetColors[i]);

            // 每行显示3个颜色
            if ((i + 1) % 3 == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = originalTexture != null;
        if (GUILayout.Button("处理纹理", GUILayout.Height(30)))
        {
            ProcessTexture();
        }

        GUI.enabled = processedTexture != null;
        if (GUILayout.Button("保存纹理", GUILayout.Height(30)))
        {
            SaveProcessedTexture();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 复制参数部分
        if (GUILayout.Button("复制参数到运行时代码", GUILayout.Height(25)))
        {
            CopyParametersToClipboard();
        }

        EditorGUILayout.Space();
    }

    private void CopyParametersToClipboard()
    {
        string code = $@"TransparentColorData transparentData = new TransparentColorData
{{
    targetColor = new Color({processingData.targetColor.r}f, {processingData.targetColor.g}f, {processingData.targetColor.b}f),
    colorTolerance = {processingData.colorTolerance}f,
    preserveEdgeColors = {processingData.preserveEdgeColors.ToString().ToLower()},
    edgeSmoothing = {processingData.edgeSmoothing}f,
    borderColor = new Color({processingData.borderColor.r}f, {processingData.borderColor.g}f, {processingData.borderColor.b}f),
    borderTolerance = {processingData.borderTolerance}f,
    searchRadius = {processingData.searchRadius},
    useEdgeDetection = {processingData.useEdgeDetection.ToString().ToLower()},
    edgeDetectionRadius = {processingData.edgeDetectionRadius},
    edgeThreshold = {processingData.edgeThreshold}f
}};";

        EditorGUIUtility.systemCopyBuffer = code;
        Debug.Log($"<color=green>参数代码已复制到剪贴板！</color>\n{code}");
        EditorUtility.DisplayDialog("成功", "参数代码已复制到剪贴板，可以将其粘贴到CharacterAnimtorManager中进行使用。", "确定");
    }

    private void DrawPreview()
    {
        if (originalTexture == null && processedTexture == null) return;

        showPreview = EditorGUILayout.Foldout(showPreview, "预览", true);
        if (!showPreview) return;

        EditorGUILayout.BeginHorizontal();

        // 原始纹理预览
        EditorGUILayout.BeginVertical(GUILayout.Width(previewSize));
        EditorGUILayout.LabelField("原始纹理", EditorStyles.centeredGreyMiniLabel);
        Rect originalRect = GUILayoutUtility.GetRect(previewSize, previewSize);
        if (originalTexture != null)
        {
            EditorGUI.DrawTextureTransparent(originalRect, originalTexture);
        }
        EditorGUILayout.EndVertical();

        // 处理后的纹理预览
        EditorGUILayout.BeginVertical(GUILayout.Width(previewSize));
        EditorGUILayout.LabelField("处理后纹理", EditorStyles.centeredGreyMiniLabel);
        Rect processedRect = GUILayoutUtility.GetRect(previewSize, previewSize);
        if (processedTexture != null)
        {
            EditorGUI.DrawTextureTransparent(processedRect, processedTexture);

            // 绘制透明背景网格
            DrawTransparentBackground(processedRect);
            GUI.DrawTexture(processedRect, processedTexture);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        // 预览尺寸控制
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("预览尺寸:");
        previewSize = EditorGUILayout.Slider(previewSize, 100f, 500f);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTransparentBackground(Rect rect)
    {
        float size = 20f;
        bool toggle = false;

        for (float y = rect.y; y < rect.y + rect.height; y += size)
        {
            toggle = !toggle;
            for (float x = rect.x; x < rect.x + rect.width; x += size * 2)
            {
                Rect cellRect = new Rect(x, y, size, size);
                EditorGUI.DrawRect(cellRect, toggle ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f));
                cellRect.x += size;
                EditorGUI.DrawRect(cellRect, !toggle ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f));
            }
        }
    }


    private void SaveProcessedTexture()
    {
        if (processedTexture == null) return;

        string path = EditorUtility.SaveFilePanel("保存处理后的纹理", "Assets", $"{originalTexture.name}_transparent", "png");
        if (string.IsNullOrEmpty(path)) return;

        // 确保路径在Assets文件夹内
        if (!path.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("错误", "请将纹理保存在Assets文件夹内", "确定");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("保存纹理", "正在保存...", 0.8f);

            // 保存为PNG
            byte[] pngData = processedTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);

            // 刷新资源数据库
            AssetDatabase.Refresh();

            // 设置导入设置
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Debug.Log($"纹理已保存: {relativePath}");
            EditorUtility.DisplayDialog("成功", "纹理保存完成！", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存纹理时出错: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"保存失败: {e.Message}", "确定");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}