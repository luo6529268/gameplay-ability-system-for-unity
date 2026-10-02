# D-024 共用抓取画面 X 投影限定验收（2026-10-03）

状态：`FOCUSED_TEST_PASS / SCENE_LOGIC_AND_POSITION_PASS / GAME_VIEW_PIXEL_PENDING`；Change 保持 `RUNTIME_PENDING`，C040、Q07、D-024 与总目标仍开放。当前正式规则为根 336B44 EXE及对应 playable live path，正式 DAT 数值未修改。

原 Battle Scene 三人普通初态正例 `b9-17-k19-x540-g560` 在修复前 run-04 完成40个生产Driver tick，正式源/根/Unity选定16字段640/640首差0；tick25自然护甲命中与kind3抓取发生时，双方画面X相对 `SpatialProjection.SourceToViewX(SourceRuleX)` 同时多3.47111777944485像素并持续至tick40。源规则X仍与正式版一致，因而差异在画面位置出口。

`BattleInteractionWriter` 的共用抓取关系姿态先用原始DAT局部偏移计算已缩放的物理X，随后单独正确计算规则X。本包只将这个共用写者的最终规则X经现有 `BattleSpatialProjection` 统一投到双方物理X并更新整数镜像；没有按角色、技能或DAT做特判，未改DAT、图片、Scene、相机或非战斗脚本。焦点测试把旧的混合空间预期改为identity和2048×1152比例投影预期，并保留缺失源载体的兼容路径。

验证：生成`Assembly-CSharp-Editor.csproj`编译0错/291警告。原Editor具名类别`NTSD28_B6_CatchRelation` 25/25 PASS，其中identity/比例两例均PASS，原始测试job为`43a2003c15bd4f30a05b7627da6cb15f`，完整返回保存在`original-editor-focused-20261003.json`。聚焦RED因Unity刷新期间连接到资源导入worker占用的旧6400端口而未取得；定位原Editor PID105896实际监听6401后完成GREEN，未启动第二项目或强制重启。

修复后原Battle Scene run-05仍从全action0、mode0/difficulty0/seed0经正式内容根推进40完整Driver tick；输入相位、三者动作与源X、Bee停顿/抓取关系、Bee/Guy HP、5个RNG字段共640项与正式源码零差（正式源与根先前640/640零差）。tick25 Bee action130/hold3、双向关系保持。三角色在40tick的画面投影最大X误差分别为slot0角都0、slot1奇拉比0.528882220555261、slot2凯0像素；奇拉比Z最大0.232876712328789像素。X旧3.471偏差已消除，剩余不足1像素来自后继CPoint对物理整数锚点的取整，不由本包扩大到另一写者。Play退出并返回初始干净Menu，Battle/Menu/两配置文件在该次运行前后SHA稳定；原始证据为`../NTSD28-336B44-Q07-C040-NATURAL-SCENE-001/kakuzu-bee-guy-natural-scene-20261003-05.json`。

未覆盖：未抓取Game View真实像素或物理键输入，也未在本探针中记录pool borrower/完整有序关闭诊断。后继CPoint整数取整与其它角色、武器、道具的D-024比例出口仍分别验，不把本包结果写成Q07或整场景完成。
