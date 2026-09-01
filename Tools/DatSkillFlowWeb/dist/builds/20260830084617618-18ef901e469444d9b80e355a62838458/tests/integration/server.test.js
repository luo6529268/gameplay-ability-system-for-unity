// dat-skill-flow-build:20260830084617618-18ef901e469444d9b80e355a62838458
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, symlink, writeFile } from "node:fs/promises";
import { request } from "node:http";
                                            
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, describe, it } from "node:test";

import {
    authorizeStateChangingRequest,
    createApplicationServer,
    getApplicationServerSecurity,
    HTTP_SERVER_LIMITS,
    listenLoopback,
    MAX_STATIC_FILE_BYTES,
    MAX_STATIC_TOTAL_BYTES,
    loadPinnedStaticConfiguration,
    STATE_CHANGE_TOKEN_HEADER,
} from "../../src/server/server.js";

const openServers                                                    = [];

                         
                       
                         
                       
 

async function createStaticFixture()                         {
    const staticRoot = await mkdtemp(join(tmpdir(), "dat-flow-static-"));
    const buildId = "test-build-0001";
    const clientRoot = join(staticRoot, "builds", buildId);
    await mkdir(clientRoot, { recursive: true });
    const indexBytes = Buffer.from(`<meta name="dat-skill-flow-build-id" content="${buildId}">current`);
    await writeFile(join(clientRoot, "index.html"), indexBytes);
    await writeFile(join(clientRoot, "stale.js"), "stale but not allowlisted");
    const indexEntry = {
        path: "index.html",
        buildId,
        size: indexBytes.length,
        sha256: createHash("sha256").update(indexBytes).digest("hex"),
    };
    const serverOutput = {
        path: `builds/${buildId}/src/server/cli.js`,
        buildId,
        size: 0,
        sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
    };
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, `${JSON.stringify({
        schemaVersion: 1,
        buildId,
        clientRoot: `builds/${buildId}`,
        serverEntry: `builds/${buildId}/src/server/cli.js`,
        testFiles: [],
        runtimeAssets: [],
        outputs: [
            { ...indexEntry, path: `builds/${buildId}/index.html` },
            serverOutput,
        ],
        clientFiles: [indexEntry],
    })}\n`);
    return { staticRoot, manifestPath, clientRoot };
}

                             
                    
                                     
                  
 

async function rawRequest(
    origin        ,
    path        ,
    options                    = {},
)                                            {
    const url = new URL(origin);
    return await new Promise((resolveRequest, rejectRequest) => {
        const outgoing = request({
            hostname: url.hostname,
            port: Number(url.port),
            method: options.method ?? "GET",
            path,
            headers: options.headers,
        }, (response) => {
            const chunks           = [];
            response.on("data", (chunk        ) => chunks.push(chunk));
            response.on("end", () => resolveRequest({
                status: response.statusCode ?? 0,
                body: Buffer.concat(chunks).toString("utf8"),
            }));
        });
        outgoing.on("error", rejectRequest);
        outgoing.end(options.body);
    });
}

afterEach(async () => {
    await Promise.all(openServers.splice(0).map((server) => new Promise      ((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
    })));
});

