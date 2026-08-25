// dat-skill-flow-build:20260823084542620-bbbbe9c535ad4e8291a2ba3b39069af7
import { readFile } from "node:fs/promises";

const MAX_INDEX_BYTES = 16 * 1024 * 1024;
const MAX_PACKAGES = 2_000;
const MAX_PACKAGE_FILES = 20_000;
const MAX_PACKAGE_RECORDS = 4_000;

                                                                                     

                                   
                         
                          
                          
                                 
                                                       
 

                                    
                               
                                       
                           
                                         
                                                  
                                         
                                         
                                                                                                                                 
 

                                    
                              
                                                    
 

export async function loadPatchPackageIndex(path        )                             {
    const bytes = await readFile(path);
    if (bytes.byteLength > MAX_INDEX_BYTES) throw new Error("Patch index exceeds its size limit.");
    let parsed         ;
    try {
        parsed = JSON.parse(bytes.toString("utf8"));
    } catch (error) {
        throw new Error("Patch index is not valid JSON.", { cause: error });
    }
    return parsePatchPackageIndex(parsed);
}

export function parsePatchPackageIndex(value         )                    {
    const root = requireRecord(value, "patch index");
    if (root.schemaVersion !== 1) throw new Error("Unsupported patch index schemaVersion.");
    const packagesRaw = requireArray(root.packages, "packages", MAX_PACKAGES);
    const packageIds = new Set        ();
    const packages = packagesRaw.map((item, index)                    => {
        const raw = requireRecord(item, `packages[${index}]`);
        const packageId = requireId(raw.packageId, `packages[${index}].packageId`);
        if (packageIds.has(packageId)) throw new Error(`Duplicate patch packageId: ${packageId}`);
        packageIds.add(packageId);
        const relativeDirectory = requireLogicalPath(raw.relativeDirectory, `packages[${index}].relativeDirectory`, true);
        const label = requireText(raw.label, `packages[${index}].label`, 240);
        const datFiles = requireArray(raw.datFiles, `packages[${index}].datFiles`, MAX_PACKAGE_FILES)
            .map((file, fileIndex) => requireLogicalPath(file, `packages[${index}].datFiles[${fileIndex}]`));
        const bmpFiles = requireArray(raw.bmpFiles, `packages[${index}].bmpFiles`, MAX_PACKAGE_FILES)
            .map((file, fileIndex) => requireLogicalPath(file, `packages[${index}].bmpFiles[${fileIndex}]`));
        const datSet = new Set(datFiles.map((file) => file.toLowerCase()));
        const datFilesByBasename = new Map                  ();
        for (const datFile of datFiles) {
            const key = datFile.split("/").at(-1) .toLowerCase();
            const existing = datFilesByBasename.get(key);
            if (existing === undefined) datFilesByBasename.set(key, [datFile]);
            else existing.push(datFile);
        }
        const records = requireArray(raw.records, `packages[${index}].records`, MAX_PACKAGE_RECORDS)
            .flatMap((recordValue, recordIndex)                     => {
                const record = requireRecord(recordValue, `packages[${index}].records[${recordIndex}]`);
                if (!Number.isInteger(record.oid) || (record.oid          ) < 0 || (record.oid          ) > 999
                    || !Number.isInteger(record.type) || (record.type          ) < 0 || (record.type          ) > 255
                    || typeof record.logicalPath !== "string") return [];
                const declaredLogicalPath = requireLogicalPath(record.logicalPath, `packages[${index}].records[${recordIndex}].logicalPath`);
                const basenameKey = declaredLogicalPath.split("/").at(-1) .toLowerCase();
                const basenameMatches = datFilesByBasename.get(basenameKey) ?? [];
                const logicalPath = datSet.has(declaredLogicalPath.toLowerCase())
                    ? declaredLogicalPath
                    : basenameMatches.length === 1 ? basenameMatches[0]  : undefined;
                if (logicalPath === undefined) return [];
                const manifestSource = record.manifestSource;
                if (manifestSource !== "source" && manifestSource !== "supplemental") {
                    throw new Error(`Invalid patch manifestSource at packages[${index}].records[${recordIndex}].`);
                }
                return [{
                    oid: requireInteger(record.oid, 0, 999, `packages[${index}].records[${recordIndex}].oid`),
                    type: requireInteger(record.type, 0, 255, `packages[${index}].records[${recordIndex}].type`),
                    file: requireText(record.file, `packages[${index}].records[${recordIndex}].file`, 512),
                    logicalPath,
                    manifestSource,
                }];
            });
        const diagnostics = requireArray(raw.diagnostics ?? [], `packages[${index}].diagnostics`, MAX_PACKAGE_RECORDS)
            .flatMap((diagnosticValue, diagnosticIndex) => {
                const diagnostic = requireRecord(diagnosticValue, `packages[${index}].diagnostics[${diagnosticIndex}]`);
                const severity = diagnostic.severity;
                if (severity === "info") return [];
                if (severity !== "warning" && severity !== "error") throw new Error("Invalid patch diagnostic severity.");
                return [{
                    code: requireId(diagnostic.code, `packages[${index}].diagnostics[${diagnosticIndex}].code`),
                    severity,
                    message: requireText(diagnostic.message, `packages[${index}].diagnostics[${diagnosticIndex}].message`, 2_000),
                }];
            });
        const characterRecords = records.filter((record) => record.type === 0);
        const characterRecordsByOid = new Map                            ();
        for (const record of characterRecords) {
            const existing = characterRecordsByOid.get(record.oid);
            if (existing === undefined) characterRecordsByOid.set(record.oid, [record]);
            else existing.push(record);
        }
        const hasConflict = [...characterRecordsByOid.values()].some((sameOid) => (
            sameOid.length > 1 && !sameOid.some((record) => record.manifestSource === "supplemental")
        ));
        const hasError = diagnostics.some((diagnostic) => diagnostic.severity === "error");
        const hasSupplemental = records.some((record) => record.manifestSource === "supplemental");
        return {
            packageId,
            relativeDirectory,
            label,
            status: hasConflict ? "conflict" : hasError ? "partial" : hasSupplemental ? "supplemental" : "source",
            records,
            datFiles,
            bmpFiles,
            diagnostics,
        };
    });
    return { schemaVersion: 1, packages };
}

