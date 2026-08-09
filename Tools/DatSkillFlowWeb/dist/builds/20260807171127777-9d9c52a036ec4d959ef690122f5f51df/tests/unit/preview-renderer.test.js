// dat-skill-flow-build:20260807171127777-9d9c52a036ec4d959ef690122f5f51df
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { spriteSheetColumnCount } from "../../src/client/preview-renderer.js";

describe("preview renderer sprite ranges", () => {
    it("uses the DAT col field for horizontal sprite-sheet layout", () => {
        assert.equal(spriteSheetColumnCount({ row: 4, col: 6 }), 6);
        assert.equal(spriteSheetColumnCount({ row: 10, col: 7 }), 7);
    });

    it("does not infer a horizontal layout from row when col is absent", () => {
        assert.equal(spriteSheetColumnCount({ row: 10 }), 0);
    });
});
