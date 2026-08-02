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
