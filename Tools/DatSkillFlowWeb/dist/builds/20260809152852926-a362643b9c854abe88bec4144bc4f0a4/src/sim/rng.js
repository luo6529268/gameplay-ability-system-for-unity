// dat-skill-flow-build:20260809152852926-a362643b9c854abe88bec4144bc4f0a4
                                   
                          
                           
 

export function nextNtsdRandom(seed        )                   {
    if (!Number.isSafeInteger(seed) || seed < 0 || seed > 0xffff_ffff) {
        throw new RangeError("RNG seed must be a uint32 integer");
    }
    const nextSeed = (Math.imul(seed, 0x343fd) + 0x269ec3) >>> 0;
    return Object.freeze({
        seed: nextSeed,
        value: (nextSeed >>> 16) & 0x7fff,
    });
}
