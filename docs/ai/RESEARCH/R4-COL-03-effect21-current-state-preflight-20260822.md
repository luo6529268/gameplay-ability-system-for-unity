# R4-COL-03 — effect21 current-state attacker-abort preflight

> 日期：2026-08-22  
> 类型：只读 source preflight；未修改 Unity/C++ gameplay。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 对应差异：`D-COL-003`。

## 1. C++ release 合同（VERIFIED）

`Makefile:10-31` 将 `src/entity/collision.cpp` 与 `src/entity/collision_collect.cpp` 共同列入
`ntsd_new.exe` 的 release `SRCS`。在 `collision.cpp` 的 consume loop 中：

1. `57-65`：先以 current vrest 跳过当前 pair，随后 `hit_confirm2` 对 character target 跳往
   `next_attacker`；
2. `69-79`：caught-cpoint / non-hurtable catcher 跳往 `next_pair_outer`，即只跳过当前 pair；
3. `91-135`：kind5 可在 local itr copy 中替换伤害字段并写 `kind=0`；
4. `138-147`：held weapon-count kind4 可写 `kind=0`；`170-185`：kind9 在条件满足时也可写
   `kind=0`；
5. `188-194`：以**上述转换后的 local `kind` / `itr.effect`**检查 `kind == 0 && effect == 21`，再以
   `vic_core.frame` 取 target **当前** `FrameData.state`。若 state 为 `18` 或 `19`，跳往
   `next_attacker`；
6. `1253-1258`：`next_attacker` 位于该 attacker pair loop 外，因此本条件不写当前 target，也不再
   消费该 attacker 后续任何 frozen candidate。

`collision_collect.cpp:325-333` 另有 collect-time filter：source kind0/effect21 在 target **prev**
state 为18/19时不记录 candidate。它不是 `collision.cpp:188-194` 的替代：前者读取 previous frame、
后者读取 consume 时 current frame，且后者的作用范围是 entire attacker abort。

## 2. Unity 现状（VERIFIED）

- `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 已依次实现 C07-A（whole-attacker abort）与
  C07-B（current-candidate skip）；随后才调用
  `BruteForceSceneQuery.ResolveRuntimeItrForPair(...)`。
- `ResolveRuntimeItrForPair` 已实现 kind5/4/9 的 runtime local-itr 转换，因此是 C++ effect21 gate
  正确的读取位置之后。
- `BruteForceSceneQuery.Kind0EffectAllowed` 的 effect21 判断只读 `target.Frame.Prev`，并服务于
  candidate collect / query filter。它不能处理“candidate已冻结后 target current frame 在同 tick 改为
  18/19”的 C++ consume-time abort。
- 当前 runner 在 runtime-itr resolve 后没有 effect21/current-state `return true`。因此 D-COL-003 是
  可复现的静态缺口，而不是“已经由 collect filter 覆盖”。

## 3. 最小实现方向（INFERRED，待 Task Contract 后验证）

在 runner的 runtime itr resolve 成功后、legacy observation/disposition/任何 writer之前，读取：

```text
runtimeItr.kind == 0
&& runtimeItr.effect == 21
&& target current authored FrameData.state ∈ {18, 19}
```

为真则返回既有 sequence-break 值 `true`。该位置保留 C++ 的 C07-A → C07-B → local itr conversion →
effect21 current-state abort 顺序，也让 kind5/4/9 的 local conversion自然纳入判断。

不应把该规则塞入 `Kind0EffectAllowed` 或全局 `RuntimeConsumeItrAllowed`：那会把 C++ 的
entire-attacker abort降格为 current-candidate reject，并错误影响非 frozen-query调用点。

## 4. 必需 fixture

1. **exact/shared × state18/state19 abort**：先以 target previous state 非18/19记录两个 frozen
   candidates；在 consume 前仅切 first target的 current frame 到18或19。C++ 合同要求 first/second均不写
   HP/vrest，sequence停止。
2. **ordinary current-state control**：同一结构但 first current state 非18/19，两个 candidate均继续。
3. **runtime transformation placement**：至少验证一条原 source itr 非kind0、经
   `ResolveRuntimeItrForPair` 成为 kind0/effect21的有效路线（优先kind5；若其合法 held precondition无法用
   最小夹具建立，再评估kind4）。若无法建立，不得默默降级测试，必须标为 blocker或单开依赖包。
4. frozen candidate count、slot order、first/second HP/vrest和 `HitConfirm2` 前置均须明确断言；不使用
   Play Mode、C++ executable或旧C#结论补足。

## 5. 未知项 / 停止条件

- **VERIFIED（Unity focused fixture）**：source kind4且 `WeaponCount=1` 可在不触及 held/CPoint writer的
  条件下合法转换为 runtime kind0/effect21，并命中本 C07-C gate；因此 placement fixture已关闭。
- **UNKNOWN**：kind5 transformed effect21的独立 held-relation场景尚未构造；它不阻断本包，因为C++ gate读取
  transformed runtime itr，且kind4已证明该转换后读取位置。若后续R5涉及该关系，须用其 own fixture重开。
- 如果 target current authored frame在 runner中的唯一正确读取接口不能由 `Frame.N`/`Frame.D`闭合，停止；
  不新增跨层 snapshot或改动 Frame writer。
- 如果 fixture证明 C++ gate还需改变 candidate collection顺序、CPoint/held关系或多pass scheduler，停止并拆包。

## 6. 非范围

D-COL-004/005、D-HIT、CPoint/held writer、candidate collect重构、broadphase、R5+、render、性能、
T8、服务器、Android、C++ executable或任何 authority 写入。
