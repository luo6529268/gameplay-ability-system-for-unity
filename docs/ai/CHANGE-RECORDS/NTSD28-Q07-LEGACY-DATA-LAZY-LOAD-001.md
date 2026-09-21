<!-- CHANGE-RECORD
id: NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001
status: VERIFIED
change-kind: BATTLE_CONTENT_INITIALIZATION_AND_TEST
code-path: Assets/NTSD/Scripts/Animation/GameDataManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07LegacyDataLazyLoadEditorTests.cs
authority: D-023 formal Logan content and Q07 legacy data dynamic reachability audit
evidence: focused EditMode 030ecccb and 8eb24dc8 PASS; publication ccab4472 PASS; q07-lazy-menu-1 Play PASS; ACCEPTANCE.md
-->

# NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001

Before: manager singleton initialization always calls `LoadDataFile()` on old `Assets/NTSD/Config/data.txt`. Formal candidate publication replaces objects later, but the old read remains reachable and its empty background list is carried forward. Direct Battle `GameConfig` setup in `Start` means an `Awake`-time root condition is not reliable.

Planned code: remove the unconditional load in `GameDataManager.InitializeSingleton` and add a focused Editor test for no implicit read plus explicit legacy load. Formal prewarm already supplies a candidate to `PrepareObjectPublication`/`CommitObjectPublication`; empty-root `ParseCharacterFrameConfigs` explicitly calls `LoadDataFile(fullDataPath)`. Preserve both APIs and all object publication semantics. This does not solve Q08 formal background count or authorize old-file deletion.

Expected side effects: formal-root startup no longer reads the old object index solely because the singleton awakens. Empty-root loading remains at the explicit legacy parser entry. A caller relying on pre-prewarm singleton data could change; inspect usages and focused runtime acceptance before closure.

Invariants: formal EXE/content identity, 33 ms logic, ordered shutdown, Scene and old resources, Unity/GAS and nonbattle behavior remain unchanged. Rollback: restore only the removed initialization call and the test in this Change ID after recording failure; no broad reset.

Actual edits: removed the `GameDataManager.InitializeSingleton` override, including its unconditional old `data.txt` load; added one Editor test for unloaded-after-Awake and explicit legacy load. No Scene, resource or nonbattle production file changed.

Validation: focused EditMode jobs `030ecccb9a00407fb3bd481cc5612159` and final-source `8eb24dc8a39647aa8d55f0115f6f4140` both 1/1 PASS. Complete formal publication job `ccab44720a734051bc4ca2dbbc58b29f` passed 1/1 with 330 objects/906 effective images. Serialized-root menu Play `q07-lazy-menu-1.json` PASS: three-owner publication, World4, owner survivors0, borrowers0, `RuntimeMapCleared`, two Stopped frames. Publication and Play ran before deleting a base-only empty override; final source was recompiled and its focused test rerun. Change Ledger 652 records/15 governed files PASS; Scene hash/clean status unchanged. Full evidence: `artifacts/diagnostics/NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001/ACCEPTANCE.md`.

Remaining boundary: Q08 background/result rule and old-resource deletion remain open; empty-root explicit legacy load and editor/test callers remain supported.
