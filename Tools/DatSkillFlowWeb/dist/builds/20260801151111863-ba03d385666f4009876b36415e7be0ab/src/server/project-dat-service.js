// dat-skill-flow-build:20260801151111863-ba03d385666f4009876b36415e7be0ab
import { execFile as nodeExecFile } from "node:child_process";
import { randomBytes } from "node:crypto";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join } from "node:path";

import { parseBmpMetadata } from "../assets/bmp.js";
                                                                     
import { DataTxtDocument, diagnoseResourcePath,                   } from "../project/data-txt.js";
import { DatSessionError, DatSessionService,                     } from "./dat-session-service.js";
             
                            
                          
                      
                         
                       
                        
                     
                        
                       
                                   
import { SafeSaveError, SafeSaveService } from "./safe-save.js";
import { WorkspaceRegistry, WorkspaceSecurityError } from "./workspace-registry.js";

const NARUTO_OID = 2;
const DEFAULT_START_FRAME = 300;
const DEFAULT_TICKS = 30;
const MAX_PREVIEW_OUTPUT_BYTES = 8 * 1024 * 1024;
const DEFAULT_CPP_DIRECTORY = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";
const DEFAULT_CPP_EXECUTABLE = "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\dat_preview_cli.exe";

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
        this.#executable = options.executable ?? DEFAULT_CPP_EXECUTABLE;
        this.#workingDirectory = options.workingDirectory ?? DEFAULT_CPP_DIRECTORY;
        this.#execFile = options.execFile ?? nodeExecFile                               ;
    }

    async preview(plaintext            , options                                          = {})                   {
        const startFrame = boundedInteger(options.startFrame ?? DEFAULT_START_FRAME, 0, 599, "startFrame");
        const ticks = boundedInteger(options.ticks ?? DEFAULT_TICKS, 1, 1800, "ticks");
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
            const bytes = await readFile(outputPath);
            if (bytes.length > MAX_PREVIEW_OUTPUT_BYTES) throw new Error("Native preview output exceeds its limit.");
            return JSON.parse(bytes.toString("utf8"))           ;
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
    #catalogObject                ;

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
        this.#safeSave = options.safeSave ?? new SafeSaveService(this.#primary);
        this.#previewRunner = options.previewRunner ?? new CppNativeDatPreviewRunner();
        this.#idFactory = options.idFactory ?? (() => randomBytes(32).toString("base64url"));
        this.#assetDirectories = (options.assetBmpDirectories ?? ["sprite/sys"])
            .map((value) => this.#primary.normalizeLogicalPath(value));
        this.#dataDocumentId = dataDocumentId;
        this.#primaryRootId = primaryRootId;
        this.#assetRootId = assetRootId;
    }

    static async initialize(options                          )                             {
        const primaryRootId = options.primaryRegistry.getStartupRootGrant()?.rootId;
        if (primaryRootId === undefined) throw new ProjectDatError("project-disabled", "The project workspace is not configured.");
        const dataPath = options.primaryRegistry.normalizeLogicalPath(options.dataTxtLogicalPath);
        const dataDocument = await options.primaryRegistry.openDocument(primaryRootId, dataPath);
        const assetRootId = options.assetRegistry?.getStartupRootGrant()?.rootId;
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
        return {
            catalogRevision: this.#catalogRevision,
            objects: this.#catalogObject === undefined ? [] : [{
                objectKey: this.#catalogObject.objectKey,
                oid: this.#catalogObject.oid,
                type: this.#catalogObject.type,
                availablePrimary: this.#catalogObject.availablePrimary,
            }],
        };
    }

    async open(objectKey        )                              {
        await this.#refreshCatalog();
        const object = this.#catalogObject;
        if (object === undefined || object.objectKey !== requireOpaqueId(objectKey, "objectKey")) {
            throw new ProjectDatError("unknown-object", "The project object is unknown.");
        }
        const opened = await this.#openCandidate(object.candidates);
        if (opened === undefined) throw new ProjectDatError("object-unavailable", "Naruto DAT is unavailable.");
        let session                ;
        try {
            session = await opened.service.openDocument(opened.documentId, "encrypted");
        } catch (error) {
            throw mapSessionError(error);
        }
        const binding                 = {
            ...opened,
            oid: NARUTO_OID,
            type: object.type,
            assetIds: new Set(),
        };
        this.#sessions.set(session.sessionId, binding);
        try {
            return await this.#buildSessionView(session, binding);
        } catch (error) {
            this.#sessions.delete(session.sessionId);
            await opened.service.close(session.sessionId);
            throw error;
        }
    }

    async edit(input         )                              {
        const request = exactRecord(input, ["sessionId", "fieldId", "value", "expectedRevision"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        await this.#refreshCatalog();
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            let view;
            try {
                view = await binding.service.edit(input);
            } catch (error) {
                throw mapSessionError(error);
            }
            this.#invalidateAssets(binding);
            return await this.#buildSessionView(view, binding);
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
                preview: await this.#runPreview(emission.plaintext, { startFrame, ticks }),
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
            if (binding.registry !== this.#primary) {
                throw new ProjectDatError("save-failed", "Fallback assets are read-only; this DAT cannot be overwritten.");
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

    async asset(assetId        )                                {
        await this.#refreshCatalog();
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
        const candidates = parsed.entries.filter((entry) => (
            entry.section === "object" && entry.id === NARUTO_OID && entry.type !== undefined
        ));
        if (candidates.length === 0) {
            this.#catalogObject = undefined;
            return;
        }
        let availablePrimary = false;
        for (const candidate of candidates) {
            if (diagnoseResourcePath(candidate.file) !== undefined) continue;
            if (await this.#canOpen(this.#primary, this.#primaryRootId, candidate.file)) {
                availablePrimary = true;
                break;
            }
        }
        this.#catalogObject = {
            objectKey: this.#newId(),
            oid: NARUTO_OID,
            type: candidates[0] .type ,
            candidates,
            availablePrimary,
        };
    }

    async #openCandidate(candidates                         )                                       {
        for (const candidate of candidates) {
            if (diagnoseResourcePath(candidate.file) !== undefined) continue;
            const primary = await this.#tryOpen(this.#primary, this.#primarySessions, this.#primaryRootId, candidate.file);
            if (primary !== undefined) return primary;
            if (this.#assets !== undefined && this.#assetSessions !== undefined && this.#assetRootId !== undefined) {
                const fallback = await this.#tryOpen(this.#assets, this.#assetSessions, this.#assetRootId, candidate.file);
                if (fallback !== undefined) return fallback;
            }
        }
        return undefined;
    }

    async #buildSessionView(session                , binding                )                              {
        const spriteRanges = [];
        for (const range of session.projection.spriteRanges) {
            const asset = await this.#resolveSpriteAsset(session.sessionId, range.file);
            if (asset === undefined) continue;
            binding.assetIds.add(asset.assetId);
            spriteRanges.push({
                frameLo: range.frameLo, frameHi: range.frameHi, assetId: asset.assetId,
                w: range.w, h: range.h, row: range.row, col: range.col,
            });
        }
        const emission = await binding.service.emit(session.sessionId, session.revision).catch((error) => { throw mapSessionError(error); });
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            oid: binding.oid,
            type: binding.type,
            name: session.projection.top.name,
            spriteRanges,
            frames: session.projection.frames.map(copySafeFrame),
            fields: session.fields.filter((field) => ![
                "head", "small", "file", "sound", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
            ].includes(field.key)),
            preview: await this.#runPreview(emission.plaintext),
            diagnostics: session.diagnostics.map((diagnostic) => ({
                code: diagnostic.code,
                severity: diagnostic.severity,
                message: diagnostic.message.replace(/(?:[A-Za-z]:[\\/]|\.\.[\\/])\S*/g, "[redacted]"),
            })),
        };
    }

    async #resolveSpriteAsset(sessionId        , rawPath        )                                           {
        if (diagnoseResourcePath(rawPath) !== undefined) return undefined;
        const exactPrimary = await this.#tryOpenDocument(this.#primary, this.#primaryRootId, rawPath);
        if (exactPrimary !== undefined) return this.#bindAsset(sessionId, this.#primary, exactPrimary.documentId);
        if (this.#assets === undefined || this.#assetRootId === undefined) return undefined;
        const exactFallback = await this.#tryOpenDocument(this.#assets, this.#assetRootId, rawPath);
        if (exactFallback !== undefined) return this.#bindAsset(sessionId, this.#assets, exactFallback.documentId);
        const name = basename(rawPath.replaceAll("\\", "/"));
        if (!/^[^/\\\0]+\.bmp$/i.test(name)) return undefined;
        const matches                                = [];
        for (const directory of this.#assetDirectories) {
            const candidate = await this.#tryOpenDocument(this.#assets, this.#assetRootId, `${directory}/${name}`);
            if (candidate !== undefined) matches.push(candidate);
        }
        return matches.length === 1 ? this.#bindAsset(sessionId, this.#assets, matches[0] .documentId) : undefined;
    }

    #bindAsset(sessionId        , registry                   , documentId        )                      {
        const assetId = this.#newId();
        this.#assetBindings.set(assetId, { sessionId, registry, documentId });
        return { assetId };
    }

    async #runPreview(plaintext            , options                                          )                             {
        try {
            return sanitizePreview(await this.#previewRunner.preview(Buffer.from(plaintext), options));
        } catch (error) {
            if (error instanceof ProjectDatError) throw error;
            throw new ProjectDatError("preview-failed", "The native Naruto preview failed.", { cause: error });
        }
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
        return await this.#tryOpenDocument(registry, rootId, path) !== undefined;
    }

    #requireSession(sessionId        )                 {
        const binding = this.#sessions.get(sessionId);
        if (binding === undefined) throw new ProjectDatError("unknown-session", "The project session is unknown.");
        return binding;
    }

    #invalidateAssets(binding                )       {
        for (const assetId of binding.assetIds) this.#assetBindings.delete(assetId);
        binding.assetIds.clear();
    }

    async #invalidateAll()                {
        const sessions = [...this.#sessions.entries()];
        this.#sessions.clear();
        this.#assetBindings.clear();
        await Promise.all(sessions.map(([sessionId, binding]) => binding.service.close(sessionId)));
    }

    async #enqueue   (sessionId        , operation                  )             {
        const previous = this.#queues.get(sessionId) ?? Promise.resolve();
        let release             ;
        const current = new Promise      ((resolveRelease) => { release = resolveRelease; });
        this.#queues.set(sessionId, current);
        await previous;
        try {
            return await operation();
        } finally {
            release();
            if (this.#queues.get(sessionId) === current) this.#queues.delete(sessionId);
        }
    }

    #newId()         {
        for (let attempt = 0; attempt < 16; attempt += 1) {
            const id = this.#idFactory();
            if (/^[A-Za-z0-9_-]{32,128}$/.test(id)) return id;
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
        slot: finite(item.slot, "slot"), oid: finite(item.oid, "oid"), frame: finite(item.frame, "frame"),
        pic: finite(item.pic, "pic"), facing: finite(item.facing, "facing"), x: finite(item.x, "x"),
        y: finite(item.y, "y"), z: finite(item.z, "z"), xInt: finite(item.x_int, "x_int"),
        yInt: finite(item.y_int, "y_int"), zInt: finite(item.z_int, "z_int"), velocity: vector(item.v, "v"),
        renderOffsetX: finite(item.render_offset_x, "render_offset_x"), frameDelay: finite(item.frame_delay, "frame_delay"),
        team: finite(item.team, "team"), target: finite(item.target, "target"), holder: finite(item.holder, "holder"),
        link: finite(item.link, "link"), ai: item.ai === true,
    };
}

