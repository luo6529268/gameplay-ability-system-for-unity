// dat-skill-flow-build:20260806232034560-c9ee0125ea734d98a762056770e672a7
                             
                         
                          
                                
 

                                                      
                                  
                                   
 

export function skillIndexesForOid                      (
    skills              ,
    oid        ,
)                    {
    return Object.freeze(skills.flatMap((skill, index) => skill.oid === oid ? [index] : []));
}

function requireIndex   (skills              , index        )       {
    if (!Number.isSafeInteger(index) || index < 0 || index >= skills.length) {
        throw new RangeError("The selected skill index is invalid.");
    }
}

export function duplicateSkill                      (
    skills              ,
    index        ,
)                   {
    requireIndex(skills, index);
    const copy = { ...skills[index] , name: `${skills[index] .name} 副本` }     ;
    const next = [...skills];
    next.splice(index + 1, 0, copy);
    return Object.freeze({ skills: Object.freeze(next), selectedIndex: index + 1 });
}

export function deleteSkillForOid                      (
    skills              ,
    index        ,
    oid        ,
)                   {
    requireIndex(skills, index);
    const visible = skillIndexesForOid(skills, oid);
    const visibleIndex = visible.indexOf(index);
    if (visibleIndex < 0) throw new RangeError("The selected skill does not belong to the active OID.");
    const next = [...skills];
    next.splice(index, 1);
    const remaining = skillIndexesForOid(next, oid);
    return Object.freeze({
        skills: Object.freeze(next),
        selectedIndex: remaining.length === 0 ? -1 : remaining[Math.min(visibleIndex, remaining.length - 1)] ,
    });
}

export function moveSkillForOid                      (
    skills              ,
    index        ,
    oid        ,
    delta        ,
)                   {
    requireIndex(skills, index);
    const visible = skillIndexesForOid(skills, oid);
    const visibleIndex = visible.indexOf(index);
    if (visibleIndex < 0) throw new RangeError("The selected skill does not belong to the active OID.");
    const targetIndex = visible[visibleIndex + delta];
    if (targetIndex === undefined) {
        return Object.freeze({ skills: Object.freeze([...skills]), selectedIndex: index });
    }
    const next = [...skills];
    [next[index], next[targetIndex]] = [next[targetIndex] , next[index] ];
    return Object.freeze({ skills: Object.freeze(next), selectedIndex: targetIndex });
}
