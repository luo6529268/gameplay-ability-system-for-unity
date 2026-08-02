// dat-skill-flow-build:20260801073018086-5d6d8db9bd4e405d9e1451ee4b03e453
import { FRAME_MS, ticksToMilliseconds } from "./constants.js";
import {
    compareUtf16CodeUnits,
    digestCanonicalSnapshot,
    normalizeJsonObject,
} from "./canonical.js";
import { frameTick } from "./frame-tick.js";
import { GATE2_RULE } from "./rules.js";
             
                   
                            
                   
              
                  
                       
                    
                           
                      
                    
                         
                    

function requireSafeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new TypeError(`${label} must be a safe integer`);
    }
    return value;
}

function normalizeFrames(frames                               )                                {
    const definitions = new Map                            ();
    for (const candidate of frames) {
        const normalized = Object.freeze({
            id: requireSafeInteger(candidate.id, "frame.id"),
            state: requireSafeInteger(candidate.state, "frame.state"),
            wait: requireSafeInteger(candidate.wait, "frame.wait"),
            next: requireSafeInteger(candidate.next, "frame.next"),
        });
        definitions.set(normalized.id, normalized);
    }
    return Object.freeze([...definitions.values()].sort((left, right) => left.id - right.id));
}

function normalizeEntity(seed               )            {
    if (seed.stableId.length === 0) {
        throw new TypeError("entity stableId must not be empty");
    }
    const slot = requireSafeInteger(seed.slot, "entity.slot");
    if (slot < 0) {
        throw new RangeError("entity.slot must be nonnegative");
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
        frame,
        waitCounter: requireSafeInteger(seed.waitCounter ?? frame, "entity.waitCounter"),
        attacking: requireSafeInteger(seed.attacking ?? 0, "entity.attacking"),
        facing,
        yInt: requireSafeInteger(seed.yInt ?? 0, "entity.yInt"),
        hitStop: requireSafeInteger(seed.hitStop ?? 0, "entity.hitStop"),
        killCount: requireSafeInteger(seed.killCount ?? -1, "entity.killCount"),
        active: seed.active ?? true,
        frames: normalizeFrames(seed.frames),
    });
}

function freezeState(state                 )                  {
    return Object.freeze({ ...state, entities: Object.freeze([...state.entities]) });
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
    const slots = new Set        ();
    for (const entity of entities) {
        if (stableIds.has(entity.stableId)) {
            throw new TypeError(`duplicate stableId: ${entity.stableId}`);
        }
        if (slots.has(entity.slot)) {
            throw new TypeError(`duplicate slot: ${entity.slot}`);
        }
        stableIds.add(entity.stableId);
        slots.add(entity.slot);
    }
    return freezeState({
        tickIndex,
        timeMs: ticksToMilliseconds(tickIndex),
        objectCount: entities.filter((entity) => entity.active).length,
        entities,
    });
}

function replaceEntityAt(
    entities                      ,
    index        ,
    entity           ,
)                       {
    const next = [...entities];
    next[index] = entity;
    return next;
}

function freeEntityAt(state                 , index        )                  {
    const entity = state.entities[index];
    if (entity === undefined || !entity.active) {
        return state;
    }
    const inactive = Object.freeze({ ...entity, active: false });
    return freezeState({
        ...state,
        objectCount: state.objectCount - 1,
        entities: replaceEntityAt(state.entities, index, inactive),
    });
}

export function freeEntity(state                 , stableId        )                  {
    const index = state.entities.findIndex((entity) => entity.stableId === stableId);
    return index < 0 ? state : freeEntityAt(state, index);
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
    let working = freezeState({ ...state, tickIndex: nextTickIndex, timeMs: nextTimeMs });
    const frameTransitions = [];
    const collisions                   = [];
    const lifecycle                   = [];
    const ruleIds           = [GATE2_RULE.clockFrameMs];

    for (let index = 0; index < working.entities.length; index++) {
        const beforeTick = working.entities[index];
        if (beforeTick === undefined || !beforeTick.active) {
            continue;
        }

        const tickResult = frameTick(beforeTick);
        working = freezeState({
            ...working,
            entities: replaceEntityAt(working.entities, index, tickResult.entity),
        });
        frameTransitions.push(...tickResult.transitions);
        ruleIds.push(...tickResult.ruleIds);

        let current = working.entities[index] ;
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

        current = working.entities[index] ;
        const lateFrame = current.frame;
        if (lateFrame >= 1100 && lateFrame <= 1299) {
            const groupHitStop = 1100 - lateFrame;
            const childStableIds           = [];
            const grouped = working.entities.map((candidate, candidateIndex) => {
                if (!candidate.active) {
                    return candidate;
                }
                if (candidateIndex === index) {
                    return Object.freeze({ ...candidate, frame: 0, hitStop: groupHitStop });
                }
                if (candidate.killCount === current.slot) {
                    childStableIds.push(candidate.stableId);
                    return Object.freeze({ ...candidate, hitStop: groupHitStop });
                }
                return candidate;
            });
            working = freezeState({ ...working, entities: grouped });
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
            working = freezeState({
                ...working,
                entities: replaceEntityAt(working.entities, index, reset),
            });
            working = freeEntityAt(working, index);
            lifecycle.push(Object.freeze({
                stableId: current.stableId,
                slot: current.slot,
                kind: "free",
                frame: lateFrame,
                childStableIds: Object.freeze([]),
            }));
            ruleIds.push(GATE2_RULE.lateInvalidFrameFree, GATE2_RULE.lifecycleActiveGuardFree);
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
        slotLifecycle: Object.freeze([]),
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
