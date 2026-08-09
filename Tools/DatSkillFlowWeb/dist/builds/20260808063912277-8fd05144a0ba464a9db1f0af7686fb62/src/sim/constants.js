// dat-skill-flow-build:20260808063912277-8fd05144a0ba464a9db1f0af7686fb62
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
