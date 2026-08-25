// dat-skill-flow-build:20260823084021356-6dd4887eb05645bab8bbbfd11ce0ccbe
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    BoundedLruCache,
    findFrameFieldCapability,
    lastFrameForId,
    mergePreview,
    movePreviewPosition,
    nativePreviewPlaybackBounds,
    previewIntentCacheKey,
    primaryPreviewEntity,
    spritePlacement,
} from "../../src/client/project-client.js";

describe("project client helpers", () => {
    it("keys complete preview scenarios and evicts the least recently used response", () => {
        const base = {
            sessionId: "session",
            revision: 3,
            startFrame: 265,
            initialFrame: 0,
            inputPlan: [{ tick: 2, keys: ["L"] }],
            ticks: 120,
        };
        assert.equal(previewIntentCacheKey(base), previewIntentCacheKey({ ...base }));
        assert.notEqual(previewIntentCacheKey(base), previewIntentCacheKey({ ...base, startFrame: 271 }));
        assert.notEqual(previewIntentCacheKey(base), previewIntentCacheKey({
            ...base,
            initial: {
                p1: { x: 200, y: 0, z: 300 },
                p2: { x: 500, y: 0, z: 400 },
            },
        }));

        const cache = new BoundedLruCache                (2);
        cache.set("jump", 1);
        cache.set("clone", 2);
        assert.equal(cache.get("jump"), 1);
        cache.set("projectile", 3);
        assert.equal(cache.get("clone"), undefined);
        assert.equal(cache.get("jump"), 1);
        assert.equal(cache.get("projectile"), 3);
    });

    it("merges preview updates without discarding the loaded project", () => {
        const project = { name: "Naruto", frames: [{ frameId: 0 }], ranges: [{ row: 3 }], revision: 1, nativeTicks: [{ tick: 0 }] };
        const merged = mergePreview(project, 2, [{ tick: 1 }]);
        assert.equal(merged.name, "Naruto");
        assert.deepEqual(merged.frames, project.frames);
        assert.equal(merged.revision, 2);
        assert.deepEqual(merged.nativeTicks, [{ tick: 1 }]);
    });

    it("never clips playback to a completion tick before the selected action starts", () => {
        assert.deepEqual(nativePreviewPlaybackBounds({
            rootSkillStartedTick: 18,
            progressEndTick: 17,
            playbackEndTick: 17,
        }, 121), {
            actionStart: 18,
            progressEnd: -1,
            playbackEnd: 120,
        });
        assert.deepEqual(nativePreviewPlaybackBounds({
            rootSkillStartedTick: null,
            progressEndTick: 14,
            playbackEndTick: 14,
        }, 121), {
            actionStart: -1,
            progressEnd: -1,
            playbackEnd: 120,
        });
        assert.deepEqual(nativePreviewPlaybackBounds({
            rootSkillStartedTick: 15,
            progressEndTick: 29,
            playbackEndTick: 36,
        }, 121), {
            actionStart: 15,
            progressEnd: 29,
            playbackEnd: 36,
        });
    });

    it("uses C++ renderer sprite placement and facing mirror", () => {
        assert.deepEqual(spritePlacement({ xInt: 100, yInt: 20, zInt: 30, renderOffsetX: 4, cameraX: 10, centerX: 12, centerY: 7, width: 40, facing: 0 }), { x: 82, y: 43, mirror: false });
        assert.deepEqual(spritePlacement({ xInt: 100, yInt: 20, zInt: 30, renderOffsetX: 4, cameraX: 10, centerX: 12, centerY: 7, width: 40, facing: 1 }), { x: 66, y: 43, mirror: true });
    });

    it("maps canvas dragging to bounded Native X/Z positions without changing height", () => {
        assert.deepEqual(movePreviewPosition(
            { x: 320, y: -15, z: 500 },
            42.4,
            -80.6,
            { width: 1000, zMin: 200, zMax: 550 },
        ), { x: 362, y: -15, z: 419 });
        assert.deepEqual(movePreviewPosition(
            { x: 980, y: 0, z: 540 },
            200,
            100,
            { width: 1000, zMin: 200, zMax: 550 },
        ), { x: 1000, y: 0, z: 550 });
    });

    it("selects the Native preview primary entity only by stable slot zero", () => {
        const clone = { slot: 5, oid: 2, frame: 301 };
        const primary = { slot: 0, oid: 2, frame: 300 };
        assert.strictEqual(primaryPreviewEntity([clone, primary]), primary);
        assert.strictEqual(primaryPreviewEntity([primary, clone]), primary);
        assert.equal(primaryPreviewEntity([clone]), undefined);
    });

    it("uses complete server locators for duplicate frames and duplicate fields", () => {
        const frames = [
            { frameId: 7, occurrence: 0, pic: 1 },
            { frameId: 7, occurrence: 1, pic: 2 },
        ];
        const fields = [
            { fieldId: "first", key: "pic", scope: "frame", occurrence: 0, frameId: 7, frameOccurrence: 1 },
            { fieldId: "last", key: "pic", scope: "frame", occurrence: 1, frameId: 7, frameOccurrence: 1 },
            { fieldId: "other-frame", key: "pic", scope: "frame", occurrence: 1, frameId: 7, frameOccurrence: 0 },
        ];

        assert.deepEqual(lastFrameForId(frames, 7), frames[1]);
        assert.equal(findFrameFieldCapability(fields, frames[1] , "pic")?.fieldId, "last");
        assert.equal(findFrameFieldCapability(fields, frames[0] , "pic")?.fieldId, "other-frame");
    });
});
