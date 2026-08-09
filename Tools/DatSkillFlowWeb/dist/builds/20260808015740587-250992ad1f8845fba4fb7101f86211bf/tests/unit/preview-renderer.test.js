// dat-skill-flow-build:20260808015740587-250992ad1f8845fba4fb7101f86211bf
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    effectivePreviewPic,
    sortPreviewEntities,
    spriteSheetColumnCount,
    stageParallaxOffset,
} from "../../src/client/preview-renderer.js";
                                                                

describe("preview renderer sprite ranges", () => {
    it("uses the Native row field for horizontal sprite-sheet layout", () => {
        assert.equal(spriteSheetColumnCount({ row: 6, col: 4 }), 6);
        assert.equal(spriteSheetColumnCount({ row: 7, col: 10 }), 7);
    });

    it("does not infer a horizontal layout from col when row is absent", () => {
        assert.equal(spriteSheetColumnCount({ col: 10 }), 0);
    });

    it("prefers the Native effective render pic over the DAT frame pic", () => {
        assert.equal(effectivePreviewPic({ renderPic: 140, pic: 12, frame: 300, oid: 2, slot: 0, x: 0, y: 0, z: 0 }, { pic: 12 }         ), 140);
        assert.equal(effectivePreviewPic({ pic: 12, frame: 300, oid: 2, slot: 0, x: 0, y: 0, z: 0 }, { pic: 12 }         ), 12);
    });

    it("sorts scene entities by Native z_int while preserving equal-z slot order", () => {
        const entities = [
            { slot: 5, oid: 33, frame: 1, x: 0, y: 0, z: 0, zInt: 500 },
            { slot: 2, oid: 204, frame: 1, x: 0, y: 0, z: 0, zInt: 450 },
            { slot: 1, oid: 121, frame: 1, x: 0, y: 0, z: 0, zInt: 500 },
        ];
        assert.deepEqual(sortPreviewEntities(entities).map((entity) => entity.slot), [2, 5, 1]);
    });

    it("uses Native stage parallax projection", () => {
        assert.equal(stageParallaxOffset(960, 794, 950, 0), 0);
        assert.equal(stageParallaxOffset(960, 794, 950, 166), -156);
        assert.equal(stageParallaxOffset(800, 794, 950, 12), -312);
    });
});
