// dat-skill-flow-build:20260808055417257-4f5434d4ce4c4ee7b7cda76a6f6eed4c
import { execFile as nodeExecFile } from "node:child_process";
import { randomBytes } from "node:crypto";
import { mkdtemp, open as openFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join } from "node:path";

import { parseBmpMetadata } from "../assets/bmp.js";
import { LosslessDatDocument } from "../model/dat-document.js";
                                                                
import { DataTxtDocument, diagnoseResourcePath,                   } from "../project/data-txt.js";
import { MAX_CATALOG_OIDS } from "../sim/catalog.js";
import {
    DatSessionError,
    DatSessionService,
                                
                            
                        
} from "./dat-session-service.js";
             
                            
                          
                      
                         
                       
                         
                        
                     
                             
                        
                       
                                   
import { enrichNativePreview } from "./native-preview-trace.js";
import { SafeSaveError, SafeSaveService } from "./safe-save.js";
import {                      WorkspaceRegistry, WorkspaceSecurityError } from "./workspace-registry.js";

const DEFAULT_START_FRAME = 300;
const DEFAULT_TICKS = 30;
const NARUTO_OID = 2;
const MAX_PREVIEW_OUTPUT_BYTES = 8 * 1024 * 1024;
const DEFAULT_CPP_DIRECTORY = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";
const DEFAULT_CPP_EXECUTABLE = "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\dat_preview_cli.exe";

