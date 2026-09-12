<!-- CHANGE-RECORD
id: NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/Input/BattleNativeLinkedWeaponActionResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterActionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CompatWeaponActionSelectorEditorTests.cs
authority: User Goal19; current NTSD2.8-Logan input_routing.cpp; Goal14 mesh triage
evidence: Temp/Goal19_FinalSummary.json; SHARED_1778_PASS; FOCUSED_W1_771_M1_22_PASS; PLAY_PASS_SCOPED; SELFCHECK_PASS; BUILDS_0_ERROR; VALIDATOR_PASS; SCENE_UNCHANGED
-->

# NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001

Goal19 / 2026-09-12 / PLANNED / TEST_FIRST

Authority: User Goal19 batch authorization; NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033. W1: input_routing.cpp linked_stats_action and standing/running/heavy/state4/state5 live routing, playable build.ps1 includes input_routing.cpp. M1: Goal14 measured MinMaxAABB growth assertion candidate; Unity setter internal behavior remains an inference pending RED.

Scope and symbols:
- Assets/NTSD/Scripts/Simulation/Input/BattleNativeLinkedWeaponActionResolver.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterActionResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6CompatWeaponActionSelectorEditorTests.cs

W1: shared pure nine-field selector, zero/absent uses call-site fallback (20/25/45/55/50/50/35/40-or-30/52); canonical NativeLinkedAction delegates without behavior change. Legacy action selection stops consuming old bool gates. Preserve native RNG sites 0x82/83/84 and count, frame-counter/control-slot contracts, heavy movement 12/6 and 16/4, current-J dash gate, direct action writes and motion.y decrement. No schema, NTSDSpec API, input sampling, content, Scene, Gen or Plugins changes.
M1: first measure 0/1-to-many, warm growth, shrink/regrow, cross-chunk, empty recovery, finite degenerate and nonfinite guard. Only select Upload/CreateMesh initialization ordering fix after RED confirms; physical high water, inert tail, quad/index template, zero allocation and active-chunk-only Build invariant remain. No log suppression or enlarged bounds.

Current state: W1 Legacy hardcodes selector values, relation6 neutral picks52, bool fallback branches; canonical already has nonzero selector. M1 remains a defect candidate, no new reproduction yet.
Lifecycle: no new runtime service, queue, allocation ownership or teardown phase; pure helper only, existing owners unchanged.
Validation: W1 dual-profile focused plus same-seed/input/tick current-weapon Play; fixture nonzero path if content zero. M1 RED then focused, eight original SelfCheck cases/full SelfCheck and rendering Play. At batch end once: both focused, B6 964+new, prior focused, refill9, full SelfCheck, runtime/editor builds, ledger validator and Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Evidence Temp/Goal19_*.
Hard stops: existing test failure outside authorized expectation classes, scope excess, Scene drift, missing editor, needed schema/input-sampling/RNG expansion/render architecture; M1 falsification stops M1 without invented fix. No git add/commit/push.
Rollback: retain prechange hashes and reviewable diff; manual inverse patch limited to this batch only after explicit rollback authorization. Never discard existing user modifications. No irreversible changes planned.
Actual validation: editor b1b02287 via project-scoped port6404 responds, 2022.3.62f3 / NTSD_Battle / not playing or compiling; formal EXE and Scene hashes match. No RED, focused, build, Play or shared regression yet. Baseline Temp/Goal19_PrechangeBaseline.json, editor Temp/Goal19_EditorPreflight.json.

Initial test-only change: NTSD28B6CompatWeaponActionSelectorEditorTests.LinkedActionMatrix, 672 parameter cases (two profiles, seven relations, four states, four directions, absent/zero/nonzero definition stats). Production unchanged. Native fixture seed1; assertions cover action/RNG/frame counter/dash motion. Nine-field independent negative/positive/zero selector coverage, heavy movement/counter and Play remain to add. Refresh requested via existing MCP, compilation pending; no RED result yet.

RED setup correction: first run discovered 0 tests (new script not imported by scripts-only refresh), not a PASS. Full asset refresh exposed CS0117 Assert.Multiple unsupported by installed NUnit; replaced new-fixture assertion grouping with ordinary assertions, no existing tests altered. Repeat import/RED pending.

