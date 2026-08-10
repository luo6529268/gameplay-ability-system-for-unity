export interface RuntimeFrameEntity {
    readonly slot: number;
    readonly frame: number;
    readonly active?: boolean;
}

export interface RuntimeFrameTick {
    readonly tick?: number;
    readonly entities: readonly RuntimeFrameEntity[];
}

export interface RuntimeFrameSegment {
    readonly frameId: number;
    readonly startTick: number;
    readonly endTick: number;
    readonly tickCount: number;
}

export interface RuntimeFrameTimeline {
    readonly segments: readonly RuntimeFrameSegment[];
}

interface MutableRuntimeFrameSegment {
    frameId: number;
    startTick: number;
    endTick: number;
    tickCount: number;
}

export function buildRuntimeFrameTimeline(
    ticks: readonly RuntimeFrameTick[],
    rootSlot = 0,
): RuntimeFrameTimeline {
    const segments: MutableRuntimeFrameSegment[] = [];
    let currentSegment: MutableRuntimeFrameSegment | undefined;

    for (const [index, tick] of ticks.entries()) {
        const root = tick.entities.find((entity) => (
            entity.slot === rootSlot && entity.active !== false
        ));
        if (root === undefined) {
            currentSegment = undefined;
            continue;
        }

        const tickIndex = tick.tick ?? index;
        if (currentSegment?.frameId === root.frame) {
            currentSegment.endTick = tickIndex;
            currentSegment.tickCount += 1;
            continue;
        }

        currentSegment = {
            frameId: root.frame,
            startTick: tickIndex,
            endTick: tickIndex,
            tickCount: 1,
        };
        segments.push(currentSegment);
    }

    const frozenSegments = Object.freeze(
        segments.map((segment) => Object.freeze({ ...segment })),
    );
    return Object.freeze({ segments: frozenSegments });
}
