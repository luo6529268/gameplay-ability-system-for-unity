// dat-skill-flow-build:20260823083835038-310047e284004944984bc88dac7a61f8
import { execFile as nodeExecFile } from "node:child_process";
import { createHash, randomBytes } from "node:crypto";
import { mkdtemp, open as openFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join } from "node:path";

import { parseBmpMetadata } from "../assets/bmp.js";
import { buildFrameEntryCatalog, buildSkillPreviewScenario, deriveSkillEntries } from "../client/skill-entries.js";
import { LosslessDatDocument } from "../model/dat-document.js";
                                                                
import { DataTxtDocument, diagnoseResourcePath,                   } from "../project/data-txt.js";
import { MAX_CATALOG_OIDS } from "../sim/catalog.js";
import {
    DatSessionError,
    DatSessionService,
                                
                            
                                  
                        
} from "./dat-session-service.js";
             
                            
                                  
                          
                           
                          
                      
                         
                       
                         
                        
                     
                             
                        
                       
                                   
import { enrichNativePreview } from "./native-preview-trace.js";
                                                                                       
import { SafeSaveError, SafeSaveService } from "./safe-save.js";
import { WorkspaceRegistry, WorkspaceSecurityError } from "./workspace-registry.js";

const DEFAULT_START_FRAME = 300;
const DEFAULT_TICKS = 30;
const DEFAULT_CHARACTER_OID = 2;
const MAX_PREVIEW_OUTPUT_BYTES = 8 * 1024 * 1024;
const MAX_NATIVE_PREVIEW_CATALOG_BYTES = 256 * 1024 * 1024;
const MAX_NATIVE_PREVIEW_CACHE_ENTRIES = 128;
const MAX_SESSION_PREVIEW_CACHE_ENTRIES = 128;
const CATALOG_REFRESH_INTERVAL_MS = 30_000;
const PREVIEW_WARMUP_CONCURRENCY = 4;
const DEFAULT_CPP_DIRECTORY = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";
const DEFAULT_CPP_GAME_ROOT = process.env.DAT_SKILL_FLOW_CPP_GAME_ROOT
    ?? "J:\\QQFile\\NTSD 2.4.1";
const DEFAULT_CPP_EXECUTABLE = process.env.DAT_SKILL_FLOW_CPP_PREVIEW_EXECUTABLE
    ?? "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\dat_preview_cli.exe";

export function previewDatProjection(bytes            )                {
    return previewDatDocument(bytes).projection;
}

function previewDatDocument(bytes            )                      {
    const input = Buffer.from(bytes);
    let offset = 0;
    if (input.length >= 3 && input[0] === 0xef && input[1] === 0xbb && input[2] === 0xbf) offset = 3;
    const plainBytes = input.subarray(offset);
    const plaintext = LosslessDatDocument.fromPlaintext(plainBytes);
    if (input.length <= 123) return plaintext;

    const encrypted = LosslessDatDocument.fromEncrypted(input);
    if (encrypted.cst.frames.length !== plaintext.cst.frames.length) {
        return encrypted.cst.frames.length > plaintext.cst.frames.length ? encrypted : plaintext;
    }
    if (encrypted.cst.spriteRanges.length !== plaintext.cst.spriteRanges.length) {
        return encrypted.cst.spriteRanges.length > plaintext.cst.spriteRanges.length ? encrypted : plaintext;
    }
    return plaintext;
}

export function previewDatPlaintext(bytes            )         {
    return Buffer.from(previewDatDocument(bytes).emitPlaintext());
}

export function completeActionFrameIds(
    frames                         ,
    oid        ,
    startFrame        ,
)                      {
    const catalog = buildFrameEntryCatalog(frames, oid);
    const ownedFrames = catalog.frames
        .filter((item) => item.effective && item.ownerStartFrames.includes(startFrame))
        .map((item) => item.frame.frameId);
    return new Set(ownedFrames.length > 0 ? ownedFrames : [startFrame]);
}

export class ProjectDatError extends Error {
             code                     ;

    constructor(code                     , message        , options               ) {
        super(message, options);
        this.name = "ProjectDatError";
        this.code = code;
    }
}

                                         
                                                                                     
 

                                            
                         
                          
                                   
 

                                
                              
                                 
                                   
                            
                                                           
                                                     
                                                                   
 

                                           
                                                
                                               
                                               
                                            
                                         
                                                    
                                        
                                                     
                                      
                                                       
 

                         
                      
                
                 
                        
                      
                         
                                 
                                                 
                            
                                            
                                                          
 

                            
                                          
                                 
 

                          
                      
                               
                       
                   
                        
                
                 
                      
                         
                                 
                                                 
                                   
                                                          
                      
                                        
                                                                 
                                        
                                                          
                                                                         
 

                          
                                         
                            
                                 
 

                        
                      
                                               
                                                  
                   
                              
 

                             
                          
                             
                             
                       
                       
                         
                         
 

                             
                             
                         
                           
                             
                             
 

                                
                         
                          
                          
                                                  
                                                  
 

                                  
                                        
                                                              
 

                           
                                
                               
                       
                   
                        
 

                         
                 
                            
                                                                                       
                                                                            
          

export class CppNativeDatPreviewRunner                                   {
             #executable        ;
             #workingDirectory        ;
             #gameRoot        ;
             #execFile                  ;
             #cache = new Map                          ();

    constructor(options   
                            
                                  
                          
                                    
      = {}) {
        this.#executable = options.executable === undefined ? DEFAULT_CPP_EXECUTABLE : options.executable;
        this.#workingDirectory = options.workingDirectory === undefined ? DEFAULT_CPP_DIRECTORY : options.workingDirectory;
        this.#gameRoot = options.gameRoot === undefined ? DEFAULT_CPP_GAME_ROOT : options.gameRoot;
        this.#execFile = options.execFile === undefined ? nodeExecFile                                : options.execFile;
    }

    async preview(plaintext            , options                       = {})                   {
        const rootOid = boundedInteger(
            options.rootOid === undefined ? DEFAULT_CHARACTER_OID : options.rootOid,
            0,
            MAX_CATALOG_OIDS - 1,
            "rootOid",
        );
        const startFrame = boundedInteger(options.startFrame === undefined ? DEFAULT_START_FRAME : options.startFrame, 0, 599, "startFrame");
        const initialFrame = boundedInteger(options.initialFrame === undefined ? startFrame : options.initialFrame, 0, 599, "initialFrame");
        const ticks = boundedInteger(options.ticks === undefined ? DEFAULT_TICKS : options.ticks, 1, 1800, "ticks");
        const inputPlan = options.inputPlan ?? [];
        const initial = options.initial ?? {
            p1: { x: 320, y: 0, z: 500 },
            p2: { x: 360, y: 0, z: 501 },
        };
        const catalogEntries = normalizedNativePreviewCatalog(options.catalogEntries);
        const normalizedOptions = { rootOid, startFrame, initialFrame, ticks, inputPlan, initial, catalogEntries };
        const cacheKey = nativePreviewCacheKey(plaintext, normalizedOptions);
        const cached = this.#cache.get(cacheKey);
        if (cached !== undefined) {
            this.#cache.delete(cacheKey);
            this.#cache.set(cacheKey, cached);
            return await cached;
        }
        while (this.#cache.size >= MAX_NATIVE_PREVIEW_CACHE_ENTRIES) {
            const oldest = this.#cache.keys().next().value                      ;
            if (oldest === undefined) break;
            this.#cache.delete(oldest);
        }
        const pending = this.#run(plaintext, rootOid, startFrame, initialFrame, ticks, inputPlan, initial, catalogEntries);
        this.#cache.set(cacheKey, pending);
        try {
            return await pending;
        } catch (error) {
            if (this.#cache.get(cacheKey) === pending) this.#cache.delete(cacheKey);
            throw error;
        }
    }

    async #run(
        plaintext            ,
        rootOid        ,
        startFrame        ,
        initialFrame        ,
        ticks        ,
        inputPlan                                   ,
        initial                               ,
        catalogEntries                                      ,
    )                   {
        const directory = await mkdtemp(join(tmpdir(), "dat-skill-flow-preview-"));
        const datPath = join(directory, "preview-character.dat");
        const catalogPath = join(directory, "preview-catalog.txt");
        const outputPath = join(directory, "preview.json");
        try {
            await writeFile(datPath, Buffer.from(plaintext), { flag: "wx" });
            if (catalogEntries.length > 0) {
                const lines = ["<object>\n"];
                for (const entry of catalogEntries) {
                    const fileName = `preview-object-${entry.oid}.dat`;
                    await writeFile(join(directory, fileName), Buffer.from(entry.plaintext), { flag: "wx" });
                    lines.push(`id: ${entry.oid} type: ${entry.type} format: plaintext file: ${fileName}\n`);
                }
                lines.push("<object_end>\n");
                await writeFile(catalogPath, Buffer.from(lines.join(""), "ascii"), { flag: "wx" });
            }
            await new Promise      ((resolveRun, rejectRun) => {
                const arguments_ = [
                    "--preview-dat", datPath,
                    "--root-oid", String(rootOid),
                    "--game-root", this.#gameRoot,
                    "--output", outputPath,
                    "--start-frame", String(initialFrame),
                    "--entry-frame", String(startFrame),
                    "--ticks", String(ticks),
                    "--p1-x", String(initial.p1.x),
                    "--p1-y", String(initial.p1.y),
                    "--p1-z", String(initial.p1.z),
                    "--p2-x", String(initial.p2.x),
                    "--p2-y", String(initial.p2.y),
                    "--p2-z", String(initial.p2.z),
                ];
                if (catalogEntries.length > 0) {
                    arguments_.push("--preview-catalog", catalogPath, "--preview-catalog-root", directory);
                }
                if (inputPlan.length > 0) {
                    arguments_.push("--input-plan", inputPlan.map((step) => `${step.tick}:${step.keys.join("+")}`).join(","));
                }
                this.#execFile(this.#executable, arguments_, {
                    cwd: this.#workingDirectory,
                    windowsHide: true,
                    timeout: 60_000,
                    maxBuffer: 1024 * 1024,
                }, (error) => error === null ? resolveRun() : rejectRun(error));
            });
            const output = await openFile(outputPath, "r");
            try {
                const statistics = await output.stat();
                if (!statistics.isFile() || statistics.size > MAX_PREVIEW_OUTPUT_BYTES) {
                    throw new Error("Native preview output exceeds its limit.");
                }
                const bytes = Buffer.allocUnsafe(MAX_PREVIEW_OUTPUT_BYTES + 1);
                let offset = 0;
                while (offset < bytes.length) {
                    const read = await output.read(bytes, offset, bytes.length - offset, offset);
                    if (read.bytesRead === 0) break;
                    offset += read.bytesRead;
                }
                if (offset > MAX_PREVIEW_OUTPUT_BYTES) throw new Error("Native preview output exceeds its limit.");
                return JSON.parse(bytes.subarray(0, offset).toString("utf8"))           ;
            } finally {
                await output.close();
            }
        } finally {
            await rm(directory, { recursive: true, force: true });
        }
    }
}

