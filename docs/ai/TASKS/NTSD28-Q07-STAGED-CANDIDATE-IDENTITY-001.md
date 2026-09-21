# NTSD28-Q07-STAGED-CANDIDATE-IDENTITY-001

Status: `IN_PROGRESS`. Parent BATCH-04/Q07; prerequisite `NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001` verified for exact bytes.

Authority: formal NTSD 2.8-Logan release runtime at `J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime`; source identity and D-023 in CURRENT-AUTHORITY. Q02 `BattleContentSource`, `LoganObjectCatalog`, `LoganVisualContentCandidate` are the production candidate paths. This task checks the **staged project-local copy** against that formal root and does not activate production content.

Declared script path: only new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs` and its Unity-generated `.meta`. No production script, GameConfig asset, Scene, old resources or nonbattle module may change. Test uses `Application.dataPath` for the staged root and the already established formal source path. It must capture both candidates with real production methods; assert 330 object entries, expected 1,010 unique images, equal object/fusion/content/visual fingerprints, and freshness checks. Source cache keys should differ because roots differ. Do not use copied-file hash equality as a substitute for parser/candidate construction.

Validation: run only this new EditMode test in the existing Editor, inspect actual test-job result and compile status. If RED, inspect first failing production contract or manifest path and fix only through a separately declared scope; do not weaken the test or switch GameConfig. No broad Q06 test matrix is needed for this test-only addition. Scene hash and GameConfig root must remain unchanged.

Rollback: leave existing content and code untouched; any test removal follows exact file deletion approval. This package can be superseded by corrected test code under this Change ID if the correction is confined to the declared test path and documented.
