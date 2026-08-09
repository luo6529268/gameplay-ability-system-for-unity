// dat-skill-flow-build:20260807120527935-93d2dc941d324769a8e3f6c449ec6179
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { parseCliArguments, parsePortValue } from "../../src/server/cli-args.js";

describe("CLI argument parsing", () => {
    it("accepts only full decimal port values in range", () => {
        assert.equal(parsePortValue("0"), 0);
        assert.equal(parsePortValue("4173"), 4173);
        assert.equal(parsePortValue("65535"), 65535);
        for (const raw of ["12abc", "-1", "65536", ""]) {
            assert.throws(() => parsePortValue(raw), /Invalid port/);
        }
    });

    it("parses supported values and preserves workspace text for registry validation", () => {
        assert.deepEqual(parseCliArguments(["--root", "assets", "--manifest", "manifest.json", "--workspace", "C:\\unsafe\\later", "--data-txt", "Assets/NTSD/Config/data.txt", "--asset-workspace", "J:\\game", "--port", "0", "--allow-test-root-grant"]), {
            root: "assets",
            manifest: "manifest.json",
            workspace: "C:\\unsafe\\later",
            dataTxt: "Assets/NTSD/Config/data.txt",
            assetWorkspace: "J:\\game",
            port: "0",
            allowTestRootGrant: true,
        });
        assert.equal(parseCliArguments(["--workspace", "C:\\repo"]).dataTxt, undefined);
        assert.throws(() => parseCliArguments(["--data-txt", "data/data.txt"]), /requires --workspace/);
        assert.throws(() => parseCliArguments(["--asset-workspace", "J:\\game"]), /requires --workspace/);
    });

    for (const argv of [
        ["--root", "a", "--root", "b"],
        ["--manifest"],
        ["--workspace", "--root", "dist"],
        ["--data-txt", "--root", "dist"],
        ["--asset-workspace", "--root", "dist"],
        ["--allow-test-root-grant", "--allow-test-root-grant"],
        ["--allow-test-root-grant", "true"],
        ["--unknown"],
        ["--port", "12abc"],
        ["--port", "-1"],
        ["--port", "65536"],
    ]) {
        it(`rejects malformed arguments: ${argv.join(" ")}`, () => {
            assert.throws(() => parseCliArguments(argv), /(?:Duplicate|Missing|Unexpected|Unknown) CLI argument|Invalid port/);
        });
    }
});
