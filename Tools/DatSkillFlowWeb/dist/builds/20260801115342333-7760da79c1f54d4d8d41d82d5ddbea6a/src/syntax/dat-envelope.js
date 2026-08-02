// dat-skill-flow-build:20260801115342333-7760da79c1f54d4d8d41d82d5ddbea6a
import { dataDiagnostic,                     } from "./data-diagnostic.js";

export const DAT_ENCRYPTION_KEY = "SiuHungIsAGoodBearBecauseHeIsVeryGood";
export const DAT_ENVELOPE_PREFIX_LENGTH = 123;

const keyBytes = Buffer.from(DAT_ENCRYPTION_KEY, "ascii");

                                      
                          
                   
                      
                                  
 

export function decryptDatPayload(input            )                      {
    const originalBytes = Buffer.from(input);
    const prefix = Buffer.from(originalBytes.subarray(
        0,
        Math.min(DAT_ENVELOPE_PREFIX_LENGTH, originalBytes.length),
    ));
    if (originalBytes.length <= DAT_ENVELOPE_PREFIX_LENGTH) {
        return {
            originalBytes,
            prefix,
            plaintext: Buffer.alloc(0),
            diagnostics: [dataDiagnostic(
                "dat-envelope-too-short",
                `Encrypted DAT must be longer than ${DAT_ENVELOPE_PREFIX_LENGTH} bytes; observed ${originalBytes.length}.`,
                { span: { start: 0, end: originalBytes.length } },
            )],
        };
    }

    const plaintext = Buffer.alloc(originalBytes.length - DAT_ENVELOPE_PREFIX_LENGTH);
    for (let bodyOffset = 0; bodyOffset < plaintext.length; bodyOffset += 1) {
        const absoluteOffset = DAT_ENVELOPE_PREFIX_LENGTH + bodyOffset;
        const encryptedByte = originalBytes[absoluteOffset] ?? 0;
        const keyByte = keyBytes[absoluteOffset % keyBytes.length] ?? 0;
        plaintext[bodyOffset] = (encryptedByte - keyByte + 0x100) & 0xff;
    }
    return { originalBytes, prefix, plaintext, diagnostics: [] };
}

export function encryptDatPayload(prefixInput            , plaintext            )         {
    const prefix = Buffer.from(prefixInput);
    if (prefix.length !== DAT_ENVELOPE_PREFIX_LENGTH) {
        throw new RangeError(`DAT prefix must be exactly ${DAT_ENVELOPE_PREFIX_LENGTH} bytes.`);
    }
    const encrypted = Buffer.alloc(prefix.length + plaintext.length);
    encrypted.set(prefix, 0);
    for (let bodyOffset = 0; bodyOffset < plaintext.length; bodyOffset += 1) {
        const absoluteOffset = DAT_ENVELOPE_PREFIX_LENGTH + bodyOffset;
        const plainByte = plaintext[bodyOffset] ?? 0;
        const keyByte = keyBytes[absoluteOffset % keyBytes.length] ?? 0;
        encrypted[absoluteOffset] = (plainByte + keyByte) & 0xff;
    }
    return encrypted;
}

export class DatEnvelopeDocument {
                    originalBytes        ;
                    prefix        ;
                    plaintext        ;
                    diagnostics                           ;

            constructor(result                     ) {
        this.originalBytes = result.originalBytes;
        this.prefix = result.prefix;
        this.plaintext = result.plaintext;
        this.diagnostics = result.diagnostics;
    }

           static open(input            )                      {
        return new DatEnvelopeDocument(decryptDatPayload(input));
    }

           emit(plaintext             = this.plaintext)         {
        if (Buffer.from(plaintext).equals(this.plaintext)) {
            return Buffer.from(this.originalBytes);
        }
        if (this.prefix.length !== DAT_ENVELOPE_PREFIX_LENGTH) {
            throw new RangeError("Cannot re-encrypt a DAT whose original prefix is incomplete.");
        }
        return encryptDatPayload(this.prefix, plaintext);
    }
}
