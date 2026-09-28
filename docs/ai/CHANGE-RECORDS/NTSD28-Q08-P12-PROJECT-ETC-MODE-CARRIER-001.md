<!-- CHANGE-RECORD
id: NTSD28-Q08-P12-PROJECT-ETC-MODE-CARRIER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/ProjectBattleModeConfig.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ProjectBattleModeConfigEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
authority: formal root NTSD2.8-Logan mode0 selected etc-mode trace and paired playable state9997 branch; user project-owned mode Asset decision
evidence: docs/ai/TASKS/NTSD28-Q08-P12-PROJECT-ETC-MODE-CARRIER-001.md
-->

# NTSD28-Q08-P12-PROJECT-ETC-MODE-CARRIER-001

Pre-code record. Existing `ProjectBattleModeConfig` selected-mode snapshot/fingerprint is the project's replacement for excluded original mode DAT data. The selected mode0 formal value is 1 and is consumed by the formal state9997 owner branch; Unity currently has no etc-mode carrier, preventing an evidence-based Q09 branch. Add only the selected scalar, immutable capture, versioned fingerprint and exact Asset value, with focused config/identity checks. It must not silently enable state9997 rendering or change collision/logic. The detailed scope, acceptance, risks and rollback are in the Task Contract.

Risk: changing the fingerprint intentionally invalidates prior cached prewarm candidates; the original Editor may need content recapture. Existing Asset/Script files contain earlier unrelated dirty edits, which remain protected. Do not treat successful config tests as Q09 visual parity, formal root pixels or Q08 aggregate completion.

Validation pending: original Editor compile and focused tests, current serialized Asset binding, protected Scene hashes, ledger validator and diff check. Q09 presentation exit requires a separate Task/Change.

CODE_WRITTEN: `ProjectBattleModeConfig` now serializes nonnegative `selectedModeEtcMode`, exposes and captures the immutable selected scalar, and includes it in `PROJECT_BATTLE_MODE_CONFIG_V4` fingerprint bytes; the project-owned Asset selects 1 for the current mode0 setup. The existing focused config test file gained a positive 1/0 clone/fingerprint isolation and negative-value rejection case. No catalog class change is needed because its existing `ProjectModeSnapshot` carries the captured Snapshot; production render still does not consume the new field. Original Editor compilation and focused test are pending. Existing dirty changes in these files remain in place.

FOCUSED_TEST_PASS / RUNTIME_PENDING: original Editor PID11944 recompiled the declared C# changes with no C# error; exact EditMode job `b409f58e8a0e4f259cc6a11023c978bc` passed four mode-config tests (new selected etc-mode positive/negative/fingerprint plus three existing adjacent cases). Raw result SHA `0AB7634726725467BD22B5A2B4FF6A605D76D86DC015201FBC7092FF6CDF989F`; acceptance report under `artifacts/diagnostics/NTSD28-Q08-P12-PROJECT-ETC-MODE-CARRIER-001/`. Asset and Scene SHA recorded there; Battle/Menu Scene Git diff remains empty. Original Battle Play catalog/frozen-frame consumption and Q09 visual repair remain unverified. No formal DAT, renderer, camera, Scene or nonbattle edit.

Pre-edit runtime witness extension: this same Q08 carrier is not yet proven inside a running Battle World. The existing opt-in Karin Play probe is now in declared scope solely to capture and compare the new Asset Snapshot with `BattleRuntimeDataCatalog.ProjectModeSnapshot`, plus active camera and presentation-origin data needed to interpret the later Q09 clamp. A third unique request/result is required; the first two artifacts and their meaning remain immutable. No production consumer, mode DAT, Scene, camera or nonbattle code edit is authorized by this extension.

Runtime probe CODE_WRITTEN: only the declared `NTSD28Q09KarinState9997BattlePlayProbeEditor` test script changed. `Prepare` now compares the current Asset and production catalog selected etc-mode/fingerprint after formal content publication, records active orthographic camera world edges, `NTSDRenderSpace.CaptureViewportTransform` origin/units, and optional current walkable bounds. The opt-in request constant advances to the unique third file. The original two result files remain immutable; original Editor compile/Play of request03 is pending.

VERIFIED scoped runtime carrier: original Editor recompiled the probe with no C# error and ran request03 in the saved Battle Scene. Asset and `BattleRuntimeDataCatalog.ProjectModeSnapshot` both reported selected etc-mode1 with identical fingerprint `973C1815889F9D04BCEFBC189CF83DFFE0685EB95B25AE11E70200F8F67618B9`. Three full Driver ticks reproduced the existing Q09 central-facing first difference without changing production behavior. Raw JSON SHA `4B7702AFA54970F50C748DE594EEB65C5FBB4FBFE758AA37960B300D1A0097A5`; active viewport presentation X `[-3.25346,2044.74658]`. Objects/slots/borrowers 4/2/2 restored, pause restored, original Editor idle/non-Play, both Scene hashes stable. The acceptance report records exact evidence and limitations. Q08 aggregate and Q09 presentation remain open; no renderer/Scene/DAT/nonbattle edit.
