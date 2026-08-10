// dat-skill-flow-build:20260809142459931-af2d35f37925409c8a5d2bb3e75da3e8
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("project-open lifecycle", () => {
    it("loads a character without implicitly waiting for an arbitrary action preview", async () => {
        const main = await readFile(resolve("src/client/main.ts"), "utf8");
        const openProject = main.match(/async function open\([\s\S]*?\n}\nfunction switchObject/)?.[0] ?? "";

        assert.match(openProject, /if \(playing\) setPlaying\(false\)/);
        assert.match(openProject, /selectedSkillIndex = -1/);
        assert.doesNotMatch(openProject, /await selectSkill\(/);
        assert.match(main, /localizedResponseError\(response\.status, path, body\)/);
    });
});
