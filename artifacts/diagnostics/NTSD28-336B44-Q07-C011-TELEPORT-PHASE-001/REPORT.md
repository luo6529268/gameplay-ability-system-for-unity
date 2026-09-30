> 2026-09-30 C011限定关闭：336B44正式根v3四例12tick/864声明字段PASS；原Battle Scene Lee02与Sakura01各12tick/144字段严格一致，合计288/288、首差0。李tick4 X680/Z401，小樱tick4 X740/Z401；两轮均PASS/DONE/exitedPlay/sceneCleanAfter，MCP确认原Editor idle/nonPlay。两Scene、GameConfig、ProjectBattleModeConfig四SHA保持，DAT与生产脚本未因补证修改。初次Lee01计数字段/地图边界夹具失败和根v1 EOF46原记录保留。该出口证明受控初态后的正式DAT自然帧链与相位，不声称物理键选招、全World/全画面或Q07整组完成。下一G1/C012。

# NTSD28-336B44-Q07-C011 teleport phase

2026-09-30 root/Scene addendum: newformalroot fourcases12tick18fields864/864/PASS, retainedBG23/Z650v2 and commonZ400/BG1v3 separately. NaturalLee350→242/Sakura96→97 teleporttick4, initiallyteleportframecontrols tick2; current frame is readbeforetailadvance. FirstrealDAT SceneLee01 finishedDIFFERENCE/exit/clean withonlyprobecounter(AnimSubwrong) andoutsideprojectwalkZ650→481 fields different; X/action/HP/timing match. Revisedprobe readsAttackingCounter/usescommonZ400 andUTC10min startup, compiledinoriginalEditor; uniqueLee02 inprogress, thenSakura01afterexit. [Root report](../NTSD28-336B44-Q07-C011-ROOT-TELEPORT-001/REPORT.md). Parent remainsopenfornaturalScene; rootpending statementsbelow are historical supersededbythisaddendum.

Status: `UNITY_BATTLE_PLAY_PASS / FORMAL_ROOT_PENDING`. Parent: BATCH-04/Q07. This is a scoped state400/401 scheduling result, not whole Q07 closure.

Authority: selected formal root `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`. Its declared playable `simulation_tick_driver.cpp::SimulationTickDriver28::step` advances `battle_world.cpp::advance_teleport_phase_4a0bd8` from zero at tick entry and calls state400/401 teleport after frame motion only at phase zero. Thus tick1/phase1 skips and tick2/phase0 executes. The source code is in the playable closure; formal-root same-state observation remains pending.

Unity first difference: `SimulationWorld.AdvanceBattleFlowTick` already flipped the corresponding `FrameToggle` each complete tick, but `NTSDBattleTickSystem.NativeTeleport` called `world.NativeTeleportAll()` on both phases. The production change gates only that canonical tick call on `FrameToggle==0`. Direct `NativeTeleportAll()` mechanism callers, state500 specials, frame motion and physics order were untouched. The old C05 Battle Play probe was rebaselined to check phase scheduling over four complete ticks. It no longer applies obsolete identity-viewport coordinate constants to the project's shared proportional spatial projection.

| Evidence | Result |
| --- | --- |
| Original Editor exact RED `129213d57b694bf7a3480c7ed20e38d9` | Both complete-tick state400/state401 cases failed at tick1/phase1: expected X100, observed X180/X240. |
| Original Editor exact GREEN `5789a9a79da1492099806100d0fb57e8` | 3/3 passed: state400 two-tick phase, state401 two-tick phase, and old direct teleport mechanism control. |
| First original Battle Play probe | Phase-1 state400 skip passed. Phase-0 action ran with Y=-31 and zero motion, but old hardcoded identity-view X180/Z131 assertion failed because current shared projection/stage yielded X115/Z238. This is a probe expectation failure, not counted as PASS. Play exited and probe cleanup passed. |
| Second original Battle Play probe | [Raw JSON](play-pass.json), SHA-256 `0A797ED394B01C68C6C00AF776FED084327CCEE033A1A96269271CC0B418B447`: `PASS`, four complete ticks 928–931; state400 and state401 both phase-1 skip and phase-0 teleport passed, collision Y -31/-29 and all motion axes zero after teleport, object/slot cleanup passed. |
| Post-Play integrity | Original Unity 2022.3.62f3 Editor idle and out of Play; Battle Scene `isDirty=false`. Battle Scene SHA `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`, Menu `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`, GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82` unchanged from pre-Play. |

Remaining: formal root EXE same-seed, same-state phase trace and any natural selected DAT state400/401 sequence needed by Q07 integration. Do not promote this package to formal `VERIFIED` or Q07 whole-stage closure yet. No full test-suite claim is made.

Governance: Change Ledger validator passed (1034 records, 81 governed code files in the existing dirty diff); scoped `git diff --check` passed. These checks cover change tracking and whitespace only.
