import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("one-click launcher contract", () => {
    it("preserves the test copy and opens the browser only after an ephemeral listener is ready", async () => {
        const launcher = await readFile(resolve("scripts/start-local.ps1"), "utf8");

        assert.match(launcher, /\[switch\]\$ResetWorkspace/);
        assert.match(launcher, /if \(\$ResetWorkspace -and \(Test-Path -LiteralPath \$testWorkspace\)\)/);
        assert.match(launcher, /if \(-not \(Test-Path -LiteralPath \$testConfig -PathType Container\)\)/);
        assert.doesNotMatch(launcher, /Get-AvailableLoopbackPort|Start-Sleep -Seconds 2/);
        assert.match(launcher, /"--port", "0"/);
        assert.match(launcher, /\$processInfo\.RedirectStandardError = \$false/);
        assert.match(launcher, /\$process\.Kill\(\)/);

        const readyCheck = launcher.indexOf("Dat Skill Flow server listening at");
        const browserOpen = launcher.indexOf("Start-Process $url");
        assert.ok(readyCheck >= 0);
        assert.ok(browserOpen > readyCheck);
        assert.match(launcher, /One-click startup prerequisites passed\./);
    });
});
