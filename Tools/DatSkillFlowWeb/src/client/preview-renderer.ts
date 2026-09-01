import { lastFrameForId, primaryPreviewEntity, spritePlacement } from "./project-client.js";
import { buildOverlayGeometry, OVERLAY_COLORS, type OverlayGeometry, type OverlayType } from "./overlay-geometry.js";
import { number, text, type Frame, type Json } from "./editor-support.js";

export type PreviewEntity = Json & { slot: number; frame: number; oid: number; x: number; y: number; z: number };
export type PreviewTick = Json & { entities: PreviewEntity[]; cameraX: number };
export interface PreviewStage {
    readonly width: number;
    readonly zMin: number;
    readonly zMax: number;
    readonly background?: {
        readonly shadow?: {
            readonly width: number;
            readonly height: number;
            readonly assetId?: string;
        };
        readonly layers: readonly {
            readonly transparency: number;
            readonly parallaxWidth: number;
            readonly x: number;
            readonly y: number;
            readonly loop: number;
            readonly cc: number;
            readonly c1: number;
            readonly c2: number;
            readonly animCounter: number;
            readonly assetId?: string;
        }[];
    };
}
export interface PreviewProject {
    readonly frames: readonly Frame[];
    readonly ranges: readonly Json[];
    readonly previewObjects?: readonly PreviewObject[];
    readonly assets: ReadonlyMap<string, string>;
    readonly stage?: PreviewStage;
}
export interface PreviewObject {
    readonly oid: number;
    readonly frames: readonly Frame[];
    readonly ranges: readonly Json[];
}
export interface PreviewRenderInput {
    readonly canvas: HTMLCanvasElement;
    readonly project: PreviewProject;
    readonly tick: PreviewTick | undefined;
    readonly authorityTick?: PreviewTick;
    readonly runtimeFrame: Frame | undefined;
    readonly images: Map<string, HTMLImageElement>;
    readonly colorKeyImages: Map<string, HTMLCanvasElement>;
    readonly visibleOverlays: ReadonlySet<OverlayType>;
    readonly draftGeometry?: OverlayGeometry;
    readonly positionMode?: boolean;
    readonly showAxes?: boolean;
    readonly selectedPositionSlot?: 0 | 1;
    readonly requestRender: () => void;
}

export interface PreviewActorHitArea {
    readonly slot: 0 | 1;
    readonly x1: number;
    readonly y1: number;
    readonly x2: number;
    readonly y2: number;
}

type ImageFactory = () => HTMLImageElement;

function drawAxes(context: CanvasRenderingContext2D, canvas: HTMLCanvasElement, tick: PreviewTick, entity: PreviewEntity): void {
    const x = number(entity.xInt ?? entity.x) + number(entity.renderOffsetX) - tick.cameraX;
    const y = number(entity.zInt ?? entity.z) + number(entity.yInt ?? entity.y);
    context.save();
    context.strokeStyle = "rgba(45, 199, 201, .45)";
    context.setLineDash([3, 3]);
    context.beginPath();
    context.moveTo(0, y); context.lineTo(canvas.width, y);
    context.moveTo(x, 0); context.lineTo(x, canvas.height);
    context.stroke();
    context.restore();
}

function drawGeometry(context: CanvasRenderingContext2D, geometry: readonly OverlayGeometry[]): void {
    context.save();
    context.lineWidth = 2;
    for (const item of geometry) {
        context.strokeStyle = OVERLAY_COLORS[item.type];
        context.fillStyle = `${OVERLAY_COLORS[item.type]}30`;
        if (item.kind === "rect") {
            context.fillRect(item.x1, item.y1, item.width, item.height);
            context.strokeRect(item.x1, item.y1, item.width, item.height);
            continue;
        }
        context.beginPath();
        context.moveTo(item.x - 7, item.y); context.lineTo(item.x + 7, item.y);
        context.moveTo(item.x, item.y - 7); context.lineTo(item.x, item.y + 7);
        context.stroke();
    }
    context.restore();
}

