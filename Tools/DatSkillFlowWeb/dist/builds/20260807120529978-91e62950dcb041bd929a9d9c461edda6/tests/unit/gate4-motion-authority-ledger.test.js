// dat-skill-flow-build:20260807120529978-91e62950dcb041bd929a9d9c461edda6
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    gate4MotionAuthorityLedger,
    validateGate4MotionTraceRuleIds,
} from "../../src/authority/gate4-motion-ledger.js";
import { GATE4_MOTION_RULE, GATE4_MOTION_RULE_IDS } from "../../src/sim/index.js";

describe("Gate4 motion authority ledger", () => {
    it("covers every planned rule with only Makefile-compiled ntsd_cpp authority", () => {
        assert.deepEqual(gate4MotionAuthorityLedger.entries.map((entry) => entry.id), [...GATE4_MOTION_RULE_IDS]);
        const deferred = new Set([
            GATE4_MOTION_RULE.characterFrameDvDeferred,
            GATE4_MOTION_RULE.type3VisualZDeferred,
            GATE4_MOTION_RULE.state12WeaponOverrideDeferred,
            GATE4_MOTION_RULE.specialCharacterLandingDeferred,
        ]);
        for (const entry of gate4MotionAuthorityLedger.entries) {
            assert.equal(entry.status, deferred.has(entry.id) ? "unimplemented" : "authoritative");
            assert.match(entry.source?.file ?? "", /J:\\QQFile\\NTSD2\.4\\ntsd_cpp\\/);
            assert.doesNotMatch(entry.source?.file ?? "", /battle_logic\.cpp/i);
            assert.match(entry.source?.region ?? "", /^lines \d+(?:-\d+)?(?: and \d+(?:-\d+)?)*$/);
        }
        assert.match(
            gate4MotionAuthorityLedger.entries.find((entry) => entry.id === GATE4_MOTION_RULE.characterFrameDvDeferred)?.note ?? "",
            /deferred/i,
        );
        assert.match(
            gate4MotionAuthorityLedger.entries.find((entry) => entry.id === GATE4_MOTION_RULE.specialCharacterLandingDeferred)?.note ?? "",
            /deferred/i,
        );
        assert.match(
            gate4MotionAuthorityLedger.entries.find((entry) => entry.id === GATE4_MOTION_RULE.type3VisualZDeferred)?.note ?? "",
            /deferred/i,
        );
        assert.match(
            gate4MotionAuthorityLedger.entries.find((entry) => entry.id === GATE4_MOTION_RULE.state12WeaponOverrideDeferred)?.note ?? "",
            /weapon_count.*canonical/i,
        );
    });

    it("validates known Gate4 rule ids and rejects unknown ids", () => {
        assert.doesNotThrow(() => validateGate4MotionTraceRuleIds(GATE4_MOTION_RULE_IDS));
        assert.throws(() => validateGate4MotionTraceRuleIds(["sim.motion.unknown"]), /unknown/i);
    });
});
