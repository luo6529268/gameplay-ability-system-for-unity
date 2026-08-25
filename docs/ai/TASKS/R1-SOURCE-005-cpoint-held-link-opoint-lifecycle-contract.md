# R1-SOURCE-005 — CPoint、持有、link、opoint 与实体生命周期源码合同

> 建立日期：2026-08-21  
> 状态：COMPLETED（静态 source inventory；runtime / joint fixture 待后续阶段）  
> 类型：只读 C++ / Unity source 审计；不修改任何 gameplay。

## Goal

以 C++ release game_tick 的 T09、T14、T15、T16 及其 live caller 为主线，闭合 CPoint、
持有武器、抓取关系、正/负 link、opoint 生成、队列可见边界、销毁/复用和 relation reset 的
行为合同；建立 Unity structural writer、held/link pass、weapon sync、opoint queue 与 pool 的
精确 crosswalk，并把影响 battle outcome 的差异登记到全量台账。

## Authority / Evidence

- 唯一行为 authority：J:\QQFile\NTSD2.4\ntsd_release 中实际参与 release 的 live source；
- 首先从 src/entity/game_tick.cpp 的 T09 / T14 / T15 / T16 调用点进入，再追到
  src/entity/cpoint.cpp、weapon.cpp、frame_advance.cpp、physics.cpp、entity lifecycle/
  spawn helper 与其字段所有者；
- collision.cpp、hit.cpp 仅用于闭合 CPoint / held / link 的 producer 或 consumer，不覆盖
  R1-SOURCE-004 已定义的 candidate/hit 主审计职责；
- Unity 当前代码和历史 C# 只用于定位现状，不能定义 authority。

## Scope

### C++ release source

- negative held 的两次 pass、其对象筛选、slot 顺序、时点和 early-return；
- CPoint kind、injury/cover、holder/caught/target、link_state 的每个写入和解除边界；
- T14 的 CPoint / weapon synchronization，含 held weapon attach、方向、位置、frame 和
  relation writer；
- T15 positive link validation、解除、captured relation 维护和与 input/collision 的边界；
- opoint 的生成条件、slot 分配、active/newborn 时间、初始 frame/velocity/team/owner、
  render visible 和 collision eligible 的最早 tick；
- delete/death/expire、pool/reuse 前 reset、slot order、generation（若 C++ 无 generation
  则记录为 Unity adapter）；
- 所有上述写入在同 tick 被 frame/candidate/hit/render 哪些消费者读取。

### Unity source crosswalk

- SimulationWorld 的 Held、CPoint、WeaponSync、positive/negative link、opoint/pool、
  structural writer 与 late entity pass；
- LF2Entity、LF2Character、LF2WeaponBase、LF2OtherObject、LF2SpecialAttack 的 relation、
  frame、runtime reset 和 visual handoff；
- object pool、slot/generation、newborn / pending structural state、central presentation
  publish boundary；
- fallback、optimized/SoA、worker 的 writer entry boundary。

## Required Deliverables

1. docs/ai/RESEARCH/R1-SOURCE-005-cpp-cpoint-held-link-opoint-lifecycle-contract.md；
2. docs/ai/RESEARCH/R1-SOURCE-005-unity-crosswalk-and-diff.md；
3. 更新 docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md；
4. 更新 docs/ai/STATE.md，必要时更新重新对齐总计划；
5. docs/ai/HANDOFFS/HANDOFF-R1-SOURCE-005-cpoint-held-link-opoint-lifecycle.md；
6. 不创建 Change ID，因为本 Work Package 不改脚本。

## Static Acceptance Contract

完成前必须能以 source 坐标回答：

1. C++ 两轮 negative held 是否都存在、各自处理哪些实体、彼此之间允许哪些 mutation；
2. CPoint/WeaponSync 位于 object collision 后时，所有被该时点影响的 field consumers 是谁；
3. 正 link、负 link、holder、target、caught index 的 source of truth、写入者、解除者、
   reset 时机和允许的非法/中间态；
4. opoint 从 DAT 发起到“可被碰撞 / 可显示 / 可再次被更新”的准确 tick 边界；
5. Unity slot/generation 和 pool 是保持 C++ slot order 的 adapter，还是造成可观察时序差异；
6. R1-SOURCE-004 已登记的 D-COL-002、D-HIT-002 与 D-SCHED-001～004、D-INP-001、
   D-MOV-002～004 的具体依赖是否闭合；
7. 每一条已确认差异都有最小的 slot、frame、link、DAT、input/tick 和字段验收合同。

## Known Dependencies / Unknowns

- R1-SOURCE-004 的 candidate consume / caught-cpoint 合同必须被作为 consumer input，不能
  在 SOURCE-005 中重新定义 collision authority。
- 当前未知 C++ opoint/newborn 的全部 active/visible/collision 边界、CPoint direct-frame 的
  wait-counter 语义、pool reset 的每个字段和 Unity worker writer 是否会改变时序。
- 若继续追踪发现其他 source family 才能闭合，先记录其依赖；不得用旧 C# 结论替代 C++。

## Stop Conditions

- C++ live caller 到字段所有者无法闭合；
- 必须运行、copy、hook、modify、rebuild、instrument C++ release runtime；
- 必须修改 Unity gameplay、pool、pass order 或 renderer 才能继续；
- render handoff 成为主要未闭合点：登记并转交 R1-SOURCE-006；
- 用户提出新的 Change Request。

## Out of Scope

- 不改 C++ / Unity gameplay、CPoint、weapon、held/link、opoint、pool 或 scheduler；
- 不运行 C++ executable、Unity compile、self-check、Play Mode、trace 或性能测试；
- 不实现 input journal、fixture、replay、comparator 或 R2；
- 不回退 CentralOnly、Texture2DArray、dynamic Mesh、URP、容量 profile、SoA/ECS 或 worker；
- 不处理 T8 默认 stage.dat 部署。
