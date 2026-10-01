# C031 正式根与原战斗场景受控双条件对照

2026-09-30。权威为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable `PhysicsIntegrator28::step` / `BattleWorld28::step_physics`。本包只做诊断，不改生产战斗代码或 DAT。

| 条件 | 源码 GameSession | 336B44 根 LFR | 原 Unity Battle Scene |
|---|---|---|---|
| 鸣人 OID2/action180/Y0，地板等号 | tick1 action180/state12/Y0 | 32 tick、1056 字段同源、root report PASS | 32 tick、1088 字段同源；tick1 action180/Y0；PASS/DONE |
| 鸣人 OID2/action180/Y+1，真实穿地 | tick1 action230/state14/Y0 | 32 tick、1056 字段同源、root report PASS | 32 tick、1088 字段同源；tick1 action230/Y0；PASS/DONE |

两例均使用正式内容、背景1、mode0、种子682973786、主角 X500/Z400 与目标 X1100/Z400、HP/MP500、两人中立输入，源/根相同 LFR；Unity 原战斗 Scene Play 克隆使用生产完整 `SimulationTickDriver.StepOneTick(..., buildPresentation:true)`。Y+1 是**初始条件夹具**，帧/物理/动作此后均由生产路径自然推进，未手动设置落地结果。Unity 两例各比较64个实体行×14字段和32 tick×6个随机数标量，共1088字段，差异0；源/根各比较64×14+32×5=1056字段，差异0。正式 LFR 的独立 CRT seed 与 EOF tick33 不计入源/根比较。结果文件：`source-root-comparison.json` 在相邻 `NTSD28-336B44-Q07-C031-ROOT-CONTROL-001`、本目录的 `penetration-source-root-comparison.json`、`equality-source-unity-comparison.json`、`penetration-source-unity-comparison.json` 及两个 `state12-*-scene-01.json`。

原 Editor 新诊断脚本刷新后编译成功、0 error；已有 C031 聚焦19/19、相邻17/17已在父 Task 留证，本包没有生产修改，故不重复全量测试。两次 Play 均正常退出、Scene clean。前后四个受保护资产 SHA-256 保持：Battle Scene `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`；Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`；GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`；ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。Editor 结束时 idle、非 Play。

限定结论：C031 的严格地板等号/穿地动作门在这两组受控完整 tick 中已三方同态。尚未用自然物理按键触发同一 state12/18 序列；环境伤害交互、C032 声道6、整场 World 与 Q07 整组都不能由本包宣称完成。父 C031 保留 `RUNTIME_PENDING / NATURAL_TRIGGER_PENDING`，新版总表继续追踪后续可达案例。