export class ProjectDatService {
             #primary                   ;
             #assets                    ;
             #patches                    ;
             #primarySessions                   ;
             #assetSessions                    ;
             #patchSessions                    ;
             #safeSave                 ;
             #previewRunner                        ;
             #idFactory              ;
             #assetDirectories                   ;
             #dataDocumentId        ;
             #primaryRootId        ;
             #assetRootId         ;
             #patchRootId         ;
             #patchIndex                    ;
             #sessions = new Map                        ();
             #assetBindings = new Map                      ();
             #queues = new Map                       ();
             #preparedSessions = new Map                            ();
    #catalogRevision = 1;
    #catalogObjects                  = [];
    #nextIdSequence = 0;
    #catalogRefreshAfter = 0;

            constructor(
        options                          ,
        dataDocumentId        ,
        primaryRootId        ,
        assetRootId                    ,
        patchRootId                    ,
    ) {
        this.#primary = options.primaryRegistry;
        this.#assets = options.assetRegistry;
        this.#patches = options.patchRegistry;
        this.#primarySessions = new DatSessionService(this.#primary, options.sessionOptions);
        this.#assetSessions = this.#assets === undefined ? undefined : new DatSessionService(this.#assets, options.sessionOptions);
        this.#patchSessions = this.#patches === undefined ? undefined : new DatSessionService(this.#patches, {
            ...options.sessionOptions,
            numericReadMode: "native-compatible",
        });
        this.#safeSave = options.safeSave === undefined ? new SafeSaveService(this.#primary) : options.safeSave;
        this.#previewRunner = options.previewRunner === undefined ? new CppNativeDatPreviewRunner() : options.previewRunner;
        this.#idFactory = options.idFactory === undefined ? (() => randomBytes(32).toString("base64url")) : options.idFactory;
        const assetBmpDirectories = options.assetBmpDirectories === undefined ? ["sprite/sys"] : options.assetBmpDirectories;
        this.#assetDirectories = assetBmpDirectories
            .map((value) => this.#primary.normalizeLogicalPath(value));
        this.#dataDocumentId = dataDocumentId;
        this.#primaryRootId = primaryRootId;
        this.#assetRootId = assetRootId;
        this.#patchRootId = patchRootId;
        this.#patchIndex = options.patchIndex;
    }

    static async initialize(options                          )                             {
        const primaryRoot = options.primaryRegistry.getStartupRootGrant();
        const primaryRootId = primaryRoot === undefined ? undefined : primaryRoot.rootId;
        if (primaryRootId === undefined) throw new ProjectDatError("project-disabled", "The project workspace is not configured.");
        const dataPath = options.primaryRegistry.normalizeLogicalPath(
            options.dataTxtLogicalPath === undefined ? "Assets/NTSD/Config/data.txt" : options.dataTxtLogicalPath,
        );
        const dataDocument = await options.primaryRegistry.openDocument(primaryRootId, dataPath);
        let assetRootId                    ;
        if (options.assetRegistry !== undefined) {
            const assetRoot = options.assetRegistry.getStartupRootGrant();
            assetRootId = assetRoot === undefined ? undefined : assetRoot.rootId;
        }
        if (options.assetRegistry !== undefined && assetRootId === undefined) {
            throw new ProjectDatError("project-disabled", "The asset workspace is not configured.");
        }
        if ((options.patchRegistry === undefined) !== (options.patchIndex === undefined)) {
            throw new ProjectDatError("project-disabled", "The patch workspace and patch index must be configured together.");
        }
        const patchRoot = options.patchRegistry?.getStartupRootGrant();
        const patchRootId = patchRoot?.rootId;
        if (options.patchRegistry !== undefined && patchRootId === undefined) {
            throw new ProjectDatError("project-disabled", "The patch workspace is not configured.");
        }
        const service = new ProjectDatService(options, dataDocument.documentId, primaryRootId, assetRootId, patchRootId);
        const read = await options.primaryRegistry.readDocument(dataDocument.documentId);
        await service.#replaceCatalog(read.bytes);
        service.#catalogRefreshAfter = Date.now() + CATALOG_REFRESH_INTERVAL_MS;
        return service;
    }

    async prepareDefaultSession(
        onProgress                                             ,
    )           
                          
                          
                       
                          
                         
                              
                           
                           
                                  
                              
           
       {
        const started = Date.now();
        const object = this.#catalogObjects.find((candidate) => (
            candidate.sourceKind === "base" && candidate.oid === DEFAULT_CHARACTER_OID
        ));
        if (object === undefined) {
            throw new ProjectDatError("object-unavailable", "Naruto OID 2 is unavailable for startup preparation.");
        }
        const view = await this.open(object.objectKey, { deferPreview: true });
        this.#preparedSessions.set(object.objectKey, view);
        const binding = this.#requireSession(view.sessionId);
        const emission = await binding.service.emit(view.sessionId, view.revision)
            .catch((error) => { throw mapSessionError(error); });
        const scenarios = deriveSkillEntries(view.frames, view.oid)
            .filter((entry) => entry.category === "input" || entry.category === "engine" || entry.nativeInputPlan !== undefined)
            .map((entry) => buildSkillPreviewScenario(view.frames, entry));
        const unique = [...new Map(scenarios.map((scenario) => [previewOptionsKey(scenario), scenario])).values()];
        const warmup = (async () => {
            const warmupStarted = Date.now();
            let completed = 0;
            let failed = 0;
            let nextIndex = 0;
            const workers = Array.from({ length: Math.min(PREVIEW_WARMUP_CONCURRENCY, unique.length) }, async () => {
                while (nextIndex < unique.length) {
                    const index = nextIndex;
                    nextIndex += 1;
                    try {
                        const raw = await this.#previewRunner.preview(emission.plaintext, {
                            ...unique[index] ,
                            rootOid: binding.oid,
                        });
                        const sanitized = sanitizePreview(raw);
                        for (const resource of sanitized.renderResources) {
                            for (const range of resource.ranges) {
                                await this.#resolveSpriteAsset(binding, range.file);
                            }
                        }
                        await this.#loadStageAssets(binding, sanitized.preview.metadata.stage);
                    } catch {
                        failed += 1;
                    } finally {
                        completed += 1;
                        onProgress?.(completed, unique.length);
                    }
                }
            });
            await Promise.all(workers);
            const assetBindings = [...binding.assetIdsByPath.values()]
                .map((assetId) => this.#assetBindings.get(assetId))
                .filter((asset)                        => asset !== undefined);
            let assetFailures = 0;
            nextIndex = 0;
            const assetWorkers = Array.from({ length: Math.min(8, assetBindings.length) }, async () => {
                while (nextIndex < assetBindings.length) {
                    const index = nextIndex;
                    nextIndex += 1;
                    try {
                        await this.#loadAssetBytes(assetBindings[index] );
                    } catch {
                        assetFailures += 1;
                    }
                }
            });
            await Promise.all(assetWorkers);
            return {
                scenarios: unique.length,
                failed,
                assets: assetBindings.length,
                assetFailures,
                elapsedMs: Date.now() - warmupStarted,
            };
        })();
        return {
            objectKey: object.objectKey,
            scenarios: unique.length,
            assets: binding.assetIdsByPath.size,
            elapsedMs: Date.now() - started,
            warmup,
        };
    }

    async catalog()                              {
        await this.#refreshCatalog(true);
        const objects = this.#catalogObjects
            .filter((object) => object.type === 0)
            .map((object) => ({
                objectKey: object.objectKey,
                effectiveId: `${object.packageId}:${object.oid}`,
                packageId: object.packageId,
                packageLabel: object.packageLabel,
                sourceKind: object.sourceKind,
                manifestStatus: object.manifestStatus,
                diagnosticCount: object.diagnosticCount,
                oid: object.oid,
                sourceOid: object.oid,
                type: object.type,
                displayName: object.displayName,
            }));
        return {
            catalogRevision: this.#catalogRevision,
            objects,
        };
    }

    async open(
        objectKey        ,
        options                                      = {},
    )                              {
        await this.#refreshCatalog();
        const objectKeyValidated = requireOpaqueId(objectKey, "objectKey");
        const object = this.#catalogObjects.find((candidate) => candidate.objectKey === objectKeyValidated);
        if (object === undefined) {
            throw new ProjectDatError("unknown-object", "The project object is unknown.");
        }
        if (object.type !== 0) {
            throw new ProjectDatError("object-unavailable", "Only data.txt type-0 character DATs can be opened in the character editor.");
        }
        const prepared = this.#preparedSessions.get(objectKeyValidated);
        if (prepared !== undefined) {
            this.#preparedSessions.delete(objectKeyValidated);
            const binding = this.#sessions.get(prepared.sessionId);
            if (binding !== undefined) {
                try {
                    await binding.service.emit(prepared.sessionId, prepared.revision);
                    return prepared;
                } catch (error) {
                    await binding.service.close(prepared.sessionId).catch(() => undefined);
                    this.#releaseBinding(prepared.sessionId, binding);
                    const mapped = mapSessionError(error);
                    if (mapped.code !== "unknown-session") throw mapped;
                }
            }
        }
        const opened = await this.#openCandidate(object.candidates);
        if (opened === undefined) throw new ProjectDatError("object-unavailable", "The selected DAT object is unavailable.");
        let session                ;
        try {
            session = await opened.service.openDocument(opened.documentId, "encrypted");
        } catch (error) {
            opened.registry.closeDocument(opened.documentId);
            throw mapSessionError(error);
        }
        const binding                 = {
            ...opened,
            sessionId: session.sessionId,
            oid: object.oid,
            type: object.type,
            packageId: object.packageId,
            packageLabel: object.packageLabel,
            sourceKind: object.sourceKind,
            manifestStatus: object.manifestStatus,
            packageDiagnosticCount: object.diagnosticCount,
            patchBmpPaths: object.patchBmpPaths,
            writable: opened.registry === this.#primary,
            assetIdsByPath: new Map(),
            previewResourcesByOid: new Map(),
            unavailablePreviewOids: new Set(),
            previewByKey: new Map(),
        };
        this.#sessions.set(session.sessionId, binding);
        try {
            return options.deferPreview === true
                ? await this.#buildDeferredSessionView(session, binding, object)
                : await this.#buildSessionView(session, binding);
        } catch (error) {
            await opened.service.close(session.sessionId);
            this.#releaseBinding(session.sessionId, binding);
            throw error;
        }
    }

    async edit(input         )                              {
        const request = exactRecord(input, ["sessionId", "fieldId", "value", "expectedRevision"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        return await this.#editSession(sessionId, async (binding, beforeCommit) => {
            await binding.service.edit(input, beforeCommit);
        });
    }

    async editBatch(input         )                              {
        const request = exactRecord(input, ["sessionId", "edits", "expectedRevision"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        return await this.#editSession(sessionId, async (binding, beforeCommit) => {
            await binding.service.editBatch(input, beforeCommit);
        });
    }

    async editStructure(input         )                              {
        const request = typeof input === "object" && input !== null && !Array.isArray(input)
            ? input                           
            : {};
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        return await this.#editSession(sessionId, async (binding, beforeCommit) => {
            await binding.service.editStructure(input, beforeCommit);
        });
    }

    async #editSession(
        sessionId        ,
        operation                                                                                  ,
    )                              {
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            if (!binding.writable) {
                throw new ProjectDatError("read-only-session", "Fallback DAT sessions are read-only.");
            }
            let prepared                                ;
            try {
                await operation(binding, async (view, emission) => {
                    prepared = await this.#buildSessionView(view, binding, emission);
                });
            } catch (error) {
                throw mapSessionError(error);
            }
            if (prepared === undefined) {
                throw new ProjectDatError("invalid-request", "The DAT edit did not prepare a session view.");
            }
            return prepared;
        });
    }

    async preview(input         )                                                                               {
        const request = exactRecord(
            input,
            ["sessionId", "expectedRevision", "startFrame", "ticks"],
            ["initialFrame", "inputPlan", "initial"],
        );
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        const expectedRevision = boundedInteger(request.expectedRevision, 0, Number.MAX_SAFE_INTEGER, "expectedRevision");
        const startFrame = boundedInteger(request.startFrame, 0, 599, "startFrame");
        const initialFrame = request.initialFrame === undefined
            ? startFrame
            : boundedInteger(request.initialFrame, 0, 599, "initialFrame");
        const ticks = boundedInteger(request.ticks, 1, 1800, "ticks");
        const inputPlan = previewInputPlan(request.inputPlan, ticks);
        const initial = request.initial === undefined ? undefined : previewInitialPositions(request.initial);
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            const emission = await binding.service.emit(sessionId, expectedRevision).catch((error) => { throw mapSessionError(error); });
            return {
                sessionId,
                revision: emission.revision,
                preview: await this.#runPreview(emission.plaintext, binding, emission.revision, {
                    startFrame, initialFrame, ticks, inputPlan, ...(initial === undefined ? {} : { initial }),
                }),
            };
        });
    }

    async save(input         )                               {
        const request = exactRecord(input, ["sessionId", "expectedRevision"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        const expectedRevision = boundedInteger(request.expectedRevision, 0, Number.MAX_SAFE_INTEGER, "expectedRevision");
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            if (!binding.writable) {
                throw new ProjectDatError("read-only-session", "Fallback DAT sessions are read-only.");
            }
            const emission = await binding.service.emit(sessionId, expectedRevision).catch((error) => { throw mapSessionError(error); });
            try {
                const challenge = await this.#safeSave.issueOverwriteChallenge(
                    binding.documentId, binding.rootId, binding.logicalPath, emission.file,
                );
                const result = await this.#safeSave.overwrite(binding.documentId, challenge.challengeId, emission.file);
                await binding.service.markPersisted(sessionId, expectedRevision);
                return {
                    sessionId,
                    revision: expectedRevision,
                    dirty: false,
                    recovery: {
                        target: safeObservation(result.recovery.target),
                        replacement: safeObservation(result.recovery.replacement),
                        backup: safeObservation(result.recovery.backup),
                    },
                };
            } catch (error) {
                if (error instanceof SafeSaveError) throw error;
                throw new ProjectDatError("save-failed", "The DAT could not be saved safely.", { cause: error });
            }
        });
    }

    async close(input         )                                {
        const request = exactRecord(input, ["sessionId"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            await binding.service.close(sessionId);
            this.#releaseBinding(sessionId, binding);
            return { sessionId, closed: true };
        });
    }

    async asset(assetId        )                                {
        this.#sweepExpired();
        const id = requireOpaqueId(assetId, "assetId");
        const binding = this.#assetBindings.get(id);
        if (binding === undefined || !this.#sessions.has(binding.sessionId)) {
            throw new ProjectDatError("unknown-asset", "The asset capability is unknown.");
        }
        const bytes = await this.#loadAssetBytes(binding);
        const metadata = parseBmpMetadata(bytes);
        if (!metadata.ok) throw new ProjectDatError("invalid-asset", "The asset is not a supported BMP.");
        return { bytes: Buffer.from(bytes) };
    }

    async #refreshCatalog(force = false)                {
        this.#sweepExpired();
        if (!force && Date.now() < this.#catalogRefreshAfter) return;
        let prepared;
        try {
            prepared = await this.#primary.prepareDocumentRefresh(this.#dataDocumentId);
        } catch (error) {
            throw new ProjectDatError("catalog-invalid", "The project catalog could not be refreshed safely.", { cause: error });
        }
        this.#catalogRefreshAfter = Date.now() + CATALOG_REFRESH_INTERVAL_MS;
        if (!prepared.snapshot.externallyModified) return;
        await this.#invalidateAll();
        await this.#replaceCatalog(prepared.snapshot.bytes);
        prepared.commit();
        this.#catalogRevision += 1;
    }

    async #replaceCatalog(bytes            )                {
        const parsed = DataTxtDocument.parse(bytes);
        const objects = parsed.entries.filter((entry) => (
            entry.section === "object"
            && entry.type !== undefined
            && entry.id >= 0
            && entry.id < MAX_CATALOG_OIDS
        ));
        if (objects.length === 0) {
            this.#catalogObjects = [];
            return;
        }

        const candidatesByOid = new Map                        ();
        for (const candidate of objects) {
            const candidates = candidatesByOid.get(candidate.id);
            if (candidates === undefined) candidatesByOid.set(candidate.id, [candidate]);
            else candidates.push(candidate);
        }

        const prepared = [...candidatesByOid.values()].map((candidates)                => {
            const first = candidates[0] ;
            return {
                objectKey: this.#newId(),
                oid: first.id,
                type: first.type ,
                displayName: datCatalogDisplayName(first.file),
                packageId: "ntsd-2.4.1",
                packageLabel: "NTSD 2.4.1",
                sourceKind: "base",
                manifestStatus: "base",
                diagnosticCount: 0,
                candidates: candidates.map((candidate) => ({ sourceKind: "base", logicalPath: candidate.file })),
                patchBmpPaths: new Map(),
            };
        });
        if (this.#patchIndex !== undefined) {
            for (const patchPackage of this.#patchIndex.packages) {
                const bmpPaths = new Map                  ();
                for (const logicalPath of patchPackage.bmpFiles) {
                    const key = basename(logicalPath.replaceAll("\\", "/")).toLowerCase();
                    const existing = bmpPaths.get(key);
                    if (existing === undefined) bmpPaths.set(key, [logicalPath]);
                    else existing.push(logicalPath);
                }
                const recordsByOid = new Map                                                      ();
                const packageRecords = [...patchPackage.records].sort((left, right) => (
                    Number(right.manifestSource === "supplemental") - Number(left.manifestSource === "supplemental")
                    || Number(right.type === 0) - Number(left.type === 0)
                ));
                for (const record of packageRecords) {
                    const existing = recordsByOid.get(record.oid);
                    if (existing === undefined) recordsByOid.set(record.oid, [record]);
                    else existing.push(record);
                }
                for (const records of recordsByOid.values()) {
                    const first = records[0] ;
                    prepared.push({
                        objectKey: this.#newId(),
                        oid: first.oid,
                        type: first.type,
                        displayName: datCatalogDisplayName(first.file),
                        packageId: patchPackage.packageId,
                        packageLabel: patchPackage.label,
                        sourceKind: "patch",
                        manifestStatus: patchPackage.status,
                        diagnosticCount: patchPackage.diagnostics.length,
                        candidates: records.map((record) => ({ sourceKind: "patch", logicalPath: record.logicalPath })),
                        patchBmpPaths: bmpPaths,
                    });
                }
            }
        }
        this.#catalogObjects = prepared;
    }

    async #openCandidate(candidates                             )                                       {
        for (const candidate of candidates) {
            if (diagnoseResourcePath(candidate.logicalPath) !== undefined) continue;
            if (candidate.sourceKind === "patch") {
                if (this.#patches === undefined || this.#patchSessions === undefined || this.#patchRootId === undefined) continue;
                const patch = await this.#tryOpen(this.#patches, this.#patchSessions, this.#patchRootId, candidate.logicalPath);
                if (patch !== undefined) return patch;
                continue;
            }
            const primary = await this.#tryOpen(this.#primary, this.#primarySessions, this.#primaryRootId, candidate.logicalPath);
            if (primary !== undefined) return primary;
            if (this.#assets !== undefined && this.#assetSessions !== undefined && this.#assetRootId !== undefined) {
                const fallback = await this.#tryOpen(this.#assets, this.#assetSessions, this.#assetRootId, candidate.logicalPath);
                if (fallback !== undefined) return fallback;
                for (const fallbackPath of this.#fallbackDatPaths(candidate.logicalPath)) {
                    const fallbackByName = await this.#tryOpen(this.#assets, this.#assetSessions, this.#assetRootId, fallbackPath);
                    if (fallbackByName !== undefined) return fallbackByName;
                }
            }
        }
        return undefined;
    }

    #fallbackDatPaths(rawPath        )                    {
        const name = basename(rawPath.replaceAll("\\", "/"));
        return /\.dat$/i.test(name) ? [`chars/${name}`] : [];
    }

    async #buildSessionView(
        session                ,
        binding                ,
        preparedEmission                     ,
    )                              {
        const emission = preparedEmission ?? await binding.service.emit(session.sessionId, session.revision)
            .catch((error) => { throw mapSessionError(error); });
        const preview = await this.#runPreview(emission.plaintext, binding, session.revision);
        const primaryResources = preview.resources.find((resource) => resource.oid === binding.oid);
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.dirty,
            writable: binding.writable,
            effectiveId: `${binding.packageId}:${binding.oid}`,
            packageId: binding.packageId,
            packageLabel: binding.packageLabel,
            sourceKind: binding.sourceKind,
            manifestStatus: binding.manifestStatus,
            oid: binding.oid,
            sourceOid: binding.oid,
            type: binding.type,
            name: session.projection.top.name,
            spriteRanges: primaryResources?.spriteRanges ?? [],
            previewObjects: preview.resources,
            frames: session.projection.frames.map(copySafeFrame),
            fields: session.fields.filter((field) => ![
                "head", "small", "file", "sound", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
            ].includes(field.key)),
            structureCapabilities: session.structureCapabilities,
            preview,
            diagnostics: this.#sessionDiagnostics(session, binding),
        };
    }

    async #buildDeferredSessionView(
        session                ,
        binding                ,
        object               ,
    )                              {
        const primaryResource = await this.#projectPreviewObject(binding, object, session.projection);
        const preview = deferredNativePreview(primaryResource);
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.dirty,
            writable: binding.writable,
            effectiveId: `${binding.packageId}:${binding.oid}`,
            packageId: binding.packageId,
            packageLabel: binding.packageLabel,
            sourceKind: binding.sourceKind,
            manifestStatus: binding.manifestStatus,
            oid: binding.oid,
            sourceOid: binding.oid,
            type: binding.type,
            name: session.projection.top.name,
            spriteRanges: primaryResource.spriteRanges,
            previewObjects: preview.resources,
            frames: session.projection.frames.map(copySafeFrame),
            fields: session.fields.filter((field) => ![
                "head", "small", "file", "sound", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
            ].includes(field.key)),
            structureCapabilities: session.structureCapabilities,
            preview,
            diagnostics: [
                ...this.#sessionDiagnostics(session, binding),
                {
                    code: "preview-warming",
                    severity: "warning"         ,
                    message: "Native action previews are warming in the background; selecting an entry reuses the prepared result.",
                },
            ],
        };
    }

    #sessionDiagnostics(session                , binding                )                                    {
        const diagnostics                                              = session.diagnostics.map((diagnostic) => ({
            code: diagnostic.code,
            severity: diagnostic.severity,
            message: diagnostic.message.replace(/(?:[A-Za-z]:[\\/]|\.\.[\\/])\S*/g, "[redacted]"),
        }));
        if (binding.sourceKind === "patch") {
            diagnostics.push({
                code: "patch-native-base-dependencies",
                severity: "warning",
                message: "补丁角色 DAT 已按包作用域加载；Native 运行时的未覆盖依赖仍从 NTSD 2.4.1 解析，包内精灵资源会优先显示。",
            });
            if (binding.packageDiagnosticCount > 0) {
                diagnostics.push({
                    code: "patch-package-diagnostics",
                    severity: "warning",
                    message: `该补丁包还有 ${binding.packageDiagnosticCount} 项清单诊断，请在正式编辑前核对。`,
                });
            }
        }
        return diagnostics;
    }

    async #resolveSpriteAsset(binding                , rawPath        )                                           {
        if (diagnoseResourcePath(rawPath) !== undefined) return undefined;
        let pathKey        ;
        try {
            pathKey = this.#primary.normalizeLogicalPath(rawPath).toLowerCase();
        } catch (error) {
            if (error instanceof WorkspaceSecurityError && error.code === "invalid-logical-path") return undefined;
            throw error;
        }
        const existing = binding.assetIdsByPath.get(pathKey);
        if (existing !== undefined) return { assetId: existing };
        const exactCandidates                   = [];
        const name = basename(rawPath.replaceAll("\\", "/"));
        if (binding.sourceKind === "patch" && this.#patches !== undefined && this.#patchRootId !== undefined) {
            const rawSuffix = rawPath.replaceAll("\\", "/").toLowerCase();
            const matches = binding.patchBmpPaths.get(name.toLowerCase()) ?? [];
            const orderedMatches = [
                ...matches.filter((logicalPath) => logicalPath.toLowerCase().endsWith(rawSuffix)),
                ...matches.filter((logicalPath) => !logicalPath.toLowerCase().endsWith(rawSuffix)),
            ];
            for (const logicalPath of orderedMatches) {
                exactCandidates.push({ registry: this.#patches, rootId: this.#patchRootId, logicalPath });
            }
        }
        if (this.#assets !== undefined && this.#assetRootId !== undefined) {
            exactCandidates.push({ registry: this.#assets, rootId: this.#assetRootId, logicalPath: rawPath });
        }
        exactCandidates.push({ registry: this.#primary, rootId: this.#primaryRootId, logicalPath: rawPath });
        const fallbackCandidates                   = [];
        if (/^[^/\\\0]+\.bmp$/i.test(name)
            && this.#assets !== undefined
            && this.#assetRootId !== undefined) {
            for (const directory of this.#assetDirectories) {
                fallbackCandidates.push({
                    registry: this.#assets,
                    rootId: this.#assetRootId,
                    logicalPath: `${directory}/${name}`,
                });
            }
        }
        return this.#bindAsset(binding, pathKey, exactCandidates, fallbackCandidates);
    }

    #bindAsset(
        binding                ,
        pathKey        ,
        exactCandidates                           ,
        fallbackCandidates                           ,
    )                      {
        const assetId = this.#newId();
        binding.assetIdsByPath.set(pathKey, assetId);
        this.#assetBindings.set(assetId, {
            sessionId: binding.sessionId,
            exactCandidates,
            fallbackCandidates,
        });
        return { assetId };
    }

    async #loadAssetBytes(binding              )                  {
        if (binding.bytes !== undefined) return binding.bytes;
        if (binding.pending !== undefined) return await binding.pending;
        const pending = this.#resolveAssetBytes(binding);
        binding.pending = pending;
        try {
            const bytes = await pending;
            binding.bytes = bytes;
            return bytes;
        } finally {
            if (binding.pending === pending) binding.pending = undefined;
        }
    }

    async #resolveAssetBytes(binding              )                  {
        for (const candidate of binding.exactCandidates) {
            const bytes = await this.#tryReadAsset(candidate);
            if (bytes !== undefined) return bytes;
        }
        const matches           = [];
        for (const candidate of binding.fallbackCandidates) {
            const bytes = await this.#tryReadAsset(candidate);
            if (bytes !== undefined) matches.push(bytes);
        }
        if (matches.length === 1) return matches[0] ;
        throw new ProjectDatError("unknown-asset", "The asset is unavailable or ambiguous.");
    }

    async #tryReadAsset(candidate                )                              {
        try {
            return await candidate.registry.readLogicalFile(candidate.rootId, candidate.logicalPath);
        } catch (error) {
            if (error instanceof WorkspaceSecurityError
                && ["not-a-file", "invalid-logical-path", "root-escape"].includes(error.code)) return undefined;
            throw error;
        }
    }

    async #runPreview(
        plaintext            ,
        binding                ,
        revision        ,
        options                       ,
    )                             {
        const effectiveOptions                       = { ...options, rootOid: binding.oid };
        const prefix = `${revision}:`;
        for (const key of binding.previewByKey.keys()) {
            if (!key.startsWith(prefix)) binding.previewByKey.delete(key);
        }
        const cacheKey = `${prefix}${previewOptionsKey(effectiveOptions)}`;
        const cached = binding.previewByKey.get(cacheKey);
        if (cached !== undefined) {
            binding.previewByKey.delete(cacheKey);
            binding.previewByKey.set(cacheKey, cached);
            return await cached;
        }
        while (binding.previewByKey.size >= MAX_SESSION_PREVIEW_CACHE_ENTRIES) {
            const oldest = binding.previewByKey.keys().next().value                      ;
            if (oldest === undefined) break;
            binding.previewByKey.delete(oldest);
        }
        const pending = this.#createPreview(plaintext, binding, effectiveOptions);
        binding.previewByKey.set(cacheKey, pending);
        try {
            return await pending;
        } catch (error) {
            if (binding.previewByKey.get(cacheKey) === pending) binding.previewByKey.delete(cacheKey);
            throw error;
        }
    }

    async #createPreview(
        plaintext            ,
        binding                ,
        options                       ,
    )                             {
        try {
            const primary = LosslessDatDocument.fromPlaintext(plaintext).projection;
            const catalogEntries = await this.#nativePreviewCatalog(binding);
            const nativeOptions = catalogEntries.length === 0
                ? options
                : { ...options, catalogEntries };
            const sanitized = sanitizePreview(await this.#previewRunner.preview(Buffer.from(plaintext), nativeOptions));
            const rawPreview = sanitized.preview;
            const objectOids = new Set(rawPreview.ticks.flatMap((tick) => tick.entities.map((entity) => entity.oid)));
            objectOids.add(binding.oid);
            const resources = await this.#loadPreviewResources(
                binding,
                objectOids,
                primary,
                sanitized.renderResources,
            );
            const stage = await this.#loadStageAssets(binding, rawPreview.metadata.stage);
            const objectTypes = new Map([
                ...this.#objectsForBinding(binding).map((object)                            => [object.oid, object.type]),
                ...sanitized.renderResources.map((resource)                            => [resource.oid, resource.type]),
            ]);
            const actionFrameIds = completeActionFrameIds(
                primary.frames,
                binding.oid,
                rawPreview.metadata.startFrame,
            );
            return enrichNativePreview({
                ...rawPreview,
                metadata: {
                    ...rawPreview.metadata,
                    stage,
                },
                resources,
            }, resources, objectTypes, binding.oid, actionFrameIds);
        } catch (error) {
            if (error instanceof ProjectDatError) throw error;
            throw new ProjectDatError("preview-failed", "The native character preview failed.", { cause: error });
        }
    }

    async #nativePreviewCatalog(binding                )                                                {
        if (binding.sourceKind !== "patch") return [];
        if (binding.nativePreviewCatalog !== undefined) return await binding.nativePreviewCatalog;
        const packageObjects = this.#catalogObjects
            .filter((object) => object.packageId === binding.packageId)
            .sort((left, right) => left.oid - right.oid);
        const pending = (async ()                                                => {
            const loaded = await Promise.all(packageObjects.map(async (object) => {
                const opened = await this.#openCandidate(object.candidates);
                if (opened === undefined) return undefined;
                try {
                    const document = await opened.registry.readDocument(opened.documentId);
                    return {
                        oid: object.oid,
                        type: object.type,
                        plaintext: previewDatPlaintext(document.bytes),
                    }                                    ;
                } catch {
                    return undefined;
                } finally {
                    opened.registry.closeDocument(opened.documentId);
                }
            }));
            return loaded.filter((entry)                                     => entry !== undefined);
        })();
        binding.nativePreviewCatalog = pending;
        try {
            return await pending;
        } catch (error) {
            if (binding.nativePreviewCatalog === pending) binding.nativePreviewCatalog = undefined;
            throw error;
        }
    }

    async #loadStageAssets(
        binding                ,
        stage                                        ,
    )                                                  {
        const background = stage.background;
        if (background === undefined) return stage;
        const layers = [];
        for (const layer of background.layers) {
            const asset = await this.#resolveSpriteAsset(binding, layer.path);
            layers.push(asset === undefined ? layer : { ...layer, assetId: asset.assetId });
        }
        const shadow = background.shadow;
        const shadowAsset = shadow === undefined ? undefined : await this.#resolveSpriteAsset(binding, shadow.path);
        return {
            ...stage,
            background: {
                layers,
                ...(shadow === undefined
                    ? {}
                    : { shadow: shadowAsset === undefined ? shadow : { ...shadow, assetId: shadowAsset.assetId } }),
            },
        };
    }

    async #loadPreviewResources(
        binding                ,
        oids                     ,
        primaryProjection               ,
        nativeResources                                 ,
    )                                               {
        const resources                             = [];
        const nativeByOid = new Map(nativeResources.map((resource) => [resource.oid, resource]));
        const requested = [binding.oid, ...[...oids].filter((oid) => oid !== binding.oid)];
        for (const oid of requested) {
            const native = nativeByOid.get(oid);
            if (native !== undefined) {
                resources.push(await this.#projectNativePreviewObject(binding, native));
                continue;
            }
            const object = this.#resolveCatalogObject(binding, oid);
            if (object === undefined) continue;
            if (oid === binding.oid) {
                resources.push(await this.#projectPreviewObject(binding, object, primaryProjection));
                continue;
            }
            const resource = await this.#openPreviewObject(binding, object);
            if (resource !== undefined) resources.push(resource);
        }
        return resources;
    }

    #objectsForBinding(binding                )                           {
        const packageObjects = this.#catalogObjects.filter((object) => object.packageId === binding.packageId);
        if (binding.sourceKind === "base") return packageObjects;
        const packageOids = new Set(packageObjects.map((object) => object.oid));
        return [
            ...this.#catalogObjects.filter((object) => object.sourceKind === "base" && !packageOids.has(object.oid)),
            ...packageObjects,
        ];
    }

    #resolveCatalogObject(binding                , oid        )                            {
        return this.#catalogObjects.find((object) => object.packageId === binding.packageId && object.oid === oid)
            ?? this.#catalogObjects.find((object) => object.sourceKind === "base" && object.oid === oid);
    }

    async #projectNativePreviewObject(
        binding                ,
        resource                      ,
    )                                    {
        if (resource.oid !== binding.oid) {
            const cached = binding.previewResourcesByOid.get(resource.oid);
            if (cached !== undefined) return cached;
        }
        const spriteRanges = [];
        for (const range of resource.ranges) {
            const asset = await this.#resolveSpriteAsset(binding, range.file);
            if (asset === undefined) continue;
            spriteRanges.push({
                frameLo: range.frameLo,
                frameHi: range.frameHi,
                assetId: asset.assetId,
                w: range.w,
                h: range.h,
                row: range.row,
                col: range.col,
            });
        }
        const projected                           = {
            oid: resource.oid,
            type: resource.type,
            name: resource.name,
            spriteRanges,
            frames: resource.frames.map(nativeRenderFrameView),
        };
        if (resource.oid !== binding.oid) binding.previewResourcesByOid.set(resource.oid, projected);
        return projected;
    }

    async #openPreviewObject(
        binding                ,
        object               ,
    )                                                {
        const cached = binding.previewResourcesByOid.get(object.oid);
        if (cached !== undefined) return cached;
        if (binding.unavailablePreviewOids.has(object.oid)) return undefined;
        const opened = await this.#openCandidate(object.candidates);
        if (opened === undefined) {
            binding.unavailablePreviewOids.add(object.oid);
            return undefined;
        }
        try {
            const document = await opened.registry.readDocument(opened.documentId);
            const resource = await this.#projectPreviewObject(
                binding,
                object,
                previewDatProjection(document.bytes),
            );
            binding.previewResourcesByOid.set(object.oid, resource);
            return resource;
        } catch {
            binding.unavailablePreviewOids.add(object.oid);
            return undefined;
        } finally {
            opened.registry.closeDocument(opened.documentId);
        }
    }

    async #projectPreviewObject(
        binding                ,
        object               ,
        projection                              ,
    )                                    {
        const spriteRanges = [];
        for (const range of projection.spriteRanges) {
            const asset = await this.#resolveSpriteAsset(binding, range.file);
            if (asset === undefined) continue;
            spriteRanges.push({
                frameLo: range.frameLo, frameHi: range.frameHi, assetId: asset.assetId,
                w: range.w, h: range.h, row: range.row, col: range.col,
            });
        }
        return {
            oid: object.oid,
            type: object.type,
            name: projection.top.name,
            spriteRanges,
            frames: projection.frames.map(copySafeFrame),
        };
    }

    async #tryOpen(
        registry                   ,
        service                   ,
        rootId        ,
        logicalPath        ,
    )                                       {
        const opened = await this.#tryOpenDocument(registry, rootId, logicalPath);
        return opened === undefined ? undefined : { registry, service, rootId, logicalPath: opened.logicalPath, documentId: opened.documentId };
    }

    async #tryOpenDocument(registry                   , rootId        , logicalPath        ) {
        try {
            const normalized = registry.normalizeLogicalPath(logicalPath);
            return await registry.openDocument(rootId, normalized);
        } catch (error) {
            if (error instanceof WorkspaceSecurityError
                && ["not-a-file", "invalid-logical-path", "root-escape"].includes(error.code)) return undefined;
            throw error;
        }
    }

    #requireSession(sessionId        )                 {
        const binding = this.#sessions.get(sessionId);
        if (binding === undefined) throw new ProjectDatError("unknown-session", "The project session is unknown.");
        return binding;
    }

    async #invalidateAll()                {
        const sessions = [...this.#sessions.entries()];
        const results = await Promise.allSettled(sessions.map(async ([sessionId, binding]) => {
            try {
                await binding.service.close(sessionId);
            } finally {
                this.#releaseBinding(sessionId, binding);
            }
        }));
        const rejected = results.find((result) => result.status === "rejected");
        if (rejected?.status === "rejected") throw rejected.reason;
    }

    #sweepExpired()       {
        const expiredSessionIds = [
            ...this.#primarySessions.sweepExpiredSessionIds(),
            ...(this.#assetSessions?.sweepExpiredSessionIds() ?? []),
            ...(this.#patchSessions?.sweepExpiredSessionIds() ?? []),
        ];
        for (const sessionId of expiredSessionIds) {
            const binding = this.#sessions.get(sessionId);
            if (binding !== undefined) this.#releaseBinding(sessionId, binding);
        }
    }

    #releaseBinding(sessionId        , binding                )       {
        this.#sessions.delete(sessionId);
        for (const assetId of binding.assetIdsByPath.values()) {
            this.#assetBindings.delete(assetId);
        }
        for (const [objectKey, prepared] of this.#preparedSessions) {
            if (prepared.sessionId === sessionId) this.#preparedSessions.delete(objectKey);
        }
        binding.assetIdsByPath.clear();
        binding.previewByKey.clear();
        binding.registry.closeDocument(binding.documentId);
    }

    async #enqueue   (sessionId        , operation                  )             {
        const previous = this.#queues.get(sessionId);
        const previousOperation = previous === undefined ? Promise.resolve() : previous;
        let release             ;
        const current = new Promise      ((resolveRelease) => { release = resolveRelease; });
        this.#queues.set(sessionId, current);
        await previousOperation;
        try {
            return await operation();
        } finally {
            release();
            if (this.#queues.get(sessionId) === current) this.#queues.delete(sessionId);
        }
    }

    #newId()         {
        for (let attempt = 0; attempt < 16; attempt += 1) {
            const randomPart = this.#idFactory();
            if (typeof randomPart !== "string" || !/^[A-Za-z0-9_-]{32,128}$/.test(randomPart)) continue;
            if (this.#nextIdSequence >= Number.MAX_SAFE_INTEGER) {
                throw new ProjectDatError("invalid-request", "The capability ID sequence is exhausted.");
            }
            this.#nextIdSequence += 1;
            const suffix = this.#nextIdSequence.toString(36);
            return `${randomPart.slice(0, 128 - suffix.length - 1)}-${suffix}`;
        }
        throw new ProjectDatError("invalid-request", "The capability ID source failed.");
    }
}

