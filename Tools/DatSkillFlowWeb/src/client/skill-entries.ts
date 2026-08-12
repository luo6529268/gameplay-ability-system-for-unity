import type { DatFrameProjection } from "../model/dat-projection.js";

export const SKILL_ENTRY_HIT_KEYS = Object.freeze([
    "hit_a", "hit_d", "hit_j", "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj", "hit_ja",
] as const);

export type SkillEntryHitKey = typeof SKILL_ENTRY_HIT_KEYS[number];
export type SkillEntryCategory = "base" | "input" | "engine";
export type FrameCatalogRole =
    | "base-entry"
    | "input-entry"
    | "engine-entry"
    | "runtime-branch"
    | "internal"
    | "unresolved"
    | "overridden";

export interface FrameCatalogReference {
    readonly sourceFrame: number;
    readonly sourceOccurrence: number;
    readonly field: string;
    readonly kind: "next" | "input" | "runtime";
}

export interface FrameCatalogItem {
    readonly frame: DatFrameProjection;
    readonly effective: boolean;
    readonly effectiveOccurrence: number;
    readonly definitionCount: number;
    readonly role: FrameCatalogRole;
    readonly roleLabel: string;
    readonly ownerStartFrames: readonly number[];
    readonly references: readonly FrameCatalogReference[];
}

export interface FrameEntryCatalog {
    readonly entries: readonly SkillEntry[];
    readonly baseContexts: readonly BaseStateContext[];
    readonly frames: readonly FrameCatalogItem[];
    readonly byOccurrence: ReadonlyMap<number, FrameCatalogItem>;
}

export interface SkillDisplayMetadata {
    readonly oid: number;
    readonly startFrame: number;
    readonly displayName?: string;
    readonly group?: string;
    readonly order?: number;
    readonly pinned?: boolean;
    readonly hidden?: boolean;
    readonly notes?: string;
}

export interface SkillEntryTrigger {
    readonly key: SkillEntryHitKey;
    readonly sourceFrames: readonly number[];
}

export type SkillEntryActionRole = "context" | "root" | "internal";

export interface SkillEntryRoute {
    readonly key: SkillEntryHitKey;
    readonly sourceFrame: number;
    readonly sourceOccurrence: number;
    readonly sourceLabel: string;
    readonly sourceState: number;
    readonly sourceKind: "base" | "action";
}

export interface SkillEntryInternalStage {
    readonly startFrame: number;
    readonly label: string;
    readonly triggers: readonly SkillEntryTrigger[];
}

export interface BaseStateContext {
    readonly id: string;
    readonly state: 0 | 1 | 2;
    readonly label: string;
    readonly primaryStartFrame: number;
    readonly variantStartFrames: readonly number[];
    readonly frameCount: number;
    readonly actionStartFrames: readonly number[];
    readonly routeCount: number;
}

export interface SkillEntry {
    readonly id: string;
    readonly oid: number;
    readonly startFrame: number;
    readonly startOccurrence: number;
    readonly label: string;
    readonly displayName: string;
    readonly category: SkillEntryCategory;
    readonly group: string;
    readonly order: number;
    readonly pinned: boolean;
    readonly hidden: boolean;
    readonly notes: string;
    readonly segmentFrameCount: number;
    readonly baseState?: 0 | 1 | 2;
    readonly actionRole: SkillEntryActionRole;
    readonly triggers: readonly SkillEntryTrigger[];
    readonly routes: readonly SkillEntryRoute[];
    readonly parentStartFrames: readonly number[];
    readonly internalStages: readonly SkillEntryInternalStage[];
    readonly nativeTrigger?: string;
    readonly nativeInputPlan?: readonly SkillPreviewInputStep[];
}

export type SkillPreviewInputKey = "A" | "D" | "W" | "S" | "J" | "K" | "L";

export interface SkillPreviewInputStep {
    readonly tick: number;
    readonly keys: readonly SkillPreviewInputKey[];
}

export interface SkillPreviewScenario {
    readonly startFrame: number;
    readonly initialFrame: number;
    readonly inputPlan: readonly SkillPreviewInputStep[];
    readonly ticks: number;
}

export interface SkillPreviewRootTick {
    readonly tick: number;
    readonly frame?: number;
}

export interface SkillInternalStageScenario {
    readonly scenario: SkillPreviewScenario;
    readonly route: SkillEntryRoute;
    readonly triggerTick: number;
}

