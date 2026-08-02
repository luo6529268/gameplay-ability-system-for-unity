import {
    parseDatCst,
    type DatBlockType,
    type DatCstDocument,
    type DatFieldCst,
} from "../syntax/byte-cst.js";
import { DatEnvelopeDocument } from "../syntax/dat-envelope.js";
import { dataDiagnostic, type DataDiagnostic } from "../syntax/data-diagnostic.js";
import {
    emitSpanPatches,
    type AppliedSpanChange,
    type SpanPatch,
} from "../syntax/patch-emitter.js";
import { projectDatCst, type DatProjection } from "./dat-projection.js";

export interface SetScalarCommand {
    kind: "set-scalar";
    name: string;
    field: DatFieldCst;
    value: number | string;
}

export interface CommandApplication {
    applied: boolean;
    diagnostics: DataDiagnostic[];
}

export interface DocumentEmission {
    bytes: Buffer;
    diagnostics: DataDiagnostic[];
    changes: AppliedSpanChange[];
}

export function createSetScalarCommand(
    name: string,
    field: DatFieldCst,
    value: number | string,
): SetScalarCommand {
    return { kind: "set-scalar", name, field, value };
}

export function isLatin1ScalarString(value: unknown): value is string {
    if (typeof value !== "string" || /[\r\n\0]/.test(value)) return false;
    for (const character of value) {
        if (character.codePointAt(0)! > 0xff) return false;
    }
    return true;
}

function replacementForCommand(command: SetScalarCommand): Buffer | DataDiagnostic {
    if (command.name.length === 0) {
        return dataDiagnostic("unsupported-edit", "Every edit command must have a non-empty name.");
    }
    if (command.field.scalarKind === "number") {
        if (typeof command.value !== "number" || !Number.isFinite(command.value)) {
            return dataDiagnostic("unsupported-edit", `Numeric field ${command.field.key} requires a finite number.`);
        }
        return Buffer.from(String(command.value), "ascii");
    }
    if (command.field.scalarKind === "string") {
        if (!isLatin1ScalarString(command.value)) {
            return dataDiagnostic("unsupported-edit", `String field ${command.field.key} requires a single-line Latin-1 value.`);
        }
        return Buffer.from(command.value, "latin1");
    }
    return dataDiagnostic("unsupported-edit", `Opaque field ${command.field.key} cannot be edited safely.`);
}

function last<T>(values: readonly T[]): T | undefined {
    return values.length === 0 ? undefined : values[values.length - 1];
}

export class LosslessDatDocument {
    public readonly cst: DatCstDocument;
    public readonly envelope?: DatEnvelopeDocument;
    private readonly patches: SpanPatch[] = [];

    private constructor(cst: DatCstDocument, envelope?: DatEnvelopeDocument) {
        this.cst = cst;
        this.envelope = envelope;
    }

    public static fromPlaintext(plaintext: Uint8Array): LosslessDatDocument {
        return new LosslessDatDocument(parseDatCst(plaintext));
    }

    public static fromEncrypted(encrypted: Uint8Array): LosslessDatDocument {
        const envelope = DatEnvelopeDocument.open(encrypted);
        return new LosslessDatDocument(parseDatCst(envelope.plaintext), envelope);
    }

    public get projection(): DatProjection {
        if (this.patches.length === 0) return projectDatCst(this.cst);
        return projectDatCst(parseDatCst(this.emitPlaintext()));
    }

    public get diagnostics(): readonly DataDiagnostic[] {
        return [
            ...(this.envelope?.diagnostics ?? []),
            ...this.cst.diagnostics,
            ...this.projection.diagnostics,
            ...this.emitPlaintextResult().diagnostics,
        ];
    }

    public findTopField(key: string, occurrence: "first" | "last" = "last"): DatFieldCst | undefined {
        const fields = this.cst.topFields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

    public findFrameField(
        frameId: number,
        key: string,
        occurrence: "first" | "last" = "last",
    ): DatFieldCst | undefined {
        const frames = this.cst.frames.filter((frame) => frame.frameId === frameId);
        const frame = occurrence === "first" ? frames[0] : last(frames);
        if (!frame) return undefined;
        const fields = frame.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

    public findNestedField(
        frameId: number,
        blockType: DatBlockType,
        blockIndex: number,
        key: string,
        occurrence: "first" | "last" = "last",
    ): DatFieldCst | undefined {
        const frame = last(this.cst.frames.filter((candidate) => candidate.frameId === frameId));
        const block = frame?.blocks.filter((candidate) => candidate.type === blockType)[blockIndex];
        if (!block) return undefined;
        const fields = block.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

    public findSpriteRangeField(
        rangeIndex: number,
        key: "file" | "w" | "h" | "row" | "col",
        occurrence: "first" | "last" = "last",
    ): DatFieldCst | undefined {
        const range = this.cst.spriteRanges[rangeIndex];
        if (!range) return undefined;
        if (key === "file") return range.fileField;
        const fields = range.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

    public apply(command: SetScalarCommand): CommandApplication {
        if (this.cst.encoding === "utf16le" || this.cst.encoding === "utf16be"
            || this.cst.encoding === "utf32le" || this.cst.encoding === "utf32be") {
            return {
                applied: false,
                diagnostics: [dataDiagnostic("unsupported-encoding", `Cannot edit ${this.cst.encoding} DAT bytes.`)],
            };
        }
        const belongsToDocument = [
            ...this.cst.topFields,
            ...this.cst.spriteRanges.flatMap((range) => [range.fileField, ...range.fields]),
            ...this.cst.frames.flatMap((frame) => [
                ...frame.fields,
                ...frame.blocks.flatMap((block) => block.fields),
            ]),
        ].some((field) => field === command.field);
        if (!belongsToDocument) {
            return {
                applied: false,
                diagnostics: [dataDiagnostic("unsupported-edit", "The selected field does not belong to this document.")],
            };
        }
        const replacement = replacementForCommand(command);
        if (!Buffer.isBuffer(replacement)) return { applied: false, diagnostics: [replacement] };
        const sameSpanIndex = this.patches.findIndex((patch) => (
            patch.span.start === command.field.valueSpan.start && patch.span.end === command.field.valueSpan.end
        ));
        if (replacement.equals(command.field.rawValue)) {
            if (sameSpanIndex < 0) return { applied: false, diagnostics: [] };
            this.patches.splice(sameSpanIndex, 1);
            return { applied: true, diagnostics: [] };
        }

        const patch: SpanPatch = {
            span: { ...command.field.valueSpan },
            replacement,
            label: command.name,
        };
        if (sameSpanIndex >= 0) this.patches[sameSpanIndex] = patch;
        else this.patches.push(patch);
        return { applied: true, diagnostics: [] };
    }

    public emitPlaintextResult(): DocumentEmission {
        return emitSpanPatches(this.cst.source, this.patches);
    }

    public emitPlaintext(): Buffer {
        return this.emitPlaintextResult().bytes;
    }

    public emitFile(): Buffer {
        const plaintext = this.emitPlaintext();
        return this.envelope ? this.envelope.emit(plaintext) : plaintext;
    }
}
