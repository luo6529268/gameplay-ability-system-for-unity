type Json = Record<string, unknown>;
import { mergePreview, spritePlacement } from "./project-client.js";
type Frame = Json & { frameId: number };
type TickEntity = Json & { frame: number; oid: number; x: number; y: number; z: number };
type NativeTick = Json & { entities: TickEntity[]; cameraX: number };
interface ProjectState { sessionId: string; revision: string | number; name: string; dirty: boolean; frames: Frame[]; ranges: Json[]; nativeTicks: NativeTick[]; assets: Map<string, string>; fieldIds: Map<string, string>; }

const select = <T extends Element>(id: string): T | null => document.querySelector<T>(`#${id}`);
const status = select<HTMLElement>("server-status"), diagnostics = select<HTMLElement>("diagnostics"), canvas = select<HTMLCanvasElement>("sprite-canvas");
const objectSelect = select<HTMLSelectElement>("object-select"), frameSelect = select<HTMLSelectElement>("frame-select"), seek = select<HTMLInputElement>("timeline-seek"), loop = select<HTMLInputElement>("loop-enabled"), playButton = select<HTMLButtonElement>("play-toggle"), fields = select<HTMLElement>("frame-fields");
let project: ProjectState | undefined, tickIndex = 0, playing = false, timer: number | undefined, renderRequest: number | undefined, stateToken = "", tokenHeader = "x-dat-skill-flow-token", renderedFrameId: number | undefined, dirtyFieldKey: string | undefined, loadedObjectKey = "", objectSwitchQueue = Promise.resolve();
const images = new Map<string, HTMLImageElement>();
const record = (value: unknown): Json => value !== null && typeof value === "object" ? value as Json : {};
const list = (value: unknown): unknown[] => Array.isArray(value) ? value : [];
const text = (value: unknown): string => typeof value === "string" ? value : "";
const number = (value: unknown, fallback = 0): number => typeof value === "number" && Number.isFinite(value) ? value : fallback;
const frameFieldLabels: Readonly<Record<string, string>> = Object.freeze({
    occurrence: "同编号帧序号", pic: "图片编号", state: "状态编号", wait: "持续时间", next: "下一帧",
    dvx: "水平速度", dvy: "垂直速度", dvz: "纵深速度", centerx: "中心点 X", centery: "中心点 Y",
    hit_a: "按攻击跳转", hit_d: "按防御跳转", hit_j: "按跳跃跳转", hit_Fj: "防御+跳跃跳转",
    hit_Fa: "防御+攻击跳转", hit_Da: "下+攻击跳转", hit_Ua: "上+攻击跳转", hit_ja: "跳跃+攻击跳转",
    hit_Dj: "下+跳跃跳转", hit_Uj: "上+跳跃跳转", mp: "消耗量", vaction: "武器动作帧",
});

