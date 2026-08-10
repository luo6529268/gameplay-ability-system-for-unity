// dat-skill-flow-build:20260810141832204-2dd606079e7245e0be58751ba247b935
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
        const openRequest = openProject.indexOf('request("\/api\/project\/open"');
        const closePrevious = openProject.indexOf("closeProjectSession(previousProject.sessionId)");
        assert.ok(openRequest >= 0 && closePrevious > openRequest, "new session must open before the previous one closes");
        assert.doesNotMatch(openProject.slice(0, openRequest), /project = undefined|clearDraft\(\)|images\.clear\(\)/);
        assert.match(openProject, /const nextProject = normalize\(response\)/);
        assert.match(openProject, /project = nextProject/);
        const switchProject = main.match(/function switchObject\([\s\S]*?\n}\nasync function start/)?.[0] ?? "";
        assert.match(switchProject, /角色切换失败，当前项目已保留/);
        assert.match(switchProject, /objectSelect\.value = loadedObjectKey/);
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
