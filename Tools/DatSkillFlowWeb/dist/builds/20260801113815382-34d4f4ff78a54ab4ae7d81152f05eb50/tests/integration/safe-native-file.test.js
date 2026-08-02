// dat-skill-flow-build:20260801113815382-34d4f4ff78a54ab4ae7d81152f05eb50
import assert from "node:assert/strict";
import {
    mkdtemp,
    lstat,
    readFile,
    rename,
    symlink,
    unlink,
    writeFile,
} from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import { SafeSaveError, SafeSaveService } from "../../src/server/safe-save.js";
import {
    PowerShellWindowsSafeFileClient,
    NativeSafeFileError,
                              
                             
} from "../../src/server/windows-safe-file-adapter.js";
import { WorkspaceRegistry, WorkspaceSecurityError } from "../../src/server/workspace-registry.js";

const windowsOnly = { skip: process.platform !== "win32" }         ;

async function temporaryRoot(label        )                  {
    const root = await mkdtemp(join(tmpdir(), `dat-flow-native-${label}-`));
    process.stdout.write(`[safe-test-artifacts] ${root}\n`);
    return root;
}

function nativeClient(
    hooks                      = {},
)                                  {
    return new PowerShellWindowsSafeFileClient({
        scriptPath: resolve("scripts/windows-safe-file.ps1"),
        hooks,
    });
}

