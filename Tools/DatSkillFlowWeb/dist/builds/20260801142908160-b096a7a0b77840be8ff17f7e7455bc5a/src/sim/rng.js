// dat-skill-flow-build:20260801142908160-b096a7a0b77840be8ff17f7e7455bc5a
                                   
                          
                           
 

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
