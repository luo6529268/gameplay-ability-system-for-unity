// dat-skill-flow-build:20260801034035657-e77b54972a8f44b28020241ce8859fad
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, writeFile } from "node:fs/promises";
                                        
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
    });

    it("rejects oversized JSON bodies before buffering them", async () => {
        const { origin, token } = await serverFixture(true);
        const response = await fetch(`${origin}/api/workspace/grant`, {
            method: "POST",
            headers: {
                Origin: origin,
                "Content-Type": "application/json",
                "x-dat-skill-flow-token": token,
                "Content-Length": String(MAX_JSON_BODY_BYTES + 1),
            },
            body: "{}",
        });
        assert.equal(response.status, 413);
    });
});
