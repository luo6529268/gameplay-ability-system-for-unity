import { type DatFieldCst } from "../syntax/byte-cst.js";
import { dataDiagnostic, type ByteSpan, type DataDiagnostic } from "../syntax/data-diagnostic.js";
import { emitSpanPatches, type SpanPatch } from "../syntax/patch-emitter.js";

export type DataTxtSection = "object" | "background";

export interface DataTxtEntry {
    section: DataTxtSection;
    id: number;
    type?: number;
    file: string;
    span: ByteSpan;
    fields: {
        id: DatFieldCst;
        type?: DatFieldCst;
        file: DatFieldCst;
    };
}

export interface ResourcePathDiagnostic extends DataDiagnostic {
    code: "unsafe-resource-path";
    path: string;
    reason: "absolute" | "traversal" | "nul" | "empty";
}

function lineSpans(source: Uint8Array): Array<{ content: ByteSpan; full: ByteSpan }> {
    const lines: Array<{ content: ByteSpan; full: ByteSpan }> = [];
    let start = 0;
    let offset = 0;
    while (offset < source.length) {
        if (source[offset] === 0x0d || source[offset] === 0x0a) {
            const end = offset;
            offset += source[offset] === 0x0d && source[offset + 1] === 0x0a ? 2 : 1;
            lines.push({ content: { start, end }, full: { start, end: offset } });
            start = offset;
        } else offset += 1;
    }
    if (start < source.length) lines.push({ content: { start, end: source.length }, full: { start, end: source.length } });
    return lines;
}

function makeField(
    source: Buffer,
    key: string,
    keyStart: number,
    keyEnd: number,
    valueStart: number,
    valueEnd: number,
    numericValue?: number,
): DatFieldCst {
    const rawValue = Buffer.from(source.subarray(valueStart, valueEnd));
    return {
        key,
        keySpan: { start: keyStart, end: keyEnd },
        valueSpan: { start: valueStart, end: valueEnd },
        rawValue,
        scalarKind: numericValue === undefined ? "string" : "number",
        ...(numericValue === undefined ? {} : { numericValue }),
    };
}

function trimFileEnd(text: string, start: number): number {
    const comment = text.indexOf("#", start);
    let end = comment < 0 ? text.length : comment;
    while (end > start && (text[end - 1] === " " || text[end - 1] === "\t")) end -= 1;
    return end;
}

function parseEntry(source: Buffer, line: { content: ByteSpan; full: ByteSpan }, section: DataTxtSection): DataTxtEntry | undefined {
    const text = source.subarray(line.content.start, line.content.end).toString("latin1");
    const idMatch = /^\s*id\s*:\s*([+-]?\d+)/.exec(text);
    if (!idMatch) return undefined;
    const id = Number.parseInt(idMatch[1]!, 10);
    if (!Number.isSafeInteger(id) || id < 0) return undefined;
    const idToken = idMatch[1]!;
    const idRelativeStart = idMatch.index + idMatch[0].lastIndexOf(idToken);
    const idKeyRelativeStart = idMatch.index + idMatch[0].indexOf("id");
    const idField = makeField(
        source, "id",
        line.content.start + idKeyRelativeStart,
        line.content.start + idKeyRelativeStart + 2,
        line.content.start + idRelativeStart,
        line.content.start + idRelativeStart + idToken.length,
        id,
    );

    const typeMatch = /\btype\s*:\s*([+-]?\d+)/.exec(text);
    if (section === "object" && !typeMatch) return undefined;
    let type: number | undefined;
    let typeField: DatFieldCst | undefined;
    if (typeMatch) {
        type = Number.parseInt(typeMatch[1]!, 10);
        const token = typeMatch[1]!;
        const valueStart = typeMatch.index + typeMatch[0].lastIndexOf(token);
        const keyStart = typeMatch.index + typeMatch[0].indexOf("type");
        typeField = makeField(
            source, "type",
            line.content.start + keyStart,
            line.content.start + keyStart + 4,
            line.content.start + valueStart,
            line.content.start + valueStart + token.length,
            type,
        );
    }

    const fileMatch = /\bfile\s*:\s*/.exec(text);
    if (!fileMatch) return undefined;
    const relativeFileStart = fileMatch.index + fileMatch[0].length;
    const relativeFileEnd = trimFileEnd(text, relativeFileStart);
    if (relativeFileEnd <= relativeFileStart) return undefined;
    const file = text.slice(relativeFileStart, relativeFileEnd);
    const keyStart = fileMatch.index + fileMatch[0].indexOf("file");
    const fileField = makeField(
        source, "file",
        line.content.start + keyStart,
        line.content.start + keyStart + 4,
        line.content.start + relativeFileStart,
        line.content.start + relativeFileEnd,
    );
    return {
        section,
        id,
        ...(type === undefined ? {} : { type }),
        file,
        span: { ...line.full },
        fields: {
            id: idField,
            ...(typeField === undefined ? {} : { type: typeField }),
            file: fileField,
        },
    };
}

