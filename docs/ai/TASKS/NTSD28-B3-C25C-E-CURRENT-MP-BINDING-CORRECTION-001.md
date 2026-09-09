# Task Contract — NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001

> 状态：`VERIFIED / CURRENT-MP-PP-BINDING / RAW-PROJECTION-CORRECTED / NO-RUNTIME-BEHAVIOR-CHANGE`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25c-e prerequisite`
> 依赖：`NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001 / VERIFIED`

## 目标

纠正Authority `EntityState28::current_mp`在Unity中的唯一绑定，消除旧B0 parity/raw `Runtime.MP`与正式action cost/damage/C06/C25b `Health.PP`之间的矛盾，为C25c/e所有MP读写建立不可歧义入口。

## Authority与生产链

- Authority `try_commit_action`、hit resource、C25c pre-display、C25e state64/66及max_mp clamp均读写同一`current_mp`。
- Unity正式native action cost与damage resource writer读写`Health.PP/Runtime.PP`；`Runtime.MP`未被这些生产路径同步更新。
- 初始生成常同时把MP/PP设为同值，不能用500==500证明旧绑定；测试必须令MP与PP互异。

## 允许代码路径

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- 必要的NTSD28Parity self-test/README、当前Task/Record、Ledger、STATE、handoff、总表及C25c-e manifest。

## 不做

- 不删除或重定义Unity旧`MP`字段；只撤销其作为native current_mp的authority绑定。
- 不同时写MP和PP，不改input/damage/C06/C25b现有生产逻辑。
- 不实现C25c-e算法、carrier或内容schema，不改Config/Scene/Prefab/Authority。

## 验收

- test-first用`MP != PP`证明旧raw exporter错误选择MP。
- contract明确`current_mp -> NTSDEntityRuntime.PP / LF2Health.PP`；raw JSON输出PP。
- parity tool build/self-test、Unity compile/focused/broad/SelfCheck通过；无Play需求（诊断投影纠正，不改runtime行为）。
- 文档追加correction/supersede，不篡改旧B0历史证据。

## 回滚

恢复EntityFieldContract和raw exporter的MP读取并删除新互异测试；不得改动C25a-b或已存在的PP生产writer。

## 结果与证据

- MP=200、PP=173的test-first job `4ad5958dd753433790d6ac3b29e4e12d`按预期1/3失败，旧raw输出currentMp200。
- `EntityFieldContract`现唯一声明`current_mp -> NTSDEntityRuntime.PP / LF2Health.PP`；Unity raw改读`runtime.PP`，没有双写或删除旧MP字段。
- parity Release build 0 warning/0 error；trace self-test 21/21、raw self-test 5/5、format通过；新contract SHA-256 `1AE87A06DD3F1C8F24A089555BBDC3151F60465E8EA67DC0C56768F54CD7EDF4`。
- Unity fresh compile0；raw/action/C06/C25b joint group `03a01ffbe5244ba2bab386dae359ae7d` 49/49 PASS；NTSD28 broad `91f21da3839541418140493307e65b7f` 401/401 PASS；SelfCheck 07:42:52 PASS。
- 此包不改变runtime行为，无需Play；Editor保持非Play，Scene SHA/mtime不变，post-clear Console0。下一C25c-e runtime carriers。
