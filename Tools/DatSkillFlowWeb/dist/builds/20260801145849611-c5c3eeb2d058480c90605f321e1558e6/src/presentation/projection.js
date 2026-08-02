// dat-skill-flow-build:20260801145849611-c5c3eeb2d058480c90605f321e1558e6
                                                       

                                                
                             
                                                                   
 

                                              
                              
                          
                          
                                   
                             
                             
 

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
