// dat-skill-flow-build:20260809165031646-f9674e8092774b199412f1007b147b27
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    authoredTraceStartFrame,
    buildFrameEntryCatalog,
    buildSkillPreviewScenario,
    deriveSkillEntries,
} from "../../src/client/skill-entries.js";
                                                                            

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
    it("keeps direct frame inspection direct and builds ordinary jump playback from idle input", () => {
        const frames = [
            frame(0, 0, "idle", { state: 0, wait: 1, next: 0 }),
            frame(210, 1, "jump", { state: 4, wait: 1, next: 211 }),
            frame(211, 2, "jump", { state: 4, wait: 1, next: 212 }),
            frame(212, 3, "jump", { state: 4, wait: 1, next: 0 }),
            frame(213, 4, "dash", { state: 4, wait: 1, next: 214 }),
        ];

        const jump = deriveSkillEntries(frames, 2).find((entry) => entry.startFrame === 210);
        assert.equal(jump?.segmentFrameCount, 3);
        assert.equal(authoredTraceStartFrame(frames, 210), 210);
        assert.equal(authoredTraceStartFrame(frames, 211), 211);
        assert.equal(authoredTraceStartFrame(frames, 212), 212);
        assert.equal(authoredTraceStartFrame(frames, 213), 213);
        assert.deepEqual(buildSkillPreviewScenario(frames, jump ), {
            startFrame: 210,
            initialFrame: 0,
            inputPlan: [{ tick: 2, keys: ["K"] }],
            ticks: 120,
        });
    });

    it("never rewrites a directly selected frame into a guessed predecessor", () => {
        assert.equal(authoredTraceStartFrame([
            frame(209, 0, "jump", { next: 210 }),
            frame(210, 1, "jump", { next: 999 }),
            frame(211, 2, "jump", { next: -212 }),
            frame(212, 3, "jump"),
        ], 212), 212);
        assert.equal(authoredTraceStartFrame([
            frame(210, 0, "prepare", { next: 211 }),
            frame(211, 1, "jump", { next: 212 }),
            frame(212, 2, "jump"),
        ], 212), 212);
        assert.equal(authoredTraceStartFrame([frame(212, 0, "jump")], 212), 212);
    });

    it("previews walking and running through the Native input state machine instead of next=999", () => {
        const frames = [
            frame(0, 0, "standing", { state: 0, next: 999 }),
            frame(5, 1, "walking", { state: 1, next: 999 }),
            frame(9, 2, "running", { state: 2, next: 999 }),
        ];
        const entries = deriveSkillEntries(frames, 2);
        const walking = entries.find((entry) => entry.startFrame === 5) ;
        const running = entries.find((entry) => entry.startFrame === 9) ;

        assert.equal(walking.category, "base");
        assert.equal(walking.nativeTrigger, "Native：按住 D 行走");
        const walkingScenario = buildSkillPreviewScenario(frames, walking);
        assert.equal(walkingScenario.initialFrame, 0);
        assert.deepEqual(walkingScenario.inputPlan.slice(0, 2), [
            { tick: 2, keys: ["D"] },
            { tick: 3, keys: ["D"] },
        ]);
        assert.deepEqual(walkingScenario.inputPlan.at(-1), { tick: 32, keys: ["D"] });

        assert.equal(running.category, "base");
        const runningScenario = buildSkillPreviewScenario(frames, running);
        assert.deepEqual(runningScenario.inputPlan.slice(0, 3), [
            { tick: 2, keys: ["D"] },
            { tick: 4, keys: ["D"] },
            { tick: 5, keys: ["D"] },
        ]);
    });

    it("drives F265 and F271 from their DAT trigger source with physical input timing", () => {
        const frames = [
            frame(0, 0, "idle", { state: 0, hit_Ua: 265, hit_Dj: 271 }),
            frame(265, 1, "clone jump", { state: 3, next: 266 }),
            frame(266, 2, "clone jump", { state: 3, next: 267, dvy: -7 }),
            frame(271, 3, "mass clone", { state: 3, next: 272 }),
            frame(272, 4, "mass clone", { state: 3, next: 273 }),
        ];
        const entries = deriveSkillEntries(frames, 2);

        assert.deepEqual(buildSkillPreviewScenario(
            frames,
            entries.find((entry) => entry.startFrame === 265) ,
        ), {
            startFrame: 265,
            initialFrame: 0,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["W"] },
                { tick: 6, keys: ["J"] },
            ],
            ticks: 120,
        });
        assert.deepEqual(buildSkillPreviewScenario(
            frames,
            entries.find((entry) => entry.startFrame === 271) ,
        ), {
            startFrame: 271,
            initialFrame: 0,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["S"] },
                { tick: 6, keys: ["K"] },
            ],
            ticks: 120,
        });
    });

    it("coalesces base-state labels without promoting arbitrary action segments to entries", () => {
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
        assert.deepEqual(entries.filter((entry) => entry.label === "punch"), []);
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
        assert.equal(entries.some((entry) => entry.startFrame === 291), false);
    });

    it("applies sidecar presentation metadata without creating DAT entries", () => {
        const source = [
            frame(0, 0, "standing", { state: 0, hit_Uj: 300 }),
            frame(300, 1, "rasenganshuriken", { state: 15 }),
        ];
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

        const skill = entries.find((entry) => entry.startFrame === 300);
        assert.equal(skill?.displayName, "螺旋手里剑");
        assert.equal(skill?.group, "奥义");
        assert.equal(skill?.hidden, true);
        assert.equal(source[1]?.label, "rasenganshuriken");
        assert.equal(entries.some((entry) => entry.startFrame === 123), false);
    });

    it("classifies duplicate catching frames globally and links their runtime branch to the real input entry", () => {
        const catchingItr = {
            kind: 3, x: 0, y: 0, w: 1, h: 1, dvx: 0, dvy: 0, fall: 0, bdefend: 0,
            injury: 0, arest: 0, vrest: 0, effect: 0, attacking: 0, catchingact: 120,
            catchingact2: 0, caughtact: 0, caughtact2: 0, respond: 0, pickingact: 0, pickedact: 0,
            throwvx: 0, throwvy: 0, zwidth: 15, throwvz: 0, throwinjury: 0,
        };
        const catalog = buildFrameEntryCatalog([
            frame(0, 0, "standing", { state: 0, hit_Fa: 240 }),
            frame(120, 1, "catching", { next: 122 }),
            frame(122, 2, "catching", { next: 123 }),
            frame(123, 3, "catching-old", { next: 999 }),
            frame(123, 4, "catching", { next: 124 }),
            frame(124, 5, "catching", { next: 999 }),
            frame(240, 6, "rasengan", { next: 258 }),
            frame(258, 7, "rasengan", { next: 999, itrs: [catchingItr] }),
        ], 2);

        assert.deepEqual(catalog.entries.map((entry) => entry.startFrame), [0, 240]);
        const old123 = catalog.byOccurrence.get(3) ;
        const effective123 = catalog.byOccurrence.get(4) ;
        assert.equal(old123.role, "overridden");
        assert.equal(old123.definitionCount, 2);
        assert.equal(effective123.effective, true);
        assert.equal(effective123.definitionCount, 2);
        assert.equal(effective123.role, "runtime-branch");
        assert.deepEqual(effective123.ownerStartFrames, [240]);
        const catching = catalog.byOccurrence.get(1) ;
        assert.equal(catching.role, "runtime-branch");
        assert.deepEqual(catching.ownerStartFrames, [240]);
        assert.deepEqual(catching.references.map((reference) => [reference.sourceFrame, reference.field]), [
            [258, "itr[0].catchingact"],
        ]);
        assert.equal(catalog.entries.some((entry) => [120, 123, 124].includes(entry.startFrame)), false);
    });

    it("offers only verified deterministic Native code entries and drives them from idle input", () => {
        const frames = [
            frame(0, 0, "standing", { state: 0 }),
            frame(60, 1, "punch"),
            frame(90, 2, "dash attack", { state: 3 }),
            frame(102, 3, "running defend", { state: 7 }),
            frame(110, 4, "defend", { state: 7 }),
            frame(210, 5, "jump", { state: 4, next: 211 }),
            frame(211, 6, "jump", { state: 4, next: 212 }),
            frame(212, 7, "jump", { state: 4 }),
            frame(213, 8, "dash", { state: 5 }),
        ];
        const entries = deriveSkillEntries(frames, 2);
        assert.equal(entries.some((entry) => entry.startFrame === 60), false);
        assert.deepEqual(
            entries.filter((entry) => entry.category === "engine").map((entry) => entry.startFrame),
            [90, 102, 110, 210, 213],
        );
        assert.equal(entries.find((entry) => entry.startFrame === 110)?.category, "engine");
        assert.deepEqual(buildSkillPreviewScenario(
            frames,
            entries.find((entry) => entry.startFrame === 90) ,
        ).inputPlan.at(-1), { tick: 12, keys: ["D", "J"] });
        const jump = entries.find((entry) => entry.startFrame === 210) ;
        assert.equal(jump.category, "engine");
        assert.equal(jump.nativeTrigger, "Native：K 跳跃");
        assert.deepEqual(buildSkillPreviewScenario(frames, jump), {
            startFrame: 210,
            initialFrame: 0,
            inputPlan: [{ tick: 2, keys: ["K"] }],
            ticks: 120,
        });
    });
});
