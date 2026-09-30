<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001
status: VERIFIED
change-kind: Q09_P20_HIDAN_NATURAL_FRAME430_FORMAL_DIAGNOSTIC
code-path: Tools/NTSD28Q09Diagnostics/hidan_natural_frame430_probe.cpp
authority: formal NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 InputRouter28 SpriteFrameResolver28 plus formal hid.dat
evidence: prior paired Session naturally reaches Hidan frame212; original Unity grid capacity focused 4 of 4; frame430 natural route and root EXE pixels pending
-->

# NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001

Status before script: `IN_PROGRESS`. Task: [declared scope and acceptance](../TASKS/NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001.md).

Existing state: formal Hidan jump frame212 has `hit_Fa:430`; prior ordinary input trace reaches 212, but no forward-attack combo was injected. Unity's shared Logan PNG grid-capacity change has original Editor 4/4 focused evidence only. No natural frame430 or same-frame pixel proof exists.

Planned side effects: a new read-only paired-source diagnostic executable input/JSONL/LFR evidence under a unique artifact folder. The script must not change formal authority data, Unity content, Scene, gameplay, editor state, or existing diagnostics. No physical removal or cleanup of old assets. Rollback: review and remove only this new diagnostic/outputs if explicitly authorized; preserve other dirty files. Acceptance and pending higher-tier validation are in the Task.

Actual file/symbol: only new `Tools/NTSD28Q09Diagnostics/hidan_natural_frame430_probe.cpp::wmain` for paired complete Session input/recording; new output under the same-ID artifact directory. g++ C++17 compiled 28 core + four playable units plus this source to a new executable, exit0. First invocation used `resources/runtime/vfs` as complete root and failed before simulation (`decoded_dat` missing), leaving an untouched 0-row file. Corrected new invocation used `resources/runtime` and found first action430 at tick16 from the first bounded combo timing candidate; its source-authored 30-tick LFR replayed by root formal EXE with `passed:true`/failure0 and no action/facing/MP override. Source/root selected actor action/MP, input phase and combo0 matched 120/120; root trace tick16 pic119. See [acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001/ACCEPTANCE.md).

Final scope: `VERIFIED` only for formal natural-input reachability and selected root replay. Original Unity complete-Driver frame430 was subsequently verified in the separate `NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001` package; natural Battle body pixels, formal root GPU pixels and Q09/P-20 remain pending. No source authority, DAT, image, Scene, Unity production/test or nonbattle path changed in this package. Final combined Ledger validator PASS (1003 records/34 governed diff files), `git diff --check` exit0.
