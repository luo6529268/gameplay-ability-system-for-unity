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
