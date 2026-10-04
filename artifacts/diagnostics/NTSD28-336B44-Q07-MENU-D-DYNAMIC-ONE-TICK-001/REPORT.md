# Q07 Menu D 单 tick 输入链诊断（2026-10-04）

结论：`VERIFIED_DIAGNOSTIC_ONLY`。原 Unity Editor 的真实 Menu→Battle 路径已分别执行两次唯一的完整生产 Driver tick；当前 `Player_1/Move` 动作包含 `/Keyboard/d#1`，与 `Keyboard.current.deviceId=1` 一致，但两种在 `EditorApplication.update` 发起的合成输入尝试均未让 D 在该 tick 前成为按下状态。动作回调与 canonical 按钮保持零、角色 X620 不动。因此本包仅证明这两个**测试注入路径**无效；没有证据认定玩家实际 D 键、生产输入映射或战斗位移有缺陷。Q07、Q09、Q12 和总目标均不关闭。

| 原件 | 测试输入与结果 | 逻辑 tick |
|---|---|---|
| [首轮 JSON](one-tick-20261003-165026-856-94f36cd9816949fd99b8ff059966bb50.json) | 直接 `InputState.Change(keyboard, D, Dynamic)`；tick 前 D=false、MoveAction X=0、回调 X=0；tick 后 canonical held/pressed/released=0、native right=0、源 X403.544921875、投影视图 X620 | 3→4 |
| [次轮 JSON](one-tick-20261003-165848-607-e5bf8233f39f4cebbaa7623fc623ccfc.json) | 临时无焦点输入策略 + `QueueStateEvent(D)` + 既有反射 typed Dynamic `InputSystem.Update`；同样 D=false、动作/回调/canonical=0、源/投影 X 不动；策略在 `finally` 恢复 | 3→4 |

两 JSON 的 `status=PASS` 仅表示探针成功取得一个完整 Driver tick 的测量，不表示 D 已注入或正式 NTSD 行为已对齐。此前 [D 输入首差](../NTSD28-336B44-Q07-MENU-D-INPUT-TRACE-001/REPORT.md)曾在另一种立即更新路径看到设备 D=true、动作值仍0；不同合成输入方式的阴性结果不可合并成生产故障。当前 `NTSDInputConfig.inputactions` 的 P1 四方向控制来自同一设备，D Right 绑定静态可见。

生成 `Assembly-CSharp-Editor.csproj` 对两个版本分别编译通过，最终版本为 0 error/296 warnings；原 Editor DLL 在源码后更新，Console 当前 0 error。每次 Play 后 MCP 返回 `is_playing=false`、`NTSD_Menu`、`isDirty=false`、Editor idle；Menu/Battle/GameConfig/ProjectBattleModeConfig 四 SHA 均保持：`F01144C9…6329F`、`93448372…D7BF60`、`0527D737…B8EA7`、`B57CFEF3…1EDD85B82`。第二轮后未见场景/资产变动。`Tools/Validate-ChangeLedger.ps1` 在脚本改动后返回 PASS，覆盖此 Editor-only 路径；最终文档/代码检查另记于 Change Record。

下一步：停止用 Editor 回调重复投递 D 键。若 Q09 姓名牌视口验收仍必须合成方向输入，只沿仓库已使用的 `PlayerLoop` 注入点做一次边界验证，并记录注入回调内设备及动作值；否则优先推进 336B44 总表中其它正式可达的 Q07 首差。没有正式 EXE/Unity 同条件键流证据前不修改生产输入、DAT 或场景。
