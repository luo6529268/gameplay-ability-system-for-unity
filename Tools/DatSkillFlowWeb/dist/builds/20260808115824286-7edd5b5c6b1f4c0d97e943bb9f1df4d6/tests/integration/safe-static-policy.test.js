// dat-skill-flow-build:20260808115824286-7edd5b5c6b1f4c0d97e943bb9f1df4d6
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, it } from "node:test";

const productionFiles = [
    "src/server/server.ts",
    "src/server/workspace-registry.ts",
    "src/server/safe-save.ts",
    "src/server/windows-safe-file-adapter.ts",
    "src/server/windows-replace-adapter.ts",
    "scripts/windows-safe-file.ps1",
    "scripts/windows-replace-file.ps1",
]         ;

describe("safe-save static policy", () => {
    it("contains no filesystem delete, rename-publication, or backup-copy primitive", async () => {
        for (const relativePath of productionFiles) {
            const source = await readFile(resolve(relativePath), "utf8");
            assert.doesNotMatch(
                source,
                /\b(?:unlink|rm|rmdir|rename|copyFile|DeleteFileW?)\s*\(|\.(?:unlink|rm|rmdir|rename|copyFile)\s*\(|\b(?:Remove-Item|Copy-Item|Move-Item)\b/i,
                relativePath,
            );
        }
    });
});