describe("Windows handle-safe file transactions", windowsOnly, () => {
    it("allows a bounded read whose base64 response is larger than the fixed protocol envelope", async () => {
        const root = await temporaryRoot("large-read");
        const bytes = Buffer.alloc(1024 * 1024 + 17, 0x5a);
        await writeFile(join(root, "large.dat"), bytes);
        const client = nativeClient();
        const rootDescriptor = await client.inspectRoot({ absoluteRoot: root });

        const result = await client.read({
            root: rootDescriptor,
            logicalPath: "large.dat",
            maximumBytes: 2 * 1024 * 1024,
        });

        assert.equal(result.bytes.length, bytes.length);
        assert.deepEqual(result.bytes, bytes);
    });

    it("reads from a validated handle and creates a new file without overwriting", async () => {
        const root = await temporaryRoot("read-save-as");
        await writeFile(join(root, "source.dat"), "original");
        const client = nativeClient();
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const opened = await registry.openDocument(rootId, "source.dat");

        const read = await registry.readDocument(opened.documentId);
        assert.equal(read.bytes.toString("utf8"), "original");
        assert.equal(read.fingerprint.sha256, opened.fingerprint.sha256);

        const service = new SafeSaveService(registry, { nativeClient: client });
        const created = await service.saveAs(opened.documentId, rootId, "copy.dat", Buffer.from("copy"));
        assert.equal(created.status, "created");
        assert.equal(await readFile(join(root, "copy.dat"), "utf8"), "copy");
        await assert.rejects(
            service.saveAs(opened.documentId, rootId, "copy.dat", Buffer.from("must-not-overwrite")),
            (error         ) => error instanceof SafeSaveError && error.code === "overwrite-required",
        );
        assert.equal(await readFile(join(root, "copy.dat"), "utf8"), "copy");
    });

    it("keeps every parent directory handle open so rename-to-junction swapping fails", async () => {
        const root = await temporaryRoot("junction-root");
        const outside = await temporaryRoot("junction-outside");
        const nested = join(root, "nested");
        await import("node:fs/promises").then(({ mkdir }) => mkdir(nested));
        await writeFile(join(nested, "source.dat"), "inside");
        await writeFile(join(outside, "source.dat"), "outside-secret");
        let renameDenied = false;
        let namespaceSwapCompleted = false;
        const client = nativeClient({
            async onBarrier(event) {
                if (event.name !== "after-directory-handles") {
                    return;
                }
                const moved = join(root, "nested-moved");
                try {
                    await rename(nested, moved);
                    await symlink(outside, nested, "junction");
                    namespaceSwapCompleted = true;
                } catch (error) {
                    const code = (error                         ).code;
                    renameDenied = code === "EPERM" || code === "EACCES" || code === "EBUSY";
                }
            },
        });
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
        const { rootId } = await registry.grantAbsoluteRoot(root);

        let opened;
        try {
            opened = await registry.openDocument(rootId, "nested/source.dat");
        } catch (error) {
            assert.equal(namespaceSwapCompleted, true);
            assert.match((error         ).message, /changed|root/i);
        }
        if (opened !== undefined) {
            assert.equal(renameDenied, true);
            const read = await registry.readDocument(opened.documentId);
            assert.equal(read.bytes.toString("utf8"), "inside");
        }
        assert.equal(renameDenied || namespaceSwapCompleted, true);
        assert.equal(await readFile(join(outside, "source.dat"), "utf8"), "outside-secret");
    });

    for (const mutationTarget of ["target", "replacement"]         ) {
        it(`blocks ${mutationTarget} writes before publication and aborts without publishing`, async () => {
            const root = await temporaryRoot(`before-publish-${mutationTarget}`);
            await writeFile(join(root, "source.dat"), "original");
            let mutationRejected = false;
            let replacementPath = "";
            let backupPath = "";
            const client = nativeClient({
                async onBarrier(event) {
                    if (event.name !== "before-publish") {
                        return;
                    }
                    replacementPath = event.replacementPath;
                    backupPath = event.backupPath;
                    try {
                        await writeFile(
                            mutationTarget === "target" ? event.targetPath : event.replacementPath,
                            `mutated-${mutationTarget}`,
                        );
                    } catch (error) {
                        const code = (error                         ).code;
                        mutationRejected = code === "EBUSY" || code === "EPERM" || code === "EACCES";
                    }
                    throw new Error("stop after the pre-publication mutation probe");
                },
            });
            const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
            const { rootId } = await registry.grantAbsoluteRoot(root);
            const { documentId } = await registry.openDocument(rootId, "source.dat");
            const service = new SafeSaveService(registry, { nativeClient: client });
            const bytes = Buffer.from("intended-new");
            const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", bytes);

            await assert.rejects(
                service.overwrite(documentId, challenge.challengeId, bytes),
                (error         ) => {
                    assert.ok(error instanceof SafeSaveError);
                    assert.equal(error.code, "replace-invocation-failed");
                    return true;
                },
            );
            assert.equal(mutationRejected, true);
            assert.equal(await readFile(join(root, "source.dat"), "utf8"), "original");
            assert.equal(await readFile(replacementPath, "utf8"), "intended-new");
            await assert.rejects(lstat(backupPath), (error         ) => (
                (error                         ).code === "ENOENT"
            ));
        });
    }

    it("blocks rename and reparse takeovers for both publication handles", async () => {
        const root = await temporaryRoot("before-publish-namespace");
        await writeFile(join(root, "source.dat"), "original");
        const rejected = new Set        ();
        let replacementPath = "";
        let backupPath = "";
        const client = nativeClient({
            async onBarrier(event) {
                if (event.name !== "before-publish") {
                    return;
                }
                replacementPath = event.replacementPath;
                backupPath = event.backupPath;
                for (const [label, path] of [
                    ["target", event.targetPath],
                    ["replacement", event.replacementPath],
                ]         ) {
                    try {
                        await rename(path, `${path}.moved`);
                    } catch (error) {
                        const code = (error                         ).code;
                        if (code === "EBUSY" || code === "EPERM" || code === "EACCES") {
                            rejected.add(`${label}-rename`);
                        }
                    }
                    try {
                        await unlink(path);
                        await symlink(join(root, "outside.dat"), path, "file");
                    } catch (error) {
                        const code = (error                         ).code;
                        if (code === "EBUSY" || code === "EPERM" || code === "EACCES") {
                            rejected.add(`${label}-reparse`);
                        }
                    }
                }
                throw new Error("stop after the pre-publication namespace probes");
            },
        });
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const { documentId } = await registry.openDocument(rootId, "source.dat");
        const service = new SafeSaveService(registry, { nativeClient: client });
        const bytes = Buffer.from("intended-new");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", bytes);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, bytes),
            (error         ) => error instanceof SafeSaveError && error.code === "replace-invocation-failed",
        );
        assert.deepEqual([...rejected].sort(), [
            "replacement-rename",
            "replacement-reparse",
            "target-rename",
            "target-reparse",
        ]);
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "original");
        assert.equal(await readFile(replacementPath, "utf8"), "intended-new");
        await assert.rejects(lstat(backupPath), (error         ) => (
            (error                         ).code === "ENOENT"
        ));
    });

    it("rejects Win32 path aliases before issuing concurrent overwrite challenges", async () => {
        const root = await temporaryRoot("aliases");
        await writeFile(join(root, "source.dat"), "original");
        const client = nativeClient();
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const { documentId } = await registry.openDocument(rootId, "source.dat");
        const service = new SafeSaveService(registry, { nativeClient: client });
        const bytes = Buffer.from("new");

        const attempts = await Promise.allSettled([
            service.issueOverwriteChallenge(documentId, rootId, "source.dat.", bytes),
            service.issueOverwriteChallenge(documentId, rootId, "source.dat ", bytes),
            service.issueOverwriteChallenge(documentId, rootId, "source.dat:stream", bytes),
            service.issueOverwriteChallenge(documentId, rootId, "NUL.txt", bytes),
            service.issueOverwriteChallenge(documentId, rootId, "COM1.dat", bytes),
        ]);

        assert.equal(attempts.every((result) => (
            result.status === "rejected"
                && result.reason instanceof WorkspaceSecurityError
                && result.reason.code === "invalid-logical-path"
        )), true);

        const descriptor = registry.getRootDescriptor(rootId);
        for (const logicalPath of ["source.dat.", "source.dat ", "source.dat:stream", "nul.TXT", "lPt9.bin"]) {
            await assert.rejects(
                client.read({ root: descriptor, logicalPath, maximumBytes: 1024 }),
                (error         ) => error instanceof NativeSafeFileError && error.code === "invalid-logical-path",
            );
        }
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "original");
    });

    it("serializes case aliases by the native canonical target path", async () => {
        const root = await temporaryRoot("canonical-lock");
        await writeFile(join(root, "source.dat"), "original");
        const realClient = nativeClient();
        let activeOverwrites = 0;
        let peakOverwrites = 0;
        const delayedClient                       = {
            inspectRoot: (request) => realClient.inspectRoot(request),
            read: (request) => realClient.read(request),
            saveAs: (request) => realClient.saveAs(request),
            async overwrite(request) {
                activeOverwrites += 1;
                peakOverwrites = Math.max(peakOverwrites, activeOverwrites);
                try {
                    await new Promise((resolveDelay) => setTimeout(resolveDelay, 50));
                    return await realClient.overwrite(request);
                } finally {
                    activeOverwrites -= 1;
                }
            },
        };
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: delayedClient });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const { documentId } = await registry.openDocument(rootId, "source.dat");
        const service = new SafeSaveService(registry, { nativeClient: delayedClient });
        const lowerBytes = Buffer.from("lower-case-write");
        const upperBytes = Buffer.from("upper-case-write");
        const [lowerChallenge, upperChallenge] = await Promise.all([
            service.issueOverwriteChallenge(documentId, rootId, "source.dat", lowerBytes),
            service.issueOverwriteChallenge(documentId, rootId, "SOURCE.DAT", upperBytes),
        ]);

        const results = await Promise.allSettled([
            service.overwrite(documentId, lowerChallenge.challengeId, lowerBytes),
            service.overwrite(documentId, upperChallenge.challengeId, upperBytes),
        ]);

        assert.equal(peakOverwrites, 1);
        assert.equal(results.filter((result) => result.status === "fulfilled").length, 1);
        assert.equal(results.filter((result) => (
            result.status === "rejected"
                && result.reason instanceof SafeSaveError
                && result.reason.code === "external-change"
        )).length, 1);
    });

    it("rejects a forged success result whose reported target or backup hashes are wrong", async () => {
        const root = await temporaryRoot("fake-ok");
        const targetPath = join(root, "source.dat");
        await writeFile(targetPath, "original");
        const realClient = nativeClient();
        const fakeClient                       = {
            inspectRoot: (request) => realClient.inspectRoot(request),
            read: (request) => realClient.read(request),
            saveAs: (request) => realClient.saveAs(request),
            async overwrite(request) {
                const wrong = "0".repeat(64);
                return {
                    canonicalPath: targetPath,
                    fingerprint: { ...request.expectedFingerprint, sha256: wrong },
                    recovery: {
                        target: { path: targetPath, exists: true, size: 5, sha256: wrong },
                        replacement: { path: join(root, request.replacementName), exists: false },
                        backup: { path: join(root, request.backupName), exists: true, size: 8, sha256: wrong },
                    },
                };
            },
        };
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: realClient });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const { documentId } = await registry.openDocument(rootId, "source.dat");
        const service = new SafeSaveService(registry, { nativeClient: fakeClient });
        const bytes = Buffer.from("intended-new");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", bytes);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, bytes),
            (error         ) => error instanceof SafeSaveError && error.code === "replace-result-inconsistent",
        );
        assert.equal(await readFile(targetPath, "utf8"), "original");
    });

    it("performs a verified ReplaceFileW publication and verifies both target and backup hashes", async () => {
        const root = await temporaryRoot("overwrite");
        await writeFile(join(root, "source.dat"), "original");
        const client = nativeClient();
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
        const { rootId } = await registry.grantAbsoluteRoot(root);
        const { documentId } = await registry.openDocument(rootId, "source.dat");
        const service = new SafeSaveService(registry, { nativeClient: client });
        const bytes = Buffer.from("intended-new");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", bytes);

        const result = await service.overwrite(documentId, challenge.challengeId, bytes);

        assert.equal(result.recovery.target.sha256, challenge.contentSha256);
        assert.equal(result.recovery.backup.sha256, challenge.targetFingerprint.sha256);
        assert.equal(result.recovery.replacement.exists, false);
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "intended-new");
        assert.equal(await readFile(result.recovery.backup.path, "utf8"), "original");
    });
});
