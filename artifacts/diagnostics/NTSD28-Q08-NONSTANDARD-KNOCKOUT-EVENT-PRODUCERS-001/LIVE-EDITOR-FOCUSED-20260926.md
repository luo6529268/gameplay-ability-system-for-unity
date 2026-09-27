# Q08 nonstandard knockout event producers — original Editor focused gate

Status: `FOCUSED_TEST_PASS / FORMAL_FULL_TICK_AND_PLAY_PENDING`. The parent Q08/BATCH-04 exit remains open.

The existing original-project Unity Editor was idle and outside Play, with no import, compilation or test job running. Its runtime DLL timestamp `2026-09-26T15:01:49Z` follows the newest owned production source (`LF2Entity.cs` at `15:01:05Z`); Editor DLL `15:25:29Z` follows all three affected fixture files. No second Editor/project was started.

One filtered EditMode job `98a9079c675a4a9b88bfde9767f49251` completed `summary.total=7, passed=7, failed=0, skipped=0` in the original Editor. The seven exact methods cover:

- State12/18 environment lethal event and already-dead no-extra-event gate (`NTSD28B4State1218EnvironmentCreditEditorTests`).
- Negative-environment slot reuse credit, missing impact-source event defaults, and zero-actual-damage no-event gate (`NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests`).
- Held CPoint direct-owner credit/event and non-type0 missing-owner no-credit/no-event gate (`NTSD28B6HeldInjuryAccountingCoverProductionEditorTests`).

These tests assert `NativeKnockoutEvents` and the existing single-count behavior in the current production writers. The prior full `BattleRuntimeSelfCheck` result file is `Temp/NTSD_BattleRuntimeSelfCheck.result`, `PASS` at `2026-09-26T15:08:56Z`; no production script changed between that result and this job. It was not rerun merely for this evidence update. Menu/Battle Scene, GameConfig and ProjectBattleModeConfig SHA-256 remain `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, `88E10D43B047952FD3053A87B7E5F60D0F87A6CD1A23313F37EA1503C686F55C`.

This advances the older `CODE_WRITTEN / OFFLINE_COMPILE_PASS` evidence to a **scoped live-Editor focused pass**. It does not prove all parameterized variants, same-seed complete-tick event order against the formal release, physical Battle Scene Play, Player exit/re-entry, or final Q08/Q09/Q10 consumer parity. No code, DAT, resource, Scene or nonbattle file was edited in this follow-up.