interface FrameSegment {
    readonly label: string;
    readonly frames: DatFrameProjection[];
}

export function latestRuntimeFrameMap(
    frames: readonly DatFrameProjection[],
): ReadonlyMap<number, DatFrameProjection> {
    const frameById = new Map<number, DatFrameProjection>();
    for (const frame of frames) {
        if (Number.isSafeInteger(frame.frameId) && frame.frameId >= 0 && frame.frameId < 600) {
            frameById.set(frame.frameId, frame);
        }
    }
    return frameById;
}

interface EntryCandidate {
    readonly frame: DatFrameProjection;
    segmentFrameCount: number;
    readonly triggerSources: Map<SkillEntryHitKey, Set<number>>;
    baseState?: 0 | 1 | 2;
    nativeCategory?: SkillEntryCategory;
    nativeTrigger?: string;
    nativeInputPlan?: readonly SkillPreviewInputStep[];
}

const DEFAULT_GROUPS: Readonly<Record<SkillEntryCategory, string>> = Object.freeze({
    base: "基础状态",
    input: "DAT 输入入口",
    engine: "Native 输入入口",
});

const FRAME_ROLE_LABELS: Readonly<Record<FrameCatalogRole, string>> = Object.freeze({
    "base-entry": "基础状态入口",
    "input-entry": "DAT 输入入口",
    "engine-entry": "Native 输入入口",
    "runtime-branch": "交互/运行时分支",
    internal: "动作内部帧",
    unresolved: "未解析运行帧",
    overridden: "已被后续同号定义覆盖",
});

interface EngineEntryRule {
    readonly frameId: number;
    readonly category: "base" | "engine";
    readonly trigger: string;
    readonly inputPlan: readonly SkillPreviewInputStep[];
}

// These are character-wide transitions implemented by Native input_handler.cpp.
// They are not inferred from DAT labels and are only offered when the DAT
// actually defines the target frame. Context-dependent/random branches stay in
// the complete Frame catalog until their setup can be reproduced faithfully.
const holdInput = (
    key: SkillPreviewInputKey,
    fromTick: number,
    toTick: number,
): readonly SkillPreviewInputStep[] => Object.freeze(Array.from(
    { length: toTick - fromTick + 1 },
    (_, index) => Object.freeze({ tick: fromTick + index, keys: Object.freeze([key]) }),
));
const runInputPrefix = Object.freeze([
    Object.freeze({ tick: 2, keys: Object.freeze(["D"] as const) }),
    ...holdInput("D", 4, 8),
]);

const ENGINE_ENTRY_RULES: readonly EngineEntryRule[] = Object.freeze([
    Object.freeze({
        frameId: 5,
        category: "base",
        trigger: "Native：按住 D 行走",
        inputPlan: holdInput("D", 2, 32),
    }),
    Object.freeze({
        frameId: 9,
        category: "base",
        trigger: "Native：双击并按住 D 跑动",
        inputPlan: Object.freeze([
            Object.freeze({ tick: 2, keys: Object.freeze(["D"] as const) }),
            ...holdInput("D", 4, 32),
        ]),
    }),
    Object.freeze({
        frameId: 90,
        category: "engine",
        trigger: "Native：跑动跳跃后 J 冲刺攻击",
        inputPlan: Object.freeze([
            ...runInputPrefix,
            Object.freeze({ tick: 9, keys: Object.freeze(["D", "K"] as const) }),
            Object.freeze({ tick: 12, keys: Object.freeze(["D", "J"] as const) }),
        ]),
    }),
    Object.freeze({
        frameId: 102,
        category: "engine",
        trigger: "Native：跑动中 L 防御",
        inputPlan: Object.freeze([
            ...runInputPrefix,
            Object.freeze({ tick: 9, keys: Object.freeze(["D", "L"] as const) }),
        ]),
    }),
    Object.freeze({
        frameId: 110,
        category: "engine",
        trigger: "Native：L 防御",
        inputPlan: Object.freeze([Object.freeze({ tick: 2, keys: Object.freeze(["L"] as const) })]),
    }),
    Object.freeze({
        frameId: 210,
        category: "engine",
        trigger: "Native：K 跳跃",
        inputPlan: Object.freeze([Object.freeze({ tick: 2, keys: Object.freeze(["K"] as const) })]),
    }),
    Object.freeze({
        frameId: 213,
        category: "engine",
        trigger: "Native：跑动中 K 冲刺跳跃",
        inputPlan: Object.freeze([
            ...runInputPrefix,
            Object.freeze({ tick: 9, keys: Object.freeze(["D", "K"] as const) }),
        ]),
    }),
]);

