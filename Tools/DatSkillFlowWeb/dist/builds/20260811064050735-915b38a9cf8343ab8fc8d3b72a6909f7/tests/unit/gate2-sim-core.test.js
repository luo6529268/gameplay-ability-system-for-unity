// dat-skill-flow-build:20260811064050735-915b38a9cf8343ab8fc8d3b72a6909f7
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    EFFECTIVE_FRAME_RATE,
    FRAME_MS,
    NOMINAL_FRAME_RATE,
    createSimulation,
    freeEntity,
    stepSimulation,
    ticksToMilliseconds,
} from "../../src/sim/index.js";
                                                                                

function frame(
    id        ,
    wait        ,
    next        ,
    state = 1,
)                     {
    return { id, state, wait, next };
}

function entity(overrides                         = {})                {
    return {
        stableId: "entity-0",
        slot: 0,
        rawObjectType: 0,
        frame: 0,
        waitCounter: 0,
        attacking: 0,
        facing: 0,
        yInt: 0,
        hitStop: 0,
        killCount: -1,
        active: true,
        frames: [frame(0, 0, 0)],
        ...overrides,
    };
}

function onlyEntity(state                                     ) {
    const result = state.entities[0];
    assert.ok(result);
    return result;
}

describe("Gate 2 integer simulation clock", () => {
    it("uses an exact 33 ms tick while labeling the nominal and effective rates honestly", () => {
        assert.equal(FRAME_MS, 33);
        assert.equal(ticksToMilliseconds(300), 9_900);
        assert.equal(NOMINAL_FRAME_RATE, 30);
        assert.equal(EFFECTIVE_FRAME_RATE, 1000 / 33);

        const initial = createSimulation({ entities: [entity()] });
        const after = stepSimulation(initial, {}).state;
        assert.equal(after.tickIndex, 1);
        assert.equal(after.timeMs, 33);
    });
});

describe("Gate 2 authoritative frame_tick subset", () => {
    it("increments attacking before a strict greater-than wait comparison", () => {
        let state = createSimulation({
            entities: [entity({ frames: [frame(0, 2, 1), frame(1, 10, 0)] })],
        });

        state = stepSimulation(state, {}).state;
        assert.deepEqual({ frame: onlyEntity(state).frame, attacking: onlyEntity(state).attacking }, {
            frame: 0,
            attacking: 1,
        });
        state = stepSimulation(state, {}).state;
        assert.equal(onlyEntity(state).frame, 0);
        assert.equal(onlyEntity(state).attacking, 2);
        state = stepSimulation(state, {}).state;
        assert.equal(onlyEntity(state).frame, 1);
        assert.equal(onlyEntity(state).attacking, 0);
    });

    it("resets attacking on a frame/waitCounter mismatch before incrementing", () => {
        const state = createSimulation({
            entities: [entity({
                attacking: 20,
                waitCounter: 99,
                frames: [frame(0, 2, 1), frame(1, 2, 0)],
            })],
        });

        const after = stepSimulation(state, {}).state;
        assert.equal(onlyEntity(after).frame, 0);
        assert.equal(onlyEntity(after).attacking, 1);
        assert.equal(onlyEntity(after).waitCounter, 0);
    });

    it("distinguishes next=0 hold from a nonzero self transition", () => {
        const held = stepSimulation(createSimulation({
            entities: [entity({ frames: [frame(0, 0, 0)] })],
        }), {});
        assert.equal(onlyEntity(held.state).frame, 0);
        assert.equal(onlyEntity(held.state).attacking, 0);
        assert.equal(held.trace.frameTransitions[0]?.kind, "hold");

        const self = stepSimulation(createSimulation({
            entities: [entity({ frame: 3, waitCounter: 3, frames: [frame(3, 0, 3)] })],
        }), {});
        assert.equal(onlyEntity(self.state).frame, 3);
        assert.equal(self.trace.frameTransitions[0]?.kind, "self");
    });

    it("flips facing before taking the absolute value of a negative next", () => {
        const result = stepSimulation(createSimulation({
            entities: [entity({ frames: [frame(0, 0, -2), frame(2, 10, 0)] })],
        }), {});

        assert.equal(onlyEntity(result.state).frame, 2);
        assert.equal(onlyEntity(result.state).facing, 1);
        assert.equal(result.trace.frameTransitions[0]?.kind, "negative");
    });

    it("resolves next=999 to 212 iff airborne raw objType 0, otherwise to 0", () => {
        const resolve = (rawObjectType        , yInt        ) => onlyEntity(stepSimulation(createSimulation({
            entities: [entity({
                rawObjectType,
                yInt,
                frames: [frame(0, 0, 999, 1), frame(212, 10, 0, 1)],
            })],
        }), {}).state).frame;

        assert.equal(resolve(0, -1), 212);
        assert.equal(resolve(0, 0), 0);
        assert.equal(resolve(3, -1), 0);
    });

    it("applies the earlier state-0 airborne preemption before authored next handling", () => {
        const result = stepSimulation(createSimulation({
            entities: [entity({
                yInt: -1,
                frames: [frame(0, 20, 999, 0), frame(212, 20, 0, 1)],
            })],
        }), {});

        assert.equal(onlyEntity(result.state).frame, 212);
    });

    it("does not apply state-0 airborne preemption when the resolved DAT object type is negative", () => {
        const result = stepSimulation(createSimulation({
            entities: [entity({
                rawObjectType: -1,
                yInt: -1,
                frames: [frame(0, 20, 0, 0), frame(212, 20, 0, 1)],
            })],
        }), {});

        assert.equal(onlyEntity(result.state).frame, 0);
        assert.equal(result.trace.frameTransitions.some((event) => event.kind === "state0-airborne"), false);
    });
});

