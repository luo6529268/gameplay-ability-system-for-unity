<!-- CHANGE-RECORD
id: NTSD28-Q09-P07-PAIRED-WARP-LEE-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/lee_shadow_warp_probe.cpp
authority: formal root NTSD2.8-Logan EXE and paired playable D3D11 offscreen source closure
evidence: docs/ai/TASKS/NTSD28-Q09-P07-PAIRED-WARP-LEE-001.md
-->

# NTSD28-Q09-P07-PAIRED-WARP-LEE-001

Pre-edit state: the Lee OID204 formal Session shadow/body commands are measured, and the original Unity Battle Play produced body camera pixels and zero child shadows. No paired D3D11 pixel output exists for the same formal snapshot. This task creates a same-frame renderer A/B without modifying the official source, EXE or Unity runtime.

Actual code: added only `Tools/NTSD28Q09Diagnostics/lee_shadow_warp_probe.cpp` (`wmain`). It reproduces the six-tick formal paired Session, checks child/body/shadow command counts, clones one render snapshot, removes only five child sprite commands from the clone, and renders both through paired `D3D11Renderer28` WARP at 1333×730. No production or source content was edited.

Validation: playable closure g++ compile exit 0 (`compile-argv.txt`, `compile-exit.txt`); execution exit 0 (`run.log`, `run-exit.txt`); both PNGs valid and independently differ at 590 pixels (`pixel-analysis.json`); repeated output path rejected with exit 3 and unchanged PNG hashes (`no-overwrite-result.txt`); Battle/Menu Scene SHA unchanged. `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit 0 (`ledger-validator.log`, historical non-current-diff warnings); `git -c core.safecrlf=false diff --check` exit 0. Full acceptance and SHA inventory: `artifacts/diagnostics/NTSD28-Q09-P07-PAIRED-WARP-LEE-001/ACCEPTANCE.md`.

Scope/risk: `VERIFIED` applies only to this diagnostic's paired-renderer pixel attribution. Root release EXE GPU output and same-world Unity comparison are unverified, so P-07/Q09/BATCH-05 and the parent goal remain open. Rollback is removal of this newly added diagnostic/source/artifacts subject to repository deletion approval; no existing code or asset needs restoration.
