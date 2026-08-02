// dat-skill-flow-build:20260801063743753-69f54ecd60f44fd6a354fedf6173019c
import assert from "node:assert/strict";
import { mkdtemp, mkdir, readFile, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";

import {
    assertPreserved,
    createPreservationManifest,
    REPOSITORY_PRESERVATION_OPTIONS,
                              
} from "../../src/server/preservation-manifest.js";

describe("preservation manifest", () => {
    it("records tracked-independent relative paths, sizes, and SHA-256 values", async () => {
        const root = await mkdtemp(join(tmpdir(), "dat-flow-manifest-"));
        await mkdir(join(root, "data"));
        await writeFile(join(root, "README.md"), "history\n");
        await writeFile(join(root, "data", "untracked.bin"), Buffer.from([0, 1, 2, 3]));

        const before = await createPreservationManifest(root);
        await writeFile(join(root, "new-source.ts"), "export {};\n");
        const after = await createPreservationManifest(root);

        assert.deepEqual(before.entries.map((entry) => entry.path), [
            "README.md",
            "data/untracked.bin",
        ]);
        assert.equal(before.entries[1]?.size, 4);
        assert.match(before.entries[1]?.sha256 ?? "", /^[a-f0-9]{64}$/);
        assert.deepEqual(assertPreserved(before, after), []);
    });

    it("reports modified or missing baseline entries", async () => {
        const root = await mkdtemp(join(tmpdir(), "dat-flow-manifest-"));
        await writeFile(join(root, "keep.txt"), "original");
        const before = await createPreservationManifest(root);
        await writeFile(join(root, "keep.txt"), "changed");
        const after = await createPreservationManifest(root);

        const failures = assertPreserved(before, after);
        assert.equal(failures.length, 1);
        assert.equal(failures[0]?.path, "keep.txt");
        assert.equal(failures[0]?.reason, "changed");
    });

    it("verifies the repository baseline against the actual tool tree", async () => {
        const toolRoot = process.cwd();
        const baseline = JSON.parse(
            await readFile(resolve(toolRoot, "audit/preservation-before.json"), "utf8"),
        )                        ;
        const actual = await createPreservationManifest(toolRoot, REPOSITORY_PRESERVATION_OPTIONS);

        assert.deepEqual(baseline.entries.map((entry) => entry.path), [
            "README.md",
            "data/cpp-runtime.json",
        ]);
        assert.deepEqual(assertPreserved(baseline, actual), []);
        assert.deepEqual(
            actual.entries
                .filter((entry) => baseline.entries.some((expected) => expected.path === entry.path))
                .map(({ path, size, sha256 }) => ({ path, size, sha256 })),
            baseline.entries,
        );
    });
});
