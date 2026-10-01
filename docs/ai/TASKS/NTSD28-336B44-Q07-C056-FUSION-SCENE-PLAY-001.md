# NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001

状态：`RUNTIME_PENDING`（原Scene受控两条件 SCOPED_PASS）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，当前总表 G1/BATCH-04/Q07/C056。继承 C056 共用合体计数修复的 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。c7-scene-01 无效前置首差留原件；修正后 c7/c0-scene-02 各3tick动作/计数/停顿/结构出生同源码，退出池0、Scene clean。正式根与本Scene关闭阶段仍待。[结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/REPORT.md)。

前置证据：336B44 对应 playable 源码使用正式融合记录7/8→51，OID51/action290带 kind2 OPoint→OID213。完整 GameSession 同初态受控计数7/停顿3，tick1～3 出生0/0/0；计数0/停顿3，tick1～3 出生1/1/0，双跑同SHA。Unity原 Editor 聚焦与相邻测试六项通过；正式根EXE与原Scene仍待。正式 decoded DAT OID7/8/51/213 在项目 `LoganRuntime/decoded_dat` 中逐份SHA一致。

本包只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C056FusionHoldBattlePlayProbeEditor.cs` 与 `.meta`，采用原 Battle Scene、现有 BattleTestBootstrap/SimulationTickDriver 与请求式Play；不改生产、DAT、Scene、Prefab、项目设置、非战斗逻辑及已批准的固定背景例外。明确两个独立请求：计数7/停顿3，计数0/停顿3。Play 启动前限定原项目/单 Battle Scene/clean/非Play；进入后将正式OID7/8主角设为同组、action9/HP100、X304/300、Z600，暂停后以三个完整 Driver tick 输入中性。每tick记录合体OID/action/计数/停顿、结构性 SpawnCount 差分、生成OID及池借用，退出并核对原Scene哈希和clean。不得用外部临时副本或 computer-use。

验收：源规则坐标经既有双域投影，定向三tick两条件与源码同态；对额外场景生物/结构出生必须记录而非掩盖。原 Editor 编译0错、两组Play退出、Scene/Menu/两配置哈希不变、有序关闭零残留，再据证据推进状态。根EXE同条件仍单列，不以源码或Scene代替。任一前置失败留原件，先查首差，不修改DAT来追绿。回滚仅本包新探针，保护全部既有脏文件；删除探针需遵守仓库批准规则。
