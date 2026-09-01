// dat-skill-flow-build:20260830084617618-18ef901e469444d9b80e355a62838458
import { number, text,           } from "./editor-support.js";
                                                                        

export const RENDER_CADENCE_RATES = Object.freeze([30, 60, 120]         );
                                                                    
export const NATIVE_LOGIC_TICK_MS = 33;

                                      
                                     
                                   
                                     
                                       
                                        
                                                       
 

function isRenderCadenceRate(value        )                             {
    return RENDER_CADENCE_RATES.includes(value                     );
}

function clamp(value        , minimum        , maximum        )         {
    return Math.min(Math.max(value, minimum), maximum);
}

function finite(value         , fallback = 0)         {
    return number(value, fallback);
}

function lineageKey(entity               )         {
    const explicit = text(entity.lineageId ?? entity.lineage_id ?? entity.stableId ?? entity.stable_id);
    if (explicit !== "") return `lineage:${explicit}`;
    return `slot:${entity.slot}:oid:${entity.oid}`;
}

function displayPosition(entity               , axis                 )         {
    const integerKey = axis === "x" ? "xInt" : axis === "y" ? "yInt" : "zInt";
    return finite(entity[integerKey] ?? entity[axis]);
}

function precisePosition(entity               , axis                 )         {
    return finite(entity[axis], displayPosition(entity, axis));
}

function displayZ(entity               )         {
    return finite(entity.displayZ ?? entity.zInt ?? entity.z);
}

function velocity(entity               , axis                 )         {
    const value = entity.velocity;
    if (typeof value !== "object" || value === null || Array.isArray(value)) return 0;
    return finite((value                           )[axis]);
}

function relationMatches(previous               , current               )          {
    return finite(previous.target, -1) === finite(current.target, -1)
        && finite(previous.holder, -1) === finite(current.holder, -1)
        && finite(previous.link, -1) === finite(current.link, -1);
}

function continuousAxis(
    previousPosition        ,
    currentPosition        ,
    previousVelocity        ,
    currentVelocity        ,
)          {
    const observedVelocity = Math.max(Math.abs(previousVelocity), Math.abs(currentVelocity));
    const maximumContinuousDelta = Math.max(64, observedVelocity * 4 + 4);
    return Math.abs(currentPosition - previousPosition) <= maximumContinuousDelta;
}

function interpolate(previous        , current        , alpha        )         {
    return previous + (current - previous) * alpha;
}

function interpolateEntity(
    previous                           ,
    current               ,
    alpha        ,
)                {
    if (previous === undefined
        || lineageKey(previous) !== lineageKey(current)
        || !relationMatches(previous, current)) {
        return current;
    }

    const previousX = precisePosition(previous, "x");
    const previousY = precisePosition(previous, "y");
    const previousZ = precisePosition(previous, "z");
    const currentX = precisePosition(current, "x");
    const currentY = precisePosition(current, "y");
    const currentZ = precisePosition(current, "z");
    if (!continuousAxis(previousX, currentX, velocity(previous, "x"), velocity(current, "x"))
        || !continuousAxis(previousY, currentY, velocity(previous, "y"), velocity(current, "y"))
        || !continuousAxis(previousZ, currentZ, velocity(previous, "z"), velocity(current, "z"))) {
        return current;
    }

    const x = interpolate(previousX, currentX, alpha);
    const y = interpolate(previousY, currentY, alpha);
    const z = interpolate(previousZ, currentZ, alpha);
    const deltaX = Math.round(x) - Math.round(currentX);
    const deltaY = Math.round(y) - Math.round(currentY);
    const deltaZ = Math.round(z) - Math.round(currentZ);

    return Object.freeze({
        ...current,
        x,
        y,
        z,
        xInt: displayPosition(current, "x") + deltaX,
        yInt: displayPosition(current, "y") + deltaY,
        zInt: displayPosition(current, "z") + deltaZ,
        displayZ: displayZ(current) + deltaZ,
    });
}

function interpolateTick(
    previous             ,
    current             ,
    alpha        ,
)              {
    if (finite(previous.tick, -1) + 1 !== finite(current.tick, -1)) return current;
    const previousByLineage = new Map(previous.entities.map((entity) => [lineageKey(entity), entity]));
    const cameraX = Math.round(interpolate(finite(previous.cameraX), finite(current.cameraX), alpha));
    return Object.freeze({
        ...current,
        cameraX,
        entities: current.entities.map((entity) => interpolateEntity(previousByLineage.get(lineageKey(entity)), entity, alpha)),
    });
}

function sampleDiscrete(
    ticks                        ,
    elapsedMs        ,
    rate                   ,
)                      {
    const tickIndex = clamp(Math.floor(elapsedMs / NATIVE_LOGIC_TICK_MS), 0, ticks.length - 1);
    return Object.freeze({
        rate,
        displayTimeMs: Math.floor(elapsedMs / (1000 / rate)) * (1000 / rate),
        sourceTickIndex: tickIndex,
        previousTickIndex: tickIndex,
        interpolationAlpha: 1,
        presentationTick: ticks[tickIndex],
    });
}

