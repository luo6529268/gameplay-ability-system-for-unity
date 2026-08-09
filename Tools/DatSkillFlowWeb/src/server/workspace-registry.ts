import { randomBytes } from "node:crypto";
import { isAbsolute, posix, win32 } from "node:path";

import {
    NativeSafeFileError,
    PowerShellWindowsSafeFileClient,
    type NativeRootDescriptor,
    type NativeSafeFileClient,
} from "./windows-safe-file-adapter.js";

export const MAX_DOCUMENT_BYTES = 16 * 1024 * 1024;

export interface WorkspaceRegistryOptions {
    allowAbsoluteRootGrant?: boolean;
    maxDocumentBytes?: number;
    nativeClient?: NativeSafeFileClient;
}

export interface FileFingerprint {
    sha256: string;
    size: number;
    modifiedNanoseconds: string;
    changedNanoseconds: string;
    device: string;
    inode: string;
}

export interface RootGrant {
    rootId: string;
}

export interface OpenedDocument {
    documentId: string;
    rootId: string;
    logicalPath: string;
    fingerprint: FileFingerprint;
}

export interface DocumentRead extends OpenedDocument {
    bytes: Buffer;
    externallyModified: boolean;
}

export interface PreparedDocumentRefresh {
    snapshot: DocumentRead;
    commit(): void;
}

interface RootRecord {
    rootId: string;
    native: NativeRootDescriptor;
}

interface DocumentRecord extends OpenedDocument {
    canonicalPath: string;
}

export type WorkspaceSecurityCode =
    | "root-grant-disabled"
    | "startup-authorization-sealed"
    | "startup-root-already-authorized"
    | "invalid-root"
    | "unknown-root"
    | "unknown-document"
    | "invalid-logical-path"
    | "root-escape"
    | "not-a-file"
    | "read-too-large"
    | "file-changed-during-read";

export class WorkspaceSecurityError extends Error {
    readonly code: WorkspaceSecurityCode;

    constructor(code: WorkspaceSecurityCode, message: string, options?: ErrorOptions) {
        super(message, options);
        this.name = "WorkspaceSecurityError";
        this.code = code;
    }
}

function opaqueId(): string {
    return randomBytes(32).toString("base64url");
}

function normalizeLogicalPath(logicalPath: string): string {
    if (typeof logicalPath !== "string"
        || logicalPath.length === 0
        || logicalPath.includes("\0")
        || posix.isAbsolute(logicalPath)
        || win32.isAbsolute(logicalPath)
        || /^[A-Za-z]:/.test(logicalPath)) {
        throw new WorkspaceSecurityError("invalid-logical-path", "The logical path is not a safe relative path.");
    }
    const segments = logicalPath.split(/[\\/]/);
    const reservedDeviceName = /^(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\..*)?$/i;
    if (segments.some((segment) => (
        segment.length === 0
        || segment === "."
        || segment === ".."
        || segment.endsWith(".")
        || segment.endsWith(" ")
        || segment.includes(":")
        || reservedDeviceName.test(segment)
    ))) {
        throw new WorkspaceSecurityError("invalid-logical-path", "The logical path contains an unsafe segment.");
    }
    return segments.join("/");
}

export function fingerprintsEqual(left: FileFingerprint, right: FileFingerprint): boolean {
    return left.sha256 === right.sha256
        && left.size === right.size
        && left.modifiedNanoseconds === right.modifiedNanoseconds
        && left.changedNanoseconds === right.changedNanoseconds
        && left.device === right.device
        && left.inode === right.inode;
}

export class WorkspaceRegistry {
    readonly #allowAbsoluteRootGrant: boolean;
    readonly #maxDocumentBytes: number;
    readonly #nativeClient: NativeSafeFileClient;
    readonly #roots = new Map<string, RootRecord>();
    readonly #documents = new Map<string, DocumentRecord>();
    #startupAuthorizationSealed = false;
    #startupRootId: string | undefined;

