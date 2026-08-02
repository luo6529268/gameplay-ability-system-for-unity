// dat-skill-flow-build:20260801035209517-1d79b9ce59424f4bbd48bf22e3461438
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, writeFile } from "node:fs/promises";
import { spawnSync } from "node:child_process";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";
import { pathToFileURL } from "node:url";

                         
                       
                         
                            
                                 
                          
                               
               
                                                                                        
      
 

function digest(bytes        )         {
    return createHash("sha256").update(bytes).digest("hex");
}

function outputEntry(path        , buildId        , bytes        ) {
    return { path, buildId, size: bytes.length, sha256: digest(bytes) };
}

async function createLoaderFixture()                         {
    const staticRoot = await mkdtemp(join(tmpdir(), "dat-flow-loader-"));
    const buildId = "loader-build-0001";
    const clientRoot = `builds/${buildId}`;
    const serverEntry = `${clientRoot}/src/server/cli.js`;
    const serverDependency = `${clientRoot}/src/server/server-dependency.js`;
    const testEntry = `${clientRoot}/tests/unit/loader-fixture.test.js`;
    const testDependency = `${clientRoot}/tests/unit/test-dependency.js`;
    const serverEntryPath = join(staticRoot, ...serverEntry.split("/"));
    const serverDependencyPath = join(staticRoot, ...serverDependency.split("/"));
    const testEntryPath = join(staticRoot, ...testEntry.split("/"));
    const testDependencyPath = join(staticRoot, ...testDependency.split("/"));
    await mkdir(join(serverEntryPath, ".."), { recursive: true });
    await mkdir(join(testEntryPath, ".."), { recursive: true });
    const serverBytes = Buffer.from('import "./server-dependency.js";\nprocess.stdout.write("verified-server\\n");\n');
    const serverDependencyBytes = Buffer.from("export const serverValue = 1;\n");
    const testBytes = Buffer.from([
        'import "./test-dependency.js";',
        'import { test } from "node:test";',
        'test("verified test dependency", () => {});',
        "",
    ].join("\n"));
    const testDependencyBytes = Buffer.from("export const testValue = 1;\n");
    await writeFile(serverEntryPath, serverBytes);
    await writeFile(serverDependencyPath, serverDependencyBytes);
    await writeFile(testEntryPath, testBytes);
    await writeFile(testDependencyPath, testDependencyBytes);
    const manifest = {
        schemaVersion: 1,
        buildId,
        clientRoot,
        serverEntry,
        testFiles: [testEntry],
        outputs: [
            outputEntry(serverEntry, buildId, serverBytes),
            outputEntry(serverDependency, buildId, serverDependencyBytes),
            outputEntry(testEntry, buildId, testBytes),
            outputEntry(testDependency, buildId, testDependencyBytes),
        ],
        clientFiles: [],
    };
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, `${JSON.stringify(manifest)}\n`);
    return {
        staticRoot,
        manifestPath,
        serverEntryPath,
        serverDependencyPath,
        testEntryPath,
        testDependencyPath,
        manifest,
    };
}

function runVerified(fixture               , argumentsAfterImport          ) {
    const registerWrapper = resolve(process.cwd(), "scripts/register-verified-loader.mjs");
    return spawnSync(process.execPath, ["--import", pathToFileURL(registerWrapper).href, ...argumentsAfterImport], {
        encoding: "utf8",
        env: {
            ...process.env,
            DAT_SKILL_FLOW_STATIC_ROOT: fixture.staticRoot,
            DAT_SKILL_FLOW_MANIFEST_PATH: fixture.manifestPath,
        },
        timeout: 10_000,
    });
}