const RUNTIME_ITR_FRAME_FIELDS = Object.freeze([
    "catchingact", "catchingact2", "caughtact", "caughtact2", "pickingact", "pickedact",
] as const);
const RUNTIME_CPOINT_FRAME_FIELDS = Object.freeze([
    "vaction", "aaction", "jaction", "daction", "taction", "fronthurtact", "backhurtact",
] as const);

function latestRuntimeFrames(frames: readonly DatFrameProjection[]): DatFrameProjection[] {
    const latestById = latestRuntimeFrameMap(frames);
    return frames.filter((frame) => latestById.get(frame.frameId) === frame);
}

const HIT_KEY_INPUTS: Readonly<Record<SkillEntryHitKey, readonly SkillPreviewInputKey[]>> = Object.freeze({
    hit_a: Object.freeze(["J"]),
    hit_d: Object.freeze(["L"]),
    hit_j: Object.freeze(["K"]),
    hit_Fa: Object.freeze(["L", "D", "J"]),
    hit_Fj: Object.freeze(["L", "D", "K"]),
    hit_Ua: Object.freeze(["L", "W", "J"]),
    hit_Uj: Object.freeze(["L", "W", "K"]),
    hit_Da: Object.freeze(["L", "S", "J"]),
    hit_Dj: Object.freeze(["L", "S", "K"]),
    hit_ja: Object.freeze(["L", "K", "J"]),
});

function idlePreviewFrame(frames: readonly DatFrameProjection[]): number | undefined {
    const runtimeFrames = latestRuntimeFrames(frames);
    return runtimeFrames.find((frame) => frame.frameId === 0 && frame.state === 0)?.frameId
        ?? runtimeFrames.find((frame) => frame.state === 0)?.frameId;
}

export function buildSkillPreviewScenario(
    frames: readonly DatFrameProjection[],
    entry: SkillEntry,
): SkillPreviewScenario {
    const runtimeFrameIds = new Set(latestRuntimeFrames(frames).map((frame) => frame.frameId));
    const route = entry.routes.find((candidate) => (
        candidate.sourceKind === "base" && runtimeFrameIds.has(candidate.sourceFrame)
    )) ?? entry.routes.find((candidate) => runtimeFrameIds.has(candidate.sourceFrame));
    if (route !== undefined) {
        const inputPlan = HIT_KEY_INPUTS[route.key].map((key, index) => Object.freeze({
            tick: 2 + index * 2,
            keys: Object.freeze([key]),
        }));
        return Object.freeze({
            startFrame: entry.startFrame,
            initialFrame: route.sourceFrame,
            inputPlan: Object.freeze(inputPlan),
            ticks: 120,
        });
    }

    const idleFrame = entry.nativeInputPlan === undefined ? undefined : idlePreviewFrame(frames);
    if (idleFrame !== undefined && entry.nativeInputPlan !== undefined) {
        return Object.freeze({
            startFrame: entry.startFrame,
            initialFrame: idleFrame,
            inputPlan: entry.nativeInputPlan,
            ticks: 120,
        });
    }

    return Object.freeze({
        startFrame: entry.startFrame,
        initialFrame: entry.startFrame,
        inputPlan: Object.freeze([]),
        ticks: 120,
    });
}

function mergePreviewInputSteps(
    base: readonly SkillPreviewInputStep[],
    injected: readonly SkillPreviewInputStep[],
): readonly SkillPreviewInputStep[] {
    const keysByTick = new Map<number, SkillPreviewInputKey[]>();
    for (const step of [...base, ...injected]) {
        const keys = keysByTick.get(step.tick) ?? [];
        for (const key of step.keys) {
            if (!keys.includes(key)) keys.push(key);
        }
        keysByTick.set(step.tick, keys);
    }
    return Object.freeze([...keysByTick.entries()]
        .sort(([left], [right]) => left - right)
        .map(([tick, keys]) => Object.freeze({ tick, keys: Object.freeze(keys) })));
}

