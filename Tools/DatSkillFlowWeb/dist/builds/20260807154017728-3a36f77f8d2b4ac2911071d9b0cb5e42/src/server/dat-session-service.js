// dat-skill-flow-build:20260807154017728-3a36f77f8d2b4ac2911071d9b0cb5e42
import { randomBytes } from "node:crypto";

import {
    createSetScalarCommand,
    createSetIntegerPairCommand,
    isLatin1ScalarString,
    LosslessDatDocument,
} from "../model/dat-document.js";
import {
    applyDatStructureEdit,
    canCopyBlock,
    canCopyFrame,
    canDeleteBlock,
    canDeleteFrame,
                             
} from "../model/dat-structure-edit.js";
             
                  
                     
                     
                       
                     
                  
                     
                          
                     
                                    
import { isSignedInt32,                                     } from "../syntax/byte-cst.js";
                                                                   
import { DAT_ENVELOPE_PREFIX_LENGTH } from "../syntax/dat-envelope.js";
             
                   
                    
                            
                               
                             
                          
                        
                        
                        
                         
                          
                             
                            
                             
                                   
                                 
                   
                                   
import { WorkspaceRegistry } from "./workspace-registry.js";

             
                   
                    
                            
                               
                             
                          
                        
                        
                         
                        
                         
                          
                             
                            
                             
                                   
                                 
                   
                                   

export const DEFAULT_DAT_SESSION_LIMITS = Object.freeze({
    maxSessions: 32,
    maxFieldsPerSession: 50_000,
    maxLoadedBytes: 64 * 1024 * 1024,
    idleTtlMs: 15 * 60 * 1_000,
    maxDiagnostics: 200,
    maxDiagnosticMessageLength: 512,
    maxProjectionBytes: 2 * 1024 * 1024,
    maxViewBytes: 8 * 1024 * 1024,
    maxStringBytes: 4 * 1024,
});

export class DatSessionError extends Error {
             code                     ;

    constructor(code                     , message        , options               ) {
        super(message, options);
        this.name = "DatSessionError";
        this.code = code;
    }
}

                                     

                           
                       
                              
                                        
                                
                            
 

                                                   
                    
                                       
 

                               
                         
                                 
                     
                       
 

                        
                      
                       
                                  
                           
                     
                              
                        
                       
                                         
                         
                                                 
                             
                                                      
 

                                     
                               
                                
                              
                            
                               
                          
 

                                      
                         
                                 
                          

                            
                                  
                           
                                   
 

                  
                        
                                
                           
                      
                           
                                       
                               
                         
                           
 

const MOVEMENT_DOUBLE_FIELDS = new Set([
    "walking_speed", "walking_speedz", "running_speed", "running_speedz",
    "heavy_walking_speed", "heavy_walking_speedz", "heavy_running_speed", "heavy_running_speedz",
    "jump_height", "jump_distance", "jump_distancez", "dash_height", "dash_distance", "dash_distancez",
    "rowing_height", "rowing_distance",
]);

const TOP_STRING_FIELDS = new Set([
    "name", "head", "small", "weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound",
]);
const TOP_INTEGER_FIELDS = new Set([
    "weapon_hp", "weapon_drop_hurt", "walking_frame_rate", "running_frame_rate",
]);
const SPRITE_INTEGER_FIELDS = new Set(["w", "h", "row", "col"]);
const FRAME_INTEGER_FIELDS = new Set([
    "pic", "state", "wait", "next", "dvx", "dvy", "dvz", "centerx", "centery",
    "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj", "hit_ja",
    "hit_a", "hit_d", "hit_j", "mp", "vaction",
]);
const BLOCK_INTEGER_FIELDS                                                      = {
    itr: new Set([
        "kind", "x", "y", "w", "h", "dvx", "dvy", "fall", "bdefend", "injury", "arest", "vrest",
        "effect", "attacking", "respond", "pickingact", "pickedact", "throwvx", "throwvy", "zwidth",
        "throwvz", "throwinjury",
    ]),
    bdy: new Set(["x", "y", "w", "h"]),
    opoint: new Set(["kind", "x", "y", "action", "dvx", "dvy", "oid", "facing"]),
    wpoint: new Set(["kind", "x", "y", "attacking", "cover", "weaponact", "dvx", "dvy", "dvz"]),
    bpoint: new Set(["x", "y"]),
    cpoint: new Set([
        "kind", "x", "y", "injury", "cover", "vaction", "aaction", "jaction", "daction", "taction",
        "throwvx", "throwvy", "throwvz", "throwinjury", "hurtable", "decrease", "dircontrol",
        "fronthurtact", "backhurtact",
    ]),
};

function positiveSafeInteger(value        , name        )         {
    if (!Number.isSafeInteger(value) || value < 1) throw new RangeError(`${name} must be a positive safe integer.`);
    return value;
}

function nonnegativeSafeInteger(value        , name        )         {
    if (!Number.isSafeInteger(value) || value < 0) throw new RangeError(`${name} must be a nonnegative safe integer.`);
    return value;
}

function sameFieldValue(
    left                      ,
    right                      ,
)          {
    if (Array.isArray(left) || Array.isArray(right)) {
        return Array.isArray(left)
            && Array.isArray(right)
            && left[0] === right[0]
            && left[1] === right[1];
    }
    return left === right;
}

