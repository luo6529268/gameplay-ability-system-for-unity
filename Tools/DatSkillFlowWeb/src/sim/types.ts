export type SimJsonPrimitive = string | number | boolean | null;
export type SimJsonValue = SimJsonPrimitive | readonly SimJsonValue[] | SimJsonObject;
export interface SimJsonObject {
    readonly [key: string]: SimJsonValue;
}

export interface SimFrameDefinition {
    readonly id: number;
    readonly state: number;
    readonly wait: number;
    readonly next: number;
    readonly dvx?: number;
    readonly dvy?: number;
    readonly dvz?: number;
    readonly cpoints?: readonly SimCpointDefinition[];
    readonly centerx?: number;
    readonly centery?: number;
    readonly opoints?: readonly SimOpointDefinition[];
    readonly wpoints?: readonly SimWpointDefinition[];
    readonly itrs?: readonly SimItrDefinition[];
    readonly mp?: number;
    readonly hit_a?: number;
    readonly hit_d?: number;
    readonly hit_j?: number;
    readonly hit_Fa?: number;
    readonly hit_Ua?: number;
    readonly hit_Da?: number;
    readonly hit_Fj?: number;
    readonly hit_Uj?: number;
    readonly hit_Dj?: number;
    readonly hit_ja?: number;
}

export interface SimCpointDefinition { readonly kind: number; }

export interface SimWpointDefinition {
    readonly kind: number;
    readonly x: number;
    readonly y: number;
    readonly attacking: number;
    readonly cover: number;
    readonly weaponact: number;
    readonly dvx: number;
    readonly dvy: number;
    readonly dvz: number;
}

export interface SimItrDefinition {
    readonly kind: number;
    readonly x: number;
    readonly y: number;
    readonly w: number;
    readonly h: number;
    readonly dvx: number;
    readonly dvy: number;
    readonly fall: number;
    readonly bdefend: number;
    readonly injury: number;
    readonly arest: number;
    readonly vrest: number;
    readonly effect: number;
    readonly attacking: number;
    readonly catchingact: number;
    readonly catchingact2: number;
    readonly caughtact: number;
    readonly caughtact2: number;
    readonly respond: number;
    readonly pickingact: number;
    readonly pickedact: number;
    readonly throwvx: number;
    readonly throwvy: number;
    readonly zwidth: number;
    readonly throwvz: number;
    readonly throwinjury: number;
}

export interface SimOpointDefinition {
    readonly kind: number;
    readonly x: number;
    readonly y: number;
    readonly action: number;
    readonly dvx: number;
    readonly dvy: number;
    readonly oid: number;
    readonly facing: number;
}

export interface SimDatDefinition {
    readonly oid: number;
    readonly rawObjectType: number;
    readonly weaponHp: number;
    readonly frameSourceIndex: number;
    readonly jumpHeight: number;
    readonly jumpDistance: number;
    readonly jumpDistanceZ: number;
    readonly frames: readonly SimFrameDefinition[];
}

export interface SimDatSeed {
    readonly oid: number;
    readonly rawObjectType: number;
    readonly weaponHp?: number;
    readonly jumpHeight?: number;
    readonly jumpDistance?: number;
    readonly jumpDistanceZ?: number;
    readonly frames: readonly SimFrameDefinition[];
}

export interface SimInputCooldowns {
    readonly right: number;
    readonly left: number;
    readonly up: number;
    readonly down: number;
    readonly attack: number;
    readonly jump: number;
    readonly defend: number;
}

export interface SimInputCombos {
    readonly DRA: number;
    readonly DLA: number;
    readonly DUA: number;
    readonly DDA: number;
    readonly DRJ: number;
    readonly DLJ: number;
    readonly DUJ: number;
    readonly DDJ: number;
    readonly DJA: number;
}

export interface SimWorldInputState {
    readonly ppMode: number;
    readonly oid6DjaGuard: number;
}

