# Task Contract — NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001

> 状态：`VERIFIED / C02-C03-WRITERS-CLOSED / NEXT-PRODUCTION-PLACEMENT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C02-C03`  
> 依赖：`NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001 / VERIFIED`  
> 建立日期：2026-09-04

## 目标

关闭C01后的actual-sequence首差：Authority在C01后立即进入C02升序live-slot
producer/object-`hit_Fa`/AI sample scan，再以C03第二次升序scan执行proxy copy与sampled route；Unity当前先运行
`Cooldown`、`RuntimeMaintenance`、`HumanInput`，随后才进入`CharacterInput`两遍producer/route。

本包只读追踪双方字段读写和调用链，把Unity `Cooldown`中属于Authority C02前置、C27～C30尾部或旧实现遗留
的职责拆开，并选定最小、可测试的下一实现包。不得在尚未闭合writer/traversal/early-return语义前移动代码。

## 允许范围

- 只读Authority `simulation_tick_driver.cpp`、`battle_world.cpp`、`native_ai.cpp`、input routing及其正式测试/构建闭包。
- 只读Unity TickSystem、World/pass pipeline、Cooldown/runtime-maintenance、human/character input、AI producer与相关测试。
- 仅修改本Task/Record、Ledger、STATE、handoff、总表与B3 pass manifest。

禁止修改任何C#、shader、Scene/Prefab、Config/DAT、ProjectSettings、Packages或authority文件；禁止顺手重排
Cooldown或input pass。

## 验收

1. 冻结Authority C02/C03逐分支顺序、dynamic slot traversal、slot销毁重取、producer/sample barrier与字段writer。
2. 冻结Unity C01后至CharacterInput结束的实际调用链，以及每个子调用读写的逻辑字段。
3. 将Unity Cooldown/maintenance职责逐项归类为C02前置、C27 reaction、C28 armor、C29 rest、C30 combo、
   Unity-only或后续B4/B5/B7 owner，未知项明确标注。
4. 判断能否以最小phase extraction关闭首差；列出测试first、同步/worker、partial-return、allocation和Play验收。
5. 更新所有恢复文档并运行Change Ledger validator；不产生production行为变化。

## 回滚

仅删除或更正本包新增的审计结论和恢复入口；没有脚本或运行时变更需要回滚。

## 暂停原因

开始读取C02/C03时，指定根EXE实测为`B1E13AE1...9033`而非锁定的`1277B70B...DAF75`；source
manifest也已变化，且新`simulation_tick_driver.cpp`出现会改变既有B3 pass表的顺序变化。本包已停止使用
该source作裁决，未修改任何脚本。先执行
`GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001`并等待用户选择恢复或晋升。

## 恢复

用户已确认当前artifact是其Bug修复后的正式版本；`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`已解除阻塞。
本审计改用EXE `B1E13AE1...9033`、82-file playable closure `39DDDA15...6109`及75-file capture子闭包
`07CD47A0...778F`重新建立C02/C03及全core顺序；
旧`CoreProducerSampleScan / Cooldown`首差只有在新版顺序复核后才可继续使用。
