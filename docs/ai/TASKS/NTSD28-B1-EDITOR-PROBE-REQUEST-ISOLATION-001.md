# Task Contract — NTSD28-B1-EDITOR-PROBE-REQUEST-ISOLATION-001

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_NO_REQUEST_PASS / REQUEST_SCENARIO_NOT_RERUN`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

修复两个旧R8 Editor Play探针的全局`PollRequest()`副作用：仅在各自request文件真实存在后，才允许
解除Editor pause、强制Driver unpause、等待tick并消费request。当前实现先无条件解除pause，随后才检查
request，导致普通Play以及B1 F1暂停被强制恢复。保留旧探针、请求路径、运行逻辑和历史用途。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 无request时，poller不得写`EditorApplication.isPaused`、不得调用`SetPaused(false)`、不得改world。
- 有request时保留原先的Editor unpause、Driver unpause、tick>=5、删除request后运行probe顺序。
- 不删除历史探针，不改其fixture、pass逻辑、输出格式或生产battle代码。
- fresh compile0；B1真实Play F1暂停至少稳定150ms且不再出现这两个poller的`SetPaused(false)`栈。
- 若另有poller产生同类副作用，必须以新证据扩展清单，不能泛化批量修改所有Editor脚本。

## 回滚

恢复两个`PollRequest`原顺序；保留真实调用栈证据并将本Change标记`ROLLED_BACK`。

## 实际验收

- first-failure栈精确指向`BattleOid5152MergeSplitPlayModeProbeEditor.PollRequest()`在无request时
  调用`SetPaused(false)`；同结构静态确认于CentralLiveness探针。
- 两个poller只将各自request存在性检查前置；request存在后的原顺序与正文未改。
- final真实Play中F1 pause跨150ms稳定，`SetPaused`总调用只保留bootstrap的1次false，不再出现
  R8 poller栈；Host全流程通过。
- 两个旧R8 request场景本包未主动创建/重跑，故只声明no-request隔离通过。
