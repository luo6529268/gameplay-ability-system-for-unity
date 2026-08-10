// dat-skill-flow-build:20260809130526477-fdef2e179820472faf37f6285771cc14
                                       
                          
                          
                          
                                   
                             
                             
                             
                           
                            
 

                                  
                       
                       
                             
 

                               
                             
                                
 

                                              
                         
                           
                                
                              
                                      
 

export const NATIVE_PREVIEW_PRIMARY_SLOT = 0;

                                   
                          
 

                                        
                               
                                       
                                
                                   
                                   
                              
                                         
        
                           
 

export class BoundedLruCache       {
             #maximumEntries        ;
             #entries = new Map      ();

    constructor(maximumEntries        ) {
        if (!Number.isSafeInteger(maximumEntries) || maximumEntries < 1) {
            throw new RangeError("maximumEntries must be a positive safe integer.");
        }
        this.#maximumEntries = maximumEntries;
    }

    get size()         {
        return this.#entries.size;
    }

    get(key   )                {
        const value = this.#entries.get(key);
        if (value === undefined) return undefined;
        this.#entries.delete(key);
        this.#entries.set(key, value);
        return value;
    }

    set(key   , value   )       {
        this.#entries.delete(key);
        this.#entries.set(key, value);
        while (this.#entries.size > this.#maximumEntries) {
            const oldest = this.#entries.keys().next().value                 ;
            if (oldest === undefined) break;
            this.#entries.delete(oldest);
        }
    }

    clear()       {
        this.#entries.clear();
    }
}

export function previewIntentCacheKey(intent                       )         {
    return JSON.stringify({
        sessionId: intent.sessionId,
        revision: intent.revision,
        startFrame: intent.startFrame,
        initialFrame: intent.initialFrame ?? intent.startFrame,
        ticks: intent.ticks,
        inputPlan: (intent.inputPlan ?? []).map((step) => ({ tick: step.tick, keys: [...step.keys] })),
    });
}

export function primaryPreviewEntity                            (
    entities              ,
)                {
    return entities.find((entity) => entity.slot === NATIVE_PREVIEW_PRIMARY_SLOT);
}

export function lastFrameForId                        (
    frames              ,
    frameId                    ,
)                {
    if (frameId === undefined) return undefined;
    for (let index = frames.length - 1; index >= 0; index -= 1) {
        if (frames[index]?.frameId === frameId) return frames[index];
    }
    return undefined;
}

export function findFrameFieldCapability                                       (
    fields              ,
    frame              ,
    key        ,
)                {
    for (let index = fields.length - 1; index >= 0; index -= 1) {
        const field = fields[index] ;
        if (field.scope === "frame"
            && field.frameId === frame.frameId
            && field.frameOccurrence === frame.occurrence
            && field.key === key) {
            return field;
        }
    }
    return undefined;
}

export function spritePlacement(input                      )                  {
    const sx = input.xInt + input.renderOffsetX - input.cameraX;
    const sy = input.zInt + input.yInt;
    const mirror = input.facing === 1;
    return Object.freeze({
        x: mirror ? sx - (input.width - input.centerX) : sx - input.centerX,
        y: sy - input.centerY,
        mirror,
    });
}

export function mergePreview                  (
    project   ,
    revision                 ,
    nativeTicks                    ,
    nativeTrace          ,
    previewObjects                     ,
)       
                                       
                                             
                                   
                                                 
  {
    return Object.freeze({
        ...project,
        revision,
        nativeTicks: Object.freeze([...nativeTicks]),
        ...(nativeTrace === undefined ? {} : { nativeTrace }),
        ...(previewObjects === undefined ? {} : { previewObjects: Object.freeze([...previewObjects]) }),
    });
}
