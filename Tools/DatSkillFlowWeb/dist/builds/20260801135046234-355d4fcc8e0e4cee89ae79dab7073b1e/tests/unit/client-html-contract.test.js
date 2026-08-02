// dat-skill-flow-build:20260801135046234-355d4fcc8e0e4cee89ae79dab7073b1e
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("Gate 2 browser document contract", () => {
    it("uses ASCII source text and keeps the loopback status element correctly closed", async () => {
        const html = await readFile(resolve("index.html"), "utf8");

        assert.doesNotMatch(html, /[^\x00-\x7F]/, "index.html must not contain mojibake or replacement glyphs");
        assert.match(
            html,
            /<p id="server-status" data-testid="server-status" role="status">Connecting to local server\.\.\.<\/p>/,
        );
    });
});
