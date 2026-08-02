// dat-skill-flow-build:20260801073730398-4b7dfd972a454bf489e4a867bb047a9b
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    DAT_INPUT_COOLDOWN_KEY_MAP,
    createSimulation,
    doFrameJump,
    postCooldownInput,
    serializeCanonicalSnapshot,
    stepSimulation,
} from "../../src/sim/index.js";
             
              
                  
                       
                       
                                

const world                     = Object.freeze({ ppMode: 1, oid6DjaGuard: 0 });

function frame(id        , overrides                              = {})                     {
    return { id, state: 1, wait: 100, next: 0, ...overrides };
}

function seed(overrides                         = {})                {
    return {
        stableId: "input-entity",
        slot: 0,
        rawObjectType: 0,
        oid: 2,
        frame: 0,
        hp: 500,
        pp: 500,
        frames: [frame(0)],
        ...overrides,
    };
}

function canonicalEntity(overrides                         = {})            {
    const entity = createSimulation({ entities: [seed(overrides)] }).entities[0];
    assert.ok(entity);
    return entity;
}

describe("Gate3A HIT-01 frame definitions and do_frame_jump", () => {
    it("defaults mp and all ten DAT hit fields to zero without breaking the old frame seed API", () => {
        const entity = canonicalEntity();
        assert.deepEqual(entity.frames[0], {
            id: 0,
            state: 1,
            wait: 100,
            next: 0,
            mp: 0,
            hit_a: 0,
            hit_d: 0,
            hit_j: 0,
            hit_Fa: 0,
            hit_Ua: 0,
            hit_Da: 0,
            hit_Fj: 0,
            hit_Uj: 0,
            hit_Dj: 0,
            hit_ja: 0,
        });
    });

    it("requires an actually defined frame in 0..599 and applies PP/HP cost side effects atomically", () => {
        const entity = canonicalEntity({
            hp: 101,
            pp: 45,
            comboCountVic: 7,
            ppDisplay: 11,
            cooldowns: { right: 5, left: 4, up: 3, down: 2, attack: 6, jump: 7, defend: 8 },
            frames: [frame(0), frame(599, { mp: 4045 })],
        });
        const success = doFrameJump(entity, -599, world, "test");

        assert.equal(success.success, true);
        assert.deepEqual({
            frame: success.entity.frame,
            facing: success.entity.facing,
            hp: success.entity.hp,
            pp: success.entity.pp,
            comboCountVic: success.entity.comboCountVic,
            ppDisplay: success.entity.ppDisplay,
            cooldowns: success.entity.cooldowns,
        }, {
            frame: 599,
            facing: 1,
            hp: 61,
            pp: 0,
            comboCountVic: 47,
            ppDisplay: 56,
            cooldowns: { right: 0, left: 0, up: 0, down: 0, attack: 0, jump: 0, defend: 0 },
        });
        assert.equal(success.event.outcome, "jump");
        assert.equal(success.event.resolvedTarget, 599);

        for (const target of [600, -600, 598]) {
            const failure = doFrameJump(entity, target, world, "test");
            assert.equal(failure.success, false, `target ${target}`);
            assert.strictEqual(failure.entity, entity);
            assert.equal(failure.event.outcome, "failure");
            assert.equal(failure.event.reason, "undefined-frame");
        }
    });

    it("maps +/-999 to frame 0, rejects insufficient resources without mutation, and preserves ppMode=0 behavior", () => {
        const costly = canonicalEntity({
            facing: 0,
            hp: 40,
            pp: 4,
            cooldowns: { attack: 5 },
            frames: [frame(0, { mp: 3005 })],
        });
        const insufficientPp = doFrameJump(costly, 999, world, "test");
        assert.equal(insufficientPp.success, false);
        assert.equal(insufficientPp.event.reason, "insufficient-pp");
        assert.strictEqual(insufficientPp.entity, costly);

        const sufficientPp = canonicalEntity({ ...seed(), hp: 30, pp: 5, frames: [frame(0, { mp: 3005 })] });
        const insufficientHp = doFrameJump(sufficientPp, -999, world, "test");
        assert.equal(insufficientHp.success, false);
        assert.equal(insufficientHp.event.reason, "insufficient-hp");
        assert.equal(insufficientHp.entity.facing, 0);

        const noCost = doFrameJump(costly, -999, { ppMode: 0, oid6DjaGuard: 0 }, "test");
        assert.equal(noCost.success, true);
        assert.equal(noCost.entity.frame, 0);
        assert.equal(noCost.entity.facing, 0, "negative target does not flip outside PP mode");
        assert.equal(noCost.entity.hp, 40);
        assert.equal(noCost.entity.pp, 4);
        assert.deepEqual(noCost.entity.cooldowns, {
            right: 0, left: 0, up: 0, down: 0, attack: 0, jump: 0, defend: 0,
        });
    });
});

