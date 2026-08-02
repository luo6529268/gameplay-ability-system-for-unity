// dat-skill-flow-build:20260801095957941-931e9d00d469455ea8da383f9d142eda
import assert from "node:assert/strict";
import { mkdtemp, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";
import { fileURLToPath } from "node:url";

import {
    mapReplaceFileError,
    WindowsReplaceFilePublisher,
                      
} from "../../src/server/windows-replace-adapter.js";

describe("Windows ReplaceFileW adapter", () => {
    it("maps the three documented recoverable publication errors separately", () => {
        assert.equal(mapReplaceFileError(1175).code, "unable-to-remove-replaced");
        assert.equal(mapReplaceFileError(1176).code, "unable-to-move-replacement");
        assert.equal(mapReplaceFileError(1177).code, "unable-to-move-replacement-2");
    });

    it("uses an argument-array execFile call with shell disabled", async () => {
        let invocation                                                                                   ;
        const fakeExecFile               = (file, args, options, callback) => {
            invocation = { file, args: [...args], shell: options.shell };
            callback(null, JSON.stringify({ ok: true, win32Code: 0 }), "");
            return undefined;
        };
        const publisher = new WindowsReplaceFilePublisher({
            scriptPath: resolve("scripts/windows-replace-file.ps1"),
            execFile: fakeExecFile,
            executable: "powershell.exe",
        });

        const result = await publisher.replace({
            targetPath: "C:\\root\\target.dat",
            replacementPath: "C:\\root\\replacement ; & injected.dat",
            backupPath: "C:\\root\\backup.dat",
        });

        assert.deepEqual(result, { ok: true });
        assert.equal(invocation?.file, "powershell.exe");
        assert.equal(invocation?.shell, false);
        assert.deepEqual(invocation?.args.slice(0, 6), [
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            resolve("scripts/windows-replace-file.ps1"),
        ]);
        assert.ok(invocation?.args.includes("C:\\root\\replacement ; & injected.dat"));
    });

    it("uses a caller-provided verified runtime descriptor without rereading the mutable current manifest", async () => {
        let invokedScript                    ;
        const fakeExecFile               = (_file, args, _options, callback) => {
            invokedScript = args[5];
            callback(null, JSON.stringify({ ok: true, win32Code: 0 }), "");
            return undefined;
        };
        const scriptPath = resolve("scripts/windows-replace-file.ps1");
        const publisher = new WindowsReplaceFilePublisher({
            runtimeAsset: {
                path: scriptPath,
                manifestPath: resolve("dist/builds/pinned/build-manifest.json"),
                buildId: "pinned",
            },
            execFile: fakeExecFile,
        });

        await publisher.replace({ targetPath: "target", replacementPath: "replacement", backupPath: "backup" });

        assert.equal(invokedScript, scriptPath);
    });

    it("resolves and verifies the current-build helper for the production default", async () => {
        let invokedScript                    ;
        const fakeExecFile               = (_file, args, _options, callback) => {
            invokedScript = args[5];
            callback(null, JSON.stringify({ ok: true, win32Code: 0 }), "");
            return undefined;
        };
        const publisher = new WindowsReplaceFilePublisher({ execFile: fakeExecFile });
        await publisher.replace({ targetPath: "target", replacementPath: "replacement", backupPath: "backup" });

        assert.equal(
            invokedScript,
            resolve(fileURLToPath(new URL("../../runtime/windows-replace-file.ps1", import.meta.url))),
        );
    });

    it("parses structured failure output even when PowerShell exits nonzero", async () => {
        const fakeExecFile               = (_file, _args, _options, callback) => {
            callback(Object.assign(new Error("exit 1"), { code: 1 }), JSON.stringify({
                ok: false,
                win32Code: 1176,
                message: "Unable to move replacement.",
            }), "");
            return undefined;
        };
        const publisher = new WindowsReplaceFilePublisher({
            scriptPath: resolve("scripts/windows-replace-file.ps1"),
            execFile: fakeExecFile,
        });

        assert.deepEqual(await publisher.replace({
            targetPath: "target",
            replacementPath: "replacement",
            backupPath: "backup",
        }), {
            ok: false,
            win32Code: 1176,
            code: "unable-to-move-replacement",
            message: "Unable to move replacement.",
        });
    });

    it("bundles a helper that P/Invokes ReplaceFileW without pre-copy or deletion", async () => {
        const script = await readFile(resolve("scripts/windows-replace-file.ps1"), "utf8");
        assert.match(script, /Add-Type/);
        assert.match(script, /ReplaceFileW/);
        assert.doesNotMatch(script, /Copy-Item|Remove-Item|DeleteFile|Move-Item/i);
    });

    it("performs a real ReplaceFileW publication on Windows", { skip: process.platform !== "win32" }, async () => {
        const root = await mkdtemp(join(tmpdir(), "dat-flow-replace-real-"));
        process.stdout.write(`[safe-test-artifacts] ${root}\n`);
        const targetPath = join(root, "target.dat");
        const replacementPath = join(root, "replacement.dat");
        const backupPath = join(root, "backup.dat");
        await writeFile(targetPath, "old");
        await writeFile(replacementPath, "new");

        const publisher = new WindowsReplaceFilePublisher({
            scriptPath: resolve("scripts/windows-replace-file.ps1"),
        });
        const result = await publisher.replace({ targetPath, replacementPath, backupPath });

        assert.deepEqual(result, { ok: true });
        assert.equal(await readFile(targetPath, "utf8"), "new");
        assert.equal(await readFile(backupPath, "utf8"), "old");
    });
});
