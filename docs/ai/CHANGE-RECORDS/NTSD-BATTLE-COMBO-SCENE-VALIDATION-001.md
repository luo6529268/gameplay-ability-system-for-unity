<!-- CHANGE-RECORD
id: NTSD-BATTLE-COMBO-SCENE-VALIDATION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/ComboSceneValidationTemporary.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleComboView.cs
authority: User authorizes original scene refresh and validation
evidence: artifacts/diagnostics/NTSD-BATTLE-COMBO-SCENE-VALIDATION-001/before.json
-->
# NTSD-BATTLE-COMBO-SCENE-VALIDATION-001
User reports Jump displaying Defend, inactive arrows, static width and no consumption clearing. Original Library Assembly-CSharp.dll at07:40 predates08:25 fixes: binary has old BattleComboInputHistory/rightSprite, lacks keyIcons/consumedDisplaySeconds/NotifyNativeInputHistory. User confirms originalEditor stopped/saved and authorizes refresh+validation. Bridge confirms BattleScene clean,13roots; refresh requested and assembly updated08:46. Existing execute_code cannot compile due Windows command length/Roslyn unavailable; use temporary Editor-only probe with actual saved scene bindings and normal physical-device->local-provider->Driver tick->native routing->events->View. No reassigning icons/activating arrows in test, no DAT/rules rewrite. Controlled Play initial actor frame/PP/reset and pause/explicit ticks are fixture-only; release keys, restore pause and exitPlay. No scene save. Temp probe and .meta will be backed up then removed under this same operation; exact hashes recorded before removal, no permanent tests restored/added. Acceptance actual scene UI/real input/timer plus fresh Unity compile, dirty/hash unchanged. Rollback remove temporary probe only after backup. Runtime source no new changes planned until actual evidence.

CODE_WRITTEN: temporary probe Begin/Run/Queue/Tick/Record/Finish loads original Scene component bindings unchanged, queues physical Keyboard events, uses existing local-provider StepOneTick, checks actual UI and native consumption, writes evidence and exits Play. Original Editor refreshed and temporary probe compiled 08:52 UTC with no CS diagnostics in Editor.log. Runtime source and Scene unchanged in this follow-up.

Live evidence after refresh: get_gameobject_component(ComboPanel51196,BattleComboView) returns new fields but keyIcons=[]; disk scene contains7correct entries. Hot reload preserved old in-memory Scene instance without migrated new array binding. Actual scene reload required after safely exiting Play. Probe Begin refused Requires saved nonplaying scene before touching input/state; another user/task entered Play after confirmed clean idle. Awaiting confirmation before stopping that Play. Evidence live-empty-mapping.json and editor-state-before-reload.json. Disk Scene SHA unchanged. Native selector also confirms only HasRequestedAction routes consume; arbitrary/unrecognized button combinations are not guaranteed to clear under existing rules, and this task will not alter that rule.

PLANNED width correction after original Scene PASS: actual widths1=406,2=407,3=574 reproduce almost invisible first growth. User requests visible increment per added key. Change Render width to max(defaultWidth, single-icon+2*padding)+(count-1)*step; default empty406, nonempty406/573/740/907/1074 with saved80icon/55arrow/32gap. No padding increase or Scene edits. Before View SHA bf860ded7ad9d3f5dc2291384dcfb512db843ac32ef6cd73b6e4fbf7aa947a20 backed up view-before-width.cs.txt; rollback separately audited. Original baseline actual Naruto K->native0->jump.png, L D J->frame285/nativeclear/UIConsumed->real timer->emptybg passed; baseline screenshots/results retained.

# Original Battle Scene Combo verification

Status: VERIFIED for the requested HUD presentation scope, 2026-10-04.

Confirmed causes are separate:
1. Original Editor Assembly-CSharp.dll was07:40 while fixes were08:25; binary lacked keyIcons/consumedDisplaySeconds/NotifyNativeInputHistory, retained six-input history. Old code inverted physical action lookup against jumpSprite that pointed to defend, enabled Image on inactive arrow GameObject, never changed background width and never observed native consumption.
2. Refresh compiled the new source but the already loaded Scene instance had keyIcons=[] (live-empty-mapping.json). Reloading the saved clean Battle Scene restored all7bindings, including native Jump0->Combo_jump.png. No Scene write/save was performed.
3. New width formula had a real presentation weakness:1key406,2keys407. Minimum correction in BattleComboView.Render uses a single-key base plus each additional actual icon/arrow step, without increasing padding. Final1..5 widths406/573/740/907/1074; empty406.

Original Unity Editor2022.3.62f3, actual Assets/NTSD/Scene/NTSD_Battle.unity, existing scene component/resources unchanged; Naruto OID2 and formal Logan runtime DAT. Temporary Editor-only probe queued InputSystem Keyboard device events, called normal local-provider driver StepOneTick while gameplay was paused, and read actual scene Images. Test did not bind Sprites, activate arrows, resize background or send UI events itself. Controlled setup returned actor to frame0 and reset native history; wait90ticks allowed jump to land. This is original Scene Play with synthetic physical-device input/controlled ticks, not human input or normal wall-clock cadence proof.

Baseline run PASS118assertions then final width run PASS137assertions (counts include successful tick-step assertions; not137independent test cases). Final evidence result.json:
- K tick10 -> frame210, native history last0, UIJump with actual Combo_jump.png.
- Direction sequence W/D/A/S/W -> counts1..5 and widths406/573/740/907/1074.
- L tick120 -> Defend9; D tick122 -> Defend/Right with one active arrow,width573.
- J tick124 -> actual native route enters frame285; InputHistory[1..5] all-1; eventConsumed retains Defend/Right/Attack, actual two arrows,width740.
- After650ms wait, actual View coroutine hides all icons/arrows, background remains enabled,width406; native queue remains empty.
- ScreenCapture original-scene-consumed.png and original-scene-empty.png are actual full Game view screenshots.

Earlier isolated45assertions additionally exercised ResourceRejected on actual CharacterActionWriter plus timer cancellation by new native input and main-thread publication from a worker producer. Those scopes remain isolated; they were not rerun as resource-rejected physical-input tests in this original Scene run. Native selector consumes only an actual requested action (including rejected resource attempt); arbitrary/unrecognized combinations or missing action targets do not acquire new auto-clear semantics. No rule change was made.

First original Scene attempt refused to take over a newly active Play session; user then confirmed idle. First owned run timed out during60s resource prewarm before any assertions; initialization-timeout-result.json preserved. Async budget extended300s; subsequent runs passed. All failed/partial attempts are retained.

Fresh original Editor script compilation completed. final-compiler-errors.json returns0 compiler error CS entries; existing unrelated Odin Inspector group errors are not claimed fixed. After temporary probe and its.meta removal, Editor refreshed again, idle/nonplaying/noncompiling, Scene clean13roots. Temporary sources and removal hashes/backups retained in temporary-probe-removal.json and *.backup.txt; no permanent tests added/restored. Scene/BattleHudView/NTSDButton/NTSDButtonRipple SHA unchanged (after.json); only2-line runtime width formula changed in this follow-up (width-only.diff). No DAT/fonts/menu/gameplay edits, no commit/push.

No full BattleRuntimeSelfCheck or formal EXE comparison was run for this UI-only adjustment; no global battle-alignment claim. Original scene-specific requested behavior has runtime evidence now, superseding the preceding original-scene RUNTIME_PENDING limitation.

Final gate: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository> PASS exit0; ledger-final.log preserves existing historical warnings. Process-local core.safecrlf=false only; no Git config changes. Scoped git diff --check exit0.
