# NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001

状态：`VERIFIED`，仅关闭模式 0、玩家关系组 5 的原 Unity Game View 姓名牌显示子门。Q09 和总目标仍开放。

## 权威与方法

- 正式根 EXE SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable `render_snapshot.cpp` 为组 5 姓名牌选择 `WORDS5`；模式 0 槽 0 无剧情组 5 抑制条件。
- 正式 `resources/runtime/vfs/sprite/UI/WORDS5.png` 与 Unity `LoganRuntime/vfs/sprite/UI/WORDS5.png` 的 SHA-256 均为 `6F704853CC86D70349C724411E74C89B93EF6BCD8DD637BA321FA1B4ECC73F90`。
- 原 Editor，正式暂存内容，保存的 Menu → Additive Battle；P1 组 5，采集同一逻辑 tick 的中央命令、绑定与 1920×1080 Game View。仅扩既有 Editor 探针，没有改生产、DAT、PNG、Scene、菜单或结果页。

## 实测结果

- [原始报告](natural-nameplate-20261003-210922-784-a9687403f48a49d0a69faf3d05d6df01.json) 为 `PASS`：`requestedBattleGroup=actorBattleGroup=5`，`captureTick=publishedTick=planTick=tickAfterScreenCapture=7`，`selectedWordSheet=5`，`selectedLabelChar=1`，`hasSelectedWordBinding=true`，对应 `OverlayGlyph` 命令 1 条，本体及姓名牌命令均存在。
- [原 Game View 截图](natural-nameplate-20261003-210922-784-a9687403f48a49d0a69faf3d05d6df01.png) 1920×1080。报告屏幕锚点 `(580.55, 320.625)` 采用左下原点；换成 PNG 左上原点约 `(581,759)`。该点 21×21 区域有 77 个 `(229,4,199)` 纯色像素；旁边背景对照区域 0 个，P2 红色字形区域也 0 个。正式 `WORDS5` 图集主体色为 `(230,4,201,255)`；这是截图中实际组 5 字形可见的像素证据，不仅是命令存在。
- 生成的 `Assembly-CSharp-Editor.csproj` 编译退出码 0、0 error（[构建输出](generated-editor-build.txt)），原 Editor 刷新后新程序集晚于探针脚本、运行前 Console 0 error。
- 运行后原 Editor `is_playing=false`、`tests.is_running=false`、`activity.phase=idle`，活动 Menu `isDirty=false`。Battle/Menu/GameConfig/ProjectBattleModeConfig 四个磁盘 SHA 与运行前逐项相同：`D8C01FD3...5AFE7F`、`9EAAA0B4...C1BA`、`0527D737...CB8EA7`、`B57CFEF3...85B82`。项目内其他人已存在的 Scene/HUD 修改原样保留。
- 运行后的 Console 错误过滤查询有一条 `MCP-FOR-UNITY: Client handler error: Cannot access a disposed object.`；未定位其产生时点，故不把运行后 Console 报为 0 error，也不将其解释为战斗脚本编译或画面失败。

## 限定与后续

这证明原 Unity Editor 的模式 0、组 5 姓名牌资源选择、中央发布和实际 Game View 可见像素。未证正式根 EXE 的实际 GPU Present 像素、默认剧情 stage 的组 5 自然入口、真人物理按键或 Q09 全部表现。默认 stage.dat 部署继续暂缓。

此前误启动的全套 EditMode 作业已由用户在原 Editor 取消；`tests.is_running=false`。该作业没有完整结果，不能计入本包或全套回归。组 5 共用选择器前包的精确 3/3 聚焦测试仍为独立证据。
