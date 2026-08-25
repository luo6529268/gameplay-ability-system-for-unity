// dat-skill-flow-build:20260823084542620-bbbbe9c535ad4e8291a2ba3b39069af7
import {
    drawPreviewCanvas,
    preloadPreviewObjectAssets,
                       
                        
                      
                     
} from "./preview-renderer.js";
import {
    lastFrameForId,
    primaryPreviewEntity,
} from "./project-client.js";
import {
    RENDER_CADENCE_RATES,
    renderCadenceLoopDurationMs,
    sampleRenderCadence,
                           
} from "./render-cadence-sampler.js";
import {
    buildFrameEntryCatalog,
    buildSkillPreviewScenario,
                    
                              
} from "./skill-entries.js";
import {
    errorText,
    list,
    number,
    record,
    text,
               
              
} from "./editor-support.js";

                         
                               
                         
                               
                                  
                                          
                                 
 

                                                 
                               
                                       
                         
                          
                               
                                  
                                                 
                               
                                                      
 

                                
                                           
                         
                                                      
                                  
 

                       
                                     
                                       
                                  
 

const select =                    (id        )           => document.querySelector   (`#${id}`);
const status = select             ("cadence-status");
const summary = select             ("cadence-summary");
const packageSelect = select                   ("cadence-package");
const characterSelect = select                   ("cadence-character");
const skillSelect = select                   ("cadence-skill");
const replayButton = select                   ("cadence-replay");
const playButton = select                   ("cadence-play");
const resetButton = select                   ("cadence-reset");
const speedSelect = select                   ("cadence-speed");
const loopToggle = select                  ("cadence-loop");

const panes                         = Object.freeze(
    Array.from(document.querySelectorAll             ("[data-cadence-rate]")).flatMap((pane)                => {
        const rate = Number(pane.dataset.cadenceRate);
        const canvas = pane.querySelector                   ("canvas");
        const readout = pane.querySelector             (".pane-readout");
        return RENDER_CADENCE_RATES.includes(rate                     ) && canvas !== null && readout !== null
            ? [{ rate: rate                     , canvas, readout }]
            : [];
    }),
);

let stateToken = "";
let tokenHeader = "x-dat-skill-flow-token";
let project                            ;
let catalogChoices                           = [];
let skills                        = [];
let selectedSkill                        ;
let playbackMs = 0;
let playing = false;
let playbackSpeed = 1;
let lastAnimationMs = performance.now();
let loadedObjectKey = "";
let pendingRender = false;
const images = new Map                          ();
const colorKeyImages = new Map                           ();

function setStatus(value        , state                                    = "connected")       {
    if (status === null) return;
    status.textContent = value;
    status.dataset.state = state;
}

async function request(path        , init              , stateChanging = false)                {
    const headers                         = { Accept: "application/json", ...(init?.headers                           ?? {}) };
    if (stateChanging) {
        headers["Content-Type"] = "application/json";
        headers[tokenHeader] = stateToken;
    }
    const response = await fetch(path, { ...init, headers });
    const body = record(await response.json());
    if (!response.ok) {
        const detail = list(body.diagnostics).map((item) => text(record(item).message)).find((value) => value !== "");
        throw new Error(detail || `请求失败（HTTP ${response.status}）：${path}`);
    }
    return body;
}

function responseSession(payload      )       {
    const data = record(payload.data);
    return record(data.document ?? data.session ?? data.project ?? data);
}

function responsePreview(payload      )       {
    const data = record(payload.data);
    const session = responseSession(payload);
    return record(session.nativePreview ?? session.preview ?? session.trace ?? data.nativePreview ?? data.preview ?? data.trace ?? data);
}

