#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Explicit Editor-only GPU audit for every published production sprite binding.
    /// This observes the approved logic-only/CentralOnly path and never requires an
    /// entity-owned GameObject or SpriteRenderer.
    /// </summary>
    public static class BattleSpriteCatalogGpuPlayModeProbeEditor
    {
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01D_05_SpriteCatalogGpu.result.json";
        private const string OutputRelativeDirectory = "Temp/R8-WP01D-05-GPU";
        private const int ReadyTimeoutEditorUpdates = 900;
        private const int MaximumRecordedDifferences = 64;
        private const int GpuWitnessImageSize = 256;
        private const int OffscreenLayer = 31;
        private const float PixelParityMeanTolerance = 3f;
        private const int PixelParityMaximumTolerance = 32;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
        private static readonly List<PixelDifferenceRow> Differences =
            new List<PixelDifferenceRow>(MaximumRecordedDifferences);
        private static readonly Dictionary<BindingReadbackKey, TextureReadback> BindingReadbacks =
            new Dictionary<BindingReadbackKey, TextureReadback>();

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static CharacterAnimtorManager manager;
        private static ProbeReport report;
        private static CatalogRow visiblePartialWitness;
        private static bool hasVisiblePartialWitness;
        private static int editorUpdates;
        private static bool previousPaused;
        private static bool running;

        [MenuItem("NTSD/Battle Diagnostics/R8/Run All Catalog GPU Pixel Play Probe")]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            manager = CharacterAnimtorManager.Instance;
            if (driver == null || world == null || manager == null)
            {
                WriteImmediateFailure("The production driver, world, or sprite manager is unavailable.");
                return;
            }

            report = new ProbeReport
            {
                status = "RUNNING",
                backendMode = world.BattlePresentation.Mode.ToString(),
                startTick = driver.CurrentTickIndex,
            };
            previousPaused = driver.IsPaused;
            running = true;
            EditorApplication.update += Observe;
            Debug.Log("[BattleSpriteCatalogGpuProbe] Waiting for the production catalog and worker boundary.");
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || world == null || manager == null)
            {
                FinishFailure("Play Mode or the production runtime ended before the GPU audit completed.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > ReadyTimeoutEditorUpdates)
            {
                FinishFailure("Timed out waiting for the production catalog or simulation worker.");
                return;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                FinishFailure(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }
            if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                manager.SpriteCatalog == null || manager.SpriteCatalog.Count <= 0)
            {
                return;
            }

            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                report.auditTick = driver.CurrentTickIndex;
                report.baselineObjectCount = world.ObjectCount;
                report.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                ExecuteAudit();
            }
            catch (Exception exception)
            {
                FinishFailure("Unhandled GPU audit exception: " + exception);
            }
        }

        private static void ExecuteAudit()
        {
            BattleSpriteCatalog catalog = manager.SpriteCatalog;
            report.catalogCount = catalog.Count;
            report.asyncGpuReadbackSupported = SystemInfo.supportsAsyncGPUReadback;
            if (!report.asyncGpuReadbackSupported)
            {
                FinishFailure("Async GPU readback is unsupported on the current graphics device.");
                return;
            }

            List<CatalogRow> rows = BuildSortedRows(catalog);
            AuditAllBindingPixels(rows);
            ExecuteDynamicMeshGpuWitness(rows, catalog);

            report.differenceCount = report.pixelBindingDifferenceCount +
                                     (report.gpuWitnessStatus == "PASS" ||
                                      report.gpuWitnessStatus == "SKIPPED_NO_VISIBLE_PARTIAL_ENTRY" ? 0 : 1);
            report.recordedDifferenceCount = Differences.Count;
            report.differences = Differences.ToArray();
            report.sourceAggregateHash = report.sourceHash.ToString("X16");
            report.centralAggregateHash = report.centralHash.ToString("X16");
            report.status = report.differenceCount == 0 ? "PASS" : "FAIL";
            report.message = report.differenceCount == 0
                ? $"All {report.auditedEntryCount} catalog entries matched source-to-binding GPU pixels; " +
                  $"gpuWitness={report.gpuWitnessStatus}."
                : $"Found {report.differenceCount} production catalog GPU difference(s).";
            CleanupAndFinish();
        }

        private static List<CatalogRow> BuildSortedRows(BattleSpriteCatalog catalog)
        {
            var rows = new List<CatalogRow>(catalog.Count);
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in catalog.Entries)
                rows.Add(new CatalogRow(pair.Key, pair.Value));
            rows.Sort((left, right) =>
            {
                int sourceComparison = StringComparer.OrdinalIgnoreCase.Compare(
                    left.Entry?.SourceSheetPath ?? string.Empty,
                    right.Entry?.SourceSheetPath ?? string.Empty);
                if (sourceComparison != 0)
                    return sourceComparison;
                int textureComparison = GetTextureId(left.Entry?.SharedTexture)
                    .CompareTo(GetTextureId(right.Entry?.SharedTexture));
                if (textureComparison != 0)
                    return textureComparison;
                int visualComparison = left.Key.VisualDataId.CompareTo(right.Key.VisualDataId);
                return visualComparison != 0
                    ? visualComparison
                    : left.Key.EffectivePic.CompareTo(right.Key.EffectivePic);
            });
            return rows;
        }

        private static void AuditAllBindingPixels(List<CatalogRow> rows)
        {
            Texture2D activeSource = null;
            TextureReadback sourceReadback = default;
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                CatalogRow row = rows[rowIndex];
                BattleSpriteEntry entry = row.Entry;
                report.auditedEntryCount++;
                if (entry == null || entry.SharedTexture == null || !entry.CentralBinding.IsValid)
                {
                    AddDifference(row, "RESOURCE_OR_BINDING_INVALID", -1, -1, default, default);
                    continue;
                }

                if (!ReferenceEquals(activeSource, entry.SharedTexture))
                {
                    activeSource = entry.SharedTexture;
                    if (!TryReadTexture(activeSource, 0, out sourceReadback, out string sourceError))
                    {
                        AddDifference(row, "SOURCE_GPU_READBACK_FAILED", -1, -1, default, default, sourceError);
                        activeSource = null;
                        continue;
                    }
                    report.sourceTextureReadbackCount++;
                }

                BattleSpriteCentralBinding binding = entry.CentralBinding;
                CountBindingMode(binding.Mode);
                TextureReadback centralReadback;
                if (binding.Mode == BattleSpriteCentralBindingMode.SourceTexture2D &&
                    ReferenceEquals(binding.Texture, entry.SharedTexture))
                {
                    centralReadback = sourceReadback;
                }
                else
                {
                    var bindingKey = new BindingReadbackKey(binding.Texture, binding.AtlasSlice);
                    if (!BindingReadbacks.TryGetValue(bindingKey, out centralReadback))
                    {
                        if (!TryReadTexture(
                                binding.Texture,
                                binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                                    ? binding.AtlasSlice
                                    : 0,
                                out centralReadback,
                                out string centralError))
                        {
                            AddDifference(row, "CENTRAL_GPU_READBACK_FAILED", -1, -1, default, default, centralError);
                            continue;
                        }
                        BindingReadbacks.Add(bindingKey, centralReadback);
                        report.centralTextureSliceReadbackCount++;
                    }
                }

                RectInt sourceRect = ToRectInt(entry.PixelRect);
                RectInt centralRect = ToRectInt(binding.AtlasContentPixelRect);
                if (sourceRect.width != centralRect.width || sourceRect.height != centralRect.height ||
                    !RectFits(sourceRect, sourceReadback.Width, sourceReadback.Height) ||
                    !RectFits(centralRect, centralReadback.Width, centralReadback.Height))
                {
                    AddDifference(row, "PIXEL_RECT_OUT_OF_READBACK_RANGE", -1, -1, default, default);
                    continue;
                }

                bool entryMatched = true;
                bool entryHasVisiblePixel = false;
                for (int y = 0; y < sourceRect.height; y++)
                {
                    int sourceRow = (sourceRect.y + y) * sourceReadback.Width + sourceRect.x;
                    int centralRow = (centralRect.y + y) * centralReadback.Width + centralRect.x;
                    for (int x = 0; x < sourceRect.width; x++)
                    {
                        Color32 sourcePixel = sourceReadback.Pixels[sourceRow + x];
                        Color32 centralPixel = centralReadback.Pixels[centralRow + x];
                        entryHasVisiblePixel |= sourcePixel.a != 0;
                        report.comparedPixelCount++;
                        report.sourceHash = HashPixel(report.sourceHash, sourcePixel);
                        report.centralHash = HashPixel(report.centralHash, centralPixel);
                        if (!ColorsEqual(sourcePixel, centralPixel))
                        {
                            if (entryMatched)
                            {
                                AddDifference(
                                    row,
                                    "SOURCE_TO_CENTRAL_PIXEL_MISMATCH",
                                    x,
                                    y,
                                    sourcePixel,
                                    centralPixel);
                                entryMatched = false;
                            }
                        }
                    }
                }

                if (entryMatched)
                    report.matchedEntryCount++;
                if (IsPartialEntry(entry))
                {
                    report.partialEntryCount++;
                    if (entryHasVisiblePixel && !hasVisiblePartialWitness)
                    {
                        visiblePartialWitness = row;
                        hasVisiblePartialWitness = true;
                    }
                }
            }
        }

        private static void ExecuteDynamicMeshGpuWitness(
            List<CatalogRow> rows,
            BattleSpriteCatalog catalog)
        {
            if (!hasVisiblePartialWitness)
            {
                report.gpuWitnessStatus = "SKIPPED_NO_VISIBLE_PARTIAL_ENTRY";
                return;
            }

            CatalogRow witness = visiblePartialWitness;
            BattleSpriteEntry entry = witness.Entry;
            Material material = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
            Material arrayMaterial = BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
            if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, false) ||
                (entry.CentralBinding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray &&
                 !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(arrayMaterial, true)))
            {
                report.gpuWitnessStatus = "FAIL_MATERIAL_CONTRACT";
                return;
            }

            var descriptor = new BattleSpriteValueDescriptor(
                true,
                entry.LegacySprite != null,
                entry.LegacySprite != null ? entry.LegacySprite.GetInstanceID() : 0,
                entry.SharedTexture != null ? entry.SharedTexture.GetInstanceID() : 0,
                0,
                entry.PixelRect,
                entry.Pivot,
                true,
                entry.Key);
            var command = new BattleRenderCommand(
                BattleRenderCommandType.Entity,
                new RuntimeEntityHandle(0, 1),
                1,
                entry.Key.VisualDataId,
                entry.Key.EffectivePic,
                0,
                0,
                0,
                0,
                0,
                Vector3.zero,
                entry.PixelRect.size,
                entry.Pivot,
                entry.NormalizedUv,
                BattleSpriteRenderState.Default(),
                descriptor);
            var frame = new BattlePresentationFrame();
            FrameAccess.Reset(frame, driver.CurrentTickIndex);
            FrameAccess.AddCommand(frame, command);

            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(catalog, BattleCommonVisualCatalog.Empty, material, arrayMaterial);
            using var backend = new BattleDynamicMeshBackend();
            backend.Build(frame, resolver);
            if (backend.Diagnostics.ResolvedCommandCount != 1 ||
                backend.SegmentCount != 1 || backend.ActiveChunkCount != 1)
            {
                report.gpuWitnessStatus = "FAIL_COMMAND_NOT_RESOLVED";
                return;
            }

            float halfExtent = ResolveCommandHalfExtent(command);
            Vector2 viewCenter = ResolveCommandVisualCenter(command, entry.Pivot);
            PixelImage legacy = RenderLegacy(
                command,
                entry,
                GpuWitnessImageSize,
                viewCenter,
                halfExtent);
            PixelImage central = RenderCentral(
                backend,
                GpuWitnessImageSize,
                viewCenter,
                halfExtent);
            PixelDifference difference = ComparePixels(legacy, central);
            string outputDirectory = ProjectPath(OutputRelativeDirectory);
            Directory.CreateDirectory(outputDirectory);
            WriteImage(outputDirectory, "partial-legacy.png", legacy);
            WriteImage(outputDirectory, "partial-central.png", central);
            WriteImage(outputDirectory, "partial-diff.png", difference.DifferenceImage);

            report.gpuWitnessVisualDataId = witness.Key.VisualDataId;
            report.gpuWitnessEffectivePic = witness.Key.EffectivePic;
            report.gpuWitnessBindingMode = entry.CentralBinding.Mode.ToString();
            report.gpuWitnessAtlasSlice = entry.CentralBinding.AtlasSlice;
            report.gpuWitnessPixelRect = RectText(entry.PixelRect);
            report.gpuWitnessPivot = VectorText(entry.Pivot);
            report.gpuWitnessLegacyPixels = legacy.NonTransparentPixelCount;
            report.gpuWitnessCentralPixels = central.NonTransparentPixelCount;
            report.gpuWitnessMeanDifference = difference.MeanChannelDifference;
            report.gpuWitnessMaximumDifference = difference.MaximumChannelDifference;
            report.gpuWitnessStatus =
                legacy.NonTransparentPixelCount > 0 &&
                central.NonTransparentPixelCount > 0 &&
                difference.MeanChannelDifference <= PixelParityMeanTolerance &&
                difference.MaximumChannelDifference <= PixelParityMaximumTolerance
                    ? "PASS"
                    : "FAIL_PIXEL_PARITY";
        }

        private static bool TryReadTexture(
            Texture texture,
            int slice,
            out TextureReadback readback,
            out string error)
        {
            readback = default;
            error = string.Empty;
            if (texture == null || texture.width <= 0 || texture.height <= 0)
            {
                error = "Texture is null or has invalid dimensions.";
                return false;
            }
            try
            {
                AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(
                    texture,
                    0,
                    0,
                    texture.width,
                    0,
                    texture.height,
                    slice,
                    1,
                    TextureFormat.RGBA32);
                request.WaitForCompletion();
                if (request.hasError)
                {
                    error = $"GPU readback failed for '{texture.name}', slice={slice}.";
                    return false;
                }
                NativeArray<Color32> data = request.GetData<Color32>();
                if (data.Length != texture.width * texture.height)
                {
                    error = $"GPU readback size mismatch for '{texture.name}', slice={slice}: " +
                            $"{data.Length} != {texture.width * texture.height}.";
                    return false;
                }
                readback = new TextureReadback(texture.width, texture.height, data.ToArray());
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static PixelImage RenderLegacy(
            in BattleRenderCommand command,
            BattleSpriteEntry entry,
            int imageSize,
            Vector2 viewCenter,
            float halfExtent)
        {
            GameObject root = null;
            Material material = null;
            try
            {
                Shader shader = Shader.Find(BattleSpriteMaterialContract.BuiltInSpriteShaderName);
                if (shader == null)
                    throw new InvalidOperationException("Sprites/Default shader is unavailable.");
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                root = new GameObject("R8 Catalog GPU Legacy Witness")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    layer = OffscreenLayer,
                };
                root.transform.position = command.Position;
                root.transform.localScale = new Vector3(
                    NTSDRenderSpace.BattleVisualScale * command.Size.x / entry.PixelWidth,
                    NTSDRenderSpace.BattleVisualScale * command.Size.y / entry.PixelHeight,
                    1f);
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = entry.LegacySprite;
                renderer.sharedMaterial = material;
                renderer.color = command.Color;
                return RenderCommandBuffer(
                    imageSize,
                    viewCenter,
                    halfExtent,
                    commandBuffer => commandBuffer.DrawRenderer(renderer, material));
            }
            finally
            {
                DestroyImmediateSafe(root);
                DestroyImmediateSafe(material);
            }
        }

        private static PixelImage RenderCentral(
            BattleDynamicMeshBackend backend,
            int imageSize,
            Vector2 viewCenter,
            float halfExtent)
        {
            return RenderCommandBuffer(imageSize, viewCenter, halfExtent, commandBuffer =>
            {
                var properties = new MaterialPropertyBlock();
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    properties.Clear();
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                        properties.SetTexture(MainTexArrayId, segment.Texture);
                    else
                        properties.SetTexture(MainTexId, segment.Texture);
                    commandBuffer.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        segment.SubMeshIndex,
                        0,
                        properties);
                }
            });
        }

        private static PixelImage RenderCommandBuffer(
            int imageSize,
            Vector2 viewCenter,
            float halfExtent,
            Action<CommandBuffer> enqueueDraws)
        {
            var target = new RenderTexture(
                imageSize,
                imageSize,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            target.Create();
            var commandBuffer = new CommandBuffer { name = "R8 Catalog GPU Witness" };
            RenderTexture previous = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                commandBuffer.SetRenderTarget(target);
                commandBuffer.SetViewport(new Rect(0f, 0f, imageSize, imageSize));
                commandBuffer.ClearRenderTarget(false, true, Color.clear);
                Matrix4x4 projection = GL.GetGPUProjectionMatrix(
                    Matrix4x4.Ortho(-halfExtent, halfExtent, -halfExtent, halfExtent, -10f, 10f),
                    true);
                Matrix4x4 view = Matrix4x4.Translate(
                    new Vector3(-viewCenter.x, -viewCenter.y, 0f));
                commandBuffer.SetViewProjectionMatrices(view, projection);
                enqueueDraws(commandBuffer);
                Graphics.ExecuteCommandBuffer(commandBuffer);
                RenderTexture.active = target;
                readback = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                readback.ReadPixels(new Rect(0f, 0f, imageSize, imageSize), 0, 0, false);
                Color32[] pixels = readback.GetPixels32();
                return new PixelImage(imageSize, imageSize, pixels, CountNonTransparent(pixels));
            }
            finally
            {
                RenderTexture.active = previous;
                commandBuffer.Release();
                DestroyImmediateSafe(readback);
                target.Release();
                DestroyImmediateSafe(target);
            }
        }

        private static PixelDifference ComparePixels(PixelImage left, PixelImage right)
        {
            var pixels = new Color32[left.Pixels.Length];
            long sum = 0;
            int maximum = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 a = left.Pixels[index];
                Color32 b = right.Pixels[index];
                int r = Math.Abs(a.r - b.r);
                int g = Math.Abs(a.g - b.g);
                int blue = Math.Abs(a.b - b.b);
                int alpha = Math.Abs(a.a - b.a);
                sum += r + g + blue + alpha;
                maximum = Math.Max(maximum, Math.Max(Math.Max(r, g), Math.Max(blue, alpha)));
                pixels[index] = new Color32((byte)r, (byte)g, (byte)blue, 255);
            }
            float mean = pixels.Length == 0 ? 0f : (float)(sum / (double)(pixels.Length * 4));
            return new PixelDifference(
                mean,
                maximum,
                new PixelImage(left.Width, left.Height, pixels, CountNonBlack(pixels)));
        }

        private static void WriteImage(string directory, string fileName, PixelImage image)
        {
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(image.Width, image.Height, TextureFormat.RGBA32, false, true);
                texture.SetPixels32(image.Pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.Combine(directory, fileName), texture.EncodeToPNG());
            }
            finally
            {
                DestroyImmediateSafe(texture);
            }
        }

        private static bool IsPartialEntry(BattleSpriteEntry entry)
        {
            if (entry == null)
                return false;
            LF2CharacterData data = manager.GetCharacterData(entry.Key.VisualDataId);
            if (data?.files == null)
                return false;
            for (int index = 0; index < data.files.Count; index++)
            {
                SpriteFileInfo file = data.files[index];
                if (file != null && entry.Key.EffectivePic >= file.startFrame &&
                    entry.Key.EffectivePic <= file.endFrame)
                {
                    return entry.PixelRect.width < file.width || entry.PixelRect.height < file.height ||
                           entry.Pivot.x < 0f || entry.Pivot.x > 1f ||
                           entry.Pivot.y < 0f || entry.Pivot.y > 1f;
                }
            }
            return false;
        }

        private static float ResolveCommandHalfExtent(in BattleRenderCommand command)
        {
            float width = command.Size.x * NTSDRenderSpace.BattleVisualScale /
                          SimulationConstants.PIXELS_PER_UNIT;
            float height = command.Size.y * NTSDRenderSpace.BattleVisualScale /
                           SimulationConstants.PIXELS_PER_UNIT;
            return Mathf.Max(0.25f, Mathf.Max(width, height) * 0.75f);
        }

        private static Vector2 ResolveCommandVisualCenter(
            in BattleRenderCommand command,
            Vector2 pivot)
        {
            float width = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                          NTSDRenderSpace.BattleVisualScale;
            float height = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                           NTSDRenderSpace.BattleVisualScale;
            return new Vector2(
                command.Position.x + (0.5f - pivot.x) * width,
                command.Position.y + (0.5f - pivot.y) * height);
        }

        private static void CountBindingMode(BattleSpriteCentralBindingMode mode)
        {
            switch (mode)
            {
                case BattleSpriteCentralBindingMode.SourceTexture2D:
                    report.sourceTexture2DBindingCount++;
                    break;
                case BattleSpriteCentralBindingMode.AtlasTextureArray:
                    report.textureArrayBindingCount++;
                    break;
                case BattleSpriteCentralBindingMode.AtlasPageTexture2D:
                    report.orderedPageBindingCount++;
                    break;
            }
        }

        private static void AddDifference(
            CatalogRow row,
            string reason,
            int localX,
            int localY,
            Color32 source,
            Color32 central,
            string detail = "")
        {
            report.pixelBindingDifferenceCount++;
            if (Differences.Count >= MaximumRecordedDifferences)
                return;
            Differences.Add(new PixelDifferenceRow
            {
                reason = reason ?? string.Empty,
                visualDataId = row.Key.VisualDataId,
                effectivePic = row.Key.EffectivePic,
                sourcePath = row.Entry?.SourceSheetPath ?? string.Empty,
                bindingMode = row.Entry?.CentralBinding.Mode.ToString() ?? string.Empty,
                atlasSlice = row.Entry?.CentralBinding.AtlasSlice ?? -1,
                sourceRect = RectText(row.Entry?.PixelRect ?? Rect.zero),
                centralRect = RectText(row.Entry?.CentralBinding.AtlasContentPixelRect ?? Rect.zero),
                localX = localX,
                localY = localY,
                sourceRgba = ColorText(source),
                centralRgba = ColorText(central),
                detail = detail ?? string.Empty,
            });
        }

        private static void CleanupAndFinish()
        {
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.cleanupRestored =
                report.afterObjectCount == report.baselineObjectCount &&
                report.afterClaimedSlots == report.baselineClaimedSlots;
            if (!report.cleanupRestored)
            {
                report.status = "FAIL";
                report.message =
                    $"Cleanup counts changed: objects {report.baselineObjectCount}->{report.afterObjectCount}, " +
                    $"claimed {report.baselineClaimedSlots}->{report.afterClaimedSlots}.";
            }
            WriteResult(report);
            Debug.Log(
                $"[BattleSpriteCatalogGpuProbe] {report.status}: entries={report.auditedEntryCount}, " +
                $"pixels={report.comparedPixelCount}, differences={report.differenceCount}, " +
                $"gpuWitness={report.gpuWitnessStatus}, cleanup={report.cleanupRestored}.");
            StopObservation();
        }

        private static void FinishFailure(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message ?? string.Empty;
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.recordedDifferenceCount = Differences.Count;
            report.differences = Differences.ToArray();
            WriteResult(report);
            Debug.LogError("[BattleSpriteCatalogGpuProbe] FAIL: " + report.message);
            StopObservation();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport { status = "FAIL", message = message ?? string.Empty });
            Debug.LogError("[BattleSpriteCatalogGpuProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeReport value)
        {
            string path = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(root, relativePath));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            BindingReadbacks.Clear();
            running = false;
        }

        private static void ResetState()
        {
            Differences.Clear();
            BindingReadbacks.Clear();
            driver = null;
            world = null;
            manager = null;
            report = null;
            visiblePartialWitness = default;
            hasVisiblePartialWitness = false;
            editorUpdates = 0;
            previousPaused = false;
            running = false;
        }

        private static RectInt ToRectInt(Rect rect)
        {
            return new RectInt(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                Mathf.RoundToInt(rect.width),
                Mathf.RoundToInt(rect.height));
        }

        private static bool RectFits(RectInt rect, int width, int height)
        {
            return rect.x >= 0 && rect.y >= 0 && rect.width > 0 && rect.height > 0 &&
                   rect.xMax <= width && rect.yMax <= height;
        }

        private static int GetTextureId(Texture texture)
        {
            return texture != null ? texture.GetInstanceID() : 0;
        }

        private static bool ColorsEqual(Color32 left, Color32 right)
        {
            return left.r == right.r && left.g == right.g &&
                   left.b == right.b && left.a == right.a;
        }

        private static ulong HashPixel(ulong hash, Color32 value)
        {
            if (hash == 0)
                hash = 14695981039346656037UL;
            hash = (hash ^ value.r) * 1099511628211UL;
            hash = (hash ^ value.g) * 1099511628211UL;
            hash = (hash ^ value.b) * 1099511628211UL;
            return (hash ^ value.a) * 1099511628211UL;
        }

        private static int CountNonTransparent(Color32[] pixels)
        {
            int count = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                if (pixels[index].a != 0)
                    count++;
            }
            return count;
        }

        private static int CountNonBlack(Color32[] pixels)
        {
            int count = 0;
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                if (pixel.r != 0 || pixel.g != 0 || pixel.b != 0)
                    count++;
            }
            return count;
        }

        private static string RectText(Rect rect)
        {
            return $"{rect.x:R},{rect.y:R},{rect.width:R},{rect.height:R}";
        }

        private static string VectorText(Vector2 value)
        {
            return $"{value.x:R},{value.y:R}";
        }

        private static string ColorText(Color32 value)
        {
            return $"{value.r},{value.g},{value.b},{value.a}";
        }

        private static void DestroyImmediateSafe(UnityEngine.Object value)
        {
            if (value != null)
                UnityEngine.Object.DestroyImmediate(value);
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string backendMode = string.Empty;
            public int startTick;
            public int auditTick;
            public int endTick;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int afterObjectCount;
            public int afterClaimedSlots;
            public bool cleanupRestored;
            public bool asyncGpuReadbackSupported;
            public int catalogCount;
            public int auditedEntryCount;
            public int matchedEntryCount;
            public int partialEntryCount;
            public int sourceTexture2DBindingCount;
            public int textureArrayBindingCount;
            public int orderedPageBindingCount;
            public int sourceTextureReadbackCount;
            public int centralTextureSliceReadbackCount;
            public long comparedPixelCount;
            [NonSerialized] public ulong sourceHash;
            [NonSerialized] public ulong centralHash;
            public string sourceAggregateHash = string.Empty;
            public string centralAggregateHash = string.Empty;
            public int pixelBindingDifferenceCount;
            public int differenceCount;
            public int recordedDifferenceCount;
            public PixelDifferenceRow[] differences = Array.Empty<PixelDifferenceRow>();
            public string gpuWitnessStatus = "NOT_RUN";
            public int gpuWitnessVisualDataId = -1;
            public int gpuWitnessEffectivePic = -1;
            public string gpuWitnessBindingMode = string.Empty;
            public int gpuWitnessAtlasSlice = -1;
            public string gpuWitnessPixelRect = string.Empty;
            public string gpuWitnessPivot = string.Empty;
            public int gpuWitnessLegacyPixels;
            public int gpuWitnessCentralPixels;
            public float gpuWitnessMeanDifference;
            public int gpuWitnessMaximumDifference;
        }

        [Serializable]
        private sealed class PixelDifferenceRow
        {
            public string reason = string.Empty;
            public int visualDataId;
            public int effectivePic;
            public string sourcePath = string.Empty;
            public string bindingMode = string.Empty;
            public int atlasSlice;
            public string sourceRect = string.Empty;
            public string centralRect = string.Empty;
            public int localX;
            public int localY;
            public string sourceRgba = string.Empty;
            public string centralRgba = string.Empty;
            public string detail = string.Empty;
        }

        private readonly struct CatalogRow
        {
            public CatalogRow(BattleSpriteKey key, BattleSpriteEntry entry)
            {
                Key = key;
                Entry = entry;
            }

            public BattleSpriteKey Key { get; }
            public BattleSpriteEntry Entry { get; }
        }

        private readonly struct BindingReadbackKey : IEquatable<BindingReadbackKey>
        {
            private readonly int textureId;
            private readonly int slice;

            public BindingReadbackKey(Texture texture, int slice)
            {
                textureId = GetTextureId(texture);
                this.slice = slice;
            }

            public bool Equals(BindingReadbackKey other)
            {
                return textureId == other.textureId && slice == other.slice;
            }

            public override bool Equals(object obj)
            {
                return obj is BindingReadbackKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return unchecked((textureId * 397) ^ slice);
            }
        }

        private readonly struct TextureReadback
        {
            public TextureReadback(int width, int height, Color32[] pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
            }

            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels { get; }
        }

        private readonly struct PixelImage
        {
            public PixelImage(int width, int height, Color32[] pixels, int nonTransparentPixelCount)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
                NonTransparentPixelCount = nonTransparentPixelCount;
            }

            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels { get; }
            public int NonTransparentPixelCount { get; }
        }

        private readonly struct PixelDifference
        {
            public PixelDifference(float mean, int maximum, PixelImage differenceImage)
            {
                MeanChannelDifference = mean;
                MaximumChannelDifference = maximum;
                DifferenceImage = differenceImage;
            }

            public float MeanChannelDifference { get; }
            public int MaximumChannelDifference { get; }
            public PixelImage DifferenceImage { get; }
        }

        private static class FrameAccess
        {
            private delegate void ResetDelegate(
                BattlePresentationFrame frame,
                int tickIndex,
                BattleCommonVisualCatalog commonVisualCatalog);

            private delegate void AddCommandDelegate(
                BattlePresentationFrame frame,
                in BattleRenderCommand command);

            private static readonly ResetDelegate ResetMethod =
                (ResetDelegate)typeof(BattlePresentationFrame)
                    .GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(ResetDelegate));

            private static readonly AddCommandDelegate AddCommandMethod =
                (AddCommandDelegate)typeof(BattlePresentationFrame)
                    .GetMethod("AddCommand", BindingFlags.Instance | BindingFlags.NonPublic)
                    .CreateDelegate(typeof(AddCommandDelegate));

            public static void Reset(BattlePresentationFrame frame, int tickIndex)
            {
                ResetMethod(frame, tickIndex, null);
            }

            public static void AddCommand(BattlePresentationFrame frame, in BattleRenderCommand command)
            {
                AddCommandMethod(frame, command);
            }
        }
    }
}
#endif
