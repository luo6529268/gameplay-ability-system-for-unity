# NTSD-MENU-LOOP-CAROUSEL-001

Status: runtime input/motion implemented; visual reference matching pending. Final Play evidence appended below.

## Production changes

- `MenuCarouselMotion.cs`: continuous bounded item position, signed wrap, nearest selection, exponential center snap, drag ownership.
- `MenuLoopCarousel.cs`: original seven items reused; temporary ownership of ScrollRect/content layout; wrapped item relocation outside viewport; wheel/drag/click; state restored on disable, reset on reopen.
- `MenuCarouselOptionPointer.cs`: first click on another item selects, centered click uses existing confirmation.
- `MenuOptionList.cs`: shared selection/focus API and carousel delegation; same-frame confirmation guard for carousel only.
- `SelectGameModeController.cs`: runtime attachment to existing ModeList ScrollRect in Awake; existing event switch unchanged.
- `MenuLoopCarouselEditorTests.cs`: focused motion/input/lifecycle checks and named original Menu Play test.

No Scene serialization required: enter Play and open game-mode selection via existing menu. Existing CanvasScaler is Scale With Screen Size, reference 1920x1080, match height=1; left unchanged. Runtime carousel Inspector: snapSeconds=0.085 (smaller snaps faster), wheelItems=1; spacing derives from existing VerticalLayoutGroup spacing and item/viewport height to hide seam. Parameter changes on runtime-added component are Play-only; persistent defaults currently live in script.

## Evidence

- Prepared Unity-reference compile: `prepared-compile-01.txt`, 0 errors/10 warnings. `prepared/` is an initial implementation snapshot; current production/test files in Assets are authoritative.
- Production motion harness: `motion-test-01.txt`, PASS 925809 assertions; 100 turns each direction, 10001 queued inputs, visible-offset continuity, drag/nearest snap.
- Original Editor script import: `editor-import-01.json`, exact six paths only. First scripts-only compile request missed new unimported types (CS0246); targeted import corrected this. Subsequent original Editor DLLs compiled and named tests ran.
- Original Editor focused test job ac9f72af8634439db6f54e6a0d79c485: 8/8 Passed, 0 failed (`editor-test-result-01.json`). These use event interfaces and focused objects, not physical mouse/keyboard.
- First Scene Play attempt failed before Play because Test Runner supplied an empty scene. Fixture now opens saved Menu only from a clean empty test scene.
- Second Scene Play attempt failed in test captured-counter setup after domain reload; saved XML `editor-play-results-02.xml`. Changed fixture to static counter; no production fix inferred.
- Full Change Ledger first run PASSED, 1180 records / 13 governed code paths. Final validation log is produced separately.
- Menu SHA throughout checks: E8BBC47AE14799BB87DB8B14D9FDB6B9D6CD546E0FA62F47A7DD83E2685D2CBD. No save/revert/commit/push.

## Existing event mismatch (unchanged)

Serialized order: 0 VS, 1 Stage, 2 1V1, 3 2V2, 4 Battle, 5 Demo, 6 Quit.
Controller: 0 opens character selection; 1 Stage placeholder; 2 Battle placeholder; 3 Training placeholder; 4 Options placeholder; 5 Application.Quit; 6 no handler.
Changing this mapping needs an explicit semantic decision. Runtime tests confirm only Stage/index1 to avoid quit or scene side effects.

## Remaining

Two references still unavailable locally: Library resolved them but Windows helper failed on os.setxattr; per instruction no further retry/bypass. No center scaling/white-red-black outline/distance color-opacity-softness implementation or visual-match claim yet. Retained existing fonts and styling. Existing Editor logs prove JifengBladeArtSC-0480 SDF/fallback lacks 请、稍、候 and full-width comma; font task not expanded.
Physical hardware mouse/keyboard, Game View pixel continuity and reference matching remain unverified. Initial Scene ownership hold was cleared by user's 22:13 saved confirmation; later Battle Play was observed read-only and allowed to finish before integration.

Final independent input/motion acceptance: original Editor runtime and Editor scripts compiled; named focused suite 8/8 PASS. Final original Menu Play UnityTest 2026-10-02 22:32:31Z–22:32:58Z Passed (editor-play-results-03.xml): 3 forward/3 reverse turns, synthetic event-interface drag/wheel, center snap, close/reopen with seven original items, unselected click selects, centered Stage click plus same-frame confirm fires once. Production code unchanged after 8/8 suite. Full Change Ledger final log PASS 1180 records/13 governed paths; source diff check passed.
Status RUNTIME_PENDING is retained for missing reference-based visuals and physical hardware/pixel acceptance, not for the already-passed event-interface Play scope. Two reference pixels never read; no size/color/outline/softness styling added. Screenshot request produced no file and is not a deliverable.
Bridge job persistence failed across Enter/ExitPlayMode; final XML from existing project Test Runner exporter is the acceptance evidence. Failed first/second attempts retained; no complete success inferred from stale job running status.

FINAL PROTECTION CORRECTION (supersedes all earlier "Scene hash remained E8..." claims for the end of this turn):
Final read-only scene query is Menu/isDirty=false, but disk SHA is now 5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D, different from earlier E8BBC47A...D2CBD. Writer/cause unknown; no save/revert/reload was requested by this task. All further Editor operations stopped. The final 1/1 Play XML establishes its event-interface assertions only, not disk protection. Status additionally MENU_DISK_CHANGE_OWNER_PENDING. Current bytes/timestamp/hash preserved in docs/ai/FILE-OPERATIONS/NTSD-MENU-LOOP-CAROUSEL-001-SCENE-OBSERVATION/. User/project confirmation of intended saved change is needed before further Editor activity.
