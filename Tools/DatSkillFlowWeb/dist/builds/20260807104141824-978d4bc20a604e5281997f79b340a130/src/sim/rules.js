// dat-skill-flow-build:20260807104141824-978d4bc20a604e5281997f79b340a130
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

export const GATE3B2_WPOINT_RULE = Object.freeze({
    dataNormalization: "sim.wpoint.data-normalization",
    rng: "sim.wpoint.rng",
    heldPassOrder: "sim.wpoint.held-pass-order",
    heldSync: "sim.wpoint.held-sync",
    stateRelease: "sim.wpoint.state-release",
    dvxRelease: "sim.wpoint.dvx-release",
    kind3Release: "sim.wpoint.kind3-release",
    negativeLinkValidation: "sim.wpoint.negative-link-validation",
    cacheValidation: "sim.wpoint.cache-validation",
    positiveLinkValidation: "sim.wpoint.positive-link-validation",
    pickupKind2: "sim.wpoint.pickup-kind2",
    pickupKind7: "sim.wpoint.pickup-kind7",
    heldAttackPayload: "sim.wpoint.held-attack-payload",
    forceDropDeferred: "sim.wpoint.force-drop-deferred",
}         );

export const GATE3B2_WPOINT_RULE_IDS = Object.freeze(Object.values(GATE3B2_WPOINT_RULE));

export const GATE4_MOTION_RULE = Object.freeze({
    passOrder: "sim.motion.pass-order",
    frameAdvanceGates: "sim.motion.frame-advance-gates",
    characterFrameDvDeferred: "sim.motion.character-frame-dv-deferred",
    nonCharacterFrameDv: "sim.motion.noncharacter-frame-dv",
    integrateXz: "sim.motion.integrate-xz",
    specialXCorrection: "sim.motion.special-x-correction",
    groundFriction: "sim.motion.ground-friction",
    verticalGravity: "sim.motion.vertical-gravity",
    fastProjectileFrame: "sim.motion.fast-projectile-frame",
    type3VisualZDeferred: "sim.motion.type3-visual-z-deferred",
    airborneFrameSelect: "sim.motion.airborne-frame-select",
    state12WeaponOverrideDeferred: "sim.motion.state12-weapon-override-deferred",
    genericCharacterLanding: "sim.motion.generic-character-landing",
    specialCharacterLandingDeferred: "sim.motion.special-character-landing-deferred",
    integerSnapshots: "sim.motion.integer-snapshots",
    explicitJumpInit: "sim.motion.explicit-jump-init",
    opointFirstMotion: "sim.motion.opoint-first-motion",
}         );

export const GATE4_MOTION_RULE_IDS = Object.freeze(Object.values(GATE4_MOTION_RULE));

export const GATE4B_PRESENTATION_RULE = Object.freeze({
    cameraSubjects: "presentation.camera.subjects",
    cameraTargetClamp: "presentation.camera.target-clamp",
    cameraSmoothing: "presentation.camera.integer-smoothing",
    cameraNarrowReset: "presentation.camera.narrow-reset",
    depthSlotOrder: "presentation.projection.depth-slot-order",
    screenProjection: "presentation.projection.screen-coordinates",
    perspectiveOffsetDeferred: "presentation.projection.perspective-offset-deferred",
}         );

export const GATE4B_PRESENTATION_RULE_IDS = Object.freeze(Object.values(GATE4B_PRESENTATION_RULE));
