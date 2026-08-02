// dat-skill-flow-build:20260801074740310-8d91d06bd66148d984a694a39cc29b33
import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, stat, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import {
    isTransientCurrentManifestPointerError,
    verifyManifestOutput,
} from "../../scripts/manifest-integrity.mjs";

                         
                          
                    
                       
                        
                        
                                                                                          
                                                                                    
                                                                                        
 

async function readCurrentManifest()                         {
    let lastError         ;
    for (let attempt = 0; attempt < 32; attempt += 1) {
        try {
            return JSON.parse(await readFile(resolve(process.cwd(), "dist/build-manifest.json"), "utf8"))                 ;
        } catch (error) {
            if (!isTransientCurrentManifestPointerError(error) || attempt === 31) {
                throw error;
            }
            lastError = error;
            await new Promise      ((resolveDelay) => setTimeout(resolveDelay, 2));
        }
    }
    throw lastError;
}

describe("native zero-dependency build", () => {
    it("publishes the mutable current manifest only through a prepared unique replacement", async () => {
        const buildSource = await readFile(resolve(process.cwd(), "scripts/build.mjs"), "utf8");

        assert.match(buildSource, /writeFile\(replacementPath, manifestBytes, \{ flag: "wx" \}\)/);
        assert.match(buildSource, /rename\(replacementPath, currentManifestPath\)/);
        assert.doesNotMatch(buildSource, /writeFile\(currentManifestPath, manifestBytes/);
    });

    it("publishes a unique build root and exact current output manifest", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = await readCurrentManifest();

        assert.equal(manifest.schemaVersion, 1);
        assert.match(manifest.buildId, /^[a-z0-9-]+$/);
        assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
        assert.equal(manifest.serverEntry, `${manifest.clientRoot}/src/server/cli.js`);
        assert.ok(manifest.testFiles.some((path) => path.endsWith("tests/integration/build.test.js")));
        assert.equal(manifest.runtimeAssets.length, 2);
        assert.ok(manifest.outputs.length > manifest.clientFiles.length);

        for (const output of manifest.outputs) {
            assert.equal(output.buildId, manifest.buildId);
            assert.match(output.sha256, /^[a-f0-9]{64}$/);
            const metadata = await stat(resolve(process.cwd(), "dist", output.path));
            assert.equal(metadata.size, output.size);
        }
    });

    it("keeps the current manifest parseable while a concurrent reader observes a unique build publication", async () => {
        const runBuild = ()                => new Promise((resolveBuild, rejectBuild) => {
            const child = spawn(process.execPath, [
                "--disable-warning=ExperimentalWarning",
                "scripts/build.mjs",
            ], {
                cwd: process.cwd(),
                stdio: "ignore",
                shell: false,
            });
            child.once("error", rejectBuild);
            child.once("exit", (code) => {
                if (code === 0) {
                    resolveBuild();
                } else {
                    rejectBuild(new Error(`Concurrent build exited with ${code}.`));
                }
            });
        });
        let complete = false;
        const build = runBuild().finally(() => {
            complete = true;
        });

        while (!complete) {
            const manifest = await readCurrentManifest();
            assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
            await new Promise      ((resolveDelay) => setTimeout(resolveDelay, 1));
        }
        await build;
    });

    it("copies both Windows helpers byte-for-byte as digest-verifiable runtime assets", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = await readCurrentManifest();
        for (const helperName of ["windows-replace-file.ps1", "windows-safe-file.ps1"]) {
            const runtimePath = `${manifest.clientRoot}/runtime/${helperName}`;
            const output = manifest.outputs.find((entry) => entry.path === runtimePath);
            const runtimeAsset = manifest.runtimeAssets.find((entry) => entry.path === runtimePath);
            assert.ok(output, "runtime PowerShell helper must be an allowlisted build output");
            assert.ok(runtimeAsset, "runtime PowerShell helper must be in the dedicated runtime asset allowlist");
            assert.deepEqual(runtimeAsset, output);
            assert.equal(output.buildId, manifest.buildId);

            const source = await readFile(resolve(process.cwd(), "scripts", helperName));
            const built = await readFile(resolve(process.cwd(), "dist", runtimePath));
            assert.deepEqual(built, source);
            assert.equal(output.size, source.length);
            assert.equal(output.sha256, createHash("sha256").update(source).digest("hex"));
            await verifyManifestOutput({
                staticRoot: resolve(process.cwd(), "dist"),
                manifestPath,
                outputPath: runtimePath,
            });
        }
    });

    it("detects a tampered distributed runtime asset through manifest integrity verification", async () => {
        const root = await mkdtemp(join(tmpdir(), "dat-flow-runtime-asset-"));
        const buildId = "runtime-build-0001";
        const clientRoot = `builds/${buildId}`;
        const runtimePaths = ["windows-replace-file.ps1", "windows-safe-file.ps1"];
        const runtimeAssets = [];
        for (const helperName of runtimePaths) {
            const runtimePath = `${clientRoot}/runtime/${helperName}`;
            const runtimeFile = join(root, ...runtimePath.split("/"));
            await mkdir(join(runtimeFile, ".."), { recursive: true });
            const source = await readFile(resolve(process.cwd(), "scripts", helperName));
            await writeFile(runtimeFile, source);
            runtimeAssets.push({
                path: runtimePath,
                buildId,
                size: source.length,
                sha256: createHash("sha256").update(source).digest("hex"),
            });
        }
        const manifestPath = join(root, "build-manifest.json");
        await writeFile(manifestPath, `${JSON.stringify({
            schemaVersion: 1,
            buildId,
            clientRoot,
            serverEntry: `${clientRoot}/src/server/cli.js`,
            testFiles: [],
            runtimeAssets,
            outputs: [{
                path: `${clientRoot}/src/server/cli.js`,
                buildId,
                size: 0,
                sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
            }, ...runtimeAssets],
            clientFiles: [],
        })}\n`);

        for (const runtimeAsset of runtimeAssets) {
            const runtimeFile = join(root, ...runtimeAsset.path.split("/"));
            await verifyManifestOutput({ staticRoot: root, manifestPath, outputPath: runtimeAsset.path });
            await writeFile(runtimeFile, Buffer.concat([await readFile(runtimeFile), Buffer.from("tampered")]));
            await assert.rejects(
                verifyManifestOutput({ staticRoot: root, manifestPath, outputPath: runtimeAsset.path }),
                /size|SHA-256|digest/i,
            );
        }
    });
});
