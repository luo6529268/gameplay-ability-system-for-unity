// dat-skill-flow-build:20260811064400996-176efb9934ee40bca081ad78251120c8
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE3B1_OPOINT_RULE, GATE3B1_OPOINT_RULE_IDS } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE3B1_OPOINT_RULE_IDS };

export const gate3OpointAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE3B1_OPOINT_RULE.fixedWorldSlots,
            summary: "The battle world owns exactly 400 runtime object slots.",
            status: "authoritative",
            source: { file: `${cppRoot}\\include\\game_world.h`, function: "MAX_OBJECTS", region: "lines 13-14" },
        },
        {
            id: GATE3B1_OPOINT_RULE.catalogResolve,
            summary: "Resolve an opoint oid through the loaded 0..999 DAT catalog before scanning for a free runtime slot.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "spawn_from_opoint", region: "lines 1822-1824 and 1835-1891" },
        },
        {
            id: GATE3B1_OPOINT_RULE.opointGuards,
            summary: "Apply first-opoint, attacking, character frame-delay, and per-row validity guards before spawning.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\frame_advance.cpp`, function: "process_opoint_spawn", region: "lines 133-178" },
        },
        {
            id: GATE3B1_OPOINT_RULE.spawnInitialize,
            summary: "Initialize a successful opoint child from reset defaults, parent fields, DAT type data, opoint values, directional state, and kind-2 links.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "spawn_from_opoint", region: "lines 1949-2028" },
        },
        {
            id: GATE3B1_OPOINT_RULE.cooldownReset,
            summary: "A successful allocation clears attack rest plus the reused slot's vrest row and column.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\entity_collision.cpp`, function: "reset_cooldowns", region: "lines 99-103" },
        },
        {
            id: GATE3B1_OPOINT_RULE.multiSpawn,
            summary: "Decode count/mode, apply deterministic velocity spread and center exemptions, then set state-3003 and sibling vrest pairs.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\frame_advance.cpp`, function: "process_opoint_spawn", region: "lines 171-178 and 249-296" },
        },
        {
            id: GATE3B1_OPOINT_RULE.dynamicLateSlots,
            summary: "Late update scans live slots 0..399 so births in later slots run this tick while births in earlier slots wait.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "run_late_entity_update and late loop", region: "lines 1117-1178 and 2859-2873" },
        },
    ],
});

const ruleIds = new Set(gate3OpointAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate3OpointTraceRuleIds(values                   )       {
    for (const value of values) {
        if (!ruleIds.has(value)) {
            throw new TypeError(`unknown Gate3B1 authority rule id: ${value}`);
        }
    }
}
