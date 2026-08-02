import { GATE3A_INPUT_RULE } from "./rules.js";
import type {
    InputJumpEvent,
    InputJumpFailureReason,
    SimEntity,
    SimFrameDefinition,
    SimInputCombos,
    SimInputCooldowns,
    SimWorldInputState,
} from "./types.js";

export const DAT_INPUT_COOLDOWN_KEY_MAP = Object.freeze({
    right: "Right",
    left: "Left",
    up: "Up",
    down: "Down",
    attack: "A (+0xBE)",
    jump: "J (+0xBF)",
    defend: "D (+0xC0)",
} as const);

const ORDINARY_COMBOS = Object.freeze([
    { combo: "DRA", direction: "right", directionMode: "R", final: "attack", finalMode: "a", hit: "hit_Fa", facing: 0 },
    { combo: "DLA", direction: "left", directionMode: "L", final: "attack", finalMode: "a", hit: "hit_Fa", facing: 1 },
    { combo: "DUA", direction: "up", directionMode: "U", final: "attack", finalMode: "a", hit: "hit_Ua", facing: null },
    { combo: "DDA", direction: "down", directionMode: "D", final: "attack", finalMode: "a", hit: "hit_Da", facing: null },
    { combo: "DRJ", direction: "right", directionMode: "R", final: "jump", finalMode: "j", hit: "hit_Fj", facing: 0 },
    { combo: "DLJ", direction: "left", directionMode: "L", final: "jump", finalMode: "j", hit: "hit_Fj", facing: 1 },
    { combo: "DUJ", direction: "up", directionMode: "U", final: "jump", finalMode: "j", hit: "hit_Uj", facing: null },
    { combo: "DDJ", direction: "down", directionMode: "D", final: "jump", finalMode: "j", hit: "hit_Dj", facing: null },
] as const);

type CooldownKey = keyof SimInputCooldowns;
type ComboKey = keyof SimInputCombos;
type InterruptMode = "U" | "D" | "L" | "R" | "d" | "j" | "a";

export interface FrameJumpResult {
    readonly entity: SimEntity;
    readonly success: boolean;
    readonly event: InputJumpEvent;
    readonly ruleIds: readonly string[];
}

export interface PostCooldownInputResult {
    readonly entity: SimEntity;
    readonly events: readonly InputJumpEvent[];
    readonly ruleIds: readonly string[];
}

const EMPTY_CURRENT_FRAME = Object.freeze({
    id: 0,
    state: 0,
    wait: 0,
    next: 0,
    mp: 0,
    hit_a: 0,
    hit_d: 0,
    hit_j: 0,
    hit_Fa: 0,
    hit_Ua: 0,
    hit_Da: 0,
    hit_Fj: 0,
    hit_Uj: 0,
    hit_Dj: 0,
    hit_ja: 0,
});

function authoredFrameDefinition(entity: SimEntity, frameId: number): SimFrameDefinition | undefined {
    if (frameId < 0 || frameId >= 600) {
        return undefined;
    }
    return entity.frames.find((definition) => definition.id === frameId);
}

function currentFrameDefinition(entity: SimEntity): SimFrameDefinition | undefined {
    if (entity.frame < 0 || entity.frame >= 600) {
        return undefined;
    }
    return authoredFrameDefinition(entity, entity.frame)
        ?? Object.freeze({ ...EMPTY_CURRENT_FRAME, id: entity.frame });
}

function replaceEntity(entity: SimEntity, changes: Partial<SimEntity>): SimEntity {
    return Object.freeze({ ...entity, ...changes });
}

function replaceCooldown(
    entity: SimEntity,
    key: CooldownKey,
    value: number,
): SimEntity {
    return replaceEntity(entity, { cooldowns: Object.freeze({ ...entity.cooldowns, [key]: value }) });
}

function replaceCombo(entity: SimEntity, key: ComboKey, value: number): SimEntity {
    return replaceEntity(entity, { combos: Object.freeze({ ...entity.combos, [key]: value }) });
}

