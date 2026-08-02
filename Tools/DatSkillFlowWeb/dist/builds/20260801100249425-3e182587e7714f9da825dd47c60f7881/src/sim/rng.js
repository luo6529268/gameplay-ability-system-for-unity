// dat-skill-flow-build:20260801100249425-3e182587e7714f9da825dd47c60f7881
                                   
                          
                           
 

export function nextNtsdRandom(seed        )                   {
    const nextSeed = (Math.imul(seed >>> 0, 0x343fd) + 0x269ec3) >>> 0;
    return Object.freeze({
        seed: nextSeed,
        value: (nextSeed >>> 16) & 0x7fff,
    });
}
