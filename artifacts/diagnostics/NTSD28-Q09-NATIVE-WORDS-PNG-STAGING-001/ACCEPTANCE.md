# NTSD28-Q09-NATIVE-WORDS-PNG-STAGING-001 acceptance

Status: `VERIFIED_STAGING_ONLY` (2026-09-22). The six exact formal `sprite/UI/WORDS0.png` through `WORDS5.png` files were absent from the original project's LoganRuntime staging root before copying. Source length/SHA-256 was checked against the Task Contract before any copy; each destination length/SHA-256 matched after copy. No overwrite or deletion was done.

Formal PNG total remains 1,255. Staged PNG count increased from 1,015 to **1,021**; formal paths absent from staged root decreased from 240 to **234** (`b/*` 110, `sprite/*` 124). `git diff --check` exited 0. The only targeted new paths are the six PNGs under `Assets/NTSD/Content/LoganRuntime/vfs/sprite/UI/` and this diagnostic package. Formal/staged `resource.dat` both name the same six paths. The formal playable uses indices 16-21 for battle name/HUD glyphs. Current Unity still has its old BMP-based WORDS publication contract; no reader, importer, script, Scene, Prefab, DAT, old image or menu flow was changed.

The original Unity Editor remained PID 33236 and pointed to the original repository. This package did not launch another Editor or use computer-use. No fresh Unity import, script compile, nameplate pixel test or formal EXE pixel parity was performed. Q09/R17 remain open; exact file staging is not visual alignment.
## Production caller correction

The earlier shorthand “Current Unity still has its old BMP-based WORDS publication contract” describes the available catalog API/diagnostic, **not an active production publication**. Read-only production caller audit `NTSD28-Q09-NATIVE-WORDS-PUBLICATION-CALLER-AUDIT-001/REPORT.md` found that `CharacterAnimtorManager` builds only Shadow+Spark and never calls `WithWords`; the latter is used by test setup. Current production `IsWordsValid` is false, so the staged PNGs are not yet drawn. This correction supersedes any suggestion that simply replacing old BMP bytes would activate formal WORDS.
