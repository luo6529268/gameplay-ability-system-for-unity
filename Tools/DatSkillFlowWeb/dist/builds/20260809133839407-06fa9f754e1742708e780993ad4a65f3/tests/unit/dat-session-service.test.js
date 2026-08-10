// dat-skill-flow-build:20260809133839407-06fa9f754e1742708e780993ad4a65f3
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { resolve } from "node:path";
import { describe, it } from "node:test";

import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";
import {
    DEFAULT_DAT_SESSION_LIMITS,
    DatSessionError,
    DatSessionService,
                                  
                        
} from "../../src/server/dat-session-service.js";
             
                      
                         
                         
                                                       
import { WorkspaceRegistry } from "../../src/server/workspace-registry.js";

const canonicalRoot = resolve("dat-session-test-root");
const canonicalDocument = resolve(canonicalRoot, "fighter.dat");

function fingerprint(bytes            , generation        ) {
    return {
        sha256: createHash("sha256").update(bytes).digest("hex"),
        size: bytes.length,
        modifiedNanoseconds: String(generation),
        changedNanoseconds: String(generation),
        device: "test-device",
        inode: "test-inode",
    };
}

class FakeNativeClient                                 {
    bytes        ;
    canonicalPath = canonicalDocument;
    failRead = false;
    generation = 1;

    constructor(bytes            ) {
        this.bytes = Buffer.from(bytes);
    }

    setBytes(bytes            )       {
        this.bytes = Buffer.from(bytes);
        this.generation += 1;
    }

    async inspectRoot()                                {
        return { canonicalPath: canonicalRoot, volumeSerial: "test-volume", fileId: "test-root" };
    }

    async ensureDirectory()                                     {
        return { canonicalPath: canonicalRoot };
    }

    async read(_request                   ) {
        if (this.failRead) throw new Error("injected read failure with secret path");
        return {
            canonicalPath: this.canonicalPath,
            bytes: Buffer.from(this.bytes),
            fingerprint: fingerprint(this.bytes, this.generation),
        };
    }

    async saveAs()                 { throw new Error("save is deferred"); }
    async overwrite()                 { throw new Error("save is deferred"); }
}

function datSource(name = "Second Hero", pic = 2)         {
    return Buffer.from([
        "# plaintext remains longer than the encrypted prefix so detection cannot rely on length alone\n",
        "name: First Hero\n",
        `name: ${name}\n`,
        "walking_speed: 4.5\n",
        "<frame> 7 editable\n",
        `pic: 1 pic: ${pic} state: 3 wait: 1 next: 8 sound: data\\001.wav\n`,
        "itr:\n kind: 0 x: 1 y: 2 w: 3 h: 4 catchingact: 5 6\n itr_end:\n",
        "cpoint:\n kind: 1 injury: 2 fronthurtact: 70\n cpoint_end:\n",
        "<frame_end>\n",
    ].join(""), "ascii");
}

function duplicateLocatorSource()         {
    return Buffer.from([
        "name: Locator Hero\n",
        "<frame> 7 first\n",
        "pic: 1 pic: 2\n",
        "itr:\n",
        " catchingact: 1 2 catchingact: 3 4\n",
        "itr_end:\n",
        "itr:\n",
        " caughtact: 5 6\n",
        "itr_end:\n",
        "<frame_end>\n",
        "<frame> 7 second\n",
        "pic: 8 pic: 9\n",
        "itr:\n",
        " catchingact: 10 11\n",
        "itr_end:\n",
        "<frame_end>\n",
    ].join(""), "ascii");
}

function idFactory()               {
    let next = 0;
    return () => `opaque-${String(++next).padStart(6, "0")}-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`;
}

async function fixture(bytes            , options                           = {}) {
    const client = new FakeNativeClient(bytes);
    const registry = new WorkspaceRegistry({ allowAbsoluteRootGrant: true, nativeClient: client });
    const { rootId } = await registry.grantAbsoluteRoot(canonicalRoot);
    const { documentId } = await registry.openDocument(rootId, "fighter.dat");
    const service = new DatSessionService(registry, { idFactory: idFactory(), ...options });
    return { client, registry, documentId, service };
}

async function rejectsCode(promise                  , code                         )                {
    await assert.rejects(promise, (error         ) => error instanceof DatSessionError && error.code === code);
}

function assertNoPrivateProjectionData(view                , documentId        )       {
    const forbidden = new Set(["cst", "span", "rawValue", "canonicalPath", "documentId", "rootId", "path"]);
    const visit = (value         )       => {
        if (Array.isArray(value)) {
            for (const child of value) visit(child);
            return;
        }
        if (typeof value !== "object" || value === null) return;
        for (const [key, child] of Object.entries(value)) {
            assert.equal(forbidden.has(key), false, `forbidden view key ${key}`);
            visit(child);
        }
    };
    visit(view);
    assert.equal(JSON.stringify(view).includes(documentId), false);
    assert.equal(JSON.stringify(view).includes(canonicalRoot), false);
}

