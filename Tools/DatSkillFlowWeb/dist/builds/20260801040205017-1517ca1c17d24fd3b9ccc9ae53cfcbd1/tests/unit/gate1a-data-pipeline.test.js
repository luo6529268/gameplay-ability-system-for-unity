// dat-skill-flow-build:20260801040205017-1517ca1c17d24fd3b9ccc9ae53cfcbd1
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    DAT_ENVELOPE_PREFIX_LENGTH,
    DAT_ENCRYPTION_KEY,
    DatEnvelopeDocument,
    decryptDatPayload,
    encryptDatPayload,
} from "../../src/syntax/dat-envelope.js";
import { lexBytes, parseDatCst } from "../../src/syntax/byte-cst.js";
import { emitSpanPatches } from "../../src/syntax/patch-emitter.js";
import {
    LosslessDatDocument,
    createSetScalarCommand,
} from "../../src/model/dat-document.js";
import {
    DataTxtDocument,
    selectFirstSuccessfulDuplicates,
} from "../../src/project/data-txt.js";
import {
    findSpriteRange,
    parseBmpMetadata,
    resolveSpriteFrame,
} from "../../src/assets/bmp.js";
import { gate1DataAuthorityLedger } from "../../src/authority/gate1-data-ledger.js";
import {
    GATE1A_DAT_PREFIX_LENGTH,
    independentlyEncryptDat,
    syntheticBmp,
    syntheticDataTxt,
    syntheticDatPlaintext,
    syntheticPrefix,
} from "./gate1a-fixtures.js";

describe("Gate1A DAT envelope", () => {
    it("uses the exact authority constants and absolute-offset key position", () => {
        assert.equal(DAT_ENVELOPE_PREFIX_LENGTH, 123);
        assert.equal(DAT_ENVELOPE_PREFIX_LENGTH, GATE1A_DAT_PREFIX_LENGTH);
        assert.equal(DAT_ENCRYPTION_KEY, "SiuHungIsAGoodBearBecauseHeIsVeryGood");

        const plaintext = Buffer.from([0x00, 0x01, 0x7f, 0x80, 0xff]);
        const prefix = syntheticPrefix();
        const expected = independentlyEncryptDat(plaintext, prefix);
        const encrypted = encryptDatPayload(prefix, plaintext);

        assert.deepEqual(encrypted, expected);
        assert.deepEqual(decryptDatPayload(encrypted).plaintext, plaintext);
        const firstKeyIndex = DAT_ENVELOPE_PREFIX_LENGTH % Buffer.byteLength(DAT_ENCRYPTION_KEY, "ascii");
        assert.equal(
            encrypted[DAT_ENVELOPE_PREFIX_LENGTH],
            ((plaintext[0] ?? 0) + Buffer.from(DAT_ENCRYPTION_KEY, "ascii")[firstKeyIndex] ) & 0xff,
        );
    });

    it("diagnoses every file of length <=123 without inventing plaintext", () => {
        for (const size of [0, 1, 122, 123]) {
            const result = decryptDatPayload(Buffer.alloc(size, 0xa5));
            assert.equal(result.plaintext.length, 0);
            assert.ok(result.diagnostics.some((diagnostic) => diagnostic.code === "dat-envelope-too-short"));
        }
    });

    it("emits original encrypted bytes exactly on no-op and preserves prefix after an edit", () => {
        const plaintext = syntheticDatPlaintext();
        const original = independentlyEncryptDat(plaintext);
        const document = DatEnvelopeDocument.open(original);

        assert.deepEqual(document.emit(), original);
        const changedPlaintext = Buffer.concat([plaintext, Buffer.from("# changed\n", "ascii")]);
        const changed = document.emit(changedPlaintext);
        assert.deepEqual(changed.subarray(0, 123), original.subarray(0, 123));
        assert.deepEqual(decryptDatPayload(changed).plaintext, changedPlaintext);
    });
});

