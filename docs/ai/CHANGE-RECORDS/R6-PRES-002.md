# R6-PRES-002 — shadow current DAT identity

<!-- CHANGE-RECORD
id: R6-PRES-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:11-35;src/render/renderer.cpp:517-531
evidence: SOURCE-CURRENT-CHAR-DATA-OID-VERIFIED / UNITY-OBJECTID-GATE-DIFFERENCE-FIXED / FRESH-UNITY-COMPILE-PASS / FULL-SELF-CHECK-P7-MATRIX-PASS / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 状态：`RUNTIME_PENDING`

## 1. Authority / requirement

C++ `Renderer::draw_shadow`以当前`char_data->oid`裁决223/224。Unity snapshot已经有
`CurrentDatObjectId`，但shadow gate使用shell `ObjectId`。本Record只修这一处字段选择。

## 2. Unity before

- shadow gate：`entity.ObjectId != 223 && entity.ObjectId != 224`；
- body resource：`entity.VisualDataId`；
- existing P7 fixture把ObjectId/currentDAT反向构造，并断言旧ObjectId gate。

## 3. Planned changes

| 文件 | 符号 | before | after |
|---|---|---|---|
| `BattlePresentationShadowBuild.cs` | `BuildCommands` shadow gate | shell ObjectId | CurrentDatObjectId |
| `BattleRuntimeSelfCheck.cs` | P7 shadow identity matrix | old Unity gate expected | C++ current DAT expected |

## 4. Protected boundaries

- 不改body/sprite lookup、snapshot schema、sort、position、camera、mesh、shader、catalog；
- 不改gameplay、C++ authority、scene、DAT、T8或扩展容量；
- 不回退CentralOnly/Texture2DArray/dynamic Mesh/URP或1.5 scale。

## 5. Acceptance

- actual shell223/current7300 draws shadow；inverse shell7300/current223 hides；shell224/current7300 draws；
- existing state/link/hit-stop/ordering/checksum assertions继续通过；
- compile/full self-check/validator/scoped diff PASS；
- PlayMode/C++ trace未取得时最高RUNTIME_PENDING。

## 6. Actual changes / verification

| 文件 | 实际改动 | 当前状态 |
|---|---|---|
| `BattlePresentationShadowBuild.cs` | `{223,224}` shadow gate改读`CurrentDatObjectId`。 | fresh compile/self-check PASS |
| `BattleRuntimeSelfCheck.cs` | actual shell223/current7300与shell224/current7300改为draw；shell7300/current223改为hide。 | P7 matrix PASS |

Fresh evidence：source `18:15:26/29` < Assembly-CSharp `18:16:56` < result `18:18:10`；
Tundra build success 6.02s、filtered `error CS/Compilation failed=0`、full self-check=`PASS`。
17:49旧结果未被用于证明本改动。

## 7. Risks / pending

- 只改变synthetic/future mismatched shell/current-DAT的shadow gate；formal identity相等路径行为不变；
- checksum isolation assertion随full self-check通过；
- 仍缺C++ runtime trace与真实Play Mode/GPU可见验证，最高`RUNTIME_PENDING`；
- D-RENDER-001/002/005保持独立。

## 8. Rollback

只回滚本Record列出的两处脚本diff及其关联文档，不触碰用户工作树其它修改。
