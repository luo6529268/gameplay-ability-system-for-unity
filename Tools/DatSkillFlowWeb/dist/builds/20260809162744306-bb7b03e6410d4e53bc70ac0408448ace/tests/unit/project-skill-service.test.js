// dat-skill-flow-build:20260809162744306-bb7b03e6410d4e53bc70ac0408448ace
import { createHash } from "node:crypto";
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    ProjectSkillError,
    ProjectSkillService,
} from "../../src/server/project-skill-service.js";
import {
    NativeSafeFileError,
                                      
                                
                           
                              
                              
                             
} from "../../src/server/windows-safe-file-adapter.js";
import { WorkspaceRegistry } from "../../src/server/workspace-registry.js";

const rootPath = "C:/project";
const rootDescriptor                       = {
    canonicalPath: rootPath,
    volumeSerial: "volume",
    fileId: "root",
};

function digest(bytes            )         {
    return createHash("sha256").update(bytes).digest("hex");
}

class MemoryNativeClient                                 {
             files = new Map                ();
             directories           = [];

    async inspectRoot()                                {
        return rootDescriptor;
    }

    async ensureDirectory(request                              )                                     {
        this.directories.push(request.logicalPath);
        return { canonicalPath: `${request.root.canonicalPath}/${request.logicalPath}` };
    }

    async read(request                   ) {
        const key = this.#key(request.root, request.logicalPath);
        const bytes = this.files.get(key);
        if (bytes === undefined) throw new NativeSafeFileError("not-a-file", "missing");
        if (bytes.length > request.maximumBytes) throw new NativeSafeFileError("read-too-large", "too large");
        return {
            canonicalPath: `${request.root.canonicalPath}/${request.logicalPath}`,
            bytes: Buffer.from(bytes),
            fingerprint: this.#fingerprint(bytes),
        };
    }

    async saveAs(request                     ) {
        const key = this.#key(request.root, request.logicalPath);
        if (this.files.has(key)) throw new NativeSafeFileError("already-exists", "exists");
        const bytes = Buffer.from(request.bytes);
        this.files.set(key, bytes);
        return {
            canonicalPath: `${request.root.canonicalPath}/${request.logicalPath}`,
            fingerprint: this.#fingerprint(bytes),
            recovery: {
                target: {
                    path: `${request.root.canonicalPath}/${request.logicalPath}`,
                    exists: true,
                    size: bytes.length,
                    sha256: digest(bytes),
                },
            },
        };
    }

    async overwrite(request                        ) {
        const key = this.#key(request.root, request.logicalPath);
        const current = this.files.get(key);
        if (current === undefined) throw new NativeSafeFileError("not-a-file", "missing");
        const actual = this.#fingerprint(current);
        if (actual.sha256 !== request.expectedFingerprint.sha256) {
            throw new NativeSafeFileError("external-change", "changed");
        }
        const bytes = Buffer.from(request.bytes);
        this.files.set(key, bytes);
        return {
            canonicalPath: `${request.root.canonicalPath}/${request.logicalPath}`,
            fingerprint: this.#fingerprint(bytes),
            recovery: {
                target: {
                    path: `${request.root.canonicalPath}/${request.logicalPath}`,
                    exists: true,
                    size: bytes.length,
                    sha256: digest(bytes),
                },
                replacement: {
                    path: `${request.root.canonicalPath}/${request.replacementName}`,
                    exists: false,
                },
                backup: {
                    path: `${request.root.canonicalPath}/${request.backupName}`,
                    exists: true,
                    size: current.length,
                    sha256: digest(current),
                },
            },
        };
    }

    #key(root                      , logicalPath        )         {
        return `${root.canonicalPath}|${logicalPath}`;
    }

    #fingerprint(bytes            ) {
        return {
            sha256: digest(bytes),
            size: bytes.length,
            modifiedNanoseconds: "1",
            changedNanoseconds: "1",
            device: "volume",
            inode: "sidecar",
        };
    }
}

async function fixture()           
                               
                                
                                 
                   
                          
   {
    const native = new MemoryNativeClient();
    native.files.set(`${rootPath}|fighter.dat`, Buffer.from("dat"));
    const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: native });
    const { rootId } = await registry.grantAbsoluteRoot(rootPath);
    const { documentId: datDocumentId } = await registry.openDocument(rootId, "fighter.dat");
    const service = new ProjectSkillService({ registry, rootId });
    return { native, registry, service, rootId, datDocumentId };
}