    constructor(options: WorkspaceRegistryOptions = {}) {
        this.#allowAbsoluteRootGrant = options.allowAbsoluteRootGrant === true;
        this.#maxDocumentBytes = options.maxDocumentBytes ?? MAX_DOCUMENT_BYTES;
        this.#nativeClient = options.nativeClient ?? new PowerShellWindowsSafeFileClient();
        if (!Number.isSafeInteger(this.#maxDocumentBytes) || this.#maxDocumentBytes < 1) {
            throw new RangeError("maxDocumentBytes must be a positive safe integer.");
        }
    }

    listRootIds(): string[] {
        return [...this.#roots.keys()];
    }

    async authorizeStartupRoot(absoluteRoot: string): Promise<RootGrant> {
        if (this.#startupAuthorizationSealed) {
            throw new WorkspaceSecurityError(
                "startup-authorization-sealed",
                "Startup workspace authorization is sealed for this process.",
            );
        }
        if (this.#startupRootId !== undefined) {
            throw new WorkspaceSecurityError(
                "startup-root-already-authorized",
                "Only one startup workspace root may be authorized.",
            );
        }
        const grant = await this.#registerAbsoluteRoot(absoluteRoot);
        this.#startupRootId = grant.rootId;
        return grant;
    }

    sealStartupAuthorization(): void {
        this.#startupAuthorizationSealed = true;
    }

    getStartupRootGrant(): RootGrant | undefined {
        return this.#startupRootId === undefined
            ? undefined
            : Object.freeze({ rootId: this.#startupRootId });
    }

    async grantAbsoluteRoot(absoluteRoot: string): Promise<RootGrant> {
        if (!this.#allowAbsoluteRootGrant) {
            throw new WorkspaceSecurityError("root-grant-disabled", "Absolute root grants are disabled for this process.");
        }
        return await this.#registerAbsoluteRoot(absoluteRoot);
    }

    async #registerAbsoluteRoot(absoluteRoot: string): Promise<RootGrant> {
        if (typeof absoluteRoot !== "string" || absoluteRoot.includes("\0") || !isAbsolute(absoluteRoot)) {
            throw new WorkspaceSecurityError("invalid-root", "The startup-authorized root must be absolute.");
        }
        let native: NativeRootDescriptor;
        try {
            native = await this.#nativeClient.inspectRoot({ absoluteRoot });
        } catch (error) {
            throw this.#mapNativeError(error, "invalid-root", "The selected root cannot be validated by handle.");
        }
        const existing = [...this.#roots.values()].find((record) => (
            record.native.volumeSerial === native.volumeSerial && record.native.fileId === native.fileId
        ));
        if (existing !== undefined) {
            return { rootId: existing.rootId };
        }
        const rootId = opaqueId();
        this.#roots.set(rootId, { rootId, native });
        return { rootId };
    }

    getDocument(documentId: string): Readonly<DocumentRecord> {
        const document = this.#documents.get(documentId);
        if (document === undefined) {
            throw new WorkspaceSecurityError("unknown-document", "The document ID is unknown.");
        }
        return document;
    }

    closeDocument(documentId: string): boolean {
        return this.#documents.delete(documentId);
    }

    getRootDescriptor(rootId: string): NativeRootDescriptor {
        const root = this.#roots.get(rootId);
        if (root === undefined) {
            throw new WorkspaceSecurityError("unknown-root", "The root ID is unknown.");
        }
        return root.native;
    }

    get nativeClient(): NativeSafeFileClient {
        return this.#nativeClient;
    }

    get maxDocumentBytes(): number {
        return this.#maxDocumentBytes;
    }

    normalizeLogicalPath(logicalPath: string): string {
        return normalizeLogicalPath(logicalPath);
    }

    async readLogicalFile(rootId: string, logicalPath: string): Promise<Buffer> {
        const normalized = normalizeLogicalPath(logicalPath);
        try {
            const snapshot = await this.#nativeClient.read({
                root: this.getRootDescriptor(rootId),
                logicalPath: normalized,
                maximumBytes: this.#maxDocumentBytes,
            });
            return Buffer.from(snapshot.bytes);
        } catch (error) {
            throw this.#mapNativeError(error, "not-a-file", "The requested file could not be read safely.");
        }
    }

