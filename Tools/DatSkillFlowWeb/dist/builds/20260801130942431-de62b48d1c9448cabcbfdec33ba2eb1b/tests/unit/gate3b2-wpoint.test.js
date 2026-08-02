// dat-skill-flow-build:20260801130942431-de62b48d1c9448cabcbfdec33ba2eb1b
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    applyPickupInputs,
    canonicalJson,
    createSimulation,
    forceDropHeldWeapon,
    nextNtsdRandom,
    normalizeFrames,
    normalizeJsonObject,
    parsePickupInputs,
    replaySimulation,
    resolveHeldAttackPayload,
    runHeldObjectPass,
    serializeCanonicalSnapshot,
    serializeTickTrace,
    stepSimulation,
    validateHeldWeaponCaches,
    validatePositiveLinks,
} from "../../src/sim/index.js";
             
                  
                       
                     
                        
                                

function wpoint(overrides                               = {})                      {
    return {
        kind: 0,
        x: 0,
        y: 0,
        attacking: 0,
        cover: 0,
        weaponact: 0,
        dvx: 0,
        dvy: 0,
        dvz: 0,
        ...overrides,
    };
}

function itr(overrides                            = {})                   {
    return {
        kind: 0, x: 0, y: 0, w: 0, h: 0,
        dvx: 0, dvy: 0, fall: 0, bdefend: 0, injury: 0,
        arest: 0, vrest: 0, effect: 0, attacking: 0,
        catchingact: 0, catchingact2: 0, caughtact: 0, caughtact2: 0,
        respond: 0, pickingact: 0, pickedact: 0,
        throwvx: 0, throwvy: 0, zwidth: 15, throwvz: 0, throwinjury: 0,
        ...overrides,
    };
}

function frame(id        , overrides                              = {})                     {
    return { id, state: 0, wait: 100, next: 0, ...overrides };
}

function entity(stableId        , slot        , overrides                         = {})                {
    return { stableId, slot, oid: slot + 1, rawObjectType: 0, frame: 0, frames: [frame(0)], ...overrides };
}

