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
import type { DatBlockType } from "../syntax/byte-cst.js";
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

export type DatSessionFieldKind = "number" | "string" | "integer-pair";
export type DatSessionFieldValue = number | string | readonly [number, number];

export interface DatFieldLocator {
    readonly scope: DatSessionFieldScope;
    readonly occurrence: number;
    readonly spriteRangeIndex?: number;
    readonly frameId?: number;
    readonly frameOccurrence?: number;
    readonly blockType?: DatBlockType;
    readonly blockIndex?: number;
}

export interface DatSessionScalarFieldView extends DatFieldLocator {
    readonly fieldId: string;
    readonly key: string;
    readonly kind: "number" | "string";
    readonly numericKind?: DatSessionNumericKind;
    readonly value: number | string;
}

export interface DatSessionPairView extends DatFieldLocator {
    readonly fieldId: string;
    readonly key: "catchingact" | "caughtact";
    readonly kind: "integer-pair";
    readonly value: readonly [number, number];
}

export type DatSessionFieldView = DatSessionScalarFieldView | DatSessionPairView;

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

export interface DatSessionBlockStructureView {
    readonly capabilityId: string;
    readonly blockType: DatBlockType;
    readonly blockIndex: number;
    readonly canCopy: boolean;
    readonly canDelete: boolean;
}

export interface DatSessionFrameStructureView {
    readonly capabilityId: string;
    readonly frameId: number;
    readonly occurrence: number;
    readonly canCopy: boolean;
    readonly canDelete: boolean;
    readonly blocks: readonly DatSessionBlockStructureView[];
}

export interface DatSessionView {
    readonly sessionId: string;
    readonly revision: number;
    readonly dirty: boolean;
    readonly format: DatInputFormat;
    readonly encrypted: boolean;
    readonly fields: readonly DatSessionFieldView[];
    readonly structureCapabilities: readonly DatSessionFrameStructureView[];
    readonly projection: DatSessionProjectionView;
    readonly diagnostics: readonly DatSessionDiagnosticView[];
}

export interface DatSessionEditRequest {
    readonly sessionId: string;
    readonly fieldId: string;
    readonly value: DatSessionFieldValue;
    readonly expectedRevision: number;
}

export interface DatSessionBatchEditItem {
    readonly fieldId: string;
    readonly value: DatSessionFieldValue;
}

export interface DatSessionBatchEditRequest {
    readonly sessionId: string;
    readonly edits: readonly DatSessionBatchEditItem[];
    readonly expectedRevision: number;
}

export type DatSessionStructureOperation =
    | "copy-frame"
    | "delete-frame"
    | "create-block"
    | "copy-block"
    | "delete-block";

export interface DatSessionStructureEditRequest {
    readonly sessionId: string;
    readonly capabilityId: string;
    readonly operation: DatSessionStructureOperation;
    readonly newFrameId?: number;
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
    readonly numericReadMode?: "strict" | "native-compatible";
    readonly now?: () => number;
    readonly idFactory?: () => string;
}

// Persistence/save is deliberately deferred until a later gate defines its revision and overwrite contract.
