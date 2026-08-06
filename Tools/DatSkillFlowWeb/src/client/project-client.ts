export interface SpritePlacementInput {
    readonly xInt: number;
    readonly yInt: number;
    readonly zInt: number;
    readonly renderOffsetX: number;
    readonly cameraX: number;
    readonly centerX: number;
    readonly centerY: number;
    readonly width: number;
    readonly facing: number;
}

export interface SpritePlacement {
    readonly x: number;
    readonly y: number;
    readonly mirror: boolean;
}

export interface FrameLocator {
    readonly frameId: number;
    readonly occurrence: number;
}

export interface FrameFieldCapabilityLocator {
    readonly key: string;
    readonly scope: string;
    readonly occurrence: number;
    readonly frameId?: number;
    readonly frameOccurrence?: number;
}

export const NATIVE_PREVIEW_PRIMARY_SLOT = 0;

export interface NativeSlotEntity {
    readonly slot: number;
}

export function primaryPreviewEntity<T extends NativeSlotEntity>(
    entities: readonly T[],
): T | undefined {
    return entities.find((entity) => entity.slot === NATIVE_PREVIEW_PRIMARY_SLOT);
}

export function lastFrameForId<T extends FrameLocator>(
    frames: readonly T[],
    frameId: number | undefined,
): T | undefined {
    if (frameId === undefined) return undefined;
    for (let index = frames.length - 1; index >= 0; index -= 1) {
        if (frames[index]?.frameId === frameId) return frames[index];
    }
    return undefined;
}

export function findFrameFieldCapability<T extends FrameFieldCapabilityLocator>(
    fields: readonly T[],
    frame: FrameLocator,
    key: string,
): T | undefined {
    for (let index = fields.length - 1; index >= 0; index -= 1) {
        const field = fields[index]!;
        if (field.scope === "frame"
            && field.frameId === frame.frameId
            && field.frameOccurrence === frame.occurrence
            && field.key === key) {
            return field;
        }
    }
    return undefined;
}

export function spritePlacement(input: SpritePlacementInput): SpritePlacement {
    const sx = input.xInt + input.renderOffsetX - input.cameraX;
    const sy = input.zInt + input.yInt;
    const mirror = input.facing === 1;
    return Object.freeze({
        x: mirror ? sx - (input.width - input.centerX) : sx - input.centerX,
        y: sy - input.centerY,
        mirror,
    });
}

export function mergePreview<T extends object>(project: T, revision: string | number, nativeTicks: readonly unknown[]): T & {
    readonly revision: string | number;
    readonly nativeTicks: readonly unknown[];
} {
    return Object.freeze({ ...project, revision, nativeTicks: Object.freeze([...nativeTicks]) });
}
