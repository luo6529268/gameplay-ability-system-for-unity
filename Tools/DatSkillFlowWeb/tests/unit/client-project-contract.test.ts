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
        assert.match(html, /<html lang="zh-CN">/);
        assert.match(html, /NTSD DAT 技能流程编辑器/);
        assert.match(html, /正在连接本地服务/);
        assert.match(html, /保存并覆盖 DAT/);
        assert.match(main, /已载入/);
        assert.match(main, /请求失败（HTTP/);
        assert.match(main, /字段 \$\{key\} 没有可编辑标识/);
        for (const path of ["/api/bootstrap", "/api/project", "/api/project/open", "/api/project/preview", "/api/project/edit", "/api/project/close"]) assert.match(main, new RegExp(path.replaceAll("/", "\\/")));
        assert.match(main, /nativeTicks/);
        assert.match(main, /tokenHeader/);
        assert.match(main, /fieldIds/);
        assert.match(main, /project\.dirty\s*&&\s*!window\.confirm/);
        assert.match(main, /objectSwitchQueue\.then/);
        assert.match(main, /event\.persisted/);
        assert.match(main, /beforeunload/);
        assert.match(main, /number\(range\.row\)/);
        for (const id of [
            "object-select", "frame-select", "frame-editor", "sprite-canvas", "play-toggle", "step-once", "reset-timeline",
            "hit-a", "hit-d", "hit-j", "hit-fj", "hit-fa", "hit-da", "hit-ua", "hit-ja", "hit-dj", "hit-uj",
        ]) assert.match(html, new RegExp(`id="${id}"`));
    });
});