function clearCooldowns(): SimInputCooldowns {
    return Object.freeze({ right: 0, left: 0, up: 0, down: 0, attack: 0, jump: 0, defend: 0 });
}

function jumpEvent(
    entity: SimEntity,
    trigger: string,
    outcome: InputJumpEvent["outcome"],
    reason: InputJumpFailureReason | null,
    rawTarget: number,
    resolvedTarget: number,
    toFrame: number,
    ruleId: string,
): InputJumpEvent {
    return Object.freeze({
        stableId: entity.stableId,
        slot: entity.slot,
        trigger,
        outcome,
        reason,
        fromFrame: entity.frame,
        toFrame,
        rawTarget,
        resolvedTarget,
        ruleId,
    });
}

function safeIntegerResult(value: number, label: string): number {
    if (!Number.isSafeInteger(value)) {
        throw new RangeError(`${label} must remain a safe integer`);
    }
    return value;
}

export function doFrameJump(
    entity: SimEntity,
    rawTarget: number,
    world: SimWorldInputState,
    trigger: string,
): FrameJumpResult {
    let resolvedTarget = rawTarget;
    let flip = false;
    if (resolvedTarget < 0) {
        resolvedTarget = -resolvedTarget;
        flip = true;
    }
    if (resolvedTarget === 999) {
        resolvedTarget = 0;
    }

    const definition = authoredFrameDefinition(entity, resolvedTarget);
    if (definition === undefined) {
        const event = jumpEvent(
            entity, trigger, "failure", "undefined-frame", rawTarget, resolvedTarget,
            entity.frame, GATE3A_INPUT_RULE.jumpDefinedTarget,
        );
        return Object.freeze({
            entity,
            success: false,
            event,
            ruleIds: Object.freeze([GATE3A_INPUT_RULE.jumpDefinedTarget]),
        });
    }

    const ppModeEnabled = world.ppMode !== 0;
    const mp = definition.mp ?? 0;
    const ppCost = mp % 1000;
    const hpCost = Math.trunc(mp / 1000) * 10;
    if (ppModeEnabled && entity.pp < ppCost) {
        const event = jumpEvent(
            entity, trigger, "failure", "insufficient-pp", rawTarget, resolvedTarget,
            entity.frame, GATE3A_INPUT_RULE.jumpResourceCost,
        );
        return Object.freeze({
            entity,
            success: false,
            event,
            ruleIds: Object.freeze([GATE3A_INPUT_RULE.jumpDefinedTarget, GATE3A_INPUT_RULE.jumpResourceCost]),
        });
    }
    if (ppModeEnabled && entity.hp <= hpCost) {
        const event = jumpEvent(
            entity, trigger, "failure", "insufficient-hp", rawTarget, resolvedTarget,
            entity.frame, GATE3A_INPUT_RULE.jumpResourceCost,
        );
        return Object.freeze({
            entity,
            success: false,
            event,
            ruleIds: Object.freeze([GATE3A_INPUT_RULE.jumpDefinedTarget, GATE3A_INPUT_RULE.jumpResourceCost]),
        });
    }

    const facing = flip && ppModeEnabled ? (entity.facing === 0 ? 1 : 0) : entity.facing;
    const hp = ppModeEnabled ? safeIntegerResult(entity.hp - hpCost, "entity.hp") : entity.hp;
    const pp = ppModeEnabled ? safeIntegerResult(entity.pp - ppCost, "entity.pp") : entity.pp;
    const comboCountVic = ppModeEnabled
        ? safeIntegerResult(entity.comboCountVic + hpCost, "entity.comboCountVic")
        : entity.comboCountVic;
    const ppDisplay = ppModeEnabled
        ? safeIntegerResult(entity.ppDisplay + ppCost, "entity.ppDisplay")
        : entity.ppDisplay;
    const current = replaceEntity(entity, {
        frame: resolvedTarget,
        facing,
        hp,
        pp,
        comboCountVic,
        ppDisplay,
        cooldowns: clearCooldowns(),
    });
    const event = jumpEvent(
        entity, trigger, "jump", null, rawTarget, resolvedTarget,
        resolvedTarget, GATE3A_INPUT_RULE.jumpSuccessEffects,
    );
    return Object.freeze({
        entity: current,
        success: true,
        event,
        ruleIds: Object.freeze([
            GATE3A_INPUT_RULE.jumpDefinedTarget,
            ...(ppModeEnabled ? [GATE3A_INPUT_RULE.jumpResourceCost] : []),
            GATE3A_INPUT_RULE.jumpSuccessEffects,
        ]),
    });
}