describe("project skill sidecar service", () => {
    it("returns an empty non-persistent state and saves without rebinding the DAT document", async () => {
        const { native, registry, service, rootId, datDocumentId } = await fixture();
        const initial = await service.get();
        assert.deepEqual(initial.skills, []);
        assert.equal(initial.revision, 0);
        assert.equal(initial.sidecarStatus, "missing");

        const saved = await service.save({
            expectedRevision: initial.revision,
            expectedEtag: initial.etag,
            skills: [{
                oid: 2,
                startFrame: 300,
                displayName: "影分身",
                group: "输入技能",
                order: 1,
                pinned: true,
                hidden: false,
                notes: "仅用于编辑器显示",
            }],
        });
        assert.equal(saved.revision, 1);
        assert.deepEqual(saved.skills, [{
            oid: 2,
            startFrame: 300,
            displayName: "影分身",
            group: "输入技能",
            order: 1,
            pinned: true,
            hidden: false,
            notes: "仅用于编辑器显示",
        }]);
        assert.equal(saved.sidecarStatus, "valid");
        assert.deepEqual(native.directories, [".dat-skill-flow"]);
        assert.equal(registry.getDocument(datDocumentId).logicalPath, "fighter.dat");
        assert.notEqual(saved.etag, initial.etag);
        assert.equal(native.files.has(`${rootPath}|.dat-skill-flow/skills.json`), true);
    });

    it("rejects stale compare-and-swap writes and invalid sidecar content", async () => {
        const { native, service, rootId } = await fixture();
        const initial = await service.get();
        await service.save({
            expectedRevision: initial.revision,
            expectedEtag: initial.etag,
            skills: [],
        });
        await assert.rejects(
            service.save({
                expectedRevision: initial.revision,
                expectedEtag: initial.etag,
                skills: [],
            }),
            (error         ) => error instanceof ProjectSkillError && error.code === "revision-conflict",
        );

        native.files.set(`${rootPath}|.dat-skill-flow/skills.json`, Buffer.from("{\"schemaVersion\":1,\"revision\":1,\"skills\":[],\"extra\":true}"));
        const invalidRegistry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: native });
        const { rootId: invalidRootId } = await invalidRegistry.grantAbsoluteRoot(rootPath);
        const invalidService = new ProjectSkillService({ registry: invalidRegistry, rootId: invalidRootId });
        const invalid = await invalidService.get();
        assert.equal(invalid.sidecarStatus, "invalid");
        assert.deepEqual(invalid.skills, []);
        assert.equal(rootId.length > 0, true);
    });

    it("reads legacy names as display metadata, migrates on save, and validates request bounds", async () => {
        const { native, registry, rootId } = await fixture();
        native.files.set(
            `${rootPath}|.dat-skill-flow/skills.json`,
            Buffer.from("{\"schemaVersion\":1,\"revision\":3,\"skills\":[{\"oid\":2,\"name\":\"旧名称\",\"startFrame\":300}]}\n"),
        );
        const service = new ProjectSkillService({
            registry,
            rootId,
        });
        const initial = await service.get();
        assert.equal(initial.sidecarStatus, "legacy");
        assert.deepEqual(initial.skills, [{ oid: 2, startFrame: 300, displayName: "旧名称" }]);
        const saved = await service.save({
            expectedRevision: initial.revision,
            expectedEtag: initial.etag,
            skills: [{ oid: 2, displayName: "新名称", startFrame: 300 }],
        });
        assert.equal(saved.sidecarStatus, "valid");
        assert.deepEqual(saved.skills, [{ oid: 2, startFrame: 300, displayName: "新名称" }]);
        assert.doesNotMatch(
            native.files.get(`${rootPath}|.dat-skill-flow/skills.json`) .toString("utf8"),
            /"name":/,
        );
        await assert.rejects(
            service.save({
                expectedRevision: saved.revision,
                expectedEtag: saved.etag,
                skills: [{ oid: 1000, displayName: "bad", startFrame: 0 }],
            }),
            (error         ) => error instanceof ProjectSkillError && error.code === "invalid-request",
        );
    });
});