describe("verified current-build module loader", () => {
    it("loads allowlisted entry points and transitive modules from verified source", async () => {
        const fixture = await createLoaderFixture();
        const server = runVerified(fixture, [fixture.serverEntryPath]);
        const tests = runVerified(fixture, ["--test", "--test-isolation=none", fixture.testEntryPath]);

        assert.equal(server.status, 0, server.stderr);
        assert.match(server.stdout, /verified-server/);
        assert.equal(tests.status, 0, tests.stderr);
    });

    it("rejects a tampered transitive server dependency while the entry remains valid", async () => {
        const fixture = await createLoaderFixture();
        const serverEntry = fixture.manifest.outputs.find((entry) => entry.path.endsWith("/cli.js"));
        assert.equal(digest(await readFile(fixture.serverEntryPath)), serverEntry?.sha256);
        await writeFile(fixture.serverDependencyPath, "export const serverValue = 2;\n");

        const result = runVerified(fixture, [fixture.serverEntryPath]);
        assert.notEqual(result.status, 0);
        assert.match(`${result.stdout}\n${result.stderr}`, /SHA-256|digest|size|allowlist/i);
    });

    it("rejects an existing transitive module omitted from the manifest allowlist", async () => {
        const fixture = await createLoaderFixture();
        fixture.manifest.outputs = fixture.manifest.outputs.filter((entry) => (
            !entry.path.endsWith("/server-dependency.js")
        ));
        await writeFile(fixture.manifestPath, `${JSON.stringify({
            schemaVersion: 1,
            buildId: "loader-build-0001",
            clientRoot: "builds/loader-build-0001",
            serverEntry: "builds/loader-build-0001/src/server/cli.js",
            testFiles: ["builds/loader-build-0001/tests/unit/loader-fixture.test.js"],
            outputs: fixture.manifest.outputs,
            clientFiles: [],
        })}\n`);

        const result = runVerified(fixture, [fixture.serverEntryPath]);
        assert.notEqual(result.status, 0);
        assert.match(`${result.stdout}\n${result.stderr}`, /allowlist/i);
    });

    it("rejects a tampered transitive test dependency while the test entry remains valid", async () => {
        const fixture = await createLoaderFixture();
        const testEntry = fixture.manifest.outputs.find((entry) => entry.path.endsWith("loader-fixture.test.js"));
        assert.equal(digest(await readFile(fixture.testEntryPath)), testEntry?.sha256);
        await writeFile(fixture.testDependencyPath, "export const testValue = 2;\n");

        const result = runVerified(fixture, ["--test", "--test-isolation=none", fixture.testEntryPath]);
        assert.notEqual(result.status, 0);
        assert.match(`${result.stdout}\n${result.stderr}`, /SHA-256|digest|size|allowlist/i);
    });

    it("rejects a stale test path when the child observes a newer build manifest", async () => {
        const fixture = await createLoaderFixture();
        const buildId = "loader-build-0002";
        const clientRoot = `builds/${buildId}`;
        const serverEntry = `${clientRoot}/src/server/cli.js`;
        const testEntry = `${clientRoot}/tests/unit/new-build.test.js`;
        const serverPath = join(fixture.staticRoot, ...serverEntry.split("/"));
        const testPath = join(fixture.staticRoot, ...testEntry.split("/"));
        await mkdir(join(serverPath, ".."), { recursive: true });
        await mkdir(join(testPath, ".."), { recursive: true });
        const serverBytes = Buffer.from("export {};\n");
        const testBytes = Buffer.from([
            'import { test } from "node:test";',
            'test("new build", () => {});',
            "",
        ].join("\n"));
        await writeFile(serverPath, serverBytes);
        await writeFile(testPath, testBytes);
        await writeFile(fixture.manifestPath, `${JSON.stringify({
            schemaVersion: 1,
            buildId,
            clientRoot,
            serverEntry,
            testFiles: [testEntry],
            outputs: [
                outputEntry(serverEntry, buildId, serverBytes),
                outputEntry(testEntry, buildId, testBytes),
            ],
            clientFiles: [],
        })}\n`);

        const result = runVerified(fixture, [
            "--test",
            "--test-isolation=none",
            fixture.testEntryPath,
        ]);
        assert.notEqual(result.status, 0);
        assert.match(`${result.stdout}\n${result.stderr}`, /stale|managed build|current verified build/i);
    });
});
