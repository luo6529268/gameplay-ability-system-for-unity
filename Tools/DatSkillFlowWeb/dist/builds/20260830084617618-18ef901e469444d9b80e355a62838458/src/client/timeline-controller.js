// dat-skill-flow-build:20260830084617618-18ef901e469444d9b80e355a62838458
import {
    applyTimelineCommand,
    createSimulation,
    createTimeline,
    samplePresentation,
} from "../sim/index.js";
import { GATE2_SIM_RULE_IDS } from "../authority/gate2-sim-ledger.js";
import { canonicalizeTraceEnvelope } from "../trace/envelope.js";
             
                       
                  
                    
                    
                       
                         

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
        facing: 0         ,
        yInt: 0,
        hitStop: 0,
        killCount: -1,
        active: true,
        frames: Object.freeze([
            Object.freeze({ id: 0, state: 1, wait: 0, next: 1 }),
            Object.freeze({ id: 1, state: 1, wait: 0, next: 2 }),
            Object.freeze({ id: 2, state: 1, wait: 0, next: 0 }),
        ]),
    }                        ),
});

                                         
                          
                           
                            
                              
                                  
                                   
                                 
                                     
                                                 
                                  
                                  
                                            
                                              
 

function initialTimeline()                     {
    return createTimeline(createSimulation({ entities: [GATE2_AUTHORITY_FIXTURE.entity] }));
}

function latestTraceSummary(controller                    )         {
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
            timeline                    ;
            loopEnabled = false;
            loopStartTick = 0;
            loopEndTick = 0;

           constructor(timeline                     = initialTimeline()) {
        this.timeline = timeline;
        this.loopStartTick = timeline.initial.tickIndex;
        this.loopEndTick = timeline.initial.tickIndex;
    }

           get canonical()                  {
        return this.timeline.canonical;
    }

           get playing()          {
        return this.timeline.playing;
    }

           play()       {
        this.timeline = applyTimelineCommand(this.timeline, { type: "play" });
    }

           pause()       {
        this.timeline = applyTimelineCommand(this.timeline, { type: "pause" });
    }

           togglePlayback()       {
        this.timeline = applyTimelineCommand(this.timeline, { type: this.timeline.playing ? "pause" : "play" });
    }

           step(input                  = {})       {
        this.timeline = applyTimelineCommand(this.timeline, { type: "step", input });
    }

           advance(input                  = {})       {
        this.timeline = applyTimelineCommand(this.timeline, { type: "advance", input });
    }

           seek(tick        )       {
        this.timeline = applyTimelineCommand(this.timeline, { type: "seek", tick });
    }

           setLoopBounds(startTick        , endTick        )       {
        this.loopStartTick = startTick;
        this.loopEndTick = endTick;
        this.applyLoopPreference();
    }

           setLoopEnabled(enabled         )       {
        this.loopEnabled = enabled;
        this.applyLoopPreference();
    }

           viewModel(alpha = 0)                         {
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

            applyLoopPreference()       {
        this.timeline = applyTimelineCommand(this.timeline, {
            type: "set-loop",
            range: this.loopEnabled
                ? { startTick: this.loopStartTick, endTick: this.loopEndTick }
                : null,
        });
    }
}
