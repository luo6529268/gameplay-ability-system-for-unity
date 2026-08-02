// dat-skill-flow-build:20260801063743753-69f54ecd60f44fd6a354fedf6173019c
import { GATE2_RULE } from "./rules.js";
             
                         
              
                       
                    

                                  
                               
                                                          
                                        
 

function frameDefinition(entity           , frameId        )                                 {
    return entity.frames.find((definition) => definition.id === frameId);
}

function replaceEntity(entity           , changes                    )            {
    return Object.freeze({ ...entity, ...changes });
}

function transitionEvent(
    entity           ,
    kind                              ,
    fromFrame        ,
    toFrame        ,
    rawNext               ,
)                       {
    return Object.freeze({
        stableId: entity.stableId,
        slot: entity.slot,
        kind,
        fromFrame,
        toFrame,
        rawNext,
    });
}

export function frameTick(entity           )                  {
    let current = entity;
    let definition = frameDefinition(current, current.frame);
    const transitions                         = [];
    const ruleIds           = [];
    if (definition === undefined) {
        return { entity: current, transitions, ruleIds };
    }

    let attacking = current.attacking;
    if (current.frame !== current.waitCounter) {
        attacking = 0;
        ruleIds.push(GATE2_RULE.frameWaitCounterReset);
    }
    attacking++;
    ruleIds.push(GATE2_RULE.frameAttackingStrictWait);
    current = replaceEntity(current, { attacking });

    if (definition.state === 0 && current.yInt < 0) {
        const fromFrame = current.frame;
        current = replaceEntity(current, { frame: 212 });
        transitions.push(transitionEvent(current, "state0-airborne", fromFrame, 212, null));
        ruleIds.push(GATE2_RULE.frameState0Airborne);
        definition = frameDefinition(current, 212);
        if (definition === undefined) {
            return { entity: current, transitions, ruleIds };
        }
    }

    if (current.attacking > definition.wait) {
        const rawNext = definition.next;
        const fromFrame = current.frame;
        current = replaceEntity(current, { attacking: 0 });
        if (rawNext === 0) {
            transitions.push(transitionEvent(current, "hold", fromFrame, fromFrame, rawNext));
            ruleIds.push(GATE2_RULE.frameNextZero);
        } else {
            let targetFrame = rawNext;
            let facing = current.facing;
            let kind                              ;
            ruleIds.push(GATE2_RULE.frameNextTransition);
            if (rawNext === 999) {
                targetFrame = current.yInt !== 0 && current.rawObjectType === 0 ? 212 : 0;
                kind = "sentinel-999";
                ruleIds.push(GATE2_RULE.frameNext999);
            } else {
                if (targetFrame < 0) {
                    facing = facing === 0 ? 1 : 0;
                    targetFrame = -targetFrame;
                    kind = "negative";
                    ruleIds.push(GATE2_RULE.frameNextNegative);
                } else {
                    kind = targetFrame === fromFrame ? "self" : "standard";
                }
            }
            current = replaceEntity(current, { frame: targetFrame, facing });
            transitions.push(transitionEvent(current, kind, fromFrame, targetFrame, rawNext));

            if (targetFrame < 0 || targetFrame >= 400) {
                return { entity: current, transitions, ruleIds };
            }
            definition = frameDefinition(current, targetFrame);
            if (definition === undefined) {
                return { entity: current, transitions, ruleIds };
            }
        }
    }

    current = replaceEntity(current, { waitCounter: current.frame });
    return { entity: current, transitions, ruleIds };
}
