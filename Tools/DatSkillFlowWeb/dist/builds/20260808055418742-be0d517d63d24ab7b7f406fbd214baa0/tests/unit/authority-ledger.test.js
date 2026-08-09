// dat-skill-flow-build:20260808055418742-be0d517d63d24ab7b7f406fbd214baa0
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    authorityLedgerSchema,
    createEmptyAuthorityLedger,
} from "../../src/authority/ledger.js";

describe("authority ledger baseline", () => {
    it("accepts an empty executable Gate 0 ledger", () => {
        const ledger = createEmptyAuthorityLedger();

        assert.deepEqual(authorityLedgerSchema.parse(ledger), ledger);
        assert.deepEqual(ledger.entries, []);
    });

    it("requires a concrete C++ source for authoritative behavior", () => {
        assert.throws(() => authorityLedgerSchema.parse({
            schemaVersion: 1,
            entries: [{
                id: "frame.wait",
                summary: "Advance wait counters",
                status: "authoritative",
            }],
        }), /source/i);
    });

    it("allows an explicitly unsupported behavior without a false citation", () => {
        const ledger = authorityLedgerSchema.parse({
            schemaVersion: 1,
            entries: [{
                id: "preview.unknown",
                summary: "Behavior has not been traced to C++",
                status: "unsupported",
            }],
        });

        assert.equal(ledger.entries[0]?.status, "unsupported");
    });

    it("rejects duplicate ids and unknown fields", () => {
        assert.throws(() => authorityLedgerSchema.parse({
            schemaVersion: 1,
            entries: [
                { id: "frame.wait", summary: "first", status: "unsupported" },
                { id: "frame.wait", summary: "second", status: "unimplemented" },
            ],
        }), /duplicate/i);

        assert.throws(() => authorityLedgerSchema.parse({
            schemaVersion: 1,
            entries: [],
            unexpected: true,
        }), /unexpected|unknown/i);
    });
});