/**
 * Samples a recorded 30 Hz Native trace without predicting the next logic tick.
 * 60/120 Hz panes intentionally render one logic tick behind and only interpolate
 * presentation coordinates of the same lineage. Frame/state/lifecycle remain current-tick discrete values.
 */
export function sampleRenderCadence(
    ticks                        ,
    elapsedMs        ,
    rate                   ,
)                      {
    if (!isRenderCadenceRate(rate)) {
        throw new RangeError(`Unsupported render cadence: ${rate}`);
    }
    if (!Number.isFinite(elapsedMs) || elapsedMs < 0) {
        throw new RangeError("elapsedMs must be a finite nonnegative number.");
    }
    if (ticks.length === 0) {
        return Object.freeze({
            rate,
            displayTimeMs: 0,
            sourceTickIndex: 0,
            previousTickIndex: 0,
            interpolationAlpha: 0,
            presentationTick: undefined,
        });
    }
    if (rate === 30 || ticks.length === 1) {
        return sampleDiscrete(ticks, elapsedMs, rate);
    }

    const displayIntervalMs = 1000 / rate;
    const displayTimeMs = Math.floor(elapsedMs / displayIntervalMs) * displayIntervalMs;
    const delayedProgress = Math.max(0, displayTimeMs / NATIVE_LOGIC_TICK_MS - 1);
    const previousTickIndex = clamp(Math.floor(delayedProgress), 0, ticks.length - 1);
    const sourceTickIndex = Math.min(previousTickIndex + 1, ticks.length - 1);
    const interpolationAlpha = sourceTickIndex === previousTickIndex
        ? 1
        : delayedProgress - Math.floor(delayedProgress);
    const previous = ticks[previousTickIndex] ;
    const current = ticks[sourceTickIndex] ;

    return Object.freeze({
        rate,
        displayTimeMs,
        sourceTickIndex,
        previousTickIndex,
        interpolationAlpha,
        presentationTick: interpolateTick(previous, current, interpolationAlpha),
    });
}

/**
 * Samples the main editor playback clock. 30 Hz remains discrete. Higher
 * presentation rates use the current Native Tick's frame/lifecycle while
 * moving its stable entities from the previous adjacent Tick toward their
 * current precise positions. The caller controls how often this is sampled.
 */
export function samplePlaybackPresentation(
    ticks                        ,
    playbackMs        ,
    rate                   ,
)                      {
    if (!isRenderCadenceRate(rate)) {
        throw new RangeError(`Unsupported render cadence: ${rate}`);
    }
    if (!Number.isFinite(playbackMs) || playbackMs < 0) {
        throw new RangeError("playbackMs must be a finite nonnegative number.");
    }
    if (ticks.length === 0) {
        return Object.freeze({
            rate,
            displayTimeMs: 0,
            sourceTickIndex: 0,
            previousTickIndex: 0,
            interpolationAlpha: 0,
            presentationTick: undefined,
        });
    }

    const lastTickIndex = ticks.length - 1;
    const endMs = lastTickIndex * NATIVE_LOGIC_TICK_MS;
    const displayTimeMs = clamp(playbackMs, 0, endMs);
    if (rate === 30 || ticks.length === 1) {
        return sampleDiscrete(ticks, displayTimeMs, rate);
    }
    if (displayTimeMs >= endMs) {
        return Object.freeze({
            rate,
            displayTimeMs,
            sourceTickIndex: lastTickIndex,
            previousTickIndex: lastTickIndex,
            interpolationAlpha: 1,
            presentationTick: ticks[lastTickIndex],
        });
    }

    const progress = displayTimeMs / NATIVE_LOGIC_TICK_MS;
    const previousTickIndex = Math.floor(progress);
    const sourceTickIndex = previousTickIndex + 1;
    const interpolationAlpha = progress - previousTickIndex;
    return Object.freeze({
        rate,
        displayTimeMs,
        sourceTickIndex,
        previousTickIndex,
        interpolationAlpha,
        presentationTick: interpolateTick(
            ticks[previousTickIndex] ,
            ticks[sourceTickIndex] ,
            interpolationAlpha,
        ),
    });
}

export function renderCadenceLoopDurationMs(ticks                        )         {
    return Math.max(NATIVE_LOGIC_TICK_MS, Math.max(0, ticks.length - 1) * NATIVE_LOGIC_TICK_MS);
}

export function isPresentationOnlyCadenceField(field            )          {
    return field === "x"
        || field === "y"
        || field === "z"
        || field === "xInt"
        || field === "yInt"
        || field === "zInt"
        || field === "displayZ"
        || field === "renderOffsetX"
        || field === "cameraX";
}
