// dat-skill-flow-build:20260801074931001-68465ec2c2c94e46bab158c7a28070ac
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

                         
                                         
 

const expectedClientFiles = [
    "index.html", "src/client/main.js", "src/client/styles.css", "src/client/timeline-controller.js",
    "src/sim/canonical.js", "src/sim/constants.js", "src/sim/core.js", "src/sim/frame-tick.js",
    "src/sim/index.js", "src/sim/input.js", "src/sim/rules.js", "src/sim/timeline.js", "src/sim/types.js",
    "src/authority/gate2-sim-ledger.js", "src/authority/ledger.js", "src/trace/envelope.js",
    "src/diagnostics/envelope.js", "src/validation/strict.js",
].sort();

describe("Gate 2 browser build allowlist", () => {
    it("allows exactly the browser entry, client UI, simulation graph, and its authority/trace validation dependencies", async () => {
        const manifest = JSON.parse(await readFile(resolve("dist/build-manifest.json"), "utf8"))                 ;
        const clientPaths = manifest.clientFiles.map((entry) => entry.path).sort();

        assert.deepEqual(clientPaths, expectedClientFiles);
        assert.ok(clientPaths.every((path) => !path.startsWith("tests/")));
        assert.ok(clientPaths.every((path) => !path.startsWith("src/server/")));
    });
});
