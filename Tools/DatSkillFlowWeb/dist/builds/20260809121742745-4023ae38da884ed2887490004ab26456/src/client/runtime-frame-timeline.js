// dat-skill-flow-build:20260809121742745-4023ae38da884ed2887490004ab26456
                                     
                          
                           
                              
 

                                   
                           
                                                     
 

                                      
                             
                               
                             
                               
 

                                       
                                                      
 

                                      
                    
                      
                    
                      
 

export function buildRuntimeFrameTimeline(
    ticks                             ,
    rootSlot = 0,
)                       {
    const segments                               = [];
    let currentSegment                                        ;

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
