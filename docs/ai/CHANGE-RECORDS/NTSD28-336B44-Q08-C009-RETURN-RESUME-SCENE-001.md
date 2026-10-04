<!-- CHANGE-RECORD
id: NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08C009ReturningGroupBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and playable BattleFlow28 GameSession28 with OID220 to OID9 natural OPoint
evidence: docs/ai/TASKS/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001.md
-->

# C009 原 Battle Scene 再次单组定向 Play 扩展

脚本修改前登记。Unity 原探针仅支持 `c009-oid304-return-scene-v1`，已给出返组暂停12tick场景证书；当前正式源/根新增 OID220→OID9 消失恢复的20tick、7字段140/140证据，但 Unity 原场景缺同初态证书。唯一脚本路径如元数据所列，计划在原探针扩一个独立 runId/config/20tick 断言和新结果路径，原请求与结果保持可回归。

不可回退边界：不修改生产结果 writer、工厂、Bootstrap、DAT、图片、Scene、Prefab、GameConfig、项目模式及非战斗；不在编译旧程序集启动 Play、不覆盖既有报告。副作用仅在显式新请求被提交时，Play副本短暂产生正式对象并写新诊断JSON；退出需四SHA与Scene clean。验收先 C# 编译，再原Editor定向完整 tick 与正式源/根比较；物理按键和全World仍待。回滚仅按本Change审阅脚本局部变更，不删除旧证据；删除须另行授权及审计。

2026-10-04 实施：实际只改声明的既有Editor脚本，新增runId/结果路径选择、OID220/action0发射者参数、OID9子体计数与20tick timer/mask预期；旧OID304入口、原结果路径、12tick预期不变。正式`data/data.txt`、OID220/OID9/OID56 DAT四对正式/Unity SHA同。生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0、0 error、299 warning；[日志](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/editor-generated-project-build.txt)。原Editor仍`is_compiling=true`且DLL早于脚本，故Unity编译/Play未证；请求仍旧runId且`requested=false`。状态`RUNTIME_PENDING`，不改变生产/DAT/Scene/非战斗。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/REPORT.md)。

治理校验：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exit0，1233条Record、当前差异34个受治理代码文件，本脚本由本ID与既有返组ID覆盖；[输出](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/change-ledger-validation.txt)。脚本 `git diff --check` exit0。原Editor真实编译、Play和场景保护仍未验证，不能升级状态。

2026-10-04 同一C009 Scene出口续验（再次修改脚本前登记）：原Editor重开同一项目后程序集已新编，v1 20tick结果分支PASS、Play退出clean、四保护SHA相同；但`emitterAction`采自保留对象引用，不能断言World成员身份。唯一拟修改路径仍为元数据中的Editor探针；新增独立v2 runId/请求文件及`FindEntityByRuntimeSlotForQuery(50)`的成员身份和动作采样，预期tick1在World/action1、tick2起不在World。原v1 JSON不可覆盖，生产、DAT、Scene及非战斗不变。完成后只跑v2定向Play与生成工程编译，核对正式源/根配对、四SHA和clean；若World差异存在，另立生产Change才可修复，不在此脚本Change中处理。回滚仅审查v2脚本增量，不清理旧诊断原件。

2026-10-04 实际v2：只在所列Editor探针增独立请求/runId、World槽50成员及动作字段、仅v2生效的20tick正式消失断言；保留v1/旧请求和所有旧结果。生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0、0 error、342 warning；原Editor程序集 UTC 05:07:13 晚于脚本 UTC 05:06:14、完成域重载，v2 Play `PASS/DONE`。正式源/Unity配对20×7=140/140零差；退出Play，唯一Battle Scene clean、四SHA于v2运行内一致、测试未运行。原探针引用的action1000不是World成员，槽50 tick2起为空；因此本Change `VERIFIED` **仅限受控原Scene恢复分支**。Battle Scene在v1/v2之间SHA不同且写入者未知，未回退。物理键、其它场景、Q08/Q12继续开放。[结果](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/REPORT.md)。
