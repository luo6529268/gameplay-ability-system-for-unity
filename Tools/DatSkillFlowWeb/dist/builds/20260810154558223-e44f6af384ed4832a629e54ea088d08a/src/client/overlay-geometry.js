// dat-skill-flow-build:20260810154558223-e44f6af384ed4832a629e54ea088d08a
             
                     
                  
                     
                       
                  
                     
                     
                                    

                                                                                    

                                    
                          
                         
                           
                            
                             
 

                                       
                               
                           
                           
                       
                       
 

                                      
                               
                           
                          
                        
                        
                        
                        
                           
                            
 

                                                                         

export const OVERLAY_COLORS                                        = Object.freeze({
    itr: "#f07832",
    bdy: "#29b6d1",
    opoint: "#e5b84b",
    wpoint: "#b66be8",
    bpoint: "#57c878",
    cpoint: "#e05aa8",
});

function point(sprite                   , x        , y        )                           {
    return {
        x: sprite.mirror ? sprite.left + sprite.width - x : sprite.left + x,
        y: sprite.top + y,
    };
}

function pointOverlay                                    (
    type             ,
    index        ,
    value   ,
    sprite                   ,
)                       {
    const position = point(sprite, value.x, value.y);
    return Object.freeze({ type, index, kind: "point"         , ...position });
}

function rectOverlay                                                          (
    type             ,
    index        ,
    value   ,
    sprite                   ,
)                      {
    const first = point(sprite, value.x, value.y);
    const second = point(sprite, value.x + value.w, value.y + value.h);
    return Object.freeze({
        type,
        index,
        kind: "rect"         ,
        x1: first.x,
        y1: first.y,
        x2: second.x,
        y2: second.y,
        width: second.x - first.x,
        height: second.y - first.y,
    });
}

function mapPoints                                    (
    type             ,
    values              ,
    sprite                   ,
)                                  {
    return values.map((value, index) => pointOverlay(type, index, value, sprite));
}

function mapRects                                                          (
    type             ,
    values              ,
    sprite                   ,
)                                 {
    return values.map((value, index) => rectOverlay(type, index, value, sprite));
}

export function buildOverlayGeometry(
    frame                    ,
    sprite                   ,
)                             {
    return Object.freeze([
        ...mapRects("itr", frame.itrs                            , sprite),
        ...mapRects("bdy", frame.bdys                            , sprite),
        ...mapPoints("opoint", frame.opoints                               , sprite),
        ...mapPoints("wpoint", frame.wpoints                               , sprite),
        ...mapPoints("bpoint", frame.bpoints                               , sprite),
        ...mapPoints("cpoint", frame.cpoints                               , sprite),
    ]);
}

export function hitTestOverlay(
    geometry                            ,
    x        ,
    y        ,
    pointRadius = 6,
)                              {
    for (let index = geometry.length - 1; index >= 0; index -= 1) {
        const item = geometry[index] ;
        if (item.kind === "point") {
            if (Math.hypot(item.x - x, item.y - y) <= pointRadius) return item;
            continue;
        }
        const left = Math.min(item.x1, item.x2);
        const right = Math.max(item.x1, item.x2);
        const top = Math.min(item.y1, item.y2);
        const bottom = Math.max(item.y1, item.y2);
        if (x >= left && x <= right && y >= top && y <= bottom) return item;
    }
    return undefined;
}
