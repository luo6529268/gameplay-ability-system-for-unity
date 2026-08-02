import type {
    BdyProjection,
    BPointProjection,
    CPointProjection,
    DatFrameProjection,
    DatTopProjection,
    ItrProjection,
    OPointProjection,
    SpriteRangeProjection,
    WPointProjection,
} from "../model/dat-projection.js";
import type { DataDiagnosticCode } from "../syntax/data-diagnostic.js";

export type DatSessionErrorCode =
    | "unknown-session"
    | "unknown-field"
    | "revision-conflict"
    | "session-limit"
    | "field-limit"
    | "byte-limit"
    | "view-limit"
    | "expired"
    | "invalid-request"
    | "reload-failed";

// This selection is trusted server-owned project metadata. An NTSD encrypted DAT envelope has no magic
// marker, so its authenticity cannot be inferred from bytes and this value must never come from a client DTO.
// The future NTSD ProjectCatalog loader must supply "encrypted" for project DAT documents.
export type DatInputFormat = "plaintext" | "encrypted";
export type DatSessionNumericKind = "integer" | "number";

export interface DatSessionDiagnosticView {
    readonly code: DataDiagnosticCode;
    readonly severity: "warning" | "error";
    readonly message: string;
}

export type DatSessionFieldScope = "top" | "sprite" | "frame" | "block";

export interface DatSessionFieldView {
    readonly fieldId: string;
    readonly key: string;
    readonly kind: "number" | "string";
    readonly numericKind?: DatSessionNumericKind;
    readonly value: number | string;
    readonly scope: DatSessionFieldScope;
    readonly occurrence?: number;
    readonly spriteRangeIndex?: number;
    readonly frameId?: number;
    readonly frameOccurrence?: number;
    readonly blockType?: "itr" | "bdy" | "opoint" | "wpoint" | "bpoint" | "cpoint";
    readonly blockIndex?: number;
}

export interface DatSessionProjectionView {
    readonly top: Readonly<DatTopProjection>;
    readonly spriteRanges: readonly Readonly<SpriteRangeProjection>[];
    readonly frames: readonly Readonly<DatFrameProjection & {
        readonly itrs: readonly Readonly<ItrProjection>[];
        readonly bdys: readonly Readonly<BdyProjection>[];
        readonly opoints: readonly Readonly<OPointProjection>[];
        readonly wpoints: readonly Readonly<WPointProjection>[];
        readonly bpoints: readonly Readonly<BPointProjection>[];
        readonly cpoints: readonly Readonly<CPointProjection>[];
    }>[];
}

export interface DatSessionView {
    readonly sessionId: string;
    readonly revision: number;
    readonly format: DatInputFormat;
    readonly encrypted: boolean;
    readonly fields: readonly DatSessionFieldView[];
    readonly projection: DatSessionProjectionView;
    readonly diagnostics: readonly DatSessionDiagnosticView[];
}

export interface DatSessionEditRequest {
    readonly sessionId: string;
    readonly fieldId: string;
    readonly value: number | string;
    readonly expectedRevision: number;
}

export interface DatSessionReloadRequest {
    readonly sessionId: string;
    readonly expectedRevision: number;
}

export interface DatSessionServiceOptions {
    readonly maxSessions?: number;
    readonly maxFieldsPerSession?: number;
    readonly maxLoadedBytes?: number;
    readonly idleTtlMs?: number;
    readonly maxDiagnostics?: number;
    readonly maxDiagnosticMessageLength?: number;
    readonly maxProjectionBytes?: number;
    readonly maxViewBytes?: number;
    readonly maxStringBytes?: number;
    readonly now?: () => number;
    readonly idFactory?: () => string;
}

// Persistence/save is deliberately deferred until a later gate defines its revision and overwrite contract.