function previewOptionsKey(options                       = {})         {
    const rootOid = options.rootOid ?? DEFAULT_CHARACTER_OID;
    const startFrame = options.startFrame ?? DEFAULT_START_FRAME;
    const initialFrame = options.initialFrame ?? startFrame;
    const ticks = options.ticks ?? DEFAULT_TICKS;
    const inputPlan = options.inputPlan ?? [];
    const initial = options.initial ?? {
        p1: { x: 320, y: 0, z: 500 },
        p2: { x: 360, y: 0, z: 501 },
    };
    return JSON.stringify({
        rootOid,
        startFrame,
        initialFrame,
        ticks,
        initial,
        inputPlan: inputPlan.map((step) => ({ tick: step.tick, keys: [...step.keys] })),
    });
}

function datCatalogDisplayName(rawPath        )         {
    const fileName = basename(rawPath.replaceAll("\\", "/"));
    const displayName = fileName.replace(/\.dat$/i, "").trim();
    return displayName === "" ? "未命名角色" : displayName;
}

function nativePreviewCacheKey(plaintext            , options                      )         {
    const hash = createHash("sha256").update(plaintext);
    for (const entry of options.catalogEntries ?? []) {
        hash.update(`\0${entry.oid}:${entry.type}:${entry.plaintext.byteLength}\0`, "ascii");
        hash.update(entry.plaintext);
    }
    const digest = hash.digest("hex");
    return `${digest}:${previewOptionsKey(options)}`;
}