function normalizeStage(value         )                           {
    const raw = record(value);
    const width = number(raw.width, -1);
    if (width < 0) return undefined;
    const background = record(raw.background);
    const layers = list(background.layers).map((value) => {
        const layer = record(value);
        return {
            transparency: number(layer.transparency),
            parallaxWidth: number(layer.parallaxWidth ?? layer.parallax_width),
            x: number(layer.x),
            y: number(layer.y),
            loop: number(layer.loop),
            cc: number(layer.cc),
            c1: number(layer.c1),
            c2: number(layer.c2),
            animCounter: number(layer.animCounter ?? layer.anim_counter),
            ...(text(layer.assetId) === "" ? {} : { assetId: text(layer.assetId) }),
        };
    });
    const shadow = record(background.shadow);
    return {
        width,
        zMin: number(raw.zMin ?? raw.z_min),
        zMax: number(raw.zMax ?? raw.z_max),
        ...(layers.length === 0 && Object.keys(shadow).length === 0 ? {} : {
            background: {
                layers,
                ...(Object.keys(shadow).length === 0 ? {} : {
                    shadow: {
                        width: number(shadow.width),
                        height: number(shadow.height),
                        ...(text(shadow.assetId) === "" ? {} : { assetId: text(shadow.assetId) }),
                    },
                }),
            },
        }),
    };
}

function normalizeTicks(value         )                         {
    return Object.freeze(list(value).map((value) => {
        const raw = record(value);
        return {
            ...raw,
            tick: number(raw.tick),
            cameraX: number(raw.cameraX ?? raw.camera_x),
            entities: list(raw.entities).map((value) => {
                const entity = record(value);
                return {
                    ...entity,
                    slot: number(entity.slot, -1),
                    oid: number(entity.oid),
                    frame: number(entity.frame),
                    x: number(entity.x),
                    y: number(entity.y),
                    z: number(entity.z),
                    xInt: number(entity.xInt ?? entity.x_int ?? entity.x),
                    yInt: number(entity.yInt ?? entity.y_int ?? entity.y),
                    zInt: number(entity.zInt ?? entity.z_int ?? entity.z),
                    displayZ: number(entity.displayZ ?? entity.display_z ?? entity.zInt ?? entity.z_int ?? entity.z),
                    renderOffsetX: number(entity.renderOffsetX ?? entity.render_offset_x),
                    frameDelay: number(entity.frameDelay ?? entity.frame_delay),
                    hitStop: number(entity.hitStop ?? entity.hit_stop),
                };
            }),
        }               ;
    }));
}

function normalizePreviewObjects(value         )                           {
    return Object.freeze(list(value).flatMap((value)                  => {
        const resource = record(value);
        const oid = number(resource.oid, -1);
        if (oid < 0) return [];
        return [{
            oid,
            frames: list(resource.frames).map((value, index) => {
                const frame = record(value);
                return {
                    ...frame,
                    frameId: number(frame.frameId ?? frame.frame_id ?? frame.id, index),
                    occurrence: number(frame.occurrence, index),
                    pic: number(frame.pic),
                    state: number(frame.state),
                    centerx: number(frame.centerx ?? frame.center_x),
                    centery: number(frame.centery ?? frame.center_y),
                }         ;
            }),
            ranges: list(resource.spriteRanges ?? resource.ranges).map(record),
        }];
    }));
}

function normalizeNativePreview(payload      )                       {
    const preview = responsePreview(payload);
    const metadata = record(preview.metadata);
    return Object.freeze({
        ticks: normalizeTicks(preview.ticks ?? preview.nativeTicks),
        trace: record(preview.trace),
        previewObjects: normalizePreviewObjects(preview.resources ?? preview.renderResources ?? preview.render_resources),
        ...(normalizeStage(metadata.stage) === undefined ? {} : { stage: normalizeStage(metadata.stage)  }),
    });
}

function normalizeProject(payload      )                 {
    const data = record(payload.data);
    const session = responseSession(payload);
    const projection = record(session.projection ?? data.projection);
    const assets = new Map                ();
    for (const value of list(session.assets ?? session.spriteAssets ?? data.assets ?? data.spriteAssets)) {
        const asset = record(value);
        const assetId = text(asset.assetId ?? asset.id);
        if (assetId !== "") assets.set(text(asset.file), assetId);
    }
    const fallbackAsset = text(session.assetId ?? data.assetId);
    if (fallbackAsset !== "") assets.set("", fallbackAsset);
    const frames = list(projection.frames ?? session.frames).map((value, index) => {
        const frame = record(value);
        return {
            ...frame,
            frameId: number(frame.frameId ?? frame.frame_id ?? frame.id, index),
            occurrence: number(frame.occurrence, index),
            pic: number(frame.pic),
            state: number(frame.state),
            centerx: number(frame.centerx ?? frame.center_x),
            centery: number(frame.centery ?? frame.center_y),
        }         ;
    });
    const preview = normalizeNativePreview(payload);
    return Object.freeze({
        sessionId: text(session.sessionId ?? data.sessionId),
        revision: (session.revision ?? data.revision ?? "-")                   ,
        oid: number(session.oid ?? data.oid),
        name: text(session.name ?? data.name, "未命名角色"),
        packageId: text(session.packageId ?? data.packageId, "ntsd-2.4.1"),
        packageLabel: text(session.packageLabel ?? data.packageLabel, "NTSD 2.4.1"),
        frames,
        ranges: list(projection.spriteRanges ?? session.spriteRanges ?? data.spriteRanges).map(record),
        assets,
        nativeTicks: preview.ticks,
        nativeTrace: preview.trace,
        previewObjects: preview.previewObjects,
        ...(preview.stage === undefined ? {} : { stage: preview.stage }),
    });
}

