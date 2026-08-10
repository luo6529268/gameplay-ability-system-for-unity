// dat-skill-flow-build:20260810142213043-054dbf2cd13c4979aabcf4b50fdbcd0f
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    GATE3A_INPUT_RULE,
    GATE3B2_WPOINT_RULE,
    GATE4_MOTION_RULE,
    createSimulation,
    frameTick,
    normalizeFrames,
    serializeCanonicalSnapshot,
    stepSimulation,
} from "../../src/sim/index.js";
             
                     
               
              
                  
                       
                        
                    
                                

                        
                          
 

                                         
                          
                          
                          
                                               
  

                               
                                 
                                   
                                    
  

                                         
                               
                                
                                  
                                     
                                 
                                  
  

                                                               
                                                                                        
   

function wpoint(overrides                               = {})                      {
    return { kind: 0, x: 0, y: 0, attacking: 0, cover: 0, weaponact: 0, dvx: 0, dvy: 0, dvz: 0, ...overrides };
}

function frame(id        , overrides                       = {})                     {
    return { id, state: 1, wait: 100, next: 0, ...overrides }                      ;
}

function dat(oid        , rawObjectType        , frames                               , overrides                     = {})             {
    return { oid, rawObjectType, frames, ...overrides }              ;
}

function entity(stableId        , slot        , overrides                            = {})                {
    return {
        stableId,
        slot,
        oid: slot + 1,
        rawObjectType: 3,
        frame: 0,
        y: -10,
        yInt: -10,
        frames: [frame(0)],
        ...overrides,
    }                 ;
}

function at(state                 , slot        )               {
    const value = state.slots[slot];
    assert.ok(value, `expected active slot ${slot}`);
    return value                ;
}

function close(actual        , expected        , message         )       {
    assert.ok(Math.abs(actual - expected) < 1e-9, `${message ?? "value"}: expected ${expected}, got ${actual}`);
}

describe("Gate4A canonical motion contract", () => {
    it("preserves ordered cpoint kinds and exact frame dv fields", () => {
        const [value] = normalizeFrames([frame(0, {
            dvx: 560,
            dvy: -2,
            dvz: 4,
            cpoints: [{ kind: 7 }, { kind: 2 }],
        })]);
        const motion = value               ;
        assert.deepEqual([motion.dvx, motion.dvy, motion.dvz], [560, -2, 4]);
        assert.deepEqual(motion.cpoints, [{ kind: 7 }, { kind: 2 }], "ordered cpoints are data, not a synthetic boolean");
    });

    it("stores finite double jump stats plus directional keys and four block flags in canonical snapshots", () => {
        const state = createSimulation({
            catalog: [dat(20, 0, [frame(0)], { jumpHeight: -10.25, jumpDistance: 4.5, jumpDistanceZ: 2.75 })],
            entities: [entity("canonical", 0, {
                oid: 20,
                rawObjectType: 0,
                keyLeft: true,
                keyRight: false,
                blockBackZ: true,
                blockForwardZ: true,
                blockLeft: true,
                blockRight: true,
            })],
        });
        const resolved = state.catalog[20]                                ;
        assert.deepEqual([resolved.jumpHeight, resolved.jumpDistance, resolved.jumpDistanceZ], [-10.25, 4.5, 2.75]);
        const defaults = createSimulation({ catalog: [dat(22, 0, [frame(0)])], entities: [] }).catalog[22] ;
        assert.deepEqual([defaults.jumpHeight, defaults.jumpDistance, defaults.jumpDistanceZ], [-16.3, 8, 3]);
        const canonical = at(state, 0);
        assert.deepEqual(
            [canonical.keyLeft, canonical.keyRight, canonical.blockBackZ, canonical.blockForwardZ, canonical.blockLeft, canonical.blockRight],
            [true, false, true, true, true, true],
        );
        const json = serializeCanonicalSnapshot(state);
        for (const field of ["jumpHeight", "jumpDistance", "jumpDistanceZ", "keyLeft", "keyRight", "blockBackZ", "blockForwardZ", "blockLeft", "blockRight"]) {
            assert.match(json, new RegExp(`\\"${field}\\"`), `${field} must participate in replay/checksum state`);
        }
        for (const invalid of [Number.NaN, Number.POSITIVE_INFINITY, Number.NEGATIVE_INFINITY]) {
            assert.throws(() => createSimulation({ catalog: [dat(21, 0, [frame(0)], { jumpHeight: invalid })], entities: [] }), /jumpHeight.*finite/i);
        }
    });
});

