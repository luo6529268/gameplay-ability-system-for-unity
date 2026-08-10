// dat-skill-flow-build:20260809153134108-02a60c47c3064d65b3218e6c7fe0b8de
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, writeFile } from "node:fs/promises";
                                        
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { afterEach, describe, it } from "node:test";

import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";
import {
    CppNativeDatPreviewRunner,
    ProjectDatService,
} from "../../src/server/project-dat-service.js";
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

    bytes(root        , logicalPath        )         {
        const file = this.#files.get(`${root}|${normalized(logicalPath)}`);
        if (file === undefined) throw new Error("missing test file");
        return Buffer.from(file.bytes);
    }

    async inspectRoot(request                          )                                {
        return {
            canonicalPath: request.absoluteRoot,
            volumeSerial: `volume:${request.absoluteRoot}`,
            fileId: `root:${request.absoluteRoot}`,
        };
    }

    async ensureDirectory(request                                                     )                                     {
        return { canonicalPath: `${request.root.canonicalPath}/${normalized(request.logicalPath)}` };
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

    async overwrite(request                        ) {
        const logicalPath = normalized(request.logicalPath);
        const key = `${request.root.canonicalPath}|${logicalPath}`;
        const current = this.#files.get(key);
        if (current === undefined) throw new NativeSafeFileError("not-a-file", "not found");
        const originalSha256 = createHash("sha256").update(current.bytes).digest("hex");
        if (originalSha256 !== request.expectedFingerprint.sha256) {
            throw new NativeSafeFileError("external-change", "changed");
        }
        const bytes = Buffer.from(request.bytes);
        const generation = current.generation + 1;
        this.#files.set(key, { bytes, generation });
        const sha256 = createHash("sha256").update(bytes).digest("hex");
        return {
            canonicalPath: `${request.root.canonicalPath}/${logicalPath}`,
            fingerprint: {
                sha256,
                size: bytes.length,
                modifiedNanoseconds: String(generation),
                changedNanoseconds: String(generation),
                device: request.root.volumeSerial,
                inode: `${request.root.fileId}:${logicalPath}`,
            },
            recovery: {
                target: {
                    path: `${request.root.canonicalPath}/${logicalPath}`,
                    exists: true,
                    size: bytes.length,
                    sha256,
                },
                replacement: {
                    path: `${request.root.canonicalPath}/${request.replacementName}`,
                    exists: false,
                },
                backup: {
                    path: `${request.root.canonicalPath}/${request.backupName}`,
                    exists: true,
                    size: current.bytes.length,
                    sha256: originalSha256,
                },
            },
        };
    }
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
        "file(0-0): Assets\\NTSD\\Sprite\\Character\\MingRen\\naruto.bmp w: 1 h: 1 row: 1 col: 1\n",
        "file(1-1): ..\\outside.bmp w: 1 h: 1 row: 1 col: 1\n",
        "file(2-2): Assets\\NTSD\\Sprite\\Character\\MingRen\\invalid.bmp w: 1 h: 1 row: 1 col: 1\n",
        "file(3-3): Assets\\NTSD\\Sprite\\\\invalid-empty-segment.bmp w: 1 h: 1 row: 1 col: 1\n",
        "<frame> 0 idle\n",
        "pic: 0 state: 0 wait: 1 next: 0 sound: secret\\sound.wav\n",
        "itr:\n",
        " catchingact: 5 6\n",
        "itr_end:\n",
        "bdy:\n",
        " x: 10 y: 20 w: 30 h: 40\n",
        "bdy_end:\n",
        "<frame_end>\n",
    ].join(""), "latin1");
    return encryptDatPayload(Buffer.alloc(123, 0x41), plaintext);
}