describe("Gate3B2 wpoint data and held attack payload", () => {
    it("normalizes ordered wpoints with exact nine integer fields and zero defaults", () => {
        const [normalized] = normalizeFrames([frame(0, {
            wpoints: [wpoint({ x: 4 }), { kind: 3 }                       ],
        })]);
        assert.deepEqual(normalized?.wpoints, [
            wpoint({ x: 4 }),
            wpoint({ kind: 3 }),
        ]);
        assert.deepEqual(Object.keys(normalized?.wpoints?.[0] ?? {}), [
            "kind", "x", "y", "attacking", "cover", "weaponact", "dvx", "dvy", "dvz",
        ]);
        assert.throws(() => normalizeFrames([frame(0, {
            wpoints: [wpoint({ dvz: Number.NaN })],
        })]), /wpoint\.dvz.*safe integer/);
    });

    it("rejects wpoint and itr rows beyond the fixed per-frame simulation boundary before mapping", () => {
        const oversizedWpoints = Array.from({ length: 401 }, () => wpoint());
        const oversizedItrs = Array.from({ length: 401 }, () => itr());
        Object.defineProperty(oversizedWpoints, 0, { get: () => { throw new Error("wpoint row accessed"); } });
        Object.defineProperty(oversizedItrs, 0, { get: () => { throw new Error("itr row accessed"); } });
        assert.throws(() => normalizeFrames([frame(0, { wpoints: oversizedWpoints })]), /wpoints.*400/);
        assert.throws(() => normalizeFrames([frame(0, { itrs: oversizedItrs })]), /itrs.*400/);
    });

    it("implements the canonical uint32 LCG and 15-bit output", () => {
        let sample = nextNtsdRandom(0);
        assert.deepEqual(sample, { seed: 0x269ec3, value: 38 });
        sample = nextNtsdRandom(sample.seed);
        assert.deepEqual(sample, { seed: 0x1e278e7a, value: 7719 });
        assert.throws(() => nextNtsdRandom(-1), /uint32/);
        assert.throws(() => nextNtsdRandom(0x1_0000_0000), /uint32/);
    });

    it("uses the compiled entity reset defaults for all three knockback components", () => {
        const created = createSimulation({ entities: [entity("reset", 0)] }).slots[0] ;
        assert.deepEqual(
            [created.knockbackVx, created.knockbackVy, created.knockbackVz],
            [0.1, 0.1, 0.1],
        );
    });

    it("selects the exact zero-based holder previous-frame itr payload while preserving held geometry", () => {
        const state = createSimulation({ entities: [
            entity("holder", 3, {
                prevFrame2: 7,
                targetIdx: 9,
                frames: [frame(0), frame(7, {
                    wpoints: [wpoint({ attacking: 1 })],
                    itrs: [itr({ injury: 11 }), itr({ injury: 77, dvx: 8, throwinjury: 9 })],
                })],
            }),
            entity("held", 9, { rawObjectType: 1, linkState: -1, holderIdx: 3 }),
            entity("victim", 12),
        ] });
        const heldItr = itr({ kind: 5, x: 10, y: 11, w: 12, h: 13, injury: 1 });
        const resolved = resolveHeldAttackPayload(
            heldItr,
            state.slots[9] ,
            state.slots[3] ,
            12,
        );
        assert.deepEqual(
            { kind: resolved?.kind, x: resolved?.x, y: resolved?.y, w: resolved?.w, h: resolved?.h,
                injury: resolved?.injury, dvx: resolved?.dvx, throwinjury: resolved?.throwinjury },
            { kind: 0, x: 10, y: 11, w: 12, h: 13, injury: 77, dvx: 8, throwinjury: 9 },
        );
    });

    it("leaves invalid outer selectors unchanged and still forces kind zero after a valid out-of-range selector", () => {
        const source = itr({ kind: 5, x: 8, injury: 17 });
        const invalid = createSimulation({ entities: [
            entity("holder", 1, { targetIdx: 2, prevFrame2: 0 }),
            entity("held", 2, { linkState: 0, holderIdx: 1 }),
        ] });
        assert.strictEqual(resolveHeldAttackPayload(source, invalid.slots[2] , invalid.slots[1] , 3), source);

        const selected = createSimulation({ entities: [
            entity("holder", 1, {
                targetIdx: 2,
                prevFrame2: 0,
                frames: [frame(0, { wpoints: [wpoint({ attacking: 9 })], itrs: [itr({ injury: 99 })] })],
            }),
            entity("held", 2, { linkState: -1, holderIdx: 1 }),
        ] });
        const resolved = resolveHeldAttackPayload(source, selected.slots[2] , selected.slots[1] , 3);
        assert.notStrictEqual(resolved, source);
        assert.deepEqual(resolved, { ...source, kind: 0 });
    });
});

