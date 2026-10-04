# NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001

状态：`ROLLED_BACK / FOCUS_HYPOTHESIS_REFUTED`。父项 [Q12限定矩阵](NTSD28-336B44-Q12-REPRESENTATIVE-MATRIX-001.md)。这是原Battle Scene合成键盘诊断入口的可靠性修正，不改变336B44正式战斗规则、生产输入或任何DAT/图片/音频值。

触发与原状：[Q12重进首轮与复验](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-ATTEMPT-20261004/REPORT.md)均把合成L事件排队，但30个完整tick的 `FrameInputSet` 全0、正式proxy全0，未进入组合技；先前同脚本首行已入包并通过。当前 Editor 只读状态 `is_focused=false`。现有其它物理输入探针会临时设 `InputSettings.BackgroundBehavior.IgnoreFocus`、`AllDeviceInputAlwaysGoesToGameView` 并恢复；本探针未做。这是可疑的测试前置差，不能先写成生产输入缺陷或已证因果。

唯一代码路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs` 的 `NaturalProbe.Begin/Queue/Complete`。在探针开始时保存 Input System 的背景/Editor Play输入策略和 `Application.runInBackground`，只在探针运行期间使用与仓库已有探针相同的失焦策略，并在通过、失败、Play退出、程序集重载和异常清理路径恢复；记录是否恢复。合成键要由它实际绑定的键盘设备排队，必要时调用 `Keyboard.MakeCurrent()`，但不得改生产 `CharacterInputModule`、`InputModule` 或场景按键绑定。

副作用边界：设置仅作用于当前 Editor Play 探针期间；不得保存 ProjectSettings/Scene/Prefab，不改真实玩家默认焦点策略，不运行全套测试，不因本探针失败扩大到角色专用规则。若外部同时改同一设置，不能静默覆盖，须保留错误并停跑。改动前后保留失败原件；成功只证明合成键诊断可重复，不能代替真人手按。

验收：生成 Editor 工程编译0错，原 Editor脚本导入0错；唯一原Battle Scene干净前置，合成L首个完整包 held/pressed非0、最终鸣人首253后J转301；完成后当前 `InputSystem.settings` 与进入前相同，Play退出/Scene clean，四保护文件SHA不变。之后同一保存身份再入一次Play、重复有限正例并核残留，才能关闭Q12重进子门。任一前置不满足即停，不反复盲跑。

回滚：仅精确逆向本Change在该Editor诊断脚本的新字段与设置保护代码；保留历史结果、其它并行脚本与用户Scene，不使用批量 `git restore`。