export function previewDatProjection(bytes            )                {
    const input = Buffer.from(bytes);
    let offset = 0;
    if (input.length >= 3 && input[0] === 0xef && input[1] === 0xbb && input[2] === 0xbf) offset = 3;
    const plainBytes = input.subarray(offset);
    const plaintext = LosslessDatDocument.fromPlaintext(plainBytes);
    if (plaintext.cst.topFields.length > 0
        || plaintext.cst.spriteRanges.length > 0
        || plaintext.cst.frames.length > 0) {
        return plaintext.projection;
    }
    return LosslessDatDocument.fromEncrypted(input).projection;
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
             #execFile                  ;

    constructor(options                                                                                  = {}) {
        this.#executable = options.executable === undefined ? DEFAULT_CPP_EXECUTABLE : options.executable;
        this.#workingDirectory = options.workingDirectory === undefined ? DEFAULT_CPP_DIRECTORY : options.workingDirectory;
        this.#execFile = options.execFile === undefined ? nodeExecFile                                : options.execFile;
    }

    async preview(plaintext            , options                                          = {})                   {
        const startFrame = boundedInteger(options.startFrame === undefined ? DEFAULT_START_FRAME : options.startFrame, 0, 599, "startFrame");
        const ticks = boundedInteger(options.ticks === undefined ? DEFAULT_TICKS : options.ticks, 1, 1800, "ticks");
        const directory = await mkdtemp(join(tmpdir(), "dat-skill-flow-preview-"));
        const datPath = join(directory, "naruto.dat");
        const outputPath = join(directory, "preview.json");
        try {
            await writeFile(datPath, Buffer.from(plaintext), { flag: "wx" });
            await new Promise      ((resolveRun, rejectRun) => {
                this.#execFile(this.#executable, [
                    "--naruto-dat", datPath,
                    "--output", outputPath,
                    "--start-frame", String(startFrame),
                    "--ticks", String(ticks),
                ], {
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
             #primarySessions                   ;
             #assetSessions                    ;
             #safeSave                 ;
             #previewRunner                        ;
             #idFactory              ;
             #assetDirectories                   ;
             #dataDocumentId        ;
             #primaryRootId        ;
             #assetRootId         ;
             #sessions = new Map                        ();
             #assetBindings = new Map                      ();
             #queues = new Map                       ();
    #catalogRevision = 1;
    #catalogObjects                  = [];
    #nextIdSequence = 0;

            constructor(
        options                          ,
        dataDocumentId        ,
        primaryRootId        ,
        assetRootId                    ,
    ) {
        this.#primary = options.primaryRegistry;
        this.#assets = options.assetRegistry;
        this.#primarySessions = new DatSessionService(this.#primary);
        this.#assetSessions = this.#assets === undefined ? undefined : new DatSessionService(this.#assets);
        this.#safeSave = options.safeSave === undefined ? new SafeSaveService(this.#primary) : options.safeSave;
        this.#previewRunner = options.previewRunner === undefined ? new CppNativeDatPreviewRunner() : options.previewRunner;
        this.#idFactory = options.idFactory === undefined ? (() => randomBytes(32).toString("base64url")) : options.idFactory;
        const assetBmpDirectories = options.assetBmpDirectories === undefined ? ["sprite/sys"] : options.assetBmpDirectories;
        this.#assetDirectories = assetBmpDirectories
            .map((value) => this.#primary.normalizeLogicalPath(value));
        this.#dataDocumentId = dataDocumentId;
        this.#primaryRootId = primaryRootId;
        this.#assetRootId = assetRootId;
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
        const service = new ProjectDatService(options, dataDocument.documentId, primaryRootId, assetRootId);
        const read = await options.primaryRegistry.readDocument(dataDocument.documentId);
        await service.#replaceCatalog(read.bytes);
        return service;
    }

    async catalog()                              {
        await this.#refreshCatalog();
        const objects = this.#catalogObjects.map((object) => ({
            objectKey: object.objectKey,
            oid: object.oid,
            type: object.type,
            availablePrimary: object.availablePrimary,
        }));
        return {
            catalogRevision: this.#catalogRevision,
            objects,
        };
    }

    async open(objectKey        )                              {
        await this.#refreshCatalog();
        const objectKeyValidated = requireOpaqueId(objectKey, "objectKey");
        const object = this.#catalogObjects.find((candidate) => candidate.objectKey === objectKeyValidated);
        if (object === undefined) {
            throw new ProjectDatError("unknown-object", "The project object is unknown.");
        }
        if (object.oid !== NARUTO_OID) {
            throw new ProjectDatError("object-unavailable", "Native preview currently supports Naruto OID 2 only.");
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
            writable: opened.registry === this.#primary,
            assetIdsByPath: new Map(),
            previewResourcesByOid: new Map(),
            unavailablePreviewOids: new Set(),
        };
        this.#sessions.set(session.sessionId, binding);
        try {
            return await this.#buildSessionView(session, binding);
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
        await this.#refreshCatalog();
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
        const request = exactRecord(input, ["sessionId", "expectedRevision", "startFrame", "ticks"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        const expectedRevision = boundedInteger(request.expectedRevision, 0, Number.MAX_SAFE_INTEGER, "expectedRevision");
        const startFrame = boundedInteger(request.startFrame, 0, 599, "startFrame");
        const ticks = boundedInteger(request.ticks, 1, 1800, "ticks");
        await this.#refreshCatalog();
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            const emission = await binding.service.emit(sessionId, expectedRevision).catch((error) => { throw mapSessionError(error); });
            return {
                sessionId,
                revision: emission.revision,
                preview: await this.#runPreview(emission.plaintext, binding, { startFrame, ticks }),
            };
        });
    }

    async save(input         )                               {
        const request = exactRecord(input, ["sessionId", "expectedRevision"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        const expectedRevision = boundedInteger(request.expectedRevision, 0, Number.MAX_SAFE_INTEGER, "expectedRevision");
        await this.#refreshCatalog();
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
        let read;
        try {
            read = await binding.registry.readDocument(binding.documentId);
        } catch (error) {
            throw new ProjectDatError("unknown-asset", "The asset is unavailable.", { cause: error });
        }
        const metadata = parseBmpMetadata(read.bytes);
        if (!metadata.ok) throw new ProjectDatError("invalid-asset", "The asset is not a supported BMP.");
        return { bytes: Buffer.from(read.bytes) };
    }

    async #refreshCatalog()                {
        this.#sweepExpired();
        let prepared;
        try {
            prepared = await this.#primary.prepareDocumentRefresh(this.#dataDocumentId);
        } catch (error) {
            throw new ProjectDatError("catalog-invalid", "The project catalog could not be refreshed safely.", { cause: error });
        }
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

        const prepared                  = [];
        for (const candidates of candidatesByOid.values()) {
            const first = candidates[0] ;
            let availablePrimary = false;
            if (first.id === NARUTO_OID) {
                for (const candidate of candidates) {
                    if (diagnoseResourcePath(candidate.file) !== undefined) continue;
                    if (await this.#canOpen(this.#primary, this.#primaryRootId, candidate.file)) {
                        availablePrimary = true;
                        break;
                    }
                }
            }
            prepared.push({
                objectKey: this.#newId(),
                oid: first.id,
                type: first.type ,
                candidates,
                availablePrimary,
            });
        }
        this.#catalogObjects = prepared;
    }

    async #openCandidate(candidates                         )                                       {
        for (const candidate of candidates) {
            if (diagnoseResourcePath(candidate.file) !== undefined) continue;
            const primary = await this.#tryOpen(this.#primary, this.#primarySessions, this.#primaryRootId, candidate.file);
            if (primary !== undefined) return primary;
            if (this.#assets !== undefined && this.#assetSessions !== undefined && this.#assetRootId !== undefined) {
                const fallback = await this.#tryOpen(this.#assets, this.#assetSessions, this.#assetRootId, candidate.file);
                if (fallback !== undefined) return fallback;
                for (const fallbackPath of this.#fallbackDatPaths(candidate.file)) {
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
        const preview = await this.#runPreview(emission.plaintext, binding);
        const primaryResources = preview.resources.find((resource) => resource.oid === binding.oid);
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.dirty,
            writable: binding.writable,
            oid: binding.oid,
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
            diagnostics: session.diagnostics.map((diagnostic) => ({
                code: diagnostic.code,
                severity: diagnostic.severity,
                message: diagnostic.message.replace(/(?:[A-Za-z]:[\\/]|\.\.[\\/])\S*/g, "[redacted]"),
            })),
        };
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
        const exactPrimary = await this.#tryOpenDocument(this.#primary, this.#primaryRootId, rawPath);
        if (exactPrimary !== undefined) return this.#bindAsset(binding, pathKey, this.#primary, exactPrimary);
        if (this.#assets === undefined || this.#assetRootId === undefined) return undefined;
        const exactFallback = await this.#tryOpenDocument(this.#assets, this.#assetRootId, rawPath);
        if (exactFallback !== undefined) return this.#bindAsset(binding, pathKey, this.#assets, exactFallback);
        const name = basename(rawPath.replaceAll("\\", "/"));
        if (!/^[^/\\\0]+\.bmp$/i.test(name)) return undefined;
        const matches                   = [];
        for (const directory of this.#assetDirectories) {
            const candidate = await this.#tryOpenDocument(this.#assets, this.#assetRootId, `${directory}/${name}`);
            if (candidate !== undefined) matches.push(candidate);
        }
        if (matches.length === 1) return this.#bindAsset(binding, pathKey, this.#assets, matches[0] );
        for (const match of matches) this.#assets.closeDocument(match.documentId);
        return undefined;
    }

    #bindAsset(
        binding                ,
        pathKey        ,
        registry                   ,
        document                        ,
    )                      {
        const assetId = this.#newId();
        binding.assetIdsByPath.set(pathKey, assetId);
        this.#assetBindings.set(assetId, {
            sessionId: binding.sessionId,
            registry,
            documentId: document.documentId,
        });
        return { assetId };
    }

    async #runPreview(
        plaintext            ,
        binding                ,
        options                                          ,
    )                             {
        try {
            const primary = LosslessDatDocument.fromPlaintext(plaintext).projection;
            const rawPreview = sanitizePreview(await this.#previewRunner.preview(Buffer.from(plaintext), options));
            const objectOids = new Set(rawPreview.ticks.flatMap((tick) => tick.entities.map((entity) => entity.oid)));
            objectOids.add(binding.oid);
            const resources = await this.#loadPreviewResources(binding, objectOids, primary);
            const stage = await this.#loadStageAssets(binding, rawPreview.metadata.stage);
            const objectTypes = new Map(this.#catalogObjects.map((object) => [object.oid, object.type]));
            return enrichNativePreview({
                ...rawPreview,
                metadata: {
                    ...rawPreview.metadata,
                    stage,
                },
                resources,
            }, resources, objectTypes, binding.oid);
        } catch (error) {
            if (error instanceof ProjectDatError) throw error;
            throw new ProjectDatError("preview-failed", "The native Naruto preview failed.", { cause: error });
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
    )                                               {
        const resources                             = [];
        const primary = this.#catalogObjects.find((object) => object.oid === binding.oid);
        if (primary !== undefined) {
            resources.push(await this.#projectPreviewObject(binding, primary, primaryProjection));
        }
        const auxiliary = [...oids]
            .filter((oid) => oid !== binding.oid)
            .map((oid) => this.#catalogObjects.find((object) => object.oid === oid))
            .filter((object)                          => object !== undefined);
        for (const object of auxiliary) {
            const resource = await this.#openPreviewObject(binding, object);
            if (resource !== undefined) resources.push(resource);
        }
        return resources;
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

    async #canOpen(registry                   , rootId        , path        )                   {
        const opened = await this.#tryOpenDocument(registry, rootId, path);
        if (opened === undefined) return false;
        registry.closeDocument(opened.documentId);
        return true;
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
        ];
        for (const sessionId of expiredSessionIds) {
            const binding = this.#sessions.get(sessionId);
            if (binding !== undefined) this.#releaseBinding(sessionId, binding);
        }
    }

    #releaseBinding(sessionId        , binding                )       {
        this.#sessions.delete(sessionId);
        for (const assetId of binding.assetIdsByPath.values()) {
            const asset = this.#assetBindings.get(assetId);
            if (asset !== undefined) asset.registry.closeDocument(asset.documentId);
            this.#assetBindings.delete(assetId);
        }
        binding.assetIdsByPath.clear();
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

function exactRecord(value         , keys                   )                          {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        throw new ProjectDatError("invalid-request", "The request must be an object.");
    }
    const record = value                           ;
    const actual = Object.keys(record);
    if (actual.length !== keys.length || actual.some((key) => !keys.includes(key)) || keys.some((key) => !Object.hasOwn(record, key))) {
        throw new ProjectDatError("invalid-request", "The request has missing or unknown fields.");
    }
    return record;
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
        zInt: previewInteger(item.z_int, -2_147_483_648, 2_147_483_647, "z_int"),
        velocity: vector(item.v, "v"),
        renderOffsetX: previewInteger(item.render_offset_x, -2_147_483_648, 2_147_483_647, "render_offset_x"),
        frameDelay: previewInteger(item.frame_delay, -2_147_483_648, 2_147_483_647, "frame_delay"),
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

function sanitizePreview(value         )                    {
    const root = record(value, "root");
    const metadata = record(root.metadata, "metadata");
    const stage = record(metadata.stage, "stage");
    const startFrame = finite(metadata.start_frame, "start_frame");
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
    return {
            metadata: {
                runtime: "ntsd_cpp", tickDriver: "SimulationTickDriver", renderer: "none",
                seed: finite(metadata.seed, "seed"), startFrame, ticksRequested,
                stage: {
                    index: finite(stage.index, "stage.index"), name: textValue(stage.name, "stage.name"),
                    width: finite(stage.width, "stage.width"), zMin: finite(stage.z_min, "stage.z_min"), zMax: finite(stage.z_max, "stage.z_max"),
                    ...(background === undefined ? {} : { background }),
                },
                initial,
        },
        resources: [],
        trace: {
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
