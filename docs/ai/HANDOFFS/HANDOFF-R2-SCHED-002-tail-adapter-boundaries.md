# HANDOFF — R2-SCHED-002 mode2 tail reset 时点

> 交接日期：2026-08-21  
> Change ID：`R2-SCHED-002`  
> 状态：`RUNTIME_PENDING`  
> 当前阶段：已完成 C++ / Unity source 合同、范围冻结、最小代码写入、Unity 编译与 focused self-check；尚未取得 Play Mode、joint fixture 或 C++ runtime trace 证据。

## 已确认事实

- C++ `game_tick.cpp:2083-2089` 的相对顺序是 late entity → mode2 tail → entity postframe tail →
  `g_init_stats=0` → `g_game_mode2=0`；
- Unity 当前在 `Mode2RandomWeaponDropTailAll` 内清 `Mode2Request`，早于
  `EntityPostFrameTailAll`；
- Unity `EndCollisionCandidateConsumption` 只清 adapter cache；实体 candidate carrier 实际在
  `EntityPostFrameTailAll` 清理，不能删除 candidate-end adapter；
- C++ `g_init_stats` 来自 F7；Unity 没有对应 runtime field。它不属于当前 mode2-only patch；
- D-SCHED-006/007/008/009/012 仍须按 task contract 保持 UNKNOWN / mapping / approved adapter，
  不能因为本次 tail 修复被声称为完成。

## 已写入的代码动作

按 `R2-SCHED-002.md`：

1. 已移除 `Mode2RandomWeaponDropTailAll` 的提前清零；
2. 已在 scheduler 的 entity postframe tail 后、results flow 前清零；
3. 已加 focused self-check；
4. 静态顺序、UnityMCP scripts compile 与 request self-check 已完成；
5. 现有 Editor 的 request result 于 `2026-08-21T14:58:30.0976482Z` 返回 `PASS`，
   `error CS` Console 过滤为 0；
6. 全 Console 中的两条 MCP bridge disposed-object 日志和两条 runtime-rest negative-path
   self-check 日志均已区分记录；它们不改变本次 self-check 的 `PASS`，也不是 C++/Play Mode
   对齐证据。

## 停止点与未关闭项

- `R2-SCHED-002` 只能写为 `RUNTIME_PENDING`，不能写为“D-SCHED-011 完整对齐”；
- `g_init_stats` / F7、D-SCHED-006～010/012、joint fixture、F8/F9 Play Mode 与 C++ full trace
  均未处理；
- 不得在没有新的 Task Contract / Change Record / 用户确认的情况下开始 R3、R4 或扩大
  R2-PASS-02。

不得开始 `InitStats`、F7、candidate、Stage-Z、slot/newborn、input、render、R3+。
