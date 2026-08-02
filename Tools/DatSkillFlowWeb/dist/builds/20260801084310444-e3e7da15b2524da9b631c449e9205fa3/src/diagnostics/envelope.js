// dat-skill-flow-build:20260801084310444-e3e7da15b2524da9b631c449e9205fa3
import {
    expectEnum,
    expectLiteral,
    expectRecord,
    expectStrictKeys,
    expectString,
    validator,
} from "../validation/strict.js";

export const diagnosticCodes = [
    "parse-failure",
    "missing-asset",
    "unsupported-rule",
    "unsafe-save",
    "method-not-allowed",
    "forbidden-request",
    "request-body-not-allowed",
    "not-found",
    "internal-error",
]         ;

export const diagnosticSeverities = ["info", "warning", "error"]         ;

                                                            
                                                                     

                                     
                     
                         
                                 
                    
                         
                                      
 

const codeSet = new Set                (diagnosticCodes);
const severitySet = new Set                    (diagnosticSeverities);
const diagnosticKeys = new Set(["schemaVersion", "code", "severity", "message", "repairApplied", "details"]);

export const diagnosticCodeSchema = validator                ((value) => expectEnum(value, codeSet, ["code"]));

export const diagnosticEnvelopeSchema = validator                    ((value) => {
    const record = expectRecord(value, []);
    expectStrictKeys(record, diagnosticKeys, []);
    const details = record.details === undefined
        ? undefined
        : { ...expectRecord(record.details, ["details"]) };
    return {
        schemaVersion: expectLiteral(record.schemaVersion, 1, ["schemaVersion"]),
        code: expectEnum(record.code, codeSet, ["code"]),
        severity: expectEnum(record.severity, severitySet, ["severity"]),
        message: expectString(record.message, ["message"], 1),
        repairApplied: expectLiteral(record.repairApplied, false, ["repairApplied"]),
        ...(details === undefined ? {} : { details }),
    };
});

export function createDiagnostic(
    code                            ,
    message        ,
    details                          ,
)                     {
    return diagnosticEnvelopeSchema.parse({
        schemaVersion: 1,
        code,
        severity: "error",
        message,
        repairApplied: false,
        ...(details === undefined ? {} : { details }),
    });
}
