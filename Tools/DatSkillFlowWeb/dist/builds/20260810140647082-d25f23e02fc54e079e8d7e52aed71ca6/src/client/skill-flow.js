// dat-skill-flow-build:20260810140647082-d25f23e02fc54e079e8d7e52aed71ca6
                                                                     
import {
    latestRuntimeFrameMap,
    SKILL_ENTRY_HIT_KEYS,
                    
} from "./skill-entries.js";

export const SKILL_FLOW_EDGE_KEYS = Object.freeze([
    "next", ...SKILL_ENTRY_HIT_KEYS,
]         );

                                                                   
                                                                                         

                                     
                        
                           
                             
                                
 

                                          
                        
                                
                            
                                               
 

                                     
                        
                           
                             
                             
                           
 

                                                                                              

                                
                        
                          
                                   
                               
                                                                       
                        
 

                                 
                            
                          
                        
 

                                 
                                
                                 
                                             
                                             
                                               
 

export function traceStartFrameForSelection(
    frames                               ,
    requestedFrameId        ,
    requestedOccurrence        ,
    graph                            ,
)         {
    const graphContainsFrame = graph?.nodes.some((node) => (
        node.kind === "frame" && node.occurrence === requestedOccurrence
    )) === true;
    const selectedStartFrame = graphContainsFrame ? graph .startFrame : requestedFrameId;
    return selectedStartFrame;
}

function frameNodeId(frameId        , occurrence        )         {
    return `frame:${frameId}:${occurrence}`;
}

function unresolvedNodeId(target        , reason                           )         {
    return `unresolved:${reason}:${target}`;
}

function entryNodeId(entry            )         {
    return `entry-ref:${entry.id}`;
}

function targetReason(target        , frameById                                         )                                      {
    if (target === 0) return "zero";
    if (target < 0) return "negative";
    if (target >= 600) return "out-of-range";
    return frameById.has(target) ? "frame" : "missing";
}

function frameNode(frame                    )                     {
    return Object.freeze({
        id: frameNodeId(frame.frameId, frame.occurrence),
        kind: "frame"         ,
        frameId: frame.frameId,
        occurrence: frame.occurrence,
    });
}

function unresolvedNode(target        , reason                           )                          {
    return Object.freeze({
        id: unresolvedNodeId(target, reason),
        kind: "unresolved"         ,
        target,
        reason,
    });
}

function entryNode(entry            )                     {
    return Object.freeze({
        id: entryNodeId(entry),
        kind: "entry",
        entryId: entry.id,
        frameId: entry.startFrame,
        label: entry.displayName,
    });
}

export function buildSkillFlow(
    frames                               ,
    startFrame        ,
    hasField                                                                = () => true,
    entryIndex                                  = new Map(),
)                 {
    const frameById = latestRuntimeFrameMap(frames);
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

    const nodes = new Map                       ();
    const edges                  = [];
    const pending                       = [start];
    const scheduled = new Set([frameNodeId(start.frameId, start.occurrence)]);
    const expanded = new Set        ();
    let pendingIndex = 0;
    nodes.set(frameNodeId(start.frameId, start.occurrence), frameNode(start));

    while (pendingIndex < pending.length) {
        const frame = pending[pendingIndex++] ;
        const from = frameNodeId(frame.frameId, frame.occurrence);
        expanded.add(from);
        for (const key of SKILL_FLOW_EDGE_KEYS) {
            if (!hasField(frame, key)) continue;
            const rawTarget = frame[key];
            const resolution = targetReason(rawTarget, frameById);
            const edgeId = `${from}:${key}`;
            const targetEntry = key === "next" || rawTarget === startFrame
                ? undefined
                : entryIndex.get(rawTarget);
            if (resolution === "frame" && targetEntry !== undefined) {
                const targetNode = entryNode(targetEntry);
                nodes.set(targetNode.id, targetNode);
                edges.push(Object.freeze({
                    id: edgeId,
                    from,
                    key,
                    rawTarget,
                    resolution: "entry"         ,
                    to: targetNode.id,
                }));
                continue;
            }
            if (resolution === "frame") {
                const target = frameById.get(rawTarget) ;
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

    const edgeByFrom = new Map                                  ();
    for (const edge of edges) {
        const current = edgeByFrom.get(edge.from) ?? [];
        edgeByFrom.set(edge.from, [...current, edge]);
    }
    const active = new Set        ();
    const visited = new Set        ();
    const cycles                   = [];
    const visit = (nodeId        )       => {
        if (active.has(nodeId)) return;
        if (visited.has(nodeId)) return;
        active.add(nodeId);
        for (const edge of edgeByFrom.get(nodeId) ?? []) {
            if (edge.resolution !== "frame" || nodes.get(edge.to)?.kind !== "frame") continue;
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
