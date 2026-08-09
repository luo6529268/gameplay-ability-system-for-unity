// dat-skill-flow-build:20260808055425459-f67be6018c754d698cb5c87bc701ce6a
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

import {
    COMPACT_DEFAULT_PANEL_WIDTHS,
    COMPACT_PANEL_MAXIMUM,
    LEFT_PANEL_MAXIMUM,
    LEFT_PANEL_MINIMUM,
    MOBILE_PANEL_MAXIMUM,
    PANEL_SEPARATOR_WIDTH,
    RIGHT_PANEL_MAXIMUM,
    RIGHT_PANEL_MINIMUM,
    WIDE_DEFAULT_PANEL_WIDTHS,
} from "../../src/client/panel-layout.js";

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

    it("exposes both desktop splitters as keyboard-operable ARIA separators", async () => {
        const html = await readFile(resolve("index.html"), "utf8");

        assert.match(html, /<section id="editor-grid" class="editor-grid"/);
        for (const id of ["left-panel-separator", "right-panel-separator"]) {
            assert.match(
                html,
                new RegExp(
                    `<div\\s+id="${id}"[\\s\\S]*?role="separator"[\\s\\S]*?`
                    + `aria-orientation="vertical"[\\s\\S]*?aria-controls="[^"]+"[\\s\\S]*?`
                    + `aria-valuemin="\\d+"[\\s\\S]*?aria-valuemax="\\d+"[\\s\\S]*?`
                    + `aria-valuenow="\\d+"[\\s\\S]*?tabindex="0"></div>`,
                ),
            );
        }
        assert.match(html, /aria-controls="left-panel preview-panel"/);
        assert.match(html, /aria-controls="preview-panel inspector-panel"/);
        assert.match(html, new RegExp(`aria-valuemin="${LEFT_PANEL_MINIMUM}"`));
        assert.match(html, new RegExp(`aria-valuemax="${LEFT_PANEL_MAXIMUM}"`));
        assert.match(html, new RegExp(`aria-valuemin="${RIGHT_PANEL_MINIMUM}"`));
        assert.match(html, new RegExp(`aria-valuemax="${RIGHT_PANEL_MAXIMUM}"`));
    });

    it("uses five desktop grid columns and hides splitters in the mobile tab layout", async () => {
        const styles = await readFile(resolve("src/client/styles.css"), "utf8");

        assert.match(styles, new RegExp(`--left-panel-width:\\s*${WIDE_DEFAULT_PANEL_WIDTHS.left}px`));
        assert.match(styles, new RegExp(`--right-panel-width:\\s*${WIDE_DEFAULT_PANEL_WIDTHS.right}px`));
        assert.match(styles, new RegExp(`--panel-separator-width:\\s*${PANEL_SEPARATOR_WIDTH}px`));
        assert.match(styles, /\.left-panel-separator\s*\{\s*grid-column:\s*2;/);
        assert.match(styles, /\.preview-panel\s*\{\s*grid-column:\s*3;/);
        assert.match(styles, /\.right-panel-separator\s*\{\s*grid-column:\s*4;/);
        assert.match(styles, /\.inspector-panel\s*\{\s*grid-column:\s*5;/);
        assert.match(styles, /\.timeline-panel\s*\{\s*grid-column:\s*1\s*\/\s*-1;/);
        const compactStyles = styles.match(
            new RegExp(`@media \\(max-width: ${COMPACT_PANEL_MAXIMUM}px\\) \\{([\\s\\S]*?)`
                + `@media \\(max-width: ${MOBILE_PANEL_MAXIMUM}px\\)`),
        )?.[1] ?? "";
        assert.match(compactStyles, new RegExp(`--left-panel-width:\\s*${COMPACT_DEFAULT_PANEL_WIDTHS.left}px`));
        assert.match(compactStyles, new RegExp(`--right-panel-width:\\s*${COMPACT_DEFAULT_PANEL_WIDTHS.right}px`));
        const mobileStyles = styles.match(
            new RegExp(`@media \\(max-width: ${MOBILE_PANEL_MAXIMUM}px\\) \\{([\\s\\S]*?)`
                + "@media \\(max-width: 520px\\)"),
        )?.[1] ?? "";
        assert.match(mobileStyles, /\.panel-separator\s*\{\s*display:\s*none;/);
    });
});
