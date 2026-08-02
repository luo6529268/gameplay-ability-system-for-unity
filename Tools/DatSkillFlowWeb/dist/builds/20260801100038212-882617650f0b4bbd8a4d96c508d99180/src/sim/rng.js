// dat-skill-flow-build:20260801100038212-882617650f0b4bbd8a4d96c508d99180
                                   
                          
                           
 

export function nextNtsdRandom(seed        )                   {
    const nextSeed = (Math.imul(seed >>> 0, 0x343fd) + 0x269ec3) >>> 0;
    return Object.freeze({
        seed: nextSeed,
        value: (nextSeed >>> 16) & 0x7fff,
    });
}
