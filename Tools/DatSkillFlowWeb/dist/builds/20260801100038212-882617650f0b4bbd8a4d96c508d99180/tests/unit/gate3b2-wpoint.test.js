// dat-skill-flow-build:20260801100038212-882617650f0b4bbd8a4d96c508d99180
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    createSimulation,
    nextNtsdRandom,
    normalizeFrames,
    resolveHeldAttackPayload,
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

    it("implements the canonical uint32 LCG and 15-bit output", () => {
        let sample = nextNtsdRandom(0);
        assert.deepEqual(sample, { seed: 0x269ec3, value: 38 });
        sample = nextNtsdRandom(sample.seed);
        assert.deepEqual(sample, { seed: 0x1e278e7a, value: 7719 });
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
});
