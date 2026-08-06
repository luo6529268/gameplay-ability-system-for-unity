// dat-skill-flow-build:20260806142111218-e0d3a43dddbf4ce38f99c8853e113bc9
import { createHash } from "node:crypto";

import {
    SafeSaveError,
    SafeSaveService,
                            
} from "./safe-save.js";
import {
                         
    WorkspaceRegistry,
    WorkspaceSecurityError,
} from "./workspace-registry.js";

const SIDECAR_PATH = ".dat-skill-flow/skills.json";
const SIDECAR_MAX_BYTES = 256 * 1024;
const MAX_SKILLS = 1000;
const EMPTY_SIDECAR = Object.freeze({
    schemaVersion: 1,
    revision: 0,
    skills: []                           ,
});
const EMPTY_ETAG = hashBytes(serializeSidecar(EMPTY_SIDECAR));

                               
                         
                          
                                
 

                                    
                              
                              
                          
                                             
 

                                   
                        
                      
                         
                   
                        

export class ProjectSkillError extends Error {
             code                       ;

    constructor(code                       , message        , options               ) {
        super(message, options);
        this.name = "ProjectSkillError";
        this.code = code;
    }
}

                                             
                                         
                            
                                        
 

                         
                              
                              
                                             
 

                           
                                 
                                           
                           
                                  
                          
 

function hashBytes(bytes            )         {
    return createHash("sha256").update(bytes).digest("hex");
}

function serializeSidecar(value               )         {
    return Buffer.from(JSON.stringify({
        schemaVersion: value.schemaVersion,
        revision: value.revision,
        skills: value.skills,
    }) + "\n", "utf8");
}

function isRecord(value         )                                   {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function exactKeys(
    value                         ,
    keys                   ,
    label        ,
    code                        = "schema-invalid",
)       {
    const actual = Object.keys(value);
    if (actual.length !== keys.length || actual.some((key) => !keys.includes(key))) {
        throw new ProjectSkillError(code, `${label} contains missing or unknown fields.`);
    }
}

function integer(
    value         ,
    minimum        ,
    maximum        ,
    label        ,
    code                        = "schema-invalid",
)         {
    if (!Number.isSafeInteger(value) || (value          ) < minimum || (value          ) > maximum) {
        throw new ProjectSkillError(code, `${label} is outside its supported range.`);
    }
    return value          ;
}

function skillName(value         , code                       )         {
    if (typeof value !== "string"
        || value.length === 0
        || Buffer.byteLength(value, "utf8") > 256
        || /[\u0000-\u001f\u007f-\u009f]/u.test(value)
        || /[\uD800-\uDFFF]/u.test(value)) {
        throw new ProjectSkillError(code, "Skill names must be valid UTF-8 text without control characters.");
    }
    return value;
}

function parseSkill(value         , index        , code                       )               {
    if (!isRecord(value)) {
        throw new ProjectSkillError(code, `Skill ${index} must be an object.`);
    }
    exactKeys(value, ["oid", "name", "startFrame"], `Skill ${index}`, code);
    return Object.freeze({
        oid: integer(value.oid, 0, 999, `Skill ${index}.oid`, code),
        name: skillName(value.name, code),
        startFrame: integer(value.startFrame, 0, 599, `Skill ${index}.startFrame`, code),
    });
}

function parseSidecarBytes(bytes            )                {
    if (bytes.byteLength > SIDECAR_MAX_BYTES) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar exceeds its size limit.");
    }
    let text        ;
    try {
        text = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
    } catch (error) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar is not valid UTF-8.", { cause: error });
    }
    let parsed         ;
    try {
        parsed = JSON.parse(text)           ;
    } catch (error) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar is not valid JSON.", { cause: error });
    }
    if (!isRecord(parsed)) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar root must be an object.");
    }
    exactKeys(parsed, ["schemaVersion", "revision", "skills"], "The skill sidecar");
    if (parsed.schemaVersion !== 1) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar schema version is unsupported.");
    }
    const revision = integer(parsed.revision, 0, Number.MAX_SAFE_INTEGER, "revision");
    if (!Array.isArray(parsed.skills) || parsed.skills.length > MAX_SKILLS) {
        throw new ProjectSkillError("schema-invalid", "The skill sidecar contains too many skills.");
    }
    const skills = parsed.skills.map((item, index) => parseSkill(item, index, "schema-invalid"));
    return Object.freeze({ schemaVersion: 1, revision, skills: Object.freeze(skills) });
}

function parseRequestSkills(value         )                          {
    if (!Array.isArray(value) || value.length > MAX_SKILLS) {
        throw new ProjectSkillError("invalid-request", "skills must contain at most 1000 items.");
    }
    return Object.freeze(value.map((item, index) => parseSkill(item, index, "invalid-request")));
}

function mapFileError(error         , message        )                    {
    if (error instanceof WorkspaceSecurityError && error.code === "not-a-file") {
        return new ProjectSkillError("save-failed", message, { cause: error });
    }
    if (error instanceof SafeSaveError) {
        return new ProjectSkillError(
            error.code === "external-change" || error.code === "overwrite-required"
                ? "revision-conflict"
                : "save-failed",
            error.message,
            { cause: error },
        );
    }
    return new ProjectSkillError("save-failed", message, { cause: error });
}

export class ProjectSkillService {
             #registry                   ;
             #rootId        ;
             #safeSave                 ;
             #path        ;
    #documentId                    ;
    #directoryReady = false;
    #tail                = Promise.resolve();

