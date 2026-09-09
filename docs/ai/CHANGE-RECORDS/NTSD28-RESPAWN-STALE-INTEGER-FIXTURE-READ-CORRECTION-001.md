# NTSD28-RESPAWN-STALE-INTEGER-FIXTURE-READ-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-RESPAWN-STALE-INTEGER-FIXTURE-READ-CORRECTION-001
status: VERIFIED
change-kind: TEST_ONLY_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 用户2026-09-09 Goal7 PartA明确授权与GLM复核；Authority battle_world.cpp:8488-8516，Unity BattleRespawnModule.cs:105-119仅写precise X/Z；getter零值回退见LF2Entity.cs:4983-4995。
evidence: Fresh full SelfCheck PASS at 2026-09-09T13:24:41Z, including every respawn stale-int assertion and subsequent global-sync guard. Result Temp/Goal7_RespawnRawInteger_SelfCheck.result. Runtime47/Editor104 warnings, both0 errors. Scene specified SHA unchanged, dirtyfalse. Only scoped assertion/message changed; fixture-only VERIFIED.
-->

[事前Task](../TASKS/NTSD28-RESPAWN-STALE-INTEGER-FIXTURE-READ-CORRECTION-001.md)定义范围、风险、不变量、验证与回滚。
原状：夹具用有zero fallback的getter检验raw整数0。
计划：改为Runtime.XInt/ZInt==0并在失败消息区分rawInteger/getter/precise。
只覆盖该方法断言与消息，不认领SelfCheck其他历史改动；后续扫描断言原样保留。
无production、资源、runtime owner或有序关闭变更；Part B不写文件、不归本Record。

实际已改：仅该方法dead两个getter断言改raw Runtime.XInt/ZInt；消息actualPrecise改precise，actualInteger改rawInteger并另打印getter。逆替换证明方法外和其余断言字节不变。等待fresh SelfCheck与构建验证。

## 最终证据

- fresh full SelfCheck通过既有Temp/NTSD_BattleRuntimeSelfCheck.request入口实际执行，2026-09-09T13:24:41Z输出PASS；副本Temp/Goal7_RespawnRawInteger_SelfCheck.result（4 bytes）。Unity日志同时出现BattleRuntimeSelfCheck.cs:263成功与Editor完成消息。
- RunAll中目标检查之后的CheckRespawnPassFreeEntityGate、stored-count、后续战斗/输入/trace检查均越过，直至最终成功。本次没有下一个失败停点；目标方法全部断言（含respawn scan不得全局同步）通过。
- dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，47 warnings/0 errors，9.34s。
- dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，104 warnings/0 errors，5.14s。
- 原脚本SHA：544A57791A37676CF77FCC508EAA0B06C1AB9F3F42D25946AE90921332899468；修改后SHA：3D1BDECBF89637EF5B3E4C1F60B5378D4B0E9E47186C348DA43C62C2A346B23F。
- 指定instance/2022.3.62f3/NTSD_Battle核验通过，未Play、未启动第二实例。最终Scene dirty=false/root13，SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11不变。
- VERIFIED仅关闭该夹具读取/消息修正；full SelfCheck PASS不证明所有Authority分支或整个战斗系统已对齐。PartB保持零文件改动，结论只在报告。

当前状态：VERIFIED / TEST_FIXTURE_ONLY / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / GOAL8_USER_HOLD

最终治理验证：Tools/Validate-ChangeLedger.ps1 PASS，436 Records / 374 governed code files / 531 warnings，exit0；日志Temp/Goal7_ChangeLedger_Validation.log。授权文件git diff --check exit0，仅Git换行提示。
对照本轮开始前逐路径SHA基线，Temp外仅6个授权文件发生变化：唯一SelfCheck脚本、本Task/Record、Ledger、STATE、对齐总表；production及其余用户文件未变。
