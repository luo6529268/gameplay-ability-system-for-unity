// dat-skill-flow-build:20260801063141570-9270908961cf4bb38ce6537832f02870
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { describe, it } from "node:test";

import { loadVerifiedBuildManifest } from "../../scripts/manifest-integrity.mjs";

                           
                       
                         
                                         
                                                
                            
      
                       
                     
 

function fileEntry(path        , buildId        , bytes        )                          {
    return {
        path,
        buildId,
        size: bytes.length,
        sha256: createHash("sha256").update(bytes).digest("hex"),
    };
}

async function createManifestFixture()                           {
    const staticRoot = await mkdtemp(join(tmpdir(), "dat-flow-integrity-"));
    const buildId = "integrity-build-0001";
    const clientRoot = `builds/${buildId}`;
    const serverEntry = `${clientRoot}/src/server/cli.js`;
    const testEntry = `${clientRoot}/tests/unit/example.test.js`;
    const serverPath = join(staticRoot, ...serverEntry.split("/"));
    const testPath = join(staticRoot, ...testEntry.split("/"));
    await mkdir(join(serverPath, ".."), { recursive: true });
    await mkdir(join(testPath, ".."), { recursive: true });
    const serverBytes = Buffer.from("export {};\n");
    const testBytes = Buffer.from("export {};\n");
    await writeFile(serverPath, serverBytes);
    await writeFile(testPath, testBytes);
    const manifest = {
        schemaVersion: 1,
        buildId,
        clientRoot,
        serverEntry,
        testFiles: [testEntry],
        runtimeAssets: [],
        outputs: [
            fileEntry(serverEntry, buildId, serverBytes),
            fileEntry(testEntry, buildId, testBytes),
        ],
        clientFiles: [],
    };
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, `${JSON.stringify(manifest)}\n`);
    await writeFile(join(staticRoot, ...clientRoot.split("/"), "build-manifest.json"), `${JSON.stringify(manifest)}\n`);
    return { staticRoot, manifestPath, manifest, serverPath, testPath };
}

async function writeManifest(fixture                 )                {
    await writeFile(fixture.manifestPath, `${JSON.stringify(fixture.manifest)}\n`);
}

describe("trusted build manifest execution", () => {
    it("verifies current server and test outputs before execution", async () => {
        const fixture = await createManifestFixture();
        const verified = await loadVerifiedBuildManifest(fixture);

        assert.equal(verified.manifest.serverEntry, fixture.manifest.serverEntry);
        assert.equal(verified.verifiedPaths.size, 2);
        assert.equal(verified.verifiedPaths.get(fixture.manifest.serverEntry), fixture.serverPath);
        assert.equal(
            verified.manifestPath,
            join(fixture.staticRoot, "builds", "integrity-build-0001", "build-manifest.json"),
        );
    });

    it("pins the immutable build-local manifest instead of retaining the mutable current manifest path", async () => {
        const fixture = await createManifestFixture();
        const verified = await loadVerifiedBuildManifest(fixture);
        await writeFile(fixture.manifestPath, "{\"partial\":");

        assert.equal(verified.manifest.buildId, "integrity-build-0001");
        assert.match(await readFile(verified.manifestPath, "utf8"), /integrity-build-0001/);
    });

    it("retries only injected transient Windows pointer-sharing failures before pinning", async () => {
        const fixture = await createManifestFixture();
        let pointerReads = 0;
        let retries = 0;
        const verified = await loadVerifiedBuildManifest({
            ...fixture,
            readFileImpl: async (filePath        , options        ) => {
                if (filePath === fixture.manifestPath && pointerReads++ < 2) {
                    throw Object.assign(new Error("manifest pointer is busy"), { code: "EBUSY" });
                }
                return await readFile(filePath, options);
            },
            retryDelay: async () => {
                retries += 1;
            },
        });

        assert.equal(verified.manifest.buildId, "integrity-build-0001");
        assert.equal(pointerReads, 3);
        assert.equal(retries, 2);
    });

    it("does not retry malformed current-manifest JSON", async () => {
        const fixture = await createManifestFixture();
        let retries = 0;
        await assert.rejects(
            loadVerifiedBuildManifest({
                ...fixture,
                readFileImpl: async (filePath        , options        ) => (
                    filePath === fixture.manifestPath ? "{\"partial\":" : await readFile(filePath, options)
                ),
                retryDelay: async () => {
                    retries += 1;
                },
            }),
            SyntaxError,
        );
        assert.equal(retries, 0);
    });

    it("rejects unknown keys, invalid types, and duplicate outputs", async () => {
        const unknown = await createManifestFixture();
        unknown.manifest.unexpected = true;
        await writeManifest(unknown);
        await assert.rejects(loadVerifiedBuildManifest(unknown), /unknown|unexpected/i);

        const invalidType = await createManifestFixture();
        invalidType.manifest.schemaVersion = "1";
        await writeManifest(invalidType);
        await assert.rejects(loadVerifiedBuildManifest(invalidType), /schemaVersion|literal/i);

        const duplicate = await createManifestFixture();
        duplicate.manifest.outputs.push({ ...duplicate.manifest.outputs[0] });
        await writeManifest(duplicate);
        await assert.rejects(loadVerifiedBuildManifest(duplicate), /duplicate/i);
    });

    it("rejects server and test entries that do not correspond to current outputs", async () => {
        const staleServer = await createManifestFixture();
        staleServer.manifest.serverEntry = "builds/integrity-build-0001/src/server/stale.js";
        await writeManifest(staleServer);
        await assert.rejects(loadVerifiedBuildManifest(staleServer), /serverEntry|output/i);

        const staleTest = await createManifestFixture();
        staleTest.manifest.testFiles = ["builds/integrity-build-0001/tests/unit/stale.test.js"];
        await writeManifest(staleTest);
        await assert.rejects(loadVerifiedBuildManifest(staleTest), /testFiles|output/i);
    });

    it("rejects runtime assets that are not exact current-build outputs", async () => {
        const missingOutput = await createManifestFixture();
        missingOutput.manifest.runtimeAssets = [{
            ...missingOutput.manifest.outputs[0],
            path: "builds/integrity-build-0001/runtime/windows-replace-file.ps1",
        }];
        await writeManifest(missingOutput);
        await assert.rejects(loadVerifiedBuildManifest(missingOutput), /runtimeAssets|output/i);

        const mismatchedDigest = await createManifestFixture();
        mismatchedDigest.manifest.runtimeAssets = [{
            ...mismatchedDigest.manifest.outputs[0],
            sha256: "0".repeat(64),
        }];
        await writeManifest(mismatchedDigest);
        await assert.rejects(loadVerifiedBuildManifest(mismatchedDigest), /runtimeAssets|output/i);
    });

    it("rejects server or test bytes changed after manifest publication", async () => {
        const staleServer = await createManifestFixture();
        await writeFile(staleServer.serverPath, "tampered server\n");
        await assert.rejects(loadVerifiedBuildManifest(staleServer), /size|sha-?256|digest/i);

        const staleTest = await createManifestFixture();
        await writeFile(staleTest.testPath, "tampered test\n");
        await assert.rejects(loadVerifiedBuildManifest(staleTest), /size|sha-?256|digest/i);
    });
});
