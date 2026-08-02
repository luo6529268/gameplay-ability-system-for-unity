// dat-skill-flow-build:20260801130629806-73fa502c70bf450c961a7f4f9e72cbf7
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
    ]) {
        it(`rejects malformed arguments: ${argv.join(" ")}`, () => {
            assert.throws(() => parseCliArguments(argv), /(?:Duplicate|Missing|Unexpected|Unknown) CLI argument/);
        });
    }
});
