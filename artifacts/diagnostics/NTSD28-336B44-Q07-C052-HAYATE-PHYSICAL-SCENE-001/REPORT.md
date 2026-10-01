# Q07/C052 疾风物理键原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_PHYSICAL_INPUT_SCENE / C052_AND_Q07_OPEN`。规则权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live source，内容使用正式非排除 DAT/角色图。没有改生产脚本、DAT/图片、Scene、Prefab、配置或非战斗逻辑。

正式源码的站立 OID73 在物理 Jump tick1～4、Defend+Right+Attack tick8～10 下，tick2 action210、tick8 action160／MP500→300、tick11/29 出生 OID417/211、tick31 两个 OID2 目标 HP420。正式源码两次运行逐文件 SHA 相同；根正式 EXE 同 LFR 的60tick×19项为1140/1140零差，完整初态／全 World 未据此宣称一致。[源／根证据](../NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001/REPORT.md)。

原 Unity Editor 的同一 Battle Scene 已经用生产 `SimulationTickDriver.StepOneTick(FrameInputSet)` 运行36tick。v1 探针误把正式物理键位当作 Unity 旧内部缓冲键位，首差 tick2 源 action210／Unity action65，684字段有308差；[v1 raw](hayate-physical-scene-v1.json)与[比较](comparison-physical-scene-v1.json)保留。生产本地键入口 `CharacterInputModule.CaptureHeldSimulationButtons`已有 Defend→Attack、Attack→Jump、Jump→Defend 交叉映射。v2 探针在直接提交帧包时沿用该映射，36tick中除资源字段外655项相同；其29处差来自把正式 `current_mp` 误对到 Unity 的 `Runtime.MP`。Unity技能成本实际从 `Health.PP`／`Runtime.PP` 扣除。

[v3 raw](hayate-physical-scene-v3.json)同时导出 `Runtime.MP`、`Runtime.PP` 和累计技能 MP 扣费。Unity 在 tick8 `PP500→300`、累计扣费200，tick31 `PP300→420`，与正式 `current_mp` 一致；[v3 独立比较](comparison-physical-scene-v3.json)为 **36tick×19项＝684/684，零首差**。同链 action210/212/160、417/211 出生时点和两目标 HP420 均通过。Play 退出、Scene clean，Menu/Battle/GameConfig/ProjectBattleModeConfig 四文件 SHA 前后相同。v1、v2 原件均保留，v3 请求与结果唯一命名、拒绝覆盖。

这是选定同初态字段和站立物理输入链的限定通过。逐 hit 内部决策、其它初态/人物、完整 World、玩家真实键盘及 Game View/声音不由此证明；父 C052、Q07 和总目标仍开放。后续按 336B44 总表 G1 查未关闭的正式可达首差，不重复本案例或据此跑所有角色。
