import type { DatFrameProjection } from "../model/dat-projection.js";
import type { SkillFlowGraph, SkillFlowFrameNode } from "./skill-flow.js";

export interface SkillTimelineSegment {
    readonly node: SkillFlowFrameNode;
    readonly wait: number;
    readonly startUnit: number;
    readonly endUnit: number;
}

export interface SkillTimeline {
    readonly segments: readonly SkillTimelineSegment[];
    readonly totalUnits: number;
}

export function datWaitVisualUnits(wait: number): number {
    return Number.isSafeInteger(wait) ? Math.max(1, wait) : 1;
}

export function buildSkillTimeline(
    graph: SkillFlowGraph,
    frames: readonly DatFrameProjection[],
): SkillTimeline {
    const segments: SkillTimelineSegment[] = [];
    const frameByOccurrence = new Map(frames.map((frame) => [frame.occurrence, frame]));
    let elapsed = 0;
    for (const node of graph.nodes) {
        if (node.kind !== "frame") continue;
        const frame = frameByOccurrence.get(node.occurrence);
        const wait = datWaitVisualUnits(frame?.wait ?? 1);
        const startUnit = elapsed;
        elapsed += wait;
        segments.push(Object.freeze({ node, wait, startUnit, endUnit: elapsed }));
    }
    return Object.freeze({
        segments: Object.freeze(segments),
        totalUnits: elapsed,
    });
}
