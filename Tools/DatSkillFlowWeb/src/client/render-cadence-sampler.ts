import { number, text, type Json } from "./editor-support.js";
import type { PreviewEntity, PreviewTick } from "./preview-renderer.js";

export const RENDER_CADENCE_RATES = Object.freeze([30, 60, 120] as const);
export type RenderCadenceRate = typeof RENDER_CADENCE_RATES[number];
export const NATIVE_LOGIC_TICK_MS = 33;

export interface RenderCadenceSample {
    readonly rate: RenderCadenceRate;
    readonly displayTimeMs: number;
    readonly sourceTickIndex: number;
    readonly previousTickIndex: number;
    readonly interpolationAlpha: number;
    readonly presentationTick: PreviewTick | undefined;
}

function isRenderCadenceRate(value: number): value is RenderCadenceRate {
    return RENDER_CADENCE_RATES.includes(value as RenderCadenceRate);
}

function clamp(value: number, minimum: number, maximum: number): number {
    return Math.min(Math.max(value, minimum), maximum);
}

function finite(value: unknown, fallback = 0): number {
    return number(value, fallback);
}

function lineageKey(entity: PreviewEntity): string {
    const explicit = text(entity.lineageId ?? entity.lineage_id ?? entity.stableId ?? entity.stable_id);
    if (explicit !== "") return `lineage:${explicit}`;
    return `slot:${entity.slot}:oid:${entity.oid}`;
}

function displayPosition(entity: PreviewEntity, axis: "x" | "y" | "z"): number {
    const integerKey = axis === "x" ? "xInt" : axis === "y" ? "yInt" : "zInt";
    return finite(entity[integerKey] ?? entity[axis]);
}

function displayZ(entity: PreviewEntity): number {
    return finite(entity.displayZ ?? entity.zInt ?? entity.z);
}

function interpolate(previous: number, current: number, alpha: number): number {
    return previous + (current - previous) * alpha;
}

function interpolateEntity(
    previous: PreviewEntity | undefined,
    current: PreviewEntity,
    alpha: number,
): PreviewEntity {
    if (previous === undefined || lineageKey(previous) !== lineageKey(current)) {
        return current;
    }

    const x = interpolate(displayPosition(previous, "x"), displayPosition(current, "x"), alpha);
    const y = interpolate(displayPosition(previous, "y"), displayPosition(current, "y"), alpha);
    const z = interpolate(displayPosition(previous, "z"), displayPosition(current, "z"), alpha);
    const zDisplay = interpolate(displayZ(previous), displayZ(current), alpha);
    const renderOffsetX = interpolate(
        finite(previous.renderOffsetX),
        finite(current.renderOffsetX),
        alpha,
    );

    return Object.freeze({
        ...current,
        x,
        y,
        z,
        xInt: x,
        yInt: y,
        zInt: z,
        displayZ: zDisplay,
        renderOffsetX,
    });
}

function interpolateTick(
    previous: PreviewTick,
    current: PreviewTick,
    alpha: number,
): PreviewTick {
    const previousByLineage = new Map(previous.entities.map((entity) => [lineageKey(entity), entity]));
    const cameraX = interpolate(finite(previous.cameraX), finite(current.cameraX), alpha);
    return Object.freeze({
        ...current,
        cameraX,
        entities: current.entities.map((entity) => interpolateEntity(previousByLineage.get(lineageKey(entity)), entity, alpha)),
    });
}

function sampleDiscrete(
    ticks: readonly PreviewTick[],
    elapsedMs: number,
    rate: RenderCadenceRate,
): RenderCadenceSample {
    const tickIndex = clamp(Math.floor(elapsedMs / NATIVE_LOGIC_TICK_MS), 0, ticks.length - 1);
    return Object.freeze({
        rate,
        displayTimeMs: Math.floor(elapsedMs / (1000 / rate)) * (1000 / rate),
        sourceTickIndex: tickIndex,
        previousTickIndex: tickIndex,
        interpolationAlpha: 1,
        presentationTick: ticks[tickIndex],
    });
}

/**
 * Samples a recorded 30 Hz Native trace without predicting the next logic tick.
 * 60/120 Hz panes intentionally render one logic tick behind and only interpolate
 * presentation coordinates of the same lineage. Frame/state/lifecycle remain current-tick discrete values.
 */
export function sampleRenderCadence(
    ticks: readonly PreviewTick[],
    elapsedMs: number,
    rate: RenderCadenceRate,
): RenderCadenceSample {
    if (!isRenderCadenceRate(rate)) {
        throw new RangeError(`Unsupported render cadence: ${rate}`);
    }
    if (!Number.isFinite(elapsedMs) || elapsedMs < 0) {
        throw new RangeError("elapsedMs must be a finite nonnegative number.");
    }
    if (ticks.length === 0) {
        return Object.freeze({
            rate,
            displayTimeMs: 0,
            sourceTickIndex: 0,
            previousTickIndex: 0,
            interpolationAlpha: 0,
            presentationTick: undefined,
        });
    }
    if (rate === 30 || ticks.length === 1) {
        return sampleDiscrete(ticks, elapsedMs, rate);
    }

    const displayIntervalMs = 1000 / rate;
    const displayTimeMs = Math.floor(elapsedMs / displayIntervalMs) * displayIntervalMs;
    const delayedProgress = Math.max(0, displayTimeMs / NATIVE_LOGIC_TICK_MS - 1);
    const previousTickIndex = clamp(Math.floor(delayedProgress), 0, ticks.length - 1);
    const sourceTickIndex = Math.min(previousTickIndex + 1, ticks.length - 1);
    const interpolationAlpha = sourceTickIndex === previousTickIndex
        ? 1
        : delayedProgress - Math.floor(delayedProgress);
    const previous = ticks[previousTickIndex]!;
    const current = ticks[sourceTickIndex]!;

    return Object.freeze({
        rate,
        displayTimeMs,
        sourceTickIndex,
        previousTickIndex,
        interpolationAlpha,
        presentationTick: interpolateTick(previous, current, interpolationAlpha),
    });
}

export function renderCadenceLoopDurationMs(ticks: readonly PreviewTick[]): number {
    return Math.max(NATIVE_LOGIC_TICK_MS, Math.max(0, ticks.length - 1) * NATIVE_LOGIC_TICK_MS);
}

export function isPresentationOnlyCadenceField(field: keyof Json): boolean {
    return field === "x"
        || field === "y"
        || field === "z"
        || field === "xInt"
        || field === "yInt"
        || field === "zInt"
        || field === "displayZ"
        || field === "renderOffsetX"
        || field === "cameraX";
}
