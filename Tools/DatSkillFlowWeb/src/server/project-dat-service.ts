import { execFile as nodeExecFile } from "node:child_process";
import { randomBytes } from "node:crypto";
import { mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join } from "node:path";

import { parseBmpMetadata } from "../assets/bmp.js";
import type { DatFrameProjection } from "../model/dat-projection.js";
import { DataTxtDocument, diagnoseResourcePath, type DataTxtEntry } from "../project/data-txt.js";
import { MAX_CATALOG_OIDS } from "../sim/catalog.js";
import { DatSessionError, DatSessionService, type DatSessionView } from "./dat-session-service.js";
import type {
    NativePreviewEntityView,
    NativePreviewTickView,
    NativePreviewView,
    ProjectAssetResponse,
    ProjectCatalogView,
    ProjectCloseResponse,
    ProjectDatErrorCode,
    ProjectFrameView,
    ProjectSaveResponse,
    ProjectSessionView,
} from "./project-dat-contract.js";
import { SafeSaveError, SafeSaveService } from "./safe-save.js";
import { type OpenedDocument, WorkspaceRegistry, WorkspaceSecurityError } from "./workspace-registry.js";

const DEFAULT_START_FRAME = 300;
const DEFAULT_TICKS = 30;
const NARUTO_OID = 2;
const MAX_PREVIEW_OUTPUT_BYTES = 8 * 1024 * 1024;
const DEFAULT_CPP_DIRECTORY = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";
const DEFAULT_CPP_EXECUTABLE = "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\dat_preview_cli.exe";

export class ProjectDatError extends Error {
    readonly code: ProjectDatErrorCode;

    constructor(code: ProjectDatErrorCode, message: string, options?: ErrorOptions) {
        super(message, options);
        this.name = "ProjectDatError";
        this.code = code;
    }
}

export interface NativeDatPreviewRunner {
    preview(plaintext: Uint8Array, options?: { startFrame?: number; ticks?: number }): Promise<unknown>;
}

export interface ProjectDatServiceOptions {
    readonly primaryRegistry: WorkspaceRegistry;
    readonly assetRegistry?: WorkspaceRegistry;
    readonly dataTxtLogicalPath?: string;
    readonly previewRunner?: NativeDatPreviewRunner;
    readonly safeSave?: SafeSaveService;
    readonly assetBmpDirectories?: readonly string[];
    readonly idFactory?: () => string;
}

interface CatalogObject {
    objectKey: string;
    oid: number;
    type: number;
    candidates: readonly DataTxtEntry[];
    availablePrimary: boolean;
}

interface SessionBinding {
    sessionId: string;
    service: DatSessionService;
    documentId: string;
    rootId: string;
    logicalPath: string;
    oid: number;
    type: number;
    assetIdsByPath: Map<string, string>;
}

interface AssetBinding {
    sessionId: string;
    registry: WorkspaceRegistry;
    documentId: string;
}

interface OpenedCandidate {
    registry: WorkspaceRegistry;
    service: DatSessionService;
    documentId: string;
    rootId: string;
    logicalPath: string;
}

type ExecFileFunction = (
    file: string,
    args: readonly string[],
    options: { cwd: string; windowsHide: boolean; timeout: number; maxBuffer: number },
    callback: (error: Error | null, stdout: string, stderr: string) => void,
) => void;

export class CppNativeDatPreviewRunner implements NativeDatPreviewRunner {
    readonly #executable: string;
    readonly #workingDirectory: string;
    readonly #execFile: ExecFileFunction;

    constructor(options: { executable?: string; workingDirectory?: string; execFile?: ExecFileFunction } = {}) {
        this.#executable = options.executable === undefined ? DEFAULT_CPP_EXECUTABLE : options.executable;
        this.#workingDirectory = options.workingDirectory === undefined ? DEFAULT_CPP_DIRECTORY : options.workingDirectory;
        this.#execFile = options.execFile === undefined ? nodeExecFile as unknown as ExecFileFunction : options.execFile;
    }

