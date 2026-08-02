// dat-skill-flow-build:20260801084329830-d8fd48a36eaf43f0aebf5d7f4b383029
             
                  
                 
                    
                        
                    

export function compareUtf16CodeUnits(left        , right        )         {
    const sharedLength = Math.min(left.length, right.length);
    for (let index = 0; index < sharedLength; index++) {
        const difference = left.charCodeAt(index) - right.charCodeAt(index);
        if (difference !== 0) {
            return difference;
        }
    }
    return left.length - right.length;
}

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
                .sort(compareUtf16CodeUnits)
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
                .sort(compareUtf16CodeUnits)
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
        nextSpawnOrdinal: state.nextSpawnOrdinal,
        worldInput: state.worldInput,
        catalog: state.catalog.filter((entry) => entry !== null),
        attackRest: state.attackRest,
        vrest: state.vrest,
        entities: state.entities.map((entity) => ({
            stableId: entity.stableId,
            slot: entity.slot,
            rawObjectType: entity.rawObjectType,
            oid: entity.oid,
            runtimeObjectType: entity.runtimeObjectType,
            entityType: entity.entityType,
            weaponHp: entity.weaponHp,
            frame: entity.frame,
            hp: entity.hp,
            hpMax: entity.hpMax,
            hp3: entity.hp3,
            pp: entity.pp,
            comboCountVic: entity.comboCountVic,
            ppDisplay: entity.ppDisplay,
            waitCounter: entity.waitCounter,
            attacking: entity.attacking,
            facing: entity.facing,
            x: entity.x,
            y: entity.y,
            z: entity.z,
            xInt: entity.xInt,
            yInt: entity.yInt,
            zInt: entity.zInt,
            vx: entity.vx,
            vy: entity.vy,
            vz: entity.vz,
            team: entity.team,
            ownerId: entity.ownerId,
            holderIdx: entity.holderIdx,
            holderCopy: entity.holderCopy,
            spawnerSlot: entity.spawnerSlot,
            targetIdx: entity.targetIdx,
            heldWeaponSlot: entity.heldWeaponSlot,
            aiControlled: entity.aiControlled,
            keyUp: entity.keyUp,
            keyDown: entity.keyDown,
            unk364: entity.unk364,
            hitStop: entity.hitStop,
            frameDelay: entity.frameDelay,
            killCount: entity.killCount,
            cooldowns: entity.cooldowns,
            combos: entity.combos,
            linkState: entity.linkState,
            unk324: entity.unk324,
            unk328: entity.unk328,
            unk32C: entity.unk32C,
            unk33C: entity.unk33C,
            unk338: entity.unk338,
            animCounter: entity.animCounter,
            attackExempt: entity.attackExempt,
            active: entity.active,
            frames: entity.frames,
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
