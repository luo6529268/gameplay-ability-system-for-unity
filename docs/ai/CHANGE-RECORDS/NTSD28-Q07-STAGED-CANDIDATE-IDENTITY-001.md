<!-- CHANGE-RECORD
id: NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001
status: FOCUSED_TEST_PASS
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs
authority: D-023 formal NTSD 2.8-Logan runtime resources and Q02 production Logan candidate path
evidence: EditMode job f0b2e9a7af184839922ca82c8f4f1e12 1/1 PASS after recorded raw-reference count oracle correction
-->

# NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001

Status: `FOCUSED_TEST_PASS` (2026-09-22). Parent BATCH-04/Q07. This is a test-only script change before production content activation.

Authority/requirement: D-023 formally sourced DAT and character-related images; Q07 must verify actual Unity parser/candidate construction and same-content identity, not just byte staging. Formal source and Q02 production candidate path are declared in the matching Task.

Before: Q07 staged 1,343 source-hash-matched files under `Assets/NTSD/Content/LoganRuntime`; GameConfig root remains empty. No Unity candidate capture or publication has been proven for this local staged root. Existing formal-root identity test covers the external source, not the new project-local copy.

Exact path/symbol: add `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs` with one focused NUnit test `StagedCandidateMatchesFormalRuntime`. Unity may generate only its matching `.meta`. No production behavior, old asset, Scene, config, package or assembly definition changes.

Expected side effects: Unity may import the new test and staged content into Library and create `.meta` files; it must not change active battle source, references or GameConfig. Acceptance: fresh focused test PASS with 330 entries, formal effective candidate image count matched by staged candidate, object/fusion/content/visual identity equality and source-local cache-key distinction; no compile errors, Scene hash unchanged. Failure is recorded as RED and investigated; cannot report Q07 content availability on a partial test. Rollback is confined to this new test file with exact deletion approval.

Actual change: added only `NTSD28Q07StagedCandidateIdentityEditorTests.StagedCandidateMatchesFormalRuntime` under the declared test path. It captures both formal and staged production candidates, compares object, fusion, composite and visual identities and asserts source-local cache distinction/freshness. Unity has generated its matching `.meta`; staged content import is still running. No production code, GameConfig or Scene was edited. `refresh_unity` request hit the MCP 30-second processing timeout while Unity continued importing new assets; it is not a test result. Focused Unity test and compile results remain pending. The test does not establish publication, menu/App caller success, Play, build packaging or Q07 exit; those remain separate gates.

Fresh focused RED: Unity finished import and job `503f7636325f4cf88bb7498202cfc211` ran the exact test; candidate construction for both formal and staged roots returned without exception, then the formal `Images.Count` assertion failed: expected Q01 raw indexed PNG count 1,010, actual production-effective candidate 906. The two counts have different meanings. No identity mismatch was reached/observed. Revise only this test's count oracle to the observed formal-effective 906, retaining full formal-versus-staged identity/freshness assertions, then rerun. This is test-only correction within the declared path, not a content deletion or production workaround.

Post-correction focused job `f0b2e9a7af184839922ca82c8f4f1e12`: EditMode 1/1 PASS, duration 33.76 s. Actual formal/staged candidates each admitted 330 object definitions and 906 effective images; object fingerprint, fusion input/semantic fingerprints, composite raw/semantic identity and visual fingerprint were equal; root-specific cache keys differed and both freshness checks passed. The staged set still contains the 1,010 raw indexed-reference PNGs; the 104-file difference is not classified as unused or deletion-authorized. Post-Unity-import destination SHA check remains 1343/1343, 0 missing/extra, with 1666 generated `.meta` inside the new root; current Scene SHA `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6` matches baseline. `GameConfig.asset` remains unmodified. Production publication, caller and Play checks are still required before Q07 content availability.
