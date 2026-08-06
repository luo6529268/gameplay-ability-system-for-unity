// dat-skill-flow-build:20260806142111218-e0d3a43dddbf4ce38f99c8853e113bc9
                                       
                          
                          
                          
                                   
                             
                             
                             
                           
                            
 

                                  
                       
                       
                             
 

                               
                             
                                
 

                                              
                         
                           
                                
                              
                                      
 

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

export function mergePreview                  (project   , revision                 , nativeTicks                    )       
                                       
                                             
  {
    return Object.freeze({ ...project, revision, nativeTicks: Object.freeze([...nativeTicks]) });
}
