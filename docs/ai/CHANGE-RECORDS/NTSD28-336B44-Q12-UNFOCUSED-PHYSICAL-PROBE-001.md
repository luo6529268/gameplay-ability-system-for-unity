<!-- CHANGE-RECORD
id: NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001
status: ROLLED_BACK
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: user Q12 bounded original-Scene reentry acceptance; current 336B44 formal EXE remains battle rule authority
evidence: docs/ai/TASKS/NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001.md
-->

# Q12合成物理键探针在原Editor失焦下的输入前置

脚本改前登记。准确路径、原状、可能的背景输入原因、预期副作用、不可触碰边界、验收和精确回滚见[Task](../TASKS/NTSD28-336B44-Q12-UNFOCUSED-PHYSICAL-PROBE-001.md)。两次实际失败的 `FrameInputSet` 全0只能证明此诊断样本不可比，尚未证明正式战斗规则或生产输入错误。

2026-10-04 已写准确脚本：`NTSD28UserRasenganPhysicalPlayProbeEditor.NaturalProbe` 不再以`Keyboard.current`默认代表P1，而从已启用的P1 `DefendAction.controls` 找绑定的键盘L及设备ID；只在本诊断运行期间保存并设 `IgnoreFocus`/`AllDeviceInputAlwaysGoesToGameView`/runInBackground，排队前让已绑定键盘成为current，所有完成路径恢复原值并在结果记录恢复状态。若运行期间外部改变同一策略，报告失败且不覆盖外部新值。原来无失焦保护、可能向不匹配键盘排队；现在诊断入口有明确设备与恢复契约。未改生产代码、Scene、DAT或资源。当前 `CODE_WRITTEN`；生成编译、原Editor导入/定向Play与重进均未验。

生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、0错误/332警告；该结果只证明生成C#工程。原Editor当时的 `Assembly-CSharp-Editor.dll` 时间早于本脚本，故原Editor导入/定向Play及失焦因果仍待；状态 `COMPILE_PASS`。

原Editor后续实际导入新程序集（DLL时间晚于脚本），唯一干净Battle Scene再Play时Editor确实未聚焦。新版探针在[失败原件](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-ATTEMPT-20261004/reentry-attempt-02-focus-policy-still-zero.json)中记录P1绑定键盘deviceId1、焦点策略已调整且恢复成功，但首行及全部30行输入包仍全0，八次L脉冲后FAIL；故“临时IgnoreFocus和绑定Keyboard可解决”假设被证伪，不能继续保留这项无效改动。已退出Play，并用精确反向patch只撤本Change加到单个Editor探针的字段与语句；当前该脚本相对HEAD diff为空，未使用`git restore`，原失败原件与编译/运行记录保留。状态`ROLLED_BACK`；原Editor曾导入试验程序集，仍须刷新回源脚本版本后才能按当前磁盘代码再运行。后续只读定位键盘事件→Action回调首差，未改生产规则、DAT或Scene。
