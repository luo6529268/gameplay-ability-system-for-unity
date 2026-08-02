// dat-skill-flow-build:20260801142807766-ea8d785d9bfb45c0b2d058af52e8e2e9
import { authoredFrame, currentFrame, MAX_WORLD_SLOTS } from "./catalog.js";
import { nextNtsdRandom } from "./rng.js";
import { GATE3B2_WPOINT_RULE } from "./rules.js";
import { entitiesFromSlots, freezeSimulationState, replaceSlot } from "./world.js";
             
                    
              
                     
                  
                   
                        
                    
                    
                    

const DEFAULT_WPOINT                      = Object.freeze({
    kind: 0,
    x: 0,
    y: 0,
    attacking: 0,
    cover: 0,
    weaponact: 0,
    dvx: 0,
    dvy: 0,
    dvz: 0,
});

const itrPayloadKeys = [
    "dvx", "dvy", "fall", "bdefend", "injury", "arest", "vrest", "effect", "attacking",
    "catchingact", "catchingact2", "caughtact", "caughtact2", "respond", "pickingact", "pickedact",
    "throwvx", "throwvy", "zwidth", "throwvz", "throwinjury",
]         ;

function safeIntegerResult(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new RangeError(`${label} must be a safe integer`);
    }
    return value;
}

const MIN_SAFE_BIGINT = BigInt(Number.MIN_SAFE_INTEGER);
const MAX_SAFE_BIGINT = BigInt(Number.MAX_SAFE_INTEGER);

function exactSafeInteger(value        , label        )         {
    if (value < MIN_SAFE_BIGINT || value > MAX_SAFE_BIGINT) {
        throw new RangeError(`${label} must be a safe integer`);
    }
    return Number(value);
}

export function firstWpoint(frame                                                                   )                      {
    return frame?.wpoints?.[0] ?? DEFAULT_WPOINT;
}

export function resolveHeldAttackPayload(
    heldItr                              ,
    held           ,
    holder                       ,
    victimSlot        ,
)                               {
    if (heldItr === undefined || heldItr.kind !== 5 || held.linkState >= 0 || holder === undefined
        || !holder.active || holder.targetIdx !== held.slot) {
        return heldItr;
    }
    const holderFrame = authoredFrame(holder.frames, holder.prevFrame2);
    const selector = firstWpoint(holderFrame).attacking;
    if (holderFrame === undefined || selector <= 0 || holder.slot === victimSlot) {
        return heldItr;
    }
    const payload = holderFrame.itrs?.[selector];
    const replacement                         = { ...heldItr, kind: 0 };
    if (payload !== undefined) {
        for (const key of itrPayloadKeys) replacement[key] = payload[key];
    }
    return Object.freeze(replacement)                               ;
}

function event(
    kind                         ,
    holder                       ,
    held                       ,
    holderSlot        ,
    heldSlot        ,
    pass               ,
    ruleId        ,
    detail        ,
)                  {
    return Object.freeze({
        kind,
        holderStableId: holder?.stableId ?? "",
        holderSlot,
        heldStableId: held?.stableId ?? "",
        heldSlot,
        pass,
        ruleId,
        detail,
    });
}

function clearNormalRelease(holder           , held           )                                  {
    const holderChanges                     = { linkState: 0 };
    if (holder.heldWeaponSlot === held.slot) {
        holderChanges.heldWeaponSlot = -1;
        holderChanges.throwFrameGuard = -1;
    }
    return Object.freeze([
        Object.freeze({ ...holder, ...holderChanges }),
        Object.freeze({ ...held, linkState: 0 }),
    ]);
}

function drawModulo(state                 , modulo        )                                     {
    const sample = nextNtsdRandom(state.rngSeed);
    return Object.freeze([
        freezeSimulationState({ ...state, rngSeed: sample.seed }),
        sample.value % modulo,
    ]);
}

                                 
                                    
                                                
                                        
 

