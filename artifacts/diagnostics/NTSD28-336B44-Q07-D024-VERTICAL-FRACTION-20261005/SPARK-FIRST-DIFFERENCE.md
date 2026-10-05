# Q07/D-024 自然 kind0 火花纵向首差（2026-10-05）

状态：`PAIRED_PRESENTATION_FIRST_DIFFERENCE / PRODUCTION_FIX_PENDING`。本次只复用 2026-10-04 已保存的同一 C040 自然命中第25相对 tick（Unity 全局 tick30），没有启动新 Play、修改 DAT 或生产火花脚本。正式规则权威仍是根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable 构建闭包；下表正式画面锚点来自 playable `GameSession28` 快照与 D3D11 离屏，不是根 EXE 实际 Present 像素。

同一 mode0/seed0、角都25/奇拉比75/凯97、普通初态及输入链此前已有正式源/根 LFR 与原 Battle Scene 40 tick 规则样本配对。[正式源第25 tick 两条火花](../NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/first-positive-20261004-02/tick25-sparks.csv)中，可绘制 ID0 由 host slot2 持有，源屏幕 Y=352、host 深度 Z=400，即火花位于其地面锚点上方48个源像素。[Unity 原 Scene JSON](../NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q01-spark-natural-20261004-01.json)同 tick 的 slot2 OID97 源 Z=400、视图 Z=631.2328767、整数 Z=631；唯一 pic0 火花命令世界 Y=-7.88000059。既有[Game View 报告](../NTSD28-336B44-Q01-SPARK-NATURAL-GAMEVIEW-001/REPORT.md)记录该帧 ScenesCamera Y=-4.8；`NTSDRenderSpace` 当前生产映射的逻辑视口高550、100像素/单位且无 `SetPresentationCameraOffset` 调用，因此视口顶部世界 Y=-2.05，该命令逆变换为逻辑视图 Y≈583.000059。它相对 host 的整数地面 Z=631 为约 -47.999941 视图像素，仍按原始48像素显示。

| 同一火花相对 host 地面 | 正式 | Unity 当前 | 固定完整视口的比例目标 |
| --- | ---: | ---: | ---: |
| 向上距离 | 48 / 730 = 6.57534% 画面高 | 47.99994 / 1152 = 4.16666% 画面高 | 48×1152/730 = 75.74795 视图像素 |

Unity 当前画面距离只有正式画面占比的约 `0.63368`，少约 `27.748` 视图像素；若把正式源锚点352经统一高度比例从原点投影，目标视图逻辑 Y≈555.485，现值583.000，差约27.515像素。两种数值因当前 host Z=400 投到视图后先截为整数631而相差约0.233，均说明同一全局高度距离未走 D-024 出口。[逐字段计算及两输入文件 SHA](spark-paired-first-difference-20261005.json)可复算。角色图片1.5倍尺寸不参与这项锚点差异。

当前正式 `battle_world.cpp::append_confirmed_native_spark` 在源域先合成 `target.position.z + hitY + CRT jitter`，`render_snapshot.cpp` 将该源事件 Y 直接发布。Unity 活跃 `BattleNativeHitSparkWriter.Append` 先读已投影的 `victim.ZInt`，再加未投影源 `hitY+jitter`；`BattleEcsHitExecutionPlan.ProjectKind0HitRecord` 预测同一混合值，中央发布直接消费 `HitRecordZ`。这是一处真实可达的混合坐标首差。修复必须在事件生成时保留正式源域的接触几何与两次 CRT 顺序，再由共用 `BattleSpatialProjection` 将整个源事件锚点一次投到视图；不能对现存混合 `AnchorZ` 整体乘倍率，否则目标 Z 会二次放大。还须核 legacy `LF2CharacterDatHitResolver.SpawnSpark` 是否是当前正式生产命中的可达 writer，并保证存活多 tick 的 record 不依赖事后移动的 target。

证据边界：正式根实际 GPU Present 与 Unity 游戏截图的逐像素 A/B 尚未取得；旧 Unity JSON 没有独立保存视口 snapshot，逻辑视图 Y 是由当时 Game View 报告的相机、现有映射代码和命令世界坐标逆算。固定相机与项目地图是用户批准例外，因此只比较相对同一 host 地面锚点的画面高度占比。生产改动前先建独立 Task/Change Record，并用一例正式源事件 + Unity 原 Battle Scene 火花命令作 RED/GREEN，既有命中/CRT/年龄回收仅做相关邻例，不扩全角色矩阵。
