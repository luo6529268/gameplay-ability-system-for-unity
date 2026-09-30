# C024 原 Battle Scene 自然子体相对选帧

状态：`VERIFIED_SCOPED`。以下启动阶段记录由后面的关闭证据覆盖。原 Editor 的唯一 `relative-next-scene-01` 请求已提交，等待同次32tick/正式源比较/正常退出，不能由观察超时再次启动。

前置根证据：正式香燐433→434生成315/43，后续自然43→44→50→300，tick10以next1320选314。源/336B44根32tick共2288声明字段相同，独立 CRT 初始seed不在LFR中，限制已在根报告明确。

原 Editor Assets/Refresh 编译0错，Tundra5.00s，Editor程序集21:05:56；重载后MCP确认idle/nonPlay/原Battle active。探针在Play clone配置77/2，初始化角色后暂停，使用生产Driver和中性输入推进，不手动生成315或设置300/随机roll。

待验字段：自然315主子体及附属子体/两个roster的14实体字段、六个源/Unity RNG标量、触发tick8/9/10的action/counter和同步调用边界。原场景其余实体不纳入这份证书。待正常退出、Scene clean和四个保护哈希复核；原报告及所有失败保留。


2026-09-30 C024相对next门限定关闭：正式Karin433→434自然OPoint315/43→44→50→300；tick8/9为300/counter0/1，tick10以next1320选314/counter0，同步调用恰增1/site0x452390。336B44根正例32tick2288声明字段一致，直接434无生成控制707字段一致；原Battle Scene唯一relative-next-scene-01 PASS/DONE，tick5→37，125实体行14字段及32tick六个源/Unity RNG标量共1942/1942，首差0、速度差0。原Editoridle/nonPlay/noncompiling，exit/Scene clean/四保护SHA稳。既有聚焦4/4、完整tick1/1和相邻证据有效；本轮只新增诊断，不改生产/DAT/Scene/Asset/非战斗。CRT初始seed未被LFR携带与根EOF33排除均保留，非全World/checksum或物理键选招证书。Q07/总目标仍开。

[原Scene01](relative-next-scene-01.json)、[1942字段比较](source-unity-comparison.json)、[退出后四SHA](protected-hashes-after.json)、[编译探针快照](compiled-probe-snapshot.txt)。


最终检查（2026-09-30）：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS，1062条Record/22个受治理代码文件，输出在RELATIVE-SCENE-PLAY-001/ledger-check.txt；`git -c core.safecrlf=false diff --check`退出0。诊断C#在原Editor编译0错，Scene01完成32tick、1942声明字段通过/正常退出/四SHA稳。源诊断各版本实际compile结果留证，当前源码对应v5快照。未跑无关角色/全量测试，既有聚焦证据没有被新生产改动失效。本轮新增诊断和证据/进度文档，没有生产脚本、DAT、Scene、配置Asset或非战斗改动。总目标ACTIVE，C023原Scene/自然门、F02及其余总表项继续开放。
