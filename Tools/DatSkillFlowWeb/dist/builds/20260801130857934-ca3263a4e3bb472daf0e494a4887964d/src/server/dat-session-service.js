// dat-skill-flow-build:20260801130857934-ca3263a4e3bb472daf0e494a4887964d
import { randomBytes } from "node:crypto";

import { createSetScalarCommand, LosslessDatDocument } from "../model/dat-document.js";
             
                  
                     
                     
                       
                     
                  
                     
                          
                     
                                    
                                                                       
                                                                   
             
                             
                          
                        
                         
                        
                             
                            
                             
                   
                                   
import { WorkspaceRegistry } from "./workspace-registry.js";

             
                             
                          
                        
                         
                        
                             
                            
                             
                   
                                   

export const DEFAULT_DAT_SESSION_LIMITS = Object.freeze({
    maxSessions: 32,
    maxFieldsPerSession: 50_000,
    maxLoadedBytes: 64 * 1024 * 1024,
    idleTtlMs: 15 * 60 * 1_000,
    maxDiagnostics: 200,
    maxDiagnosticMessageLength: 512,
    maxProjectionBytes: 2 * 1024 * 1024,
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

                         
                                
                       
                              
                     
                             
                             
                        
 

                           
                       
                              
                           
                            
 

                                                   
                    
                                  
 

                        
                      
                       
                                  
                       
                     
                        
                       
                                         
                         
 

                            
                                  
                       
                                   
 

                  
                        
                                
                           
                      
                           
                                       
                               
                           
 

function positiveSafeInteger(value        , name        )         {
    if (!Number.isSafeInteger(value) || value < 1) throw new RangeError(`${name} must be a positive safe integer.`);
    return value;
}

function nonnegativeSafeInteger(value        , name        )         {
    if (!Number.isSafeInteger(value) || value < 0) throw new RangeError(`${name} must be a nonnegative safe integer.`);
    return value;
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
    if (typeof value !== "string" || value.length === 0 || value.includes("\0")) {
        throw new DatSessionError("invalid-request", `${name} must be a nonempty opaque ID.`);
    }
    return value;
}

function requiredRevision(value         )         {
    if (!Number.isSafeInteger(value) || (value          ) < 0) {
        throw new DatSessionError("invalid-request", "expectedRevision must be a nonnegative safe integer.");
    }
    return value          ;
}

function structureScore(document                     )         {
    const cst = document.cst;
    let score = cst.topFields.length + cst.spriteRanges.length * 4;
    for (const frame of cst.frames) {
        score += 8 + frame.fields.length;
        for (const block of frame.blocks) score += 2 + block.fields.length;
    }
    return score;
}

function detectDatDocument(bytes            )                                                        {
    const plaintext = LosslessDatDocument.fromPlaintext(bytes);
    if (bytes.length <= 123) return { document: plaintext, encrypted: false };
    const encrypted = LosslessDatDocument.fromEncrypted(bytes);
    const plaintextScore = structureScore(plaintext);
    const encryptedScore = structureScore(encrypted);
    return encryptedScore > plaintextScore && encryptedScore > 0
        ? { document: encrypted, encrypted: true }
        : { document: plaintext, encrypted: false };
}

function fieldValue(field             )                              {
    if (field.scalarKind === "number") return field.numericValue;
    if (field.scalarKind === "string") return field.rawValue.toString("latin1");
    return undefined;
}

function collectFields(document                     )                    {
    const descriptors                    = [];
    const append = (fields                        , base                                   )       => {
        const occurrences = new Map                ();
        for (const field of fields) {
            const occurrence = occurrences.get(field.key) ?? 0;
            occurrences.set(field.key, occurrence + 1);
            const value = fieldValue(field);
            if (value === undefined || (field.scalarKind !== "number" && field.scalarKind !== "string")) continue;
            descriptors.push({ field, kind: field.scalarKind, value, location: { ...base, occurrence } });
        }
    };
    append(document.cst.topFields, { scope: "top" });
    for (let rangeIndex = 0; rangeIndex < document.cst.spriteRanges.length; rangeIndex += 1) {
        const range = document.cst.spriteRanges[rangeIndex] ;
        append([range.fileField, ...range.fields], { scope: "sprite", spriteRangeIndex: rangeIndex });
    }
    for (const frame of document.cst.frames) {
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
        frameId: value.frameId, occurrence: value.occurrence, pic: value.pic, state: value.state, wait: value.wait,
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
             #issuedIds = new Set        ();
             #expiredSessionIds = new Set        ();
    #loadedBytes = 0;
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
            maxStringBytes: positiveSafeInteger(options.maxStringBytes ?? DEFAULT_DAT_SESSION_LIMITS.maxStringBytes, "maxStringBytes"),
        };
        this.#now = options.now ?? Date.now;
        this.#idFactory = options.idFactory ?? (() => randomBytes(32).toString("base64url"));
    }

    async openDocument(documentId        )                          {
        this.#ensureActive();
        requiredId(documentId, "documentId");
        if (this.#sessions.size >= this.#limits.maxSessions) {
            throw new DatSessionError("session-limit", "The DAT session limit has been reached.");
        }
        let read;
        try {
            read = await this.#registry.readDocument(documentId);
        } catch (error) {
            throw new DatSessionError("invalid-request", "The document cannot be opened as a DAT session.", { cause: error });
        }
        if (this.#loadedBytes + read.bytes.length > this.#limits.maxLoadedBytes) {
            throw new DatSessionError("byte-limit", "The loaded DAT byte limit has been reached.");
        }
        const analyzed = this.#analyze(read.bytes);
        const sessionId = this.#newId();
        const session = this.#createState(sessionId, documentId, read.bytes.length, 0, analyzed);
        const view = this.#createView(session);
        this.#sessions.set(sessionId, session);
        this.#loadedBytes += read.bytes.length;
        return view;
    }

    async edit(input         )                          {
        this.#ensureActive();
        const request = this.#parseEditRequest(input);
        return await this.#enqueue(request.sessionId, () => {
            const session = this.#requireSession(request.sessionId);
            session.lastAccess = this.#clock();
            if (session.revision !== request.expectedRevision) {
                throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
            }
            const capability = session.fields.get(request.fieldId);
            if (capability === undefined) {
                throw new DatSessionError("unknown-field", "The field capability is unknown for this session epoch.");
            }
            if (capability.kind !== typeof request.value) {
                throw new DatSessionError("invalid-request", "The scalar value type does not match the field capability.");
            }
            if (capability.currentValue === request.value) return this.#createView(session);

            const previousValue = capability.currentValue;
            const applied = session.document.apply(createSetScalarCommand("dat-session-edit", capability.field, request.value));
            if (!applied.applied) {
                if (applied.diagnostics.length === 0) return this.#createView(session);
                throw new DatSessionError("invalid-request", "The scalar edit is not supported.");
            }
            const previousRevision = session.revision;
            capability.currentValue = request.value;
            session.revision += 1;
            try {
                return this.#createView(session);
            } catch (error) {
                session.document.apply(createSetScalarCommand("dat-session-edit-rollback", capability.field, previousValue));
                capability.currentValue = previousValue;
                session.revision = previousRevision;
                throw error;
            }
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
            let prepared;
            try {
                prepared = await this.#registry.prepareDocumentRefresh(session.documentId);
            } catch (error) {
                throw new DatSessionError("reload-failed", "The DAT document could not be refreshed safely.", { cause: error });
            }
            const nextLoadedBytes = this.#loadedBytes - session.loadedBytes + prepared.snapshot.bytes.length;
            if (nextLoadedBytes > this.#limits.maxLoadedBytes) {
                throw new DatSessionError("byte-limit", "The loaded DAT byte limit has been reached.");
            }
            const analyzed = this.#analyze(prepared.snapshot.bytes);
            const replacement = this.#createState(
                session.sessionId,
                session.documentId,
                prepared.snapshot.bytes.length,
                session.revision + 1,
                analyzed,
            );
            const view = this.#createView(replacement);
            try {
                prepared.commit();
            } catch (error) {
                throw new DatSessionError("reload-failed", "The DAT document refresh could not commit safely.", { cause: error });
            }
            this.#loadedBytes = nextLoadedBytes;
            this.#sessions.set(session.sessionId, replacement);
            return view;
        });
    }

    async close(sessionId        )                   {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        await this.#queues.get(sessionId);
        const session = this.#sessions.get(sessionId);
        if (session === undefined) return false;
        this.#release(session, false);
        return true;
    }

    sweepExpired()         {
        this.#ensureActive();
        const now = this.#clock();
        let count = 0;
        for (const session of [...this.#sessions.values()]) {
            if (this.#queues.has(session.sessionId)) continue;
            if (now - session.lastAccess < this.#limits.idleTtlMs) continue;
            this.#release(session, true);
            count += 1;
        }
        return count;
    }

    dispose()       {
        if (this.#disposed) return;
        this.#disposed = true;
        this.#sessions.clear();
        this.#queues.clear();
        this.#expiredSessionIds.clear();
        this.#issuedIds.clear();
        this.#loadedBytes = 0;
    }

    #analyze(bytes            )                   {
        const detected = detectDatDocument(bytes);
        const descriptors = collectFields(detected.document);
        if (descriptors.length > this.#limits.maxFieldsPerSession) {
            throw new DatSessionError("field-limit", "The DAT field capability limit has been reached.");
        }
        for (const descriptor of descriptors) {
            this.#boundedString(descriptor.field.key);
            if (typeof descriptor.value === "string") this.#boundedString(descriptor.value);
        }
        this.#copyProjection(detected.document);
        return { ...detected, descriptors };
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
        return {
            sessionId,
            documentId,
            document: analyzed.document,
            encrypted: analyzed.encrypted,
            revision,
            loadedBytes,
            lastAccess: this.#clock(),
            fields,
            fieldOrder,
        };
    }

    #createView(session              )                 {
        const fields = session.fieldOrder.map((fieldId) => this.#copyField(session.fields.get(fieldId) ));
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            encrypted: session.encrypted,
            fields,
            projection: this.#copyProjection(session.document),
            diagnostics: this.#copyDiagnostics(session.document.diagnostics),
        };
    }

    #copyField(capability                 )                      {
        const view   
                            
                        
                                      
                                   
                                        
                                
                                      
                             
                                     
                                     
                                
          = {
            fieldId: capability.fieldId,
            key: capability.field.key,
            kind: capability.kind,
            value: capability.currentValue,
            scope: capability.location.scope,
            occurrence: capability.location.occurrence,
        };
        if (capability.location.spriteRangeIndex !== undefined) view.spriteRangeIndex = capability.location.spriteRangeIndex;
        if (capability.location.frameId !== undefined) view.frameId = capability.location.frameId;
        if (capability.location.frameOccurrence !== undefined) view.frameOccurrence = capability.location.frameOccurrence;
        if (capability.location.blockType !== undefined) view.blockType = capability.location.blockType;
        if (capability.location.blockIndex !== undefined) view.blockIndex = capability.location.blockIndex;
        return view;
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
        if (typeof value !== "string" || Buffer.byteLength(value, "utf8") > this.#limits.maxStringBytes) {
            throw new DatSessionError("view-limit", "A DAT string exceeds the response limit.");
        }
        return value;
    }

    #parseEditRequest(input         )                        {
        const record = exactRecord(input, ["sessionId", "fieldId", "value", "expectedRevision"]);
        const value = record.value;
        if ((typeof value !== "number" && typeof value !== "string")
            || (typeof value === "number" && !Number.isFinite(value))
            || (typeof value === "string" && (/\0|\r|\n/.test(value) || Buffer.byteLength(value, "utf8") > this.#limits.maxStringBytes))) {
            throw new DatSessionError("invalid-request", "value must be a bounded finite number or single-line NUL-free string.");
        }
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            fieldId: requiredId(record.fieldId, "fieldId"),
            value,
            expectedRevision: requiredRevision(record.expectedRevision),
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
        const result = previous.then(operation, operation);
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
        if (expired) this.#expiredSessionIds.add(session.sessionId);
    }

    #newId()         {
        for (let attempt = 0; attempt < 128; attempt += 1) {
            const value = this.#idFactory();
            if (typeof value !== "string" || !/^[A-Za-z0-9_-]{32,}$/.test(value) || this.#issuedIds.has(value)) continue;
            this.#issuedIds.add(value);
            this.#expiredSessionIds.delete(value);
            return value;
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
}