export function buildInternalStagePreviewScenario(
    parentScenario: SkillPreviewScenario,
    stage: SkillEntry,
    rootTicks: readonly SkillPreviewRootTick[],
): SkillInternalStageScenario | undefined {
    if (stage.actionRole !== "internal") return undefined;
    for (const route of stage.routes) {
        const sourceTick = rootTicks.find((tick) => tick.frame === route.sourceFrame);
        if (sourceTick === undefined) continue;
        const triggerTick = sourceTick.tick + 1;
        const injected = HIT_KEY_INPUTS[route.key].map((key, index) => Object.freeze({
            tick: triggerTick + index * 2,
            keys: Object.freeze([key]),
        }));
        if ((injected.at(-1)?.tick ?? triggerTick) > parentScenario.ticks) continue;
        return Object.freeze({
            scenario: Object.freeze({
                ...parentScenario,
                inputPlan: mergePreviewInputSteps(parentScenario.inputPlan, injected),
            }),
            route,
            triggerTick,
        });
    }
    return undefined;
}

export function authoredTraceStartFrame(
    _frames: readonly DatFrameProjection[],
    requestedFrameId: number,
): number {
    return requestedFrameId;
}

function labelKey(frame: DatFrameProjection): string {
    const label = frame.label.trim();
    return label === "" ? `\0${frame.frameId}` : label.toLocaleLowerCase("en-US");
}

function buildSegments(frames: readonly DatFrameProjection[]): FrameSegment[] {
    const result: FrameSegment[] = [];
    for (const frame of frames) {
        const previous = result[result.length - 1];
        const previousFrame = previous?.frames[previous.frames.length - 1];
        if (previous !== undefined
            && previousFrame !== undefined
            && labelKey(frame) === labelKey(previousFrame)
            && frame.frameId === previousFrame.frameId + 1) {
            previous.frames.push(frame);
        } else {
            result.push({ label: frame.label.trim(), frames: [frame] });
        }
    }
    return result;
}

function candidateFor(
    candidates: Map<number, EntryCandidate>,
    frame: DatFrameProjection,
    segmentFrameCount: number,
    baseState?: 0 | 1 | 2,
): EntryCandidate {
    const existing = candidates.get(frame.frameId);
    if (existing !== undefined) {
        existing.segmentFrameCount = Math.max(existing.segmentFrameCount, segmentFrameCount);
        if (baseState !== undefined) existing.baseState = baseState;
        return existing;
    }
    const candidate: EntryCandidate = {
        frame,
        segmentFrameCount,
        triggerSources: new Map(),
        baseState,
    };
    candidates.set(frame.frameId, candidate);
    return candidate;
}

function metadataFor(
    metadata: readonly SkillDisplayMetadata[],
    oid: number,
    startFrame: number,
): SkillDisplayMetadata | undefined {
    return metadata.find((entry) => entry.oid === oid && entry.startFrame === startFrame);
}

function categoryFor(candidate: EntryCandidate): SkillEntryCategory {
    if (candidate.baseState !== undefined) return "base";
    if (candidate.triggerSources.size > 0) return "input";
    return candidate.nativeCategory ?? "base";
}

function groupRank(group: string): number {
    const index = Object.values(DEFAULT_GROUPS).indexOf(group);
    return index < 0 ? Object.keys(DEFAULT_GROUPS).length : index;
}

interface IndexedReference extends FrameCatalogReference {
    readonly targetFrame: number;
}

function validRuntimeTarget(
    rawTarget: number,
    frameById: ReadonlyMap<number, DatFrameProjection>,
): rawTarget is number {
    return Number.isSafeInteger(rawTarget) && rawTarget > 0 && frameById.has(rawTarget);
}

function collectFrameReferences(
    runtimeFrames: readonly DatFrameProjection[],
    frameById: ReadonlyMap<number, DatFrameProjection>,
): readonly IndexedReference[] {
    const references: IndexedReference[] = [];
    const add = (
        source: DatFrameProjection,
        targetFrame: number,
        field: string,
        kind: IndexedReference["kind"],
    ): void => {
        if (!validRuntimeTarget(targetFrame, frameById)) return;
        references.push(Object.freeze({
            sourceFrame: source.frameId,
            sourceOccurrence: source.occurrence,
            targetFrame,
            field,
            kind,
        }));
    };
    for (const source of runtimeFrames) {
        add(source, source.next, "next", "next");
        for (const key of SKILL_ENTRY_HIT_KEYS) add(source, source[key], key, "input");
        source.itrs.forEach((itr, index) => {
            for (const field of RUNTIME_ITR_FRAME_FIELDS) {
                add(source, itr[field], `itr[${index}].${field}`, "runtime");
            }
        });
        source.cpoints.forEach((cpoint, index) => {
            for (const field of RUNTIME_CPOINT_FRAME_FIELDS) {
                add(source, cpoint[field], `cpoint[${index}].${field}`, "runtime");
            }
        });
    }
    return Object.freeze(references);
}

