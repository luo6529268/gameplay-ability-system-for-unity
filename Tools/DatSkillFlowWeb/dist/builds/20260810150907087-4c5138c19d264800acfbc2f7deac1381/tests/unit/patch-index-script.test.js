// dat-skill-flow-build:20260810150907087-4c5138c19d264800acfbc2f7deac1381
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { describe, it } from "node:test";
const scriptPath = resolve("scripts/build-patch-index.ps1");
const powershellExecutable = "powershell.exe";
const powershellAvailable = process.platform === "win32" && spawnSync(
    powershellExecutable,
    ["-NoProfile", "-NonInteractive", "-Command", "$PSVersionTable.PSVersion.ToString()"],
    { encoding: "utf8", windowsHide: true },
).status === 0;

function runScanner(args                   ) {
    return spawnSync(
        powershellExecutable,
        [
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-File",
            scriptPath,
            ...args,
        ],
        { encoding: "utf8", windowsHide: true },
    );
}

async function createFixture() {
    const root = await mkdtemp(join(tmpdir(), "dat-patch-index-test-"));
    const libraryRoot = join(root, "library");
    const supplementalRoot = join(root, "supplemental");
    const packageDirectory = join(libraryRoot, "Pack Ω");
    const supplementalPackageDirectory = join(supplementalRoot, "Pack Ω");
    const outputPath = join(root, "output", "patch-index.json");

    await mkdir(join(packageDirectory, "nested"), { recursive: true });
    await mkdir(supplementalPackageDirectory, { recursive: true });
    await writeFile(join(packageDirectory, "manifest [special] #1.txt"), [
        "id: 7 type: 0 file: wrong.dat",
        "id: 7 type: 1 file: source-effect.dat",
        "id: 7 type: 2 file: source-two.dat",
        "id: 8 tupe: 3 file: typo.dat",
        "id: 9 tpye: 4 file: nested\\nested-effect.dat",
        "id: 10 type: 5 file: ../outside.dat",
        "id: 16 type: 5 file: illegal?.dat",
        "id: 11 type: 6",
        "id: 12",
        "id: 13 type: 7",
        "id: 14",
        "type: 8 file: no-id.dat",
        "id: 15",
        "type: 9 file: split-record.dat",
    ].join("\r\n"), "utf8");
    await writeFile(join(packageDirectory, "correct.dat"), "source DAT placeholder", "utf8");
    await writeFile(join(packageDirectory, "source-effect.dat"), "source DAT placeholder", "utf8");
    await writeFile(join(packageDirectory, "source-two.dat"), "source DAT placeholder", "utf8");
    await writeFile(join(packageDirectory, "typo.dat"), "source DAT placeholder", "utf8");
    await writeFile(join(packageDirectory, "nested", "nested-effect.dat"), "source DAT placeholder", "utf8");
    await writeFile(join(packageDirectory, "nested", "sheet.BMP"), "source BMP placeholder", "utf8");
    await writeFile(join(supplementalPackageDirectory, "ID.editor-recovered.txt"), [
        "id: 7 type: 0 file: correct.dat",
        "id: 20 type: 10 file: supplemental-only.dat",
        "id: 21 tupe: 11 file: supplemental-typo.dat",
    ].join("\r\n"), "utf8");
    await writeFile(join(supplementalPackageDirectory, "supplemental-only.dat"), "must not become a source asset", "utf8");
    await writeFile(join(supplementalPackageDirectory, "supplemental-only.bmp"), "must not become a source asset", "utf8");

    return { root, libraryRoot, supplementalRoot, packageDirectory, outputPath };
}

async function parseIndexAsync(outputPath        ) {
    return JSON.parse(await readFile(outputPath, "utf8"));
}