function normalizedNativePreviewCatalog(
    value                                                  ,
)                                       {
    if (value === undefined || value.length === 0) return [];
    if (value.length > MAX_CATALOG_OIDS) {
        throw new ProjectDatError("preview-failed", "Native preview catalog exceeds its object limit.");
    }
    const seenOids = new Set        ();
    let totalBytes = 0;
    const entries = value.map((entry, index)                            => {
        const oid = boundedInteger(entry.oid, 0, MAX_CATALOG_OIDS - 1, `catalogEntries[${index}].oid`);
        const type = boundedInteger(entry.type, 0, 255, `catalogEntries[${index}].type`);
        if (seenOids.has(oid)) {
            throw new ProjectDatError("preview-failed", "Native preview catalog contains duplicate OIDs.");
        }
        seenOids.add(oid);
        const plaintext = Buffer.from(entry.plaintext);
        if (plaintext.byteLength === 0) {
            throw new ProjectDatError("preview-failed", "Native preview catalog contains an empty DAT.");
        }
        totalBytes += plaintext.byteLength;
        if (totalBytes > MAX_NATIVE_PREVIEW_CATALOG_BYTES) {
            throw new ProjectDatError("preview-failed", "Native preview catalog exceeds its byte limit.");
        }
        return { oid, type, plaintext };
    });
    return entries.sort((left, right) => left.oid - right.oid);
}