describe("Gate4A pass order and frame-advance gates", () => {
    it("orders post-cooldown input, step-4 motion, then held pass 5", () => {
        const result = stepSimulation(createSimulation({ entities: [
            entity("holder", 0, {
                rawObjectType: 0,
                x: 100,
                xInt: 100,
                vx: 7,
                cooldowns: { attack: 9 },
                frames: [
                    frame(0, { hit_a: 1 }),
                    frame(1, { cpoints: [{ kind: 2 }], wpoints: [wpoint({ x: 4, y: 5 })] }),
                ],
                targetIdx: 50,
            }),
            entity("held", 50, { rawObjectType: 1, linkState: -1, holderIdx: 0, y: 0, yInt: 0 }),
        ] }), {});
        const input = result.trace.ruleIds.indexOf(GATE3A_INPUT_RULE.jumpSuccessEffects);
        const motion = result.trace.ruleIds.indexOf(GATE4_MOTION_RULE.passOrder);
        const held = result.trace.ruleIds.indexOf(GATE3B2_WPOINT_RULE.heldPassOrder);
        assert.ok(input >= 0 && motion > input && held > motion, `unexpected pass order: ${result.trace.ruleIds.join(", ")}`);
        assert.equal(at(result.state, 0).x, 100, "the first cpoint kind 2 gates motion after the input frame jump");
    });

    it("moves frameDelay toward zero before returning, then gates zero-delay negative links and first cpoint kind 2", () => {
        let state = createSimulation({ entities: [
            entity("positive-delay", 0, { frameDelay: 1, vx: 3, blockRight: true }),
            entity("negative-delay", 1, { frameDelay: -1, vx: 3, blockRight: true }),
            entity("linked", 2, { linkState: -1, vx: 3, blockRight: true }),
            entity("cpoint", 3, { vx: 3, blockRight: true, frames: [frame(0, { cpoints: [{ kind: 2 }] })] }),
            entity("second-cpoint", 4, { vx: 3, frames: [frame(0, { cpoints: [{ kind: 1 }, { kind: 2 }] })] }),
        ] });
        state = stepSimulation(state, {}).state;
        assert.deepEqual([at(state, 0).frameDelay, at(state, 1).frameDelay], [0, 0]);
        for (let slot = 0; slot < 4; slot++) {
            assert.equal(at(state, slot).x, 0);
            assert.equal(at(state, slot).blockRight, true, "early returns preserve block flags");
        }
        state = stepSimulation(state, {}).state;
        assert.equal(at(state, 0).x, 0, "the matching block prevents X integration");
        assert.equal(at(state, 0).vx, 3, "a blocked axis preserves speed");
        assert.equal(at(state, 0).blockRight, false, "a completed physics pass clears every block flag");
        assert.equal(at(state, 4).x, 6, "only the first ordered cpoint can gate frame advance");
    });
});

