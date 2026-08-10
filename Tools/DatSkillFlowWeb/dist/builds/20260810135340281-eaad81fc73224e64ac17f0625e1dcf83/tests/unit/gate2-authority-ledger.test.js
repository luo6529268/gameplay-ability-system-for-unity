// dat-skill-flow-build:20260810135340281-eaad81fc73224e64ac17f0625e1dcf83
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import {
    GATE2_SIM_RULE_IDS,
    gate2SimAuthorityLedger,
    validateGate2TraceRuleIds,
} from "../../src/authority/gate2-sim-ledger.js";
import {
    GATE3B1_OPOINT_RULE_IDS,
    validateGate3OpointTraceRuleIds,
} from "../../src/authority/gate3-opoint-ledger.js";
import {
    GATE3B2_WPOINT_RULE_IDS,
    validateGate3WpointTraceRuleIds,
} from "../../src/authority/gate3-wpoint-ledger.js";
import {
    GATE4_MOTION_RULE_IDS,
    validateGate4MotionTraceRuleIds,
} from "../../src/authority/gate4-motion-ledger.js";
import { createSimulation, stepSimulation } from "../../src/sim/index.js";

describe("Gate 2 simulation authority ledger", () => {
    it("cites an exact formal-build file/function/region for every authoritative rule", () => {
        assert.ok(gate2SimAuthorityLedger.entries.length > 0);
        for (const entry of gate2SimAuthorityLedger.entries) {
            assert.equal(entry.status, "authoritative");
            assert.ok(entry.source);
            assert.match(entry.source.file, /ntsd_cpp/);
            assert.doesNotMatch(entry.source.file, /battle_logic\.cpp/i);
            assert.ok(entry.source.function.length > 0);
            assert.match(entry.source.region, /lines? \d+/i);
        }
        assert.deepEqual(
            [...GATE2_SIM_RULE_IDS].sort(),
            gate2SimAuthorityLedger.entries.map((entry) => entry.id).sort(),
        );
    });

    it("cross-validates every emitted trace rule ID and rejects unknown IDs", () => {
        const initial = createSimulation({
            entities: [{
                stableId: "ledger-entity",
                slot: 0,
                rawObjectType: 0,
                frame: 0,
                waitCounter: 0,
                attacking: 0,
                facing: 0,
                yInt: 0,
                hitStop: 0,
                killCount: -1,
                active: true,
                frames: [{ id: 0, state: 1, wait: 0, next: 1000 }],
            }],
        });
        const trace = stepSimulation(initial, {}).trace;

        const gate2Ids = new Set(GATE2_SIM_RULE_IDS);
        const gate3Ids = new Set(GATE3B1_OPOINT_RULE_IDS);
        const gate3WpointIds = new Set(GATE3B2_WPOINT_RULE_IDS);
        const gate4Ids = new Set(GATE4_MOTION_RULE_IDS);
        assert.equal(trace.ruleIds.every((ruleId) => gate2Ids.has(ruleId) || gate3Ids.has(ruleId) || gate3WpointIds.has(ruleId) || gate4Ids.has(ruleId)), true);
        assert.doesNotThrow(() => validateGate2TraceRuleIds(trace.ruleIds.filter((ruleId) => gate2Ids.has(ruleId))));
        assert.doesNotThrow(() => validateGate3OpointTraceRuleIds(trace.ruleIds.filter((ruleId) => gate3Ids.has(ruleId))));
        assert.doesNotThrow(() => validateGate3WpointTraceRuleIds(trace.ruleIds.filter((ruleId) => gate3WpointIds.has(ruleId))));
        assert.doesNotThrow(() => validateGate4MotionTraceRuleIds(trace.ruleIds.filter((ruleId) => gate4Ids.has(ruleId))));
        assert.throws(() => validateGate2TraceRuleIds(["gate3.hit.not-implemented"]), /unknown/i);
    });
});