describe("Gate3A HIT-02 eight ordinary combo wrappers", () => {
    it("uses the fixed wrapper order, rereads the current frame, and permits same-call cascading", () => {
        const entity = canonicalEntity({
            combos: { DRA: 3, DLA: 3, DUA: 3 },
            frames: [
                frame(0, { hit_Fa: 1 }),
                frame(1, { hit_Fa: 2 }),
                frame(2, { hit_Ua: 3 }),
                frame(3),
            ],
        });
        const result = postCooldownInput(entity, world);

        assert.equal(result.entity.frame, 3);
        assert.equal(result.entity.facing, 1, "DLA facing side effect survives the later DUA wrapper");
        assert.deepEqual(result.events.map((event) => [event.trigger, event.fromFrame, event.toFrame]), [
            ["DRA", 0, 1],
            ["DLA", 1, 2],
            ["DUA", 2, 3],
        ]);
    });

    it("clears an attempted nonzero combo and applies F-facing even when its jump fails", () => {
        const entity = canonicalEntity({
            facing: 1,
            pp: 0,
            combos: { DRA: 3 },
            frames: [frame(0, { hit_Fa: 1 }), frame(1, { mp: 1 })],
        });
        const result = postCooldownInput(entity, world);

        assert.equal(result.entity.frame, 0);
        assert.equal(result.entity.facing, 0);
        assert.equal(result.entity.combos.DRA, 0);
        assert.equal(result.events[0]?.outcome, "failure");
        assert.equal(result.events[0]?.reason, "insufficient-pp");
    });

    it("keeps target-zero combos ready and blocks ordinary triggers while linkState is 2", () => {
        const targetZero = postCooldownInput(canonicalEntity({ combos: { DRA: 3 } }), world);
        assert.equal(targetZero.entity.combos.DRA, 3);
        assert.deepEqual(targetZero.events, []);

        const linked = postCooldownInput(canonicalEntity({
            linkState: 2,
            combos: { DRA: 3 },
            frames: [frame(0, { hit_Fa: 1 }), frame(1)],
        }), world);
        assert.equal(linked.entity.frame, 0);
        assert.equal(linked.entity.combos.DRA, 3);
    });
});

describe("Gate3A HIT-03 DJA special cases", () => {
    it("preserves ready state for the oid-6 global guard and for unk328 while clearing unk338", () => {
        const guarded = postCooldownInput(canonicalEntity({
            oid: 6,
            hp: 178,
            combos: { DJA: 3 },
            frames: [frame(0, { hit_ja: 300 }), frame(300)],
        }), world);
        assert.equal(guarded.entity.frame, 0);
        assert.equal(guarded.entity.combos.DJA, 3);

        const unk328 = postCooldownInput(canonicalEntity({
            unk324: 7,
            unk328: 1,
            unk338: 99,
            combos: { DJA: 3 },
            frames: [frame(0, { hit_ja: 1 }), frame(1)],
        }), world);
        assert.equal(unk328.entity.frame, 0);
        assert.equal(unk328.entity.combos.DJA, 3);
        assert.equal(unk328.entity.unk338, 0);
    });

    it("uses unk324=-1 as the default trigger sentinel and does not apply the ordinary linkState==2 gate", () => {
        const entity = canonicalEntity({
            linkState: 2,
            combos: { DJA: 3 },
            frames: [frame(0, { hit_ja: 1 }), frame(1)],
        });
        assert.equal(entity.unk324, -1);
        const result = postCooldownInput(entity, world);
        assert.equal(result.entity.frame, 0, "DJA follows its own link/unk324 branch and does not trigger here");
        assert.equal(result.entity.combos.DJA, 3);
    });
});

