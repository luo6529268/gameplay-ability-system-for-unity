// dat-skill-flow-build:20260810142555053-2342a9cd97b748478af583aa85ff65b4
             
                     
               
                  
                       
                     
                        
                        
                    

export const MAX_WORLD_SLOTS = 400;
export const MAX_CATALOG_OIDS = 1000;
export const MAX_FRAME_IDS = 600;
export const MAX_OPOINTS_PER_FRAME = 400;
export const MAX_WPOINTS_PER_FRAME = 400;
export const MAX_ITRS_PER_FRAME = 400;

function safeInteger(value        , label        )         {
    if (!Number.isSafeInteger(value)) {
        throw new TypeError(`${label} must be a safe integer`);
    }
    return value;
}

function finiteNumber(value        , label        )         {
    if (!Number.isFinite(value)) throw new TypeError(`${label} must be finite`);
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

const wpointKeys = ["kind", "x", "y", "attacking", "cover", "weaponact", "dvx", "dvy", "dvz"]         ;

function normalizeWpoint(candidate                     )                      {
    return Object.freeze(Object.fromEntries(wpointKeys.map((key) => [
        key,
        safeInteger(candidate[key] ?? 0, `wpoint.${key}`),
    ])))                                  ;
}

const itrDefaults                   = Object.freeze({
    kind: 0, x: 0, y: 0, w: 0, h: 0,
    dvx: 0, dvy: 0, fall: 0, bdefend: 0, injury: 0,
    arest: 0, vrest: 0, effect: 0, attacking: 0,
    catchingact: 0, catchingact2: 0, caughtact: 0, caughtact2: 0,
    respond: 0, pickingact: 0, pickedact: 0,
    throwvx: 0, throwvy: 0, zwidth: 15, throwvz: 0, throwinjury: 0,
});

const itrKeys = Object.keys(itrDefaults)                                       ;

function normalizeItr(candidate                  )                   {
    return Object.freeze(Object.fromEntries(itrKeys.map((key) => [
        key,
        safeInteger(candidate[key] ?? itrDefaults[key], `itr.${key}`),
    ])))                               ;
}

export function normalizeFrames(frames                               )                                {
    if (frames.length > MAX_FRAME_IDS) {
        throw new RangeError("frames must contain at most 600 definitions");
    }
    const definitions = new Map                            ();
    for (const candidate of frames) {
        if (!Number.isSafeInteger(candidate.id) || candidate.id < 0 || candidate.id >= MAX_FRAME_IDS) {
            throw new RangeError("frame.id must be in 0..599");
        }
        if ((candidate.opoints?.length ?? 0) > MAX_OPOINTS_PER_FRAME) {
            throw new RangeError("frame.opoints must contain at most 400 entries");
        }
        if ((candidate.wpoints?.length ?? 0) > MAX_WPOINTS_PER_FRAME) {
            throw new RangeError("frame.wpoints must contain at most 400 entries");
        }
        if ((candidate.itrs?.length ?? 0) > MAX_ITRS_PER_FRAME) {
            throw new RangeError("frame.itrs must contain at most 400 entries");
        }
        const centerx = candidate.centerx === undefined
            ? undefined
            : safeInteger(candidate.centerx, "frame.centerx");
        const centery = candidate.centery === undefined
            ? undefined
            : safeInteger(candidate.centery, "frame.centery");
        const opoints = candidate.opoints === undefined
            ? undefined
            : Object.freeze(candidate.opoints.map(normalizeOpoint));
        const wpoints = candidate.wpoints === undefined
            ? undefined
            : Object.freeze(candidate.wpoints.map(normalizeWpoint));
        const itrs = candidate.itrs === undefined
            ? undefined
            : Object.freeze(candidate.itrs.map(normalizeItr));
        const cpoints = candidate.cpoints === undefined
            ? undefined
            : Object.freeze(candidate.cpoints.map((value) => Object.freeze({ kind: safeInteger(value.kind, "cpoint.kind") })));
        const normalized = Object.freeze({
            id: safeInteger(candidate.id, "frame.id"),
            state: safeInteger(candidate.state, "frame.state"),
            wait: safeInteger(candidate.wait, "frame.wait"),
            next: safeInteger(candidate.next, "frame.next"),
            ...(candidate.dvx === undefined ? {} : { dvx: safeInteger(candidate.dvx, "frame.dvx") }),
            ...(candidate.dvy === undefined ? {} : { dvy: safeInteger(candidate.dvy, "frame.dvy") }),
            ...(candidate.dvz === undefined ? {} : { dvz: safeInteger(candidate.dvz, "frame.dvz") }),
            ...(cpoints === undefined ? {} : { cpoints }),
            ...(centerx === undefined ? {} : { centerx }),
            ...(centery === undefined ? {} : { centery }),
            ...(opoints === undefined ? {} : { opoints }),
            ...(wpoints === undefined ? {} : { wpoints }),
            ...(itrs === undefined ? {} : { itrs }),
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
    dvx: 0, dvy: 0, dvz: 0,
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
        jumpHeight: finiteNumber(candidate.jumpHeight ?? -16.3, "catalog.jumpHeight"),
        jumpDistance: finiteNumber(candidate.jumpDistance ?? 8.0, "catalog.jumpDistance"),
        jumpDistanceZ: finiteNumber(candidate.jumpDistanceZ ?? 3.0, "catalog.jumpDistanceZ"),
        frameSourceIndex: -1,
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
