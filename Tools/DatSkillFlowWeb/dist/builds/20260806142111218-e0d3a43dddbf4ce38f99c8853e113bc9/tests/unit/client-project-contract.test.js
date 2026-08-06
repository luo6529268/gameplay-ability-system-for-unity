// dat-skill-flow-build:20260806142111218-e0d3a43dddbf4ce38f99c8853e113bc9
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("project-backed client contract", () => {
    it("uses project DTO endpoints rather than the Gate2 fixture", async () => {
        const [html, main, projectClient, previewRenderer, editorSupport, latestScheduler] = await Promise.all([
            readFile(resolve("index.html"), "utf8"),
            readFile(resolve("src/client/main.ts"), "utf8"),
            readFile(resolve("src/client/project-client.ts"), "utf8"),
            readFile(resolve("src/client/preview-renderer.ts"), "utf8"),
            readFile(resolve("src/client/editor-support.ts"), "utf8"),
            readFile(resolve("src/client/latest-task-scheduler.ts"), "utf8"),
        ]);
        const clientSource = `${main}\n${projectClient}\n${previewRenderer}\n${editorSupport}\n${latestScheduler}`;

        assert.doesNotMatch(`${html}\n${clientSource}`, /GATE2_AUTHORITY_FIXTURE|Synthetic fixture|Gate2 authority fixture/);
        assert.match(html, /<html lang="zh-CN">/);
        assert.match(html, /NTSD DAT 技能流程编辑器/);
        assert.match(html, /正在连接本地服务/);
        assert.match(html, /覆盖 DAT 文件/);
        assert.match(html, /技能列表/);
        assert.match(html, /当前技能帧流程/);
        assert.match(html, /帧属性检查/);
        assert.match(main, /已载入/);
        assert.match(clientSource, /请求失败（HTTP/);
        for (const path of ["/api/bootstrap", "/api/project", "/api/project/open", "/api/project/preview", "/api/project/edit", "/api/project/close", "/api/project/skills"]) assert.match(main, new RegExp(path.replaceAll("/", "\\/")));
        assert.match(main, /nativeTicks/);
        assert.match(main, /tokenHeader/);
        assert.match(main, /currentRuntimeFrame/);
        assert.match(main, /runtime\?\.occurrence !== frame\.occurrence/);
        assert.match(clientSource, /findFrameFieldCapability/);
        assert.match(clientSource, /frameOccurrence/);
        assert.match(main, /input\.disabled\s*=\s*capability\s*===\s*undefined/);
        assert.match(main, /fieldDraft \|\| project\.dirty/);
        assert.match(main, /objectSwitchQueue\.then/);
        assert.match(main, /createLatestTaskScheduler/);
        assert.match(latestScheduler, /pending\?\.resolve\(\{ status: "superseded" \}\)/);
        assert.match(clientSource, /primaryPreviewEntity/);
        assert.doesNotMatch(`${main}\n${previewRenderer}`, /find\(\(entity\) => entity\.oid === 2\) \?\?/);
        assert.match(main, /option\.disabled = oid !== 2/);
        assert.match(main, /input\.valueAsNumber/);
        assert.match(main, /Number\.isSafeInteger/);
        assert.match(main, /INT32_MIN/);
        assert.match(main, /frameSelect\.disabled = actionBusy\.edit/);
        assert.match(main, /if \(actionBusy\.edit\) return/);
        assert.match(main, /fieldDraft !== undefined \|\| actionBusy\.save \|\| actionBusy\.edit/);
        assert.match(main, /actionBusy\.edit \|\| !hit \|\| !blockSelect/);
        assert.match(main, /project\?\.writable !== true/);
        assert.match(main, /fallback 资源中，当前会话为只读预览/);
        assert.match(main, /backup\.name/);
        assert.match(main, /rawValue/);
        assert.doesNotMatch(main.match(/function renderFields\(\)[\s\S]*?\nfunction render\(\)/)?.[0] ?? "", /clearDraft\(\)/);
        assert.match(main, /runExclusiveAction/);
        assert.match(main, /event\.persisted/);
        assert.match(main, /beforeunload/);
        assert.equal((html.match(/aria-busy="false"/g) ?? []).length, 3);
        assert.match(previewRenderer, /number\(range\?\.row\)/);
        for (const id of [
            "object-select", "frame-select", "frame-editor", "sprite-canvas", "play-toggle", "step-once", "reset-timeline",
            "skill-list", "flow-list", "block-select", "timeline-segments", "new-skill", "edit-skill",
        ]) assert.match(html, new RegExp(`id="${id}"`));
    });
});
