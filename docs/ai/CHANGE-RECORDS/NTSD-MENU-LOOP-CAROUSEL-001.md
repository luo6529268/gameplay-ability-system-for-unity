<!-- CHANGE-RECORD
id: NTSD-MENU-LOOP-CAROUSEL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Menu/MenuCarouselMotion.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuLoopCarousel.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuCarouselOptionPointer.cs
code-path: Assets/NTSD/Scripts/UI/Menu/MenuOptionList.cs
code-path: Assets/NTSD/Scripts/UI/SelectGameModeController.cs
code-path: Assets/NTSD/Scripts/Test/Editor/MenuLoopCarouselEditorTests.cs
authority: User requested infinite centered mode selector; follow-up authorizes independent input and motion implementation before image access
evidence: docs/ai/TASKS/NTSD-MENU-LOOP-CAROUSEL-001.md
-->

# NTSD-MENU-LOOP-CAROUSEL-001

Pre-change: MenuOptionList owns seven indexed entries and discrete navigation; SelectGameModeController has legacy 0..5 handlers. Scene and TMP resources already modified by user. No events/index mapping changes authorized. No battle authority changes.

Scope: prepare continuous cyclic motion, drag/scroll/click through existing focus owner, center snap and lifecycle reset; runtime-only attachment from existing mode controller avoids Scene serialization. Preserve fonts/text, seven original option objects and OnOptionConfirmed(index). No DAT/background/font/input-action changes. Reference pixels unavailable: no final styling or visual match claimed.

Pre-change requirement: user confirmed saved Menu at 22:13 UTC (Sentinel_749667c92b3c8191a400c16f8270e514). Subsequent read-only original Editor PID105896 port6401 showed Battle/Play transition. Stage code outside Assets until it is idle, never force stop/reload. Scene SHA E8BBC47AE14799BB87DB8B14D9FDB6B9D6CD546E0FA62F47A7DD83E2685D2CBD.

Side effects: while enabled carousel temporarily owns ScrollRect/content layout and item positions; disables native ScrollRect motion to avoid dual writers, restores captured layout on disable. No cloned option events. Confirmation only via list, frame guard for pointer/keyboard overlap; dragging blocks confirm.

Validation: standalone production motion tests (signed multi-turn wrap, rapid input, snap, nearest selection, bounded phase), compile prepared Unity code, named focused Unity tests and real Menu pointer/keyboard/reopen/confirm once after original Editor is free. Missing references keep visual acceptance pending.

Rollback: prepared artifacts can remain inert; once integrated, forward patch only declared files from operation backups. No destructive Git, no Scene restore. No irreversible boundary. Implementation and evidence appended below.

Code integrated after original Editor returned clean/idle Menu. All six declared files installed from prepared copies, existing source preimages hash-checked. Five runtime files plus focused test. No Scene/data/font modifications. Isolated prepared compile 0 errors/10 warnings; standalone model 925809 assertions PASS. Original Editor compile/focused/Play pending.

Original Editor compile completed after importing six exact script paths; first scripts-only request missed new assets, causing transient CS0246. Named test job ac9f72af8634439db6f54e6a0d79c485 completed 8/8 PASS. Next add explicit original Menu Scene Play UnityTest to the already-declared test file; no physical-input/visual-match claim.

First Scene Play job failed before entering Play because Unity Test Runner runs the method in its own empty scene. This is a fixture precondition failure, not production behavior. Test now loads the saved Menu only when the runner scene is clean and has zero roots; never saves/discards a dirty scene.

Scene Play 02 final result recovered from existing Temp/Goal18_LastTestResults.xml because MCP job status remained running after domain reload. Final XML is FAILED/NullReference at test captured-counter initialization (line225), after navigation/drag/snap/reopen statements; not a complete acceptance. Replace lambda-captured local with static test counter to survive EnterPlayMode reload; preserve XML and prior test source.

Final independent input/motion acceptance: original Editor runtime and Editor scripts compiled; named focused suite 8/8 PASS. Final original Menu Play UnityTest 2026-10-02 22:32:31Z–22:32:58Z Passed (editor-play-results-03.xml): 3 forward/3 reverse turns, synthetic event-interface drag/wheel, center snap, close/reopen with seven original items, unselected click selects, centered Stage click plus same-frame confirm fires once. Production code unchanged after 8/8 suite. Full Change Ledger final log PASS 1180 records/13 governed paths; source diff check passed.
Status RUNTIME_PENDING is retained for missing reference-based visuals and physical hardware/pixel acceptance, not for the already-passed event-interface Play scope. Two reference pixels never read; no size/color/outline/softness styling added. Screenshot request produced no file and is not a deliverable.
Bridge job persistence failed across Enter/ExitPlayMode; final XML from existing project Test Runner exporter is the acceptance evidence. Failed first/second attempts retained; no complete success inferred from stale job running status.

FINAL PROTECTION CORRECTION (supersedes all earlier "Scene hash remained E8..." claims for the end of this turn):
Final read-only scene query is Menu/isDirty=false, but disk SHA is now 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D, different from earlier E8BBC47A...D2CBD. Writer/cause unknown; no save/revert/reload was requested by this task. All further Editor operations stopped. The final 1/1 Play XML establishes its event-interface assertions only, not disk protection. Status additionally MENU_DISK_CHANGE_OWNER_PENDING. Current bytes/timestamp/hash preserved in docs/ai/FILE-OPERATIONS/NTSD-MENU-LOOP-CAROUSEL-001-SCENE-OBSERVATION/. User/project confirmation of intended saved change is needed before further Editor activity.
