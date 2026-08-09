import type {
    DatSessionFieldView,
    DatSessionFrameStructureView,
} from "./dat-session-contract.js";
import type { DatFrameProjection } from "../model/dat-projection.js";

export type ProjectDatErrorCode =
    | "project-disabled"
    | "catalog-invalid"
    | "unknown-object"
    | "object-unavailable"
    | "unknown-session"
    | "unknown-asset"
    | "invalid-asset"
    | "revision-conflict"
    | "read-only-session"
    | "preview-failed"
    | "save-failed"
    | "invalid-request";

export interface ProjectObjectView {
    readonly objectKey: string;
    readonly oid: number;
    readonly type: number;
    readonly availablePrimary: boolean;
}

export interface ProjectCatalogView {
    readonly catalogRevision: number;
    readonly objects: readonly ProjectObjectView[];
}

export interface ProjectSpriteRangeView {
    readonly frameLo: number;
    readonly frameHi: number;
    readonly assetId: string;
    readonly w: number;
    readonly h: number;
    readonly row: number;
    readonly col: number;
}

export type ProjectFrameView = Omit<DatFrameProjection, "sound">;

export interface ProjectPreviewObjectView {
    readonly oid: number;
    readonly type: number;
    readonly name: string;
    readonly spriteRanges: readonly ProjectSpriteRangeView[];
    readonly frames: readonly ProjectFrameView[];
}

export interface NativePreviewEntityView {
    readonly slot: number;
    readonly oid: number;
    readonly frame: number;
    readonly pic: number;
    readonly renderPic?: number;
    readonly facing: number;
    readonly x: number;
    readonly y: number;
    readonly z: number;
    readonly xInt: number;
    readonly yInt: number;
    readonly zInt: number;
    readonly displayZ: number;
    readonly velocity: { readonly x: number; readonly y: number; readonly z: number };
    readonly renderOffsetX: number;
    readonly frameDelay: number;
    readonly hitStop: number;
    readonly team: number;
    readonly target: number;
    readonly holder: number;
    readonly link: number;
    readonly ai: boolean;
    readonly objectType: number | null;
    readonly kind: "root" | "actor" | "clone" | "projectile" | "unknown";
    readonly lineageId: string;
    readonly firstSeenTick: number;
    readonly lastSeenTick: number;
    readonly resourceAvailable: boolean;
}

export interface NativePreviewTickView {
    readonly tick: number;
    readonly cameraX: number;
    readonly cameraVelocity: number;
    readonly background: {
        readonly width: number;
        readonly zMin: number;
        readonly zMax: number;
        readonly boundLeft: number;
        readonly boundRight: number;
    };
    readonly entities: readonly NativePreviewEntityView[];
}

export type NativePreviewInputKey = "A" | "D" | "W" | "S" | "J" | "K" | "L";

export interface NativePreviewInputStep {
    readonly tick: number;
    readonly keys: readonly NativePreviewInputKey[];
}

export interface NativePreviewView {
    readonly metadata: {
        readonly runtime: "ntsd_cpp";
        readonly tickDriver: "SimulationTickDriver";
        readonly renderer: "none";
        readonly seed: number;
        readonly startFrame: number;
        readonly initialFrame?: number;
        readonly inputPlan?: readonly NativePreviewInputStep[];
        readonly ticksRequested: number;
        readonly stage: {
            readonly index: number;
            readonly name: string;
            readonly width: number;
            readonly zMin: number;
            readonly zMax: number;
            readonly background?: {
                readonly shadow?: {
                    readonly path: string;
                    readonly width: number;
                    readonly height: number;
                    readonly assetId?: string;
                };
                readonly layers: readonly {
                    readonly path: string;
                    readonly transparency: number;
                    readonly parallaxWidth: number;
                    readonly x: number;
                    readonly y: number;
                    readonly loop: number;
                    readonly cc: number;
                    readonly c1: number;
                    readonly c2: number;
                    readonly animCounter: number;
                    readonly assetId?: string;
                }[];
            };
        };
        readonly initial: {
            readonly p1: { readonly x: number; readonly y: number; readonly z: number };
            readonly p2: { readonly x: number; readonly y: number; readonly z: number };
        };
    };
    readonly ticks: readonly NativePreviewTickView[];
    readonly resources: readonly ProjectPreviewObjectView[];
    readonly trace: NativePreviewTraceView;
}

export interface NativePreviewTraceEntityView {
    readonly lineageId: string;
    readonly slot: number;
    readonly oid: number;
    readonly kind: "root" | "actor" | "clone" | "projectile" | "unknown";
    readonly firstSeenTick: number;
    readonly lastSeenTick: number;
    readonly completedTick: number | null;
    readonly completion: "root-ended" | "spawned" | "despawned" | "landed" | "persistent" | "unknown";
}

export interface NativePreviewTraceEventView {
    readonly tick: number;
    readonly kind: "spawn" | "despawn";
    readonly lineageId: string;
    readonly slot: number;
    readonly oid: number;
}

export interface NativePreviewTraceView {
    readonly rootSkillEndedTick: number | null;
    readonly progressEndTick: number | null;
    readonly playbackEndTick: number;
    readonly status: "complete" | "timeout" | "persistent";
    readonly pendingProjectiles: readonly string[];
    readonly entities: readonly NativePreviewTraceEntityView[];
    readonly events: readonly NativePreviewTraceEventView[];
}

export interface ProjectSessionView {
    readonly sessionId: string;
    readonly revision: number;
    readonly dirty: boolean;
    readonly writable: boolean;
    readonly oid: number;
    readonly type: number;
    readonly name: string;
    readonly spriteRanges: readonly ProjectSpriteRangeView[];
    readonly previewObjects: readonly ProjectPreviewObjectView[];
    readonly frames: readonly ProjectFrameView[];
    readonly fields: readonly DatSessionFieldView[];
    readonly structureCapabilities: readonly DatSessionFrameStructureView[];
    readonly preview: NativePreviewView;
    readonly diagnostics: readonly {
        readonly code: string;
        readonly severity: "warning" | "error";
        readonly message: string;
    }[];
}

export interface ProjectPreviewRequest {
    readonly sessionId: string;
    readonly expectedRevision: number;
    readonly startFrame: number;
    readonly initialFrame?: number;
    readonly inputPlan?: readonly NativePreviewInputStep[];
    readonly ticks: number;
}

export interface ProjectPreviewResponse {
    readonly sessionId: string;
    readonly revision: number;
    readonly preview: NativePreviewView;
}

export interface ProjectCloseResponse {
    readonly sessionId: string;
    readonly closed: true;
}

export interface ProjectAssetResponse {
    readonly bytes: Buffer;
}

export interface ProjectSaveResponse {
    readonly sessionId: string;
    readonly revision: number;
    readonly dirty: false;
    readonly recovery: {
        readonly target: { readonly name: string; readonly exists: boolean; readonly size?: number; readonly sha256?: string };
        readonly replacement: { readonly name: string; readonly exists: boolean; readonly size?: number; readonly sha256?: string };
        readonly backup: { readonly name: string; readonly exists: boolean; readonly size?: number; readonly sha256?: string };
    };
}