    constructor(options                            ) {
        this.#registry = options.registry;
        this.#rootId = options.rootId;
        this.#safeSave = options.safeSave ?? new SafeSaveService(this.#registry);
        this.#path = this.#registry.normalizeLogicalPath(SIDECAR_PATH);
    }

    async get()                             {
        return await this.#exclusive(async () => {
            const snapshot = await this.#readSnapshot();
            return this.#view(snapshot);
        });
    }

    async save(input         )                             {
        if (!isRecord(input)) {
            throw new ProjectSkillError("invalid-request", "The skill sidecar request must be an object.");
        }
        exactKeys(
            input,
            ["expectedRevision", "expectedEtag", "skills"],
            "The skill sidecar request",
            "invalid-request",
        );
        const expectedRevision = integer(
            input.expectedRevision,
            0,
            Number.MAX_SAFE_INTEGER,
            "expectedRevision",
            "invalid-request",
        );
        const expectedEtag = input.expectedEtag;
        if (typeof expectedEtag !== "string" || !/^[a-f0-9]{64}$/u.test(expectedEtag)) {
            throw new ProjectSkillError("invalid-request", "expectedEtag must be a SHA-256 digest.");
        }
        const skills = parseRequestSkills(input.skills);
        return await this.#exclusive(async () => {
            const current = await this.#readSnapshot();
            if (current.value.revision !== expectedRevision || current.etag !== expectedEtag) {
                throw new ProjectSkillError("revision-conflict", "The skill sidecar changed before it was saved.");
            }
            if (current.value.revision === Number.MAX_SAFE_INTEGER) {
                throw new ProjectSkillError("save-failed", "The skill sidecar revision is exhausted.");
            }
            const next = Object.freeze({
                schemaVersion: 1         ,
                revision: current.value.revision + 1,
                skills,
            });
            const bytes = serializeSidecar(next);
            if (bytes.length > SIDECAR_MAX_BYTES) {
                throw new ProjectSkillError("invalid-request", "The skill sidecar exceeds its size limit.");
            }
            const document = await this.#write(current, bytes);
            return this.#view({
                documentId: document.documentId,
                fingerprint: document.fingerprint,
                bytes,
                value: next,
                etag: document.fingerprint.sha256,
            });
        });
    }

    async #write(current                 , bytes        )                                                                {
        try {
            if (!this.#directoryReady) {
                await this.#registry.nativeClient.ensureDirectory({
                    root: this.#registry.getRootDescriptor(this.#rootId),
                    logicalPath: ".dat-skill-flow",
                });
                this.#directoryReady = true;
            }
            if (current.documentId === undefined || current.fingerprint === undefined) {
                const created = await this.#safeSave.saveAsNew(this.#rootId, this.#path, bytes);
                this.#documentId = created.document.documentId;
                return created.document;
            }
            const challenge                     = await this.#safeSave.issueOverwriteChallenge(
                current.documentId,
                this.#rootId,
                this.#path,
                bytes,
            );
            const overwritten = await this.#safeSave.overwrite(current.documentId, challenge.challengeId, bytes);
            this.#documentId = overwritten.document.documentId;
            return overwritten.document;
        } catch (error) {
            this.#directoryReady = false;
            throw mapFileError(error, "The skill sidecar could not be saved safely.");
        }
    }

    async #readSnapshot()                           {
        let documentId = this.#documentId;
        if (documentId === undefined) {
            try {
                documentId = (await this.#registry.openDocument(this.#rootId, this.#path)).documentId;
                this.#documentId = documentId;
            } catch (error) {
                if (error instanceof WorkspaceSecurityError && error.code === "not-a-file") {
                    return this.#emptySnapshot();
                }
                if (error instanceof WorkspaceSecurityError && error.code === "read-too-large") {
                    throw new ProjectSkillError("schema-invalid", "The skill sidecar exceeds its size limit.", { cause: error });
                }
                throw new ProjectSkillError("save-failed", "The skill sidecar could not be opened safely.", { cause: error });
            }
        }

        let prepared;
        try {
            prepared = await this.#registry.prepareDocumentRefresh(documentId);
        } catch (error) {
            if (error instanceof WorkspaceSecurityError && error.code === "not-a-file") {
                this.#registry.closeDocument(documentId);
                this.#documentId = undefined;
                return this.#emptySnapshot();
            }
            if (error instanceof WorkspaceSecurityError && error.code === "read-too-large") {
                throw new ProjectSkillError("schema-invalid", "The skill sidecar exceeds its size limit.", { cause: error });
            }
            throw new ProjectSkillError("save-failed", "The skill sidecar could not be read safely.", { cause: error });
        }
        const current = prepared.snapshot;
        const value = parseSidecarBytes(current.bytes);
        prepared.commit();
        return {
            documentId,
            fingerprint: current.fingerprint,
            bytes: current.bytes,
            value,
            etag: current.fingerprint.sha256,
        };
    }

    #emptySnapshot()                  {
        return { bytes: serializeSidecar(EMPTY_SIDECAR), value: EMPTY_SIDECAR, etag: EMPTY_ETAG };
    }

    #view(snapshot                 )                    {
        return {
            schemaVersion: 1,
            revision: snapshot.value.revision,
            etag: snapshot.etag,
            skills: snapshot.value.skills.map((skill) => ({ ...skill })),
        };
    }

    async #exclusive   (operation                  )             {
        const previous = this.#tail;
        let release             ;
        this.#tail = new Promise      ((resolve) => { release = resolve; });
        await previous;
        try {
            return await operation();
        } finally {
            release();
        }
    }
}
