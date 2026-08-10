// dat-skill-flow-build:20260810141832204-2dd606079e7245e0be58751ba247b935
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    GATE3B1_OPOINT_RULE,
    createSimulation,
    replaySimulation,
    serializeCanonicalSnapshot,
    serializeTickTrace,
    stepSimulation,
    vrestAt,
} from "../../src/sim/index.js";
import { freezeSimulationState } from "../../src/sim/world.js";
             
               
                  
                       
                        
                    
                                

function opoint(overrides                               = {})                      {
    return { kind: 1, x: 0, y: 0, action: 1, dvx: 6, dvy: -3, oid: 2, facing: 0, ...overrides };
}

function frame(id        , overrides                              = {})                     {
    return { id, state: 1, wait: 100, next: 0, centerx: 0, centery: 0, opoints: [], ...overrides };
}

function dat(oid        , rawObjectType        , frames                               , weaponHp = 0)             {
    return { oid, rawObjectType, weaponHp, frames };
}

function parent(overrides                         = {})                {
    return {
        stableId: "parent",
        slot: 0,
        oid: 1,
        rawObjectType: 0,
        frame: 0,
        attacking: -1,
        frames: [frame(0)],
        ...overrides,
    };
}

function bySlot(state                 , slot        ) {
    return state.slots[slot];
}

describe("Gate3B1 OP-01 fixed world, catalog, and guards", () => {
    it("owns exactly 400 unique runtime slots and rejects out-of-range seeds", () => {
        const state = createSimulation({ entities: [parent()] });
        assert.equal(state.slots.length, 400);
        assert.strictEqual(state.slots[0], state.entities[0]);
        assert.throws(() => createSimulation({ entities: [parent({ slot: 400 })] }), /0\.\.399/);
        assert.throws(() => createSimulation({ entities: [parent({ slot: -1 })] }), /0\.\.399/);
    });

    it("keeps the legacy per-entity frames API through a minimal catalog adapter", () => {
        const state = createSimulation({ entities: [parent({ oid: 77, rawObjectType: 3 })] });
        assert.equal(state.catalog[77]?.oid, 77);
        assert.equal(state.catalog[77]?.rawObjectType, 3);
        assert.deepEqual(state.catalog[77]?.frames, state.entities[0]?.frames);
    });

    it("rejects frame and opoint structures beyond the compiled fixed budgets", () => {
        assert.throws(() => createSimulation({
            entities: [parent({ frames: [frame(600)] })],
        }), /frame\.id.*0\.\.599/);
        assert.throws(() => createSimulation({
            entities: [parent({ frames: [frame(0, {
                opoints: Array.from({ length: 401 }, () => opoint()),
            })] })],
        }), /opoints.*400/);
    });

    it("rejects unsafe canonical clock, spawn ordinal, and coordinate arithmetic", () => {
        assert.throws(() => createSimulation({
            tickIndex: Number.MAX_SAFE_INTEGER,
            entities: [parent()],
        }), /timeMs.*safe integer/);
        const spawning = createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint()] })] })],
        });
        assert.throws(() => stepSimulation(Object.freeze({
            ...spawning,
            nextSpawnOrdinal: Number.MAX_SAFE_INTEGER,
        }), {}), /nextSpawnOrdinal.*safe integer/);
        const unsafeCoordinate = createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({
                xInt: Number.MAX_SAFE_INTEGER,
                frames: [frame(0, { opoints: [opoint({ x: 1 })] })],
            })],
        });
        assert.throws(() => stepSimulation(unsafeCoordinate, {}), /child xInt.*safe integer/);
    });

    it("blocks on absent/invalid first opoint and attacking, while step-4 delay decay and later invalid rows remain visible", () => {
        const child = dat(2, 3, [frame(1)]);
        const run = (overrides                        , ops                                ) => stepSimulation(createSimulation({
            catalog: [child],
            entities: [parent({ ...overrides, frames: [frame(0, { opoints: ops })] })],
        }), {});

        for (const ops of [[], [opoint({ kind: 0 })], [opoint({ oid: 0 })]]) {
            assert.equal(run({}, ops).state.objectCount, 1);
        }
        assert.equal(run({ attacking: 0 }, [opoint()]).state.objectCount, 1);
        assert.equal(run({ frameDelay: 1 }, [opoint()]).state.objectCount, 2, "step4 decrements delay to zero before late frame_tick/opoint");
        const nonCharacterDelay = stepSimulation(createSimulation({
            catalog: [child],
            entities: [parent({
                rawObjectType: 3,
                attacking: 0,
                frameDelay: 1,
                frames: [frame(0, { wait: 0, opoints: [opoint()] })],
            })],
        }), {});
        assert.equal(nonCharacterDelay.state.objectCount, 2, "frameDelay guard is character-DAT-only");
        const partial = run({}, [opoint(), opoint({ kind: 0 }), opoint({ oid: -1 }), opoint({ action: 2 })]);
        assert.equal(partial.state.objectCount, 3);
        assert.deepEqual(partial.trace.spawns.map((event) => event.action), [1, 2]);
    });
});