export function diagnoseResourcePath(path: string, span?: ByteSpan): ResourcePathDiagnostic | undefined {
    let reason: ResourcePathDiagnostic["reason"] | undefined;
    if (path.length === 0) reason = "empty";
    else if (path.includes("\0")) reason = "nul";
    else if (/^(?:[a-zA-Z]:[\\/]|[\\/]{2}|[\\/])/.test(path)) reason = "absolute";
    else if (path.split(/[\\/]+/).includes("..")) reason = "traversal";
    if (!reason) return undefined;
    return dataDiagnostic(
        "unsafe-resource-path",
        `Resource path is syntactically unsafe (${reason}): ${path}`,
        { path, reason, ...(span === undefined ? {} : { span }) },
    ) as ResourcePathDiagnostic;
}

export class DataTxtDocument {
    public readonly source: Buffer;
    public readonly entries: readonly DataTxtEntry[];
    public readonly diagnostics: readonly ResourcePathDiagnostic[];
    private readonly patches: SpanPatch[] = [];

    private constructor(source: Buffer, entries: DataTxtEntry[], diagnostics: ResourcePathDiagnostic[]) {
        this.source = source;
        this.entries = entries;
        this.diagnostics = diagnostics;
    }

    public static parse(input: Uint8Array): DataTxtDocument {
        const source = Buffer.from(input);
        const entries: DataTxtEntry[] = [];
        const diagnostics: ResourcePathDiagnostic[] = [];
        let section: DataTxtSection | undefined;
        for (const line of lineSpans(source)) {
            const text = source.subarray(line.content.start, line.content.end).toString("latin1");
            const trimmed = text.trimStart();
            if (trimmed.startsWith("#") || trimmed.length === 0) continue;
            if (trimmed.startsWith("<")) {
                if (trimmed.startsWith("<object>")) section = "object";
                else if (trimmed.startsWith("<background>")) section = "background";
                else section = undefined;
                continue;
            }
            if (!section) continue;
            const entry = parseEntry(source, line, section);
            if (!entry) continue;
            entries.push(entry);
            const diagnostic = diagnoseResourcePath(entry.file, entry.fields.file.valueSpan);
            if (diagnostic) diagnostics.push(diagnostic);
        }
        return new DataTxtDocument(source, entries, diagnostics);
    }

    public findEntryField(
        section: DataTxtSection,
        id: number,
        key: "id" | "type" | "file",
        duplicateIndex = 0,
    ): DatFieldCst | undefined {
        return this.entries.filter((entry) => entry.section === section && entry.id === id)[duplicateIndex]?.fields[key];
    }

