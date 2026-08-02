// dat-skill-flow-build:20260801034218797-3d68cc7421f747da9bedd9a9e5a254b8
import assert from "node:assert/strict";
import { copyFile, lstat, mkdtemp, open, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join } from "node:path";
import { describe, it } from "node:test";

import {
    SafeSaveError,
    SafeSaveService,
                          
} from "../../src/server/safe-save.js";
import { WorkspaceRegistry } from "../../src/server/workspace-registry.js";

async function fixture(label        )           
                 
                                
                   
                       
   {
    const root = await mkdtemp(join(tmpdir(), `dat-flow-safe-save-${label}-`));
    process.stdout.write(`[safe-test-artifacts] ${root}\n`);
    await writeFile(join(root, "source.dat"), "original");
    const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true });
    const { rootId } = await registry.grantAbsoluteRoot(root);
    const { documentId } = await registry.openDocument(rootId, "source.dat");
    return { root, registry, rootId, documentId };
}

const neverPublish                   = {
    async replace()                 {
        throw new Error("publisher must not be called");
    },
};

describe("recoverable safe save", () => {
    it("Save As writes the final path through wx and refuses an existing destination", async () => {
        const { root, registry, rootId, documentId } = await fixture("save-as");
        const flags           = [];
        const service = new SafeSaveService(registry, {
            publisher: neverPublish,
            fileSystem: {
                open: async (filePath        , flag        ) => {
                    flags.push(flag);
                    return await open(filePath, flag);
                },
            },
        });

        const saved = await service.saveAs(documentId, rootId, "copy.dat", Buffer.from("replacement"));
        assert.equal(saved.status, "created");
        assert.equal(await readFile(join(root, "copy.dat"), "utf8"), "replacement");
        assert.deepEqual(flags, ["wx"]);

        await assert.rejects(
            service.saveAs(documentId, rootId, "copy.dat", Buffer.from("must not replace")),
            (error         ) => error instanceof SafeSaveError && error.code === "overwrite-required",
        );
        assert.equal(await readFile(join(root, "copy.dat"), "utf8"), "replacement");
    });

    it("serializes concurrent writers to one canonical target", async () => {
        const { registry, rootId, documentId } = await fixture("lock");
        let active = 0;
        let peak = 0;
        const service = new SafeSaveService(registry, {
            publisher: neverPublish,
            hooks: {
                async beforeSaveAsOpen()                {
                    active += 1;
                    peak = Math.max(peak, active);
                    await new Promise((resolve) => setTimeout(resolve, 25));
                    active -= 1;
                },
            },
        });

        const results = await Promise.allSettled([
            service.saveAs(documentId, rootId, "contended.dat", Buffer.from("one")),
            service.saveAs(documentId, rootId, "contended.dat", Buffer.from("two")),
        ]);
        assert.equal(peak, 1);
        assert.equal(results.filter((result) => result.status === "fulfilled").length, 1);
        assert.equal(results.filter((result) => result.status === "rejected").length, 1);
    });

    it("keeps a partially written Save As output and reports its observed recovery state", async () => {
        const { root, registry, rootId, documentId } = await fixture("partial");
        const service = new SafeSaveService(registry, {
            publisher: neverPublish,
            fileSystem: {
                open: async (filePath        , flag        ) => {
                    const handle = await open(filePath, flag);
                    return {
                        async writeFile()                {
                            await handle.writeFile("partial");
                            throw Object.assign(new Error("injected write fault"), { code: "EIO" });
                        },
                        async sync()                {
                            await handle.sync();
                        },
                        async close()                {
                            await handle.close();
                        },
                    };
                },
            },
        });

        await assert.rejects(
            service.saveAs(documentId, rootId, "recoverable.dat", Buffer.from("complete")),
            (error         ) => {
                assert.ok(error instanceof SafeSaveError);
                assert.equal(error.code, "save-as-write-failed");
                assert.equal(error.recovery?.target.exists, true);
                assert.match(error.recovery?.target.sha256 ?? "", /^[a-f0-9]{64}$/);
                return true;
            },
        );
        assert.equal(await readFile(join(root, "recoverable.dat"), "utf8"), "partial");
    });

    it("binds overwrite challenges to content and fingerprint and consumes them once", async () => {
        const { root, registry, rootId, documentId } = await fixture("challenge");
        const publisher                   = {
            async replace(request) {
                await copyFile(request.targetPath, request.backupPath, 1);
                await copyFile(request.replacementPath, request.targetPath);
                return { ok: true };
            },
        };
        const service = new SafeSaveService(registry, { publisher });
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
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "original");
    });

    it("rehashes under lock and rejects an external mutation before publishing", async () => {
        const { root, registry, rootId, documentId } = await fixture("mutation");
        let publications = 0;
        const service = new SafeSaveService(registry, {
            publisher: {
                async replace()                        {
                    publications += 1;
                    return { ok: true };
                },
            },
        });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);
        await writeFile(join(root, "source.dat"), "external mutation");

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => error instanceof SafeSaveError && error.code === "external-change",
        );
        assert.equal(publications, 0);
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "external mutation");
    });

    it("retries backup collisions and reports target/temp/backup after publisher failure", async () => {
        const { root, registry, rootId, documentId } = await fixture("recovery");
        const collision = join(root, ".source.dat.backup-collision.bak");
        await writeFile(collision, "preexisting backup candidate");
        const names = ["collision", "unique", "temp"];
        const service = new SafeSaveService(registry, {
            nameFactory: () => names.shift() ?? crypto.randomUUID(),
            publisher: {
                async replace() {
                    return { ok: false, win32Code: 1177, code: "unable-to-move-replacement-2", message: "injected" };
                },
            },
        });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => {
                assert.ok(error instanceof SafeSaveError);
                assert.equal(error.code, "replace-failed");
                assert.equal(error.win32Code, 1177);
                assert.equal(error.recovery?.target.exists, true);
                assert.equal(error.recovery?.replacement.exists, true);
                assert.equal(error.recovery?.backup.exists, false);
                assert.notEqual(basename(error.recovery?.backup.path ?? ""), basename(collision));
                return true;
            },
        );
        assert.equal((await lstat(collision)).isFile(), true);
    });
});