function exactRecord(
    value         ,
    keys                   ,
    optionalKeys                    = [],
)                          {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        throw new ProjectDatError("invalid-request", "The request must be an object.");
    }
    const record = value                           ;
    const actual = Object.keys(record);
    if (actual.some((key) => !keys.includes(key) && !optionalKeys.includes(key))
        || keys.some((key) => !Object.hasOwn(record, key))) {
        throw new ProjectDatError("invalid-request", "The request has missing or unknown fields.");
    }
    return record;
}

const PREVIEW_INPUT_KEYS = new Set                       (["A", "D", "W", "S", "J", "K", "L"]);

function previewInitialPositions(value         )                                {
    const initial = exactRecord(value, ["p1", "p2"]);
    const position = (raw         , name        )                                      => {
        const item = exactRecord(raw, ["x", "y", "z"]);
        return {
            x: requestFinite(item.x, `${name}.x`),
            y: requestFinite(item.y, `${name}.y`),
            z: requestFinite(item.z, `${name}.z`),
        };
    };
    return { p1: position(initial.p1, "initial.p1"), p2: position(initial.p2, "initial.p2") };
}

function previewInputPlan(value         , ticks        )                                    {
    if (value === undefined) return [];
    if (!Array.isArray(value) || value.length > 64) {
        throw new ProjectDatError("invalid-request", "inputPlan must contain at most 64 input steps.");
    }
    const seenTicks = new Set        ();
    const result = value.map((raw, index)                         => {
        const step = exactRecord(raw, ["tick", "keys"]);
        const tick = boundedInteger(step.tick, 1, ticks, `inputPlan[${index}].tick`);
        if (seenTicks.has(tick)) throw new ProjectDatError("invalid-request", "inputPlan ticks must be unique.");
        seenTicks.add(tick);
        if (!Array.isArray(step.keys) || step.keys.length < 1 || step.keys.length > PREVIEW_INPUT_KEYS.size) {
            throw new ProjectDatError("invalid-request", `inputPlan[${index}].keys is invalid.`);
        }
        const keys = step.keys.map((key)                        => {
            if (typeof key !== "string" || !PREVIEW_INPUT_KEYS.has(key                         )) {
                throw new ProjectDatError("invalid-request", `inputPlan[${index}] contains an unsupported key.`);
            }
            return key                         ;
        });
        if (new Set(keys).size !== keys.length) {
            throw new ProjectDatError("invalid-request", `inputPlan[${index}] contains duplicate keys.`);
        }
        return { tick, keys };
    });
    return result.sort((left, right) => left.tick - right.tick);
}