function drawDraftGeometry(context: CanvasRenderingContext2D, geometry: OverlayGeometry): void {
    context.save();
    context.lineWidth = 2;
    context.strokeStyle = "#fff4a3";
    context.fillStyle = "rgba(255, 244, 163, .12)";
    context.setLineDash([5, 3]);
    if (geometry.kind === "rect") {
        context.fillRect(geometry.x1, geometry.y1, geometry.width, geometry.height);
        context.strokeRect(geometry.x1, geometry.y1, geometry.width, geometry.height);
    } else {
        context.beginPath();
        context.arc(geometry.x, geometry.y, 8, 0, Math.PI * 2);
        context.stroke();
    }
    context.restore();
}

function drawGround(context: CanvasRenderingContext2D, tick: PreviewTick): void {
    const ground = number(recordValue(tick.background)?.zMin, 0);
    if (ground <= 0) return;
    context.save();
    context.fillStyle = "rgba(80, 60, 30, .45)";
    context.fillRect(0, ground, context.canvas.width, 1);
    context.restore();
}

function recordValue(value: unknown): Record<string, unknown> | undefined {
    return typeof value === "object" && value !== null && !Array.isArray(value)
        ? value as Record<string, unknown>
        : undefined;
}

function ensureImage(
    images: Map<string, HTMLImageElement>,
    assetId: string,
    requestRender: () => void,
    createImage: ImageFactory = () => new Image(),
): HTMLImageElement {
    let image = images.get(assetId);
    if (image === undefined) {
        image = createImage();
        images.set(assetId, image);
        image.addEventListener("load", requestRender);
        image.addEventListener("error", requestRender);
        image.src = `/api/assets/${encodeURIComponent(assetId)}`;
    }
    return image;
}

function loadImage(input: PreviewRenderInput, assetId: string): HTMLImageElement {
    return ensureImage(input.images, assetId, input.requestRender);
}

export function previewObjectAssetIds(project: PreviewProject): readonly string[] {
    const result = new Set<string>();
    for (const resource of project.previewObjects ?? []) {
        for (const range of resource.ranges) {
            const assetId = text(range.assetId)
                || project.assets.get(text(range.file))
                || project.assets.get("")
                || "";
            if (assetId) result.add(assetId);
        }
    }
    return Object.freeze([...result]);
}

function waitForImage(image: HTMLImageElement): Promise<void> {
    if (image.complete) return Promise.resolve();
    return new Promise((resolve) => {
        const settle = (): void => {
            image.removeEventListener("load", settle);
            image.removeEventListener("error", settle);
            resolve();
        };
        image.addEventListener("load", settle);
        image.addEventListener("error", settle);
    });
}

export async function preloadPreviewObjectAssets(
    project: PreviewProject,
    images: Map<string, HTMLImageElement>,
    requestRender: () => void,
    createImage: ImageFactory = () => new Image(),
): Promise<void> {
    await Promise.all(previewObjectAssetIds(project).map(async (assetId) => {
        const image = ensureImage(images, assetId, requestRender, createImage);
        await waitForImage(image);
    }));
}

function loadColorKeyImage(
    input: PreviewRenderInput,
    assetId: string,
    image: HTMLImageElement,
): HTMLCanvasElement | undefined {
    const existing = input.colorKeyImages.get(assetId);
    if (existing !== undefined) return existing;
    const width = image.naturalWidth || image.width;
    const height = image.naturalHeight || image.height;
    if ((!image.complete && width <= 0) || width <= 0 || height <= 0) return undefined;
    const canvas = document.createElement("canvas");
    canvas.width = width;
    canvas.height = height;
    const canvasContext = canvas.getContext("2d");
    if (canvasContext === null) return undefined;
    try {
        canvasContext.drawImage(image, 0, 0);
        const pixels = canvasContext.getImageData(0, 0, width, height);
        for (let index = 0; index < pixels.data.length; index += 4) {
            if (pixels.data[index] === 0 && pixels.data[index + 1] === 0 && pixels.data[index + 2] === 0) {
                pixels.data[index + 3] = 0;
            }
        }
        canvasContext.putImageData(pixels, 0, 0);
    } catch {
        return undefined;
    }
    input.colorKeyImages.set(assetId, canvas);
    return canvas;
}

function imageReady(image: HTMLImageElement): boolean {
    return image.complete && (image.naturalWidth || image.width) > 0 && (image.naturalHeight || image.height) > 0;
}