function previewProject(value                )                 {
    return value;
}

function populatePackages()                            {
    const packages = [...new Map(catalogChoices.map((choice) => [choice.packageId, choice])).values()];
    packageSelect?.replaceChildren(...packages.map((choice) => new Option(
        `${choice.sourceKind === "patch" ? "补丁包" : "基础版"} · ${choice.packageLabel}`,
        choice.packageId,
        false,
        choice.sourceKind === "base",
    )));
    return packages.find((choice) => choice.packageId === packageSelect?.value) ?? packages[0];
}

function populateCharacters(packageId        )                            {
    const choices = catalogChoices.filter((choice) => choice.packageId === packageId);
    const preferred = choices.find((choice) => choice.sourceKind === "base" && choice.oid === 2) ?? choices[0];
    characterSelect?.replaceChildren(...choices.map((choice) => new Option(
        `OID ${choice.oid} · ${choice.displayName}`,
        choice.objectKey,
        false,
        choice.objectKey === preferred?.objectKey,
    )));
    return preferred;
}

function populateSkills()                         {
    const playable = skills.filter((skill) => skill.actionRole !== "internal");
    skillSelect?.replaceChildren(...playable.map((skill) => new Option(
        `${skill.group} · ${skill.displayName} · F${skill.startFrame}`,
        skill.id,
        false,
        skill.category !== "base",
    )));
    const selected = playable.find((skill) => skill.id === skillSelect?.value) ?? playable[0];
    if (skillSelect !== null && selected !== undefined) skillSelect.value = selected.id;
    return selected;
}

function setControlsEnabled(enabled         )       {
    for (const element of [packageSelect, characterSelect, skillSelect, replayButton, playButton, resetButton, speedSelect, loopToggle]) {
        if (element !== null) element.disabled = !enabled;
    }
}

function resetPlayback()       {
    playbackMs = 0;
    playing = false;
    if (playButton !== null) {
        playButton.textContent = "播放";
        playButton.ariaPressed = "false";
    }
}

async function closeCurrentSession()                {
    if (project?.sessionId === undefined || stateToken === "") return;
    await request("/api/project/close", {
        method: "POST",
        body: JSON.stringify({ sessionId: project.sessionId }),
    }, true);
}

async function openCharacter(choice               )                {
    setStatus(`正在载入 ${choice.packageLabel} / OID ${choice.oid}…`, "loading");
    resetPlayback();
    const previousSessionId = project?.sessionId;
    const response = await request("/api/project/open", {
        method: "POST",
        body: JSON.stringify({ objectKey: choice.objectKey }),
    }, true);
    const nextProject = normalizeProject(response);
    project = nextProject;
    loadedObjectKey = choice.objectKey;
    images.clear();
    colorKeyImages.clear();
    skills = buildFrameEntryCatalog(project.frames, project.oid).entries;
    selectedSkill = populateSkills();
    if (previousSessionId !== undefined && previousSessionId !== nextProject.sessionId) {
        void request("/api/project/close", {
            method: "POST",
            body: JSON.stringify({ sessionId: previousSessionId }),
        }, true).catch(() => undefined);
    }
    setStatus(`已载入 ${project.packageLabel} / ${project.name}。只读模式下不能编辑或保存 DAT。`);
    if (selectedSkill !== undefined) await replaySelectedSkill();
}

