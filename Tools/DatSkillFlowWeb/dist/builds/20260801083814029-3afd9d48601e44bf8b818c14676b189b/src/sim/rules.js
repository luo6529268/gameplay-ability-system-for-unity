// dat-skill-flow-build:20260801083814029-3afd9d48601e44bf8b818c14676b189b
export const GATE2_RULE = Object.freeze({
    clockFrameMs: "sim.clock.frame-ms",
    frameWaitCounterReset: "sim.frame.wait-counter-reset",
    frameAttackingStrictWait: "sim.frame.attacking-strict-wait",
    frameState0Airborne: "sim.frame.state0-airborne",
    frameNextZero: "sim.frame.next-zero",
    frameNextTransition: "sim.frame.next-transition",
    frameNextNegative: "sim.frame.next-negative",
    frameNext999: "sim.frame.next-999",
    lateFrameCollisionOrder: "sim.late.frame-collision-order",
    lateGroupReset: "sim.late.group-reset",
    lateInvalidFrameFree: "sim.late.invalid-frame-free",
    lifecycleActiveGuardFree: "sim.lifecycle.active-guard-free",
}         );

export const GATE2_SIM_RULE_IDS = Object.freeze(Object.values(GATE2_RULE));

export const GATE3A_INPUT_RULE = Object.freeze({
    eligibleCharacterDat: "sim.input.eligible-character-dat",
    comboWrapperOrder: "sim.input.combo-wrapper-order",
    comboCallerSideEffects: "sim.input.combo-caller-side-effects",
    djaSpecialCases: "sim.input.dja-special-cases",
    directStrictMaximum: "sim.input.direct-strict-maximum",
    jumpDefinedTarget: "sim.input.jump-defined-target",
    jumpResourceCost: "sim.input.jump-resource-cost",
    jumpSuccessEffects: "sim.input.jump-success-effects",
}         );

export const GATE3A_INPUT_RULE_IDS = Object.freeze(Object.values(GATE3A_INPUT_RULE));

export const GATE3B1_OPOINT_RULE = Object.freeze({
    fixedWorldSlots: "sim.world.fixed-slots",
    catalogResolve: "sim.opoint.catalog-resolve",
    opointGuards: "sim.opoint.guards",
    spawnInitialize: "sim.opoint.spawn-initialize",
    cooldownReset: "sim.opoint.cooldown-reset",
    multiSpawn: "sim.opoint.multi-spawn",
    dynamicLateSlots: "sim.late.dynamic-slots",
}         );

export const GATE3B1_OPOINT_RULE_IDS = Object.freeze(Object.values(GATE3B1_OPOINT_RULE));
