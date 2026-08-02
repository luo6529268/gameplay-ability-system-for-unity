// dat-skill-flow-build:20260801043004318-d93dcf84657249b0852ba7b7591bf049
import assert from "node:assert/strict";
import { copyFile, lstat, mkdtemp, open, readFile, rename, writeFile } from "node:fs/promises";
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

    it("rehashes again after closing the temp and immediately before publication", async () => {
        const { root, registry, rootId, documentId } = await fixture("late-mutation");
        let publications = 0;
        const service = new SafeSaveService(registry, {
            hooks: {
                async beforeOverwriteRehash()                {
                    await writeFile(join(root, "source.dat"), "late external mutation");
                },
            },
            publisher: {
                async replace()                        {
                    publications += 1;
                    return { ok: true };
                },
            },
        });
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => {
                assert.ok(error instanceof SafeSaveError);
                assert.equal(error.code, "external-change");
                assert.equal(error.recovery?.replacement?.exists, true);
                return true;
            },
        );
        assert.equal(publications, 0);
    });

    it("expires and consumes a challenge", async () => {
        const { registry, rootId, documentId } = await fixture("expiry");
        let now = 1_000;
        const service = new SafeSaveService(registry, {
            publisher: neverPublish,
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

    it("preserves an incomplete replacement temp after an injected write fault", async () => {
        const { registry, rootId, documentId } = await fixture("temp-partial");
        const service = new SafeSaveService(registry, {
            publisher: neverPublish,
            fileSystem: {
                open: async (filePath        , flag        ) => {
                    const handle = await open(filePath, flag);
                    return {
                        async writeFile()                {
                            await handle.writeFile("temp-partial");
                            throw Object.assign(new Error("injected temp write fault"), { code: "EIO" });
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
        const content = Buffer.from("new bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);

        await assert.rejects(
            service.overwrite(documentId, challenge.challengeId, content),
            (error         ) => {
                assert.ok(error instanceof SafeSaveError);
                assert.equal(error.code, "temp-write-failed");
                assert.equal(error.recovery?.replacement?.exists, true);
                assert.match(error.recovery?.replacement?.sha256 ?? "", /^[a-f0-9]{64}$/);
                return true;
            },
        );
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

    for (const win32Code of [1175, 1176, 1177]         ) {
        it(`inspects all recovery paths after mapped Win32 ${win32Code}`, async () => {
            const { registry, rootId, documentId } = await fixture(`win32-${win32Code}`);
            const service = new SafeSaveService(registry, {
                publisher: {
                    async replace(request) {
                        if (win32Code === 1177) {
                            await rename(request.targetPath, request.backupPath);
                        }
                        return {
                            ok: false,
                            win32Code,
                            code: `mapped-${win32Code}`,
                            message: `injected ${win32Code}`,
                        };
                    },
                },
            });
            const content = Buffer.from(`new-${win32Code}`);
            const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);

            await assert.rejects(
                service.overwrite(documentId, challenge.challengeId, content),
                (error         ) => {
                    assert.ok(error instanceof SafeSaveError);
                    assert.equal(error.win32Code, win32Code);
                    assert.equal(error.recovery?.replacement?.exists, true);
                    if (win32Code === 1177) {
                        assert.equal(error.recovery?.target.exists, false);
                        assert.equal(error.recovery?.backup?.exists, true);
                    } else {
                        assert.equal(error.recovery?.target.exists, true);
                        assert.equal(error.recovery?.backup?.exists, false);
                    }
                    return true;
                },
            );
        });
    }

    it("publishes through ReplaceFileW and leaves the backup on Windows", { skip: process.platform !== "win32" }, async () => {
        const { root, registry, rootId, documentId } = await fixture("replace-success");
        const service = new SafeSaveService(registry);
        const content = Buffer.from("new published bytes");
        const challenge = await service.issueOverwriteChallenge(documentId, rootId, "source.dat", content);
        const result = await service.overwrite(documentId, challenge.challengeId, content);

        assert.equal(result.status, "overwritten");
        assert.equal(result.recovery.target.exists, true);
        assert.equal(result.recovery.replacement.exists, false);
        assert.equal(result.recovery.backup.exists, true);
        assert.equal(await readFile(join(root, "source.dat"), "utf8"), "new published bytes");
        assert.equal(await readFile(result.recovery.backup.path, "utf8"), "original");
    });
});