function exactRecord(value         , keys                   )                          {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        throw new DatSessionError("invalid-request", "The request must be an object.");
    }
    const record = value                           ;
    const ownKeys = Object.keys(record);
    if (ownKeys.length !== keys.length
        || ownKeys.some((key) => !keys.includes(key))
        || keys.some((key) => !Object.hasOwn(record, key))) {
        throw new DatSessionError("invalid-request", "The request has missing or unknown fields.");
    }
    return record;
}

function requiredId(value         , name        )         {
    if (typeof value !== "string" || value.length === 0 || value.length > 128 || value.includes("\0")) {
        throw new DatSessionError("invalid-request", `${name} must be an opaque ID of at most 128 characters.`);
    }
    return value;
}

function requiredFormat(value         )                 {
    if (value !== "plaintext" && value !== "encrypted") {
        throw new DatSessionError("invalid-request", "format must be plaintext or encrypted.");
    }
    return value;
}

function requiredRevision(value         )         {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < 0) {
        throw new DatSessionError("invalid-request", "expectedRevision must be a nonnegative safe integer.");
    }
    return value;
}

function authorityKind(
    field             ,
    location                                   ,
)                                                               {
    if (location.scope === "top") {
        if (TOP_STRING_FIELDS.has(field.key)) return "string";
        if (MOVEMENT_DOUBLE_FIELDS.has(field.key)) return "number";
        if (TOP_INTEGER_FIELDS.has(field.key)) return "integer";
        return undefined;
    }
    if (location.scope === "sprite") {
        if (field.key === "file") return "string";
        return SPRITE_INTEGER_FIELDS.has(field.key) ? "integer" : undefined;
    }
    if (location.scope === "frame") {
        if (field.key === "sound") return "string";
        return FRAME_INTEGER_FIELDS.has(field.key) ? "integer" : undefined;
    }
    if (location.blockType === undefined) return undefined;
    if (location.blockType === "itr" && (field.key === "catchingact" || field.key === "caughtact")) return "integer-pair";
    return BLOCK_INTEGER_FIELDS[location.blockType].has(field.key) ? "integer" : undefined;
}

function collectFields(
    document                     ,
    failureCode                                     ,
)                    {
    const descriptors                    = [];
    const append = (fields                        , base                                   )       => {
        const occurrences = new Map                ();
        for (const field of fields) {
            const occurrence = occurrences.get(field.key) ?? 0;
            occurrences.set(field.key, occurrence + 1);
            const semanticKind = authorityKind(field, base);
            if (semanticKind === undefined) continue;
            let value                      ;
            if (semanticKind === "integer-pair") {
                if (field.integerPairValue === undefined) {
                    throw new DatSessionError(failureCode, `ITR field ${field.key} requires exactly two signed 32-bit integers.`);
                }
                value = field.integerPairValue;
                descriptors.push({
                    field,
                    kind: semanticKind,
                    value,
                    location: { ...base, occurrence },
                });
                continue;
            }
            if (semanticKind === "string") {
                value = field.rawValue.toString("latin1");
                if (!isLatin1ScalarString(value)) {
                    throw new DatSessionError(failureCode, `String DAT field ${field.key} is not a safe Latin-1 scalar.`);
                }
            } else {
                const numericValue = field.numericValue;
                if (field.scalarKind !== "number" || numericValue === undefined || !Number.isFinite(numericValue)) {
                    throw new DatSessionError(failureCode, `Numeric DAT field ${field.key} is not a complete finite scalar.`);
                }
                if (semanticKind === "integer"
                    && (!/^[+-]?\d+$/.test(field.rawValue.toString("latin1"))
                        || !isSignedInt32(numericValue))) {
                    throw new DatSessionError(failureCode, `Integer DAT field ${field.key} is outside the signed 32-bit contract.`);
                }
                value = numericValue;
            }
            const location = { ...base, occurrence };
            descriptors.push({
                field,
                kind: semanticKind === "string" ? "string" : "number",
                numericKind: semanticKind === "string" ? undefined : semanticKind,
                value,
                location,
            });
        }
    };
    append(document.cst.topFields, { scope: "top" });
    for (let rangeIndex = 0; rangeIndex < document.cst.spriteRanges.length; rangeIndex += 1) {
        const range = document.cst.spriteRanges[rangeIndex] ;
        append([range.fileField, ...range.fields], { scope: "sprite", spriteRangeIndex: rangeIndex });
    }
    for (const frame of document.cst.frames) {
        if (frame.frameId < 0 || frame.frameId >= 600) continue;
        append(frame.fields, {
            scope: "frame",
            frameId: frame.frameId,
            frameOccurrence: frame.occurrence,
        });
        for (const block of frame.blocks) {
            append(block.fields, {
                scope: "block",
                frameId: frame.frameId,
                frameOccurrence: frame.occurrence,
                blockType: block.type,
                blockIndex: block.index,
            });
        }
    }
    return descriptors;
}

function copyTop(value                  , stringValue                           )                   {
    return {
        name: stringValue(value.name), head: stringValue(value.head), small: stringValue(value.small),
        weapon_hit_sound: stringValue(value.weapon_hit_sound), weapon_drop_sound: stringValue(value.weapon_drop_sound),
        weapon_broken_sound: stringValue(value.weapon_broken_sound), weapon_hp: value.weapon_hp,
        weapon_drop_hurt: value.weapon_drop_hurt, walking_frame_rate: value.walking_frame_rate,
        walking_speed: value.walking_speed, walking_speedz: value.walking_speedz,
        running_frame_rate: value.running_frame_rate, running_speed: value.running_speed,
        running_speedz: value.running_speedz, heavy_walking_speed: value.heavy_walking_speed,
        heavy_walking_speedz: value.heavy_walking_speedz, heavy_running_speed: value.heavy_running_speed,
        heavy_running_speedz: value.heavy_running_speedz, jump_height: value.jump_height,
        jump_distance: value.jump_distance, jump_distancez: value.jump_distancez, dash_height: value.dash_height,
        dash_distance: value.dash_distance, dash_distancez: value.dash_distancez, rowing_height: value.rowing_height,
        rowing_distance: value.rowing_distance,
    };
}