describe("build-patch-index.ps1", () => {
    it("is a PowerShell-only scanner without cmd invocation or a J-drive default", async () => {
        const script = await readFile(scriptPath, "utf8");
        assert.match(script, /\[CmdletBinding\(\)\]/);
        assert.doesNotMatch(script, /\bcmd(?:\.exe)?\b|cmd\s*\/c/i);
        assert.doesNotMatch(script, /Start-Process|Invoke-Expression|Invoke-Item/i);
        assert.doesNotMatch(script, /J:[\\/]/i);
    });

    it("parses with the Windows PowerShell 5.1 Parser", { skip: !powershellAvailable }, () => {
        const escapedScriptPath = scriptPath.replace(/'/g, "''");
        const parserCommand = [
            `$scriptPath = '${escapedScriptPath}'`,
            "$tokens = $null",
            "$errors = $null",
            "[System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$errors) | Out-Null",
            "if ($errors.Count -gt 0) { $errors | ForEach-Object { Write-Error $_.Message }; exit 1 }",
        ].join("; ");
        const result = spawnSync(
            powershellExecutable,
            ["-NoProfile", "-NonInteractive", "-Command", parserCommand],
            { encoding: "utf8", windowsHide: true },
        );
        assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
    });

    it("scans special manifest names, keeps explicit types, reports tupe/tpye, and merges supplemental records", { skip: !powershellAvailable }, async () => {
        const fixture = await createFixture();
        try {
            const result = runScanner([
                "-LibraryRoot",
                fixture.libraryRoot,
                "-SupplementalRoot",
                fixture.supplementalRoot,
                "-OutputPath",
                fixture.outputPath,
            ]);
            assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);

            const index = await parseIndexAsync(fixture.outputPath);
            assert.equal(index.schemaVersion, 1);
            assert.equal(index.pathBase, "LibraryRoot");
            assert.ok(Array.isArray(index.packages));
            const packageIndex = index.packages.find((item                               ) => item.relativeDirectory === "Pack Ω");
            assert.ok(packageIndex);
            assert.equal(packageIndex.label, "Pack Ω");
            assert.match(packageIndex.packageId, /^pkg-[0-9a-f]{16}$/);
            assert.deepEqual(
                Object.keys(packageIndex).sort(),
                ["bmpFiles", "datFiles", "diagnostics", "label", "packageId", "records", "relativeDirectory"].sort(),
            );

            assert.deepEqual(packageIndex.datFiles, [
                "Pack Ω/correct.dat",
                "Pack Ω/nested/nested-effect.dat",
                "Pack Ω/source-effect.dat",
                "Pack Ω/source-two.dat",
                "Pack Ω/typo.dat",
            ]);
            assert.deepEqual(packageIndex.bmpFiles, ["Pack Ω/nested/sheet.BMP"]);
            assert.equal(packageIndex.datFiles.some((path        ) => path.includes("supplemental")), false);
            assert.equal(packageIndex.bmpFiles.some((path        ) => path.includes("supplemental")), false);

            assert.ok(packageIndex.records.length >= 8);
            assert.ok(packageIndex.records.every((record                         ) => {
                return Object.keys(record).sort().join(",") === "file,logicalPath,manifestPath,manifestSource,oid,type";
            }));
            assert.ok(packageIndex.records.every((record                            ) => {
                return record.manifestSource === "source" || record.manifestSource === "supplemental";
            }));
            const sourceTypes = packageIndex.records
                .filter((record                            ) => record.manifestSource === "source")
                .map((record                  ) => record.type)
                .sort((left        , right        ) => left - right);
            assert.deepEqual(sourceTypes, [0, 1, 2, 3, 4, 5, 5]);
            const recoveredSource = packageIndex.records.find((record                                                       ) => {
                return record.oid === 7 && record.type === 0 && record.manifestSource === "source";
            });
            const recoveredSupplemental = packageIndex.records.find((record                                                       ) => {
                return record.oid === 7 && record.type === 0 && record.manifestSource === "supplemental";
            });
            assert.equal(recoveredSource?.logicalPath, "Pack Ω/wrong.dat");
            assert.equal(recoveredSupplemental?.logicalPath, "Pack Ω/correct.dat");
            assert.equal(recoveredSupplemental?.manifestPath, "Pack Ω/ID.editor-recovered.txt");
            assert.ok(packageIndex.records.every((record                                ) => {
                return record.logicalPath === null || (!record.logicalPath.startsWith("/") && !/^[A-Za-z]:/i.test(record.logicalPath) && !record.logicalPath.split("/").includes(".."));
            }));

            const diagnosticCodes = packageIndex.diagnostics.map((diagnostic                  ) => diagnostic.code);
            assert.ok(diagnosticCodes.includes("typo-field-token"));
            assert.ok(diagnosticCodes.includes("path-traversal"));
            assert.ok(diagnosticCodes.includes("invalid-path"));
            assert.ok(diagnosticCodes.includes("supplemental-overridden"));
            assert.ok(diagnosticCodes.includes("supplemental-recovery"));
            assert.ok(packageIndex.diagnostics.every((diagnostic                         ) => {
                return Object.keys(diagnostic).sort().join(",") === "code,message,severity";
            }));
        }
        finally {
            await rm(fixture.root, { recursive: true, force: true });
        }
    });

    it("rejects an output path inside LibraryRoot without writing the source tree", { skip: !powershellAvailable }, async () => {
        const fixture = await createFixture();
        try {
            const outputInsideSource = join(fixture.libraryRoot, "generated-index.json");
            const result = runScanner([
                "-LibraryRoot",
                fixture.libraryRoot,
                "-OutputPath",
                outputInsideSource,
            ]);
            assert.notEqual(result.status, 0, `${result.stdout}\n${result.stderr}`);
            const sourceContents = await readFile(join(fixture.packageDirectory, "manifest [special] #1.txt"), "utf8");
            assert.match(sourceContents, /id: 7 type: 0 file: wrong\.dat/);
            await assert.rejects(readFile(outputInsideSource, "utf8"));
        }
        finally {
            await rm(fixture.root, { recursive: true, force: true });
        }
    });

    it("enforces text-size and total-file limits", { skip: !powershellAvailable }, async () => {
        const fixture = await createFixture();
        try {
            const textLimited = runScanner([
                "-LibraryRoot",
                fixture.libraryRoot,
                "-OutputPath",
                fixture.outputPath,
                "-MaxTextBytes",
                "8",
            ]);
            assert.equal(textLimited.status, 0, `${textLimited.stdout}\n${textLimited.stderr}`);
            const limitedIndex = await parseIndexAsync(fixture.outputPath);
            assert.ok(limitedIndex.diagnostics.some((diagnostic                  ) => diagnostic.code === "manifest-too-large"));

            const fileLimited = runScanner([
                "-LibraryRoot",
                fixture.libraryRoot,
                "-OutputPath",
                fixture.outputPath,
                "-MaxFileCount",
                "1",
            ]);
            assert.notEqual(fileLimited.status, 0, `${fileLimited.stdout}\n${fileLimited.stderr}`);
        }
        finally {
            await rm(fixture.root, { recursive: true, force: true });
        }
    });
});
