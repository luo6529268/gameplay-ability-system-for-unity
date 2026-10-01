# Q07/C050 特殊链接跳垂直反应：原 Battle Scene 限定出口

状态：`VERIFIED_SCOPED_SCENE`。父 C050、Q07、336B44 总目标仍开放。

正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 live path、DAT 入口及根近 X520／远 X1200 的 40 tick 证据见[正式入口报告](../NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001/REPORT.md)。原 Unity 生产 Driver 修前近距错误进入 action186，通用 writer 修复及近远完整 Driver 对照见[首差报告](../NTSD28-336B44-Q07-C050-UNITY-DRIVER-001/REPORT.md)。本包没有再改生产代码或 DAT 数值。

原项目 Unity 2022.3.62f3 Editor PID 105896，单一 clean `NTSD_Battle` Scene。新增[定向 Editor 探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C050VerticalSkipScenePlayProbeEditor.cs)在 Play 副本的 `BattleTestBootstrap.Start` 前设置正式 OID24/56 roster；mode0、difficulty0、seed682973786、actor action37/X500、target action259/X520 或 X1200、双方 Z400、HP/MP500、面右、队伍1/2。等原 World 稳定暂停后，分别走生产 `SimulationTickDriver.StepOneTick` 的 3 个中性输入 tick。两份结果均为 `SCOPED_PASS / DONE`、`configuredBeforeStart=true`、`exitedPlay=true`、`sceneCleanAfter=true`。

| Scene 条件 | 第 2 tick | 第 3 tick | 与正式根同 tick 对照 |
| --- | --- | --- | --- |
| 近 X520 | actor 38；target 259，HP465，Vy0，hold -3 | target 259，HP465，Vy0，hold -2 | actor/target 动作、HP、Vy、hold 共 24/24 字段零差 |
| 远 X1200 | actor 93；target 261，HP500，Vy0 | actor 186，HP435；target 261，HP500 | 同八字段×三 tick，24/24 零差；远距目标未被该击命中 |

[逐字段比较原件](root-scene-comparison-v1.json)保留两条正式根 trace 与 Scene 结果各自 SHA、全部比较字段和零差结果。[两份 Scene 原始结果](x520-scene-v1.json)、[远距原始结果](x1200-scene-v1.json)各自独立保存，不覆盖；没有用探针内局部 PASS 代替逐字段对照。

原 Editor 已回到 idle、非 Play、非编译；Battle Scene `isDirty=false`。Menu、Battle、GameConfig、ProjectBattleModeConfig 四文件的前后 SHA 完全一致，`Assets/NTSD/Content/LoganRuntime` 无 Git 差异。两次新建临时请求被探针自动消费，精确路径、载荷 SHA、事前 manifest、实际时间和事后状态见[文件操作记录](../../../docs/ai/FILE-OPERATIONS/NTSD28-C050-SCENE-REQUEST-20261001-001/RECORD.md)与[事后 JSON](request-post-v1.json)。原 Editor 导入编译成功；生成 `Assembly-CSharp-Editor.csproj` 的 `dotnet build --no-restore -v:q -clp:ErrorsOnly` 为 0 error、241 warnings。没有运行全量 SelfCheck，因为本包只加隔离的 Editor 诊断，生产规则未再变。

此结果仅证明正式动作初态后，原 Battle Scene 中三 tick 的选定可观察状态与当前正式根一致。物理按键自然选出前驱动作、逐 hit 内部决策、完整 World/checksum、画面声音和整场尚未由本包验证；C050/Q07 不能据此写成完全对齐。