    async preview(plaintext: Uint8Array, options: { startFrame?: number; ticks?: number } = {}): Promise<unknown> {
        const startFrame = boundedInteger(options.startFrame === undefined ? DEFAULT_START_FRAME : options.startFrame, 0, 599, "startFrame");
        const ticks = boundedInteger(options.ticks === undefined ? DEFAULT_TICKS : options.ticks, 1, 1800, "ticks");
        const directory = await mkdtemp(join(tmpdir(), "dat-skill-flow-preview-"));
        const datPath = join(directory, "naruto.dat");
        const outputPath = join(directory, "preview.json");
        try {
            await writeFile(datPath, Buffer.from(plaintext), { flag: "wx" });
            await new Promise<void>((resolveRun, rejectRun) => {
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
            return JSON.parse(bytes.toString("utf8")) as unknown;
        } finally {
            await rm(directory, { recursive: true, force: true });
        }
    }
}

export class ProjectDatService {
    readonly #primary: WorkspaceRegistry;
    readonly #assets?: WorkspaceRegistry;
    readonly #primarySessions: DatSessionService;
    readonly #assetSessions?: DatSessionService;
    readonly #safeSave: SafeSaveService;
    readonly #previewRunner: NativeDatPreviewRunner;
    readonly #idFactory: () => string;
    readonly #assetDirectories: readonly string[];
    readonly #dataDocumentId: string;
    readonly #primaryRootId: string;
    readonly #assetRootId?: string;
    readonly #sessions = new Map<string, SessionBinding>();
    readonly #assetBindings = new Map<string, AssetBinding>();
    readonly #queues = new Map<string, Promise<void>>();
    #catalogRevision = 1;
    #catalogObjects: CatalogObject[] = [];
    #nextIdSequence = 0;

    private constructor(
        options: ProjectDatServiceOptions,
        dataDocumentId: string,
        primaryRootId: string,
        assetRootId: string | undefined,
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

    static async initialize(options: ProjectDatServiceOptions): Promise<ProjectDatService> {
        const primaryRoot = options.primaryRegistry.getStartupRootGrant();
        const primaryRootId = primaryRoot === undefined ? undefined : primaryRoot.rootId;
        if (primaryRootId === undefined) throw new ProjectDatError("project-disabled", "The project workspace is not configured.");
        const dataPath = options.primaryRegistry.normalizeLogicalPath(
            options.dataTxtLogicalPath === undefined ? "Assets/NTSD/Config/data.txt" : options.dataTxtLogicalPath,
        );
        const dataDocument = await options.primaryRegistry.openDocument(primaryRootId, dataPath);
        let assetRootId: string | undefined;
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

    async catalog(): Promise<ProjectCatalogView> {
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

    async open(objectKey: string): Promise<ProjectSessionView> {
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
        let session: DatSessionView;
        try {
            session = await opened.service.openDocument(opened.documentId, "encrypted");
        } catch (error) {
            opened.registry.closeDocument(opened.documentId);
            throw mapSessionError(error);
        }
        const binding: SessionBinding = {
            ...opened,
            sessionId: session.sessionId,
            oid: object.oid,
            type: object.type,
            assetIdsByPath: new Map(),
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

    async edit(input: unknown): Promise<ProjectSessionView> {
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
            return await this.#buildSessionView(view, binding);
        });
    }

    async preview(input: unknown): Promise<{ sessionId: string; revision: number; preview: NativePreviewView }> {
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

    async save(input: unknown): Promise<ProjectSaveResponse> {
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

    async close(input: unknown): Promise<ProjectCloseResponse> {
        const request = exactRecord(input, ["sessionId"]);
        const sessionId = requireOpaqueId(request.sessionId, "sessionId");
        return await this.#enqueue(sessionId, async () => {
            const binding = this.#requireSession(sessionId);
            await binding.service.close(sessionId);
            this.#releaseBinding(sessionId, binding);
            return { sessionId, closed: true };
        });
    }

    async asset(assetId: string): Promise<ProjectAssetResponse> {
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

    async #refreshCatalog(): Promise<void> {
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

    async #replaceCatalog(bytes: Uint8Array): Promise<void> {
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

        const candidatesByOid = new Map<number, DataTxtEntry[]>();
        for (const candidate of objects) {
            const candidates = candidatesByOid.get(candidate.id);
            if (candidates === undefined) candidatesByOid.set(candidate.id, [candidate]);
            else candidates.push(candidate);
        }

        const prepared: CatalogObject[] = [];
        for (const candidates of candidatesByOid.values()) {
            const first = candidates[0]!;
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
                type: first.type!,
                candidates,
                availablePrimary,
            });
        }
        this.#catalogObjects = prepared;
    }

    async #openCandidate(candidates: readonly DataTxtEntry[]): Promise<OpenedCandidate | undefined> {
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

    async #buildSessionView(session: DatSessionView, binding: SessionBinding): Promise<ProjectSessionView> {
        const spriteRanges = [];
        for (const range of session.projection.spriteRanges) {
            const asset = await this.#resolveSpriteAsset(binding, range.file);
            if (asset === undefined) continue;
            spriteRanges.push({
                frameLo: range.frameLo, frameHi: range.frameHi, assetId: asset.assetId,
                w: range.w, h: range.h, row: range.row, col: range.col,
            });
        }
        const emission = await binding.service.emit(session.sessionId, session.revision).catch((error) => { throw mapSessionError(error); });
        return {
            sessionId: session.sessionId,
            revision: session.revision,
            dirty: session.dirty,
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

    async #resolveSpriteAsset(binding: SessionBinding, rawPath: string): Promise<{ assetId: string } | undefined> {
        if (diagnoseResourcePath(rawPath) !== undefined) return undefined;
        let pathKey: string;
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
        const matches: OpenedDocument[] = [];
        for (const directory of this.#assetDirectories) {
            const candidate = await this.#tryOpenDocument(this.#assets, this.#assetRootId, `${directory}/${name}`);
            if (candidate !== undefined) matches.push(candidate);
        }
        if (matches.length === 1) return this.#bindAsset(binding, pathKey, this.#assets, matches[0]!);
        for (const match of matches) this.#assets.closeDocument(match.documentId);
        return undefined;
    }

    #bindAsset(
        binding: SessionBinding,
        pathKey: string,
        registry: WorkspaceRegistry,
        document: { documentId: string },
    ): { assetId: string } {
        const assetId = this.#newId();
        binding.assetIdsByPath.set(pathKey, assetId);
        this.#assetBindings.set(assetId, {
            sessionId: binding.sessionId,
            registry,
            documentId: document.documentId,
        });
        return { assetId };
    }

