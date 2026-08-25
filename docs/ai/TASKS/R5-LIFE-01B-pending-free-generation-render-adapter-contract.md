# R5-LIFE-01B — pending/free/generation 与 render logic adapter 合同

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source/mapping、focused tests、compile request与full self-check已通过；Play Mode/C++ trace待验）  
> 对应：`D-SCHED-012` pending/free subset、`D-LIFE-001`、`D-RENDER-003` logic half

## Goal

不修改 production 脚本，只认证 Unity 的 pending destroy、slot generation、pool finalization、
normal late-opoint first-visible tick 与 C++ Release active/free/render 合同的可观察等价性。

## Scope

允许：

1. 只读 C++ release source与Unity runtime；
2. 运行既有 `W05OpointLifecycleEditorTests`、`BattlePresentationBeginFrameReuseEditorTests`；
3. 运行 fresh Unity compile与full `BattleRuntimeSelfCheck`；
4. 更新 research/task/state/diff register/main plan/handoff。

禁止：

- 修改任何 `.cs`、shader、scene、prefab、DAT或C++ authority文件；
- 删除 generation、PendingFlushDestroy、OidMergeDormant、FirstPresentationTick 或 CentralOnly gate；
- 把R6 visual/order、R7性能、R8 Play Mode混入本包。

## Authority / Evidence

- C++：`game_tick.cpp:577-691,1017-1154,2061-2073,2190-2194`；
- C++ reset：`include/game_world.h:216-258`；
- C++ spawn allocation：`collision.cpp:1271-1285`、`game_tick.cpp:699-719`及release live battle中slot20/50起点；
- Unity：`SimulationWorld.Registry.partial.cs:1054-1415`、`BattlePresentationShadowBuild.cs:1831-1843,1977-1988`；
- 既有 tests：`W05OpointLifecycleEditorTests`、`BattlePresentationBeginFrameReuseEditorTests`、full self-check P3/OID maintenance。

## Required behavior

1. free 后旧 handle立即不可解析；
2. pending slot可以按最低空槽规则被newborn复用；
3. old pool finalization不清理newborn generation/command；
4. RenderDispatch(T)后的late opoint不修改已发布T画面，首次进入T+1；
5. dormant/pending实体不进入active query与central capture；
6. production normal opoint不依赖非零FirstPresentationTick；
7. approved Extended capacity与desktop growth保持不变。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | release build参与性、free/reset、render、spawn slot domain闭合。 |
| S1 static | Unity writer/reader inventory、FirstPresentationTick production赋值搜索。 |
| S2 focused | W05 lifecycle class + presentation reuse class全部PASS。 |
| S3 Unity | fresh scripts compile 0 error；full self-check PASS。 |
| S4 governance | STATE/register/main plan/handoff更新；validator与doc diff check。 |
| S5 honesty | 最高RUNTIME_PENDING；C++ trace/真实Play Mode/R6 visual仍待。 |

## Stop conditions

- focused test显示old generation ghost、slot不复用或first-visible tick不一致；
- 发现正式 battle-time slot0..19 allocator；
- 发现production非零FirstPresentationTick writer；
- 需要修改任何脚本或架构：必须停止本no-code包，先建立独立Change Record。

## Out of scope

R6 visual/render descriptor、R1-WP02 trace、T8、Android、性能压测、Unity场景人工验收。

## 实际结果

- production/test C#均未修改；
- UnityMCP force scripts refresh/compile request完成，domain reload后恢复ready；因本包无C# diff，程序集时间保持17:14:38，不把它误报为新DLL；
- focused EditMode job `582b9e9212264d39b4377b72d7e0374d`为19/19 PASS；
- 2026-08-22 17:49:18 full `BattleRuntimeSelfCheck`=`PASS`；
- 普通pending/free/generation与D-RENDER-003 logic half关闭到Unity自动证据层，最高仍为`RUNTIME_PENDING`；
- D-LIFE-001 OidMergeDormant为`INFERRED safe adapter`，真实Play Mode/C++ trace仍待后续。
