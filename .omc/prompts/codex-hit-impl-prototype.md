# Task: LF2Character.Hit() Implementation Prototype

## Role
You are a senior C# game engineer porting FLF (Little Fighter 2 open-source JS engine) to Unity C#.
**Output ONLY unified diff patches. Do NOT modify any real files.**

---

## FLF Reference: `character.prototype.hit` (character.js:1893-2130)

```js
character.prototype.hit = function (ITR, att, attps, rect) {
    const $ = this
    if (!$.itr_vrest_test(att.uid)) { return false }

    let accepthit = false
    let defended = false
    let ef_dvx = 0; let ef_dvy = 0; let inj = 0

    // State 10: being caught
    if ($.state() === 10) {
        if ($.catching.caught_cpointhurtable()) {
            accepthit = true
            fall()
        }
        if ($.catching.caught_cpointhurtable() === 0 && $.catching !== att) {
            // not hurtable, skip
        } else {
            accepthit = true
            inj += Math.abs(ITR.injury)
            if (ITR.injury > 0) {
                $.effect_create(0, GC.effect.duration)
                let tar = ITR.vaction
                    ? ITR.vaction
                    : (attps.x > $.ps.x) === ($.ps.dir === 'right')
                        ? $.frame.D.cpoint.fronthurtact
                        : $.frame.D.cpoint.backhurtact
                $.trans.frame(tar, 20)
            }
        }
    } else if ($.state() === 14) {
        // lying - invincible, do nothing
    } else if ($.state() === 19 && att.state() === 3000) {
        return false // fire-run immune
    } else if (ITR.kind >= 5000 && ITR.kind < 6000) {
        // NTSD M01: direct HP deduction
        accepthit = true
        const damage = ITR.kind - 5000
        $.health.hp = Math.max(0, $.health.hp - damage)
    } else if (ITR.kind >= 6000 && ITR.kind < 7000) {
        // NTSD M02: frame jump
        accepthit = true
        const targetFrame = ITR.kind - 6000
        if ($.data.frame[targetFrame]) { $.trans.frame(targetFrame) }
    } else if (ITR.kind === undefined ||
               GC.match_itr_kind(ITR.kind, 0) ||
               GC.match_itr_kind(ITR.kind, 4) ||
               GC.match_itr_kind(ITR.kind, 9)) {
        accepthit = true
        const compen = $.ps.y === 0 ? 1 : 0
        const attdir = att.ps.vx === 0 ? att.dirh() : (att.ps.vx > 0 ? 1 : -1)
        ef_dvx = ITR.dvx ? attdir * (ITR.dvx - compen) : 0
        ef_dvy = ITR.dvy ? ITR.dvy : 0
        const effectnum = ITR.effect !== undefined ? ITR.effect : GC.default.effect.num

        if ($.state() === 13 && effectnum === 30) return false  // frozen immune weak-ice
        if (($.state() === 18 || $.state() === 19) && (effectnum === 20 || effectnum === 21)) return false  // burning immune weak-fire

        if ($.state() === 7 && (attps.x > $.ps.x) === ($.ps.dir === 'right')) {
            // defend
            if (ITR.injury)  { inj += GC.defend.injury.factor * ITR.injury }
            if (ITR.bdefend) { $.health.bdefend += ITR.bdefend }
            if ($.health.bdefend > GC.defend.break_limit) {
                $.trans.frame(112, 20)
            } else {
                $.trans.frame(111, 20)
            }
            if (ef_dvx) { ef_dvx += (ef_dvx > 0 ? -1 : 1) * util.lookup_abs(GC.defend.absorb, ef_dvx) }
            ef_dvy = 0
            if ($.health.hp - inj <= 0) { falldown() } else { defended = true }
        } else {
            // not defending
            if ($.hold.obj && $.hold.obj.type === 'heavyweapon') { $.drop_weapon(0, 0) }
            if (ITR.injury) { inj += ITR.injury }
            $.health.bdefend = 45
            fall()
        }

        let vanish = GC.effect.duration - 1
        switch ($.trans.next()) { case 111: vanish = 3; break; case 112: vanish = 4; break }
        $.effect_create(effectnum, vanish, ef_dvx, ef_dvy)
        posteffect(effectnum)

    } else if (GC.match_itr_kind(ITR.kind, 10) || ITR.kind === 11) {
        $.flute_force()
        if ($.state() === 12) { inj = ITR.injury * 2; accepthit = true }
    } else if (ITR.kind === 15) {
        $.whirlwind_force(rect)
    } else if (ITR.kind === 16) {
        $.trans.frame(200, 38); inj = ITR.injury; accepthit = true
    }

    function fall() {
        $.health.fall += (ITR.fall !== undefined) ? ITR.fall : GC.default.fall.value
        const fall = $.health.fall
        if ($.state() == 13)                   { falldown() }
        else if ($.ps.y < 0 || $.ps.vy < 0)   { falldown() }
        else if ($.health.hp - inj <= 0)       { falldown() }
        else if (fall > 0  && fall <= 20)      { $.trans.frame(220, 20) }
        else if (fall > 20 && fall <= 30)      { $.trans.frame(222, 20) }
        else if (fall > 30 && fall <= 40)      { $.trans.frame(224, 20) }
        else if (fall > 40 && fall <= 60)      { $.trans.frame(226, 20) }
        else if (GC.fall.KO < fall)            { falldown() }
    }

    function falldown() {
        if (ITR.dvy === undefined) { ef_dvy = GC.default.fall.dvy }
        $.health.fall = 0
        $.ps.vy = 0
        const front = (attps.x > $.ps.x) === ($.ps.dir === 'right')
        if (front && ITR.dvx < 0 && ITR.bdefend >= 60) { $.trans.frame(186, 21) }
        else if (front)  { $.trans.frame(180, 21) }
        else             { $.trans.frame(186, 21) }
    }

    function posteffect(effectnum) {
        if (defended) {
            if (effectnum === 0 || effectnum === 1) { sound.play('1/002') }
            return
        }
        switch (effectnum) {
            case 0: case 1:
                if ($.trans.next() === 180 || $.trans.next() === 186) { $.drop_weapon(ef_dvx, ef_dvy) }
                $.visualeffect_create(effectnum, rect, attps.x < $.ps.x, $.health.fall > 0 ? 0 : 1, true)
                break
            case 2: case 21: case 22: case 23:
                $.drop_weapon(ef_dvx, ef_dvy)
                // fallthrough
            case 20:
                $.trans.frame(203, 36); sound.play('1/070'); break
            case 3: case 30:
                $.drop_weapon(ef_dvx, ef_dvy)
                $.trans.frame($.state() !== 13 ? 200 : 182, $.state() !== 13 ? 38 : 21)
                sound.play($.state() === 13 ? '1/066' : '1/065'); break
            case 4:
                $.drop_weapon(ef_dvx, ef_dvy); break
        }
    }

    if (accepthit) {
        $.itr.attacker = att
        $.itr_vrest_update(att.uid, ITR)
    }
    $.injury(inj)
    return accepthit ? inj : false
}

character.prototype.injury = function (inj) {
    this.health.hp -= inj
    this.health.hp_lost += inj
    this.health.hp_bound -= Math.ceil(inj * 1 / 3)
    if (this.is_npc && this.itr.attacker) { this.itr.attacker.offset_attack(inj) }
}
```

