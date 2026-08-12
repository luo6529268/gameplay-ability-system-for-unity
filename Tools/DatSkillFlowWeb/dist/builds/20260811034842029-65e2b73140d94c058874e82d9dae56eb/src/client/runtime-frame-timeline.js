// dat-skill-flow-build:20260811034842029-65e2b73140d94c058874e82d9dae56eb
                                     
                          
                           
                              
 

                                   
                           
                                                     
 

                                      
                             
                               
                             
                               
 

                                       
                                                      
 

                                      
                    
                      
                    
                      
 

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
