# D-024 kind-1 OPoint random birth X/Z outlet

Status: `FOCUSED_TEST_PASS / RUNTIME_PENDING`, 2026-09-24. Scope is the shared `BattleNativeOpointBirthWriter.InitializeBirth` used by logic-only and component materializers after `kind=1` OPoint child creation. The shipped playable `BattleWorld28::materialize_spawn_intents` adds synchronized raw random integer deltas in X/Y/Z/action order, then mirrors precise XYZ. Official Yagura OID510 action418 in LoganRuntime DAT contains three `kind=1` OID511 OPoints with nonzero X amplitudes 600/400/200 and Z amplitude 80; this branch is content-reachable. No DAT values were changed.

| Job | Exact scope | Result |
| --- | --- | --- |
| `126a974eb91243fba35b1eaa740971b0` | New actual late structural birth fixture before production edit | RED 0/1: same raw X draw 147, configured view expected 225.84846211552889 but actual 147. Earlier factor1/draw-count/Y/Z controls passed. |
| `d531a9f5424246a586014df8c72520bb` | X-only first corrected version | GREEN 1/1; later superseded by X/Z strengthened fixture. |
| `99d1c0184d6245dfb8232342eb223661` | Strengthened X and projected-Z view-fraction, integer mirrors, unchanged Y and synchronized call order | GREEN 1/1. |
| `840455c81a6349c89a9efcc35f418ab3` | Existing original-source row0 immediate materialization, World and component routes | GREEN 2/2, exact default-view source comparison. |

The test cloned an existing source fixture in memory and gave its first OPoint nonzero `centerx:600` and `centerz:80`; repository DAT and source fixture bytes were untouched. Four registered logic-only Worlds isolate the random offset from the already-scaled base OPoint offset. Factor1 random X/Z changes provide the raw expected deltas. The configured World now adds each raw delta once at the final birth output using X `2048/1333` and projected Z `1152/730`. Precise coordinates retain the fractional result and integer mirrors truncate it; native random X/Y/Z/action call order and count remain unchanged. Y and frame action remain raw. Both materialization routes share this writer; the explicit source-row regression ran each route at factor1, not at the configured view.

Post-edit `git diff --check` and `Tools/Validate-ChangeLedger.ps1` passed (732 records, 22 governed code files in the entire current diff). `Assets/NTSD/Config`, `Assets/NTSD/Content/LoganRuntime`, `Assets/NTSD/Scene` and `ProjectSettings` had no Git status entries. Battle Scene SHA-256 remained `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`.

This is a focused D-024 battle-position outlet correction, not source-rule history completion. There is no real Battle Scene Play or formal EXE screen-fraction capture for this exact Yagura action; target-history OID219, fusion, other direct writers and final Q07 exit remain open. The future source-rule carrier must record the unscaled raw random integer result separately; it cannot infer it from the new scaled battle coordinate.
