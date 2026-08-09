// dat-skill-flow-build:20260808033647462-6e31ea5f987d44709aca2ef260417fa4
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("Gate 2 browser render synchronization contract", () => {
    it("keeps requestAnimationFrame presentation-only and never rewrites editable loop values", async () => {
        const source = await readFile(resolve("src/client/main.ts"), "utf8");
        const renderBody = /function renderFrame\(\): void \{([\s\S]*?)\n\}/.exec(source)?.[1];
        const readOnlySyncBody = /function syncReadOnlyUi\(\): void \{([\s\S]*?)\n\}/.exec(source)?.[1];

        assert.ok(renderBody);
        assert.match(renderBody, /drawPreview\(\)/);
        assert.doesNotMatch(renderBody, /sync(?:Ui|EditableUi|ReadOnlyUi)\s*\(/);
        assert.ok(readOnlySyncBody);
        assert.doesNotMatch(readOnlySyncBody, /(?:loopStartInput|loopEndInput)\.value\s*=/);
    });
});
