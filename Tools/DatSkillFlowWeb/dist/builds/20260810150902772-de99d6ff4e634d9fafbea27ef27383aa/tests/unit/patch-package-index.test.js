// dat-skill-flow-build:20260810150902772-de99d6ff4e634d9fafbea27ef27383aa
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { parsePatchPackageIndex } from "../../src/server/patch-package-index.js";

describe("patch package index", () => {
    it("keeps every dependency record while deriving supplemental package status", () => {
        const parsed = parsePatchPackageIndex({
            schemaVersion: 1,
            packages: [{
                packageId: "pkg-a",
                relativeDirectory: "series/pkg-a",
                label: "Package A",
                datFiles: ["series/pkg-a/hero.dat", "series/pkg-a/effect.dat"],
                bmpFiles: ["series/pkg-a/hero_0.bmp"],
                records: [
                    { oid: 2, type: 0, file: "hero.dat", logicalPath: "series/pkg-a/hero.dat", manifestSource: "supplemental" },
                    { oid: 205, type: 3, file: "effect.dat", logicalPath: "series/pkg-a/effect.dat", manifestSource: "supplemental" },
                ],
                diagnostics: [],
            }],
        });
        assert.equal(parsed.packages[0]?.status, "supplemental");
        assert.deepEqual(parsed.packages[0]?.records.map((record) => record.type), [0, 3]);
    });

    it("rejects absolute paths, traversal, unknown DAT records and duplicate package ids", () => {
        const base = {
            schemaVersion: 1,
            packages: [{ packageId: "pkg", relativeDirectory: "pkg", label: "Pkg", datFiles: ["pkg/a.dat"], bmpFiles: [], records: [], diagnostics: [] }],
        };
        assert.throws(() => parsePatchPackageIndex({ ...base, packages: [{ ...base.packages[0], datFiles: ["J:/a.dat"] }] }), /relative/);
        assert.throws(() => parsePatchPackageIndex({ ...base, packages: [{ ...base.packages[0], relativeDirectory: "../pkg" }] }), /unsafe/);
        assert.equal(parsePatchPackageIndex({ ...base, packages: [{ ...base.packages[0], records: [{ oid: 1, type: 0, file: "a.dat", logicalPath: "pkg/missing/a.dat", manifestSource: "source" }] }] }).packages[0]?.records[0]?.logicalPath, "pkg/a.dat");
        assert.equal(parsePatchPackageIndex({ ...base, packages: [{ ...base.packages[0], records: [{ oid: 1, type: 0, file: "b.dat", logicalPath: "pkg/b.dat", manifestSource: "source" }] }] }).packages[0]?.records.length, 0);
        assert.throws(() => parsePatchPackageIndex({ ...base, packages: [base.packages[0], base.packages[0]] }), /Duplicate/);
    });
});
