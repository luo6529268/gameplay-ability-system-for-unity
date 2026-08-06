// dat-skill-flow-build:20260806142111218-e0d3a43dddbf4ce38f99c8853e113bc9
import {
    findFrameFieldCapability,
    lastFrameForId,
    mergePreview,
    primaryPreviewEntity,
} from "./project-client.js";
import { buildSkillFlow,                                         } from "./skill-flow.js";
import {
    hitTestOverlay,
                         
                     
} from "./overlay-geometry.js";
import {
    drawPreviewCanvas,
                       
                     
} from "./preview-renderer.js";
import { createLatestTaskScheduler } from "./latest-task-scheduler.js";
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

let project                          ;
let skillState             = { revision: 0, etag: "", skills: [] };
let selectedSkillIndex = -1;
let editingSkillIndex = -1;
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
const flowCache = new Map                        ();
const visibleOverlays = new Set             (allOverlayTypes);
const images = new Map                          ();
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
    const frames = list(projection.frames ?? session.frames).map((value, index) => ({ ...record(value), frameId: number(record(value).frameId ?? record(value).id, index), occurrence: number(record(value).occurrence, index) }))           ;
    const assets = new Map                ();
    for (const value of list(session.assets ?? session.spriteAssets ?? data.assets ?? data.spriteAssets)) { const asset = record(value); const id = text(asset.assetId ?? asset.id); if (id) assets.set(text(asset.file), id); }
    const fallbackAsset = text(session.assetId ?? data.assetId); if (fallbackAsset) assets.set("", fallbackAsset);
    const fieldCapabilities = list(session.fields ?? data.fields).flatMap((value)                    => {
        const field = record(value), fieldId = text(field.fieldId ?? field.id), key = text(field.key), kind = text(field.kind), scope = text(field.scope);
        return fieldId && key && kind && scope ? [{ ...field, fieldId, key, kind, scope, value: field.value, occurrence: number(field.occurrence) }                   ] : [];
    });
    const ticks = list(preview.ticks ?? preview.nativeTicks ?? session.ticks).map((value) => { const raw = record(value); return { ...raw, cameraX: number(raw.camera_x ?? raw.cameraX), entities: list(raw.entities).map((entity) => { const item = record(entity); return { ...item, slot: number(item.slot, -1), oid: number(item.oid), frame: number(item.frame), x: number(item.x), y: number(item.y), z: number(item.z) }; }) }; });
    return { sessionId: text(session.sessionId ?? data.sessionId), revision: (session.revision ?? data.revision ?? "-")                   , name: text(session.name ?? data.name ?? "项目"), dirty: session.dirty === true || data.dirty === true, writable: session.writable === true || data.writable === true, frames, ranges: list(projection.spriteRanges ?? session.spriteRanges ?? data.spriteRanges).map(record), nativeTicks: ticks, assets, fields: fieldCapabilities };
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
            ticks: 180,
        }),
    }, true),
    syncPreviewBusy,
);
function primaryEntity()                         { return primaryPreviewEntity(project?.nativeTicks[tickIndex]?.entities ?? []); }
function currentFrame()                    { const primary = primaryEntity(), selected = project?.frames.find((frame) => frame.occurrence === selectedFrameOccurrence); if (selected && (!primary || selected.frameId === primary.frame)) return selected; return lastFrameForId(project?.frames ?? [], primary?.frame); }
function currentRuntimeFrame()                    { return lastFrameForId(project?.frames ?? [], primaryEntity()?.frame); }
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
function syncActionState()       {
    const skillForm = select                 ("skill-form"), saveSkill = select                   ("save-skill");
    const frameEditor = select                 ("frame-editor");
    const newSkill = select                   ("new-skill"), editSkill = select                   ("edit-skill");
    if (skillForm) skillForm.ariaBusy = String(actionBusy.skill);
    if (saveSkill) {
        saveSkill.textContent = actionBusy.skill ? "保存中…" : "保存技能";
        saveSkill.disabled = actionBusy.skill;
    }
    if (newSkill) newSkill.disabled = !project || actionBusy.skill;
    if (editSkill) editSkill.disabled = !project || selectedSkillIndex < 0 || actionBusy.skill;
    if (frameSelect) frameSelect.disabled = actionBusy.edit;
    if (blockSelect) blockSelect.disabled = actionBusy.edit;
    for (const id of ["play-toggle", "step-once", "reset-timeline", "step-back", "jump-end"]) {
        const control = select                   (id);
        if (control) control.disabled = actionBusy.edit;
    }
    document.querySelectorAll                   (".timeline-segment").forEach((button) => button.disabled = actionBusy.edit);
    document.querySelectorAll             ("#skill-list tr, #flow-list tr").forEach((row) => row.ariaDisabled = String(actionBusy.edit));
    if (seek) seek.disabled = actionBusy.edit;
    if (frameEditor) frameEditor.ariaBusy = String(actionBusy.edit);
    syncDraftActions();
    syncSaveState();
}
async function runExclusiveAction   (kind                 , operation                  )                         {
    if (actionBusy[kind]) return undefined;
    actionBusy[kind] = true;
    syncActionState();
    try {
        return await operation();
    } finally {
        actionBusy[kind] = false;
        syncActionState();
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
    setText("time-readout", `${tickIndex * 33} 毫秒`);
    setText("preview-frame-count", String(count));
    setText("play-state", primary ? playing ? "播放中" : "已暂停" : "主实体不可用");
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
        visibleOverlays,
        requestRender: requestPreviewRender,
    });
}
function renderFrame()       {
    renderRequest = undefined;
    drawPreview();
}
function selectedSkill()                           { return skillState.skills[selectedSkillIndex]; }
function skillFlow(startFrame        )                             {
    if (!project) return undefined;
    const projectKey = `${project.sessionId}:${project.revision}`;
    if (flowCacheProjectKey !== projectKey) {
        flowCacheProjectKey = projectKey;
        flowCache.clear();
    }
    let graph = flowCache.get(startFrame);
    if (!graph) {
        graph = buildSkillFlow(project.frames, startFrame);
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
    body.replaceChildren(...skillState.skills.map((skill, index) => {
        const row = document.createElement("tr"), graph = skillFlow(skill.startFrame);
        if (index === selectedSkillIndex) row.classList.add("is-selected");
        row.innerHTML = `<td></td><td>${skill.startFrame}</td><td>${graph?.nodes.filter((node) => node.kind === "frame").length ?? 0}</td><td>${graph?.edges.filter((edge) => edge.key !== "next" && edge.resolution === "frame").length ?? 0}</td>`;
        row.cells[0] .textContent = skill.name;
        row.addEventListener("click", () => {
            if (!actionBusy.edit) reportOperation(selectSkill(index), "技能预览失败。");
        });
        return row;
    }));
    setText("skill-count", String(skillState.skills.length));
    select             ("skill-empty") .hidden = skillState.skills.length > 0;
    const editButton = select                   ("edit-skill");
    if (editButton) editButton.disabled = selectedSkillIndex < 0 || actionBusy.skill;
}
function flowSummary(edges                          )         {
    return edges.filter((edge) => edge.key !== "next" && edge.rawTarget !== 0)
        .slice(0, 2).map((edge) => `${edge.key.replace("hit_", "")}:${edge.rawTarget}`).join(" · ") || "—";
}
function renderFlow()       {
    const body = select                         ("flow-list"), graph = currentFlow(), skill = selectedSkill();
    if (!body) return;
    setText("flow-title", skill ? `${skill.name} · 帧流程` : "当前技能帧流程");
    if (!graph || !project) { body.replaceChildren(); setText("flow-count", "0"); renderTimelineSegments(); return; }
    const edgesByFrom = new Map                         ();
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
        const frame = project .frames.find((candidate) => candidate.occurrence === node.occurrence) ;
        const edges = edgesByFrom.get(node.id) ?? [], next = edges.find((edge) => edge.key === "next");
        row.dataset.frameOccurrence = String(frame.occurrence);
        row.innerHTML = `<td>${frame.frameId}</td><td>状态 ${frame.state}</td><td>${next?.rawTarget ?? "—"}</td><td>${flowSummary(edges)}</td>`;
        row.addEventListener("click", () => {
            if (!actionBusy.edit) reportOperation(selectFrame(frame.frameId, frame.occurrence, true), "预览失败。");
        });
        return row;
    });
    body.replaceChildren(...rows);
    highlightedFrameOccurrence = undefined;
    setText("flow-count", String(graph.nodes.filter((node) => node.kind === "frame").length));
    renderTimelineSegments(graph);
}
function renderTimelineSegments(graph = currentFlow())       {
    const container = select             ("timeline-segments"), markers = select             ("timeline-markers");
    if (!container || !markers) return;
    const nodes = graph?.nodes.filter((node) => node.kind === "frame") ?? [];
    container.replaceChildren(...nodes.map((node) => {
        const button = document.createElement("button");
        button.type = "button"; button.className = "timeline-segment"; button.dataset.frameOccurrence = String(node.occurrence);
        button.textContent = `帧 ${node.frameId}`;
        button.disabled = actionBusy.edit;
        button.addEventListener("click", () => {
            if (!actionBusy.edit) reportOperation(selectFrame(node.frameId, node.occurrence, true), "预览失败。");
        });
        return button;
    }));
    markers.replaceChildren(...nodes.map((node, index) => {
        const marker = document.createElement("span");
        marker.className = "timeline-marker"; marker.style.left = `${((index + .5) / Math.max(1, nodes.length)) * 100}%`;
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
    syncDraftActions();
    syncSaveState();
}
function clearDraft()       {
    fieldDraft = undefined;
    fields?.querySelectorAll(".is-dirty").forEach((candidate) => candidate.classList.remove("is-dirty"));
    syncDraftActions();
    syncSaveState();
}
function syncDraftActions()       {
    const apply = select                   ("apply-draft"), topApply = select                   ("apply-session");
    const discard = select                   ("discard-draft");
    const canApply = project?.writable === true && fieldDraft?.valid === true && fieldDraft.value !== undefined && !actionBusy.edit;
    if (apply) {
        apply.textContent = actionBusy.edit ? "应用中…" : "应用本次修改";
        apply.disabled = !canApply;
    }
    if (topApply) {
        topApply.textContent = actionBusy.edit ? "应用中…" : "应用会话修改";
        topApply.disabled = !canApply;
    }
    if (discard) discard.disabled = fieldDraft === undefined || actionBusy.edit;
    fields?.querySelectorAll                  ("input[data-field-id]").forEach((input) => {
        input.disabled = project?.writable !== true || actionBusy.edit || (fieldDraft !== undefined && input.dataset.fieldId !== fieldDraft.capability.fieldId);
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
    project = mergePreview(project, partial.revision, partial.nativeTicks)                ;
    tickIndex = 0;
    render();
}
function step()       {
    const last = Math.max(0, (project?.nativeTicks.length ?? 1) - 1);
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
        save.disabled = project?.writable !== true || !dirty || fieldDraft !== undefined || actionBusy.save || actionBusy.edit;
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
        skills: list(data.skills).map((value) => ({ oid: number(record(value).oid), name: text(record(value).name), startFrame: number(record(value).startFrame) })),
    };
    selectedSkillIndex = skillState.skills.findIndex((skill) => skill.oid === 2);
}
async function saveSkills(skills                )                {
    const response = await request("/api/project/skills", { method: "POST", body: JSON.stringify({ expectedRevision: skillState.revision, expectedEtag: skillState.etag, skills }) }, true);
    const data = record(response.data);
    skillState = { revision: number(data.revision), etag: text(data.etag), skills: list(data.skills).map((value) => ({ oid: number(record(value).oid), name: text(record(value).name), startFrame: number(record(value).startFrame) })) };
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
        requestPreviewRender();
    }
    const response = await request("/api/project/open", { method: "POST", body: JSON.stringify({ objectKey }) }, true);
    project = normalize(response); loadedObjectKey = objectKey; tickIndex = 0; selectedBlock = { type: "frame" };
    selectedFrameOccurrence = lastFrameForId(project.frames, primaryPreviewEntity(project.nativeTicks[0]?.entities ?? [])?.frame)?.occurrence;
    status .dataset.state = "connected"; status .textContent = `已载入 ${project.name} / OID ${oid}${project.writable ? "" : "（只读）"}`;
    diagnostics .textContent = project.writable
        ? "项目数据已载入，可以选择技能、播放、查看叠加层或编辑当前帧。"
        : "项目仅存在于 fallback 资源中，当前会话为只读预览。";
    render();
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
        await open(objectSelect?.value || "", number(selected?.dataset.oid, 2));
        if (selectedSkill()) await selectSkill(selectedSkillIndex);
    } catch (error) {
        status .dataset.state = "error"; status .textContent = "项目不可用";
        diagnostics .textContent = errorText(error, "项目载入失败。");
    }
}
async function selectFrame(frameId        , occurrence        , refreshPreview         )                {
    if (actionBusy.edit) return;
    selectedFrameOccurrence = occurrence; selectedBlock = { type: "frame" }; renderedFrameKey = "";
    if (refreshPreview) await preview(frameId); else render();
}
async function selectSkill(index        )                {
    if (actionBusy.edit) return;
    const skill = skillState.skills[index];
    if (!skill || !project) return;
    selectedSkillIndex = index; renderSkillList(); renderFlow();
    const frame = lastFrameForId(project.frames, skill.startFrame);
    if (frame) await selectFrame(frame.frameId, frame.occurrence, true);
    else diagnostics .textContent = `技能“${skill.name}”的起始帧 ${skill.startFrame} 不存在。`;
}
function openSkillDialog(index        )       {
    editingSkillIndex = index;
    const skill = skillState.skills[index], title = select             ("skill-dialog-title");
    if (title) title.textContent = skill ? "编辑技能信息" : "新建技能";
    const name = select                  ("skill-name"), startFrame = select                  ("skill-start-frame");
    if (name) name.value = skill?.name ?? "";
    if (startFrame) startFrame.value = String(skill?.startFrame ?? currentFrame()?.frameId ?? 0);
    skillDialog?.showModal();
}
async function submitSkillForm(event             )                {
    event.preventDefault();
    if (event.submitter instanceof HTMLButtonElement && event.submitter.value === "cancel") { skillDialog?.close(); return; }
    const name = select                  ("skill-name")?.value.trim() ?? "", startFrame = Number(select                  ("skill-start-frame")?.value);
    if (!name || !Number.isInteger(startFrame) || startFrame < 0 || startFrame > 599) return;
    await runExclusiveAction("skill", async () => {
        const oid = number(objectSelect?.selectedOptions[0]?.dataset.oid, 2), skills = [...skillState.skills], next = { oid, name, startFrame };
        if (editingSkillIndex >= 0) skills[editingSkillIndex] = next; else skills.push(next);
        await saveSkills(skills); selectedSkillIndex = editingSkillIndex >= 0 ? editingSkillIndex : skills.length - 1;
        skillDialog?.close(); renderSkillList();
        try {
            await selectSkill(selectedSkillIndex);
        } catch (error) {
            if (diagnostics) diagnostics.textContent = `技能已保存，但${errorText(error, "预览失败。")}`;
        }
    });
}
async function applyDraft()                {
    const draft = fieldDraft;
    if (!draft?.valid || draft.value === undefined || project?.writable !== true) return;
    const previewFrameId = currentFrame()?.frameId ?? draft.capability.frameId ?? 0;
    await runExclusiveAction("edit", async () => {
        previewScheduler.invalidate();
        const response = await request("/api/project/edit", { method: "POST", body: JSON.stringify({ sessionId: project .sessionId, fieldId: draft.capability.fieldId, value: draft.value, expectedRevision: project .revision }) }, true);
        project = normalize(response); fieldDraft = undefined; diagnostics .textContent = "修改已应用到当前会话，尚未覆盖 DAT 文件。";
        render();
        try {
            await preview(previewFrameId);
        } catch (error) {
            if (diagnostics) diagnostics.textContent = `修改已应用，但${errorText(error, "预览刷新失败。")}`;
        }
    });
}
async function saveProject()                {
    if (actionBusy.edit || project?.writable !== true || !project.dirty || fieldDraft || !window.confirm("确定覆盖当前工作区中的 DAT 文件吗？")) return;
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
    switchObject(objectSelect.value, number(option?.dataset.oid));
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
select("jump-end")?.addEventListener("click", () => { tickIndex = Math.max(0, (project?.nativeTicks.length ?? 1) - 1); update(); });
select("new-skill")?.addEventListener("click", () => openSkillDialog(-1));
select("edit-skill")?.addEventListener("click", () => openSkillDialog(selectedSkillIndex));
select                 ("skill-form")?.addEventListener("submit", (event) => void submitSkillForm(event).catch((error) => diagnostics .textContent = errorText(error, "技能保存失败。")));
select                 ("frame-editor")?.addEventListener("submit", (event) => { event.preventDefault(); void applyDraft().catch((error) => diagnostics .textContent = errorText(error, "修改失败。")); });
select("discard-draft")?.addEventListener("click", () => { clearDraft(); renderFields(); });
select("apply-session")?.addEventListener("click", () => select                 ("frame-editor")?.requestSubmit());
select("save-project")?.addEventListener("click", () => void saveProject().catch((error) => diagnostics .textContent = errorText(error, "保存失败。")));
blockSelect?.addEventListener("change", () => { selectedBlock = parseBlockSelection(blockSelect.value); renderFields(); requestPreviewRender(); });
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
canvas?.addEventListener("click", (event) => {
    const rect = canvas.getBoundingClientRect(), x = (event.clientX - rect.left) * canvas.width / rect.width, y = (event.clientY - rect.top) * canvas.height / rect.height;
    const hit = hitTestOverlay(currentGeometry, x, y);
    if (actionBusy.edit || !hit || !blockSelect) return;
    selectedBlock = { type: hit.type, index: hit.index }; populateBlockSelect(); renderFields();
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
void start();
