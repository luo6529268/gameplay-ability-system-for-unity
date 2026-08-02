// dat-skill-flow-build:20260801084310444-e3e7da15b2524da9b631c449e9205fa3
import { FRAME_MS, ticksToMilliseconds } from "./constants.js";
import { createDatCatalog, MAX_WORLD_SLOTS, normalizeFrames } from "./catalog.js";
import {
    compareUtf16CodeUnits,
    digestCanonicalSnapshot,
    normalizeJsonObject,
} from "./canonical.js";
import { frameTick } from "./frame-tick.js";
import { postCooldownInput } from "./input.js";
import { processOpointSpawn } from "./opoint.js";
import { GATE2_RULE, GATE3B1_OPOINT_RULE } from "./rules.js";
import {
    createSlots,
    entitiesFromSlots,
    freezeSimulationState,
    normalizeVrest,
    replaceSlot,
} from "./world.js";
             
                   
                            
                   
              
                  
                   
                      
                    
                           
                      
                    
                         
                    

function requireSafeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new TypeError(`${label} must be a safe integer`);
    }
    return value;
}

function requireFinite(value        , label        )         {
    if (!Number.isFinite(value)) {
        throw new TypeError(`${label} must be finite`);
    }
    return value;
}

const cooldownKeys = ["right", "left", "up", "down", "attack", "jump", "defend"]         ;
const comboKeys = ["DRA", "DLA", "DUA", "DDA", "DRJ", "DLJ", "DUJ", "DDJ", "DJA"]         ;

function normalizeCooldowns(seed                                        )                    {
    return Object.freeze(Object.fromEntries(cooldownKeys.map((key) => [
        key,
        requireSafeInteger(seed?.[key] ?? 0, `entity.cooldowns.${key}`),
    ])))                                ;
}

function normalizeCombos(seed                                     )                 {
    const entries = comboKeys.map((key) => {
        const value = requireSafeInteger(seed?.[key] ?? 0, `entity.combos.${key}`);
        if (value < 0 || value > 3) {
            throw new RangeError(`entity.combos.${key} must be in 0..3`);
        }
        return [key, value]         ;
    });
    return Object.freeze(Object.fromEntries(entries))                             ;
}

