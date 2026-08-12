// dat-skill-flow-build:20260811062528173-158bad8c2c3d43049af8789318d736b9
import { authorityLedgerSchema } from "./ledger.js";
                                                   
import { GATE4_MOTION_RULE, GATE4_MOTION_RULE_IDS } from "../sim/rules.js";

const cppRoot = "J:\\QQFile\\NTSD2.4\\ntsd_cpp";

export { GATE4_MOTION_RULE_IDS };

export const gate4MotionAuthorityLedger                  = authorityLedgerSchema.parse({
    schemaVersion: 1,
    entries: [
        {
            id: GATE4_MOTION_RULE.passOrder,
            summary: "Post-cooldown character input precedes the global step-4 frame-motion scan, which precedes held-object pass 5.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\game_tick.cpp`, function: "game_tick", region: "lines 1576-1579 and 1874-1916 and 2088-2100" },
        },
        {
            id: GATE4_MOTION_RULE.frameAdvanceGates,
            summary: "Frame delay first moves one step toward zero and returns; only a zero delay reaches negative-link and first-cpoint-kind-2 early returns.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\frame_advance.cpp`, function: "frame_advance", region: "lines 56-79" },
            note: "A delay of +/-1 reaches zero at step 4; later frame_tick and process_opoint_spawn therefore observe zero in the same tick.",
        },
        {
            id: GATE4_MOTION_RULE.characterFrameDvDeferred,
            summary: "Character DAT frame dv is applied by the post-cooldown input path rather than by frame_advance motion.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\input\\input_handler.cpp`, function: "InputHandler::apply_input", region: "lines 3639-3683" },
            note: "Deferred until the web input slice owns the full character movement/input path; Gate4 motion must not double-apply it.",
        },
        {
            id: GATE4_MOTION_RULE.nonCharacterFrameDv,
            summary: "Non-character frame dv uses exact >500 absolute encoding, facing-aware dvx thresholds, additive dvy, and two ordered dvz direction tests.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\frame_advance.cpp`, function: "frame_advance", region: "lines 83-116" },
        },
        {
            id: GATE4_MOTION_RULE.integrateXz,
            summary: "X and Z integrate their current velocities unless the matching directional block flag is set; blocking preserves velocity and all four flags clear afterward.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 14-36" },
        },
        {
            id: GATE4_MOTION_RULE.specialXCorrection,
            summary: "After base X integration, DAT type 4 or oid 120 adds 0.2*vx and oid 101 independently subtracts 0.2*vx.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 18-28" },
        },
        {
            id: GATE4_MOTION_RULE.groundFriction,
            summary: "Grounded entities apply one unit of toward-zero friction to vx and vz after X/Z integration.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 49-65" },
        },
        {
            id: GATE4_MOTION_RULE.verticalGravity,
            summary: "Y first integrates old vy, then airborne entities add the exact raw-DAT-type, state-1002, and oid-specific gravity to vy.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 78-108" },
        },
        {
            id: GATE4_MOTION_RULE.fastProjectileFrame,
            summary: "DAT type 4 or 6 in state 1000 selects frame 40 after horizontal friction when absolute vx remains above 9.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 67-75" },
        },
        {
            id: GATE4_MOTION_RULE.type3VisualZDeferred,
            summary: "DAT type 3 can add hit_j-50 to render Z while tracking the same amount as a visual-only Z offset.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 38-47" },
            note: "Deferred until the presentation slice owns type3_visual_z_offset; logical Z motion and gravity exclusion remain in scope.",
        },
        {
            id: GATE4_MOTION_RULE.airborneFrameSelect,
            summary: "Airborne character state 12 and 18 frames select their post-gravity animation frame from exact vy thresholds when the unmodeled negative weapon-count override does not apply.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 110-139" },
        },
        {
            id: GATE4_MOTION_RULE.state12WeaponOverrideDeferred,
            summary: "Airborne state-12 characters with negative weapon_count override the velocity-selected frame from (game_tick-1)%12.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 119-125" },
            note: "Deferred until weapon_count is canonical; the existing tick index is sufficient for the clock term but must not invent the missing weapon field.",
        },
        {
            id: GATE4_MOTION_RULE.genericCharacterLanding,
            summary: "A descending character crossing flat y=0 outside states 12, 13, and 18 applies the generic vx/3, y/vy reset and frame 94, 215, or 219 branch without clearing vz.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 141-149 and 205-217" },
        },
        {
            id: GATE4_MOTION_RULE.specialCharacterLandingDeferred,
            summary: "State 12, 13, and 18 landing damage, rebound, and frame branches require additional combat fields.",
            status: "unimplemented",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 147-204" },
            note: "Deferred until weapon_count, fall_damage_div, and the associated HP/HP-max effects are in canonical state.",
        },
        {
            id: GATE4_MOTION_RULE.integerSnapshots,
            summary: "Motion ends by truncating double X, Y, and Z toward zero into their integer snapshots.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\physics.cpp`, function: "physics_update", region: "lines 320-336" },
        },
        {
            id: GATE4_MOTION_RULE.explicitJumpInit,
            summary: "Only an explicit normal next of positive or negative 212 initializes finite DAT jump velocities; state-0 and next-999 recovery suppress initialization.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\frame_advance.cpp`, function: "frame_tick", region: "lines 1199-1205 and 1240-1257 and 1283-1297" },
        },
        {
            id: GATE4_MOTION_RULE.opointFirstMotion,
            summary: "A late opoint child uses explicit opoint velocity rather than parent velocity and cannot run the already-finished step-4 motion pass until the next tick.",
            status: "authoritative",
            source: { file: `${cppRoot}\\src\\entity\\collision.cpp`, function: "spawn_from_opoint", region: "lines 1949-1984" },
            note: "Late spawn placement is compiled in game_tick.cpp lines 1175-1178 and the step-4 motion scan is lines 1911-1916.",
        },
    ],
});

const ruleIds = new Set(gate4MotionAuthorityLedger.entries.map((entry) => entry.id));

export function validateGate4MotionTraceRuleIds(values                   )       {
    for (const value of values) {
        if (!ruleIds.has(value)) throw new TypeError(`unknown Gate4 motion authority rule id: ${value}`);
    }
}