describe("loopback application server", () => {
    it("retries an ephemeral browser-blocked port and returns the next safe loopback port", async () => {
        const attemptedPorts           = [];
        let closeCount = 0;
        const addresses                = [
            { address: "127.0.0.1", family: "IPv4", port: 6000 },
            { address: "127.0.0.1", family: "IPv4", port: 48123 },
        ];

        const origin = await listenLoopback({}                                              , 0, {
            listen: async (_server, requestedPort) => {
                attemptedPorts.push(requestedPort);
                return addresses.shift() ?? null;
            },
            close: async () => {
                closeCount += 1;
            },
        });

        assert.equal(origin, "http://127.0.0.1:48123");
        assert.deepEqual(attemptedPorts, [0, 0]);
        assert.equal(closeCount, 1);
    });

    it("accepts a browser-safe ephemeral port without retrying", async () => {
        let listenCount = 0;
        let closeCount = 0;

        const origin = await listenLoopback({}                                              , 0, {
            listen: async () => {
                listenCount += 1;
                return { address: "127.0.0.1", family: "IPv4", port: 48123 };
            },
            close: async () => {
                closeCount += 1;
            },
        });

        assert.equal(origin, "http://127.0.0.1:48123");
        assert.equal(listenCount, 1);
        assert.equal(closeCount, 0);
    });

    it("fails explicitly after exhausting browser-blocked ephemeral port retries", async () => {
        let listenCount = 0;
        let closeCount = 0;

        await assert.rejects(
            listenLoopback({}                                              , 0, {
                maxEphemeralListenAttempts: 3,
                listen: async () => {
                    listenCount += 1;
                    return { address: "127.0.0.1", family: "IPv4", port: 6000 };
                },
                close: async () => {
                    closeCount += 1;
                },
            }),
            /Unable to allocate a browser-safe loopback port after 3 attempts/,
        );

        assert.equal(listenCount, 3);
        assert.equal(closeCount, 3);
    });

    it("keeps build A static bytes when current publication flips to build B before the first request", async () => {
        const fixture = await createStaticFixture();
        const buildAManifest = JSON.parse(await readFile(fixture.manifestPath, "utf8"));
        const staticConfiguration = await loadPinnedStaticConfiguration(fixture.staticRoot, buildAManifest);

        const buildId = "test-build-0002";
        const clientRoot = `builds/${buildId}`;
        const indexBytes = Buffer.from(`<meta name="dat-skill-flow-build-id" content="${buildId}">new`);
        await mkdir(join(fixture.staticRoot, ...clientRoot.split("/")), { recursive: true });
        await writeFile(join(fixture.staticRoot, ...clientRoot.split("/"), "index.html"), indexBytes);
        const indexEntry = {
            path: "index.html",
            buildId,
            size: indexBytes.length,
            sha256: createHash("sha256").update(indexBytes).digest("hex"),
        };
        const buildBManifest = {
            schemaVersion: 1,
            buildId,
            clientRoot,
            serverEntry: `${clientRoot}/src/server/cli.js`,
            testFiles: [],
            runtimeAssets: [],
            outputs: [
                { ...indexEntry, path: `${clientRoot}/index.html` },
                {
                    path: `${clientRoot}/src/server/cli.js`,
                    buildId,
                    size: 0,
                    sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
                },
            ],
            clientFiles: [indexEntry],
        };
        await writeFile(fixture.manifestPath, `${JSON.stringify(buildBManifest)}\n`);
        await writeFile(join(fixture.staticRoot, ...clientRoot.split("/"), "build-manifest.json"), `${JSON.stringify(buildBManifest)}\n`);

        const server = createApplicationServer({
            ...fixture,
            staticConfiguration: Promise.resolve(staticConfiguration),
        });
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        const response = await fetch(`${origin}/`);
        assert.equal(response.status, 200);
        assert.match(await response.text(), /test-build-0001/);
    });

    it("binds explicitly to 127.0.0.1 and serves same-origin health", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);
        const address = server.address()               ;

        assert.equal(address.address, "127.0.0.1");
        assert.equal(origin, `http://127.0.0.1:${address.port}`);

        const response = await fetch(`${origin}/api/health`);
        assert.equal(response.status, 200);
        assert.match(response.headers.get("content-security-policy") ?? "", /default-src 'self'/);
        assert.equal(response.headers.get("x-content-type-options"), "nosniff");
        const health = await response.json()                                                           ;
        assert.equal(health.ok, true);
        assert.deepEqual(health.data, {
            status: "ok",
            host: "127.0.0.1",
            authorityEntries: 0,
        });
    });

    it("rejects unsupported methods with an Allow header", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        const response = await fetch(`${origin}/api/health`, { method: "POST" });

        assert.equal(response.status, 405);
        assert.equal(response.headers.get("allow"), "GET, HEAD");
        const body = await response.json()                   ;
        assert.equal(body.ok, false);
    });

    it("serves only files allowlisted by the current build manifest", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        const current = await fetch(`${origin}/`);
        assert.equal(current.status, 200);
        assert.match(await current.text(), /test-build-0001/);
        assert.equal((await fetch(`${origin}/stale.js`)).status, 404);
    });

    it("rejects stale current-build bytes before caching", async () => {
        const fixture = await createStaticFixture();
        await writeFile(join(fixture.clientRoot, "index.html"), "mismatched stale content");
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        assert.notEqual((await fetch(`${origin}/`)).status, 200);
    });

    it("serves the cached verified immutable buffer without rehashing disk", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        const first = await fetch(`${origin}/`);
        const originalBody = await first.text();
        assert.equal(first.status, 200);
        await writeFile(join(fixture.clientRoot, "index.html"), "tampered after verification");
        const second = await fetch(`${origin}/`);
        assert.equal(second.status, 200);
        assert.equal(await second.text(), originalBody);
    });

    it("rejects literal, encoded, and repeatedly encoded traversal", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        for (const path of [
            "/../secret.txt",
            "/%2e%2e/secret.txt",
            "/%252e%252e/secret.txt",
            "/..%2fsecret.txt",
            "/%2e%2e%5csecret.txt",
        ]) {
            const response = await rawRequest(origin, path);
            assert.notEqual(response.status, 200, path);
            assert.doesNotMatch(response.body, /secret contents/i, path);
        }
    });

    it("rejects a symlink or junction that escapes the canonical client root", async (context) => {
        const fixture = await createStaticFixture();
        const outside = await mkdtemp(join(tmpdir(), "dat-flow-outside-"));
        await writeFile(join(outside, "escaped.txt"), "secret contents");
        const escapeLink = join(fixture.clientRoot, "escape");
        try {
            await symlink(outside, escapeLink, process.platform === "win32" ? "junction" : "dir");
        } catch (error) {
            const code = (error                         ).code;
            if (code === "EPERM" || code === "EACCES" || code === "ENOSYS") {
                context.skip(`Link creation is unavailable on this platform: ${code}`);
                return;
            }
            throw error;
        }

        const bytes = Buffer.from("secret contents");
        const manifest = {
            schemaVersion: 1,
            buildId: "test-build-0001",
            clientRoot: "builds/test-build-0001",
            serverEntry: "builds/test-build-0001/src/server/cli.js",
            testFiles: [],
            runtimeAssets: [],
            outputs: [{
                path: "builds/test-build-0001/escape/escaped.txt",
                buildId: "test-build-0001",
                size: bytes.length,
                sha256: createHash("sha256").update(bytes).digest("hex"),
            }, {
                path: "builds/test-build-0001/src/server/cli.js",
                buildId: "test-build-0001",
                size: 0,
                sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
            }],
            clientFiles: [{
                path: "escape/escaped.txt",
                buildId: "test-build-0001",
                size: bytes.length,
                sha256: createHash("sha256").update(bytes).digest("hex"),
            }],
        };
        await writeFile(fixture.manifestPath, `${JSON.stringify(manifest)}\n`);

        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);
        const response = await fetch(`${origin}/escape/escaped.txt`);
        assert.notEqual(response.status, 200);
        assert.doesNotMatch(await response.text(), /secret contents/i);
    });

    it("accepts only the actual loopback Host and an exact optional Origin", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        assert.equal((await rawRequest(origin, "/api/health")).status, 200);
        assert.equal((await rawRequest(origin, "/api/health", {
            headers: { Origin: origin },
        })).status, 200);
        assert.equal((await rawRequest(origin, "/api/health", {
            headers: { Host: "attacker.invalid" },
        })).status, 403);
        assert.equal((await rawRequest(origin, "/api/health", {
            headers: { Origin: "http://attacker.invalid" },
        })).status, 403);
    });

    it("rejects requests that advertise a body", async () => {
        const fixture = await createStaticFixture();
        const server = createApplicationServer(fixture);
        openServers.push(server);
        const origin = await listenLoopback(server, 0);

        assert.equal((await rawRequest(origin, "/api/health", {
            method: "POST",
            headers: { "Content-Length": "1" },
            body: "x",
        })).status, 413);
        assert.equal((await rawRequest(origin, "/api/health", {
            method: "POST",
            headers: { "Transfer-Encoding": "chunked" },
            body: "x",
        })).status, 413);
    });

    it("establishes per-process state-changing request token isolation", async () => {
        const first = createApplicationServer(await createStaticFixture());
        const second = createApplicationServer(await createStaticFixture());
        const firstSecurity = getApplicationServerSecurity(first);
        const secondSecurity = getApplicationServerSecurity(second);

        assert.equal(firstSecurity.tokenHeader, STATE_CHANGE_TOKEN_HEADER);
        assert.match(firstSecurity.token, /^[A-Za-z0-9_-]{40,}$/);
        assert.equal(firstSecurity.token, secondSecurity.token);
        assert.equal(authorizeStateChangingRequest(first, firstSecurity.token), true);
        assert.equal(authorizeStateChangingRequest(first, `${secondSecurity.token}x`), false);
        assert.equal(authorizeStateChangingRequest(first, undefined), false);
    });

    it("configures bounded HTTP resource limits", async () => {
        const server = createApplicationServer(await createStaticFixture());

        assert.equal(server.headersTimeout, HTTP_SERVER_LIMITS.headersTimeoutMs);
        assert.equal(server.requestTimeout, HTTP_SERVER_LIMITS.requestTimeoutMs);
        assert.equal(server.keepAliveTimeout, HTTP_SERVER_LIMITS.keepAliveTimeoutMs);
        assert.equal(server.maxConnections, HTTP_SERVER_LIMITS.maxConnections);
    });

    it("rejects manifest client files above per-file and aggregate size limits", async () => {
        const oversizedFile = await createStaticFixture();
        const fileManifest = JSON.parse(await readFile(oversizedFile.manifestPath, "utf8"));
        fileManifest.clientFiles[0].size = MAX_STATIC_FILE_BYTES + 1;
        fileManifest.outputs[0].size = MAX_STATIC_FILE_BYTES + 1;
        await writeFile(oversizedFile.manifestPath, `${JSON.stringify(fileManifest)}\n`);
        const fileServer = createApplicationServer(oversizedFile);
        openServers.push(fileServer);
        const fileOrigin = await listenLoopback(fileServer, 0);
        assert.notEqual((await fetch(`${fileOrigin}/`)).status, 200);

        const oversizedTotal = await createStaticFixture();
        const totalManifest = JSON.parse(await readFile(oversizedTotal.manifestPath, "utf8"));
        totalManifest.clientFiles = [];
        totalManifest.outputs = [{
            path: `${totalManifest.clientRoot}/src/server/cli.js`,
            buildId: totalManifest.buildId,
            size: 0,
            sha256: createHash("sha256").update(Buffer.alloc(0)).digest("hex"),
        }];
        const entrySize = Math.floor(MAX_STATIC_TOTAL_BYTES / 4);
        for (let index = 0; index < 5; index += 1) {
            const path = `asset-${index}.js`;
            const entry = {
                path,
                buildId: totalManifest.buildId,
                size: entrySize,
                sha256: "0".repeat(64),
            };
            totalManifest.clientFiles.push(entry);
            totalManifest.outputs.push({ ...entry, path: `${totalManifest.clientRoot}/${path}` });
        }
        await writeFile(oversizedTotal.manifestPath, `${JSON.stringify(totalManifest)}\n`);
        const totalServer = createApplicationServer(oversizedTotal);
        openServers.push(totalServer);
        const totalOrigin = await listenLoopback(totalServer, 0);
        assert.notEqual((await fetch(`${totalOrigin}/`)).status, 200);
    });
});
