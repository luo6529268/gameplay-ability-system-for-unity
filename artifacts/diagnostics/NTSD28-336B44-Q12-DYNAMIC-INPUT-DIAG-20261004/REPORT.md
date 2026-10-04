# Q12 合成键输入链定向诊断（2026-10-04）

> **13:00 UTC 后续有效样本（覆盖下文“合取未运行”快照）：** 原 Editor 的 Battle Scene 在 clean 且磁盘身份稳定后完成了一次显式 Dynamic＋临时失焦路由的合取 Play。原件 [`natural-first253-20261004T130015931-00a48018c64c4eb2871f470df858fa42.json`](natural-first253-20261004T130015931-00a48018c64c4eb2871f470df858fa42.json)，SHA-256 `9ED9F9DB3245BC694A853784231DC7514503A1AC6817B9B5172FED0546ED9E92`。`focusSettingsApplied=true` 且 `focusSettingsRestored=true`；30/30 tick 的 `inputHeld=0`、键盘 L 状态 0、P1 Defend Action 未 pressed，八次有限 L 脉冲未进入战斗包。前后的 Battle Scene SHA `F585EBCC4F170DEC8C8B1C372F5D3BF99E55B74889119102E620C9110F79F910`、Menu/两配置 SHA 及 Scene clean 保持。合取假设已被本诊断环境的这一轮否定；这是合成输入域失败，**不构成生产战斗逻辑首差**。停止同一输入探针，Q12 重进改用不注入按键的生命周期见证；下文是更早的调查快照。

当前结论：`INPUT_ACTION_FIRST_BREAK / COMBINED_FOCUS_TEST_NOT_EXECUTED`。只在原项目原 Unity Editor PID 138072、原 `NTSD_Battle` Scene 使用现有菜单探针；正式 336B44 战斗规则、生产输入、DAT、图片和非战斗代码未改。此次不满足 Q12 同冻结版退出后重进；不能将全 0 输入包解释为正式战斗规则差异。

原 Editor 当前桥接端口 6401；此前使用的 6402 属于同项目 AssetImportWorker，不是该 Editor 的可用控制端口。进入第一轮前，Editor 非 Play/idle、唯一 Battle Scene clean，Battle/Menu/GameConfig/ProjectBattleModeConfig SHA-256 分别为 `876C025831BF0B45611AECDB426E5EF6AF09E84F28603846ED0272E19108A421`、`9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。

已保留五份原始 JSON，文件名含 UTC 时间和唯一 ID。首份显式 Dynamic 更新样本 `...122713148...json` 从 tick732 开始，30/30 行输入包 held/pressed/released 均 0，八次 L 脉冲耗尽；原 Editor 已导入脚本，不能再称“Unity 编译待验”。第二份 `...123408776...json` 在前置检查处因鸣人 frame515/state3 失败，未测试输入。第三份 `...123520539...json` 是有效观测：30/30 行输入包仍全 0，键盘 L 在 16/30 个完整 tick 后为 pressed，但 P1 Defend Action 仍 `Waiting`、`IsPressed=false`，本地输入源 held 全 0。因而首个已观察断点在设备状态到 Action；后续正式 input proxy/战斗 pass 未收到键，不能按此样本判断技能逻辑。

安装的 Input System 1.7.0 `InputManager` 显示，未聚焦时游戏输入无视焦点需要 `BackgroundBehavior.IgnoreFocus` 与 `EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView` 同时成立；单独临时焦点策略旧试验和本轮单独显式 Dynamic 更新分别失败。已在同一 Editor 诊断脚本准备两者合取、保存并恢复设置的有限试验，生成 Editor 工程和原 Editor 导入均 0 错。随后两份 `...124150249...json`、`...124404951...json` 均在 `EditorApplication.isPlaying=false` 前置退出，`focusSettingsApplied=false`，零 tick、零 L 样本；**合取方案没有被运行，更不能宣称成功或失败**。

同窗口 Battle Scene 磁盘 SHA 变为 `4559FC86FEBA0D07B6AAD7B5B1E2BC72E20D65304A5887E91B83C7F0C82D00C7`，当前 Git Scene diff 为另一路 BattleControls 图标/箭头引用及控制组件序列化增加，文件写入时间 `2026-10-04T12:41:53Z`。本诊断没有保存、覆盖或回退 Scene；写入者/意图未证。Editor 状态在定向菜单前后出现 Play/非 Play 与 domain reload 切换，第三轮未获得可比 Play 前置。此时停止运行时重试并保留现场，不操作当前他人 Play；需要原 Editor 暂时独占、唯一 Scene clean 且新磁盘身份稳定后，再做一次合取探针与退出后重进。若合取仍未进入 Action，记录新首断点并停排，不扩成全角色测试。

验证边界：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 最近一轮 exit 0、0 错/332 警告；原 Editor `Assembly-CSharp-Editor.dll` 时间新于诊断脚本。成功的正式技能正例仍只来自此前已保存的同一原场景样本，不能从本轮失败反推其失效。五份原件的 SHA-256 依文件名时间顺序为 `09C47FECA79D7A9855F7F3103A8511DFB8796A83CFC165506D5142A6C81F21D6`、`ED1B58FA7CB639DD30F568D35630DB29E648FD2D1E9ADC8E71E69043C9E028A7`、`843EC1642CC7862F2096D48A02450CD5185086385A2BDF209A4055571D8AF50A`、`67015A807D7276FB6BAB0551AC20CE12F768E53AC53331C8B0F9226AE0F21474`、`5BF0F7985448B25D07EB6AD2427322A0D634602875E9DC7CA4EC8755386BB5EB`。

安全停点后的只读复核又见 Battle Scene 磁盘 SHA 变为 `F585EBCC4F170DEC8C8B1C372F5D3BF99E55B74889119102E620C9110F79F910`，原 Editor 再次处于 Play/transition。它是随后发生的外部变化，以上 `4559FC86...2D00C7` 仅为本轮较早快照；本任务未介入这次 Play 或保存。故独占与稳定身份前置仍未满足。
