# HANDOFF — R4-COL-04B immediate query source preflight

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。）  
> Change ID：`R4-COL-004B`。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改或写入 C++ authority。

## 已确认

- `R4-COL-04A` 只处理 frozen candidate collection；其余 `QueryBodyHits` usage不能并入同一 helper-only
  修改。
- Unity `LF2Weapon.OnLanded()` 的 state13/high-speed branch实际可达，并直接扫附近 target写 `Hit`；
  C++ release `physics.cpp` 该 weapon landing contract没有对应 target scan / hit。
- C++ held WPoint attack通过 `collision.cpp` kind5 local ITR transform进入正常 collect/consume；不是 Unity
  `ProcessAttack` 的即时 scan。
- Unity `ProcessAttack`当前没有生产静态 caller，且 held `Act` self-check断言其不被调用；这只是
  `INFERRED` dormant，不授权删除。

## 已完成的最小脚本包

`R4-COL-04B / R4-COL-004B` 已具备：

- `RESEARCH/R4-COL-04B-immediate-query-source-preflight-20260822.md`；
- `TASKS/R4-COL-04B-immediate-landing-query-contract.md`；
- `CHANGE-RECORDS/R4-COL-004B.md`。

已只在记录许可的两个 Unity 脚本内完成最小修改和fixture：active landing splash的 target scan / direct
`Hit`已移除；不能删 helper、不能清理 dormant held path、不能动 C++ / DAT / pass ordering。

## 验证待办

1. Unity Editor / UnityMCP（port 6401，Unity 2022.3.62f3）refresh后 Console `error CS`=0；
2. full `BattleRuntimeSelfCheck`实际通过：`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入
   2026-08-22 04:52:29 +08:00；
3. `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\Tools\\Validate-ChangeLedger.ps1` PASS（19 records /
   15 governed code files）；`git diff --check` exit 0，只有既有 LF/CRLF warning；
4. C++ runtime trace与真实 weapon landing Play Mode仍未执行。

仍未关闭：C++ runtime trace、真实 weapon landing Play Mode、dormant `ProcessAttack` runtime reachability。

## 连续下一步

本包的脚本最小实现已完成并记录，按 D-009 自动进入 `D-COL-005` 的 kind1 target-type source preflight；
不得把本包当成整个 R4、weapon、held 或 battle alignment的完成声明。
