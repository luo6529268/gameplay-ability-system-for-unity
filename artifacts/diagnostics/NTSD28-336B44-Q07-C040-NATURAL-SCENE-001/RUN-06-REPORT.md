# Q07/C040 原 Battle Scene 自然三人链有序关闭补证

状态：`VERIFIED_SCOPED_DISCRETE_SCENE`。当前正式根 EXE SHA-256 复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本包沿用 [run-05](kakuzu-bee-guy-natural-scene-20261003-05.json) 已配对的角都 OID25、奇拉比 OID75、凯 OID97 全 action0、mode0/difficulty0/seed0、40 个离散输入完整 Driver tick；run-05 曾对正式源/根选定16字段每tick共640/640无首差，并在tick25自然出现护甲命中加kind3抓取。

原 Editor 本次 [run-06 JSON](kakuzu-bee-guy-natural-scene-20261003-06.json) 与 run-05 的整个 `samples` 数组逐值完全相同（各40 tick，含三人动作/关系/源和画面坐标及RNG），不是只比末态。run-05 JSON SHA-256 `80DBE10F396881DC5FEA3B72B95560851E87EB8F3D1B64D9533F5863EFF2C92A`；run-06 JSON SHA-256 `A97C8C001BDB98D7A8D30BD1AA6F2574AB3C720E8F1235CAA276E4F57B6A7BB2`。因此 run-05 对正式源/根的选定字段配对可按完全相同的 run-06 样本继承，但不把它扩展成全World等价。

run-06 在采样后调用生产已有的有序关闭接口，记录 `Completed / RuntimeMapCleared`；World对象、运行槽、池借用、活动池对象和活动Sprite全为0，`poolQuiesced=true`、`worldDetached=true`、`orderedShutdownComplete=true`。原 Editor 已返回非Play、单一干净 Menu Scene；Battle/Menu/GameConfig/ProjectBattleModeConfig 四件保护文件前后SHA相同。生成 Editor 工程编译0错误/270警告，原 Editor完成脚本刷新与真实Play；当前更改只在原 C040 Editor 测试探针增加退出见证，未改战斗生产、DAT、图片、Scene、Menu或非战斗代码。

这关闭的是 **C040 该普通初态离散输入链的原 Battle Scene 规则/比例与有序关闭子出口**。物理设备按键完成整条三人链、Game View 像素、C040其他可达分支、Q07/Q09/Q12及总目标均仍开放。没有因为同一输入链已配对而重复跑其它角色或全量场景。