describe("Gate3B1 OP-02 authoritative child initialization", () => {
    it("implements facing 0/1/other and >10 count/mode decoding", () => {
        const run = (facing        , parentFacing        = 1) => stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({
                facing: parentFacing,
                frames: [frame(0, { opoints: [opoint({ facing })] })],
            })],
        }), {});
        assert.equal(bySlot(run(0).state, 50)?.facing, 1);
        assert.equal(bySlot(run(1).state, 50)?.facing, 0);
        assert.equal(bySlot(run(2).state, 50)?.facing, 0);
        const encoded = run(21, 0);
        assert.equal(encoded.trace.spawns.length, 2);
        assert.deepEqual(encoded.trace.spawns.map((event) => event.facing), [1, 1]);
    });

    it("copies exact position, velocity, ownership, DAT mapping, type-0 chain, and reset defaults", () => {
        const result = stepSimulation(createSimulation({
            catalog: [dat(9, 0, [frame(1, { state: 3000 })], 17)],
            entities: [parent({
                slot: 7,
                team: 4,
                holderCopy: 12,
                killCount: -1,
                hitStop: 8,
                x: 100.75,
                xInt: 100,
                y: -20.25,
                yInt: -20,
                z: 4.75,
                unk364: 6,
                keyUp: true,
                frames: [frame(0, { centerx: 30, centery: 40, opoints: [opoint({ oid: 9, x: 8, y: 5, dvx: 7, dvy: -2 })] })],
            })],
        }), {});
        const child = bySlot(result.state, 50);
        assert.ok(child);
        assert.deepEqual({
            oid: child.oid, rawObjectType: child.rawObjectType, entityType: child.entityType,
            weaponHp: child.weaponHp, ownerId: child.ownerId, holderIdx: child.holderIdx,
            holderCopy: child.holderCopy, spawnerSlot: child.spawnerSlot, team: child.team,
            x: child.x, xInt: child.xInt, y: child.y, yInt: child.yInt, z: child.z, zInt: child.zInt,
            facing: child.facing, vx: child.vx, vy: child.vy, vz: child.vz, unk364: child.unk364,
            killCount: child.killCount, hitStop: child.hitStop, aiControlled: child.aiControlled,
            hp: child.hp, hpMax: child.hpMax, hp3: child.hp3, pp: child.pp,
            targetIdx: child.targetIdx, heldWeaponSlot: child.heldWeaponSlot,
            prevFrame2: child.prevFrame2,
            knockbackVx: child.knockbackVx, knockbackVy: child.knockbackVy, knockbackVz: child.knockbackVz,
            holderDefault: child.holderCopy, unk324: child.unk324, unk328: child.unk328,
            unk32C: child.unk32C, unk33C: child.unk33C,
        }, {
            oid: 9, rawObjectType: 0, entityType: 0, weaponHp: 17,
            ownerId: 7, holderIdx: 7, holderCopy: 12, spawnerSlot: -1, team: 4,
            x: 78, xInt: 78, y: -55, yInt: -55, z: 5.75, zInt: 5,
            facing: 0, vx: 7, vy: -2, vz: -2.5, unk364: 6,
            killCount: 7, hitStop: 8, aiControlled: true,
            hp: 500, hpMax: 500, hp3: 500, pp: 500,
            targetIdx: -1, heldWeaponSlot: -1,
            prevFrame2: 0,
            knockbackVx: 0.1, knockbackVy: 0.1, knockbackVz: 0.1,
            holderDefault: 12, unk324: -1, unk328: -1, unk32C: -1, unk33C: -1,
        });
    });

    it("applies oid 5/52 vitals, initial state Up/Down exceptions, oid211 scaling, and kind2 links", () => {
        const spawn = (oid        , state        , seed                         = {}, kind = 1) => stepSimulation(createSimulation({
            catalog: [dat(oid, 3, [frame(1, { state })])],
            entities: [parent({ ...seed, frames: [frame(0, { opoints: [opoint({ oid, kind })] })] })],
        }), {}).state;
        for (const oid of [5, 52]) {
            const child = bySlot(spawn(oid, 1), 50) ;
            assert.deepEqual([child.hp, child.hpMax, child.hp3, child.pp], [10, 10, 10, 5]);
        }
        assert.equal(bySlot(spawn(211, 3000, { keyDown: true }), 50)?.vz, 0.625);
        assert.equal(bySlot(spawn(223, 3000, { keyUp: true }), 50)?.vz, 0);
        assert.equal(bySlot(spawn(224, 1002, { keyDown: true }), 50)?.vz, 0);
        assert.equal(bySlot(spawn(8, 3006, { keyUp: true, keyDown: true }), 50)?.vz, 0);
        const held = spawn(8, 1, {}, 2);
        assert.deepEqual([bySlot(held, 0)?.linkState, bySlot(held, 0)?.targetIdx, bySlot(held, 0)?.heldWeaponSlot], [1, 50, 50]);
        assert.deepEqual([bySlot(held, 50)?.linkState, bySlot(held, 50)?.holderIdx], [-1, 0]);
        assert.equal(bySlot(held, 50)?.attacking, 0, "held kind2 child returns before same-tick wait progression");
    });
});

