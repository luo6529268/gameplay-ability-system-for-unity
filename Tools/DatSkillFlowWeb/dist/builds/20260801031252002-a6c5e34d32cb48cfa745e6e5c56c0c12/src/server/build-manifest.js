// dat-skill-flow-build:20260801031252002-a6c5e34d32cb48cfa745e6e5c56c0c12
import {
    expectArray,
    expectLiteral,
    expectNonnegativeInteger,
    expectRecord,
    expectStrictKeys,
    expectString,
    fail,
    validator,
} from "../validation/strict.js";

                                 
                 
                    
                 
                   
 

                                
                     
                    
                       
                        
                        
                              
                                  
 

const manifestKeys = new Set([
    "schemaVersion",
    "buildId",
    "clientRoot",
    "serverEntry",
    "testFiles",
    "outputs",
    "clientFiles",
]);
const fileKeys = new Set(["path", "buildId", "size", "sha256"]);
const buildIdPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const sha256Pattern = /^[a-f0-9]{64}$/;

function expectPortableRelativePath(value         , path                                )         {
    const filePath = expectString(value, path, 1);
    const segments = filePath.split("/");
    if (filePath.startsWith("/") || filePath.includes("\\") || segments.some((segment) => (
        segment.length === 0 || segment === "." || segment === ".."
    ))) {
        fail(path, "expected a normalized portable relative path");
    }
    return filePath;
}

function parseFileEntry(
    value         ,
    path                                ,
    expectedBuildId        ,
)                 {
    const record = expectRecord(value, path);
    expectStrictKeys(record, fileKeys, path);
    const buildId = expectString(record.buildId, [...path, "buildId"], 1);
    if (buildId !== expectedBuildId) {
        fail([...path, "buildId"], "build ID does not match the current manifest");
    }
    const sha256 = expectString(record.sha256, [...path, "sha256"], 64);
    if (!sha256Pattern.test(sha256)) {
        fail([...path, "sha256"], "expected a lowercase SHA-256 digest");
    }
    return {
        path: expectPortableRelativePath(record.path, [...path, "path"]),
        buildId,
        size: expectNonnegativeInteger(record.size, [...path, "size"]),
        sha256,
    };
}

export const buildManifestSchema = validator               ((value) => {
    const record = expectRecord(value, []);
    expectStrictKeys(record, manifestKeys, []);
    const buildId = expectString(record.buildId, ["buildId"], 1);
    if (!buildIdPattern.test(buildId)) {
        fail(["buildId"], "invalid build ID");
    }
    const clientRoot = expectPortableRelativePath(record.clientRoot, ["clientRoot"]);
    if (clientRoot !== `builds/${buildId}`) {
        fail(["clientRoot"], "client root must identify the current build");
    }
    const serverEntry = expectPortableRelativePath(record.serverEntry, ["serverEntry"]);
    if (serverEntry !== `${clientRoot}/src/server/cli.js`) {
        fail(["serverEntry"], "server entry must identify the current build");
    }
    const outputs = expectArray(record.outputs, ["outputs"]).map((entry, index) => (
        parseFileEntry(entry, ["outputs", index], buildId)
    ));
    const clientFiles = expectArray(record.clientFiles, ["clientFiles"]).map((entry, index) => (
        parseFileEntry(entry, ["clientFiles", index], buildId)
    ));
    const outputByPath = new Map                        ();
    for (const [index, entry] of outputs.entries()) {
        if (!entry.path.startsWith(`${clientRoot}/`)) {
            fail(["outputs", index, "path"], "output is outside the current build root");
        }
        if (outputByPath.has(entry.path)) {
            fail(["outputs", index, "path"], `duplicate output: ${entry.path}`);
        }
        outputByPath.set(entry.path, entry);
    }
    if (!outputByPath.has(serverEntry)) {
        fail(["serverEntry"], "server entry is not a current-build output");
    }
    const testFiles = expectArray(record.testFiles, ["testFiles"]).map((entry, index) => (
        expectPortableRelativePath(entry, ["testFiles", index])
    ));
    const uniqueTestFiles = new Set        ();
    for (const [index, testFile] of testFiles.entries()) {
        if (!testFile.startsWith(`${clientRoot}/tests/`) || !outputByPath.has(testFile)) {
            fail(["testFiles", index], "test file is not a current-build output");
        }
        if (uniqueTestFiles.has(testFile)) {
            fail(["testFiles", index], `duplicate test file: ${testFile}`);
        }
        uniqueTestFiles.add(testFile);
    }
    const clientPaths = new Set        ();
    for (const [index, entry] of clientFiles.entries()) {
        if (clientPaths.has(entry.path)) {
            fail(["clientFiles", index, "path"], `duplicate client file: ${entry.path}`);
        }
        clientPaths.add(entry.path);
        const output = outputByPath.get(`${clientRoot}/${entry.path}`);
        if (output === undefined || output.size !== entry.size || output.sha256 !== entry.sha256) {
            fail(["clientFiles", index], "client file is not an exact current-build output");
        }
    }
    return {
        schemaVersion: expectLiteral(record.schemaVersion, 1, ["schemaVersion"]),
        buildId,
        clientRoot,
        serverEntry,
        testFiles,
        outputs,
        clientFiles,
    };
});
