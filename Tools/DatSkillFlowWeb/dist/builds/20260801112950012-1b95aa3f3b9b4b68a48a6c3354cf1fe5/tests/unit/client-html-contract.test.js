// dat-skill-flow-build:20260801112950012-1b95aa3f3b9b4b68a48a6c3354cf1fe5
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
