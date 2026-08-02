// dat-skill-flow-build:20260801043533698-cc310d81acc0422c8279233ba6eb3ea0
import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, stat, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import { verifyManifestOutput } from "../../scripts/manifest-integrity.mjs";

                         
                          
                    
                       
                        
                        
                                                                                          
                                                                                    
                                                                                        
 

describe("native zero-dependency build", () => {
    it("publishes the mutable current manifest only through a prepared unique replacement", async () => {
        const buildSource = await readFile(resolve(process.cwd(), "scripts/build.mjs"), "utf8");

        assert.match(buildSource, /writeFile\(replacementPath, manifestBytes, \{ flag: "wx" \}\)/);
        assert.match(buildSource, /rename\(replacementPath, currentManifestPath\)/);
        assert.doesNotMatch(buildSource, /writeFile\(currentManifestPath, manifestBytes/);
    });

    it("publishes a unique build root and exact current output manifest", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = JSON.parse(await readFile(manifestPath, "utf8"))                 ;

        assert.equal(manifest.schemaVersion, 1);
        assert.match(manifest.buildId, /^[a-z0-9-]+$/);
        assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
        assert.equal(manifest.serverEntry, `${manifest.clientRoot}/src/server/cli.js`);
        assert.ok(manifest.testFiles.some((path) => path.endsWith("tests/integration/build.test.js")));
        assert.equal(manifest.runtimeAssets.length, 1);
        assert.ok(manifest.outputs.length > manifest.clientFiles.length);

        for (const output of manifest.outputs) {
            assert.equal(output.buildId, manifest.buildId);
            assert.match(output.sha256, /^[a-f0-9]{64}$/);
            const metadata = await stat(resolve(process.cwd(), "dist", output.path));
            assert.equal(metadata.size, output.size);
        }
    });

    it("keeps the current manifest parseable while concurrent unique builds publish", async () => {
        const readCurrentManifest = async ()                         => {
            let lastError         ;
            for (let attempt = 0; attempt < 32; attempt += 1) {
                try {
                    return JSON.parse(await readFile(resolve(process.cwd(), "dist/build-manifest.json"), "utf8"))                 ;
                } catch (error) {
                    if ((error                         ).code !== "ENOENT" || attempt === 31) {
                        throw error;
                    }
                    lastError = error;
                    await new Promise      ((resolveDelay) => setTimeout(resolveDelay, 2));
                }
            }
            throw lastError;
        };
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
        const builds = Promise.all([runBuild(), runBuild()]).finally(() => {
            complete = true;
        });

        while (!complete) {
            const manifest = await readCurrentManifest();
            assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
            await new Promise      ((resolveDelay) => setTimeout(resolveDelay, 1));
        }
        await builds;
    });

    it("copies the Windows ReplaceFile helper byte-for-byte as a digest-verifiable runtime asset", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = JSON.parse(await readFile(manifestPath, "utf8"))                 ;
        const runtimePath = `${manifest.clientRoot}/runtime/windows-replace-file.ps1`;
        const output = manifest.outputs.find((entry) => entry.path === runtimePath);
        const runtimeAsset = manifest.runtimeAssets.find((entry) => entry.path === runtimePath);
        assert.ok(output, "runtime PowerShell helper must be an allowlisted build output");
        assert.ok(runtimeAsset, "runtime PowerShell helper must be in the dedicated runtime asset allowlist");
        assert.deepEqual(runtimeAsset, output);
        assert.equal(output.buildId, manifest.buildId);

        const source = await readFile(resolve(process.cwd(), "scripts/windows-replace-file.ps1"));
        const built = await readFile(resolve(process.cwd(), "dist", runtimePath));
        assert.deepEqual(built, source);
        assert.equal(output.size, source.length);
        assert.equal(output.sha256, createHash("sha256").update(source).digest("hex"));
        await verifyManifestOutput({
            staticRoot: resolve(process.cwd(), "dist"),
            manifestPath,
            outputPath: runtimePath,
        });
    });

    it("detects a tampered distributed runtime asset through manifest integrity verification", async () => {
        const root = await mkdtemp(join(tmpdir(), "dat-flow-runtime-asset-"));
        const buildId = "runtime-build-0001";
        const clientRoot = `builds/${buildId}`;
        const runtimePath = `${clientRoot}/runtime/windows-replace-file.ps1`;
        const runtimeFile = join(root, ...runtimePath.split("/"));
        await mkdir(join(runtimeFile, ".."), { recursive: true });
        const source = await readFile(resolve(process.cwd(), "scripts/windows-replace-file.ps1"));
        await writeFile(runtimeFile, source);
        const digest = createHash("sha256").update(source).digest("hex");
        const manifestPath = join(root, "build-manifest.json");
        const runtimeAsset = {
            path: runtimePath,
            buildId,
            size: source.length,
            sha256: digest,
        };
        await writeFile(manifestPath, `${JSON.stringify({
            schemaVersion: 1,
            buildId,
            clientRoot,
            serverEntry: `${clientRoot}/src/server/cli.js`,
            testFiles: [],
            runtimeAssets: [runtimeAsset],
            outputs: [{
                path: `${clientRoot}/src/server/cli.js`,
                buildId,
                size: 0,
                sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
            }, runtimeAsset],
            clientFiles: [],
        })}\n`);

        await verifyManifestOutput({ staticRoot: root, manifestPath, outputPath: runtimePath });
        await writeFile(runtimeFile, Buffer.concat([source, Buffer.from("tampered")]));
        await assert.rejects(
            verifyManifestOutput({ staticRoot: root, manifestPath, outputPath: runtimePath }),
            /size|SHA-256|digest/i,
        );
    });
});
