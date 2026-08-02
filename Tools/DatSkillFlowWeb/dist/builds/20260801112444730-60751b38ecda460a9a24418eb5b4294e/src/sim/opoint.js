// dat-skill-flow-build:20260801112444730-60751b38ecda460a9a24418eb5b4294e
import { currentFrame, resolveDat } from "./catalog.js";
import { GATE3B1_OPOINT_RULE } from "./rules.js";
import {
    createVrestBuilder,
    firstFreeSpawnSlot,
    freezeSimulationState,
    replaceSlot,
} from "./world.js";
             
                     
                     
              
                       
                   
                      
                        
                    
                      
                       
                    

                                      
                                    
                                                 
                                                          
                                        
 

const EMPTY_COOLDOWNS                    = Object.freeze({
    right: 0, left: 0, up: 0, down: 0, attack: 0, jump: 0, defend: 0,
});
const EMPTY_COMBOS                 = Object.freeze({
    DRA: 0, DLA: 0, DUA: 0, DDA: 0, DRJ: 0, DLJ: 0, DUJ: 0, DDJ: 0, DJA: 0,
});

function nextStableId(state                 , slot        )                                        {
    const existing = new Set(state.entities.map((entity) => entity.stableId));
    let ordinal = state.nextSpawnOrdinal;
    while (true) {
        const stableId = `opoint:${state.tickIndex}:${ordinal}:${slot}`;
        if (!existing.has(stableId)) {
            return { stableId, ordinal };
        }
        ordinal = safeIntegerResult(ordinal + 1, "nextSpawnOrdinal");
    }
}

function replaceEntity(entity           , changes                    )            {
    return Object.freeze({ ...entity, ...changes });
}

function safeIntegerResult(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new RangeError(`${label} exceeded the safe integer range`);
    }
    return value;
}

function finiteResult(value        , label        )         {
    if (!Number.isFinite(value)) {
        throw new RangeError(`${label} must remain finite`);
    }
    return value;
}

function decodeFacing(facing        )                                  {
    if (facing > 10) {
        return { count: Math.trunc(facing / 10), mode: facing % 10 };
    }
    return { count: 1, mode: facing };
}

