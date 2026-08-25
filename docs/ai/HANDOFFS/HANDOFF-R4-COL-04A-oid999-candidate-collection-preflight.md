# HANDOFF — R4-COL-04A oid999 candidate-collection preflight

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-COL-004A`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改、构建或写入 C++ authority。

## 已确认

- C++ `collision_collect.cpp:107-120,220-371` 的正式 collect链无 oid999/transition-smoke全局排除；
  只要对象满足一般 ITR/BDY/geometry/kind/team/effect/select条件，就可参与。
- Unity `CandidateCollectionPairAllowed`和 role-aware cached participant base均额外用
  `IsPureTransitionSmoke`排除 oid999；这是 D-COL-004 的 candidate-collection 差异。
- 当前 Unity adapted production `broken_weapon.dat` 的被 gate 帧无有效碰撞几何；该事实不能证明额外
  filter等价，只说明当前资产下差异可能不触发。

## 当前边界

- 本包只处理 frozen candidate collection的两处 extra gate；
- `QueryBodyHits` immediate query中的两个 helper调用仍保留，需独立 C++ caller audit；
- synthetic valid geometry将用来验证 C++ 规则，不修改 oid999 DAT/资源或生命周期。

## 验证证据

| 检查 | 结果 |
|---|---|
| Unity scripts refresh / compile | 现有 Unity Editor / UnityMCP port 6401；domain reload的 TCP disconnect后，Console `error CS`=0。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 04:33:40 +08:00。 |
| focused behavior | synthetic oid999 target/attacker的 valid geometry在 brute与role-aware formal collection均记录相同 candidate / RNG。 |
| C++ runtime / Play Mode | 未执行；R1-WP02 C++ trace仍 `BLOCKED`。 |

## 连续下一步（D-009）

本包达到 `RUNTIME_PENDING` 后不等待逐包确认，进入 `D-COL-004B` 的只读 source preflight：只审计
`QueryBodyHits` immediate-query中剩余两个 `IsPureTransitionSmoke`调用的 production caller与 C++ 对应路径。
不得将它和 D-COL-005、D-HIT或R5合并。
