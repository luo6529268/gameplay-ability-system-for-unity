# D-024 target-history velocity coordinate audit

Status: `ORIGINAL_EDITOR_PRODUCTION_MECHANICS_FIRST_DIFFERENCE_CONFIRMED / FULL_DRIVER_AND_EXE_PENDING` (2026-09-24). The source calculation has now been checked with actual registered World entities, production character mechanics movement, formal-DAT-loaded OID219 child birth, and production non-character mechanics movement in the original Editor. It is not a full Driver/Play or formal EXE visible trace.

2026-09-24 same-current-coordinate control: original Editor job `d248af5583144fa4822db084fa43cee3` passed the focused class 11/11 after explicit integer-gap assertions. A newly spawned target at Unity X324 with source X100 has current integer gap224 and formal initial gap224, so formal behavior5 velocity is `224/50=4`. The independently moved target starts at X251, then production character mechanics adds `48*2048/1333` to its precise X and syncs the integer mirror to X324. Its current Unity integer gap is also224, but its formal history is initial151+48=199, so formal behavior5 velocity is `199/50=3`. Unity currently produces4 in both histories. The test asserts both the static-gap224 control and the moved-history integer gap224; it is a defect characterization, not a parity exit.

This pair proves that a memoryless conversion `f(currentTargetX-currentChildX, viewScale)` cannot give the correct formal velocity for both histories. The two inputs to such a function are identical, but the required formal outputs differ. An implementation must retain or reconstruct enough per-entity coordinate history, or use an equivalent explicit two-domain position contract. It may not infer the source-domain gap from one current World gap by simple multiplication/division.

Authority and active code:

- Formal indexed `resources/runtime/decoded_dat/data/data.txt` contains OID219 `w/e.dat`; frame51 has `hit_Fa: 5`.
- Paired playable `source/ntsd28_core/src/simulation/native_ai.cpp`, `NativeAi28::step_non_character_hit_fa`, behavior5, creates the child at the source position and writes `child->motion.x = double((friendly->position.x - child->position.x) / 50)`. The integer division happens before conversion.
- Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`, `RunHitFa5FrameLogic`, now matches that integer division for the *current* integer World gap. `CharacterMechanics.StepNonCharacterBattleLogic` then applies `world.FixedViewRunDistanceScale` to X travel. The 2026-09-24 focused original-Editor equal-position test passed 8/8, including the indexed DAT frame and one production mechanics step.

The equal-position test does not cover a target that has already moved under D-024. At 16:9 Training reference, `Sx=2048/1333=1.536384096...`. For a static source/child X=100, target initially X=251, and one or two subsequent target movements each equal to formal 48px, the current formula gives:

| Target history | Formal integer gap | Unity current integer gap | Formal raw child Vx | Unity raw child Vx | Formal screen fraction next step | Unity screen fraction next step |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| No movement | 151 | 151 | 3 | 3 | 3/1333 | 3/1333 |
| One 48px movement | 199 | 224 | 3 | 4 | 3/1333 | 4/1333 |
| Two 48px movements | 247 | 298 | 4 | 5 | 4/1333 | 5/1333 |

The Unity gaps are `int(151+n*48*Sx)`, matching the current integer mirror's truncation for positive positions. Original-Editor focused job `92b7728e66144755aafa43f9b4e8deb0` finished 10/10 PASS: the two new characterization cases used `CharacterMechanics.StepBattleLogic` for one/two 48px target movement steps in a configured registered World, then called the production frame logic to birth OID219 from the staged formal `w/e.dat` frame51 and stepped the child through production non-character mechanics. Both measured the predicted 3/4 and 4/5 formal/current velocity divergence; four equal-start birth controls and four earlier ratio controls also passed. These cases deliberately characterize a known defect, rather than certify parity. Full Driver action/pass sequencing, actual stage bounds, presentation and formal EXE are still pending.

Crucial coordinate ambiguity: current absolute spawn gaps remain raw, while movement deltas are scaled. Therefore the same current World gap can correspond to different formal histories. A blanket inverse factor on `(targetX-childX)` would fix some moved histories but break the previously passing equal-start case. Resolve this by identifying or introducing a consistent source-coordinate/position-history contract, and audit target selection, collision/reach and snap/teleport consumers before changing production logic. Do not edit DAT data, camera, or the nonbattle framework for this diagnosis.

Any production contract must explicitly assign both the current D-024 scaled battle position and the source-rule reference position at entity birth, each physical motion outlet, direct frame displacement, target-relative teleport, parent-relative OPoint/fragment birth, held/caught attachment, stage-absolute spawn, stage clamp, death/revival and object-pool reset. Reference integer rounding must match the playable source before `/50`. Because `NTSDEntityRuntime` is copied by `TryCopyCanonicalStateTo` into lockstep snapshots and reset by `Reset()`, any new history carrier must be copied/reset and included in checksum or equivalent replay invariants; a local producer cache is insufficient. The battle positions used by collision, AI and presentation must stay aligned with the user's accepted enlarged actual battle distance. These are design obligations, not evidence that the implementation exists.

Exit remains open: full Driver/Play dynamic case, formal same-input trace/observable result, a coherent production coordinate-history implementation, and adjacent initial-gap regression after that implementation. Q07 is still behind D-024's requested non-perceptual audit.

The required two-domain state and writer/replay gates are now organized in `COORDINATE-HISTORY-CONTRACT-DESIGN.md`. That file is a design gate, not an implementation or a parity certificate.
