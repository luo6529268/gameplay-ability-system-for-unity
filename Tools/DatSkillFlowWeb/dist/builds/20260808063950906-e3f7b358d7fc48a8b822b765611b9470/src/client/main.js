// dat-skill-flow-build:20260808063950906-e3f7b358d7fc48a8b822b765611b9470
import {
    findFrameFieldCapability,
    lastFrameForId,
    mergePreview,
    primaryPreviewEntity,
} from "./project-client.js";
import {
    buildSkillFlow,
    traceStartFrameForSelection,
                       
                        
} from "./skill-flow.js";
import { renderFlowSvg } from "./flow-svg.js";
import {
    deriveSkillEntries,
    entriesByStartFrame,
                              
                    
} from "./skill-entries.js";
import { buildSkillTimeline } from "./skill-timeline.js";
import {
    draftOverlayGeometry,
    hitResizeHandle,
    moveDatPoint,
    resizeDatRect,
    snapDelta,
                      
} from "./canvas-geometry-edit.js";
import {
    hitTestOverlay,
                         
                     
} from "./overlay-geometry.js";
import {
    drawPreviewCanvas,
    preloadPreviewObjectAssets,
                      
                       
                     
} from "./preview-renderer.js";
import { createLatestTaskScheduler } from "./latest-task-scheduler.js";
import {
    clampPanelWidths,
    defaultPanelWidths,
    MOBILE_PANEL_MAXIMUM,
    PANEL_SEPARATOR_WIDTH,
    resizePanelWidths,
                     
                     
                        
} from "./panel-layout.js";
import {
    allOverlayTypes,
    blockCollections,
    blockLabel,
    errorText,
    frameFieldLabels,
    frameGroups,
    list,
    localizedRequestError,
    number,
    parseBlockSelection,
    record,
    text,
                        
                         
                    
                                  
               
              
                      
                    
} from "./editor-support.js";

                                
                              
                        
                      
                              
                
                 
                   
                      
                    
                   
                              
                      
                              
                    
                        
                       
        
                         
                                
                              
                                           
 
                                                                                                                        
                                                 

const select =                    (id        )           => document.querySelector   (`#${id}`);
const status = select             ("server-status");
const diagnostics = select             ("diagnostics");
const canvas = select                   ("sprite-canvas");
const objectSelect = select                   ("object-select");
const frameSelect = select                   ("frame-select");
const seek = select                  ("timeline-seek");
const loop = select                  ("loop-enabled");
const playButton = select                   ("play-toggle");
const fields = select             ("frame-fields");
const blockSelect = select                   ("block-select");
const skillDialog = select                   ("skill-dialog");
const editorGrid = select             ("editor-grid");
const leftPanelSeparator = select             ("left-panel-separator");
const rightPanelSeparator = select             ("right-panel-separator");
const mobilePanelQuery = window.matchMedia(`(max-width: ${MOBILE_PANEL_MAXIMUM}px)`);

let project                          ;
let skillState             = {
    revision: 0,
    etag: "",
    sidecarStatus: "missing",
    metadata: [],
    skills: [],
};
let selectedSkillIndex = -1;
let selectedBlock                 = { type: "frame" };
let selectedFrameOccurrence                    ;
let fieldDraft                        ;
let tickIndex = 0;
let playing = false;
let zoom = 1;
let timer                    ;
let renderRequest                    ;
let stateToken = "";
let tokenHeader = "x-dat-skill-flow-token";
let renderedFrameKey = "";
let loadedObjectKey = "";
let objectSwitchQueue = Promise.resolve();
let currentGeometry                             = [];
let highlightedFrameOccurrence                    ;
let flowCacheProjectKey = "";
let frameFieldCacheProjectKey = "";
let selectedFlowEdgeId                    ;
let disposeFlowSvg                          ;
let canvasDraftGeometry                             ;
let canvasInteraction   
                      
                   
                   
                         
                          
                    
                                                             
                                                            
             ;
let panelLayout                         ;
let panelContainerWidthValue = 0;
let splitterInteraction   
                               
                                   
                                    
                                  
                                      
             ;
const flowCache = new Map                        ();
const frameFieldsByLocator = new Map                         ();
const visibleOverlays = new Set             (allOverlayTypes);
const images = new Map                          ();
const colorKeyImages = new Map                           ();
const actionBusy                                   = { skill: false, edit: false, save: false };
const INT32_MIN = -2_147_483_648;
const INT32_MAX = 2_147_483_647;

async function request(path        , init              , stateChanging = false)                {
    const headers                         = { Accept: "application/json", ...(init?.headers                           ?? {}) };
    if (stateChanging) { headers["Content-Type"] = "application/json"; if (stateToken) headers[tokenHeader] = stateToken; }
    const response = await fetch(path, { ...init, headers });
    const body = record(await response.json());
    if (!response.ok) throw new Error(localizedRequestError(response.status, path));
    return body;
}
async function closeProjectSession(sessionId        , keepalive = false)                {
    await request("/api/project/close", {
        method: "POST",
        body: JSON.stringify({ sessionId }),
        keepalive,
    }, true);
}
function normalize(payload      )               {
    const data = record(payload.data), session = record(data.document ?? data.session ?? data.project ?? data), projection = record(session.projection ?? data.projection);
    const preview = record(session.nativePreview ?? session.preview ?? session.trace ?? data.nativePreview ?? data.preview ?? data.trace);
    const stage = normalizePreviewStage(record(preview.metadata).stage);
    const frames = list(projection.frames ?? session.frames).map((value, index) => ({ ...record(value), frameId: number(record(value).frameId ?? record(value).id, index), occurrence: number(record(value).occurrence, index) }))           ;
    const assets = new Map                ();
    for (const value of list(session.assets ?? session.spriteAssets ?? data.assets ?? data.spriteAssets)) { const asset = record(value); const id = text(asset.assetId ?? asset.id); if (id) assets.set(text(asset.file), id); }
    const fallbackAsset = text(session.assetId ?? data.assetId); if (fallbackAsset) assets.set("", fallbackAsset);
    const fieldCapabilities = list(session.fields ?? data.fields).flatMap((value)                    => {
        const field = record(value), fieldId = text(field.fieldId ?? field.id), key = text(field.key), kind = text(field.kind), scope = text(field.scope);
        return fieldId && key && kind && scope ? [{ ...field, fieldId, key, kind, scope, value: field.value, occurrence: number(field.occurrence) }                   ] : [];
    });
    const ticks = list(preview.ticks ?? preview.nativeTicks ?? session.ticks).map((value) => { const raw = record(value); return { ...raw, cameraX: number(raw.camera_x ?? raw.cameraX), entities: list(raw.entities).map((entity) => { const item = record(entity); return { ...item, slot: number(item.slot, -1), oid: number(item.oid), frame: number(item.frame), x: number(item.x), y: number(item.y), z: number(item.z) }; }) }; });
    const previewObjects = list(preview.resources ?? session.previewObjects).flatMap((value) => {
        const resource = record(value);
        const oid = number(resource.oid, -1);
        if (oid < 0) return [];
        const resourceFrames = list(resource.frames).map((frame, index) => {
            const item = record(frame);
            return {
                ...item,
                frameId: number(item.frameId ?? item.id, index),
                occurrence: number(item.occurrence, index),
            }         ;
        });
        return [{
            oid,
            frames: resourceFrames,
            ranges: list(resource.spriteRanges).map(record),
        }];
    });
    const structures = list(session.structureCapabilities ?? data.structureCapabilities).flatMap((value) => {
        const item = record(value);
        const capabilityId = text(item.capabilityId);
        if (!capabilityId) return [];
        const blocks = list(item.blocks).flatMap((blockValue) => {
            const block = record(blockValue);
            const blockCapabilityId = text(block.capabilityId);
            return blockCapabilityId ? [{
                capabilityId: blockCapabilityId,
                blockType: text(block.blockType)                                                           ,
                blockIndex: number(block.blockIndex),
                canCopy: block.canCopy === true,
                canDelete: block.canDelete === true,
            }] : [];
        });
        return [{
            capabilityId,
            frameId: number(item.frameId),
            occurrence: number(item.occurrence),
            canCopy: item.canCopy === true,
            canDelete: item.canDelete === true,
            blocks,
        }];
    });
    return {
        sessionId: text(session.sessionId ?? data.sessionId),
        revision: (session.revision ?? data.revision ?? "-")                   ,
        oid: number(session.oid ?? data.oid),
        name: text(session.name ?? data.name ?? "项目"),
        dirty: session.dirty === true || data.dirty === true,
        writable: session.writable === true || data.writable === true,
        frames,
        ranges: list(projection.spriteRanges ?? session.spriteRanges ?? data.spriteRanges).map(record),
        nativeTicks: ticks,
        nativeTrace: record(preview.trace),
        previewObjects,
        ...(stage === undefined ? {} : { stage }),
        assets,
        fields: fieldCapabilities,
        structures,
    };
}

