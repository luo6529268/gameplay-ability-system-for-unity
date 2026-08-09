// dat-skill-flow-build:20260808055059689-c4ead27e48bb44bb9b5efe3eb14c3391
                                                                     

export const SKILL_ENTRY_HIT_KEYS = Object.freeze([
    "hit_a", "hit_d", "hit_j", "hit_Fa", "hit_Fj", "hit_Ua", "hit_Uj", "hit_Da", "hit_Dj", "hit_ja",
]         );

                                                                   
                                                             

                                       
                         
                                
                                  
                            
                            
                              
                              
                            
 

                                    
                                   
                                             
 

                             
                        
                         
                                
                                     
                           
                                 
                                          
                           
                           
                             
                             
                           
                                       
                                                    
 

                        
                           
                                          
 

export function latestRuntimeFrameMap(
    frames                               ,
)                                          {
    const frameById = new Map                            ();
    for (const frame of frames) {
        if (Number.isSafeInteger(frame.frameId) && frame.frameId >= 0 && frame.frameId < 600) {
            frameById.set(frame.frameId, frame);
        }
    }
    return frameById;
}

                          
                                       
                              
                                                                
 

const DEFAULT_GROUPS                                               = Object.freeze({
    base: "基础状态",
    input: "输入技能",
    action: "其他动作",
});

function latestRuntimeFrames(frames                               )                       {
    const latestById = latestRuntimeFrameMap(frames);
    return frames.filter((frame) => latestById.get(frame.frameId) === frame);
}

export function authoredTraceStartFrame(
    frames                               ,
    requestedFrameId        ,
)         {
    const runtimeFrames = latestRuntimeFrames(frames);
    const frameById = new Map(runtimeFrames.map((frame) => [frame.frameId, frame]));
    let current = frameById.get(requestedFrameId);
    if (current === undefined) return requestedFrameId;

    const visited = new Set        ();
    while (!visited.has(current.frameId)) {
        visited.add(current.frameId);
        const predecessor = frameById.get(current.frameId - 1);
        if (predecessor === undefined
            || labelKey(predecessor) !== labelKey(current)
            || Math.abs(predecessor.next) !== current.frameId) {
            break;
        }
        current = predecessor;
    }
    return current.frameId;
}

function labelKey(frame                    )         {
    const label = frame.label.trim();
    return label === "" ? `\0${frame.frameId}` : label.toLocaleLowerCase("en-US");
}

function buildSegments(frames                               )                 {
    const result                 = [];
    for (const frame of frames) {
        const previous = result[result.length - 1];
        const previousFrame = previous?.frames[previous.frames.length - 1];
        if (previous !== undefined
            && previousFrame !== undefined
            && labelKey(frame) === labelKey(previousFrame)
            && frame.frameId === previousFrame.frameId + 1) {
            previous.frames.push(frame);
        } else {
            result.push({ label: frame.label.trim(), frames: [frame] });
        }
    }
    return result;
}

function candidateFor(
    candidates                             ,
    frame                    ,
    segmentFrameCount        ,
)                 {
    const existing = candidates.get(frame.frameId);
    if (existing !== undefined) {
        existing.segmentFrameCount = Math.max(existing.segmentFrameCount, segmentFrameCount);
        return existing;
    }
    const candidate                 = {
        frame,
        segmentFrameCount,
        triggerSources: new Map(),
    };
    candidates.set(frame.frameId, candidate);
    return candidate;
}

function metadataFor(
    metadata                                 ,
    oid        ,
    startFrame        ,
)                                   {
    return metadata.find((entry) => entry.oid === oid && entry.startFrame === startFrame);
}

function categoryFor(candidate                )                     {
    if (candidate.triggerSources.size > 0) return "input";
    return candidate.frame.state === 0 || candidate.frame.state === 1 || candidate.frame.state === 2
        ? "base"
        : "action";
}

function groupRank(group        )         {
    const index = Object.values(DEFAULT_GROUPS).indexOf(group);
    return index < 0 ? Object.keys(DEFAULT_GROUPS).length : index;
}

export function deriveSkillEntries(
    frames                               ,
    oid        ,
    metadata                                  = [],
)                        {
    const runtimeFrames = latestRuntimeFrames(frames);
    const frameById = new Map(runtimeFrames.map((frame) => [frame.frameId, frame]));
    const segments = buildSegments(runtimeFrames);
    const segmentByOccurrence = new Map                      ();
    const candidates = new Map                        ();

    for (const segment of segments) {
        segment.frames.forEach((frame) => segmentByOccurrence.set(frame.occurrence, segment));
        const start = segment.frames[0] ;
        candidateFor(candidates, start, segment.frames.length);
    }

    for (const source of runtimeFrames) {
        for (const key of SKILL_ENTRY_HIT_KEYS) {
            const rawTarget = source[key];
            if (rawTarget === 0) continue;
            const target = frameById.get(rawTarget);
            if (target === undefined) continue;
            const segment = segmentByOccurrence.get(target.occurrence);
            const targetIndex = segment?.frames.indexOf(target) ?? -1;
            const candidate = candidateFor(
                candidates,
                target,
                targetIndex < 0 ? 1 : segment .frames.length - targetIndex,
            );
            const sources = candidate.triggerSources.get(key) ?? new Set        ();
            sources.add(source.frameId);
            candidate.triggerSources.set(key, sources);
        }
    }

    const entries = [...candidates.values()].map((candidate)             => {
        const override = metadataFor(metadata, oid, candidate.frame.frameId);
        const category = categoryFor(candidate);
        const label = candidate.frame.label.trim() || `frame_${candidate.frame.frameId}`;
        const triggers = SKILL_ENTRY_HIT_KEYS.flatMap((key)                      => {
            const sources = candidate.triggerSources.get(key);
            return sources === undefined ? [] : [{
                key,
                sourceFrames: Object.freeze([...sources].sort((left, right) => left - right)),
            }];
        });
        return Object.freeze({
            id: `entry:${oid}:${candidate.frame.frameId}`,
            oid,
            startFrame: candidate.frame.frameId,
            startOccurrence: candidate.frame.occurrence,
            label,
            displayName: override?.displayName || label,
            category,
            group: override?.group || DEFAULT_GROUPS[category],
            order: override?.order ?? candidate.frame.frameId,
            pinned: override?.pinned === true,
            hidden: override?.hidden === true,
            notes: override?.notes ?? "",
            segmentFrameCount: candidate.segmentFrameCount,
            triggers: Object.freeze(triggers.map((trigger) => Object.freeze(trigger))),
        });
    });
    entries.sort((left, right) => (
        Number(right.pinned) - Number(left.pinned)
        || groupRank(left.group) - groupRank(right.group)
        || left.group.localeCompare(right.group, "zh-CN")
        || left.order - right.order
        || left.startFrame - right.startFrame
    ));
    return Object.freeze(entries);
}

export function entriesByStartFrame(entries                       )                                  {
    return new Map(entries.map((entry) => [entry.startFrame, entry]));
}