function addNextChainOwner(
    startFrame: number,
    ownerStartFrame: number,
    frameById: ReadonlyMap<number, DatFrameProjection>,
    ownersByFrame: Map<number, Set<number>>,
    branchFrames?: Set<number>,
): boolean {
    let current = startFrame;
    const visited = new Set<number>();
    let changed = false;
    while (!visited.has(current)) {
        visited.add(current);
        const frame = frameById.get(current);
        if (frame === undefined) break;
        const owners = ownersByFrame.get(current) ?? new Set<number>();
        if (!owners.has(ownerStartFrame)) {
            owners.add(ownerStartFrame);
            ownersByFrame.set(current, owners);
            changed = true;
        }
        branchFrames?.add(current);
        if (!validRuntimeTarget(frame.next, frameById)) break;
        current = frame.next;
    }
    return changed;
}

function buildEntries(
    frames: readonly DatFrameProjection[],
    oid: number,
    metadata: readonly SkillDisplayMetadata[] = [],
): readonly SkillEntry[] {
    const runtimeFrames = latestRuntimeFrames(frames);
    const frameById = new Map(runtimeFrames.map((frame) => [frame.frameId, frame]));
    const segments = buildSegments(runtimeFrames);
    const segmentByOccurrence = new Map<number, FrameSegment>();
    const candidates = new Map<number, EntryCandidate>();

    for (const segment of segments) {
        segment.frames.forEach((frame) => segmentByOccurrence.set(frame.occurrence, segment));
        const start = segment.frames[0]!;
        if (start.state === 0 || start.state === 1 || start.state === 2) {
            candidateFor(candidates, start, segment.frames.length, start.state);
        }
    }

    for (const source of runtimeFrames) {
        for (const key of SKILL_ENTRY_HIT_KEYS) {
            const rawTarget = source[key];
            if (rawTarget === 0) continue;
            const target = frameById.get(rawTarget);
            if (target === undefined) continue;
            const segment = segmentByOccurrence.get(target.occurrence);
            const targetIndex = segment?.frames.indexOf(target) ?? -1;
            const candidate = candidateFor(
                candidates,
                target,
                targetIndex < 0 ? 1 : segment!.frames.length - targetIndex,
            );
            const sources = candidate.triggerSources.get(key) ?? new Set<number>();
            sources.add(source.frameId);
            candidate.triggerSources.set(key, sources);
        }
    }

    for (const rule of ENGINE_ENTRY_RULES) {
        const target = frameById.get(rule.frameId);
        if (target === undefined) continue;
        const segment = segmentByOccurrence.get(target.occurrence);
        const targetIndex = segment?.frames.indexOf(target) ?? -1;
        const candidate = candidateFor(
            candidates,
            target,
            targetIndex < 0 ? 1 : segment!.frames.length - targetIndex,
        );
        if (candidate.triggerSources.size === 0) {
            candidate.nativeCategory = rule.category;
            candidate.nativeTrigger = rule.trigger;
            candidate.nativeInputPlan = rule.inputPlan;
        }
    }

    const entries = [...candidates.values()].map((candidate): SkillEntry => {
        const override = metadataFor(metadata, oid, candidate.frame.frameId);
        const category = categoryFor(candidate);
        const label = candidate.frame.label.trim() || `frame_${candidate.frame.frameId}`;
        const triggers = SKILL_ENTRY_HIT_KEYS.flatMap((key): SkillEntryTrigger[] => {
            const sources = candidate.triggerSources.get(key);
            return sources === undefined ? [] : [{
                key,
                sourceFrames: Object.freeze([...sources].sort((left, right) => left - right)),
            }];
        });
        const routes = triggers.flatMap((trigger): SkillEntryRoute[] => (
            trigger.sourceFrames.flatMap((sourceFrame): SkillEntryRoute[] => {
                const source = frameById.get(sourceFrame);
                if (source === undefined) return [];
                return [Object.freeze({
                    key: trigger.key,
                    sourceFrame,
                    sourceOccurrence: source.occurrence,
                    sourceLabel: source.label.trim() || `frame_${source.frameId}`,
                    sourceState: source.state,
                    sourceKind: source.state === 0 || source.state === 1 || source.state === 2
                        ? "base"
                        : "action",
                })];
            })
        ));
        routes.sort((left, right) => (
            Number(left.sourceKind !== "base") - Number(right.sourceKind !== "base")
            || left.sourceState - right.sourceState
            || left.sourceFrame - right.sourceFrame
            || SKILL_ENTRY_HIT_KEYS.indexOf(left.key) - SKILL_ENTRY_HIT_KEYS.indexOf(right.key)
        ));
        return Object.freeze({
            id: `entry:${oid}:${candidate.frame.frameId}`,
            oid,
            startFrame: candidate.frame.frameId,
            startOccurrence: candidate.frame.occurrence,
            label,
            displayName: override?.displayName || label,
            category,
            group: override?.group || DEFAULT_GROUPS[category],
            order: override?.order ?? candidate.frame.frameId,
            pinned: override?.pinned === true,
            hidden: override?.hidden === true,
            notes: override?.notes ?? "",
            segmentFrameCount: candidate.segmentFrameCount,
            baseState: candidate.baseState,
            actionRole: candidate.baseState === undefined ? "root" : "context",
            triggers: Object.freeze(triggers.map((trigger) => Object.freeze(trigger))),
            routes: Object.freeze(routes),
            parentStartFrames: Object.freeze([]),
            internalStages: Object.freeze([]),
            nativeTrigger: candidate.nativeTrigger,
            nativeInputPlan: candidate.nativeInputPlan,
        });
    });
    entries.sort((left, right) => (
        Number(right.pinned) - Number(left.pinned)
        || groupRank(left.group) - groupRank(right.group)
        || left.group.localeCompare(right.group, "zh-CN")
        || left.order - right.order
        || left.startFrame - right.startFrame
    ));
    return Object.freeze(entries);
}