function drawBackgroundLayer(
    context: CanvasRenderingContext2D,
    input: PreviewRenderInput,
    layer: NonNullable<PreviewStage["background"]>["layers"][number],
    tick: PreviewTick,
): void {
    const assetId = text(layer.assetId);
    if (!assetId || (layer.cc > 0 && (layer.animCounter < layer.c1 || layer.animCounter > layer.c2))) return;
    const image = loadImage(input, assetId);
    if (!imageReady(image)) return;
    const source = layer.transparency === 0 ? image : loadColorKeyImage(input, assetId, image);
    if (!source) return;
    const stageWidth = number(input.project.stage?.width, number(tick.background?.width, context.canvas.width));
    const parallaxWidth = number(layer.parallaxWidth, stageWidth);
    const parallax = stageParallaxOffset(stageWidth, context.canvas.width, parallaxWidth, tick.cameraX);
    if (layer.x >= parallaxWidth) return;
    const x = layer.x + parallax;
    const y = layer.y;
    const width = image.naturalWidth || image.width;
    const height = image.naturalHeight || image.height;
    context.drawImage(source, x, y, width, height);
    if (layer.loop <= 0) return;
    for (let tilePosition = layer.x + layer.loop, tileX = x + layer.loop, tileCount = 0;
        tilePosition < parallaxWidth && tileCount < 4096;
        tilePosition += layer.loop, tileX += layer.loop, tileCount += 1) {
        context.drawImage(source, tileX, y, width, height);
    }
}

function drawSceneBackground(context: CanvasRenderingContext2D, input: PreviewRenderInput, tick: PreviewTick): void {
    context.fillStyle = "#151b25";
    context.fillRect(0, 0, context.canvas.width, context.canvas.height);
    for (const layer of input.project.stage?.background?.layers ?? []) {
        drawBackgroundLayer(context, input, layer, tick);
    }
    drawGround(context, tick);
}

function drawShadow(
    context: CanvasRenderingContext2D,
    input: PreviewRenderInput,
    entity: PreviewEntity,
    primary: PreviewEntity | undefined,
    resource: PreviewObject | undefined,
): void {
    const shadow = input.project.stage?.background?.shadow;
    const assetId = text(shadow?.assetId);
    const hitStop = number(entity.hitStop);
    if (!shadow || !assetId || entity.link < 0 || hitStop <= -70 || Math.abs(hitStop) % 4 >= 2) return;
    const isPrimary = entity === primary;
    const frames = resource?.frames ?? (isPrimary ? input.project.frames : []);
    const frame = isPrimary ? input.runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
    if (frame?.state === 3005 || frame?.state === 9997) return;
    const image = loadImage(input, assetId);
    if (!imageReady(image)) return;
    const source = loadColorKeyImage(input, assetId, image);
    if (!source) return;
    const width = number(shadow.width, image.naturalWidth || image.width);
    const height = number(shadow.height, image.naturalHeight || image.height);
    const x = number(entity.xInt ?? entity.x) + number(entity.renderOffsetX) - input.tick!.cameraX - width / 2;
    const y = number(entity.zInt ?? entity.z) - height / 2;
    context.drawImage(source, 0, 0, width, height, x, y, width, height);
}

export function spriteSheetColumnCount(range: Json | undefined): number {
    // ntsd_cpp passes DAT SpriteRange::row as Renderer::load_sprite(..., cols).
    return number(range?.row);
}

export function effectivePreviewPic(entity: PreviewEntity, frame: Frame | undefined): number {
    return number(entity.renderPic ?? frame?.pic ?? entity.pic, 999);
}

export function sortPreviewEntities(entities: readonly PreviewEntity[]): PreviewEntity[] {
    return [...entities].sort((left, right) =>
        number(left.zInt ?? left.z) - number(right.zInt ?? right.z));
}