describe("Gate3B2 held passes and releases", () => {
    it("uses s_empty for authored-missing holder and selected held frames while preserving out-of-range undefined behavior", () => {
        const missing = createSimulation({ entities: [
            entity("holder", 1, {
                frame: 5, targetIdx: 2, xInt: 40, yInt: 20, zInt: 7, facing: 1, frameDelay: -3,
                frames: [frame(1)],
            }),
            entity("held", 2, {
                rawObjectType: 1, frame: 4, linkState: -1, holderIdx: 1,
                frames: [frame(2)],
            }),
        ] });
        const result = runHeldObjectPass(missing, 5);
        assert.deepEqual(
            { frame: result.state.slots[2]?.frame, facing: result.state.slots[2]?.facing,
                frameDelay: result.state.slots[2]?.frameDelay, xInt: result.state.slots[2]?.xInt,
                yInt: result.state.slots[2]?.yInt, zInt: result.state.slots[2]?.zInt },
            { frame: 0, facing: 1, frameDelay: -3, xInt: 40, yInt: 19, zInt: 8 },
        );

        const outOfRange = createSimulation({ entities: [
            entity("holder", 1, { frame: 600, targetIdx: 2, frames: [frame(1)] }),
            entity("held", 2, { rawObjectType: 1, frame: 4, linkState: -1, holderIdx: 1, frames: [frame(2)] }),
        ] });
        assert.equal(runHeldObjectPass(outOfRange, 5).state.slots[2]?.frame, 4);
    });

    it("uses exact integer anchor arithmetic and rejects only a truly out-of-range final coordinate", () => {
        const exact = createSimulation({ entities: [
            entity("holder", 1, {
                targetIdx: 2,
                xInt: Number.MAX_SAFE_INTEGER,
                frames: [frame(0, { centerx: -2, wpoints: [wpoint({ x: -3 })] })],
            }),
            entity("held", 2, { rawObjectType: 1, linkState: -1, holderIdx: 1 }),
        ] });
        assert.equal(runHeldObjectPass(exact, 5).state.slots[2]?.xInt, 9_007_199_254_740_990);

        const overflow = createSimulation({ entities: [
            entity("holder", 1, {
                targetIdx: 2,
                xInt: Number.MAX_SAFE_INTEGER,
                frames: [frame(0, { centerx: -1, wpoints: [wpoint({ x: 1 })] })],
            }),
            entity("held", 2, { rawObjectType: 1, linkState: -1, holderIdx: 1 }),
        ] });
        assert.throws(() => runHeldObjectPass(overflow, 5), /held sync xInt.*safe integer/);
    });
    it("runs injected pickup between pass 5 and pass 12, then syncs exact integer anchors from first wpoints", () => {
        const initial = createSimulation({ entities: [
            entity("holder", 0, {
                xInt: 100, yInt: 50, zInt: 10,
                frames: [frame(0), frame(115, {
                    centerx: 10, centery: 20,
                    wpoints: [wpoint({ x: 3, y: 7, weaponact: 8 }), wpoint({ x: 999, weaponact: 9 })],
                })],
            }),
            entity("held", 8, {
                rawObjectType: 1,
                frames: [frame(0), frame(8, {
                    centerx: 5, centery: 6,
                    wpoints: [wpoint({ x: 2, y: 4 })],
                })],
            }),
        ] });
        const result = stepSimulation(initial, { pickups: [{ kind: 2, pickerSlot: 0, weaponSlot: 8 }] });
        const held = result.state.slots[8] ;
        assert.deepEqual(
            { frame: held.frame, facing: held.facing, frameDelay: held.frameDelay,
                xInt: held.xInt, yInt: held.yInt, zInt: held.zInt, x: held.x, y: held.y, z: held.z },
            { frame: 8, facing: 0, frameDelay: 0, xInt: 96, yInt: 38, zInt: 11, x: 96, y: 38, z: 11 },
        );
        assert.deepEqual(result.trace.heldObjects.map((entry) => [entry.kind, entry.pass]), [
            ["pickup", null], ["sync", 12],
        ]);
    });

    it("clears only the negative held link when its holder relation is invalid", () => {
        const initial = createSimulation({ entities: [entity("held", 4, {
            rawObjectType: 1, linkState: -4, holderIdx: 99, targetIdx: 7, holderCopy: 12,
        })] });
        const result = runHeldObjectPass(initial, 5);
        assert.deepEqual(
            { linkState: result.state.slots[4]?.linkState, holderIdx: result.state.slots[4]?.holderIdx,
                targetIdx: result.state.slots[4]?.targetIdx, holderCopy: result.state.slots[4]?.holderCopy },
            { linkState: 0, holderIdx: 99, targetIdx: 7, holderCopy: 12 },
        );
    });

    it("applies state release, type-2 dvx release, then independent kind-3 RNG in exact call order", () => {
        const initial = createSimulation({ rngSeed: 0, entities: [
            entity("holder", 2, {
                targetIdx: 6, heldWeaponSlot: 6, throwFrameGuard: 44,
                yInt: -10,
                hitCount: 1, knockbackVx: 12, knockbackVy: -7, knockbackVz: 3,
                frames: [frame(0, { wpoints: [wpoint({ kind: 3, weaponact: 0, dvx: 8, dvy: -5, dvz: 9 })] })],
            }),
            entity("held", 6, {
                rawObjectType: 2, linkState: -2, holderIdx: 2,
                y: -9, yInt: -9, vz: 17,
                frames: [frame(0, { state: 10 })],
            }),
        ] });
        const result = runHeldObjectPass(initial, 5);
        let seed = 0;
        const values           = [];
        for (let index = 0; index < 6; index++) {
            const sample = nextNtsdRandom(seed);
            seed = sample.seed;
            values.push(sample.value);
        }
        const held = result.state.slots[6] ;
        const holder = result.state.slots[2] ;
        assert.deepEqual(
            { frame: held.frame, vx: held.vx, vy: held.vy, vz: held.vz, y: held.y, yInt: held.yInt,
                linkState: held.linkState, holderLink: holder.linkState, targetIdx: holder.targetIdx,
                holderIdx: held.holderIdx, heldWeaponSlot: holder.heldWeaponSlot, throwFrameGuard: holder.throwFrameGuard },
            { frame: values[2]  % 6, vx: values[3]  % 7 - 3, vy: -(values[4]  % 4),
                vz: (values[5]  % 5 - 2) * 0.2, y: -2, yInt: -11,
                linkState: 0, holderLink: 0, targetIdx: 6,
                holderIdx: 2, heldWeaponSlot: -1, throwFrameGuard: -1 },
        );
        assert.equal(result.state.rngSeed, seed);
        assert.deepEqual(result.events.filter((entry) => entry.kind === "release").map((entry) => entry.detail), [
            "state-10", "type-2", "kind-3",
        ]);
    });

    it("preserves stale depth velocity when directional input is neither or both", () => {
        for (const keys of [{ keyUp: false, keyDown: false }, { keyUp: true, keyDown: true }]) {
            const initial = createSimulation({ entities: [
                entity("holder", 1, {
                    targetIdx: 2, heldWeaponSlot: 2, ...keys,
                    frames: [frame(0, { wpoints: [wpoint({ weaponact: 0, dvx: 3, dvz: 9 })] })],
                }),
                entity("held", 2, { rawObjectType: 1, linkState: -1, holderIdx: 1, vz: 27 }),
            ] });
            assert.equal(runHeldObjectPass(initial, 5).state.slots[2]?.vz, 27);
        }
    });
});

