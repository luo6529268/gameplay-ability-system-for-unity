using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextMeshProUV : MonoBehaviour
{
    public TextMeshProUGUI text;
    [Tooltip("可选：指定要共用的原始字体材质。留空时使用文字当前的材质。")]
    public Material sharedMaterial;
    public float faceDilate;
    public float outlineWidth;
    public Color32 effectColor = Color.black;
    public Color32 GrayColor = Color.gray;
    public float underlayOffsetX;
    public float underlayOffsetY;
    public float underlayDilate;

    private Color32 TextColor;
    private bool bGray;
    private SharedMaterialEntry sharedEntry;
    private TextMeshProUGUI materialOwner;
    private bool materialWarningShown;

    private const string VertexEffectsMarker = "_TMPVertexEffects";
    private static readonly Dictionary<(Material source, int zTest), SharedMaterialEntry> SharedMaterials =
        new Dictionary<(Material source, int zTest), SharedMaterialEntry>();
    private static readonly Dictionary<Material, SharedMaterialEntry> SharedMaterialSources =
        new Dictionary<Material, SharedMaterialEntry>();

    private sealed class SharedMaterialEntry
    {
        public Material source;
        public Material material;
        public int zTest;
        public int users;
    }

    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            QueueEditorRefresh();
            return;
        }
#endif
        Refresh();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall -= RefreshEditor;
#endif
        ReleaseSharedMaterial();
    }

    public void Refresh()
    {
        if (!TryGetText() || !EnsureSharedMaterial())
            return;

        text.faceDilate = bGray ? 0f : faceDilate;
        text.outlineWidth = bGray ? 0f : outlineWidth;
        text.underlayOffsetX = underlayOffsetX;
        text.underlayOffsetY = underlayOffsetY;
        text.underlayDilate = underlayDilate;
        ApplyEffectColor();
        text.UpdateMeshPadding();
        text.SetVerticesDirty();
    }

    public void SetGray(bool isGray)
    {
        if (bGray == isGray || !TryGetText())
            return;

        if (isGray)
        {
            TextColor = text.color;
            text.color = GrayColor;
        }
        else
        {
            text.color = TextColor;
        }

        bGray = isGray;
        Refresh();
    }

    public void SetEffectColor(Color color)
    {
        effectColor = color;
        Refresh();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            QueueEditorRefresh();
            return;
        }
#endif
        Refresh();
    }

#if UNITY_EDITOR
    private void QueueEditorRefresh()
    {
        UnityEditor.EditorApplication.delayCall -= RefreshEditor;
        UnityEditor.EditorApplication.delayCall += RefreshEditor;
    }

    private void RefreshEditor()
    {
        UnityEditor.EditorApplication.delayCall -= RefreshEditor;
        if (this == null || Application.isPlaying || !isActiveAndEnabled)
            return;

        Refresh();
        if (text != null && text.isActiveAndEnabled)
        {
            text.ForceMeshUpdate();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif

    private bool TryGetText()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
        return text != null;
    }

    private void ApplyEffectColor()
    {
        text.effectColorFloat = new Vector4(
            effectColor.r / 255f,
            effectColor.g / 255f,
            effectColor.b / 255f,
            effectColor.a / 255f);
    }

    private bool EnsureSharedMaterial()
    {
        Material source = sharedMaterial;
        if (source == null)
        {
            source = sharedEntry != null && text.fontSharedMaterial == sharedEntry.material
                ? sharedEntry.source : text.fontSharedMaterial;
        }

        Material fontMaterial = text.font != null ? text.font.material : null;
        if (source == null)
            source = fontMaterial;

        // Recover default font instances left by the previous compatibility adapter.
        if (sharedMaterial == null && source != null && fontMaterial != null &&
            source.name == fontMaterial.name + " (Instance)" &&
            source.GetTexture("_MainTex") == fontMaterial.GetTexture("_MainTex"))
        {
            source = fontMaterial;
        }

        if (source == null)
            return false;

        // Duplicated UI objects can inherit the cached material from another text.
        if (SharedMaterialSources.TryGetValue(source, out SharedMaterialEntry inheritedEntry))
            source = inheritedEntry.source;

        if (source.HasProperty(VertexEffectsMarker))
        {
            ReleaseSharedMaterial();
            text.fontSharedMaterial = source;
            return true;
        }

        if (!source.HasProperty("_GradientScale"))
        {
            WarnMaterial("TextMeshProUV 需要 SDF 字体材质；当前材质不支持顶点描边效果。");
            return false;
        }

        int zTest = text.canvas != null && text.canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? 8 : 4;
        if (sharedEntry != null && materialOwner == text && sharedEntry.source == source &&
            sharedEntry.zTest == zTest && sharedEntry.material != null)
        {
            text.fontSharedMaterial = sharedEntry.material;
            return true;
        }

        ReleaseSharedMaterial();
        Material template = Resources.Load<Material>("TextMeshPro/VertexEffectsMaterial");
        if (template == null || !template.HasProperty(VertexEffectsMarker))
        {
            WarnMaterial("缺少 TextMeshPro/VertexEffectsMaterial 配套资源，请完整导入 TextMeshPro 文件夹。");
            return false;
        }

        var key = (source, zTest);
        if (!SharedMaterials.TryGetValue(key, out sharedEntry))
        {
            Material converted = new Material(source)
            {
                shader = template.shader,
                name = source.name + " [Shared Vertex Effects]",
                hideFlags = HideFlags.HideAndDontSave
            };
            converted.SetFloat("_ZTest", zTest);
            if (!source.HasProperty("_AlphaSize"))
                converted.SetFloat("_AlphaSize", 2f);
            if (!source.HasProperty("_AlphaRange"))
                converted.SetFloat("_AlphaRange", -1f);

            sharedEntry = new SharedMaterialEntry { source = source, material = converted, zTest = zTest };
            SharedMaterials.Add(key, sharedEntry);
            SharedMaterialSources.Add(converted, sharedEntry);
        }

        sharedEntry.users++;
        materialOwner = text;
        text.fontSharedMaterial = sharedEntry.material;
        materialWarningShown = false;
        return true;
    }

    private void ReleaseSharedMaterial()
    {
        if (sharedEntry == null)
            return;

        if (materialOwner != null && materialOwner.fontSharedMaterial == sharedEntry.material)
            materialOwner.fontSharedMaterial = sharedEntry.source;

        sharedEntry.users--;
        if (sharedEntry.users == 0)
        {
            SharedMaterials.Remove((sharedEntry.source, sharedEntry.zTest));
            SharedMaterialSources.Remove(sharedEntry.material);
            if (Application.isPlaying)
                Destroy(sharedEntry.material);
            else
                DestroyImmediate(sharedEntry.material);
        }

        sharedEntry = null;
        materialOwner = null;
    }

    private void WarnMaterial(string message)
    {
        if (materialWarningShown)
            return;
        materialWarningShown = true;
        Debug.LogWarning(message, this);
    }
}
