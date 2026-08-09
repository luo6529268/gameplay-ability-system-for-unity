// dat-skill-flow-build:20260807164848322-67d8115ba97143f7ba6d8982158f1086
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE3A_INPUT_RULE, GATE3A_INPUT_RULE_IDS } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE3A_INPUT_RULE_IDS };

export const gate3aInputAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE3A_INPUT_RULE.eligibleCharacterDat,
            summary: "Post-cooldown input dispatch processes only active entities whose current resolved DAT object type is character type 0.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\core\\main.cpp`,
                function: "battle loop (main)",
                region: "lines 5726-5745",
            },
        },
        {
            id: GATE3A_INPUT_RULE.comboWrapperOrder,
            summary: "Run DRA,DLA,DUA,DDA,DRJ,DLJ,DUJ,DDJ in order with a fresh current-frame lookup before each wrapper, then DJA.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "InputHandler::apply_input",
                region: "lines 3350-3463",
            },
        },
        {
            id: GATE3A_INPUT_RULE.comboCallerSideEffects,
            summary: "Combo and direct callers preserve their own clear/facing side effects even though they ignore do_frame_jump failure.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "InputHandler::apply_input",
                region: "lines 3402-3415 and 3465-3478",
            },
        },
        {
            id: GATE3A_INPUT_RULE.djaSpecialCases,
            summary: "DJA follows the oid-6 global guard and unk_324/link_state/unk_328 branches independently of ordinary combo gating.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "InputHandler::apply_input",
                region: "lines 3426-3460",
            },
        },
        {
            id: GATE3A_INPUT_RULE.directStrictMaximum,
            summary: "Direct hit_a, hit_d, hit_j use ordered if/else-if strict cooldown maxima, so ties do not trigger and a failed earlier candidate blocks later candidates.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "InputHandler::apply_input",
                region: "lines 3465-3478",
            },
        },
        {
            id: GATE3A_INPUT_RULE.jumpDefinedTarget,
            summary: "do_frame_jump resolves negative and +/-999 targets, then requires a truly defined DAT frame in the 0..599 range.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "do_frame_jump",
                region: "lines 300-307",
            },
        },
        {
            id: GATE3A_INPUT_RULE.jumpResourceCost,
            summary: "In nonzero PP mode, the target frame charges mp%1000 PP and C++ toward-zero integer-division (mp/1000)*10 HP with strict preconditions and records both display accumulators.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "do_frame_jump",
                region: "lines 308-318",
            },
        },
        {
            id: GATE3A_INPUT_RULE.jumpSuccessEffects,
            summary: "A successful jump assigns the frame, flips a negative target only in PP mode, and clears all seven cooldown carriers.",
            status: "authoritative",
            source: {
                file: `${cppRoot}\\src\\input\\input_handler.cpp`,
                function: "do_frame_jump",
                region: "lines 319-326",
            },
        },
    ],
});

const gate3aRuleIdSet = new Set(gate3aInputAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate3aInputTraceRuleIds(ruleIds                   )       {
    for (const ruleId of ruleIds) {
        if (!gate3aRuleIdSet.has(ruleId)) {
            throw new TypeError(`unknown Gate3A input authority rule id: ${ruleId}`);
        }
    }
}
