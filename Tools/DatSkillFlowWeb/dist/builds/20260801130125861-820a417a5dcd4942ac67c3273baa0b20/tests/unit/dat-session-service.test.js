// dat-skill-flow-build:20260801130125861-820a417a5dcd4942ac67c3273baa0b20
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { resolve } from "node:path";
import { describe, it } from "node:test";

import { encryptDatPayload } from "../../src/syntax/dat-envelope.js";
import {
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
    it("auto-detects plaintext versus encrypted DAT and emits opaque, duplicate-specific field capabilities", async () => {
        const plaintext = datSource();
        const encrypted = encryptDatPayload(Buffer.alloc(123, 0x41), plaintext);
        const plainFixture = await fixture(plaintext);
        const encryptedFixture = await fixture(encrypted);

        const plain = await plainFixture.service.openDocument(plainFixture.documentId);
        const cipher = await encryptedFixture.service.openDocument(encryptedFixture.documentId);
        assert.equal(plain.encrypted, false);
        assert.equal(cipher.encrypted, true);
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

    it("uses strict edit schemas and validates finite, NUL-free, single-line bounded scalar values", async () => {
        const { service, documentId } = await fixture(datSource());
        const view = await service.openDocument(documentId);
        const numberField = view.fields.find((field) => field.key === "pic") ;
        const stringField = view.fields.find((field) => field.key === "name") ;

        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: numberField.fieldId, value: 3, expectedRevision: 0, extra: true }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: numberField.fieldId, value: Number.POSITIVE_INFINITY, expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "bad\0value", expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "bad\nvalue", expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: view.sessionId, fieldId: stringField.fieldId, value: "x".repeat(4097), expectedRevision: 0 }), "invalid-request");
        await rejectsCode(service.edit({ sessionId: "missing", fieldId: numberField.fieldId, value: 3, expectedRevision: 0 }), "unknown-session");
    });

    it("keeps field IDs stable within an epoch, preserves no-op revision, and prioritizes stale revision", async () => {
        const { service, documentId } = await fixture(datSource());
        const initial = await service.openDocument(documentId);
        const field = initial.fields.find((candidate) => candidate.key === "pic" && candidate.value === 2) ;
        const edited = await service.edit({ sessionId: initial.sessionId, fieldId: field.fieldId, value: 9, expectedRevision: 0 });
        assert.equal(edited.revision, 1);
        assert.deepEqual(edited.fields.map((candidate) => candidate.fieldId), initial.fields.map((candidate) => candidate.fieldId));
        assert.equal(edited.projection.frames[0]?.pic, 9);
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
        const first = await service.openDocument(documentId);
        const second = await service.openDocument(documentId);
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
        const initial = await service.openDocument(documentId);
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

    it("enforces session, field, byte, TTL quotas and releases accounting on close, sweep, and dispose", async () => {
        const source = datSource();
        let now = 1_000;
        const limited = await fixture(source, {
            maxSessions: 1,
            maxLoadedBytes: source.length,
            idleTtlMs: 10,
            now: () => now,
        });
        const first = await limited.service.openDocument(limited.documentId);
        const dirtyField = first.fields.find((field) => field.key === "pic") ;
        await limited.service.edit({ sessionId: first.sessionId, fieldId: dirtyField.fieldId, value: 9, expectedRevision: 0 });
        await rejectsCode(limited.service.openDocument(limited.documentId), "session-limit");
        assert.equal(await limited.service.close(first.sessionId), true);
        const reopened = await limited.service.openDocument(limited.documentId);
        now += 10;
        assert.equal(limited.service.sweepExpired(), 1);
        await rejectsCode(
            limited.service.edit({ sessionId: reopened.sessionId, fieldId: reopened.fields[0] .fieldId, value: reopened.fields[0] .value, expectedRevision: 0 }),
            "expired",
        );
        await limited.service.openDocument(limited.documentId);
        limited.service.dispose();
        await rejectsCode(limited.service.openDocument(limited.documentId), "invalid-request");

        const fieldLimited = await fixture(source, { maxFieldsPerSession: 1 });
        await rejectsCode(fieldLimited.service.openDocument(fieldLimited.documentId), "field-limit");
        const byteLimited = await fixture(source, { maxLoadedBytes: source.length - 1 });
        await rejectsCode(byteLimited.service.openDocument(byteLimited.documentId), "byte-limit");
    });

    it("bounds diagnostic copies and fails closed for oversized projection or string fields", async () => {
        const malformed = Buffer.from(Array.from(
            { length: 205 },
            (_unused, index) => `<frame> ${600 + index} invalid\npic: ${index}\n<frame_end>\n`,
        ).join(""), "ascii");
        const bounded = await fixture(malformed, { maxDiagnosticMessageLength: 24 });
        const view = await bounded.service.openDocument(bounded.documentId);
        assert.equal(view.diagnostics.length, 200);
        assert.ok(view.diagnostics.every((diagnostic) => diagnostic.message.length <= 24));
        assert.ok(view.diagnostics.every((diagnostic) => Object.keys(diagnostic).every((key) => ["code", "severity", "message"].includes(key))));

        const projectionLimited = await fixture(datSource(), { maxProjectionBytes: 32 });
        await rejectsCode(projectionLimited.service.openDocument(projectionLimited.documentId), "view-limit");
        const longString = await fixture(Buffer.from(`name: ${"x".repeat(4097)}\n`, "ascii"));
        await rejectsCode(longString.service.openDocument(longString.documentId), "view-limit");
    });

    it("keeps persistence save explicitly deferred", async () => {
        const { service } = await fixture(datSource());
        assert.equal(typeof (service                                 ).save, "undefined");
    });
});
