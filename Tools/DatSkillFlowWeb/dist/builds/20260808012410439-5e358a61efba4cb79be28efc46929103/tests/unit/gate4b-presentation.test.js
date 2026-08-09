// dat-skill-flow-build:20260808012410439-5e358a61efba4cb79be28efc46929103
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { canonicalJson, createSimulation } from "../../src/sim/index.js";
                                                                                
import {
    createPresentationCamera,
    projectPresentationEntities,
    stepPresentationCamera,
} from "../../src/presentation/index.js";

function frame(id        , state = 1)                     {
    return { id, state, wait: 100, next: 0 };
}

function entity(stableId        , slot        , overrides                         = {})                {
    return { stableId, slot, oid: slot + 1, rawObjectType: 0, runtimeObjectType: 0, frame: 0, hp: 500, xInt: 0, yInt: 0, zInt: 0, frames: [frame(0)], ...overrides };
}

describe("Gate4B1 presentation camera", () => {
    it("uses exact primary subjects, state-14 facing treatment, target clamps, and override", () => {
        const world = createSimulation({ entities: [
            entity("primary-normal", 0, { xInt: 800, facing: 0 }),
            entity("primary-death", 1, { xInt: 1200, facing: 1, frames: [frame(0, 14)] }),
            entity("fallback-not-primary", 2, { xInt: 10_000, runtimeObjectType: 1 }),
            entity("slot-eight", 8, { xInt: 10_000 }),
            entity("dead", 3, { xInt: 10_000, hp: 0 }),
        ] });
        const sample = stepPresentationCamera(createPresentationCamera({ cameraMaxOverride: 500 }), world, 2000);
        assert.deepEqual(sample.subjectSlots, [0, 1]);
        assert.equal(sample.subjectKind, "primary");
        assert.equal(sample.targetX, 500);
        assert.deepEqual([sample.camera.cameraX, sample.camera.cameraVel, sample.camera.cameraMaxOverride], [5, 5, 500]);
    });

    it("uses all living character-DAT fallback subjects, then x=800 when none exist", () => {
        const fallbackWorld = createSimulation({ entities: [
            entity("fallback", 20, { xInt: 600, runtimeObjectType: 1 }),
            entity("not-character-dat", 2, { rawObjectType: 3, runtimeObjectType: 1, xInt: 10_000 }),
        ] });
        const fallback = stepPresentationCamera(createPresentationCamera(), fallbackWorld, 2000);
        assert.deepEqual([fallback.subjectKind, fallback.subjectSlots, fallback.targetX, fallback.camera.cameraX], ["fallback", [20], 203, 2]);

        const empty = stepPresentationCamera(createPresentationCamera(), createSimulation({ entities: [] }), 2000);
        assert.deepEqual([empty.subjectKind, empty.subjectSlots, empty.targetX, empty.camera.cameraX], ["synthetic", [], 403, 4]);
    });

    it("uses C++ truncation and forced +/-1, clamps final X, and resets at width <= 794", () => {
        const noSubjects = createSimulation({ entities: [] });
        const negative = stepPresentationCamera(createPresentationCamera({ cameraX: 100, cameraVel: 1, cameraMaxOverride: 1 }), noSubjects, 2000);
        assert.deepEqual([negative.targetX, negative.camera.cameraVel, negative.camera.cameraX], [1, -1, 99]);
        const clamped = stepPresentationCamera(createPresentationCamera({ cameraX: 1200, cameraVel: 100 }), noSubjects, 1000);
        assert.equal(clamped.camera.cameraX, 206);
        for (const width of [794, 700]) {
            const reset = stepPresentationCamera(createPresentationCamera({ cameraX: 9, cameraVel: -4, cameraMaxOverride: 3 }), noSubjects, width);
            assert.deepEqual([reset.camera.cameraX, reset.camera.cameraVel, reset.camera.cameraMaxOverride], [0, 0, 3]);
        }
    });
});

describe("Gate4B1 presentation projection", () => {
    it("stable-sorts active entities by zInt then slot and projects exact integer anchors", () => {
        const world = createSimulation({ entities: [
            entity("slot-five", 5, { xInt: 50, yInt: -2, zInt: 10 }),
            entity("slot-two", 2, { xInt: 20, yInt: 3, zInt: 10 }),
            entity("front", 7, { xInt: 70, yInt: -5, zInt: -1 }),
            entity("inactive", 1, { active: false, zInt: -100 }),
        ] });
        const projected = projectPresentationEntities(world, { cameraX: 4, renderOffsetBySlot: { 2: 3 } });
        assert.deepEqual(projected.map((value) => value.slot), [7, 2, 5]);
        assert.deepEqual(projected.map((value) => [value.renderOffsetX, value.screenX, value.screenY]), [
            [0, 66, -6], [3, 19, 13], [0, 46, 8],
        ]);
    });

    it("defaults deferred perspective offsets to zero and repeated sampling never mutates canonical state", () => {
        const world = createSimulation({ entities: [entity("stable", 0, { xInt: 40, yInt: -3, zInt: 8 })] });
        const before = canonicalJson(world);
        const camera = createPresentationCamera({ cameraX: 2 });
        const firstCamera = stepPresentationCamera(camera, world, 1200);
        const secondCamera = stepPresentationCamera(camera, world, 1200);
        const first = projectPresentationEntities(world, { cameraX: camera.cameraX });
        const second = projectPresentationEntities(world, { cameraX: camera.cameraX });
        assert.deepEqual(firstCamera, secondCamera);
        assert.deepEqual(first, second);
        assert.deepEqual([first[0]?.renderOffsetX, first[0]?.screenX, first[0]?.screenY], [0, 38, 5]);
        assert.equal(canonicalJson(world), before);
    });
});
