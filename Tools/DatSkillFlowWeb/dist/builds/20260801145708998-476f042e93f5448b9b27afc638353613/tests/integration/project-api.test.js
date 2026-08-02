// dat-skill-flow-build:20260801145708998-476f042e93f5448b9b27afc638353613
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, writeFile } from "node:fs/promises";
                                        
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { afterEach, describe, it } from "node:test";

import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";
import { ProjectDatService } from "../../src/server/project-dat-service.js";
import {
    createApplicationServer,
    getApplicationServerSecurity,
    listenLoopback,
} from "../../src/server/server.js";
import {
    NativeSafeFileError,
                           
                              
                              
} from "../../src/server/windows-safe-file-adapter.js";
import { WorkspaceRegistry } from "../../src/server/workspace-registry.js";
import { syntheticBmp } from "../unit/gate1a-fixtures.js";

const servers           = [];

function normalized(path        )         {
    return path.replaceAll("\\", "/");
}

class OverlayNativeClient                                 {
             reads                                               = [];
             #files = new Map                                               ();

    set(root        , logicalPath        , bytes            )       {
        const key = `${root}|${normalized(logicalPath)}`;
        const previous = this.#files.get(key);
        this.#files.set(key, { bytes: Buffer.from(bytes), generation: (previous?.generation ?? 0) + 1 });
    }

    async inspectRoot(request                          )                                {
        return {
            canonicalPath: request.absoluteRoot,
            volumeSerial: `volume:${request.absoluteRoot}`,
            fileId: `root:${request.absoluteRoot}`,
        };
    }

    async read(request                   ) {
        const logicalPath = normalized(request.logicalPath);
        this.reads.push({ root: request.root.canonicalPath, logicalPath });
        const file = this.#files.get(`${request.root.canonicalPath}|${logicalPath}`);
        if (file === undefined) throw new NativeSafeFileError("not-a-file", "not found");
        if (file.bytes.length > request.maximumBytes) throw new NativeSafeFileError("read-too-large", "too large");
        return {
            canonicalPath: `${request.root.canonicalPath}/${logicalPath}`,
            bytes: Buffer.from(file.bytes),
            fingerprint: {
                sha256: createHash("sha256").update(file.bytes).digest("hex"),
                size: file.bytes.length,
                modifiedNanoseconds: String(file.generation),
                changedNanoseconds: String(file.generation),
                device: request.root.volumeSerial,
                inode: `${request.root.fileId}:${logicalPath}`,
            },
        };
    }

    async saveAs()                 { throw new Error("save deferred"); }
    async overwrite()                 { throw new Error("save deferred"); }
}

function catalogBytes(validDatPath        )         {
    return Buffer.from([
        "<object>\n",
        "id: 2 type: 0 file: Assets\\NTSD\\Config\\missing.dat\n",
        `id: 2 type: 0 file: ${validDatPath}\n`,
        "id: 4 type: 3 file: ..\\outside.dat\n",
        "id: 1000 type: 0 file: ignored.dat\n",
        "<object_end>\n",
    ].join(""), "latin1");
}

function narutoDat()         {
    const plaintext = Buffer.from([
        "name: Naruto\n",
        "head: secret\\head.bmp\n",
        "file(0-0): sprite\\naruto.bmp w: 1 h: 1 row: 1 col: 1\n",
        "file(1-1): ..\\outside.bmp w: 1 h: 1 row: 1 col: 1\n",
        "file(2-2): sprite\\invalid.bmp w: 1 h: 1 row: 1 col: 1\n",
        "<frame> 0 idle\n",
        "pic: 0 state: 0 wait: 1 next: 0 sound: secret\\sound.wav\n",
        "<frame_end>\n",
    ].join(""), "latin1");
    return encryptDatPayload(Buffer.alloc(123, 0x41), plaintext);
}

async function staticFixture()                                                        {
    const staticRoot = await mkdtemp(join(tmpdir(), "dat-flow-project-static-"));
    const buildId = "project-api-build";
    const clientRoot = join(staticRoot, "builds", buildId);
    await mkdir(clientRoot, { recursive: true });
    const index = Buffer.from("project-api");
    await writeFile(join(clientRoot, "index.html"), index);
    const entry = {
        path: "index.html",
        buildId,
        size: index.length,
        sha256: createHash("sha256").update(index).digest("hex"),
    };
    const manifestPath = join(staticRoot, "build-manifest.json");
    await writeFile(manifestPath, JSON.stringify({
        schemaVersion: 1,
        buildId,
        clientRoot: `builds/${buildId}`,
        serverEntry: `builds/${buildId}/src/server/cli.js`,
        testFiles: [],
        runtimeAssets: [],
        outputs: [{ ...entry, path: `builds/${buildId}/index.html` }],
        clientFiles: [entry],
    }));
    return { staticRoot, manifestPath };
}

