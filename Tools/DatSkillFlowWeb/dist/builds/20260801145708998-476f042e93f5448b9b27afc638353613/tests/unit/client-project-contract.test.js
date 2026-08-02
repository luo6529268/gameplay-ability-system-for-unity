// dat-skill-flow-build:20260801145708998-476f042e93f5448b9b27afc638353613
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("project-backed client contract", () => {
    it("uses project DTO endpoints rather than the Gate2 fixture", async () => {
        const [html, main, timeline] = await Promise.all([
            readFile(resolve("index.html"), "utf8"),
            readFile(resolve("src/client/main.ts"), "utf8"),
            readFile(resolve("src/client/timeline-controller.ts"), "utf8"),
        ]);

        assert.doesNotMatch(`${html}\n${main}\n${timeline}`, /GATE2_AUTHORITY_FIXTURE|Synthetic fixture|Gate2 authority fixture/);
        assert.match(main, /fetch\("\/api\/bootstrap"/);
        assert.match(main, /fetch\("\/api\/project"/);
        assert.match(main, /fetch\("\/api\/project\/open"/);
        assert.match(main, /fetch\("\/api\/project\/edit"/);
        for (const id of [
            "object-select", "frame-select", "frame-editor", "sprite-canvas", "play-toggle", "step-once", "reset-timeline",
            "hit-a", "hit-d", "hit-j", "hit-fj", "hit-fa", "hit-da", "hit-ua", "hit-ja", "hit-dj", "hit-uj",
        ]) assert.match(html, new RegExp(`id="${id}"`));
        assert.match(timeline, /export function createProjectTimeline/);
    });
});
