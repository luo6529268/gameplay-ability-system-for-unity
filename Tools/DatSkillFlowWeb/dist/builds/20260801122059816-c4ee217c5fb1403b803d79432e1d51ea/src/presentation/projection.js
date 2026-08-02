// dat-skill-flow-build:20260801122059816-c4ee217c5fb1403b803d79432e1d51ea
                                                       

                                                
                             
                                                                   
 

                                              
                              
                          
                          
                                   
                             
                             
 

function safeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) throw new TypeError(`${label} must be a safe integer`);
    return value;
}

export function projectPresentationEntities(
    world                 ,
    options                               ,
)                                         {
    const cameraX = safeInteger(options.cameraX, "projection.cameraX");
    const ordered = world.entities.filter((entity) => entity.active).slice().sort((left, right) => (
        left.zInt < right.zInt ? -1 : left.zInt > right.zInt ? 1 : left.slot - right.slot
    ));
    return Object.freeze(ordered.map((entity) => {
        const renderOffsetX = safeInteger(options.renderOffsetBySlot?.[entity.slot] ?? 0, `projection.renderOffsetBySlot.${entity.slot}`);
        return Object.freeze({
            stableId: entity.stableId,
            slot: entity.slot,
            zInt: entity.zInt,
            renderOffsetX,
            screenX: safeInteger(entity.xInt + renderOffsetX - cameraX, `projection.screenX.${entity.slot}`),
            screenY: safeInteger(entity.zInt + entity.yInt, `projection.screenY.${entity.slot}`),
        });
    }));
}