    async #runPreview(plaintext: Uint8Array, options?: { startFrame?: number; ticks?: number }): Promise<NativePreviewView> {
        try {
            return sanitizePreview(await this.#previewRunner.preview(Buffer.from(plaintext), options));
        } catch (error) {
            if (error instanceof ProjectDatError) throw error;
            throw new ProjectDatError("preview-failed", "The native Naruto preview failed.", { cause: error });
        }
    }

    async #tryOpen(
        registry: WorkspaceRegistry,
        service: DatSessionService,
        rootId: string,
        logicalPath: string,
    ): Promise<OpenedCandidate | undefined> {
        const opened = await this.#tryOpenDocument(registry, rootId, logicalPath);
        return opened === undefined ? undefined : { registry, service, rootId, logicalPath: opened.logicalPath, documentId: opened.documentId };
    }

    async #tryOpenDocument(registry: WorkspaceRegistry, rootId: string, logicalPath: string) {
        try {
            const normalized = registry.normalizeLogicalPath(logicalPath);
            return await registry.openDocument(rootId, normalized);
        } catch (error) {
            if (error instanceof WorkspaceSecurityError
                && ["not-a-file", "invalid-logical-path", "root-escape"].includes(error.code)) return undefined;
            throw error;
        }
    }

    async #canOpen(registry: WorkspaceRegistry, rootId: string, path: string): Promise<boolean> {
        const opened = await this.#tryOpenDocument(registry, rootId, path);
        if (opened === undefined) return false;
        registry.closeDocument(opened.documentId);
        return true;
    }

    #requireSession(sessionId: string): SessionBinding {
        const binding = this.#sessions.get(sessionId);
        if (binding === undefined) throw new ProjectDatError("unknown-session", "The project session is unknown.");
        return binding;
    }

    async #invalidateAll(): Promise<void> {
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

    #sweepExpired(): void {
        const expiredSessionIds = [
            ...this.#primarySessions.sweepExpiredSessionIds(),
            ...(this.#assetSessions?.sweepExpiredSessionIds() ?? []),
        ];
        for (const sessionId of expiredSessionIds) {
            const binding = this.#sessions.get(sessionId);
            if (binding !== undefined) this.#releaseBinding(sessionId, binding);
        }
    }

    #releaseBinding(sessionId: string, binding: SessionBinding): void {
        this.#sessions.delete(sessionId);
        for (const assetId of binding.assetIdsByPath.values()) {
            const asset = this.#assetBindings.get(assetId);
            if (asset !== undefined) asset.registry.closeDocument(asset.documentId);
            this.#assetBindings.delete(assetId);
        }
        binding.assetIdsByPath.clear();
        binding.registry.closeDocument(binding.documentId);
    }

    async #enqueue<T>(sessionId: string, operation: () => Promise<T>): Promise<T> {
        const previous = this.#queues.get(sessionId);
        const previousOperation = previous === undefined ? Promise.resolve() : previous;
        let release!: () => void;
        const current = new Promise<void>((resolveRelease) => { release = resolveRelease; });
        this.#queues.set(sessionId, current);
        await previousOperation;
        try {
            return await operation();
        } finally {
            release();
            if (this.#queues.get(sessionId) === current) this.#queues.delete(sessionId);
        }
    }

    #newId(): string {
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

