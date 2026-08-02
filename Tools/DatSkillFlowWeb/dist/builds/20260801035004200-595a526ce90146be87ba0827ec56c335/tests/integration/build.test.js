// dat-skill-flow-build:20260801035004200-595a526ce90146be87ba0827ec56c335
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, stat, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import { verifyManifestOutput } from "../../scripts/manifest-integrity.mjs";

                         
                          
                    
                       
                        
                        
                                                                                    
                                                                                        
 

describe("native zero-dependency build", () => {
    it("publishes a unique build root and exact current output manifest", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = JSON.parse(await readFile(manifestPath, "utf8"))                 ;

        assert.equal(manifest.schemaVersion, 1);
        assert.match(manifest.buildId, /^[a-z0-9-]+$/);
        assert.equal(manifest.clientRoot, `builds/${manifest.buildId}`);
        assert.equal(manifest.serverEntry, `${manifest.clientRoot}/src/server/cli.js`);
        assert.ok(manifest.testFiles.some((path) => path.endsWith("tests/integration/build.test.js")));
        assert.ok(manifest.outputs.length > manifest.clientFiles.length);

        for (const output of manifest.outputs) {
            assert.equal(output.buildId, manifest.buildId);
            assert.match(output.sha256, /^[a-f0-9]{64}$/);
            const metadata = await stat(resolve(process.cwd(), "dist", output.path));
            assert.equal(metadata.size, output.size);
        }
    });

    it("copies the Windows ReplaceFile helper byte-for-byte as a digest-verifiable runtime asset", async () => {
        const manifestPath = resolve(process.cwd(), "dist/build-manifest.json");
        const manifest = JSON.parse(await readFile(manifestPath, "utf8"))                 ;
        const runtimePath = `${manifest.clientRoot}/runtime/windows-replace-file.ps1`;
        const output = manifest.outputs.find((entry) => entry.path === runtimePath);
        assert.ok(output, "runtime PowerShell helper must be an allowlisted build output");
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
        await writeFile(manifestPath, `${JSON.stringify({
            schemaVersion: 1,
            buildId,
            clientRoot,
            serverEntry: `${clientRoot}/src/server/cli.js`,
            testFiles: [],
            outputs: [{
                path: `${clientRoot}/src/server/cli.js`,
                buildId,
                size: 0,
                sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
            }, {
                path: runtimePath,
                buildId,
                size: source.length,
                sha256: digest,
            }],
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
