// dat-skill-flow-build:20260801053910730-965ac37083284c9abd340a893782bcd2
import {
    parseDatCst,
                      
                        
                     
} from "../syntax/byte-cst.js";
import { DatEnvelopeDocument } from "../syntax/dat-envelope.js";
import { dataDiagnostic,                     } from "../syntax/data-diagnostic.js";
import {
    emitSpanPatches,
                           
                   
} from "../syntax/patch-emitter.js";
import { projectDatCst,                    } from "./dat-projection.js";

                                   
                       
                 
                       
                           
 

                                     
                     
                                  
 

                                   
                  
                                  
                                 
 

export function createSetScalarCommand(
    name        ,
    field             ,
    value                 ,
)                   {
    return { kind: "set-scalar", name, field, value };
}

function replacementForCommand(command                  )                          {
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
        if (typeof command.value !== "string" || /[\r\n\0]/.test(command.value)) {
            return dataDiagnostic("unsupported-edit", `String field ${command.field.key} requires a single NUL-free line.`);
        }
        return Buffer.from(command.value, "utf8");
    }
    return dataDiagnostic("unsupported-edit", `Opaque field ${command.field.key} cannot be edited safely.`);
}

function last   (values              )                {
    return values.length === 0 ? undefined : values[values.length - 1];
}

export class LosslessDatDocument {
                    cst                ;
                    envelope                      ;
                     patches              = [];

            constructor(cst                , envelope                      ) {
        this.cst = cst;
        this.envelope = envelope;
    }

           static fromPlaintext(plaintext            )                      {
        return new LosslessDatDocument(parseDatCst(plaintext));
    }

           static fromEncrypted(encrypted            )                      {
        const envelope = DatEnvelopeDocument.open(encrypted);
        return new LosslessDatDocument(parseDatCst(envelope.plaintext), envelope);
    }

           get projection()                {
        if (this.patches.length === 0) return projectDatCst(this.cst);
        return projectDatCst(parseDatCst(this.emitPlaintext()));
    }

           get diagnostics()                            {
        return [
            ...(this.envelope?.diagnostics ?? []),
            ...this.cst.diagnostics,
            ...this.projection.diagnostics,
            ...this.emitPlaintextResult().diagnostics,
        ];
    }

           findTopField(key        , occurrence                   = "last")                          {
        const fields = this.cst.topFields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

           findFrameField(
        frameId        ,
        key        ,
        occurrence                   = "last",
    )                          {
        const frames = this.cst.frames.filter((frame) => frame.frameId === frameId);
        const frame = occurrence === "first" ? frames[0] : last(frames);
        if (!frame) return undefined;
        const fields = frame.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

           findNestedField(
        frameId        ,
        blockType              ,
        blockIndex        ,
        key        ,
        occurrence                   = "last",
    )                          {
        const frame = last(this.cst.frames.filter((candidate) => candidate.frameId === frameId));
        const block = frame?.blocks.filter((candidate) => candidate.type === blockType)[blockIndex];
        if (!block) return undefined;
        const fields = block.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

           findSpriteRangeField(
        rangeIndex        ,
        key                                    ,
        occurrence                   = "last",
    )                          {
        const range = this.cst.spriteRanges[rangeIndex];
        if (!range) return undefined;
        if (key === "file") return range.fileField;
        const fields = range.fields.filter((field) => field.key === key);
        return occurrence === "first" ? fields[0] : last(fields);
    }

           apply(command                  )                     {
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

        const patch            = {
            span: { ...command.field.valueSpan },
            replacement,
            label: command.name,
        };
        if (sameSpanIndex >= 0) this.patches[sameSpanIndex] = patch;
        else this.patches.push(patch);
        return { applied: true, diagnostics: [] };
    }

           emitPlaintextResult()                   {
        return emitSpanPatches(this.cst.source, this.patches);
    }

           emitPlaintext()         {
        return this.emitPlaintextResult().bytes;
    }

           emitFile()         {
        const plaintext = this.emitPlaintext();
        return this.envelope ? this.envelope.emit(plaintext) : plaintext;
    }
}
