---
provider: "codex"
agent_role: "code-reviewer"
model: "gpt-5.3-codex"
timestamp: "2026-03-02T16:29:10.836Z"
---

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

## Code Review: WPoint/OPoint System Implementation

Review these 5 code changes for a Unity port of FLF (Little Fighter 2 engine). Only analyze — do NOT modify files. Return analysis + unified diff if corrections needed.

### Context
- FLF pixel units → Unity: x/100 = Unity X; z/100 = Unity Y (ground); y is jump offset (negative = in air = higher Y)
- ps.sx = sprite left-edge x (pixels); ps.sy = sprite top-edge y (pixels); ps.sz = sprite z
- wpoint.kind 1=hold/throw, 2=throwable (on weapon frame), 3=force drop

### Change O-1: Uncomment OPoint processing
```
// Generic_Frame() line 157
ObjectPointModule?.ProcessFrame(this);   // was: //ObjectPointModule.ProcessTransit(this);
```

### Change O-2 + O-3: LF2ObjectRenderer
```csharp
// SetLogicObject - initialize Sprite module
_logicObject?.Sprite?.Initialize(_spriteRenderer, null);

// UpdateSprite
var frame = _logicObject.Frame?.D;
_logicObject.Sprite?.ShowPic(frame.pic);
_logicObject.Sprite?.SwitchLR(ps.dir);

// UpdatePosition
float worldX = ps.x / 100f;
float worldY = ps.z / 100f - ps.y / 100f;
transform.position = new Vector3(worldX, worldY, transform.position.z);
```

### Change W-3: Clear character hold ref on throw
```csharp
_holdObj = null;
(holder as LF2Character)?.HoldWeapon(null);
result.Thrown = true;
```

### Change W-1: LF2WeaponPointFactory (new file)
```csharp
public void UpdateWeaponPoints(LF2LivingObject animator, LF2FrameData frameData, List<WeaponPoint> weaponPoints)
{
    var character = animator as LF2Character;
    foreach (var wpoint in weaponPoints)
    {
        switch (wpoint.kind)
        {
            case 1: // hold
                var weapon = character.GetHeldWeapon() as LF2WeaponBase;
                float spriteWidth = character.Sprite?.GetWidthPx() ?? 0f;
                float holdX = (character.PS.dir == "right")
                    ? character.PS.sx + wpoint.x
                    : character.PS.sx + spriteWidth - wpoint.x;
                weapon.Act(character, wpoint, new Vector3(holdX, character.PS.sy + wpoint.y, character.PS.sz));
                break;
            case 3: // force drop
                character.DropWeapon();
                break;
        }
    }
}
```

### Change W-2: Inject WPoint factory in ModuleBind
```csharp
if (WeaponPointModule != null && WeaponPointModule.Factory == null && LF2WeaponPointFactory.Instance != null)
    WeaponPointModule.SetFactory(LF2WeaponPointFactory.Instance);
```

## Review questions
1. UpdatePosition coordinate math: is `worldY = ps.z/100 - ps.y/100` correct for FLF→Unity?
2. CalcHoldPoint: is `ps.sx + spriteWidth - wpoint.x` the right mirror formula?
3. LF2Sprite.Initialize(renderer, null) — will ShowPic no-op safely with null sprites list?
4. Is kind=2 on CHARACTER frames ever seen? (FLF: kind=2 is on WEAPON frame, not character — so skipping it here is correct?)
5. Any ordering issues: does WPoint run BEFORE or AFTER the weapon's own SimTransit?
