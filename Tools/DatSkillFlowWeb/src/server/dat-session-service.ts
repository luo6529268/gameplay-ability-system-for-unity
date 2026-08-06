import { randomBytes } from "node:crypto";

import {
    createSetScalarCommand,
    isLatin1ScalarString,
    LosslessDatDocument,
} from "../model/dat-document.js";
import type {
    BdyProjection,
    BPointProjection,
    CPointProjection,
    DatFrameProjection,
    DatTopProjection,
    ItrProjection,
    OPointProjection,
    SpriteRangeProjection,
    WPointProjection,
} from "../model/dat-projection.js";
import type { DatBlockType, DatFieldCst } from "../syntax/byte-cst.js";
import type { DataDiagnostic } from "../syntax/data-diagnostic.js";
import { DAT_ENVELOPE_PREFIX_LENGTH } from "../syntax/dat-envelope.js";
import type {
    DatInputFormat,
    DatSessionDiagnosticView,
    DatSessionEditRequest,
    DatSessionErrorCode,
    DatSessionFieldScope,
    DatSessionFieldView,
    DatSessionNumericKind,
    DatSessionProjectionView,
    DatSessionReloadRequest,
    DatSessionServiceOptions,
    DatSessionView,
} from "./dat-session-contract.js";
import { WorkspaceRegistry } from "./workspace-registry.js";

export type {
    DatInputFormat,
    DatSessionDiagnosticView,
    DatSessionEditRequest,
    DatSessionErrorCode,
    DatSessionFieldScope,
    DatSessionFieldView,
    DatSessionNumericKind,
    DatSessionProjectionView,
    DatSessionReloadRequest,
    DatSessionServiceOptions,
    DatSessionView,
} from "./dat-session-contract.js";

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
    readonly code: DatSessionErrorCode;

    constructor(code: DatSessionErrorCode, message: string, options?: ErrorOptions) {
        super(message, options);
        this.name = "DatSessionError";
        this.code = code;
    }
}

interface FieldLocation {
    scope: DatSessionFieldScope;
    occurrence: number;
    spriteRangeIndex?: number;
    frameId?: number;
    frameOccurrence?: number;
    blockType?: DatBlockType;
    blockIndex?: number;
}

interface FieldDescriptor {
    field: DatFieldCst;
    kind: "number" | "string";
    numericKind?: DatSessionNumericKind;
    value: number | string;
    location: FieldLocation;
}

interface FieldCapability extends FieldDescriptor {
    fieldId: string;
    currentValue: number | string;
}

interface SessionState {
    sessionId: string;
    documentId: string;
    document: LosslessDatDocument;
    format: DatInputFormat;
    revision: number;
    persistedRevision: number;
    loadedBytes: number;
    lastAccess: number;
    fields: Map<string, FieldCapability>;
    fieldOrder: string[];
}

export interface DatSessionEmission {
    readonly sessionId: string;
    readonly documentId: string;
    readonly revision: number;
    readonly dirty: boolean;
    readonly plaintext: Buffer;
    readonly file: Buffer;
}

interface AnalyzedDocument {
    document: LosslessDatDocument;
    format: DatInputFormat;
    descriptors: FieldDescriptor[];
}

interface Limits {
    maxSessions: number;
    maxFieldsPerSession: number;
    maxLoadedBytes: number;
    idleTtlMs: number;
    maxDiagnostics: number;
    maxDiagnosticMessageLength: number;
    maxProjectionBytes: number;
    maxViewBytes: number;
    maxStringBytes: number;
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
const BLOCK_INTEGER_FIELDS: Readonly<Record<DatBlockType, ReadonlySet<string>>> = {
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

const INT32_MIN = -2_147_483_648;
const INT32_MAX = 2_147_483_647;

function positiveSafeInteger(value: number, name: string): number {
    if (!Number.isSafeInteger(value) || value < 1) throw new RangeError(`${name} must be a positive safe integer.`);
    return value;
}

function nonnegativeSafeInteger(value: number, name: string): number {
    if (!Number.isSafeInteger(value) || value < 0) throw new RangeError(`${name} must be a nonnegative safe integer.`);
    return value;
}

function exactRecord(value: unknown, keys: readonly string[]): Record<string, unknown> {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        throw new DatSessionError("invalid-request", "The request must be an object.");
    }
    const record = value as Record<string, unknown>;
    const ownKeys = Object.keys(record);
    if (ownKeys.length !== keys.length
        || ownKeys.some((key) => !keys.includes(key))
        || keys.some((key) => !Object.hasOwn(record, key))) {
        throw new DatSessionError("invalid-request", "The request has missing or unknown fields.");
    }
    return record;
}

function requiredId(value: unknown, name: string): string {
    if (typeof value !== "string" || value.length === 0 || value.length > 128 || value.includes("\0")) {
        throw new DatSessionError("invalid-request", `${name} must be an opaque ID of at most 128 characters.`);
    }
    return value;
}

function requiredFormat(value: unknown): DatInputFormat {
    if (value !== "plaintext" && value !== "encrypted") {
        throw new DatSessionError("invalid-request", "format must be plaintext or encrypted.");
    }
    return value;
}

function requiredRevision(value: unknown): number {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < 0) {
        throw new DatSessionError("invalid-request", "expectedRevision must be a nonnegative safe integer.");
    }
    return value;
}