function comboInterrupt(
    cooldowns: SimInputCooldowns,
    mode: InterruptMode,
    advancedThisWrapper: boolean,
): boolean {
    if (!advancedThisWrapper) {
        return Object.values(cooldowns).some((cooldown) => cooldown === 5);
    }
    const excluded: Readonly<Record<InterruptMode, CooldownKey>> = {
        U: "up",
        D: "down",
        L: "left",
        R: "right",
        d: "defend",
        j: "jump",
        a: "attack",
    };
    return (Object.keys(cooldowns) as CooldownKey[])
        .some((key) => key !== excluded[mode] && cooldowns[key] === 5);
}

function advanceCombo(
    entity: SimEntity,
    combo: ComboKey,
    step2: CooldownKey,
    step2Mode: InterruptMode,
    step3: CooldownKey,
): { readonly entity: SimEntity; readonly advanced: boolean; readonly changed: boolean } {
    let current = entity;
    let comboState = current.combos[combo];
    let advanced = false;
    const initialState = comboState;
    if (comboState === 0 && current.cooldowns.defend === 5) {
        comboState = 1;
        advanced = true;
    }
    if (comboState === 1) {
        if (current.cooldowns[step2] === 5) {
            comboState = 2;
            advanced = true;
        } else if (comboInterrupt(current.cooldowns, "d", advanced)) {
            comboState = 0;
        }
    }
    if (comboState === 2) {
        if (current.cooldowns[step3] === 5) {
            comboState = 3;
            advanced = true;
        } else if (comboInterrupt(current.cooldowns, step2Mode, advanced)) {
            comboState = 0;
        }
    }
    if (comboState !== initialState) {
        current = replaceCombo(current, combo, comboState);
    }
    return Object.freeze({ entity: current, advanced, changed: comboState !== initialState });
}

function uniqueRuleIds(ruleIds: readonly string[]): readonly string[] {
    return Object.freeze([...new Set(ruleIds)]);
}

