// dat-skill-flow-build:20260810141118537-1fe33929bb5a42a19b07c771bcf550ed
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    buildSkillFlow,
    traceStartFrameForSelection,
} from "../../src/client/skill-flow.js";
import { deriveSkillEntries, entriesByStartFrame } from "../../src/client/skill-entries.js";
import { buildOverlayGeometry, hitTestOverlay } from "../../src/client/overlay-geometry.js";
                                                                            

function frame(frameId        , occurrence        , overrides                              = {})                     {
    return {
        frameId,
        occurrence,
        label: "",
        pic: 0,
        state: 0,
        wait: 1,
        next: 0,
        dvx: 0,
        dvy: 0,
        dvz: 0,
        centerx: 0,
        centery: 0,
        hit_Fa: 0,
        hit_Fj: 0,
        hit_Ua: 0,
        hit_Uj: 0,
        hit_Da: 0,
        hit_Dj: 0,
        hit_ja: 0,
        hit_a: 0,
        hit_d: 0,
        hit_j: 0,
        mp: 0,
        vaction: 0,
        sound: "",
        itrs: [],
        bdys: [],
        opoints: [],
        wpoints: [],
        bpoints: [],
        cpoints: [],
        ...overrides,
    };
}

describe("skill flow graph", () => {
    it("keeps frame inspection on the selected node while tracing from the authored skill entry", () => {
        const frames = [
            frame(210, 0, { label: "jump", state: 4, next: 211 }),
            frame(211, 1, { label: "jump", state: 4, next: 212 }),
            frame(212, 2, { label: "jump", state: 4, next: 0 }),
            frame(271, 3, { label: "mass clone", state: 3, next: 272 }),
        ];
        const jump = buildSkillFlow(frames, 210);

        assert.equal(traceStartFrameForSelection(frames, 212, 2, jump), 210);
        assert.equal(traceStartFrameForSelection(frames, 211, 1, jump), 210);
        assert.equal(traceStartFrameForSelection(frames, 271, 3, jump), 271);

        const directTail = buildSkillFlow(frames, 212);
        assert.equal(traceStartFrameForSelection(frames, 212, 2, directTail), 212);
    });

    it("uses the last duplicate frame occurrence and preserves branches, self-loops, zero, negative, 999, and missing targets", () => {
        const graph = buildSkillFlow([
            frame(10, 0, { next: 11, hit_a: 12 }),
            frame(10, 1, { next: 13, hit_a: 12 }),
            frame(11, 0, { next: 11, hit_d: 0, hit_j: -4 }),
            frame(12, 0, { next: 999 }),
            frame(13, 0, { next: 11, hit_a: 77 }),
        ], 10);

        assert.equal(graph.startNodeId, "frame:10:1");
        assert.deepEqual(graph.nodes.filter((node) => node.kind === "frame").map((node) => node.id), [
            "frame:10:1", "frame:13:0", "frame:12:0", "frame:11:0",
        ]);
        assert.deepEqual(
            graph.edges.filter((edge) => edge.from === "frame:10:1" && edge.resolution === "frame")
                .map((edge) => [edge.key, edge.rawTarget, edge.to]),
            [
                ["next", 13, "frame:13:0"],
                ["hit_a", 12, "frame:12:0"],
            ],
        );
        assert.equal(graph.edges.find((edge) => edge.from === "frame:11:0" && edge.key === "hit_d")?.resolution, "zero");
        assert.equal(graph.edges.find((edge) => edge.from === "frame:11:0" && edge.key === "hit_j")?.resolution, "negative");
        assert.equal(graph.edges.find((edge) => edge.from === "frame:12:0" && edge.key === "next")?.resolution, "out-of-range");
        assert.equal(graph.edges.find((edge) => edge.from === "frame:13:0" && edge.key === "hit_a")?.resolution, "missing");
    });

    it("reports a self-loop and does not create an inferred frame zero jump", () => {
        const graph = buildSkillFlow([frame(7, 0, { next: 7 })], 7);
        assert.deepEqual(graph.cycles, [{ edgeId: "frame:7:0:next", from: "frame:7:0", to: "frame:7:0" }]);
        assert.equal(graph.edges.find((edge) => edge.key === "hit_a")?.resolution, "zero");
    });

    it("keeps next in the current flow and collapses another hit entry into a clickable leaf", () => {
        const frames = [
            frame(0, 0, { label: "standing", state: 0, next: 1, hit_Uj: 300 }),
            frame(1, 1, { label: "standing", state: 0, next: 999 }),
            frame(300, 2, { label: "rasenganshuriken", state: 15, next: 301 }),
            frame(301, 3, { label: "rasenganshuriken", state: 15, next: 999 }),
        ];
        const entries = deriveSkillEntries(frames, 2);
        const graph = buildSkillFlow(frames, 0, () => true, entriesByStartFrame(entries));

        assert.equal(graph.nodes.some((node) => node.kind === "frame" && node.frameId === 1), true);
        assert.equal(graph.nodes.some((node) => node.kind === "frame" && node.frameId === 300), false);
        assert.equal(graph.nodes.some((node) => node.kind === "frame" && node.frameId === 301), false);
        assert.deepEqual(
            graph.nodes.find((node) => node.kind === "entry" && node.frameId === 300),
            {
                id: "entry-ref:entry:2:300",
                kind: "entry",
                entryId: "entry:2:300",
                frameId: 300,
                label: "rasenganshuriken",
            },
        );
        assert.equal(graph.edges.find((edge) => edge.key === "hit_Uj")?.resolution, "entry");
    });
});