function selectedSkillFromUi()                         {
    return skills.find((skill) => skill.id === skillSelect?.value);
}

function movementSummary(ticks                        )         {
    const rootPositions = ticks.flatMap((tick) => {
        const root = primaryPreviewEntity(tick.entities);
        return root === undefined ? [] : [{ x: number(root.xInt ?? root.x), y: number(root.yInt ?? root.y), z: number(root.zInt ?? root.z) }];
    });
    if (rootPositions.length === 0) return "主角色未出现在 trace 中。";
    const xs = rootPositions.map((value) => value.x);
    const ys = rootPositions.map((value) => value.y);
    const zs = rootPositions.map((value) => value.z);
    return `主角色轨迹范围：ΔX ${Math.max(...xs) - Math.min(...xs)} · ΔY ${Math.max(...ys) - Math.min(...ys)} · ΔZ ${Math.max(...zs) - Math.min(...zs)}。`;
}

async function replaySelectedSkill()                {
    if (project === undefined) return;
    const skill = selectedSkillFromUi();
    if (skill === undefined) return;
    selectedSkill = skill;
    const scenario                       = buildSkillPreviewScenario(project.frames, skill);
    setStatus(`正在生成 ${skill.displayName} / F${skill.startFrame} 的 Native trace…`, "loading");
    resetPlayback();
    const response = await request("/api/project/preview", {
        method: "POST",
        body: JSON.stringify({
            sessionId: project.sessionId,
            expectedRevision: project.revision,
            startFrame: scenario.startFrame,
            initialFrame: scenario.initialFrame,
            inputPlan: scenario.inputPlan,
            ticks: scenario.ticks,
        }),
    }, true);
    const nativePreview = normalizeNativePreview(response);
    project = Object.freeze({
        ...project,
        nativeTicks: nativePreview.ticks,
        nativeTrace: nativePreview.trace,
        previewObjects: nativePreview.previewObjects,
        ...(nativePreview.stage === undefined ? {} : { stage: nativePreview.stage }),
    });
    void preloadPreviewObjectAssets(previewProject(project), images, requestRender).catch(() => undefined);
    setStatus(`已生成 ${project.nativeTicks.length} 个 Native tick：${skill.displayName} / F${skill.startFrame}。`);
    if (summary !== null) {
        summary.textContent = `${movementSummary(project.nativeTicks)} 三栏使用同一份 trace；30 Hz 为离散 snapshot，60/120 Hz 仅平滑表现坐标。`;
    }
    requestRender();
}

function requestRender()       {
    pendingRender = true;
}

function drawEmptyPane(pane             , message        )       {
    const context = pane.canvas.getContext("2d");
    if (context === null) return;
    context.clearRect(0, 0, pane.canvas.width, pane.canvas.height);
    context.fillStyle = "#0a1015";
    context.fillRect(0, 0, pane.canvas.width, pane.canvas.height);
    context.fillStyle = "#9db1c4";
    context.font = "16px Segoe UI, Microsoft YaHei, sans-serif";
    context.fillText(message, 24, 32);
}

function drawPane(pane             )       {
    if (project === undefined || project.nativeTicks.length === 0) {
        drawEmptyPane(pane, "尚未生成 Native skill trace。");
        pane.readout.textContent = "等待角色与技能。";
        return;
    }
    const loopDuration = renderCadenceLoopDurationMs(project.nativeTicks);
    const elapsed = loopToggle?.checked === true ? playbackMs % loopDuration : playbackMs;
    const sample = sampleRenderCadence(project.nativeTicks, elapsed, pane.rate);
    const tick = sample.presentationTick;
    if (tick === undefined) {
        drawEmptyPane(pane, "当前 trace 没有可绘制 tick。");
        return;
    }
    const root = primaryPreviewEntity(tick.entities);
    const runtimeFrame = lastFrameForId(project.frames, root?.frame);
    drawPreviewCanvas({
        canvas: pane.canvas,
        project: previewProject(project),
        tick,
        runtimeFrame,
        images,
        colorKeyImages,
        visibleOverlays: new Set(),
        requestRender,
    });
    const alphaText = pane.rate === 30
        ? "离散 snapshot"
        : `T${sample.previousTickIndex} → T${sample.sourceTickIndex} · α=${sample.interpolationAlpha.toFixed(3)}`;
    pane.readout.textContent = `Native T${sample.sourceTickIndex} · F${root?.frame ?? "-"} · ${alphaText}`;
}

