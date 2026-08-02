// dat-skill-flow-build:20260801130125861-820a417a5dcd4942ac67c3273baa0b20
import { createHash, randomUUID } from "node:crypto";
import { posix } from "node:path";

import {
    NativeSafeFileError,
                               
                              
                              
} from "./windows-safe-file-adapter.js";
import {
    fingerprintsEqual,
    MAX_DOCUMENT_BYTES,
                         
                        
    WorkspaceRegistry,
} from "./workspace-registry.js";

export const DEFAULT_OVERWRITE_CHALLENGE_TTL_MS = 30_000;

                                         
                                        
                               
                       
                            
 

                                  
                 
                    
                  
                    
                             
 

                                 
                            
                                  
                             
 

                               
                         
                          
                            
                         
                         
                                  
                                 
                       
                         
                                 
                      
                                    

export class SafeSaveError extends Error {
             code                   ;
             recovery                 ;
             win32Code         ;

    constructor(
        code                   ,
        message        ,
        options                                                                   = {},
    ) {
        super(message, options);
        this.name = "SafeSaveError";
        this.code = code;
        this.recovery = options.recovery;
        this.win32Code = options.win32Code;
    }
}

                               
                      
                             
 

                                     
                        
                       
                   
                        
                          
                                       
                      
 

                                  
                          
                             
                                       
 

                                                      
                                
 

class TargetLockManager {
             #tails = new Map                       ();

    async runExclusive   (targetKey        , operation                  )             {
        const key = targetKey.toLowerCase();
        const previous = this.#tails.get(key) ?? Promise.resolve();
        let release                          ;
        const current = new Promise      ((resolveRelease) => {
            release = resolveRelease;
        });
        this.#tails.set(key, current);
        await previous;
        try {
            return await operation();
        } finally {
            release?.();
            if (this.#tails.get(key) === current) {
                this.#tails.delete(key);
            }
        }
    }
}

function contentHash(bytes            )         {
    return createHash("sha256").update(bytes).digest("hex");
}

function ensureContentBound(bytes            )       {
    if (!(bytes instanceof Uint8Array) || bytes.byteLength > MAX_DOCUMENT_BYTES) {
        throw new SafeSaveError("content-too-large", "Save content exceeds the document limit.");
    }
}

function publicObservation(value                       )                  {
    return { ...value };
}

function publicRecovery(value                                  )                             {
    return value === undefined
        ? undefined
        : {
            target: publicObservation(value.target),
            ...(value.replacement === undefined ? {} : { replacement: publicObservation(value.replacement) }),
            ...(value.backup === undefined ? {} : { backup: publicObservation(value.backup) }),
        };
}

function targetLockKey(rootId        , logicalPath        )         {
    return `${rootId}\0${logicalPath}`;
}

export class SafeSaveService {
             #registry                   ;
             #nativeClient                      ;
             #nameFactory              ;
             #now              ;
             #challengeTtlMs        ;
             #locks = new TargetLockManager();
             #challenges = new Map                         ();