function normalizeEntity(seed               )            {
    if (seed.stableId.length === 0) {
        throw new TypeError("entity stableId must not be empty");
    }
    const slot = requireSafeInteger(seed.slot, "entity.slot");
    if (slot < 0 || slot >= MAX_WORLD_SLOTS) {
        throw new RangeError("entity.slot must be in 0..399");
    }
    const frame = requireSafeInteger(seed.frame, "entity.frame");
    const facing = seed.facing ?? 0;
    if (facing !== 0 && facing !== 1) {
        throw new RangeError("entity.facing must be 0 or 1");
    }
    return Object.freeze({
        stableId: seed.stableId,
        slot,
        rawObjectType: requireSafeInteger(seed.rawObjectType, "entity.rawObjectType"),
        runtimeObjectType: requireSafeInteger(seed.runtimeObjectType ?? (seed.rawObjectType === 0 ? 0 : 1), "entity.runtimeObjectType"),
        entityType: requireSafeInteger(seed.entityType ?? seed.rawObjectType, "entity.entityType"),
        weaponHp: requireSafeInteger(seed.weaponHp ?? 0, "entity.weaponHp"),
        oid: requireSafeInteger(seed.oid ?? 0, "entity.oid"),
        frame,
        hp: requireSafeInteger(seed.hp ?? 500, "entity.hp"),
        hpMax: requireSafeInteger(seed.hpMax ?? seed.hp ?? 500, "entity.hpMax"),
        hp3: requireSafeInteger(seed.hp3 ?? seed.hp ?? 500, "entity.hp3"),
        pp: requireSafeInteger(seed.pp ?? 500, "entity.pp"),
        comboCountVic: requireSafeInteger(seed.comboCountVic ?? 0, "entity.comboCountVic"),
        ppDisplay: requireSafeInteger(seed.ppDisplay ?? 0, "entity.ppDisplay"),
        waitCounter: requireSafeInteger(seed.waitCounter ?? frame, "entity.waitCounter"),
        attacking: requireSafeInteger(seed.attacking ?? 0, "entity.attacking"),
        facing,
        x: requireFinite(seed.x ?? seed.xInt ?? 0, "entity.x"),
        y: requireFinite(seed.y ?? seed.yInt ?? 0, "entity.y"),
        z: requireFinite(seed.z ?? seed.zInt ?? 0, "entity.z"),
        xInt: requireSafeInteger(seed.xInt ?? Math.trunc(seed.x ?? 0), "entity.xInt"),
        yInt: requireSafeInteger(seed.yInt ?? 0, "entity.yInt"),
        zInt: requireSafeInteger(seed.zInt ?? Math.trunc(seed.z ?? 0), "entity.zInt"),
        vx: requireFinite(seed.vx ?? 0, "entity.vx"),
        vy: requireFinite(seed.vy ?? 0, "entity.vy"),
        vz: requireFinite(seed.vz ?? 0, "entity.vz"),
        team: requireSafeInteger(seed.team ?? 0, "entity.team"),
        ownerId: requireSafeInteger(seed.ownerId ?? -1, "entity.ownerId"),
        holderIdx: requireSafeInteger(seed.holderIdx ?? -1, "entity.holderIdx"),
        holderCopy: requireSafeInteger(seed.holderCopy ?? 99, "entity.holderCopy"),
        spawnerSlot: requireSafeInteger(seed.spawnerSlot ?? -1, "entity.spawnerSlot"),
        targetIdx: requireSafeInteger(seed.targetIdx ?? -1, "entity.targetIdx"),
        heldWeaponSlot: requireSafeInteger(seed.heldWeaponSlot ?? -1, "entity.heldWeaponSlot"),
        aiControlled: seed.aiControlled ?? false,
        keyUp: seed.keyUp ?? false,
        keyDown: seed.keyDown ?? false,
        unk364: requireSafeInteger(seed.unk364 ?? 0, "entity.unk364"),
        hitStop: requireSafeInteger(seed.hitStop ?? 0, "entity.hitStop"),
        frameDelay: requireSafeInteger(seed.frameDelay ?? 0, "entity.frameDelay"),
        killCount: requireSafeInteger(seed.killCount ?? -1, "entity.killCount"),
        cooldowns: normalizeCooldowns(seed.cooldowns),
        combos: normalizeCombos(seed.combos),
        linkState: requireSafeInteger(seed.linkState ?? 0, "entity.linkState"),
        unk324: requireSafeInteger(seed.unk324 ?? -1, "entity.unk324"),
        unk328: requireSafeInteger(seed.unk328 ?? -1, "entity.unk328"),
        unk32C: requireSafeInteger(seed.unk32C ?? -1, "entity.unk32C"),
        unk33C: requireSafeInteger(seed.unk33C ?? -1, "entity.unk33C"),
        unk338: requireSafeInteger(seed.unk338 ?? 0, "entity.unk338"),
        animCounter: requireSafeInteger(seed.animCounter ?? 0, "entity.animCounter"),
        attackExempt: requireSafeInteger(seed.attackExempt ?? 0, "entity.attackExempt"),
        active: seed.active ?? true,
        frames: normalizeFrames(seed.frames),
    });
}

