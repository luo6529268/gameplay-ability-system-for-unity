// dat-skill-flow-build:20260823084021356-6dd4887eb05645bab8bbbfd11ce0ccbe
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    Gate2TimelinePreviewController,
    GATE2_AUTHORITY_FIXTURE,
} from "../../src/client/timeline-controller.js";
import { serializeCanonicalSnapshot } from "../../src/sim/index.js";

describe("Gate 2 browser timeline controller", () => {
    it("labels its fixture as authority-only and drives canonical state through timeline commands", () => {
        const controller = new Gate2TimelinePreviewController();
        const initial = controller.canonical;

        assert.match(GATE2_AUTHORITY_FIXTURE.label, /authority fixture/i);
        assert.match(GATE2_AUTHORITY_FIXTURE.label, /no project loaded/i);
        assert.equal(controller.viewModel().rateLabel, "30 fps nominal / 33 ms");

        controller.step();
        assert.equal(controller.canonical.tickIndex, initial.tickIndex + 1);
        assert.notEqual(serializeCanonicalSnapshot(controller.canonical), serializeCanonicalSnapshot(initial));
    });

    it("makes play/advance equivalent to an explicit step, supports seek, and loops inclusively", () => {
        const stepped = new Gate2TimelinePreviewController();
        stepped.step({ source: "test" });

        const playing = new Gate2TimelinePreviewController();
        playing.play();
        playing.advance({ source: "test" });
        assert.equal(
            serializeCanonicalSnapshot(playing.canonical),
            serializeCanonicalSnapshot(stepped.canonical),
        );

        stepped.step();
        stepped.setLoopBounds(1, 2);
        stepped.setLoopEnabled(true);
        stepped.play();
        stepped.advance();
        assert.equal(stepped.canonical.tickIndex, 1);

        stepped.seek(2);
        assert.equal(stepped.canonical.tickIndex, 2);
    });

    it("samples presentation at arbitrary render frequencies without changing canonical state", () => {
        const controller = new Gate2TimelinePreviewController();
        controller.step();
        const before = serializeCanonicalSnapshot(controller.canonical);

        for (let index = 0; index < 240; index += 1) {
            const view = controller.viewModel((index % 101) / 100);
            assert.equal(view.tick, 1);
        }

        assert.equal(serializeCanonicalSnapshot(controller.canonical), before);
    });
});