    constructor(registry                   , options                         = {}) {
        this.#registry = registry;
        this.#nativeClient = options.nativeClient ?? registry.nativeClient;
        this.#nameFactory = options.nameFactory ?? randomUUID;
        this.#now = options.now ?? Date.now;
        this.#challengeTtlMs = options.challengeTtlMs ?? DEFAULT_OVERWRITE_CHALLENGE_TTL_MS;
        if (!Number.isSafeInteger(this.#challengeTtlMs) || this.#challengeTtlMs < 1) {
            throw new RangeError("challengeTtlMs must be a positive safe integer.");
        }
    }

    async saveAs(
        documentId        ,
        rootId        ,
        logicalPath        ,
        bytes            ,
    )                        {
        ensureContentBound(bytes);
        this.#registry.getDocument(documentId);
        const stableBytes = Buffer.from(bytes);
        const normalized = this.#registry.normalizeLogicalPath(logicalPath);
        return await this.#locks.runExclusive(targetLockKey(rootId, normalized), async () => {
            let result;
            try {
                result = await this.#nativeClient.saveAs({
                    root: this.#registry.getRootDescriptor(rootId),
                    logicalPath: normalized,
                    maximumBytes: this.#registry.maxDocumentBytes,
                    bytes: stableBytes,
                });
            } catch (error) {
                throw this.#mapSaveAsFailure(error);
            }
            const digest = contentHash(stableBytes);
            if (result.fingerprint.sha256 !== digest
                || result.fingerprint.size !== stableBytes.length
                || !result.recovery.target.exists
                || result.recovery.target.sha256 !== digest) {
                throw new SafeSaveError("save-as-write-failed", "The native Save As postconditions were inconsistent.", {
                    recovery: publicRecovery(result.recovery),
                });
            }
            const document = this.#registry.bindNativeDocument(
                documentId,
                rootId,
                normalized,
                result.canonicalPath,
                result.fingerprint,
            );
            return { status: "created", document };
        });
    }

    async issueOverwriteChallenge(
        documentId        ,
        rootId        ,
        logicalPath        ,
        bytes            ,
    )                              {
        ensureContentBound(bytes);
        const stableBytes = Buffer.from(bytes);
        const document = this.#registry.getDocument(documentId);
        const normalized = this.#registry.normalizeLogicalPath(logicalPath);
        let target;
        try {
            target = await this.#nativeClient.read({
                root: this.#registry.getRootDescriptor(rootId),
                logicalPath: normalized,
                maximumBytes: this.#registry.maxDocumentBytes,
            });
        } catch (error) {
            throw new SafeSaveError("challenge-target-mismatch", "The overwrite target could not be read safely.", { cause: error });
        }
        if (document.rootId === rootId
            && document.logicalPath === normalized
            && (document.canonicalPath.toLowerCase() !== target.canonicalPath.toLowerCase()
                || !fingerprintsEqual(document.fingerprint, target.fingerprint))) {
            throw new SafeSaveError("external-change", "The loaded document changed before overwrite confirmation.");
        }
        const challenge                  = Object.freeze({
            challengeId: randomUUID(),
            documentId,
            rootId,
            logicalPath: normalized,
            targetCanonicalPath: target.canonicalPath,
            contentSha256: contentHash(stableBytes),
            targetFingerprint: target.fingerprint,
            expiresAt: this.#now() + this.#challengeTtlMs,
        });
        this.#challenges.set(challenge.challengeId, challenge);
        const { targetCanonicalPath: _hidden, ...publicChallenge } = challenge;
        return publicChallenge;
    }

    async overwrite(documentId        , challengeId        , bytes            )                           {
        ensureContentBound(bytes);
        const stableBytes = Buffer.from(bytes);
        const challenge = this.#challenges.get(challengeId);
        this.#challenges.delete(challengeId);
        if (challenge === undefined || challenge.documentId !== documentId) {
            throw new SafeSaveError("challenge-invalid", "The overwrite challenge is unknown or already consumed.");
        }
        if (challenge.expiresAt < this.#now()) {
            throw new SafeSaveError("challenge-expired", "The overwrite challenge expired and was consumed.");
        }
        if (challenge.contentSha256 !== contentHash(stableBytes)) {
            throw new SafeSaveError("challenge-content-mismatch", "The overwrite content does not match the confirmed challenge.");
        }
        return await this.#locks.runExclusive(challenge.targetCanonicalPath, async () => {
            if (challenge.expiresAt < this.#now()) {
                throw new SafeSaveError("challenge-expired", "The overwrite challenge expired while waiting for the target lock.");
            }
            const targetName = posix.basename(challenge.logicalPath);
            const replacementName = `.${targetName}.replacement-${this.#nameFactory()}.tmp`;
            const backupName = `.${targetName}.backup-${this.#nameFactory()}.bak`;
            let result;
            try {
                result = await this.#nativeClient.overwrite({
                    root: this.#registry.getRootDescriptor(challenge.rootId),
                    logicalPath: challenge.logicalPath,
                    maximumBytes: this.#registry.maxDocumentBytes,
                    bytes: stableBytes,
                    expectedFingerprint: challenge.targetFingerprint,
                    replacementName,
                    backupName,
                });
            } catch (error) {
                throw this.#mapOverwriteFailure(error);
            }
            const recovery = publicRecovery(result.recovery);
            if (result.canonicalPath.toLowerCase() !== challenge.targetCanonicalPath.toLowerCase()
                || result.fingerprint.sha256 !== challenge.contentSha256
                || recovery?.target.exists !== true
                || recovery.target.sha256 !== challenge.contentSha256
                || recovery.replacement?.exists !== false
                || recovery.backup?.exists !== true
                || recovery.backup.sha256 !== challenge.targetFingerprint.sha256) {
                throw new SafeSaveError("replace-result-inconsistent", "The native overwrite success result failed hash postconditions.", {
                    recovery,
                });
            }
            const document = this.#registry.bindNativeDocument(
                documentId,
                challenge.rootId,
                challenge.logicalPath,
                result.canonicalPath,
                result.fingerprint,
            );
            return {
                status: "overwritten",
                document,
                recovery: recovery                            ,
            };
        });
    }

    #mapSaveAsFailure(error         )                {
        if (error instanceof NativeSafeFileError) {
            if (error.code === "already-exists") {
                return new SafeSaveError("overwrite-required", "The destination exists and requires overwrite confirmation.", {
                    cause: error,
                    recovery: publicRecovery(error.recovery),
                });
            }
            return new SafeSaveError("save-as-write-failed", error.message, {
                cause: error,
                recovery: publicRecovery(error.recovery),
                win32Code: error.win32Code,
            });
        }
        return new SafeSaveError("save-as-write-failed", "The native Save As transaction could not be invoked.", { cause: error });
    }

    #mapOverwriteFailure(error         )                {
        if (!(error instanceof NativeSafeFileError)) {
            return new SafeSaveError("replace-invocation-failed", "The native overwrite transaction could not be invoked.", { cause: error });
        }
        const recovery = publicRecovery(error.recovery);
        if (error.code === "postcondition-failed") {
            return new SafeSaveError("replace-result-inconsistent", error.message, {
                cause: error,
                recovery,
                win32Code: error.win32Code,
            });
        }
        if (["external-change", "backup-exists", "already-exists", "root-changed"].includes(error.code)) {
            return new SafeSaveError("external-change", error.message, {
                cause: error,
                recovery,
                win32Code: error.win32Code,
            });
        }
        if (error.code === "replace-failed") {
            return new SafeSaveError("replace-failed", error.message, {
                cause: error,
                recovery,
                win32Code: error.win32Code,
            });
        }
        return new SafeSaveError("temp-write-failed", error.message, {
            cause: error,
            recovery,
            win32Code: error.win32Code,
        });
    }
}
