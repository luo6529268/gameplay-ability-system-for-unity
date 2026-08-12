// dat-skill-flow-build:20260811064417029-9d9cb31234734a139b8e6157ac0f4d15
                                                                     
                                                                          

                                       
                                      
                          
                               
                             
 

                                
                                                       
                                
 

export function datWaitVisualUnits(wait        )         {
    return Number.isSafeInteger(wait) ? Math.max(1, wait) : 1;
}

export function buildSkillTimeline(
    graph                ,
    frames                               ,
)                {
    const segments                         = [];
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
