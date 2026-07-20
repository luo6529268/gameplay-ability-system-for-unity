# UNRES.02-05 Final Code-Parity Verification (2026-07-18)

## Scope and rule

This is a read-only source comparison. The sole authority is
`J:\QQFile\NTSD2.4\ntsd_release_C#`. It does not use Play Mode, resources,
presentation intent, or inferred design intent as evidence. A suspicious label,
unreachable branch, or possible authority typo is classified as code-equivalent
when Unity preserves the authority expression and ordering. Only a missing or
different reachable expression/write is classified as a confirmed difference.

## Final status

| ID | Code-level status | Result |
|---|---|---|
| `UNRES.02` | **equivalent** | The label-only AI helper ownership, call order, OID groups, thresholds, RNG calls, writes, short-circuit returns, and final input-edge application match. The labels themselves have no additional runtime contract. |
| `UNRES.03` | **equivalent** | Unity preserves both unusual frame expressions: the negative `mpDelta` comparison and the `value > 500` branch before `value == 550`. No correction based on inferred intent is required. |
| `UNRES.04` | **confirmed-difference** | `Unk360` cache lifecycle and the reset/consume/clear lifecycle of `Unk3FC`/`Unk400` match, but Unity is missing the authority's reachable `triggerCode == 100` broadcast writes to `Unk3FC` and `Unk400`. |
| `UNRES.05` | **equivalent** | Unity preserves the authority's target/self relation test in the AI line-cover loop, including the possibly suspicious use of `target` rather than `cand`; state, HP, Y, Z, and X-between filters remain in the same order. |

## Evidence

### UNRES.02: label-only helper ownership

Authority `InputRuntime.cs:478-494` invokes, in order,
`AiUpdateOid33_19_16PredictedDuaDecision`,
`AiUpdateOid52_1_2_21PreLabel591Decision`, then
`AiUpdateLabel591Oid51_2_18_7Decision`; each successful helper applies input
edges and returns before the movement/held-object paths. Unity
`SimulationWorld.AiInput.partial.cs:249-263` preserves that same order and
short-circuit boundary.

The helper bodies also match:

- Authority `InputRuntime.cs:1742-1796` and Unity
  `SimulationWorld.AiInput.partial.cs:544-563` use OIDs `52/1/2/21`, the same
  `targetState` gates, PP limits, RNG divisors (`10/5/14/5/5`), X/Z thresholds,
  facing test, combo writes, and returns.
- Authority `InputRuntime.cs:1798-1837` and Unity
  `SimulationWorld.AiInput.partial.cs:565-578` use OIDs `51/2/18/7`, frame range
  `266..279`, the same target-DAT condition, PP/RNG/distance gates, input/combo
  writes, and returns.
- Authority calls `AiProcessSubLabel435PressurePrewrite` after
  `AiProcessSubCallerPrewrite` and before `AiProcessSubHelper`
  (`InputRuntime.cs:602-606`). Unity preserves the same placement
  (`SimulationWorld.AiInput.partial.cs:303-306`) and the helper's pressure,
  phase, character-DAT, slot, team, coordinate, state-7, history-window, retreat,
  defend-RNG, and write ordering (`InputRuntime.cs:2242-2286` versus Unity
  `:755-776`).

No enum, field, state transition, or side effect is attached to the words
`Label591` or `Label435`; they only name these preserved blocks. Therefore this
is code-equivalent without assigning a guessed business meaning.

### UNRES.03: negative mpDelta and value 550

For negative frame MP, authority `FrameTick.cs:178-200` performs:

1. `mpDelta = frame.Mp` under `Frame < 400`.
2. Enter only for `mpDelta < 0 && PpMode`.
3. Test the literal expression `entity.Pp < mpDelta`.
4. Either jump to `HitD` and reload the frame, or add `mpDelta` and refund
   display by `-mpDelta`.
5. Apply the post-reload `HitD` turn branch.

