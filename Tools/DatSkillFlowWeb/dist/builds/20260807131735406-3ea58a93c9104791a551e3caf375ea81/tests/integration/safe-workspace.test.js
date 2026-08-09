// dat-skill-flow-build:20260807131735406-3ea58a93c9104791a551e3caf375ea81
import assert from "node:assert/strict";
import { mkdtemp, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { describe, it } from "node:test";

import {
    MAX_DOCUMENT_BYTES,
    WorkspaceRegistry,
    WorkspaceSecurityError,
} from "../../src/server/workspace-registry.js";

async function temporaryRoot(label        )                  {
    const root = await mkdtemp(join(tmpdir(), `dat-flow-safe-${label}-`));
    process.stdout.write(`[safe-test-artifacts] ${root}\n`);
    return root;
}

describe("opaque workspace and document registry", () => {
    it("allows one startup-only absolute root and rejects authorization after sealing", async () => {
        const root = await temporaryRoot("startup-root");
        const secondRoot = await temporaryRoot("startup-second-root");
        const registry = new WorkspaceRegistry();

        const grant = await registry.authorizeStartupRoot(root);
        assert.match(grant.rootId, /^[A-Za-z0-9_-]{32,}$/);
        assert.deepEqual(registry.getStartupRootGrant(), grant);
        assert.equal(JSON.stringify(grant).includes(root), false);
        await assert.rejects(
            registry.authorizeStartupRoot(secondRoot),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "startup-root-already-authorized",
        );

        registry.sealStartupAuthorization();
        await assert.rejects(
            registry.authorizeStartupRoot(root),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "startup-authorization-sealed",
        );
        await assert.rejects(
            registry.grantAbsoluteRoot(root),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "root-grant-disabled",
        );
    });

    it("starts with no roots and requires an explicit absolute-root grant option", async () => {
        const root = await temporaryRoot("default-deny");
        const registry = new WorkspaceRegistry();

        await assert.rejects(
            registry.grantAbsoluteRoot(root),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "root-grant-disabled",
        );
        assert.deepEqual(registry.listRootIds(), []);
    });

    it("returns opaque IDs and performs bounded reads with a stable fingerprint", async () => {
        const root = await temporaryRoot("opaque-read");
        await writeFile(join(root, "skill.dat"), "<frame> 0 standing\r\n");
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true });

        const granted = await registry.grantAbsoluteRoot(root);
        const opened = await registry.openDocument(granted.rootId, "skill.dat");
        const read = await registry.readDocument(opened.documentId);

        assert.match(granted.rootId, /^[A-Za-z0-9_-]{32,}$/);
        assert.match(opened.documentId, /^[A-Za-z0-9_-]{32,}$/);
        assert.equal(JSON.stringify(granted).includes(root), false);
        assert.equal(JSON.stringify(opened).includes(root), false);
        assert.equal(read.bytes.toString("utf8"), "<frame> 0 standing\r\n");
        assert.match(read.fingerprint.sha256, /^[a-f0-9]{64}$/);
        assert.equal(read.externallyModified, false);

        await writeFile(join(root, "skill.dat"), "externally changed");
        const changed = await registry.readDocument(opened.documentId);
        assert.equal(changed.externallyModified, true);
        assert.notEqual(changed.fingerprint.sha256, opened.fingerprint.sha256);
        assert.equal(registry.closeDocument(opened.documentId), true);
        assert.equal(registry.closeDocument(opened.documentId), false);
        await assert.rejects(
            registry.readDocument(opened.documentId),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "unknown-document",
        );
    });

    it("rejects NUL, traversal, absolute, drive-relative, and oversized reads", async () => {
        const root = await temporaryRoot("reject-paths");
        await writeFile(join(root, "large.dat"), Buffer.alloc(MAX_DOCUMENT_BYTES + 1, 0x61));
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true });
        const { rootId } = await registry.grantAbsoluteRoot(root);

        for (const unsafePath of [
            "../outside.dat",
            "nested/../../outside.dat",
            "/absolute.dat",
            "C:\\absolute.dat",
            "Z:drive-relative.dat",
            "bad\0name.dat",
        ]) {
            await assert.rejects(
                registry.openDocument(rootId, unsafePath),
                (error         ) => error instanceof WorkspaceSecurityError,
                unsafePath,
            );
        }
        await assert.rejects(
            registry.openDocument(rootId, "large.dat"),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "read-too-large",
        );
    });

    it("rejects a symlink or junction escape during logical resolution", async (context) => {
        const root = await temporaryRoot("link-root");
        const outside = await temporaryRoot("link-outside");
        await writeFile(join(outside, "secret.dat"), "secret");
        try {
            await symlink(outside, join(root, "escape"), process.platform === "win32" ? "junction" : "dir");
        } catch (error) {
            const code = (error                         ).code;
            if (code === "EPERM" || code === "EACCES" || code === "ENOSYS") {
                context.skip(`Link creation unavailable: ${code}`);
                return;
            }
            throw error;
        }
        const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true });
        const { rootId } = await registry.grantAbsoluteRoot(root);

        await assert.rejects(
            registry.openDocument(rootId, "escape/secret.dat"),
            (error         ) => error instanceof WorkspaceSecurityError && error.code === "root-escape",
        );
    });
});