async function post(origin        , token                    , path        , body         , requestOrigin = origin)                    {
    return await fetch(`${origin}${path}`, {
        method: "POST",
        headers: {
            Origin: requestOrigin,
            "Content-Type": "application/json",
            ...(token === undefined ? {} : { "x-dat-skill-flow-token": token }),
        },
        body: JSON.stringify(body),
    });
}

afterEach(async () => {
    await Promise.all(servers.splice(0).map((server) => new Promise      ((resolveClose) => server.close(() => resolveClose()))));
});

describe("Naruto project DAT HTTP vertical slice", () => {
    it("opens OID 2 through a primary/fallback overlay, edits it, and serves verified BMP bytes without path leaks", async () => {
        const primaryRoot = resolve("project-api-primary");
        const assetRoot = resolve("project-api-assets");
        const dataTxtPath = "Assets/NTSD/Config/data.txt";
        const datPath = "Assets/NTSD/Config/2.dat";
        const native = new OverlayNativeClient();
        native.set(primaryRoot, dataTxtPath, catalogBytes(datPath));
        native.set(primaryRoot, datPath, narutoDat());
        const bmp = syntheticBmp(24, 2, 2);
        native.set(assetRoot, "sprite/naruto.bmp", bmp);
        native.set(assetRoot, "sprite/invalid.bmp", Buffer.from("not a bmp"));

        const primary = new WorkspaceRegistry({ nativeClient: native });
        const assets = new WorkspaceRegistry({ nativeClient: native });
        await primary.authorizeStartupRoot(primaryRoot);
        await assets.authorizeStartupRoot(assetRoot);
        primary.sealStartupAuthorization();
        assets.sealStartupAuthorization();
        const previewInputs           = [];
        const project = await ProjectDatService.initialize({
            primaryRegistry: primary,
            assetRegistry: assets,
            dataTxtLogicalPath: dataTxtPath,
            previewRunner: {
                preview: async (plaintext            ) => {
                    const input = Buffer.from(plaintext);
                    previewInputs.push(input);
                    const pic = /pic:\s*2\b/.test(input.toString("latin1")) ? 2 : 0;
                    return {
                        metadata: {
                            runtime: "ntsd_cpp",
                            tick_driver: "SimulationTickDriver",
                            renderer: "none",
                            seed: 1,
                            start_frame: 300,
                            ticks_requested: 30,
                            naruto_dat_override: `${primaryRoot}/secret-preview.dat`,
                            stage: { index: 0, data_path: "data/stage.dat", name: "Stage", width: 1000, z_min: 0, z_max: 500 },
                        },
                        ticks: [{
                            tick: 0,
                            camera_x: 0,
                            camera_vel: 0,
                            bg: { width: 1000, z_min: 0, z_max: 500, bound_left: 0, bound_right: 1000 },
                            entities: [{
                                slot: 0, oid: 2, frame: 300, pic, facing: 0,
                                x: 320, y: 0, z: 500, x_int: 320, y_int: 0, z_int: 500,
                                v: { x: 0, y: 0, z: 0 }, render_offset_x: 0, frame_delay: 0,
                                team: 1, target: 1, holder: -1, link: 0, ai: false,
                            }],
                        }],
                    };
                },
            },
        });
        const staticFiles = await staticFixture();
        const server = createApplicationServer({ ...staticFiles, workspace: primary, projectDatService: project });
        servers.push(server);
        const origin = await listenLoopback(server, 0);
        const token = getApplicationServerSecurity(server).token;

        const catalogResponse = await fetch(`${origin}/api/project`);
        const catalogText = await catalogResponse.text();
        const catalog = JSON.parse(catalogText)                                                                                                                                      ;
        assert.equal(catalogResponse.status, 200);
        assert.equal(catalogResponse.headers.get("cache-control"), "no-store");
        assert.deepEqual(catalog.data.objects.map((entry) => entry.oid), [2, 4]);
        assert.equal(catalog.data.objects[0]?.availablePrimary, true);
        assert.equal(catalogText.includes(primaryRoot), false);
        assert.equal(catalogText.includes(assetRoot), false);
        assert.equal(catalogText.includes(dataTxtPath), false);

        const objectKey = catalog.data.objects[0] .objectKey;
        assert.equal((await post(origin, undefined, "/api/project/open", { objectKey })).status, 403);
        assert.equal((await post(origin, token, "/api/project/open", { objectKey }, "http://attacker.invalid")).status, 403);
        assert.equal((await post(origin, token, "/api/project/open", { objectKey, path: datPath })).status, 400);

        const openedResponse = await post(origin, token, "/api/project/open", { objectKey });
        const openedText = await openedResponse.text();
        const opened = JSON.parse(openedText)             
                              
                             
                        
                         
                         
                                                                                                  
                                                     
                                                                                                               
                                                        
           ;
        assert.equal(openedResponse.status, 200);
        assert.equal(opened.data.oid, 2);
        assert.equal(opened.data.name, "Naruto");
        assert.equal(opened.data.preview.ticks[0]?.entities[0]?.pic, 0);
        assert.equal(opened.data.preview.metadata.runtime, "ntsd_cpp");
        assert.equal(opened.data.preview.metadata.tickDriver, "SimulationTickDriver");
        assert.equal(Object.hasOwn(opened.data.preview.metadata, "naruto_dat_override"), false);
        assert.equal(Object.hasOwn(opened.data.preview.metadata, "data_path"), false);
        assert.equal(opened.data.spriteRanges.length, 2, "unsafe traversal range is not minted");
        assert.ok(opened.data.fields.some((field) => field.key === "name" && field.kind === "string"));
        assert.ok(opened.data.fields.some((field) => field.key === "pic" && field.kind === "number"));
        assert.equal(opened.data.fields.some((field) => ["head", "small", "file", "sound"].includes(field.key)), false);
        for (const secret of [primaryRoot, assetRoot, dataTxtPath, datPath, "sprite/naruto.bmp", "outside.bmp", "secret\\sound.wav", "data/stage.dat"]) {
            assert.equal(openedText.includes(secret), false, `response leaked ${secret}`);
        }
        assert.ok(opened.data.diagnostics.every((diagnostic) => !Object.hasOwn(diagnostic, "path") && !Object.hasOwn(diagnostic, "span")));
        assert.equal(native.reads.some((read) => read.logicalPath.includes("..")), false);

        const pic = opened.data.fields.find((field) => field.key === "pic") ;
        const editedResponse = await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pic.fieldId,
            value: 2,
            expectedRevision: 0,
        });
        const edited = await editedResponse.json()                                                                                                   ;
        assert.equal(editedResponse.status, 200);
        assert.equal(edited.data.revision, 1);
        assert.equal(edited.data.preview.ticks[0]?.entities[0]?.pic, 2);
        assert.equal(previewInputs.length, 2);
        assert.equal((await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pic.fieldId,
            value: 3,
            expectedRevision: 0,
        })).status, 409);

        const validAsset = await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}`);
        assert.equal(validAsset.status, 200);
        assert.equal(validAsset.headers.get("content-type"), "image/bmp");
        assert.equal(validAsset.headers.get("cache-control"), "no-store");
        assert.equal(validAsset.headers.get("x-content-type-options"), "nosniff");
        assert.deepEqual(Buffer.from(await validAsset.arrayBuffer()), bmp);
        const invalidAsset = await fetch(`${origin}/api/assets/${opened.data.spriteRanges[1] .assetId}`);
        const invalidText = await invalidAsset.text();
        assert.equal(invalidAsset.status, 422);
        assert.equal(invalidText.includes("invalid.bmp"), false);
        assert.equal((await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}?path=../outside`)).status, 400);

        native.set(primaryRoot, dataTxtPath, catalogBytes("Assets/NTSD/Config/replaced.dat"));
        const refreshed = await (await fetch(`${origin}/api/project`)).json()                                                                                ;
        assert.ok(refreshed.data.catalogRevision > catalog.data.catalogRevision);
        assert.notEqual(refreshed.data.objects[0]?.objectKey, objectKey);
        assert.equal((await post(origin, token, "/api/project/open", { objectKey })).status, 404);
        assert.equal((await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pic.fieldId,
            value: 3,
            expectedRevision: 1,
        })).status, 404);
    });
});
