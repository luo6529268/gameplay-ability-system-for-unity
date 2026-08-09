// dat-skill-flow-build:20260808054125811-48e26cf031f34bc09d2b5119d8063ba8
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
