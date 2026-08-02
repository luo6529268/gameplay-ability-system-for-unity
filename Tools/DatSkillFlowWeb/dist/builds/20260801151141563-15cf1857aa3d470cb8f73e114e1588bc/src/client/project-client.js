// dat-skill-flow-build:20260801151141563-15cf1857aa3d470cb8f73e114e1588bc
                                       
                          
                          
                          
                                   
                             
                             
                             
                           
                            
 

                                  
                       
                       
                             
 

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