function copyItr(value               )                {
    return {
        kind: value.kind, x: value.x, y: value.y, w: value.w, h: value.h, dvx: value.dvx, dvy: value.dvy,
        fall: value.fall, bdefend: value.bdefend, injury: value.injury, arest: value.arest, vrest: value.vrest,
        effect: value.effect, attacking: value.attacking, catchingact: value.catchingact,
        catchingact2: value.catchingact2, caughtact: value.caughtact, caughtact2: value.caughtact2,
        respond: value.respond, pickingact: value.pickingact, pickedact: value.pickedact,
        throwvx: value.throwvx, throwvy: value.throwvy, zwidth: value.zwidth,
        throwvz: value.throwvz, throwinjury: value.throwinjury,
    };
}

function copyBdy(value               )                {
    return { x: value.x, y: value.y, w: value.w, h: value.h };
}

function copyOpoint(value                  )                   {
    return { kind: value.kind, x: value.x, y: value.y, action: value.action, dvx: value.dvx, dvy: value.dvy, oid: value.oid, facing: value.facing };
}

function copyWpoint(value                  )                   {
    return { kind: value.kind, x: value.x, y: value.y, attacking: value.attacking, cover: value.cover, weaponact: value.weaponact, dvx: value.dvx, dvy: value.dvy, dvz: value.dvz };
}

function copyBpoint(value                  )                   {
    return { x: value.x, y: value.y };
}

function copyCpoint(value                  )                   {
    return {
        kind: value.kind, x: value.x, y: value.y, injury: value.injury, cover: value.cover,
        vaction: value.vaction, aaction: value.aaction, jaction: value.jaction, daction: value.daction,
        taction: value.taction, throwvx: value.throwvx, throwvy: value.throwvy, throwvz: value.throwvz,
        throwinjury: value.throwinjury, hurtable: value.hurtable, decrease: value.decrease,
        dircontrol: value.dircontrol, fronthurtact: value.fronthurtact, backhurtact: value.backhurtact,
    };
}

function copyFrame(value                    , stringValue                           )                     {
    return {
        frameId: value.frameId, occurrence: value.occurrence, label: stringValue(value.label),
        pic: value.pic, state: value.state, wait: value.wait,
        next: value.next, dvx: value.dvx, dvy: value.dvy, dvz: value.dvz, centerx: value.centerx,
        centery: value.centery, hit_Fa: value.hit_Fa, hit_Fj: value.hit_Fj, hit_Ua: value.hit_Ua,
        hit_Uj: value.hit_Uj, hit_Da: value.hit_Da, hit_Dj: value.hit_Dj, hit_ja: value.hit_ja,
        hit_a: value.hit_a, hit_d: value.hit_d, hit_j: value.hit_j, mp: value.mp, vaction: value.vaction,
        sound: stringValue(value.sound), itrs: value.itrs.map(copyItr), bdys: value.bdys.map(copyBdy),
        opoints: value.opoints.map(copyOpoint), wpoints: value.wpoints.map(copyWpoint),
        bpoints: value.bpoints.map(copyBpoint), cpoints: value.cpoints.map(copyCpoint),
    };
}

export class DatSessionService {
             #registry                   ;
             #limits        ;
             #now              ;
             #idFactory              ;
             #sessions = new Map                      ();
             #queues = new Map                       ();
             #expiredSessionIds = new Map                ();
    #loadedBytes = 0;
    #pendingOpens = 0;
    #nextIdSequence = 0;
    #lifecycleVersion = 0;
    #disposed = false;

