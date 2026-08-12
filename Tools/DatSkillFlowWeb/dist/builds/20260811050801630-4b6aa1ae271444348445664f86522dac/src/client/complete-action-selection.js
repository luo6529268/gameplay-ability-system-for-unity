// dat-skill-flow-build:20260811050801630-4b6aa1ae271444348445664f86522dac
                                                     
                                                                     

export function nextDistanceToFrame(
    graph                            ,
    occurrence        ,
)         {
    if (!graph) return -1;
    const nodes = new Map(graph.nodes.map((node) => [node.id, node]));
    const outgoing = new Map                         ();
    for (const edge of graph.edges) {
        if (edge.key !== "next" || edge.resolution !== "frame") continue;
        const values = outgoing.get(edge.from);
        if (values) values.push(edge);
        else outgoing.set(edge.from, [edge]);
    }
    const pending                                     = [{ id: graph.startNodeId, distance: 0 }];
    const visited = new Set        ();
    for (let index = 0; index < pending.length; index += 1) {
        const current = pending[index] ;
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
    skills                       ,
    oid        ,
    occurrence        ,
    graphFor                                                                  ,
    preferredIndex = -1,
)         {
    const preferred = skills[preferredIndex];
    if (preferred?.oid === oid && nextDistanceToFrame(graphFor(preferred, preferredIndex), occurrence) >= 0) {
        return preferredIndex;
    }
    return buildCompleteActionIndex(skills, oid, graphFor).get(occurrence) ?? -1;
}

export function buildCompleteActionIndex(
    skills                       ,
    oid        ,
    graphFor                                                                  ,
)                              {
    const candidates = new Map                                                                 ();
    const fallback = new Map                ();
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
    return new Map([...fallback, ...[...candidates].map(([occurrence, candidate]) => [occurrence, candidate.index]         )]);
}

export function buildInternalStageChain(
    parent            ,
    target            ,
    entries                       ,
    sourceBelongsTo                                                     ,
)                                    {
    if (parent.actionRole !== "root"
        || target.actionRole !== "internal"
        || !target.parentStartFrames.includes(parent.startFrame)) return undefined;
    const internalEntries = entries.filter((entry) => (
        entry.actionRole === "internal"
        && entry.parentStartFrames.includes(parent.startFrame)
    ));
    const visiting = new Set        ();
    const visit = (stage            )                                    => {
        if (visiting.has(stage.startFrame)) return undefined;
        visiting.add(stage.startFrame);
        try {
            for (const route of stage.routes) {
                if (sourceBelongsTo(parent, route.sourceFrame)) return Object.freeze([stage]);
                for (const prerequisite of internalEntries) {
                    if (prerequisite.startFrame === stage.startFrame
                        || !sourceBelongsTo(prerequisite, route.sourceFrame)) continue;
                    const chain = visit(prerequisite);
                    if (chain !== undefined) return Object.freeze([...chain, stage]);
                }
            }
            return undefined;
        }
        finally {
            visiting.delete(stage.startFrame);
        }
    };
    return visit(target);
}
