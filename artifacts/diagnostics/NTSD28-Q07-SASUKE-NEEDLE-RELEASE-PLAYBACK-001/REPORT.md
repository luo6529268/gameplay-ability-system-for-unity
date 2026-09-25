# Q07 Sasuke 千鸟千本 formal release playback (2026-09-25)

Status: `VERIFIED_SCOPED_RELEASE_PLAYBACK`. Governing Task/Change: `NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001`. Q07 remains open.

`Tools/NTSD28Q07Diagnostics/sasuke_needle_lfr_probe.cpp` compiled from the official playable/core source closure (28 core files plus `game_session.cpp`, `selection_flow.cpp`, `lfr_recorder.cpp`, `game_session_lfr.cpp`) with `g++ -std=c++17 -O2 -Wall -Wextra -Wpedantic -municode`, zlib and static libgcc/libstdc++; exit 0, empty `compile.log`. Formal source and DAT were read only. Initial world: Sasuke OID11 at `(500,0,350)`, action110, HP/MP500, team1; OID7 at `(1200,0,350)`, team2; seed `0x28A55A5A`, background23. Slot0 input was defend tick2, right tick4, attack tick6, no other buttons through tick26. The harness wrote 26 source CSV rows and an LFR, refusing existing output names.

The root formal `NTSD2.8-Logan.exe` SHA-256 was `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` before and after. Successful playback passed that LFR, `--lfr-slot0-action 110`, `--resource-root` and `--complete-vfs-root` pointing at formal `resources/runtime`. It exited 0 with `passed:true`, `failureCode:0`, 27 completed ticks including the terminal row, and 28 trace rows including tick zero. Comparing CSV ticks 1–26 to root-EXE trace on action, inputLastAction144, HP, MP and live OID440 count gave **26 × 5 = 130 equal field comparisons; no first difference**. Both entered action261/MP400 at tick6, action264 and four OID440 at tick15. Trace tick0 confirms the stated roster/action. The report retains `nativeParityClaim:false`.

The prior original-Editor `NTSD_Battle` physical L/D/J witness (`../NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001/ACCEPTANCE.md`, `q07-sasuke-needle-2.json`) also recorded keys at ticks2/4/6, action261 at tick6, frame264/four OID440 at tick15, plus formal chi.png pic0 binding and source-formula birth vectors. Its initial parent position was `(800,0,714)`, different from this release setup. Unity seed, full input masks and phase were not captured for same-world comparison. Common milestones alone do not prove hit, projectile lifetime, pixel/sorting/shadow parity or Q07 completion.

The first PowerShell `Start-Process` invocation used unquoted paths with spaces and exited 10 without report/trace. The second quoted-path invocation omitted `--complete-vfs-root` and exited 45 with `initialize_session: background DAT was not found for id 23` (`formal_release_playback_action110_report.json`). The third invocation supplied both roots and succeeded. Earlier outputs were preserved.

| Artifact | SHA-256 |
| --- | --- |
| `sasuke_needle_lfr_probe.exe` | `2E761E1B7843C3D0961E4F40420195A4D6119A1DCE3E3CDD4C774FC710E001F2` |
| `sasuke_needle_source_packets.lfr` | `21FE541BAD50895FBE8B659432D9E63A2706D6D51C1CAD1647FB622A3038AFC6` |
| `sasuke_needle_source_ticks.csv` | `EA17193F47A502440AF1543FA1C9011FAC4A970EC8DA2E8C93033503D678F906` |
| `formal_release_explicit_vfs_report.json` | `BA027E92D88B8801BAEF01F2789FD2B50FABCFA21DDFC53EE656A52E08A07939` |
| `formal_release_explicit_vfs_trace.jsonl` | `2450850DC5D2548E8904DD2C815146B550D6708EF5172807B545D44E9DAFD957` |
| failed `formal_release_playback_action110_report.json` | `E164BF78FA004319FC2EC4B4804908AEC79A49950364720AC2BC5A8CEEE6AF93` |

Original Unity Editor import/Play of the separate DDJ same-state probe remains pending. This package changed no Unity production, Scene, DAT, PNG or nonbattle file.
