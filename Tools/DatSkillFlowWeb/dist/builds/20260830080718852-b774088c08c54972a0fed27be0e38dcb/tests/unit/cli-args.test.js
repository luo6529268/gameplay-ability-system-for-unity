// dat-skill-flow-build:20260830080718852-b774088c08c54972a0fed27be0e38dcb
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
        assert.deepEqual(parseCliArguments(["--root", "assets", "--manifest", "manifest.json", "--workspace", "C:\\unsafe\\later", "--data-txt", "Assets/NTSD/Config/data.txt", "--asset-workspace", "J:\\game", "--patch-workspace", "J:\\patches", "--patch-index", "C:\\cache\\patch-index.json", "--port", "0", "--allow-test-root-grant", "--read-only"]), {
            root: "assets",
            manifest: "manifest.json",
            workspace: "C:\\unsafe\\later",
            dataTxt: "Assets/NTSD/Config/data.txt",
            assetWorkspace: "J:\\game",
            patchWorkspace: "J:\\patches",
            patchIndex: "C:\\cache\\patch-index.json",
            port: "0",
            allowTestRootGrant: true,
            readOnly: true,
        });
        const ordinary = parseCliArguments(["--workspace", "C:\\repo"]);
        assert.equal(ordinary.dataTxt, undefined);
        assert.equal(ordinary.readOnly, false);
        assert.throws(() => parseCliArguments(["--data-txt", "data/data.txt"]), /requires --workspace/);
        assert.throws(() => parseCliArguments(["--asset-workspace", "J:\\game"]), /requires --workspace/);
        assert.throws(() => parseCliArguments(["--workspace", "C:\\repo", "--patch-workspace", "J:\\patches"]), /provided together/);
        assert.throws(() => parseCliArguments(["--workspace", "C:\\repo", "--patch-index", "C:\\cache\\patch-index.json"]), /provided together/);
    });

    for (const argv of [
        ["--root", "a", "--root", "b"],
        ["--manifest"],
        ["--workspace", "--root", "dist"],
        ["--data-txt", "--root", "dist"],
        ["--asset-workspace", "--root", "dist"],
        ["--patch-workspace", "--root", "dist"],
        ["--patch-index", "--root", "dist"],
        ["--allow-test-root-grant", "--allow-test-root-grant"],
        ["--allow-test-root-grant", "true"],
        ["--read-only", "--read-only"],
        ["--read-only", "true"],
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
