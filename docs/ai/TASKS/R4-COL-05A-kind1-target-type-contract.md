# R4-COL-05A — kind1 non-character target consume

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — source、最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-005A`。  
> 前置调查：`RESEARCH/R4-COL-05A-kind1-target-type-preflight-20260822.md`。

## Goal

让已由正式 frozen candidate collect记录的 kind1 non-character target进入与 C++ `collision.cpp:921-993`
一致的通用 grab writer；kind3 / kind8的 Character-only gate必须保持。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Simulation/Ecs/BattleInteractionWriter.cs`
   - `TryApplyGrab` 的 victim type gate；仅让 kind3继续要求 Character。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - kind1 character attacker → non-character target frozen candidate consume正例；
   - kind3同类 target rejection负例。
3. 关联 Change Record、ledger、STATE、差异登记、主计划与 handoff。

禁止：

- 修改 `BruteForceSceneQuery`、kind1 nearest/RNG、weapon/special attacker dispatch、`HandlePreInteractionKind1`、
  pickup kind2/7、CPoint、held/link、opoint、scheduler、DAT/资源、render、C++ authority；
- 合并 `D-COL-005B` non-character attacker / weapon kind1可达性。

## Authority / Evidence

### VERIFIED

- C++ `collision_collect.cpp:264-335`和`collision.cpp:250-263`只给 kind3/8加 target `obj_type==0` gate；
- C++ `collision.cpp:921-993`的kind1 case随后写通用 Entity字段，无 kind1 target type rejection；
- Unity `BruteForceSceneQuery.ItrAllowed`已是kind3/8-only gate；
- Unity `BattleInteractionWriter.TryApplyGrab`错误地把kind1/3一并限制为Character；
- 当前 character / shared-character / special kind1入口都委托到该 writer。

### UNKNOWN

- C++ runtime trace、真实DAT/Play Mode；
- weapon/special non-character attacker的kind1 candidate可达性和pickup关系，明确归 D-COL-005B。

## Deliverables

1. kind1不再被 common writer按 target Character type提前拒绝；
2. kind3 non-character target保持拒绝；
3. candidate sequence focused fixture、Unity compile、full self-check、ledger validator和diff check真实通过；
4. 文档留痕完整，最高状态不超过 `RUNTIME_PENDING`。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 | C++ type gate / case1 writer及Unity producer/consumer crosswalk复核。 |
| S1 | kind1 overlapping non-character target被frozen并消费，写 catcher/caught、frame、duration、fall。 |
| S2 | kind3同类 target不被记录或不被消费，且不写关系字段。 |
| S3 | Unity compile=0 error、full self-check PASS、`pwsh` ledger validator和`git diff --check`通过。 |
| S4 | 仅 `RUNTIME_PENDING`；C++ trace / Play Mode / D-COL-005B未关闭。 |

## Stop conditions

- 需要修改 selector、RNG、weapon/special attacker或pickup才能让本 character-attacker fixture通过；
- generic writer对 non-character target缺少 C++ case1所需字段/数据契约；
- source发现C++ target type gate在未读 live caller中覆盖此 case；
- 要求变更DAT、C++、pass ordering或已批准Unity adapter。

## Out of scope

`D-COL-005B`、D-HIT、R5～R8、T8、C++ executable、Unity Play Mode、服务器、Android、性能和render。

## 实施进度（2026-08-22）

- 已将 `BattleInteractionWriter.TryApplyGrab` 的 type gate收窄为 `kind == 3`；kind1保留 method-kind、
  raw-frame、position、relation、duration、fall与snapshot writer。
- self-check新增正式 frozen candidate链矩阵：character attacker + LightWeapon-type target的kind1记录1个
  candidate且消费后写 frame297/130、caught/catcher、duration300、fall0；kind3同类 target记录0个candidate且
  保持原 frame/relation/fall。
- 第一次 Unity compile报告 `BattleRuntimeSelfCheck.cs(14735,50) CS0165`（short-circuit `&&` 下的
  `out candidates` 可能未赋值）；已在测试局部变量处显式初始化为 `default`，没有改变 battle runtime。
- 重新 refresh/compile后 Console `error CS`=0；现有 Unity 2022.3.62f3 / UnityMCP port 6401的 full
  self-check结果 `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 05:05:17 +08:00。
- 文档最终更新后仍需运行 `pwsh` ledger validator与`git diff --check`；C++ trace / Play Mode / D-COL-005B
  保持未关闭。
