// dat-skill-flow-build:20260810154230737-504b0d643abc4a8796b3a37c14d40ce6
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

                         
                                         
 

const expectedClientFiles = [
    "index.html", "src/client/main.js", "src/client/canvas-geometry-edit.js", "src/client/complete-action-selection.js", "src/client/editor-support.js", "src/client/flow-layout.js", "src/client/flow-svg.js", "src/client/latest-task-scheduler.js", "src/client/panel-layout.js", "src/client/preview-renderer.js", "src/client/project-client.js", "src/client/runtime-frame-timeline.js", "src/client/skill-entries.js", "src/client/skill-flow.js", "src/client/skill-timeline.js", "src/client/overlay-geometry.js", "src/client/styles.css", "src/client/timeline-controller.js",
    "src/presentation/camera.js", "src/presentation/index.js", "src/presentation/projection.js",
    "src/sim/canonical.js", "src/sim/catalog.js", "src/sim/constants.js", "src/sim/core.js", "src/sim/frame-tick.js",
    "src/sim/index.js", "src/sim/input.js", "src/sim/motion.js", "src/sim/opoint.js", "src/sim/rng.js", "src/sim/rules.js", "src/sim/timeline.js", "src/sim/types.js",
    "src/sim/wpoint.js",
    "src/sim/world.js", "src/authority/gate2-sim-ledger.js", "src/authority/gate4-motion-ledger.js", "src/authority/gate4b-presentation-ledger.js", "src/authority/ledger.js", "src/trace/envelope.js",
    "src/diagnostics/envelope.js", "src/validation/strict.js",
].sort();

describe("Gate 2 browser build allowlist", () => {
    it("allows exactly the browser entry, client modules, simulation graph, and authority dependencies", async () => {
        const manifest = JSON.parse(await readFile(resolve("dist/build-manifest.json"), "utf8"))                 ;
        const clientPaths = manifest.clientFiles.map((entry) => entry.path).sort();

        assert.deepEqual(clientPaths, expectedClientFiles);
        assert.ok(clientPaths.every((path) => !path.startsWith("tests/")));
        assert.ok(clientPaths.every((path) => !path.startsWith("src/server/")));
    });
});
