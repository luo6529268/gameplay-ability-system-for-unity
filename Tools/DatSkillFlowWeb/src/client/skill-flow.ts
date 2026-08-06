import type { DatFrameProjection } from "../model/dat-projection.js";

export const SKILL_FLOW_EDGE_KEYS = Object.freeze([
    "next", "hit_a", "hit_d", "hit_j", "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj", "hit_ja",
] as const);

export type SkillFlowEdgeKey = typeof SKILL_FLOW_EDGE_KEYS[number];
export type SkillFlowUnresolvedReason = "zero" | "negative" | "out-of-range" | "missing";

export interface SkillFlowFrameNode {
    readonly id: string;
    readonly kind: "frame";
    readonly frameId: number;
    readonly occurrence: number;
}

export interface SkillFlowUnresolvedNode {
    readonly id: string;
    readonly kind: "unresolved";
    readonly target: number;
    readonly reason: SkillFlowUnresolvedReason;
}

export type SkillFlowNode = SkillFlowFrameNode | SkillFlowUnresolvedNode;

export interface SkillFlowEdge {
    readonly id: string;
    readonly from: string;
    readonly key: SkillFlowEdgeKey;
    readonly rawTarget: number;
    readonly resolution: "frame" | SkillFlowUnresolvedReason;
    readonly to: string;
}

export interface SkillFlowCycle {
    readonly edgeId: string;
    readonly from: string;
    readonly to: string;
}

export interface SkillFlowGraph {
    readonly startFrame: number;
    readonly startNodeId: string;
    readonly nodes: readonly SkillFlowNode[];
    readonly edges: readonly SkillFlowEdge[];
    readonly cycles: readonly SkillFlowCycle[];
}

function frameNodeId(frameId: number, occurrence: number): string {
    return `frame:${frameId}:${occurrence}`;
}

function unresolvedNodeId(target: number, reason: SkillFlowUnresolvedReason): string {
    return `unresolved:${reason}:${target}`;
}

function targetReason(target: number, frameById: ReadonlyMap<number, DatFrameProjection>): "frame" | SkillFlowUnresolvedReason {
    if (target === 0) return "zero";
    if (target < 0) return "negative";
    if (target >= 600) return "out-of-range";
    return frameById.has(target) ? "frame" : "missing";
}

function latestFrames(frames: readonly DatFrameProjection[]): Map<number, DatFrameProjection> {
    const result = new Map<number, DatFrameProjection>();
    for (const frame of frames) {
        if (Number.isSafeInteger(frame.frameId) && frame.frameId >= 0 && frame.frameId < 600) {
            result.set(frame.frameId, frame);
        }
    }
    return result;
}

function frameNode(frame: DatFrameProjection): SkillFlowFrameNode {
    return Object.freeze({
        id: frameNodeId(frame.frameId, frame.occurrence),
        kind: "frame" as const,
        frameId: frame.frameId,
        occurrence: frame.occurrence,
    });
}

function unresolvedNode(target: number, reason: SkillFlowUnresolvedReason): SkillFlowUnresolvedNode {
    return Object.freeze({
        id: unresolvedNodeId(target, reason),
        kind: "unresolved" as const,
        target,
        reason,
    });
}

export function buildSkillFlow(
    frames: readonly DatFrameProjection[],
    startFrame: number,
    hasField: (frame: DatFrameProjection, key: SkillFlowEdgeKey) => boolean = () => true,
): SkillFlowGraph {
    const frameById = latestFrames(frames);
    const start = frameById.get(startFrame);
    if (start === undefined) {
        const reason = startFrame < 0 ? "negative" : startFrame >= 600 ? "out-of-range" : "missing";
        const node = unresolvedNode(startFrame, reason);
        return Object.freeze({
            startFrame,
            startNodeId: node.id,
            nodes: Object.freeze([node]),
            edges: Object.freeze([]),
            cycles: Object.freeze([]),
        });
    }

    const nodes = new Map<string, SkillFlowNode>();
    const edges: SkillFlowEdge[] = [];
    const pending: DatFrameProjection[] = [start];
    const scheduled = new Set([frameNodeId(start.frameId, start.occurrence)]);
    const expanded = new Set<string>();
    let pendingIndex = 0;
    nodes.set(frameNodeId(start.frameId, start.occurrence), frameNode(start));

    while (pendingIndex < pending.length) {
        const frame = pending[pendingIndex++]!;
        const from = frameNodeId(frame.frameId, frame.occurrence);
        expanded.add(from);
        for (const key of SKILL_FLOW_EDGE_KEYS) {
            if (!hasField(frame, key)) continue;
            const rawTarget = frame[key];
            const resolution = targetReason(rawTarget, frameById);
            const edgeId = `${from}:${key}`;
            if (resolution === "frame") {
                const target = frameById.get(rawTarget)!;
                const targetNode = frameNode(target);
                nodes.set(targetNode.id, targetNode);
                if (!scheduled.has(targetNode.id)) {
                    scheduled.add(targetNode.id);
                    pending.push(target);
                }
                edges.push(Object.freeze({
                    id: edgeId,
                    from,
                    key,
                    rawTarget,
                    resolution,
                    to: targetNode.id,
                }));
                continue;
            }
            const targetNode = unresolvedNode(rawTarget, resolution);
            nodes.set(targetNode.id, targetNode);
            edges.push(Object.freeze({
                id: edgeId,
                from,
                key,
                rawTarget,
                resolution,
                to: targetNode.id,
            }));
        }
    }

    const edgeByFrom = new Map<string, readonly SkillFlowEdge[]>();
    for (const edge of edges) {
        const current = edgeByFrom.get(edge.from) ?? [];
        edgeByFrom.set(edge.from, [...current, edge]);
    }
    const active = new Set<string>();
    const visited = new Set<string>();
    const cycles: SkillFlowCycle[] = [];
    const visit = (nodeId: string): void => {
        if (active.has(nodeId)) return;
        if (visited.has(nodeId)) return;
        active.add(nodeId);
        for (const edge of edgeByFrom.get(nodeId) ?? []) {
            if (nodes.get(edge.to)?.kind !== "frame") continue;
            if (active.has(edge.to)) cycles.push(Object.freeze({ edgeId: edge.id, from: edge.from, to: edge.to }));
            else visit(edge.to);
        }
        active.delete(nodeId);
        visited.add(nodeId);
    };
    visit(frameNodeId(start.frameId, start.occurrence));

    return Object.freeze({
        startFrame,
        startNodeId: frameNodeId(start.frameId, start.occurrence),
        nodes: Object.freeze([...nodes.values()]),
        edges: Object.freeze(edges),
        cycles: Object.freeze(cycles),
    });
}
