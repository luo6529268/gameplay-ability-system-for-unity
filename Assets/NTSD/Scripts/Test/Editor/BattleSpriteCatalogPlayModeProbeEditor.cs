#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Explicit Editor-only audit of the C++ Release generic DAT sprite mapping
    /// contract against every currently loaded Unity battle catalog entry.
    /// </summary>
    public static class BattleSpriteCatalogPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/R8/运行全DAT精灵映射Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01D_02_SpriteCatalog.result.json";
        private const int ReadyTimeoutEditorUpdates = 900;
        private const int MaximumRecordedDifferences = 64;
        private const int ProbeSourceObjectId = 739002;
        private const float RectEpsilon = 0.001f;

        private static readonly Dictionary<string, SheetBindingState> SheetBindings =
            new Dictionary<string, SheetBindingState>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BMPLoader.BmpData> SourceBmpData =
            new Dictionary<string, BMPLoader.BmpData>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<DifferenceRow> Differences =
            new List<DifferenceRow>(MaximumRecordedDifferences);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static CharacterAnimtorManager manager;
        private static GenericStateTransformProbeEntity probeEntity;
        private static ProbeReport report;
        private static ProbePhase phase;
        private static int editorUpdates;
        private static bool previousPaused;
        private static bool running;

        [MenuItem(MenuPath)]
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
            if (driver == null || world == null || manager == null || GameDataManager.Instance == null)
            {
                WriteImmediateFailure("The production driver, world, or data/catalog managers are unavailable.");
                return;
            }

            report = new ProbeReport
            {
                status = "RUNNING",
                message = string.Empty,
                backendMode = world.BattlePresentation.Mode.ToString(),
                startTick = driver.CurrentTickIndex,
                baselineObjectCount = world.ObjectCount,
                baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics,
                workerWasActive = driver.DedicatedSimulationWorkerActiveForDiagnostics,
            };
            phase = ProbePhase.WaitingForBattleReady;
            running = true;
            EditorApplication.update += Observe;
            Debug.Log("[BattleSpriteCatalogPlayModeProbe] Waiting for the production battle catalog and world.");
        }

        [MenuItem("NTSD/Battle Diagnostics/R8/Run All DAT Sprite Mapping Play Probe")]
        public static void RunFromAsciiMenu()
        {
            RunFromMenu();
        }

        private static void Observe()
        {
            if (!running)
                return;

            if (!EditorApplication.isPlaying || driver == null || world == null || manager == null)
            {
                FinishFailure("Play Mode or the production runtime ended before the audit completed.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > ReadyTimeoutEditorUpdates)
            {
                FinishFailure($"Timed out in phase {phase}.");
                return;
            }

            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                FinishFailure(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            try
            {
                switch (phase)
                {
                    case ProbePhase.WaitingForBattleReady:
                        if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                            manager.SpriteCatalog == null || manager.SpriteCatalog.Count <= 0 ||
                            manager.GetAllLoadedCharacterIds().Count <= 0)
                        {
                            return;
                        }

                        previousPaused = driver.IsPaused;
                        driver.SetPaused(true);
                        phase = ProbePhase.WaitingForWorkerIdle;
                        editorUpdates = 0;
                        return;

                    case ProbePhase.WaitingForWorkerIdle:
                        if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                            return;
                        report.startTick = driver.CurrentTickIndex;
                        report.baselineObjectCount = world.ObjectCount;
                        report.baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                        report.workerWasActive = driver.DedicatedSimulationWorkerActiveForDiagnostics;
                        ExecuteAudit();
                        return;
                }
            }
            catch (Exception exception)
            {
                FinishFailure($"Unhandled audit exception: {exception}");
            }
        }

        private static void ExecuteAudit()
        {
            BattleSpriteCatalog catalog = manager.SpriteCatalog;
            report.auditTick = driver.CurrentTickIndex;
            report.catalogCount = catalog.Count;
            report.backendMode = world.BattlePresentation.Mode.ToString();

            AuditCatalogEntries(catalog);
            AuditAuthoredFrames(catalog);
            ExecuteDynamicState8000CommandWitness(catalog);

            report.differences = Differences.ToArray();
            report.recordedDifferenceCount = Differences.Count;
            report.status = report.differenceCount == 0 ? "PASS" : "FAIL";
            report.message = report.differenceCount == 0
                ? "All loaded DAT sprite descriptors matched the C++ source contract; " +
                  $"state8000Witness={report.state8000WitnessStatus}."
                : $"Found {report.differenceCount} generic sprite mapping difference(s); see the structured rows.";

            CleanupAndFinish();
        }

        private static void AuditCatalogEntries(BattleSpriteCatalog catalog)
        {
            SheetBindings.Clear();
            List<BattleSpriteKey> keys = new List<BattleSpriteKey>(catalog.Entries.Keys);
            keys.Sort(CompareKeys);
            report.auditedCatalogEntryCount = keys.Count;

            for (int keyIndex = 0; keyIndex < keys.Count; keyIndex++)
            {
                BattleSpriteKey key = keys[keyIndex];
                BattleSpriteEntry entry = catalog.Entries[key];
                LF2CharacterData data = manager.GetCharacterData(key.VisualDataId);
                if (data == null)
                {
                    AddDifference(
                        "CATALOG_KEY_WITHOUT_LOADED_DAT",
                        key.VisualDataId,
                        -1,
                        key.EffectivePic,
                        key.EffectivePic,
                        null,
                        entry?.SourceSheetPath,
                        Rect.zero,
                        entry?.PixelRect ?? Rect.zero,
                        "Catalog key has no loaded LF2CharacterData.");
                    continue;
                }

                SpriteFileInfo fileInfo = FindFirstRange(data, key.EffectivePic);
                if (fileInfo == null)
                {
                    AddDifference(
                        "CATALOG_KEY_OUTSIDE_CPP_RANGE",
                        key.VisualDataId,
                        -1,
                        key.EffectivePic,
                        key.EffectivePic,
                        null,
                        entry?.SourceSheetPath,
                        Rect.zero,
                        entry?.PixelRect ?? Rect.zero,
                        "C++ declared-order range selection has no matching file range.");
                    continue;
                }

                ValidateCatalogEntry(key, entry, fileInfo);
            }
        }

        private static void ValidateCatalogEntry(
            BattleSpriteKey key,
            BattleSpriteEntry entry,
            SpriteFileInfo fileInfo)
        {
            if (entry == null || entry.SharedTexture == null || entry.LegacySprite == null)
            {
                AddDifference(
                    "CATALOG_ENTRY_RESOURCE_MISSING",
                    key.VisualDataId,
                    -1,
                    key.EffectivePic,
                    key.EffectivePic,
                    fileInfo,
                    entry?.SourceSheetPath,
                    Rect.zero,
                    entry?.PixelRect ?? Rect.zero,
                    "SharedTexture and LegacySprite are required by the published descriptor.");
                return;
            }

            int localPic = key.EffectivePic - fileInfo.startFrame;
            Rect requestedRect = ComputeCppSourceRect(fileInfo, entry.SharedTexture.height, localPic);
            Rect expectedRect = ClipToTexture(
                requestedRect,
                entry.SharedTexture.width,
                entry.SharedTexture.height);
            Vector2 expectedPivot = ComputeClippedPivot(fileInfo, requestedRect, expectedRect);
            Rect actualRect = entry.PixelRect;
            string expectedPath = NormalizePath(fileInfo.filePath);
            string actualPath = NormalizePath(entry.SourceSheetPath);
            if (!string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase) ||
                !RectApproximately(expectedRect, actualRect) ||
                (expectedPivot - entry.Pivot).sqrMagnitude > RectEpsilon * RectEpsilon)
            {
                AddDifference(
                    "CPP_SOURCE_DESCRIPTOR_MISMATCH",
                    key.VisualDataId,
                    -1,
                    key.EffectivePic,
                    key.EffectivePic,
                    fileInfo,
                    entry.SourceSheetPath,
                    expectedRect,
                    actualRect,
                    $"expectedPath='{expectedPath}', actualPath='{actualPath}', " +
                    $"expectedPivot={expectedPivot}, actualPivot={entry.Pivot}.");
            }

            if (!ReferenceEquals(entry.LegacySprite.texture, entry.SharedTexture) ||
                !RectApproximately(entry.LegacySprite.rect, actualRect))
            {
                AddDifference(
                    "LEGACY_DESCRIPTOR_MISMATCH",
                    key.VisualDataId,
                    -1,
                    key.EffectivePic,
                    key.EffectivePic,
                    fileInfo,
                    entry.SourceSheetPath,
                    actualRect,
                    entry.LegacySprite.rect,
                    "Legacy sprite must retain the same source texture and pixel rect as the immutable catalog entry.");
            }

            BattleSpriteCentralBinding binding = entry.CentralBinding;
            if (!binding.IsValid ||
                !Approximately(binding.AtlasContentPixelRect.width, actualRect.width) ||
                !Approximately(binding.AtlasContentPixelRect.height, actualRect.height))
            {
                AddDifference(
                    "CENTRAL_BINDING_INVALID",
                    key.VisualDataId,
                    -1,
                    key.EffectivePic,
                    key.EffectivePic,
                    fileInfo,
                    entry.SourceSheetPath,
                    actualRect,
                    binding.AtlasContentPixelRect,
                    $"mode={binding.Mode}, slice={binding.AtlasSlice}, page={binding.AtlasPageIndex}, uv={RectText(binding.NormalizedUv)}.");
                return;
            }

            switch (binding.Mode)
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

            ValidateSheetBinding(key, entry, binding);
        }

        private static void ValidateSheetBinding(
            BattleSpriteKey key,
            BattleSpriteEntry entry,
            BattleSpriteCentralBinding binding)
        {
            string path = NormalizePath(entry.SourceSheetPath);
            float deltaX = binding.AtlasContentPixelRect.x - entry.PixelRect.x;
            float deltaY = binding.AtlasContentPixelRect.y - entry.PixelRect.y;
            if (!SheetBindings.TryGetValue(path, out SheetBindingState state))
            {
                SheetBindings.Add(path, new SheetBindingState
                {
                    sourceWidth = entry.SharedTexture.width,
                    sourceHeight = entry.SharedTexture.height,
                    centralTextureInstanceId = binding.Texture != null ? binding.Texture.GetInstanceID() : 0,
                    mode = binding.Mode,
                    atlasSlice = binding.AtlasSlice,
                    atlasPageIndex = binding.AtlasPageIndex,
                    deltaX = deltaX,
                    deltaY = deltaY,
                });
                report.distinctSourceSheetCount++;
                return;
            }

            bool consistent = state.sourceWidth == entry.SharedTexture.width &&
                              state.sourceHeight == entry.SharedTexture.height &&
                              state.centralTextureInstanceId ==
                                  (binding.Texture != null ? binding.Texture.GetInstanceID() : 0) &&
                              state.mode == binding.Mode &&
                              state.atlasSlice == binding.AtlasSlice &&
                              state.atlasPageIndex == binding.AtlasPageIndex &&
                              Approximately(state.deltaX, deltaX) &&
                              Approximately(state.deltaY, deltaY);
            if (!consistent)
            {
                AddDifference(
                    "SHEET_BINDING_NOT_STABLE",
                    key.VisualDataId,
                    -1,
                    key.EffectivePic,
                    key.EffectivePic,
                    null,
                    entry.SourceSheetPath,
                    entry.PixelRect,
                    binding.AtlasContentPixelRect,
                    $"expectedMode={state.mode}, actualMode={binding.Mode}, expectedSlice/Page={state.atlasSlice}/{state.atlasPageIndex}, " +
                    $"actualSlice/Page={binding.AtlasSlice}/{binding.AtlasPageIndex}, expectedDelta={state.deltaX},{state.deltaY}, " +
                    $"actualDelta={deltaX},{deltaY}.");
            }
        }

        private static void AuditAuthoredFrames(BattleSpriteCatalog catalog)
        {
            List<int> loadedIds = manager.GetAllLoadedCharacterIds();
            loadedIds.Sort();
            report.loadedDefinitionCount = loadedIds.Count;

            for (int idIndex = 0; idIndex < loadedIds.Count; idIndex++)
            {
                int visualDataId = loadedIds[idIndex];
                LF2CharacterData data = manager.GetCharacterData(visualDataId);
                if (data == null)
                    continue;

                report.auditedDefinitionCount++;
                if (data.files != null)
                    report.auditedRangeCount += data.files.Count;
                if (data.frames == null)
                    continue;

                for (int frameIndex = 0; frameIndex < data.frames.Count; frameIndex++)
                {
                    LF2FrameData frame = data.frames[frameIndex];
                    if (frame == null)
                        continue;
                    report.auditedFrameCount++;
                    if (frame.pic == 999)
                    {
                        report.hiddenFrameCount++;
                        continue;
                    }

                    report.visibleFrameCount++;
                    SpriteFileInfo fileInfo = FindFirstRange(data, frame.pic);
                    bool hasCatalogEntry = catalog.TryGet(
                        visualDataId,
                        frame.pic,
                        out BattleSpriteEntry entry);
                    if (fileInfo == null)
                    {
                        report.cppRangeMissFrameCount++;
                        if (hasCatalogEntry)
                        {
                            AddDifference(
                                "UNITY_DRAWS_CPP_RANGE_MISS",
                                visualDataId,
                                frame.frameId,
                                frame.pic,
                                frame.pic,
                                null,
                                entry?.SourceSheetPath,
                                Rect.zero,
                                entry?.PixelRect ?? Rect.zero,
                                "C++ range lookup would skip this frame, but Unity published a catalog entry.");
                        }
                        continue;
                    }

                    if (!hasCatalogEntry || entry == null || entry.SharedTexture == null)
                    {
                        Rect missingExpectedRect = Rect.zero;
                        string normalizedPath = NormalizePath(fileInfo.filePath);
                        if ((!hasCatalogEntry || entry == null) &&
                            SheetBindings.TryGetValue(normalizedPath, out SheetBindingState sheetState))
                        {
                            int missingLocalPic = frame.pic - fileInfo.startFrame;
                            missingExpectedRect = ComputeCppSourceRect(
                                fileInfo,
                                sheetState.sourceHeight,
                                missingLocalPic);
                            if (!RectOverlapsTexture(
                                    missingExpectedRect,
                                    sheetState.sourceWidth,
                                    sheetState.sourceHeight))
                            {
                                report.fullyOutsideSourceFrameCount++;
                                continue;
                            }

                            if (!HasVisibleCppPixels(
                                    fileInfo.filePath,
                                    missingExpectedRect,
                                    sheetState.sourceWidth,
                                    sheetState.sourceHeight))
                            {
                                report.colorKeyOnlyMissingFrameCount++;
                                continue;
                            }
                        }

                        AddDifference(
                            "VISIBLE_FRAME_CATALOG_ENTRY_MISSING",
                            visualDataId,
                            frame.frameId,
                            frame.pic,
                            frame.pic,
                            fileInfo,
                            fileInfo.filePath,
                            missingExpectedRect,
                            Rect.zero,
                            "C++ source rect still overlaps the source sheet, but Unity has no immutable catalog entry.");
                        continue;
                    }

                    int localPic = frame.pic - fileInfo.startFrame;
                    Rect requested = ComputeCppSourceRect(fileInfo, entry.SharedTexture.height, localPic);
                    if (!RectWithinTexture(requested, entry.SharedTexture.width, entry.SharedTexture.height))
                    {
                        report.referencedClippedRectCount++;
                    }
                }
            }
        }

        private static void ExecuteDynamicState8000CommandWitness(BattleSpriteCatalog catalog)
        {
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
            {
                AddDifference(
                    "PRODUCTION_BACKEND_NOT_CENTRAL_ONLY",
                    -1,
                    -1,
                    -1,
                    -1,
                    null,
                    string.Empty,
                    Rect.zero,
                    Rect.zero,
                    $"actual={world.BattlePresentation.Mode}.");
                return;
            }

            if (!TrySelectState8000Candidate(catalog, out State8000Candidate candidate))
            {
                if (report.authoredState8000FrameCount == 0)
                {
                    report.state8000WitnessStatus = "SKIPPED_NO_AUTHORED_SOURCE";
                    return;
                }

                report.state8000WitnessStatus = "FAILED_NO_COMPLETE_CANDIDATE";
                AddDifference(
                    "NO_DYNAMIC_STATE8000_CANDIDATE",
                    -1,
                    -1,
                    -1,
                    -1,
                    null,
                    string.Empty,
                    Rect.zero,
                    Rect.zero,
                    $"authored={report.authoredState8000FrameCount}, loadedTarget={report.state8000LoadedTargetDefinitionCount}, " +
                    $"visibleFrame0={report.state8000VisibleTargetFrame0Count}, effectiveCatalog={report.state8000EffectiveCatalogEntryCount}.");
                return;
            }

            report.state8000WitnessStatus = "SELECTED";
            report.state8000SourceDefinitionId = candidate.sourceVisualDataId;
            report.state8000SourceFrameId = candidate.sourceFrameId;
            report.state8000TargetVisualDataId = candidate.targetVisualDataId;
            report.state8000RawPic = candidate.rawPic;
            report.state8000EffectivePic = candidate.effectivePic;

            probeEntity = new GenericStateTransformProbeEntity(
                ProbeSourceObjectId,
                candidate.targetVisualDataId);
            world.Register(probeEntity);
            if (probeEntity.Runtime == null || probeEntity.Runtime.SlotIndex < 0 ||
                !world.TryGetCurrentRuntimeHandleForDiagnostics(
                    probeEntity.Runtime.SlotIndex,
                    probeEntity,
                    out RuntimeEntityHandle handle) ||
                !handle.IsValid)
            {
                report.state8000WitnessStatus = "FAILED_REGISTRATION";
                AddDifference(
                    "DYNAMIC_PROBE_REGISTRATION_FAILED",
                    candidate.targetVisualDataId,
                    0,
                    candidate.rawPic,
                    candidate.effectivePic,
                    null,
                    candidate.entry.SourceSheetPath,
                    candidate.entry.PixelRect,
                    Rect.zero,
                    "The generic probe did not acquire a current runtime handle.");
                return;
            }

            probeEntity.RunStateSpecialPreCollision();
            bool transformCorrect =
                LF2Entity.ResolveCurrentDataObjectId(probeEntity) == candidate.targetVisualDataId &&
                probeEntity.Frame.N == 0 &&
                probeEntity.Runtime.RenderPicOffset == 140 &&
                probeEntity.HitStun == 0 &&
                probeEntity.GetRenderPicIndex() == candidate.effectivePic;
            if (!transformCorrect)
            {
                report.state8000WitnessStatus = "FAILED_TRANSFORM";
                AddDifference(
                    "DYNAMIC_STATE8000_TRANSFORM_MISMATCH",
                    candidate.targetVisualDataId,
                    0,
                    candidate.rawPic,
                    probeEntity.GetRenderPicIndex(),
                    null,
                    candidate.entry.SourceSheetPath,
                    candidate.entry.PixelRect,
                    Rect.zero,
                    $"currentData={LF2Entity.ResolveCurrentDataObjectId(probeEntity)}, frame={probeEntity.Frame.N}, " +
                    $"offset={probeEntity.Runtime.RenderPicOffset}, hitStop={probeEntity.HitStun}.");
                return;
            }

            world.RenderDispatchAll(driver.CurrentTickIndex, buildPresentation: true);
            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            bool snapshotFound = TryFindSnapshot(frame, handle, out BattlePresentationEntitySnapshot snapshot);
            bool commandFound = TryFindEntityCommand(frame, handle, out BattleRenderCommand command);
            BattleVisualResourceKey expectedLogicalKey =
                BattleVisualResourceKey.FromEntity(
                    new BattleSpriteKey(candidate.targetVisualDataId, candidate.effectivePic));
            bool commandCorrect = snapshotFound && commandFound &&
                                  snapshot.CurrentDatObjectId == candidate.targetVisualDataId &&
                                  snapshot.EffectivePic == candidate.effectivePic &&
                                  snapshot.HasCatalogKey &&
                                  command.VisualDataId == candidate.targetVisualDataId &&
                                  command.EffectivePic == candidate.effectivePic &&
                                  command.SpriteDescriptor.HasLogicalResourceKey &&
                                  command.SpriteDescriptor.LogicalResourceKey == expectedLogicalKey &&
                                  MatchesCatalogDescriptor(
                                      candidate.entry,
                                      command.SpriteDescriptor,
                                      expectedLogicalKey) &&
                                  RectApproximately(command.NormalizedUv, candidate.entry.NormalizedUv) &&
                                  ReferenceEquals(frame.BoundCatalogForAcceptance, catalog);
            report.dynamicSnapshotFound = snapshotFound;
            report.dynamicEntityCommandFound = commandFound;
            report.dynamicCommandMatched = commandCorrect;
            report.state8000WitnessStatus = commandCorrect ? "PASS" : "FAILED_COMMAND";
            if (!commandCorrect)
            {
                AddDifference(
                    "DYNAMIC_CENTRAL_COMMAND_MISMATCH",
                    candidate.targetVisualDataId,
                    0,
                    candidate.rawPic,
                    candidate.effectivePic,
                    null,
                    candidate.entry.SourceSheetPath,
                    candidate.entry.PixelRect,
                    commandFound ? command.SpriteDescriptor.PixelRect : Rect.zero,
                    $"snapshotFound={snapshotFound}, commandFound={commandFound}, " +
                    $"snapshotKey={snapshot.CurrentDatObjectId}/{snapshot.EffectivePic}, " +
                    $"commandKey={command.VisualDataId}/{command.EffectivePic}.");
            }
        }

        private static bool TrySelectState8000Candidate(
            BattleSpriteCatalog catalog,
            out State8000Candidate candidate)
        {
            List<int> loadedIds = manager.GetAllLoadedCharacterIds();
            loadedIds.Sort();
            for (int idIndex = 0; idIndex < loadedIds.Count; idIndex++)
            {
                int sourceId = loadedIds[idIndex];
                LF2CharacterData sourceData = manager.GetCharacterData(sourceId);
                if (sourceData?.frames == null)
                    continue;

                List<LF2FrameData> frames = new List<LF2FrameData>(sourceData.frames);
                frames.Sort((left, right) =>
                    (left?.frameId ?? int.MaxValue).CompareTo(right?.frameId ?? int.MaxValue));
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    LF2FrameData sourceFrame = frames[frameIndex];
                    int state = sourceFrame?.state ?? -1;
                    if (state < 8000 || state >= 9000)
                        continue;

                    report.authoredState8000FrameCount++;
                    int targetId = state - 8000;
                    if (manager.GetCharacterData(targetId) == null)
                        continue;
                    report.state8000LoadedTargetDefinitionCount++;
                    LF2FrameData targetFrame = manager.GetFrameData(targetId, 0);
                    if (targetFrame == null || targetFrame.pic < 0 || targetFrame.pic == 999)
                        continue;
                    report.state8000VisibleTargetFrame0Count++;
                    int effectivePic = targetFrame.pic + 140;
                    if (!catalog.TryGet(targetId, effectivePic, out BattleSpriteEntry entry) || entry == null)
                        continue;
                    report.state8000EffectiveCatalogEntryCount++;

                    candidate = new State8000Candidate
                    {
                        sourceVisualDataId = sourceId,
                        sourceFrameId = sourceFrame.frameId,
                        targetVisualDataId = targetId,
                        rawPic = targetFrame.pic,
                        effectivePic = effectivePic,
                        entry = entry,
                    };
                    return true;
                }
            }

            candidate = default;
            return false;
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            if (frame != null)
            {
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                    if (candidate.Handle == handle)
                    {
                        snapshot = candidate;
                        return true;
                    }
                }
            }

            snapshot = default;
            return false;
        }

        private static bool TryFindEntityCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattleRenderCommand command)
        {
            if (frame != null)
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand candidate = frame.GetCommand(index);
                    if (candidate.Handle == handle && candidate.Type == BattleRenderCommandType.Entity)
                    {
                        command = candidate;
                        return true;
                    }
                }
            }

            command = default;
            return false;
        }

        private static SpriteFileInfo FindFirstRange(LF2CharacterData data, int effectivePic)
        {
            if (data?.files == null)
                return null;
            for (int index = 0; index < data.files.Count; index++)
            {
                SpriteFileInfo fileInfo = data.files[index];
                if (fileInfo != null && effectivePic >= fileInfo.startFrame && effectivePic <= fileInfo.endFrame)
                    return fileInfo;
            }
            return null;
        }

        private static Rect ComputeCppSourceRect(SpriteFileInfo fileInfo, int textureHeight, int localPic)
        {
            if (fileInfo == null || fileInfo.row <= 0)
                return Rect.zero;
            int strideX = fileInfo.width + 1;
            int strideY = fileInfo.height + 1;
            int x = (localPic % fileInfo.row) * strideX;
            int rowFromTop = localPic / fileInfo.row;
            int y = textureHeight - rowFromTop * strideY - fileInfo.height;
            return new Rect(x, y, fileInfo.width, fileInfo.height);
        }

        private static bool RectWithinTexture(Rect rect, int width, int height)
        {
            return rect.x >= 0f && rect.y >= 0f &&
                   rect.width > 0f && rect.height > 0f &&
                   rect.xMax <= width && rect.yMax <= height;
        }

        private static bool RectOverlapsTexture(Rect rect, int width, int height)
        {
            return width > 0 && height > 0 &&
                   rect.width > 0f && rect.height > 0f &&
                   rect.x < width && rect.y < height &&
                   rect.xMax > 0f && rect.yMax > 0f;
        }

        private static Rect ClipToTexture(Rect rect, int width, int height)
        {
            float xMin = Mathf.Max(0f, rect.xMin);
            float yMin = Mathf.Max(0f, rect.yMin);
            float xMax = Mathf.Min(width, rect.xMax);
            float yMax = Mathf.Min(height, rect.yMax);
            return xMin < xMax && yMin < yMax
                ? Rect.MinMaxRect(xMin, yMin, xMax, yMax)
                : Rect.zero;
        }

        private static Vector2 ComputeClippedPivot(
            SpriteFileInfo fileInfo,
            Rect requestedRect,
            Rect clippedRect)
        {
            if (fileInfo == null || clippedRect.width <= 0f || clippedRect.height <= 0f)
                return new Vector2(0.5f, 0f);
            float offsetX = clippedRect.x - requestedRect.x;
            float offsetY = clippedRect.y - requestedRect.y;
            return new Vector2(
                (fileInfo.width * 0.5f - offsetX) / clippedRect.width,
                -offsetY / clippedRect.height);
        }

        private static bool HasVisibleCppPixels(
            string sourcePath,
            Rect sourceRect,
            int expectedWidth,
            int expectedHeight)
        {
            string path = NormalizePath(sourcePath);
            if (!SourceBmpData.TryGetValue(path, out BMPLoader.BmpData data))
            {
                data = BMPLoader.LoadBmpData(sourcePath);
                SourceBmpData[path] = data;
            }

            if (data?.Pixels == null || data.Width != expectedWidth || data.Height != expectedHeight)
                return true;

            int minX = Mathf.Max(0, Mathf.CeilToInt(sourceRect.x));
            int minY = Mathf.Max(0, Mathf.CeilToInt(sourceRect.y));
            int maxX = Mathf.Min(data.Width, Mathf.CeilToInt(sourceRect.xMax));
            int maxY = Mathf.Min(data.Height, Mathf.CeilToInt(sourceRect.yMax));
            for (int y = minY; y < maxY; y++)
            {
                int rowStart = y * data.Width;
                for (int x = minX; x < maxX; x++)
                {
                    Color pixel = data.Pixels[rowStart + x];
                    if (pixel.r > 0.0001f || pixel.g > 0.0001f || pixel.b > 0.0001f)
                        return true;
                }
            }

            return false;
        }

        private static bool RectApproximately(Rect left, Rect right)
        {
            return Approximately(left.x, right.x) &&
                   Approximately(left.y, right.y) &&
                   Approximately(left.width, right.width) &&
                   Approximately(left.height, right.height);
        }

        private static bool MatchesCatalogDescriptor(
            BattleSpriteEntry entry,
            in BattleSpriteValueDescriptor descriptor,
            BattleVisualResourceKey expectedLogicalKey)
        {
            return entry != null &&
                   descriptor.RequiresSprite &&
                   descriptor.HasSprite &&
                   descriptor.HasLogicalResourceKey &&
                   descriptor.LogicalResourceKey == expectedLogicalKey &&
                   descriptor.SpriteInstanceId ==
                       (entry.LegacySprite != null ? entry.LegacySprite.GetInstanceID() : 0) &&
                   descriptor.TextureInstanceId ==
                       (entry.SharedTexture != null ? entry.SharedTexture.GetInstanceID() : 0) &&
                   descriptor.MaterialInstanceId == 0 &&
                   RectApproximately(descriptor.PixelRect, entry.PixelRect) &&
                   (descriptor.PivotNormalized - entry.Pivot).sqrMagnitude <=
                       RectEpsilon * RectEpsilon;
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= RectEpsilon;
        }

        private static int CompareKeys(BattleSpriteKey left, BattleSpriteKey right)
        {
            int visualComparison = left.VisualDataId.CompareTo(right.VisualDataId);
            return visualComparison != 0
                ? visualComparison
                : left.EffectivePic.CompareTo(right.EffectivePic);
        }

        private static string NormalizePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            try
            {
                return Path.GetFullPath(value).Replace('\\', '/');
            }
            catch
            {
                return value.Replace('\\', '/');
            }
        }

        private static string RangeText(SpriteFileInfo fileInfo)
        {
            return fileInfo == null
                ? string.Empty
                : $"{fileInfo.startFrame}-{fileInfo.endFrame};w={fileInfo.width};h={fileInfo.height};" +
                  $"row={fileInfo.row};col={fileInfo.col}";
        }

        private static string RectText(Rect rect)
        {
            return $"{rect.x:R},{rect.y:R},{rect.width:R},{rect.height:R}";
        }

        private static void AddDifference(
            string reason,
            int visualDataId,
            int frameId,
            int rawPic,
            int effectivePic,
            SpriteFileInfo fileInfo,
            string sourcePath,
            Rect expectedRect,
            Rect actualRect,
            string detail)
        {
            report.differenceCount++;
            if (Differences.Count >= MaximumRecordedDifferences)
                return;
            Differences.Add(new DifferenceRow
            {
                reason = reason ?? string.Empty,
                visualDataId = visualDataId,
                frameId = frameId,
                rawPic = rawPic,
                effectivePic = effectivePic,
                range = RangeText(fileInfo),
                sourcePath = sourcePath ?? string.Empty,
                expectedRect = RectText(expectedRect),
                actualRect = RectText(actualRect),
                detail = detail ?? string.Empty,
            });
        }

        private static void CleanupAndFinish()
        {
            try
            {
                if (probeEntity != null && world != null)
                    world.Unregister(probeEntity);
                probeEntity = null;
                if (world != null && driver != null)
                    world.RenderDispatchAll(driver.CurrentTickIndex, buildPresentation: true);
            }
            catch (Exception exception)
            {
                report.cleanupException = exception.ToString();
                report.differenceCount++;
                report.status = "FAIL";
                report.message = "Cleanup failed after the sprite catalog audit.";
            }

            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.cleanupRestored =
                report.cleanupException == string.Empty &&
                report.afterObjectCount == report.baselineObjectCount &&
                report.afterClaimedSlots == report.baselineClaimedSlots;
            if (!report.cleanupRestored)
            {
                report.status = "FAIL";
                report.message =
                    $"Cleanup did not restore world counts: objects {report.baselineObjectCount}->{report.afterObjectCount}, " +
                    $"claimed {report.baselineClaimedSlots}->{report.afterClaimedSlots}.";
            }
            report.differences = Differences.ToArray();
            report.recordedDifferenceCount = Differences.Count;
            WriteResult(report);
            Debug.Log(
                $"[BattleSpriteCatalogPlayModeProbe] {report.status}: " +
                $"definitions={report.auditedDefinitionCount}, frames={report.auditedFrameCount}, " +
                $"catalog={report.auditedCatalogEntryCount}, differences={report.differenceCount}, " +
                $"dynamicCommand={report.dynamicCommandMatched}, cleanup={report.cleanupRestored}.");
            StopObservation();
        }

        private static void FinishFailure(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.message = message ?? string.Empty;
            try
            {
                if (probeEntity != null && world != null)
                    world.Unregister(probeEntity);
            }
            catch (Exception exception)
            {
                report.cleanupException = exception.ToString();
            }
            probeEntity = null;
            if (driver != null)
                driver.SetPaused(previousPaused);
            report.endTick = driver?.CurrentTickIndex ?? -1;
            report.afterObjectCount = world?.ObjectCount ?? -1;
            report.afterClaimedSlots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            report.differences = Differences.ToArray();
            report.recordedDifferenceCount = Differences.Count;
            WriteResult(report);
            Debug.LogError($"[BattleSpriteCatalogPlayModeProbe] FAIL: {report.message}");
            StopObservation();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeReport
            {
                status = "FAIL",
                message = message ?? string.Empty,
                cleanupException = string.Empty,
                differences = Array.Empty<DifferenceRow>(),
            });
            Debug.LogError($"[BattleSpriteCatalogPlayModeProbe] FAIL: {message}");
        }

        private static void WriteResult(ProbeReport value)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string path = Path.GetFullPath(Path.Combine(projectRoot, ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.Combine(projectRoot, "Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            SourceBmpData.Clear();
            running = false;
            phase = ProbePhase.None;
        }

        private static void ResetState()
        {
            SheetBindings.Clear();
            Differences.Clear();
            driver = null;
            world = null;
            manager = null;
            probeEntity = null;
            report = null;
            phase = ProbePhase.None;
            editorUpdates = 0;
            previousPaused = false;
            running = false;
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public string backendMode = string.Empty;
            public bool workerWasActive;
            public int startTick;
            public int auditTick;
            public int endTick;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int afterObjectCount;
            public int afterClaimedSlots;
            public bool cleanupRestored;
            public string cleanupException = string.Empty;
            public int catalogCount;
            public int loadedDefinitionCount;
            public int auditedDefinitionCount;
            public int auditedRangeCount;
            public int auditedFrameCount;
            public int visibleFrameCount;
            public int hiddenFrameCount;
            public int cppRangeMissFrameCount;
            public int referencedOutOfBoundsRectCount;
            public int referencedClippedRectCount;
            public int fullyOutsideSourceFrameCount;
            public int colorKeyOnlyMissingFrameCount;
            public int auditedCatalogEntryCount;
            public int distinctSourceSheetCount;
            public int sourceTexture2DBindingCount;
            public int textureArrayBindingCount;
            public int orderedPageBindingCount;
            public int differenceCount;
            public int recordedDifferenceCount;
            public DifferenceRow[] differences = Array.Empty<DifferenceRow>();
            public int state8000SourceDefinitionId = -1;
            public int state8000SourceFrameId = -1;
            public int state8000TargetVisualDataId = -1;
            public int state8000RawPic = -1;
            public int state8000EffectivePic = -1;
            public string state8000WitnessStatus = "NOT_RUN";
            public int authoredState8000FrameCount;
            public int state8000LoadedTargetDefinitionCount;
            public int state8000VisibleTargetFrame0Count;
            public int state8000EffectiveCatalogEntryCount;
            public bool dynamicSnapshotFound;
            public bool dynamicEntityCommandFound;
            public bool dynamicCommandMatched;
        }

        [Serializable]
        private sealed class DifferenceRow
        {
            public string reason = string.Empty;
            public int visualDataId;
            public int frameId;
            public int rawPic;
            public int effectivePic;
            public string range = string.Empty;
            public string sourcePath = string.Empty;
            public string expectedRect = string.Empty;
            public string actualRect = string.Empty;
            public string detail = string.Empty;
        }

        private sealed class SheetBindingState
        {
            public int sourceWidth;
            public int sourceHeight;
            public int centralTextureInstanceId;
            public BattleSpriteCentralBindingMode mode;
            public int atlasSlice;
            public int atlasPageIndex;
            public float deltaX;
            public float deltaY;
        }

        private struct State8000Candidate
        {
            public int sourceVisualDataId;
            public int sourceFrameId;
            public int targetVisualDataId;
            public int rawPic;
            public int effectivePic;
            public BattleSpriteEntry entry;
        }

        private sealed class GenericStateTransformProbeEntity : LF2OtherObject
        {
            public GenericStateTransformProbeEntity(int sourceObjectId, int targetObjectId)
            {
                Name = "R8GenericState8000SpriteProbe";
                ObjectId = sourceObjectId;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                var sourceFrame = new LF2FrameData
                {
                    frameId = 0,
                    state = 8000 + targetObjectId,
                    wait = 100,
                    next = 0,
                    pic = 999,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    sourceObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Other,
                        frames = new List<LF2FrameData> { sourceFrame },
                    }));
                Frame.D = sourceFrame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Frame.Prev2 = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.FirstPresentationTick = 0;
                Runtime.SetPosition(500, 0, 300);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
            }

            public override void SimFrameTick(int tickIndex)
            {
            }
        }

        private enum ProbePhase
        {
            None,
            WaitingForBattleReady,
            WaitingForWorkerIdle,
        }
    }
}
#endif
