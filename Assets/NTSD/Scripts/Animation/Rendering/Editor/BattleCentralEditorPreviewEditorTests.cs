#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralEditorPreviewEditorTests
    {
        [Test]
        public void HealthBatch_UsesThreeQuadsAndClampedWidths()
        {
            var backend = new BattleHealthBarBatchBackend();
            try
            {
                BattleHealthBarStyle style = BattleHealthBarStyle.Default;
                var instances = new[]
                {
                    new BattleHealthBarInstance(
                        new Vector2(10f, 20f),
                        3f,
                        50,
                        80,
                        100),
                };

                backend.Build(instances, instances.Length, style);

                Assert.That(backend.ActiveBarCount, Is.EqualTo(1));
                Assert.That(backend.ActiveQuadCount, Is.EqualTo(3));
                Assert.That(backend.ActiveVertexCount, Is.EqualTo(12));
                Assert.That(backend.ActiveIndexCount, Is.EqualTo(18));
                Assert.That(backend.Mesh, Is.Not.Null);
                Assert.That(backend.Mesh.subMeshCount, Is.EqualTo(1));
                Assert.That(backend.Mesh.GetSubMesh(0).indexCount, Is.EqualTo(18));

                float unitX = NTSDRenderSpace.UnitsPerPixelX;
                float innerLeft = 10f - 30f * unitX + unitX;
                float innerWidth = 58f * unitX;
                Vector3 recoverableRight = backend.GetVertexPosition(6);
                Vector3 currentRight = backend.GetVertexPosition(10);
                Assert.That(
                    recoverableRight.x,
                    Is.EqualTo(innerLeft + innerWidth * 0.8f).Within(0.0001f));
                Assert.That(
                    currentRight.x,
                    Is.EqualTo(innerLeft + innerWidth * 0.5f).Within(0.0001f));
                Assert.That(
                    backend.GetVertexColor(0),
                    Is.EqualTo(style.BackgroundColor));
                Assert.That(
                    backend.GetVertexColor(4),
                    Is.EqualTo(style.RecoverableColor));
                Assert.That(
                    backend.GetVertexColor(8),
                    Is.EqualTo(style.CurrentColor));
            }
            finally
            {
                backend.Dispose();
            }
        }

        [Test]
        public void HealthBatch_OneThousandBarsRemainOneMeshAndOneSubMesh()
        {
            var backend = new BattleHealthBarBatchBackend();
            var instances = new BattleHealthBarInstance[1000];
            for (int index = 0; index < instances.Length; index++)
            {
                instances[index] = new BattleHealthBarInstance(
                    new Vector2(index * 0.01f, index * 0.005f),
                    0f,
                    index % 101,
                    100,
                    100);
            }

            try
            {
                BattleHealthBarStyle style = BattleHealthBarStyle.Default;
                backend.Build(instances, instances.Length, style);

                Assert.That(backend.ActiveBarCount, Is.EqualTo(1000));
                Assert.That(backend.ActiveQuadCount, Is.EqualTo(3000));
                Assert.That(backend.ActiveVertexCount, Is.EqualTo(12000));
                Assert.That(backend.ActiveIndexCount, Is.EqualTo(18000));
                Assert.That(backend.ActiveQuadCount, Is.LessThan(BattleDynamicMeshBackend.QuadsPerChunk));
                Assert.That(backend.Mesh.subMeshCount, Is.EqualTo(1));
                Assert.That(backend.Mesh.GetSubMesh(0).indexCount, Is.EqualTo(18000));
            }
            finally
            {
                backend.Dispose();
            }
        }

        [Test]
        public void TopLeftSourceRect_SkipsOnePixelGridSeparator()
        {
            var texture = new Texture2D(800, 560, TextureFormat.RGBA32, false);
            try
            {
                RectInt rect = BattleCentralEditorPreview.ResolveTopLeftSourceRectForEditor(
                    texture,
                    79,
                    79);

                Assert.That(rect, Is.EqualTo(new RectInt(0, 481, 79, 79)));
                Assert.That(rect.yMin, Is.GreaterThan(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void PreviewController_BuildsCharacterAndHealthFromSameSpriteTop()
        {
            GameObject previewObject = null;
            Texture2D texture = null;
            Sprite sprite = null;
            Material material = null;
            try
            {
                Shader shader = Shader.Find("NTSD/BattleCentralTransparent");
                Assert.That(shader, Is.Not.Null);
                material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                texture = new Texture2D(8, 16, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 16f),
                    new Vector2(0.5f, 0f),
                    100f);
                sprite.hideFlags = HideFlags.HideAndDontSave;
                var actor = new BattleCentralEditorPreviewActor();
                actor.ConfigureForSelfCheck(sprite, new Vector3(1f, 2f, 3f), 25, 75, 100);
                BattleHealthBarStyle style = BattleHealthBarStyle.Default;

                previewObject = new GameObject("BattleCentralEditorPreview_Test");
                BattleCentralEditorPreview preview =
                    previewObject.AddComponent<BattleCentralEditorPreview>();
                preview.ConfigureForSelfCheck(material, actor, style);

                Assert.That(preview.RebuildForSelfCheck(), Is.True);
                Assert.That(preview.PreviewActorCount, Is.EqualTo(1));
                Assert.That(preview.PreviewHealthBarCount, Is.EqualTo(1));
                Assert.That(preview.PreviewHealthQuadCount, Is.EqualTo(3));
                Assert.That(
                    preview.TryGetEditorLayout(
                        0,
                        out BattleCentralEditorPreviewLayout editorLayout),
                    Is.True);
                Assert.That(
                    editorLayout.PivotWorldPosition,
                    Is.EqualTo(new Vector3(1f, 2f, 3f)));
                Assert.That(editorLayout.HasHealthBar, Is.True);

                float expectedSpriteTop =
                    2f + 16f * NTSDRenderSpace.UnitsPerPixelY *
                    NTSDRenderSpace.BattleVisualScale;
                float expectedBarBottom =
                    expectedSpriteTop + style.HeadGapPixels * NTSDRenderSpace.UnitsPerPixelY;
                Vector3 backgroundBottomLeft =
                    preview.HealthBackendForSelfCheck.GetVertexPosition(0);
                Assert.That(backgroundBottomLeft.y, Is.EqualTo(expectedBarBottom).Within(0.0001f));
                Assert.That(
                    editorLayout.SpriteBounds.max.y,
                    Is.EqualTo(expectedSpriteTop).Within(0.0001f));
                Assert.That(
                    editorLayout.HealthBarBounds.min.y,
                    Is.EqualTo(expectedBarBottom).Within(0.0001f));

                var commandBuffer = new CommandBuffer();
                var propertyBlock = new MaterialPropertyBlock();
                try
                {
                    Assert.That(
                        preview.AppendDrawCommands(commandBuffer, propertyBlock),
                        Is.EqualTo(2),
                        "one character segment plus one health batch draw should be recorded");
                }
                finally
                {
                    commandBuffer.Release();
                }
            }
            finally
            {
                if (previewObject != null)
                    UnityEngine.Object.DestroyImmediate(previewObject);
                if (sprite != null)
                    UnityEngine.Object.DestroyImmediate(sprite);
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void PreviewCameraGate_IsEditModeOnlyAndWorldCameraScoped()
        {
            GameObject previewObject = null;
            GameObject worldCameraObject = null;
            GameObject otherCameraObject = null;
            try
            {
                previewObject = new GameObject("BattleCentralEditorPreview_CameraGate");
                BattleCentralEditorPreview preview =
                    previewObject.AddComponent<BattleCentralEditorPreview>();
                worldCameraObject = new GameObject("BattleCentralEditorPreview_WorldCamera");
                Camera worldCamera = worldCameraObject.AddComponent<Camera>();
                otherCameraObject = new GameObject("BattleCentralEditorPreview_OtherCamera");
                Camera otherCamera = otherCameraObject.AddComponent<Camera>();

                Assert.That(
                    preview.CanRenderCameraForSelfCheck(
                        otherCamera,
                        CameraRenderType.Base,
                        CameraType.SceneView,
                        false,
                        worldCamera),
                    Is.True);
                Assert.That(
                    preview.CanRenderCameraForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        CameraType.Game,
                        false,
                        worldCamera),
                    Is.True);
                Assert.That(
                    preview.CanRenderCameraForSelfCheck(
                        otherCamera,
                        CameraRenderType.Base,
                        CameraType.Game,
                        false,
                        worldCamera),
                    Is.False);
                Assert.That(
                    preview.CanRenderCameraForSelfCheck(
                        worldCamera,
                        CameraRenderType.Overlay,
                        CameraType.Game,
                        false,
                        worldCamera),
                    Is.False);
                Assert.That(
                    preview.CanRenderCameraForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        CameraType.Game,
                        true,
                        worldCamera),
                    Is.False);
            }
            finally
            {
                if (otherCameraObject != null)
                    UnityEngine.Object.DestroyImmediate(otherCameraObject);
                if (worldCameraObject != null)
                    UnityEngine.Object.DestroyImmediate(worldCameraObject);
                if (previewObject != null)
                    UnityEngine.Object.DestroyImmediate(previewObject);
            }
        }

        [Test]
        public void PreviewController_UsesSceneViewAuthoringEditor()
        {
            GameObject previewObject = null;
            UnityEditor.Editor inspector = null;
            try
            {
                previewObject = new GameObject("BattleCentralEditorPreview_Inspector");
                BattleCentralEditorPreview preview =
                    previewObject.AddComponent<BattleCentralEditorPreview>();

                inspector = UnityEditor.Editor.CreateEditor(preview);

                Assert.That(inspector, Is.Not.Null);
                Assert.That(inspector, Is.TypeOf<BattleCentralEditorPreviewEditor>());
            }
            finally
            {
                if (inspector != null)
                    UnityEngine.Object.DestroyImmediate(inspector);
                if (previewObject != null)
                    UnityEngine.Object.DestroyImmediate(previewObject);
            }
        }
    }

    public static class BattleCentralEditorPreviewValidationEditor
    {
        private const string MenuPath =
            "NTSD/Battle Rendering/Validate Edit Mode Central Preview";
        private const string SourceTexturePath =
            "Assets/NTSD/Sprite/Character/Zuozhu/sasuke_0.bmp";
        private const string MaterialPath =
            "Assets/NTSD/Materials/BattleCentralTransparent.mat";
        private const string OutputDirectory =
            "Temp/BATTLE-CENTRAL-EDITOR-PREVIEW-001";

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var report = new ValidationReport
            {
                status = "FAIL",
                sourceTexturePath = SourceTexturePath,
            };
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, OutputDirectory);
            string imagePath = Path.Combine(outputDirectory, "editmode-preview.png");
            string resultPath = Path.Combine(outputDirectory, "editmode-preview.result.json");
            GameObject previewObject = null;
            Sprite sprite = null;
            RenderTexture target = null;
            Texture2D readback = null;
            Camera sceneCamera = null;
            IDisposable validationScope = null;
            CameraState cameraState = default;
            bool cameraStateCaptured = false;
            RenderTexture previousActive = RenderTexture.active;
            UnityEngine.SceneManagement.Scene activeScene =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            bool sceneWasDirty = activeScene.isDirty;

            try
            {
                if (Application.isPlaying)
                    throw new InvalidOperationException("Validation requires Edit Mode.");

                Camera worldCamera = NTSDRenderSpace.WorldCamera;
                if (worldCamera == null)
                    throw new InvalidOperationException("The battle world camera is unavailable.");
                SceneView sceneView = SceneView.lastActiveSceneView ??
                                      EditorWindow.GetWindow<SceneView>();
                sceneCamera = sceneView?.camera;
                if (sceneCamera == null || sceneCamera.cameraType != CameraType.SceneView)
                    throw new InvalidOperationException("A SceneView camera is unavailable.");

                Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material == null)
                    throw new InvalidOperationException("The central material is unavailable.");

                CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
                LF2FrameData frame = manager?.GetFrameData(0, 0);
                var loadedSprites = manager?.GetCharacterSpriteByID(0);
                int pic = frame?.pic ?? -1;
                if (loadedSprites != null && (uint)pic < (uint)loadedSprites.Count)
                {
                    sprite = loadedSprites[pic];
                    report.usedCharacterManagerSprite = sprite != null;
                }

                previewObject = new GameObject("__BattleCentralEditorPreview_Validation__")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                previewObject.transform.position = new Vector3(
                    worldCamera.transform.position.x,
                    worldCamera.transform.position.y,
                    0f);
                BattleCentralEditorPreview preview =
                    previewObject.AddComponent<BattleCentralEditorPreview>();
                var actor = new BattleCentralEditorPreviewActor();
                if (sprite != null)
                {
                    actor.ConfigureForSelfCheck(sprite, Vector3.zero, 35, 75, 100);
                }
                else
                {
                    Texture2D sourceTexture =
                        AssetDatabase.LoadAssetAtPath<Texture2D>(SourceTexturePath);
                    if (sourceTexture == null)
                        throw new InvalidOperationException("The fallback preview texture is unavailable.");
                    actor.ConfigureTextureForSelfCheck(
                        sourceTexture,
                        BattleCentralEditorPreview.ResolveTopLeftSourceRectForEditor(
                            sourceTexture,
                            79,
                            79),
                        new Vector2(0.5f, 0f),
                        35,
                        75,
                        100);
                    report.usedProcessedSourceSheet = true;
                }
                preview.ConfigureForSelfCheck(material, actor, BattleHealthBarStyle.Default);
                report.controllerBuilt = preview.RebuildForSelfCheck();
                report.actorCount = preview.PreviewActorCount;
                report.healthBarCount = preview.PreviewHealthBarCount;
                report.healthQuadCount = preview.PreviewHealthQuadCount;
                validationScope =
                    BattleCentralEditorPreview.BeginExclusiveValidationForSelfCheck(preview);

                cameraState = new CameraState(sceneCamera);
                cameraStateCaptured = true;
                target = new RenderTexture(
                    512,
                    512,
                    24,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                target.Create();
                sceneCamera.transform.SetPositionAndRotation(
                    worldCamera.transform.position,
                    worldCamera.transform.rotation);
                sceneCamera.orthographic = true;
                sceneCamera.orthographicSize = worldCamera.orthographicSize;
                sceneCamera.nearClipPlane = worldCamera.nearClipPlane;
                sceneCamera.farClipPlane = worldCamera.farClipPlane;
                sceneCamera.aspect = 1f;
                sceneCamera.rect = new Rect(0f, 0f, 1f, 1f);
                sceneCamera.cullingMask = 0;
                sceneCamera.clearFlags = CameraClearFlags.SolidColor;
                sceneCamera.backgroundColor = Color.white;
                sceneCamera.allowHDR = false;
                sceneCamera.allowMSAA = false;
                sceneCamera.targetTexture = target;
                sceneCamera.ResetProjectionMatrix();
                sceneCamera.Render();

                RenderTexture.active = target;
                readback = new Texture2D(
                    target.width,
                    target.height,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                readback.ReadPixels(
                    new Rect(0f, 0f, target.width, target.height),
                    0,
                    0,
                    false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    if (pixel.r < 250 || pixel.g < 250 || pixel.b < 250)
                        report.nonClearPixelCount++;
                    if (pixel.r > 80 && pixel.r > pixel.g + 40 && pixel.r > pixel.b + 40)
                        report.redDominantPixelCount++;
                    if (pixel.g > 200 && pixel.r < 40 && pixel.b < 40)
                        report.greenSeparatorPixelCount++;
                }

                Directory.CreateDirectory(outputDirectory);
                File.WriteAllBytes(imagePath, readback.EncodeToPNG());
                report.imagePath = Path.GetRelativePath(projectRoot, imagePath)
                    .Replace('\\', '/');
                report.sceneDirtyUnchanged =
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty == sceneWasDirty;
                report.status = report.controllerBuilt && report.actorCount == 1 &&
                                report.healthBarCount == 1 && report.healthQuadCount == 3 &&
                                report.nonClearPixelCount > 0 &&
                                report.redDominantPixelCount > 0 &&
                                report.greenSeparatorPixelCount == 0 &&
                                report.sceneDirtyUnchanged
                    ? "PASS"
                    : "FAIL";
            }
            catch (Exception exception)
            {
                report.message = exception.ToString();
            }
            finally
            {
                RenderTexture.active = previousActive;
                validationScope?.Dispose();
                if (cameraStateCaptured && sceneCamera != null)
                    cameraState.Restore(sceneCamera);
                if (previewObject != null)
                    UnityEngine.Object.DestroyImmediate(previewObject);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                report.sceneDirtyUnchanged =
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty == sceneWasDirty;
                Directory.CreateDirectory(outputDirectory);
                File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
            }

            if (report.status == "PASS")
            {
                Debug.Log(
                    $"[BattleCentralEditorPreviewValidation] PASS: actors={report.actorCount}, " +
                    $"health={report.healthBarCount}, pixels={report.nonClearPixelCount}, " +
                    $"red={report.redDominantPixelCount}, " +
                    $"greenSeparator={report.greenSeparatorPixelCount}.");
            }
            else
            {
                Debug.LogError(
                    "[BattleCentralEditorPreviewValidation] FAIL: " +
                    JsonUtility.ToJson(report));
            }
        }

        private readonly struct CameraState
        {
            private readonly Vector3 position;
            private readonly Quaternion rotation;
            private readonly bool orthographic;
            private readonly float orthographicSize;
            private readonly float nearClipPlane;
            private readonly float farClipPlane;
            private readonly float aspect;
            private readonly Rect rect;
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color backgroundColor;
            private readonly bool allowHdr;
            private readonly bool allowMsaa;
            private readonly RenderTexture targetTexture;

            public CameraState(Camera camera)
            {
                position = camera.transform.position;
                rotation = camera.transform.rotation;
                orthographic = camera.orthographic;
                orthographicSize = camera.orthographicSize;
                nearClipPlane = camera.nearClipPlane;
                farClipPlane = camera.farClipPlane;
                aspect = camera.aspect;
                rect = camera.rect;
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                backgroundColor = camera.backgroundColor;
                allowHdr = camera.allowHDR;
                allowMsaa = camera.allowMSAA;
                targetTexture = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                camera.targetTexture = targetTexture;
                camera.transform.SetPositionAndRotation(position, rotation);
                camera.orthographic = orthographic;
                camera.orthographicSize = orthographicSize;
                camera.nearClipPlane = nearClipPlane;
                camera.farClipPlane = farClipPlane;
                camera.aspect = aspect;
                camera.rect = rect;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
                camera.ResetProjectionMatrix();
            }
        }

        [Serializable]
        private sealed class ValidationReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string sourceTexturePath = string.Empty;
            public string imagePath = string.Empty;
            public bool controllerBuilt;
            public int actorCount;
            public int healthBarCount;
            public int healthQuadCount;
            public int nonClearPixelCount;
            public int redDominantPixelCount;
            public int greenSeparatorPixelCount;
            public bool sceneDirtyUnchanged;
            public bool usedCharacterManagerSprite;
            public bool usedProcessedSourceSheet;
        }
    }
}
#endif