export function previewActorHitAreas(
    project: PreviewProject,
    tick: PreviewTick | undefined,
    runtimeFrame: Frame | undefined,
): readonly PreviewActorHitArea[] {
    if (tick === undefined) return [];
    const primary = primaryPreviewEntity(tick.entities);
    const resourcesByOid = new Map((project.previewObjects ?? []).map((resource) => [resource.oid, resource]));
    return tick.entities.flatMap((entity): PreviewActorHitArea[] => {
        if (entity.slot !== 0 && entity.slot !== 1) return [];
        const hitStop = number(entity.hitStop);
        if (hitStop <= -25 || Math.abs(hitStop) % 4 >= 2) return [];
        const isPrimary = entity === primary;
        const resource = resourcesByOid.get(entity.oid);
        const frames = resource?.frames ?? (isPrimary ? project.frames : []);
        const ranges = resource?.ranges ?? (isPrimary ? project.ranges : []);
        const frame = isPrimary ? runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
        const pic = effectivePreviewPic(entity, frame);
        const range = ranges.find((candidate) => (
            pic >= number(candidate.frameLo ?? candidate.frame_lo)
            && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)
        ));
        const width = number(range?.w), height = number(range?.h);
        if (!frame || !range || pic === 999 || width <= 0 || height <= 0) return [];
        const placement = spritePlacement({
            xInt: number(entity.xInt ?? entity.x),
            yInt: number(entity.yInt ?? entity.y),
            zInt: number(entity.displayZ ?? entity.zInt ?? entity.z),
            renderOffsetX: number(entity.renderOffsetX),
            cameraX: tick.cameraX,
            centerX: number(frame.centerx),
            centerY: number(frame.centery),
            width,
            facing: number(entity.facing),
        });
        const x1 = frame.state === 9997 ? Math.max(0, Math.min(714, placement.x)) : placement.x;
        return [{ slot: entity.slot, x1, y1: placement.y, x2: x1 + width, y2: placement.y + height }];
    });
}

export function hitTestPreviewActor(
    areas: readonly PreviewActorHitArea[],
    x: number,
    y: number,
): PreviewActorHitArea | undefined {
    for (let index = areas.length - 1; index >= 0; index -= 1) {
        const area = areas[index]!;
        if (x >= area.x1 && x <= area.x2 && y >= area.y1 && y <= area.y2) return area;
    }
    return undefined;
}

function drawPositionHandles(
    context: CanvasRenderingContext2D,
    areas: readonly PreviewActorHitArea[],
    selectedSlot: 0 | 1 | undefined,
): void {
    context.save();
    context.font = "bold 11px sans-serif";
    context.textBaseline = "bottom";
    for (const area of areas) {
        const color = area.slot === 0 ? "#32d6d9" : "#ffca3a";
        context.strokeStyle = selectedSlot === area.slot ? "#ffffff" : color;
        context.fillStyle = color;
        context.lineWidth = selectedSlot === area.slot ? 2 : 1;
        context.setLineDash(selectedSlot === area.slot ? [5, 3] : []);
        context.strokeRect(area.x1 - 2, area.y1 - 2, area.x2 - area.x1 + 4, area.y2 - area.y1 + 4);
        context.fillText(area.slot === 0 ? "P1" : "P2", area.x1, area.y1 - 4);
    }
    context.restore();
}

export function stageParallaxOffset(
    stageWidth: number,
    viewportWidth: number,
    parallaxWidth: number,
    cameraX: number,
): number {
    if (stageWidth <= viewportWidth) return 0;
    const offset = -((parallaxWidth - viewportWidth) * cameraX) / (stageWidth - viewportWidth);
    return offset === 0 ? 0 : offset;
}