export function postCooldownInput(entity: SimEntity, world: SimWorldInputState): PostCooldownInputResult {
    if (!entity.active || entity.rawObjectType !== 0) {
        return Object.freeze({ entity, events: Object.freeze([]), ruleIds: Object.freeze([]) });
    }
    if (currentFrameDefinition(entity) === undefined) {
        return Object.freeze({ entity, events: Object.freeze([]), ruleIds: Object.freeze([]) });
    }

    let current = entity;
    const events: InputJumpEvent[] = [];
    const ruleIds: string[] = [];
    for (const wrapper of ORDINARY_COMBOS) {
        const definition = currentFrameDefinition(current);
        if (definition === undefined) {
            continue;
        }
        const advanced = advanceCombo(
            current,
            wrapper.combo,
            wrapper.direction,
            wrapper.directionMode,
            wrapper.final,
        );
        current = advanced.entity;
        if (advanced.changed) {
            ruleIds.push(GATE3A_INPUT_RULE.comboWrapperOrder);
        }
        if (current.combos[wrapper.combo] !== 3) {
            continue;
        }

        const target = definition[wrapper.hit] ?? 0;
        if (target !== 0 && current.linkState !== 2) {
            const jumped = doFrameJump(current, target, world, wrapper.combo);
            current = jumped.entity;
            events.push(jumped.event);
            ruleIds.push(...jumped.ruleIds, GATE3A_INPUT_RULE.comboWrapperOrder, GATE3A_INPUT_RULE.comboCallerSideEffects);
            if (wrapper.facing !== null) {
                current = replaceEntity(current, { facing: wrapper.facing });
            }
            current = replaceCombo(current, wrapper.combo, 0);
            continue;
        }
        if (comboInterrupt(current.cooldowns, wrapper.finalMode, advanced.advanced)) {
            current = replaceCombo(current, wrapper.combo, 0);
            ruleIds.push(GATE3A_INPUT_RULE.comboCallerSideEffects);
        }
    }

    const djaDefinition = currentFrameDefinition(current);
    const djaAdvanced = advanceCombo(current, "DJA", "jump", "j", "attack");
    current = djaAdvanced.entity;
    if (djaAdvanced.changed) {
        ruleIds.push(GATE3A_INPUT_RULE.djaSpecialCases);
    }
    if (djaDefinition !== undefined && current.combos.DJA === 3) {
        const target = djaDefinition.hit_ja ?? 0;
        const guarded = current.oid === 6 && target === 300 && current.hp > 177 && world.oid6DjaGuard === 0;
        if (guarded) {
            ruleIds.push(GATE3A_INPUT_RULE.djaSpecialCases);
        } else if (target !== 0 && current.unk324 === -1 && current.linkState !== 2) {
            const jumped = doFrameJump(current, target, world, "DJA");
            current = replaceCombo(jumped.entity, "DJA", 0);
            events.push(jumped.event);
            ruleIds.push(...jumped.ruleIds, GATE3A_INPUT_RULE.djaSpecialCases, GATE3A_INPUT_RULE.comboCallerSideEffects);
        } else if (current.unk328 === 1) {
            current = replaceEntity(current, { unk338: 0 });
            ruleIds.push(GATE3A_INPUT_RULE.djaSpecialCases);
        } else if (comboInterrupt(current.cooldowns, "a", djaAdvanced.advanced)) {
            current = replaceCombo(current, "DJA", 0);
            ruleIds.push(GATE3A_INPUT_RULE.djaSpecialCases);
        }
    }

    const directDefinition = currentFrameDefinition(current);
    if (directDefinition !== undefined) {
        let target = 0;
        let cooldown: CooldownKey | null = null;
        let trigger = "";
        if ((directDefinition.hit_a ?? 0) !== 0
            && current.cooldowns.attack > current.cooldowns.defend
            && current.cooldowns.attack > current.cooldowns.jump) {
            target = directDefinition.hit_a ?? 0;
            cooldown = "attack";
            trigger = "hit_a";
        } else if ((directDefinition.hit_d ?? 0) !== 0
            && current.cooldowns.defend > current.cooldowns.attack
            && current.cooldowns.defend > current.cooldowns.jump) {
            target = directDefinition.hit_d ?? 0;
            cooldown = "defend";
            trigger = "hit_d";
        } else if ((directDefinition.hit_j ?? 0) !== 0
            && current.cooldowns.jump > current.cooldowns.attack
            && current.cooldowns.jump > current.cooldowns.defend) {
            target = directDefinition.hit_j ?? 0;
            cooldown = "jump";
            trigger = "hit_j";
        }
        if (cooldown !== null) {
            const jumped = doFrameJump(current, target, world, trigger);
            current = replaceCooldown(jumped.entity, cooldown, 0);
            events.push(jumped.event);
            ruleIds.push(...jumped.ruleIds, GATE3A_INPUT_RULE.directStrictMaximum, GATE3A_INPUT_RULE.comboCallerSideEffects);
        }
    }

    if (events.length > 0 || ruleIds.length > 0) {
        ruleIds.push(GATE3A_INPUT_RULE.eligibleCharacterDat);
    }
    return Object.freeze({
        entity: current,
        events: Object.freeze(events),
        ruleIds: uniqueRuleIds(ruleIds),
    });
}
