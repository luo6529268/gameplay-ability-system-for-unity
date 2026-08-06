import type {
    OverlayGeometry,
    OverlayPointGeometry,
    OverlayRectGeometry,
} from "./overlay-geometry.js";

export type ResizeHandle = "nw" | "ne" | "sw" | "se";

export interface DatPoint {
    readonly x: number;
    readonly y: number;
}

export interface DatRect extends DatPoint {
    readonly w: number;
    readonly h: number;
}

export function snapDelta(value: number, gridSize: 1 | 4): number {
    return Math.round(value / gridSize) * gridSize;
}

export function moveDatPoint(
    value: DatPoint,
    screenDx: number,
    screenDy: number,
    mirror: boolean,
): DatPoint {
    return Object.freeze({
        x: value.x + (mirror ? -screenDx : screenDx),
        y: value.y + screenDy,
    });
}

export function resizeDatRect(
    value: DatRect,
    handle: ResizeHandle,
    screenDx: number,
    screenDy: number,
    mirror: boolean,
): DatRect | undefined {
    const localDx = mirror ? -screenDx : screenDx;
    const screenLeft = handle === "nw" || handle === "sw";
    const localStart = mirror ? !screenLeft : screenLeft;
    const top = handle === "nw" || handle === "ne";
    let { x, y, w, h } = value;
    if (localStart) {
        x += localDx;
        w -= localDx;
    } else {
        w += localDx;
    }
    if (top) {
        y += screenDy;
        h -= screenDy;
    } else {
        h += screenDy;
    }
    return w > 0 && h > 0 ? Object.freeze({ x, y, w, h }) : undefined;
}

export function hitResizeHandle(
    geometry: OverlayRectGeometry,
    x: number,
    y: number,
    radius = 7,
): ResizeHandle | undefined {
    const left = Math.min(geometry.x1, geometry.x2);
    const right = Math.max(geometry.x1, geometry.x2);
    const top = Math.min(geometry.y1, geometry.y2);
    const bottom = Math.max(geometry.y1, geometry.y2);
    const handles: readonly [ResizeHandle, number, number][] = [
        ["nw", left, top],
        ["ne", right, top],
        ["sw", left, bottom],
        ["se", right, bottom],
    ];
    return handles.find(([, handleX, handleY]) => (
        Math.hypot(handleX - x, handleY - y) <= radius
    ))?.[0];
}

function moveDraftPoint(
    geometry: OverlayPointGeometry,
    screenDx: number,
    screenDy: number,
): OverlayPointGeometry {
    return Object.freeze({
        ...geometry,
        x: geometry.x + screenDx,
        y: geometry.y + screenDy,
    });
}

function moveDraftRect(
    geometry: OverlayRectGeometry,
    screenDx: number,
    screenDy: number,
): OverlayRectGeometry {
    return Object.freeze({
        ...geometry,
        x1: geometry.x1 + screenDx,
        y1: geometry.y1 + screenDy,
        x2: geometry.x2 + screenDx,
        y2: geometry.y2 + screenDy,
    });
}

export function draftOverlayGeometry(
    geometry: OverlayGeometry,
    screenDx: number,
    screenDy: number,
    handle?: ResizeHandle,
): OverlayGeometry | undefined {
    if (geometry.kind === "point") return moveDraftPoint(geometry, screenDx, screenDy);
    if (handle === undefined) return moveDraftRect(geometry, screenDx, screenDy);
    const left = Math.min(geometry.x1, geometry.x2);
    const right = Math.max(geometry.x1, geometry.x2);
    const top = Math.min(geometry.y1, geometry.y2);
    const bottom = Math.max(geometry.y1, geometry.y2);
    const nextLeft = handle === "nw" || handle === "sw" ? left + screenDx : left;
    const nextRight = handle === "ne" || handle === "se" ? right + screenDx : right;
    const nextTop = handle === "nw" || handle === "ne" ? top + screenDy : top;
    const nextBottom = handle === "sw" || handle === "se" ? bottom + screenDy : bottom;
    if (nextRight - nextLeft < 1 || nextBottom - nextTop < 1) return undefined;
    return Object.freeze({
        ...geometry,
        x1: nextLeft,
        y1: nextTop,
        x2: nextRight,
        y2: nextBottom,
        width: nextRight - nextLeft,
        height: nextBottom - nextTop,
    });
}