function drawEntity(
    context: CanvasRenderingContext2D,
    input: PreviewRenderInput,
    entity: PreviewEntity,
    primary: PreviewEntity | undefined,
    resource: PreviewObject | undefined,
): readonly OverlayGeometry[] {
    const hitStop = number(entity.hitStop);
    if (hitStop <= -25 || Math.abs(hitStop) % 4 >= 2) return [];
    const isPrimary = entity === primary;
    const loaded = resource !== undefined || isPrimary;
    const frames = resource?.frames ?? (isPrimary ? input.project.frames : []);
    const ranges = resource?.ranges ?? (isPrimary ? input.project.ranges : []);
    const frame = isPrimary ? input.runtimeFrame ?? lastFrameForId(frames, entity.frame) : lastFrameForId(frames, entity.frame);
    const pic = effectivePreviewPic(entity, frame);
    const range = loaded ? ranges.find((candidate) => pic >= number(candidate.frameLo ?? candidate.frame_lo) && pic <= number(candidate.frameHi ?? candidate.frame_hi, -1)) : undefined;
    const width = number(range?.w), height = number(range?.h), columns = spriteSheetColumnCount(range);
    const rows = number(range?.col);
    const assetId = range === undefined ? undefined : (text(range.assetId) || input.project.assets.get(text(range.file)) || input.project.assets.get(""));
    if (!range || !assetId || pic === 999 || columns <= 0 || rows <= 0 || width <= 0 || height <= 0) {
        return [];
    }

    const renderPhase = Math.trunc(number(input.tick!.tick)) & 1;
    const extraX = number(entity.frameDelay) < 0 ? 6 * renderPhase - 3 : 0;
    const placement = spritePlacement({
        xInt: number(entity.xInt ?? entity.x) + extraX,
        yInt: number(entity.yInt ?? entity.y),
        zInt: number(entity.displayZ ?? entity.zInt ?? entity.z),
        renderOffsetX: number(entity.renderOffsetX),
        cameraX: input.tick!.cameraX,
        centerX: number(frame?.centerx),
        centerY: number(frame?.centery),
        width,
        facing: number(entity.facing),
    });
    const drawX = frame?.state === 9997 ? Math.max(0, Math.min(714, placement.x)) : placement.x;

    const local = pic - number(range.frameLo ?? range.frame_lo), image = loadImage(input, assetId);
    const colorKeyImage = loadColorKeyImage(input, assetId, image);
    if (colorKeyImage === undefined) {
        return [];
    }
    context.save();
    if (placement.mirror) {
        context.translate(drawX + width, placement.y); context.scale(-1, 1);
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, 0, 0, width, height);
    } else {
        context.drawImage(colorKeyImage, (local % columns) * (width + 1), Math.floor(local / columns) * (height + 1), width, height, drawX, placement.y, width, height);
    }
    context.restore();
    if (entity !== primary || !frame) return [];
    const authorityTick = input.authorityTick ?? input.tick!;
    const authorityEntity = primaryPreviewEntity(authorityTick.entities) ?? entity;
    const authorityPlacement = spritePlacement({
        xInt: number(authorityEntity.xInt ?? authorityEntity.x),
        yInt: number(authorityEntity.yInt ?? authorityEntity.y),
        zInt: number(authorityEntity.displayZ ?? authorityEntity.zInt ?? authorityEntity.z),
        renderOffsetX: number(authorityEntity.renderOffsetX),
        cameraX: authorityTick.cameraX,
        centerX: number(frame.centerx),
        centerY: number(frame.centery),
        width,
        facing: number(authorityEntity.facing),
    });
    const authorityDrawX = frame.state === 9997
        ? Math.max(0, Math.min(714, authorityPlacement.x))
        : authorityPlacement.x;
    return buildOverlayGeometry(frame, {
        left: authorityDrawX,
        top: authorityPlacement.y,
        width,
        height,
        mirror: authorityPlacement.mirror,
    })
        .filter((item) => input.visibleOverlays.has(item.type));
}

export function drawPreviewCanvas(input: PreviewRenderInput): readonly OverlayGeometry[] {
    const context = input.canvas.getContext("2d");
    if (context === null) return [];
    context.clearRect(0, 0, input.canvas.width, input.canvas.height);
    if (input.tick === undefined) {
        context.fillStyle = "#8d98a6";
        context.fillText("尚未收到原生预览数据。", 20, 30);
        return [];
    }
    drawSceneBackground(context, input, input.tick);
    const primary = primaryPreviewEntity(input.tick.entities);
    const authorityTick = input.authorityTick ?? input.tick;
    const axisEntity = primaryPreviewEntity(authorityTick.entities) ?? authorityTick.entities[0];
    if (input.showAxes && axisEntity !== undefined) drawAxes(context, input.canvas, authorityTick, axisEntity);
    let geometry: readonly OverlayGeometry[] = [];
    const entities = sortPreviewEntities(input.tick.entities);
    const resourcesByOid = new Map((input.project.previewObjects ?? []).map((resource) => [resource.oid, resource]));
    for (const entity of entities) {
        const resource = resourcesByOid.get(entity.oid);
        drawShadow(context, input, entity, primary, resource);
        const candidate = drawEntity(context, input, entity, primary, resource);
        if (candidate.length > 0) geometry = candidate;
    }
    drawGeometry(context, geometry);
    if (input.draftGeometry !== undefined) drawDraftGeometry(context, input.draftGeometry);
    if (input.positionMode) {
        drawPositionHandles(context, previewActorHitAreas(input.project, authorityTick, input.runtimeFrame), input.selectedPositionSlot);
    }
    return geometry;
}