describe("server-owned DAT session core", () => {
    it("publishes the fixed default resource limits", () => {
        assert.deepEqual(DEFAULT_DAT_SESSION_LIMITS, {
            maxSessions: 32,
            maxFieldsPerSession: 50_000,
            maxLoadedBytes: 64 * 1024 * 1024,
            idleTtlMs: 15 * 60 * 1_000,
            maxDiagnostics: 200,
            maxDiagnosticMessageLength: 512,
            maxProjectionBytes: 2 * 1024 * 1024,
            maxViewBytes: 8 * 1024 * 1024,
            maxStringBytes: 4 * 1024,
        });
    });

    it("requires an explicit DAT input format and emits opaque, duplicate-specific field capabilities", async () => {
        const plaintext = datSource();
        const encrypted = encryptDatPayload(Buffer.alloc(123, 0x41), plaintext);
        const plainFixture = await fixture(plaintext);
        const encryptedFixture = await fixture(encrypted);

        const plain = await plainFixture.service.openDocument(plainFixture.documentId, "plaintext");
        const cipher = await encryptedFixture.service.openDocument(encryptedFixture.documentId, "encrypted");
        assert.equal(plain.format, "plaintext");
        assert.equal(cipher.format, "encrypted");
        assert.equal(plain.projection.top.name, "Second Hero");
        assert.equal(cipher.projection.top.name, "Second Hero");
        const duplicateNames = plain.fields.filter((field) => field.scope === "top" && field.key === "name");
        assert.equal(duplicateNames.length, 2);
        assert.notEqual(duplicateNames[0]?.fieldId, duplicateNames[1]?.fieldId);
        assert.match(plain.sessionId, /^[A-Za-z0-9_-]{32,}$/);
        for (const field of plain.fields) assert.match(field.fieldId, /^[A-Za-z0-9_-]{32,}$/);
        assertNoPrivateProjectionData(plain, plainFixture.documentId);
        assertNoPrivateProjectionData(cipher, encryptedFixture.documentId);
        assert.ok(plain.diagnostics.every((diagnostic) => !Object.hasOwn(diagnostic, "span") && !Object.hasOwn(diagnostic, "path")));
    });

    it("fails closed when the trusted format selection is missing or contains no editable authority fields", async () => {
        const plainFixture = await fixture(datSource());
        const shortFixture = await fixture(Buffer.alloc(123, 0xa5));
        const zeroFixture = await fixture(Buffer.alloc(0));
        const craftedFixture = await fixture(Buffer.alloc(124, 0));

        await rejectsCode(plainFixture.service.openDocument(plainFixture.documentId, undefined         ), "invalid-request");
        await rejectsCode(shortFixture.service.openDocument(shortFixture.documentId, "encrypted"), "invalid-request");
        await rejectsCode(zeroFixture.service.openDocument(zeroFixture.documentId, "plaintext"), "invalid-request");
        await rejectsCode(craftedFixture.service.openDocument(craftedFixture.documentId, "plaintext"), "invalid-request");
        await rejectsCode(craftedFixture.service.openDocument(craftedFixture.documentId, "encrypted"), "invalid-request");
    });

    it("deterministically follows trusted format metadata even when an encrypted prefix is valid plaintext DAT", async () => {
        const prefix = Buffer.alloc(123, 0x23);
        Buffer.from("name: Trojan\n", "ascii").copy(prefix);
        prefix[122] = 0x0a;
        const selectedBytes = encryptDatPayload(prefix, datSource("Real Body"));
        const selected = await fixture(selectedBytes);

        const plaintextView = await selected.service.openDocument(selected.documentId, "plaintext");
        const encryptedView = await selected.service.openDocument(selected.documentId, "encrypted");
        assert.equal(plaintextView.format, "plaintext");
        assert.equal(plaintextView.projection.top.name, "Trojan");
        assert.equal(encryptedView.format, "encrypted");
        assert.equal(encryptedView.projection.top.name, "Real Body");
    });

    it("uses strict edit schemas and validates finite, NUL-free, single-line bounded scalar values", async () => {
        const { service, documentId } = await fixture(datSource());
        const view = await service.openDocument(documentId, "plaintext");
        const numberField = view.fields.find((field) => field.key === "pic") ;
        const stringField = view.fields.find((field) => field.key === "name") ;

        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: numberField.fieldId, value: 3, expectedRevision: 0, extra: true }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: numberField.fieldId, value: Number.POSITIVE_INFINITY, expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "bad\0value", expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "bad\nvalue", expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "x".repeat(4097), expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: "x".repeat(129), fieldId: numberField.fieldId, value: 3, expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: "x".repeat(129), value: 3, expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: "missing", fieldId: numberField.fieldId, value: 3, expectedRevision: 0 }), "unknown-session");

        const oversizedIds = await fixture(datSource(), { idFactory: () => "x".repeat(129) });
        await rejectsCode(oversizedIds.service.openDocument(oversizedIds.documentId, "plaintext"), "invalid-request");
        const boundaryIds = await fixture(datSource(), { idFactory: () => "x".repeat(128) });
        const boundaryView = await boundaryIds.service.openDocument(boundaryIds.documentId, "plaintext");
        assert.ok(boundaryView.sessionId.length <= 128);
        assert.ok(boundaryView.fields.every((field) => field.fieldId.length <= 128));
    });

    it("keeps Latin-1 string bytes, field values, and projection values in one contract", async () => {
        const { service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const nameField = initial.fields.find((field) => field.key === "name" && field.value === "Second Hero") ;

        const edited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: nameField.fieldId,
            value: "é",
            expectedRevision: 0,
        });
        assert.equal(edited.revision, 1);
        assert.equal(edited.fields.find((field) => field.fieldId === nameField.fieldId)?.value, "é");
        assert.equal(edited.projection.top.name, "é");

        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: nameField.fieldId,
            value: "英雄",
            expectedRevision: 1,
        }), "invalid-request");
        const unchanged = await service.edit({
            sessionId: initial.sessionId,
            fieldId: nameField.fieldId,
            value: "é",
            expectedRevision: 1,
        });
        assert.equal(unchanged.revision, 1);
        assert.equal(unchanged.projection.top.name, "é");

        const oneByte = await fixture(Buffer.from("name: éééé\n", "latin1"), { maxStringBytes: 4 });
        const oneByteInitial = await oneByte.service.openDocument(oneByte.documentId, "plaintext");
        const oneByteName = oneByteInitial.fields.find((field) => field.key === "name") ;
        const oneByteEdited = await oneByte.service.edit({
            sessionId: oneByteInitial.sessionId,
            fieldId: oneByteName.fieldId,
            value: "ÿÿÿÿ",
            expectedRevision: 0,
        });
        assert.equal(oneByteEdited.projection.top.name, "ÿÿÿÿ");
    });

    it("treats authority string fields as Latin-1 bytes even when their text looks numeric", async () => {
        const source = Buffer.from(datSource("123").toString("latin1")
            .replace("walking_speed: 4.5\n", "walking_speed: 4.5\nfile(0-0): 789 w: 3 h: 2 row: 1 col: 1\n")
            .replace("sound: data\\001.wav", "sound: 456"), "latin1");
        const { service, documentId } = await fixture(source);
        const initial = await service.openDocument(documentId, "plaintext");
        const name = initial.fields.find((field) => field.key === "name" && field.value === "123") ;
        const sound = initial.fields.find((field) => field.key === "sound") ;
        const file = initial.fields.find((field) => field.scope === "sprite" && field.key === "file") ;
        assert.equal(name.kind, "string");
        assert.equal(sound.kind, "string");
        assert.equal(file.kind, "string");
        assert.equal(sound.value, "456");
        assert.equal(file.value, "789");
        assert.equal(initial.projection.top.name, "123");
        assert.equal(initial.projection.frames[0]?.sound, "456");
        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: name.fieldId,
            value: 123,
            expectedRevision: 0,
        }), "invalid-request");
        const edited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: name.fieldId,
            value: "321",
            expectedRevision: 0,
        });
        assert.equal(edited.projection.top.name, "321");
        const soundEdited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: sound.fieldId,
            value: "654",
            expectedRevision: 1,
        });
        assert.equal(soundEdited.fields.find((field) => field.fieldId === sound.fieldId)?.value, "654");
        assert.equal(soundEdited.projection.frames[0]?.sound, "654");
        const fileEdited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: file.fieldId,
            value: "987",
            expectedRevision: 2,
        });
        assert.equal(fileEdited.fields.find((field) => field.fieldId === file.fieldId)?.value, "987");
        assert.equal(fileEdited.projection.spriteRanges[0]?.file, "987");
    });

    it("distinguishes C++ int32 fields from movement doubles", async () => {
        const { service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const pic = initial.fields.find((field) => field.key === "pic" && field.value === 2) ;
        const walkingSpeed = initial.fields.find((field) => field.key === "walking_speed") ;
        assert.equal(initial.dirty, false);
        assert.equal(pic.numericKind, "integer");
        assert.equal(walkingSpeed.numericKind, "number");
        assert.equal(Object.hasOwn(initial.fields.find((field) => field.key === "name") , "numericKind"), false);

        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: pic.fieldId,
            value: 1.5,
            expectedRevision: 0,
        }), "invalid-request");
        const afterFraction = await service.edit({
            sessionId: initial.sessionId,
            fieldId: pic.fieldId,
            value: 2,
            expectedRevision: 0,
        });
        assert.equal(afterFraction.revision, 0);
        assert.equal(afterFraction.dirty, false);
        assert.equal(afterFraction.projection.frames[0]?.pic, 2);

        const max = await service.edit({
            sessionId: initial.sessionId,
            fieldId: pic.fieldId,
            value: 2_147_483_647,
            expectedRevision: 0,
        });
        assert.equal(max.revision, 1);
        assert.equal(max.dirty, true);
        assert.equal(max.projection.frames[0]?.pic, 2_147_483_647);
        const min = await service.edit({
            sessionId: initial.sessionId,
            fieldId: pic.fieldId,
            value: -2_147_483_648,
            expectedRevision: 1,
        });
        assert.equal(min.revision, 2);
        assert.equal(min.projection.frames[0]?.pic, -2_147_483_648);
        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: pic.fieldId,
            value: 2_147_483_648,
            expectedRevision: 2,
        }), "invalid-request");

        const movement = await service.edit({
            sessionId: initial.sessionId,
            fieldId: walkingSpeed.fieldId,
            value: 1.5,
            expectedRevision: 2,
        });
        assert.equal(movement.revision, 3);
        assert.equal(movement.fields.find((field) => field.fieldId === walkingSpeed.fieldId)?.value, 1.5);
        assert.equal(movement.projection.top.walking_speed, 1.5);
    });

    it("fails closed for malformed scalar or ITR pair values and withholds unknown capabilities", async () => {
        const fractional = await fixture(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2", "pic: 1 pic: 1.5"), "latin1"));
        await rejectsCode(fractional.service.openDocument(fractional.documentId, "plaintext"), "invalid-request");
        const overflowing = await fixture(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2", "pic: 1 pic: 2147483648"), "latin1"));
        await rejectsCode(overflowing.service.openDocument(overflowing.documentId, "plaintext"), "invalid-request");
        const nonnumeric = await fixture(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2", "pic: 1 pic: nope"), "latin1"));
        await rejectsCode(nonnumeric.service.openDocument(nonnumeric.documentId, "plaintext"), "invalid-request");
        const empty = await fixture(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2 state:", "pic: 1 pic: state:"), "latin1"));
        await rejectsCode(empty.service.openDocument(empty.documentId, "plaintext"), "invalid-request");
        const multiToken = await fixture(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2", "pic: 1 pic: 2 3"), "latin1"));
        await rejectsCode(multiToken.service.openDocument(multiToken.documentId, "plaintext"), "invalid-request");
        const badMovement = await fixture(Buffer.from(datSource().toString("latin1").replace("walking_speed: 4.5", "walking_speed: nope"), "latin1"));
        await rejectsCode(badMovement.service.openDocument(badMovement.documentId, "plaintext"), "invalid-request");

        for (const malformedPair of ["5", "5 6 7", "5.0 6", "2147483648 0", "-2147483649 0"]) {
            const malformed = await fixture(Buffer.from(
                datSource().toString("latin1").replace("catchingact: 5 6", `catchingact: ${malformedPair}`),
                "latin1",
            ));
            await rejectsCode(malformed.service.openDocument(malformed.documentId, "plaintext"), "invalid-request");
        }

        const source = Buffer.from(datSource().toString("latin1").replace(
            "catchingact: 5 6",
            "catchingact: 5 6 caughtact: 7 8 mystery: hello",
        ), "latin1");
        const { service, documentId } = await fixture(source);
        const view = await service.openDocument(documentId, "plaintext");
        assert.deepEqual(view.fields.find((field) => field.key === "catchingact")?.value, [5, 6]);
        assert.deepEqual(view.fields.find((field) => field.key === "caughtact")?.value, [7, 8]);
        assert.equal(view.fields.some((field) => field.key === "mystery"), false);
        await rejectsCode(service.edit({
            sessionId: view.sessionId,
            fieldId: "forged-pair-capability",
            value: "hello",
            expectedRevision: 0,
        }), "unknown-field");

        const reloadFixture = await fixture(datSource());
        const initial = await reloadFixture.service.openDocument(reloadFixture.documentId, "plaintext");
        const initialPic = initial.fields.find((field) => field.key === "pic" && field.value === 2) ;
        reloadFixture.client.setBytes(Buffer.from(datSource().toString("latin1").replace("pic: 1 pic: 2", "pic: 1 pic: nope"), "latin1"));
        await rejectsCode(reloadFixture.service.reload({ sessionId: initial.sessionId, expectedRevision: 0 }), "reload-failed");
        const unchanged = await reloadFixture.service.edit({
            sessionId: initial.sessionId,
            fieldId: initialPic.fieldId,
            value: 2,
            expectedRevision: 0,
        });
        assert.equal(unchanged.revision, 0);
        assert.equal(unchanged.projection.frames[0]?.pic, 2);
        assert.equal((await reloadFixture.registry.readDocument(reloadFixture.documentId)).externallyModified, true);
    });

    it("locates duplicate frames, blocks, fields, and edits ITR pairs atomically", async () => {
        const source = duplicateLocatorSource();
        const { service, documentId } = await fixture(source);
        const initial = await service.openDocument(documentId, "plaintext");
        const firstFramePair = initial.fields.find((field) => (
            field.kind === "integer-pair"
            && field.key === "catchingact"
            && field.frameOccurrence === 0
            && field.blockType === "itr"
            && field.blockIndex === 0
            && field.occurrence === 1
        ));
        const secondFramePic = initial.fields.find((field) => (
            field.kind === "number"
            && field.key === "pic"
            && field.frameOccurrence === 1
            && field.occurrence === 1
        ));
        const caughtPair = initial.fields.find((field) => (
            field.kind === "integer-pair"
            && field.key === "caughtact"
            && field.frameOccurrence === 0
            && field.blockIndex === 1
        ));
        assert.ok(firstFramePair);
        assert.ok(secondFramePic);
        assert.ok(caughtPair);
        assert.deepEqual(firstFramePair.value, [3, 4]);
        assert.deepEqual(caughtPair.value, [5, 6]);
        assert.deepEqual(
            {
                scope: firstFramePair.scope,
                occurrence: firstFramePair.occurrence,
                frameId: firstFramePair.frameId,
                frameOccurrence: firstFramePair.frameOccurrence,
                blockType: firstFramePair.blockType,
                blockIndex: firstFramePair.blockIndex,
            },
            {
                scope: "block",
                occurrence: 1,
                frameId: 7,
                frameOccurrence: 0,
                blockType: "itr",
                blockIndex: 0,
            },
        );
        assert.notEqual(firstFramePair.fieldId, initial.fields.find((field) => (
            field.kind === "integer-pair" && field.frameOccurrence === 1
        ))?.fieldId);

        for (const value of [
            [1],
            [1, 2, 3],
            [1.5, 2],
            ["1", 2],
            [Number.NaN, 2],
            [Number.POSITIVE_INFINITY, 2],
            [2_147_483_648, 0],
            [-2_147_483_649, 0],
            [null, 0],
        ]) {
            await rejectsCode(service.edit({
                sessionId: initial.sessionId,
                fieldId: firstFramePair.fieldId,
                value,
                expectedRevision: 0,
            }), "invalid-request");
        }

        const pairEdited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: firstFramePair.fieldId,
            value: [30, 40],
            expectedRevision: 0,
        });
        assert.equal(pairEdited.revision, 1);
        assert.equal(pairEdited.dirty, true);
        assert.deepEqual(pairEdited.fields.find((field) => field.fieldId === firstFramePair.fieldId)?.value, [30, 40]);
        assert.deepEqual(
            [pairEdited.projection.frames[0]?.itrs[0]?.catchingact, pairEdited.projection.frames[0]?.itrs[0]?.catchingact2],
            [30, 40],
        );
        assert.deepEqual(
            [pairEdited.projection.frames[1]?.itrs[0]?.catchingact, pairEdited.projection.frames[1]?.itrs[0]?.catchingact2],
            [10, 11],
        );
        const pairEmission = await service.emit(initial.sessionId, 1);
        assert.equal(
            pairEmission.plaintext.toString("ascii"),
            source.toString("ascii").replace("catchingact: 3 4", "catchingact: 30 40"),
        );

        const noOp = await service.edit({
            sessionId: initial.sessionId,
            fieldId: firstFramePair.fieldId,
            value: [30, 40],
            expectedRevision: 1,
        });
        assert.equal(noOp.revision, 1);

        const scalarEdited = await service.edit({
            sessionId: initial.sessionId,
            fieldId: secondFramePic.fieldId,
            value: 99,
            expectedRevision: 1,
        });
        assert.equal(scalarEdited.revision, 2);
        assert.equal(scalarEdited.projection.frames[0]?.pic, 2);
        assert.equal(scalarEdited.projection.frames[1]?.pic, 99);
        assert.equal((await service.emit(initial.sessionId, 2)).plaintext.toString("ascii"), source.toString("ascii")
            .replace("catchingact: 3 4", "catchingact: 30 40")
            .replace("pic: 8 pic: 9", "pic: 8 pic: 99"));
    });

    it("applies a batch as one revision and rolls back every field on validation failure", async () => {
        const source = duplicateLocatorSource();
        const { service, documentId } = await fixture(source);
        const initial = await service.openDocument(documentId, "plaintext");
        const pic = initial.fields.find((field) => (
            field.key === "pic" && field.frameOccurrence === 1 && field.occurrence === 1
        )) ;
        const pair = initial.fields.find((field) => field.kind === "integer-pair") ;

        const edited = await service.editBatch({
            sessionId: initial.sessionId,
            edits: [
                { fieldId: pic.fieldId, value: 99 },
                { fieldId: pair.fieldId, value: [30, 31] },
            ],
            expectedRevision: 0,
        });
        assert.equal(edited.revision, 1);
        assert.equal(edited.dirty, true);
        assert.equal(edited.projection.frames[1]?.pic, 99);
        assert.deepEqual(edited.fields.find((field) => field.fieldId === pair.fieldId)?.value, [30, 31]);
        assert.equal((await service.emit(initial.sessionId, 1)).plaintext.toString("ascii"), source.toString("ascii")
            .replace("pic: 8 pic: 9", "pic: 8 pic: 99")
            .replace("catchingact: 1 2", "catchingact: 30 31"));

        await assert.rejects(service.editBatch({
            sessionId: initial.sessionId,
            edits: [
                { fieldId: pic.fieldId, value: 100 },
                { fieldId: pair.fieldId, value: [40, 41] },
            ],
            expectedRevision: 1,
        }, () => {
            throw new Error("preview failed");
        }), /preview failed/u);
        await rejectsCode(service.editBatch({
            sessionId: initial.sessionId,
            edits: [{ fieldId: pic.fieldId, value: 12 }],
            expectedRevision: 0,
        }), "revision-conflict");
        await rejectsCode(service.editBatch({
            sessionId: initial.sessionId,
            edits: [
                { fieldId: pic.fieldId, value: 12 },
                { fieldId: "missing-capability", value: 13 },
            ],
            expectedRevision: 1,
        }), "unknown-field");
        assert.equal((await service.emit(initial.sessionId, 1)).plaintext.toString("ascii"), source.toString("ascii")
            .replace("pic: 8 pic: 9", "pic: 8 pic: 99")
            .replace("catchingact: 1 2", "catchingact: 30 31"));
    });

    it("re-signs structure capabilities after lossless frame and block transactions", async () => {
        const source = duplicateLocatorSource();
        const { service, documentId } = await fixture(source);
        const initial = await service.openDocument(documentId, "plaintext");
        const frameCapability = initial.structureCapabilities.find((frame) => frame.occurrence === 0) ;
        const firstBlock = frameCapability.blocks.find((block) => block.blockType === "itr" && block.blockIndex === 0) ;
        assert.equal(frameCapability.canCopy, true);
        assert.equal(firstBlock.canCopy, true);

        const copiedFrame = await service.editStructure({
            sessionId: initial.sessionId,
            capabilityId: frameCapability.capabilityId,
            operation: "copy-frame",
            newFrameId: 17,
            expectedRevision: 0,
        });
        assert.equal(copiedFrame.revision, 1);
        assert.equal(copiedFrame.dirty, true);
        assert.ok(copiedFrame.projection.frames.some((frame) => frame.frameId === 17));
        assert.equal(copiedFrame.structureCapabilities.some((frame) => frame.capabilityId === frameCapability.capabilityId), false);
        await rejectsCode(service.editStructure({
            sessionId: initial.sessionId,
            capabilityId: frameCapability.capabilityId,
            operation: "delete-frame",
            expectedRevision: 1,
        }), "unknown-field");

        const copiedFrameCapability = copiedFrame.structureCapabilities.find((frame) => frame.occurrence === 0) ;
        await assert.rejects(service.editStructure({
            sessionId: copiedFrame.sessionId,
            capabilityId: copiedFrameCapability.capabilityId,
            operation: "copy-frame",
            newFrameId: 16,
            expectedRevision: 1,
        }, () => {
            throw new Error("preview failed");
        }), /preview failed/u);
        assert.doesNotMatch((await service.emit(initial.sessionId, 1)).plaintext.toString("ascii"), /<frame> 16/u);

        const copiedBlockCapability = copiedFrame.structureCapabilities
            .find((frame) => frame.occurrence === 0) .blocks
            .find((block) => block.blockType === "itr" && block.blockIndex === 0) ;
        const copiedBlock = await service.editStructure({
            sessionId: copiedFrame.sessionId,
            capabilityId: copiedBlockCapability.capabilityId,
            operation: "copy-block",
            expectedRevision: 1,
        });
        assert.equal(copiedBlock.revision, 2);
        assert.equal(copiedBlock.projection.frames[0]?.itrs.length, 3);
        assert.equal(copiedBlock.structureCapabilities.find((frame) => frame.occurrence === 0)?.blocks
            .filter((block) => block.blockType === "itr").length, 3);
        const emitted = await service.emit(initial.sessionId, 2);
        assert.match(emitted.plaintext.toString("ascii"), /<frame> 17 first/u);
        assert.match(emitted.plaintext.toString("ascii"), /catchingact: 3 4/u);
    });

    it("preserves the encrypted envelope when a structure transaction changes plaintext", async () => {
        const prefix = Buffer.alloc(123, 0x51);
        const encrypted = encryptDatPayload(prefix, duplicateLocatorSource());
        const { service, documentId } = await fixture(encrypted);
        const initial = await service.openDocument(documentId, "encrypted");
        const frame = initial.structureCapabilities.find((candidate) => candidate.occurrence === 0) ;
        const result = await service.editStructure({
            sessionId: initial.sessionId,
            capabilityId: frame.capabilityId,
            operation: "copy-frame",
            newFrameId: 18,
            expectedRevision: 0,
        });
        assert.equal(result.format, "encrypted");
        const emitted = await service.emit(initial.sessionId, result.revision);
        assert.deepEqual(emitted.file.subarray(0, prefix.length), encrypted.subarray(0, prefix.length));
        assert.match(emitted.plaintext.toString("ascii"), /<frame> 18 first/u);
    });

    it("keeps field IDs stable within an epoch, preserves no-op revision, and prioritizes stale revision", async () => {
        const { service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const field = initial.fields.find((candidate) => candidate.key === "pic" && candidate.value === 2) ;
        const edited = await service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 9, expectedRevision: 0 });
        assert.equal(edited.revision, 1);
        assert.deepEqual(edited.fields.map((candidate) => candidate.fieldId), initial.fields.map((candidate) => candidate.fieldId));
        assert.equal(edited.projection.frames[0]?.pic, 9);
        await rejectsCode(
            service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 1.5, expectedRevision: 0 }),
            "revision-conflict",
        );
        const noOp = await service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 9, expectedRevision: 1 });
        assert.equal(noOp.revision, 1);
        await rejectsCode(
            service.edit({ sessionId: initial.sessionId, fieldId: "unknown-field", value: 10, expectedRevision: 0 }),
            "revision-conflict",
        );
        await rejectsCode(
            service.edit({ sessionId: initial.sessionId, fieldId: "unknown-field", value: 10, expectedRevision: 1 }),
            "unknown-field",
        );
    });

    it("rejects cross-session fields and serializes concurrent same-revision edits per session", async () => {
        const { service, documentId } = await fixture(datSource());
        const first = await service.openDocument(documentId, "plaintext");
        const second = await service.openDocument(documentId, "plaintext");
        const firstField = first.fields.find((field) => field.key === "pic") ;
        const secondField = second.fields.find((field) => field.key === "pic") ;
        await rejectsCode(
            service.edit({ sessionId: first.sessionId, fieldId: secondField.fieldId, value: 5, expectedRevision: 0 }),
            "unknown-field",
        );
        const results = await Promise.allSettled([
            service.edit({ sessionId: first.sessionId, fieldId: firstField.fieldId, value: 5, expectedRevision: 0 }),
            service.edit({ sessionId: first.sessionId, fieldId: firstField.fieldId, value: 6, expectedRevision: 0 }),
        ]);
        assert.equal(results.filter((result) => result.status === "fulfilled").length, 1);
        const rejected = results.find((result)                                  => result.status === "rejected");
        assert.ok(rejected?.reason instanceof DatSessionError);
        assert.equal(rejected.reason.code, "revision-conflict");
    });

    it("reloads external bytes atomically, discards patches, refreshes fingerprints, and rotates every field capability", async () => {
        const { client, registry, service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const field = initial.fields.find((candidate) => candidate.key === "pic" && candidate.value === 2) ;
        const edited = await service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 5, expectedRevision: 0 });
        client.setBytes(datSource("Reloaded Hero", 9));
        const reloaded = await service.reload({ sessionId: initial.sessionId, expectedRevision: 1 });

        assert.equal(reloaded.revision, 2);
        assert.equal(reloaded.projection.top.name, "Reloaded Hero");
        assert.equal(reloaded.projection.frames[0]?.pic, 9);
        assert.equal(reloaded.fields.some((candidate) => edited.fields.some((old) => old.fieldId === candidate.fieldId)), false);
        assert.equal((await registry.readDocument(documentId)).externallyModified, false);
        await rejectsCode(
            service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 8, expectedRevision: 2 }),
            "unknown-field",
        );

        const currentField = reloaded.fields.find((candidate) => candidate.key === "pic" && candidate.value === 9) ;
        client.canonicalPath = resolve(canonicalRoot, "replacement.dat");
        await rejectsCode(service.reload({ sessionId: initial.sessionId, expectedRevision: 2 }), "reload-failed");
        const unchanged = await service.edit({ sessionId: initial.sessionId, fieldId: currentField.fieldId, value: 9, expectedRevision: 2 });
        assert.equal(unchanged.revision, 2);
        assert.equal(unchanged.projection.frames[0]?.pic, 9);
    });

    it("captures the explicit encrypted format across reload and leaves state unchanged on format failure", async () => {
        const prefix = Buffer.alloc(123, 0x51);
        const { client, service, documentId } = await fixture(encryptDatPayload(prefix, datSource()));
        const initial = await service.openDocument(documentId, "encrypted");
        const oldPic = initial.fields.find((field) => field.key === "pic" && field.value === 2) ;
        client.setBytes(encryptDatPayload(prefix, datSource("Encrypted Reload", 11)));

        const reloaded = await service.reload({ sessionId: initial.sessionId, expectedRevision: 0 });
        assert.equal(reloaded.format, "encrypted");
        assert.equal(reloaded.revision, 1);
        assert.equal(reloaded.projection.top.name, "Encrypted Reload");
        assert.equal(reloaded.projection.frames[0]?.pic, 11);

        const currentPic = reloaded.fields.find((field) => field.key === "pic" && field.value === 11) ;
        client.setBytes(Buffer.alloc(123, 0xa5));
        await rejectsCode(service.reload({ sessionId: initial.sessionId, expectedRevision: 1 }), "reload-failed");
        const unchanged = await service.edit({
            sessionId: initial.sessionId,
            fieldId: currentPic.fieldId,
            value: 11,
            expectedRevision: 1,
        });
        assert.equal(unchanged.format, "encrypted");
        assert.equal(unchanged.revision, 1);
        assert.equal(unchanged.projection.frames[0]?.pic, 11);
        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: oldPic.fieldId,
            value: 11,
            expectedRevision: 1,
        }), "unknown-field");
    });

    it("does not publish a reload replacement when the prepared refresh commit fails", async () => {
        const { client, registry, service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const field = initial.fields.find((candidate) => candidate.key === "pic" && candidate.value === 2) ;
        client.setBytes(datSource("Uncommitted Hero", 99));
        const prepare = registry.prepareDocumentRefresh.bind(registry);
        Object.defineProperty(registry, "prepareDocumentRefresh", {
            configurable: true,
            value: async (id        ) => {
                const prepared = await prepare(id);
                return { snapshot: prepared.snapshot, commit: () => { throw new Error("injected commit failure"); } };
            },
        });

        await rejectsCode(service.reload({ sessionId: initial.sessionId, expectedRevision: 0 }), "reload-failed");
        const unchanged = await service.edit({
            sessionId: initial.sessionId,
            fieldId: field.fieldId,
            value: 2,
            expectedRevision: 0,
        });
        assert.equal(unchanged.revision, 0);
        assert.equal(unchanged.projection.frames[0]?.pic, 2);
        assert.deepEqual(unchanged.fields.map((candidate) => candidate.fieldId), initial.fields.map((candidate) => candidate.fieldId));
    });

    it("fails closed when dispose races a deferred reload plus queued edit and close", async () => {
        const { client, service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId, "plaintext");
        const field = initial.fields.find((candidate) => candidate.key === "pic" && candidate.value === 2) ;
        const read = client.read.bind(client);
        let releaseRead             ;
        let markStarted             ;
        const readGate = new Promise      ((resolveGate) => { releaseRead = resolveGate; });
        const readStarted = new Promise      ((resolveStarted) => { markStarted = resolveStarted; });
        Object.defineProperty(client, "read", {
            configurable: true,
            value: async (request                   ) => {
                markStarted();
                await readGate;
                return await read(request);
            },
        });

        const reload = service.reload({ sessionId: initial.sessionId, expectedRevision: 0 });
        await readStarted;
        const edit = service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 3, expectedRevision: 0 });
        const close = service.close(initial.sessionId);
        service.dispose();
        releaseRead();

        await rejectsCode(reload, "invalid-request");
        await rejectsCode(edit, "invalid-request");
        await rejectsCode(close, "invalid-request");
        await rejectsCode(service.openDocument(documentId, "plaintext"), "invalid-request");
    });

    it("enforces session, field, byte, TTL quotas and releases accounting on close, sweep, and dispose", async () => {
        const source = datSource();
        let now = 1_000;
        const limited = await fixture(source, {
            maxSessions: 1,
            maxLoadedBytes: source.length,
            idleTtlMs: 10,
            now: () => now,
        });
        const first = await limited.service.openDocument(limited.documentId, "plaintext");
        const dirtyField = first.fields.find((field) => field.key === "pic") ;
        await limited.service.edit({ sessionId: first.sessionId, fieldId: dirtyField.fieldId, value: 9, expectedRevision: 0 });
        await rejectsCode(limited.service.openDocument(limited.documentId, "plaintext"), "session-limit");
        assert.equal(await limited.service.close(first.sessionId), true);
        const reopened = await limited.service.openDocument(limited.documentId, "plaintext");
        now += 10;
        assert.equal(limited.service.sweepExpired(), 1);
        await rejectsCode(
            limited.service.edit({ sessionId: reopened.sessionId, fieldId: reopened.fields[0] .fieldId, value: reopened.fields[0] .value, expectedRevision: 0 }),
            "expired",
        );
        await limited.service.openDocument(limited.documentId, "plaintext");
        limited.service.dispose();
        await rejectsCode(limited.service.openDocument(limited.documentId, "plaintext"), "invalid-request");

        const fieldLimited = await fixture(source, { maxFieldsPerSession: 1 });
        await rejectsCode(fieldLimited.service.openDocument(fieldLimited.documentId, "plaintext"), "field-limit");
        const structureSource = Buffer.from([
            "name: structure limit",
            "<frame> 0 a",
            "<frame_end>",
            "<frame> 1 b",
            "<frame_end>",
            "",
        ].join("\n"), "ascii");
        const structureLimited = await fixture(structureSource, { maxFieldsPerSession: 2 });
        await rejectsCode(structureLimited.service.openDocument(structureLimited.documentId, "plaintext"), "field-limit");
        const byteLimited = await fixture(source, { maxLoadedBytes: source.length - 1 });
        await rejectsCode(byteLimited.service.openDocument(byteLimited.documentId, "plaintext"), "byte-limit");
    });

    it("reserves concurrent opens and keeps ID/tombstone lifecycle bounded", async () => {
        const source = datSource();
        const sessionLimited = await fixture(source, { maxSessions: 1, maxLoadedBytes: source.length * 2 });
        const simultaneous = await Promise.allSettled([
            sessionLimited.service.openDocument(sessionLimited.documentId, "plaintext"),
            sessionLimited.service.openDocument(sessionLimited.documentId, "plaintext"),
        ]);
        assert.equal(simultaneous.filter((result) => result.status === "fulfilled").length, 1);
        const sessionRejected = simultaneous.find((result)                                  => result.status === "rejected");
        assert.ok(sessionRejected?.reason instanceof DatSessionError);
        assert.equal(sessionRejected.reason.code, "session-limit");

        const byteLimited = await fixture(source, { maxSessions: 2, maxLoadedBytes: source.length });
        const byteRace = await Promise.allSettled([
            byteLimited.service.openDocument(byteLimited.documentId, "plaintext"),
            byteLimited.service.openDocument(byteLimited.documentId, "plaintext"),
        ]);
        assert.equal(byteRace.filter((result) => result.status === "fulfilled").length, 1);
        const byteRejected = byteRace.find((result)                                  => result.status === "rejected");
        assert.ok(byteRejected?.reason instanceof DatSessionError);
        assert.equal(byteRejected.reason.code, "byte-limit");

        let now = 0;
        const lifecycle = await fixture(source, {
            maxSessions: 1,
            idleTtlMs: 1,
            now: () => now,
            idFactory: () => "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
        });
        const expired                   = [];
        for (let index = 0; index < 4; index += 1) {
            const view = await lifecycle.service.openDocument(lifecycle.documentId, "plaintext");
            expired.push(view);
            now += 1;
            assert.equal(lifecycle.service.sweepExpired(), 1);
        }
        const oldest = expired[0] ;
        const newest = expired.at(-1) ;
        await rejectsCode(
            lifecycle.service.edit({ sessionId: oldest.sessionId, fieldId: oldest.fields[0] .fieldId, value: oldest.fields[0] .value, expectedRevision: 0 }),
            "unknown-session",
        );
        await rejectsCode(
            lifecycle.service.edit({ sessionId: newest.sessionId, fieldId: newest.fields[0] .fieldId, value: newest.fields[0] .value, expectedRevision: 0 }),
            "expired",
        );
    });

    it("bounds diagnostic copies and fails closed for oversized projection or string fields", async () => {
        const malformed = Buffer.from(`name: Keeper\n${Array.from(
            { length: 205 },
            (_unused, index) => `<frame> ${600 + index} invalid\npic: ${index}\n<frame_end>\n`,
        ).join("")}`, "ascii");
        const bounded = await fixture(malformed, { maxDiagnosticMessageLength: 24 });
        const view = await bounded.service.openDocument(bounded.documentId, "plaintext");
        assert.equal(view.diagnostics.length, 200);
        assert.ok(view.diagnostics.every((diagnostic) => diagnostic.message.length <= 24));
        assert.ok(view.diagnostics.every((diagnostic) => Object.keys(diagnostic).every((key) => ["code", "severity", "message"].includes(key))));
        assert.ok(view.fields.every((field) => field.scope === "top"));
        assert.equal(view.projection.frames.length, 0);

        const projectionLimited = await fixture(datSource(), { maxProjectionBytes: 32 });
        await rejectsCode(projectionLimited.service.openDocument(projectionLimited.documentId, "plaintext"), "view-limit");
        const longString = await fixture(Buffer.from(`name: ${"x".repeat(4097)}\n`, "ascii"));
        await rejectsCode(longString.service.openDocument(longString.documentId, "plaintext"), "view-limit");
    });

    it("bounds the complete session view and keeps open, edit, and reload failures atomic", async () => {
        const baselineFixture = await fixture(datSource());
        const baseline = await baselineFixture.service.openDocument(baselineFixture.documentId, "plaintext");
        const baselineBytes = Buffer.byteLength(JSON.stringify(baseline), "utf8");

        const openLimited = await fixture(datSource(), { maxSessions: 1, maxViewBytes: baselineBytes - 1 });
        await rejectsCode(openLimited.service.openDocument(openLimited.documentId, "plaintext"), "view-limit");
        await rejectsCode(openLimited.service.openDocument(openLimited.documentId, "plaintext"), "view-limit");

        const { client, registry, service, documentId } = await fixture(datSource(), { maxViewBytes: baselineBytes + 8 });
        const initial = await service.openDocument(documentId, "plaintext");
        const name = initial.fields.find((field) => field.key === "name" && field.value === "Second Hero") ;
        const pair = initial.fields.find((field) => field.kind === "integer-pair" && field.key === "catchingact") ;
        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: name.fieldId,
            value: "x".repeat(100),
            expectedRevision: 0,
        }), "view-limit");
        const afterEditFailure = await service.edit({
            sessionId: initial.sessionId,
            fieldId: name.fieldId,
            value: "Second Hero",
            expectedRevision: 0,
        });
        assert.equal(afterEditFailure.revision, 0);
        assert.equal(afterEditFailure.projection.top.name, "Second Hero");
        await rejectsCode(service.edit({
            sessionId: initial.sessionId,
            fieldId: pair.fieldId,
            value: [2_147_483_647, -2_147_483_648],
            expectedRevision: 0,
        }), "view-limit");
        const afterPairFailure = await service.edit({
            sessionId: initial.sessionId,
            fieldId: pair.fieldId,
            value: [5, 6],
            expectedRevision: 0,
        });
        assert.equal(afterPairFailure.revision, 0);
        assert.deepEqual(afterPairFailure.fields.find((field) => field.fieldId === pair.fieldId)?.value, [5, 6]);
        assert.deepEqual((await service.emit(initial.sessionId, 0)).plaintext, datSource());

        client.setBytes(datSource("x".repeat(100), 9));
        await rejectsCode(service.reload({ sessionId: initial.sessionId, expectedRevision: 0 }), "view-limit");
        const afterReloadFailure = await service.edit({
            sessionId: initial.sessionId,
            fieldId: name.fieldId,
            value: "Second Hero",
            expectedRevision: 0,
        });
        assert.equal(afterReloadFailure.revision, 0);
        assert.equal(afterReloadFailure.projection.top.name, "Second Hero");
        assert.equal((await registry.readDocument(documentId)).externallyModified, true);
    });

    it("keeps persistence save explicitly deferred", async () => {
        const { service } = await fixture(datSource());
        assert.equal(typeof (service                                 ).save, "undefined");
    });
});
