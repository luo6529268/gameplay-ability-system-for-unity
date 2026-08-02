// dat-skill-flow-build:20260801122713965-9be1b45b04a846ef944e304147d93adc
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
