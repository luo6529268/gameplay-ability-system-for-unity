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

export interface PreviewIntentIdentity {
    readonly sessionId: string;
    readonly revision: string | number;
    readonly startFrame: number;
    readonly initialFrame?: number;
    readonly inputPlan?: readonly {
        readonly tick: number;
        readonly keys: readonly string[];
    }[];
    readonly initial?: PreviewInitialPositions;
    readonly ticks: number;
}

export interface PreviewPosition {
    readonly x: number;
    readonly y: number;
    readonly z: number;
}

export interface PreviewInitialPositions {
    readonly p1: PreviewPosition;
    readonly p2: PreviewPosition;
}

export interface PreviewPositionBounds {
    readonly width: number;
    readonly zMin: number;
    readonly zMax: number;
}

export interface NativePreviewPlaybackBounds {
    readonly actionStart: number;
    readonly progressEnd: number;
    readonly playbackEnd: number;
}

interface NativePreviewPlaybackTrace {
    readonly rootSkillStartedTick?: unknown;
    readonly progressEndTick?: unknown;
    readonly playbackEndTick?: unknown;
}

export class BoundedLruCache<K, V> {
    readonly #maximumEntries: number;
    readonly #entries = new Map<K, V>();

    constructor(maximumEntries: number) {
        if (!Number.isSafeInteger(maximumEntries) || maximumEntries < 1) {
            throw new RangeError("maximumEntries must be a positive safe integer.");
        }
        this.#maximumEntries = maximumEntries;
    }

    get size(): number {
        return this.#entries.size;
    }

    get(key: K): V | undefined {
        const value = this.#entries.get(key);
        if (value === undefined) return undefined;
        this.#entries.delete(key);
        this.#entries.set(key, value);
        return value;
    }

    set(key: K, value: V): void {
        this.#entries.delete(key);
        this.#entries.set(key, value);
        while (this.#entries.size > this.#maximumEntries) {
            const oldest = this.#entries.keys().next().value as K | undefined;
            if (oldest === undefined) break;
            this.#entries.delete(oldest);
        }
    }

    clear(): void {
        this.#entries.clear();
    }
}

export function previewIntentCacheKey(intent: PreviewIntentIdentity): string {
    return JSON.stringify({
        sessionId: intent.sessionId,
        revision: intent.revision,
        startFrame: intent.startFrame,
        initialFrame: intent.initialFrame ?? intent.startFrame,
        ticks: intent.ticks,
        initial: intent.initial,
        inputPlan: (intent.inputPlan ?? []).map((step) => ({ tick: step.tick, keys: [...step.keys] })),
    });
}

export function movePreviewPosition(
    position: PreviewPosition,
    deltaCanvasX: number,
    deltaCanvasY: number,
    bounds: PreviewPositionBounds,
): PreviewPosition {
    const clamp = (value: number, minimum: number, maximum: number): number => (
        Math.min(Math.max(value, minimum), maximum)
    );
    return Object.freeze({
        x: clamp(Math.round(position.x + deltaCanvasX), 0, Math.max(0, bounds.width)),
        y: position.y,
        z: clamp(Math.round(position.z + deltaCanvasY), Math.min(bounds.zMin, bounds.zMax), Math.max(bounds.zMin, bounds.zMax)),
    });
}

export function nativePreviewPlaybackBounds(
    trace: NativePreviewPlaybackTrace | undefined,
    tickCount: number,
): NativePreviewPlaybackBounds {
    const last = Math.max(0, Math.trunc(Number.isFinite(tickCount) ? tickCount : 0) - 1);
    const clamp = (value: number): number => Math.min(last, Math.max(0, Math.trunc(value)));
    const tick = (value: unknown, fallback: number): number => (
        typeof value === "number" && Number.isFinite(value) ? value : fallback
    );
    const startValue = tick(trace?.rootSkillStartedTick, -1);
    const actionStart = startValue < 0 ? -1 : clamp(startValue);
    const rawPlaybackEnd = clamp(tick(trace?.playbackEndTick, last));
    const progressValue = tick(trace?.progressEndTick, -1);
    return Object.freeze({
        actionStart,
        progressEnd: actionStart < 0 || progressValue < actionStart ? -1 : clamp(progressValue),
        playbackEnd: actionStart < 0 || rawPlaybackEnd < actionStart ? last : rawPlaybackEnd,
    });
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

export function mergePreview<T extends object>(
    project: T,
    revision: string | number,
    nativeTicks: readonly unknown[],
    nativeTrace?: unknown,
    previewObjects?: readonly unknown[],
): T & {
    readonly revision: string | number;
    readonly nativeTicks: readonly unknown[];
    readonly nativeTrace?: unknown;
    readonly previewObjects?: readonly unknown[];
} {
    return Object.freeze({
        ...project,
        revision,
        nativeTicks: Object.freeze([...nativeTicks]),
        ...(nativeTrace === undefined ? {} : { nativeTrace }),
        ...(previewObjects === undefined ? {} : { previewObjects: Object.freeze([...previewObjects]) }),
    });
}