    public setScalar(name: string, field: DatFieldCst, value: number | string): DataDiagnostic[] {
        if (!this.entries.some((entry) => Object.values(entry.fields).includes(field))) {
            return [dataDiagnostic("unsupported-edit", "The selected data.txt field does not belong to this document.")];
        }
        let replacement: Buffer;
        if (field.scalarKind === "number" && typeof value === "number" && Number.isFinite(value)) {
            replacement = Buffer.from(String(value), "ascii");
        } else if (field.scalarKind === "string" && typeof value === "string" && !/[\r\n\0]/.test(value)) {
            if (field.key === "file") {
                const pathDiagnostic = diagnoseResourcePath(value, field.valueSpan);
                if (pathDiagnostic) return [pathDiagnostic];
            }
            replacement = Buffer.from(value, "utf8");
        } else {
            return [dataDiagnostic("unsupported-edit", `Unsupported replacement for data.txt field ${field.key}.`)];
        }
        const existing = this.patches.findIndex((patch) => patch.span.start === field.valueSpan.start && patch.span.end === field.valueSpan.end);
        if (replacement.equals(field.rawValue)) {
            if (existing >= 0) this.patches.splice(existing, 1);
            return [];
        }
        const patch = { label: name, span: { ...field.valueSpan }, replacement };
        if (existing >= 0) this.patches[existing] = patch;
        else this.patches.push(patch);
        return [];
    }

    public emit(): Buffer {
        return emitSpanPatches(this.source, this.patches).bytes;
    }
}

export type CppObjectLoadOutcome =
    | "decrypt-failed"
    | "allocation-failed"
    | "parse-failed"
    | "loaded";

export interface CppObjectLoadAttempt<T> {
    entry: T;
    outcome: CppObjectLoadOutcome;
}

export interface CppObjectLoadSimulation<T> {
    attempts: readonly CppObjectLoadAttempt<T>[];
    occupied: ReadonlyMap<number, CppObjectLoadAttempt<T>>;
    loaded: ReadonlyMap<number, T>;
}

function recordCppObjectLoadOutcome<T extends Pick<DataTxtEntry, "id">>(
    entry: T,
    outcome: CppObjectLoadOutcome,
    attempts: CppObjectLoadAttempt<T>[],
    occupied: Map<number, CppObjectLoadAttempt<T>>,
    loaded: Map<number, T>,
): void {
    const attempt = { entry, outcome };
    attempts.push(attempt);
    if (outcome === "decrypt-failed" || outcome === "allocation-failed") return;
    occupied.set(entry.id, attempt);
    if (outcome === "loaded") loaded.set(entry.id, entry);
}

/**
 * Models LoadingScene::load_oid duplicate behavior. An OID is not occupied until
 * alloc_char succeeds; once allocated it remains occupied even if DAT parsing fails.
 */
export function simulateCppObjectLoads<T extends Pick<DataTxtEntry, "id">>(
    entries: readonly T[],
    load: (entry: T) => CppObjectLoadOutcome,
): CppObjectLoadSimulation<T> {
    const attempts: CppObjectLoadAttempt<T>[] = [];
    const occupied = new Map<number, CppObjectLoadAttempt<T>>();
    const loaded = new Map<number, T>();
    for (const entry of entries) {
        if (occupied.has(entry.id)) continue;
        recordCppObjectLoadOutcome(entry, load(entry), attempts, occupied, loaded);
    }
    return { attempts, occupied, loaded };
}

export async function simulateCppObjectLoadsAsync<T extends Pick<DataTxtEntry, "id">>(
    entries: readonly T[],
    load: (entry: T) => CppObjectLoadOutcome | Promise<CppObjectLoadOutcome>,
): Promise<CppObjectLoadSimulation<T>> {
    const attempts: CppObjectLoadAttempt<T>[] = [];
    const occupied = new Map<number, CppObjectLoadAttempt<T>>();
    const loaded = new Map<number, T>();
    for (const entry of entries) {
        if (occupied.has(entry.id)) continue;
        recordCppObjectLoadOutcome(entry, await load(entry), attempts, occupied, loaded);
    }
    return { attempts, occupied, loaded };
}