describe("Gate3B1 OP-03 allocation, spread, cooldowns, and vrest", () => {
    it("resolves DAT before allocating, uses first free 50..399, and mutates count/lifecycle only on success", () => {
        const missing = stepSimulation(createSimulation({
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ oid: 999 })] })] })],
        }), {});
        assert.equal(missing.state.objectCount, 1);
        assert.deepEqual(missing.trace.slotLifecycle, []);
        assert.deepEqual(missing.trace.spawns, []);

        const occupied = Array.from({ length: 350 }, (_, index) => parent({
            stableId: `occupied-${index}`,
            slot: index + 50,
            oid: 100 + (index % 2),
            rawObjectType: 3,
            active: true,
            frames: [frame(0)],
        }));
        const full = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint()] })] }), ...occupied],
        }), {});
        assert.equal(full.state.objectCount, 351);
        assert.deepEqual(full.trace.slotLifecycle, []);

        occupied[2] = { ...occupied[2] , active: false };
        const partial = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint()] })] }), ...occupied],
        }), {});
        assert.equal(partial.trace.slotLifecycle[0]?.slot, 52);
        assert.equal(partial.state.objectCount, 351);

        const nearlyFull = Array.from({ length: 348 }, (_, index) => parent({
            stableId: `nearly-full-${index}`,
            slot: index + 50,
            oid: 200,
            rawObjectType: 3,
            frames: [frame(0)],
        }));
        const capacityPartial = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ facing: 31 })] })] }), ...nearlyFull],
        }), {});
        assert.deepEqual(capacityPartial.trace.spawns.map((event) => event.slot), [398, 399]);
        assert.equal(capacityPartial.trace.slotLifecycle.length, 2);
        assert.equal(capacityPartial.state.objectCount, 351);
    });

    it("clears polluted reused-slot cooldowns, applies exact multi spread/center exemption, and pairwise vrest", () => {
        const run = (count        ) => stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            attackRest: Object.assign(Array(400).fill(0), { 50: 9 }),
            vrest: [{ fromSlot: 12, toSlot: 50, ticks: 9 }, { fromSlot: 50, toSlot: 13, ticks: 8 }],
            entities: [parent({ stableId: "linked", slot: 12, oid: 12, rawObjectType: 3 }), parent({
                slot: 300,
                rawObjectType: 3,
                animCounter: 12,
                frames: [frame(0, { state: 3003, opoints: [opoint({ facing: count * 10 })] })],
            })],
        }), {});
        for (const [count, expected] of [[2, [0, 0]], [3, [2, 0, 2]], [4, [2, 0, 0, 2]]]         ) {
            const result = run(count);
            const children = result.trace.spawns.map((event) => bySlot(result.state, event.slot) );
            assert.deepEqual(children.map((child) => child.attackExempt), expected);
            assert.equal(children[0]?.vz, -5);
            assert.equal(children.at(-1)?.vz, 5);
            assert.equal(children[0]?.vx, 1);
            assert.equal(children.at(-1)?.vx, 1);
            for (let left = 0; left < children.length; left++) {
                for (let right = left + 1; right < children.length; right++) {
                    assert.equal(vrestAt(result.state, children[left] .slot, children[right] .slot), 0x28);
                    assert.equal(vrestAt(result.state, children[right] .slot, children[left] .slot), 0x28);
                }
            }
            assert.equal(vrestAt(result.state, 12, children[0] .slot), 10);
            assert.equal(vrestAt(result.state, children[0] .slot, 12), 10);
            assert.equal(result.state.attackRest[50], 0);
            assert.equal(vrestAt(result.state, 50, 13), 0);
        }
        const early = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ facing: 30 })] })] })],
        }), {});
        assert.deepEqual(early.trace.spawns.map((event) => bySlot(early.state, event.slot)?.attackExempt), [1, 0, 1]);
    });

    it("bounds allocation attempts for huge safe facing counts without changing successful si spread", () => {
        const hugeFacing = Number.MAX_SAFE_INTEGER;
        const decodedCount = Math.trunc(hugeFacing / 10);
        const source = parent({ frames: [frame(0, { opoints: [opoint({ facing: hugeFacing })] })] });

        let missingAttempts = 0;
        const missing = stepSimulation(createSimulation({ entities: [source] }), {}, {
            onOpointAllocationAttempt: () => { missingAttempts++; },
        });
        assert.equal(missingAttempts, 0, "missing DAT skips the row before allocator work");
        assert.equal(missing.trace.spawns.length, 0);

        const occupied = Array.from({ length: 350 }, (_, index) => parent({
            stableId: `huge-full-${index}`,
            slot: index + 50,
            oid: 300,
            rawObjectType: 3,
            frames: [frame(0)],
        }));
        let fullAttempts = 0;
        const full = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [source, ...occupied],
        }), {}, { onOpointAllocationAttempt: () => { fullAttempts++; } });
        assert.equal(fullAttempts, 1, "first full-slot result terminates the impossible remainder");
        assert.equal(full.trace.spawns.length, 0);

        const nearlyFull = occupied.slice(0, 348);
        let partialAttempts = 0;
        const partial = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [source, ...nearlyFull],
        }), {}, { onOpointAllocationAttempt: () => { partialAttempts++; } });
        assert.equal(partialAttempts, 3, "two successes plus the first impossible full-slot attempt");
        assert.deepEqual(partial.trace.spawns.map((event) => event.slot), [398, 399]);
        for (const [si, event] of partial.trace.spawns.entries()) {
            const child = bySlot(partial.state, event.slot) ;
            const spread = (si * 10) / (decodedCount - 1) - 5;
            assert.equal(child.vz, spread);
            assert.equal(child.vx, -6 + Math.abs(spread));
        }
        assert.equal(partialAttempts <= 351, true);
    });

    it("builds all 350-child vrest pairs with one materialization and bounded mutations", () => {
        let operationCount = 0;
        let materializeCount = 0;
        const result = stepSimulation(createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ facing: 3500 })] })] })],
        }), {}, {
            onOpointVrestOperation: (kind) => {
                operationCount++;
                if (kind === "materialize") materializeCount++;
            },
        });
        assert.equal(result.trace.spawns.length, 350);
        assert.equal(result.state.vrest.length, 350 * 349);
        assert.equal(materializeCount, 1);
        assert.equal(operationCount < 200_000, true);
    });
});