function spawnChild(
    state                 ,
    parentSlot        ,
    frame                    ,
    opoint                     ,
    facingMode        ,
    childDat                  ,
    slot        ,
)                                                                                                                   {
    const parent = state.slots[parentSlot];
    if (parent === null || !parent.active) {
        return undefined;
    }
    const { stableId, ordinal } = nextStableId(state, slot);
    const facing        = facingMode === 0
        ? parent.facing
        : facingMode === 1
            ? (parent.facing === 0 ? 1 : 0)
            : 0;
    const xInt = safeIntegerResult(parent.facing === 0
        ? parent.xInt - (frame.centerx ?? 0) + opoint.x
        : parent.xInt + (frame.centerx ?? 0) - opoint.x, "opoint child xInt");
    const yInt = safeIntegerResult(parent.yInt - (frame.centery ?? 0) + opoint.y, "opoint child yInt");
    const z = finiteResult(parent.z + 1, "opoint child z");
    const zInt = safeIntegerResult(Math.trunc(z), "opoint child zInt");
    let vz = 0;
    const childInitialFrame = currentFrame(childDat.frames, opoint.action);
    if (childInitialFrame !== undefined
        && (childInitialFrame.state === 3000 || childInitialFrame.state === 1002 || childInitialFrame.state === 3006)
        && opoint.oid !== 223 && opoint.oid !== 224) {
        if (parent.keyUp && !parent.keyDown) {
            vz = -2.5;
        } else if (parent.keyDown && !parent.keyUp) {
            vz = 2.5;
        }
        if (opoint.oid === 211) {
            vz *= 0.25;
        }
    }
    const specialVitals = opoint.oid === 5 || opoint.oid === 52;
    const characterDat = childDat.rawObjectType === 0;
    const child            = Object.freeze({
        stableId,
        slot,
        rawObjectType: childDat.rawObjectType,
        runtimeObjectType: characterDat ? 0 : 1,
        entityType: childDat.rawObjectType,
        weaponHp: childDat.weaponHp,
        oid: opoint.oid,
        frame: opoint.action,
        hp: specialVitals ? 10 : 500,
        hpMax: specialVitals ? 10 : 500,
        hp3: specialVitals ? 10 : 500,
        pp: specialVitals ? 5 : 500,
        comboCountVic: 0,
        ppDisplay: 0,
        waitCounter: 0,
        attacking: 0,
        facing,
        x: xInt,
        y: yInt,
        z,
        xInt,
        yInt,
        zInt,
        vx: facing === 1 ? -opoint.dvx : opoint.dvx,
        vy: opoint.dvy,
        vz,
        team: parent.team,
        ownerId: parent.slot,
        holderIdx: parent.slot,
        holderCopy: parent.holderCopy,
        spawnerSlot: -1,
        targetIdx: -1,
        heldWeaponSlot: -1,
        prevFrame2: 0,
        hitCount: 0,
        knockbackVx: 0.1,
        knockbackVy: 0.1,
        knockbackVz: 0.1,
        throwFrameGuard: -1,
        pickupCount: 0,
        catcherIdx: -1,
        caughtIdx: -1,
        caughtDuration: 0,
        fall: 0,
        unk31C: 0,
        aiControlled: characterDat,
        keyUp: false,
        keyDown: false,
        unk364: parent.unk364,
        hitStop: characterDat ? parent.hitStop : 0,
        frameDelay: 0,
        killCount: characterDat ? (parent.killCount > -1 ? parent.killCount : parent.slot) : -1,
        cooldowns: EMPTY_COOLDOWNS,
        combos: EMPTY_COMBOS,
        linkState: opoint.kind === 2 ? -1 : 0,
        unk324: -1,
        unk328: -1,
        unk32C: -1,
        unk33C: -1,
        unk338: 0,
        animCounter: 0,
        attackExempt: 0,
        active: true,
        frames: childDat.frames,
        frameSourceIndex: childDat.frameSourceIndex,
    });

    let working = state;
    if (opoint.kind === 2) {
        working = replaceSlot(working, parent.slot, replaceEntity(parent, {
            linkState: 1,
            targetIdx: slot,
            heldWeaponSlot: slot,
        }));
    }
    working = replaceSlot(working, slot, child, {
        objectCount: safeIntegerResult(working.objectCount + 1, "objectCount"),
        nextSpawnOrdinal: safeIntegerResult(ordinal + 1, "nextSpawnOrdinal"),
    });
    const event                   = Object.freeze({
        stableId,
        slot,
        parentStableId: parent.stableId,
        parentSlot: parent.slot,
        oid: opoint.oid,
        action: opoint.action,
        kind: opoint.kind,
        facing,
        generation: state.tickIndex,
        ordinal,
        ruleId: GATE3B1_OPOINT_RULE.spawnInitialize,
    });
    return {
        state: working,
        child,
        event,
        lifecycle: Object.freeze({ slot, kind: "allocate", stableId }),
    };
}

