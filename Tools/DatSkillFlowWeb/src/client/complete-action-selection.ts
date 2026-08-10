import type { SkillEntry } from "./skill-entries.js";
import type { SkillFlowEdge, SkillFlowGraph } from "./skill-flow.js";

export function nextDistanceToFrame(
    graph: SkillFlowGraph | undefined,
    occurrence: number,
): number {
    if (!graph) return -1;
    const nodes = new Map(graph.nodes.map((node) => [node.id, node]));
    const outgoing = new Map<string, SkillFlowEdge[]>();
    for (const edge of graph.edges) {
        if (edge.key !== "next" || edge.resolution !== "frame") continue;
        const values = outgoing.get(edge.from);
        if (values) values.push(edge);
        else outgoing.set(edge.from, [edge]);
    }
    const pending: { id: string; distance: number }[] = [{ id: graph.startNodeId, distance: 0 }];
    const visited = new Set<string>();
    for (let index = 0; index < pending.length; index += 1) {
        const current = pending[index]!;
        if (visited.has(current.id)) continue;
        visited.add(current.id);
        const node = nodes.get(current.id);
        if (node?.kind === "frame" && node.occurrence === occurrence) return current.distance;
        for (const edge of outgoing.get(current.id) ?? []) {
            pending.push({ id: edge.to, distance: current.distance + 1 });
        }
    }
    return -1;
}

export function selectCompleteActionIndex(
    skills: readonly SkillEntry[],
    oid: number,
    occurrence: number,
    graphFor: (skill: SkillEntry, index: number) => SkillFlowGraph | undefined,
    preferredIndex = -1,
): number {
    const preferred = skills[preferredIndex];
    if (preferred?.oid === oid && nextDistanceToFrame(graphFor(preferred, preferredIndex), occurrence) >= 0) {
        return preferredIndex;
    }
    return buildCompleteActionIndex(skills, oid, graphFor).get(occurrence) ?? -1;
}

export function buildCompleteActionIndex(
    skills: readonly SkillEntry[],
    oid: number,
    graphFor: (skill: SkillEntry, index: number) => SkillFlowGraph | undefined,
): ReadonlyMap<number, number> {
    const candidates = new Map<number, { index: number; distance: number; startFrame: number }>();
    const fallback = new Map<number, number>();
    for (const [index, skill] of skills.entries()) {
        if (skill.oid !== oid) continue;
        const graph = graphFor(skill, index);
        if (!graph) continue;
        for (const node of graph.nodes) {
            if (node.kind === "frame" && !fallback.has(node.occurrence)) fallback.set(node.occurrence, index);
        }
        for (const node of graph.nodes) {
            if (node.kind !== "frame") continue;
            const distance = nextDistanceToFrame(graph, node.occurrence);
            if (distance < 0) continue;
            const current = candidates.get(node.occurrence);
            if (current === undefined
                || distance > current.distance
                || (distance === current.distance && skill.startFrame < current.startFrame)) {
                candidates.set(node.occurrence, { index, distance, startFrame: skill.startFrame });
            }
        }
    }
    return new Map([...fallback, ...[...candidates].map(([occurrence, candidate]) => [occurrence, candidate.index] as const)]);
}