describe("Gate3B1 OP-04 dynamic late slot visibility and empty frames", () => {
    it("processes a later-slot birth in the same tick and defers an earlier-slot birth until next tick", () => {
        const chainCatalog = [
            dat(2, 3, [frame(1, { wait: 0, opoints: [opoint({ oid: 3, action: 1 })] })]),
            dat(3, 3, [frame(1)]),
        ];
        const later = stepSimulation(createSimulation({
            catalog: chainCatalog,
            entities: [parent({ slot: 49, frames: [frame(0, { opoints: [opoint({ oid: 2 })] })] })],
        }), {});
        assert.deepEqual(later.trace.spawns.map((event) => event.slot), [50, 51]);

        const earlier = stepSimulation(createSimulation({
            catalog: chainCatalog,
            entities: [parent({ slot: 300, frames: [frame(0, { opoints: [opoint({ oid: 2 })] })] })],
        }), {});
        assert.deepEqual(earlier.trace.spawns.map((event) => event.slot), [50]);
        const next = stepSimulation(earlier.state, {});
        assert.equal(next.trace.spawns.some((event) => event.parentSlot === 50), true);
    });

    it("uses s_empty for authored-missing action 0..599 and still performs >=400 late cleanup", () => {
        const run = (action        ) => stepSimulation(createSimulation({
            catalog: [dat(2, 3, [])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ action })] })] })],
        }), {});
        const inRange = run(399);
        assert.equal(bySlot(inRange.state, 50)?.active, true);
        assert.equal(bySlot(inRange.state, 50)?.frame, 399);
        assert.equal(bySlot(inRange.state, 50)?.attacking, 1, "s_empty wait=1 is read by same-tick frame_tick");
        const outOfRange = run(400);
        assert.equal(bySlot(outOfRange.state, 50)?.active, false);
        assert.equal(outOfRange.trace.lifecycle.some((event) => event.slot === 50 && event.kind === "free"), true);
        assert.deepEqual(outOfRange.trace.slotLifecycle.map((event) => [event.kind, event.slot]), [
            ["allocate", 50],
            ["release", 50],
        ]);
    });
});

