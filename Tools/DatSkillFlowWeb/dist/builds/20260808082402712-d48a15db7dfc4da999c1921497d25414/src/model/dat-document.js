// dat-skill-flow-build:20260808082402712-d48a15db7dfc4da999c1921497d25414
import {
    isSignedInt32,
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

export function createSetIntegerPairCommand(
    name        ,
    field             ,
    value                           ,
)                        {
    return { kind: "set-integer-pair", name, field, value };
}

export function isLatin1ScalarString(value         )                  {
    if (typeof value !== "string" || /[\r\n\0]/.test(value)) return false;
    for (const character of value) {
        if (character.codePointAt(0)  > 0xff) return false;
    }
    return true;
}

function replacementForCommand(command                  )                          {
    if (command.name.length === 0) {
        return dataDiagnostic("unsupported-edit", "Every edit command must have a non-empty name.");
    }
    if (command.field.integerPairValue !== undefined) {
        return dataDiagnostic("unsupported-edit", `Pair field ${command.field.key} requires a pair edit command.`);
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

function replacementForIntegerPair(command                       )                          {
    if (command.name.length === 0) {
        return dataDiagnostic("unsupported-edit", "Every edit command must have a non-empty name.");
    }
    if (command.field.blockType !== "itr"
        || (command.field.key !== "catchingact" && command.field.key !== "caughtact")
        || command.field.integerPairValue === undefined) {
        return dataDiagnostic("unsupported-edit", "The selected field is not a valid ITR integer pair.");
    }
    const [first, second] = command.value;
    if (![first, second].every(isSignedInt32)) {
        return dataDiagnostic("unsupported-edit", "An ITR integer pair requires two signed 32-bit integers.");
    }
    if (first === command.field.integerPairValue[0] && second === command.field.integerPairValue[1]) {
        return Buffer.from(command.field.rawValue);
    }
    return Buffer.from(`${first} ${second}`, "ascii");
}

function last   (values              )                {
    return values.length === 0 ? undefined : values[values.length - 1];
}

                                                    

function selectOccurrence   (values              , selector                    )                {
    if (typeof selector === "number") return values[selector];
    return selector === "first" ? values[0] : last(values);
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

           withPlaintext(plaintext            )                      {
        return new LosslessDatDocument(parseDatCst(plaintext), this.envelope);
    }

           findTopField(key        , occurrence                     = "last")                          {
        const fields = this.cst.topFields.filter((field) => field.key === key);
        return selectOccurrence(fields, occurrence);
    }

           findFrameField(
        frameId        ,
        key        ,
        frameOccurrenceOrFieldOccurrence                            = "last",
        fieldOccurrence                     = "last",
    )                          {
        const frames = this.cst.frames.filter((frame) => frame.frameId === frameId);
        const frame = typeof frameOccurrenceOrFieldOccurrence === "number"
            ? frames.find((candidate) => candidate.occurrence === frameOccurrenceOrFieldOccurrence)
            : selectOccurrence(frames, frameOccurrenceOrFieldOccurrence);
        if (!frame) return undefined;
        const fields = frame.fields.filter((field) => field.key === key);
        const occurrence = typeof frameOccurrenceOrFieldOccurrence === "number"
            ? fieldOccurrence
            : frameOccurrenceOrFieldOccurrence;
        return selectOccurrence(fields, occurrence);
    }

           findNestedField(
        frameId        ,
        blockType              ,
        blockIndex        ,
        key        ,
        frameOccurrenceOrFieldOccurrence                            = "last",
        fieldOccurrence                     = "last",
    )                          {
        const frame = typeof frameOccurrenceOrFieldOccurrence === "number"
            ? this.cst.frames.find((candidate) => (
                candidate.frameId === frameId && candidate.occurrence === frameOccurrenceOrFieldOccurrence
            ))
            : last(this.cst.frames.filter((candidate) => candidate.frameId === frameId));
        const block = frame?.blocks.filter((candidate) => candidate.type === blockType)[blockIndex];
        if (!block) return undefined;
        const fields = block.fields.filter((field) => field.key === key);
        const occurrence = typeof frameOccurrenceOrFieldOccurrence === "number"
            ? fieldOccurrence
            : frameOccurrenceOrFieldOccurrence;
        return selectOccurrence(fields, occurrence);
    }

           findSpriteRangeField(
        rangeIndex        ,
        key                                    ,
        occurrence                     = "last",
    )                          {
        const range = this.cst.spriteRanges[rangeIndex];
        if (!range) return undefined;
        if (key === "file") return range.fileField;
        const fields = range.fields.filter((field) => field.key === key);
        return selectOccurrence(fields, occurrence);
    }

           createPatchSavepoint()                       {
        return this.patches.map((patch) => ({
            span: { ...patch.span },
            replacement: Buffer.from(patch.replacement),
            label: patch.label,
        }));
    }

           restorePatchSavepoint(savepoint                      )       {
        this.patches.length = 0;
        this.patches.push(...savepoint.map((patch) => ({
            span: { ...patch.span },
            replacement: Buffer.from(patch.replacement),
            label: patch.label,
        })));
    }

           apply(command                                          )                     {
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
        const replacement = command.kind === "set-scalar"
            ? replacementForCommand(command)
            : replacementForIntegerPair(command);
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
