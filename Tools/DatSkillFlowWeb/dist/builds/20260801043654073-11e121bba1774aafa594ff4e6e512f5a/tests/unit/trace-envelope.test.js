// dat-skill-flow-build:20260801043654073-11e121bba1774aafa594ff4e6e512f5a
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    canonicalizeTraceEnvelope,
    traceEnvelopeSchema,
} from "../../src/trace/envelope.js";

describe("canonical trace envelope baseline", () => {
    it("validates an empty system trace without a selected corpus", () => {
        const trace = traceEnvelopeSchema.parse({
            schemaVersion: 1,
            streamId: "gate0",
            sequence: 0,
            category: "system",
            tick: null,
            ruleIds: [],
            payload: {},
            diagnostics: [],
        });

        assert.equal(trace.tick, null);
    });

    it("serializes equivalent payloads byte-identically", () => {
        const common = {
            schemaVersion: 1         ,
            streamId: "gate0",
            sequence: 1,
            category: "system"         ,
            tick: null,
            ruleIds: [],
            diagnostics: [],
        };

        const left = canonicalizeTraceEnvelope({ ...common, payload: { z: 1, a: 2 } });
        const right = canonicalizeTraceEnvelope({ ...common, payload: { a: 2, z: 1 } });

        assert.equal(left, right);
    });

    it("rejects non-JSON payload values and unknown fields", () => {
        const base = {
            schemaVersion: 1,
            streamId: "gate0",
            sequence: 0,
            category: "system",
            tick: null,
            ruleIds: [],
            diagnostics: [],
        };

        assert.throws(() => traceEnvelopeSchema.parse({
            ...base,
            payload: { invalid: Number.NaN },
        }), /payload|finite/i);
        assert.throws(() => traceEnvelopeSchema.parse({
            ...base,
            payload: {},
            extra: true,
        }), /extra|unknown/i);
    });
});