function nativePreview(startFrame = 300, ticksRequested = 30, rootOid = 2)          {
    return {
        metadata: {
            runtime: "ntsd_cpp",
            tick_driver: "SimulationTickDriver",
            renderer: "none",
            seed: 1,
            start_frame: startFrame,
            ticks_requested: ticksRequested,
            stage: { index: 0, name: "Stage", width: 1000, z_min: 0, z_max: 500 },
        },
        ticks: [{
            tick: 0,
            camera_x: 0,
            camera_vel: 0,
            bg: { width: 1000, z_min: 0, z_max: 500, bound_left: 0, bound_right: 1000 },
            entities: [{
                slot: 0, oid: rootOid, frame: startFrame, pic: 0, facing: 0,
                x: 320, y: 0, z: 500, x_int: 320, y_int: 0, z_int: 500,
                v: { x: 0, y: 0, z: 0 }, render_offset_x: 0, frame_delay: 0,
                team: 1, target: 1, holder: -1, link: 0, ai: false,
            }],
        }],
    };
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

describe("native DAT preview output bounds", () => {
    it("reuses completed Native output for the same DAT revision and scenario", async () => {
        let executions = 0;
        const runner = new CppNativeDatPreviewRunner({
            executable: "preview-test.exe",
            workingDirectory: tmpdir(),
            gameRoot: "J:\\QQFile\\NTSD 2.4.1",
            execFile: (_file, args, _options, callback) => {
                executions += 1;
                const outputPath = args[args.indexOf("--output") + 1] ;
                void writeFile(outputPath, "{}", "utf8").then(
                    () => callback(null, "", ""),
                    (error         ) => callback(error instanceof Error ? error : new Error(String(error)), "", ""),
                );
            },
        });
        const plaintext = Buffer.from("name: Naruto\n", "latin1");
        const scenario = { rootOid: 2, startFrame: 265, initialFrame: 0, ticks: 120 };

        const [first, concurrent] = await Promise.all([
            runner.preview(plaintext, scenario),
            runner.preview(plaintext, scenario),
        ]);
        const repeated = await runner.preview(plaintext, scenario);

        assert.deepEqual(first, {});
        assert.strictEqual(concurrent, first);
        assert.strictEqual(repeated, first);
        assert.equal(executions, 1);
        await runner.preview(plaintext, { ...scenario, startFrame: 271 });
        await runner.preview(Buffer.from("name: Edited Naruto\n", "latin1"), scenario);
        assert.equal(executions, 3);
        await runner.preview(plaintext, { ...scenario, rootOid: 3 });
        assert.equal(executions, 4, "the selected root character participates in the Native cache key");
    });

    it("passes the selected entry, real initial frame, and deterministic input plan to Native", async () => {
        let capturedArguments                    = [];
        const runner = new CppNativeDatPreviewRunner({
            executable: "preview-test.exe",
            workingDirectory: tmpdir(),
            execFile: (_file, args, _options, callback) => {
                capturedArguments = args;
                const outputPath = args[args.indexOf("--output") + 1];
                if (outputPath === undefined) {
                    callback(new Error("missing preview output path"), "", "");
                    return;
                }
                void writeFile(outputPath, "{}", "utf8").then(
                    () => callback(null, "", ""),
                    (error         ) => callback(error instanceof Error ? error : new Error(String(error)), "", ""),
                );
            },
        });

        await runner.preview(Buffer.from("name: Naruto\n", "latin1"), {
            rootOid: 3,
            startFrame: 265,
            initialFrame: 0,
            ticks: 120,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["W"] },
                { tick: 6, keys: ["J"] },
            ],
        });

        const gameRootIndex = capturedArguments.indexOf("--game-root");
        assert.deepEqual(capturedArguments.slice(gameRootIndex, gameRootIndex + 2), [
            "--game-root", "J:\\QQFile\\NTSD 2.4.1",
        ]);
        const previewDatIndex = capturedArguments.indexOf("--preview-dat");
        assert.ok(previewDatIndex >= 0 && capturedArguments[previewDatIndex + 1]?.endsWith("preview-character.dat"));
        const rootOidIndex = capturedArguments.indexOf("--root-oid");
        assert.deepEqual(capturedArguments.slice(rootOidIndex, rootOidIndex + 2), ["--root-oid", "3"]);
        assert.deepEqual(capturedArguments.slice(capturedArguments.indexOf("--start-frame")), [
            "--start-frame", "0",
            "--entry-frame", "265",
            "--ticks", "120",
            "--input-plan", "2:L,4:W,6:J",
        ]);
    });

    it("rejects output larger than 8 MiB before parsing it", async () => {
        const runner = new CppNativeDatPreviewRunner({
            executable: "preview-test.exe",
            workingDirectory: tmpdir(),
            execFile: (_file, args, _options, callback) => {
                const outputPath = args[args.indexOf("--output") + 1];
                if (outputPath === undefined) {
                    callback(new Error("missing preview output path"), "", "");
                    return;
                }
                void writeFile(outputPath, Buffer.alloc(8 * 1024 * 1024 + 1))
                    .then(
                        () => callback(null, "", ""),
                        (error         ) => callback(
                            error instanceof Error ? error : new Error(String(error)),
                            "",
                            "",
                        ),
                    );
            },
        });

        await assert.rejects(
            runner.preview(Buffer.from("name: Naruto\n", "latin1")),
            /Native preview output exceeds its limit/,
        );
    });
});

