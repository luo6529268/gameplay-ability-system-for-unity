# NTSD28-336B44-Q12-DYNAMIC-INPUT-PROBE-001

状态：`ROLLED_BACK / DIAGNOSTIC_HYPOTHESIS_FAILED`。父项[Q12限定矩阵](NTSD28-336B44-Q12-REPRESENTATIVE-MATRIX-001.md)，当前正式战斗规则权威336B44不变。本包只修原Battle Scene合成键盘**诊断脚本**在Editor未聚焦时选错Input System更新类型的可比性，不改生产输入或角色规则。

证据与原状：连续三份Q12重进探针原件的合成L已排队，但30tick `FrameInputSet` 全0。`Library/PackageCache/com.unity.inputsystem@1.7.0/InputSystem/InputManager.cs::defaultUpdateType` 在Unity Editor且`gameHasFocus=false`时返回`InputUpdateType.Editor`；该探针 `NaturalProbe.Queue` 使用无参`InputSystem.Update()`，故事件可能在非Play的Editor输入更新域被消费。前轮临时`IgnoreFocus`处理仍全0并已撤回，证实不能把失焦策略本身当成修复。相邻现有探针通过反射调用当前包的内部`InputSystem.Update(InputUpdateType.Dynamic)`以避免这一歧义。此因果尚待一次原Editor可观察验证，不宣称当前生产输入坏了。

唯一脚本路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs` 的`NaturalProbe.Queue/Complete`；添加一个缓存的内部`Update(InputUpdateType)`反射入口，开始前检查存在，所有本探针合成事件与释放均显式在`Dynamic`更新中处理。保持原键盘选择、角色初始化、逻辑步进、正式proxy和结果格式不变；不给生产代码加特判。反射入口缺失则有限失败，不回落到无参Editor更新。

验收：生成Editor工程0错、原Editor实际导入；在用户当前保存的唯一Battle Scene clean/非Play/无测试前置后，合成L首个完整包held/pressed非0，防→前→跳→首253→J转301，再同一保存身份退出后重进一次并对比结果；四保护SHA和Scene clean，所有失败原件保留。若Input包仍全0，停在Action回调/缓冲断点，不连续盲跑。

回滚：仅精确逆向本Change对单个Editor诊断脚本的typed update调用，保留已有探针与所有原件；不使用批量`git restore`，不删除资源或更改Scene。

2026-10-04 原 Editor 定向运行更正：已在保存且 clean 的原 Battle Scene 用显式 Dynamic 更新运行一次，仍是 30 tick 输入包全 0、八次 L 脉冲失败；原件见 `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-first253-20261004T122713148-700a6ff9a0624b469e89161492108ede.json`。Play 前后 Battle/Menu/两配置四 SHA 稳定、Scene clean。单独更换更新类型不能恢复可比输入，不能声称 Q12 重进或生产规则回归。为落实本 Task 原定“设备事件→Action→缓冲首断点”出口，下一次只在**同一诊断脚本**记录合成事件更新后的键盘 L 状态、P1 Defend Action 状态与本地输入源 held 值，并记录完整 tick 后的同组值；不增加输入脉冲、不改事件顺序、生产脚本、场景或数据。若更新后键盘仍为 0，则在设备层停点；若键盘为 1 而 Action/本地源为 0，则在 Action/绑定层停点。此项先记入任务与 Change，再改脚本；只运行一轮，不盲跑重进。

该诊断的有效首253样本显示：合成L更新后键盘L为0，完整tick后键盘L为1，但P1 Defend Action仍`Waiting`、本地源和输入包均0；可见设备状态到Action的失焦路由边界。安装的InputSystem1.7.0 `InputManager.gameShouldGetInputRegardlessOfFocus` 仅在 `IgnoreFocus` 与 `AllDeviceInputAlwaysGoesToGameView` 同时成立时为真；该包`OnUpdate`在未聚焦状态会提前跳过不匹配的更新域。此前单独的焦点策略试验使用无参Update失败，本包单独的显式Dynamic也失败，因此下一轮只验证两者**合取**：在诊断期间临时保存并设置两项InputSystem策略及`Application.runInBackground=true`，结束/异常时按原值恢复并把恢复状态写入结果。它只改变诊断环境，不保存ProjectSettings，不改生产输入或场景；若仍不入Action，停止尝试，保留失败样本并标Q12运行时待确认。

合取诊断代码已编译并由原Editor导入，但两次菜单都在`EditorApplication.isPlaying=false`前置失败，`focusSettingsApplied=false`；同时Battle Scene磁盘身份出现另一路BattleControls变动。零tick原件不裁决合取输入方案，Q12重进继续待验。当前安全停点与五份原件见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DYNAMIC-INPUT-DIAG-20261004/REPORT.md)。待原Editor暂停其它场景/Play操作、Scene clean且身份稳定，只允许一次合取探针；若成功入包才继续同冻结版重进。