function authorityKind(field: DatFieldCst, location: Omit<FieldLocation, "occurrence">): "integer" | "number" | "string" | undefined {
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
    if (location.blockType === undefined || field.key === "catchingact" || field.key === "caughtact") return undefined;
    return BLOCK_INTEGER_FIELDS[location.blockType].has(field.key) ? "integer" : undefined;
}

function collectFields(
    document: LosslessDatDocument,
    failureCode: "invalid-request" | "reload-failed",
): FieldDescriptor[] {
    const descriptors: FieldDescriptor[] = [];
    const append = (fields: readonly DatFieldCst[], base: Omit<FieldLocation, "occurrence">): void => {
        const occurrences = new Map<string, number>();
        for (const field of fields) {
            const occurrence = occurrences.get(field.key) ?? 0;
            occurrences.set(field.key, occurrence + 1);
            const semanticKind = authorityKind(field, base);
            if (semanticKind === undefined) continue;
            let value: number | string;
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
                        || !Number.isInteger(numericValue) || numericValue < INT32_MIN || numericValue > INT32_MAX)) {
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
        const range = document.cst.spriteRanges[rangeIndex]!;
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

function copyTop(value: DatTopProjection, stringValue: (value: string) => string): DatTopProjection {
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

function copyItr(value: ItrProjection): ItrProjection {
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

function copyBdy(value: BdyProjection): BdyProjection {
    return { x: value.x, y: value.y, w: value.w, h: value.h };
}

function copyOpoint(value: OPointProjection): OPointProjection {
    return { kind: value.kind, x: value.x, y: value.y, action: value.action, dvx: value.dvx, dvy: value.dvy, oid: value.oid, facing: value.facing };
}

function copyWpoint(value: WPointProjection): WPointProjection {
    return { kind: value.kind, x: value.x, y: value.y, attacking: value.attacking, cover: value.cover, weaponact: value.weaponact, dvx: value.dvx, dvy: value.dvy, dvz: value.dvz };
}

function copyBpoint(value: BPointProjection): BPointProjection {
    return { x: value.x, y: value.y };
}

function copyCpoint(value: CPointProjection): CPointProjection {
    return {
        kind: value.kind, x: value.x, y: value.y, injury: value.injury, cover: value.cover,
        vaction: value.vaction, aaction: value.aaction, jaction: value.jaction, daction: value.daction,
        taction: value.taction, throwvx: value.throwvx, throwvy: value.throwvy, throwvz: value.throwvz,
        throwinjury: value.throwinjury, hurtable: value.hurtable, decrease: value.decrease,
        dircontrol: value.dircontrol, fronthurtact: value.fronthurtact, backhurtact: value.backhurtact,
    };
}

function copyFrame(value: DatFrameProjection, stringValue: (value: string) => string): DatFrameProjection {
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
    readonly #registry: WorkspaceRegistry;
    readonly #limits: Limits;
    readonly #now: () => number;
    readonly #idFactory: () => string;
    readonly #sessions = new Map<string, SessionState>();
    readonly #queues = new Map<string, Promise<void>>();
    readonly #expiredSessionIds = new Map<string, number>();
    #loadedBytes = 0;
    #pendingOpens = 0;
    #nextIdSequence = 0;
    #lifecycleVersion = 0;
    #disposed = false;

    constructor(registry: WorkspaceRegistry, options: DatSessionServiceOptions = {}) {
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
    async openDocument(documentId: string, inputFormat: DatInputFormat): Promise<DatSessionView> {
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

    async edit(input: unknown): Promise<DatSessionView> {
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
            if (capability.kind === "number" && capability.numericKind === "integer"
                && (!Number.isInteger(request.value) || request.value < INT32_MIN || request.value > INT32_MAX)) {
                throw new DatSessionError("invalid-request", "The numeric field requires a signed 32-bit integer.");
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

    /** Trusted server-side snapshot for native preview and persistence. */
    async emit(sessionId: string, expectedRevision: number): Promise<DatSessionEmission> {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        requiredRevision(expectedRevision);
        return await this.#enqueue(sessionId, () => {
            const session = this.#requireSession(sessionId);
            session.lastAccess = this.#clock();
            if (session.revision !== expectedRevision) {
                throw new DatSessionError("revision-conflict", "The DAT session revision is stale.");
            }
            return {
                sessionId,
                documentId: session.documentId,
                revision: session.revision,
                dirty: session.revision !== session.persistedRevision,
                plaintext: Buffer.from(session.document.emitPlaintext()),
                file: Buffer.from(session.document.emitFile()),
            };
        });
    }

    /** Called only after the emitted file bytes have been durably overwritten. */
    async markPersisted(sessionId: string, expectedRevision: number): Promise<void> {
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

    async reload(input: unknown): Promise<DatSessionView> {
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

    async close(sessionId: string): Promise<boolean> {
        this.#ensureActive();
        requiredId(sessionId, "sessionId");
        await this.#queues.get(sessionId);
        this.#ensureActive();
        const session = this.#sessions.get(sessionId);
        if (session === undefined) return false;
        this.#release(session, false);
        return true;
    }

    sweepExpired(): number {
        return this.sweepExpiredSessionIds().length;
    }

    sweepExpiredSessionIds(): string[] {
        this.#ensureActive();
        const now = this.#clock();
        const expiredSessionIds: string[] = [];
        for (const session of [...this.#sessions.values()]) {
            if (this.#queues.has(session.sessionId)) continue;
            if (now - session.lastAccess < this.#limits.idleTtlMs) continue;
            this.#release(session, true);
            expiredSessionIds.push(session.sessionId);
        }
        return expiredSessionIds;
    }

    dispose(): void {
        if (this.#disposed) return;
        this.#disposed = true;
        this.#lifecycleVersion += 1;
        this.#sessions.clear();
        this.#queues.clear();
        this.#expiredSessionIds.clear();
        this.#loadedBytes = 0;
    }

    #analyze(
        bytes: Uint8Array,
        format: DatInputFormat,
        failureCode: "invalid-request" | "reload-failed",
    ): AnalyzedDocument {
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
        for (const descriptor of descriptors) {
            this.#boundedString(descriptor.field.key);
            if (typeof descriptor.value === "string") this.#boundedString(descriptor.value);
        }
        this.#copyProjection(document);
        return { document, format, descriptors };
    }

    #createState(
        sessionId: string,
        documentId: string,
        loadedBytes: number,
        revision: number,
        analyzed: AnalyzedDocument,
    ): SessionState {
        const fields = new Map<string, FieldCapability>();
        const fieldOrder: string[] = [];
        for (const descriptor of analyzed.descriptors) {
            const fieldId = this.#newId();
            fields.set(fieldId, { ...descriptor, fieldId, currentValue: descriptor.value });
            fieldOrder.push(fieldId);
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
        };
    }

    #createView(session: SessionState): DatSessionView {
        const fields = session.fieldOrder.map((fieldId) => this.#copyField(session.fields.get(fieldId)!));
        const view: DatSessionView = {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.revision !== session.persistedRevision,
            format: session.format,
            encrypted: session.format === "encrypted",
            fields,
            projection: this.#copyProjection(session.document),
            diagnostics: this.#copyDiagnostics(session.document.diagnostics),
        };
        let encoded: string;
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

    #copyField(capability: FieldCapability): DatSessionFieldView {
        const view: {
            fieldId: string;
            key: string;
            kind: "number" | "string";
            numericKind?: DatSessionNumericKind;
            value: number | string;
            scope: DatSessionFieldScope;
            occurrence?: number;
            spriteRangeIndex?: number;
            frameId?: number;
            frameOccurrence?: number;
            blockType?: DatBlockType;
            blockIndex?: number;
        } = {
            fieldId: capability.fieldId,
            key: capability.field.key,
            kind: capability.kind,
            value: capability.currentValue,
            scope: capability.location.scope,
            occurrence: capability.location.occurrence,
        };
        if (capability.kind === "number") view.numericKind = capability.numericKind;
        if (capability.location.spriteRangeIndex !== undefined) view.spriteRangeIndex = capability.location.spriteRangeIndex;
        if (capability.location.frameId !== undefined) view.frameId = capability.location.frameId;
        if (capability.location.frameOccurrence !== undefined) view.frameOccurrence = capability.location.frameOccurrence;
        if (capability.location.blockType !== undefined) view.blockType = capability.location.blockType;
        if (capability.location.blockIndex !== undefined) view.blockIndex = capability.location.blockIndex;
        return view;
    }

    #copyDiagnostics(values: readonly DataDiagnostic[]): DatSessionDiagnosticView[] {
        return values.slice(0, this.#limits.maxDiagnostics).map((value) => ({
            code: value.code,
            severity: value.severity,
            message: value.message.slice(0, this.#limits.maxDiagnosticMessageLength),
        }));
    }

    #copyProjection(document: LosslessDatDocument): DatSessionProjectionView {
        const value = document.projection;
        const stringValue = (candidate: string): string => this.#boundedString(candidate);
        const projection: DatSessionProjectionView = {
            top: copyTop(value.top, stringValue),
            spriteRanges: value.spriteRanges.map((range): SpriteRangeProjection => ({
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
        let encoded: string;
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

    #boundedString(value: string): string {
        if (!isLatin1ScalarString(value) || Buffer.byteLength(value, "latin1") > this.#limits.maxStringBytes) {
            throw new DatSessionError("view-limit", "A DAT string exceeds the response limit.");
        }
        return value;
    }

    #parseEditRequest(input: unknown): DatSessionEditRequest {
        const record = exactRecord(input, ["sessionId", "fieldId", "value", "expectedRevision"]);
        const value = record.value;
        if ((typeof value !== "number" && typeof value !== "string")
            || (typeof value === "number" && !Number.isFinite(value))
            || (typeof value === "string" && (!isLatin1ScalarString(value) || Buffer.byteLength(value, "latin1") > this.#limits.maxStringBytes))) {
            throw new DatSessionError("invalid-request", "value must be a bounded finite number or single-line Latin-1 string.");
        }
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            fieldId: requiredId(record.fieldId, "fieldId"),
            value,
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    #parseReloadRequest(input: unknown): DatSessionReloadRequest {
        const record = exactRecord(input, ["sessionId", "expectedRevision"]);
        return {
            sessionId: requiredId(record.sessionId, "sessionId"),
            expectedRevision: requiredRevision(record.expectedRevision),
        };
    }

    #enqueue<T>(sessionId: string, operation: () => Promise<T> | T): Promise<T> {
        const previous = this.#queues.get(sessionId) ?? Promise.resolve();
        const guardedOperation = (): Promise<T> | T => {
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

    #requireSession(sessionId: string): SessionState {
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

    #release(session: SessionState, expired: boolean): void {
        if (!this.#sessions.delete(session.sessionId)) return;
        this.#loadedBytes -= session.loadedBytes;
        if (expired) {
            this.#expiredSessionIds.delete(session.sessionId);
            this.#expiredSessionIds.set(session.sessionId, this.#clock());
            while (this.#expiredSessionIds.size > this.#limits.maxSessions) {
                const oldest = this.#expiredSessionIds.keys().next().value as string | undefined;
                if (oldest === undefined) break;
                this.#expiredSessionIds.delete(oldest);
            }
        }
    }

    #newId(): string {
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

    #clock(): number {
        const value = this.#now();
        if (!Number.isFinite(value)) throw new DatSessionError("invalid-request", "The session clock is invalid.");
        return value;
    }

    #ensureActive(): void {
        if (this.#disposed) throw new DatSessionError("invalid-request", "The DAT session service is disposed.");
    }

    #ensureLifecycle(expectedVersion: number): void {
        this.#ensureActive();
        if (this.#lifecycleVersion !== expectedVersion) {
            throw new DatSessionError("invalid-request", "The DAT session service lifecycle changed during the operation.");
        }
    }
}
