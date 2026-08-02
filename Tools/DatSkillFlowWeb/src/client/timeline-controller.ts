import {
    applyTimelineCommand,
    createSimulation,
    createTimeline,
    samplePresentation,
} from "../sim/index.js";
import { GATE2_SIM_RULE_IDS } from "../authority/gate2-sim-ledger.js";
import { canonicalizeTraceEnvelope } from "../trace/envelope.js";
import type {
    PresentationSample,
    SimEntitySeed,
    SimulationInput,
    SimulationState,
    TimelineController,
} from "../sim/index.js";

export const GATE2_AUTHORITY_FIXTURE = Object.freeze({
    label: "Gate2 authority fixture / no project loaded",
    description: "A synthetic timeline entity for validating the authoritative frame-tick flow; it is not a loaded DAT character.",
    entity: Object.freeze({
        stableId: "gate2-fixture-entity",
        slot: 0,
        rawObjectType: 0,
        frame: 0,
        waitCounter: 0,
        attacking: 0,
        facing: 0 as const,
        yInt: 0,
        hitStop: 0,
        killCount: -1,
        active: true,
        frames: Object.freeze([
            Object.freeze({ id: 0, state: 1, wait: 0, next: 1 }),
            Object.freeze({ id: 1, state: 1, wait: 0, next: 2 }),
            Object.freeze({ id: 2, state: 1, wait: 0, next: 0 }),
        ]),
    } satisfies SimEntitySeed),
});

export interface Gate2TimelineViewModel {
    readonly tick: number;
    readonly frame: number;
    readonly timeMs: number;
    readonly playing: boolean;
    readonly loopEnabled: boolean;
    readonly loopStartTick: number;
    readonly loopEndTick: number;
    readonly recordedEndTick: number;
    readonly rateLabel: "30 fps nominal / 33 ms";
    readonly fixtureLabel: string;
    readonly traceSummary: string;
    readonly diagnostics: readonly string[];
    readonly presentation: PresentationSample;
}

function initialTimeline(): TimelineController {
    return createTimeline(createSimulation({ entities: [GATE2_AUTHORITY_FIXTURE.entity] }));
}

function latestTraceSummary(controller: TimelineController): string {
    const trace = controller.traces.at(-1);
    if (trace === undefined) {
        return "No canonical tick has been recorded yet.";
    }
    const envelope = canonicalizeTraceEnvelope({
        schemaVersion: 1,
        streamId: "gate2-browser-fixture",
        sequence: trace.tickIndex,
        category: "simulation",
        tick: trace.tickIndex,
        ruleIds: [...trace.ruleIds],
        payload: { snapshotDigest: trace.snapshotDigest },
        diagnostics: [],
    });
    return `Tick ${trace.tickIndex}: ${trace.frameTransitions.length} transition${trace.frameTransitions.length === 1 ? "" : "s"}, ${trace.collisions.length} collision check${trace.collisions.length === 1 ? "" : "s"}, ${trace.lifecycle.length} lifecycle event${trace.lifecycle.length === 1 ? "" : "s"}; ${trace.snapshotDigest}; ${GATE2_SIM_RULE_IDS.length} authority rules / ${envelope.length} B trace envelope.`;
}

/** DOM-free adapter; every canonical change passes through applyTimelineCommand. */
export class Gate2TimelinePreviewController {
    private timeline: TimelineController;
    private loopEnabled = false;
    private loopStartTick = 0;
    private loopEndTick = 0;

    public constructor(timeline: TimelineController = initialTimeline()) {
        this.timeline = timeline;
        this.loopStartTick = timeline.initial.tickIndex;
        this.loopEndTick = timeline.initial.tickIndex;
    }

    public get canonical(): SimulationState {
        return this.timeline.canonical;
    }

    public get playing(): boolean {
        return this.timeline.playing;
    }

    public play(): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: "play" });
    }

    public pause(): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: "pause" });
    }

    public togglePlayback(): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: this.timeline.playing ? "pause" : "play" });
    }

    public step(input: SimulationInput = {}): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: "step", input });
    }

    public advance(input: SimulationInput = {}): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: "advance", input });
    }

    public seek(tick: number): void {
        this.timeline = applyTimelineCommand(this.timeline, { type: "seek", tick });
    }

    public setLoopBounds(startTick: number, endTick: number): void {
        this.loopStartTick = startTick;
        this.loopEndTick = endTick;
        this.applyLoopPreference();
    }

    public setLoopEnabled(enabled: boolean): void {
        this.loopEnabled = enabled;
        this.applyLoopPreference();
    }

    public viewModel(alpha = 0): Gate2TimelineViewModel {
        const entity = this.timeline.canonical.entities[0];
        return Object.freeze({
            tick: this.timeline.canonical.tickIndex,
            frame: entity?.frame ?? 0,
            timeMs: this.timeline.canonical.timeMs,
            playing: this.timeline.playing,
            loopEnabled: this.loopEnabled,
            loopStartTick: this.loopStartTick,
            loopEndTick: this.loopEndTick,
            recordedEndTick: this.timeline.initial.tickIndex + this.timeline.script.length,
            rateLabel: "30 fps nominal / 33 ms",
            fixtureLabel: GATE2_AUTHORITY_FIXTURE.label,
            traceSummary: latestTraceSummary(this.timeline),
            diagnostics: Object.freeze([
                "No project loaded. Synthetic fixture only; no DAT character is being represented.",
                "Canonical simulation changes only through timeline commands. Canvas sampling is presentation-only.",
            ]),
            presentation: samplePresentation(this.timeline, alpha),
        });
    }

    private applyLoopPreference(): void {
        this.timeline = applyTimelineCommand(this.timeline, {
            type: "set-loop",
            range: this.loopEnabled
                ? { startTick: this.loopStartTick, endTick: this.loopEndTick }
                : null,
        });
    }
}
