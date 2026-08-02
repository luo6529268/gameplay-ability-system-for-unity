// dat-skill-flow-build:20260801080235531-87df618a42a04a94934d7345a6213c99
import { FRAME_MS, ticksToMilliseconds } from "./constants.js";
import {
    compareUtf16CodeUnits,
    digestCanonicalSnapshot,
    normalizeJsonObject,
} from "./canonical.js";
import { frameTick } from "./frame-tick.js";
import { postCooldownInput } from "./input.js";
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
            mp: requireSafeInteger(candidate.mp ?? 0, "frame.mp"),
            hit_a: requireSafeInteger(candidate.hit_a ?? 0, "frame.hit_a"),
            hit_d: requireSafeInteger(candidate.hit_d ?? 0, "frame.hit_d"),
            hit_j: requireSafeInteger(candidate.hit_j ?? 0, "frame.hit_j"),
            hit_Fa: requireSafeInteger(candidate.hit_Fa ?? 0, "frame.hit_Fa"),
            hit_Ua: requireSafeInteger(candidate.hit_Ua ?? 0, "frame.hit_Ua"),
            hit_Da: requireSafeInteger(candidate.hit_Da ?? 0, "frame.hit_Da"),
            hit_Fj: requireSafeInteger(candidate.hit_Fj ?? 0, "frame.hit_Fj"),
            hit_Uj: requireSafeInteger(candidate.hit_Uj ?? 0, "frame.hit_Uj"),
            hit_Dj: requireSafeInteger(candidate.hit_Dj ?? 0, "frame.hit_Dj"),
            hit_ja: requireSafeInteger(candidate.hit_ja ?? 0, "frame.hit_ja"),
        });
        definitions.set(normalized.id, normalized);
    }
    return Object.freeze([...definitions.values()].sort((left, right) => left.id - right.id));
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
        oid: requireSafeInteger(seed.oid ?? 0, "entity.oid"),
        frame,
        hp: requireSafeInteger(seed.hp ?? 0, "entity.hp"),
        pp: requireSafeInteger(seed.pp ?? 0, "entity.pp"),
        comboCountVic: requireSafeInteger(seed.comboCountVic ?? 0, "entity.comboCountVic"),
        ppDisplay: requireSafeInteger(seed.ppDisplay ?? 0, "entity.ppDisplay"),
        waitCounter: requireSafeInteger(seed.waitCounter ?? frame, "entity.waitCounter"),
        attacking: requireSafeInteger(seed.attacking ?? 0, "entity.attacking"),
        facing,
        yInt: requireSafeInteger(seed.yInt ?? 0, "entity.yInt"),
        hitStop: requireSafeInteger(seed.hitStop ?? 0, "entity.hitStop"),
        frameDelay: requireSafeInteger(seed.frameDelay ?? 0, "entity.frameDelay"),
        killCount: requireSafeInteger(seed.killCount ?? -1, "entity.killCount"),
        cooldowns: normalizeCooldowns(seed.cooldowns),
        combos: normalizeCombos(seed.combos),
        linkState: requireSafeInteger(seed.linkState ?? 0, "entity.linkState"),
        unk324: requireSafeInteger(seed.unk324 ?? -1, "entity.unk324"),
        unk328: requireSafeInteger(seed.unk328 ?? 0, "entity.unk328"),
        unk338: requireSafeInteger(seed.unk338 ?? 0, "entity.unk338"),
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
        worldInput: Object.freeze({
            ppMode: requireSafeInteger(options.worldInput?.ppMode ?? 1, "worldInput.ppMode"),
            oid6DjaGuard: requireSafeInteger(options.worldInput?.oid6DjaGuard ?? 0, "worldInput.oid6DjaGuard"),
        }),
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
    const inputJumps = [];
    const ruleIds           = [GATE2_RULE.clockFrameMs];

    const afterInput = working.entities.map((entity) => {
        const result = postCooldownInput(entity, working.worldInput);
        inputJumps.push(...result.events);
        ruleIds.push(...result.ruleIds);
        return result.entity;
    });
    working = freezeState({ ...working, entities: afterInput });

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