export interface SimEntitySeed {
    readonly stableId: string;
    readonly slot: number;
    readonly rawObjectType: number;
    readonly oid?: number;
    readonly frame: number;
    readonly hp?: number;
    readonly hpMax?: number;
    readonly hp3?: number;
    readonly pp?: number;
    readonly comboCountVic?: number;
    readonly ppDisplay?: number;
    readonly waitCounter?: number;
    readonly attacking?: number;
    readonly facing?: 0 | 1;
    readonly x?: number;
    readonly y?: number;
    readonly z?: number;
    readonly xInt?: number;
    readonly yInt?: number;
    readonly zInt?: number;
    readonly vx?: number;
    readonly vy?: number;
    readonly vz?: number;
    readonly team?: number;
    readonly ownerId?: number;
    readonly holderIdx?: number;
    readonly holderCopy?: number;
    readonly spawnerSlot?: number;
    readonly targetIdx?: number;
    readonly heldWeaponSlot?: number;
    readonly prevFrame2?: number;
    readonly hitCount?: number;
    readonly knockbackVx?: number;
    readonly knockbackVy?: number;
    readonly knockbackVz?: number;
    readonly throwFrameGuard?: number;
    readonly pickupCount?: number;
    readonly catcherIdx?: number;
    readonly caughtIdx?: number;
    readonly caughtDuration?: number;
    readonly fall?: number;
    readonly unk31C?: number;
    readonly runtimeObjectType?: number;
    readonly entityType?: number;
    readonly weaponHp?: number;
    readonly aiControlled?: boolean;
    readonly keyUp?: boolean;
    readonly keyDown?: boolean;
    readonly keyLeft?: boolean;
    readonly keyRight?: boolean;
    readonly blockBackZ?: boolean;
    readonly blockForwardZ?: boolean;
    readonly blockLeft?: boolean;
    readonly blockRight?: boolean;
    readonly unk364?: number;
    readonly unk32C?: number;
    readonly unk33C?: number;
    readonly animCounter?: number;
    readonly attackExempt?: number;
    readonly hitStop?: number;
    readonly frameDelay?: number;
    readonly killCount?: number;
    readonly cooldowns?: Partial<SimInputCooldowns>;
    readonly combos?: Partial<SimInputCombos>;
    readonly linkState?: number;
    readonly unk324?: number;
    readonly unk328?: number;
    readonly unk338?: number;
    readonly active?: boolean;
    readonly frames: readonly SimFrameDefinition[];
}

export interface SimEntity {
    readonly stableId: string;
    readonly slot: number;
    readonly rawObjectType: number;
    readonly oid: number;
    readonly frame: number;
    readonly hp: number;
    readonly hpMax: number;
    readonly hp3: number;
    readonly pp: number;
    readonly comboCountVic: number;
    readonly ppDisplay: number;
    readonly waitCounter: number;
    readonly attacking: number;
    readonly facing: 0 | 1;
    readonly x: number;
    readonly y: number;
    readonly z: number;
    readonly xInt: number;
    readonly yInt: number;
    readonly zInt: number;
    readonly vx: number;
    readonly vy: number;
    readonly vz: number;
    readonly team: number;
    readonly ownerId: number;
    readonly holderIdx: number;
    readonly holderCopy: number;
    readonly spawnerSlot: number;
    readonly targetIdx: number;
    readonly heldWeaponSlot: number;
    readonly prevFrame2: number;
    readonly hitCount: number;
    readonly knockbackVx: number;
    readonly knockbackVy: number;
    readonly knockbackVz: number;
    readonly throwFrameGuard: number;
    readonly pickupCount: number;
    readonly catcherIdx: number;
    readonly caughtIdx: number;
    readonly caughtDuration: number;
    readonly fall: number;
    readonly unk31C: number;
    readonly runtimeObjectType: number;
    readonly entityType: number;
    readonly weaponHp: number;
    readonly aiControlled: boolean;
    readonly keyUp: boolean;
    readonly keyDown: boolean;
    readonly keyLeft: boolean;
    readonly keyRight: boolean;
    readonly blockBackZ: boolean;
    readonly blockForwardZ: boolean;
    readonly blockLeft: boolean;
    readonly blockRight: boolean;
    readonly unk364: number;
    readonly unk32C: number;
    readonly unk33C: number;
    readonly animCounter: number;
    readonly attackExempt: number;
    readonly hitStop: number;
    readonly frameDelay: number;
    readonly killCount: number;
    readonly cooldowns: SimInputCooldowns;
    readonly combos: SimInputCombos;
    readonly linkState: number;
    readonly unk324: number;
    readonly unk328: number;
    readonly unk338: number;
    readonly active: boolean;
    readonly frames: readonly SimFrameDefinition[];
    readonly frameSourceIndex: number;
}

export interface SimulationState {
    readonly tickIndex: number;
    readonly timeMs: number;
    readonly objectCount: number;
    readonly worldInput: SimWorldInputState;
    readonly slots: readonly (SimEntity | null)[];
    readonly entities: readonly SimEntity[];
    readonly catalog: readonly (SimDatDefinition | null)[];
    readonly frameSources: readonly (readonly SimFrameDefinition[])[];
    readonly attackRest: readonly number[];
    readonly vrest: readonly SimVrestEntry[];
    readonly nextSpawnOrdinal: number;
    readonly rngSeed: number;
}

