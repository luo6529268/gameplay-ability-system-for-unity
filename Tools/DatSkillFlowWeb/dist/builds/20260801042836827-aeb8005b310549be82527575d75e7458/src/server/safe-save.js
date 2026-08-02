// dat-skill-flow-build:20260801042836827-aeb8005b310549be82527575d75e7458
import { createHash, randomUUID } from "node:crypto";
import { lstat as nodeLstat, open as nodeOpen } from "node:fs/promises";
import { basename, dirname, join } from "node:path";

import {
    fingerprintsEqual,
    MAX_DOCUMENT_BYTES,
                         
                        
                                 
    WorkspaceRegistry,
} from "./workspace-registry.js";
import {
                          
                       
    WindowsReplaceFilePublisher,
} from "./windows-replace-adapter.js";

                                                                     

export const DEFAULT_OVERWRITE_CHALLENGE_TTL_MS = 30_000;
const MAX_EXCLUSIVE_NAME_ATTEMPTS = 64;

                          
                                               
                          
                           
 

                              
                                                                   
                                                            
 

                                
                                       
                                            
                                    
 

                                         
                                 
                                             
                          
                               
                       
                            
 

                                  
                 
                    
                  
                    
                             
 

                                 
                            
                                  
                             
 

                               
                         
                          
                            
                         
                         
                                  
                                 
                       
                         
                                 
                      
                                    

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

    async runExclusive   (canonicalTarget        , operation                  )             {
        const key = process.platform === "win32" ? canonicalTarget.toLowerCase() : canonicalTarget;
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

function sameCanonicalPath(left        , right        )          {
    return process.platform === "win32" ? left.toLowerCase() === right.toLowerCase() : left === right;
}

async function observePath(filePath        )                           {
    try {
        const handle = await nodeOpen(filePath, "r");
        try {
            const metadata = await handle.stat();
            if (!metadata.isFile()) {
                return { path: filePath, exists: true, inspectionError: "not-a-regular-file" };
            }
            if (metadata.size > MAX_DOCUMENT_BYTES) {
                return { path: filePath, exists: true, size: metadata.size, inspectionError: "file-exceeds-inspection-limit" };
            }
            const chunks           = [];
            let total = 0;
            while (true) {
                const chunk = Buffer.allocUnsafe(Math.min(64 * 1024, MAX_DOCUMENT_BYTES + 1 - total));
                const read = await handle.read(chunk, 0, chunk.length, null);
                if (read.bytesRead === 0) {
                    break;
                }
                total += read.bytesRead;
                if (total > MAX_DOCUMENT_BYTES) {
                    return { path: filePath, exists: true, size: total, inspectionError: "file-grew-beyond-inspection-limit" };
                }
                chunks.push(chunk.subarray(0, read.bytesRead));
            }
            const bytes = Buffer.concat(chunks, total);
            return {
                path: filePath,
                exists: true,
                size: total,
                sha256: contentHash(bytes),
            };
        } finally {
            await handle.close();
        }
    } catch (error) {
        if ((error                         ).code === "ENOENT") {
            return { path: filePath, exists: false };
        }
        return {
            path: filePath,
            exists: false,
            inspectionError: (error                         ).code ?? "inspection-failed",
        };
    }
}

async function observeRecovery(targetPath        , replacementPath         , backupPath         )                          {
    const [target, replacement, backup] = await Promise.all([
        observePath(targetPath),
        replacementPath === undefined ? undefined : observePath(replacementPath),
        backupPath === undefined ? undefined : observePath(backupPath),
    ]);
    return {
        target,
        ...(replacement === undefined ? {} : { replacement }),
        ...(backup === undefined ? {} : { backup }),
    };
}

export class SafeSaveService {
             #registry                   ;
             #publisher                  ;
             #fileSystem                    ;
             #hooks               ;
             #nameFactory              ;
             #now              ;
             #challengeTtlMs        ;
             #locks = new TargetLockManager();
             #challenges = new Map                         ();

    constructor(registry                   , options                         = {}) {
        this.#registry = registry;
        this.#publisher = options.publisher ?? new WindowsReplaceFilePublisher();
        this.#fileSystem = {
            open: options.fileSystem?.open ?? (async (filePath, flags) => await nodeOpen(filePath, flags)),
            lstat: options.fileSystem?.lstat ?? nodeLstat,
        };
        this.#hooks = options.hooks ?? {};
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
        const stableBytes = Buffer.from(bytes);
        this.#registry.getDocument(documentId);
        const initialTarget = await this.#registry.resolveTarget(rootId, logicalPath);
        return await this.#locks.runExclusive(initialTarget.canonicalPath, async () => {
            const target = await this.#registry.resolveTarget(rootId, logicalPath);
            if (!sameCanonicalPath(initialTarget.canonicalPath, target.canonicalPath)) {
                throw new SafeSaveError("external-change", "The Save As target changed during canonicalization.");
            }
            if (target.exists) {
                throw new SafeSaveError("overwrite-required", "The destination exists and requires an overwrite challenge.", {
                    recovery: await observeRecovery(target.canonicalPath),
                });
            }
            await this.#hooks.beforeSaveAsOpen?.();
            let handle                            ;
            let failure         ;
            try {
                handle = await this.#fileSystem.open(target.canonicalPath, "wx");
                await handle.writeFile(stableBytes);
                await handle.sync();
            } catch (error) {
                failure = error;
            }
            if (handle !== undefined) {
                try {
                    await handle.close();
                } catch (error) {
                    failure ??= error;
                }
            }
            if (failure !== undefined) {
                const code = (failure                         ).code;
                if (code === "EEXIST") {
                    throw new SafeSaveError("overwrite-required", "The destination was created concurrently and requires an overwrite challenge.", {
                        cause: failure,
                        recovery: await observeRecovery(target.canonicalPath),
                    });
                }
                throw new SafeSaveError("save-as-write-failed", "Save As did not complete; the final path was preserved for recovery.", {
                    cause: failure,
                    recovery: await observeRecovery(target.canonicalPath),
                });
            }
            const document = await this.#registry.openDocument(rootId, logicalPath);
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
        const target = await this.#registry.resolveTarget(rootId, logicalPath);
        if (!target.exists) {
            throw new SafeSaveError("challenge-target-mismatch", "The overwrite target does not exist; use Save As.");
        }
        const targetFingerprint = await this.#registry.fingerprintTarget(target);
        if (sameCanonicalPath(document.canonicalPath, target.canonicalPath)
            && !fingerprintsEqual(document.fingerprint, targetFingerprint)) {
            throw new SafeSaveError("external-change", "The loaded document changed before overwrite confirmation.");
        }
        const challenge                  = Object.freeze({
            challengeId: randomUUID(),
            documentId,
            rootId,
            logicalPath: target.logicalPath,
            canonicalTarget: target.canonicalPath,
            contentSha256: contentHash(stableBytes),
            targetFingerprint,
            expiresAt: this.#now() + this.#challengeTtlMs,
        });
        this.#challenges.set(challenge.challengeId, challenge);
        const { canonicalTarget: _hidden, ...publicChallenge } = challenge;
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
        return await this.#locks.runExclusive(challenge.canonicalTarget, async () => {
            if (challenge.expiresAt < this.#now()) {
                throw new SafeSaveError("challenge-expired", "The overwrite challenge expired while waiting for the target lock.");
            }
            const target = await this.#registry.resolveTarget(challenge.rootId, challenge.logicalPath);
            if (!target.exists || !sameCanonicalPath(target.canonicalPath, challenge.canonicalTarget)) {
                throw new SafeSaveError("challenge-target-mismatch", "The overwrite target no longer matches the confirmed target.");
            }
            const commitFingerprint = await this.#registry.fingerprintTarget(target);
            if (!fingerprintsEqual(commitFingerprint, challenge.targetFingerprint)) {
                throw new SafeSaveError("external-change", "The overwrite target changed after confirmation.", {
                    recovery: await observeRecovery(target.canonicalPath),
                });
            }

            const backupPath = await this.#selectNonexistingSibling(target, "backup", ".bak");
            let replacementPath                    ;
            try {
                replacementPath = await this.#writeReplacement(target, stableBytes);
            } catch (error) {
                if (error instanceof SafeSaveError) {
                    throw error;
                }
                throw new SafeSaveError("temp-write-failed", "The replacement temporary file could not be completed.", {
                    cause: error,
                    recovery: await observeRecovery(target.canonicalPath, replacementPath, backupPath),
                });
            }

            await this.#hooks.beforeOverwriteRehash?.();
            const finalTarget = await this.#registry.resolveTarget(challenge.rootId, challenge.logicalPath);
            if (!finalTarget.exists || !sameCanonicalPath(finalTarget.canonicalPath, challenge.canonicalTarget)) {
                throw new SafeSaveError("challenge-target-mismatch", "The overwrite target changed immediately before publication.", {
                    recovery: await observeRecovery(target.canonicalPath, replacementPath, backupPath),
                });
            }
            const finalFingerprint = await this.#registry.fingerprintTarget(finalTarget);
            if (!fingerprintsEqual(finalFingerprint, challenge.targetFingerprint)) {
                throw new SafeSaveError("external-change", "The overwrite target changed immediately before publication.", {
                    recovery: await observeRecovery(finalTarget.canonicalPath, replacementPath, backupPath),
                });
            }
            try {
                await this.#fileSystem.lstat(backupPath);
                throw new SafeSaveError("external-change", "The reserved backup name was claimed before publication.", {
                    recovery: await observeRecovery(finalTarget.canonicalPath, replacementPath, backupPath),
                });
            } catch (error) {
                if (error instanceof SafeSaveError) {
                    throw error;
                }
                if ((error                         ).code !== "ENOENT") {
                    throw error;
                }
            }
            await this.#hooks.beforePublish?.();
            let publication               ;
            try {
                publication = await this.#publisher.replace({
                    targetPath: finalTarget.canonicalPath,
                    replacementPath,
                    backupPath,
                });
            } catch (error) {
                throw new SafeSaveError("replace-invocation-failed", "ReplaceFileW could not be invoked; all observed recovery paths were preserved.", {
                    cause: error,
                    recovery: await observeRecovery(finalTarget.canonicalPath, replacementPath, backupPath),
                });
            }
            const recovery = await observeRecovery(finalTarget.canonicalPath, replacementPath, backupPath)                            ;
            if (!publication.ok) {
                throw new SafeSaveError("replace-failed", publication.message, {
                    win32Code: publication.win32Code,
                    recovery,
                });
            }
            if (!recovery.target.exists || recovery.replacement.exists || !recovery.backup.exists) {
                throw new SafeSaveError("replace-result-inconsistent", "ReplaceFileW reported success but the three recovery paths were inconsistent.", {
                    recovery,
                });
            }
            const document = await this.#registry.rebindDocument(documentId, challenge.rootId, challenge.logicalPath);
            return { status: "overwritten", document, recovery };
        });
    }

    async #selectNonexistingSibling(
        target                         ,
        kind                          ,
        extension        ,
    )                  {
        const directory = dirname(target.canonicalPath);
        const targetName = basename(target.canonicalPath);
        for (let attempt = 0; attempt < MAX_EXCLUSIVE_NAME_ATTEMPTS; attempt += 1) {
            const candidate = join(directory, `.${targetName}.${kind}-${this.#nameFactory()}${extension}`);
            try {
                await this.#fileSystem.lstat(candidate);
            } catch (error) {
                if ((error                         ).code === "ENOENT") {
                    return candidate;
                }
                throw error;
            }
        }
        throw new Error(`Unable to choose a unique ${kind} path.`);
    }

    async #writeReplacement(target                         , bytes            )                  {
        for (let attempt = 0; attempt < MAX_EXCLUSIVE_NAME_ATTEMPTS; attempt += 1) {
            const replacementPath = await this.#selectNonexistingSibling(target, "replacement", ".tmp");
            let handle                ;
            try {
                handle = await this.#fileSystem.open(replacementPath, "wx");
            } catch (error) {
                if ((error                         ).code === "EEXIST") {
                    continue;
                }
                throw error;
            }
            let failure         ;
            try {
                await handle.writeFile(bytes);
                await handle.sync();
            } catch (error) {
                failure = error;
            }
            try {
                await handle.close();
            } catch (error) {
                failure ??= error;
            }
            if (failure !== undefined) {
                throw new SafeSaveError("temp-write-failed", "The replacement temporary file is incomplete and was preserved.", {
                    cause: failure,
                    recovery: await observeRecovery(target.canonicalPath, replacementPath),
                });
            }
            return replacementPath;
        }
        throw new Error("Unable to create an exclusive replacement temporary file.");
    }
}