function exactRecord(value: unknown, keys: readonly string[]): Record<string, unknown> {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        throw new ProjectDatError("invalid-request", "The request must be an object.");
    }
    const record = value as Record<string, unknown>;
    const actual = Object.keys(record);
    if (actual.length !== keys.length || actual.some((key) => !keys.includes(key)) || keys.some((key) => !Object.hasOwn(record, key))) {
        throw new ProjectDatError("invalid-request", "The request has missing or unknown fields.");
    }
    return record;
}

function requireOpaqueId(value: unknown, name: string): string {
    if (typeof value !== "string" || !/^[A-Za-z0-9_-]{32,128}$/.test(value)) {
        throw new ProjectDatError("invalid-request", `${name} must be an opaque capability ID.`);
    }
    return value;
}

function boundedInteger(value: unknown, minimum: number, maximum: number, name: string): number {
    if (typeof value !== "number" || !Number.isSafeInteger(value) || value < minimum || value > maximum) {
        throw new ProjectDatError("invalid-request", `${name} is outside its supported integer range.`);
    }
    return value;
}

function finite(value: unknown, name: string): number {
    if (typeof value !== "number" || !Number.isFinite(value)) throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    return value;
}

function record(value: unknown, name: string): Record<string, unknown> {
    if (typeof value !== "object" || value === null || Array.isArray(value)) throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    return value as Record<string, unknown>;
}

function textValue(value: unknown, name: string, maximum = 256): string {
    if (typeof value !== "string" || Buffer.byteLength(value, "utf8") > maximum || value.includes("\0")) {
        throw new ProjectDatError("preview-failed", `Native preview ${name} is invalid.`);
    }
    return value;
}

function vector(value: unknown, name: string): { x: number; y: number; z: number } {
    const item = record(value, name);
    return { x: finite(item.x, `${name}.x`), y: finite(item.y, `${name}.y`), z: finite(item.z, `${name}.z`) };
}

function sanitizeEntity(value: unknown): NativePreviewEntityView {
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

function sanitizeTick(value: unknown): NativePreviewTickView {
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

function sanitizePreview(value: unknown): NativePreviewView {
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
                },
                initial,
        },
        ticks: ticks.map(sanitizeTick),
    };
}

function copySafeFrame(value: DatFrameProjection): ProjectFrameView {
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

function safeObservation(value: { exists: boolean; size?: number; sha256?: string }) {
    return { exists: value.exists, ...(value.size === undefined ? {} : { size: value.size }), ...(value.sha256 === undefined ? {} : { sha256: value.sha256 }) };
}

function mapSessionError(error: unknown): ProjectDatError {
    if (!(error instanceof DatSessionError)) return new ProjectDatError("invalid-request", "The DAT session operation failed.", { cause: error });
    if (error.code === "revision-conflict") return new ProjectDatError("revision-conflict", "The DAT session revision is stale.", { cause: error });
    if (error.code === "unknown-session" || error.code === "expired") return new ProjectDatError("unknown-session", "The project session is unknown.", { cause: error });
    return new ProjectDatError("invalid-request", error.message, { cause: error });
}