describe("Gate1A byte CST and typed DAT projection", () => {
    it("covers every source byte with lossless tokens including invalid UTF-8, NUL, and mixed line endings", () => {
        const plaintext = syntheticDatPlaintext();
        const tokens = lexBytes(plaintext);
        const reconstructed = Buffer.concat(tokens.map((token) => plaintext.subarray(token.span.start, token.span.end)));

        assert.deepEqual(reconstructed, plaintext);
        assert.ok(tokens.some((token) => token.kind === "line-break" && token.span.end - token.span.start === 2));
        assert.ok(tokens.some((token) => token.kind === "opaque"));
        assert.deepEqual(parseDatCst(plaintext).emit(), plaintext);
    });

    it("projects C++ defaults, duplicate-last frames, block fields, aliases, and the bdy compatibility quirk", () => {
        const document = LosslessDatDocument.fromPlaintext(syntheticDatPlaintext());
        const frame = document.projection.getFrame(7);

        assert.equal(document.projection.top.walking_speed, 4.5);
        assert.equal(document.projection.top.running_speed, 8.0, "CharData top-level default");
        assert.equal(frame?.pic, 2, "last duplicate frame index wins");
        assert.equal(frame?.wait, 1, "FrameData default");
        assert.equal(frame?.itrs[0]?.zwidth, 15, "ItrData default");
        assert.equal(frame?.bdys[0]?.h, 10, "bdy zwidth aliases the projected h field");
        assert.equal(frame?.opoints[0]?.oid, 200);
        assert.equal(frame?.wpoints[0]?.weaponact, 10);
        assert.equal(frame?.bpoints[0]?.x, 11);
        assert.equal(frame?.cpoints[0]?.fronthurtact, 70);
        assert.equal(frame?.cpoints[0]?.injury, 70);
        assert.equal(frame?.cpoints[0]?.backhurtact, 71);
        assert.equal(frame?.cpoints[0]?.cover, 71);
        assert.deepEqual(document.emitPlaintext(), syntheticDatPlaintext());
    });

    it("replaces only a selected numeric/string value span and re-encrypts around untouched bytes", () => {
        const originalPlaintext = syntheticDatPlaintext();
        const originalEncrypted = independentlyEncryptDat(originalPlaintext);
        const document = LosslessDatDocument.fromEncrypted(originalEncrypted);
        const picField = document.findFrameField(7, "pic");
        const nameField = document.findTopField("name");
        assert.ok(picField);
        assert.ok(nameField);

        assert.equal(document.apply(createSetScalarCommand("set-frame-pic", picField, 42)).applied, true);
        assert.equal(document.apply(createSetScalarCommand("rename-object", nameField, "Edited Hero")).applied, true);
        const editedPlaintext = decryptDatPayload(document.emitFile()).plaintext;
        const expected = Buffer.from(originalPlaintext);
        const direct = emitSpanPatches(expected, [
            { span: picField.valueSpan, replacement: Buffer.from("42", "ascii"), label: "set-frame-pic" },
            { span: nameField.valueSpan, replacement: Buffer.from("Edited Hero", "utf8"), label: "rename-object" },
        ]);

        assert.deepEqual(direct.diagnostics, []);
        assert.deepEqual(editedPlaintext, direct.bytes);
        assert.match(editedPlaintext.toString("latin1"), /unknown_top: keep-me/);
        assert.ok(editedPlaintext.includes(Buffer.from([0x00, 0xff, 0xc3, 0x28])));
        assert.deepEqual(document.emitFile().subarray(0, 123), originalEncrypted.subarray(0, 123));
    });

    it("reports overlapping patches and unsupported UTF-16 without changing bytes", () => {
        const source = Buffer.from("abcdef", "ascii");
        const overlap = emitSpanPatches(source, [
            { span: { start: 1, end: 4 }, replacement: Buffer.from("x"), label: "one" },
            { span: { start: 3, end: 5 }, replacement: Buffer.from("y"), label: "two" },
        ]);
        assert.deepEqual(overlap.bytes, source);
        assert.ok(overlap.diagnostics.some((diagnostic) => diagnostic.code === "overlapping-edit"));

        const utf16 = Buffer.from([0xff, 0xfe, 0x3c, 0x00, 0x66, 0x00]);
        const cst = parseDatCst(utf16);
        assert.ok(cst.diagnostics.some((diagnostic) => diagnostic.code === "unsupported-encoding"));
        assert.deepEqual(cst.emit(), utf16);
    });
});

