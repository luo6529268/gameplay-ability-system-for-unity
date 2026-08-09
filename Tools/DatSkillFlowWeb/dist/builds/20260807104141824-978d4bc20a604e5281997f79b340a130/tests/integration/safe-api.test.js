// dat-skill-flow-build:20260807104141824-978d4bc20a604e5281997f79b340a130
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
                                                                                     
import { WorkspaceRegistry, WorkspaceSecurityError } from "../../src/server/workspace-registry.js";

const servers           = [];

async function serverFixture(
    allowAbsoluteRootGrant         ,
    startupWorkspace = false,
    projectSkillService                      ,
)           
                   
                   
                  
                 
                                 
                           
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
    const workspace = new WorkspaceRegistry({ allowAbsoluteRootGrant });
    const startupGrant = startupWorkspace
        ? await workspace.authorizeStartupRoot(root)
        : undefined;
    const server = createApplicationServer({
        staticRoot,
        manifestPath,
        workspace,
        projectSkillService,
    });
    servers.push(server);
    const origin = await listenLoopback(server, 0);
    return { server, origin, token: getApplicationServerSecurity(server).token, root, workspace, startupRootId: startupGrant?.rootId };
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

async function getStatusWithHeaders(origin        , path        , headers                                  )                  {
    const url = new URL(origin);
    return await new Promise        ((resolveResponse, rejectResponse) => {
        const outgoing = request({
            hostname: url.hostname,
            port: url.port,
            path,
            method: "GET",
            headers,
        }, (response) => {
            response.resume();
            response.once("end", () => resolveResponse(response.statusCode ?? 0));
        });
        outgoing.once("error", rejectResponse);
        outgoing.end();
    });
}

async function requestStatus(origin        , path        , method        , headers                                   = {})                  {
    const url = new URL(origin);
    return await new Promise        ((resolveResponse, rejectResponse) => {
        const outgoing = request({ hostname: url.hostname, port: url.port, path, method, headers }, (response) => {
            response.resume();
            response.once("end", () => resolveResponse(response.statusCode ?? 0));
        });
        outgoing.once("error", rejectResponse);
        outgoing.end();
    });
}

afterEach(async () => {
    await Promise.all(servers.splice(0).map((server) => new Promise      ((resolveClose) => server.close(() => resolveClose()))));
});

describe("safe workspace HTTP API", () => {
    it("returns a no-store bootstrap token without inventing an unconfigured root capability", async () => {
        const { origin, token, root } = await serverFixture(false);
        const response = await fetch(`${origin}/api/bootstrap`, { headers: { Origin: origin } });
        const text = await response.text();
        const body = JSON.parse(text)                                                                             ;

        assert.equal(response.status, 200);
        assert.equal(response.headers.get("cache-control"), "no-store");
        assert.deepEqual(Object.keys(body.data).sort(), ["capabilities", "stateToken"]);
        assert.deepEqual(body.data.capabilities, []);
        assert.equal(body.data.rootId, undefined);
        assert.equal(body.data.stateToken, token);
        assert.equal(text.includes(root), false);
        assert.equal(text.includes(root.replaceAll("\\", "\\\\")), false);
    });

    it("bootstraps only the opaque startup root, seals it at listen, and keeps production grant disabled", async () => {
        const { origin, token, root, workspace, startupRootId } = await serverFixture(false, true);
        const response = await fetch(`${origin}/api/bootstrap`);
        const text = await response.text();
        const body = JSON.parse(text)                                                                            ;

        assert.equal(response.status, 200);
        assert.equal(response.headers.get("cache-control"), "no-store");
        assert.deepEqual(Object.keys(body.data).sort(), ["capabilities", "rootId", "stateToken"]);
        assert.equal(body.data.rootId, startupRootId);
        assert.deepEqual(body.data.capabilities, ["documents.open", "documents.read", "documents.save-as", "documents.overwrite"]);
        assert.equal(body.data.stateToken, token);
        assert.equal(text.includes(root), false);
        assert.equal(text.includes(root.replaceAll("\\", "\\\\")), false);
        await assert.rejects(
            workspace.authorizeStartupRoot(root),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "startup-authorization-sealed",
        );
        assert.equal((await post(origin, token, "/api/workspace/grant", { absoluteRoot: root })).status, 403);
    });

    it("protects bootstrap with the active loopback Host and exact optional Origin", async () => {
        const { origin } = await serverFixture(false);
        assert.equal((await fetch(`${origin}/api/bootstrap`, { headers: { Origin: "http://attacker.invalid" } })).status, 403);
        assert.equal(await getStatusWithHeaders(origin, "/api/bootstrap", { Host: "attacker.invalid" }), 403);
        assert.equal((await fetch(`${origin}/api/bootstrap`)).status, 200);
    });

    it("rejects public bootstrap methods and bodies before exposing bootstrap data", async () => {
        const { origin } = await serverFixture(false);
        assert.equal(await requestStatus(origin, "/api/bootstrap", "POST"), 405);
        assert.equal(await requestStatus(origin, "/api/bootstrap", "HEAD"), 405);
        assert.equal(await requestStatus(origin, "/api/bootstrap", "GET", { "Content-Length": "1" }), 413);
    });

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

    it("serves project skills read-only and protects skill writes with the process token", async () => {
        const initial = {
            schemaVersion: 1         ,
            revision: 0,
            etag: "0".repeat(64),
            sidecarStatus: "missing"         ,
            skills: []         ,
        };
        let savedRequest         ;
        const projectSkillService = {
            get: async () => initial,
            save: async (input         ) => {
                savedRequest = input;
                return {
                    schemaVersion: 1         ,
                    revision: 1,
                    etag: "1".repeat(64),
                    sidecarStatus: "valid"         ,
                    skills: [{ oid: 2, displayName: "影分身", startFrame: 300 }],
                };
            },
        }                       ;
        const { origin, token } = await serverFixture(false, false, projectSkillService);

        const read = await fetch(`${origin}/api/project/skills`);
        assert.equal(read.status, 200);
        assert.equal(read.headers.get("cache-control"), "no-store");
        assert.deepEqual((await read.json()                     ).data, initial);
        assert.equal((await post(origin, undefined, "/api/project/skills", {
            expectedRevision: 0,
            expectedEtag: initial.etag,
            skills: [],
        })).status, 403);

        const requestBody = {
            expectedRevision: 0,
            expectedEtag: initial.etag,
            skills: [{ oid: 2, displayName: "影分身", startFrame: 300 }],
        };
        const saved = await post(origin, token, "/api/project/skills", requestBody);
        assert.equal(saved.status, 200);
        assert.deepEqual(savedRequest, requestBody);
        assert.equal((await saved.json()                                  ).data.revision, 1);
    });
});
