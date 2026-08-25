# R4-COL-01 — `hit_confirm2` 整 attacker abort 只读预检

> 日期：2026-08-22  
> 状态：`VERIFIED`（C++ release source / Unity source control-flow preflight）；尚未修改脚本、未运行 C++ executable、未运行 Unity 测试。  
> 对应差异：`D-COL-001`。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。

## 1. C++ release 合同

- `src/entity/collision.cpp:32-1258` 的 `collision_check_impl(...)` 是 `collision_check_loop1(...)`
  和 `collision_check_loop2(...)` 的共同消费实现；两个 wrapper 分别在 1263、1264 调用它。
- 每个 attacker 以固定 slot 递增扫描；每个冻结 candidate 先检查目标有效和 `s_vrest[target][attacker]`。
- `collision.cpp:57-65` 的顺序是：

  1. `s_vrest[j][i] > 0`：跳过**当前 candidate**，继续下一条；
  2. `atk.hit_confirm2 != 0 && vic_core.char_data->obj_type == 0`：`goto next_attacker`；
  3. 之后才进入 caught-cpoint gate、runtime ITR replacement、effect21 和 kind dispatch。

- `next_attacker` 位于 1257、在 pair loop 之外，因此它终止的是该 attacker 的**全部剩余冻结
  candidate**，不是仅跳过当前角色目标。
- `game_tick.cpp` 的 Loop1/Loop2 调度与 `collision.cpp` 的 wrapper 共同证明：该规则对当前
  obj_type 为 character 的 attacker 与 non-character 的 attacker 都生效；攻击者类别不改变 C07-A
  的 gate 语义。

## 2. Unity 当前映射与差异

- `BattleHitCandidateSequenceRunner.TryConsumeCaptured(...)` 是 exact data-oriented 和 fallback
  character/object consumers 的共同 candidate sequence owner。
- 当前 `TryConsumeCandidate(...)` 解析 target 后先运行
  `BruteForceSceneQuery.ResolveRuntimeItrForPair(...)`，之后才调用
  `CanConsumeRecordedCandidate(...)`；其中没有 `attacker.HitConfirm2` reader。
- `CanConsumeRecordedCandidate(...)` 已覆盖 Unity 对 C++ vrest/current-target validity 的消费侧
  再检查，且没有 writer；`ResolveRuntimeItrForPair(...)` 只读取关系/帧数据并在 replacement 时创建
  shallow copy，不写 attacker、target 或 world。
- `LF2Entity.GetCurrentDataObjectTypeForSimulation()` / `ResolveCurrentDataObjectType(...)` 从当前 DAT
  type 解析类别，不依赖 CLR subclass；这是与 C++ `char_data->obj_type == 0` 对齐所需的 Unity
  adapter。`LF2ObjectType.Character` 为 0。

## 3. 最小适配结论

在统一 runner 中，保留既有 `ItrIndex` 防御性有效性检查；对于正常 collect 产生的有效冻结
candidate，target resolve 后应严格按下面顺序处理：

```text
candidate target resolve
  -> CanConsumeRecordedCandidate (vrest/current-target validity)
  -> attacker.HitConfirm2 && target current DAT type == Character ? abort sequence
  -> ResolveRuntimeItrForPair / CPoint / effect21 / kind dispatch
```

其中“abort sequence”可复用现有 `TryConsumeCandidate` 的 `true` 返回语义；
`TryConsumeCaptured` 已将 `true` 实现为 `break`，故无需改变 scheduler、candidate collection 或每个
consumer 的循环。

将 vrest check 前移是为了同时保留 C++ 的“vrest skip 只跳过当前 candidate”与“hit_confirm2 abort
发生在 vrest 通过之后”的关系，并避免在 C++ 本不会进入 replacement 的 candidate 上创建 Unity
runtime ITR shallow copy。

## 4. 最小回归 fixture 合同

1. 使用一个攻击者、至少两个冻结 candidate，二者均是 current-DAT character target；
2. candidate collect 后、character consume 前手动将 attacker `HitConfirm2=1`，模拟更低 slot
   attacker 在同 tick 前段已将该实体命中的 C++ 可观察状态；
3. 断言本 attacker 不对第一或后续 character target dispatch damage，且 candidate range 仍是两条；
4. 同时覆盖 exact `LF2Character` consumer 和 shared-character-DAT fallback consumer，证明共同 runner
   对两条消费入口相同；
5. 不在本包内建立 caught-cpoint、effect21、kind1 target-type、type3 damage 或 held/link fixture。

## 5. 未知项与边界

- 未取得 C++ runtime trace；R1-WP02 仍 `BLOCKED`。
- 本预检不证明真实技能 DAT 在 Play Mode 中何时产生 `HitConfirm2`；只证明 C++ 规则及 Unity
  缺失的 shared gate。
- 本预检不授权修改 C++、scheduler、candidate collection、CPoint、held/link、opoint、renderer、
  capacity/SoA/pool 或任何 D-COL-002+ / D-HIT 条目。