Unity `LF2Entity.cs:4943-4976` preserves the same effective guards, literal
`Health.PP < mpDelta` expression, frame reload, PP/display writes, and turn
ordering. The early returns (`!PpMode`, frame `>= 400`, `mpDelta >= 0`) are the
structured equivalent of the authority's nested conditions.

For axis velocity, authority `FrameAdvance.cs:1019-1060` first handles
`value > 500` by assigning `value - 550` and returning, then tests
`value == 550`. Unity `LF2Entity.cs:5642-5681` preserves the same branch order,
constants, assignment, and return. Consequently the `value == 550` block is
unreachable in both implementations and is still code-equivalent; no inferred
correction is authorized.

### UNRES.04: Unk360 / Unk3FC / Unk400 lifecycle

The shared parts match:

- Defaults/reset are `Unk360=-1`, `Unk3FC=-1000`, `Unk400=-1000` in authority
  `NtsdEntityRuntime.cs:326-340,371-385` and Unity
  `NTSDEntityRuntime.cs:137-145,441-449`.
- The nearest-target cache reads and writes `Unk360` in authority
  `InputRuntime.cs:47-69,255` and Unity
  `SimulationWorld.AiInput.partial.cs:57-64,192`.
- Coordinate-mode selection, movement thresholds (`6/250/100`, Z `3`),
  completion radius `0x5A`/`90`, and sentinel clear writes match between
  authority `InputRuntime.cs:19-24,1611-1646,1680-1690` and Unity
  `SimulationWorld.AiInput.partial.cs:47-52,472-489,514-516`.

The reachable producer does not match. Authority
`GameTick.cs:1681-1750` recognizes input history `9,0,9,0`, creates OID 998 with
`triggerCode=100`, then iterates active living character-DAT entities with the
same `Unk364`. For each one it writes:

```csharp
target.Unk3FC = spawned.XInt + (NtsdRng.Rand() % 0x51) - 0x28;
target.Unk400 = spawned.ZInt + (NtsdRng.Rand() % 0x51) - 0x28;
```

Unity's current generic production path recognizes the same history and creates
OID 998 in `LF2Entity.TryResolveLateN30InputTriggerCode` and
`RunLateCharacterDatInputTrigger` (`LF2Entity.cs:3419-3469`). Its broadcast
helper `ApplyLateN30HistoryGateBroadcast` (`:3526-3558`) returns unless the code
is `102` or `104`; it has no `100` branch and performs no `Unk3FC`/`Unk400`
coordinate writes. A production-wide search finds no other corresponding
non-test assignment; the only Unity writes are reset, AI completion clear, and
test fixture setup. The older CLR-character-specific late helper also creates
998 without the coordinate broadcast and does not close this gap.

This is therefore a confirmed code difference, not an unresolved field-name or
Play Mode question.

### UNRES.05: label/state and line-cover relation

Authority `InputRuntime.cs:2288-2321` filters the first 20 runtime slots by:
self exclusion, active/CharData, `cand.Unk364 != 0`, the literal
`target.Unk364 != self.Unk364` relation test, living HP, state not 14, absolute
Y at most 2, Z distance below 15, and candidate X between self and target.

Unity `SimulationWorld.AiInput.partial.cs:778-796` preserves the same filters
and order. `Team(e)` is defined at `:368` as `e.Runtime.RelationTeam`, the Unity
mapping of authority `Unk364`; therefore `Team(target) != Team(self)` is the same
target/self test, not a candidate-team correction. The following state-2 RNG
branch also preserves defend when line-covered and jump otherwise.

Even if the authority target/self relation check was historically intended to
reference `cand`, that intention is not part of the source contract. Unity
faithfully retains the observable code and is therefore equivalent.

## Verification boundary

This report identifies one code defect (`UNRES.04`) and closes the other three
items as code-equivalent. It makes no Play Mode, resource, rendering, or full
battle-alignment claim and changes no production source or project document.