describe("Gate3B1 OP-05 deterministic canonical replay and trace authority", () => {
    it("is byte-identical across replays and exposes allocation/spawn authority IDs", () => {
        const make = () => createSimulation({
            catalog: [dat(2, 3, [frame(1)])],
            entities: [parent({ frames: [frame(0, { opoints: [opoint({ facing: 31 })] })] })],
        });
        const left = replaySimulation(make(), [{ tick: 1 }, { tick: 2 }]);
        const right = replaySimulation(make(), [{ tick: 1 }, { tick: 2 }]);
        assert.equal(serializeCanonicalSnapshot(left.state), serializeCanonicalSnapshot(right.state));
        assert.deepEqual(left.traces.map(serializeTickTrace), right.traces.map(serializeTickTrace));
        assert.equal(left.traces[0]?.slotLifecycle.every((event) => event.kind === "allocate"), true);
        assert.equal(left.traces[0]?.spawns.length, 3);
        assert.equal(left.traces[0]?.ruleIds.includes(GATE3B1_OPOINT_RULE.spawnInitialize), true);
        assert.equal(left.traces[0]?.ruleIds.includes(GATE3B1_OPOINT_RULE.dynamicLateSlots), true);
        assert.equal(new Set(left.state.entities.map((entity) => entity.stableId)).size, left.state.entities.length);
        assert.match(left.state.entities.find((entity) => entity.slot === 50)?.stableId ?? "", /^opoint:\d+:\d+:50$/);
    });

    it("serializes a shared 600-frame DAT payload once for 350 entities and replays identically", () => {
        const sharedFrames = Array.from({ length: 600 }, (_, id) => frame(id, {
            state: id === 599 ? 987_654_321 : 1,
        }));
        const make = (count        , onFrameSourceCanonicalize             ) => createSimulation({
            onFrameSourceCanonicalize,
            entities: Array.from({ length: count }, (_, index) => parent({
                stableId: `shared-${index}`,
                slot: index + 50,
                oid: 2,
                rawObjectType: 3,
                frames: sharedFrames,
            })),
        });
        const one = serializeCanonicalSnapshot(make(1));
        let canonicalizeCount = 0;
        const manyState = make(350, () => { canonicalizeCount++; });
        const many = serializeCanonicalSnapshot(manyState);
        assert.equal(canonicalizeCount, 1, "shared array identity takes the WeakMap fast path");
        assert.equal(many.split("987654321").length - 1, 1, "shared frame payload appears once");
        assert.equal(many.length < one.length + 350 * 1_120, true, "budget includes Gate4 canonical keys and four block flags");
        const left = replaySimulation(manyState, [{}]);
        const right = replaySimulation(make(350), [{}]);
        assert.equal(serializeCanonicalSnapshot(left.state), serializeCanonicalSnapshot(right.state));
        assert.deepEqual(left.traces.map(serializeTickTrace), right.traces.map(serializeTickTrace));
    });

    it("retains content dedupe for distinct frame-array identities", () => {
        const leftFrames = [frame(0), frame(1, { state: 99 })];
        const rightFrames = leftFrames.map((definition) => ({ ...definition }));
        let canonicalizeCount = 0;
        const state = createSimulation({
            onFrameSourceCanonicalize: () => { canonicalizeCount++; },
            entities: [
                parent({ stableId: "left", oid: 2, frames: leftFrames }),
                parent({ stableId: "right", slot: 1, oid: 2, frames: rightFrames }),
            ],
        });
        assert.equal(canonicalizeCount, 2, "distinct identities use the content-comparison fallback");
        assert.equal(state.frameSources.length, 1);
        assert.equal(state.entities[0] .frameSourceIndex, state.entities[1] .frameSourceIndex);
    });

    it("preserves frozen aggregate references while replacing entities, including dense vrest", () => {
        const denseVrest = Array.from({ length: 350 }, (_, fromSlot) => (
            Array.from({ length: 349 }, (_, offset) => {
                const toSlot = offset >= fromSlot ? offset + 1 : offset;
                return { fromSlot, toSlot, ticks: 40 };
            })
        )).flat();
        const initial = createSimulation({
            entities: [parent({ frames: [frame(0)] })],
            vrest: denseVrest,
        });
        const refrozen = freezeSimulationState(initial);
        assert.strictEqual(refrozen.slots, initial.slots);
        assert.strictEqual(refrozen.entities, initial.entities);
        assert.strictEqual(refrozen.catalog, initial.catalog);
        assert.strictEqual(refrozen.frameSources, initial.frameSources);
        assert.strictEqual(refrozen.attackRest, initial.attackRest);
        assert.strictEqual(refrozen.vrest, initial.vrest);
        const result = stepSimulation(initial, {});
        assert.equal(result.state.vrest.length, 350 * 349);
        assert.strictEqual(result.state.vrest, initial.vrest);
        assert.strictEqual(result.state.attackRest, initial.attackRest);
        assert.strictEqual(result.state.catalog, initial.catalog);
        assert.strictEqual(result.state.frameSources, initial.frameSources);
    });
});
