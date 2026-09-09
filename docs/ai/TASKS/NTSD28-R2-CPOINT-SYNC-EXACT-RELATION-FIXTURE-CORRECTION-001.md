# NTSD28-R2-CPOINT-SYNC-EXACT-RELATION-FIXTURE-CORRECTION-001 — Task Contract

> 2026-09-09，Goal 1.B，test-only。任务与Record先于脚本修改建立。

## 授权与判断依据

用户本轮明确仅授权Goal 1；直接采用GLM报告5.B的已核验结论：
`CheckReleaseTickCpointSyncFollowsCandidates()`只设置compat关系，缺少victim的exact
`CatchSourceSlot90`，使CPoint sync早退。禁止重新审计该结论或恢复production compat读取。
当前NTSD 2.8-Logan正式EXE/闭包身份与battle rules不改变。

## 精确写入清单

- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`：只在
  `CheckReleaseTickCpointSyncFollowsCandidates()`的关系夹具初始化处补exact source slot。
- 本Task与同ID的`docs/ai/CHANGE-RECORDS/`文件。
- `docs/ai/CHANGE-LEDGER.md`：追加本包索引及最终证据。

Goal 1.A的独立治理Record负责其Ledger correction，不属于本包脚本改动。
不修改STATE/handoff、已有Record、production/Simulation、Scene、Prefab、Config、资源、
ProjectSettings、Authority、GLM报告、continuation prompt或`.codex/config.toml`。
本轮精确写入清单优先于AGENTS通用STATE/handoff同步要求；若validator因此报告活跃ID缺失，
如实报告已知原因，不越权更新STATE、不改validator、不虚报VERIFIED。

## 原状、最小变更与不变量

原夹具设置`Catching`、`CaughtSlotIndex`、`CatcherSlotIndex`，保持这三者和全部断言不变。
参照同文件`LinkCpointEntities()`及`BattleGrabCpointLinkPlayModeProbeEditor`，在victim上补
`victim.Runtime.CatchSourceSlot90 = catcher.Runtime.SlotIndex;`。
预期只让本夹具具有当前exact关系；不改变生产默认值、candidate/CPoint顺序、帧或位置预期。
只有实际消费所必需的最小字段才可追加；若一行修正后R2仍失败，立即停止，不继续试修。

## 验收顺序

1. 固定`gameplay-ability-system-for-unity@b1b02287`；核验Unity `2022.3.62f3`、
   active scene `Assets/NTSD/Scene/NTSD_Battle.unity`、idle、非Play/编译/测试占用。
   无法连接或被占用时不启动第二个Editor，走dotnet编译并标记SelfCheck未运行。
2. 修改前执行现有菜单`NTSD/验证/运行战斗运行时自检`，读新鲜result/Console，记录RED停点。
3. 最小test-only修改后刷新脚本、重建连接与身份核验，重跑同一SelfCheck。
   记录是否越过R2及最终停点；后续独立断言失败不修。
4. 顺序运行`dotnet build Assembly-CSharp.csproj --no-restore`及
   `dotnet build Assembly-CSharp-Editor.csproj --no-restore`，要求两者0 error。
5. 执行`& ./Tools/Validate-ChangeLedger.ps1`；核对本轮增量文件清单。
6. 报告RED/GREEN、build、validator、未验证项与真实状态后停止，等待Goal 2授权。

## 硬停止与回滚

R2修正后仍失败、需要production/Scene/Config/Authority改动、增量diff越界，或validator原因不明，
立即停止。不得为修后续断言而改变范围。SelfCheck未成功运行时状态只能为RUNTIME_PENDING。
回滚须用户明确批准，只逆向本包新增的exact赋值与本包文档增量；不能整文件restore，不能
回退其他Change已有修改。无不可逆资源迁移、schema变化或Git操作。