function nextChainOwners(
    entries: readonly SkillEntry[],
    frameById: ReadonlyMap<number, DatFrameProjection>,
): Map<number, Set<number>> {
    const owners = new Map<number, Set<number>>();
    for (const entry of entries) {
        addNextChainOwner(entry.startFrame, entry.startFrame, frameById, owners);
    }
    return owners;
}

function classifyCompleteActions(
    entries: readonly SkillEntry[],
    frameById: ReadonlyMap<number, DatFrameProjection>,
): readonly SkillEntry[] {
    const structuralOwners = nextChainOwners(entries, frameById);
    const entryByStartFrame = new Map(entries.map((entry) => [entry.startFrame, entry]));
    return Object.freeze(entries.map((entry): SkillEntry => {
        if (entry.category === "base") {
            return Object.freeze({ ...entry, actionRole: "context", parentStartFrames: Object.freeze([]) });
        }
        if (entry.category !== "input" || entry.routes.length === 0) {
            return Object.freeze({ ...entry, actionRole: "root", parentStartFrames: Object.freeze([]) });
        }

        let hasBaseSource = false;
        let hasExternalSource = false;
        const parentStartFrames = new Set<number>();
        for (const route of entry.routes) {
            const sourceOwners = [...(structuralOwners.get(route.sourceFrame) ?? [])]
                .filter((owner) => owner !== entry.startFrame);
            if (route.sourceKind === "base"
                || sourceOwners.some((owner) => entryByStartFrame.get(owner)?.category === "base")) {
                hasBaseSource = true;
            }
            const actionOwners = sourceOwners.filter((owner) => entryByStartFrame.get(owner)?.category !== "base");
            actionOwners.forEach((owner) => parentStartFrames.add(owner));
            if (route.sourceKind !== "base" && sourceOwners.length === 0) hasExternalSource = true;
        }
        const actionRole: SkillEntryActionRole = !hasBaseSource
            && !hasExternalSource
            && parentStartFrames.size > 0
            ? "internal"
            : "root";
        return Object.freeze({
            ...entry,
            actionRole,
            parentStartFrames: Object.freeze([...parentStartFrames].sort((left, right) => left - right)),
        });
    }));
}