export function runHeldObjectPass(state                 , pass        )                 {
    let working = state;
    const events                    = [];
    const rules           = [GATE3B2_WPOINT_RULE.heldPassOrder];
    for (let heldSlot = 0; heldSlot < MAX_WORLD_SLOTS; heldSlot++) {
        let held = working.slots[heldSlot];
        if (held === null || !held.active || held.linkState >= 0) continue;
        const holderSlot = held.holderIdx;
        let holder = holderSlot >= 0 && holderSlot < MAX_WORLD_SLOTS ? working.slots[holderSlot] : null;
        if (holder === null || !holder.active || holder.targetIdx !== heldSlot) {
            held = Object.freeze({ ...held, linkState: 0 });
            working = replaceSlot(working, heldSlot, held);
            events.push(event("link-validation", holder ?? undefined, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.negativeLinkValidation, "clear-held-link-only"));
            rules.push(GATE3B2_WPOINT_RULE.negativeLinkValidation);
            continue;
        }

        const holderFrame = currentFrame(holder.frames, holder.frame);
        if (holderFrame !== undefined) {
            const wp = firstWpoint(holderFrame);
            held = Object.freeze({
                ...held,
                frame: wp.weaponact,
                facing: holder.facing,
                frameDelay: holder.frameDelay,
            });
            const caughtFrame = currentFrame(held.frames, held.frame);
            let positioned = false;
            if (caughtFrame !== undefined) {
                const caughtWp = firstWpoint(caughtFrame);
                const holderCx = holderFrame.centerx ?? 0;
                const holderCy = holderFrame.centery ?? 0;
                const caughtCx = caughtFrame.centerx ?? 0;
                const caughtCy = caughtFrame.centery ?? 0;
                const xBase = holder.facing === 0
                    ? BigInt(holder.xInt) - BigInt(holderCx) + BigInt(wp.x)
                    : BigInt(holderCx) - BigInt(wp.x) + BigInt(holder.xInt);
                const xAdjustment = held.facing === 0
                    ? BigInt(caughtCx) - BigInt(caughtWp.x)
                    : BigInt(caughtWp.x) - BigInt(caughtCx);
                const coverOffset = wp.cover === 0 ? -1n : 1n;
                const xInt = exactSafeInteger(xBase + xAdjustment, "held sync xInt");
                const yInt = exactSafeInteger(
                    BigInt(holder.yInt) - BigInt(holderCy) + BigInt(wp.y)
                        + BigInt(caughtCy) - BigInt(caughtWp.y) + coverOffset,
                    "held sync yInt",
                );
                const zInt = exactSafeInteger(
                    BigInt(holder.zInt) - coverOffset,
                    "held sync zInt",
                );
                held = Object.freeze({ ...held, xInt, yInt, zInt, x: xInt, y: yInt, z: zInt });
                positioned = true;
            }
            working = replaceSlot(working, heldSlot, held);
            events.push(event("sync", holder, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.heldSync, positioned ? "frame-and-position" : "frame-only"));
            rules.push(GATE3B2_WPOINT_RULE.heldSync);
        }

        const heldFrame = currentFrame(held.frames, held.frame);
        if (heldFrame !== undefined && (heldFrame.state === 12 || heldFrame.state === 10)) {
            [holder, held] = clearNormalRelease(holder, held);
            let randomFrame;
            [working, randomFrame] = drawModulo(working, 16);
            held = Object.freeze({
                ...held,
                frame: randomFrame,
                vy: holder.hitCount === 1 ? holder.knockbackVy : holder.vy,
                vz: holder.hitCount === 1 ? holder.knockbackVz : holder.vz,
                vx: (holder.hitCount === 1 ? holder.knockbackVx : holder.vx) * 0.3333333333333333,
                y: held.y < -2 ? -2 : held.y,
            });
            working = replaceSlot(working, holderSlot, holder);
            working = replaceSlot(working, heldSlot, held);
            events.push(event("release", holder, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.stateRelease, `state-${heldFrame.state}`));
            rules.push(GATE3B2_WPOINT_RULE.stateRelease);
        }

        if (holderFrame === undefined) continue;
        const wp = firstWpoint(holderFrame);
        if (wp.dvx !== 0 && (held.rawObjectType === 1 || held.rawObjectType === 4 || held.rawObjectType === 6)) {
            [holder, held] = clearNormalRelease(holder, held);
            held = Object.freeze({
                ...held,
                spawnerSlot: holderSlot,
                frame: 40,
                vx: holder.facing === 0 ? wp.dvx : -wp.dvx,
                vy: wp.dvy,
                ...(holder.keyUp && !holder.keyDown ? { vz: -wp.dvz }
                    : (!holder.keyUp && holder.keyDown ? { vz: wp.dvz } : {})),
            });
            working = replaceSlot(working, holderSlot, holder);
            working = replaceSlot(working, heldSlot, held);
            events.push(event("release", holder, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.dvxRelease, `type-${held.rawObjectType}`));
            rules.push(GATE3B2_WPOINT_RULE.dvxRelease);
        }
        if (wp.dvx !== 0 && held.rawObjectType === 2) {
            [holder, held] = clearNormalRelease(holder, held);
            let randomFrame;
            [working, randomFrame] = drawModulo(working, 6);
            held = Object.freeze({
                ...held,
                frame: randomFrame,
                vx: holder.facing === 0 ? wp.dvx : -wp.dvx,
                vy: wp.dvy,
                ...(holder.keyUp && !holder.keyDown ? { vz: -wp.dvz }
                    : (!holder.keyUp && holder.keyDown ? { vz: wp.dvz } : {})),
            });
            working = replaceSlot(working, holderSlot, holder);
            working = replaceSlot(working, heldSlot, held);
            events.push(event("release", holder, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.dvxRelease, "type-2"));
            rules.push(GATE3B2_WPOINT_RULE.dvxRelease);
        }
        if (wp.kind === 3) {
            [holder, held] = clearNormalRelease(holder, held);
            let frameValue; let vxValue; let vyValue; let vzValue;
            [working, frameValue] = drawModulo(working, 6);
            [working, vxValue] = drawModulo(working, 7);
            [working, vyValue] = drawModulo(working, 4);
            [working, vzValue] = drawModulo(working, 5);
            held = Object.freeze({
                ...held,
                frame: frameValue,
                vx: vxValue - 3,
                vy: -vyValue,
                vz: (vzValue - 2) * 0.2,
            });
            working = replaceSlot(working, holderSlot, holder);
            working = replaceSlot(working, heldSlot, held);
            events.push(event("release", holder, held, holderSlot, heldSlot, pass,
                GATE3B2_WPOINT_RULE.kind3Release, "kind-3"));
            rules.push(GATE3B2_WPOINT_RULE.kind3Release);
        }
    }
    return Object.freeze({ state: working, events: Object.freeze(events), ruleIds: Object.freeze(rules) });
}

