<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable controlled C053 double type3 hit case
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001.md
-->

# Q07/C053 受控辅助双命中 40 tick 原场景五槽探针

2026-10-04 限定验收：原Editor已运行单一受控40完整tick生产Driver；[Unity原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001/ank610-jira500-firz700-yminus60-world40-01.json)与336B44正式源CSV五槽有效字段1136/1136、RNG标量200/200零首差；源/根同槽先前1136/1136同。tick7双type3/目标HP440同；结果为完成后CreateNew写出。原Editor两轮导入/生成Editor工程编译0错误，第二次299 warning；退出非Play/唯一Scene clean，四SHA稳；独立只读残留报告Driver1/World绑定0/Pool0。实际脚本修改仅声明路径中的新 run/menu、五槽sample、40tick/默认hit plan分支、CreateNew结束写入与残留菜单。`VERIFIED`仅本测试载体与受控Scene子门，真实键、自然第三体、其余C053/Q07/Q12开放。[完整报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001/REPORT.md)。

2026-10-04 二次脚本改动前登记：原Editor已经跑完40tick并退出，正式源/Unity五槽有效字段1136/1136、RNG标量200/200零首差，四保护SHA稳定、Scene clean。为补本Task原定的“World/池残留”出口，仅在同一脚本加不进入Play的只读菜单：加载已完成结果并核对Scene SHA，枚举原Scene序列化Driver的World绑定与Pool组件，向新残留JSON仅CreateNew写一次。先登记后改，真实结果待运行。

脚本修改前登记。当前已有同初态正式源/根 40 tick 五槽 1136/1136 有效字段同态，而原 Unity Battle Scene 仅 AuxGreen 12 tick、261/261 所选字段。仅在既有 Editor-only `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor` 增加独立不覆写结果的 40 tick 菜单模式和只读五槽快照；复用生产 Driver、正式内容与原初态，不改任何生产分支、DAT、图片、Scene、Prefab 或非战斗逻辑。

实际代码已写：在声明的单一脚本中新增 `AuxWorld40RunId`/菜单 `StartAuxWorld40`、`WorldSlotSample`/`CaptureWorldSlot`、新模式40tick上限、五槽采样及tick7受控见证；`Save` 对新模式在完成前只存 SessionState，完成后以 `FileMode.CreateNew` 一次写新路径；`Poll`/`WaitForRoster` 沿 AuxGreen 默认 hit plan 初态。旧请求模式及其12tick行为保持。`git diff --check` 对本脚本通过；生成工程/原Editor编译和真实Play尚待，不能称对齐。

修改符号预计为 run ID/菜单入口、`TickRow` 五槽诊断字段、`MeasureOneTick` 限定计数与快照、`CompleteMeasurement` 的新模式结果、`Save` 的新模式 CreateNew 出口。新结果只在最终 EditMode 完成时写一次；无请求文件变更。验收、风险与回滚见 [Task](../TASKS/NTSD28-336B44-Q07-C053-AUX-WORLD40-SCENE-001.md)。真实编译、Play、CSV 首差、清理和哈希证据待执行，不预报对齐。
