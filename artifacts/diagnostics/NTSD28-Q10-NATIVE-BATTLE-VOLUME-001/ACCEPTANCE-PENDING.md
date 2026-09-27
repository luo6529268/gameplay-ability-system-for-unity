# Q10 native battle volume SFX checkpoint (2026-09-27)

Change `NTSD28-Q10-NATIVE-BATTLE-VOLUME-001`; status `RUNTIME_PENDING`. The root formal EXE/source authority and the pre-edit first difference are in `../NTSD28-Q10-NATIVE-VOLUME-FIRST-DIFF-20260927/REPORT.md`. This checkpoint covers the battle SFX percent/gain path only; O-03 and Q10 remain open.

Original Unity Editor PID11944 was observed idle, not playing, with `NTSD_Menu.unity` active. The project's existing local MCP bridge at `127.0.0.1:6402` was used to request refresh/compile and run named EditMode tests. No second Editor/project or computer-use was used. Compilation/domain reload completed; the named new tests were discoverable and executed from the refreshed assembly.

Test-first RED:

- Job `bd7583127fa1470bbedda374ff2e1c94`: one exact SFX test failed before production code at missing `NativeBattleVolumePercentForDiagnostics` (`Expected: not null / But was: null`).
- After the initial host code, job `b769d86e31244790a02d333088e5070d` failed at the test's first successful host-tick precondition. The fixture then explicitly rebound the local frame provider. Job `772a95037bd64cbd9dd3f0517c14a3b3` reached a successful host tick and failed at `expected 99 / actual 100`, establishing the missing volume effect through the real driver entry. The current code now uses its selected battle sound presentation sink, with production AppManager fallback.

Post-edit GREEN and scoped neighbors:

- Original Editor job `632f3c4b9dd040d19d335de441a56180`: 3/3 pass. The two new tests cover 100→99→0→1→100, formal SFX gain at 99 and 1, retuning retained pooled voices, gain on a newly presented cue, one change on a successful LocalFreeRun host tick, no change on a rejected repeated tick, and no change in Manual. The third test is the existing pooled-voice saturation/unknown-cue zero-allocation case.
- Original Editor job `c1209707806a463c8f0738d0729e128b`: 2/2 pass for existing physical latch F12 priority and F11/F12 continuous routing. A prior attempt with the wrong `NTSD.Test` namespace selected zero tests; it is not counted as a pass.
- After extending the same host test with paused-no-step and accepted F2 single-step assertions, original Editor recompiled and job `7c9b3b8363c04878b172546fd369bb3c` passed 1/1. The volume stayed at 100 while paused without a step, fell to 99 exactly when F2 advanced one LocalFreeRun tick, and remained 99 on the subsequent Manual tick. This is a controlled host test, not physical keyboard Play.

Related failures requiring separate ownership:

- Job `00a7d676b6b3478a9508a49e31bddb32` selected four sound tests; the above two new tests and saturation test passed, but the old `Driver_CatchUpPublishesEveryTickAndPresentationHostDispatchesOnceWithoutDropOrDuplication` failed with expected event count 3, actual 0.
- Job `cda16a623ea64467a0fe18a3ff2b5c6d` selected two old dispatch/suppression tests; both failed with expected `PendingSounds` count 1, actual 0. These old tests use `TickSoundEmitter.SimTransit`; this Change did not modify that emitter or the direct `StepOneTick(FrameInputSet)` / world sound queue route. The first actual missing writer in their current fixture has not been diagnosed, so these results are not presented as passing or proven pre-existing. Do not change their expectations merely to make this package green.

2026-09-27 correction to the above pending diagnosis: the fixture was frameless with `FrameDelay=0`, so the already-verified Q06 missing-native-frame guard skipped its `SimTransit` before an event could be queued. The pre-edit Task/Change extension declared a test-only correction. `TickSoundEmitter` now starts with `FrameDelay=4`, which stays positive for the three tested ticks; all expectations and production code are unchanged. The original Editor refreshed and domain-reloaded. Job `064bee20b94e4ed1a752f76f7255edc8` selected and passed all three formerly failing dispatch/catch-up/suppression cases (3/3). Job `070214f87eb14ce28aa6bbeb597adc4e` selected and passed the two native battle volume tests after this last edit (2/2). This supersedes the claim that the fixture cause is unknown, while preserving the original FAIL JSON as the before evidence.

| New result file | SHA-256 |
|---|---|
| `green-sound-fixture-neighbors.json` | `90EFF365246C854AC7471A61D80F41314CF19E3F2B6F3E8B1939783F20072877` |
| `green-volume-after-fixture.json` | `6636DC72B679F43C6C721D555404E46EED488122853E55EBE2B845C15C6C9D1A` |

Post-fixture checks: `Tools/Validate-ChangeLedger.ps1` exited 0 with `Change ledger validation PASSED` (928 records, 36 governed code files in the current dirty diff; historical unrelated warnings remain). `git diff --check` exited 0 with only line-ending warnings. Menu and Battle saved Scene SHA-256 still match the protection values above. The EditMode results prove fixture reachability and battle SFX host logic, not the remaining physical Play or audio-device gates.

Protection and residuals: `git diff --check` exited 0. `Tools/Validate-ChangeLedger.ps1` exited 0 (`Change ledger validation PASSED`; unrelated historical warnings were printed). Saved Scene hashes are Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, matching the documented pre-existing scene hashes; neither scene is in Git status. The original Q08 stage-gate Driver dirty hunks remain.

Physical Play follow-up: independent `NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001` has now passed the selected formal-content Menu→Battle physical F11/F12/F2 percent gate; see its `ACCEPTANCE.md` and result JSON. This supersedes physical Play as a missing item in this paragraph, but does not provide worker or audible proof.

Not yet verified: worker publication under a real session, battle sound voice start/publication on each of two successful ticks within one Update, BGM/WMA owner/gain, audible device output, and formal EXE same-condition audio. The current code applies the correct final battle SFX gain but batches both tick events until LateUpdate. The paired source audit `../NTSD28-Q10-AUDIO-PER-TICK-PUBLICATION-AUDIT-20260927/REPORT.md` corrects the earlier shorthand: native starts each cue per tick and later volume changes retune active voices; it does not permanently freeze a distinct gain for each cue. This remains an explicit Q10 publication/timing subtask, not an implicit B12 deferral.

Fresh MCP `get_test_job` responses were saved from this original Editor domain with these SHA-256 values:

| File | SHA-256 |
|---|---|
| `green-volume-host-pool.json` | `6A499708990FE21681AB11FB1F48B941DBDD9AF573C9F6CE630B8218A1129A31` |
| `green-paused-f2-host-tick.json` | `1F6EA9C66CF19526F005B4A2B1032A195330D3D4084CEA464C55D7E3AB88D76D` |
| `green-function-key-route.json` | `7447860354C24B96C41A65C158A8CC71D9ECD8695145E768C0862B1577B7B777` |
| `failed-sound-catchup-neighbor.json` | `4C32E3B831CC928BA8D3DBC75C12DD9779424FC1EE03C44383D0771E362097F9` |
| `failed-sound-dispatch-neighbors.json` | `3CBFDFFE89EF6AAFB60644B02BD7379AE346EFE810CD1CABE3A06AF8BC3C4FE4` |
