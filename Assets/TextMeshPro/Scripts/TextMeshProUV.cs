using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextMeshProUV : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float faceDilate;
    public float outlineWidth;
    public Color32 effectColor = Color.black;
    public Color32 GrayColor = Color.gray;
    public float underlayOffsetX;
    public float underlayOffsetY;
    public float underlayDilate;

    private Color32 TextColor;
    private bool bGray;

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

#if UNITY_EDITOR
    private void OnDisable()
    {
        UnityEditor.EditorApplication.delayCall -= RefreshEditor;
    }
#endif

    public void Refresh()
    {
        if (!TryGetText())
            return;

        text.faceDilate = bGray ? 0f : faceDilate;
        text.outlineWidth = bGray ? 0f : outlineWidth;
        text.underlayOffsetX = underlayOffsetX;
        text.underlayOffsetY = underlayOffsetY;
        text.underlayDilate = underlayDilate;
        ApplyEffectColor();
        ApplyMaterialProperties();
    }

    public void SetGray(bool isGray)
    {
        if (bGray == isGray || !TryGetText())
            return;

        if (isGray)
        {
            TextColor = text.color;
            text.color = GrayColor;
            text.faceDilate = 0f;
            text.outlineWidth = 0f;
        }
        else
        {
            text.color = TextColor;
            text.faceDilate = faceDilate;
            text.outlineWidth = outlineWidth;
        }

        bGray = isGray;
        ApplyMaterialProperties();
    }

    public void SetEffectColor(Color color)
    {
        effectColor = color;
        if (TryGetText())
        {
            ApplyEffectColor();
            ApplyMaterialProperties();
        }
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

    private void ApplyMaterialProperties()
    {
        if (text.fontSharedMaterial == null)
            return;

        // Older TMP shaders read these values from the material rather than the text mesh.
        // fontMaterial gives this text its own material instance, leaving the font asset untouched.
        Material material = text.fontMaterial;
        if (material.HasProperty("_FaceDilate"))
            material.SetFloat("_FaceDilate", bGray ? 0f : faceDilate);
        if (material.HasProperty("_OutlineWidth"))
            material.SetFloat("_OutlineWidth", bGray ? 0f : outlineWidth);
        if (material.HasProperty("_OutlineColor"))
            material.SetColor("_OutlineColor", effectColor);
        if (material.HasProperty("_UnderlayOffsetX"))
            material.SetFloat("_UnderlayOffsetX", underlayOffsetX);
        if (material.HasProperty("_UnderlayOffsetY"))
            material.SetFloat("_UnderlayOffsetY", underlayOffsetY);
        if (material.HasProperty("_UnderlayDilate"))
            material.SetFloat("_UnderlayDilate", underlayDilate);

        text.UpdateMeshPadding();
        text.SetMaterialDirty();
    }
}
