# HANDOFF — R4-COL-03 effect21 current-state attacker-abort preflight

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-COL-003`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改、构建或写入 C++ authority。

## 已确认

- C++ `collision.cpp:188-194` 是 consume-time local itr gate：经 kind5/4/9 local conversion后的
  kind0/effect21，遇 target **current** state18/19时跳转 `next_attacker`。
- C++ `collision_collect.cpp:325-333` 的 source kind0/effect21 / target **previous** state filter是独立
  collection规则，不能替代本 whole-attacker abort。
- Unity runner已实现 C07-A/C07-B，但在 `ResolveRuntimeItrForPair` 之后尚无 C07-C。其现有
  `Kind0EffectAllowed`也只读 previous state，故 D-COL-003仍是静态缺口。

## 当前边界

- 已建立 `R4-COL-03` Task Contract、Change Record和本预检；已仅触及 Unity runner与focused fixture；
- 优先最小改动是 shared runner在 runtime itr resolve后、任何 writer前以 `return true` sequence break；
- 必须有 exact/shared state18/state19 abort、ordinary control，及一条可行 runtime conversion route的
  fixture；转换路线不能无证据省略。

## 未知 / 停止点

- kind5/kind4 transformed effect21 fixture的最小可达性还未验证；如需改 held/CPoint或 scheduler，停止并拆包；
- 不运行 C++ executable，不开始 Play Mode、trace、D-COL-004/005、D-HIT或R5。

## 验证证据

| 检查 | 结果 |
|---|---|
| Unity scripts refresh / compile | 现有 Unity Editor / UnityMCP port 6401；domain reload的 TCP disconnect后，Console `error CS`=0。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 04:16:17 +08:00。 |
| focused behavior | exact/shared state18/state19 abort、ordinary control、kind4 runtime conversion placement均通过 full self-check。 |
| C++ runtime / Play Mode | 未执行；R1-WP02 C++ trace仍 `BLOCKED`。 |

## 连续下一步（D-009）

本包达到 `RUNTIME_PENDING` 后不等待逐包确认，进入 `D-COL-004` 的只读 source preflight：确认 C++ candidate
collect对 oid999的实际 pair筛选与 Unity `IsPureTransitionSmoke` 的额外全局排除是否冲突，再建立独立合同。
不得和 D-COL-005、D-HIT或R5合并。
