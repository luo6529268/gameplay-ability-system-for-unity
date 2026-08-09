// dat-skill-flow-build:20260808064149020-3d7165865ac3460c9a7b2ea481155025
import assert from "node:assert/strict";
import { opendir, readFile } from "node:fs/promises";
import { join } from "node:path";
import { it } from "node:test";

import { LosslessDatDocument } from "../../src/model/dat-document.js";

it("optionally round-trips the explicitly selected external DAT corpus in memory", async (context) => {
    const selectedRoot = process.env.NTSD_DAT_CORPUS_ROOT;
    if (!selectedRoot) {
        context.skip("NTSD_DAT_CORPUS_ROOT is not explicitly set");
        return;
    }

    const datFiles           = [];
    async function collect(directoryPath        )                {
        const directory = await opendir(directoryPath);
        for await (const entry of directory) {
            const entryPath = join(directoryPath, entry.name);
            if (entry.isDirectory()) {
                await collect(entryPath);
            } else if (entry.isFile() && entry.name.toLowerCase().endsWith(".dat")) {
                datFiles.push(entryPath);
            }
        }
    }

    await collect(selectedRoot);
    assert.ok(datFiles.length > 0, "selected corpus contains no DAT files");
    for (const datPath of datFiles.slice(0, 250)) {
        const original = await readFile(datPath);
        const document = LosslessDatDocument.fromEncrypted(original);
        assert.deepEqual(document.emitFile(), original, datPath);
    }
});
