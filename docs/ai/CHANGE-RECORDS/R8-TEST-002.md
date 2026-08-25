# R8-TEST-002 — W07 positive-link residue fixture sync

<!-- CHANGE-RECORD
id: R8-TEST-002
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleParityStructuralWitnessEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1828-1845; R5-LINK-001
evidence: FULL EDITMODE SINGLE FAILURE + FRESH EXACT 1/1 FAILURE + SOURCE/DIFF CLOSED
-->

> 创建日期：2026-08-23
> 类型：test / structural witness

## 1. 状态与范围

`VERIFIED / TEST-ONLY`。只修W07 fixture/postconditions；production link逻辑无授权。

## 2. First difference

R5-LINK-001已使Legacy/DataOriented invalid positive relation只清`LinkState`。W07 setup仍在tick3后要求holder
`TargetSlotIndex/HeldWeaponStableId == -1`，且结构测试也期待event after=-1/-1；当前actual保持1/1并符合C++。

## 3. Planned change

- W07 fixture postcondition：holder `LinkState=0, TargetSlotIndex=1, HeldWeaponStableId=1`；
- target reverse residue继续必须是`HolderStableId=2, LinkState=0`；
- event test同步after 0/1/1，并改清晰的方法名；
- 不改变event producer、production pass或任何其他fixture。

## 4. Verification / rollback

exact→class→full EditMode→same-domain/fresh self-check→compile/validator/diff。失败则仅回滚这两个test文件增量并
标BLOCKED，不得恢复production extra clears迎合旧fixture。

## 5. Worktree boundary

保留所有既有用户/R2～R7修改；本包未提交，不触碰C++ authority、scene、resource或config。

## 6. Actual change

- fixture postcondition由holder `0/-1/-1`同步为`0/1/1`，target reverse `2/0`保持；
- structural event assertions同步after `0/1/1`；方法名明确为clears-only-link-state；
- production/event producer/其他fixture未改；尚未取得编译或测试证据。

## 7. Current verification

- Unity `Assembly-CSharp-Editor.dll` 03:50:47晚于两目标source 03:50:00/03:50:02，未检出`error CS`；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly`：0 errors、18 existing warnings、exit0；
- validator：PASSED，56 Records / 55 governed code files；
- UnityMCP listener在本次domain reload后未恢复监听，Unity PID 2880仍存活/响应；exact/class/full/self-check尚未运行，
  不能提升为focused pass。恢复条件：用户在MCP For Unity面板重新`Start Session`，不需要启动第二个Editor。

MCP恢复后，W07 exact job `ad10828ee9d741aa8c2068c1ad7db6c8` 1/1、structural class job
`4101eded225e493aa48ad1f4549e6d54` 4/4、full job `6a6336d0e1e94abd9585110358012ca5`
1357/1357 PASS；同域self-check 07:31:17、fresh-domain self-check 07:32:39均PASS。故本test-only合同
VERIFIED；production R5-LINK-001的Play Mode/C++ trace状态不因本结果改变。
