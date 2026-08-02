// dat-skill-flow-build:20260801150924691-a965db5c58fb428bac2b8b55832a9401
                                       
                          
                          
                          
                                   
                             
                             
                             
                           
                            
 

                                  
                       
                       
                             
 

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