function sanitizeTick(value         )                        {
    const item = record(value, "tick");
    const bg = record(item.bg, "background");
    const entities = Array.isArray(item.entities) ? item.entities : [];
    if (entities.length > 400) throw new ProjectDatError("preview-failed", "Native preview entity count exceeds its limit.");
    return {
        tick: finite(item.tick, "tick"), cameraX: finite(item.camera_x, "camera_x"),
        cameraVelocity: finite(item.camera_vel, "camera_vel"),
        background: {
            width: finite(bg.width, "bg.width"), zMin: finite(bg.z_min, "bg.z_min"), zMax: finite(bg.z_max, "bg.z_max"),
            boundLeft: finite(bg.bound_left, "bg.bound_left"), boundRight: finite(bg.bound_right, "bg.bound_right"),
        },
        entities: entities.map(sanitizeEntity),
    };
}

function sanitizePreview(value         )                    {
    const root = record(value, "root");
    const metadata = record(root.metadata, "metadata");
    const stage = record(metadata.stage, "stage");
    const initial = metadata.initial === undefined
        ? { p1: { x: 0, y: 0, z: 0 }, p2: { x: 0, y: 0, z: 0 } }
        : (() => {
            const raw = record(metadata.initial, "initial");
            return { p1: vector(raw.p1, "initial.p1"), p2: vector(raw.p2, "initial.p2") };
        })();
    const ticks = Array.isArray(root.ticks) ? root.ticks : [];
    if (ticks.length > 1800) throw new ProjectDatError("preview-failed", "Native preview tick count exceeds its limit.");
    if (metadata.runtime !== "ntsd_cpp" || metadata.tick_driver !== "SimulationTickDriver" || metadata.renderer !== "none") {
        throw new ProjectDatError("preview-failed", "Native preview authority metadata is invalid.");
    }
    return {
        metadata: {
            runtime: "ntsd_cpp", tickDriver: "SimulationTickDriver", renderer: "none",
            seed: finite(metadata.seed, "seed"), startFrame: finite(metadata.start_frame, "start_frame"),
            ticksRequested: finite(metadata.ticks_requested, "ticks_requested"),
            stage: {
                index: finite(stage.index, "stage.index"), name: textValue(stage.name, "stage.name"),
                width: finite(stage.width, "stage.width"), zMin: finite(stage.z_min, "stage.z_min"), zMax: finite(stage.z_max, "stage.z_max"),
            },
            initial,
        },
        ticks: ticks.map(sanitizeTick),
    };
}