    async openDocument(rootId: string, logicalPath: string): Promise<OpenedDocument> {
        const normalized = normalizeLogicalPath(logicalPath);
        let snapshot;
        try {
            snapshot = await this.#nativeClient.read({
                root: this.getRootDescriptor(rootId),
                logicalPath: normalized,
                maximumBytes: this.#maxDocumentBytes,
            });
        } catch (error) {
            throw this.#mapNativeError(error, "not-a-file", "The requested document could not be opened safely.");
        }
        const documentId = opaqueId();
        const document = Object.freeze({
            documentId,
            rootId,
            logicalPath: normalized,
            canonicalPath: snapshot.canonicalPath,
            fingerprint: snapshot.fingerprint,
        });
        this.#documents.set(documentId, document);
        return {
            documentId,
            rootId,
            logicalPath: normalized,
            fingerprint: snapshot.fingerprint,
        };
    }

    async readDocument(documentId: string): Promise<DocumentRead> {
        const document = this.getDocument(documentId);
        let current;
        try {
            current = await this.#nativeClient.read({
                root: this.getRootDescriptor(document.rootId),
                logicalPath: document.logicalPath,
                maximumBytes: this.#maxDocumentBytes,
            });
        } catch (error) {
            throw this.#mapNativeError(error, "not-a-file", "The requested document could not be read safely.");
        }
        if (current.canonicalPath.toLowerCase() !== document.canonicalPath.toLowerCase()) {
            throw new WorkspaceSecurityError("root-escape", "The document no longer identifies its registered target.");
        }
        return {
            documentId,
            rootId: document.rootId,
            logicalPath: document.logicalPath,
            bytes: current.bytes,
            fingerprint: current.fingerprint,
            externallyModified: !fingerprintsEqual(document.fingerprint, current.fingerprint),
        };
    }

    async prepareDocumentRefresh(documentId: string): Promise<PreparedDocumentRefresh> {
        const document = this.getDocument(documentId);
        let current;
        try {
            current = await this.#nativeClient.read({
                root: this.getRootDescriptor(document.rootId),
                logicalPath: document.logicalPath,
                maximumBytes: this.#maxDocumentBytes,
            });
        } catch (error) {
            throw this.#mapNativeError(error, "not-a-file", "The requested document could not be refreshed safely.");
        }
        if (current.canonicalPath.toLowerCase() !== document.canonicalPath.toLowerCase()) {
            throw new WorkspaceSecurityError("root-escape", "The document no longer identifies its registered target.");
        }
        const snapshot: DocumentRead = {
            documentId,
            rootId: document.rootId,
            logicalPath: document.logicalPath,
            bytes: current.bytes,
            fingerprint: current.fingerprint,
            externallyModified: !fingerprintsEqual(document.fingerprint, current.fingerprint),
        };
        let committed = false;
        return {
            snapshot,
            commit: () => {
                if (committed || this.#documents.get(documentId) !== document) {
                    throw new WorkspaceSecurityError(
                        "file-changed-during-read",
                        "The document registry changed before refresh could commit.",
                    );
                }
                committed = true;
                this.#documents.set(documentId, Object.freeze({
                    ...document,
                    fingerprint: current.fingerprint,
                }));
            },
        };
    }

    bindNativeDocument(
        documentId: string,
        rootId: string,
        logicalPath: string,
        canonicalPath: string,
        fingerprint: FileFingerprint,
    ): OpenedDocument {
        this.getDocument(documentId);
        this.getRootDescriptor(rootId);
        const normalized = normalizeLogicalPath(logicalPath);
        const replacement = Object.freeze({
            documentId,
            rootId,
            logicalPath: normalized,
            canonicalPath,
            fingerprint,
        });
        this.#documents.set(documentId, replacement);
        return {
            documentId,
            rootId,
            logicalPath: normalized,
            fingerprint,
        };
    }

    #mapNativeError(
        error: unknown,
        fallbackCode: WorkspaceSecurityCode,
        fallbackMessage: string,
    ): WorkspaceSecurityError {
        if (!(error instanceof NativeSafeFileError)) {
            return new WorkspaceSecurityError(fallbackCode, fallbackMessage, { cause: error });
        }
        const code: WorkspaceSecurityCode = error.code === "read-too-large"
            ? "read-too-large"
            : error.code === "file-changed-during-read"
                ? "file-changed-during-read"
                : ["root-escape", "root-changed", "reparse-point"].includes(error.code)
                    ? "root-escape"
                    : error.code === "invalid-logical-path"
                        ? "invalid-logical-path"
                        : error.code === "not-a-file"
                            ? "not-a-file"
                            : fallbackCode;
        return new WorkspaceSecurityError(code, error.message, { cause: error });
    }
}