function requirePickupInteger(value         , label        )         {
    if (!Number.isSafeInteger(value)) throw new TypeError(`${label} must be a safe integer`);
    const integer = value          ;
    if (integer < 0 || integer >= MAX_WORLD_SLOTS) throw new RangeError(`${label} must be in 0..399`);
    return integer;
}

export function parsePickupInputs(input                 )                            {
    assertPickupInputCapacity(input);
    if (input.pickups === undefined) return Object.freeze([]);
    return Object.freeze(input.pickups.map((candidate, index) => {
        if (candidate === null || typeof candidate !== "object" || Array.isArray(candidate)) {
            throw new TypeError(`input.pickups[${index}] must be an object`);
        }
        const record = candidate                 ;
        const keys = Object.keys(record).sort();
        if (keys.join(",") !== "kind,pickerSlot,weaponSlot") {
            throw new TypeError(`input.pickups[${index}] must contain exactly kind, pickerSlot, weaponSlot`);
        }
        if (record.kind !== 2 && record.kind !== 7) {
            throw new RangeError(`input.pickups[${index}].kind must be 2 or 7`);
        }
        return Object.freeze({
            kind: record.kind,
            pickerSlot: requirePickupInteger(record.pickerSlot, `input.pickups[${index}].pickerSlot`),
            weaponSlot: requirePickupInteger(record.weaponSlot, `input.pickups[${index}].weaponSlot`),
        })                  ;
    }));
}

