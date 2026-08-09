// dat-skill-flow-build:20260808012007938-f37312af0cd84a31b68f574016559631
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { spriteSheetColumnCount } from "../../src/client/preview-renderer.js";

describe("preview renderer sprite ranges", () => {
    it("uses the Native row field for horizontal sprite-sheet layout", () => {
        assert.equal(spriteSheetColumnCount({ row: 6, col: 4 }), 6);
        assert.equal(spriteSheetColumnCount({ row: 7, col: 10 }), 7);
    });

    it("does not infer a horizontal layout from col when row is absent", () => {
        assert.equal(spriteSheetColumnCount({ col: 10 }), 0);
    });
});
