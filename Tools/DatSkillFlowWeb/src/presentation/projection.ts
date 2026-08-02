import type { SimulationState } from "../sim/types.js";

export interface PresentationProjectionOptions {
    readonly cameraX: number;
    readonly renderOffsetBySlot?: Readonly<Record<number, number>>;
}

export interface ProjectedPresentationEntity {
    readonly stableId: string;
    readonly slot: number;
    readonly zInt: number;
    readonly renderOffsetX: number;
    readonly screenX: number;
    readonly screenY: number;
}

function safeInteger(value: number, label: string): number {
    if (!Number.isSafeInteger(value)) throw new TypeError(`${label} must be a safe integer`);
    return value;
}

export function projectPresentationEntities(
    world: SimulationState,
    options: PresentationProjectionOptions,
): readonly ProjectedPresentationEntity[] {
    const cameraX = safeInteger(options.cameraX, "projection.cameraX");
    const ordered = world.entities.filter((entity) => entity.active).slice().sort((left, right) => (
        left.zInt < right.zInt ? -1 : left.zInt > right.zInt ? 1 : left.slot - right.slot
    ));
    return Object.freeze(ordered.map((entity) => {
        const renderOffsetX = safeInteger(options.renderOffsetBySlot?.[entity.slot] ?? 0, `projection.renderOffsetBySlot.${entity.slot}`);
        return Object.freeze({
            stableId: entity.stableId,
            slot: entity.slot,
            zInt: entity.zInt,
            renderOffsetX,
            screenX: safeInteger(entity.xInt + renderOffsetX - cameraX, `projection.screenX.${entity.slot}`),
            screenY: safeInteger(entity.zInt + entity.yInt, `projection.screenY.${entity.slot}`),
        });
    }));
}