export function processOpointSpawn(
    state                 ,
    parentSlot        ,
    runtime                                                                                  = {},
)                      {
    const parent = state.slots[parentSlot];
    if (parent === null || !parent.active) {
        return { state, spawns: [], slotLifecycle: [], ruleIds: [] };
    }
    const frame = currentFrame(parent.frames, parent.frame);
    if (frame === undefined) {
        return { state, spawns: [], slotLifecycle: [], ruleIds: [] };
    }
    const opoints = frame.opoints ?? [];
    const rules = [GATE3B1_OPOINT_RULE.opointGuards];
    if (opoints.length === 0 || opoints[0] .kind <= 0 || opoints[0] .oid <= 0 || parent.attacking !== 0) {
        return { state, spawns: [], slotLifecycle: [], ruleIds: rules };
    }
    if (parent.frameDelay !== 0 && parent.rawObjectType === 0) {
        return { state, spawns: [], slotLifecycle: [], ruleIds: rules };
    }

    let working = state;
    const attackRest = [...state.attackRest];
    const vrest = createVrestBuilder(state.vrest, runtime.onOpointVrestOperation);
    let cooldownsChanged = false;
    const spawns                     = [];
    const slotLifecycle                       = [];
    for (const opoint of opoints) {
        if (opoint.kind <= 0 || opoint.oid <= 0) {
            continue;
        }
        const decoded = decodeFacing(opoint.facing);
        const spawnedSlots           = [];
        rules.push(GATE3B1_OPOINT_RULE.catalogResolve);
        const childDat = resolveDat(working.catalog, opoint.oid);
        if (childDat === undefined) {
            continue;
        }
        for (let index = 0; index < decoded.count; index++) {
            const liveParent = working.slots[parentSlot];
            if (liveParent === null || !liveParent.active) {
                break;
            }
            runtime.onOpointAllocationAttempt?.();
            const freeSlot = firstFreeSpawnSlot(working);
            if (freeSlot < 0) {
                break;
            }
            const spawned = spawnChild(working, parentSlot, frame, opoint, decoded.mode, childDat, freeSlot);
            if (spawned === undefined) {
                break;
            }
            working = spawned.state;
            attackRest[spawned.child.slot] = 0;
            vrest.resetSlot(spawned.child.slot);
            cooldownsChanged = true;
            let child = spawned.child;
            if (decoded.count > 1) {
                const spread = (index * 10) / (decoded.count - 1) - 5;
                const vx = child.vx > 0
                    ? child.vx - Math.abs(spread)
                    : child.vx < 0
                        ? child.vx + Math.abs(spread)
                        : child.vx + spread;
                child = replaceEntity(child, { vz: child.vz + spread, vx });
                working = replaceSlot(working, child.slot, child);
            }
            const latestParent = working.slots[parentSlot];
            if (latestParent !== null && latestParent.rawObjectType === 3 && frame.state === 3003) {
                const linkedSlot = latestParent.animCounter;
                if (linkedSlot >= 0 && linkedSlot < working.slots.length && working.slots[linkedSlot]?.active === true) {
                    vrest.set(linkedSlot, child.slot, 10);
                    vrest.set(child.slot, linkedSlot, 10);
                }
            }
            child = replaceEntity(working.slots[child.slot] , { attackExempt: 0 });
            working = replaceSlot(working, child.slot, child);
            spawnedSlots.push(child.slot);
            spawns.push(spawned.event);
            slotLifecycle.push(spawned.lifecycle);
            rules.push(GATE3B1_OPOINT_RULE.spawnInitialize, GATE3B1_OPOINT_RULE.cooldownReset);
        }

        if (spawnedSlots.length > 1) {
            const center = Math.trunc(spawnedSlots.length / 2);
            for (let index = 0; index < spawnedSlots.length; index++) {
                const slot = spawnedSlots[index] ;
                const child = working.slots[slot];
                if (child === null || !child.active) {
                    continue;
                }
                let attackExempt = child.attackExempt;
                if ((spawnedSlots.length & 1) === 0) {
                    if (index < center - 1) attackExempt = (center - index - 1) * 2;
                    else if (index > center) attackExempt = (index - center) * 2;
                } else {
                    if (index < center) attackExempt = (center - index) * 2;
                    else if (index > center) attackExempt = (index - center) * 2;
                }
                working = replaceSlot(working, slot, replaceEntity(child, { attackExempt }));
                for (let previous = 0; previous < index; previous++) {
                    const other = spawnedSlots[previous] ;
                    if (working.slots[other]?.active !== true) {
                        continue;
                    }
                    vrest.set(slot, other, 0x28);
                    vrest.set(other, slot, 0x28);
                }
            }
            rules.push(GATE3B1_OPOINT_RULE.multiSpawn);
        }
    }
    if (cooldownsChanged) {
        working = freezeSimulationState({
            ...working,
            attackRest: Object.freeze(attackRest),
            vrest: vrest.materialize(),
        });
    }
    return {
        state: working,
        spawns: Object.freeze(spawns),
        slotLifecycle: Object.freeze(slotLifecycle),
        ruleIds: Object.freeze(rules),
    };
}
