// dat-skill-flow-build:20260806222345881-71018388a2fb4031926eb2894c5181ff
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("Gate 2 browser document contract", () => {
    it("uses valid UTF-8 Chinese source text and keeps the loopback status element correctly closed", async () => {
        const html = await readFile(resolve("index.html"), "utf8");

        assert.doesNotMatch(html, /\uFFFD/, "index.html must not contain replacement glyphs");
        assert.match(html, /<html lang="zh-CN">/);
        assert.match(
            html,
            /<p id="server-status" data-testid="server-status" role="status">正在连接本地服务……<\/p>/,
        );
    });
});