describe("Gate4A frame dv and horizontal/depth physics", () => {
    it("applies non-character dv encoding, two ordered depth directions, and never reapplies character dv in motion", () => {
        const result = stepSimulation(createSimulation({ entities: [
            entity("absolute", 0, { facing: 0, frames: [frame(0, { dvx: 560, dvy: 550, dvz: 553 })] }),
            entity("facing", 1, { facing: 1, frames: [frame(0, { dvx: 4 })] }),
            entity("depth-tie", 2, { keyUp: true, keyDown: true, cooldowns: { up: 5, down: 5 }, frames: [frame(0, { dvz: 4 })] }),
            entity("character", 3, { rawObjectType: 0, vx: 2, frames: [frame(0, { dvx: 10 })] }),
        ] }), {}).state;
        assert.deepEqual([at(result, 0).vx, at(result, 0).vy, at(result, 0).vz], [10, 0, 3]);
        assert.equal(at(result, 1).vx, -4);
        assert.equal(at(result, 2).vz, 4, "down is the second independent if and overwrites up on a cooldown tie");
        assert.equal(at(result, 3).x, 2, "character frame dv belongs to post-input, while motion integrates its existing velocity");
    });

    it("integrates old X/Z velocity, applies independent special-X corrections, clears blocks, and applies grounded friction afterward", () => {
        const state = stepSimulation(createSimulation({ entities: [
            entity("blocked", 0, { vx: 4, vz: -3, blockLeft: true, blockForwardZ: true, blockBackZ: true, blockRight: true }),
            entity("grounded", 1, { x: 10, y: 0, yInt: 0, z: 10, vx: 3, vz: -2 }),
            entity("type4", 2, { rawObjectType: 4, vx: 5 }),
            entity("oid120", 3, { oid: 120, vx: 5 }),
            entity("oid101", 4, { oid: 101, vx: 5 }),
            entity("both", 5, { oid: 101, rawObjectType: 4, vx: 5 }),
        ] }), {}).state;
        assert.deepEqual([at(state, 0).x, at(state, 0).z, at(state, 0).vx, at(state, 0).vz], [0, 0, 4, -3]);
        assert.deepEqual(
            [at(state, 0).blockBackZ, at(state, 0).blockForwardZ, at(state, 0).blockLeft, at(state, 0).blockRight],
            [false, false, false, false],
        );
        assert.deepEqual([at(state, 1).x, at(state, 1).z, at(state, 1).vx, at(state, 1).vz], [13, 8, 2, -1]);
        close(at(state, 2).x, 6);
        close(at(state, 3).x, 6);
        close(at(state, 4).x, 4);
        close(at(state, 5).x, 5, "type4 add and oid101 subtract are independent");
    });

    it("uses the compiled asymmetric post-friction residual comparisons", () => {
        const state = stepSimulation(createSimulation({ entities: [
            entity("positive-residual", 0, { y: 0, yInt: 0, vx: 1.00005, vz: 1.00005 }),
            entity("negative-residual", 1, { y: 0, yInt: 0, vx: -1.00005, vz: -1.00005 }),
        ] }), {}).state;
        const negativeResidual = -1.00005 + 1;
        assert.deepEqual(
            [at(state, 0).vx, at(state, 0).vz, at(state, 1).vx, at(state, 1).vz],
            [0, 0, negativeResidual, negativeResidual],
        );
    });
});