export const MAX_INJECTED_PICKUPS = MAX_WORLD_SLOTS;

export function assertPickupInputCapacity(input                 )       {
    if (input.pickups === undefined) return;
    if (!Array.isArray(input.pickups)) throw new TypeError("input.pickups must be an array");
    if (input.pickups.length > MAX_INJECTED_PICKUPS) {
        throw new RangeError("input.pickups must contain at most 400 entries");
    }
}

                                        
                                    
                                                
                                        
 

export function applyPickupInputs(state                 , pickups                           )                        {
    const slots = [...state.slots];
    let changed = false;
    const events                    = [];
    const ruleIds           = [];
    for (const pickup of pickups) {
        let picker = slots[pickup.pickerSlot];
        let held = slots[pickup.weaponSlot];
        if (picker === null || held === null || !picker.active || !held.active || picker.rawObjectType !== 0
            || ![1, 2, 4, 6].includes(held.rawObjectType)) continue;
        if (pickup.kind === 7 && picker.linkState !== 0) continue;

        let pickerLink = 1;
        let heldLink = -1;
        let pickerFrame = picker.frame;
        if (pickup.kind === 2) {
            pickerFrame = held.rawObjectType === 2 ? 116 : 115;
            pickerLink = held.rawObjectType;
            if (held.rawObjectType === 1 && (held.oid === 0x78 || held.oid === 0x7c)) pickerLink = 101;
            if (held.rawObjectType === 6 && held.hp <= 0) pickerLink = 4;
            heldLink = -pickerLink;
        } else {
            if (held.oid === 0x78 || held.oid === 0x7c) pickerLink = 101;
            if (held.rawObjectType === 4) pickerLink = 4;
            if (held.rawObjectType === 6) pickerLink = held.hp > 0 ? 6 : 4;
            heldLink = held.rawObjectType === 4 || held.rawObjectType === 6 ? -pickerLink : -1;
        }
        picker = Object.freeze({
            ...picker,
            frame: pickerFrame,
            linkState: pickerLink,
            targetIdx: held.slot,
            heldWeaponSlot: held.slot,
            pickupCount: safeIntegerResult(picker.pickupCount + 1, "picker.pickupCount"),
            ...(pickup.kind === 2 ? { attacking: 0 } : {}),
        });
        held = Object.freeze({
            ...held,
            linkState: heldLink,
            targetIdx: held.targetIdx,
            holderIdx: picker.slot,
            holderCopy: picker.slot,
            unk364: picker.unk364,
            ...(held.rawObjectType === 6 && held.hp <= 0 ? { unk31C: 0 } : {}),
        });
        slots[picker.slot] = picker;
        slots[held.slot] = held;
        changed = true;
        const ruleId = pickup.kind === 2 ? GATE3B2_WPOINT_RULE.pickupKind2 : GATE3B2_WPOINT_RULE.pickupKind7;
        events.push(event("pickup", picker, held, picker.slot, held.slot, null, ruleId, `kind-${pickup.kind}-type-${held.rawObjectType}`));
        ruleIds.push(ruleId);
    }
    const nextState = changed
        ? freezeSimulationState({ ...state, slots: Object.freeze(slots), entities: entitiesFromSlots(slots) })
        : state;
    return Object.freeze({ state: nextState, events: Object.freeze(events), ruleIds: Object.freeze(ruleIds) });
}

