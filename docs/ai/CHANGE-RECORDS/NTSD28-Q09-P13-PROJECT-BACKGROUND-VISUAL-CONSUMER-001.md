<!-- CHANGE-RECORD
id: NTSD28-Q09-P13-PROJECT-BACKGROUND-VISUAL-CONSUMER-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeBackgroundEditorTests.cs
authority: formal root NTSD2.8-Logan EXE paired playable NativeEarthquake28 and D3D11 background-only draw with user fixed project-map exception
evidence: docs/ai/TASKS/NTSD28-Q09-P13-PROJECT-BACKGROUND-VISUAL-CONSUMER-001.md
-->

# NTSD28-Q09-P13-PROJECT-BACKGROUND-VISUAL-CONSUMER-001

`RUNTIME_PENDING` after scoped implementation. This Record and Task were created as `PLANNED` before edits. The Map Sprite is the camera framing source, so moving it would couple earthquake to the user's fixed camera. This package draws that same Sprite with a transient vertex offset from the already-frozen battle frame and restores its original renderer material at offset zero, world-camera end, or component shutdown. Production change is limited to `BattleBackgroundPlatformPresentation.cs` and `BattleEarthquakeBackground.shader`; one new Editor test validates GPU behavior. No native background/mode DAT or nonbattle path is included.

The exact shader asset `Assets/NTSD/Resources/BattleEarthquakeBackground.shader` and its Unity `.meta` are owned by this Record and Task. The Ledger validator currently accepts `code-path` metadata only under `Assets/NTSD/Scripts/` and `Tools/`; listing this Resources shader as a `code-path` fails its path policy. The shader remains explicitly covered here and by the scoped GPU import/render evidence, rather than being silently omitted from the change scope.

Original Editor exact RED job `0af8f3b6b8464266bffbd7c872009787` failed because the consumer was absent. GPU job `23abdee4b122435d94c53a27564bd0cd` passed 1/1 for red Map pixels moving `(2,-1)` in Unity screenshot while unrelated green pixels, Map bounds/Transform and camera did not move, with zero-offset baseline restoration. After adding end-camera material restoration, the first test version had a timing assumption error: job `0a0c2214a8ee41ddbf4cd0226f0d3d00` observed the material already restored after `Camera.Render()`; only the test was corrected. Final original Editor job `8b17b4acf0884c10955920c32233e47c` passed 1/1 including target/non-target camera cleanup. Two focused existing background frame tests passed 2/2 in job `b4bc13d49c8f4b7fab43ffb7afc17434`. Generated Runtime and Editor builds completed with zero errors. Original Menu/Battle Scene SHA remains `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`.

Final audit: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0, `Change ledger validation PASSED`, 939 Records and 12 governed code files in the diff; output is in the scoped `change-ledger-validation.log`. `git diff --check` exited 0. The validator's first failed run was caused solely by the unsupported Resources shader `code-path` metadata; that metadata was corrected while the exact shader ownership remained in this Record and Task. No production/script behavior was changed for the validator.

Actual remaining validation: direct Editor GPU fixture is not a production Battle Play chain. Natural Han/Lee action0→frame150/151 project background pixels, worker publication, and formal-root same-frame pixels are not yet verified. P-13/Q09/BATCH-05 and the master goal remain open. Exact scoped report: `artifacts/diagnostics/NTSD28-Q09-P13-PROJECT-BACKGROUND-VISUAL-CONSUMER-001/ACCEPTANCE-PENDING.md`. Rollback remains confined to the declared script/shader/test diff, subject to protected-file approval for any destructive operation.

2026-09-27 correction: the later original Battle Scene probe confirmed that the dedicated worker is currently ineligible due to attached Unity presentation bindings, consistent with B1. Worker publication is therefore a future B1/B9 activation revisit, not a current Q09 exit gate. The live inline natural Han/Lee Map pixels and formal-root visual comparison remain open; see `artifacts/diagnostics/NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001/ACCEPTANCE.md`.

2026-09-27 scoped continuation: `NTSD28-Q09-P13-INLINE-BACKGROUND-PLAY-001` completed original Battle Scene controlled frame150→151 full inline Driver publication and project Map capture. Its upper-band pixels shift with the runtime/frozen `(2,0)` background and restore at `(0,0)`. This closes the controlled current-live-path bridge between this consumer and the producer, while natural Han/Lee reach and formal-root visual comparison remain P-13/Q09 exits. See that package's `ACCEPTANCE.md`.