describe("Gate4A vertical physics, landing, and integer mirrors", () => {
    it("selects frame 40 only for type 4/6 state 1000 with post-friction |vx| above 9", () => {
        const state = stepSimulation(createSimulation({ entities: [
            entity("type4-fast", 0, { rawObjectType: 4, vx: 10, frames: [frame(0, { state: 1000 }), frame(40)] }),
            entity("type6-fast", 1, { rawObjectType: 6, vx: -10, frames: [frame(0, { state: 1000 }), frame(40)] }),
            entity("boundary", 2, { rawObjectType: 4, vx: 9, frames: [frame(0, { state: 1000 }), frame(40)] }),
            entity("wrong-state", 3, { rawObjectType: 6, vx: 10, frames: [frame(0, { state: 1 }), frame(40)] }),
            entity("wrong-type", 4, { rawObjectType: 3, vx: 10, frames: [frame(0, { state: 1000 }), frame(40)] }),
        ] }), {}).state;
        assert.deepEqual([0, 1, 2, 3, 4].map((slot) => at(state, slot).frame), [40, 40, 0, 0, 0]);
    });

    it("integrates old vy before applying the exact raw-type/state/oid gravity branches", () => {
        const cases = [
            ["default", 0, 1, 1.7], ["type4", 4, 1, 0.85], ["type6", 6, 1, 1.1333333333333333], ["type3", 3, 1, 0],
            ["s1002-124", 0, 1002, 0.17, 124], ["s1002-120", 0, 1002, 0.425, 120],
            ["s1002-101", 0, 1002, 1.1333333333333333, 101], ["s1002-other", 0, 1002, 0.5666666666666667, 77],
        ]         ;
        const state = stepSimulation(createSimulation({ entities: cases.map(([name, raw, frameState, , oid], slot) => entity(name, slot, {
            rawObjectType: raw,
            oid: oid ?? slot + 1,
            y: -10,
            yInt: -10,
            vy: -2,
            frames: [frame(0, { state: frameState })],
        })) }), {}).state;
        cases.forEach(([, , , gravity], slot) => {
            close(at(state, slot).y, -12, "Y uses old vy");
            close(at(state, slot).vy, -2 + gravity, "gravity updates the next velocity");
        });
    });

    it("selects airborne character frames and implements only generic flat landing", () => {
        const state = stepSimulation(createSimulation({ entities: [
            entity("rise-fast", 0, { rawObjectType: 0, vy: -10, frames: [frame(0, { state: 12 }), frame(180, { state: 12 })] }),
            entity("fall", 1, { rawObjectType: 0, vy: 0, frames: [frame(0, { state: 18 }), frame(205, { state: 18 })] }),
            entity("land", 2, { rawObjectType: 0, y: -1, yInt: -1, vy: 2, vx: 6, vz: 2, attacking: 9, frames: [frame(0, { state: 1 }), frame(219)] }),
            entity("land-212", 3, { rawObjectType: 0, frame: 212, y: -1, yInt: -1, vy: 2, frames: [frame(212, { state: 1 }), frame(215)] }),
            entity("land-100", 4, { rawObjectType: 0, frame: 100, y: -1, yInt: -1, vy: 2, frames: [frame(100, { state: 100 }), frame(94)] }),
        ] }), {}).state;
        assert.equal(at(state, 0).frame, 180);
        assert.equal(at(state, 1).frame, 205);
        assert.deepEqual([at(state, 2).y, at(state, 2).vy, at(state, 2).vx, at(state, 2).vz, at(state, 2).frame, at(state, 2).attacking], [0, 0, 2, 2, 219, 1]);
        assert.equal(at(state, 3).frame, 215);
        assert.equal(at(state, 4).frame, 94);
    });

    it("truncates X/Y/Z mirrors toward zero after motion", () => {
        const state = stepSimulation(createSimulation({ entities: [entity("truncate", 0, {
            x: 1.9, xInt: 1, y: -1.9, yInt: -1, z: -1.9, zInt: -1, vx: -0.2, vz: 0.2,
        })] }), {}).state;
        close(at(state, 0).x, 1.7);
        close(at(state, 0).z, -1.7);
        assert.deepEqual([at(state, 0).xInt, at(state, 0).yInt, at(state, 0).zInt], [1, -1, -1]);
    });
});

