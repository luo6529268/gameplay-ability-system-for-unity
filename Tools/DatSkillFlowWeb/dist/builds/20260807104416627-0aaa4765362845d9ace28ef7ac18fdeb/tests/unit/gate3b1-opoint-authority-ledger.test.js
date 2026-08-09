// dat-skill-flow-build:20260807104416627-0aaa4765362845d9ace28ef7ac18fdeb
import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { gate3OpointAuthorityLedger } from "../../src/authority/gate3-opoint-ledger.js";
import { GATE3B1_OPOINT_RULE_IDS } from "../../src/sim/index.js";

describe("Gate3B1 opoint authority ledger", () => {
    it("has one precise authoritative C++ citation for every implemented rule", () => {
        assert.deepEqual(gate3OpointAuthorityLedger.entries.map((entry) => entry.id), [...GATE3B1_OPOINT_RULE_IDS]);
        for (const entry of gate3OpointAuthorityLedger.entries) {
            assert.equal(entry.status, "authoritative");
            assert.match(entry.source?.file ?? "", /J:\\QQFile\\NTSD2\.4\\ntsd_cpp\\/);
            assert.doesNotMatch(entry.source?.file ?? "", /battle_logic\.cpp/);
            assert.match(entry.source?.region ?? "", /^lines \d+(?:-\d+)?(?: and \d+(?:-\d+)?)*$/);
            assert.doesNotMatch(entry.summary, /state 501|4000|wpoint|pickup/i);
        }
    });
});