W1 measured RED: corrected-authority matrix672 = canonical336 PASS, Legacy127 PASS/209 FAIL. Evidence Temp/Goal19_W1_RED.xml and REDSummary.json; initial incorrect domain expectation remains in RED_initial.xml. Native running relation3 defaults direct85 (source1283-1311); standing/state4/state5 domain guards differ. Proceed with authorized production changes; no existing test failures observed because only new fixture executed.

Production written: new shared nine-field pure resolver/constants; canonical private enum becomes alias and NativeLinkedAction delegates (same field switch semantics). LF2CharacterActionResolver uses relation2 ownership, linked lookup without reciprocal cleanup, nine-field stats/fallbacks, native synchronized82/83/84 for registered world, native direct weapon writes, mixed-direction state5, exact heavy movement sequences/control resets. Legacy unregistered RNG remains original fallback. Three NTSDSpec table wrappers and action-only heavy wrapper removed; public IsHeavyWeapon and resolver IsHeldHeavyWeapon retained for unrelated state consumers. All changes within declared files. Inputs/sampling/schema/content unchanged. Compile and focused pending.

Main focused672/672 PASS (336 each) in Temp/Goal19_W1_GreenAttempt1.xml. Boundary suite759:755PASS/4FAIL. Two standing expectations were new-fixture errors: source1163 and canonical require current J AND buffered attack. Corrected new expectation to unchanged0. Two measured Legacy held running/state4 failures remain: current released but edge window active fails to select35/30. Fix will consume existing CdAttack mirror at these held call sites only; no sampling/helper/KeyJump rename. Bare state4 compatibility gate intentionally outside weapon change. Pure field72/72, warmed0B, both RNG arms/bool and heavy movement/control tests passed.

Buffered boundary RED6=4PASS/2FAIL saved Temp/Goal19_W1_BufferRED.xml. Changed only held running/state4 and heavy attack predicates to existing CdAttack>0; standing current gate and state5 current-J gate unchanged. No input sampling or LF2Character input helper changes. Focused759 pending rerun.

Focused759/759 PASS: Temp/Goal19_W1_Focused.xml. Added test-file-only Goal19WeaponPlayProbe: runtime-only GameConfig clone before scene load selects requested profile, live production factory Naruto2 and weapon122/123, driver tick21..52 from shared tick20 boundary, producer-boundary input fixture and post-routing observations, renderer presence. No physical keyboard claim. Harness stops polling after request, restores hooks/RNG, frees its entities through existing FreeEntityLikeExe/flush, then exits Play through existing ordered shutdown; config clone destroyed after exit. No new production runtime owner. Compile/Play pending.

Play harness compile correction: Unity event enum is EnteredEditMode; LF2Entity has no Initialize method (factory owns initialization). Both errors were new test harness only, fixed without production/API changes.

Running counter boundary RED2=canonicalPASS/LegacyFAIL7-vs0, Temp/Goal19_W1_RunCounterRED.xml. Current C++1252..1313 does not clear frame_counter, nor canonical RouteNativeRunning. Removed Legacy pre-clear only when relation!=0; bare branch unchanged. Final761 focused/Play pending.

Final explicit boundary RED10=6PASS/4LegacyFAIL: held state4 fractional/raised collision reference and heavy standing buffered jump/defend current-release gates, Temp/Goal19_W1_LastBoundaryRED.xml. Fixed held-only Y<groundY predicate and heavy jump/defend existing cooldown mirrors; reentry lock preserved. Bare state4 YInt contract remains unchanged. All production diffs still limited to declared selector/call-site files. Focused771 and Play pending.

Final focused771/771 PASS before Play (Focused771.xml). Canonical Play attempt1: current Naruto2+actual LF2Weapon122, seed424242 tick21 neutral standing produced55/counter0/RNG0, statsallzero, cleanuptrue. Harness incorrectly required per-entity Renderer in CentralOnly logic-only materialization. Preserved attempt1 JSON; changed only probe to require existing Central snapshot/current frame/catalog and record central entity command presence. No renderer architecture or production fix. Remaining Play pending.

Play attempt2 retained: action55/counter0/RNG0/cleanuptrue, actor command exists but snapshot HasCatalogKey false. Source BattlePresentationShadowBuild.BuildCommands2670 confirms deferred materialization resolves catalog into local variables without updating immutable snapshot fields. Probe now checks frame.BoundCatalogForAcceptance.TryGet(currentDAT,effectivePic) and records actual snapshot fields; this validates the current deferred publication path, not per-entity SpriteRenderer or pre-resolution flag. Production unchanged.

