// dat-skill-flow-build:20260801115423928-09c81873a9d24b06a1aef468bacdd1fb
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE3B2_WPOINT_RULE, GATE3B2_WPOINT_RULE_IDS } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE3B2_WPOINT_RULE_IDS };

export const gate3WpointAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE3B2_WPOINT_RULE.dataNormalization,
            summary: "Wpoint rows contain exactly nine ordered integer fields, initialized to zero and retained in source order.",
            status: "authoritative",
            source: { file: `${cppRoot}\\include\\dat_parser.h`, function: "WPointData", region: "lines 89-95" },
        },
        {
            id: GATE3B2_WPOINT_RULE.rng,
            summary: "The simulation RNG advances a uint32 LCG and exposes bits 16 through 30.",
            status: "authoritative",
            source: { file: `${cppRoot}\\include\\ntsd_types.h`, function: "ntsd_rand", region: "lines 138-146" },
        },
        {
            id: GATE3B2_WPOINT_RULE.heldPassOrder,
            summary: "Both held-object passes scan active negative-link objects in ascending slot order.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 2088-2100 and 2570-2577" },
        },
        {
            id: GATE3B2_WPOINT_RULE.heldSync,
            summary: "Held-object frame, facing, delay, integer anchors, cover offsets, and doubles derive from each side's first wpoint.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick held pass 5 and 12", region: "lines 2170-2206 and 2628-2657" },
        },
        {
            id: GATE3B2_WPOINT_RULE.stateRelease,
            summary: "Held states 10 and 12 release first, consume one random frame, copy holder motion, and clamp only double y.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick held release", region: "lines 2223-2244 and 2674-2689" },
        },
        {
            id: GATE3B2_WPOINT_RULE.dvxRelease,
            summary: "A nonzero holder wpoint dvx releases weapon types through their exact type-specific frame and directional depth rules.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick held release", region: "lines 2246-2288 and 2690-2726" },
        },
        {
            id: GATE3B2_WPOINT_RULE.kind3Release,
            summary: "Wpoint kind 3 independently overrides release motion using four RNG calls in frame, x, y, z order.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick held release", region: "lines 2289-2296 and 2727-2734" },
        },
        {
            id: GATE3B2_WPOINT_RULE.negativeLinkValidation,
            summary: "An invalid negative held relation clears only the held object's link state.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick held validation", region: "lines 2090-2100 and 2571-2577" },
        },
        {
            id: GATE3B2_WPOINT_RULE.cacheValidation,
            summary: "A stale held-weapon cache clears only the cached slot and throw-frame guard.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\weapon.cpp`, function: "weapon_sync_held", region: "lines 263-282" },
        },
        {
            id: GATE3B2_WPOINT_RULE.positiveLinkValidation,
            summary: "An invalid positive holder relation clears only the holder link state.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick positive link validation", region: "lines 2538-2556" },
        },
        {
            id: GATE3B2_WPOINT_RULE.pickupKind2,
            summary: "An accepted kind-2 collision candidate maps weapon DAT types 1, 2, 4, and 6 to exact frames and relations.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "collision_check_loop kind 2", region: "lines 1541-1613" },
        },
        {
            id: GATE3B2_WPOINT_RULE.pickupKind7,
            summary: "An accepted kind-7 collision candidate creates its default relation, then applies type and oid overrides.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "collision_check_loop kind 7", region: "lines 1488-1539" },
        },
        {
            id: GATE3B2_WPOINT_RULE.heldAttackPayload,
            summary: "A held kind-5 itr keeps geometry, selects holder previous-frame itr payload by an exact zero-based wpoint index, and forces kind zero after selection.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "collision_check_loop held itr resolution", region: "lines 560-601" },
        },
        {
            id: GATE3B2_WPOINT_RULE.forceDropDeferred,
            summary: "Force-drop cleanup exists, but its hp<=0 character-cleanup scheduling is deferred until that lifecycle branch exists in the web simulator.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "run_late_entity_update", region: "lines 1154-1155" },
            note: "The pure helper is exported and tested from weapon.cpp lines 285-304; no unrelated death-cleanup pass is introduced by Gate3B2.",
        },
    ],
});

const ruleIds = new Set(gate3WpointAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate3WpointTraceRuleIds(values                   )       {
    for (const value of values) {
        if (!ruleIds.has(value)) throw new TypeError(`unknown Gate3B2 authority rule id: ${value}`);
    }
}
