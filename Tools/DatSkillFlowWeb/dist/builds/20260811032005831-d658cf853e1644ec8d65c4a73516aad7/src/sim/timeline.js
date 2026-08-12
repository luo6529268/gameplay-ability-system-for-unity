// dat-skill-flow-build:20260811032005831-d658cf853e1644ec8d65c4a73516aad7
import { FRAME_MS } from "./constants.js";
import { stepSimulation } from "./core.js";
             
              
                    
                      
                    
                        
                    

                                    
                               
                             
 

                                     
                                      
                                        
                                                
                                                
                                                    
                              
                                                 
 

                             
                               
                                
                                                                
                                                                   
                                                      
                                                                              

                                           
                              
                               
                             
 

                                     
                           
                                  
                                                           
 

function freezeController(controller                    )                     {
    return Object.freeze({
        ...controller,
        script: Object.freeze([...controller.script]),
        traces: Object.freeze([...controller.traces]),
        loopRange: controller.loopRange === null ? null : Object.freeze({ ...controller.loopRange }),
    });
}

export function createTimeline(initial                 )                     {
    return freezeController({
        initial,
        canonical: initial,
        previousCanonical: initial,
        script: [],
        traces: [],
        playing: false,
        loopRange: null,
    });
}

function seekTimeline(
    controller                    ,
    tick        ,
    runtime                   ,
)                     {
    if (!Number.isSafeInteger(tick)) {
        throw new TypeError("seek tick must be a safe integer");
    }
    const offset = tick - controller.initial.tickIndex;
    if (offset < 0 || offset > controller.script.length) {
        throw new RangeError("seek tick is outside the recorded script");
    }
    let canonical = controller.initial;
    let previousCanonical = controller.initial;
    const traces                        = [];
    for (let index = 0; index < offset; index++) {
        previousCanonical = canonical;
        const result = stepSimulation(canonical, controller.script[index] , runtime);
        canonical = result.state;
        traces.push(result.trace);
    }
    if (offset === 0) {
        previousCanonical = controller.initial;
    }
    return freezeController({
        ...controller,
        canonical,
        previousCanonical,
        traces,
    });
}

function stepTimeline(
    controller                    ,
    input                 ,
    runtime                   ,
)                     {
    const offset = controller.canonical.tickIndex - controller.initial.tickIndex;
    const traces = controller.traces.slice(0, offset);
    const result = stepSimulation(controller.canonical, input, runtime);
    const script = [...controller.script.slice(0, offset), result.trace.inputs];
    return freezeController({
        ...controller,
        canonical: result.state,
        previousCanonical: controller.canonical,
        script,
        traces: [...traces, result.trace],
    });
}

function validateLoopRange(
    controller                    ,
    range                          ,
)                           {
    if (range === null) {
        return null;
    }
    if (!Number.isSafeInteger(range.startTick) || !Number.isSafeInteger(range.endTick)) {
        throw new TypeError("loop ticks must be safe integers");
    }
    const lastRecordedTick = controller.initial.tickIndex + controller.script.length;
    if (
        range.startTick < controller.initial.tickIndex
        || range.startTick > range.endTick
        || range.endTick > lastRecordedTick
    ) {
        throw new RangeError("loop range must be ordered and inside the recorded script");
    }
    return Object.freeze({ ...range });
}

export function applyTimelineCommand(
    controller                    ,
    command                 ,
    runtime                    = {},
)                     {
    switch (command.type) {
        case "play":
            return controller.playing ? controller : freezeController({ ...controller, playing: true });
        case "pause":
            return controller.playing ? freezeController({ ...controller, playing: false }) : controller;
        case "step":
            return stepTimeline(controller, command.input, runtime);
        case "advance":
            if (!controller.playing) {
                return controller;
            }
            if (controller.loopRange !== null && controller.canonical.tickIndex >= controller.loopRange.endTick) {
                return seekTimeline(controller, controller.loopRange.startTick, runtime);
            }
            return stepTimeline(controller, command.input, runtime);
        case "seek":
            return seekTimeline(controller, command.tick, runtime);
        case "set-loop":
            return freezeController({ ...controller, loopRange: validateLoopRange(controller, command.range) });
    }
}

function entityByStableId(entities                      , stableId        )                        {
    return entities.find((entity) => entity.stableId === stableId);
}

export function samplePresentation(controller                    , alpha        )                     {
    if (!Number.isFinite(alpha) || alpha < 0 || alpha > 1) {
        throw new RangeError("presentation alpha must be finite and in [0, 1]");
    }
    const entities = controller.canonical.entities.map((current) => {
        const previous = entityByStableId(controller.previousCanonical.entities, current.stableId);
        return Object.freeze({
            stableId: current.stableId,
            fromFrame: previous?.frame ?? current.frame,
            toFrame: current.frame,
        });
    });
    return Object.freeze({
        alpha,
        sampleTimeMs: controller.previousCanonical.timeMs + (FRAME_MS * alpha),
        entities: Object.freeze(entities),
    });
}