async function request(path: string, init?: RequestInit, stateChanging = false): Promise<Json> {
    const headers: Record<string, string> = { Accept: "application/json", ...(init?.headers as Record<string, string> ?? {}) };
    if (stateChanging) { headers["Content-Type"] = "application/json"; if (stateToken) headers[tokenHeader] = stateToken; }
    const response = await fetch(path, { ...init, headers }); const body = record(await response.json());
    if (!response.ok) throw new Error(localizedRequestError(response.status, path)); return body;
}
function localizedRequestError(statusCode: number, path: string): string {
    if (statusCode === 403) return "页面会话已经失效，请刷新页面后重试。";
    if (statusCode === 404 && path === "/api/project/open") return "当前对象尚未接入原生预览，请选择 OID 2 Naruto。";
    if (statusCode === 404 && path.startsWith("/api/assets/")) return "图片资源已经失效，请重新打开项目。";
    if (statusCode === 404) return "项目会话已经失效，请重新打开项目。";
    if (statusCode === 409) return "数据版本已经变化，请重新载入后再修改。";
    if (statusCode === 413) return "请求数据过大，服务器已拒绝处理。";
    if (statusCode === 422) return "图片资源格式无效，无法预览。";
    if (statusCode === 503) return "项目服务尚未就绪，请稍后重试。";
    return `请求失败（HTTP ${statusCode}）。`;
}
function errorText(error: unknown, fallback: string): string { return error instanceof Error ? error.message : fallback; }
async function closeProjectSession(sessionId: string, keepalive = false): Promise<void> {
    await request("/api/project/close", {
        method: "POST",
        body: JSON.stringify({ sessionId }),
        keepalive,
    }, true);
}
function normalize(payload: Json): ProjectState {
    const data = record(payload.data), session = record(data.document ?? data.session ?? data.project ?? data), projection = record(session.projection ?? data.projection);
    const preview = record(session.nativePreview ?? session.preview ?? session.trace ?? data.nativePreview ?? data.preview ?? data.trace);
    const frames = list(projection.frames ?? session.frames).map((value, index) => ({ ...record(value), frameId: number(record(value).frameId ?? record(value).id, index) }));
    const assets = new Map<string, string>();
    for (const value of list(session.assets ?? session.spriteAssets ?? data.assets ?? data.spriteAssets)) { const asset = record(value); const id = text(asset.assetId ?? asset.id); if (id) assets.set(text(asset.file), id); }
    const fallbackAsset = text(session.assetId ?? data.assetId); if (fallbackAsset) assets.set("", fallbackAsset);
    const fieldIds = new Map<string, string>();
    for (const value of list(session.fields ?? data.fields)) { const field = record(value), id = text(field.fieldId ?? field.id); if (id) fieldIds.set(`${number(field.frameId)}:${text(field.key)}`, id); }
    const ticks = list(preview.ticks ?? preview.nativeTicks ?? session.ticks).map((value) => { const raw = record(value); return { ...raw, cameraX: number(raw.camera_x ?? raw.cameraX), entities: list(raw.entities).map((entity) => { const item = record(entity); return { ...item, oid: number(item.oid), frame: number(item.frame), x: number(item.x), y: number(item.y), z: number(item.z) }; }) }; });
    return { sessionId: text(session.sessionId ?? data.sessionId), revision: (session.revision ?? data.revision ?? "-") as string | number, name: text(session.name ?? data.name ?? "项目"), dirty: session.dirty === true || data.dirty === true, frames, ranges: list(projection.spriteRanges ?? session.spriteRanges ?? data.spriteRanges).map(record), nativeTicks: ticks, assets, fieldIds };
}
function currentFrame(): Frame | undefined { const primary = project?.nativeTicks[tickIndex]?.entities.find((entity) => entity.oid === 2) ?? project?.nativeTicks[tickIndex]?.entities[0]; return project?.frames.find((frame) => frame.frameId === primary?.frame) ?? project?.frames[0]; }
function requestPreviewRender(): void { if (renderRequest === undefined) renderRequest = window.requestAnimationFrame(renderFrame); }
function syncReadOnlyUi(): void {
    const frame = currentFrame(), count = project?.nativeTicks.length ?? 0; select<HTMLElement>("tick-readout")!.textContent = String(tickIndex); select<HTMLElement>("frame-readout")!.textContent = frame ? String(frame.frameId) : "-"; select<HTMLElement>("time-readout")!.textContent = `${tickIndex * 33} 毫秒`; if (frameSelect && frame) frameSelect.value = String(frame.frameId); if (renderedFrameId !== frame?.frameId) { renderedFrameId = frame?.frameId; renderFields(); } if (seek) { seek.max = String(Math.max(0, count - 1)); seek.value = String(tickIndex); } if (playButton) { playButton.textContent = playing ? "暂停" : "播放"; playButton.ariaPressed = String(playing); }
}
function update(): void { syncReadOnlyUi(); requestPreviewRender(); }
function loadImage(assetId: string): HTMLImageElement { let image = images.get(assetId); if (!image) { image = new Image(); image.src = `/api/assets/${encodeURIComponent(assetId)}`; image.onload = requestPreviewRender; images.set(assetId, image); } return image; }
function drawPreview(): void { const context = canvas?.getContext("2d"), tick = project?.nativeTicks[tickIndex]; if (!canvas || !context) return; context.clearRect(0, 0, canvas.width, canvas.height); if (!tick || !project) { context.fillText("尚未收到原生预览数据。", 20, 30); return; } for (const entity of tick.entities) { const isLoadedObject = entity.oid === 2; const frame = isLoadedObject ? project.frames.find((candidate) => candidate.frameId === entity.frame) : undefined, pic = number(frame?.pic ?? entity.pic, 999); const range = isLoadedObject ? project.ranges.find((candidate) => pic >= number(candidate.frameLo ?? candidate.frame_lo) && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)) : undefined; const w = number(range?.w, 24), h = number(range?.h, 24); const placement = spritePlacement({ xInt: number(entity.xInt ?? entity.x), yInt: number(entity.yInt ?? entity.y), zInt: number(entity.zInt ?? entity.z), renderOffsetX: number(entity.renderOffsetX), cameraX: tick.cameraX, centerX: number(frame?.centerx), centerY: number(frame?.centery), width: w, facing: number(entity.facing) }); const assetId = range === undefined ? undefined : (text(range.assetId) || project.assets.get(text(range.file)) || project.assets.get("")); if (!range || !assetId || pic === 999) { context.strokeStyle = "#f4b34b"; context.strokeRect(placement.x, placement.y, 24, 24); context.fillText(`OID ${entity.oid} · 帧 ${entity.frame}`, placement.x, placement.y - 6); continue; } const cols = number(range.row); if (w <= 0 || h <= 0 || cols <= 0) continue; const local = pic - number(range.frameLo ?? range.frame_lo), image = loadImage(assetId); context.save(); if (placement.mirror) { context.translate(placement.x + w, placement.y); context.scale(-1, 1); context.drawImage(image, (local % cols) * (w + 1), Math.floor(local / cols) * (h + 1), w, h, 0, 0, w, h); } else context.drawImage(image, (local % cols) * (w + 1), Math.floor(local / cols) * (h + 1), w, h, placement.x, placement.y, w, h); context.restore(); } }
function renderFrame(): void {
    renderRequest = undefined;
    drawPreview();
}
function render(): void { if (!project) return; frameSelect?.replaceChildren(...project.frames.map((frame) => { const option = document.createElement("option"); option.value = String(frame.frameId); option.textContent = `第 ${frame.frameId} 帧 · 图片 ${number(frame.pic)}`; return option; })); renderFields(); update(); }
function renderFields(): void { fields?.replaceChildren(); dirtyFieldKey = undefined; const frame = currentFrame(); if (!frame || !fields) return; for (const [key, value] of Object.entries(frame)) { if (typeof value !== "number" || key === "frameId") continue; const label = document.createElement("label"), input = document.createElement("input"); label.textContent = `${frameFieldLabels[key] ?? "DAT 字段"}（${key}）`; input.name = key; input.type = "number"; input.value = String(value); input.addEventListener("input", () => { dirtyFieldKey = key; }); label.append(input); fields.append(label); } }
async function preview(startFrame: number): Promise<void> { if (!project) return; const response = await request("/api/project/preview", { method: "POST", body: JSON.stringify({ sessionId: project.sessionId, expectedRevision: project.revision, startFrame, ticks: 180 }) }, true); const partial = normalize(response); project = mergePreview(project, partial.revision, partial.nativeTicks) as ProjectState; tickIndex = 0; render(); }
function step(): void { const last = Math.max(0, (project?.nativeTicks.length ?? 1) - 1); tickIndex = tickIndex >= last ? (loop?.checked ? 0 : last) : tickIndex + 1; update(); }
function schedule(): void { if (!playing || timer !== undefined) return; timer = window.setTimeout(() => { timer = undefined; step(); schedule(); }, 33); }
async function open(objectKey: string, oid: number): Promise<void> {
    if (project?.sessionId) {
        if (project.dirty && !window.confirm("当前 DAT 有未保存修改。确定放弃修改并切换对象吗？")) {
            if (objectSelect) objectSelect.value = loadedObjectKey;
            return;
        }
        await closeProjectSession(project.sessionId);
        project = undefined;
        images.clear();
        requestPreviewRender();
    }
    const response = await request("/api/project/open", { method: "POST", body: JSON.stringify({ objectKey }) }, true);
    project = normalize(response); loadedObjectKey = objectKey; tickIndex = 0; status!.textContent = `已载入 ${project.name} / OID ${oid}`; select<HTMLElement>("session-name")!.textContent = project.name; select<HTMLElement>("revision-readout")!.textContent = `修订版本 ${project.revision}`; select<HTMLElement>("dirty-readout")!.textContent = project.dirty ? "有未保存修改" : "无未保存修改"; diagnostics!.textContent = "项目数据已载入，可以播放、跳转或编辑当前帧。"; render();
}
function switchObject(objectKey: string, oid: number): void {
    const operation = objectSwitchQueue.then(() => open(objectKey, oid));
    objectSwitchQueue = operation.catch(() => undefined);
    void operation.catch((error) => {
        status!.textContent = "项目不可用";
        diagnostics!.textContent = errorText(error, "项目载入失败。");
    });
}
async function start(): Promise<void> { try { const bootstrap = record((await request("/api/bootstrap")).data), security = record(bootstrap.security); stateToken = text(security.token ?? bootstrap.stateToken ?? bootstrap.token); tokenHeader = text(security.tokenHeader ?? bootstrap.tokenHeader) || tokenHeader; const listing = record((await request("/api/project")).data); const choices = list(listing.objects ?? listing.entries); objectSelect?.replaceChildren(...choices.map((value) => { const item = record(value), oid = number(item.oid), objectKey = text(item.objectKey); const option = document.createElement("option"); option.value = objectKey; option.dataset.oid = String(oid); option.textContent = `OID ${oid}${oid === 2 ? " · Naruto（当前支持）" : " · 暂未接入预览"}`; option.selected = oid === 2; return option; })); const selected = objectSelect?.selectedOptions[0]; await open(objectSelect?.value || "", number(selected?.dataset.oid, 2)); } catch (error) { status!.textContent = "项目不可用"; diagnostics!.textContent = errorText(error, "项目载入失败。"); } }
objectSelect?.addEventListener("change", () => { const option = objectSelect.selectedOptions[0]; switchObject(objectSelect.value, number(option?.dataset.oid)); }); frameSelect?.addEventListener("change", () => void preview(Number(frameSelect.value)).catch((error) => diagnostics!.textContent = errorText(error, "预览失败。"))); seek?.addEventListener("input", () => { tickIndex = Number(seek.value); update(); }); playButton?.addEventListener("click", () => { playing = !playing; update(); schedule(); }); select("step-once")?.addEventListener("click", step); select("reset-timeline")?.addEventListener("click", () => { tickIndex = 0; update(); });
select<HTMLFormElement>("frame-editor")?.addEventListener("submit", (event) => { event.preventDefault(); const form = new FormData(event.currentTarget), frame = currentFrame(), key = dirtyFieldKey; if (!key || !project || !frame) return; const fieldId = project.fieldIds.get(`${frame.frameId}:${key}`); if (!fieldId) { diagnostics!.textContent = `字段 ${key} 没有可编辑标识，无法修改。`; return; } void request("/api/project/edit", { method: "POST", body: JSON.stringify({ sessionId: project.sessionId, fieldId, value: Number(form.get(key)), expectedRevision: project.revision }) }, true).then((response) => { project = normalize(response); select<HTMLElement>("revision-readout")!.textContent = `修订版本 ${project.revision}`; select<HTMLElement>("dirty-readout")!.textContent = project.dirty ? "有未保存修改" : "无未保存修改"; diagnostics!.textContent = "修改已应用到当前会话，尚未写入 DAT 文件。"; return preview(frame.frameId); }).catch((error) => diagnostics!.textContent = errorText(error, "修改失败。")); });
select("save-project")?.addEventListener("click", () => project && void request("/api/project/save", { method: "POST", body: JSON.stringify({ sessionId: project.sessionId, expectedRevision: project.revision }) }, true).then((response) => { const saved = normalize(response); if (project) project = { ...project, revision: saved.revision, dirty: false }; select<HTMLElement>("revision-readout")!.textContent = `修订版本 ${project?.revision ?? "-"}`; select<HTMLElement>("dirty-readout")!.textContent = "无未保存修改"; diagnostics!.textContent = "DAT 已安全保存并覆盖原文件。"; }).catch((error) => diagnostics!.textContent = errorText(error, "保存失败。")));
for (const key of ["a", "d", "j", "Fj", "Fa", "Da", "Ua", "ja", "Dj", "Uj"]) select(`hit-${key.toLowerCase()}`)?.addEventListener("click", () => { const target = number(currentFrame()?.[`hit_${key}`]); if (target >= 0) void preview(target).catch((error) => diagnostics!.textContent = errorText(error, "预览失败。")); });
window.addEventListener("beforeunload", (event) => {
    if (!project?.dirty) return;
    event.preventDefault();
    event.returnValue = "";
});
window.addEventListener("pagehide", (event) => {
    if (event.persisted) return;
    if (!project?.sessionId || !stateToken) return;
    void closeProjectSession(project.sessionId, true).catch(() => undefined);
});
void start();