describe("Naruto project DAT HTTP vertical slice", () => {
    it("opens OID 2 through a primary/fallback overlay, edits it, and serves verified BMP bytes without path leaks", async () => {
        const primaryRoot = resolve("project-api-primary");
        const assetRoot = resolve("project-api-assets");
        const dataTxtPath = "Assets/NTSD/Config/data.txt";
        const datPath = "Assets/NTSD/Config/2.dat";
        const native = new OverlayNativeClient();
        native.set(primaryRoot, dataTxtPath, catalogBytes(datPath));
        const originalDat = narutoDat();
        native.set(primaryRoot, datPath, originalDat);
        const bmp = syntheticBmp(24, 2, 2);
        native.set(assetRoot, "sprite/sys/naruto.bmp", bmp);
        native.set(assetRoot, "sprite/sys/invalid.bmp", Buffer.from("not a bmp"));

        const primary = new WorkspaceRegistry({ nativeClient: native });
        const assets = new WorkspaceRegistry({ nativeClient: native });
        await primary.authorizeStartupRoot(primaryRoot);
        await assets.authorizeStartupRoot(assetRoot);
        primary.sealStartupAuthorization();
        assets.sealStartupAuthorization();
        const previewInputs           = [];
        const previewOptions         
                             
                                
                                  
                           
                                                                                                                
                       = [];
        let previewEntityTransform                                                                       ;
        const project = await ProjectDatService.initialize({
            primaryRegistry: primary,
            assetRegistry: assets,
            dataTxtLogicalPath: dataTxtPath,
            idFactory: () => "a".repeat(32),
            previewRunner: {
                preview: async (plaintext            , options    
                                     
                                        
                                          
                                   
                                                                                                                        
                 ) => {
                    const input = Buffer.from(plaintext);
                    previewInputs.push(input);
                    previewOptions.push(options);
                    const pic = /pic:\s*2\b/.test(input.toString("latin1")) ? 2 : 0;
                    const entities                                 = [{
                        slot: 0, oid: 2, frame: options?.startFrame ?? 300, pic, facing: 0,
                        x: 320, y: 0, z: 500, x_int: 320, y_int: 0, z_int: 500,
                        v: { x: 0, y: 0, z: 0 }, render_offset_x: 0, frame_delay: 0,
                        team: 1, target: 1, holder: -1, link: 0, ai: false,
                    }];
                    return {
                        metadata: {
                            runtime: "ntsd_cpp",
                            tick_driver: "SimulationTickDriver",
                            renderer: "none",
                            seed: 1,
                            start_frame: options?.startFrame ?? 300,
                            ticks_requested: options?.ticks ?? 30,
                            naruto_dat_override: `${primaryRoot}/secret-preview.dat`,
                            stage: { index: 0, data_path: "data/stage.dat", name: "Stage", width: 1000, z_min: 0, z_max: 500 },
                        },
                        render_resources: [{
                            oid: 2,
                            type: 0,
                            name: "Naruto",
                            ranges: [
                                {
                                    file: "Assets\\NTSD\\Sprite\\Character\\MingRen\\naruto.bmp",
                                    frame_lo: 0, frame_hi: 0, w: 1, h: 1, row: 1, col: 1,
                                },
                                {
                                    file: "Assets\\NTSD\\Sprite\\Character\\MingRen\\invalid.bmp",
                                    frame_lo: 2, frame_hi: 2, w: 1, h: 1, row: 1, col: 1,
                                },
                            ],
                            frames: [{
                                frame_id: options?.startFrame ?? 300,
                                pic,
                                state: 0,
                                center_x: 0,
                                center_y: 0,
                            }],
                        }],
                        ticks: [{
                            tick: 0,
                            camera_x: 0,
                            camera_vel: 0,
                            bg: { width: 1000, z_min: 0, z_max: 500, bound_left: 0, bound_right: 1000 },
                            entities: previewEntityTransform?.(entities) ?? entities,
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
        assert.deepEqual(catalog.data.objects.map((entry) => entry.oid), [2]);
        assert.equal(catalog.data.objects[0]?.availablePrimary, true);
        assert.equal(catalogText.includes(primaryRoot), false);
        assert.equal(catalogText.includes(assetRoot), false);
        assert.equal(catalogText.includes(dataTxtPath), false);

        const objectKey = catalog.data.objects[0] .objectKey;
        assert.equal((await post(origin, undefined, "/api/project/open", { objectKey })).status, 403);
        assert.equal((await post(origin, token, "/api/project/open", { objectKey }, "http://attacker.invalid")).status, 403);
        assert.equal((await post(origin, token, "/api/project/open", { objectKey, path: datPath })).status, 400);
        const wrongMethod = await fetch(`${origin}/api/project/open`);
        assert.equal(wrongMethod.status, 405);
        assert.equal(wrongMethod.headers.get("allow"), "POST");

        const openedResponse = await post(origin, token, "/api/project/open", { objectKey });
        const openedText = await openedResponse.text();
        const opened = JSON.parse(openedText)             
                              
                             
                           
                              
                        
                         
                         
                                                              
                                                                                                             
                                          
                                     
                                   
                                                                                               
               
                                                     
                                                                                                               
                                                        
           ;
        assert.equal(openedResponse.status, 200);
        assert.equal(opened.data.dirty, false);
        assert.equal(opened.data.writable, true);
        assert.equal(opened.data.oid, 2);
        assert.equal(opened.data.name, "Naruto");
        assert.equal(opened.data.frames[0]?.frameId, 0);
        assert.equal(opened.data.frames[0]?.label, "idle");
        assert.equal(opened.data.preview.ticks[0]?.entities[0]?.pic, 0);
        assert.equal(opened.data.preview.metadata.runtime, "ntsd_cpp");
        assert.equal(opened.data.preview.metadata.tickDriver, "SimulationTickDriver");
        assert.equal(Object.hasOwn(opened.data.preview.metadata, "naruto_dat_override"), false);
        assert.equal(Object.hasOwn(opened.data.preview.metadata, "data_path"), false);
        assert.equal(opened.data.spriteRanges.length, 2, "unsafe sprite paths do not mint capabilities");
        assert.equal(
            native.reads.filter((read) => /\.bmp$/i.test(read.logicalPath)).length,
            0,
            "project opening must not block on BMP reads",
        );
        assert.ok(opened.data.fields.some((field) => field.key === "name" && field.kind === "string"));
        assert.ok(opened.data.fields.some((field) => field.key === "pic" && field.kind === "number"));
        assert.ok(opened.data.fields.some((field) => field.key === "catchingact" && field.kind === "integer-pair"));
        assert.equal(opened.data.structureCapabilities.length, 1);
        assert.equal(opened.data.structureCapabilities[0]?.blocks.some((block) => block.blockType === "bdy"), true);
        assert.equal(opened.data.fields.some((field) => ["head", "small", "file", "sound"].includes(field.key)), false);
        for (const secret of [primaryRoot, assetRoot, dataTxtPath, datPath, "naruto.bmp", "outside.bmp", "secret\\sound.wav", "data/stage.dat"]) {
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
        const edited = await editedResponse.json()                                                                                                                   ;
        assert.equal(editedResponse.status, 200);
        assert.equal(edited.data.revision, 1);
        assert.equal(edited.data.dirty, true);
        assert.equal(edited.data.preview.ticks[0]?.entities[0]?.pic, 2);
        assert.equal(previewInputs.length, 2);
        const pair = opened.data.fields.find((field) => field.key === "catchingact") ;
        const pairEditedResponse = await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pair.fieldId,
            value: [70, 71],
            expectedRevision: 1,
        });
        const pairEdited = await pairEditedResponse.json()                                                                                      ;
        assert.equal(pairEditedResponse.status, 200);
        assert.equal(pairEdited.data.revision, 2);
        assert.deepEqual(pairEdited.data.fields.find((field) => field.fieldId === pair.fieldId)?.value, [70, 71]);
        assert.match(previewInputs[2]?.toString("latin1") ?? "", /catchingact:\s*70 71/);

        const pairEditedData = pairEdited                        
                             
                                                                                                 
                                          
                                     
                                   
                                                                                               
               
           ;
        const x = pairEditedData.data.fields.find((field) => field.key === "x" && field.blockType === "bdy") ;
        const y = pairEditedData.data.fields.find((field) => field.key === "y" && field.blockType === "bdy") ;
        previewEntityTransform = (entities) => [{ ...entities[0], slot: -1 }];
        assert.equal((await post(origin, token, "/api/project/edit-batch", {
            sessionId: opened.data.sessionId,
            edits: [
                { fieldId: x.fieldId, value: 13 },
                { fieldId: y.fieldId, value: 24 },
            ],
            expectedRevision: 2,
        })).status, 422);
        previewEntityTransform = undefined;
        const batchResponse = await post(origin, token, "/api/project/edit-batch", {
            sessionId: opened.data.sessionId,
            edits: [
                { fieldId: x.fieldId, value: 13 },
                { fieldId: y.fieldId, value: 24 },
            ],
            expectedRevision: 2,
        });
        const batch = await batchResponse.json()             
                             
                                                               
                                                                                    
           ;
        assert.equal(batchResponse.status, 200);
        assert.equal(batch.data.revision, 3);
        assert.equal(batch.data.fields.find((field) => field.fieldId === x.fieldId)?.value, 13);
        assert.equal(batch.data.fields.find((field) => field.fieldId === y.fieldId)?.value, 24);
        assert.equal((await post(origin, token, "/api/project/edit-batch", {
            sessionId: opened.data.sessionId,
            edits: [
                { fieldId: x.fieldId, value: 14 },
                { fieldId: "missing-capability", value: 25 },
            ],
            expectedRevision: 3,
        })).status, 400);

        const bdy = batch.data.structureCapabilities[0] .blocks.find((block) => block.blockType === "bdy") ;
        previewEntityTransform = (entities) => [{ ...entities[0], slot: -1 }];
        assert.equal((await post(origin, token, "/api/project/edit-structure", {
            sessionId: opened.data.sessionId,
            capabilityId: bdy.capabilityId,
            operation: "copy-block",
            expectedRevision: 3,
        })).status, 422);
        previewEntityTransform = undefined;
        const copiedBlockResponse = await post(origin, token, "/api/project/edit-structure", {
            sessionId: opened.data.sessionId,
            capabilityId: bdy.capabilityId,
            operation: "copy-block",
            expectedRevision: 3,
        });
        const copiedBlock = await copiedBlockResponse.json()             
                             
                                                                                    
           ;
        assert.equal(copiedBlockResponse.status, 200);
        assert.equal(copiedBlock.data.revision, 4);
        assert.equal(copiedBlock.data.structureCapabilities[0]?.blocks.filter((block) => block.blockType === "bdy").length, 2);
        assert.equal((await post(origin, token, "/api/project/edit-structure", {
            sessionId: opened.data.sessionId,
            capabilityId: bdy.capabilityId,
            operation: "delete-block",
            expectedRevision: 4,
        })).status, 400, "structure capabilities rotate after every structure transaction");

        const frameCapability = copiedBlock.data.structureCapabilities[0] ;
        const copiedFrameResponse = await post(origin, token, "/api/project/edit-structure", {
            sessionId: opened.data.sessionId,
            capabilityId: frameCapability.capabilityId,
            operation: "copy-frame",
            newFrameId: 17,
            expectedRevision: 4,
        });
        const copiedFrame = await copiedFrameResponse.json()             
                             
                                               
           ;
        assert.equal(copiedFrameResponse.status, 200);
        assert.equal(copiedFrame.data.revision, 5);
        assert.equal(copiedFrame.data.frames.some((frame) => frame.frameId === 17), true);
        assert.equal((await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pic.fieldId,
            value: 3,
            expectedRevision: 0,
        })).status, 409);

        const previewResponse = await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 265,
            initialFrame: 0,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["W"] },
                { tick: 6, keys: ["J"] },
            ],
            ticks: 8,
        });
        const preview = await previewResponse.json()                                                                                       ;
        assert.equal(previewResponse.status, 200);
        assert.equal(preview.data.preview.metadata.startFrame, 265);
        assert.equal(preview.data.preview.metadata.ticksRequested, 8);
        assert.deepEqual(previewOptions.at(-1), {
            rootOid: 2,
            startFrame: 265,
            initialFrame: 0,
            ticks: 8,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["W"] },
                { tick: 6, keys: ["J"] },
            ],
        });
        const previewCallsAfterFirst = previewOptions.length;
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 265,
            initialFrame: 0,
            inputPlan: [
                { tick: 2, keys: ["L"] },
                { tick: 4, keys: ["W"] },
                { tick: 6, keys: ["J"] },
            ],
            ticks: 8,
        })).status, 200);
        assert.equal(previewOptions.length, previewCallsAfterFirst, "identical session previews reuse their completed result");
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 599,
            ticks: 1800,
        })).status, 200);
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 600,
            ticks: 2,
        })).status, 400);
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 265,
            initialFrame: 0,
            inputPlan: [{ tick: 2, keys: ["X"] }],
            ticks: 8,
        })).status, 400);
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 0,
            ticks: 1801,
        })).status, 400);
        for (const invalidSlot of [-1, 0.5, 400]) {
            previewEntityTransform = (entities) => [{ ...entities[0], slot: invalidSlot }];
            assert.equal((await post(origin, token, "/api/project/preview", {
                sessionId: opened.data.sessionId,
                expectedRevision: 5,
                startFrame: 0,
                ticks: 2,
            })).status, 422);
        }
        previewEntityTransform = (entities) => [entities[0] , { ...entities[0] }];
        assert.equal((await post(origin, token, "/api/project/preview", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
            startFrame: 0,
            ticks: 2,
        })).status, 422);
        previewEntityTransform = undefined;

        const savedResponse = await post(origin, token, "/api/project/save", {
            sessionId: opened.data.sessionId,
            expectedRevision: 5,
        });
        const savedText = await savedResponse.text();
        const saved = JSON.parse(savedText)             
                             
                           
                       
                                                                          
                                                                          
                                                               
              
           ;
        assert.equal(savedResponse.status, 200);
        assert.equal(saved.data.revision, 5);
        assert.equal(saved.data.dirty, false);
        assert.match(saved.data.recovery.backup.name, /^\.2\.dat\.backup-[A-Za-z0-9-]+\.bak$/);
        assert.equal(saved.data.recovery.backup.sha256, createHash("sha256").update(originalDat).digest("hex"));
        assert.equal(saved.data.recovery.target.sha256, createHash("sha256").update(native.bytes(primaryRoot, datPath)).digest("hex"));
        assert.equal(saved.data.recovery.replacement.exists, false);
        assert.equal(savedText.includes(primaryRoot), false);
        assert.equal(savedText.includes(assetRoot), false);

        const readsBeforeAsset = native.reads.length;
        const validAsset = await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}`);
        assert.equal(validAsset.status, 200);
        assert.equal(validAsset.headers.get("content-type"), "image/bmp");
        assert.equal(validAsset.headers.get("cache-control"), "no-store");
        assert.equal(validAsset.headers.get("x-content-type-options"), "nosniff");
        assert.deepEqual(Buffer.from(await validAsset.arrayBuffer()), bmp);
        const readsAfterAsset = native.reads.length;
        assert.ok(readsAfterAsset > readsBeforeAsset, "the first asset request resolves its safe path lazily");
        assert.equal((await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}`)).status, 200);
        assert.equal(native.reads.length, readsAfterAsset, "resolved asset bytes are reused for the session");
        const invalidAsset = await fetch(`${origin}/api/assets/${opened.data.spriteRanges[1] .assetId}`);
        const invalidText = await invalidAsset.text();
        assert.equal(invalidAsset.status, 422);
        assert.equal(invalidText.includes("invalid.bmp"), false);
        assert.equal((await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}?path=../outside`)).status, 400);

        const closedResponse = await post(origin, token, "/api/project/close", {
            sessionId: opened.data.sessionId,
        });
        const closed = await closedResponse.json()                                                    ;
        assert.equal(closedResponse.status, 200);
        assert.deepEqual(closed.data, { sessionId: opened.data.sessionId, closed: true });
        assert.equal((await fetch(`${origin}/api/assets/${opened.data.spriteRanges[0] .assetId}`)).status, 404);
        assert.equal((await post(origin, token, "/api/project/edit", {
            sessionId: opened.data.sessionId,
            fieldId: pic.fieldId,
            value: 3,
            expectedRevision: 1,
        })).status, 404);
        const persistedResponse = await post(origin, token, "/api/project/open", { objectKey });
        const persisted = await persistedResponse.json()             
                              
                             
                           
                                                           
                                                                
           ;
        assert.equal(persistedResponse.status, 200);
        assert.equal(persisted.data.revision, 0);
        assert.equal(persisted.data.dirty, false);
        assert.equal(persisted.data.fields.find((field) => field.key === "pic")?.value, 2);
        assert.deepEqual(persisted.data.fields.find((field) => field.key === "catchingact")?.value, [70, 71]);
        assert.equal(persisted.data.frames.some((frame) => frame.frameId === 17), true);
        assert.equal(persisted.data.frames.find((frame) => frame.frameId === 0)?.bdys.length, 2);
        assert.equal((await post(origin, token, "/api/project/close", {
            sessionId: persisted.data.sessionId,
        })).status, 200);
        for (let index = 0; index < 33; index += 1) {
            const reopenedResponse = await post(origin, token, "/api/project/open", { objectKey });
            const reopened = await reopenedResponse.json()                                   ;
            assert.equal(reopenedResponse.status, 200);
            assert.equal((await post(origin, token, "/api/project/close", {
                sessionId: reopened.data.sessionId,
            })).status, 200);
        }

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

    it("catalogs only type-0 DAT entries and opens a non-Naruto root character", async () => {
        const primaryRoot = resolve("project-api-multi-character-primary");
        const dataTxtPath = "Assets/NTSD/Config/data.txt";
        const native = new OverlayNativeClient();
        native.set(primaryRoot, dataTxtPath, Buffer.from([
            "<object>\n",
            "id: 2 type: 0 file: Assets\\NTSD\\Config\\2.dat\n",
            "id: 3 type: 0 file: Assets\\NTSD\\Config\\3.dat\n",
            "id: 4 type: 3 file: Assets\\NTSD\\Config\\4.dat\n",
            "<object_end>\n",
        ].join(""), "latin1"));
        native.set(primaryRoot, "Assets/NTSD/Config/2.dat", narutoDat());
        native.set(primaryRoot, "Assets/NTSD/Config/3.dat", narutoDat());

        const primary = new WorkspaceRegistry({ nativeClient: native });
        await primary.authorizeStartupRoot(primaryRoot);
        primary.sealStartupAuthorization();
        const previewRootOids           = [];
        const project = await ProjectDatService.initialize({
            primaryRegistry: primary,
            dataTxtLogicalPath: dataTxtPath,
            idFactory: () => "c".repeat(32),
            previewRunner: {
                preview: async (_plaintext, options) => {
                    const rootOid = options?.rootOid ?? 2;
                    previewRootOids.push(rootOid);
                    return nativePreview(options?.startFrame, options?.ticks, rootOid);
                },
            },
        });

        const catalog = await project.catalog();
        assert.deepEqual(catalog.objects.map((entry) => entry.oid), [2, 3]);
        assert.ok(catalog.objects.every((entry) => entry.type === 0));
        const kakashi = catalog.objects.find((entry) => entry.oid === 3) ;
        const opened = await project.open(kakashi.objectKey);
        assert.equal(opened.oid, 3);
        assert.equal(opened.type, 0);
        assert.equal(opened.preview.ticks[0]?.entities[0]?.oid, 3);
        assert.deepEqual(previewRootOids, [3]);
        await project.close({ sessionId: opened.sessionId });
    });

    it("renews a prepared session when the browser claims it after Native warmup", async () => {
        const primaryRoot = resolve("project-api-prepared-session-primary");
        const dataTxtPath = "Assets/NTSD/Config/data.txt";
        const datPath = "Assets/NTSD/Config/2.dat";
        const native = new OverlayNativeClient();
        native.set(primaryRoot, dataTxtPath, catalogBytes(datPath));
        native.set(primaryRoot, datPath, narutoDat());

        const primary = new WorkspaceRegistry({ nativeClient: native });
        await primary.authorizeStartupRoot(primaryRoot);
        primary.sealStartupAuthorization();
        let now = 1_000;
        const project = await ProjectDatService.initialize({
            primaryRegistry: primary,
            dataTxtLogicalPath: dataTxtPath,
            idFactory: () => "d".repeat(32),
            sessionOptions: { idleTtlMs: 10, now: () => now },
            previewRunner: {
                preview: async (_plaintext, options) => nativePreview(options?.startFrame, options?.ticks),
            },
        });

        const prepared = await project.prepareDefaultSession();
        now += 9;
        const opened = await project.open(prepared.objectKey);
        const pic = opened.fields.find((field) => field.key === "pic") ;
        now += 9;
        const edited = await project.edit({
            sessionId: opened.sessionId,
            fieldId: pic.fieldId,
            value: Number(pic.value) + 1,
            expectedRevision: opened.revision,
        });

        assert.equal(edited.revision, opened.revision + 1);
        await project.close({ sessionId: opened.sessionId });
    });

    it("opens fallback Naruto as an explicit read-only session", async () => {
        const primaryRoot = resolve("project-api-readonly-primary");
        const assetRoot = resolve("project-api-readonly-assets");
        const dataTxtPath = "Assets/NTSD/Config/data.txt";
        const datPath = "Assets/NTSD/Config/2.dat";
        const native = new OverlayNativeClient();
        native.set(primaryRoot, dataTxtPath, catalogBytes(datPath));
        native.set(assetRoot, datPath, narutoDat());

        const primary = new WorkspaceRegistry({ nativeClient: native });
        const assets = new WorkspaceRegistry({ nativeClient: native });
        await primary.authorizeStartupRoot(primaryRoot);
        await assets.authorizeStartupRoot(assetRoot);
        primary.sealStartupAuthorization();
        assets.sealStartupAuthorization();
        const project = await ProjectDatService.initialize({
            primaryRegistry: primary,
            assetRegistry: assets,
            dataTxtLogicalPath: dataTxtPath,
            idFactory: () => "b".repeat(32),
            previewRunner: {
                preview: async (_plaintext, options) => nativePreview(options?.startFrame, options?.ticks),
            },
        });
        const staticFiles = await staticFixture();
        const server = createApplicationServer({ ...staticFiles, workspace: primary, projectDatService: project });
        servers.push(server);
        const origin = await listenLoopback(server, 0);
        const token = getApplicationServerSecurity(server).token;

        const catalog = await (await fetch(`${origin}/api/project`)).json()             
                                                                                          
           ;
        const naruto = catalog.data.objects.find((entry) => entry.oid === 2) ;
        assert.equal(naruto.availablePrimary, false);
        const openedResponse = await post(origin, token, "/api/project/open", { objectKey: naruto.objectKey });
        const opened = await openedResponse.json()             
                              
                             
                           
                              
                                                            
                                                                   
           ;
        assert.equal(openedResponse.status, 200);
        assert.equal(opened.data.writable, false);
        assert.equal(opened.data.dirty, false);
        const pic = opened.data.fields.find((field) => field.key === "pic") ;

        for (const [path, body] of [
            ["/api/project/edit", {
                sessionId: opened.data.sessionId,
                fieldId: pic.fieldId,
                value: 2,
                expectedRevision: opened.data.revision,
            }],
            ["/api/project/edit-batch", {
                sessionId: opened.data.sessionId,
                edits: [{ fieldId: pic.fieldId, value: 2 }],
                expectedRevision: opened.data.revision,
            }],
            ["/api/project/edit-structure", {
                sessionId: opened.data.sessionId,
                capabilityId: opened.data.structureCapabilities[0] .capabilityId,
                operation: "copy-frame",
                newFrameId: 17,
                expectedRevision: opened.data.revision,
            }],
            ["/api/project/save", {
                sessionId: opened.data.sessionId,
                expectedRevision: opened.data.revision,
            }],
        ]         ) {
            const response = await post(origin, token, path, body);
            const text = await response.text();
            assert.equal(response.status, 409);
            assert.match(text, /read-only-session/);
            assert.equal(text.includes(primaryRoot), false);
            assert.equal(text.includes(assetRoot), false);
        }
        assert.equal((await post(origin, token, "/api/project/close", {
            sessionId: opened.data.sessionId,
        })).status, 200);
    });
});
