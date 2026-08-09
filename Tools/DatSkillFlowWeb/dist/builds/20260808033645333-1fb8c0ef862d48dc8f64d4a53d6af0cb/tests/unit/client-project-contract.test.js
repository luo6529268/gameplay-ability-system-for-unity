// dat-skill-flow-build:20260808033645333-1fb8c0ef862d48dc8f64d4a53d6af0cb
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("project-backed client contract", () => {
    it("uses project DTO endpoints rather than the Gate2 fixture", async () => {
        const [html, main, projectClient, previewRenderer, editorSupport, latestScheduler, skillEntries, panelLayout] = await Promise.all([
            readFile(resolve("index.html"), "utf8"),
            readFile(resolve("src/client/main.ts"), "utf8"),
            readFile(resolve("src/client/project-client.ts"), "utf8"),
            readFile(resolve("src/client/preview-renderer.ts"), "utf8"),
            readFile(resolve("src/client/editor-support.ts"), "utf8"),
            readFile(resolve("src/client/latest-task-scheduler.ts"), "utf8"),
            readFile(resolve("src/client/skill-entries.ts"), "utf8"),
            readFile(resolve("src/client/panel-layout.ts"), "utf8"),
        ]);
        const clientSource = `${main}\n${projectClient}\n${previewRenderer}\n${editorSupport}\n${latestScheduler}\n${skillEntries}\n${panelLayout}`;

        assert.doesNotMatch(`${html}\n${clientSource}`, /GATE2_AUTHORITY_FIXTURE|Synthetic fixture|Gate2 authority fixture/);
        assert.match(html, /<html lang="zh-CN">/);
        assert.match(html, /NTSD DAT 技能流程编辑器/);
        assert.match(html, /正在连接本地服务/);
        assert.match(html, /覆盖 DAT 文件/);
        assert.match(html, /状态与技能入口/);
        assert.match(html, /当前技能帧流程/);
        assert.match(html, /帧属性检查/);
        assert.match(main, /已载入/);
        assert.match(clientSource, /请求失败（HTTP/);
        for (const path of [
            "/api/bootstrap", "/api/project", "/api/project/open", "/api/project/preview", "/api/project/edit-batch",
            "/api/project/edit-structure", "/api/project/close", "/api/project/skills",
        ]) assert.match(main, new RegExp(path.replaceAll("/", "\\/")));
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
        assert.match(main, /number\(Number\(option\?\.dataset\.oid\)\)/);
        assert.match(main, /input\.valueAsNumber/);
        assert.match(main, /Number\.isSafeInteger/);
        assert.match(main, /INT32_MIN/);
        assert.match(main, /frameSelect\.disabled = selectionLocked/);
        assert.equal((main.match(/if \(isSelectionLocked\(\)\) return/g) ?? []).length, 3);
        assert.match(main, /!dirty \|\| actionBusy\.save \|\| isSelectionLocked\(\)/);
        assert.match(main, /actionBusy\.edit \|\| project\?\.writable !== true/);
        assert.match(main, /project\?\.writable !== true/);
        assert.match(main, /fallback 资源中，当前会话为只读预览/);
        assert.match(main, /backup\.name/);
        assert.match(main, /rawValue/);
        assert.doesNotMatch(main.match(/function renderFields\(\)[\s\S]*?\nfunction render\(\)/)?.[0] ?? "", /clearDraft\(\)/);
        assert.match(main, /runExclusiveAction/);
        assert.match(main, /Object\.values\(actionBusy\)\.some\(Boolean\)/);
        assert.match(main, /syncFlowEdgeEditor\(currentFlow\(\)\)/);
        assert.match(main, /if \(kind === "edit"\) renderFlow\(\)/);
        assert.match(main, /event\.persisted/);
        assert.match(main, /beforeunload/);
        assert.equal((html.match(/aria-busy="false"/g) ?? []).length, 3);
        assert.match(previewRenderer, /spriteSheetColumnCount\(range\)/);
        assert.match(previewRenderer, /effectivePreviewPic/);
        assert.match(previewRenderer, /sortPreviewEntities/);
        assert.match(previewRenderer, /stageParallaxOffset/);
        assert.match(main, /normalizePreviewStage/);
        for (const id of [
            "object-select", "frame-select", "frame-editor", "sprite-canvas", "play-toggle", "step-once", "reset-timeline",
            "skill-list", "flow-list", "block-select", "timeline-segments", "edit-skill", "show-hidden-skills",
            "skill-name", "skill-group", "skill-order", "skill-pinned", "skill-hidden", "skill-notes",
            "flow-svg", "flow-edge-target",
            "apply-flow-edge", "copy-frame", "delete-frame", "new-block", "copy-block", "delete-block", "grid-four",
            "editor-grid", "left-panel-separator", "right-panel-separator",
        ]) assert.match(html, new RegExp(`id="${id}"`));
        assert.match(main, /deriveSkillEntries\(project\.frames, project\.oid, skillState\.metadata\)/);
        assert.match(main, /entriesByStartFrame\(skillState\.skills\)/);
        assert.match(skillEntries, /if \(rawTarget === 0\) continue/);
        assert.match(skillEntries, /target = frameById\.get\(rawTarget\)/);
        assert.match(main, /skill\.oid !== project\.oid/);
        assert.match(main, /sidecarStatus === "invalid"/);
        assert.match(html, /不修改 DAT/);
        const skillActionState = main.match(/function syncSkillActionState\(\)[\s\S]*?\n\}/)?.[0] ?? "";
        assert.match(skillActionState, /isSelectionLocked\(\)/);
        const selectionLock = main.match(/function isSelectionLocked\(\)[\s\S]*?\n\}/)?.[0] ?? "";
        assert.match(selectionLock, /isActionBusy\(\)/);
        assert.match(selectionLock, /fieldDraft !== undefined/);
        assert.match(selectionLock, /canvasInteraction !== undefined/);
        assert.match(main, /objectSelect\.disabled = selectionLocked/);
        assert.match(main, /const editorLocked = isActionBusy\(\) \|\| canvasInteraction !== undefined/);
        assert.match(main, /new TextEncoder\(\)\.encode\(value\)\.byteLength/);
        assert.match(main, /validateSkillText\(nameInput, 256/);
        assert.match(main, /validateSkillText\(notesInput, 4096/);
        assert.match(main, /applyBatchEdits/);
        assert.match(main, /canvas\.setPointerCapture/);
        assert.match(main, /canvasDraftGeometry/);
        assert.match(main, /resizeDatRect/);
        assert.match(main, /separator\.setPointerCapture/);
        assert.match(main, /hasPointerCapture/);
        assert.match(main, /pointercancel/);
        assert.match(main, /lostpointercapture/);
        assert.match(main, /event\.key === "Escape"/);
        assert.match(main, /new ResizeObserver/);
        assert.match(main, /mobilePanelQuery\.addEventListener\("change"/);
        assert.match(main, /aria-valuetext/);
        assert.match(panelLayout, /middleMinimumForWidth/);
        assert.match(panelLayout, /resizePanelWidths/);
        assert.match(main, /buildSkillTimeline/);
        assert.match(main, /renderFlowSvg/);
        assert.match(html, /DAT wait 视觉轴/);
        assert.match(html, /重定向已有字段/);
        assert.doesNotMatch(html, />\s*0 毫秒\s*</);
    });
});