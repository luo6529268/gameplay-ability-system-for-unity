// dat-skill-flow-build:20260809133839407-06fa9f754e1742708e780993ad4a65f3
                                                       

                                                
                             
                                                                   
 

                                              
                              
                          
                          
                                   
                             
                             
 

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
