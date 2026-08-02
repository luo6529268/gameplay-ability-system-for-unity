// dat-skill-flow-build:20260801084922031-aa0c067270d344438afd96d636b6bad9
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

import { canonicalJson, compareUtf16CodeUnits } from "../../src/sim/canonical.js";
import { canonicalizeTraceEnvelope } from "../../src/trace/envelope.js";

describe("Gate 2 locale-independent canonical ordering", () => {
    it("orders canonical and trace keys by explicit UTF-16 code units", () => {
        assert.ok(compareUtf16CodeUnits("Z", "a") < 0);
        assert.ok(compareUtf16CodeUnits("z", "ä") < 0);
        assert.equal(canonicalJson({ ä: 4, a: 2, Z: 1, z: 3 }), '{"Z":1,"a":2,"z":3,"ä":4}');

        const trace = canonicalizeTraceEnvelope({
            schemaVersion: 1,
            streamId: "order",
            sequence: 0,
            category: "simulation",
            tick: 0,
            ruleIds: [],
            payload: { ä: 4, a: 2, Z: 1, z: 3 },
            diagnostics: [],
        });
        assert.match(trace, /"payload":\{"Z":1,"a":2,"z":3,"ä":4\}/);
    });

    it("contains no localeCompare in canonical, core, or trace production sources", async () => {
        for (const relativePath of ["src/sim/canonical.ts", "src/sim/core.ts", "src/trace/envelope.ts"]) {
            const source = await readFile(resolve(relativePath), "utf8");
            assert.doesNotMatch(source, /\.localeCompare\s*\(/, relativePath);
        }
    });
});