export interface SimVrestEntry {
    readonly fromSlot: number;
    readonly toSlot: number;
    readonly ticks: number;
}

export interface CreateSimulationOptions {
    readonly entities: readonly SimEntitySeed[];
    readonly catalog?: readonly SimDatSeed[];
    readonly attackRest?: readonly number[];
    readonly vrest?: readonly SimVrestEntry[];
    readonly onFrameSourceCanonicalize?: () => void;
    readonly tickIndex?: number;
    readonly rngSeed?: number;
    readonly worldInput?: Partial<SimWorldInputState>;
}

export interface SimPickupInput extends SimJsonObject {
    readonly kind: 2 | 7;
    readonly pickerSlot: number;
    readonly weaponSlot: number;
}

export type SimulationInput = SimJsonObject & {
    readonly pickups?: readonly SimPickupInput[];
};

export type FrameTransitionKind =
    | "hold"
    | "self"
    | "standard"
    | "negative"
    | "sentinel-999"
    | "state0-airborne";

export interface FrameTransitionEvent {
    readonly stableId: string;
    readonly slot: number;
    readonly kind: FrameTransitionKind;
    readonly fromFrame: number;
    readonly toFrame: number;
    readonly rawNext: number | null;
}

export interface CollisionEvent {
    readonly stableId: string;
    readonly slot: number;
    readonly frame: number;
    readonly detail: SimJsonObject | null;
}

export interface LifecycleEvent {
    readonly stableId: string;
    readonly slot: number;
    readonly kind: "frame-group-reset" | "free";
    readonly frame: number;
    readonly childStableIds: readonly string[];
}

export interface SlotLifecycleEvent {
    readonly slot: number;
    readonly kind: "allocate" | "release";
    readonly stableId: string;
}

export interface OpointSpawnEvent {
    readonly stableId: string;
    readonly slot: number;
    readonly parentStableId: string;
    readonly parentSlot: number;
    readonly oid: number;
    readonly action: number;
    readonly kind: number;
    readonly facing: 0 | 1;
    readonly generation: number;
    readonly ordinal: number;
    readonly ruleId: string;
}

export interface HeldObjectEvent {
    readonly kind: "pickup" | "sync" | "release" | "link-validation";
    readonly holderStableId: string;
    readonly holderSlot: number;
    readonly heldStableId: string;
    readonly heldSlot: number;
    readonly pass: 5 | 12 | null;
    readonly ruleId: string;
    readonly detail: string;
}

export type InputJumpOutcome = "jump" | "failure";
export type InputJumpFailureReason = "undefined-frame" | "insufficient-pp" | "insufficient-hp";

export interface InputJumpEvent {
    readonly stableId: string;
    readonly slot: number;
    readonly trigger: string;
    readonly outcome: InputJumpOutcome;
    readonly reason: InputJumpFailureReason | null;
    readonly fromFrame: number;
    readonly toFrame: number;
    readonly rawTarget: number;
    readonly resolvedTarget: number;
    readonly ruleId: string;
}

export interface SimulationTickTrace {
    readonly schemaVersion: 1;
    readonly tickIndex: number;
    readonly timeMs: number;
    readonly inputs: SimulationInput;
    readonly frameTransitions: readonly FrameTransitionEvent[];
    readonly collisions: readonly CollisionEvent[];
    readonly lifecycle: readonly LifecycleEvent[];
    readonly slotLifecycle: readonly SlotLifecycleEvent[];
    readonly spawns: readonly OpointSpawnEvent[];
    readonly inputJumps: readonly InputJumpEvent[];
    readonly heldObjects: readonly HeldObjectEvent[];
    readonly ruleIds: readonly string[];
    readonly snapshotDigest: string;
}

export interface CollisionContext {
    readonly tickIndex: number;
    readonly timeMs: number;
    readonly input: SimulationInput;
}

export type CollisionCallback = (
    entity: SimEntity,
    context: CollisionContext,
) => SimJsonObject | null | undefined;

export interface SimulationRuntime {
    readonly collision?: CollisionCallback;
    readonly onOpointAllocationAttempt?: () => void;
    readonly onOpointVrestOperation?: (kind: OpointVrestOperationKind) => void;
}

export type OpointVrestOperationKind = "reset-row" | "reset-column" | "set" | "materialize";

export interface SimulationStepResult {
    readonly state: SimulationState;
    readonly trace: SimulationTickTrace;
}

export interface SimulationReplayResult {
    readonly state: SimulationState;
    readonly traces: readonly SimulationTickTrace[];
}
