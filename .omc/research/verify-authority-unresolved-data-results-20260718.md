# Authority-Unresolved Data / Results Verification (2026-07-18)

## Scope

This is a read-only source verification of `DEP.DATA.01` and the results-slot/relation-identity dependency. It compares the formal C# runtime source at `J:\\QQFile\\NTSD2.4\\ntsd_release_C#` with the Unity scripts. It does not compare raw DAT files, does not deploy `stage.dat`, and does not perform Play Mode validation.

The purpose is to turn code-contract questions into one of three states:

- **confirmed code difference**: the two source paths have different defaults, capacity, or reachable behavior;
- **Unity adapter / masked**: representation differs but the inspected production consumer normalizes the value to the authority default;
- **closed in source**: the relevant fields and consumers are equivalent in the inspected path.

## Executive Result

The original `DEP.DATA.01` item is not a single unresolved question. The source audit splits it into four confirmed code-level differences, one adapter-level default difference currently masked by consumers, and two closed mappings.

The results item `FW-RESULT-01` also cannot remain wholly `authority-unresolved`: the normal two-team initial path is structurally equivalent after `AppManager` sets `RelationTeam`, but Unity has a different dormant/inactive selection rule and uses `RelationTeam` as an implicit alias for authority `Unk364`. Those are open code differences for non-normal roster/lifecycle states.

## DAT Parser / Population

### DATA-01A: movement default `running_speed` is different (confirmed code difference)

| Side | Evidence |
|---|---|
| Authority | `src/Data/DatModels.cs:160-177` declares `RunningSpeed = 8.0`, with the other movement defaults (`WalkingSpeed=4`, `RunningSpeedZ=3.3`, heavy/jump/dash/rowing values). `src/Data/DatLoader.cs:272-285` overwrites the value only when the DAT contains `running_speed`. |
| Unity | `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs:19-48` declares `running_speed = 15.0f`; `CharacterAnimtorManager.cs:694-782` overwrites it only when a parsed property exists. |
| Consumer | `LF2Entity.cs:2895-2901` and `LF2CharacterActionResolver.cs:186-192` use `characterData.running_speed` directly for run velocity. |

If a DAT/fixture/configuration path omits the property, Unity runs at 15 while authority runs at 8. This is independent of raw DAT representation and is a script-side contract difference.

### DATA-01B: frame index capacity is 400 in Unity versus 600 in authority (confirmed code difference)

| Side | Evidence |
|---|---|
| Authority | `src/Data/DatModels.cs:145,181-195` defines `MaxFrameId = 600`, keeps a 600-entry `FrameIndex`, and treats ids `0..599` as valid. |
| Unity | `Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs:12,16,40-43` allocates only 400 entries and writes `_frames[frameData.frameId]` without a bounds check. `GetFrameDataById` (`:57-61`) rejects ids `>=400`. |
| Consumer | `SimulationWorld.Passes.partial.cs:202-204` explicitly rejects `partnerFrameId >= 400`; the frame/action and hit resolvers call `GetFrameDataById` throughout the battle path. |

Frame ids 400-599 are valid in the authority model but unavailable to Unity. This can either throw during cache load or silently return null on lookup, depending on the path.

### DATA-01C: legal missing-frame semantics differ (confirmed code difference)

| Side | Evidence |
|---|---|
| Authority | `CharData.GetFrameOrNull` (`src/Data/DatModels.cs:185-191`) returns `null` only for out-of-range ids; an in-range id with `FrameIndex[id] == -1` returns a shared default `EmptyFrame` (`Wait=1`, other values default). `HasFrame` (`:193-196`) separately reports whether a concrete frame exists. |
| Unity | `LF2FrameCache.GetFrameDataById` (`Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs:57-61`) returns `null` for both out-of-range and legal-but-unpopulated frame ids; `HasFrame` is only a null test (`:63-66`). |
| Consumer | Unity frame advance, collision and input code frequently uses null guards or skips logic after `GetFrameDataById`; authority passes receive a non-null `EmptyFrame` for an in-range missing id. |

This is not a raw DAT difference. It changes the contract whenever a valid frame reference is absent from the populated list.

### DATA-01D: cpoint front/back action aliases are not populated in Unity (confirmed code difference)

| Side | Evidence |
|---|---|
| Authority | `src/Data/DatLoader.cs:821-864` parses `fronthurtact` and assigns both `CPointData.Fronthurtact` and `CPointData.Injury`; `backhurtact` assigns both `Backhurtact` and `Cover`. The runtime reads `CPointData.Injury/Cover` in `CPointRuntime.cs:219,283-284` and reads the directional fields in `HitResolve.cs:1206-1218`. |
| Unity | `Lf2DatConverter.ConvertToCatchPoint` (`Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs:160-193`) parses `fronthurtact`/`backhurtact` only into `CatchPoint.fronthurtact`/`backhurtact`; it does not mirror those values to `CatchPoint.injury`/`cover`. |
| Consumer | `LF2CharacterCatchResolver.cs:226-227` and `LF2Entity.cs:4730-4768` consume `cpoint.cover` and `cpoint.injury` for throw direction/injury. |

