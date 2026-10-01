# C056 合体停帧计数与 OPoint 时点（2026-10-01）

范围：当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable 源码及正式 `resources/runtime` 的融合 DAT/角色 DAT。源码诊断为当前 28 Core + `game_session.cpp`、`selection_flow.cpp`、`scenario28.cpp` 重新链接；它不是根 EXE 自身的观测。没有修改正式源码、DAT、Scene 或非战斗功能。

正式记录 7/8→51、主角 action9/HP100/X304、伙伴 X300、同组，从 `GameSession28` 开始完整 tick。只在初始化后给主角设置受控动作计数与停顿，之后三个 tick 均无输入：

| 初始计数/停顿 | tick1 合体后 | OPoint 出生数 tick1/2/3 |
|---|---|---|
| 7 / 3 | OID51/action290，计数7，停顿2，旧动作 latch9 保留 | 0/0/0 |
| 7 / 0 | OID51/action290，计数1，停顿0 | 0/0/0 |
| 0 / 3 | OID51/action290，计数0，停顿2 | 1/1/0 |

两次 `source-a.csv`、`source-b.csv` 字节同 SHA-256 `BEAB10C63892B484743499A75F3BD89EAE69D038915158890B77E68D30FD51B2`。首轮误用 C++20 导致当前源码的 `path.u8string()`/`char8_t` 编译错误，日志保留；按正式可构建的 C++17 重编译 28+3 文件 exit0、0 诊断。正式 OID51 `c/saso/saso.dat` frame290 的 kind2 OPoint 是内容正例；计数非零且停顿未结束时不应因合体动作切换提前生成。

Unity 静态首差：`BattleOid5152RuntimeModule.TryMerge` 原调用 `SetAction`，把 `AttackingCounter` 置0且覆盖 `Prev2`/`PrevFrame2`；聚焦测试已加入对应非零计数、旧动作快照断言。生产合体入口现改用已有 `DirectWriteNativeRawFramePreserveWaitCounter`，保留计数与旧锁存，并保留声音锁存复位。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`：exit0、264 warning、0 error。测试先写入，但原 Editor DLL 尚未导入新脚本，**未取得实际 RED/GREEN 运行结果**，不可将静态预期冒充测试通过。

原项目 Editor PID11944 存在，但 `unity status` 返回 `STATUS_NO_INSTANCES`，因为本项目没有 Pipeline 连接；未启动第二 Editor。正式根 EXE 同条件入口、原 Editor 聚焦与原 Battle Scene 完整 tick、子体池/有序退出仍待。C056=`COMPILE_PASS / RUNTIME_PENDING`，Q07和总目标开放。保护的 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig SHA 仍为 `3A089236…235ED`、`DD6A48A…1DC3`、`0527D737…8EA7`、`B57CFEF3…85B82`。

后续原 Editor 已通过本项目 PID11944 的现有 MCPForUnity 本地桥接完成刷新与域重载，新增 C056 聚焦 EditMode 任务 `2f7c76037a5e494b95a8b91e4911929e`：1/1 Passed，0 Failed，验证非零计数与旧锁存/快照。相邻两项任务 `ffb0df8ef11243568447e2bbdd1040b5` 共2项：C054小数拆分通过；旧 `RecordMergeAndControlledDefuseMatchSource(0)` 失败，差异恰是旧 source4 JSON 写的 `actionLatch=290`、`tickActionSnapshot=290`、`frameCounter=0`，而336B44实际应保留合体前的13、20、7。旧 JSON 原件未改，测试现仅对新版已确认的合体保留三字段做版本化投影；重新编译生成Editor工程0错。原 Editor 第二次刷新进行中，投影后的相邻测试尚未重跑。先前“原Editor未导入”的描述只属于首次测试前快照。

第二次原 Editor 刷新/域重载后，`Assembly-CSharp-Editor.dll` 更新至 2026-10-01 03:20:42 UTC。回归任务 `a2e01c94bd164b679089fcfe7d20929e`：旧融合行0加C054拆分 2/2 Passed；任务 `49aca2db1e9e4eaba14d9e6abe7866d1`：旧融合行1～3 3/3 Passed。因此新C056聚焦1/1、相邻旧融合四行4/4、C054拆分1/1实际通过。前述旧夹具失败保留为版本错位证据；投影只修改三字段预期，不修改 JSON/正式数据。再次核对原Battle/Menu Scene与GameConfig/ProjectBattleModeConfig四SHA稳定，原Editor空闲、非Play。C056现为 `FOCUSED_TEST_PASS / RUNTIME_PENDING`：尚无原Scene完整Driver的OPoint/池借用及正式根EXE同条件结果。

后继 [原Scene报告](../NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/REPORT.md) 已给出c7/c0两独立Play各3完整tick：正式源码与Unity的OID51/action290、计数7/7/1及0/0/1、停顿2/1/0、结构出生0/0/0及1/1/0同态；c0 OID213实际出生，退出池归零、Scene clean/四哈希稳。前述“原Scene尚未验”只属于当时快照。正式根EXE同受控入口、源活体数及本Scene十一阶段关闭轨迹仍待，C056/Q07保持开放。
