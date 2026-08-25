// dat-skill-flow-build:20260823083835038-310047e284004944984bc88dac7a61f8
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("render cadence client entry contract", () => {
    it("keeps the comparison page isolated from DAT mutation and presents exactly three render rates", async () => {
        const [html, client] = await Promise.all([
            readFile(resolve("render-cadence.html"), "utf8"),
            readFile(resolve("src/client/render-cadence-main.ts"), "utf8"),
        ]);
        assert.equal((html.match(/data-cadence-rate="(?:30|60|120)"/g) ?? []).length, 3);
        assert.match(html, /只读诊断入口/);
        assert.match(client, /RENDER_CADENCE_RATES/);
        assert.match(client, /sampleRenderCadence/);
        assert.match(client, /buildSkillPreviewScenario/);
        assert.doesNotMatch(client, /\/api\/project\/(?:edit|edit-batch|edit-structure|save)/);
        assert.doesNotMatch(client, /\/api\/project\/skills/);
    });

    it("uses the existing Native preview renderer and never mutates the source trace from the render loop", async () => {
        const client = await readFile(resolve("src/client/render-cadence-main.ts"), "utf8");
        const drawBody = /function drawPane\(pane: CadencePane\): void \{([\s\S]*?)\n\}/.exec(client)?.[1] ?? "";
        assert.match(drawBody, /drawPreviewCanvas/);
        assert.match(drawBody, /sampleRenderCadence/);
        assert.doesNotMatch(drawBody, /nativeTicks\s*=/);
        assert.doesNotMatch(drawBody, /\.push\(/);
    });
});
