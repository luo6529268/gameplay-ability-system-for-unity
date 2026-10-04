<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable Naruto natural Attack windows
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001.md
-->

# Q07 鸣人后续 Attack 当前场景输入探针

脚本修改前登记。原状：现有自然物理键探针首253当前原 Scene PASS，但 Editor 失焦的第二次 Play 中 L 键 30 tick 未进 FrameInputSet，故第二/254后的当前336B44 Unity结果仍未知。当前正式源码/根三案动作、MP和相位 990/990 同。计划仅在既有 Editor 探针 `NaturalProbe.Queue` 排键后推进 InputSystem 一次，保留全部原菜单与结果路径。影响仅测试时的合成设备注入，不改变生产、DAT、资源、Scene、Prefab、配置或非战斗。验收、风险及回滚见 [Task](../TASKS/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001.md)。修改、编译、Play、残留与未验证项均在执行后追加。

2026-10-04 实际脚本已写：仅在 NaturalProbe.Queue 的合成键盘 QueueStateEvent 后调用 InputSystem.Update()，保持事件通过原 Keyboard/InputAction/LocalFreeRun 提供者；无新的菜单、字段、输出覆盖或生产路径。预期后台Editor也能在下一生产tick取得输入。编译、Play、正式逐tick配对、残留待，状态 CODE_WRITTEN。


2026-10-04 验证追加：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0/0 error/332 warning；原Editor真实程序集时间晚于脚本，原Battle Scene首窗口Play PASS，相对tick34 action301/PP250/phase0。严格源/Unity初态首帧action0/1不等，防御消费后tick2～34四字段132/132同；[原件与配对](../../../artifacts/diagnostics/NTSD28-336B44-Q07-RASENGAN-ATTACK-CURRENT-SCENE-001/REPORT.md)。第二窗口一次失焦键未入FrameInputSet，修探针后下一次监测漏bootstrap而未执行菜单；254后未测。两次有效Play退出Scene clean/Battle SHA稳定。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot` exit0；无生产首差，状态`RUNTIME_PENDING`，按最新停止线只在用户可见/物理键首差或Q12终验触发另两窗口。生产/DAT/Scene/非战斗未改。