function buildCompleteActionOwners(
    entries: readonly SkillEntry[],
    references: readonly IndexedReference[],
    frameById: ReadonlyMap<number, DatFrameProjection>,
): { readonly ownersByFrame: Map<number, Set<number>>; readonly runtimeBranchFrames: Set<number> } {
    const ownersByFrame = new Map<number, Set<number>>();
    for (const entry of entries) {
        if (entry.actionRole !== "internal") {
            addNextChainOwner(entry.startFrame, entry.startFrame, frameById, ownersByFrame);
        }
    }
    const entryByStartFrame = new Map(entries.map((entry) => [entry.startFrame, entry]));
    const runtimeBranchFrames = new Set<number>();
    let changed = true;
    while (changed) {
        changed = false;
        for (const reference of references) {
            const targetEntry = entryByStartFrame.get(reference.targetFrame);
            const followsCompleteAction = reference.kind === "runtime"
                || (reference.kind === "input" && targetEntry?.actionRole === "internal");
            if (!followsCompleteAction) continue;
            const sourceOwners = ownersByFrame.get(reference.sourceFrame);
            if (sourceOwners === undefined) continue;
            for (const owner of sourceOwners) {
                changed = addNextChainOwner(
                    reference.targetFrame,
                    owner,
                    frameById,
                    ownersByFrame,
                    reference.kind === "runtime" ? runtimeBranchFrames : undefined,
                ) || changed;
            }
        }
    }
    return { ownersByFrame, runtimeBranchFrames };
}

function attachCompleteActionRelations(
    entries: readonly SkillEntry[],
    ownersByFrame: ReadonlyMap<number, ReadonlySet<number>>,
): readonly SkillEntry[] {
    const internalEntries = entries.filter((entry) => entry.actionRole === "internal");
    const rootStarts = new Set(entries.filter((entry) => entry.actionRole === "root").map((entry) => entry.startFrame));
    return Object.freeze(entries.map((entry): SkillEntry => {
        const completeOwners = [...(ownersByFrame.get(entry.startFrame) ?? [])]
            .filter((owner) => rootStarts.has(owner) && owner !== entry.startFrame)
            .sort((left, right) => left - right);
        const internalStages = entry.actionRole === "root"
            ? internalEntries
                .filter((candidate) => ownersByFrame.get(candidate.startFrame)?.has(entry.startFrame) === true)
                .map((candidate): SkillEntryInternalStage => Object.freeze({
                    startFrame: candidate.startFrame,
                    label: candidate.displayName,
                    triggers: candidate.triggers,
                }))
                .sort((left, right) => left.startFrame - right.startFrame)
            : [];
        return Object.freeze({
            ...entry,
            parentStartFrames: entry.actionRole === "internal"
                ? Object.freeze(completeOwners)
                : entry.parentStartFrames,
            internalStages: Object.freeze(internalStages),
        });
    }));
}

const BASE_CONTEXT_LABELS: Readonly<Record<0 | 1 | 2, string>> = Object.freeze({
    0: "standing",
    1: "walking",
    2: "running",
});

const BASE_CONTEXT_PREFERRED_FRAMES: Readonly<Record<0 | 1 | 2, number>> = Object.freeze({
    0: 0,
    1: 5,
    2: 9,
});

function buildBaseStateContexts(entries: readonly SkillEntry[]): readonly BaseStateContext[] {
    const result: BaseStateContext[] = [];
    for (const state of [0, 1, 2] as const) {
        const variants = entries
            .filter((entry) => entry.actionRole === "context" && entry.baseState === state)
            .sort((left, right) => left.startFrame - right.startFrame);
        if (variants.length === 0) continue;
        const preferred = variants.find((entry) => entry.startFrame === BASE_CONTEXT_PREFERRED_FRAMES[state])
            ?? variants[0]!;
        const actions = entries.filter((entry) => (
            entry.actionRole === "root"
            && entry.category !== "base"
            && entry.routes.some((route) => route.sourceKind === "base" && route.sourceState === state)
        ));
        const actionStartFrames = [...new Set(actions.map((entry) => entry.startFrame))].sort((left, right) => left - right);
        const routeCount = actions.reduce((count, entry) => count + entry.routes.filter((route) => (
            route.sourceKind === "base" && route.sourceState === state
        )).length, 0);
        result.push(Object.freeze({
            id: `base-state:${state}`,
            state,
            label: BASE_CONTEXT_LABELS[state],
            primaryStartFrame: preferred.startFrame,
            variantStartFrames: Object.freeze(variants.map((entry) => entry.startFrame)),
            frameCount: variants.reduce((count, entry) => count + entry.segmentFrameCount, 0),
            actionStartFrames: Object.freeze(actionStartFrames),
            routeCount,
        }));
    }
    return Object.freeze(result);
}