function renderPanes()       {
    panes.forEach(drawPane);
    pendingRender = false;
}

function animationFrame(nowMs        )       {
    const elapsed = Math.min(100, nowMs - lastAnimationMs);
    lastAnimationMs = nowMs;
    if (playing && project !== undefined) {
        const loopDuration = renderCadenceLoopDurationMs(project.nativeTicks);
        playbackMs += elapsed * playbackSpeed;
        if (loopToggle?.checked === true) {
            playbackMs %= loopDuration;
        } else if (playbackMs >= loopDuration) {
            playbackMs = loopDuration;
            playing = false;
            if (playButton !== null) {
                playButton.textContent = "播放";
                playButton.ariaPressed = "false";
            }
        }
        pendingRender = true;
    }
    if (pendingRender) renderPanes();
    requestAnimationFrame(animationFrame);
}

async function start()                {
    try {
        const bootstrap = record((await request("/api/bootstrap")).data);
        const security = record(bootstrap.security);
        stateToken = text(security.token ?? bootstrap.stateToken ?? bootstrap.token);
        tokenHeader = text(security.tokenHeader ?? bootstrap.tokenHeader) || tokenHeader;
        const listing = record((await request("/api/project")).data);
        catalogChoices = Object.freeze(list(listing.objects ?? listing.entries)
            .filter((value) => number(record(value).type, -1) === 0)
            .flatMap((value)                  => {
                const item = record(value);
                const objectKey = text(item.objectKey);
                if (objectKey === "") return [];
                const oid = number(item.sourceOid ?? item.oid);
                return [{
                    objectKey,
                    oid,
                    packageId: text(item.packageId, "ntsd-2.4.1"),
                    packageLabel: text(item.packageLabel, "NTSD 2.4.1"),
                    sourceKind: text(item.sourceKind) === "patch" ? "patch" : "base",
                    displayName: text(item.displayName) || `OID ${oid}`,
                }];
            }));
        const packageChoice = populatePackages();
        const characterChoice = populateCharacters(packageChoice?.packageId ?? "");
        if (characterChoice === undefined) throw new Error("当前数据包没有可播放的 type-0 角色。");
        setControlsEnabled(true);
        await openCharacter(characterChoice);
    } catch (error) {
        setStatus(errorText(error, "只读渲染帧率对比入口无法启动。"), "error");
        if (summary !== null) summary.textContent = "未生成 trace；请检查本地 2.4.1 资源、Native preview adapter 与服务启动日志。";
    }
}

packageSelect?.addEventListener("change", () => {
    const next = populateCharacters(packageSelect.value);
    if (next !== undefined) void openCharacter(next).catch((error) => setStatus(errorText(error, "角色载入失败。"), "error"));
});

characterSelect?.addEventListener("change", () => {
    const next = catalogChoices.find((choice) => choice.objectKey === characterSelect.value);
    if (next !== undefined && next.objectKey !== loadedObjectKey) {
        void openCharacter(next).catch((error) => setStatus(errorText(error, "角色载入失败。"), "error"));
    }
});

skillSelect?.addEventListener("change", () => {
    void replaySelectedSkill().catch((error) => setStatus(errorText(error, "技能 Native 回放生成失败。"), "error"));
});
replayButton?.addEventListener("click", () => {
    void replaySelectedSkill().catch((error) => setStatus(errorText(error, "技能 Native 回放生成失败。"), "error"));
});
playButton?.addEventListener("click", () => {
    playing = !playing;
    playButton.textContent = playing ? "暂停" : "播放";
    playButton.ariaPressed = String(playing);
    lastAnimationMs = performance.now();
    requestRender();
});
resetButton?.addEventListener("click", () => {
    resetPlayback();
    requestRender();
});
speedSelect?.addEventListener("change", () => {
    playbackSpeed = Number(speedSelect.value);
    lastAnimationMs = performance.now();
});
window.addEventListener("pagehide", () => {
    void closeCurrentSession().catch(() => undefined);
});

setControlsEnabled(false);
requestAnimationFrame(animationFrame);
void start();
