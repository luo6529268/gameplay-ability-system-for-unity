# Q09/P-08 正式根 LFR 与原 Unity Battle 同局部初态诊断

状态：`FIRST_RUN_MARK_REACHED / CORRECTED_INPUT_ORIGINAL_EDITOR_COMPILE_PENDING`。P-08、Q09、Q12 及总目标仍开放。本包只新增专用 Editor 诊断及证据，未改生产战斗逻辑、DAT、角色图片、Scene 或配置。

## 权威与配对范围

- 当前根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。已有等 HP LFR 由该**正式根 EXE 本身**以 headless playback 运行，退出码 0，`passed=true`，22 个声明 tick、24 行 trace；根结果 `nativeParityClaim=false`，不输出 bleed command 或 GPU 像素。根 tick0 的 CRT 为 `3374725112` / 3000 calls，等价 seed0；该回放的 22 tick 六项选定字段与旧录制源 132/132 相同，24 行根 trace JSON 逐行相同。原始证据在 [正式根输出](../NTSD28-336B44-Q09-P08-EQUAL-HP-ROOT-LFR-001/) 和 [根字段对照](../NTSD28-336B44-Q09-P08-EQUAL-HP-ROOT-LFR-001/field-comparison.json)。
- 原 Unity Battle Scene 生产 Driver 使用 `LoganRuntime`、Naruto OID2 与 Ita OID9、mode/difficulty 0、同 Z650，设源 X500/540、HP500/30、基础 HP30、MP500、动作0、面向相对、RNG seed0。此为选定**局部初态**，并非完整 World 序列化相等。两边 Ita DAT SHA 同为 `2EC65F77FB74CB1106506E87FCEFB643DEDD022D395E0C4A325E1816EEDA8958`。

## 第一轮原 Battle Scene Play 结果

- [原始结果](ita-equal-hp-336b44-unity-01.json)与[原相机图](ita-equal-hp-336b44-unity-01.png)保留。探针在原 Editor 的干净 Battle Scene 启动，预 Start 设双 roster，tick5 暂停后恢复上述局部初态，逐 tick 提交 22 个完整 `FrameInputSet`，终点全局 tick27。`configuredBeforeStart=true`，退出 Play 后 Scene clean，Battle Scene SHA 前后均为 `D8C01FD32FD41CBDFC55BBE9D642B43B3251898A4E6390F71EFAD865E45AFE7F`。
- 首轮 Play 后 [四保护文件哈希](protected-after-first-play.txt)与 [运行前](protected-before.txt)一致；第二轮尚未运行，故这里不代替第二轮的终态检查。
- 2026-10-04 后续现场更正：首轮结果写完后、独立只读比例核算时发现 Battle Scene 磁盘在 `2026-10-03 22:45:00 UTC` 另被写入，现 SHA-256 为 `6592BED4FB1341842A479C20C4E4D90F56CCE46733C6EF35BC79C029BC164971`，不再是首轮的 `D8C01F...AFE7F`。当前 Git 差异包含 HUD 文字/材质/布局，写入者和此次具体增量未证，保留原状。上条四SHA一致**仅指首轮Play前后即时核对**；修正输入第二轮必须先取得新Scene的 clean 状态与四份新前置SHA，不能复用首轮场景证书。
- 随后通过原 Editor 只读 MCP 确认：非Play、测试未运行，但 `is_compiling=true`，当前 Battle Scene `isDirty=true`。因此不切 Scene、不保存或重启原 Editor，也不尝试第二轮；已请用户在其或其他任务完成编辑后自行保存并确认空闲。此前基于 clean Scene 提出的重启问题已不再具备原前提。
- 再次只读核对发现 Battle Scene 于 `2026-10-03 22:51:49 UTC` 又写为 SHA `223A6C2DA59B2579C3AE25C8BCD98C0147B9D40201F4866E699D5628A3EE4F01`。因此上方 `6592BED4...` 只是中间观测值，当前 Scene 尚不能建立稳定第二轮基线；不追认写入者，也不恢复任何旧版本。
- 2026-10-03 23:19 UTC 再次只读磁盘核对：Battle Scene 于 22:57:56 UTC 写盘，SHA 又变为 `D88AD2111715AB2D970A85DDDAFFAAB206DFFD3BB54B9DF071AD25901D76CDF6`。用户确认此前误启动的全套 EditMode Test Runner“已取消”；本轮 `unity status --json --non-interactive` 返回 `STATUS_NO_INSTANCES`（此 Editor 未装/未暴露 Pipeline），不能据此验证 Test Runner 或当前内存 Scene 已干净。原 Editor 进程仍在，修正探针源码时间晚于当前 `Assembly-CSharp-Editor.dll`；因此继续保护现场，不切换/保存/重启/Play，待取得新的只读内存状态及 clean 前置。
- Unity 相对 tick8 目标 HP30→10，tick22 目标 action0/HP10/baseHP30；正式根相同。相对 tick22 中央快照目标实体索引1、血点索引2，血点恰1条、1×3，原相机图已写入。Unity 与正式根的 22 tick 选定六字段 [对照](first-run-field-comparison.json)为 **130/132 相同**，RNG state/calls 为44/44相同；两处差异均为相对 tick2、3 鸣人 action：正式60，Unity65。
- 原相机图为 1920×1080；记录的标记投影 X约670、从底部 Y约163，PNG 中该 X 的 `(670,917..921)` 为五个纯红像素。此为画面观测，不是同tick消融 A/B 的因果像素证明；1×3逻辑尺寸在项目保留的完整背景取景下会放大到多个输出像素。
- [独立视口比例核算](viewport-ratio-check-first-run.json)：当前 playable 源码 WARP 的同一血点有/无图差为纵向3个输出像素，正式视口高730；原 Unity 相机图在记录的标记投影锚点附近纵向5个纯红像素，视口高1080。若只按输出视口高度比例，`3×1080/730=4.438`，像素取整后5与观测相容。这只排除本样本明显的标记**输出尺寸**比例异常；Unity没有同tick移除标记的配对图，且背景不同，不能据此断言标记像素因果、位置或整画面相等。
- 这两处首先是**测试输入错配**：正式根 trace tick2 的 `inputCurrentMask=16`，事件为 `native standing attack RNG 0x82`；第一轮 Unity 探针错误提交了 Jump mask32。已把探针改为 Attack，生成的 Editor 工程重新编译 0 error/299 warning，见 [构建输出](generated-editor-build-attack-input.txt)。第一轮不能作为生产逻辑首差，也不能作为六字段完整通过。

## 当前验证门

原 Editor 在第二次 `refresh_unity` 后报告 `is_compiling=true`，其 `Assembly-CSharp-Editor.dll` 时间仍早于修正的探针源码，且 `tests.is_running=false`、非 Play、Battle Scene clean；因此**修正输入的第二轮原 Editor Play 尚未运行**。待原 Editor 完成编译后，使用新的 runId 保存第二轮原始结果，按同一六字段逐 tick 比较，并重新核对四保护文件 SHA，最后回到运行前的 clean Menu。此处不得把生成工程编译代替原 Editor 编译或 Play。

即使第二轮选定字段全同，正式根 EXE 自身 GPU Present、完整 World 状态、同视口像素、Legacy 渲染出口仍需独立证据；项目自有背景和固定完整视野属于用户例外。
