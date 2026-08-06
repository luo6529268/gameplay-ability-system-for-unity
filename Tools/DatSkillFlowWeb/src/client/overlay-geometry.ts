import type {
    BPointProjection,
    BdyProjection,
    CPointProjection,
    DatFrameProjection,
    ItrProjection,
    OPointProjection,
    WPointProjection,
} from "../model/dat-projection.js";

export type OverlayType = "itr" | "bdy" | "opoint" | "wpoint" | "bpoint" | "cpoint";

export interface OverlaySpriteRect {
    readonly left: number;
    readonly top: number;
    readonly width: number;
    readonly height: number;
    readonly mirror: boolean;
}

export interface OverlayPointGeometry {
    readonly type: OverlayType;
    readonly index: number;
    readonly kind: "point";
    readonly x: number;
    readonly y: number;
}

export interface OverlayRectGeometry {
    readonly type: OverlayType;
    readonly index: number;
    readonly kind: "rect";
    readonly x1: number;
    readonly y1: number;
    readonly x2: number;
    readonly y2: number;
    readonly width: number;
    readonly height: number;
}

export type OverlayGeometry = OverlayPointGeometry | OverlayRectGeometry;

export const OVERLAY_COLORS: Readonly<Record<OverlayType, string>> = Object.freeze({
    itr: "#f07832",
    bdy: "#29b6d1",
    opoint: "#e5b84b",
    wpoint: "#b66be8",
    bpoint: "#57c878",
    cpoint: "#e05aa8",
});

function point(sprite: OverlaySpriteRect, x: number, y: number): { x: number; y: number } {
    return {
        x: sprite.mirror ? sprite.left + sprite.width - x : sprite.left + x,
        y: sprite.top + y,
    };
}

function pointOverlay<T extends { x: number; y: number }>(
    type: OverlayType,
    index: number,
    value: T,
    sprite: OverlaySpriteRect,
): OverlayPointGeometry {
    const position = point(sprite, value.x, value.y);
    return Object.freeze({ type, index, kind: "point" as const, ...position });
}

function rectOverlay<T extends { x: number; y: number; w: number; h: number }>(
    type: OverlayType,
    index: number,
    value: T,
    sprite: OverlaySpriteRect,
): OverlayRectGeometry {
    const first = point(sprite, value.x, value.y);
    const second = point(sprite, value.x + value.w, value.y + value.h);
    return Object.freeze({
        type,
        index,
        kind: "rect" as const,
        x1: first.x,
        y1: first.y,
        x2: second.x,
        y2: second.y,
        width: second.x - first.x,
        height: second.y - first.y,
    });
}

function mapPoints<T extends { x: number; y: number }>(
    type: OverlayType,
    values: readonly T[],
    sprite: OverlaySpriteRect,
): readonly OverlayPointGeometry[] {
    return values.map((value, index) => pointOverlay(type, index, value, sprite));
}

function mapRects<T extends { x: number; y: number; w: number; h: number }>(
    type: OverlayType,
    values: readonly T[],
    sprite: OverlaySpriteRect,
): readonly OverlayRectGeometry[] {
    return values.map((value, index) => rectOverlay(type, index, value, sprite));
}

export function buildOverlayGeometry(
    frame: DatFrameProjection,
    sprite: OverlaySpriteRect,
): readonly OverlayGeometry[] {
    return Object.freeze([
        ...mapRects("itr", frame.itrs as readonly ItrProjection[], sprite),
        ...mapRects("bdy", frame.bdys as readonly BdyProjection[], sprite),
        ...mapPoints("opoint", frame.opoints as readonly OPointProjection[], sprite),
        ...mapPoints("wpoint", frame.wpoints as readonly WPointProjection[], sprite),
        ...mapPoints("bpoint", frame.bpoints as readonly BPointProjection[], sprite),
        ...mapPoints("cpoint", frame.cpoints as readonly CPointProjection[], sprite),
    ]);
}

export function hitTestOverlay(
    geometry: readonly OverlayGeometry[],
    x: number,
    y: number,
    pointRadius = 6,
): OverlayGeometry | undefined {
    for (let index = geometry.length - 1; index >= 0; index -= 1) {
        const item = geometry[index]!;
        if (item.kind === "point") {
            if (Math.hypot(item.x - x, item.y - y) <= pointRadius) return item;
            continue;
        }
        const left = Math.min(item.x1, item.x2);
        const right = Math.max(item.x1, item.x2);
        const top = Math.min(item.y1, item.y2);
        const bottom = Math.max(item.y1, item.y2);
        if (x >= left && x <= right && y >= top && y <= bottom) return item;
    }
    return undefined;
}