describe("Gate3B2 pickup, link validation, and force drop", () => {
    it("maps kind 2 for types 1/2/4/6 and kind 7 special links without geometry", () => {
        const cases = [
            { type: 1, hp: 1, frame: 115, link: 1, heldLink: -1 },
            { type: 2, hp: 1, frame: 116, link: 2, heldLink: -2 },
            { type: 4, hp: 1, frame: 115, link: 4, heldLink: -4 },
            { type: 6, hp: 1, frame: 115, link: 6, heldLink: -6 },
            { type: 6, hp: 0, frame: 115, link: 4, heldLink: -4 },
        ];
        for (const expected of cases) {
            const initial = createSimulation({ entities: [
                entity("picker", 0),
                entity("weapon", 5, { rawObjectType: expected.type, hp: expected.hp }),
            ] });
            const result = applyPickupInputs(initial, [{ kind: 2, pickerSlot: 0, weaponSlot: 5 }]);
            assert.deepEqual(
                { frame: result.state.slots[0]?.frame, link: result.state.slots[0]?.linkState,
                    heldLink: result.state.slots[5]?.linkState },
                { frame: expected.frame, link: expected.link, heldLink: expected.heldLink },
            );
        }
        for (const expected of [
            { type: 1, hp: 1, oid: 0x78, link: 101, heldLink: -1 },
            { type: 2, hp: 1, oid: 2, link: 1, heldLink: -1 },
            { type: 4, hp: 1, oid: 4, link: 4, heldLink: -4 },
            { type: 6, hp: 1, oid: 6, link: 6, heldLink: -6 },
            { type: 6, hp: 0, oid: 6, link: 4, heldLink: -4 },
        ]) {
            const special = createSimulation({ entities: [
                entity("picker", 0),
                entity("weapon", 5, { oid: expected.oid, rawObjectType: expected.type, hp: expected.hp, targetIdx: 77 }),
            ] });
            const result = applyPickupInputs(special, [{ kind: 7, pickerSlot: 0, weaponSlot: 5 }]);
            assert.equal(result.state.slots[0]?.linkState, expected.link);
            assert.equal(result.state.slots[5]?.linkState, expected.heldLink);
            assert.equal(result.state.slots[5]?.targetIdx, 77, "collision.cpp kind 7 leaves held targetIdx stale");
        }
    });

    it("strictly validates the narrow injected pickup envelope", () => {
        assert.deepEqual(parsePickupInputs({ pickups: [{ kind: 2, pickerSlot: 0, weaponSlot: 399 }] }), [
            { kind: 2, pickerSlot: 0, weaponSlot: 399 },
        ]);
        assert.throws(() => parsePickupInputs({ pickups: [{ kind: 3, pickerSlot: 0, weaponSlot: 1 }]          }), /kind.*2 or 7/);
        assert.throws(() => parsePickupInputs({ pickups: [{ kind: 2, pickerSlot: 0, weaponSlot: 400 }]          }), /0\.\.399/);
        assert.throws(() => parsePickupInputs({ pickups: [{ kind: 2, pickerSlot: 0, weaponSlot: 1, extra: 1 }]          }), /exactly/);
        const oversized = new Array(401);
        Object.defineProperty(oversized, 0, { get: () => { throw new Error("pickup row accessed"); } });
        assert.throws(() => parsePickupInputs({ pickups: oversized          }), /at most 400/);
        assert.throws(() => stepSimulation(createSimulation({ entities: [] }), { pickups: oversized          }), /at most 400/);
    });

    it("accepts the 400-row host seam maximum sequentially without inventing a kind-2 already-linked guard", () => {
        const initial = createSimulation({ entities: [
            entity("picker", 0, { frames: [frame(0), frame(115)] }),
            entity("weapon", 5, { rawObjectType: 1 }),
        ] });
        const pickups = Array.from({ length: 400 }, () => ({ kind: 2         , pickerSlot: 0, weaponSlot: 5 }));
        const pure = applyPickupInputs(initial, pickups);
        assert.equal(pure.events.length, 400);
        assert.equal(pure.state.slots[0]?.pickupCount, 400);
        assert.strictEqual(pure.state.entities.find((candidate) => candidate.slot === 0), pure.state.slots[0]);

        const stepped = stepSimulation(initial, { pickups });
        assert.equal(stepped.trace.inputs.pickups?.length, 400);
        assert.equal(stepped.trace.heldObjects.filter((entry) => entry.kind === "pickup").length, 400);
    });

    it("limits cache and positive-link validation to their exact fields", () => {
        const initial = createSimulation({ entities: [
            entity("holder", 1, {
                linkState: 4, targetIdx: 5, heldWeaponSlot: 5, throwFrameGuard: 8, holderIdx: 77,
            }),
            entity("weapon", 5, { rawObjectType: 4, linkState: 0, holderIdx: 99 }),
        ] });
        const cached = validateHeldWeaponCaches(initial).state;
        assert.deepEqual(
            { heldWeaponSlot: cached.slots[1]?.heldWeaponSlot, throwFrameGuard: cached.slots[1]?.throwFrameGuard,
                linkState: cached.slots[1]?.linkState, targetIdx: cached.slots[1]?.targetIdx },
            { heldWeaponSlot: -1, throwFrameGuard: -1, linkState: 4, targetIdx: 5 },
        );
        const linked = validatePositiveLinks(cached).state;
        assert.deepEqual(
            { linkState: linked.slots[1]?.linkState, targetIdx: linked.slots[1]?.targetIdx,
                holderIdx: linked.slots[1]?.holderIdx },
            { linkState: 0, targetIdx: 5, holderIdx: 77 },
        );
    });

    it("validates a held cache against s_empty when the holder frame is authored-missing", () => {
        const initial = createSimulation({ entities: [
            entity("holder", 1, { frame: 5, heldWeaponSlot: 4, throwFrameGuard: 9, frames: [frame(0)] }),
            entity("weapon", 4, { rawObjectType: 1, linkState: 0, holderIdx: 99 }),
        ] });
        const result = validateHeldWeaponCaches(initial).state;
        assert.equal(result.slots[1]?.heldWeaponSlot, -1);
        assert.equal(result.slots[1]?.throwFrameGuard, -1);
    });

    it("exports exact force-drop cleanup but leaves scheduling outside this gate", () => {
        const initial = createSimulation({ entities: [
            entity("holder", 1, { linkState: 4, targetIdx: 5, heldWeaponSlot: 5, throwFrameGuard: 9 }),
            entity("weapon", 5, {
                rawObjectType: 4, linkState: -4, targetIdx: 1, holderIdx: 1, holderCopy: 1,
                catcherIdx: 8, caughtIdx: 7, caughtDuration: 6, vx: 12,
            }),
        ] });
        const dropped = forceDropHeldWeapon(initial, 1);
        assert.deepEqual(
            { holderLink: dropped.slots[1]?.linkState, holderTarget: dropped.slots[1]?.targetIdx,
                heldWeaponSlot: dropped.slots[1]?.heldWeaponSlot, guard: dropped.slots[1]?.throwFrameGuard,
                heldLink: dropped.slots[5]?.linkState, heldTarget: dropped.slots[5]?.targetIdx,
                holderIdx: dropped.slots[5]?.holderIdx, holderCopy: dropped.slots[5]?.holderCopy,
                catcherIdx: dropped.slots[5]?.catcherIdx, caughtIdx: dropped.slots[5]?.caughtIdx,
                caughtDuration: dropped.slots[5]?.caughtDuration, vx: dropped.slots[5]?.vx },
            { holderLink: 0, holderTarget: -1, heldWeaponSlot: -1, guard: -1,
                heldLink: 0, heldTarget: -1, holderIdx: -1, holderCopy: -1,
                catcherIdx: -1, caughtIdx: -1, caughtDuration: 0, vx: 6 },
        );
    });

    it("keeps pickup input and held events canonical and replay-identical", () => {
        const initial = createSimulation({ rngSeed: 0x1234, entities: [
            entity("holder", 0, { frames: [frame(0), frame(115, { wpoints: [wpoint()] })] }),
            entity("weapon", 2, { rawObjectType: 1 }),
        ] });
        const input = { pickups: [{ kind: 2         , pickerSlot: 0, weaponSlot: 2 }] };
        const stepped = stepSimulation(initial, input);
        const replayed = replaySimulation(initial, [input]);
        assert.equal(serializeCanonicalSnapshot(stepped.state), serializeCanonicalSnapshot(replayed.state));
        assert.equal(serializeTickTrace(stepped.trace), serializeTickTrace(replayed.traces[0] ));
        assert.deepEqual(stepped.trace.inputs.pickups, input.pickups);
        assert.ok(stepped.trace.heldObjects.every((entry) => entry.ruleId.startsWith("sim.wpoint.")));
    });
});

