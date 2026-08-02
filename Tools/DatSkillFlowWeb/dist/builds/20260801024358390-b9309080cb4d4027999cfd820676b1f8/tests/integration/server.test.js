// dat-skill-flow-build:20260801024358390-b9309080cb4d4027999cfd820676b1f8
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, symlink, writeFile } from "node:fs/promises";
import { request } from "node:http";
                                            
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, describe, it } from "node:test";

import { createApplicationServer, listenLoopback } from "../../src/server/server.js";

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
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, `${JSON.stringify({
        schemaVersion: 1,
        buildId,
        clientRoot: `builds/${buildId}`,
        serverEntry: `builds/${buildId}/src/server/cli.js`,
        testFiles: [],
        outputs: [{ ...indexEntry, path: `builds/${buildId}/index.html` }],
        clientFiles: [indexEntry],
    })}\n`);
    return { staticRoot, manifestPath, clientRoot };
}

async function rawRequest(origin        , path        )                                            {
    const url = new URL(origin);
    return await new Promise((resolveRequest, rejectRequest) => {
        const outgoing = request({
            hostname: url.hostname,
            port: Number(url.port),
            method: "GET",
            path,
        }, (response) => {
            const chunks           = [];
            response.on("data", (chunk        ) => chunks.push(chunk));
            response.on("end", () => resolveRequest({
                status: response.statusCode ?? 0,
                body: Buffer.concat(chunks).toString("utf8"),
            }));
        });
        outgoing.on("error", rejectRequest);
        outgoing.end();
    });
}

afterEach(async () => {
    await Promise.all(openServers.splice(0).map((server) => new Promise      ((resolve, reject) => {
        server.close((error) => error ? reject(error) : resolve());
    })));
});

describe("loopback application server", () => {
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

        await writeFile(join(fixture.clientRoot, "index.html"), "mismatched stale content");
        assert.equal((await fetch(`${origin}/`)).status, 404);
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
            outputs: [{
                path: "builds/test-build-0001/escape/escaped.txt",
                buildId: "test-build-0001",
                size: bytes.length,
                sha256: createHash("sha256").update(bytes).digest("hex"),
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
        assert.equal(response.status, 404);
        assert.doesNotMatch(await response.text(), /secret contents/i);
    });
});
