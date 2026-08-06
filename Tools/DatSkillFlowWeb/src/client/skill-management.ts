export interface SkillEntry {
    readonly oid: number;
    readonly name: string;
    readonly startFrame: number;
}

export interface SkillMutation<T extends SkillEntry> {
    readonly skills: readonly T[];
    readonly selectedIndex: number;
}

export function skillIndexesForOid<T extends SkillEntry>(
    skills: readonly T[],
    oid: number,
): readonly number[] {
    return Object.freeze(skills.flatMap((skill, index) => skill.oid === oid ? [index] : []));
}

function requireIndex<T>(skills: readonly T[], index: number): void {
    if (!Number.isSafeInteger(index) || index < 0 || index >= skills.length) {
        throw new RangeError("The selected skill index is invalid.");
    }
}

export function duplicateSkill<T extends SkillEntry>(
    skills: readonly T[],
    index: number,
): SkillMutation<T> {
    requireIndex(skills, index);
    const copy = { ...skills[index]!, name: `${skills[index]!.name} 副本` } as T;
    const next = [...skills];
    next.splice(index + 1, 0, copy);
    return Object.freeze({ skills: Object.freeze(next), selectedIndex: index + 1 });
}

export function deleteSkillForOid<T extends SkillEntry>(
    skills: readonly T[],
    index: number,
    oid: number,
): SkillMutation<T> {
    requireIndex(skills, index);
    const visible = skillIndexesForOid(skills, oid);
    const visibleIndex = visible.indexOf(index);
    if (visibleIndex < 0) throw new RangeError("The selected skill does not belong to the active OID.");
    const next = [...skills];
    next.splice(index, 1);
    const remaining = skillIndexesForOid(next, oid);
    return Object.freeze({
        skills: Object.freeze(next),
        selectedIndex: remaining.length === 0 ? -1 : remaining[Math.min(visibleIndex, remaining.length - 1)]!,
    });
}

export function moveSkillForOid<T extends SkillEntry>(
    skills: readonly T[],
    index: number,
    oid: number,
    delta: -1 | 1,
): SkillMutation<T> {
    requireIndex(skills, index);
    const visible = skillIndexesForOid(skills, oid);
    const visibleIndex = visible.indexOf(index);
    if (visibleIndex < 0) throw new RangeError("The selected skill does not belong to the active OID.");
    const targetIndex = visible[visibleIndex + delta];
    if (targetIndex === undefined) {
        return Object.freeze({ skills: Object.freeze([...skills]), selectedIndex: index });
    }
    const next = [...skills];
    [next[index], next[targetIndex]] = [next[targetIndex]!, next[index]!];
    return Object.freeze({ skills: Object.freeze(next), selectedIndex: targetIndex });
}
