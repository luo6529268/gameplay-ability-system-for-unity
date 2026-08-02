// dat-skill-flow-build:20260801151111863-ba03d385666f4009876b36415e7be0ab
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE4B_PRESENTATION_RULE, GATE4B_PRESENTATION_RULE_IDS } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE4B_PRESENTATION_RULE_IDS };

export const gate4bPresentationAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE4B_PRESENTATION_RULE.cameraSubjects,
            summary: "Camera primary subjects are living active runtime characters in slots below 8; fallback subjects are all living active character-DAT entities, then synthetic x=800.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 2755-2775" },
        },
        {
            id: GATE4B_PRESENTATION_RULE.cameraTargetClamp,
            summary: "The integer-average target subtracts 397, clamps to stage width minus 794, then applies a positive camera maximum override.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 2776-2780" },
        },
        {
            id: GATE4B_PRESENTATION_RULE.cameraSmoothing,
            summary: "Camera step and velocity use C++ truncating integer division, force +/-1 when a nonzero difference smooths to zero, then clamp camera X.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 2781-2788" },
        },
        {
            id: GATE4B_PRESENTATION_RULE.cameraNarrowReset,
            summary: "A stage width at most 794 resets camera X and velocity to zero.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 2757-2758 and 2789-2791" },
        },
        {
            id: GATE4B_PRESENTATION_RULE.depthSlotOrder,
            summary: "Active entities are collected in slot order and stable-sorted by ascending z_int.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\render\\renderer.cpp`, function: "Renderer::render_world", region: "lines 1517-1534" },
        },
        {
            id: GATE4B_PRESENTATION_RULE.screenProjection,
            summary: "The scoped entity anchor projects X as x_int plus render offset minus camera and Y as z_int plus y_int.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\render\\renderer.cpp`, function: "Renderer::draw_entity", region: "lines 613-618" },
            note: "Gate4B1 excludes the independent negative-frame-delay render jitter and type-3 visual-only Z offset branches.",
        },
        {
            id: GATE4B_PRESENTATION_RULE.perspectiveOffsetDeferred,
            summary: "Perspective render_offset_x depends on stage near/far and Z-boundary fields not yet present in the presentation input model.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\render\\renderer.cpp`, function: "Renderer::render_world", region: "lines 1535-1547" },
            note: "Deferred explicitly; projection defaults renderOffsetX to zero and does not claim full perspective rendering.",
        },
    ],
});

const ruleIds = new Set(gate4bPresentationAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate4bPresentationRuleIds(values                   )       {
    for (const value of values) {
        if (!ruleIds.has(value)) throw new TypeError(`unknown Gate4B presentation authority rule id: ${value}`);
    }
}
