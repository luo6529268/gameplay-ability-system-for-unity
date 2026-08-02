// dat-skill-flow-build:20260801100636145-dd66b0468c524d2087248924398c50c5
                                   
                          
                           
 

export function nextNtsdRandom(seed        )                   {
    const nextSeed = (Math.imul(seed >>> 0, 0x343fd) + 0x269ec3) >>> 0;
    return Object.freeze({
        seed: nextSeed,
        value: (nextSeed >>> 16) & 0x7fff,
    });
}
