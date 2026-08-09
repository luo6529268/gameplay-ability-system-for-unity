// dat-skill-flow-build:20260807164850212-06d2878bd4fe43b59adae8ccf5a51820
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
