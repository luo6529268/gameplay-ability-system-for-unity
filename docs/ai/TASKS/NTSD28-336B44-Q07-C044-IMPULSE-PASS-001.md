# NTSD28-336B44-Q07-C044-IMPULSE-PASS-001

状态：`VERIFIED_SCOPED_UNITY / ROOT_NATURAL_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C044。唯一规则权威为336B44正式 EXE 对应 playable Core `BattleWorld28::advance_catch_relations()` 与 `finalize_horizontal_hit_impulses()`；正式 DAT OID16 action128 与 OID2 action130 的低timeout2/3/4受控逐pass两次同SHA见[源报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001/REPORT.md)。自然抓取负帧只到timeout35，不把受控源证据当成根或Scene同态。

Unity 当前 `BattleCpointWriter.RunKind1` 在负decrease跨零时立即写受害者 `Runtime.Vx/Vy`、将双方 `AttackingCounter` 置1，却不写双方 `HitCount`。生产 `FramePostProcessAll` / DataOriented writer 已持有共用pending结算：HitCount>0 时按 Knockback×2/(HitCount+1) 写速度并清 pending。只修改以下声明代码路径：

- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchExactConsumerAndAdvanceOrderProductionEditorTests.cs`：正式跨零逐pass正反，两postprocess profile与朝向，先取得RED。
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`：跨零分支共用写者精确修正。
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`：将现有旧断言改为当前正式源时序，并运行邻近 Play 探针（若可用）。

预期：跨零时动作0/181、关系/负timeout及双方动作计数保留，双方 HitCount=1；只给受害者 KnockbackVx/Vy=±4/-3，不立刻写速度；正式 FramePostProcess 后 HitCount/Knockback清零且受害者速度=±4/-3。等于零仍走投掷，不改 DAT/Scene/用户比例、GAS框架和非战斗。先原Editor测试RED再修并跑同测试、相邻CPOINT/结算测试；Unity编译0错，检查两 profile、保护四SHA、Ledger、diff。若源/Unity适配首差不同，保持本包开放并记录，不扩大到全量案例。回滚仅审阅本包代码差量；保护全部既有脏工作。

进展：原Editor新程序集聚焦19例中4个新增跨零例RED，其余15 PASS；四个首差均是动作计数2被旧实现写为1。随后已将共用writer改为双方HitCount=1、只写受害者Knockback，不即时写Runtime速度；旧Play探针已跟随新源合同更正。修后Editor Refresh/编译/聚焦尚待。初次旧Play runner方法名导致CS1061已在同声明测试文件修正；旧程序集16/16不计证书。

修后原Editor Assembly-CSharp/Editor Tundra build success、0 error；job a4813146a2884a6c894c40382f30f108 抓取类19/19 PASS，新增4例与其余15例均通过；job b6deff00b29c4209aab80d92fc547c6f 相邻帧后处理对照2/2 PASS。Ledger 1075 Records/15 governed code files PASS；四个受保护Scene/Asset SHA保持。原Battle Scene Play探针进行中，正式根自然跨零仍待。

原Battle Scene既有 R8 抓取 Play 探针 PASS：受控跨零即时 HitCount1/冲量4/-3、后处理 HitCount0/速度4/-3，tick1707；对象/slot/池/统计还原且退出Play clean，四保护SHA不变。仅关闭 Unity 受控逐pass出口，正式根自然跨零未证，父 C044/Q07 仍开放。[验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-IMPULSE-PASS-001/ACCEPTANCE.md)。