export function buildFrameEntryCatalog(
    frames: readonly DatFrameProjection[],
    oid: number,
    metadata: readonly SkillDisplayMetadata[] = [],
): FrameEntryCatalog {
    const preliminaryEntries = buildEntries(frames, oid, metadata);
    const runtimeFrames = latestRuntimeFrames(frames);
    const frameById = new Map(runtimeFrames.map((frame) => [frame.frameId, frame]));
    const references = collectFrameReferences(runtimeFrames, frameById);
    const referencesByTarget = new Map<number, IndexedReference[]>();
    for (const reference of references) {
        const values = referencesByTarget.get(reference.targetFrame);
        if (values) values.push(reference);
        else referencesByTarget.set(reference.targetFrame, [reference]);
    }

    const classifiedEntries = classifyCompleteActions(preliminaryEntries, frameById);
    const { ownersByFrame, runtimeBranchFrames } = buildCompleteActionOwners(
        classifiedEntries,
        references,
        frameById,
    );
    const entries = attachCompleteActionRelations(classifiedEntries, ownersByFrame);
    const baseContexts = buildBaseStateContexts(entries);

    const entryByStartFrame = new Map(entries.map((entry) => [entry.startFrame, entry]));
    const definitionsById = new Map<number, DatFrameProjection[]>();
    for (const frame of frames) {
        const definitions = definitionsById.get(frame.frameId);
        if (definitions) definitions.push(frame);
        else definitionsById.set(frame.frameId, [frame]);
    }
    const catalogFrames = frames.map((frame): FrameCatalogItem => {
        const definitions = definitionsById.get(frame.frameId) ?? [frame];
        const effectiveFrame = frameById.get(frame.frameId) ?? frame;
        const effective = effectiveFrame === frame;
        const incoming = effective ? referencesByTarget.get(frame.frameId) ?? [] : [];
        const entry = effective ? entryByStartFrame.get(frame.frameId) : undefined;
        let role: FrameCatalogRole;
        if (!effective) role = "overridden";
        else if (entry?.actionRole === "internal") role = "internal";
        else if (entry?.category === "base") role = "base-entry";
        else if (entry?.category === "input") role = "input-entry";
        else if (entry?.category === "engine") role = "engine-entry";
        else if (runtimeBranchFrames.has(frame.frameId) || incoming.some((reference) => reference.kind === "runtime")) {
            role = "runtime-branch";
        } else if (incoming.some((reference) => reference.kind === "next")) role = "internal";
        else role = "unresolved";
        const item: FrameCatalogItem = Object.freeze({
            frame,
            effective,
            effectiveOccurrence: effectiveFrame.occurrence,
            definitionCount: definitions.length,
            role,
            roleLabel: FRAME_ROLE_LABELS[role],
            ownerStartFrames: Object.freeze([...(ownersByFrame.get(frame.frameId) ?? [])].sort((left, right) => left - right)),
            references: Object.freeze(incoming.map((reference): FrameCatalogReference => Object.freeze({
                sourceFrame: reference.sourceFrame,
                sourceOccurrence: reference.sourceOccurrence,
                field: reference.field,
                kind: reference.kind,
            }))),
        });
        return item;
    });
    const byOccurrence = new Map(catalogFrames.map((item) => [item.frame.occurrence, item]));
    return Object.freeze({
        entries,
        baseContexts,
        frames: Object.freeze(catalogFrames),
        byOccurrence,
    });
}

export function deriveSkillEntries(
    frames: readonly DatFrameProjection[],
    oid: number,
    metadata: readonly SkillDisplayMetadata[] = [],
): readonly SkillEntry[] {
    return buildFrameEntryCatalog(frames, oid, metadata).entries;
}

export function entriesByStartFrame(entries: readonly SkillEntry[]): ReadonlyMap<number, SkillEntry> {
    return new Map(entries.map((entry) => [entry.startFrame, entry]));
}
