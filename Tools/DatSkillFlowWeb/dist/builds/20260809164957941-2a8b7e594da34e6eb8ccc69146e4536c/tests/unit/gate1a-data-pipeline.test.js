// dat-skill-flow-build:20260809164957941-2a8b7e594da34e6eb8ccc69146e4536c
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
    createSetIntegerPairCommand,
    createSetScalarCommand,
} from "../../src/model/dat-document.js";
import {
    DataTxtDocument,
    simulateCppObjectLoads,
} from "../../src/project/data-txt.js";
import {
    findSpriteRange,
    parseBmpMetadata,
    resolveSpriteFrame,
} from "../../src/assets/bmp.js";
import { gate1DataAuthorityLedger } from "../../src/authority/gate1-data-ledger.js";
import {
    GATE1A_DAT_PREFIX_LENGTH,
    cppDuplicateLoadFixture,
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

    it("projects the DAT frame header label without changing source bytes", () => {
        const source = Buffer.from([
            "<frame> 0 standing # comment\n",
            "pic: 0 state: 0 wait: 1 next: 0\n",
            "<frame_end>\n",
        ].join(""), "latin1");
        const document = LosslessDatDocument.fromPlaintext(source);

        assert.equal(document.cst.frames[0]?.label, "standing");
        assert.equal(document.projection.frames[0]?.label, "standing");
        assert.deepEqual(document.emitPlaintext(), source);
    });

    it("projects C++ defaults, duplicate-last frames, block fields, aliases, and ignores unknown bdy zwidth", () => {
        const document = LosslessDatDocument.fromPlaintext(syntheticDatPlaintext());
        const frame = document.projection.getFrame(7);

        assert.equal(document.projection.top.walking_speed, 4.5);
        assert.equal(document.projection.top.running_speed, 8.0, "CharData top-level default");
        assert.equal(frame?.pic, 2, "last duplicate frame index wins");
        assert.equal(frame?.wait, 1, "FrameData default");
        assert.equal(frame?.itrs[0]?.zwidth, 15, "ItrData default");
        assert.equal(frame?.bdys[0]?.h, 230, "parse_bdy only recognizes x/y/w/h");
        assert.equal(Object.hasOwn(frame?.bdys[0] ?? {}, "zwidth"), false, "unknown bdy fields are not authority projection fields");
        assert.equal(frame?.opoints[0]?.oid, 200);
        assert.equal(frame?.wpoints[0]?.weaponact, 10);
        assert.equal(frame?.bpoints[0]?.x, 11);
        assert.equal(frame?.cpoints[0]?.fronthurtact, 70);
        assert.equal(frame?.cpoints[0]?.injury, 70);
        assert.equal(frame?.cpoints[0]?.backhurtact, 71);
        assert.equal(frame?.cpoints[0]?.cover, 71);
        assert.deepEqual(document.emitPlaintext(), syntheticDatPlaintext());
    });

    it("preserves legacy duplicate selection while allowing an exact frame occurrence", () => {
        const document = LosslessDatDocument.fromPlaintext(Buffer.from([
            "<frame> 7 first\n",
            "pic: 1 pic: 2\n",
            "<frame_end>\n",
            "<frame> 7 last\n",
            "pic: 3 pic: 4\n",
            "<frame_end>\n",
        ].join(""), "ascii"));

        assert.equal(document.findFrameField(7, "pic", "first")?.rawValue.toString("ascii"), "1");
        assert.equal(document.findFrameField(7, "pic", "last")?.rawValue.toString("ascii"), "4");
        assert.equal(document.findFrameField(7, "pic", 0, "last")?.rawValue.toString("ascii"), "2");
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

    it("removes numeric and string span patches when an edit returns to the original value", () => {
        const original = syntheticDatPlaintext();
        const document = LosslessDatDocument.fromPlaintext(original);
        const picField = document.findFrameField(7, "pic");
        const nameField = document.findTopField("name");
        assert.ok(picField);
        assert.ok(nameField);

        assert.equal(document.apply(createSetScalarCommand("pic-b", picField, 42)).applied, true);
        assert.equal(document.apply(createSetScalarCommand("name-b", nameField, "Edited Hero")).applied, true);
        assert.notDeepEqual(document.emitPlaintext(), original);

        assert.equal(document.apply(createSetScalarCommand("pic-a", picField, picField.numericValue )).applied, true);
        assert.equal(document.apply(createSetScalarCommand("name-a", nameField, nameField.rawValue.toString("utf8"))).applied, true);
        assert.deepEqual(document.emitPlaintext(), original);
        assert.deepEqual(document.emitPlaintextResult().changes, []);
    });

    it("edits an ITR integer pair as one lossless value-span patch", () => {
        const original = Buffer.from([
            "<frame> 7 pair\n",
            "itr:\n",
            " catchingact:\t+005  -006\t# keep comment\n",
            "itr_end:\n",
            "<frame_end>\n",
        ].join(""), "ascii");
        const document = LosslessDatDocument.fromPlaintext(original);
        const pair = document.findNestedField(7, "itr", 0, "catchingact", 0);
        assert.ok(pair);
        assert.deepEqual(pair.integerPairValue, [5, -6]);

        const applied = document.apply(createSetIntegerPairCommand("set-catching-actions", pair, [70, 71]));
        assert.equal(applied.applied, true);
        assert.deepEqual(document.emitPlaintextResult().changes, [{
            label: "set-catching-actions",
            originalSpan: pair.valueSpan,
            outputSpan: { start: pair.valueSpan.start, end: pair.valueSpan.start + 5 },
            originalLength: pair.valueSpan.end - pair.valueSpan.start,
            replacementLength: 5,
        }]);
        assert.equal(
            document.emitPlaintext().toString("ascii"),
            original.toString("ascii").replace("+005  -006", "70 71"),
        );

        assert.equal(document.apply(createSetIntegerPairCommand("restore-catching-actions", pair, [5, -6])).applied, true);
        assert.deepEqual(document.emitPlaintext(), original);
        assert.deepEqual(document.emitPlaintextResult().changes, []);
    });

    it("edits DAT strings as lossless Latin-1 bytes and rejects non-Latin-1 text", () => {
        const original = syntheticDatPlaintext();
        const document = LosslessDatDocument.fromPlaintext(original);
        const nameField = document.findTopField("name");
        assert.ok(nameField);

        const latin1 = document.apply(createSetScalarCommand("latin1-name", nameField, "é"));
        assert.equal(latin1.applied, true);
        const edited = document.emitPlaintext();
        assert.deepEqual(
            edited.subarray(nameField.valueSpan.start, nameField.valueSpan.start + 1),
            Buffer.from([0xe9]),
        );
        assert.equal(LosslessDatDocument.fromPlaintext(edited).projection.top.name, "é");

        const beforeRejectedEdit = document.emitPlaintext();
        const rejected = document.apply(createSetScalarCommand("non-latin1-name", nameField, "英雄"));
        assert.equal(rejected.applied, false);
        assert.ok(rejected.diagnostics.some((diagnostic) => diagnostic.code === "unsupported-edit"));
        assert.deepEqual(document.emitPlaintext(), beforeRejectedEdit);

        const highByteField = parseDatCst(Buffer.from([...
            Buffer.from("name: ", "ascii"), 0xe9, 0x0a,
        ])).topFields[0];
        const nulField = parseDatCst(Buffer.from([...
            Buffer.from("name: ", "ascii"), 0x00, 0x0a,
        ])).topFields[0];
        assert.equal(highByteField?.scalarKind, "string");
        assert.equal(highByteField?.rawValue.toString("latin1"), "é");
        assert.equal(nulField?.scalarKind, "opaque");
    });

    it("classifies authority strings in CST context without exposing a scalar-kind override", () => {
        const source = Buffer.from([
            "name: 123\n",
            "file(0-0): 456 w: 3 h: 2 row: 1 col: 1\n",
            "<frame> 0 test\n",
            "pic: 1 sound: 789\n",
            "<frame_end>\n",
        ].join(""), "ascii");
        const document = LosslessDatDocument.fromPlaintext(source);
        const name = document.findTopField("name");
        const file = document.findSpriteRangeField(0, "file");
        const sound = document.findFrameField(0, "sound");
        const pic = document.findFrameField(0, "pic");
        assert.ok(name);
        assert.ok(file);
        assert.ok(sound);
        assert.ok(pic);
        assert.equal(name.scalarKind, "string");
        assert.equal(file.scalarKind, "string");
        assert.equal(sound.scalarKind, "string");
        assert.equal(pic.scalarKind, "number");

        const forgedFactory = createSetScalarCommand                
                         
                              
                                   
                               
                                                      ;
        const forged = forgedFactory("forged-pic", pic, "not-a-number", "string");
        assert.equal(Object.hasOwn(forged, "valueKind"), false);
        assert.equal(document.apply(forged).applied, false);
        assert.equal(document.apply(createSetScalarCommand("plain-pic-string", pic, "still-not-a-number")).applied, false);

        assert.equal(document.apply(createSetScalarCommand("numeric-name", name, "321")).applied, true);
        assert.equal(document.apply(createSetScalarCommand("numeric-file", file, "654")).applied, true);
        assert.equal(document.apply(createSetScalarCommand("numeric-sound", sound, "987")).applied, true);
        const reparsed = LosslessDatDocument.fromPlaintext(document.emitPlaintext());
        assert.equal(reparsed.projection.top.name, "321");
        assert.equal(reparsed.projection.spriteRanges[0]?.file, "654");
        assert.equal(reparsed.projection.frames[0]?.sound, "987");

        const nulDocument = LosslessDatDocument.fromPlaintext(Buffer.from([...
            Buffer.from("name: ", "ascii"), 0x00, 0x0a,
        ]));
        const nulName = nulDocument.findTopField("name");
        assert.ok(nulName);
        assert.equal(nulName.scalarKind, "opaque");
        assert.equal(nulDocument.apply(createSetScalarCommand("nul-name", nulName, "safe")).applied, false);

        const other = LosslessDatDocument.fromPlaintext(Buffer.from("pic: 2\n", "ascii"));
        const foreignPic = LosslessDatDocument.fromPlaintext(Buffer.from("<frame> 0 x\npic: 2\n<frame_end>\n", "ascii"))
            .findFrameField(0, "pic");
        assert.ok(foreignPic);
        assert.equal(other.apply(createSetScalarCommand("foreign", foreignPic, 3)).applied, false);

        const highByteSprite = LosslessDatDocument.fromPlaintext(Buffer.from([...
            Buffer.from("file(0-0): ", "ascii"), 0xe9,
            ...Buffer.from(" w: 1 h: 1 row: 1 col: 1\n", "ascii"),
        ]));
        const highByteFile = highByteSprite.findSpriteRangeField(0, "file");
        assert.ok(highByteFile);
        assert.equal(highByteFile.scalarKind, "string");
        assert.equal(highByteFile.rawValue.toString("latin1"), "é");
        assert.equal(highByteSprite.apply(createSetScalarCommand("high-byte-file", highByteFile, "ÿ")).applied, true);
        assert.equal(LosslessDatDocument.fromPlaintext(highByteSprite.emitPlaintext()).projection.spriteRanges[0]?.file, "ÿ");

        const nulSpriteBytes = Buffer.from([...
            Buffer.from("file(0-0): bad", "ascii"), 0x00,
            ...Buffer.from("name w: 1 h: 1 row: 1 col: 1\n", "ascii"),
        ]);
        const nulSprite = LosslessDatDocument.fromPlaintext(nulSpriteBytes);
        const nulFile = nulSprite.findSpriteRangeField(0, "file");
        assert.ok(nulFile);
        assert.equal(nulFile.scalarKind, "opaque");
        assert.equal(nulSprite.apply(createSetScalarCommand("nul-file", nulFile, "safe")).applied, false);
        assert.deepEqual(nulSprite.emitPlaintext(), nulSpriteBytes);
    });

    it("keeps out-of-range frames in the CST bytes but excludes and diagnoses them in authority projection", () => {
        const source = Buffer.from([
            ...Buffer.from("<frame> -1 below\npic: 1\n<frame_end>\n", "ascii"),
            ...Buffer.from("<frame> 599 valid\npic: 2\n<frame_end>\n", "ascii"),
            ...Buffer.from("<frame> 600 upper\npic: 3\n<frame_end>\n", "ascii"),
            0xff,
        ]);
        const document = LosslessDatDocument.fromPlaintext(source);

        assert.deepEqual(document.cst.frames.map((frame) => frame.frameId), [-1, 599, 600]);
        assert.deepEqual(document.projection.frames.map((frame) => frame.frameId), [599]);
        assert.equal(document.projection.getFrame(-1), undefined);
        assert.equal(document.projection.getFrame(600), undefined);
        assert.equal(document.diagnostics.filter((diagnostic) => diagnostic.code === "malformed-frame").length, 2);
        assert.deepEqual(document.emitPlaintext(), source);
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

    it("matches loading.cpp duplicate occupancy across decrypt and parse failure boundaries", () => {
        const simulation = simulateCppObjectLoads(cppDuplicateLoadFixture, (entry) => entry.outcome);

        assert.deepEqual(simulation.attempts.map((attempt) => attempt.entry.file), [
            "chars\\missing.dat",
            "chars\\malformed.dat",
            "chars\\missing.dat",
            "chars\\working.dat",
        ]);
        assert.equal(simulation.occupied.get(10)?.entry.file, "chars\\malformed.dat");
        assert.equal(simulation.occupied.get(10)?.outcome, "parse-failed");
        assert.equal(simulation.loaded.has(10), false, "alloc_char occupies OID 10 before parse fails");
        assert.equal(simulation.loaded.get(11)?.file, "chars\\working.dat");
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

    it("removes numeric and string data.txt span patches when edits return to original values", () => {
        const source = syntheticDataTxt();
        const document = DataTxtDocument.parse(source);
        const typeField = document.findEntryField("object", 10, "type", 1);
        const fileField = document.findEntryField("object", 10, "file", 1);
        assert.ok(typeField);
        assert.ok(fileField);

        assert.deepEqual(document.setScalar("type-b", typeField, 4), []);
        assert.deepEqual(document.setScalar("file-b", fileField, "chars\\alternate.dat"), []);
        assert.notDeepEqual(document.emit(), source);

        assert.deepEqual(document.setScalar("type-a", typeField, typeField.numericValue ), []);
        assert.deepEqual(document.setScalar("file-a", fileField, fileField.rawValue.toString("utf8")), []);
        assert.deepEqual(document.emit(), source);
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
        const ranges = [{ frameLo: 20, frameHi: 29, file: "sprite.bmp", w: 3, h: 2, row: 3, col: 2 }];
        assert.equal(findSpriteRange(ranges, 22), ranges[0]);
        assert.equal(findSpriteRange(ranges, 30), undefined);
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
        assert.deepEqual(resolveSpriteFrame(29, ranges), {
            render: true,
            file: "sprite.bmp",
            rangeIndex: 0,
            localPicture: 9,
            column: 0,
            row: 3,
            source: { x: 0, y: 9, width: 3, height: 2 },
            colorKey: { enabled: true, red: 0, green: 0, blue: 0 },
        }, "declared range selection feeds local pic directly to C++ SpriteSheet::src_rect without a grid-capacity gate");
        assert.deepEqual(resolveSpriteFrame(999, ranges), { render: false, reason: "pic-999" });
    });

    it("uses padded BMP row stride for truncation and rejects truncated BI_BITFIELDS pixels", () => {
        const padded24 = syntheticBmp(24, 1, 2);
        assert.equal(parseBmpMetadata(padded24).rowStride, 4);
        assert.equal(parseBmpMetadata(padded24.subarray(0, 54 + 6)).ok, false, "two RGB rows require 8 padded bytes, not 6 packed bytes");

        const bitfields32 = syntheticBmp(32, 2, 2);
        bitfields32.writeUInt32LE(3, 30);
        assert.equal(parseBmpMetadata(bitfields32).ok, true);
        assert.equal(parseBmpMetadata(bitfields32.subarray(0, bitfields32.length - 1)).ok, false);
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
            "dat.frame.authority-range",
            "data.object-allocation-occupies-duplicate",
            "sprite.grid-row-is-columns",
        ]) {
            assert.ok(ids.has(id), id);
        }
        assert.ok(gate1DataAuthorityLedger.every((entry) => entry.source.includes("ntsd_cpp")));
        assert.equal(ids.has("dat.bdy-zwidth-height-compatibility"), false);
    });
});