function requireOpaqueId(value         , name        )         {
    if (typeof value !== "string" || !/^[A-Za-z0-9_-]{32,128}$/.test(value)) {
        throw new ProjectDatError("invalid-request", `${name} must be an opaque capability ID.`);
    }
    return value;
}

function boundedInteger(value         , minimum        , maximum        , name        )         {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < minimum || value > maximum) {
        throw new ProjectDatError("invalid-request", `${name} is outside its supported integer range.`);
    }
    return value;
}

function requestFinite(value         , name        )         {
    if (typeof value !== "number" || !Number.isFinite(value) || Math.abs(value) > 1_000_000) {
        throw new ProjectDatError("invalid-request", `${name} is outside its supported finite range.`);
    }
    return value === 0 ? 0 : value;
}

function finite(value         , name        )         {
    if (typeof value !== "number" || !Number.isFinite(value)) throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    return value;
}

function previewInteger(value         , minimum        , maximum        , name        )         {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < minimum || value > maximum) {
        throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    }
    return value;
}

function record(value         , name        )                          {
    if (typeof value !== "object" || value === null || Array.isArray(value)) throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    return value                           ;
}

function textValue(value         , name        , maximum = 256)         {
    if (typeof value !== "string" || Buffer.byteLength(value, "utf8") > maximum || value.includes("\0")) {
        throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    }
    return value;
}

