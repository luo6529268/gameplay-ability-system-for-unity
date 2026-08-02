// dat-skill-flow-build:20260801131725857-f394b90a7ab94b99b24c7191c69bc8d2
import { currentFrame } from "../sim/catalog.js";
                                                       

                                          
                             
                               
                                       
 

                                                                                 

                                         
                                             
                             
                              
                                                        
                                             
 

function safeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) throw new TypeError(`${label} must be a safe integer`);
    return value;
}

function truncDivision(numerator        , denominator        , label        )         {
    return safeInteger(Math.trunc(numerator / denominator), label);
}

export function createPresentationCamera(seed                                   = {})                          {
    return Object.freeze({
        cameraX: safeInteger(seed.cameraX ?? 0, "camera.cameraX"),
        cameraVel: safeInteger(seed.cameraVel ?? 0, "camera.cameraVel"),
        cameraMaxOverride: safeInteger(seed.cameraMaxOverride ?? 0, "camera.cameraMaxOverride"),
    });
}

export function stepPresentationCamera(
    camera                         ,
    world                 ,
    stageWidth        ,
)                         {
    const normalized = createPresentationCamera(camera);
    const maximumX = safeInteger(stageWidth, "stageWidth") - 794;
    if (maximumX <= 0) {
        return Object.freeze({
            camera: createPresentationCamera({ ...normalized, cameraX: 0, cameraVel: 0 }),
            targetX: 0,
            maximumX,
            subjectKind: "synthetic"         ,
            subjectSlots: Object.freeze([]),
        });
    }

    let subjects = world.entities.filter((entity) => (
        entity.active && entity.runtimeObjectType === 0 && entity.slot < 8 && entity.hp > 0
    ));
    let subjectKind                                = "primary";
    let positions          ;
    if (subjects.length > 0) {
        positions = subjects.map((entity) => {
            const state = currentFrame(entity.frames, entity.frame)?.state ?? 0;
            return state === 14 ? entity.xInt : entity.xInt - entity.facing * 260 + 130;
        });
    } else {
        subjects = world.entities.filter((entity) => entity.active && entity.rawObjectType === 0 && entity.hp > 0);
        if (subjects.length > 0) {
            subjectKind = "fallback";
            positions = subjects.map((entity) => entity.xInt);
        } else {
            subjectKind = "synthetic";
            positions = [800];
        }
    }
    let sum = 0;
    for (const position of positions) sum = safeInteger(sum + position, "camera subject sum");
    let targetX = truncDivision(sum, positions.length, "camera target average") - 397;
    targetX = Math.max(0, Math.min(maximumX, targetX));
    if (normalized.cameraMaxOverride > 0 && targetX > normalized.cameraMaxOverride) {
        targetX = normalized.cameraMaxOverride;
    }
    const difference = safeInteger(targetX - normalized.cameraX, "camera difference");
    const step = truncDivision(difference, 14, "camera step");
    let cameraVel = truncDivision(safeInteger(step + normalized.cameraVel * 6, "camera smoothing numerator"), 7, "camera velocity");
    if (cameraVel === 0 && difference !== 0) cameraVel = difference > 0 ? 1 : -1;
    const cameraX = Math.max(0, Math.min(maximumX, safeInteger(normalized.cameraX + cameraVel, "camera X")));
    return Object.freeze({
        camera: createPresentationCamera({ cameraX, cameraVel, cameraMaxOverride: normalized.cameraMaxOverride }),
        targetX,
        maximumX,
        subjectKind,
        subjectSlots: Object.freeze(subjects.map((entity) => entity.slot)),
    });
}
