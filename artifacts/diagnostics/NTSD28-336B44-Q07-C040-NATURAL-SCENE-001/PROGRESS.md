# Q07/C040 原 Battle Scene 进度（2026-10-03）

状态：`RUNTIME_PENDING`。当前正式规则仍以根 336B44 EXE 和对应 playable live path 为准；C040、Q07 与总目标均未关闭。此包只新增测试 Editor 探针，未改生产输入、DAT、图片或战斗 Scene。

原 Editor 的只读预检 `c040-natural-preflight-20261003-01.json` 为 `CAPTURED`：单一干净 Menu，非 Play/编译/更新，Battle/Menu/两配置磁盘 SHA 在预检前后稳定。首次 Scene 探针 `kakuzu-bee-guy-natural-scene-20261003-01.json` 进入原 Battle Scene、使用 LoganRuntime/mode0/difficulty0/seed0，从全 action0 推进 40 个完整 Driver tick（全局5→45），退出 Play 返回干净 Menu；四件受保护文件在该次运行前后 SHA 稳定。测得 D-024 源规则到画面坐标的单像素比例 X=1.53638409602401、Z=1.57808219178082。**它没有通过正式同输入对照**：首差为相对 tick2，正式奇拉比动作65，Unity 动作110；Unity 三人源 X 到 tick40 不动。

该首差的已定位原因是测试输入转换错误，不是生产映射缺陷。`FrameInputSet` 旧枚举与物理键交叉：`CharacterInputModule.CaptureHeldSimulationButtons()` 把物理攻击采为 `SimulationInputButtons.Jump`；`SimulationFrameInputModule` 再送入 `FuncKeyMask.jump`/旧 `KeyJump`，`NTSD28InputTwoPassModule.FreezeProducerState` 把它投影到正式原生 attack 槽4。首轮探针直接送 `SimulationInputButtons.Attack`，经旧 `KeyAttack` 投影到正式 defend 槽6，所以动作110。现有 `NTSD28-336B44-Q07-NATIVE-THREE-KEY-ORDER-001` 已对冻结映射做原 Editor 1/1 守护，本轮未改生产。探针已改用实际物理攻击位，并为下一轮增加旧字段/正式槽位采样；生成 Editor C# 工程编译0错、260警告。

探针随后加了 `enteredPlay` 标记，避免今后在 Play 前安全拒绝时误报“已退出 Play”或“Battle Scene 改变”；修订后生成 Editor C# 再编译0错、260警告，尚未在原 Editor 刷新。

第二次请求 `kakuzu-bee-guy-natural-scene-20261003-02.json` 在运行前安全拒绝：当时 Menu Scene `isDirty=true`，且磁盘 SHA 已从首次预检的 `FE14D8...` 变为 `1ED380...`，因此没有进入 Battle 或 Play、样本数0。报告中的 `Battle Scene or protected disk file changed` 是失败收尾对未进入 Battle 的附带文字；本次请求内部四件文件前后 SHA 仍一致。当前 Menu 未保存内容及磁盘变化的写入者未确认，不保存、不丢弃、不切 Scene；已询问用户。下一步在 Menu 重新变为单一干净场景且用户确认空闲之后刷新测试脚本，再用新的唯一 runId 运行正式输入三人链，逐 tick 比对源/根16字段并查看 Game View/退出保护。此前两份 JSON 保留，不覆盖。

## 用户保存 Menu 后的同输入复验

用户回复“已保存，直接切换”；MCP `manage_scene/get_active` 复核单一 Menu `isDirty=false`。原 Editor 刷新后，正确键位第三次运行 `kakuzu-bee-guy-natural-scene-20261003-03.json` 在原 Battle Scene 完成 40 个完整 Driver tick（全局5→45），三名角色25/75/97均从action0出发，正式内容根、mode0/difficulty0/seed0。逐 tick 比对正式 `b9-17-k19-x540-g560` 的输入相位、三者动作/源X、Bee hold/双方抓取关系、Bee/Guy HP和5个RNG标量，**40×16=640字段首差0**。tick25 Unity角都action336/catch target1、Bee action130/hold3/catch source0，与正式正停顿分源一致。第三次退出Play回干净Menu，四保护文件在本次前后SHA稳定。这里是离散帧输入+完整Driver证据，不是物理键和Game View像素证据。

第四次 `kakuzu-bee-guy-natural-scene-20261003-04.json` 为D-024另采未取整源规则X/Z，再次完成40tick、退出Play、四SHA稳定。tick1–24三者 `viewX - SourceToViewX(SourceRuleX)` 误差≤0.01画面像素；tick25抓取开始双方X误差同为+3.47111777944485画面像素并持续到tick40，Z误差最大0.232876712328789。源规则X仍逐tick与正式版一致，故该偏差局限于画面位置出口。静态定位到共用`BattleInteractionWriter`：抓取姿态先用物理整数X加未缩放DAT局部center/cpoint偏移及半差，再单独写正式源规则X；原B6测试曾将这种混合空间姿态锁为预期。按用户D-024比例决定，已另建 `NTSD28-336B44-Q07-D024-GRAB-VIEW-PROJECTION-001`，只在共用出口把最终源规则X投影到双方物理X，并把聚焦旧断言改为比例断言。生成C#编译0错；**原Editor聚焦/修复后Scene复验尚未运行**，不能把+3.471偏差报告为已消除。

原 Editor 本地工具刷新随后超时；PID105896仍在、进程层面Responding=true，但TCP命令不返回，Editor.log自05:59:26后未更新。已请用户检查弹窗/卡住情况；未强制关闭/重启、未启动第二项目或保存Scene。

## 端口恢复与修复后第五次复验

后续查明原 Editor 已完成 domain reload；端口6400被 AssetImportWorker15 占用，原 Editor PID105896 的监听移至6401。先前超时不能证明 Editor 卡死。通过原 Editor 完成 `NTSD28_B6_CatchRelation` 25/25 聚焦测试；抓取画面X的共用投影修复属于独立 `NTSD28-336B44-Q07-D024-GRAB-VIEW-PROJECTION-001`，不属于本测试探针的生产改动。

修复后第五次 `kakuzu-bee-guy-natural-scene-20261003-05.json` 再在原 Battle Scene 完成40个完整Driver tick，正式源/根/Unity选定16字段640/640首差0，tick25自然抓取正例保持。双方X投影最大偏差由旧+3.471117779画面像素降为角都0、奇拉比0.528882221像素；第三名凯为0，Z最大0.232876712像素。第五次退出Play返回干净Menu，Battle/Menu/两配置文件本次前后SHA稳定。剩余不足1像素与后继CPoint物理整数锚点取整有关；未进行Game View像素、物理按键及pool borrower验收，因此本包仍为 `RUNTIME_PENDING`，Q07不关闭。详细对照见独立D-024 `ACCEPTANCE.md`。

## 第六次有序关闭补证（覆盖本文件旧 RUNTIME_PENDING）

原Editor新run-06完整40tick `samples` 数组与正式源/根已配对640/640字段的run-05逐值完全相同。新增的测试专用退出见证调用现有生产有序关闭，得到`Completed/RuntimeMapCleared`，World对象、运行槽、池借用、活动池对象和Sprite均为0，pool quiesced、World detached；退出回干净Menu，四保护SHA稳定。此 Scene 载体状态升为`VERIFIED_SCOPED_DISCRETE_SCENE`；实际物理设备整链、Game View像素、其它C040/Q07/Q09/Q12及总目标仍开。完整数值、哈希和边界见[RUN-06-REPORT.md](RUN-06-REPORT.md)。