function vector(value         , name        )                                      {
    const item = record(value, name);
    return { x: finite(item.x, `${name}.x`), y: finite(item.y, `${name}.y`), z: finite(item.z, `${name}.z`) };
}

function sanitizeEntity(value         )                          {
    const item = record(value, "entity");
    const zInt = previewInteger(item.z_int, -2_147_483_648, 2_147_483_647, "z_int");
    return {
        slot: previewInteger(item.slot, 0, 399, "slot"),
        oid: previewInteger(item.oid, 0, 999, "oid"),
        frame: previewInteger(item.frame, 0, 599, "frame"),
        pic: previewInteger(item.pic, -1, 2_147_483_647, "pic"),
        facing: previewInteger(item.facing, 0, 1, "facing"),
        x: finite(item.x, "x"),
        ...(item.render_pic === undefined
            ? {}
            : { renderPic: previewInteger(item.render_pic, -1, 2_147_483_647, "render_pic") }),
        y: finite(item.y, "y"), z: finite(item.z, "z"),
        xInt: previewInteger(item.x_int, -2_147_483_648, 2_147_483_647, "x_int"),
        yInt: previewInteger(item.y_int, -2_147_483_648, 2_147_483_647, "y_int"),
        zInt,
        displayZ: item.display_z === undefined
            ? zInt
            : previewInteger(item.display_z, -2_147_483_648, 2_147_483_647, "display_z"),
        velocity: vector(item.v, "v"),
        renderOffsetX: previewInteger(item.render_offset_x, -2_147_483_648, 2_147_483_647, "render_offset_x"),
        frameDelay: previewInteger(item.frame_delay, -2_147_483_648, 2_147_483_647, "frame_delay"),
        hitStop: item.hit_stop === undefined
            ? 0
            : previewInteger(item.hit_stop, -2_147_483_648, 2_147_483_647, "hit_stop"),
        team: previewInteger(item.team, -1, 255, "team"),
        target: previewInteger(item.target, -1, 399, "target"),
        holder: previewInteger(item.holder, -1, 399, "holder"),
        link: previewInteger(item.link, -1, 399, "link"), ai: item.ai === true,
        objectType: null, kind: "unknown", lineageId: "unclassified", firstSeenTick: 0, lastSeenTick: 0,
        resourceAvailable: false,
    };
}

function sanitizeTick(value         )                        {
    const item = record(value, "tick");
    const bg = record(item.bg, "background");
    const entities = Array.isArray(item.entities) ? item.entities : [];
    if (entities.length > 400) throw new ProjectDatError("preview-failed", "Native preview entity count exceeds its limit.");
    const sanitizedEntities = entities.map(sanitizeEntity);
    if (new Set(sanitizedEntities.map((entity) => entity.slot)).size !== sanitizedEntities.length) {
        throw new ProjectDatError("preview-failed", "Native preview contains duplicate entity slots.");
    }
    return {
        tick: finite(item.tick, "tick"), cameraX: finite(item.camera_x, "camera_x"),
        cameraVelocity: finite(item.camera_vel, "camera_vel"),
        background: {
            width: finite(bg.width, "bg.width"), zMin: finite(bg.z_min, "bg.z_min"), zMax: finite(bg.z_max, "bg.z_max"),
            boundLeft: finite(bg.bound_left, "bg.bound_left"), boundRight: finite(bg.bound_right, "bg.bound_right"),
        },
        entities: sanitizedEntities,
    };
}

function deferredNativePreview(primaryResource                          )                    {
    return {
        metadata: {
            runtime: "ntsd_cpp",
            tickDriver: "SimulationTickDriver",
            renderer: "none",
            seed: 0,
            startFrame: 0,
            ticksRequested: 0,
            stage: { index: 0, name: "Preview warming", width: 1600, zMin: 0, zMax: 1000 },
            initial: {
                p1: { x: 0, y: 0, z: 0 },
                p2: { x: 0, y: 0, z: 0 },
            },
        },
        ticks: [],
        resources: [primaryResource],
        trace: {
            rootSkillStartedTick: null,
            rootSkillEntryFrame: null,
            rootSkillEndedTick: null,
            progressEndTick: null,
            playbackEndTick: 0,
            status: "timeout",
            pendingProjectiles: [],
            entities: [],
            events: [],
        },
    };
}