export function createSimulation(options                         )                  {
    const tickIndex = requireSafeInteger(options.tickIndex ?? 0, "tickIndex");
    if (tickIndex < 0) {
        throw new RangeError("tickIndex must be nonnegative");
    }
    const entities = options.entities
        .map(normalizeEntity)
        .sort((left, right) => left.slot - right.slot || compareUtf16CodeUnits(left.stableId, right.stableId));
    const stableIds = new Set        ();
    const occupiedSlots = new Set        ();
    for (const entity of entities) {
        if (stableIds.has(entity.stableId)) {
            throw new TypeError(`duplicate stableId: ${entity.stableId}`);
        }
        if (occupiedSlots.has(entity.slot)) {
            throw new TypeError(`duplicate slot: ${entity.slot}`);
        }
        stableIds.add(entity.stableId);
        occupiedSlots.add(entity.slot);
    }
    const slots = createSlots(entities);
    const attackRest = [...(options.attackRest ?? [])];
    if (attackRest.length > MAX_WORLD_SLOTS) {
        throw new RangeError("attackRest must contain at most 400 slots");
    }
    while (attackRest.length < MAX_WORLD_SLOTS) attackRest.push(0);
    for (const [slot, value] of attackRest.entries()) {
        requireSafeInteger(value, `attackRest[${slot}]`);
    }
    return freezeSimulationState({
        tickIndex,
        timeMs: ticksToMilliseconds(tickIndex),
        objectCount: entities.filter((entity) => entity.active).length,
        worldInput: Object.freeze({
            ppMode: requireSafeInteger(options.worldInput?.ppMode ?? 1, "worldInput.ppMode"),
            oid6DjaGuard: requireSafeInteger(options.worldInput?.oid6DjaGuard ?? 0, "worldInput.oid6DjaGuard"),
        }),
        slots,
        entities: entitiesFromSlots(slots),
        catalog: createDatCatalog(options.catalog, options.entities),
        attackRest: Object.freeze(attackRest),
        vrest: normalizeVrest(options.vrest),
        nextSpawnOrdinal: 0,
    });
}

function freeEntityAt(state                 , slot        )                  {
    const entity = state.slots[slot];
    if (entity === undefined || entity === null || !entity.active) {
        return state;
    }
    const inactive = Object.freeze({ ...entity, active: false });
    return replaceSlot(state, slot, inactive, {
        objectCount: state.objectCount - 1,
    });
}

export function freeEntity(state                 , stableId        )                  {
    const entity = state.entities.find((candidate) => candidate.stableId === stableId);
    return entity === undefined ? state : freeEntityAt(state, entity.slot);
}

function uniqueRuleIds(ruleIds                   )                    {
    return Object.freeze([...new Set(ruleIds)]);
}