describe("Gate 2 late entity update ordering and cleanup", () => {
    it("runs collision once after a next=1000 transition, then frees the entity", () => {
        const calls           = [];
        const initial = createSimulation({
            entities: [entity({ frame: 1, waitCounter: 1, frames: [frame(1, 0, 1000)] })],
        });
        const result = stepSimulation(initial, { command: "cast" }, {
            collision: (current) => {
                calls.push(`${current.stableId}:${current.frame}`);
                return { observedFrame: current.frame };
            },
        });

        assert.deepEqual(calls, ["entity-0:1000"]);
        assert.equal(result.trace.collisions.length, 1);
        assert.equal(result.trace.collisions[0]?.frame, 1000);
        assert.equal(onlyEntity(result.state).frame, 0);
        assert.equal(onlyEntity(result.state).active, false);
        assert.equal(result.state.objectCount, 0);
        assert.equal(onlyEntity(result.state).waitCounter, 1, "range return preserves old waitCounter");
    });

    it("runs collision once before treating next=1280 as the 12xx group", () => {
        const source = entity({
            stableId: "source",
            slot: 7,
            frame: 1,
            waitCounter: 1,
            frames: [frame(0, 20, 0), frame(1, 0, 1280)],
        });
        const child = entity({
            stableId: "child",
            slot: 2,
            killCount: 7,
            frames: [frame(0, 20, 0)],
        });
        const result = stepSimulation(createSimulation({ entities: [source, child] }), {}, {
            collision: (current) => ({ seen: `${current.stableId}:${current.frame}` }),
        });
        const byId = Object.fromEntries(result.state.entities.map((current) => [current.stableId, current]));

        assert.equal(result.trace.collisions.filter((event) => event.stableId === "source").length, 1);
        assert.equal(result.trace.collisions.find((event) => event.stableId === "source")?.frame, 1280);
        assert.equal(byId.source?.frame, 0);
        assert.equal(byId.source?.hitStop, -180);
        assert.equal(byId.source?.active, true);
        assert.equal(byId.source?.waitCounter, 1, "group path follows the range return from frame_tick");
        assert.equal(byId.child?.hitStop, -180);
        assert.equal(result.state.objectCount, 2);
    });

    it("uses exact inclusive 1100..1299 group bounds and frees 1099/1300", () => {
        for (const [target, expectedActive, expectedHitStop] of [
            [1099, false, 0],
            [1100, true, 0],
            [1299, true, -199],
            [1300, false, 0],
        ]         ) {
            const result = stepSimulation(createSimulation({
                entities: [entity({ frame: 1, waitCounter: 1, frames: [frame(1, 0, target)] })],
            }), {});
            assert.equal(onlyEntity(result.state).active, expectedActive, `active for ${target}`);
            assert.equal(onlyEntity(result.state).hitStop, expectedHitStop, `hitStop for ${target}`);
            assert.equal(onlyEntity(result.state).frame, 0, `frame for ${target}`);
        }
    });

    it("free is idempotent and decrements objectCount only behind the active guard", () => {
        const initial = createSimulation({ entities: [entity()] });
        const once = freeEntity(initial, "entity-0");
        const twice = freeEntity(once, "entity-0");

        assert.equal(once.objectCount, 0);
        assert.equal(twice.objectCount, 0);
        assert.equal(onlyEntity(twice).active, false);
    });
});