For cpoints that provide the standard front/back action fields, Unity can therefore leave the injury/cover values at zero while authority has the alias values. This is a reachable battle logic difference in the parser/population layer.

### DATA-01E: itr `zwidth` default differs but current consumers mask it (Unity adapter / masked)

Authority `ItrData.Zwidth` defaults to `15` (`src/Data/DatModels.cs:7-35`). Unity `InteractionArea.zwidth` defaults to `0` (`Assets/NTSD/Scripts/Animation/LF2FrameData.cs:204-220`). The inspected Unity geometry consumers explicitly substitute 15 when the parsed value is zero (`BruteForceSceneQuery.cs:1240`, `PhysicsState.cs:280,306`), so the current production collision volume is adapter-equivalent for omitted `zwidth`. The stored model field remains different and should not be treated as proof that every future consumer is safe.

### DATA-01F: frame-level `vaction` is absent in Unity but has no current authority consumer (schema-only omission)

Authority `FrameData` contains `Vaction` (`src/Data/DatModels.cs:99-129`) and `DatLoader.ParseFrameBlock` reads it (`src/Data/DatLoader.cs:631-634`). Unity `LF2FrameData` has no frame-level `vaction` member and `Lf2DatConverter` does not populate one. A production search found no authority read of `frame.Vaction`; cpoint `Vaction` is a separate field and is present in Unity. This is a schema gap, not a currently demonstrated battle behavior difference.

### DATA-01G: frame/opoint/itr primary field mapping is otherwise closed in source

The authority model (`DatModels.cs:45-129`) and Unity converter (`Lf2DatConverter.cs:17-105,112-133,262-304`) map frame scalar fields, opoint fields, itr fields, bdy fields, wpoint fields, and cpoint fields by the same names. Opoint defaults are zero on both sides. Unity retains both `opoint` (first item compatibility property) and `opoints` (full list); the runtime spawn path consumes the list. No additional code-level difference was found in this mapping.

## Results Slot / Relation Identity

### RESULT-01: dormant/inactive and relation identity rules differ (confirmed code difference for non-normal states)

| Side | Evidence |
|---|---|
| Authority | `src/BattleCore/Simulation/GameTick.cs:509-586` loops fixed indices `0..7` of `BattleSlotEntity`. It skips only when `BattleSlotState[slotIndex] == 0 && !entity.Active` (`:520-527`), then derives team as `entity.Unk364 != 0 ? entity.Unk364 : BattleSlotTeam[slotIndex]` (`:529`). Alive count requires `entity.Active && entity.Hp > 0` (`:549-550`). |
| Unity | `Assets/NTSD/Scripts/Simulation/SimulationWorld.StageRender.partial.cs:174-265` loops roster slots (`:185-200`), skips any inactive roster slot before entity lookup (`:196-199`), finds entities including dormant slots (`:200`), and derives team as `entity.RelationTeam != 0 ? entity.RelationTeam : rosterSlot.Team` (`:207`). Alive count additionally requires `IsActiveForCurrentPass(entity)` (`:227`). |
| Bootstrap | `Assets/NTSD/Scripts/App/AppManager.cs:224-243` sets `Team` and `RelationTeam`, but does not set `Unk344`; Unity therefore treats `RelationTeam` as an implicit alias rather than writing the authority result identity field. |

For the normal active two-player path, `RelationTeam` and roster team are initialized to the same value and the result is equivalent. For slot state changes, dormant entities, relation-team changes, or an entity whose `Unk344` and `RelationTeam` diverge, Unity and authority can select different result teams/alive counts. This item should remain open as a code difference until the identity alias and dormant semantics are explicitly aligned.

## Items Closed by This Verification

- `DATA.FRAME.01`, `DATA.OP.01`, `DATA.CP.01` scalar field names and ordinary defaults: closed for fields directly consumed by the current runtime, except the cpoint aliases in DATA-01D.
- `DATA.FRAME.02`: not equivalent because Unity conflates legal missing frames with out-of-range null; see DATA-01C.
- Raw DAT file/manifest differences remain outside scope as requested.
- Play Mode scenarios remain outside this report and are intentionally left for the user.

## Required Follow-up

The following source differences should be entered into the frozen code-difference inventory before production repair begins:

1. DATA-01A: set Unity's movement default contract to authority values (especially `running_speed=8.0`).
2. DATA-01B: raise Unity frame cache capacity to the authority bound and guard cache population.
3. DATA-01C: provide an authority-compatible legal-missing `EmptyFrame` path or prove every Unity consumer intentionally treats null as the same default.
4. DATA-01D: mirror cpoint front/back action fields into `injury/cover` during conversion, matching authority parser population.
5. RESULT-01: align results slot selection and relation identity (`Unk344`/`RelationTeam`) for dormant and slot-state transitions.

No production code was changed while producing this report.
