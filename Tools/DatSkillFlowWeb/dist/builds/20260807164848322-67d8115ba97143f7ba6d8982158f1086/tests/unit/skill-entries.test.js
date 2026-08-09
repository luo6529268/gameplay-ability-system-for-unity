// dat-skill-flow-build:20260807164848322-67d8115ba97143f7ba6d8982158f1086
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { deriveSkillEntries } from "../../src/client/skill-entries.js";
                                                                            

function frame(
    frameId        ,
    occurrence        ,
    label        ,
    values                              = {},
)                     {
    return {
        frameId,
        occurrence,
        label,
        pic: 0,
        state: 3,
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

describe("automatic DAT skill entries", () => {
    it("coalesces contiguous labels and keeps separated equal labels as distinct entries", () => {
        const entries = deriveSkillEntries([
            frame(0, 0, "standing", { state: 0, next: 1 }),
            frame(1, 1, "standing", { state: 0, next: 2 }),
            frame(2, 2, "standing", { state: 0, next: 3 }),
            frame(3, 3, "standing", { state: 0, next: 999 }),
            frame(5, 4, "walking", { state: 1, next: 999 }),
            frame(6, 5, "walking", { state: 1, next: 999 }),
            frame(60, 6, "punch", { next: 61 }),
            frame(61, 7, "punch", { next: 999 }),
            frame(65, 8, "punch", { next: 66 }),
            frame(66, 9, "punch", { next: 999 }),
        ], 2);

        assert.equal(entries.find((entry) => entry.startFrame === 0)?.segmentFrameCount, 4);
        assert.equal(entries.find((entry) => entry.startFrame === 5)?.segmentFrameCount, 2);
        assert.deepEqual(
            entries.filter((entry) => entry.label === "punch").map((entry) => entry.startFrame),
            [60, 65],
        );
        assert.equal(entries.find((entry) => entry.startFrame === 0)?.category, "base");
    });

    it("uses the exact hit target as the skill first frame and records every trigger source", () => {
        const entries = deriveSkillEntries([
            frame(0, 0, "standing", { state: 0, next: 1, hit_Uj: 300 }),
            frame(1, 1, "standing", { state: 0, next: 999, hit_Uj: 300 }),
            frame(291, 2, "charge", { next: 292 }),
            frame(292, 3, "charge", { next: 293 }),
            frame(293, 4, "charge", { next: 294, hit_a: 294 }),
            frame(294, 5, "charge", { next: 999 }),
            frame(300, 6, "rasenganshuriken", { state: 15, next: 301 }),
            frame(301, 7, "rasenganshuriken", { state: 15, next: 999 }),
        ], 2);

        const rasenganshuriken = entries.find((entry) => entry.startFrame === 300);
        assert.equal(rasenganshuriken?.category, "input");
        assert.deepEqual(rasenganshuriken?.triggers, [{
            key: "hit_Uj",
            sourceFrames: [0, 1],
        }]);
        assert.equal(rasenganshuriken?.segmentFrameCount, 2);
        assert.equal(entries.find((entry) => entry.startFrame === 294)?.category, "input");
        assert.equal(entries.some((entry) => entry.startFrame === 291), true);
    });

    it("applies sidecar presentation metadata without creating DAT entries", () => {
        const source = [frame(300, 0, "rasenganshuriken", { state: 15 })];
        const entries = deriveSkillEntries(source, 2, [{
            oid: 2,
            startFrame: 300,
            displayName: "螺旋手里剑",
            group: "奥义",
            order: -1,
            pinned: true,
            hidden: true,
            notes: "展示备注",
        }, {
            oid: 2,
            startFrame: 123,
            displayName: "DAT 中不存在",
        }]);

        assert.equal(entries.length, 1);
        assert.equal(entries[0]?.displayName, "螺旋手里剑");
        assert.equal(entries[0]?.group, "奥义");
        assert.equal(entries[0]?.hidden, true);
        assert.equal(source[0]?.label, "rasenganshuriken");
    });
});