function normalizePreviewStage(value         )                           {
    const raw = record(value);
    const width = number(raw.width, -1);
    if (width < 0) return undefined;
    const background = record(raw.background);
    const layers = list(background.layers).map((value) => {
        const layer = record(value);
        return {
            transparency: number(layer.transparency),
            parallaxWidth: number(layer.parallaxWidth),
            x: number(layer.x),
            y: number(layer.y),
            loop: number(layer.loop),
            cc: number(layer.cc),
            c1: number(layer.c1),
            c2: number(layer.c2),
            animCounter: number(layer.animCounter),
            ...(text(layer.assetId) ? { assetId: text(layer.assetId) } : {}),
        };
    });
    const rawShadow = record(background.shadow);
    const shadow = text(rawShadow.assetId)
        ? {
            width: number(rawShadow.width),
            height: number(rawShadow.height),
            assetId: text(rawShadow.assetId),
        }
        : undefined;
    return {
        width,
        zMin: number(raw.zMin),
        zMax: number(raw.zMax),
        ...(layers.length === 0 && shadow === undefined ? {} : {
            background: {
                layers,
                ...(shadow === undefined ? {} : { shadow }),
            },
        }),
    };
}
function syncPreviewBusy(busy         )       {
    const panel = select             ("preview-panel");
    if (panel) panel.ariaBusy = String(busy);
    panel?.classList.toggle("is-busy", busy);
    if (busy) setText("play-state", "正在生成预览…");
    else if (project) update();
}
const previewScheduler = createLatestTaskScheduler                     (
    async (intent) => await request("/api/project/preview", {
        method: "POST",
        body: JSON.stringify({
            sessionId: intent.sessionId,
            expectedRevision: intent.revision,
            startFrame: intent.startFrame,
            ticks: 30,
        }),
    }, true),
    syncPreviewBusy,
);
function primaryEntity()                         { return primaryPreviewEntity(project?.nativeTicks[tickIndex]?.entities ?? []); }
function currentFrame()                    { const primary = primaryEntity(), selected = project?.frames.find((frame) => frame.occurrence === selectedFrameOccurrence); if (selected && (!primary || selected.frameId === primary.frame)) return selected; return lastFrameForId(project?.frames ?? [], primary?.frame); }
function currentRuntimeFrame()                    { return lastFrameForId(project?.frames ?? [], primaryEntity()?.frame); }
function playbackEndIndex()         {
    const last = Math.max(0, (project?.nativeTicks.length ?? 1) - 1);
    return Math.min(last, Math.max(0, Math.trunc(number(project?.nativeTrace.playbackEndTick, last))));
}
function progressEndIndex()         {
    const last = Math.max(0, (project?.nativeTicks.length ?? 1) - 1);
    const value = number(project?.nativeTrace.progressEndTick, -1);
    return value < 0 ? -1 : Math.min(last, Math.max(0, Math.trunc(value)));
}
function fieldCapability(frame       , key        , block                )                              {
    const runtime = lastFrameForId(project?.frames ?? [], frame.frameId);
    if (runtime?.occurrence !== frame.occurrence) return undefined;
    if (block.type === "frame") return findFrameFieldCapability(project?.fields ?? [], frame, key)                               ;
    for (let index = (project?.fields.length ?? 0) - 1; index >= 0; index -= 1) {
        const field = project .fields[index] ;
        if (field.scope === "block" && field.frameId === frame.frameId && field.frameOccurrence === frame.occurrence
            && field.blockType === block.type && field.blockIndex === block.index && field.key === key) return field;
    }
    return undefined;
}
function frameFieldLocatorKey(frameId        , occurrence        , key        )         {
    return `${frameId}:${occurrence}:${key}`;
}
function currentFrameFieldIndex()                                       {
    if (!project) return new Map();
    const projectKey = `${project.sessionId}:${project.revision}`;
    if (frameFieldCacheProjectKey !== projectKey) {
        frameFieldCacheProjectKey = projectKey;
        frameFieldsByLocator.clear();
        for (const field of project.fields) {
            if (field.scope === "frame" && field.frameId !== undefined && field.frameOccurrence !== undefined) {
                frameFieldsByLocator.set(
                    frameFieldLocatorKey(field.frameId, field.frameOccurrence, field.key),
                    field,
                );
            }
        }
    }
    return frameFieldsByLocator;
}
function existingFrameField(frame       , key        )                              {
    return currentFrameFieldIndex().get(frameFieldLocatorKey(frame.frameId, frame.occurrence, key));
}
function requestPreviewRender()       { if (renderRequest === undefined) renderRequest = window.requestAnimationFrame(renderFrame); }
function setText(id        , value        )       {
    const element = select             (id);
    if (element) element.textContent = value;
}
function reportOperation(operation                  , fallback        )       {
    void operation.catch((error) => {
        if (diagnostics) diagnostics.textContent = errorText(error, fallback);
    });
}
function panelSeparator(panel                )                     {
    return panel === "left" ? leftPanelSeparator : rightPanelSeparator;
}
function samePanelLayout(left                         , right             )          {
    return left?.left === right.left
        && left.right === right.right
        && left.middle === right.middle
        && left.middleMinimum === right.middleMinimum
        && left.leftMinimum === right.leftMinimum
        && left.leftMaximum === right.leftMaximum
        && left.rightMinimum === right.rightMinimum
        && left.rightMaximum === right.rightMaximum;
}
function applyPanelLayout(next             )       {
    if (!editorGrid || samePanelLayout(panelLayout, next)) return;
    const previous = panelLayout;
    panelLayout = next;
    if (previous?.left !== next.left) {
        editorGrid.style.setProperty("--left-panel-width", `${next.left}px`);
    }
    if (previous?.right !== next.right) {
        editorGrid.style.setProperty("--right-panel-width", `${next.right}px`);
    }
    const containerWidth = next.left + next.middle + next.right + PANEL_SEPARATOR_WIDTH * 2;
    const leftValues = {
        minimum: next.leftMinimum,
        maximum: next.leftMaximum,
        now: next.left,
        text: `状态技能区宽 ${next.left} 像素`,
    };
    const rightValues = {
        minimum: containerWidth - next.rightMaximum,
        maximum: containerWidth - next.rightMinimum,
        now: containerWidth - next.right,
        text: `属性区宽 ${next.right} 像素`,
    };
    for (const [panel, values] of [["left", leftValues], ["right", rightValues]]         ) {
        const separator = panelSeparator(panel);
        separator?.setAttribute("aria-valuemin", String(values.minimum));
        separator?.setAttribute("aria-valuemax", String(values.maximum));
        separator?.setAttribute("aria-valuenow", String(values.now));
        separator?.setAttribute("aria-valuetext", values.text);
    }
}
function refreshPanelContainerWidth()         {
    panelContainerWidthValue = editorGrid?.getBoundingClientRect().width ?? 0;
    return panelContainerWidthValue;
}
function syncPanelLayout()       {
    if (!editorGrid || mobilePanelQuery.matches) return;
    const width = refreshPanelContainerWidth();
    if (width <= 0) return;
    applyPanelLayout(clampPanelWidths(
        width,
        panelLayout ?? defaultPanelWidths(width),
    ));
}
function finishSplitterInteraction(restoreStart         , releaseCapture         )       {
    const interaction = splitterInteraction;
    if (!interaction) return;
    splitterInteraction = undefined;
    if (restoreStart) {
        const width = refreshPanelContainerWidth();
        if (width > 0) applyPanelLayout(clampPanelWidths(width, interaction.startWidths));
    }
    interaction.separator.classList.remove("is-dragging");
    document.body.classList.remove("is-resizing-panels");
    if (releaseCapture && interaction.separator.hasPointerCapture(interaction.pointerId)) {
        interaction.separator.releasePointerCapture(interaction.pointerId);
    }
}
function startSplitterInteraction(
    event              ,
    panel                ,
    separator             ,
)       {
    if (event.button !== 0 || mobilePanelQuery.matches || splitterInteraction) return;
    syncPanelLayout();
    if (!panelLayout) return;
    event.preventDefault();
    separator.focus();
    splitterInteraction = {
        pointerId: event.pointerId,
        panel,
        separator,
        startClientX: event.clientX,
        startWidths: { left: panelLayout.left, right: panelLayout.right },
    };
    separator.classList.add("is-dragging");
    document.body.classList.add("is-resizing-panels");
    separator.setPointerCapture(event.pointerId);
}
function moveSplitterInteraction(event              )       {
    const interaction = splitterInteraction;
    if (!interaction || interaction.pointerId !== event.pointerId) return;
    const width = panelContainerWidthValue;
    if (width <= 0) return;
    event.preventDefault();
    applyPanelLayout(resizePanelWidths(
        width,
        interaction.startWidths,
        interaction.panel,
        event.clientX - interaction.startClientX,
    ));
}
function handleSplitterKeydown(event               , panel                )       {
    if (event.key === "Escape" && splitterInteraction?.panel === panel) {
        event.preventDefault();
        finishSplitterInteraction(true, true);
        return;
    }
    if (splitterInteraction || mobilePanelQuery.matches) return;
    const direction = event.key === "ArrowLeft" ? -1 : event.key === "ArrowRight" ? 1 : 0;
    if (direction === 0) return;
    syncPanelLayout();
    if (!panelLayout) return;
    event.preventDefault();
    applyPanelLayout(resizePanelWidths(
        panelContainerWidthValue,
        panelLayout,
        panel,
        direction * (event.shiftKey ? 32 : 8),
    ));
}
function initializePanelSplitters()       {
    if (!editorGrid || !leftPanelSeparator || !rightPanelSeparator) return;
    const separators = [
        { panel: "left", element: leftPanelSeparator },
        { panel: "right", element: rightPanelSeparator },
    ]         ;
    for (const { panel, element } of separators) {
        element.addEventListener("pointerdown", (event) => startSplitterInteraction(event, panel, element));
        element.addEventListener("pointermove", moveSplitterInteraction);
        element.addEventListener("pointerup", (event) => {
            if (splitterInteraction?.pointerId === event.pointerId) finishSplitterInteraction(false, true);
        });
        element.addEventListener("pointercancel", (event) => {
            if (splitterInteraction?.pointerId === event.pointerId) finishSplitterInteraction(true, false);
        });
        element.addEventListener("lostpointercapture", (event) => {
            if (splitterInteraction?.pointerId === event.pointerId) finishSplitterInteraction(false, false);
        });
        element.addEventListener("keydown", (event) => handleSplitterKeydown(event, panel));
    }
    const resizeObserver = new ResizeObserver(() => {
        finishSplitterInteraction(false, true);
        syncPanelLayout();
    });
    resizeObserver.observe(editorGrid);
    mobilePanelQuery.addEventListener("change", () => {
        finishSplitterInteraction(false, true);
        syncPanelLayout();
    });
    syncPanelLayout();
}
function isActionBusy()          {
    return Object.values(actionBusy).some(Boolean);
}
function isSelectionLocked()          {
    return isActionBusy()
        || fieldDraft !== undefined
        || canvasInteraction !== undefined;
}
function syncSkillActionState()       {
    const editSkill = select                   ("edit-skill");
    if (!editSkill) return;
    editSkill.disabled = project?.writable !== true
        || selectedSkillIndex < 0
        || isSelectionLocked()
        || skillState.sidecarStatus === "invalid";
}
function syncActionState()       {
    const skillForm = select                 ("skill-form"), saveSkill = select                   ("save-skill");
    const frameEditor = select                 ("frame-editor");
    if (skillForm) skillForm.ariaBusy = String(actionBusy.skill);
    if (saveSkill) {
        saveSkill.textContent = actionBusy.skill ? "保存中…" : "保存显示信息";
        saveSkill.disabled = actionBusy.skill || skillState.sidecarStatus === "invalid";
    }
    const selectionLocked = isSelectionLocked();
    syncSkillActionState();
    if (objectSelect) objectSelect.disabled = selectionLocked;
    if (frameSelect) frameSelect.disabled = selectionLocked;
    if (blockSelect) blockSelect.disabled = selectionLocked;
    for (const id of ["play-toggle", "step-once", "reset-timeline", "step-back", "jump-end"]) {
        const control = select                   (id);
        if (control) control.disabled = selectionLocked;
    }
    document.querySelectorAll                   (".timeline-segment").forEach((button) => button.disabled = selectionLocked);
    document.querySelectorAll             ("#skill-list tr, #flow-list tr").forEach((row) => row.ariaDisabled = String(selectionLocked));
    if (seek) seek.disabled = selectionLocked;
    syncFlowEdgeEditor(currentFlow());
    if (frameEditor) frameEditor.ariaBusy = String(actionBusy.edit);
    syncStructureActions();
    syncDraftActions();
    syncSaveState();
}
function currentStructureCapability()                                       {
    const frame = currentFrame();
    return project?.structures.find((candidate) => candidate.occurrence === frame?.occurrence);
}
function currentBlockStructureCapability()                                                         {
    if (selectedBlock.type === "frame") return undefined;
    return currentStructureCapability()?.blocks.find((candidate) => (
        candidate.blockType === selectedBlock.type && candidate.blockIndex === selectedBlock.index
    ));
}
function syncStructureActions()       {
    const frame = currentStructureCapability();
    const block = currentBlockStructureCapability();
    const unavailable = project?.writable !== true || isSelectionLocked();
    const states                                    = {
        "copy-frame": unavailable || frame?.canCopy !== true,
        "delete-frame": unavailable || frame?.canDelete !== true,
        "new-block": unavailable || block?.canCopy !== true,
        "copy-block": unavailable || block?.canCopy !== true,
        "delete-block": unavailable || block?.canDelete !== true,
    };
    for (const [id, disabled] of Object.entries(states)) {
        const control = select                   (id);
        if (control) control.disabled = disabled;
    }
}
async function runExclusiveAction   (kind                 , operation                  )                         {
    if (isActionBusy()) throw new Error("另一项操作正在进行，请稍候重试。");
    actionBusy[kind] = true;
    syncActionState();
    try {
        return await operation();
    } finally {
        actionBusy[kind] = false;
        syncActionState();
        if (kind === "edit") renderFlow();
    }
}
function syncFrameSelectionIndicators(frame                   )       {
    if (highlightedFrameOccurrence === frame?.occurrence) return;
    highlightedFrameOccurrence = frame?.occurrence;
    document.querySelectorAll             ("[data-frame-occurrence]").forEach((element) => {
        element.classList.toggle("is-selected", Number(element.dataset.frameOccurrence) === frame?.occurrence);
    });
}
function syncReadOnlyUi()       {
    const primary = primaryEntity(), frame = currentFrame(), count = project?.nativeTicks.length ?? 0;
    const frameKey = frame === undefined ? "" : `${frame.frameId}:${frame.occurrence}`;
    setText("tick-readout", String(tickIndex));
    setText("frame-readout", frame ? String(frame.frameId) : "-");
    const progressEnd = progressEndIndex();
    const playbackEnd = playbackEndIndex();
    setText("time-readout", progressEnd >= 0
        ? `原生预览采样 ${tickIndex} / 主体结束 ${progressEnd} / 尾迹 ${playbackEnd}`
        : `原生预览采样 ${tickIndex} / 主体未结束`);
    setText("preview-frame-count", String(count));
    const rootEnded = progressEnd >= 0 && tickIndex >= progressEnd;
    setText("play-state", primary
        ? playing ? rootEnded ? "主体结束，播放尾迹" : "播放中" : rootEnded ? "主体已结束" : "已暂停"
        : "主实体不可用");
    setText("facing-readout", primary ? number(primary.facing) === 1 ? "左" : "右" : "—");
    if (frameSelect && frame) frameSelect.value = String(frame.occurrence);
    if (renderedFrameKey !== frameKey) { renderedFrameKey = frameKey; populateBlockSelect(); renderFields(); }
    if (seek) { seek.max = String(Math.max(0, count - 1)); seek.value = String(tickIndex); }
    if (playButton) { playButton.textContent = playing ? "Ⅱ" : "▶"; playButton.ariaPressed = String(playing); }
    syncFrameSelectionIndicators(frame);
}
function update()       { syncReadOnlyUi(); requestPreviewRender(); }
function drawPreview()       {
    if (!canvas || !project) return;
    currentGeometry = drawPreviewCanvas({
        canvas,
        project,
        tick: project.nativeTicks[tickIndex],
        runtimeFrame: currentRuntimeFrame(),
        images,
        colorKeyImages,
        visibleOverlays,
        draftGeometry: canvasDraftGeometry,
        requestRender: requestPreviewRender,
    });
}
function renderFrame()       {
    renderRequest = undefined;
    drawPreview();
}
function activeProjectOid()         {
    return project?.oid ?? number(Number(objectSelect?.selectedOptions[0]?.dataset.oid), 2);
}
function selectedSkill()                           {
    const skill = skillState.skills[selectedSkillIndex];
    return skill?.oid === activeProjectOid() ? skill : undefined;
}
function rebuildSkillEntries(preferredStartFrame = selectedSkill()?.startFrame)       {
    if (!project) {
        skillState.skills = [];
        selectedSkillIndex = -1;
        return;
    }
    skillState.skills = [...deriveSkillEntries(project.frames, project.oid, skillState.metadata)];
    selectedSkillIndex = preferredStartFrame === undefined
        ? -1
        : skillState.skills.findIndex((entry) => entry.startFrame === preferredStartFrame);
    if (selectedSkillIndex < 0 || skillState.skills[selectedSkillIndex]?.hidden === true) {
        selectedSkillIndex = skillState.skills.findIndex((entry) => !entry.hidden);
    }
}
function skillFlow(startFrame        )                             {
    if (!project) return undefined;
    const projectKey = `${project.sessionId}:${project.revision}:${skillState.revision}:${skillState.etag}`;
    if (flowCacheProjectKey !== projectKey) {
        flowCacheProjectKey = projectKey;
        flowCache.clear();
    }
    let graph = flowCache.get(startFrame);
    if (!graph) {
        graph = buildSkillFlow(
            project.frames,
            startFrame,
            (frame, key) => existingFrameField(frame         , key) !== undefined,
            entriesByStartFrame(skillState.skills),
        );
        flowCache.set(startFrame, graph);
    }
    return graph;
}
function currentFlow()                             {
    const skill = selectedSkill();
    return skill ? skillFlow(skill.startFrame) : undefined;
}
function renderSkillList()       {
    const body = select                         ("skill-list");
    if (!body) return;
    const showHidden = select                  ("show-hidden-skills")?.checked === true;
    const visibleSkills = skillState.skills.flatMap((skill, index) => (
        skill.oid === activeProjectOid() && (!skill.hidden || showHidden) ? [index] : []
    ));
    const rows                        = [];
    let lastGroup = "";
    for (const index of visibleSkills) {
        const skill = skillState.skills[index] ;
        if (skill.group !== lastGroup) {
            const heading = document.createElement("tr");
            heading.className = "skill-group-row";
            const cell = document.createElement("td");
            cell.colSpan = 4;
            cell.textContent = skill.group;
            heading.append(cell);
            rows.push(heading);
            lastGroup = skill.group;
        }
        const row = document.createElement("tr");
        if (index === selectedSkillIndex) row.classList.add("is-selected");
        if (skill.hidden) row.classList.add("is-hidden-entry");
        const name = document.createElement("td");
        const frame = document.createElement("td");
        const count = document.createElement("td");
        const trigger = document.createElement("td");
        name.textContent = `${skill.pinned ? "★ " : ""}${skill.displayName}`;
        name.title = skill.displayName === skill.label
            ? skill.label
            : `${skill.displayName}（DAT: ${skill.label}）`;
        frame.textContent = String(skill.startFrame);
        count.textContent = String(skill.segmentFrameCount);
        trigger.textContent = skill.triggers.map((item) => item.key).join(" · ") || "—";
        row.append(name, frame, count, trigger);
        row.addEventListener("click", () => {
            if (!isSelectionLocked()) reportOperation(selectSkill(index), "技能预览失败。");
        });
        rows.push(row);
    }
    body.replaceChildren(...rows);
    setText("skill-count", String(visibleSkills.length));
    select             ("skill-empty") .hidden = visibleSkills.length > 0;
    syncSkillActionState();
}
function flowSummary(edges                          )         {
    return edges.filter((edge) => edge.key !== "next" && edge.rawTarget !== 0)
        .slice(0, 2).map((edge) => `${edge.key.replace("hit_", "")}:${edge.rawTarget}`).join(" · ") || "—";
}
function editableFlowFields(graph                )                              {
    if (project?.writable !== true || isSelectionLocked()) {
        return new Map();
    }
    const result = new Map                ();
    const fieldsByLocator = currentFrameFieldIndex();
    const nodesById = new Map(graph.nodes.map((node) => [node.id, node]));
    for (const edge of graph.edges) {
        const source = nodesById.get(edge.from);
        if (source?.kind !== "frame") continue;
        const capability = fieldsByLocator.get(frameFieldLocatorKey(source.frameId, source.occurrence, edge.key));
        if (capability !== undefined) result.set(edge.id, capability.fieldId);
    }
    return result;
}
function syncFlowEdgeEditor(
    graph                            ,
    editableFields = graph === undefined ? new Map                () : editableFlowFields(graph),
)       {
    const target = select                   ("flow-edge-target");
    const apply = select                   ("apply-flow-edge");
    const readout = select             ("flow-edge-readout");
    const edge = graph?.edges.find((candidate) => candidate.id === selectedFlowEdgeId);
    if (!target || !apply || !readout) return;
    const latestById = new Map               ();
    for (const frame of project?.frames ?? []) latestById.set(frame.frameId, frame);
    target.replaceChildren(...[...latestById.values()].map((frame) => (
        new Option(`帧 ${frame.frameId}`, String(frame.frameId), false, frame.frameId === edge?.rawTarget)
    )));
    const editable = edge !== undefined && editableFields.has(edge.id);
    readout.textContent = edge === undefined
        ? "选择一条已有跳转连线"
        : `${edge.key}: ${edge.rawTarget}${editable ? "" : "（只读）"}`;
    target.disabled = !editable;
    apply.disabled = !editable || isSelectionLocked();
}
function renderFlow()       {
    const body = select                         ("flow-list"), graph = currentFlow(), skill = selectedSkill();
    if (!body) return;
    setText("flow-title", skill ? `${skill.displayName} · 帧流程` : "当前技能帧流程");
    if (!graph || !project) {
        body.replaceChildren();
        select               ("flow-svg")?.replaceChildren();
        disposeFlowSvg?.();
        disposeFlowSvg = undefined;
        selectedFlowEdgeId = undefined;
        syncFlowEdgeEditor(undefined);
        setText("flow-count", "0");
        renderTimelineSegments();
        return;
    }
    const edgesByFrom = new Map                         ();
    const framesByOccurrence = new Map(project.frames.map((frame) => [frame.occurrence, frame]));
    const editableFields = editableFlowFields(graph);
    for (const edge of graph.edges) {
        const outgoing = edgesByFrom.get(edge.from);
        if (outgoing) outgoing.push(edge);
        else edgesByFrom.set(edge.from, [edge]);
    }
    const rows = graph.nodes.map((node) => {
        const row = document.createElement("tr");
        if (node.kind === "unresolved") {
            row.classList.add("is-unresolved");
            row.innerHTML = `<td>${node.target}</td><td>${node.reason}</td><td>—</td><td>未解析</td>`;
            return row;
        }
        if (node.kind === "entry") {
            row.classList.add("is-entry-link");
            row.innerHTML = `<td>${node.frameId}</td><td></td><td>切换入口</td><td>跨技能</td>`;
            row.cells[1] .textContent = node.label;
            row.addEventListener("click", () => {
                const index = skillState.skills.findIndex((entry) => entry.id === node.entryId);
                if (index >= 0 && !isSelectionLocked()) {
                    reportOperation(selectSkill(index), "入口预览失败。");
                }
            });
            return row;
        }
        const frame = framesByOccurrence.get(node.occurrence) ;
        const edges = edgesByFrom.get(node.id) ?? [], next = edges.find((edge) => edge.key === "next");
        row.dataset.frameOccurrence = String(frame.occurrence);
        row.innerHTML = `<td>${frame.frameId}</td><td></td><td>${next?.rawTarget ?? "—"}</td><td>${flowSummary(edges)}</td>`;
        row.cells[1] .textContent = frame.label || `状态 ${frame.state}`;
        row.addEventListener("click", () => {
            if (!isSelectionLocked()) reportOperation(selectFrame(frame.frameId, frame.occurrence, true), "预览失败。");
        });
        return row;
    });
    body.replaceChildren(...rows);
    highlightedFrameOccurrence = undefined;
    setText("flow-count", String(graph.nodes.filter((node) => node.kind === "frame").length));
    if (!graph.edges.some((edge) => edge.id === selectedFlowEdgeId)) selectedFlowEdgeId = undefined;
    disposeFlowSvg?.();
    const svg = select               ("flow-svg");
    if (svg) {
        disposeFlowSvg = renderFlowSvg(svg, graph, {
            editableFieldIds: editableFields,
            selectedEdgeId: selectedFlowEdgeId,
            onSelectEdge: (edge) => {
                if (!isSelectionLocked()) {
                    selectedFlowEdgeId = edge.id;
                    renderFlow();
                }
            },
            onSelectNode: (node) => {
                if (!isSelectionLocked()) {
                    reportOperation(selectFrame(node.frameId, node.occurrence, true), "预览失败。");
                }
            },
            onSelectEntry: (node) => {
                const index = skillState.skills.findIndex((entry) => entry.id === node.entryId);
                if (index >= 0 && !isSelectionLocked()) {
                    reportOperation(selectSkill(index), "入口预览失败。");
                }
            },
        });
    }
    syncFlowEdgeEditor(graph, editableFields);
    renderTimelineSegments(graph);
}
function renderTimelineSegments(graph = currentFlow())       {
    const container = select             ("timeline-segments"), markers = select             ("timeline-markers");
    if (!container || !markers) return;
    const timeline = graph && project
        ? buildSkillTimeline(graph, project.frames)
        : { segments: [], totalUnits: 0 };
    setText("dat-wait-readout", `${timeline.totalUnits} DAT wait 视觉单位`);
    container.replaceChildren(...timeline.segments.map((segment) => {
        const { node, wait } = segment;
        const button = document.createElement("button");
        button.type = "button"; button.className = "timeline-segment"; button.dataset.frameOccurrence = String(node.occurrence);
        button.textContent = `${node.frameId} · ${wait}`;
        button.title = `帧 ${node.frameId}，DAT wait 视觉单位 ${wait}`;
        button.style.flex = `${wait} 1 0`;
        button.disabled = actionBusy.edit;
        button.addEventListener("click", () => {
            if (!isSelectionLocked()) reportOperation(selectFrame(node.frameId, node.occurrence, true), "预览失败。");
        });
        return button;
    }));
    markers.replaceChildren(...timeline.segments.map((segment) => {
        const marker = document.createElement("span");
        marker.className = "timeline-marker";
        marker.style.left = `${segment.startUnit / Math.max(1, timeline.totalUnits) * 100}%`;
        return marker;
    }));
}
function blockEntries(frame       , block                )                                   {
    if (block.type === "frame") return [];
    const values = frame[blockCollections[block.type]]           ;
    const item = Array.isArray(values) ? record(values[block.index ?? 0]) : {};
    return Object.entries(item).filter(([key]) => !key.endsWith("2"));
}
function populateBlockSelect()       {
    const frame = currentFrame();
    if (!blockSelect || !frame) return;
    const options = [new Option("帧基础", "frame")];
    for (const type of allOverlayTypes) {
        const values = frame[blockCollections[type]]           ;
        if (!Array.isArray(values)) continue;
        values.forEach((_, index) => options.push(new Option(`${blockLabel(type)} #${index + 1}`, `${type}:${index}`)));
    }
    const desired = selectedBlock.type === "frame" ? "frame" : `${selectedBlock.type}:${selectedBlock.index}`;
    blockSelect.replaceChildren(...options);
    blockSelect.value = options.some((option) => option.value === desired) ? desired : "frame";
    if (blockSelect.value === "frame") selectedBlock = { type: "frame" };
}
function createFieldRow(frame       , block                , key        , rawValue         )              {
    const capability = project?.writable === true ? fieldCapability(frame, key, block) : undefined;
    const row = document.createElement("div"), label = document.createElement("label");
    row.className = "field-row"; label.textContent = `${frameFieldLabels[key] ?? "DAT 字段"} `; label.append(Object.assign(document.createElement("small"), { textContent: `(${key})` }));
    const control = capability?.kind === "integer-pair" ? createPairInputs(capability, rawValue) : createScalarInput(capability, key, rawValue);
    const badge = document.createElement("span");
    badge.className = `field-status${capability ? "" : " is-readonly"}`; badge.textContent = capability ? "可编辑" : "只读";
    row.append(label, control, badge);
    return row;
}
function createScalarInput(capability                             , key        , rawValue         )                   {
    const input = document.createElement("input");
    const draft = capability && fieldDraft?.capability.fieldId === capability.fieldId ? fieldDraft : undefined;
    input.name = key; input.type = typeof rawValue === "string" ? "text" : "number";
    if (input.type === "number") {
        input.min = String(INT32_MIN);
        input.max = String(INT32_MAX);
        input.step = "1";
    }
    input.value = draft && typeof draft.rawValue === "string" ? draft.rawValue : String(rawValue ?? "");
    input.disabled = capability === undefined;
    if (capability) input.dataset.fieldId = capability.fieldId;
    if (capability) input.addEventListener("input", () => {
        const valid = input.validity.valid && (input.type !== "number"
            || Number.isSafeInteger(input.valueAsNumber) && input.valueAsNumber >= INT32_MIN && input.valueAsNumber <= INT32_MAX);
        const value = valid ? (input.type === "number" ? input.valueAsNumber : input.value) : undefined;
        setDraft(capability, input.value, value, valid, input);
    });
    return input;
}
function createPairInputs(capability                 , rawValue         )              {
    const wrapper = document.createElement("div"), draft = fieldDraft?.capability.fieldId === capability.fieldId ? fieldDraft : undefined;
    const values = Array.isArray(draft?.rawValue) ? draft.rawValue : Array.isArray(capability.value) ? capability.value : [rawValue, 0];
    wrapper.className = "pair-inputs";
    const inputs = [0, 1].map((index) => {
        const input = document.createElement("input");
        input.type = "number"; input.min = String(INT32_MIN); input.max = String(INT32_MAX); input.step = "1"; input.value = String(values[index] ?? 0);
        input.dataset.fieldId = capability.fieldId;
        input.addEventListener("input", () => {
            const raw                   = [inputs[0] .value, inputs[1] .value];
            const valid = inputs.every((candidate) => candidate.validity.valid
                && Number.isSafeInteger(candidate.valueAsNumber)
                && candidate.valueAsNumber >= INT32_MIN
                && candidate.valueAsNumber <= INT32_MAX);
            const value                               = valid ? [inputs[0] .valueAsNumber, inputs[1] .valueAsNumber] : undefined;
            setDraft(capability, raw, value, valid, wrapper);
        });
        return input;
    });
    wrapper.append(...inputs);
    return wrapper;
}
function setDraft(
    capability                 ,
    rawValue                        ,
    value                     ,
    valid         ,
    element             ,
)       {
    fieldDraft = { capability, rawValue, value, valid };
    fields?.querySelectorAll(".is-dirty").forEach((candidate) => candidate.classList.remove("is-dirty"));
    element.closest(".field-row")?.classList.add("is-dirty");
    syncActionState();
}
function clearDraft()       {
    fieldDraft = undefined;
    fields?.querySelectorAll(".is-dirty").forEach((candidate) => candidate.classList.remove("is-dirty"));
    syncActionState();
}
function syncDraftActions()       {
    const apply = select                   ("apply-draft"), topApply = select                   ("apply-session");
    const discard = select                   ("discard-draft");
    const editorLocked = isActionBusy() || canvasInteraction !== undefined;
    const canApply = project?.writable === true && fieldDraft?.valid === true && fieldDraft.value !== undefined && !editorLocked;
    if (apply) {
        apply.textContent = actionBusy.edit ? "应用中…" : "应用本次修改";
        apply.disabled = !canApply;
    }
    if (topApply) {
        topApply.textContent = actionBusy.edit ? "应用中…" : "应用会话修改";
        topApply.disabled = !canApply;
    }
    if (discard) discard.disabled = fieldDraft === undefined || editorLocked;
    fields?.querySelectorAll                  ("input[data-field-id]").forEach((input) => {
        input.disabled = project?.writable !== true
            || editorLocked
            || (fieldDraft !== undefined && input.dataset.fieldId !== fieldDraft.capability.fieldId);
    });
}
function renderFields()       {
    fields?.replaceChildren();
    const frame = currentFrame();
    if (!frame || !fields) {
        syncDraftActions();
        return;
    }
    setText("inspector-context", `帧 ${frame.frameId} · occurrence ${frame.occurrence}`);
    const groups = selectedBlock.type === "frame" ? frameGroups : [{ title: `${blockLabel(selectedBlock.type)} #${(selectedBlock.index ?? 0) + 1}`, keys: blockEntries(frame, selectedBlock).map(([key]) => key) }];
    for (const group of groups) {
        const fieldset = document.createElement("fieldset"); fieldset.className = "field-group";
        fieldset.append(Object.assign(document.createElement("legend"), { textContent: group.title }));
        for (const key of group.keys) {
            const rawValue = selectedBlock.type === "frame" ? frame[key               ] : blockEntries(frame, selectedBlock).find(([candidate]) => candidate === key)?.[1];
            fieldset.append(createFieldRow(frame, selectedBlock, key, rawValue));
        }
        fields.append(fieldset);
    }
    fields.querySelector(`[data-field-id="${CSS.escape(fieldDraft?.capability.fieldId ?? "")}"]`)?.closest(".field-row")?.classList.add("is-dirty");
    syncDraftActions();
    syncStructureActions();
}
function render()       {
    if (!project) return;
    frameSelect?.replaceChildren(...project.frames.map((frame) => new Option(`帧 ${frame.frameId} · occurrence ${frame.occurrence}`, String(frame.occurrence))));
    renderSkillList(); renderFlow(); populateBlockSelect(); renderFields(); syncActionState(); update();
}
async function preview(startFrame        )                {
    if (!project) return;
    const intent = { sessionId: project.sessionId, revision: project.revision, startFrame };
    const result = await previewScheduler.schedule(intent);
    if (result.status !== "committed" || project?.sessionId !== intent.sessionId || project.revision !== intent.revision) return;
    const partial = normalize(result.value);
    await preloadPreviewObjectAssets(partial, images, requestPreviewRender);
    if (project?.sessionId !== intent.sessionId || project.revision !== intent.revision) return;
    project = mergePreview(project, partial.revision, partial.nativeTicks, partial.nativeTrace, partial.previewObjects)                ;
    tickIndex = 0;
    render();
}
function step()       {
    const last = playbackEndIndex();
    if (tickIndex >= last && !loop?.checked) { playing = false; update(); return; }
    tickIndex = tickIndex >= last ? 0 : tickIndex + 1;
    update();
}
function schedule()       { if (!playing || timer !== undefined) return; timer = window.setTimeout(() => { timer = undefined; step(); schedule(); }, 33); }
function setPlaying(next         )       {
    playing = next;
    if (!playing && timer !== undefined) { window.clearTimeout(timer); timer = undefined; }
    update();
    schedule();
}
function syncSaveState()       {
    const dirty = project?.dirty === true, save = select                   ("save-project"), dirtyReadout = select             ("dirty-readout");
    if (save) {
        save.textContent = actionBusy.save ? "保存中…" : "覆盖 DAT 文件";
        save.disabled = project?.writable !== true || !dirty || actionBusy.save || isSelectionLocked();
    }
    if (dirtyReadout) {
        dirtyReadout.dataset.dirty = String(dirty);
        dirtyReadout.dataset.draft = String(fieldDraft !== undefined);
        dirtyReadout.textContent = fieldDraft
            ? dirty ? "有未应用修改 · 未保存至文件" : "有未应用修改"
            : dirty ? "未保存至文件" : "已保存";
    }
    setText("revision-readout", `修订版本 ${project?.revision ?? "-"}`);
}
async function loadSkills()                {
    const data = record((await request("/api/project/skills")).data);
    skillState = {
        revision: number(data.revision),
        etag: text(data.etag),
        sidecarStatus: (["missing", "valid", "legacy", "invalid"].includes(text(data.sidecarStatus))
            ? text(data.sidecarStatus)
            : "invalid")                               ,
        metadata: list(data.skills).flatMap((value)                         => {
            const item = record(value);
            const oid = number(item.oid, -1);
            const startFrame = number(item.startFrame, -1);
            if (!Number.isSafeInteger(oid) || !Number.isSafeInteger(startFrame)) return [];
            return [{
                oid,
                startFrame,
                ...(text(item.displayName) === "" ? {} : { displayName: text(item.displayName) }),
                ...(text(item.group) === "" ? {} : { group: text(item.group) }),
                ...(Number.isSafeInteger(item.order) ? { order: number(item.order) } : {}),
                ...(item.pinned === true ? { pinned: true } : {}),
                ...(item.hidden === true ? { hidden: true } : {}),
                ...(text(item.notes) === "" ? {} : { notes: text(item.notes) }),
            }];
        }),
        skills: [],
    };
    rebuildSkillEntries();
}
async function saveSkillMetadata(metadata                                 )                {
    const response = await request("/api/project/skills", {
        method: "POST",
        body: JSON.stringify({
            expectedRevision: skillState.revision,
            expectedEtag: skillState.etag,
            skills: metadata,
        }),
    }, true);
    const data = record(response.data);
    skillState.revision = number(data.revision);
    skillState.etag = text(data.etag);
    skillState.sidecarStatus = "valid";
    skillState.metadata = [...metadata];
}
async function open(objectKey        , oid        )                {
    if (project?.sessionId) {
        if ((fieldDraft || project.dirty) && !window.confirm("当前项目有未应用或未保存修改。确定放弃并切换对象吗？")) {
            if (objectSelect) objectSelect.value = loadedObjectKey;
            return;
        }
        clearDraft();
        previewScheduler.invalidate();
        await closeProjectSession(project.sessionId);
        project = undefined;
        images.clear();
        colorKeyImages.clear();
        requestPreviewRender();
    }
    const response = await request("/api/project/open", { method: "POST", body: JSON.stringify({ objectKey }) }, true);
    project = normalize(response); loadedObjectKey = objectKey; tickIndex = 0; selectedBlock = { type: "frame" };
    rebuildSkillEntries();
    selectedFrameOccurrence = lastFrameForId(project.frames, primaryPreviewEntity(project.nativeTicks[0]?.entities ?? [])?.frame)?.occurrence;
    status .dataset.state = "connected"; status .textContent = `已载入 ${project.name} / OID ${oid}${project.writable ? "" : "（只读）"}`;
    const sidecarNotice = skillState.sidecarStatus === "invalid"
        ? "技能 sidecar 无效，已忽略；DAT 自动入口仍可使用。"
        : skillState.sidecarStatus === "legacy"
            ? "已读取旧版技能 sidecar；下次编辑显示信息时会迁移。"
            : "";
    diagnostics .textContent = sidecarNotice || (project.writable
        ? "项目数据已载入，可以选择技能、播放、查看叠加层或编辑当前帧。"
        : "项目仅存在于 fallback 资源中，当前会话为只读预览。");
    render();
    if (selectedSkillIndex >= 0) await selectSkill(selectedSkillIndex);
}
function switchObject(objectKey        , oid        )       {
    const operation = objectSwitchQueue.then(() => open(objectKey, oid));
    objectSwitchQueue = operation.catch(() => undefined);
    void operation.catch((error) => {
        status .textContent = "项目不可用";
        diagnostics .textContent = errorText(error, "项目载入失败。");
    });
}
async function start()                {
    try {
        const bootstrap = record((await request("/api/bootstrap")).data), security = record(bootstrap.security);
        stateToken = text(security.token ?? bootstrap.stateToken ?? bootstrap.token);
        tokenHeader = text(security.tokenHeader ?? bootstrap.tokenHeader) || tokenHeader;
        const [listing] = await Promise.all([request("/api/project"), loadSkills()]);
        const choices = list(record(listing.data).objects ?? record(listing.data).entries);
        objectSelect?.replaceChildren(...choices.map((value) => {
            const item = record(value), oid = number(item.oid), option = new Option(`OID ${oid}${oid === 2 ? " · Naruto" : " · 暂未接入预览"}`, text(item.objectKey), false, oid === 2);
            option.dataset.oid = String(oid);
            option.disabled = oid !== 2;
            return option;
        }));
        const selected = objectSelect?.selectedOptions[0];
        await open(objectSelect?.value || "", number(Number(selected?.dataset.oid), 2));
    } catch (error) {
        status .dataset.state = "error"; status .textContent = "项目不可用";
        diagnostics .textContent = errorText(error, "项目载入失败。");
    }
}
async function selectFrame(frameId        , occurrence        , refreshPreview         )                {
    if (isSelectionLocked()) return;
    selectedFrameOccurrence = occurrence; selectedBlock = { type: "frame" }; renderedFrameKey = "";
    if (refreshPreview) {
        const traceStartFrame = traceStartFrameForSelection(
            project?.frames ?? [], frameId, occurrence, currentFlow(),
        );
        await preview(traceStartFrame);
    } else {
        render();
    }
}
async function selectSkill(index        )                {
    if (isSelectionLocked()) return;
    const skill = skillState.skills[index];
    if (!skill || !project || skill.oid !== project.oid) return;
    selectedSkillIndex = index; renderSkillList(); renderFlow();
    const frame = lastFrameForId(project.frames, skill.startFrame);
    if (frame) await selectFrame(frame.frameId, frame.occurrence, true);
    else diagnostics .textContent = `入口“${skill.displayName}”的起始帧 ${skill.startFrame} 不存在。`;
}
function currentSkillMetadata(skill            )                                   {
    return skillState.metadata.find((entry) => (
        entry.oid === skill.oid && entry.startFrame === skill.startFrame
    ));
}
function validateSkillText(
    input                                               ,
    maximumBytes        ,
    label        ,
)          {
    const value = input?.value.trim() ?? "";
    const valid = new TextEncoder().encode(value).byteLength <= maximumBytes
        && !/[\u0000-\u001f\u007f-\u009f\uD800-\uDFFF]/u.test(value);
    if (!valid) {
        if (diagnostics) diagnostics.textContent = `${label}必须是不含控制字符且不超过 ${maximumBytes} 个 UTF-8 字节的文本。`;
        input?.focus();
    }
    return valid;
}
function openSkillDialog()       {
    const skill = selectedSkill();
    if (!skill || skillState.sidecarStatus === "invalid") return;
    const metadata = currentSkillMetadata(skill);
    setText("skill-dialog-title", "编辑入口显示信息");
    setText("skill-start-frame-readout", `${skill.startFrame} · ${skill.label}`);
    const name = select                  ("skill-name");
    const group = select                  ("skill-group");
    const order = select                  ("skill-order");
    const pinned = select                  ("skill-pinned");
    const hidden = select                  ("skill-hidden");
    const notes = select                     ("skill-notes");
    if (name) name.value = metadata?.displayName ?? "";
    if (group) group.value = metadata?.group ?? "";
    if (order) order.value = metadata?.order === undefined ? "" : String(metadata.order);
    if (pinned) pinned.checked = metadata?.pinned === true;
    if (hidden) hidden.checked = metadata?.hidden === true;
    if (notes) notes.value = metadata?.notes ?? "";
    skillDialog?.showModal();
}
async function submitSkillForm(event             )                {
    event.preventDefault();
    if (event.submitter instanceof HTMLButtonElement && event.submitter.value === "cancel") { skillDialog?.close(); return; }
    const skill = selectedSkill();
    const nameInput = select                  ("skill-name");
    const groupInput = select                  ("skill-group");
    const notesInput = select                     ("skill-notes");
    const displayName = nameInput?.value.trim() ?? "";
    const group = groupInput?.value.trim() ?? "";
    const orderValue = select                  ("skill-order")?.value.trim() ?? "";
    const order = orderValue === "" ? undefined : Number(orderValue);
    const pinned = select                  ("skill-pinned")?.checked === true;
    const hidden = select                  ("skill-hidden")?.checked === true;
    const notes = notesInput?.value.trim() ?? "";
    if (!skill
        || !validateSkillText(nameInput, 256, "显示名称")
        || !validateSkillText(groupInput, 256, "自定义分组")
        || !validateSkillText(notesInput, 4096, "备注")
        || (order !== undefined && (!Number.isSafeInteger(order) || order < -1_000_000 || order > 1_000_000))) return;
    const next                       = {
        oid: skill.oid,
        startFrame: skill.startFrame,
        ...(displayName === "" ? {} : { displayName }),
        ...(group === "" ? {} : { group }),
        ...(order === undefined ? {} : { order }),
        ...(pinned ? { pinned: true } : {}),
        ...(hidden ? { hidden: true } : {}),
        ...(notes === "" ? {} : { notes }),
    };
    const hasOverride = Object.keys(next).length > 2;
    await runExclusiveAction("skill", async () => {
        const metadata = skillState.metadata.filter((entry) => !(
            entry.oid === skill.oid && entry.startFrame === skill.startFrame
        ));
        if (hasOverride) metadata.push(next);
        await saveSkillMetadata(metadata);
        rebuildSkillEntries(skill.startFrame);
        skillDialog?.close();
        render();
    });
}
async function applyDraft()                {
    const draft = fieldDraft;
    if (!draft?.valid || draft.value === undefined || project?.writable !== true) return;
    const previewFrameId = currentFrame()?.frameId ?? draft.capability.frameId ?? 0;
    await applyBatchEdits([{ fieldId: draft.capability.fieldId, value: draft.value }], previewFrameId);
}
async function applyBatchEdits(
    edits                                                                                    ,
    previewFrameId        ,
)                {
    if (project?.writable !== true || edits.length === 0) return;
    const preferredEntryFrame = selectedSkill()?.startFrame;
    await runExclusiveAction("edit", async () => {
        previewScheduler.invalidate();
        const response = await request("/api/project/edit-batch", {
            method: "POST",
            body: JSON.stringify({
                sessionId: project .sessionId,
                edits,
                expectedRevision: project .revision,
            }),
        }, true);
        project = normalize(response);
        rebuildSkillEntries(preferredEntryFrame);
        fieldDraft = undefined;
        diagnostics .textContent = "修改已应用到当前会话，尚未覆盖 DAT 文件。";
        render();
        try {
            await preview(previewFrameId);
        } catch (error) {
            if (diagnostics) diagnostics.textContent = `修改已应用，但${errorText(error, "预览刷新失败。")}`;
        }
    });
}
async function applyFlowReroute()                {
    if (isSelectionLocked()) return;
    const edge = currentFlow()?.edges.find((candidate) => candidate.id === selectedFlowEdgeId);
    const target = select                   ("flow-edge-target");
    if (!edge || !target || project?.writable !== true) return;
    const fieldId = editableFlowFields(currentFlow() ).get(edge.id);
    const targetFrameId = Number(target.value);
    if (fieldId === undefined || !Number.isSafeInteger(targetFrameId)) return;
    const source = currentFlow()?.nodes.find((node) => node.id === edge.from);
    await applyBatchEdits(
        [{ fieldId, value: targetFrameId }],
        source?.kind === "frame" ? source.frameId : (currentFrame()?.frameId ?? 0),
    );
    selectedFlowEdgeId = edge.id;
    renderFlow();
}
async function applyStructureEdit(
    operation                                                                                ,
)                {
    const frame = currentFrame();
    const frameCapability = currentStructureCapability();
    const blockCapability = currentBlockStructureCapability();
    if (!frame || !frameCapability || project?.writable !== true || fieldDraft !== undefined) return;
    const frameOperation = operation === "copy-frame" || operation === "delete-frame";
    const capabilityId = frameOperation ? frameCapability.capabilityId : blockCapability?.capabilityId;
    if (capabilityId === undefined) return;
    let newFrameId                    ;
    if (operation === "copy-frame") {
        const raw = window.prompt("输入新帧 ID（0–599）。副本内部跳转不会自动修改。", String(frame.frameId));
        if (raw === null || !/^\d{1,3}$/u.test(raw.trim())) return;
        newFrameId = Number(raw);
        if (!Number.isSafeInteger(newFrameId) || newFrameId < 0 || newFrameId >= 600) return;
    }
    if ((operation === "delete-frame" || operation === "delete-block")
        && !window.confirm("此操作将删除完整 DAT 字节区间，且不会自动修复任何引用。确定继续吗？")) {
        return;
    }
    const oldOccurrence = frame.occurrence;
    const oldBlock = { ...selectedBlock };
    const preferredEntryFrame = selectedSkill()?.startFrame;
    await runExclusiveAction("edit", async () => {
        previewScheduler.invalidate();
        project = normalize(await request("/api/project/edit-structure", {
            method: "POST",
            body: JSON.stringify({
                sessionId: project .sessionId,
                capabilityId,
                operation,
                ...(newFrameId === undefined ? {} : { newFrameId }),
                expectedRevision: project .revision,
            }),
        }, true));
        rebuildSkillEntries(preferredEntryFrame);
        fieldDraft = undefined;
        selectedFlowEdgeId = undefined;
        let selectedFrame                   ;
        if (operation === "copy-frame") {
            selectedFrame = project.frames.find((candidate) => (
                candidate.occurrence === oldOccurrence + 1 && candidate.frameId === newFrameId
            ));
            selectedBlock = { type: "frame" };
        } else if (operation === "delete-frame") {
            selectedFrame = project.frames.find((candidate) => candidate.occurrence === oldOccurrence)
                ?? [...project.frames].reverse().find((candidate) => candidate.occurrence < oldOccurrence);
            selectedBlock = { type: "frame" };
        } else {
            selectedFrame = project.frames.find((candidate) => candidate.occurrence === oldOccurrence);
            if (selectedFrame && oldBlock.type !== "frame") {
                const collection = selectedFrame[blockCollections[oldBlock.type]]           ;
                const count = Array.isArray(collection) ? collection.length : 0;
                if (operation === "copy-block") {
                    selectedBlock = { type: oldBlock.type, index: Math.min((oldBlock.index ?? 0) + 1, count - 1) };
                } else if (operation === "create-block") {
                    selectedBlock = { type: oldBlock.type, index: count - 1 };
                } else if (count > 0) {
                    selectedBlock = { type: oldBlock.type, index: Math.min(oldBlock.index ?? 0, count - 1) };
                } else {
                    selectedBlock = { type: "frame" };
                }
            }
        }
        selectedFrameOccurrence = selectedFrame?.occurrence;
        diagnostics .textContent = "结构修改已应用到当前会话，未自动修复引用，尚未覆盖 DAT 文件。";
        render();
        if (selectedFrame) {
            try {
                await preview(selectedFrame.frameId);
            } catch (error) {
                diagnostics .textContent = `结构已修改，但${errorText(error, "预览刷新失败。")}`;
            }
        }
    });
}
async function saveProject()                {
    if (actionBusy.save || isSelectionLocked() || project?.writable !== true || !project.dirty || !window.confirm("确定覆盖当前工作区中的 DAT 文件吗？")) return;
    await runExclusiveAction("save", async () => {
        previewScheduler.invalidate();
        const response = await request("/api/project/save", { method: "POST", body: JSON.stringify({ sessionId: project .sessionId, expectedRevision: project .revision }) }, true);
        const saved = normalize(response), recovery = record(record(response.data).recovery), backup = record(recovery.backup);
        project = { ...project , revision: saved.revision, dirty: false };
        diagnostics .textContent = backup.exists === true
            ? `DAT 已安全保存，恢复备份：${text(backup.name, "名称不可用")}`
            : "DAT 已安全保存并覆盖工作区文件。";
        syncSaveState();
    });
}

