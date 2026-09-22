<!-- CHANGE-RECORD
id: NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001
status: VERIFIED
change-kind: Q08_DIRECT_BATTLE_RESULT_HOST_PROBE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleResultHostPlayModeTests.cs
authority: formal Logan first battle-only transition2 and second result selection source witnesses; existing Unity direct BattleTestBootstrap caller
evidence: isolated real Battle Scene GPU Play one of one PASS; direct AppManager MenuMain, command2 pending and old tick frozen; formal content bootstrap complete
-->

# NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001

Created before test script. Task: `docs/ai/TASKS/NTSD28-Q08-DIRECT-BATTLE-RESULT-HOST-PROBE-001.md`. Before: temporary synthetic no-Menu test shows command2 pending, but the actual direct Battle Scene `BattleTestBootstrap`/AppManager state and result boundary have not been observed in this Q08 campaign. Planned: one EditMode-runner test that enters real Battle Scene Play in an isolated Unity clone, waits for bootstrap, seeds native result command2 and measures old-world freeze and pending host state. Expected effects are a new test script, isolated import/test output and no original Scene mutation. Validation/rollback are in Task; no production rematch implementation is authorized by this Record.

Code written: new `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleResultHostPlayModeTests.cs` opens the actual Battle Scene in the isolated Editor runner, enters Play, waits for the direct bootstrap AppManager and a running nonempty World, checks the observed MenuMain/no-Menu scene owner, then seeds canonical phase3/timer350/transition2 and asserts five Play frames keep old tick and command pending. Original and isolated Scene/AppManager/driver/bootstrap SHA were equal before running. No production script was edited by this Task; fresh Play result pending.

First isolated Play XML `UNITY-DIRECT-BATTLE-RESULT-PROBE.xml` failed at the precondition `Running` versus `Preparing`: the 900 rendered-frame limit elapsed while formal DAT conversion was still active in the log. The seeded transition assertions were not reached; this is not a battle first difference. Test wait changed to a bounded 120 seconds of actual time with one-second yields, preserving the same result assertions. Retest pending.

Second run `UNITY-DIRECT-BATTLE-RESULT-PROBE-2.xml` reached formal sprite publication but failed during bootstrap before the result assertion: `-nographics` NullGfxDevice reported `MaxTextureSize=4096`, while the official staged content contains sheets up to 10891 pixels wide. The runtime deliberately rejected seven unrenderable oversized sources. This is a validation-device limitation, not a measured battle transition difference. The next run uses the same isolated scene/scripts and real graphics device by omitting `-nographics`; no source or resource edit.

Final probe: `UNITY-DIRECT-BATTLE-RESULT-PROBE-GPU.xml` passed 1/1 under Direct3D 11.0 feature level 11.1. The real Battle Scene's `BattleTestBootstrap` completed official content loading and resumed a nonempty running World. The direct-scene AppManager was `AppManager [TestBootstrap]`, State `MenuMain`, with no Menu Scene. After seeding phase3/timer350/transition2, five Play frames kept the old host tick unchanged and command2 pending while Battle Scene stayed loaded. Editor exited Play and terminated; original Scene SHA stayed `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. Diagnostic scope `VERIFIED`: it proves the missing direct-scene host seam, not natural KO or implemented rematch. Details and failed-at-precondition history in `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/DIRECT-BATTLE-RESULT-PLAY-ACCEPTANCE.md`. No production script, Scene or resource edit; full SelfCheck was not run because this package adds only a diagnostic Editor test.
