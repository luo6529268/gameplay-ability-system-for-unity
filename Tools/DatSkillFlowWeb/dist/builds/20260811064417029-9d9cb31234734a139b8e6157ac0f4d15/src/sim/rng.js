// dat-skill-flow-build:20260811064417029-9d9cb31234734a139b8e6157ac0f4d15
                                   
                          
                           
 

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
