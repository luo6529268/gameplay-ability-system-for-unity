# Q09/P-13 Han natural-input earthquake: scoped acceptance (2026-09-27)

Status: `VERIFIED_SCOPED_NATURAL_INPUT_ROOT_TRACE`. This closes only the action-0-to-catch-to-earthquake reachability diagnostic, not P-13, Q09, BATCH-05 or the master goal.

The new Tools diagnostic used the current formal runtime DAT with mode 0, background 23, seed 2833, Han OID 726 at X500 and Lee OID 7 at X520 or X580. Both began at action 0. Attack was held on ticks 1–2; jump was held on ticks 3–4 or 5–6. Each case ran 50 complete paired `GameSession28` ticks and produced a per-tick CSV and LFR without seeding a later action or catch relation. The first failed compile was retained in `compile-v1.log` (missing zlib link); the second compile with `-lz` and the four-case run exited 0.

| Lee X | Jump first tick | First Han 145 | Grab window | Reciprocal 149 relation | Background (+2,0) | Reset (0,0) | Source result |
|---:|---:|---:|---:|---:|---:|---:|---|
| 520 | 3 | 4 | 9 | 10 | 16 | 21 | PASS |
| 520 | 5 | 6 | 11 | 12 | 18 | 23 | PASS |
| 580 | 3 | 4 | 9 | 11 | 17 | 22 | PASS |
| 580 | 5 | 6 | 11 | 13 | 19 | 24 | PASS |

The first case's LFR was replayed by the **formal root** `NTSD2.8-Logan.exe`, SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, using headless playback against its current `resources/runtime`. The process exited 0. Its report says `passed=true`, `failureCode=0`, `declaredTicks=50`, `completedTicks=51`, and explicitly `nativeParityClaim=false`. Tick 51 is the replay wrapper's extra completed tick and is excluded from the comparison. An independent parse of root trace ticks 1–50 against the first source CSV compared Han/Lee action, X, Han MP, Lee HP, and both reciprocal catch slots: **400/400 fields match, zero differences**. The root trace shows Han145 at tick 4, Han149/Lee131 and reciprocal slots 1/0 at tick 10, Han150/state55052 at tick 16, and Han151/state55050 at tick 21.

The paired Session CSV records background owner 0 with offset (+2,0) at tick 16 and (0,0) at tick 21. The root EXE's JSONL trace has no earthquake owner/offset field, so root pixels and the root's exact background offset were **not independently measured** by this playback. No Unity Editor or Play validation was run in this package. P-13 still needs the original Unity Battle Scene background-only consumer and a suitable formal-root visual comparison. Do not move entities or the fixed camera, or load the user-excluded native background/mode DAT, to simulate this effect.

Evidence hashes: new Tools source `BD9D5A3AD7CBAFF92419C4E4D5370C160B3901D1C9EA57E7BC553284EDE83B73`; compiled diagnostic `331F58848EDE81AEE656D513BD2AE1EB739A2FA4E08DCD6837CB89A8D032A62E`; first-case LFR `7954A8DD6C21DE188C40C770B84B92A66F2CA0C9E5B7E7321E4292ABCA7C13CA`; root report `BBA083382CB3A8C3706934B243D234611C6893C0BBCF9E135BEC7FFB2E83BBAA`; root trace `A524DE2393A41244DAEB5933BF605B07A4D1797E0C6004505BAA3F1AB776890E`.

This package added only a diagnostic source and evidence. It did not change formal source/EXE, Unity production/test, DAT values, images, audio, Scene, camera, mode asset or nonbattle behavior.

Post-record checks: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 (`PASSED`, 937 records; both changed Tools sources covered), and `git diff --check` exited 0. The existing ledger warnings about historical records whose code is absent from the current diff do not negate coverage of this source. Original `Assets/NTSD/Scene/NTSD_Menu.unity` and `NTSD_Battle.unity` SHA-256 remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. No Unity script compilation or Play was required or run for this Tools-only diagnostic.
