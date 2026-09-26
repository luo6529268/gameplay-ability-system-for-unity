# Q07 Naruto ordinary punch original-Editor raw comparison

Status: `VERIFIED / SCOPED_EDITMODE_NARUTO_PUNCH_ENTITY_INPUT_PARITY`. Parent Q07, R09/R10/R18 and the overall battle-alignment goal remain open.

The unchanged formal root EXE has SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The paired playable source witness and root EXE LFR use two Naruto OID2 participants at X500/X525, Z650, HP500/10, MP500, teams1/2, mode0/stage23, seed682973786, one J press on completed tick2 and 30 completed ticks. The formal and staged Naruto DAT are byte-identical. The source and root release witness established an authored frame513 punch at tick8, target HP10→-10 and KO attribution source/victim/credit/four-owner 0/1/0/0; see the preceding `NTSD28-Q07-NARUTO-PUNCH-FORMAL-HIT-001/REPORT.md`.

The original project's already running Unity Editor compiled the new exact fixture in `NTSD28UnityRawCaptureEditor` and consumed a unique EditMode raw-capture request. Run 1 failed **before any tick** because the new schema was omitted from the difficulty-zero fixture predicate. The run 1 result is retained. After correcting only that predicate, run 2 returned PASS and exported 30 tick rows each in raw, domain and input-RNG streams.

For completed ticks 1–30, occupied slot sets match in all 30 ticks. For the two occupied slots, 26 mapped entity fields per tick match **1,560/1,560**: OID/type, action/state/frame counter/previous action/facing, integer and precise XYZ, velocity XYZ, HP/effective and base maximum HP/MP, team/owner, render phase, environment state/platform source slot and special-hit latch. The 12 mapped input values plus seven W/S/A/D/J/K/L edge-window values match **1,140/1,140**; input-update phase matches **30/30**. No first difference occurs in these mappings. At tick8 both sides have attacker action513, target action186 and target HP -10; at tick30 both have attacker action3 and target action231/HP -10.

Synchronized RNG starts at counter/index/calls 0/0/0 and its counter/index/call count/last site/table hash match all **30/30** ticks. Unity records one synchronized call at site130 on tick2, bound2/result1. CRT total call count matches **30/30** ticks, reaching 3002 by tick8; the CRT **state does not match** because root LFR playback initializes with its default seed0 (`3374725112` after 3000 draws), while paired source and Unity scenario declare seed682973786 (Unity `1758127634` after 3000 draws). This playback limitation prevents an all-RNG-state parity claim. The selected fields and knockout outcome agree despite that initial CRT state difference.

Run 2 artifact SHA-256:

| File | SHA-256 |
|---|---|
| `original-editor-run2.raw.jsonl` | `2E650D47B5B11A8E3230C64479A8C67A75C947FD77637EC7158D9BCE258317EB` |
| `original-editor-run2.domain.jsonl` | `BD93E86F3D64C3A7ABB6E2FFEBA8AE2BD1A0897817CA2E81D3C5BD6D7007147D` |
| `original-editor-run2.input-rng.jsonl` | `E3CC3F55D299F00624AAB0BF839EEBF1CB96C2B3A142EE9C03D346B400145BCD` |
| `original-editor-run2.result.txt` | `01A6E8DE118AFC5B362BD693FBCC88C0A701C8321B321BE225617AE3A2957761` |

Generated Editor C# build exited 0 with 0 errors/208 warnings. The original Editor PID11944 remained responsive and the project-scoped Unity bridge reported ready after the request. Saved Menu/Battle Scene SHA-256 remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. No production C#, DAT, images, Scene, ProjectSettings or nonbattle file was changed by this package.

Final original-Editor state via the project-scoped local bridge: `NTSD_Battle` active, EditMode (`is_playing=false`), idle, no compilation, refresh or test job. The real `Tools/Validate-ChangeLedger.ps1` exited 0 with `Change ledger validation PASSED`, 852 Records and seven governed code files in the current overall diff; historical non-diff path warnings are preserved in `change-ledger-validation.log`. `git diff --check` exited 0. All three current progress documents are NUL-free; the approved v3 candidates had already been installed before this package, and later progress was retained rather than overwritten.

This is one exact EditMode complete-Driver raw-state comparison. It does not prove physical-key routing in Battle Scene Play, Unity knockout-event attribution, natural Menu entry, GPU pixels, all positions or attacks, or aggregate Q07 completion. Those are separate exits; the earlier Q08 physical-J Play witness lacks the complete initial state needed to stand in for this comparison.
