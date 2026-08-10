// dat-skill-flow-build:20260809120430485-1c6dc9f4bb684e76bc0cdc6549bf817d
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE2_SIM_RULE_IDS, GATE2_RULE } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE2_SIM_RULE_IDS };

export const gate2SimAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE2_RULE.clockFrameMs,
            summary: "Advance canonical battle time in exact integer 33 ms ticks.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\core\\main.cpp`,
                function: "battle loop (main)",
                region: "lines 35 and 2480-2493",
            },
        },
        {
            id: GATE2_RULE.frameWaitCounterReset,
            summary: "A frame/wait_counter mismatch resets attacking before its per-tick increment.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1186-1197",
            },
        },
        {
            id: GATE2_RULE.frameAttackingStrictWait,
            summary: "Increment attacking first and advance only when attacking is strictly greater than wait.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1196-1197 and 1231-1236",
            },
        },
        {
            id: GATE2_RULE.frameState0Airborne,
            summary: "Before authored next handling, an airborne state-0 entity with resolved DAT object type >= 0 enters frame 212.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1199-1205",
            },
        },
        {
            id: GATE2_RULE.frameNextZero,
            summary: "At the wait threshold next=0 clears attacking but does not change frame, then reaches the common tail.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1231-1238 and 1332-1340",
            },
        },
        {
            id: GATE2_RULE.frameNextTransition,
            summary: "Every nonzero authored next takes the frame-change branch, including a nonzero self transition and values outside the normal frame range.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1237-1263",
            },
        },
        {
            id: GATE2_RULE.frameNextNegative,
            summary: "A negative next flips facing before its absolute frame target is assigned.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1249-1257",
            },
        },
        {
            id: GATE2_RULE.frameNext999,
            summary: "next=999 resolves to 212 only for y_int!=0 with raw DAT obj_type 0; all other cases resolve to frame 0.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\frame_advance.cpp`,
                function: "frame_tick",
                region: "lines 1240-1248",
            },
        },
        {
            id: GATE2_RULE.lateFrameCollisionOrder,
            summary: "For each active entity, late update runs frame_tick before exactly one collision dispatch.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\game_tick.cpp`,
                function: "run_late_entity_update",
                region: "lines 1117-1132",
            },
        },
        {
            id: GATE2_RULE.lateGroupReset,
            summary: "Frames 1100..1299 propagate 1100-frame hit_stop to active kill_count children and self, reset self to frame 0, and remain active.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\game_tick.cpp`,
                function: "run_late_entity_update",
                region: "lines 1134-1146",
            },
        },
        {
            id: GATE2_RULE.lateInvalidFrameFree,
            summary: "After collision and group handling, frames below 0 or at least 400 reset to 0 and free the entity.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\game_tick.cpp`,
                function: "run_late_entity_update",
                region: "lines 1147-1152",
            },
        },
        {
            id: GATE2_RULE.lifecycleActiveGuardFree,
            summary: "free_entity is guarded by a valid active slot and decrements object_count only once.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\entity\\game_tick.cpp`,
                function: "GameWorld::free_entity",
                region: "lines 3007-3054",
            },
        },
    ],
});

const gate2RuleIdSet = new Set(gate2SimAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate2TraceRuleIds(ruleIds                   )       {
    for (const ruleId of ruleIds) {
        if (!gate2RuleIdSet.has(ruleId)) {
            throw new TypeError(`unknown Gate 2 authority rule id: ${ruleId}`);
        }
    }
}
