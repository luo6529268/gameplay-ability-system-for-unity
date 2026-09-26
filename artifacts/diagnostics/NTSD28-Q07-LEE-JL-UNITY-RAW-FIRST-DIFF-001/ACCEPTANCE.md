# Q07 Lee J→L original-Editor raw comparison

Status: `VERIFIED / SCOPED_EDITMODE_JL_ENTITY_RNG_PARITY`. Parent Q07 and D-024 remain open.

The paired playable source and root formal EXE established the ordinary-input 45-tick Lee witness in `NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001`: physical J on completed tick 2, L on ticks 3–4, and five Lee-owned OID204/action20 children at tick 6. The root formal EXE identity is SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The source/release selected four fields matched 180/180 across 45 ticks; the exact native trace is `../NTSD28-Q07-LEE-NATURAL-NONSOUND-CHILD-001/run4/formal-release-trace.jsonl`.

The original Unity project's already-running Editor compiled the narrowly extended `NTSD28UnityRawCaptureEditor` and consumed the exact `lee-jl-formal-45.json` request in EditMode. Run 1 returned PASS and matched 2557/2557 mapped input-phase and occupied-entity fields, but its diagnostic fixture used `ResetForDirectBattle(seed)` and drew once for random BGM. The native source fixture explicitly selected BGM 2, so run 1 started synchronized RNG at 1/1/1 versus native 0/0/0. Run 1 outputs and the measured cause are preserved in `RUN1-ANALYSIS.md`.

Only this new Lee diagnostic schema was changed to use `ResetFromSeed(seed)`, retaining the old 3/26-tick schemas and production reset behavior. Independent run 2 returned PASS. Its 45 completed ticks match the root formal EXE trace at **2557/2557 mapped comparisons, zero first differences**: input update phase and, for every jointly occupied slot, OID, type, action, state, frame counter, facing, X/Y/Z, Vx/Vy/Vz, HP, MP, team, and owner. The occupied slot sets also agree. Tick 5 has OID814 in slot 50. Tick 6 has OID204/action20/owner0 in slots 51–55, each at `(437,-35,651)`, equal on both sides.

Run 2 synchronized RNG begins at counter/index/calls 0/0/0, equals the native trace's per-tick counter/index/call count and last call site in all 45 ticks, and records one Unity call at site 130 on tick 2 with bound 2/result 1. Both streams make no further synchronized draws in the measured window. The Unity diagnostic's CRT initialization state is `1758127634` after 3000 draws from seed `682973786`, with zero further CRT draws; root LFR playback uses its default seed 0 and starts CRT at `3374725112` after 3000 draws. This playback seed difference prevents claiming identical CRT state. The formal source and Unity scenario share seed `682973786`; this package does not infer a production CRT defect from root LFR's default.

Run 2 artifacts and SHA-256:

| File | SHA-256 |
|---|---|
| `original-editor-run2.raw.jsonl` | `62BC27E269948033A3A6C6CB4F661CB42A9C64206B5393EA0B75719C082B538D` |
| `original-editor-run2.domain.jsonl` | `4FE4DAA76355F353A436CCAC7E5CC686A1D800ABD3A857AF4266CFF622BAC753` |
| `original-editor-run2.input-rng.jsonl` | `533125DFFEA8FA5567EB687B038AB9AD6DB96753970D5E46ADAE0EAC72B6522F` |
| `original-editor-run2.result.txt` | `97F7A7718DE66371B27E8E1939B38C0EE0224BEAC2EABE16ADA5FA7DC06D5EB7` |

The pre-existing `Temp/NTSD28UnityTrace/unity-raw-capture.result` remained SHA-256 `C5ADAAC2E58D39DECD8311FB6A34950804DA7D4E276F1906BE9645D35F4E0859`. Saved Menu/Battle Scene SHAs remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; neither Scene nor Build Settings has a current Git diff. `git diff --check` passed. The Change Ledger validator result is recorded in the Change Record.

This is an EditMode complete-Driver raw-state comparison for one exact human Lee scenario. It does not prove physical keyboard routing, Battle Scene Play, camera pixels, full child lifetime, all other skills, or Q07/D-024 exit. The next Q07 gate is the same ordinary Lee J→L sequence in the saved original Battle Scene, with natural child publication/cleanup and a bounded visible-pixel witness against the formal EXE where the user has not excluded presentation.
