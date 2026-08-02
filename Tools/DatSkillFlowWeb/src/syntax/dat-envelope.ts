import { dataDiagnostic, type DataDiagnostic } from "./data-diagnostic.js";

export const DAT_ENCRYPTION_KEY = "SiuHungIsAGoodBearBecauseHeIsVeryGood";
export const DAT_ENVELOPE_PREFIX_LENGTH = 123;

const keyBytes = Buffer.from(DAT_ENCRYPTION_KEY, "ascii");

export interface DecryptedDatPayload {
    originalBytes: Buffer;
    prefix: Buffer;
    plaintext: Buffer;
    diagnostics: DataDiagnostic[];
}

export function decryptDatPayload(input: Uint8Array): DecryptedDatPayload {
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

export function encryptDatPayload(prefixInput: Uint8Array, plaintext: Uint8Array): Buffer {
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
    public readonly originalBytes: Buffer;
    public readonly prefix: Buffer;
    public readonly plaintext: Buffer;
    public readonly diagnostics: readonly DataDiagnostic[];

    private constructor(result: DecryptedDatPayload) {
        this.originalBytes = result.originalBytes;
        this.prefix = result.prefix;
        this.plaintext = result.plaintext;
        this.diagnostics = result.diagnostics;
    }

    public static open(input: Uint8Array): DatEnvelopeDocument {
        return new DatEnvelopeDocument(decryptDatPayload(input));
    }

    public emit(plaintext: Uint8Array = this.plaintext): Buffer {
        if (Buffer.from(plaintext).equals(this.plaintext)) {
            return Buffer.from(this.originalBytes);
        }
        if (this.prefix.length !== DAT_ENVELOPE_PREFIX_LENGTH) {
            throw new RangeError("Cannot re-encrypt a DAT whose original prefix is incomplete.");
        }
        return encryptDatPayload(this.prefix, plaintext);
    }
}
