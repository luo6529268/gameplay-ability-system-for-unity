// dat-skill-flow-build:20260811074054162-11f1dac9f90141028c320086a9da3ed5
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    clampPanelWidths,
    defaultPanelWidths,
    PANEL_SEPARATOR_WIDTH,
    resizePanelWidths,
                     
} from "../../src/client/panel-layout.js";

function assertFits(containerWidth        , layout             )       {
    assert.equal(
        layout.left + layout.middle + layout.right + PANEL_SEPARATOR_WIDTH * 2,
        containerWidth,
    );
    assert.ok(layout.middle >= layout.middleMinimum);
    assert.ok(layout.left >= layout.leftMinimum);
    assert.ok(layout.left <= layout.leftMaximum);
    assert.ok(layout.right >= layout.rightMinimum);
    assert.ok(layout.right <= layout.rightMaximum);
}

describe("desktop panel layout", () => {
    it("fits compact defaults at 1024 while preserving the preview minimum", () => {
        const layout = clampPanelWidths(1024, defaultPanelWidths(1024));

        assertFits(1024, layout);
        assert.equal(layout.middleMinimum, 360);
        assert.equal(layout.left, 230);
        assert.equal(layout.right, 286);
        assert.equal(layout.middle, 496);
    });

    it("fits wide defaults at 1440 while preserving the preview minimum", () => {
        const layout = clampPanelWidths(1440, defaultPanelWidths(1440));

        assertFits(1440, layout);
        assert.equal(layout.middleMinimum, 420);
        assert.equal(layout.left, 286);
        assert.equal(layout.right, 330);
        assert.equal(layout.middle, 812);
    });

    it("clamps non-finite and extreme requested widths to finite bounds", () => {
        const layout = clampPanelWidths(1440, {
            left: Number.POSITIVE_INFINITY,
            right: 10_000,
        });

        assertFits(1440, layout);
    });

    it("preserves the right side while dragging the left separator", () => {
        const start = clampPanelWidths(1024, defaultPanelWidths(1024));
        const layout = resizePanelWidths(1024, start, "left", 10_000);

        assertFits(1024, layout);
        assert.equal(layout.right, start.right);
        assert.equal(layout.left, layout.leftMaximum);
        assert.equal(layout.middle, 360);
    });

    it("preserves the left side while dragging the right separator", () => {
        const start = clampPanelWidths(1024, defaultPanelWidths(1024));
        const layout = resizePanelWidths(1024, start, "right", -10_000);

        assertFits(1024, layout);
        assert.equal(layout.left, start.left);
        assert.equal(layout.right, layout.rightMaximum);
        assert.equal(layout.middle, 360);
    });

    it("reclamps previous wide widths after the container shrinks", () => {
        const wide = clampPanelWidths(1440, { left: 420, right: 460 });
        const compact = clampPanelWidths(900, wide);

        assertFits(900, compact);
        assert.equal(compact.middleMinimum, 360);
    });

    it("keeps the desktop edge above 850px usable without overflow", () => {
        const layout = clampPanelWidths(851, defaultPanelWidths(851));

        assertFits(851, layout);
    });

    it("reclamps an oversized opposite panel during a narrow drag", () => {
        const layout = resizePanelWidths(
            851,
            { left: 420, right: 460 },
            "left",
            100,
        );

        assertFits(851, layout);
    });
});
