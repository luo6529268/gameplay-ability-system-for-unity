// dat-skill-flow-build:20260801063334282-ea2c0ce05037426f8f9537203e0df12d
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    applyTimelineCommand,
    createSimulation,
    createTimeline,
    replaySimulation,
    samplePresentation,
    serializeCanonicalSnapshot,
    serializeTickTrace,
} from "../../src/sim/index.js";
                                                                                

function frame(id        , next        )                     {
    return { id, state: 1, wait: 0, next };
}

function seed()                {
    return {
        stableId: "timeline-entity",
        slot: 4,
        rawObjectType: 0,
        frame: 0,
        waitCounter: 0,
        attacking: 0,
        facing: 0,
        yInt: 0,
        hitStop: 0,
        killCount: -1,
        active: true,
        frames: [frame(0, 1), frame(1, 2), frame(2, 0)],
    };
}

describe("Gate 2 pure timeline controller", () => {
    it("makes playing advance byte-equivalent to an explicit step", () => {
        const initial = createTimeline(createSimulation({ entities: [seed()] }));
        const stepped = applyTimelineCommand(initial, { type: "step", input: { key: "a" } });
        const playing = applyTimelineCommand(initial, { type: "play" });
        const advanced = applyTimelineCommand(playing, { type: "advance", input: { key: "a" } });

        assert.equal(
            serializeCanonicalSnapshot(advanced.canonical),
            serializeCanonicalSnapshot(stepped.canonical),
        );
        assert.equal(serializeTickTrace(advanced.traces[0] ), serializeTickTrace(stepped.traces[0] ));
    });

    it("supports pause, deterministic seek/replay, and a pure inclusive loop range", () => {
        const initial = createTimeline(createSimulation({ entities: [seed()] }));
        const tick1 = applyTimelineCommand(initial, { type: "step", input: { sequence: 1 } });
        const tick2 = applyTimelineCommand(tick1, { type: "step", input: { sequence: 2 } });
        const seek0 = applyTimelineCommand(tick2, { type: "seek", tick: 0 });
        const seek1 = applyTimelineCommand(tick2, { type: "seek", tick: 1 });

        assert.equal(seek0.canonical.tickIndex, 0);
        assert.equal(seek1.canonical.tickIndex, 1);
        assert.equal(
            serializeCanonicalSnapshot(seek1.canonical),
            serializeCanonicalSnapshot(tick1.canonical),
        );

        const paused = applyTimelineCommand(tick2, { type: "pause" });
        const noAdvance = applyTimelineCommand(paused, { type: "advance", input: { ignored: true } });
        assert.strictEqual(noAdvance, paused);

        const looped = applyTimelineCommand(
            applyTimelineCommand(
                applyTimelineCommand(tick2, { type: "set-loop", range: { startTick: 1, endTick: 2 } }),
                { type: "play" },
            ),
            { type: "advance", input: {} },
        );
        assert.equal(looped.canonical.tickIndex, 1);
    });

    it("samples presentation without changing canonical state", () => {
        const initial = createTimeline(createSimulation({ entities: [seed()] }));
        const stepped = applyTimelineCommand(initial, { type: "step", input: {} });
        const before = serializeCanonicalSnapshot(stepped.canonical);
        const sample = samplePresentation(stepped, 0.25);
        const after = serializeCanonicalSnapshot(stepped.canonical);

        assert.equal(before, after);
        assert.equal(sample.alpha, 0.25);
        assert.equal(sample.sampleTimeMs, 8.25);
        assert.deepEqual(sample.entities[0], {
            stableId: "timeline-entity",
            fromFrame: 0,
            toFrame: 1,
        });
    });
});

describe("Gate 2 canonical per-tick trace", () => {
    it("records inputs, transitions, collisions, lifecycle, rule IDs, digest, and empty slot lifecycle", () => {
        const initial = createSimulation({ entities: [seed()] });
        const replay = replaySimulation(initial, [{ z: 1, a: 2 }], {
            collision: (current) => ({ observed: current.frame }),
        });
        const trace = replay.traces[0];
        assert.ok(trace);

        assert.deepEqual(trace.inputs, { a: 2, z: 1 });
        assert.equal(trace.frameTransitions.length, 1);
        assert.equal(trace.collisions.length, 1);
        assert.deepEqual(trace.lifecycle, []);
        assert.deepEqual(trace.slotLifecycle, []);
        assert.ok(trace.ruleIds.length > 0);
        assert.match(trace.snapshotDigest, /^fnv1a32:[0-9a-f]{8}$/);
    });

    it("replays byte-identically from the initial state and input script", () => {
        const initial = createSimulation({ entities: [seed()] });
        const script = [{ key: "a", power: 1 }, { power: 2, key: "b" }, {}]         ;
        const runtime = { collision: (current                   ) => ({ frame: current.frame }) };
        const left = replaySimulation(initial, script, runtime);
        const right = replaySimulation(initial, script, runtime);

        assert.equal(
            left.traces.map(serializeTickTrace).join("\n"),
            right.traces.map(serializeTickTrace).join("\n"),
        );
        assert.equal(
            serializeCanonicalSnapshot(left.state),
            serializeCanonicalSnapshot(right.state),
        );
    });
});
