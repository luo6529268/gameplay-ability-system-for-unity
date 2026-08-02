// dat-skill-flow-build:20260801032133229-bfbf449f49d047a0886e91830127b4c6
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { diagnosticEnvelopeSchema } from "../../src/diagnostics/envelope.js";

describe("diagnostic envelope", () => {
    for (const code of [
        "parse-failure",
        "missing-asset",
        "unsupported-rule",
        "unsafe-save",
    ]         ) {
        it(`represents ${code} without silently repairing data`, () => {
            const diagnostic = diagnosticEnvelopeSchema.parse({
                schemaVersion: 1,
                code,
                severity: "error",
                message: `Observed ${code}`,
                repairApplied: false,
            });

            assert.equal(diagnostic.repairApplied, false);
        });
    }

    it("rejects unknown fields and invalid enum values", () => {
        assert.throws(() => diagnosticEnvelopeSchema.parse({
            schemaVersion: 1,
            code: "parse-failure",
            severity: "fatal",
            message: "invalid severity",
            repairApplied: false,
        }), /severity/i);

        assert.throws(() => diagnosticEnvelopeSchema.parse({
            schemaVersion: 1,
            code: "parse-failure",
            severity: "error",
            message: "unknown field",
            repairApplied: false,
            extra: true,
        }), /extra|unknown/i);
    });
});
