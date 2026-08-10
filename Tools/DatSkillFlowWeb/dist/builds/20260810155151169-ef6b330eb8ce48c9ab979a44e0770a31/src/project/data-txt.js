// dat-skill-flow-build:20260810155151169-ef6b330eb8ce48c9ab979a44e0770a31
import {                  } from "../syntax/byte-cst.js";
import { dataDiagnostic,                                    } from "../syntax/data-diagnostic.js";
import { emitSpanPatches,                } from "../syntax/patch-emitter.js";

                                                     

                               
                            
               
                  
                 
                   
             
                        
                           
                          
      
 

                                                                
                                 
                 
                                                       
 

function lineSpans(source            )                                               {
    const lines                                               = [];
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
    source        ,
    key        ,
    keyStart        ,
    keyEnd        ,
    valueStart        ,
    valueEnd        ,
    numericValue         ,
)              {
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

function trimFileEnd(text        , start        )         {
    const comment = text.indexOf("#", start);
    let end = comment < 0 ? text.length : comment;
    while (end > start && (text[end - 1] === " " || text[end - 1] === "\t")) end -= 1;
    return end;
}

function parseEntry(source        , line                                       , section                )                           {
    const text = source.subarray(line.content.start, line.content.end).toString("latin1");
    const idMatch = /^\s*id\s*:\s*([+-]?\d+)/.exec(text);
    if (!idMatch) return undefined;
    const id = Number.parseInt(idMatch[1] , 10);
    if (!Number.isSafeInteger(id) || id < 0) return undefined;
    const idToken = idMatch[1] ;
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
    let type                    ;
    let typeField                         ;
    if (typeMatch) {
        type = Number.parseInt(typeMatch[1] , 10);
        const token = typeMatch[1] ;
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

export function diagnoseResourcePath(path        , span           )                                     {
    let reason                                              ;
    if (path.length === 0) reason = "empty";
    else if (path.includes("\0")) reason = "nul";
    else if (/^(?:[a-zA-Z]:[\\/]|[\\/]{2}|[\\/])/.test(path)) reason = "absolute";
    else if (path.split(/[\\/]+/).includes("..")) reason = "traversal";
    if (!reason) return undefined;
    return dataDiagnostic(
        "unsafe-resource-path",
        `Resource path is syntactically unsafe (${reason}): ${path}`,
        { path, reason, ...(span === undefined ? {} : { span }) },
    )                          ;
}

export class DataTxtDocument {
                    source        ;
                    entries                         ;
                    diagnostics                                   ;
                     patches              = [];

            constructor(source        , entries                , diagnostics                          ) {
        this.source = source;
        this.entries = entries;
        this.diagnostics = diagnostics;
    }

           static parse(input            )                  {
        const source = Buffer.from(input);
        const entries                 = [];
        const diagnostics                           = [];
        let section                            ;
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

           findEntryField(
        section                ,
        id        ,
        key                        ,
        duplicateIndex = 0,
    )                          {
        return this.entries.filter((entry) => entry.section === section && entry.id === id)[duplicateIndex]?.fields[key];
    }

           setScalar(name        , field             , value                 )                   {
        if (!this.entries.some((entry) => Object.values(entry.fields).includes(field))) {
            return [dataDiagnostic("unsupported-edit", "The selected data.txt field does not belong to this document.")];
        }
        let replacement        ;
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

           emit()         {
        return emitSpanPatches(this.source, this.patches).bytes;
    }
}

                                  
                      
                         
                    
               

                                          
             
                                  
 

                                             
                                                 
                                                           
                                   
 

function recordCppObjectLoadOutcome                                    (
    entry   ,
    outcome                      ,
    attempts                           ,
    occupied                                      ,
    loaded                ,
)       {
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
export function simulateCppObjectLoads                                    (
    entries              ,
    load                                    ,
)                             {
    const attempts                            = [];
    const occupied = new Map                                 ();
    const loaded = new Map           ();
    for (const entry of entries) {
        if (occupied.has(entry.id)) continue;
        recordCppObjectLoadOutcome(entry, load(entry), attempts, occupied, loaded);
    }
    return { attempts, occupied, loaded };
}

export async function simulateCppObjectLoadsAsync                                    (
    entries              ,
    load                                                                    ,
)                                      {
    const attempts                            = [];
    const occupied = new Map                                 ();
    const loaded = new Map           ();
    for (const entry of entries) {
        if (occupied.has(entry.id)) continue;
        recordCppObjectLoadOutcome(entry, await load(entry), attempts, occupied, loaded);
    }
    return { attempts, occupied, loaded };
}
