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
] as const;

export const diagnosticSeverities = ["info", "warning", "error"] as const;

export type DiagnosticCode = typeof diagnosticCodes[number];
export type DiagnosticSeverity = typeof diagnosticSeverities[number];

export interface DiagnosticEnvelope {
    schemaVersion: 1;
    code: DiagnosticCode;
    severity: DiagnosticSeverity;
    message: string;
    repairApplied: false;
    details?: Record<string, unknown>;
}

const codeSet = new Set<DiagnosticCode>(diagnosticCodes);
const severitySet = new Set<DiagnosticSeverity>(diagnosticSeverities);
const diagnosticKeys = new Set(["schemaVersion", "code", "severity", "message", "repairApplied", "details"]);

export const diagnosticCodeSchema = validator<DiagnosticCode>((value) => expectEnum(value, codeSet, ["code"]));

export const diagnosticEnvelopeSchema = validator<DiagnosticEnvelope>((value) => {
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
    code: DiagnosticEnvelope["code"],
    message: string,
    details?: Record<string, unknown>,
): DiagnosticEnvelope {
    return diagnosticEnvelopeSchema.parse({
        schemaVersion: 1,
        code,
        severity: "error",
        message,
        repairApplied: false,
        ...(details === undefined ? {} : { details }),
    });
}
