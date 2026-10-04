<!-- CHANGE-RECORD
id: NTSD28-336B44-Q12-DYNAMIC-INPUT-PROBE-001
status: ROLLED_BACK
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: user Q12 bounded original-Scene reentry; local installed Unity InputSystem 1.7.0 InputManager defaultUpdateType contract
evidence: docs/ai/TASKS/NTSD28-336B44-Q12-DYNAMIC-INPUT-PROBE-001.md
-->

# Q12合成键探针显式Dynamic输入更新

2026-10-04 最终更正：原 Editor 在 clean 且磁盘身份稳定的 Battle Scene 有效运行了 Dynamic＋临时失焦路由合取；设置应用与恢复均为 true，30/30 tick 合成 L 未入输入包，P1 Action 无 pressed。原件及 SHA 在[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DYNAMIC-INPUT-DIAG-20261004/REPORT.md)。这否定本诊断修正的有效性，并非生产战斗规则差异。本 Change 添加的 typed update、输入状态快照及临时焦点设置已精确逆向撤回；`git diff --exit-code` 对唯一脚本返回 0，未回退他人内容、Scene 或 DAT。状态 `ROLLED_BACK`；不再作为 Q12 ONE 或后续 Play 前置。Q12 转至独立的生命周期见证。

脚本改前登记。原状、当前安装包调用链、精确代码路径、边界、验收及回滚见[Task](../TASKS/NTSD28-336B44-Q12-DYNAMIC-INPUT-PROBE-001.md)。相邻失焦策略假设已在上一Change试验失败且撤回；新首差目前属于诊断输入域，不是已证生产战斗规则缺陷。

2026-10-04 已写单个Editor诊断脚本`NTSD28UserRasenganPhysicalPlayProbeEditor.NaturalProbe`：缓存并检查当前InputSystem1.7.0内部`Update(InputUpdateType)`入口；合成键排队与最终释放只调用显式`Dynamic`更新，不再以无参Update让未聚焦Editor选择`Editor`域。其它旧探针分支未动，生产`CharacterInputModule`/provider/DAT/Scene未动。当前`CODE_WRITTEN`，生成编译/原Editor导入及真实Play因果待验。

生成`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、0错误/332警告；原Editor是否已导入与真实Play仍待。查询原Editor桥接时连接在读帧前EOF，可能处于重载，不能据此称编译失败或通过。状态`COMPILE_PASS`仅覆盖生成工程。

2026-10-04 原 Editor PID138072 实际桥接端口6401（旧6402当时属 AssetImportWorker），只读状态证实非 Play/idle、Battle Scene clean，脚本源早于原 Editor DLL；在原 Battle Scene 单次 Play 中完成 bootstrap 后运行首253菜单，明确读到新结果的 `queued-for-Dynamic-InputSystem-update`。输出为失败：30 tick `FrameInputSet` held/pressed/released 全0、八次L脉冲耗尽，Q12重进未验。停止 Play 后四保护 SHA 与前一致且 Scene clean；正式规则未见 first difference。此结果证伪“显式Dynamic更新**单独**足以恢复合成输入”的假设，不能推广为Input System包或生产代码错误。下一项同文件诊断快照范围已先补入Task；预期只读设备/Action/本地源值，不更改输入逻辑和生产所有权。验证仍为单轮原Scene聚焦测试、生成Editor编译及四SHA/clean门；若异常则保留原件并停止。

同文件快照已写并经生成Editor工程编译0错/299警告、原Editor DLL新于脚本。第二Play首次菜单因鸣人当时处于frame515/state3而在前置失败，未排队L，不能作为输入观察；待回到站立后同次Play的唯一有效样本仍30tick全0，但`Keyboard.lKey`在完整tick后为true，P1 DefendAction一直`Waiting`、本地held=0。故首断点为设备状态到Action，尚未进正式战斗pass；第一次前置失败与第二次输入失败均保留，不掩盖。InputSystem1.7.0源码显示未聚焦时Player更新路由需`IgnoreFocus`与`AllDeviceInputAlwaysGoesToGameView`同时成立；上一Change的单独焦点策略和本Change的单独显式Dynamic各自失败，不等于两者合取已验。修改前扩充同Task范围：本诊断期间临时合取三项焦点/后台设置，并在所有终止路径恢复、记录恢复状态；只再运行一轮。不得将其作为生产规则改动。

合取设置及恢复已写入同一Editor诊断脚本，生成Editor工程exit0、0错/332警告，原Editor DLL时间新于脚本。之后两份菜单原件均在`EditorApplication.isPlaying=false`前置退出，`focusSettingsApplied=false`，未进入输入试验；同期原Editor Play/非Play与domain reload状态切换，Battle Scene磁盘SHA由`876C0258...A421`变`4559FC86...2D00C7`，差异为BattleControls另一路序列化改动。没有覆盖/保存/回退Scene，也不推断写入者；停止重复Play。原件及当前安全停点见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DYNAMIC-INPUT-DIAG-20261004/REPORT.md)。Change状态`RUNTIME_PENDING`：代码编译和原Editor导入已证，合取运行时尚未执行；下一步仅在独占且稳定Scene身份下做一轮有限探针及必要重进。若依旧失败，记首断点，不改生产战斗规则。