export function stepSimulation(
    state                 ,
    input                 ,
    runtime                    = {},
)                       {
    const normalizedInput = normalizeJsonObject(input, "input");
    const nextTickIndex = state.tickIndex + 1;
    const nextTimeMs = state.timeMs + FRAME_MS;
    let working = freezeSimulationState({ ...state, tickIndex: nextTickIndex, timeMs: nextTimeMs });
    const frameTransitions = [];
    const collisions                   = [];
    const lifecycle                   = [];
    const slotLifecycle = [];
    const spawns = [];
    const inputJumps = [];
    const ruleIds           = [
        GATE2_RULE.clockFrameMs,
        GATE3B1_OPOINT_RULE.fixedWorldSlots,
        GATE3B1_OPOINT_RULE.dynamicLateSlots,
    ];

    const afterInputSlots = working.slots.map((entity) => {
        if (entity === null) return null;
        const result = postCooldownInput(entity, working.worldInput);
        inputJumps.push(...result.events);
        ruleIds.push(...result.ruleIds);
        return result.entity;
    });
    working = freezeSimulationState({
        ...working,
        slots: Object.freeze(afterInputSlots),
        entities: entitiesFromSlots(afterInputSlots),
    });

    for (let slot = 0; slot < MAX_WORLD_SLOTS; slot++) {
        const beforeTick = working.slots[slot];
        if (beforeTick === null || !beforeTick.active) {
            continue;
        }

        const tickResult = frameTick(beforeTick);
        working = replaceSlot(working, slot, tickResult.entity);
        frameTransitions.push(...tickResult.transitions);
        ruleIds.push(...tickResult.ruleIds);

        let current = working.slots[slot] ;
        if (current.active) {
            const detail = runtime.collision?.(current, {
                tickIndex: nextTickIndex,
                timeMs: nextTimeMs,
                input: normalizedInput,
            });
            collisions.push(Object.freeze({
                stableId: current.stableId,
                slot: current.slot,
                frame: current.frame,
                detail: detail === undefined || detail === null
                    ? null
                    : normalizeJsonObject(detail, "collision.detail"),
            }));
            ruleIds.push(GATE2_RULE.lateFrameCollisionOrder);
        }

        current = working.slots[slot] ;
        const lateFrame = current.frame;
        if (lateFrame >= 1100 && lateFrame <= 1299) {
            const groupHitStop = 1100 - lateFrame;
            const childStableIds           = [];
            const groupedSlots = working.slots.map((candidate, candidateSlot) => {
                if (candidate === null) {
                    return null;
                }
                if (!candidate.active) {
                    return candidate;
                }
                if (candidateSlot === slot) {
                    return Object.freeze({ ...candidate, frame: 0, hitStop: groupHitStop });
                }
                if (candidate.killCount === current.slot) {
                    childStableIds.push(candidate.stableId);
                    return Object.freeze({ ...candidate, hitStop: groupHitStop });
                }
                return candidate;
            });
            working = freezeSimulationState({
                ...working,
                slots: Object.freeze(groupedSlots),
                entities: entitiesFromSlots(groupedSlots),
            });
            lifecycle.push(Object.freeze({
                stableId: current.stableId,
                slot: current.slot,
                kind: "frame-group-reset",
                frame: lateFrame,
                childStableIds: Object.freeze(childStableIds),
            }));
            ruleIds.push(GATE2_RULE.lateGroupReset);
            continue;
        }

        if (lateFrame < 0 || lateFrame >= 400) {
            const reset = Object.freeze({ ...current, frame: 0 });
            working = replaceSlot(working, slot, reset);
            working = freeEntityAt(working, slot);
            lifecycle.push(Object.freeze({
                stableId: current.stableId,
                slot: current.slot,
                kind: "free",
                frame: lateFrame,
                childStableIds: Object.freeze([]),
            }));
            slotLifecycle.push(Object.freeze({
                stableId: current.stableId,
                slot: current.slot,
                kind: "release"         ,
            }));
            ruleIds.push(GATE2_RULE.lateInvalidFrameFree, GATE2_RULE.lifecycleActiveGuardFree);
            continue;
        }

        if (working.slots[slot]?.active === true) {
            const spawnResult = processOpointSpawn(working, slot);
            working = spawnResult.state;
            spawns.push(...spawnResult.spawns);
            slotLifecycle.push(...spawnResult.slotLifecycle);
            ruleIds.push(...spawnResult.ruleIds);
        }
    }

    const trace = Object.freeze({
        schemaVersion: 1         ,
        tickIndex: nextTickIndex,
        timeMs: nextTimeMs,
        inputs: normalizedInput,
        frameTransitions: Object.freeze(frameTransitions),
        collisions: Object.freeze(collisions),
        lifecycle: Object.freeze(lifecycle),
        slotLifecycle: Object.freeze(slotLifecycle),
        spawns: Object.freeze(spawns),
        inputJumps: Object.freeze(inputJumps),
        ruleIds: uniqueRuleIds(ruleIds),
        snapshotDigest: digestCanonicalSnapshot(working),
    });
    return Object.freeze({ state: working, trace });
}

export function replaySimulation(
    initial                 ,
    script                            ,
    runtime                    = {},
)                         {
    let state = initial;
    const traces = [];
    for (const input of script) {
        const result = stepSimulation(state, input, runtime);
        state = result.state;
        traces.push(result.trace);
    }
    return Object.freeze({ state, traces: Object.freeze(traces) });
}