Play attempt3 identifies real content boundary: actor snapshot catalog resolves; OID122 frame21 pic999 has no catalog/command. Frozen projection confirms OID122/123 frame0 and21 pic999 (Temp/Goal19_W1_CurrentWeaponPic999.tsv). Probe accepts only catalog-present or explicit pic999+no-command publication, records details; does not claim visible weapon pixel proof or change content. Corrected grounded probe initial Vy to0 (air remains-3), avoiding artificial jump after ground input observation. Current weapon fallback/RNG proof remains required.

Canonical current-weapon Play final32/32 PASS, seed424242/tick21..52, cleanuptrue, runtime-only config clone. Published actors/catalog valid; weapon pic999/no-command matches frozen content. Temp/Goal19_W1_Play_DataOrientedCanonical.json. Legacy current-profile Play in progress; shared regression/builds/validator not yet run.

Dual-profile current-weapon Play32+32 PASS, 512 compared fields firstDifference=null, both cleanuptrue, GameConfig/Scene hashes unchanged (Temp/Goal19_W1_PlayComparison.json). W1 package-level focused and targeted runtime evidence acquired; RUNTIME_PENDING means batch shared regression/builds/validator still pending, not a claim of missing this targeted Play.

## Goal19 closure / 2026-09-12 / VERIFIED (scoped)

W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`: shared nine-field nonzero/fallback selector; canonical delegates without selection behavior change; Legacy relation/stats and exact held call-site gates/counters/direct writes corrected; old table bool action consumers retired, NTSDSpec API unchanged. Authority input_routing.cpp808-879/997-1093/1163-1169/1200-1313/1350-1490, formal EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; README_SOURCE + playable build inclusion checked. RED672=209FAIL/463PASS, plus separate boundary RED2/6,1/2,4/10. Final focused771/771. Real driver NTSD_Battle Naruto2 + current weapons122/123, profiles32/32 each, seed424242/tick21..52, 512 compared fields firstDifference=null, cleanuptrue. Nonzero stats remain fixture-only; current zero stats and pic999 content mean fallback/field proof, no physical keyboard or weapon pixel claim.

M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`: RED10=7PASS/3FAIL, one actual MinMaxAABB at growth plus three index-range overlap warnings and NaN accepted. Not falsified. Selected batch SetSubMeshes candidate with reusable value-only staging, current finite vertex upload first, batch only changed counts/ranges, stable active-prefix and stale-tail handling retained. No Build/CreateMesh/index template changes; no log filtering or enlarged bounds. Final focused22/22 (14new+8existing), includes original3 SelfCheck groups covering the8 reported sites, cache recovery and NaN/+Inf/-Inf. Stable high-water4096/active1,256Builds:0B and4.930078125us average in this environment. Real normal Play tick2759 observed; full1920x1080 battle image visually verified in Temp/Goal19_M1_RenderPlay_Camera.png (byte-identical to first screenshot); original composited capture2errors retained, not a mesh failure. No GPU performance claim from zero EditorStats.

Exactly one shared regression job637100deade44a0eb4bf25d0d9950b9c:1778/1778 PASS = originalB6964 + W1771 + M114 + existingbackend8 + refill9 + nativeground12. Original B6 class counts unchanged; no old case missing. Full SelfCheck requested once, fresh PASS at2026-09-12T10:09:52Z. Actual builds: `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` and Editor equivalent, both exit0/errors0, warnings47/104. Validator passes with explicit RepositoryRoot and process-only Git warning settings; first default-parameter invocation failure retained, validator source/.git unchanged. Final Console9errors =7expected registration/rest negative fixtures +2composited screenshot-tool errors; MinMaxAABB0/overlap0, do not claim Console0.

Scene NTSD_Battle remains loaded, isDirty=false/root13; SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Existing user modifications preserved; no unexpected script paths or staged files. No schema/NTSDSpec/Gen/Plugins/input sampling/content/Scene changes, no git add/commit/push. These facts close only Goal19 scope, not full battle parity or outstanding content strategy.

Authoritative batch evidence index: Temp/Goal19_FinalSummary.json; raw RED/focused/shared XML, Play comparisons, screenshots, Console, build and validator files linked there. Earlier PLANNED/CODE_WRITTEN/RUNTIME_PENDING entries are superseded by this closure, while their failures/corrections remain preserved.
