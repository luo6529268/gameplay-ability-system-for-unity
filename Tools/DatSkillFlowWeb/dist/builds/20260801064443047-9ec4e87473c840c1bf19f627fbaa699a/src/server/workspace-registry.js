// dat-skill-flow-build:20260801064443047-9ec4e87473c840c1bf19f627fbaa699a
import { randomBytes } from "node:crypto";
import { isAbsolute, posix, win32 } from "node:path";

import {
    NativeSafeFileError,
    PowerShellWindowsSafeFileClient,
                              
                              
} from "./windows-safe-file-adapter.js";

export const MAX_DOCUMENT_BYTES = 16 * 1024 * 1024;

                                           
                                     
                              
                                        
 

                                  
                   
                 
                                
                               
                   
                  
 

                            
                   
 

                                 
                       
                   
                        
                                 
 

                                                      
                  
                                
 

                      
                   
                                 
 

                                                 
                          
 

                                   
                           
                    
                    
                        
                            
                   
                  
                      
                                 

export class WorkspaceSecurityError extends Error {
             code                       ;

    constructor(code                       , message        , options               ) {
        super(message, options);
        this.name = "WorkspaceSecurityError";
        this.code = code;
    }
}

function opaqueId()         {
    return randomBytes(32).toString("base64url");
}

function normalizeLogicalPath(logicalPath        )         {
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

export function fingerprintsEqual(left                 , right                 )          {
    return left.sha256 === right.sha256
        && left.size === right.size
        && left.modifiedNanoseconds === right.modifiedNanoseconds
        && left.changedNanoseconds === right.changedNanoseconds
        && left.device === right.device
        && left.inode === right.inode;
}

export class WorkspaceRegistry {
             #allowAbsoluteRootGrant         ;
             #maxDocumentBytes        ;
             #nativeClient                      ;
             #roots = new Map                    ();
             #documents = new Map                        ();

    constructor(options                           = {}) {
        this.#allowAbsoluteRootGrant = options.allowAbsoluteRootGrant === true;
        this.#maxDocumentBytes = options.maxDocumentBytes ?? MAX_DOCUMENT_BYTES;
        this.#nativeClient = options.nativeClient ?? new PowerShellWindowsSafeFileClient();
        if (!Number.isSafeInteger(this.#maxDocumentBytes) || this.#maxDocumentBytes < 1) {
            throw new RangeError("maxDocumentBytes must be a positive safe integer.");
        }
    }

    listRootIds()           {
        return [...this.#roots.keys()];
    }

    async grantAbsoluteRoot(absoluteRoot        )                     {
        if (!this.#allowAbsoluteRootGrant) {
            throw new WorkspaceSecurityError("root-grant-disabled", "Absolute root grants are disabled for this process.");
        }
        if (typeof absoluteRoot !== "string" || absoluteRoot.includes("\0") || !isAbsolute(absoluteRoot)) {
            throw new WorkspaceSecurityError("invalid-root", "The startup-authorized root must be absolute.");
        }
        let native                      ;
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

    getDocument(documentId        )                           {
        const document = this.#documents.get(documentId);
        if (document === undefined) {
            throw new WorkspaceSecurityError("unknown-document", "The document ID is unknown.");
        }
        return document;
    }

    getRootDescriptor(rootId        )                       {
        const root = this.#roots.get(rootId);
        if (root === undefined) {
            throw new WorkspaceSecurityError("unknown-root", "The root ID is unknown.");
        }
        return root.native;
    }

    get nativeClient()                       {
        return this.#nativeClient;
    }

    get maxDocumentBytes()         {
        return this.#maxDocumentBytes;
    }

    normalizeLogicalPath(logicalPath        )         {
        return normalizeLogicalPath(logicalPath);
    }

    async openDocument(rootId        , logicalPath        )                          {
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

    async readDocument(documentId        )                        {
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

    bindNativeDocument(
        documentId        ,
        rootId        ,
        logicalPath        ,
        canonicalPath        ,
        fingerprint                 ,
    )                 {
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
        error         ,
        fallbackCode                       ,
        fallbackMessage        ,
    )                         {
        if (!(error instanceof NativeSafeFileError)) {
            return new WorkspaceSecurityError(fallbackCode, fallbackMessage, { cause: error });
        }
        const code                        = error.code === "read-too-large"
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