export function validateHeldWeaponCaches(state                 )                        {
    let working = state;
    const events                    = [];
    for (let slot = 0; slot < MAX_WORLD_SLOTS; slot++) {
        const holder = working.slots[slot];
        if (holder === null || !holder.active || currentFrame(holder.frames, holder.frame) === undefined
            || holder.heldWeaponSlot < 0) continue;
        const heldSlot = holder.heldWeaponSlot;
        const held = heldSlot < MAX_WORLD_SLOTS ? working.slots[heldSlot] : null;
        if (heldSlot >= MAX_WORLD_SLOTS || held === null || !held.active || held.linkState >= 0 || held.holderIdx !== slot) {
            const cleared = Object.freeze({ ...holder, heldWeaponSlot: -1, throwFrameGuard: -1 });
            working = replaceSlot(working, slot, cleared);
            events.push(event("link-validation", cleared, held ?? undefined, slot, heldSlot, null,
                GATE3B2_WPOINT_RULE.cacheValidation, "clear-holder-cache-and-guard"));
        }
    }
    return Object.freeze({
        state: working,
        events: Object.freeze(events),
        ruleIds: events.length === 0 ? Object.freeze([]) : Object.freeze([GATE3B2_WPOINT_RULE.cacheValidation]),
    });
}

export function validatePositiveLinks(state                 )                        {
    let working = state;
    const events                    = [];
    for (let slot = 0; slot < MAX_WORLD_SLOTS; slot++) {
        const holder = working.slots[slot];
        if (holder === null || !holder.active || holder.linkState <= 0) continue;
        const heldSlot = holder.targetIdx;
        const held = heldSlot >= 0 && heldSlot < MAX_WORLD_SLOTS ? working.slots[heldSlot] : null;
        if (held === null || !held.active || held.holderIdx !== slot) {
            const cleared = Object.freeze({ ...holder, linkState: 0 });
            working = replaceSlot(working, slot, cleared);
            events.push(event("link-validation", cleared, held ?? undefined, slot, heldSlot, null,
                GATE3B2_WPOINT_RULE.positiveLinkValidation, "clear-holder-link-only"));
        }
    }
    return Object.freeze({
        state: working,
        events: Object.freeze(events),
        ruleIds: events.length === 0 ? Object.freeze([]) : Object.freeze([GATE3B2_WPOINT_RULE.positiveLinkValidation]),
    });
}

export function forceDropHeldWeapon(state                 , holderSlot        )                  {
    const holder = holderSlot >= 0 && holderSlot < MAX_WORLD_SLOTS ? state.slots[holderSlot] : null;
    if (holder === null || holder.heldWeaponSlot < 0) return state;
    const heldSlot = holder.heldWeaponSlot;
    const held = heldSlot < MAX_WORLD_SLOTS ? state.slots[heldSlot] : null;
    if (held === null || !held.active) {
        return replaceSlot(state, holderSlot, Object.freeze({ ...holder, heldWeaponSlot: -1 }));
    }
    const droppedHolder = Object.freeze({
        ...holder,
        linkState: 0,
        targetIdx: -1,
        heldWeaponSlot: -1,
        throwFrameGuard: -1,
    });
    const droppedHeld = Object.freeze({
        ...held,
        linkState: 0,
        targetIdx: -1,
        holderIdx: -1,
        holderCopy: -1,
        catcherIdx: -1,
        caughtIdx: -1,
        caughtDuration: 0,
        vx: held.vx * 0.5,
    });
    const slots = [...state.slots];
    slots[holderSlot] = droppedHolder;
    slots[heldSlot] = droppedHeld;
    return freezeSimulationState({ ...state, slots: Object.freeze(slots), entities: entitiesFromSlots(slots) });
}