objectSelect?.addEventListener("change", () => {
    const option = objectSelect.selectedOptions[0];
    switchObject(objectSelect.value, number(Number(option?.dataset.oid)));
});
frameSelect?.addEventListener("change", () => {
    const frame = project?.frames.find((candidate) => candidate.occurrence === Number(frameSelect.value));
    if (frame) void selectFrame(frame.frameId, frame.occurrence, true).catch((error) => diagnostics .textContent = errorText(error, "预览失败。"));
});
seek?.addEventListener("input", () => { tickIndex = Number(seek.value); update(); });
select("step-once")?.addEventListener("click", step);
select("reset-timeline")?.addEventListener("click", () => { tickIndex = 0; update(); });
playButton?.addEventListener("click", () => setPlaying(!playing));
select("step-back")?.addEventListener("click", () => { tickIndex = Math.max(0, tickIndex - 1); update(); });
select("jump-end")?.addEventListener("click", () => { tickIndex = playbackEndIndex(); update(); });
select("edit-skill")?.addEventListener("click", openSkillDialog);
select("show-hidden-skills")?.addEventListener("change", renderSkillList);
select                 ("skill-form")?.addEventListener("submit", (event) => void submitSkillForm(event).catch((error) => diagnostics .textContent = errorText(error, "技能保存失败。")));
select                 ("frame-editor")?.addEventListener("submit", (event) => { event.preventDefault(); void applyDraft().catch((error) => diagnostics .textContent = errorText(error, "修改失败。")); });
select("discard-draft")?.addEventListener("click", () => { clearDraft(); renderFields(); });
select("apply-session")?.addEventListener("click", () => select                 ("frame-editor")?.requestSubmit());
select("save-project")?.addEventListener("click", () => void saveProject().catch((error) => diagnostics .textContent = errorText(error, "保存失败。")));
blockSelect?.addEventListener("change", () => { selectedBlock = parseBlockSelection(blockSelect.value); renderFields(); requestPreviewRender(); });
select("apply-flow-edge")?.addEventListener("click", () => void applyFlowReroute().catch((error) => {
    diagnostics .textContent = errorText(error, "流程重定向失败。");
}));
select("copy-frame")?.addEventListener("click", () => reportOperation(applyStructureEdit("copy-frame"), "帧复制失败。"));
select("delete-frame")?.addEventListener("click", () => reportOperation(applyStructureEdit("delete-frame"), "帧删除失败。"));
select("new-block")?.addEventListener("click", () => reportOperation(applyStructureEdit("create-block"), "数据块新建失败。"));
select("copy-block")?.addEventListener("click", () => reportOperation(applyStructureEdit("copy-block"), "数据块复制失败。"));
select("delete-block")?.addEventListener("click", () => reportOperation(applyStructureEdit("delete-block"), "数据块删除失败。"));
select("zoom-in")?.addEventListener("click", () => setZoom(zoom + .1));
select("zoom-out")?.addEventListener("click", () => setZoom(zoom - .1));
select("fit-preview")?.addEventListener("click", () => setZoom(1));
function setZoom(value        )       {
    zoom = Math.max(.5, Math.min(2, Math.round(value * 10) / 10));
    canvas?.style.setProperty("--preview-zoom", String(zoom)); setText("zoom-readout", `${Math.round(zoom * 100)}%`);
}
document.querySelectorAll                   ("[data-overlay]").forEach((button) => button.addEventListener("click", () => {
    const type = button.dataset.overlay               ;
    if (visibleOverlays.has(type)) visibleOverlays.delete(type); else visibleOverlays.add(type);
    button.ariaPressed = String(visibleOverlays.has(type)); requestPreviewRender();
}));
canvas?.addEventListener("mousemove", (event) => {
    const rect = canvas.getBoundingClientRect(), x = Math.round((event.clientX - rect.left) * canvas.width / rect.width), y = Math.round((event.clientY - rect.top) * canvas.height / rect.height);
    setText("coordinate-readout", `坐标 (${x}, ${y})`);
});
function canvasPoint(event                              )                           {
    const rect = canvas .getBoundingClientRect();
    if (event instanceof KeyboardEvent) return { x: 0, y: 0 };
    return {
        x: (event.clientX - rect.left) * canvas .width / rect.width,
        y: (event.clientY - rect.top) * canvas .height / rect.height,
    };
}
function canvasCapabilities(hit                 )                                            {
    const frame = currentFrame();
    if (!frame) return {};
    const block                 = { type: hit.type, index: hit.index };
    return Object.fromEntries(["x", "y", "w", "h"].flatMap((key) => {
        const capability = fieldCapability(frame, key, block);
        return capability === undefined ? [] : [[key, capability]];
    }));
}
function canvasBlockValues(hit                 )                                                   {
    const frame = currentFrame();
    if (!frame) return { x: 0, y: 0 };
    const values = frame[blockCollections[hit.type]]           ;
    const item = Array.isArray(values) ? record(values[hit.index]) : {};
    return {
        x: number(item.x),
        y: number(item.y),
        ...(hit.kind === "rect" ? { w: number(item.w), h: number(item.h) } : {}),
    };
}
function finishCanvasInteraction(pointerId = canvasInteraction?.pointerId)       {
    canvasInteraction = undefined;
    canvasDraftGeometry = undefined;
    if (pointerId !== undefined) {
        try { canvas?.releasePointerCapture(pointerId); } catch {}
    }
    syncActionState();
    requestPreviewRender();
}
canvas?.addEventListener("pointerdown", (event) => {
    if (!blockSelect || isSelectionLocked()) return;
    const point = canvasPoint(event);
    const hit = hitTestOverlay(currentGeometry, point.x, point.y);
    if (!hit) return;
    selectedBlock = { type: hit.type, index: hit.index };
    populateBlockSelect();
    renderFields();
    if (project?.writable !== true) return;
    const capabilities = canvasCapabilities(hit);
    const handle = hit.kind === "rect" ? hitResizeHandle(hit, point.x, point.y) : undefined;
    const canResize = handle !== undefined
        && ["x", "y", "w", "h"].every((key) => capabilities[key] !== undefined);
    const canMove = capabilities.x !== undefined && capabilities.y !== undefined;
    if (!canResize && !canMove) return;
    const primary = primaryEntity();
    canvasInteraction = {
        pointerId: event.pointerId,
        startX: point.x,
        startY: point.y,
        hit,
        ...(canResize ? { handle } : {}),
        mirror: number(primary?.facing) === 1,
        values: canvasBlockValues(hit),
        capabilities,
    };
    canvasDraftGeometry = hit;
    canvas.focus();
    canvas.setPointerCapture(event.pointerId);
    syncActionState();
});
canvas?.addEventListener("pointermove", (event) => {
    const interaction = canvasInteraction;
    if (!interaction || interaction.pointerId !== event.pointerId) return;
    const point = canvasPoint(event);
    const grid        = select                  ("grid-four")?.checked ? 4 : 1;
    const dx = snapDelta(point.x - interaction.startX, grid);
    const dy = snapDelta(point.y - interaction.startY, grid);
    canvasDraftGeometry = draftOverlayGeometry(interaction.hit, dx, dy, interaction.handle);
    requestPreviewRender();
});
canvas?.addEventListener("pointerup", (event) => {
    const interaction = canvasInteraction;
    if (!interaction || interaction.pointerId !== event.pointerId) return;
    finishCanvasInteraction(event.pointerId);
    const point = canvasPoint(event);
    const grid        = select                  ("grid-four")?.checked ? 4 : 1;
    const dx = snapDelta(point.x - interaction.startX, grid);
    const dy = snapDelta(point.y - interaction.startY, grid);
    if (dx === 0 && dy === 0) return;
    const next = interaction.handle && interaction.values.w !== undefined && interaction.values.h !== undefined
        ? resizeDatRect(interaction.values                                                  , interaction.handle, dx, dy, interaction.mirror)
        : moveDatPoint(interaction.values, dx, dy, interaction.mirror);
    if (next === undefined) return;
    const keys = interaction.handle ? ["x", "y", "w", "h"] : ["x", "y"];
    reportOperation(applyBatchEdits(keys.map((key) => ({
        fieldId: interaction.capabilities[key] .fieldId,
        value: next[key                     ] ,
    })), currentFrame()?.frameId ?? 0), "画布几何修改失败。");
});
canvas?.addEventListener("pointercancel", (event) => finishCanvasInteraction(event.pointerId));
canvas?.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        finishCanvasInteraction();
        return;
    }
    if (actionBusy.edit || project?.writable !== true || fieldDraft !== undefined || selectedBlock.type === "frame") return;
    const frame = currentFrame();
    if (!frame) return;
    const x = fieldCapability(frame, "x", selectedBlock), y = fieldCapability(frame, "y", selectedBlock);
    if (!x || !y) return;
    const delta = ({
        ArrowLeft: [-1, 0],
        ArrowRight: [1, 0],
        ArrowUp: [0, -1],
        ArrowDown: [0, 1],
    }         )[event.key];
    if (!delta) return;
    event.preventDefault();
    const primary = primaryEntity();
    const amount = event.shiftKey ? 4 : 1;
    const next = moveDatPoint(
        { x: number(x.value), y: number(y.value) },
        delta[0] * amount,
        delta[1] * amount,
        number(primary?.facing) === 1,
    );
    reportOperation(applyBatchEdits([
        { fieldId: x.fieldId, value: next.x },
        { fieldId: y.fieldId, value: next.y },
    ], frame.frameId), "键盘几何修改失败。");
});
document.querySelectorAll                   ("[data-tab-target]").forEach((button) => button.addEventListener("click", () => {
    document.querySelectorAll             ("[data-mobile-panel]").forEach((panel) => panel.classList.toggle("is-mobile-active", panel.id === button.dataset.tabTarget));
    document.querySelectorAll                   ("[data-tab-target]").forEach((candidate) => candidate.ariaPressed = String(candidate === button));
}));
window.addEventListener("beforeunload", (event) => {
    if (!fieldDraft && !project?.dirty) return;
    event.preventDefault();
    event.returnValue = "";
});
window.addEventListener("pagehide", (event) => {
    if (event.persisted) return;
    if (!project?.sessionId || !stateToken) return;
    void closeProjectSession(project.sessionId, true).catch(() => undefined);
});
initializePanelSplitters();
void start();
