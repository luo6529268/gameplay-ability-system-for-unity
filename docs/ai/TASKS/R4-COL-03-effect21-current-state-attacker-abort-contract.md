# R4-COL-03 — C++ effect21 current-state whole-attacker abort

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、Unity compile、full self-check已通过；C++ trace / Play Mode待补。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-003`。  
> 前置调查：`RESEARCH/R4-COL-03-effect21-current-state-preflight-20260822.md`。

## Goal

让 Unity frozen-candidate shared runner复现 C++ `collision.cpp:188-194`：当 local runtime itr已变为
`kind=0/effect=21`，并且当前 target frame state为18或19时，在任何 writer之前中止该 attacker剩余的
candidate sequence。collect-time previous-state filter保留其原职责，不代替本 consume-time gate。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
   - 在 C07-A/C07-B后、runtime itr resolve成功后、disposition/dispatch前，加入最小 current-state
     sequence-break gate。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 新增 exact/shared current-state 18/19 two-candidate abort与ordinary control；
   - 仅在当前 fixture可不扩张依赖地建立合法条件时，覆盖一条 runtime transformation route。

禁止：

- 修改 C++、candidate collection、`Kind0EffectAllowed`、`RuntimeConsumeItrAllowed`、CPoint/held writer、
  scheduler、render、pool、容量；
- 合并 D-COL-004/005、D-HIT、R5+、T8、服务器、Android或性能修改。

## Authority / Evidence

### VERIFIED

- C++ `collision.cpp:57-79,91-194,1253-1258` 的顺序、local itr转化和 `next_attacker` 范围；
- C++ `collision_collect.cpp:325-333` 的 separate previous-frame effect21 collection filter；
- Unity runner与 `ResolveRuntimeItrForPair` 的现有位置和转换职责。

### UNKNOWN

- kind5/kind4 transformed effect21 fixture是否可在本包范围内建立；
- C++ runtime trace与真实 effect21 Play Mode仍不可用 / 未执行。

## Files likely involved

| 文件 | 责任 |
|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` | C07-C consume-time whole-attacker abort。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | exact/shared frozen-candidate current-state fixture。 |
| `docs/ai/CHANGE-RECORDS/R4-COL-003.md` | 改动、验证、未关闭项与回滚记录。 |

## Deliverables

1. 唯一 shared runner的 C07-C current-state gate；
2. exact/shared state18/state19 abort、ordinary control和可行的 transformation placement fixture；
3. Unity scripts refresh/compile、full self-check、ledger validator、diff check真实证据；
4. Change Record、ledger、STATE、diff register、主计划与结构化 handoff更新。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 source order | C07-A → C07-B → runtime itr conversion → C07-C；C07-C使用 sequence break。 |
| S1 exact | current state18 /19的 first frozen candidate不写HP/vrest，second candidate也不消费。 |
| S2 shared | current character-DAT fallback得到同一 attacker-wide abort。 |
| S3 control | current state非18/19时两个 candidate继续。 |
| S4 transformation | 有效 runtime conversion后产生 kind0/effect21时同样 abort；若无法最小构造，显式BLOCKED。 |
| S5 regression | existing Unity compile、full self-check、ledger validator、diff check通过。 |
| S6 boundary | 最高 `RUNTIME_PENDING`；不关闭 C++ trace/Play Mode。 |

## Stop conditions

- 需要向 candidate collect、CPoint/held writer或 scheduler扩张才能构造/实现主 gate；
- target current-frame读取接口不确定且需要新长期数据契约；
- transformation fixture只能靠更改已确认的战斗关系逻辑实现；
- 需要回退任何已批准的 CentralOnly/Texture2DArray/容量/30Hz/FrameInputSet/SoA/pool边界。

## Out of scope

R1-WP02、C++ executable、D-COL-001/002额外重构、D-COL-004/005、所有D-HIT、R5～R8、T8默认
`stage.dat`、服务器、Android、长时间性能与Play Mode。

## 实施进度（2026-08-22）

- 已在 runner `ResolveRuntimeItrForPair(...)` 成功后、legacy observation/disposition/任何 writer之前加入
  C07-C：transformed `runtimeItr.kind == 0 && effect == 21` 且 target `Frame.D.state`为18/19时返回
  existing sequence-break值 `true`。
- exact/shared source-kind0 fixture已覆盖 current state18、state19和ordinary control；每个 case都先冻结
  two-candidate list，再仅改 first target current frame，证明 previous-frame collection filter没有被误当成
  consume-time gate。
- exact/shared source-kind4 / `WeaponCount=1` fixture已确认 runtime conversion为 kind0/effect21并触发同一
  attacker-wide abort，故C07-C放置点有定向覆盖；本包未尝试扩大至kind5 held关系。
- 2026-08-22 04:16 +08:00：现有 Unity Editor（UnityMCP port 6401）force scripts refresh/compile后，
  Console `error CS` 查询为0；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入
  04:16:17 +08:00。
- 因 C++ runtime trace与真实 Play Mode仍未关闭，本包状态保持 `RUNTIME_PENDING`。
