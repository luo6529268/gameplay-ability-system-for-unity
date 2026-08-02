// dat-skill-flow-build:20260801084918054-1dfb493da1c743a89352a3b37343e9d9
             
                     
               
                  
                       
                        
                    

export const MAX_WORLD_SLOTS = 400;
export const MAX_CATALOG_OIDS = 1000;
export const MAX_FRAME_IDS = 600;

function safeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new TypeError(`${label} must be a safe integer`);
    }
    return value;
}

function normalizeOpoint(candidate                     )                      {
    return Object.freeze({
        kind: safeInteger(candidate.kind, "opoint.kind"),
        x: safeInteger(candidate.x, "opoint.x"),
        y: safeInteger(candidate.y, "opoint.y"),
        action: safeInteger(candidate.action, "opoint.action"),
        dvx: safeInteger(candidate.dvx, "opoint.dvx"),
        dvy: safeInteger(candidate.dvy, "opoint.dvy"),
        oid: safeInteger(candidate.oid, "opoint.oid"),
        facing: safeInteger(candidate.facing, "opoint.facing"),
    });
}

export function normalizeFrames(frames                               )                                {
    const definitions = new Map                            ();
    for (const candidate of frames) {
        const centerx = candidate.centerx === undefined
            ? undefined
            : safeInteger(candidate.centerx, "frame.centerx");
        const centery = candidate.centery === undefined
            ? undefined
            : safeInteger(candidate.centery, "frame.centery");
        const opoints = candidate.opoints === undefined
            ? undefined
            : Object.freeze(candidate.opoints.map(normalizeOpoint));
        const normalized = Object.freeze({
            id: safeInteger(candidate.id, "frame.id"),
            state: safeInteger(candidate.state, "frame.state"),
            wait: safeInteger(candidate.wait, "frame.wait"),
            next: safeInteger(candidate.next, "frame.next"),
            ...(centerx === undefined ? {} : { centerx }),
            ...(centery === undefined ? {} : { centery }),
            ...(opoints === undefined ? {} : { opoints }),
            mp: safeInteger(candidate.mp ?? 0, "frame.mp"),
            hit_a: safeInteger(candidate.hit_a ?? 0, "frame.hit_a"),
            hit_d: safeInteger(candidate.hit_d ?? 0, "frame.hit_d"),
            hit_j: safeInteger(candidate.hit_j ?? 0, "frame.hit_j"),
            hit_Fa: safeInteger(candidate.hit_Fa ?? 0, "frame.hit_Fa"),
            hit_Ua: safeInteger(candidate.hit_Ua ?? 0, "frame.hit_Ua"),
            hit_Da: safeInteger(candidate.hit_Da ?? 0, "frame.hit_Da"),
            hit_Fj: safeInteger(candidate.hit_Fj ?? 0, "frame.hit_Fj"),
            hit_Uj: safeInteger(candidate.hit_Uj ?? 0, "frame.hit_Uj"),
            hit_Dj: safeInteger(candidate.hit_Dj ?? 0, "frame.hit_Dj"),
            hit_ja: safeInteger(candidate.hit_ja ?? 0, "frame.hit_ja"),
        });
        definitions.set(normalized.id, normalized);
    }
    return Object.freeze([...definitions.values()].sort((left, right) => left.id - right.id));
}

export function authoredFrame(
    frames                               ,
    frameId        ,
)                                 {
    if (frameId < 0 || frameId >= MAX_FRAME_IDS) {
        return undefined;
    }
    return frames.find((definition) => definition.id === frameId);
}

const EMPTY_FRAME                     = Object.freeze({
    id: 0,
    state: 0,
    wait: 1,
    next: 0,
    mp: 0,
    hit_a: 0,
    hit_d: 0,
    hit_j: 0,
    hit_Fa: 0,
    hit_Ua: 0,
    hit_Da: 0,
    hit_Fj: 0,
    hit_Uj: 0,
    hit_Dj: 0,
    hit_ja: 0,
});

export function currentFrame(
    frames                               ,
    frameId        ,
)                                 {
    if (frameId < 0 || frameId >= MAX_FRAME_IDS) {
        return undefined;
    }
    return authoredFrame(frames, frameId) ?? EMPTY_FRAME;
}

function normalizeDat(candidate   
                         
                                   
                               
                                                   
 )                   {
    const oid = safeInteger(candidate.oid, "catalog.oid");
    if (oid < 0 || oid >= MAX_CATALOG_OIDS) {
        throw new RangeError("catalog.oid must be in 0..999");
    }
    return Object.freeze({
        oid,
        rawObjectType: safeInteger(candidate.rawObjectType, "catalog.rawObjectType"),
        weaponHp: safeInteger(candidate.weaponHp ?? 0, "catalog.weaponHp"),
        frames: normalizeFrames(candidate.frames),
    });
}

export function createDatCatalog(
    explicit                                   ,
    legacySeeds                          ,
)                                       {
    const catalog                              = Array(MAX_CATALOG_OIDS).fill(null);
    for (const candidate of explicit ?? []) {
        const normalized = normalizeDat(candidate);
        if (catalog[normalized.oid] !== null) {
            throw new TypeError(`duplicate catalog oid: ${normalized.oid}`);
        }
        catalog[normalized.oid] = normalized;
    }
    for (const seed of legacySeeds) {
        const oid = safeInteger(seed.oid ?? 0, "entity.oid");
        if (oid < 0 || oid >= MAX_CATALOG_OIDS) {
            throw new RangeError("entity.oid must be in 0..999");
        }
        if (catalog[oid] === null) {
            catalog[oid] = normalizeDat({
                oid,
                rawObjectType: seed.rawObjectType,
                weaponHp: seed.weaponHp ?? 0,
                frames: seed.frames,
            });
        }
    }
    return Object.freeze(catalog);
}

export function resolveDat(
    catalog                                      ,
    oid        ,
)                               {
    if (!Number.isSafeInteger(oid) || oid < 0 || oid >= MAX_CATALOG_OIDS) {
        return undefined;
    }
    return catalog[oid] ?? undefined;
}
