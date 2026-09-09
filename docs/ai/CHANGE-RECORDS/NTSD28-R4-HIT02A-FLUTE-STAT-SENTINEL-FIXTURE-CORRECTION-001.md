# NTSD28-R4-HIT02A-FLUTE-STAT-SENTINEL-FIXTURE-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-R4-HIT02A-FLUTE-STAT-SENTINEL-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIXTURE_EXPECTATION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 用户2026-09-09 Goal5 PartA；直接采用GLM结论及已VERIFIED的NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001。Authority BattleWorld28::resolve_special_relation_hit battle_world.cpp:5453-5457不写Unity legacy统计，保留既有frame/motion合同。
evidence: 55 sentinel/expectation/message only. Fresh SelfCheck12:41:25Z passed four actual/shared x tick12/13 cases and stopped later at respawn stale-int:28849. Runtime47/Editor104 warnings, both0 errors; specified Scene SHA unchanged. Fixture-only VERIFIED.
-->

完整边界与验收见[Task](../TASKS/NTSD28-R4-HIT02A-FLUTE-STAT-SENTINEL-FIXTURE-CORRECTION-001.md)。
只覆盖本方法的本轮增量，不将SelfCheck整文件历史归给本包。不改production及其他条件。
变更没有运行时所有者、持久数据或资源副作用；不涉及kind10的两跳owner源链。

## 最终证据

命中前新增preservedComboCount=55及holder赋值，期望保持该值，消息补统计实际值；其他条件不变。
fresh SelfCheck于2026-09-09T12:41:25Z停在后续CheckRespawnReadsPhysicsTailIntegerCoordinates():28849。
expectedPrecise=(60,61)，actualPrecise=(60,61)，actualInteger=(60,61)，stale=(10,20)/(30,40)/(50,60)/(70,80)。
原四组检查在RunAllChecksStatic第202行，后续停点调用在第230行，因此四组全部越过，未修后续检查。
结果：Temp/Goal5_R4Hit02A_StatSentinel_SelfCheck.result。
源码SHA：544A57791A37676CF77FCC508EAA0B06C1AB9F3F42D25946AE90921332899468。

## 公共验证与边界

最终执行 `Tools/Validate-ChangeLedger.ps1`：PASS，434 Records / 374 governed code files / 531 warnings。warnings未在本包扩大处理。

dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，47 warnings/0 errors，00:00:06.92。
dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，104 warnings/0 errors，00:00:06.26。
Unity按指定instance核验2022.3.62f3/NTSD_Battle，未进入Play。Scene dirty=false/root13，SHA保持D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
VERIFIED分别仅关闭期望修正/取证，不代表完整kind10、full SelfCheck或原stress测试通过。production不变，完成后停止等待Goal6。

当前状态：VERIFIED / TEST_FIXTURE_ONLY / FOUR_CASES_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_LATER_RESPAWN_STALE_INT / GOAL6_USER_HOLD