function requireRecord(value         , label        )                          {
    if (typeof value !== "object" || value === null || Array.isArray(value)) throw new Error(`${label} must be an object.`);
    return value                           ;
}

function requireArray(value         , label        , maximum        )                     {
    if (!Array.isArray(value) || value.length > maximum) throw new Error(`${label} must be an array within its limit.`);
    return value;
}

function requireText(value         , label        , maximum        )         {
    if (typeof value !== "string" || value.length === 0 || value.length > maximum || value.includes("\0")) {
        throw new Error(`${label} must be valid text.`);
    }
    return value;
}

function requireId(value         , label        )         {
    const id = requireText(value, label, 128);
    if (!/^[A-Za-z0-9][A-Za-z0-9._-]*$/.test(id)) throw new Error(`${label} is invalid.`);
    return id;
}

function requireLogicalPath(value         , label        , allowDot = false)         {
    if (allowDot && value === "") return ".";
    const input = requireText(value, label, 1_024).replaceAll("\\", "/");
    if (/^(?:[A-Za-z]:|\/)/.test(input)) throw new Error(`${label} must be relative.`);
    const parts = input.split("/");
    if (parts.some((part) => part === "" || part === ".." || (!allowDot && part === "."))) {
        throw new Error(`${label} contains an unsafe segment.`);
    }
    return input;
}

function requireInteger(value         , minimum        , maximum        , label        )         {
    if (!Number.isInteger(value) || (value          ) < minimum || (value          ) > maximum) {
        throw new Error(`${label} must be an integer in range.`);
    }
    return value          ;
}
