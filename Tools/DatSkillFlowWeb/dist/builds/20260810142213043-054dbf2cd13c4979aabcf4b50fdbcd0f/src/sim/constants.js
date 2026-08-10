// dat-skill-flow-build:20260810142213043-054dbf2cd13c4979aabcf4b50fdbcd0f
export const FRAME_MS = 33         ;
export const NOMINAL_FRAME_RATE = 30         ;
export const EFFECTIVE_FRAME_RATE = 1000 / FRAME_MS;
export const SIMULATION_RATE_LABEL = `${NOMINAL_FRAME_RATE} nominal (${EFFECTIVE_FRAME_RATE.toFixed(3)} effective)`;

export function ticksToMilliseconds(ticks        )         {
    if (!Number.isSafeInteger(ticks) || ticks < 0) {
        throw new RangeError("ticks must be a nonnegative safe integer");
    }
    return ticks * FRAME_MS;
}
