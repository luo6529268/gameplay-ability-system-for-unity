import type {
    SimJsonObject,
    SimJsonValue,
    SimulationState,
    SimulationTickTrace,
} from "./types.js";

export function compareUtf16CodeUnits(left: string, right: string): number {
    const sharedLength = Math.min(left.length, right.length);
    for (let index = 0; index < sharedLength; index++) {
        const difference = left.charCodeAt(index) - right.charCodeAt(index);
        if (difference !== 0) {
            return difference;
        }
    }
    return left.length - right.length;
}

export const JSON_MAX_DEPTH = 100;
export const JSON_MAX_NODES = 1_000_000;

interface JsonTraversalContext {
    nodes: number;
    readonly ancestors: Set<object>;
    readonly freeze: boolean;
}

function transformJsonValue(
    value: unknown,
    path: string,
    depth: number,
    context: JsonTraversalContext,
    nodeReserved = false,
): SimJsonValue {
    if (depth > JSON_MAX_DEPTH) {
        throw new RangeError(`${path} exceeds the JSON depth limit of ${JSON_MAX_DEPTH}`);
    }
    if (!nodeReserved) {
        context.nodes++;
        if (context.nodes > JSON_MAX_NODES) {
            throw new RangeError(`${path} exceeds the JSON node budget of ${JSON_MAX_NODES}`);
        }
    }
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
        if (value.length > JSON_MAX_NODES - context.nodes) {
            throw new RangeError(`${path} exceeds the JSON node budget of ${JSON_MAX_NODES}`);
        }
        context.nodes += value.length;
        for (let index = 0; index < value.length; index++) {
            if (!Object.hasOwn(value, index)) {
                throw new TypeError(`${path}[${index}] is a sparse array hole`);
            }
        }
        if (context.ancestors.has(value)) throw new TypeError(`${path} contains a cycle`);
        context.ancestors.add(value);
        try {
            const result: SimJsonValue[] = new Array(value.length);
            for (let index = 0; index < value.length; index++) {
                result[index] = transformJsonValue(value[index], `${path}[${index}]`, depth + 1, context, true);
            }
            return context.freeze ? Object.freeze(result) : result;
        } finally {
            context.ancestors.delete(value);
        }
    }
    if (typeof value === "object") {
        if (context.ancestors.has(value)) throw new TypeError(`${path} contains a cycle`);
        const record = value as Record<string, unknown>;
        const keys = Object.keys(record).sort(compareUtf16CodeUnits);
        if (keys.length > JSON_MAX_NODES - context.nodes) {
            throw new RangeError(`${path} exceeds the JSON node budget of ${JSON_MAX_NODES}`);
        }
        context.ancestors.add(value);
        try {
            const result = Object.fromEntries(keys.map((key) => [
                key,
                transformJsonValue(record[key], `${path}.${key}`, depth + 1, context),
            ]));
            return context.freeze ? Object.freeze(result) : result;
        } finally {
            context.ancestors.delete(value);
        }
    }
    throw new TypeError(`${path} must be JSON-compatible`);
}

export function normalizeJsonObject(value: SimJsonObject, path = "value"): SimJsonObject {
    return transformJsonValue(value, path, 0, {
        nodes: 0,
        ancestors: new Set(),
        freeze: true,
    }) as SimJsonObject;
}

function sortForSerialization(value: unknown): unknown {
    return transformJsonValue(value, "value", 0, {
        nodes: 0,
        ancestors: new Set(),
        freeze: false,
    });
}

export function canonicalJson(value: unknown): string {
    return JSON.stringify(sortForSerialization(value));
}

function canonicalSnapshot(state: SimulationState) {
    return {
        tickIndex: state.tickIndex,
        timeMs: state.timeMs,
        objectCount: state.objectCount,
        nextSpawnOrdinal: state.nextSpawnOrdinal,
        rngSeed: state.rngSeed,
        worldInput: state.worldInput,
        frameSources: state.frameSources,
        catalog: state.catalog
            .filter((entry) => entry !== null)
            .map((entry) => ({
                oid: entry.oid,
                rawObjectType: entry.rawObjectType,
                weaponHp: entry.weaponHp,
                frameSourceIndex: entry.frameSourceIndex,
                jumpHeight: entry.jumpHeight,
                jumpDistance: entry.jumpDistance,
                jumpDistanceZ: entry.jumpDistanceZ,
            })),
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
            prevFrame2: entity.prevFrame2,
            hitCount: entity.hitCount,
            knockbackVx: entity.knockbackVx,
            knockbackVy: entity.knockbackVy,
            knockbackVz: entity.knockbackVz,
            throwFrameGuard: entity.throwFrameGuard,
            pickupCount: entity.pickupCount,
            catcherIdx: entity.catcherIdx,
            caughtIdx: entity.caughtIdx,
            caughtDuration: entity.caughtDuration,
            fall: entity.fall,
            unk31C: entity.unk31C,
            aiControlled: entity.aiControlled,
            keyUp: entity.keyUp,
            keyDown: entity.keyDown,
            keyLeft: entity.keyLeft,
            keyRight: entity.keyRight,
            blockBackZ: entity.blockBackZ,
            blockForwardZ: entity.blockForwardZ,
            blockLeft: entity.blockLeft,
            blockRight: entity.blockRight,
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
            frameSourceIndex: entity.frameSourceIndex,
        })),
    };
}

export function serializeCanonicalSnapshot(state: SimulationState): string {
    return canonicalJson(canonicalSnapshot(state));
}

export function digestCanonicalSnapshot(state: SimulationState): string {
    const value = serializeCanonicalSnapshot(state);
    let hash = 0x811c9dc5;
    for (let index = 0; index < value.length; index++) {
        hash ^= value.charCodeAt(index);
        hash = Math.imul(hash, 0x01000193) >>> 0;
    }
    return `fnv1a32:${hash.toString(16).padStart(8, "0")}`;
}

export function serializeTickTrace(trace: SimulationTickTrace): string {
    return canonicalJson(trace);
}
