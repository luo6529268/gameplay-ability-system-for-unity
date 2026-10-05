# Q07 菜单到战斗接口的并行改动影响（2026-10-05）

状态：`IN_FLIGHT_UI_INTEGRATION_RISK / NO_BATTLE_RULE_FIRST_DIFF`。本报告是正在进行的选人界面任务 `NTSD-ROLE-SELECTION-UI-001` 的只读工作树快照，不裁决其最终实现，也不把菜单行为纳入 NTSD 战斗规则对齐。没有修改该任务的脚本、Scene、资源或测试。

当前 `SelectRoleItem.HandleConfirm` 在 `SelectingTeam` 分支调用 `OnCancel()`，使状态回到 `SelectingCharacter`；现有 `CharacterSelectionController.UpdateJoinAndSelect` 只有 `AreAllPlayersConfirmed()` 为真才启动倒计时，而该方法要求每个已加入槽位的状态为 `Confirmed`。虽然 `SelectRoleItem.OnConfirmTeam()` 仍能写 `Confirmed`，当前普通确认键路径不调用它。`CharacterSelectionController.ConfirmMatch()` 是另一公开入口；所检 `NTSD_Menu.unity` 没有对它或 `OnConfirmTeam` 的序列化方法引用。故在这个**未完成的工作树版本**，自动选人→倒计时→战斗的普通确认链存在明确接口风险；没有运行 Editor/Play，不能断言最终 UI 或用户实际操作一定失败。

本次三个相关脚本的原始 SHA-256：`SelectRoleItem.cs` `E22F771C32B95E4B27166B435752009C2A95B14461FEAD3D7CB21E79B17649FF`；`CharacterSelectionController.cs` `20E514B438F626DC8092C78219807AF160AED6C34FE9B4E27D1570644C2E4937`；新增 `CharacterSelectionBoard.cs` `A8BBDA15D4C602D135B2E0BC42A02D934685B95AA62C97E09C6E93D3E295D448`。源或 Scene 后续改变时必须重新判断，不能从本快照推断最终产品状态。

对当前对齐队列的处理：已有 Q07/Q12 真实战斗内部证据不因这个未完成 UI 改动失效；等 UI 任务稳定后，只复用现有最小 Menu→Battle 接口案例，定向检查一次角色/队伍配置交接、战斗出生和输入。若该接口出现实际首差，由选人界面 owner 修复，不扩大到战斗模拟或全角色测试。

## 2026-10-05 稳定版范围复核（覆盖上段的候选下一步）

并行任务 `NTSD-ROLE-SELECTION-UI-001` 的 Change Record 现为 `VERIFIED (scoped)`，其最终报告明确写明“No ready/countdown/battle-start extension”；原 Menu Scene 73 项及独立夹具 115 项只验选人 UI、WORDS 预热与复用，不包含 Battle handoff。本次重算 `SelectRoleItem.cs`、`CharacterSelectionController.cs`、`CharacterSelectionBoard.cs` SHA-256，分别仍为上文 `E22F771C…E79B17649FF`、`20E514B4…C2E4937`、`A8BBDA15…3E295D448`，说明上文三个源快照并未因该 scoped 验收而改变。当前 `HandleConfirm(SelectingTeam)` 仍转 `OnCancel()`；`AreAllPlayersConfirmed()` 仍要求 `Confirmed`；仓库脚本只在 `CharacterSelectionController` 自身声明/触发 `MatchConfirmed`，所检 Menu Scene 也没有 `OnConfirmTeam`、`ConfirmMatch` 序列化调用。

结论分层：这证明当前**普通 UI 确认链不能作为本战斗对齐的 Menu→Battle 验收入口**，不证明用户通过其它项目功能绝无进战斗方式，也不代表原 Battle Scene 的输入/生成/规则有首差。用户已要求保留非战斗功能、只对齐战斗场景逻辑；因此不为 Q07 修改选人状态机，不用这个未实现的 UI 路由重复跑 Play。Q07 的战斗出生、输入、退出仍以原 Battle Scene 现有定向证据判断；以后若用户另行要求普通菜单启动战斗，由选人 UI owner 建独立任务，再作最小战斗交接回归。本次只有只读源码、Scene 和既有报告复核，无脚本/Scene/资源改动。
