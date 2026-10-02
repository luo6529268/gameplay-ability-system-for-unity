# NTSD-MENU-LOOP-CAROUSEL-001

User scope: implement infinite vertical game-mode selector in existing Canvas/uGUI, 1920x1080, original event order and font preserved. Independent input/motion work approved while reference images unavailable. Seven current indices: VS, Stage, 1V1, 2V2, Battle, Demo, Quit. Current handlers: 0 VS character screen; 1 Stage placeholder; 2 Battle placeholder; 3 Training placeholder; 4 Options placeholder; 5 Application.Quit; 6 no handler. Report mismatch; do not silently change it.

Implementation: runtime-only carousel attachment, one original item per index, bounded continuous phase, wrap relocated beyond clipped viewport; exclusive pointer drag ownership; use existing MenuFocusManager. Single confirmed event. Restore content and item geometry on disable, reset on reopen. No production visuals until references inspected.

Hold: original Editor newly busy in Battle Play transition. Stage under artifacts/diagnostics/NTSD-MENU-LOOP-CAROUSEL-001/prepared; do not add scripts to Assets during active work. Compile/testing artifacts are not live implementation. Reference image materialization failed on Windows os.setxattr; do not retry or bypass.

Acceptance: compile; motion math stress; Unity focused tests; real scene drag, wheel, keyboard, center click once, rapid input, reopen and boundary continuity. Final visual matching after reference pixels available. Exact changed paths listed in Change Record. Backups and restoration in FILE-OPERATIONS/NTSD-MENU-LOOP-CAROUSEL-001-PREPARE/RECORD.md.
