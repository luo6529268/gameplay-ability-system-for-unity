# NTSD28-336B44-Q07-C042-DELAYED-CATCH-SCENE-001

状态：`VERIFIED / NATURAL_NONZERO_SCOPED_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C042/R12；C042 与 Q07 总出口仍开放。

2026-10-02 实施：仅在现有 Editor 探针的 `Request` 增加 `targetX`，`TryStart` 对 X550/650/800 和匹配 runId 做有界校验，再将请求值写入 `Report.targetX`。原 Editor 编译 0 错；X650/X800 原 Battle Scene 各 60 tick×20 字段 1200/1200 与正式源码一致，退出 Play、Scene clean、四保护 SHA 稳定。限该自然入口与所选字段 `VERIFIED`；[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-DELAYED-CATCH-20261002/REPORT.md)。

目标：验证正式版自然延迟抓取是否让被投者的非零动作计数在投掷后保留，并与原项目 Battle Scene 的生产 Driver 同 tick 对照。正式 OID75/action355/X500 对鸣人 OID2/action0：目标 X650 在 tick2 抓取、tick56 投掷前计数1；X800 在 tick4 抓取、tick58 投掷前计数3。两组正式 playable 源各160 tick 和 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 根 EXE 的同 LFR 回放均 `passed=true/failureCode=0`，按20个选定字段各3200/3200零差；CRT 原始 state、根附加终端 tick 和完整 World 不在该证书内。原件在 `artifacts/diagnostics/NTSD28-336B44-Q07-C042-DELAYED-CATCH-20261002/`。

脚本改动仅限 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs` 的 `Request.targetX`、请求校验和 `Report.targetX` 初始化。沿用其现有 Battle Scene、正式 LoganRuntime、同 seed/mode、60 tick 生产 `SimulationTickDriver.StepOneTick`、独立 runId 结果、防覆盖与退出保护。允许的请求位置限定为正式已证的 X650 或 X800；现有 X550 请求仍须兼容。不得改生产规则、DAT 数值、图片、Scene、Prefab、ProjectSettings 或非战斗功能，也不得覆盖旧报告。

验收：修改前保留正式源/根结果；原 Editor 唯一项目、非 Play/Scene clean 时导入编译0错；X650 与 X800 分别以独立请求完成原 Battle Scene 60 tick；逐 tick 比较动作、计数、规则 X/Y、速度、HP、抓取关系和 RNG，并专门核 tick56/58 投前与投后被投者计数；退出 Play 后 Scene clean、借用归零、Battle/Menu Scene 与两个配置资产 SHA 稳定。只运行受影响探针及必要编译/账本检查；不因局部通过宣布全 World、物理按键、C042/Q07 整体完成。

风险与回滚：共享 Editor 中的 Play 副本可能受另一个任务占用，先核状态；60 tick 内可能出现源/Unity 首差，必须保留独立失败原件再分析，不改测试预期掩盖。回滚仅审阅本 ID 的探针参数化差量；任何 `git restore`/删除仍需另获明确批准，现有脏文件全部保留。

验收边界更正：原验收项中的“借用归零”没有独立计数输出，本次只证明探针报告 Scene clean，不把两者等同。X800 投掷前计数3，完整 tick 后计数1，符合 action181 帧入口正常重置；即时投掷 writer 保留非零值由父任务聚焦断言覆盖。未验证完整 World、物理按键、其它自然动作或 Q07 整体。

审计：`Tools/Validate-ChangeLedger.ps1` PASS（1134 Records），`git -c core.safecrlf=false diff --check` PASS；validator 原件在同 ID 诊断目录。未执行全量 SelfCheck 重跑；近期完整自检 PASS 属于其它独立 Change，不能充当本子包的新证据。
