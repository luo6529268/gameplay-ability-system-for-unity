# NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001

Status: `PLANNED`. Parent BATCH-04/Q07. This is a test-only script change before production content activation.

Authority/requirement: D-023 formally sourced DAT and character-related images; Q07 must verify actual Unity parser/candidate construction and same-content identity, not just byte staging. Formal source and Q02 production candidate path are declared in the matching Task.

Before: Q07 staged 1,343 source-hash-matched files under `Assets/NTSD/Content/LoganRuntime`; GameConfig root remains empty. No Unity candidate capture or publication has been proven for this local staged root. Existing formal-root identity test covers the external source, not the new project-local copy.

Exact path/symbol: add `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs` with one focused NUnit test `StagedCandidateMatchesFormalRuntime`. Unity may generate only its matching `.meta`. No production behavior, old asset, Scene, config, package or assembly definition changes.

Expected side effects: Unity may import the new test and staged content into Library and create `.meta` files; it must not change active battle source, references or GameConfig. Acceptance: fresh focused test PASS with 330 entries, 1,010 images, object/fusion/content/visual identity equality and source-local cache-key distinction; no compile errors, Scene hash unchanged. Failure is recorded as RED and investigated; cannot report Q07 content availability on a partial test. Rollback is confined to this new test file with exact deletion approval.

Validation commands/results and actual changes: pending. The test does not establish publication, menu/App caller success, Play, build packaging or Q07 exit; those remain separate gates.
