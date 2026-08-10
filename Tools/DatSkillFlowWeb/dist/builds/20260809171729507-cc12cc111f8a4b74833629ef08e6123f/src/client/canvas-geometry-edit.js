// dat-skill-flow-build:20260809171729507-cc12cc111f8a4b74833629ef08e6123f
             
                    
                         
                        
                               

                                                     

                           
                       
                       
 

                                           
                       
                       
 

export function snapDelta(value        , gridSize       )         {
    return Math.round(value / gridSize) * gridSize;
}

export function moveDatPoint(
    value          ,
    screenDx        ,
    screenDy        ,
    mirror         ,
)           {
    return Object.freeze({
        x: value.x + (mirror ? -screenDx : screenDx),
        y: value.y + screenDy,
    });
}

export function resizeDatRect(
    value         ,
    handle              ,
    screenDx        ,
    screenDy        ,
    mirror         ,
)                      {
    const localDx = mirror ? -screenDx : screenDx;
    const screenLeft = handle === "nw" || handle === "sw";
    const localStart = mirror ? !screenLeft : screenLeft;
    const top = handle === "nw" || handle === "ne";
    let { x, y, w, h } = value;
    if (localStart) {
        x += localDx;
        w -= localDx;
    } else {
        w += localDx;
    }
    if (top) {
        y += screenDy;
        h -= screenDy;
    } else {
        h += screenDy;
    }
    return w > 0 && h > 0 ? Object.freeze({ x, y, w, h }) : undefined;
}

export function hitResizeHandle(
    geometry                     ,
    x        ,
    y        ,
    radius = 7,
)                           {
    const left = Math.min(geometry.x1, geometry.x2);
    const right = Math.max(geometry.x1, geometry.x2);
    const top = Math.min(geometry.y1, geometry.y2);
    const bottom = Math.max(geometry.y1, geometry.y2);
    const handles                                            = [
        ["nw", left, top],
        ["ne", right, top],
        ["sw", left, bottom],
        ["se", right, bottom],
    ];
    return handles.find(([, handleX, handleY]) => (
        Math.hypot(handleX - x, handleY - y) <= radius
    ))?.[0];
}

function moveDraftPoint(
    geometry                      ,
    screenDx        ,
    screenDy        ,
)                       {
    return Object.freeze({
        ...geometry,
        x: geometry.x + screenDx,
        y: geometry.y + screenDy,
    });
}

function moveDraftRect(
    geometry                     ,
    screenDx        ,
    screenDy        ,
)                      {
    return Object.freeze({
        ...geometry,
        x1: geometry.x1 + screenDx,
        y1: geometry.y1 + screenDy,
        x2: geometry.x2 + screenDx,
        y2: geometry.y2 + screenDy,
    });
}

export function draftOverlayGeometry(
    geometry                 ,
    screenDx        ,
    screenDy        ,
    handle               ,
)                              {
    if (geometry.kind === "point") return moveDraftPoint(geometry, screenDx, screenDy);
    if (handle === undefined) return moveDraftRect(geometry, screenDx, screenDy);
    const left = Math.min(geometry.x1, geometry.x2);
    const right = Math.max(geometry.x1, geometry.x2);
    const top = Math.min(geometry.y1, geometry.y2);
    const bottom = Math.max(geometry.y1, geometry.y2);
    const nextLeft = handle === "nw" || handle === "sw" ? left + screenDx : left;
    const nextRight = handle === "ne" || handle === "se" ? right + screenDx : right;
    const nextTop = handle === "nw" || handle === "ne" ? top + screenDy : top;
    const nextBottom = handle === "sw" || handle === "se" ? bottom + screenDy : bottom;
    if (nextRight - nextLeft < 1 || nextBottom - nextTop < 1) return undefined;
    return Object.freeze({
        ...geometry,
        x1: nextLeft,
        y1: nextTop,
        x2: nextRight,
        y2: nextBottom,
        width: nextRight - nextLeft,
        height: nextBottom - nextTop,
    });
}
