// dat-skill-flow-build:20260811074458769-cdf65e6b9e1f4ab4af73cb3217dda8c9
             
                            
                                 
                                
                      
                             
                                   

                                                   
                                                             

                              
                               
                          
                         
                               
                                   
                         
                                 
                           
                                   
                                  
 

export function objectKind(
    entity                                               ,
    objectType               ,
    resource                                      ,
    rootOid        ,
)              {
    if (entity.slot === 0 && entity.oid === rootOid) return "root";
    if (objectType === 0) {
        return resource?.name.toLowerCase().includes("clone") === true ? "clone" : "actor";
    }
    return objectType === null ? "unknown" : "projectile";
}

export function enrichNativePreview(
    preview                   ,
    resources                                     ,
    objectTypes                             ,
    rootOid        ,
    actionFrameIds                      = new Set([preview.metadata.startFrame]),
)                    {
    const resourcesByOid = new Map(resources.map((resource) => [resource.oid, resource]));
    const entities = new Map                            ();
    const activeBySlot = new Map                            ();
    const events                                = [];
    let rootSkillStartedTick                = null;
    let rootSkillEntryFrame                = null;
    let rootSkillEndedTick                = null;

    const ticks = preview.ticks.map((tick) => {
        const currentSlots = new Set        ();
        const enrichedEntities = tick.entities.map((entity) => {
            currentSlots.add(entity.slot);
            const objectType = objectTypes.get(entity.oid) ?? null;
            const resource = resourcesByOid.get(entity.oid);
            const kind = objectKind(entity, objectType, resource, rootOid);
            let lineage = activeBySlot.get(entity.slot);
            if (lineage !== undefined && lineage.oid !== entity.oid) {
                if (lineage.completedTick === null) {
                    lineage.completedTick = tick.tick;
                    lineage.completion = "despawned";
                }
                events.push({
                    tick: tick.tick,
                    kind: "despawn",
                    lineageId: lineage.lineageId,
                    slot: lineage.slot,
                    oid: lineage.oid,
                });
                lineage = undefined;
            }
            if (lineage === undefined) {
                lineage = createLineage(entity, kind, tick.tick);
                activeBySlot.set(entity.slot, lineage);
                entities.set(lineage.lineageId, lineage);
                events.push({
                    tick: tick.tick,
                    kind: "spawn",
                    lineageId: lineage.lineageId,
                    slot: entity.slot,
                    oid: entity.oid,
                });
            }
            lineage.lastSeenTick = tick.tick;
            if (kind === "projectile" && (entity.yInt < 0 || entity.velocity.y < 0)) {
                lineage.sawAirborneProjectile = true;
            }
            const frame = resource?.frames.find((candidate) => candidate.frameId === entity.frame);
            if (kind === "root"
                && rootSkillStartedTick === null
                && actionFrameIds.has(entity.frame)) {
                rootSkillStartedTick = tick.tick;
                rootSkillEntryFrame = entity.frame;
            }
            if (kind === "root"
                && rootSkillStartedTick !== null
                && frame?.state !== 0) {
                lineage.sawNonIdleActionRoot = true;
            }
            if (kind === "projectile"
                && lineage.completedTick === null
                && lineage.sawAirborneProjectile
                && entity.yInt >= 0
                && entity.velocity.y === 0) {
                lineage.completedTick = tick.tick;
                lineage.completion = "landed";
            }
            if (kind === "root"
                && rootSkillEndedTick === null
                && rootSkillStartedTick !== null
                && tick.tick > rootSkillStartedTick
                && lineage.sawNonIdleActionRoot
                && !actionFrameIds.has(entity.frame)
                && isRootEnded(entity, resource)) {
                rootSkillEndedTick = tick.tick;
                lineage.completedTick = tick.tick;
                lineage.completion = "root-ended";
            } else if (kind === "clone" && lineage.completedTick === null) {
                lineage.completedTick = tick.tick;
                lineage.completion = "spawned";
            }
            return {
                ...entity,
                objectType,
                kind,
                lineageId: lineage.lineageId,
                firstSeenTick: lineage.firstSeenTick,
                lastSeenTick: lineage.lastSeenTick,
                resourceAvailable: resource !== undefined,
            };
        });

        for (const [slot, lineage] of activeBySlot) {
            if (currentSlots.has(slot)) continue;
            activeBySlot.delete(slot);
            if (lineage.completedTick === null) {
                lineage.completedTick = tick.tick;
                lineage.completion = "despawned";
            }
            events.push({
                tick: tick.tick,
                kind: "despawn",
                lineageId: lineage.lineageId,
                slot: lineage.slot,
                oid: lineage.oid,
            });
        }
        return { ...tick, entities: enrichedEntities };
    });

    const lastTick = ticks.at(-1)?.tick ?? 0;
    const traceEntities = [...entities.values()].map(toTraceEntity);
    const pendingProjectiles = traceEntities
        .filter((entity) => entity.kind === "projectile" && entity.completedTick === null)
        .map((entity) => entity.lineageId);
    const projectileEndTick = traceEntities
        .filter((entity) => entity.kind === "projectile" && entity.completedTick !== null)
        .reduce((latest, entity) => Math.max(latest, entity.completedTick ?? 0), 0);
    const playbackEndTick = pendingProjectiles.length > 0
        ? lastTick
        : Math.max(rootSkillEndedTick ?? lastTick, projectileEndTick);
    const status = rootSkillStartedTick === null
        ? "entry-not-reached"
        : rootSkillEndedTick === null
            ? "timeout"
            : pendingProjectiles.length > 0 ? "persistent" : "complete";

    return {
        ...preview,
        ticks,
        trace: {
            rootSkillStartedTick,
            rootSkillEntryFrame,
            rootSkillEndedTick,
            progressEndTick: rootSkillEndedTick,
            playbackEndTick,
            status,
            pendingProjectiles,
            entities: traceEntities,
            events,
        },
    };
}

function createLineage(
    entity                                               ,
    kind             ,
    tick        ,
)                     {
    const lineageId = `${kind}:${entity.oid}:${entity.slot}:${tick}`;
    return {
        lineageId,
        slot: entity.slot,
        oid: entity.oid,
        kind,
        firstSeenTick: tick,
        lastSeenTick: tick,
        completedTick: null,
        completion: "unknown",
        sawAirborneProjectile: false,
        sawNonIdleActionRoot: false,
    };
}

function isRootEnded(
    entity                         ,
    resource                                      ,
)          {
    if (entity.yInt !== 0) return false;
    const frame = resource?.frames.find((candidate) => candidate.frameId === entity.frame);
    return frame?.state === 0;
}

function toTraceEntity(entity                    )                               {
    return {
        lineageId: entity.lineageId,
        slot: entity.slot,
        oid: entity.oid,
        kind: entity.kind,
        firstSeenTick: entity.firstSeenTick,
        lastSeenTick: entity.lastSeenTick,
        completedTick: entity.completedTick,
        completion: entity.completion,
    };
}
