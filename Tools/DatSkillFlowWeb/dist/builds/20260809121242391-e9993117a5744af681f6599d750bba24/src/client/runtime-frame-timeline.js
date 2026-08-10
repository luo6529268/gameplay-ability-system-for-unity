// dat-skill-flow-build:20260809121242391-e9993117a5744af681f6599d750bba24
                                     
                          
                           
                              
 

                                   
                           
                                                     
 

                                      
                             
                               
                             
                               
 

                                       
                                                      
 

                                      
                    
                      
                    
                      
 

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