describe("Gate1A data.txt project catalog", () => {
    it("preserves entries/comments/order/malformed bytes and projects object/background records", () => {
        const source = syntheticDataTxt();
        const document = DataTxtDocument.parse(source);

        assert.deepEqual(document.emit(), source);
        assert.deepEqual(document.entries.map((entry) => [entry.section, entry.id]), [
            ["object", 10], ["object", 10], ["object", 10], ["object", 11], ["background", 2], ["background", 3],
        ]);
        assert.equal(document.entries[1]?.type, 3);
        assert.equal(document.entries[4]?.file, "bg\\district.dat");
        assert.ok(document.diagnostics.some((diagnostic) => diagnostic.code === "unsafe-resource-path"));
        assert.ok(source.includes(Buffer.from([0xff, 0x00])));
    });

    it("selects the first successful duplicate, allowing a later duplicate after a failed load", () => {
        const document = DataTxtDocument.parse(syntheticDataTxt());
        const selected = selectFirstSuccessfulDuplicates(
            document.entries.filter((entry) => entry.section === "object"),
            (entry) => entry.file.endsWith("working.dat") || entry.id === 11,
        );

        assert.equal(selected.get(10)?.file, "chars\\working.dat");
        assert.equal(selected.get(11)?.file, "..\\escape.dat");
    });

    it("diagnoses path syntax only and never resolves a filesystem path", () => {
        const document = DataTxtDocument.parse(syntheticDataTxt());
        const traversal = document.diagnostics.find((diagnostic) => diagnostic.path === "..\\escape.dat");
        const absolute = document.diagnostics.find((diagnostic) => diagnostic.path === "C:\\outside\\stage.dat");

        assert.equal(traversal?.reason, "traversal");
        assert.equal(absolute?.reason, "absolute");
        assert.equal(document.entries[0]?.file, "chars\\missing.dat");
    });

    it("edits one catalog scalar span and refuses a syntactically escaping replacement", () => {
        const source = syntheticDataTxt();
        const document = DataTxtDocument.parse(source);
        const typeField = document.findEntryField("object", 10, "type", 1);
        const fileField = document.findEntryField("object", 10, "file", 1);
        assert.ok(typeField);
        assert.ok(fileField);

        assert.deepEqual(document.setScalar("set-type", typeField, 4), []);
        const rejected = document.setScalar("escape", fileField, "..\\outside.dat");
        assert.equal(rejected[0]?.code, "unsafe-resource-path");
        assert.deepEqual(
            document.emit(),
            emitSpanPatches(source, [{ span: typeField.valueSpan, replacement: Buffer.from("4"), label: "set-type" }]).bytes,
        );
    });
});

describe("Gate1A BMP and sprite metadata", () => {
    for (const bitDepth of [8, 24, 32]         ) {
        it(`reads ${bitDepth}-bit BMP dimensions, offset, stride, and black colorkey metadata`, () => {
            const bytes = syntheticBmp(bitDepth);
            const metadata = parseBmpMetadata(bytes);
            assert.equal(metadata.ok, true);
            assert.equal(metadata.width, 12);
            assert.equal(metadata.height, 6);
            assert.equal(metadata.bitDepth, bitDepth);
            assert.equal(metadata.pixelOffset, bitDepth === 8 ? 1078 : 54);
            assert.deepEqual(metadata.colorKey, { enabled: true, red: 0, green: 0, blue: 0 });
        });
    }

    it("looks up inclusive ranges, treats row as columns, uses w+1/h+1 stride, and suppresses pic 999", () => {
        const ranges = [{ frameLo: 20, frameHi: 25, file: "sprite.bmp", w: 3, h: 2, row: 3, col: 2 }];
        assert.equal(findSpriteRange(ranges, 22), ranges[0]);
        assert.equal(findSpriteRange(ranges, 26), undefined);
        assert.deepEqual(resolveSpriteFrame(24, ranges), {
            render: true,
            file: "sprite.bmp",
            rangeIndex: 0,
            localPicture: 4,
            column: 1,
            row: 1,
            source: { x: 4, y: 3, width: 3, height: 2 },
            colorKey: { enabled: true, red: 0, green: 0, blue: 0 },
        });
        assert.deepEqual(resolveSpriteFrame(999, ranges), { render: false, reason: "pic-999" });
    });

    it("diagnoses unsupported or truncated BMP data without throwing", () => {
        const unsupported = syntheticBmp(24);
        unsupported.writeUInt16LE(16, 28);
        assert.equal(parseBmpMetadata(unsupported).diagnostics[0]?.code, "invalid-bmp");
        assert.equal(parseBmpMetadata(Buffer.from("not a bmp")).ok, false);
    });
});

describe("Gate1A authority ledger", () => {
    it("names concrete C++ parser contracts covered by this gate", () => {
        const ids = new Set(gate1DataAuthorityLedger.map((entry) => entry.id));
        for (const id of [
            "dat.envelope.absolute-key-offset",
            "dat.frame.defaults-and-duplicates",
            "dat.cpoint.alias-side-effects",
            "data.object-first-successful-duplicate",
            "sprite.grid-row-is-columns",
        ]) {
            assert.ok(ids.has(id), id);
        }
        assert.ok(gate1DataAuthorityLedger.every((entry) => entry.source.includes("ntsd_cpp")));
    });
});
