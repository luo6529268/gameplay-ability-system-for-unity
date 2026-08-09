// dat-skill-flow-build:20260808055425459-f67be6018c754d698cb5c87bc701ce6a
                                       
                          
                          
                          
                                   
                             
                             
                             
                           
                            
 

                                  
                       
                       
                             
 

                               
                             
                                
 

                                              
                         
                           
                                
                              
                                      
 

export const NATIVE_PREVIEW_PRIMARY_SLOT = 0;

                                   
                          
 

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
