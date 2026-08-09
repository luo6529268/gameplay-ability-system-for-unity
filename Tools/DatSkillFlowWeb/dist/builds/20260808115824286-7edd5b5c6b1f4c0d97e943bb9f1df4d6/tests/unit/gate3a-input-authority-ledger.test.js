// dat-skill-flow-build:20260808115824286-7edd5b5c6b1f4c0d97e943bb9f1df4d6
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    GATE3A_INPUT_RULE_IDS,
    gate3aInputAuthorityLedger,
    validateGate3aInputTraceRuleIds,
} from "../../src/authority/gate3-input-ledger.js";

describe("Gate3A hit-input authority ledger", () => {
    it("maps every Gate3A rule to exact ntsd_cpp compiled-source lines", () => {
        assert.ok(gate3aInputAuthorityLedger.entries.length > 0);
        assert.deepEqual(
            [...GATE3A_INPUT_RULE_IDS].sort(),
            gate3aInputAuthorityLedger.entries.map((entry) => entry.id).sort(),
        );
        for (const entry of gate3aInputAuthorityLedger.entries) {
            assert.equal(entry.status, "authoritative");
            assert.ok(entry.source);
            assert.match(entry.source.file, /ntsd_cpp/);
            assert.doesNotMatch(entry.source.file, /battle_logic\.cpp/i);
            assert.match(entry.source.region, /lines? \d+/i);
        }
    });

    it("rejects trace rules outside the Gate3A ledger", () => {
        assert.doesNotThrow(() => validateGate3aInputTraceRuleIds(GATE3A_INPUT_RULE_IDS));
        assert.throws(() => validateGate3aInputTraceRuleIds(["sim.input.guessed"]), /unknown Gate3A/i);
    });
});
