// dat-skill-flow-build:20260810142555053-2342a9cd97b748478af583aa85ff65b4
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    gate3WpointAuthorityLedger,
    validateGate3WpointTraceRuleIds,
} from "../../src/authority/gate3-wpoint-ledger.js";
import { GATE3B2_WPOINT_RULE, GATE3B2_WPOINT_RULE_IDS } from "../../src/sim/index.js";

describe("Gate3B2 wpoint authority ledger", () => {
    it("covers every rule with exact compiled ntsd_cpp sources and never cites excluded battle_logic.cpp", () => {
        assert.deepEqual(gate3WpointAuthorityLedger.entries.map((entry) => entry.id), [...GATE3B2_WPOINT_RULE_IDS]);
        for (const entry of gate3WpointAuthorityLedger.entries) {
            assert.match(entry.source?.file ?? "", /J:\\QQFile\\NTSD2\.4\\ntsd_cpp\\/);
            assert.doesNotMatch(entry.source?.file ?? "", /battle_logic\.cpp/i);
            assert.match(entry.source?.region ?? "", /^lines \d+(?:-\d+)?(?: and \d+(?:-\d+)?)*$/);
            assert.ok(entry.source?.function);
        }
        assert.equal(
            gate3WpointAuthorityLedger.entries.find((entry) => entry.id === GATE3B2_WPOINT_RULE.forceDropDeferred)?.status,
            "unimplemented",
        );
        assert.ok(gate3WpointAuthorityLedger.entries
            .filter((entry) => entry.id !== GATE3B2_WPOINT_RULE.forceDropDeferred)
            .every((entry) => entry.status === "authoritative"));
    });

    it("validates known trace rule ids and rejects unknown ids", () => {
        assert.doesNotThrow(() => validateGate3WpointTraceRuleIds(GATE3B2_WPOINT_RULE_IDS));
        assert.throws(() => validateGate3WpointTraceRuleIds(["sim.wpoint.unknown"]), /unknown/i);
    });
});
