// dat-skill-flow-build:20260801100636145-dd66b0468c524d2087248924398c50c5
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, writeFile } from "node:fs/promises";
import { request,             } from "node:http";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { afterEach, describe, it } from "node:test";

import {
    createApplicationServer,
    getApplicationServerSecurity,
    listenLoopback,
    MAX_JSON_BODY_BYTES,
} from "../../src/server/server.js";

const servers           = [];

async function serverFixture(allowAbsoluteRootGrant         )           
                   
                   
                  
                 
   {
    const staticRoot = await mkdtemp(join(tmpdir(), "dat-flow-safe-api-static-"));
    process.stdout.write(`[safe-test-artifacts] ${staticRoot}\n`);
    const buildId = "safe-api-build";
    const clientRoot = join(staticRoot, "builds", buildId);
    await mkdir(join(clientRoot, "src", "server"), { recursive: true });
    const index = Buffer.from("safe-api");
    const serverEntry = Buffer.alloc(0);
    await writeFile(join(clientRoot, "index.html"), index);
    await writeFile(join(clientRoot, "src", "server", "cli.js"), serverEntry);
    const entry = (path        , body        ) => ({
        path,
        buildId,
        size: body.length,
        sha256: createHash("sha256").update(body).digest("hex"),
    });
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, JSON.stringify({
        schemaVersion: 1,
        buildId,
        clientRoot: `builds/${buildId}`,
        serverEntry: `builds/${buildId}/src/server/cli.js`,
        testFiles: [],
        runtimeAssets: [],
        outputs: [
            entry(`builds/${buildId}/index.html`, index),
            entry(`builds/${buildId}/src/server/cli.js`, serverEntry),
        ],
        clientFiles: [entry("index.html", index)],
    }));
    const root = await mkdtemp(join(tmpdir(), "dat-flow-safe-api-root-"));
    process.stdout.write(`[safe-test-artifacts] ${root}\n`);
    await writeFile(join(root, "source.dat"), "api original");
    const server = createApplicationServer({
        staticRoot,
        manifestPath,
        workspace: { allowAbsoluteRootGrant },
    });
    servers.push(server);
    const origin = await listenLoopback(server, 0);
    return { server, origin, token: getApplicationServerSecurity(server).token, root };
}

async function post(origin        , token                    , path        , body         , withOrigin = true)                    {
    return await fetch(`${origin}${path}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            ...(withOrigin ? { Origin: origin } : {}),
            ...(token === undefined ? {} : { "x-dat-skill-flow-token": token }),
        },
        body: JSON.stringify(body),
    });
}

afterEach(async () => {
    await Promise.all(servers.splice(0).map((server) => new Promise      ((resolveClose) => server.close(() => resolveClose()))));
});

describe("safe workspace HTTP API", () => {
    it("keeps root grant disabled by default", async () => {
        const { origin, token, root } = await serverFixture(false);
        const response = await post(origin, token, "/api/workspace/grant", { absoluteRoot: root });
        assert.equal(response.status, 403);
    });

    it("requires exact Origin/Host and token for every state-changing route", async () => {
        const { origin, token, root } = await serverFixture(true);
        assert.equal((await post(origin, undefined, "/api/workspace/grant", { absoluteRoot: root })).status, 403);
        assert.equal((await post(origin, token, "/api/workspace/grant", { absoluteRoot: root }, false)).status, 403);

        const granted = await post(origin, token, "/api/workspace/grant", { absoluteRoot: root });
        assert.equal(granted.status, 200);
        const grantBody = await granted.json()                                ;
        const opened = await post(origin, token, "/api/documents/open", {
            rootId: grantBody.data.rootId,
            path: "source.dat",
        });
        assert.equal(opened.status, 200);
        const openBody = await opened.json()                                    ;
        assert.equal(JSON.stringify(openBody).includes(root), false);

        const read = await fetch(`${origin}/api/documents/${openBody.data.documentId}/read`);
        assert.equal(read.status, 200);
        const readBody = await read.json()                                       ;
        assert.equal(Buffer.from(readBody.data.contentBase64, "base64").toString("utf8"), "api original");

        const saved = await post(origin, token, `/api/documents/${openBody.data.documentId}/save-as`, {
            rootId: grantBody.data.rootId,
            path: "copy.dat",
            contentBase64: Buffer.from("api copy").toString("base64"),
        });
        assert.equal(saved.status, 201);

        const confirmedContent = Buffer.from("confirmed overwrite").toString("base64");
        const challengeResponse = await post(origin, token, `/api/documents/${openBody.data.documentId}/overwrite-challenge`, {
            rootId: grantBody.data.rootId,
            path: "source.dat",
            contentBase64: confirmedContent,
        });
        assert.equal(challengeResponse.status, 200);
        const challengeBody = await challengeResponse.json()                                                            ;
        assert.match(challengeBody.data.contentSha256, /^[a-f0-9]{64}$/);

        const mismatch = await post(origin, token, `/api/documents/${openBody.data.documentId}/overwrite`, {
            challengeId: challengeBody.data.challengeId,
            contentBase64: Buffer.from("not confirmed").toString("base64"),
        });
        assert.equal(mismatch.status, 409);
        const replay = await post(origin, token, `/api/documents/${openBody.data.documentId}/overwrite`, {
            challengeId: challengeBody.data.challengeId,
            contentBase64: confirmedContent,
        });
        assert.equal(replay.status, 409);
    });

    it("rejects oversized JSON bodies before buffering them", async () => {
        const { origin, token } = await serverFixture(true);
        const url = new URL(origin);
        const status = await new Promise        ((resolveResponse, rejectResponse) => {
            const outgoing = request({
                hostname: url.hostname,
                port: url.port,
                path: "/api/workspace/grant",
                method: "POST",
                headers: {
                    Origin: origin,
                    "Content-Type": "application/json",
                    "x-dat-skill-flow-token": token,
                    "Content-Length": String(MAX_JSON_BODY_BYTES + 1),
                },
            }, (response) => {
                response.resume();
                response.once("end", () => {
                    outgoing.destroy();
                    resolveResponse(response.statusCode ?? 0);
                });
            });
            outgoing.once("error", rejectResponse);
            outgoing.flushHeaders();
        });
        assert.equal(status, 413);
    });
});
