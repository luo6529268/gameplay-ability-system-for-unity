export interface ByteSpan {
    start: number;
    end: number;
}

export type DataDiagnosticCode =
    | "dat-envelope-too-short"
    | "invalid-dat-prefix"
    | "unsupported-encoding"
    | "invalid-span"
    | "overlapping-edit"
    | "malformed-frame"
    | "malformed-block"
    | "unsupported-edit"
    | "unsafe-resource-path"
    | "invalid-bmp";

export interface DataDiagnostic {
    code: DataDiagnosticCode;
    severity: "warning" | "error";
    message: string;
    span?: ByteSpan;
    labels?: string[];
    path?: string;
    reason?: "absolute" | "traversal" | "nul" | "empty";
}

export function dataDiagnostic(
    code: DataDiagnosticCode,
    message: string,
    details: Omit<DataDiagnostic, "code" | "severity" | "message"> = {},
): DataDiagnostic {
    return { code, severity: "error", message, ...details };
}