describe("DAT overlay geometry", () => {
    it("projects rectangles and points through the same mirrored sprite transform", () => {
        const frameValue = frame(7, 0, {
            itrs: [{
                kind: 0, x: 10, y: 20, w: 30, h: 40, dvx: 0, dvy: 0, fall: 0, bdefend: 0,
                injury: 0, arest: 0, vrest: 0, effect: 0, attacking: 0, catchingact: 0, catchingact2: 0,
                caughtact: 0, caughtact2: 0, respond: 0, pickingact: 0, pickedact: 0, throwvx: 0,
                throwvy: 0, zwidth: 15, throwvz: 0, throwinjury: 0,
            }],
            bpoints: [{ x: 8, y: 9 }],
        });
        const geometry = buildOverlayGeometry(frameValue, { left: 100, top: 50, width: 80, height: 120, mirror: false });
        assert.deepEqual(geometry[0], {
            type: "itr", index: 0, kind: "rect",
            x1: 110, y1: 70, x2: 140, y2: 110, width: 30, height: 40,
        });
        assert.deepEqual(geometry[1], {
            type: "bpoint", index: 0, kind: "point", x: 108, y: 59,
        });

        const mirrored = buildOverlayGeometry(frameValue, { left: 100, top: 50, width: 80, height: 120, mirror: true });
        assert.deepEqual(mirrored[0], {
            type: "itr", index: 0, kind: "rect",
            x1: 170, y1: 70, x2: 140, y2: 110, width: -30, height: 40,
        });
        assert.deepEqual(mirrored[1], {
            type: "bpoint", index: 0, kind: "point", x: 172, y: 59,
        });
    });

    it("hit-tests the topmost overlay without changing geometry", () => {
        const geometry = buildOverlayGeometry(frame(1, 0, {
            bdys: [{ x: 0, y: 0, w: 20, h: 20 }],
            bpoints: [{ x: 10, y: 10 }],
        }), { left: 100, top: 100, width: 50, height: 50, mirror: false });
        assert.equal(hitTestOverlay(geometry, 110, 110)?.type, "bpoint");
        assert.equal(hitTestOverlay(geometry, 101, 101)?.type, "bdy");
        assert.equal(hitTestOverlay(geometry, 200, 200), undefined);
    });
});