describe("Gate3B2 bounded canonical JSON", () => {
    it("rejects 15k nesting and cycles with controlled errors in normalization and canonical serialization", () => {
        let deep          = null;
        for (let index = 0; index < 15_000; index++) deep = [deep];
        for (const operation of [
            () => normalizeJsonObject({ deep }         ),
            () => canonicalJson({ deep }),
        ]) {
            assert.throws(operation, (error         ) => (
                error instanceof RangeError && /depth/i.test(error.message) && !/call stack/i.test(error.message)
            ));
        }

        const cyclic                     = {};
        cyclic.self = cyclic;
        assert.throws(() => normalizeJsonObject(cyclic         ), /cycle/i);
        assert.throws(() => canonicalJson(cyclic), /cycle/i);
        assert.throws(() => canonicalJson({ value: Number.POSITIVE_INFINITY }), /finite/i);
    });

    it("rejects a flat node-budget attack before accessing its rows and keeps normal traces canonical", () => {
        const tooWide = new Array(1_000_001);
        Object.defineProperty(tooWide, 0, { get: () => { throw new Error("node accessed"); } });
        assert.throws(() => normalizeJsonObject({ values: tooWide }         ), /node budget/i);
        assert.throws(() => canonicalJson({ values: tooWide }), /node budget/i);

        const stepped = stepSimulation(createSimulation({ entities: [entity("normal", 0)] }), { nested: { ok: [1, 2, 3] } });
        assert.doesNotThrow(() => serializeTickTrace(stepped.trace));
    });

    it("rejects sparse arrays instead of letting holes bypass the global node budget", () => {
        const sparseAggregate = [new Array(600_000), new Array(600_000)];
        for (const operation of [
            () => normalizeJsonObject({ values: sparseAggregate }         ),
            () => canonicalJson({ values: sparseAggregate }),
        ]) {
            assert.throws(operation, (error         ) => (
                (error instanceof TypeError || error instanceof RangeError)
                && /sparse|node budget/i.test(error.message)
            ));
        }

        const sparse = new Array(2);
        sparse[0] = 1;
        const interiorHole = [1, 2, 3];
        delete interiorHole[1];
        for (const value of [sparse, interiorHole]) {
            assert.throws(() => normalizeJsonObject({ value }         ), /sparse/i);
            assert.throws(() => canonicalJson({ value }), /sparse/i);
        }

        assert.equal(canonicalJson({ value: [1, null, 3] }), '{"value":[1,null,3]}');
    });
});
