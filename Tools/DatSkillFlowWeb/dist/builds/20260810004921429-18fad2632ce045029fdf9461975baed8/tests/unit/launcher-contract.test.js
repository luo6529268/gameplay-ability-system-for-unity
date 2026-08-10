// dat-skill-flow-build:20260810004921429-18fad2632ce045029fdf9461975baed8
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

describe("one-click launcher contract", () => {
    it("selects a writable project or test workspace without exposing repository Config to reset operations", async () => {
        const launcherPath = resolve("scripts/start-local.ps1");
        const launcherBytes = await readFile(launcherPath);
        const launcher = await readFile(launcherPath, "utf8");

        assert.deepEqual([...launcherBytes.subarray(0, 3)], [0xef, 0xbb, 0xbf]);
        assert.match(launcher, /\[ValidateSet\("Project", "Test"\)\]\s*\[string\]\$Mode/);
        assert.match(launcher, /Non-interactive startup requires -Mode Project or -Mode Test/);
        assert.match(launcher, /请选择启动模式/);
        assert.match(launcher, /"1" \{ return "Project" \}/);
        assert.match(launcher, /"2" \{ return "Test" \}/);
        assert.match(launcher, /"0" \{ return \$null \}/);
        assert.match(launcher, /if \(\$launchMode -eq "Project" -and \$ResetWorkspace\)/);
        assert.match(launcher, /\$workspace = \$repositoryRoot/);
        assert.match(launcher, /Join-Path \$repositoryRoot "\.dat-skill-flow\\skills\.json"/);
        assert.match(launcher, /\$workspace = \$testWorkspace/);
        assert.match(launcher, /Join-Path \$testWorkspace "\.dat-skill-flow\\skills\.json"/);
        assert.match(launcher, /"--workspace", \$workspace/);

        const modeResolution = launcher.indexOf("$launchMode = Resolve-LaunchMode");
        const cancellation = launcher.indexOf("已取消启动。");
        const postSelectionPrerequisites = launcher.indexOf("\nAssert-StartupPrerequisites\n", modeResolution);
        assert.ok(modeResolution >= 0);
        assert.ok(cancellation > modeResolution);
        assert.ok(postSelectionPrerequisites > cancellation);
        const prerequisites = launcher.match(/function Assert-StartupPrerequisites \{([\s\S]*?)\n\}/);
        assert.ok(prerequisites);
        assert.match(prerequisites[1] , /Get-Command npm\.cmd/);
        assert.match(prerequisites[1] , /scripts\\build\.mjs/);
        assert.match(prerequisites[1] , /scripts\\start\.mjs/);
        assert.ok(launcher.indexOf("\n    Initialize-TestWorkspace", postSelectionPrerequisites) > postSelectionPrerequisites);

        const projectBranch = launcher.match(/if \(\$launchMode -eq "Project"\) \{([\s\S]*?)\n\}\nelse \{/);
        assert.ok(projectBranch);
        assert.doesNotMatch(projectBranch[1] , /Copy-Item|Remove-Item|Initialize-TestWorkspace/);

        const testInitializer = launcher.match(/function Initialize-TestWorkspace \{([\s\S]*?)\n\}/);
        assert.ok(testInitializer);
        assert.match(launcher, /\[switch\]\$ResetWorkspace/);
        assert.match(testInitializer[1] , /if \(\$ResetWorkspace -and \(Test-Path -LiteralPath \$testWorkspace\)\)/);
        assert.match(testInitializer[1] , /Remove-Item -LiteralPath \$testWorkspace -Recurse -Force/);
        assert.match(testInitializer[1] , /if \(-not \(Test-Path -LiteralPath \$testConfig -PathType Container\)\)/);
        assert.match(testInitializer[1] , /Copy-Item -Path \(Join-Path \$sourceConfig "\*"\) -Destination \$testConfig -Recurse/);
    });

    it("opens the browser only after an ephemeral listener is ready", async () => {
        const launcher = await readFile(resolve("scripts/start-local.ps1"), "utf8");

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

    it("builds and injects the workspace Native adapter before starting the server", async () => {
        const launcher = await readFile(resolve("scripts/start-local.ps1"), "utf8");

        assert.match(launcher, /Join-Path \$toolRoot "native\\bin\\dat_preview_cli\.exe"/);
        assert.match(launcher, /Join-Path \$toolRoot "scripts\\build-native-preview\.ps1"/);
        assert.match(launcher, /\$assetWorkspace = "J:\\QQFile\\NTSD 2\.4\.1"/);
        assert.doesNotMatch(launcher, /\$previewExecutable\s*=\s*"J:\\QQFile/);
        assert.ok(launcher.indexOf('& $previewBuildScript') < launcher.indexOf('& npm.cmd run build'));
        assert.match(launcher, /DAT_SKILL_FLOW_CPP_PREVIEW_EXECUTABLE/);
        assert.match(launcher, /DAT_SKILL_FLOW_CPP_GAME_ROOT/);
        const environmentSet = launcher.indexOf("SetEnvironmentVariable($previewEnvironmentName, $previewExecutable");
        const processStart = launcher.indexOf("[System.Diagnostics.Process]::Start($processInfo)");
        const environmentRestore = launcher.indexOf("SetEnvironmentVariable($previewEnvironmentName, $previousPreviewExecutable");
        assert.ok(environmentSet >= 0 && processStart > environmentSet && environmentRestore > processStart);
        assert.match(launcher, /function Test-WebBuildRequired/);
        assert.match(launcher, /if \(Test-WebBuildRequired\)/);
        assert.match(launcher, /DAT Skill Flow Web build is up to date\./);
    });

    it("publishes after the editable session while verified entry previews keep warming in the background", async () => {
        const cli = await readFile(resolve("src/server/cli.ts"), "utf8");
        const preparation = cli.indexOf("prepareDefaultSession");
        const listener = cli.indexOf("const origin = await listenLoopback");

        assert.ok(preparation >= 0);
        assert.ok(listener > preparation);
        assert.match(cli, /Preparing DAT catalog and editable character session/);
        assert.match(cli, /prepared\.warmup\.then/);
        assert.match(cli, /continue warming in the background/);
        assert.match(cli, /DAT preparation complete/);
    });
});
