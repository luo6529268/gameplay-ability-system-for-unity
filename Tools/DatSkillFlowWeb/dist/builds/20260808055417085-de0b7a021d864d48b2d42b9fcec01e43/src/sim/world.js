// dat-skill-flow-build:20260808055417085-de0b7a021d864d48b2d42b9fcec01e43
             
                             
              
                  
                    
                    
import { MAX_WORLD_SLOTS } from "./catalog.js";

export function createSlots(entities                      )                                {
    const slots                       = Array(MAX_WORLD_SLOTS).fill(null);
    for (const entity of entities) {
        if (entity.slot < 0 || entity.slot >= MAX_WORLD_SLOTS) {
            throw new RangeError("entity.slot must be in 0..399");
        }
        if (slots[entity.slot] !== null) {
            throw new TypeError(`duplicate slot: ${entity.slot}`);
        }
        slots[entity.slot] = entity;
    }
    return Object.freeze(slots);
}

export function entitiesFromSlots(slots                               )                       {
    return Object.freeze(slots.filter((entity)                      => entity !== null));
}

export function replaceSlot(
    state                 ,
    slot        ,
    entity                  ,
    changes                           = {},
)                  {
    const slots = [...state.slots];
    slots[slot] = entity;
    const frozenSlots = Object.freeze(slots);
    return freezeSimulationState({ ...state, ...changes, slots: frozenSlots, entities: entitiesFromSlots(frozenSlots) });
}

function freezeArray   (values              )               {
    return Object.isFrozen(values) ? values : Object.freeze([...values]);
}

export function freezeSimulationState(state                 )                  {
    return Object.freeze({
        ...state,
        slots: freezeArray(state.slots),
        entities: freezeArray(state.entities),
        catalog: freezeArray(state.catalog),
        frameSources: freezeArray(state.frameSources),
        attackRest: freezeArray(state.attackRest),
        vrest: freezeArray(state.vrest),
    });
}

export function firstFreeSpawnSlot(state                 )         {
    for (let slot = 50; slot < MAX_WORLD_SLOTS; slot++) {
        if (state.slots[slot]?.active !== true) {
            return slot;
        }
    }
    return -1;
}

export function normalizeVrest(entries                                      )                           {
    const values = new Map                       ();
    for (const entry of entries ?? []) {
        if (!Number.isSafeInteger(entry.fromSlot) || entry.fromSlot < 0 || entry.fromSlot >= MAX_WORLD_SLOTS
            || !Number.isSafeInteger(entry.toSlot) || entry.toSlot < 0 || entry.toSlot >= MAX_WORLD_SLOTS) {
            throw new RangeError("vrest slots must be in 0..399");
        }
        if (!Number.isSafeInteger(entry.ticks)) {
            throw new TypeError("vrest.ticks must be a safe integer");
        }
        if (entry.ticks !== 0) {
            values.set(`${entry.fromSlot}:${entry.toSlot}`, Object.freeze({ ...entry }));
        }
    }
    return Object.freeze([...values.values()].sort((left, right) => (
        left.fromSlot - right.fromSlot || left.toSlot - right.toSlot
    )));
}

export function vrestAt(state                 , fromSlot        , toSlot        )         {
    return state.vrest.find((entry) => entry.fromSlot === fromSlot && entry.toSlot === toSlot)?.ticks ?? 0;
}

export function resetSlotCooldowns(state                 , slot        )                                                {
    const attackRest = [...state.attackRest];
    attackRest[slot] = 0;
    return {
        attackRest: Object.freeze(attackRest),
        vrest: Object.freeze(state.vrest.filter((entry) => entry.fromSlot !== slot && entry.toSlot !== slot)),
    };
}

export function setVrest(
    entries                          ,
    fromSlot        ,
    toSlot        ,
    ticks        ,
)                           {
    const filtered = entries.filter((entry) => entry.fromSlot !== fromSlot || entry.toSlot !== toSlot);
    if (ticks !== 0) {
        filtered.push(Object.freeze({ fromSlot, toSlot, ticks }));
    }
    return Object.freeze(filtered.sort((left, right) => left.fromSlot - right.fromSlot || left.toSlot - right.toSlot));
}

                               
                                  
                                                               
                                            
 

export function createVrestBuilder(
    entries                          ,
    onOperation                                           ,
)               {
    const rows = new Map                             ();
    for (const entry of entries) {
        let row = rows.get(entry.fromSlot);
        if (row === undefined) {
            row = new Map();
            rows.set(entry.fromSlot, row);
        }
        row.set(entry.toSlot, entry.ticks);
    }
    return {
        resetSlot(slot        )       {
            onOperation?.("reset-row");
            rows.delete(slot);
            for (const row of rows.values()) {
                if (row.delete(slot)) onOperation?.("reset-column");
            }
        },
        set(fromSlot        , toSlot        , ticks        )       {
            onOperation?.("set");
            let row = rows.get(fromSlot);
            if (row === undefined) {
                row = new Map();
                rows.set(fromSlot, row);
            }
            if (ticks === 0) row.delete(toSlot);
            else row.set(toSlot, ticks);
            if (row.size === 0) rows.delete(fromSlot);
        },
        materialize()                           {
            onOperation?.("materialize");
            const result                  = [];
            for (const [fromSlot, row] of rows) {
                for (const [toSlot, ticks] of row) {
                    result.push(Object.freeze({ fromSlot, toSlot, ticks }));
                }
            }
            result.sort((left, right) => left.fromSlot - right.fromSlot || left.toSlot - right.toSlot);
            return Object.freeze(result);
        },
    };
}
