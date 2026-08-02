// dat-skill-flow-build:20260801122059816-c4ee217c5fb1403b803d79432e1d51ea
                                   
                          
                           
 

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
