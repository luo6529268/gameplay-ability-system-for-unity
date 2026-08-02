// dat-skill-flow-build:20260801064157944-d919fa75ab2543e99e4f65c18ae6d2f1
             
                  
                 
                    
                        
                    

function normalizeJsonValue(value              , path        )               {
    if (value === null || typeof value === "string" || typeof value === "boolean") {
        return value;
    }
    if (typeof value === "number") {
        if (!Number.isFinite(value)) {
            throw new TypeError(`${path} must contain only finite numbers`);
        }
        return value;
    }
    if (Array.isArray(value)) {
        return Object.freeze(value.map((entry, index) => normalizeJsonValue(entry, `${path}[${index}]`)));
    }
    if (typeof value === "object") {
        const record = value                 ;
        return Object.freeze(Object.fromEntries(
            Object.keys(record)
                .sort((left, right) => left.localeCompare(right))
                .map((key) => [key, normalizeJsonValue(record[key] , `${path}.${key}`)]),
        ));
    }
    throw new TypeError(`${path} must be JSON-compatible`);
}

export function normalizeJsonObject(value               , path = "value")                {
    return normalizeJsonValue(value, path)                 ;
}

function sortForSerialization(value         )          {
    if (Array.isArray(value)) {
        return value.map(sortForSerialization);
    }
    if (value !== null && typeof value === "object") {
        const record = value                           ;
        return Object.fromEntries(
            Object.keys(record)
                .sort((left, right) => left.localeCompare(right))
                .map((key) => [key, sortForSerialization(record[key])]),
        );
    }
    return value;
}

export function canonicalJson(value         )         {
    return JSON.stringify(sortForSerialization(value));
}

function canonicalSnapshot(state                 ) {
    return {
        tickIndex: state.tickIndex,
        timeMs: state.timeMs,
        objectCount: state.objectCount,
        entities: state.entities.map((entity) => ({
            stableId: entity.stableId,
            slot: entity.slot,
            rawObjectType: entity.rawObjectType,
            frame: entity.frame,
            waitCounter: entity.waitCounter,
            attacking: entity.attacking,
            facing: entity.facing,
            yInt: entity.yInt,
            hitStop: entity.hitStop,
            killCount: entity.killCount,
            active: entity.active,
        })),
    };
}

export function serializeCanonicalSnapshot(state                 )         {
    return canonicalJson(canonicalSnapshot(state));
}

export function digestCanonicalSnapshot(state                 )         {
    const value = serializeCanonicalSnapshot(state);
    let hash = 0x811c9dc5;
    for (let index = 0; index < value.length; index++) {
        hash ^= value.charCodeAt(index);
        hash = Math.imul(hash, 0x01000193) >>> 0;
    }
    return `fnv1a32:${hash.toString(16).padStart(8, "0")}`;
}

export function serializeTickTrace(trace                     )         {
    return canonicalJson(trace);
}
