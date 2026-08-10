// dat-skill-flow-build:20260810005501884-debc800001fd499cac56a1556ff229f3
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import { SafeSaveError, SafeSaveService } from "../../src/server/safe-save.js";
import {
    PowerShellWindowsSafeFileClient,
                              
} from "../../src/server/windows-safe-file-adapter.js";
import { WorkspaceRegistry } from "../../src/server/workspace-registry.js";

const windowsOnly = { skip: process.platform !== "win32" }         ;

function nativeClient()                                  {
    return new PowerShellWindowsSafeFileClient({ scriptPath: resolve("scripts/windows-safe-file.ps1") });
}

async function fixture(label        )           
                 
                                            
                                
                   
                       
   {
    const root = await mkdtemp(join(tmpdir(), `dat-flow-safe-save-${label}-`));
    process.stdout.write(`[safe-test-artifacts] ${root}\n`);
    await writeFile(join(root, "source.dat"), "original");
    const client = nativeClient();
    const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
    const { rootId } = await registry.grantAbsoluteRoot(root);
    const { documentId } = await registry.openDocument(rootId, "source.dat");
    return { root, client, registry, rootId, documentId };
}

describe("recoverable safe-save protocol", windowsOnly, () => {
    it("binds overwrite challenges to content and consumes them once", async () => {
        const { client, registry, rootId, documentId } = await fixture("challenge");
        const service = new SafeSaveService(registry, { nativeClient: client });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, Buffer.from("different")),
            (error         ) => error instanceof SafeSaveError && error.code === "challenge-content-mismatch",
        );
        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => error instanceof SafeSaveError && error.code === "challenge-invalid",
        );
    });

    it("rejects an external target mutation under the native overwrite transaction", async () => {
        const { root, client, registry, rootId, documentId } = await fixture("external-change");
        const service = new SafeSaveService(registry, { nativeClient: client });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);
        await writeFile(join(root, "source.dat"), "external mutation");

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => error instanceof SafeSaveError && error.code === "external-change",
        );
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "external mutation");
    });

    it("expires and consumes a challenge", async () => {
        const { client, registry, rootId, documentId } = await fixture("expiry");
        let now = 1_000;
        const service = new SafeSaveService(registry, {
            nativeClient: client,
            now: () => now,
            challengeTtlMs: 10,
        });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);
        now = challenge.expiresAt + 1;

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => error instanceof SafeSaveError && error.code === "challenge-expired",
        );
        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => error instanceof SafeSaveError && error.code === "challenge-invalid",
        );
    });

    it("serializes concurrent Save As requests by opaque root and logical target", async () => {
        const { client, registry, rootId, documentId } = await fixture("lock");
        let active = 0;
        let peak = 0;
        const delayed                       = {
            inspectRoot: (request) => client.inspectRoot(request),
            ensureDirectory: (request) => client.ensureDirectory(request),
            read: (request) => client.read(request),
            overwrite: (request) => client.overwrite(request),
            async saveAs(request) {
                active += 1;
                peak = Math.max(peak, active);
                await new Promise((resolveDelay) => setTimeout(resolveDelay, 25));
                active -= 1;
                const sha256 = createHash("sha256").update(request.bytes).digest("hex");
                const canonicalPath = join(request.root.canonicalPath, ...request.logicalPath.split("/"));
                return {
                    canonicalPath,
                    fingerprint: {
                        sha256,
                        size: request.bytes.byteLength,
                        modifiedNanoseconds: "1",
                        changedNanoseconds: "1",
                        device: "1",
                        inode: "1",
                    },
                    recovery: {
                        target: {
                            path: canonicalPath,
                            exists: true,
                            size: request.bytes.byteLength,
                            sha256,
                        },
                    },
                };
            },
        };
        const service = new SafeSaveService(registry, { nativeClient: delayed });

        const results = await Promise.all([
            service.saveAs(documentId, rootId, "contended.dat", Buffer.from("one")),
            service.saveAs(documentId, rootId, "contended.dat", Buffer.from("two")),
        ]);

        assert.equal(peak, 1);
        assert.equal(results.length, 2);
    });
});