describe("Gate4A late jump initialization and opoint motion visibility", () => {
    it("initializes only explicit +/-212 from finite resolved DAT stats and leaves displacement to the next tick", () => {
        const frames = [frame(0, { wait: 0, next: 212 }), frame(212, { state: 4 })];
        let state = createSimulation({
            catalog: [dat(40, 0, frames, { jumpHeight: -10.5, jumpDistance: 4.25, jumpDistanceZ: 2.5 })],
            entities: [entity("jump", 0, { oid: 40, rawObjectType: 0, y: 0, yInt: 0, keyLeft: true, keyUp: true, frames })],
        });
        state = stepSimulation(state, {}).state;
        assert.deepEqual([at(state, 0).x, at(state, 0).y, at(state, 0).z], [0, 0, 0], "late jump init cannot retroactively enter step-4 motion");
        assert.deepEqual([at(state, 0).vx, at(state, 0).vy, at(state, 0).vz, at(state, 0).facing], [-4.25, -10.5, -2.5, 0]);
        state = stepSimulation(state, {}).state;
        assert.deepEqual([at(state, 0).x, at(state, 0).y, at(state, 0).z], [-4.25, -10.5, -2.5]);

        const invoke = frameTick                                                                                                    ;
        const make = (next        , extra                            = {}) => createSimulation({
            catalog: [dat(41, 0, [frame(0, { wait: 0, next }), frame(212)], { jumpHeight: -9.5, jumpDistance: 3.5, jumpDistanceZ: 1.5 })],
            entities: [entity(`jump-${next}`, 0, { oid: 41, rawObjectType: 0, y: 0, yInt: 0, frames: [frame(0, { wait: 0, next }), frame(212)], ...extra })],
        });
        const negative = make(-212, { keyRight: true, keyDown: true, facing: 0 });
        const negativeTick = invoke(at(negative, 0), negative.catalog[41] ).entity;
        assert.deepEqual([negativeTick.frame, negativeTick.facing, negativeTick.vx, negativeTick.vy, negativeTick.vz], [212, 1, 3.5, -9.5, 1.5]);

        const simultaneous = make(212, {
            facing: 1,
            vx: 8,
            vz: 6,
            keyLeft: true,
            keyRight: true,
            keyUp: true,
            keyDown: true,
        });
        const simultaneousTick = invoke(at(simultaneous, 0), simultaneous.catalog[41] ).entity;
        assert.deepEqual(
            [simultaneousTick.facing, simultaneousTick.vx, simultaneousTick.vy, simultaneousTick.vz],
            [1, 8, -9.5, 6],
            "opposed direction pairs preserve old velocity and input direction does not change facing",
        );

        const state0Frames = [frame(0, { state: 0 }), frame(212)];
        const state0Recovery = createSimulation({
            catalog: [dat(42, 0, state0Frames, { jumpHeight: -9.5, jumpDistance: 3.5, jumpDistanceZ: 1.5 })],
            entities: [entity("state0-recovery", 0, { oid: 42, rawObjectType: 0, y: -1, yInt: -1, vx: 8, vy: 7, vz: 6, frames: state0Frames })],
        });
        for (const recovery of [state0Recovery, make(999, { y: -1, yInt: -1, vx: 8, vy: 7, vz: 6 })]) {
            const tick = invoke(at(recovery, 0), recovery.catalog[41] ).entity;
            assert.equal(tick.frame, 212);
            assert.deepEqual([tick.vx, tick.vy, tick.vz], [8, 7, 6], "state-0 and next-999 recovery suppress jump initialization");
        }
    });

    it("spawns an opoint with explicit velocity/reset flags but gives it first motion on the next tick", () => {
        let state = createSimulation({
            catalog: [dat(90, 3, [frame(1)])],
            entities: [entity("parent", 0, {
                rawObjectType: 3,
                vx: 100,
                vy: 80,
                vz: 60,
                attacking: -1,
                frames: [frame(0, { opoints: [{ kind: 1, x: 0, y: 0, action: 1, dvx: 6, dvy: -3, oid: 90, facing: 0 }] })],
            })],
        });
        state = stepSimulation(state, {}).state;
        const child = at(state, 50);
        const birth = [child.x, child.y, child.z]         ;
        assert.deepEqual([child.vx, child.vy, child.vz], [6, -3, 0], "parent velocity is not inherited");
        assert.deepEqual([child.keyLeft, child.keyRight, child.blockBackZ, child.blockForwardZ, child.blockLeft, child.blockRight], [false, false, false, false, false, false]);
        state = stepSimulation(state, {}).state;
        assert.deepEqual([at(state, 50).x, at(state, 50).y, at(state, 50).z], [birth[0] + 6, birth[1] - 3, birth[2]]);
    });
});
