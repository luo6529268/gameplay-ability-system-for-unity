import type { SkillFlowGraph, SkillFlowNode } from "./skill-flow.js";

export interface FlowNodePosition {
    readonly node: SkillFlowNode;
    readonly x: number;
    readonly y: number;
    readonly width: number;
    readonly height: number;
}

export interface FlowLayout {
    readonly width: number;
    readonly height: number;
    readonly nodes: readonly FlowNodePosition[];
}

export function layoutSkillFlow(graph: SkillFlowGraph): FlowLayout {
    const columns = 3;
    const horizontalGap = 28;
    const verticalGap = 22;
    const nodeWidth = 112;
    const nodeHeight = 42;
    const width = 18 + columns * nodeWidth + (columns - 1) * horizontalGap + 18;
    const height = 76;
    const nodes = graph.nodes.map((node, index) => ({
        node,
        x: 18 + (index % columns) * (nodeWidth + horizontalGap),
        y: 16 + Math.floor(index / columns) * (nodeHeight + verticalGap),
        width: nodeWidth,
        height: nodeHeight,
    }));
    return Object.freeze({
        width,
        height: Math.max(height, 16 + Math.ceil(nodes.length / columns) * (nodeHeight + verticalGap)),
        nodes: Object.freeze(nodes),
    });
}
