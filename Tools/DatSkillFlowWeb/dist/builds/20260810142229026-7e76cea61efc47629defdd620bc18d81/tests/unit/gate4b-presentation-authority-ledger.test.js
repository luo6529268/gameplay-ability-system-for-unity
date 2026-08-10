// dat-skill-flow-build:20260810142229026-7e76cea61efc47629defdd620bc18d81
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    GATE4B_PRESENTATION_RULE_IDS,
    gate4bPresentationAuthorityLedger,
    validateGate4bPresentationRuleIds,
} from "../../src/authority/gate4b-presentation-ledger.js";
import { GATE4B_PRESENTATION_RULE } from "../../src/sim/rules.js";

describe("Gate4B1 presentation authority ledger", () => {
    it("covers compiled camera/projection authority without claiming perspective offset", () => {
        assert.deepEqual(gate4bPresentationAuthorityLedger.entries.map((entry) => entry.id), [...GATE4B_PRESENTATION_RULE_IDS]);
        for (const entry of gate4bPresentationAuthorityLedger.entries) {
            assert.match(entry.source?.file ?? "", /J:\\QQFile\\NTSD2\.4\\ntsd_cpp\\/);
            assert.doesNotMatch(entry.source?.file ?? "", /battle_logic\.cpp/i);
        }
        const deferred = gate4bPresentationAuthorityLedger.entries.find((entry) => entry.id === GATE4B_PRESENTATION_RULE.perspectiveOffsetDeferred);
        assert.equal(deferred?.status, "unimplemented");
        assert.match(deferred?.note ?? "", /defaults renderOffsetX to zero/i);
        assert.equal(gate4bPresentationAuthorityLedger.entries.filter((entry) => entry.id !== deferred?.id).every((entry) => entry.status === "authoritative"), true);
    });

    it("validates every Gate4B1 rule id", () => {
        assert.doesNotThrow(() => validateGate4bPresentationRuleIds(GATE4B_PRESENTATION_RULE_IDS));
        assert.throws(() => validateGate4bPresentationRuleIds(["presentation.unknown"]), /unknown/i);
    });
});
