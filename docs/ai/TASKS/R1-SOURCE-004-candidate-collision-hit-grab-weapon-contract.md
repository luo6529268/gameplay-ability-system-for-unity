# R1-SOURCE-004 — 候选、碰撞、命中、抓取与武器交互源码合同

> 建立日期：2026-08-21  
> 状态：COMPLETED（静态 source；runtime / joint fixture 待后续阶段）  
> 类型：只读 C++ / Unity source 审计；不修改任何 gameplay。

## Goal

以 C++ release 的 T10 / T11 / T13 为起点，闭合 candidate collect、bdy/itr 过滤、
pair 扫描顺序、vRest、candidate carrier、character/object consume、伤害/硬直/击飞、
抓取及武器命中的 source behavior contract；然后建立 Unity snapshot、broadphase、
candidate list、consume runner 和 hit resolver 的精确 crosswalk，并登记所有可确认差异。

## Authority / Evidence

- 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 中实际参与 release 的 live source；
- 权威入口：`src/entity/game_tick.cpp:1645-1656,1818-1822`，
  `src/entity/collision_collect.cpp`、`collision.cpp`、`hit.cpp`、
  `entity_collision.cpp` 以及实际调用到的 field/helper；
- Unity 当前实现只用于定位现状，不定义正确性；
- 旧 C#、Unity self-check、Authority400、shadow compare、fast path、checksum 和性能资料只可作为
  历史辅助或回归资料。

## Scope

- C++：
  - T10 的 `prev_frame2` write、candidate collect、pair / slot order；
  - T11 character collision consume；
  - T13 object collision consume；
  - bdy/itr 建立、过滤、same-team、state/effect、facing、vRest/arest、candidate store、
    damage / hit-state / knockback / grab / weapon side effect；
  - candidate carrier 的 clear / reuse / end-consumption 时点。
- Unity：
  - `CaptureCollisionFrameSnapshotsAll`、`TickCollisionPairVRestAll`、
    `CollectCollisionCandidatesAll`、`PostInteractionTickAll`、
    `ObjectInteractionTickAll`、`EndCollisionCandidateConsumption`；
  - `BruteForceSceneQuery`、loose quadtree/broadphase adapter、candidate sequence runner、
    character/weapon/special hit resolver 和 runtime rest store；
  - fallback/optimized path 的 input、output、ordering 和 mutation boundary。
- 记录每一条差异的 C++/Unity source 坐标、前置状态、slot/order、字段写入、后续 fixture、
  R2/R4/R5 依赖和验收标准。

## Out of Scope

- 不修改 C++ / Unity gameplay、broadphase、candidate、hit、grab、weapon 或 pass order；
- 不运行 C++ executable、Unity compile/self-check/Play Mode、trace、性能或 1000 AI；
- 不把 CentralOnly、Mesh、Texture2DArray、URP、profile capacity、worker、pool 或 ECS
  writer 回退为 Legacy；
- 不实现 trace/comparator/replay/fixture，不开始 R2/R3/R4/R5；
- 不处理 T8 默认 `stage.dat` 部署。

## Required Deliverables

1. `docs/ai/RESEARCH/R1-SOURCE-004-cpp-collision-hit-contract.md`；
2. `docs/ai/RESEARCH/R1-SOURCE-004-unity-collision-crosswalk-and-diff.md`；
3. 更新 `R1-SOURCE-ALL-DIFF-REGISTER.md`、`STATE.md`，必要时更新 plan；
4. `docs/ai/HANDOFFS/HANDOFF-R1-SOURCE-004-candidate-collision-hit.md`；
5. 不创建 Change ID，因为本 Work Package 不改脚本。

## Static Acceptance Contract

- 能从 C++ source 说明 candidate 的创建、顺序、去重、过滤、消费及清理分别发生在何处；
- 能区分 “Unity 多一个 adapter pass” 与 “它改变 C++ 可观察结果的静态差异”；
- 对每一项差异给出最小 state、tick、slot、bdy/itr 与字段验收表；
- D-MOV-002 的 `Frame.PN` / attacking 提前写入是否影响 candidate/hit filter 必须得到
  `VERIFIED（source）`、`INFERRED` 或 `UNKNOWN` 三者之一，不能略过；
- 不得将静态阅读写成 runtime / trace VERIFIED。

## Stop Conditions

- C++ live source 调用链无法闭合；
- 要继续必须运行、hook、复制、重建、插桩或修改 C++ Release runtime；
- 要继续必须修改 Unity gameplay 或移动 long-term pass order；
- first difference 指向 CPoint/held/opoint/lifecycle，记录依赖后转交 R1-SOURCE-005；
- 用户提出新的 Change Request。

## Closure

- 已在不修改 gameplay、未运行 C++ executable / Unity 的条件下完成 candidate collect、Loop1 /
  Loop2 consume、runtime ITR、kind dispatch、typed hit writer 与 Unity optimized/fallback
  consumer boundary的静态审计。
- CPoint / held / link / opoint / identity / pool 的 downstream consumer 不是遗漏，而是显式
  移交 R1-SOURCE-005；render presentation 移交 R1-SOURCE-006。
- 所有已确认静态差异、adapter、UNKNOWN 和最小 fixture 已写入对应 research、总登记册和
  SOURCE-004 handoff。不得把本 closure 写成运行时或完整战斗对齐。
