# Q07-Q08/F05 terminal state14 held frame counter

Formal authority: root EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and playable `BattleWorld28::step_frames_range`. A physical primary slot below20, type0, HP<=0, current state14, revive lives<=1 and queued revive HP<=0 holds its action14 frame and clears frame counter (+0x088) before emitting held. Multilife, queued revival and transient slots do not take this terminal shortcut.

Unity first difference: `LF2Entity.RunNativeC25FrameBodyForWorldPass` and `RunNativeC25FrameTransaction` returned early for the same terminal gate without clearing `AttackingCounter`. Original Editor RED job `9b3dc358c1f64b378f17f51e3b66cb1f` ran DataOriented and Legacy production frame modes; both retained counter7 rather than0 while action14 stayed.

Implementation: one `TryHoldTerminalPrimaryState14Frame` helper checks the existing membership and clears `AttackingCounter` only when the shortcut applies. Both existing early returns call it after the kind2 gate. The owned test class includes lives2, queued HP80 and transient slot20 non-clear controls. Final original Editor job `500453a047254fe5bcfd4cc3ca87f605` compiled and passed the affected revival participant class 21/21, including all new positive/negative cases and existing revival/result handling.

No DAT, image, WAV, Scene, config, UI, framework or result-page edit in F05. Status: `UNITY_FOCUSED_PASS / RUNTIME_PENDING`; formal root EXE paired frame counter, natural Battle Play and Q07/Q08 whole-stage exits remain unverified.

Audit: `Tools/Validate-ChangeLedger.ps1` passed with 1,044 records and 96 governed code files in the working diff; `git -c core.safecrlf=false diff --check` passed. Protected Battle/Menu Scene and GameConfig/ProjectBattleModeConfig SHA-256 remain `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`, `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, and `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`.
