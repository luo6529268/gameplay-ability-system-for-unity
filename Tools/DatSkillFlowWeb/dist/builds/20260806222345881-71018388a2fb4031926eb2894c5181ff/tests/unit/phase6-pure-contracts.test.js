// dat-skill-flow-build:20260806222345881-71018388a2fb4031926eb2894c5181ff
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    draftOverlayGeometry,
    hitResizeHandle,
    moveDatPoint,
    resizeDatRect,
    snapDelta,
} from "../../src/client/canvas-geometry-edit.js";
import { layoutSkillFlow } from "../../src/client/flow-layout.js";
import {
    deleteSkillForOid,
    duplicateSkill,
    moveSkillForOid,
    skillIndexesForOid,
} from "../../src/client/skill-management.js";
import { buildSkillFlow } from "../../src/client/skill-flow.js";
import {
    buildSkillTimeline,
    datWaitVisualUnits,
} from "../../src/client/skill-timeline.js";
                                                                            

function frame(
    frameId        ,
    occurrence        ,
    values                              = {},
)                     {
    return {
        frameId,
        occurrence,
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
        ...values,
    };
}

describe("Phase 6 skill management", () => {
    const skills = [
        { oid: 2, name: "A", startFrame: 1 },
        { oid: 2, name: "B", startFrame: 2 },
    ];

    it("duplicates after the selected skill and keeps the copy selected", () => {
        const result = duplicateSkill(skills, 0);
        assert.deepEqual(result.skills, [
            skills[0],
            { oid: 2, name: "A 副本", startFrame: 1 },
            skills[1],
        ]);
        assert.equal(result.selectedIndex, 1);
        assert.deepEqual(skills.map((skill) => skill.name), ["A", "B"]);
    });

    it("isolates deletion and one-position ordering to the active OID without mutating input", () => {
        const mixed = [
            { oid: 2, name: "A", startFrame: 1 },
            { oid: 3, name: "Other", startFrame: 10 },
            { oid: 2, name: "B", startFrame: 2 },
        ];
        assert.deepEqual(skillIndexesForOid(mixed, 2), [0, 2]);
        assert.deepEqual(moveSkillForOid(mixed, 2, 2, -1), {
            skills: [mixed[2], mixed[1], mixed[0]],
            selectedIndex: 0,
        });
        assert.deepEqual(deleteSkillForOid(mixed, 0, 2), {
            skills: [mixed[1], mixed[2]],
            selectedIndex: 1,
        });
        assert.deepEqual(mixed.map((skill) => skill.name), ["A", "Other", "B"]);
    });
});

describe("Phase 6 canvas geometry math", () => {
    it("snaps total pointer deltas and inverts horizontal movement for mirrored sprites", () => {
        assert.equal(snapDelta(2.4, 1), 2);
        assert.equal(snapDelta(2.4, 4), 4);
        assert.equal(snapDelta(-2.4, 4), -4);
        assert.deepEqual(moveDatPoint({ x: 10, y: 20 }, 4, -3, false), { x: 14, y: 17 });
        assert.deepEqual(moveDatPoint({ x: 10, y: 20 }, 4, -3, true), { x: 6, y: 17 });
    });

    it("resizes all four DAT values and rejects non-positive dimensions", () => {
        assert.deepEqual(
            resizeDatRect({ x: 10, y: 20, w: 30, h: 40 }, "nw", 4, 5, false),
            { x: 14, y: 25, w: 26, h: 35 },
        );
        assert.deepEqual(
            resizeDatRect({ x: 10, y: 20, w: 30, h: 40 }, "nw", 4, 5, true),
            { x: 10, y: 25, w: 26, h: 35 },
        );
        assert.equal(resizeDatRect({ x: 0, y: 0, w: 2, h: 2 }, "se", -2, 0, false), undefined);
    });

    it("hit-tests handles and builds local draft geometry without changing the source", () => {
        const source = {
            type: "bdy"         ,
            index: 0,
            kind: "rect"         ,
            x1: 10,
            y1: 20,
            x2: 40,
            y2: 60,
            width: 30,
            height: 40,
        };
        assert.equal(hitResizeHandle(source, 10, 20), "nw");
        assert.deepEqual(draftOverlayGeometry(source, 4, 5, "se"), {
            ...source,
            x2: 44,
            y2: 65,
            width: 34,
            height: 45,
        });
        assert.equal(source.x2, 40);
    });
});

describe("Phase 6 flow and DAT wait visual timeline", () => {
    it("lays out real and unresolved nodes inside a bounded SVG view", () => {
        const graph = buildSkillFlow([
            frame(1, 0, { next: 2, hit_a: 99 }),
            frame(2, 1),
        ], 1);
        const layout = layoutSkillFlow(graph);
        assert.ok(layout.nodes.length >= 3);
        assert.ok(layout.width >= Math.max(...layout.nodes.map((node) => node.x + node.width)));
        assert.ok(layout.height >= Math.max(...layout.nodes.map((node) => node.y + node.height)));
    });

    it("uses max(1, wait) only as a visual ratio and preserves graph discovery order", () => {
        const frames = [
            frame(1, 0, { wait: 0, next: 2 }),
            frame(2, 1, { wait: 6 }),
        ];
        const timeline = buildSkillTimeline(buildSkillFlow(frames, 1), frames);
        assert.equal(datWaitVisualUnits(-4), 1);
        assert.equal(datWaitVisualUnits(6), 6);
        assert.deepEqual(timeline.segments.map((segment) => ({
            frame: segment.node.frameId,
            wait: segment.wait,
            start: segment.startUnit,
            end: segment.endUnit,
        })), [
            { frame: 1, wait: 1, start: 0, end: 1 },
            { frame: 2, wait: 6, start: 1, end: 7 },
        ]);
        assert.equal(timeline.totalUnits, 7);
    });
});