    constructor(registry                   , options                           = {}) {
        this.#registry = registry;
        this.#limits = {
            maxSessions: positiveSafeInteger(options.maxSessions ?? DEFAULT_DAT_SESSION_LIMITS.maxSessions, "maxSessions"),
            maxFieldsPerSession: positiveSafeInteger(options.maxFieldsPerSession ?? DEFAULT_DAT_SESSION_LIMITS.maxFieldsPerSession, "maxFieldsPerSession"),
            maxLoadedBytes: positiveSafeInteger(options.maxLoadedBytes ?? DEFAULT_DAT_SESSION_LIMITS.maxLoadedBytes, "maxLoadedBytes"),
            idleTtlMs: positiveSafeInteger(options.idleTtlMs ?? DEFAULT_DAT_SESSION_LIMITS.idleTtlMs, "idleTtlMs"),
            maxDiagnostics: nonnegativeSafeInteger(options.maxDiagnostics ?? DEFAULT_DAT_SESSION_LIMITS.maxDiagnostics, "maxDiagnostics"),
            maxDiagnosticMessageLength: positiveSafeInteger(options.maxDiagnosticMessageLength ?? DEFAULT_DAT_SESSION_LIMITS.maxDiagnosticMessageLength, "maxDiagnosticMessageLength"),
            maxProjectionBytes: positiveSafeInteger(options.maxProjectionBytes ?? DEFAULT_DAT_SESSION_LIMITS.maxProjectionBytes, "maxProjectionBytes"),
            maxViewBytes: positiveSafeInteger(options.maxViewBytes ?? DEFAULT_DAT_SESSION_LIMITS.maxViewBytes, "maxViewBytes"),
            maxStringBytes: positiveSafeInteger(options.maxStringBytes ?? DEFAULT_DAT_SESSION_LIMITS.maxStringBytes, "maxStringBytes"),
        };
        this.#now = options.now ?? Date.now;
        this.#idFactory = options.idFactory ?? (() => randomBytes(32).toString("base64url"));
    }

    // inputFormat must be supplied by trusted server-side project metadata, never by an HTTP/client payload.
    // The envelope has no magic signature: this method selects a deterministic interpretation and does not detect it.
    async openDocument(documentId        , inputFormat                )                          {
        this.#ensureActive();
        requiredId(documentId, "documentId");
        const format = requiredFormat(inputFormat);
        if (this.#sessions.size + this.#pendingOpens >= this.#limits.maxSessions) {
            throw new DatSessionError("session-limit", "The DAT session limit has been reached.");
        }
        const lifecycleVersion = this.#lifecycleVersion;
        this.#pendingOpens += 1;
        try {
            let read;
            try {
                read = await this.#registry.readDocument(documentId);
            } catch (error) {
                throw new DatSessionError("invalid-request", "The document cannot be opened as a DAT session.", { cause: error });
            }
            this.#ensureLifecycle(lifecycleVersion);
            // No await occurs between this recheck and accounting publication, so concurrent reads cannot overcommit bytes.
            if (this.#loadedBytes + read.bytes.length > this.#limits.maxLoadedBytes) {
                throw new DatSessionError("byte-limit", "The loaded DAT byte limit has been reached.");
            }
            const analyzed = this.#analyze(read.bytes, format, "invalid-request");
            const sessionId = this.#newId();
            const session = this.#createState(sessionId, documentId, read.bytes.length, 0, analyzed);
            const view = this.#createView(session);
            this.#sessions.set(sessionId, session);
            this.#loadedBytes += read.bytes.length;
            return view;
        } finally {
            this.#pendingOpens -= 1;
        }
    }

    async edit(input         , beforeCommit                         )                          {
        this.#ensureActive();
        const request = this.#parseEditRequest(input);
        return await this.#enqueue(request.sessionId, () => {
            const session = this.#requireSession(request.sessionId);
            session.lastAccess = this.#clock();
            return this.#applyBatch(session, {
                sessionId: request.sessionId,
                edits: [{ fieldId: request.fieldId, value: request.value }],
                expectedRevision: request.expectedRevision,
            }, beforeCommit);
        });
    }

    async editBatch(input         , beforeCommit                         )                          {
        this.#ensureActive();
        const request = this.#parseBatchEditRequest(input);
        return await this.#enqueue(request.sessionId, () => {
            const session = this.#requireSession(request.sessionId);
            session.lastAccess = this.#clock();
            return this.#applyBatch(session, request, beforeCommit);
        });
    }

    async editStructure(input         , beforeCommit                         )                          {
        this.#ensureActive();
        const request = this.#parseStructureEditRequest(input);
        return await this.#enqueue(request.sessionId, () => {
            const session = this.#requireSession(request.sessionId);
            session.lastAccess = this.#clock();
            return this.#applyStructureEdit(session, request, beforeCommit);
        });
    }

    /** Trusted server-side snapshot for native preview and persistence. */
    async emit(sessionId        , expectedRevision        )                              {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        requiredRevision(expectedRevision);
        return await this.#enqueue(sessionId, () => {
            const session = this.#requireSession(sessionId);
            session.lastAccess = this.#clock();
            if (session.revision !== expectedRevision) {
                throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
            }
            return this.#createEmission(session);
        });
    }

    /** Called only after the emitted file bytes have been durably overwritten. */
    async markPersisted(sessionId        , expectedRevision        )                {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        requiredRevision(expectedRevision);
        await this.#enqueue(sessionId, () => {
            const session = this.#requireSession(sessionId);
            if (session.revision !== expectedRevision) {
                throw new DatSessionError("revision-conflict", "The DAT session changed while it was being saved.");
            }
            session.persistedRevision = session.revision;
            session.lastAccess = this.#clock();
        });
    }

    async reload(input         )                          {
        this.#ensureActive();
        const request = this.#parseReloadRequest(input);
        return await this.#enqueue(request.sessionId, async () => {
            const session = this.#requireSession(request.sessionId);
            session.lastAccess = this.#clock();
            if (session.revision !== request.expectedRevision) {
                throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
            }
            const lifecycleVersion = this.#lifecycleVersion;
            let prepared;
            try {
                prepared = await this.#registry.prepareDocumentRefresh(session.documentId);
            } catch (error) {
                throw new DatSessionError("reload-failed", "The DAT document could not be refreshed safely.", { cause: error });
            }
            this.#ensureLifecycle(lifecycleVersion);
            const nextLoadedBytes = this.#loadedBytes - session.loadedBytes + prepared.snapshot.bytes.length;
            if (nextLoadedBytes > this.#limits.maxLoadedBytes) {
                throw new DatSessionError("byte-limit", "The loaded DAT byte limit has been reached.");
            }
            const analyzed = this.#analyze(prepared.snapshot.bytes, session.format, "reload-failed");
            const replacement = this.#createState(
                session.sessionId,
                session.documentId,
                prepared.snapshot.bytes.length,
                session.revision + 1,
                analyzed,
            );
            const view = this.#createView(replacement);
            this.#ensureLifecycle(lifecycleVersion);
            try {
                prepared.commit();
            } catch (error) {
                throw new DatSessionError("reload-failed", "The DAT document refresh could not commit safely.", { cause: error });
            }
            this.#ensureLifecycle(lifecycleVersion);
            this.#loadedBytes = nextLoadedBytes;
            this.#sessions.set(session.sessionId, replacement);
            return view;
        });
    }

    async close(sessionId        )                   {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        await this.#queues.get(sessionId);
        this.#ensureActive();
        const session = this.#sessions.get(sessionId);
        if (session === undefined) return false;
        this.#release(session, false);
        return true;
    }

    sweepExpired()         {
        return this.sweepExpiredSessionIds().length;
    }

    sweepExpiredSessionIds()           {
        this.#ensureActive();
        const now = this.#clock();
        const expiredSessionIds           = [];
        for (const session of [...this.#sessions.values()]) {
            if (this.#queues.has(session.sessionId)) continue;
            if (now - session.lastAccess < this.#limits.idleTtlMs) continue;
            this.#release(session, true);
            expiredSessionIds.push(session.sessionId);
        }
        return expiredSessionIds;
    }

    dispose()       {
        if (this.#disposed) return;
        this.#disposed = true;
        this.#lifecycleVersion += 1;
        this.#sessions.clear();
        this.#queues.clear();
        this.#expiredSessionIds.clear();
        this.#loadedBytes = 0;
    }

    #analyze(
        bytes            ,
        format                ,
        failureCode                                     ,
    )                   {
        if (format === "encrypted" && bytes.length <= DAT_ENVELOPE_PREFIX_LENGTH) {
            throw new DatSessionError(failureCode, "The encrypted DAT envelope is too short.");
        }
        const document = format === "encrypted"
            ? LosslessDatDocument.fromEncrypted(bytes)
            : LosslessDatDocument.fromPlaintext(bytes);
        const descriptors = collectFields(document, failureCode);
        if (descriptors.length === 0) {
            throw new DatSessionError(failureCode, "The selected bytes contain no editable DAT fields for the declared format.");
        }
        if (descriptors.length > this.#limits.maxFieldsPerSession) {
            throw new DatSessionError("field-limit", "The DAT field capability limit has been reached.");
        }
        const structureCount = document.cst.frames.reduce((count, frame) => (
            count + 1 + frame.blocks.length
        ), 0);
        if (descriptors.length + structureCount > this.#limits.maxFieldsPerSession) {
            throw new DatSessionError("field-limit", "The DAT capability limit has been reached.");
        }
        for (const descriptor of descriptors) {
            this.#boundedString(descriptor.field.key);
            if (typeof descriptor.value === "string") this.#boundedString(descriptor.value);
        }
        return { document, format, descriptors };
    }

    #createState(
        sessionId        ,
        documentId        ,
        loadedBytes        ,
        revision        ,
        analyzed                  ,
    )               {
        const fields = new Map                         ();
        const fieldOrder           = [];
        for (const descriptor of analyzed.descriptors) {
            const fieldId = this.#newId();
            fields.set(fieldId, { ...descriptor, fieldId, currentValue: descriptor.value });
            fieldOrder.push(fieldId);
        }
        const structures = new Map                             ();
        const structureOrder           = [];
        const blocksByFrame = new Map                               ();
        for (const frame of analyzed.document.cst.frames) {
            if (frame.frameId < 0 || frame.frameId >= 600) continue;
            const frameCapabilityId = this.#newId();
            structures.set(frameCapabilityId, {
                capabilityId: frameCapabilityId,
                locator: { kind: "frame", frameOccurrence: frame.occurrence },
                canCopy: canCopyFrame(analyzed.document.cst, frame),
                canDelete: canDeleteFrame(frame),
            });
            structureOrder.push(frameCapabilityId);
            for (const block of frame.blocks) {
                const blockCapabilityId = this.#newId();
                structures.set(blockCapabilityId, {
                    capabilityId: blockCapabilityId,
                    locator: {
                        kind: "block",
                        frameOccurrence: frame.occurrence,
                        blockType: block.type,
                        blockIndex: block.index,
                    },
                    canCopy: canCopyBlock(analyzed.document.cst, block),
                    canDelete: canDeleteBlock(block),
                });
                structureOrder.push(blockCapabilityId);
                const blocks = blocksByFrame.get(frame.occurrence);
                if (blocks) blocks.push(structures.get(blockCapabilityId) );
                else blocksByFrame.set(frame.occurrence, [structures.get(blockCapabilityId) ]);
            }
        }
        return {
            sessionId,
            documentId,
            document: analyzed.document,
            format: analyzed.format,
            revision,
            persistedRevision: revision,
            loadedBytes,
            lastAccess: this.#clock(),
            fields,
            fieldOrder,
            structures,
            structureOrder,
            blocksByFrame,
        };
    }

    #createView(session              )                 {
        const fields = session.fieldOrder.map((fieldId) => this.#copyField(session.fields.get(fieldId) ));
        const framesByOccurrence = new Map(session.document.cst.frames.map((frame) => [frame.occurrence, frame]));
        const view                 = {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.revision !== session.persistedRevision,
            format: session.format,
            encrypted: session.format === "encrypted",
            fields,
            structureCapabilities: session.structureOrder.flatMap((capabilityId) => {
                const capability = session.structures.get(capabilityId) ;
                if (capability.locator.kind !== "frame") return [];
                const blocks = (session.blocksByFrame.get(capability.locator.frameOccurrence) ?? []).map((block) => {
                    if (block.locator.kind !== "block") {
                        throw new DatSessionError("view-limit", "The DAT block capability cannot be represented safely.");
                    }
                    return {
                        capabilityId: block.capabilityId,
                        blockType: block.locator.blockType,
                        blockIndex: block.locator.blockIndex,
                        canCopy: block.canCopy,
                        canDelete: block.canDelete,
                    };
                });
                const frame = framesByOccurrence.get(capability.locator.frameOccurrence);
                if (frame === undefined) {
                    throw new DatSessionError("view-limit", "The DAT frame capability cannot be represented safely.");
                }
                return [{
                    capabilityId: capability.capabilityId,
                    frameId: frame.frameId,
                    occurrence: frame.occurrence,
                    canCopy: capability.canCopy,
                    canDelete: capability.canDelete,
                    blocks,
                }];
            }),
            projection: this.#copyProjection(session.document),
            diagnostics: this.#copyDiagnostics(session.document.diagnostics),
        };
        let encoded        ;
        try {
            encoded = JSON.stringify(view);
        } catch (error) {
            throw new DatSessionError("view-limit", "The DAT session view cannot be encoded safely.", { cause: error });
        }
        if (Buffer.byteLength(encoded, "utf8") > this.#limits.maxViewBytes) {
            throw new DatSessionError("view-limit", "The DAT session view exceeds the response limit.");
        }
        return view;
    }

    #copyField(capability                 )                      {
        if (capability.kind === "integer-pair") {
            const key = capability.field.key;
            const value = capability.currentValue;
            if ((key !== "catchingact" && key !== "caughtact") || !Array.isArray(value) || value.length !== 2) {
                throw new DatSessionError("view-limit", "The DAT pair field cannot be represented safely.");
            }
            return {
                fieldId: capability.fieldId,
                key,
                kind: "integer-pair",
                value: [value[0] , value[1] ]         ,
                ...capability.location,
            };
        }
        const value = capability.currentValue;
        if (typeof value !== "number" && typeof value !== "string") {
            throw new DatSessionError("view-limit", "The DAT scalar field cannot be represented safely.");
        }
        return {
            fieldId: capability.fieldId,
            key: capability.field.key,
            kind: capability.kind,
            value,
            ...(capability.kind === "number" ? { numericKind: capability.numericKind } : {}),
            ...capability.location,
        };
    }

    #copyDiagnostics(values                           )                             {
        return values.slice(0, this.#limits.maxDiagnostics).map((value) => ({
            code: value.code,
            severity: value.severity,
            message: value.message.slice(0, this.#limits.maxDiagnosticMessageLength),
        }));
    }

    #copyProjection(document                     )                           {
        const value = document.projection;
        const stringValue = (candidate        )         => this.#boundedString(candidate);
        const projection                           = {
            top: copyTop(value.top, stringValue),
            spriteRanges: value.spriteRanges.map((range)                        => ({
                frameLo: range.frameLo,
                frameHi: range.frameHi,
                file: stringValue(range.file),
                w: range.w,
                h: range.h,
                row: range.row,
                col: range.col,
            })),
            frames: value.frames.map((frame) => copyFrame(frame, stringValue)),
        };
        let encoded        ;
        try {
            encoded = JSON.stringify(projection);
        } catch (error) {
            throw new DatSessionError("view-limit", "The DAT projection cannot be encoded safely.", { cause: error });
        }
        if (Buffer.byteLength(encoded, "utf8") > this.#limits.maxProjectionBytes) {
            throw new DatSessionError("view-limit", "The DAT projection exceeds the response limit.");
        }
        return projection;
    }

    #boundedString(value        )         {
        if (!isLatin1ScalarString(value) || Buffer.byteLength(value, "latin1") > this.#limits.maxStringBytes) {
            throw new DatSessionError("view-limit", "A DAT string exceeds the response limit.");
        }
        return value;
    }

    #parseEditRequest(input         )                        {
        const record = exactRecord(input, ["sessionId", "fieldId", "value", "expectedRevision"]);
        const value = this.#parseEditValue(record.value);
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            fieldId: requiredId(record.fieldId, "fieldId"),
            value,
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    #parseEditValue(value         )                       {
        if (Array.isArray(value)) {
            if (value.length !== 2 || !value.every(isSignedInt32)) {
                throw new DatSessionError("invalid-request", "Pair value must contain two signed 32-bit integers.");
            }
            return [value[0] , value[1] ]         ;
        } else if ((typeof value !== "number" && typeof value !== "string")
            || (typeof value === "number" && !Number.isFinite(value))
            || (typeof value === "string" && (!isLatin1ScalarString(value) || Buffer.byteLength(value, "latin1") > this.#limits.maxStringBytes))) {
            throw new DatSessionError("invalid-request", "value must be a bounded finite number, pair, or single-line Latin-1 string.");
        }
        return value;
    }

    #parseBatchEditRequest(input         )                             {
        const record = exactRecord(input, ["sessionId", "edits", "expectedRevision"]);
        if (!Array.isArray(record.edits) || record.edits.length < 1 || record.edits.length > 16) {
            throw new DatSessionError("invalid-request", "edits must contain between 1 and 16 items.");
        }
        const fieldIds = new Set        ();
        const edits                            = record.edits.map((value) => {
            const item = exactRecord(value, ["fieldId", "value"]);
            const fieldId = requiredId(item.fieldId, "fieldId");
            if (fieldIds.has(fieldId)) {
                throw new DatSessionError("invalid-request", "A batch cannot edit the same field capability twice.");
            }
            fieldIds.add(fieldId);
            return { fieldId, value: this.#parseEditValue(item.value) };
        });
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            edits,
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    #parseStructureEditRequest(input         )                                 {
        if (typeof input !== "object" || input === null || Array.isArray(input)) {
            throw new DatSessionError("invalid-request", "The request must be an object.");
        }
        const record = input                           ;
        const supported                                          = [
            "copy-frame", "delete-frame", "create-block", "copy-block", "delete-block",
        ];
        const operation = supported.find((candidate) => candidate === record.operation);
        if (operation === undefined) {
            throw new DatSessionError("invalid-request", "The structure operation is unsupported.");
        }
        exactRecord(input, operation === "copy-frame"
            ? ["sessionId", "capabilityId", "operation", "newFrameId", "expectedRevision"]
            : ["sessionId", "capabilityId", "operation", "expectedRevision"]);
        let newFrameId                    ;
        if (operation === "copy-frame") {
            if (!Number.isSafeInteger(record.newFrameId)
                || (record.newFrameId          ) < 0
                || (record.newFrameId          ) >= 600) {
                throw new DatSessionError("invalid-request", "newFrameId must be an integer from 0 through 599.");
            }
            newFrameId = record.newFrameId          ;
        }
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            capabilityId: requiredId(record.capabilityId, "capabilityId"),
            operation,
            ...(newFrameId === undefined ? {} : { newFrameId }),
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    async #applyBatch(
        session              ,
        request                            ,
        beforeCommit                         ,
    )                          {
        if (session.revision !== request.expectedRevision) {
            throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
        }
        const resolved = request.edits.map((edit) => {
            const capability = session.fields.get(edit.fieldId);
            if (capability === undefined) {
                throw new DatSessionError("unknown-field", "A field capability is unknown for this session epoch.");
            }
            if (capability.kind === "integer-pair") {
                if (!Array.isArray(edit.value)) {
                    throw new DatSessionError("invalid-request", "The pair field requires two integer values.");
                }
            } else if (Array.isArray(edit.value) || capability.kind !== typeof edit.value) {
                throw new DatSessionError("invalid-request", "The scalar value type does not match the field capability.");
            }
            if (capability.kind === "number"
                && capability.numericKind === "integer"
                && !isSignedInt32(edit.value)) {
                throw new DatSessionError("invalid-request", "The numeric field requires a signed 32-bit integer.");
            }
            return { capability, value: edit.value };
        });
        const previousRevision = session.revision;
        const previousLoadedBytes = session.loadedBytes;
        const previousValues = resolved.map(({ capability }) => capability.currentValue);
        const savepoint = session.document.createPatchSavepoint();
        let changed = false;
        try {
            for (const { capability, value } of resolved) {
                if (sameFieldValue(capability.currentValue, value)) continue;
                const applied = capability.kind === "integer-pair"
                    ? session.document.apply(createSetIntegerPairCommand(
                        "dat-session-batch-pair-edit",
                        capability.field,
                        value                             ,
                    ))
                    : session.document.apply(createSetScalarCommand(
                        "dat-session-batch-scalar-edit",
                        capability.field,
                        value                   ,
                    ));
                if (!applied.applied && applied.diagnostics.length > 0) {
                    throw new DatSessionError("invalid-request", "A batch field edit is not supported.");
                }
                changed ||= applied.applied;
                capability.currentValue = Array.isArray(value)
                    ? [value[0] , value[1] ]         
                    : value;
            }
            if (!changed) {
                const view = this.#createView(session);
                await beforeCommit?.(view, this.#createEmission(session));
                return view;
            }
            const nextLoadedBytes = session.document.emitFile().length;
            const totalLoadedBytes = this.#loadedBytes - session.loadedBytes + nextLoadedBytes;
            if (totalLoadedBytes > this.#limits.maxLoadedBytes) {
                throw new DatSessionError("byte-limit", "The edited DAT exceeds the loaded byte limit.");
            }
            session.revision += 1;
            session.loadedBytes = nextLoadedBytes;
            const view = this.#createView(session);
            await beforeCommit?.(view, this.#createEmission(session));
            this.#loadedBytes = totalLoadedBytes;
            return view;
        } catch (error) {
            session.document.restorePatchSavepoint(savepoint);
            resolved.forEach(({ capability }, index) => {
                capability.currentValue = previousValues[index] ;
            });
            session.revision = previousRevision;
            session.loadedBytes = previousLoadedBytes;
            throw error;
        }
    }

    async #applyStructureEdit(
        session              ,
        request                                ,
        beforeCommit                         ,
    )                          {
        if (session.revision !== request.expectedRevision) {
            throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
        }
        const capability = session.structures.get(request.capabilityId);
        if (capability === undefined) {
            throw new DatSessionError("unknown-field", "The structure capability is unknown for this session epoch.");
        }
        const frameOperation = request.operation === "copy-frame" || request.operation === "delete-frame";
        if ((capability.locator.kind === "frame") !== frameOperation) {
            throw new DatSessionError("invalid-request", "The structure operation does not match its capability.");
        }
        if ((request.operation === "copy-frame"
                || request.operation === "copy-block"
                || request.operation === "create-block")
            && !capability.canCopy) {
            throw new DatSessionError("invalid-request", "The selected structure cannot be copied safely.");
        }
        if ((request.operation === "delete-frame" || request.operation === "delete-block")
            && !capability.canDelete) {
            throw new DatSessionError("invalid-request", "The selected structure cannot be deleted safely.");
        }
        const current = session.document.withPlaintext(session.document.emitPlaintext());
        let plaintext        ;
        try {
            plaintext = applyDatStructureEdit(current.cst, request.operation === "copy-frame"
                ? {
                    operation: request.operation,
                    target: capability.locator                                                   ,
                    newFrameId: request.newFrameId ,
                }
                : {
                    operation: request.operation,
                    target: capability.locator         ,
                });
        } catch (error) {
            throw new DatSessionError("invalid-request", "The lossless structure edit could not be applied.", { cause: error });
        }
        const nextDocument = current.withPlaintext(plaintext);
        const nextFile = nextDocument.emitFile();
        const nextLoadedBytes = this.#loadedBytes - session.loadedBytes + nextFile.length;
        if (nextLoadedBytes > this.#limits.maxLoadedBytes) {
            throw new DatSessionError("byte-limit", "The edited DAT exceeds the loaded byte limit.");
        }
        const analyzed = this.#analyze(nextFile, session.format, "invalid-request");
        const replacement = this.#createState(
            session.sessionId,
            session.documentId,
            nextFile.length,
            session.revision + 1,
            analyzed,
        );
        replacement.persistedRevision = session.persistedRevision;
        const view = this.#createView(replacement);
        await beforeCommit?.(view, this.#createEmission(replacement));
        this.#loadedBytes = nextLoadedBytes;
        this.#sessions.set(session.sessionId, replacement);
        return view;
    }

    #createEmission(session              )                     {
        return {
            sessionId: session.sessionId,
            documentId: session.documentId,
            revision: session.revision,
            dirty: session.revision !== session.persistedRevision,
            plaintext: Buffer.from(session.document.emitPlaintext()),
            file: Buffer.from(session.document.emitFile()),
        };
    }

    #parseReloadRequest(input         )                          {
        const record = exactRecord(input, ["sessionId", "expectedRevision"]);
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    #enqueue   (sessionId        , operation                      )             {
        const previous = this.#queues.get(sessionId) ?? Promise.resolve();
        const guardedOperation = ()                 => {
            this.#ensureActive();
            return operation();
        };
        const result = previous.then(guardedOperation, guardedOperation);
        const tail = result.then(() => undefined, () => undefined);
        this.#queues.set(sessionId, tail);
        void tail.then(() => {
            if (this.#queues.get(sessionId) === tail) this.#queues.delete(sessionId);
        });
        return result;
    }

    #requireSession(sessionId        )               {
        const session = this.#sessions.get(sessionId);
        if (session === undefined) {
            if (this.#expiredSessionIds.has(sessionId)) {
                throw new DatSessionError("expired", "The DAT session has expired.");
            }
            throw new DatSessionError("unknown-session", "The DAT session is unknown.");
        }
        if (this.#clock() - session.lastAccess >= this.#limits.idleTtlMs) {
            this.#release(session, true);
            throw new DatSessionError("expired", "The DAT session has expired.");
        }
        return session;
    }

    #release(session              , expired         )       {
        if (!this.#sessions.delete(session.sessionId)) return;
        this.#loadedBytes -= session.loadedBytes;
        if (expired) {
            this.#expiredSessionIds.delete(session.sessionId);
            this.#expiredSessionIds.set(session.sessionId, this.#clock());
            while (this.#expiredSessionIds.size > this.#limits.maxSessions) {
                const oldest = this.#expiredSessionIds.keys().next().value                      ;
                if (oldest === undefined) break;
                this.#expiredSessionIds.delete(oldest);
            }
        }
    }

    #newId()         {
        for (let attempt = 0; attempt < 128; attempt += 1) {
            const randomPart = this.#idFactory();
            if (typeof randomPart !== "string" || !/^[A-Za-z0-9_-]{32,128}$/.test(randomPart)) continue;
            if (this.#nextIdSequence >= Number.MAX_SAFE_INTEGER) {
                throw new DatSessionError("invalid-request", "The opaque ID sequence is exhausted.");
            }
            this.#nextIdSequence += 1;
            const suffix = this.#nextIdSequence.toString(36);
            const boundedRandomPart = randomPart.slice(0, 128 - suffix.length - 1);
            return `${boundedRandomPart}-${suffix}`;
        }
        throw new DatSessionError("invalid-request", "The opaque ID source failed to produce a unique identifier.");
    }

    #clock()         {
        const value = this.#now();
        if (!Number.isFinite(value)) throw new DatSessionError("invalid-request", "The session clock is invalid.");
        return value;
    }

    #ensureActive()       {
        if (this.#disposed) throw new DatSessionError("invalid-request", "The DAT session service is disposed.");
    }

    #ensureLifecycle(expectedVersion        )       {
        this.#ensureActive();
        if (this.#lifecycleVersion !== expectedVersion) {
            throw new DatSessionError("invalid-request", "The DAT session service lifecycle changed during the operation.");
        }
    }
}
