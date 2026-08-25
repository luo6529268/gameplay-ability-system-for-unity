# R8-WP01G-R02 — normal combat four-difference closure

> 日期：2026-08-23  
> 状态：`COMPLETE TO AVAILABLE EVIDENCE / RUNTIME PENDING`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source（只读）  
> D-ID：`D-MOV-005`、`D-COL-005B`、`D-HIT-005`、`D-LIFE-001`

## Goal

只处理用户批准的四项正常战斗风险，逐项闭合 C++ Release live source、Unity production route、当前数据可达性与必要的最小修复：

1. `D-MOV-005`：state2000 facing / 状态移动表现；
2. `D-COL-005B`：non-character kind1 selector、generic grab 与 weapon pickup 分流；
3. `D-HIT-005`：current-DAT target dispatch 与 CLR shell priority；
4. `D-LIFE-001`：oid7/8 merge partner 的 dormant、slot、split 与 cleanup。

本包完成前不进入其他差异、R8 最终综合、服务器、IL2CPP、T8 或渲染架构工作。

## Scope

- 只读核对 C++ Release Makefile、live caller/callee、字段读写和通用 Entity 规则；
- 核对 Unity production caller、resolver/writer、current-DAT gate、slot/generation 与数据资产；
- 每项给出 `SOURCE EQUIVALENT`、`APPROVED UNITY ADAPTER`、`PRODUCTION UNREACHABLE`、`SOURCE-CONFIRMED DIFFERENCE` 或 `UNKNOWN`；
- 仅对 `SOURCE-CONFIRMED DIFFERENCE` 建立独立 Change Record 后修改 Unity；
- 对修改项执行 compile、focused test、full `BattleRuntimeSelfCheck` 与可实施的定向 Play Mode；
- 将结论同步到 all-diff register、STATE、主计划和 handoff。

## Authority / Evidence

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 正式 release live source；
- Unity source、DAT、self-check、Play Mode 只用于实现映射与回归证据；
- C# 历史工程不得单独裁决；
- C++ authority 目录保持只读，不运行、构建、修改、复制、插桩或写入。

## Files likely involved

### C++ read-only

- `src/entity/frame_advance.cpp`
- `src/entity/collision_collect.cpp`
- `src/entity/collision.cpp`
- `src/entity/hit.cpp`
- `src/entity/game_tick.cpp`
- `src/core/main.cpp`
- `include/game_world.h`
- `Makefile`

### Unity inspection / possible repair

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
- current interaction / hit / lifecycle resolver and ECS writer files discovered by symbol tracing
- focused tests under `Assets/NTSD/Scripts/Test/` and `Assets/NTSD/Scripts/Test/Editor/`

## Unknowns

- current production data 是否存在 non-character `itr kind1` 及其 key producer；
- CLR shell 与 current-DAT 不一致的 target 是否会进入正式 candidate/consume；
- merge dormant partner 是否在当前扩展容量、pool 和 split 流程中产生可观察差异；
- future/mod DAT 是否会让 type0 state2000 进入 exact character route。

## Deliverables

1. 四项逐项 source/reachability 报告；
2. 必要时每项独立 Task/Change Record 与最小 Unity 修复；
3. focused fixture 与实际验证记录；
4. all-diff register、STATE、主计划和 handoff 更新；
5. 四项最终状态表，明确已写、编译、自检、Play 与仍待证据。

## Verification

- source caller/callee/field crosswalk；
- Unity production route 与 DAT inventory；
- 脚本有改动时：fresh Unity compile 0 error；
- focused EditMode/self-check；
- 可构造时执行定向 Play Mode，并记录 cleanup；
- `Tools/Validate-ChangeLedger.ps1`；
- `git diff --check`。

## Stop conditions

- 权威规则无法从 C++ Release live source闭合；
- 需要修改 C++、pass ordering、长期架构或受保护 Unity adapter；
- first difference 指向四项以外模块；
- 修复需要用户未批准的资源、T8、IL2CPP、服务器或渲染范围；
- 编译、自检或定向验证出现无法在本项内解释的失败。

## Out of scope

- F1/F2 debug step、A→B→C debug unlock及其 candidate-tail差异；
- 其他 D-ID 与 R8 最终综合；
- T8 默认 `stage.dat`、Android、IL2CPP、服务器；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× visual scale、fixed-world camera、扩展实体容量、30Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC 的重构或回退。

## Authorization

用户于 2026-08-23 明确要求先处理以上四项，再讨论后续工作；本合同据此进入 `IN_PROGRESS`。

## Final result

| D-ID | 结果 | 自动证据 | 未关闭边界 |
|---|---|---|---|
| `D-MOV-005` | exact state2000按Vx写朝向已补齐；`RUNTIME_PENDING` | compile0、focused 1/1、full self-check PASS | current type0 production DAT不可达；C++ trace blocked |
| `D-COL-005B` | kind1 generic key/grab已补齐；`RUNTIME_PENDING` | compile0、actual weapon attacker self-check、full PASS | current DAT `itr kind1=0`，无production Play |
| `D-HIT-005` | 四attacker current-DAT-first dispatch已补齐；`RUNTIME_PENDING` | compile0、focused 178/178、full PASS | shell/DAT mismatch production Play不可得；C++ trace blocked |
| `D-LIFE-001` | 现有dormant-slot方案确认为批准adapter；无需代码修改；`RUNTIME_PENDING` | focused 32/32、七组OID5152 full self-check PASS | 真实production Play与C++ trace未取得 |

四项已按本合同处理到当前可获得的最高证据层。本包不宣称完整C++ runtime对齐；不自动进入任何后续D-ID。
全局Change Ledger validator因任务外`WEB-CADENCE-001`记录/脚本差异失败，本包没有改动该用户工作。
