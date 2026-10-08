using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class TMPSharedMaterialVerification
{
    [Serializable]
    private sealed class Result
    {
        public bool passed;
        public bool editorMode;
        public bool sameSharedMaterial;
        public bool sameRendererMaterial;
        public bool differentVertexEffects;
        public bool parameterUpdateKeepsMaterial;
        public bool sourceUnchanged;
        public bool disableEnableKeepsSharedMaterial;
        public bool duplicateKeepsSharedMaterial;
        public bool lastUserReleasesMaterial;
        public int sharedMaterialId;
        public int redPixels;
        public int bluePixels;
        public int whitePixels;
        public string shaderPath;
        public string rendererShaderPath;
        public bool rendererIsShared;
        public string firstVertex;
        public string secondVertex;
        public string error;
    }

    public static void Run()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        string evidence = arguments[Array.IndexOf(arguments, "-tmpEvidencePath") + 1];
        Directory.CreateDirectory(evidence);
        Result result = new Result { editorMode = !Application.isPlaying };
        try
        {
            ShaderUtil.allowAsyncCompilation = false;
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMeshPro/Fonts/LiberationSans.ttf");
            Require(sourceFont != null, "Source font missing");
            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(sourceFont, 100, 12,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDF, 512, 512, AtlasPopulationMode.Dynamic);
            Require(font != null && font.TryAddCharacters("TMP"), "Font creation failed");
            Material source = font.material;
            float originalOutline = source.GetFloat("_OutlineWidth");
            Color originalColor = source.GetColor("_OutlineColor");
            Texture originalAtlas = source.GetTexture("_MainTex");
            Require(!source.HasProperty("_TMPVertexEffects"), "Expected a legacy uniform shader as source");

            GameObject cameraObject = new GameObject("Verification Camera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
            RenderTexture renderTexture = new RenderTexture(960, 400, 24);
            camera.targetTexture = renderTexture;

            GameObject canvasObject = new GameObject("Verification Canvas", typeof(Canvas));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            TextMeshProUV first = CreateText(canvas, font, source, -220f, 0.04f, new Color32(240, 30, 30, 255));
            TextMeshProUV second = CreateText(canvas, font, source, 220f, 0.18f, new Color32(30, 90, 240, 255));
            first.Refresh();
            second.Refresh();
            first.text.ForceMeshUpdate();
            second.text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();

            Material shared = first.text.fontSharedMaterial;
            result.sharedMaterialId = shared.GetInstanceID();
            result.shaderPath = AssetDatabase.GetAssetPath(shared.shader);
            result.firstVertex = first.text.mesh.uv4[0] + " / " + first.text.mesh.tangents[0];
            result.secondVertex = second.text.mesh.uv4[0] + " / " + second.text.mesh.tangents[0];
            result.sameSharedMaterial = shared == second.text.fontSharedMaterial && shared != source;
            Require(result.sameSharedMaterial, "Texts did not share the same adapted material");
            Require(shared.HasProperty("_TMPVertexEffects"), "Wrong shader selected");
            Require(shared.GetTexture("_MainTex") == originalAtlas, "Atlas was not retained");
            result.differentVertexEffects = first.text.mesh.uv4[0] != second.text.mesh.uv4[0] &&
                first.text.mesh.tangents[0] != second.text.mesh.tangents[0];
            Require(result.differentVertexEffects, "Effects were not encoded separately in the meshes");
            Require(first.text.textInfo.characterCount == 3 && second.text.textInfo.characterCount == 3, "Missing characters");

            camera.Render();
            result.rendererShaderPath = AssetDatabase.GetAssetPath(first.text.canvasRenderer.GetMaterial().shader);
            result.rendererIsShared = first.text.canvasRenderer.GetMaterial() == shared;
            result.sameRendererMaterial = first.text.canvasRenderer.GetMaterial() == second.text.canvasRenderer.GetMaterial();
            Require(result.sameRendererMaterial, "Canvas renderers received different materials");
            RenderTexture.active = renderTexture;
            Texture2D image = new Texture2D(960, 400, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 960, 400), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(evidence, "shared-material-different-outlines.png"), image.EncodeToPNG());
            foreach (Color32 pixel in image.GetPixels32())
            {
                if (pixel.r > 90 && pixel.r > pixel.g * 2 && pixel.r > pixel.b * 2)
                    result.redPixels++;
                if (pixel.b > 90 && pixel.b > pixel.r * 2 && pixel.b > pixel.g * 1.5f)
                    result.bluePixels++;
                if (pixel.r > 220 && pixel.g > 220 && pixel.b > 220)
                    result.whitePixels++;
            }
            Material diagnostic = new Material(Shader.Find("Hidden/TMP Verification Vertex Channels"));
            first.text.canvasRenderer.SetMaterial(diagnostic, originalAtlas);
            second.text.canvasRenderer.SetMaterial(diagnostic, originalAtlas);
            camera.Render();
            RenderTexture.active = renderTexture;
            image.ReadPixels(new Rect(0, 0, 960, 400), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(evidence, "vertex-channel-diagnostic.png"), image.EncodeToPNG());
            first.text.canvasRenderer.SetMaterial(shared, originalAtlas);
            second.text.canvasRenderer.SetMaterial(shared, originalAtlas);
            Require(result.redPixels > 30 && result.bluePixels > 30 && result.whitePixels > 100,
                "Rendered outline colors or white faces missing");

            first.outlineWidth = 0.12f;
            first.SetEffectColor(Color.green);
            for (int i = 0; i < 20; i++)
                first.Refresh();
            first.text.ForceMeshUpdate();
            result.parameterUpdateKeepsMaterial = first.text.fontSharedMaterial.GetInstanceID() == result.sharedMaterialId &&
                second.text.fontSharedMaterial.GetInstanceID() == result.sharedMaterialId &&
                first.text.mesh.tangents[0] != second.text.mesh.tangents[0];
            Require(result.parameterUpdateKeepsMaterial, "Changing outline parameters changed the material");

            first.enabled = false;
            Require(second.text.fontSharedMaterial == shared && shared != null, "Disabling one user destroyed a shared material");
            first.enabled = true;
            first.Refresh();
            result.disableEnableKeepsSharedMaterial = first.text.fontSharedMaterial == shared;
            Require(result.disableEnableKeepsSharedMaterial, "Re-enabling did not reuse the material");

            GameObject duplicateObject = UnityEngine.Object.Instantiate(second.gameObject, canvas.transform);
            TextMeshProUV duplicate = duplicateObject.GetComponent<TextMeshProUV>();
            duplicate.sharedMaterial = null;
            duplicate.Refresh();
            result.duplicateKeepsSharedMaterial = duplicate.text.fontSharedMaterial == shared;
            Require(result.duplicateKeepsSharedMaterial, "Duplicate UI object did not reuse the material");
            first.enabled = false;
            second.enabled = false;
            Require(duplicate.text.fontSharedMaterial == shared && shared != null, "Inherited user was not retained");
            duplicate.enabled = false;
            result.lastUserReleasesMaterial = shared == null;
            Require(result.lastUserReleasesMaterial, "Last user did not release the converted material");
            result.sourceUnchanged = source.GetFloat("_OutlineWidth") == originalOutline &&
                source.GetColor("_OutlineColor") == originalColor && source.GetTexture("_MainTex") == originalAtlas &&
                !source.HasProperty("_TMPVertexEffects");
            Require(result.sourceUnchanged, "Original material effects or shader changed");
            result.passed = true;
        }
        catch (Exception exception)
        {
            result.error = exception.ToString();
            Debug.LogException(exception);
        }
        File.WriteAllText(Path.Combine(evidence, "verification.json"), JsonUtility.ToJson(result, true));
        Debug.Log("TMP_SHARED_MATERIAL_VERIFICATION: " + result.passed);
        EditorApplication.Exit(result.passed ? 0 : 1);
    }

    private static TextMeshProUV CreateText(Canvas canvas, TMP_FontAsset font, Material source,
        float x, float width, Color32 color)
    {
        GameObject gameObject = new GameObject("TMP " + x, typeof(RectTransform), typeof(CanvasRenderer));
        gameObject.SetActive(false);
        gameObject.transform.SetParent(canvas.transform, false);
        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSharedMaterial = source;
        text.text = "TMP";
        text.fontSize = 100f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.rectTransform.sizeDelta = new Vector2(360f, 160f);
        text.rectTransform.anchoredPosition = new Vector2(x, 0f);
        TextMeshProUV effects = gameObject.AddComponent<TextMeshProUV>();
        effects.text = text;
        effects.sharedMaterial = source;
        effects.outlineWidth = width;
        effects.effectColor = color;
        gameObject.SetActive(true);
        return effects;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
