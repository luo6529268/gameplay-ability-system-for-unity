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

## Goal19 closure / 2026-09-12 / VERIFIED (scoped)

W1 `NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001`: shared nine-field nonzero/fallback selector; canonical delegates without selection behavior change; Legacy relation/stats and exact held call-site gates/counters/direct writes corrected; old table bool action consumers retired, NTSDSpec API unchanged. Authority input_routing.cpp808-879/997-1093/1163-1169/1200-1313/1350-1490, formal EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; README_SOURCE + playable build inclusion checked. RED672=209FAIL/463PASS, plus separate boundary RED2/6,1/2,4/10. Final focused771/771. Real driver NTSD_Battle Naruto2 + current weapons122/123, profiles32/32 each, seed424242/tick21..52, 512 compared fields firstDifference=null, cleanuptrue. Nonzero stats remain fixture-only; current zero stats and pic999 content mean fallback/field proof, no physical keyboard or weapon pixel claim.

M1 `NTSD-BATTLE-MESH-SUBMESH-GROWTH-INITIALIZATION-001`: RED10=7PASS/3FAIL, one actual MinMaxAABB at growth plus three index-range overlap warnings and NaN accepted. Not falsified. Selected batch SetSubMeshes candidate with reusable value-only staging, current finite vertex upload first, batch only changed counts/ranges, stable active-prefix and stale-tail handling retained. No Build/CreateMesh/index template changes; no log filtering or enlarged bounds. Final focused22/22 (14new+8existing), includes original3 SelfCheck groups covering the8 reported sites, cache recovery and NaN/+Inf/-Inf. Stable high-water4096/active1,256Builds:0B and4.930078125us average in this environment. Real normal Play tick2759 observed; full1920x1080 battle image visually verified in Temp/Goal19_M1_RenderPlay_Camera.png (byte-identical to first screenshot); original composited capture2errors retained, not a mesh failure. No GPU performance claim from zero EditorStats.

Exactly one shared regression job637100deade44a0eb4bf25d0d9950b9c:1778/1778 PASS = originalB6964 + W1771 + M114 + existingbackend8 + refill9 + nativeground12. Original B6 class counts unchanged; no old case missing. Full SelfCheck requested once, fresh PASS at2026-09-12T10:09:52Z. Actual builds: `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` and Editor equivalent, both exit0/errors0, warnings47/104. Validator passes with explicit RepositoryRoot and process-only Git warning settings; first default-parameter invocation failure retained, validator source/.git unchanged. Final Console9errors =7expected registration/rest negative fixtures +2composited screenshot-tool errors; MinMaxAABB0/overlap0, do not claim Console0.

Scene NTSD_Battle remains loaded, isDirty=false/root13; SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. Existing user modifications preserved; no unexpected script paths or staged files. No schema/NTSDSpec/Gen/Plugins/input sampling/content/Scene changes, no git add/commit/push. These facts close only Goal19 scope, not full battle parity or outstanding content strategy.

Authoritative batch evidence index: Temp/Goal19_FinalSummary.json; raw RED/focused/shared XML, Play comparisons, screenshots, Console, build and validator files linked there. Earlier PLANNED/CODE_WRITTEN/RUNTIME_PENDING entries are superseded by this closure, while their failures/corrections remain preserved.