describe("Gate3A HIT-04 direct hit_a/hit_d/hit_j arbitration", () => {
    it("uses strict maxima and no-ops on ties", () => {
        const tie = postCooldownInput(canonicalEntity({
            cooldowns: { attack: 5, defend: 5, jump: 1 },
            frames: [frame(0, { hit_a: 1, hit_d: 2, hit_j: 3 }), frame(1), frame(2), frame(3)],
        }), world);
        assert.equal(tie.entity.frame, 0);
        assert.deepEqual(tie.events, []);
    });

    it("clears the selected cooldown and blocks later else-if candidates even when the selected jump fails", () => {
        const result = postCooldownInput(canonicalEntity({
            pp: 0,
            cooldowns: { attack: 9, defend: 8, jump: 7 },
            frames: [frame(0, { hit_a: 1, hit_d: 2, hit_j: 3 }), frame(1, { mp: 1 }), frame(2), frame(3)],
        }), world);
        assert.equal(result.entity.frame, 0);
        assert.equal(result.entity.cooldowns.attack, 0);
        assert.equal(result.entity.cooldowns.defend, 8);
        assert.equal(result.entity.cooldowns.jump, 7);
        assert.deepEqual(result.events.map((event) => event.trigger), ["hit_a"]);
        assert.equal(result.events[0]?.outcome, "failure");
    });
});

describe("Gate3A HIT-05 eligibility, canonical state, and auditable pass integration", () => {
    it("runs only active resolved DAT type-0 entities, with no hitStop/frameDelay entry gate", () => {
        for (const overrides of [{ active: false }, { rawObjectType: 3 }]         ) {
            const entity = canonicalEntity({
                ...overrides,
                cooldowns: { attack: 9 },
                frames: [frame(0, { hit_a: 1 }), frame(1)],
            });
            assert.strictEqual(postCooldownInput(entity, world).entity, entity);
        }

        const eligible = postCooldownInput(canonicalEntity({
            hitStop: 20,
            frameDelay: -8,
            cooldowns: { attack: 9 },
            frames: [frame(0, { hit_a: 1 }), frame(1)],
        }), world);
        assert.equal(eligible.entity.frame, 1);
    });

    it("keeps arbitrary SimulationInput JSON trace-only while stepSimulation runs canonical post-cooldown state", () => {
        const initial = createSimulation({
            entities: [seed({
                cooldowns: { attack: 9 },
                frames: [frame(0, { hit_a: 1 }), frame(1)],
            })],
        });
        const left = stepSimulation(initial, { cooldowns: { attack: 0 }, frame: 599 });
        const right = stepSimulation(initial, { unrelated: true });

        assert.equal(left.trace.inputJumps[0]?.trigger, "hit_a");
        assert.equal(left.trace.inputJumps[0]?.ruleId.length  > 0, true);
        assert.equal(left.state.entities[0]?.frame, right.state.entities[0]?.frame);
        assert.notEqual(left.trace.inputs, right.trace.inputs);
    });

    it("serializes every input state field and world guard deterministically", () => {
        const state = createSimulation({ entities: [seed()], worldInput: { oid6DjaGuard: 7 } });
        const snapshot = serializeCanonicalSnapshot(state);
        assert.match(snapshot, /"ppMode":1/);
        assert.match(snapshot, /"oid6DjaGuard":7/);
        assert.match(snapshot, /"comboCountVic":0/);
        assert.match(snapshot, /"ppDisplay":0/);
        assert.equal(Object.isFrozen(state.worldInput), true);
        assert.deepEqual(DAT_INPUT_COOLDOWN_KEY_MAP, {
            right: "Right",
            left: "Left",
            up: "Up",
            down: "Down",
            attack: "A (+0xBE)",
            jump: "J (+0xBF)",
            defend: "D (+0xC0)",
        });
    });
});
