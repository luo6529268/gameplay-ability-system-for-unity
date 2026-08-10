// dat-skill-flow-build:20260809163945218-dc591ea430b343f5b7fdfc50792c1b0b
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    BoundedLruCache,
    findFrameFieldCapability,
    lastFrameForId,
    mergePreview,
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

    it("uses C++ renderer sprite placement and facing mirror", () => {
        assert.deepEqual(spritePlacement({ xInt: 100, yInt: 20, zInt: 30, renderOffsetX: 4, cameraX: 10, centerX: 12, centerY: 7, width: 40, facing: 0 }), { x: 82, y: 43, mirror: false });
        assert.deepEqual(spritePlacement({ xInt: 100, yInt: 20, zInt: 30, renderOffsetX: 4, cameraX: 10, centerX: 12, centerY: 7, width: 40, facing: 1 }), { x: 66, y: 43, mirror: true });
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
