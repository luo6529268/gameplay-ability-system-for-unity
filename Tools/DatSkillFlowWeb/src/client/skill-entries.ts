import type { DatFrameProjection } from "../model/dat-projection.js";

export const SKILL_ENTRY_HIT_KEYS = Object.freeze([
    "hit_a", "hit_d", "hit_j", "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj", "hit_ja",
] as const);

export type SkillEntryHitKey = typeof SKILL_ENTRY_HIT_KEYS[number];
export type SkillEntryCategory = "base" | "input" | "action";

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
    readonly triggers: readonly SkillEntryTrigger[];
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
}

const DEFAULT_GROUPS: Readonly<Record<SkillEntryCategory, string>> = Object.freeze({
    base: "基础状态",
    input: "输入技能",
    action: "其他动作",
});

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
    const trigger = entry.triggers.find((candidate) => (
        candidate.sourceFrames.some((frameId) => runtimeFrameIds.has(frameId))
    ));
    if (trigger !== undefined) {
        const initialFrame = trigger.sourceFrames.find((frameId) => runtimeFrameIds.has(frameId))!;
        const inputPlan = HIT_KEY_INPUTS[trigger.key].map((key, index) => Object.freeze({
            tick: 2 + index * 2,
            keys: Object.freeze([key]),
        }));
        return Object.freeze({
            startFrame: entry.startFrame,
            initialFrame,
            inputPlan: Object.freeze(inputPlan),
            ticks: 120,
        });
    }

    // Frame 210 is the engine-level ordinary jump entry. Feed the physical
    // jump key (K in the Native P1 layout) from an idle state so C++ performs
    // the same 210 -> 211 -> 212 initialization as the battle scene.
    const idleFrame = entry.startFrame === 210 ? idlePreviewFrame(frames) : undefined;
    if (idleFrame !== undefined) {
        return Object.freeze({
            startFrame: entry.startFrame,
            initialFrame: idleFrame,
            inputPlan: Object.freeze([Object.freeze({ tick: 2, keys: Object.freeze(["K"] as const) })]),
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
): EntryCandidate {
    const existing = candidates.get(frame.frameId);
    if (existing !== undefined) {
        existing.segmentFrameCount = Math.max(existing.segmentFrameCount, segmentFrameCount);
        return existing;
    }
    const candidate: EntryCandidate = {
        frame,
        segmentFrameCount,
        triggerSources: new Map(),
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
    if (candidate.triggerSources.size > 0) return "input";
    return candidate.frame.state === 0 || candidate.frame.state === 1 || candidate.frame.state === 2
        ? "base"
        : "action";
}

function groupRank(group: string): number {
    const index = Object.values(DEFAULT_GROUPS).indexOf(group);
    return index < 0 ? Object.keys(DEFAULT_GROUPS).length : index;
}

export function deriveSkillEntries(
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
        candidateFor(candidates, start, segment.frames.length);
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
            triggers: Object.freeze(triggers.map((trigger) => Object.freeze(trigger))),
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

export function entriesByStartFrame(entries: readonly SkillEntry[]): ReadonlyMap<number, SkillEntry> {
    return new Map(entries.map((entry) => [entry.startFrame, entry]));
}
