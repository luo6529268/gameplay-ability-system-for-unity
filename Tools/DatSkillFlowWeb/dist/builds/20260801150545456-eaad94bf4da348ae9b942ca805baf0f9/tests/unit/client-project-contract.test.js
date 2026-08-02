// dat-skill-flow-build:20260801150545456-eaad94bf4da348ae9b942ca805baf0f9
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("project-backed client contract", () => {
    it("uses project DTO endpoints rather than the Gate2 fixture", async () => {
        const [html, main] = await Promise.all([
            readFile(resolve("index.html"), "utf8"),
            readFile(resolve("src/client/main.ts"), "utf8"),
        ]);

        assert.doesNotMatch(`${html}\n${main}`, /GATE2_AUTHORITY_FIXTURE|Synthetic fixture|Gate2 authority fixture/);
        for (const path of ["/api/bootstrap", "/api/project", "/api/project/open", "/api/project/preview", "/api/project/edit"]) assert.match(main, new RegExp(path.replaceAll("/", "\\/")));
        assert.match(main, /nativeTicks/);
        assert.match(main, /tokenHeader/);
        assert.match(main, /fieldIds/);
        assert.match(main, /number\(range\.row\)/);
        for (const id of [
            "object-select", "frame-select", "frame-editor", "sprite-canvas", "play-toggle", "step-once", "reset-timeline",
            "hit-a", "hit-d", "hit-j", "hit-fj", "hit-fa", "hit-da", "hit-ua", "hit-ja", "hit-dj", "hit-uj",
        ]) assert.match(html, new RegExp(`id="${id}"`));
    });
});
