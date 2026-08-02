import {
    dataDiagnostic,
    type ByteSpan,
    type DataDiagnostic,
} from "./data-diagnostic.js";

export interface SpanPatch {
    span: ByteSpan;
    replacement: Uint8Array;
    label: string;
}

export interface AppliedSpanChange {
    label: string;
    originalSpan: ByteSpan;
    outputSpan: ByteSpan;
    originalLength: number;
    replacementLength: number;
}

export interface SpanPatchEmission {
    bytes: Buffer;
    diagnostics: DataDiagnostic[];
    changes: AppliedSpanChange[];
}

function validatePatches(sourceLength: number, patches: readonly SpanPatch[]): DataDiagnostic[] {
    const diagnostics: DataDiagnostic[] = [];
    for (const patch of patches) {
        if (!Number.isSafeInteger(patch.span.start)
            || !Number.isSafeInteger(patch.span.end)
            || patch.span.start < 0
            || patch.span.end < patch.span.start
            || patch.span.end > sourceLength) {
            diagnostics.push(dataDiagnostic(
                "invalid-span",
                `Edit ${patch.label} has an invalid source span.`,
                { span: { ...patch.span }, labels: [patch.label] },
            ));
        }
    }
    if (diagnostics.length > 0) return diagnostics;

    const ordered = [...patches].sort((left, right) => (
        left.span.start - right.span.start || left.span.end - right.span.end
    ));
    for (let index = 1; index < ordered.length; index += 1) {
        const previous = ordered[index - 1]!;
        const current = ordered[index]!;
        if (current.span.start < previous.span.end
            || (current.span.start === previous.span.start && current.span.end === previous.span.end)) {
            diagnostics.push(dataDiagnostic(
                "overlapping-edit",
                `Edits ${previous.label} and ${current.label} overlap.`,
                {
                    span: {
                        start: Math.min(previous.span.start, current.span.start),
                        end: Math.max(previous.span.end, current.span.end),
                    },
                    labels: [previous.label, current.label],
                },
            ));
        }
    }
    return diagnostics;
}

export function emitSpanPatches(sourceInput: Uint8Array, patches: readonly SpanPatch[]): SpanPatchEmission {
    const source = Buffer.from(sourceInput);
    if (patches.length === 0) {
        return { bytes: Buffer.from(source), diagnostics: [], changes: [] };
    }
    const diagnostics = validatePatches(source.length, patches);
    if (diagnostics.length > 0) {
        return { bytes: Buffer.from(source), diagnostics, changes: [] };
    }

    const ordered = [...patches].sort((left, right) => (
        left.span.start - right.span.start || left.span.end - right.span.end
    ));
    const chunks: Buffer[] = [];
    const changes: AppliedSpanChange[] = [];
    let sourceOffset = 0;
    let outputOffset = 0;
    for (const patch of ordered) {
        const unchanged = source.subarray(sourceOffset, patch.span.start);
        const replacement = Buffer.from(patch.replacement);
        chunks.push(unchanged, replacement);
        outputOffset += unchanged.length;
        changes.push({
            label: patch.label,
            originalSpan: { ...patch.span },
            outputSpan: { start: outputOffset, end: outputOffset + replacement.length },
            originalLength: patch.span.end - patch.span.start,
            replacementLength: replacement.length,
        });
        outputOffset += replacement.length;
        sourceOffset = patch.span.end;
    }
    chunks.push(source.subarray(sourceOffset));
    return { bytes: Buffer.concat(chunks), diagnostics: [], changes };
}
