// dat-skill-flow-build:20260801130942431-de62b48d1c9448cabcbfdec33ba2eb1b
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { parseCliArguments } from "../../src/server/cli-args.js";

describe("CLI argument parsing", () => {
    it("parses supported values and preserves workspace text for registry validation", () => {
        assert.deepEqual(parseCliArguments(["--root", "assets", "--manifest", "manifest.json", "--workspace", "C:\\unsafe\\later", "--port", "0", "--allow-test-root-grant"]), {
            root: "assets",
            manifest: "manifest.json",
            workspace: "C:\\unsafe\\later",
            port: "0",
            allowTestRootGrant: true,
        });
    });

    for (const argv of [
        ["--root", "a", "--root", "b"],
        ["--manifest"],
        ["--workspace", "--root", "dist"],
        ["--allow-test-root-grant", "--allow-test-root-grant"],
        ["--allow-test-root-grant", "true"],
        ["--unknown"],
        ["--port", "12abc"],
        ["--port", "-1"],
        ["--port", "65536"],
    ]) {
        it(`rejects malformed arguments: ${argv.join(" ")}`, () => {
            assert.throws(() => parseCliArguments(argv), /(?:Duplicate|Missing|Unexpected|Unknown) CLI argument/);
        });
    }
});
