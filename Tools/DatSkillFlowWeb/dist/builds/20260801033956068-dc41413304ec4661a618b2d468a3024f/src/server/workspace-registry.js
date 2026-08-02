// dat-skill-flow-build:20260801033956068-dc41413304ec4661a618b2d468a3024f
import { createHash, randomBytes } from "node:crypto";
import { open, realpath, stat } from "node:fs/promises";
import { isAbsolute, posix, relative, resolve, sep, win32 } from "node:path";

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

function canonicalKey(filePath        )         {
    return process.platform === "win32" ? filePath.toLowerCase() : filePath;
}

function isContained(root        , candidate        )          {
    const difference = relative(root, candidate);
    return difference !== ""
        && difference !== ".."
        && !difference.startsWith(`..${sep}`)
        && !isAbsolute(difference);
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
    if (segments.some((segment) => segment.length === 0 || segment === "." || segment === "..")) {
        throw new WorkspaceSecurityError("invalid-logical-path", "The logical path contains an unsafe segment.");
    }
    return segments.join(sep);
}

function publicLogicalPath(nativePath        )         {
    return nativePath.split(sep).join("/");
}

function sameStat(left                                  , right                                  )          {
    return left.dev === right.dev
        && left.ino === right.ino
        && left.size === right.size
        && left.mtimeNs === right.mtimeNs
        && left.ctimeNs === right.ctimeNs;
}

function fingerprintFrom(bytes        , metadata                                  )                  {
    return Object.freeze({
        sha256: createHash("sha256").update(bytes).digest("hex"),
        size: Number(metadata.size),
        modifiedNanoseconds: metadata.mtimeNs.toString(),
        changedNanoseconds: metadata.ctimeNs.toString(),
        device: metadata.dev.toString(),
        inode: metadata.ino.toString(),
    });
}

export function fingerprintsEqual(left                 , right                 )          {
    return left.sha256 === right.sha256
        && left.size === right.size
        && left.modifiedNanoseconds === right.modifiedNanoseconds
        && left.changedNanoseconds === right.changedNanoseconds
        && left.device === right.device
        && left.inode === right.inode;
}

async function readBounded(canonicalPath        , maximumBytes        )           
                  
                                 
   {
    const handle = await open(canonicalPath, "r");
    try {
        const before = await handle.stat({ bigint: true });
        if (!before.isFile()) {
            throw new WorkspaceSecurityError("not-a-file", "The selected document is not a regular file.");
        }
        if (before.size > BigInt(maximumBytes)) {
            throw new WorkspaceSecurityError("read-too-large", "The selected document exceeds the read limit.");
        }
        const chunks           = [];
        let total = 0;
        while (true) {
            const chunk = Buffer.allocUnsafe(Math.min(64 * 1024, maximumBytes + 1 - total));
            const result = await handle.read(chunk, 0, chunk.length, null);
            if (result.bytesRead === 0) {
                break;
            }
            total += result.bytesRead;
            if (total > maximumBytes) {
                throw new WorkspaceSecurityError("read-too-large", "The selected document grew beyond the read limit.");
            }
            chunks.push(chunk.subarray(0, result.bytesRead));
        }
        const after = await handle.stat({ bigint: true });
        if (!sameStat(before, after) || after.size !== BigInt(total)) {
            throw new WorkspaceSecurityError("file-changed-during-read", "The selected document changed while it was read.");
        }
        const bytes = Buffer.concat(chunks, total);
        return { bytes, fingerprint: fingerprintFrom(bytes, after) };
    } finally {
        await handle.close();
    }
}

export class WorkspaceRegistry {
             #allowAbsoluteRootGrant         ;
             #maxDocumentBytes        ;
             #roots = new Map                    ();
             #documents = new Map                        ();