---

## C# Project Context

### Existing types (DO NOT REMOVE existing fields):

**`LF2Health`** (LF2LivingObject.cs):
```csharp
public class LF2Health {
    public int HP { get; set; } = 100;
    public int MP { get; set; } = 100;
    // MISSING: HPLost, HPBound (FLF: hp_lost, hp_bound)
}
```

**`LF2HitCountersModule`** (Character/LF2HitCountersModule.cs):
```csharp
// Has: Fall (int), Bdefend (int)
// AddFall(int), ResetFall(), AddBdefend(int), ResetBdefend()
// Maps to FLF: $.health.fall, $.health.bdefend
```

**`LF2LivingObject`** key members:
```csharp
public LF2Health Health { get; }          // $.health.hp / $.health.mp
public virtual LF2HitCountersModule HitCounters => null; // $.health.fall, $.health.bdefend
public LF2EffectState Effect { get; }     // $.effect
public PhysicsState PS { get; }           // $.ps

// Methods:
public bool ItrVrestTest(int uid)         // $.itr_vrest_test(uid)
public void ItrVrestUpdate(int uid, InteractionArea itr) // $.itr_vrest_update(uid, ITR)
public virtual void EffectCreate(int num, int duration, float dvx=0, float dvy=0) // $.effect_create
public virtual void VisualEffectCreate(int num, PhysicsState.FlfVolume rect, bool righttip, int variant, bool withSound) // $.visualeffect_create
public virtual void FluteForce()          // $.flute_force()
public virtual void WhirlwindForce(PhysicsState.FlfVolume rect) // $.whirlwind_force(rect)
public virtual void DropWeapon(float dvx, float dvy) // $.drop_weapon()
public virtual bool Hit(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos, PhysicsState.FlfVolume vol) // base: only vrest check
public int GetState()                     // $.state()
public int Dirh()                         // $.dirh()
// MISSING: Attacker field ($.itr.attacker)
```

