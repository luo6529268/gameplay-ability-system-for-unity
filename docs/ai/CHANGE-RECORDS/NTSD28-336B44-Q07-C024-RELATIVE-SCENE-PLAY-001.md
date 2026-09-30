<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C024-RELATIVE-SCENE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07RelativeNextBattlePlayProbeEditor.cs
authority: selected336B44 currentKarin433 naturalOPoint315 relative13xx transition
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C024-RELATIVE-SCENE-PLAY-001.md
-->

# NTSD28-336B44-Q07-C024-RELATIVE-SCENE-PLAY-001

Created before code. Exact scope, source/root preconditions, comparison declaration, shutdown, validation and rollback in Task. Only diagnostic code, no production or serialized Scene/resource changes.

Actual diagnostic written: existing Editor lifecycle reused in separate owned probe; roster77/2 before Start, Karin433 initial action, full neutral32tick, runtime-slot indexed roster/315 samples14fields and RNG scalars. No forced child/roll/action300. CompleteMeasurement uses source tick8/9/10 action300/counter0→300/1→314/0 and one sync call at0x452390; comparison remains independent. No production changes. Compile/Scene pending.

原Editor Assets/Refresh：Tundra build success5.00s、Editor DLL21:05:56；MCP重载后确认idle/nonPlay/noncompiling，原Battle active。提交唯一relative-next-scene-01，等32tick/字段对照/正常退出，不能由观察超时重启。


2026-09-30 C024相对next门限定关闭：正式Karin433→434自然OPoint315/43→44→50→300；tick8/9为300/counter0/1，tick10以next1320选314/counter0，同步调用恰增1/site0x452390。336B44根正例32tick2288声明字段一致，直接434无生成控制707字段一致；原Battle Scene唯一relative-next-scene-01 PASS/DONE，tick5→37，125实体行14字段及32tick六个源/Unity RNG标量共1942/1942，首差0、速度差0。原Editoridle/nonPlay/noncompiling，exit/Scene clean/四保护SHA稳。既有聚焦4/4、完整tick1/1和相邻证据有效；本轮只新增诊断，不改生产/DAT/Scene/Asset/非战斗。CRT初始seed未被LFR携带与根EOF33排除均保留，非全World/checksum或物理键选招证书。Q07/总目标仍开。


最终检查（2026-09-30）：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS，1062条Record/22个受治理代码文件，输出在RELATIVE-SCENE-PLAY-001/ledger-check.txt；`git -c core.safecrlf=false diff --check`退出0。诊断C#在原Editor编译0错，Scene01完成32tick、1942声明字段通过/正常退出/四SHA稳。源诊断各版本实际compile结果留证，当前源码对应v5快照。未跑无关角色/全量测试，既有聚焦证据没有被新生产改动失效。本轮新增诊断和证据/进度文档，没有生产脚本、DAT、Scene、配置Asset或非战斗改动。总目标ACTIVE，C023原Scene/自然门、F02及其余总表项继续开放。