function copySafeFrame(value                    )                   {
    return {
        frameId: value.frameId, occurrence: value.occurrence, pic: value.pic, state: value.state, wait: value.wait,
        next: value.next, dvx: value.dvx, dvy: value.dvy, dvz: value.dvz, centerx: value.centerx, centery: value.centery,
        hit_Fa: value.hit_Fa, hit_Fj: value.hit_Fj, hit_Ua: value.hit_Ua, hit_Uj: value.hit_Uj,
        hit_Da: value.hit_Da, hit_Dj: value.hit_Dj, hit_ja: value.hit_ja, hit_a: value.hit_a,
        hit_d: value.hit_d, hit_j: value.hit_j, mp: value.mp, vaction: value.vaction,
        itrs: value.itrs.map((entry) => ({ ...entry })), bdys: value.bdys.map((entry) => ({ ...entry })),
        opoints: value.opoints.map((entry) => ({ ...entry })), wpoints: value.wpoints.map((entry) => ({ ...entry })),
        bpoints: value.bpoints.map((entry) => ({ ...entry })), cpoints: value.cpoints.map((entry) => ({ ...entry })),
    };
}

function safeObservation(value                                                     ) {
    return { exists: value.exists, ...(value.size === undefined ? {} : { size: value.size }), ...(value.sha256 === undefined ? {} : { sha256: value.sha256 }) };
}

function mapSessionError(error         )                  {
    if (!(error instanceof DatSessionError)) return new ProjectDatError("invalid-request", "The DAT session operation failed.", { cause: error });
    if (error.code === "revision-conflict") return new ProjectDatError("invalid-request", "The DAT session revision is stale.", { cause: error });
    if (error.code === "unknown-session" || error.code === "expired-session") return new ProjectDatError("unknown-session", "The project session is unknown.", { cause: error });
    return new ProjectDatError("invalid-request", error.message, { cause: error });
}
