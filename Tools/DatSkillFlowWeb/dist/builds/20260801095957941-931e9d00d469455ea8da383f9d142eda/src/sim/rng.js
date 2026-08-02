// dat-skill-flow-build:20260801095957941-931e9d00d469455ea8da383f9d142eda
                                   
                          
                           
 

export function nextNtsdRandom(seed        )                   {
    const nextSeed = (Math.imul(seed >>> 0, 0x343fd) + 0x269ec3) >>> 0;
    return Object.freeze({
        seed: nextSeed,
        value: (nextSeed >>> 16) & 0x7fff,
    });
}
