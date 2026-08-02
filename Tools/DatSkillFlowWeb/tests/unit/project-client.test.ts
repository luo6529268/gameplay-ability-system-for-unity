import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { mergePreview, spritePlacement } from "../../src/client/project-client.js";

describe("project client helpers", () => {
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
});
