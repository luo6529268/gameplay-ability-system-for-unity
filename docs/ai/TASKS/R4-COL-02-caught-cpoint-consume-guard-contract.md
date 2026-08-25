# R4-COL-02 — C++ caught-cpoint `hurtable` current-candidate skip

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、Unity compile、full self-check已通过；C++ trace / Play Mode待补。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-002`。  
> 前置调查：`RESEARCH/R4-COL-02-caught-cpoint-consume-preflight-20260822.md`。

## Goal

让 Unity shared frozen-candidate runner按 C++ `collision.cpp:69-79` 处理 caught-cpoint protection：
在 vrest recheck和C07-A `HitConfirm2` whole-attacker abort之后，若当前 target的 prev2 cpoint.kind为2、
其 active catcher与 attacker slot的 caught relation匹配、且 catcher prev2 `hurtable`缺失或为0，则只跳过
当前 candidate；不执行 runtime ITR replacement、disposition或任何 writer，且继续该 attacker后续 candidate。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
   - 复用已有 `BruteForceSceneQuery.IsReleaseConsumerPairBlocked(...)`；
   - 仅在 C07-A 后、runtime ITR replacement 前插入 current-candidate `return false`。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 新增 exact/shared kind0 two-candidate continuation fixture；
   - 新增 `hurtable=1` positive control和kind6 no-hit-confirm writer check；
   - 注册到 full self-check。

禁止：

- 修改 C++、`TargetBeingCaughtPairBlocked`、candidate collection、scheduler、CPoint writer、held/link、
  opoint、renderer、容量、pool/worker；
- 同包处理 D-COL-003～005、D-HIT-001～003、R5+、T8、服务器、Android或性能重构。

## Authority / Evidence

### VERIFIED — C++ release source

- `collision.cpp:57-80` 的 C07-A / C07-B order与 skip/abort范围；
- `collision.cpp:1253-1258` 的 `next_pair_outer` / `next_attacker` label位置；
- `include/entity_runtime_groups.h:63-64,147-155` 的 catcher/caught field mapping。

### VERIFIED — Unity source

- `TargetBeingCaughtPairBlocked` 已实现C++字段合同；
- active catcher查询等价于 `FindEntityByRuntimeSlotCurrent` 的 active-only return；
- unified runner没有调用这一 helper，且现有 direct query path不能代表所有 runner dispositions。

### UNKNOWN / boundary

C++ runtime trace、真实 cpoint skill Play Mode仍未关闭；本包不以旧C#或Unity current behavior覆盖C++ source。

## Files likely involved

| 文件 | 责任 |
|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` | C07-B shared consume gate。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | frozen-candidate current-skip / continuation fixture。 |
| `docs/ai/CHANGE-RECORDS/R4-COL-002.md` | 改动、验证和回滚记录。 |

## Deliverables

1. 一处 shared runner C07-B gate；
2. exact/shared continuation、positive control、kind6 writer negative fixture；
3. Unity scripts refresh/compile、full self-check、ledger validator、diff check真实证据；
4. Change Record、ledger、STATE、diff register、主计划和 handoff更新。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 source order | C07-B位于 existing C07-A后、runtime ITR/disposition前；其返回只继续下一candidate。 |
| S1 exact | kind0 first caught target不写HP/vrest，second ordinary target仍受击。 |
| S2 shared | current-DAT fallback得到同一 first-skip / second-hit结果。 |
| S3 conditional | `hurtable=1`不被误拦截，kind0可正常受击。 |
| S4 direct writer | kind6 blocked时不写 `HitConfirmCounter`。 |
| S5 regression | existing Unity Editor compile、full self-check、ledger validator、diff check通过。 |
| S6 boundary | 最高 `RUNTIME_PENDING`；Play Mode/C++ trace不关闭。 |

## Stop conditions

- focused fixture证明现有 helper字段语义不等价于C++，且修复需触及helper之外的CPoint/relationship writer；
- 为构造 candidate必须改变 scheduler、candidate collection或长期架构；
- C07-B放置后使normal `hurtable=1` control失败且需要扩展到D-COL-003+；
- 需要回退任何批准的 Unity render/capacity/30Hz/FrameInputSet/SoA/pool边界。

## Out of scope

R1-WP02、C++ executable、D-COL-001已完成代码闭环后的额外重构、D-COL-003～005、所有D-HIT、R5～R8、
T8 default `stage.dat`、服务器、Android、长时间性能与Play Mode。

## 实施进度（2026-08-22）

- 已在C07-A后、runtime ITR replacement前调用现有 `IsReleaseConsumerPairBlocked(...)`，blocked时返回
  `false`，保持 current-candidate skip。
- 已新增 exact/shared × kind0 blocked/hurtable continuation 与 kind6 blocked/hurtable direct-writer matrix。
- 2026-08-22 04:01 +08:00：现有 Unity Editor（UnityMCP port 6401）force scripts refresh/compile 后，
  Console `error CS` 查询返回 0；`Temp/NTSD_BattleRuntimeSelfCheck.result` 写入 `PASS`。
- `Tools/Validate-ChangeLedger.ps1` 于写入后的复核通过（16 records / 13 governed code files）；
  `git diff --check` 退出为 0（仅现有 CRLF 提示）。
- 所以本包达到 `RUNTIME_PENDING`，不将 Unity focused regression 夸大为 C++ runtime trace 或 Play Mode 对齐。