function sanitizePreview(value         )                         {
    const root = record(value, "root");
    const metadata = record(root.metadata, "metadata");
    const stage = record(metadata.stage, "stage");
    const startFrame = finite(metadata.start_frame, "start_frame");
    const initialFrame = metadata.initial_frame === undefined
        ? undefined
        : previewInteger(metadata.initial_frame, 0, 599, "initial_frame");
    const ticksRequested = finite(metadata.ticks_requested, "ticks_requested");
    const initial = metadata.initial === undefined
        ? { p1: { x: 0, y: 0, z: 0 }, p2: { x: 0, y: 0, z: 0 } }
        : (() => {
            const raw = record(metadata.initial, "initial");
            return { p1: vector(raw.p1, "initial.p1"), p2: vector(raw.p2, "initial.p2") };
        })();
    const background = metadataBackground(stage);
    const ticks = Array.isArray(root.ticks) ? root.ticks : [];
    if (ticks.length > ticksRequested + 1) throw new ProjectDatError("preview-failed", "Native preview tick count exceeds its limit.");
    if (metadata.runtime !== "ntsd_cpp" || metadata.tick_driver !== "SimulationTickDriver" || metadata.renderer !== "none") {
        throw new ProjectDatError("preview-failed", "Native preview authority metadata is invalid.");
    }
    const preview                    = {
            metadata: {
                runtime: "ntsd_cpp", tickDriver: "SimulationTickDriver", renderer: "none",
                seed: finite(metadata.seed, "seed"), startFrame, ticksRequested,
                ...(initialFrame === undefined ? {} : { initialFrame }),
                stage: {
                    index: finite(stage.index, "stage.index"), name: textValue(stage.name, "stage.name"),
                    width: finite(stage.width, "stage.width"), zMin: finite(stage.z_min, "stage.z_min"), zMax: finite(stage.z_max, "stage.z_max"),
                    ...(background === undefined ? {} : { background }),
                },
                initial,
        },
        resources: [],
        trace: {
            rootSkillStartedTick: null,
            rootSkillEntryFrame: null,
            rootSkillEndedTick: null,
            progressEndTick: null,
            playbackEndTick: Math.max(0, ticks.length - 1),
            status: "timeout",
            pendingProjectiles: [],
            entities: [],
            events: [],
        },
        ticks: ticks.map(sanitizeTick),
    };
    return {
        preview,
        renderResources: sanitizeRenderResources(root.render_resources),
    };
}

function sanitizeRenderResources(value         )                                  {
    if (value === undefined) return [];
    if (!Array.isArray(value) || value.length > MAX_CATALOG_OIDS) {
        throw new ProjectDatError("preview-failed", "Native preview render resource count exceeds its limit.");
    }
    const seenOids = new Set        ();
    return value.map((rawResource, resourceIndex) => {
        const resource = record(rawResource, `render_resources[${resourceIndex}]`);
        const oid = previewInteger(resource.oid, 0, 999, `render_resources[${resourceIndex}].oid`);
        if (seenOids.has(oid)) {
            throw new ProjectDatError("preview-failed", "Native preview contains duplicate render resource OIDs.");
        }
        seenOids.add(oid);
        if (!Array.isArray(resource.ranges) || resource.ranges.length > 64) {
            throw new ProjectDatError("preview-failed", "Native preview render range count exceeds its limit.");
        }
        if (!Array.isArray(resource.frames) || resource.frames.length > 600) {
            throw new ProjectDatError("preview-failed", "Native preview render frame count exceeds its limit.");
        }
        const ranges = resource.ranges.map((rawRange, rangeIndex)                    => {
            const range = record(rawRange, `render_resources[${resourceIndex}].ranges[${rangeIndex}]`);
            const frameLo = previewInteger(range.frame_lo, 0, 2_147_483_647, "render range frame_lo");
            const frameHi = previewInteger(range.frame_hi, frameLo, 2_147_483_647, "render range frame_hi");
            return {
                file: textValue(range.file, "render range file"),
                frameLo,
                frameHi,
                w: previewInteger(range.w, 1, 4096, "render range w"),
                h: previewInteger(range.h, 1, 4096, "render range h"),
                row: previewInteger(range.row, 1, 4096, "render range row"),
                col: previewInteger(range.col, 1, 4096, "render range col"),
            };
        });
        const frames = resource.frames.map((rawFrame, frameIndex)                    => {
            const frame = record(rawFrame, `render_resources[${resourceIndex}].frames[${frameIndex}]`);
            return {
                frameId: previewInteger(frame.frame_id, 0, 599, "render frame frame_id"),
                pic: previewInteger(frame.pic, -1, 2_147_483_647, "render frame pic"),
                state: previewInteger(frame.state, -2_147_483_648, 2_147_483_647, "render frame state"),
                centerx: previewInteger(frame.center_x, -2_147_483_648, 2_147_483_647, "render frame center_x"),
                centery: previewInteger(frame.center_y, -2_147_483_648, 2_147_483_647, "render frame center_y"),
            };
        });
        return {
            oid,
            type: previewInteger(resource.type, 0, 255, `render_resources[${resourceIndex}].type`),
            name: textValue(resource.name, `render_resources[${resourceIndex}].name`),
            ranges,
            frames,
        };
    });
}

function metadataBackground(stage                         )                                                       {
    const raw = stage.background;
    if (raw === undefined) return undefined;
    const value = record(raw, "stage.background");
    const rawLayers = value.layers;
    if (!Array.isArray(rawLayers) || rawLayers.length > 64) {
        throw new ProjectDatError("preview-failed", "Native preview background layer count exceeds its limit.");
    }
    const layers = rawLayers.map((rawLayer) => {
        const layer = record(rawLayer, "stage.background.layer");
        return {
            path: textValue(layer.path, "stage.background.layer.path"),
            transparency: finite(layer.transparency, "stage.background.layer.transparency"),
            parallaxWidth: finite(layer.parallax_width, "stage.background.layer.parallax_width"),
            x: finite(layer.x, "stage.background.layer.x"),
            y: finite(layer.y, "stage.background.layer.y"),
            loop: backgroundLoop(layer.loop, "stage.background.layer.loop"),
            cc: finite(layer.cc, "stage.background.layer.cc"),
            c1: finite(layer.c1, "stage.background.layer.c1"),
            c2: finite(layer.c2, "stage.background.layer.c2"),
            animCounter: finite(layer.anim_counter, "stage.background.layer.anim_counter"),
        };
    });
    const rawShadow = value.shadow;
    if (rawShadow === undefined) return { layers };
    const shadow = record(rawShadow, "stage.background.shadow");
    return {
        layers,
        shadow: {
            path: textValue(shadow.path, "stage.background.shadow.path"),
            width: finite(shadow.width, "stage.background.shadow.width"),
            height: finite(shadow.height, "stage.background.shadow.height"),
        },
    };
}

function backgroundLoop(value         , name        )         {
    const loop = finite(value, name);
    if (!Number.isSafeInteger(loop) || loop < 0 || loop > 4096) {
        throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    }
    return loop;
}

function copySafeFrame(value                    )                   {
    return {
        frameId: value.frameId, occurrence: value.occurrence, label: value.label,
        pic: value.pic, state: value.state, wait: value.wait,
        next: value.next, dvx: value.dvx, dvy: value.dvy, dvz: value.dvz, centerx: value.centerx, centery: value.centery,
        hit_Fa: value.hit_Fa, hit_Fj: value.hit_Fj, hit_Ua: value.hit_Ua, hit_Uj: value.hit_Uj,
        hit_Da: value.hit_Da, hit_Dj: value.hit_Dj, hit_ja: value.hit_ja, hit_a: value.hit_a,
        hit_d: value.hit_d, hit_j: value.hit_j, mp: value.mp, vaction: value.vaction,
        itrs: value.itrs.map((entry) => ({ ...entry })), bdys: value.bdys.map((entry) => ({ ...entry })),
        opoints: value.opoints.map((entry) => ({ ...entry })), wpoints: value.wpoints.map((entry) => ({ ...entry })),
        bpoints: value.bpoints.map((entry) => ({ ...entry })), cpoints: value.cpoints.map((entry) => ({ ...entry })),
    };
}

function nativeRenderFrameView(value                   , occurrence        )                   {
    return {
        frameId: value.frameId,
        occurrence,
        label: "",
        pic: value.pic,
        state: value.state,
        wait: 0,
        next: 0,
        dvx: 0,
        dvy: 0,
        dvz: 0,
        centerx: value.centerx,
        centery: value.centery,
        hit_Fa: 0,
        hit_Fj: 0,
        hit_Ua: 0,
        hit_Uj: 0,
        hit_Da: 0,
        hit_Dj: 0,
        hit_ja: 0,
        hit_a: 0,
        hit_d: 0,
        hit_j: 0,
        mp: 0,
        vaction: 0,
        itrs: [],
        bdys: [],
        opoints: [],
        wpoints: [],
        bpoints: [],
        cpoints: [],
    };
}

function safeObservation(value                                                                   ) {
    return {
        name: basename(value.path),
        exists: value.exists,
        ...(value.size === undefined ? {} : { size: value.size }),
        ...(value.sha256 === undefined ? {} : { sha256: value.sha256 }),
    };
}

function mapSessionError(error         )                  {
    if (error instanceof ProjectDatError) return error;
    if (!(error instanceof DatSessionError)) return new ProjectDatError("invalid-request", "The DAT session operation failed.", { cause: error });
    if (error.code === "revision-conflict") return new ProjectDatError("revision-conflict", "The DAT session revision is stale.", { cause: error });
    if (error.code === "unknown-session" || error.code === "expired") return new ProjectDatError("unknown-session", "The project session is unknown.", { cause: error });
    return new ProjectDatError("invalid-request", error.message, { cause: error });
}