**`LF2Character`** additional members:
```csharp
public override LF2HitCountersModule HitCounters => _HitCounters; // has Fall, Bdefend
public bool caught_cpointhurtable()       // $.catching.caught_cpointhurtable()
public LF2LivingObject Catching { get; } // $.catching
public LF2WeaponBase GetHeldWeapon()     // $.hold.obj (null if no weapon)
public FrameTransistor Trans { get; }    // $.trans
// Trans.SetNext(int frame), Trans.SetWait(int wait) → $.trans.frame(frame, wait)
// Trans.Next → $.trans.next()
public LF2FrameCache FrameCache { get; } // $.data.frame[id] → FrameCache.GetFrameDataById(id) != null
```

**`NTSDGlobal`** existing constants:
```csharp
NTSDGlobal.Gameplay.FallKO = 70              // GC.fall.KO
NTSDGlobal.Gameplay.DefendBreakLimit = 60    // GC.defend.break_limit
NTSDGlobal.Default.Fall.Value = 20           // GC.default.fall.value
NTSDGlobal.Default.Fall.Dvy = -6.9f         // GC.default.fall.dvy
NTSDGlobal.Default.Effect.Num = 0            // GC.default.effect.num
NTSDGlobal.LookupAbs(table, x)              // util.lookup_abs(A, x)
// MISSING: GC.defend.injury.factor, GC.defend.absorb table, GC.effect.duration
```

**`InteractionArea`** (itr data):
```csharp
public int kind;
public int injury;
public int bdefend;
public int fall;      // may be 0 = "not set"
public int dvx;
public int dvy;
public int effect;    // may be -1 = "not set"
public int vaction;   // may be 0 = "not set"
public int vrest;
public int arest;
```

**`LF2StandardFrames`** constants:
```csharp
Defend1 = 111, DefendBroken = 112
Injured = 220, Injured2 = 222, Injured4 = 224, Injured6 = 226
FallingFront = 180, FallingBack = 186
Frozen = 200, Burning = 203
```

**`BruteForceSceneQuery.MatchItrKind`** - currently PRIVATE static. FLF `GC.match_itr_kind` needs a PUBLIC version.

---

## Required Deliverables (unified diff only, no real file changes)

### 1. `NTSDGlobal.cs` - add missing constants
- `GC.defend.injury.factor` = 0.5f
- `GC.defend.absorb` table (FLF: `{15: 5}` meaning: if |ef_dvx| >= 15, absorb 5)
- `GC.effect.duration` = 20 (FLF default effect vanish duration)

### 2. `LF2Health.cs` or `LF2LivingObject.cs` - add to `LF2Health`
- `HPLost` (int, FLF: `hp_lost`)
- `HPBound` (int, FLF: `hp_bound`, starts equal to HP)

### 3. `LF2LivingObject.cs` - add `Attacker` field
- `public LF2LivingObject Attacker { get; set; }` (FLF: `$.itr.attacker`)

### 4. `LF2LivingObject.cs` - add public static `MatchItrKind`
- Public static version of the itr kind matching (GC.match_itr_kind) with same table as BruteForceSceneQuery

### 5. `LF2Character.cs` or new partial file - `LF2Character.Hit()` override
Full implementation translating FLF `character.prototype.hit` to C#:
- All state checks (10, 14, 19+3000)
- NTSD kind 5000-5999, 6000-6999
- kind 0/4/9 main branch (defend + fall/falldown + posteffect)
- kind 10/11 (flute), kind 15 (whirlwind), kind 16 (freeze)
- Local helpers as private methods: `Fall()`, `Falldown()`, `PostEffect()`
  (These local helpers are called with captured closure variables in JS → in C# use private methods with parameters or local functions)
- `Injury(inj)` call at end
- `ItrVrestUpdate` when accepthit=true

### 6. `LF2Character.cs` or `LF2LivingObject.cs` - `Injury()` method
```csharp
// FLF: character.prototype.injury
protected virtual void Injury(int inj) {
    Health.HP -= inj;
    Health.HPLost += inj;
    Health.HPBound -= Mathf.CeilToInt(inj / 3f);
}
```

---

## Important Notes for Code Generation
- C# return type is `bool` (not `int inj`), `true` = accepthit, `false` = rejected
- `Trans.SetNext(frame)` + `Trans.SetWait(wait)` replaces `$.trans.frame(frame, wait)`
- `itr.effect == -1` (or some sentinel) means "not set" → use `NTSDGlobal.Default.Effect.Num`
- `itr.fall == 0` means "not set" → use `NTSDGlobal.Default.Fall.Value` (check if field is 0-means-default vs actual 0)
- `itr.dvy == 0` means "not set" → use `NTSDGlobal.Default.Fall.Dvy`
- Keep all local functions (`fall`, `falldown`, `posteffect`) as C# local functions or private methods with ref parameters
- Sound calls are stubs for now (PlaySound is TODO)
- Do NOT refactor or change any existing logic — add only what's needed
