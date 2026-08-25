// dat-skill-flow-build:20260823084414142-b505fda7f1b34881a2e08f6e5c172b57
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    buildRuntimeFrameTimeline,
                            
                          
} from "../../src/client/runtime-frame-timeline.js";

function entity(slot        , frame        , active          )                     {
    return active === undefined ? { slot, frame } : { slot, frame, active };
}

function tick(tickIndex        , ...entities                      )                   {
    return { tick: tickIndex, entities };
}

describe("runtime frame timeline", () => {
    it("merges consecutive occurrences of the same root frame", () => {
        const timeline = buildRuntimeFrameTimeline([
            tick(4, entity(0, 300)),
            tick(5, entity(0, 300)),
            tick(6, entity(0, 301)),
        ]);

        assert.deepEqual(timeline.segments, [
            { frameId: 300, startTick: 4, endTick: 5, tickCount: 2 },
            { frameId: 301, startTick: 6, endTick: 6, tickCount: 1 },
        ]);
        assert.equal(Object.isFrozen(timeline), true);
        assert.equal(Object.isFrozen(timeline.segments), true);
        assert.equal(Object.isFrozen(timeline.segments[0]), true);
    });

    it("starts a new segment when a frame is visited again after another frame", () => {
        const timeline = buildRuntimeFrameTimeline([
            tick(0, entity(0, 100)),
            tick(1, entity(0, 100)),
            tick(2, entity(0, 101)),
            tick(3, entity(0, 100)),
            tick(4, entity(0, 100)),
        ]);

        assert.deepEqual(timeline.segments, [
            { frameId: 100, startTick: 0, endTick: 1, tickCount: 2 },
            { frameId: 101, startTick: 2, endTick: 2, tickCount: 1 },
            { frameId: 100, startTick: 3, endTick: 4, tickCount: 2 },
        ]);
    });

    it("closes a segment across missing and inactive root ticks without inventing a frame", () => {
        const timeline = buildRuntimeFrameTimeline([
            tick(0, entity(0, 200)),
            tick(1, entity(1, 999)),
            tick(2, entity(0, 200, false)),
            tick(3, entity(0, 201)),
        ]);

        assert.deepEqual(timeline.segments, [
            { frameId: 200, startTick: 0, endTick: 0, tickCount: 1 },
            { frameId: 201, startTick: 3, endTick: 3, tickCount: 1 },
        ]);
    });

    it("tracks only a non-zero root slot", () => {
        const timeline = buildRuntimeFrameTimeline([
            tick(0, entity(0, 1), entity(7, 400)),
            tick(1, entity(0, 2), entity(7, 400)),
            tick(2, entity(0, 3), entity(7, 401)),
        ], 7);

        assert.deepEqual(timeline.segments, [
            { frameId: 400, startTick: 0, endTick: 1, tickCount: 2 },
            { frameId: 401, startTick: 2, endTick: 2, tickCount: 1 },
        ]);
    });
});
