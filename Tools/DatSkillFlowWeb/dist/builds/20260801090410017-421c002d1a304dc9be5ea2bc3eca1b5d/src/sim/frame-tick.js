// dat-skill-flow-build:20260801090410017-421c002d1a304dc9be5ea2bc3eca1b5d
import { currentFrame } from "./catalog.js";
import { GATE2_RULE } from "./rules.js";
             
                         
              
                       
                    

                                  
                               
                                                          
                                        
 

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
    const transitions                         = [];
    const ruleIds           = [];
    if (current.frameDelay !== 0 && current.rawObjectType !== 3) {
        return { entity: current, transitions, ruleIds };
    }
    if (current.attackExempt > 0) {
        current = replaceEntity(current, { attackExempt: current.attackExempt - 1 });
    }
    if (current.linkState < 0) {
        return { entity: current, transitions, ruleIds };
    }
    let definition = currentFrame(current.frames, current.frame);
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

    if (current.rawObjectType >= 0 && definition.state === 0 && current.yInt < 0) {
        const fromFrame = current.frame;
        current = replaceEntity(current, { frame: 212 });
        transitions.push(transitionEvent(current, "state0-airborne", fromFrame, 212, null));
        ruleIds.push(GATE2_RULE.frameState0Airborne);
        definition = currentFrame(current.frames, 212);
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
            definition = currentFrame(current.frames, targetFrame);
            if (definition === undefined) {
                return { entity: current, transitions, ruleIds };
            }
        }
    }

    current = replaceEntity(current, { waitCounter: current.frame });
    return { entity: current, transitions, ruleIds };
}