    constructor(options                           = {}) {
        this.#allowAbsoluteRootGrant = options.allowAbsoluteRootGrant === true;
        this.#maxDocumentBytes = options.maxDocumentBytes ?? MAX_DOCUMENT_BYTES;
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
        let canonicalPath        ;
        try {
            canonicalPath = await realpath(resolve(absoluteRoot));
            const metadata = await stat(canonicalPath);
            if (!metadata.isDirectory()) {
                throw new WorkspaceSecurityError("invalid-root", "The selected root is not a directory.");
            }
        } catch (error) {
            if (error instanceof WorkspaceSecurityError) {
                throw error;
            }
            throw new WorkspaceSecurityError("invalid-root", "The selected root cannot be canonicalized.", { cause: error });
        }
        const existing = [...this.#roots.values()].find((record) => canonicalKey(record.canonicalPath) === canonicalKey(canonicalPath));
        if (existing !== undefined) {
            return { rootId: existing.rootId };
        }
        const rootId = opaqueId();
        this.#roots.set(rootId, { rootId, canonicalPath });
        return { rootId };
    }

    getDocument(documentId        )                           {
        const document = this.#documents.get(documentId);
        if (document === undefined) {
            throw new WorkspaceSecurityError("unknown-document", "The document ID is unknown.");
        }
        return document;
    }

    async resolveTarget(rootId        , logicalPath        )                                   {
        const root = this.#roots.get(rootId);
        if (root === undefined) {
            throw new WorkspaceSecurityError("unknown-root", "The root ID is unknown.");
        }
        const normalized = normalizeLogicalPath(logicalPath);
        const lexicalCandidate = resolve(root.canonicalPath, normalized);
        if (!isContained(root.canonicalPath, lexicalCandidate)) {
            throw new WorkspaceSecurityError("root-escape", "The requested path escapes the selected root.");
        }
        try {
            const canonicalPath = await realpath(lexicalCandidate);
            if (!isContained(root.canonicalPath, canonicalPath)) {
                throw new WorkspaceSecurityError("root-escape", "The requested path resolves outside the selected root.");
            }
            return {
                rootId,
                logicalPath: publicLogicalPath(normalized),
                canonicalPath,
                exists: true,
            };
        } catch (error) {
            if (error instanceof WorkspaceSecurityError) {
                throw error;
            }
            if ((error                         ).code !== "ENOENT") {
                throw error;
            }
        }

        const lastSeparator = normalized.lastIndexOf(sep);
        const parentLogical = lastSeparator < 0 ? "" : normalized.slice(0, lastSeparator);
        const fileName = lastSeparator < 0 ? normalized : normalized.slice(lastSeparator + 1);
        const requestedParent = parentLogical.length === 0
            ? root.canonicalPath
            : resolve(root.canonicalPath, parentLogical);
        const firstParent = await realpath(requestedParent);
        const secondParent = await realpath(requestedParent);
        if (canonicalKey(firstParent) !== canonicalKey(secondParent)
            || (canonicalKey(secondParent) !== canonicalKey(root.canonicalPath) && !isContained(root.canonicalPath, secondParent))) {
            throw new WorkspaceSecurityError("root-escape", "The requested parent resolves outside or changed within the selected root.");
        }
        const canonicalPath = resolve(secondParent, fileName);
        if (!isContained(root.canonicalPath, canonicalPath)) {
            throw new WorkspaceSecurityError("root-escape", "The requested target escapes the selected root.");
        }
        return {
            rootId,
            logicalPath: publicLogicalPath(normalized),
            canonicalPath,
            exists: false,
        };
    }

    async openDocument(rootId        , logicalPath        )                          {
        const target = await this.resolveTarget(rootId, logicalPath);
        if (!target.exists) {
            throw new WorkspaceSecurityError("not-a-file", "The requested document does not exist.");
        }
        const { fingerprint } = await readBounded(target.canonicalPath, this.#maxDocumentBytes);
        const documentId = opaqueId();
        const document = Object.freeze({
            documentId,
            rootId,
            logicalPath: target.logicalPath,
            canonicalPath: target.canonicalPath,
            fingerprint,
        });
        this.#documents.set(documentId, document);
        return {
            documentId,
            rootId,
            logicalPath: target.logicalPath,
            fingerprint,
        };
    }

    async readDocument(documentId        )                        {
        const document = this.getDocument(documentId);
        const target = await this.resolveTarget(document.rootId, document.logicalPath);
        if (!target.exists || canonicalKey(target.canonicalPath) !== canonicalKey(document.canonicalPath)) {
            throw new WorkspaceSecurityError("root-escape", "The document no longer resolves to its registered target.");
        }
        const current = await readBounded(target.canonicalPath, this.#maxDocumentBytes);
        return {
            documentId,
            rootId: document.rootId,
            logicalPath: document.logicalPath,
            bytes: current.bytes,
            fingerprint: current.fingerprint,
            externallyModified: !fingerprintsEqual(document.fingerprint, current.fingerprint),
        };
    }

    async fingerprintTarget(target                         )                           {
        if (!target.exists) {
            throw new WorkspaceSecurityError("not-a-file", "The requested target does not exist.");
        }
        return (await readBounded(target.canonicalPath, this.#maxDocumentBytes)).fingerprint;
    }

    async rebindDocument(documentId        , rootId        , logicalPath        )                          {
        this.getDocument(documentId);
        const target = await this.resolveTarget(rootId, logicalPath);
        if (!target.exists) {
            throw new WorkspaceSecurityError("not-a-file", "The published document is missing.");
        }
        const { fingerprint } = await readBounded(target.canonicalPath, this.#maxDocumentBytes);
        const replacement = Object.freeze({
            documentId,
            rootId,
            logicalPath: target.logicalPath,
            canonicalPath: target.canonicalPath,
            fingerprint,
        });
        this.#documents.set(documentId, replacement);
        return {
            documentId,
            rootId,
            logicalPath: target.logicalPath,
            fingerprint,
        };
    }
}
