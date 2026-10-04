# NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001

状态：`VERIFIED_SCOPED_SCENE / Q08_OPEN`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q08/C009；依赖[正式源/根限定阳性](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-001/REPORT.md)。

2026-10-04 v2出口：原Editor已编译新探针、完成原Battle Scene 20tick；与正式源码7选定字段140/140零差。槽50 tick2起空，v1 action1000只是回收对象引用；退出非Play、唯一Scene clean、四SHA在本次运行内一致。仅C009受控恢复子门关闭，物理键和Q08整体开放。[结果](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/REPORT.md)。

仅扩展现有原 `NTSD_Battle` Play 副本探针 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08C009ReturningGroupBattlePlayProbeEditor.cs`，新增独立请求 `c009-oid220-resume-scene-v1`，保留原 `c009-oid304-return-scene-v1` 行为与原结果路径。新请求由正式暂存 OID56/team1、生产对象工厂 OID220/type3/action0/team2 构建同 seed、同源位置和中性输入，暂停生产 Driver 后推进20完整 tick，记录结果 timer/output/group mask、OID9子体数和发射者动作；核验 tick1 timer1、tick2～4暂停1、tick4子体结束、tick5起恢复，并与正式源/根的同相位数据对照。

修改前后必须保持原Battle Scene单独加载且clean、非Play、无运行测试，旧程序集未更新时不得发请求。请求只在 Editor `isCompiling=false` 且新 DLL 时间晚于脚本、内容根和正式 DAT 引用齐备时发出；Play退出后核对 Battle/Menu/GameConfig/ProjectBattleModeConfig 四文件SHA及Scene clean。只准改声明的 Editor 诊断脚本，不能改 Unity生产、DAT数值、Scene/Prefab、项目背景模式和非战斗逻辑。受控初始 OID220 不等于玩家物理按键；即使通过，C009/Q08仍待物理输入门与其他全World出口。

验证：先生成 Editor C# 工程编译0错，后原 Editor 编译及定向 Play；本轮原 Editor 当前长期 `is_compiling=true`，允许诚实停在 `COMPILE_PASS/RUNTIME_PENDING`。回滚只审查该脚本的新增分支与 Task/Change/输出；不清理或覆盖已有请求/结果，删除需用户授权与文件操作审计。

2026-10-04 实施：脚本已按声明加独立runId、OID220/action0与OID9/20tick预期；原runId和12tick路径保留。四个正式DAT与Unity暂存逐SHA同；生成Editor工程`dotnet build --no-restore` exit0、0 error。原Editor仍编译中且DLL旧，未写新请求、未Play；不能称Unity runtime通过。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURN-RESUME-SCENE-001/REPORT.md)。

2026-10-04 续验范围（探针二次修改前登记）：原Editor现已在同一项目重新编译，首轮`c009-oid220-resume-scene-v1`于原Battle Scene完成20tick并报计时/组别/子体PASS，退出Scene clean、四SHA稳。首轮`emitterAction`只读保留对象引用，tick2的1000不能证明World仍有对象。仅在同一Editor诊断脚本增加独立`v2`请求、World槽50成员/动作采样与正式tick2起消失断言；保留v1原件、原请求与既有路径，不改生产或资源。v2定向Play后配对正式源/根、复核Scene和四SHA；若失败仅记录首差，不为测试修改生产。回滚只审查本脚本的v2局部增量，旧输出不删除/覆盖。
