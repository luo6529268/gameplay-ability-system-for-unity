<!-- CHANGE-RECORD
id: NTSD28-Q07-SIX-BOUNDARY-CATALOG-001
status: VERIFIED
change-kind: EDITOR_PLAY_DIAGNOSTIC_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid32PublishedCatalogProbeEditor.cs
authority: formal Logan indexed DAT and paired playable point-clamp sprite resolver; Q07 all-frame source census
evidence: ORIGINAL_EDITOR_COMPILE_0_ERROR / ORIGINAL_BATTLE_SCENE_TICK5_SIX_OF_SIX_CATALOG_PASS / CONSOLE_ERROR_0 / SCENE_SHA_STABLE / LEDGER_841_32_PASS / GIT_DIFF_CHECK_PASS
-->

# NTSD28-Q07-SIX-BOUNDARY-CATALOG-001

Before edit, the OID32 opt-in catalog probe reports only pic0/pic64 and cannot establish the other five formal out-of-image base-pic entries in the original Battle Scene. Add a request-gated six-key read-only result to that same probe. The default request path and observed OID32 fields remain intact. Expected side effects are one Editor recompile and one bounded Play enter/exit; no simulation or resource mutation. The Task states exact paths, authority, acceptance, noninterference, risk and rollback.

After edit, record the actual symbols changed, original Editor compile/result, six-key observations, Scene SHA, Ledger outcome and outstanding natural/EXE/GPU gates here. Do not mark Q07 delivered from this diagnostic.

Actual edit: extended `Request` with opt-in `allBoundaryCells`; added the six formal OID/pic constants and serializable `BoundaryCell` result; only when opted in, the existing `Poll` reads six immutable `BattleSpriteCatalog` entries and checks derived source, 79×79 texture, Legacy Sprite and central binding. The default OID32 path and its report fields remain intact. Original Editor compilation was observed after the script write, with Editor assembly timestamp newer than source and zero current Console errors. One create-new request produced `OBSERVED` at tick5: six of six entries present, all checks true; raw JSON SHA `D4397B8AC4F4371BCD6E416C38DA1627337F4BF2104C5BBA8EEF0212592E3901`. Afterward Editor was idle/non-Play, request running flag false, Menu/Battle Scene SHA unchanged and Git Scene diff empty. No production/DAT/PNG/Scene/nonbattle edit, no NUnit/SelfCheck. Natural action selection, entity pixels and formal EXE same-state GPU remain unverified. Full result: `artifacts/diagnostics/NTSD28-Q07-SIX-BOUNDARY-CATALOG-001/ACCEPTANCE-20260926.md`.

Final governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository>` exited 0 (`841` Records, `32` governed code files in the current diff); `git diff --check` exited 0. The three live progress documents remain NUL-free. No commit or asset deletion was performed.
