// dat-skill-flow-build:20260809171729507-cc12cc111f8a4b74833629ef08e6123f
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
        assert.match(openProject, /if \(!isUnknownProjectSessionError\(error\)\) throw error/);
        const preview = main.match(/async function preview\([\s\S]*?\n}\nfunction commitPreview/)?.[0] ?? "";
        assert.match(preview, /allowSessionRecovery = true/);
        assert.match(preview, /projectSessionRecoveryDecision/);
        assert.match(preview, /await open\(objectKey, oid\)/);
        assert.match(preview, /await preview\(scenario, false\)/);
        assert.match(preview, /当前页面有未保存修改/);
        const framePreview = main.match(/async function previewFrameWithinCompleteAction\([\s\S]*?\n}\nasync function selectSkill/)?.[0] ?? "";
        assert.match(framePreview, /const previousSkillIndex = selectedSkillIndex/);
        assert.match(framePreview, /selectedSkillIndex = previousSkillIndex/);
    });
});
