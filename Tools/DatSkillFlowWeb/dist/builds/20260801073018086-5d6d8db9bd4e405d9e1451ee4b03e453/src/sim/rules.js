// dat-skill-flow-build:20260801073018086-5d6d8db9bd4e405d9e1451ee4b03e453
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
