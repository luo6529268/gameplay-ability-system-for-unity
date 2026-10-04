# NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001

状态：`VERIFIED_SCOPED_SCENE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。仅把已证 OID0 Tobi 普通输入→OID251 自然出生的原 Battle Scene 链从 14 tick 延至 40 tick，寻找后续首差；不把它当受控 OID251/action0 双 Uj 或完整 Q07 验收。

权威：当前根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，与对应 playable `GameSession28`、`SimulationTickDriver28`、帧机及 OPoint live path。现有[正式源/根报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)已确认 OID0 X600/Z400、OID2 X1100/Z400、seed682973786、mode0，tick1–4 跳跃、tick8–10 防右攻，40 tick 所选八字段源/根 320/320；源码[CSV](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/run-d/jump_from_zero-ticks.csv)为本包逐 tick 比较依据。原 Unity Scene 已有同前置 14 tick 八字段112/112通过，后26 tick尚未测。

声明代码路径：仅修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053TobiNaturalBattlePlayProbeEditor.cs`，增加独立 40 tick 菜单与独占输出路径、序列化目标 tick 数和外部提前退出留痕；沿现有 OID0/2、正式内容、离散输入、生产 `StepOneTick`、保护哈希与残留检查，不改14 tick菜单/结果。结果新增于 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001/`。不修改生产逻辑、DAT/图片、背景/模式、Scene、Prefab 或非战斗。

验收：原 Editor 导入新程序集和生成 Editor 工程0 error；原项目唯一clean Battle Scene 跑完整40个生产Driver tick；正式CSV与Unity输入映射/相位、Tobi动作/Y、OID251槽/动作/X/Y共八字段×40逐 tick 找首差；退出非Play、World解绑/Pool0及本次 Battle/Menu/GameConfig/ModeAsset SHA。若 Scene并行修改，保留输出并标明保护门未定。真实键盘设备、全World、正式EXE像素、自然action0双Uj和其它Q07条件仍需后继。

风险与回滚：仅Play clone临时对象；脚本结果 `FileMode.CreateNew`，不覆盖14 tick结果。若发现首差，先核验前置和正式/Unity字段映射，生产修复另立Change；撤销本新增代码或输出须按文件操作审计合同记录并取得需要的授权，不动用户/其它任务内容。

执行结果（2026-10-04）：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001/REPORT.md)。原Editor导入新菜单，生成Editor工程0 error；第一次调用因Scene当时dirty安全拒绝，第二次干净前置完成40生产Driver tick，正式源码/Unity八字段320/320同，后26tick无首差。Play内四SHA同；独立EditMode残留Scene clean、同Battle SHA、Driver World0/Pool0。只闭此40tick所选字段子门，真实设备键、全World、自然action0双Uj及Q07/C053/Q12仍开放。
