# R1 — C++ Release → Unity 全量差异盘点覆盖矩阵

> 建立日期：2026-08-21  
> 状态：COMPLETED（COV-001～006 静态源码盘点已闭合；runtime acceptance pending）  
> 用途：确保 R1 不会只围绕已经暴露的技能现象工作，也不会在上下文压缩后遗失尚未审计的
> C++ release battle subflow。  
> 行为 authority：J:\QQFile\NTSD2.4\ntsd_release 中参与 ntsd_new.exe release 构建的
> live battle source。  
> 证据边界：本矩阵只组织 source contract、Unity source crosswalk 和静态差异登记；不是
> C++ executable trace、Unity Play Mode 验收或 gameplay 修复清单。

## 1. 盘点原则

1. 先从 C++ live caller 进入，再追到被调 helper、字段所有者、重置路径和同 tick 消费者；
   不能按函数名称猜测规则。
2. 每个 C++ 行为点都必须获得一个 Unity 对应项，或明确写为“无对应实现 / UNKNOWN”。
3. Unity 多出的缓存、SoA、worker、pool、central renderer 或 adapter pass 只有在它静态改变
   C++ 规则、时序、字段或可观察输出时，才登记为差异。
4. 任何已确认差异先登记为“待处理（静态确认）”；不得在 R1 中修改 gameplay，也不得把
   静态阅读标成运行时 VERIFIED。
5. 一条主流程必须拆成可独立审计、可独立验收的子流程；不能独立运行的项必须有明确的
   代码级验收和后续 joint fixture，而不是被遗漏。

## 2. 全覆盖路径清单

| Coverage ID | C++ release live path / 领域 | Unity 对照入口 | 责任 Work Package | 当前状态 | 盘点完成的最低条件 |
|---|---|---|---|---|---|
| COV-001 | game_tick：tick header、cooldown、OID maintenance、state maintenance、主 pass 顺序 | SimulationTickDriver、NTSDBattleTickSystem、SimulationWorld passes | R1-SOURCE-001 | 完成（静态） | T00–T18、关键 writes、同 tick consumers 和调度差异均已登记。 |
| COV-002 | main callback / input_handler：human、AI、combo、direct action、F1/F2 | FrameInputSet、input buffer、HumanInput / CharacterInput | R1-SOURCE-002 | 完成（静态） | callback 时点、边沿、held、AI caller 与 F1/F2 返回边界均已映射。 |
| COV-003 | frame_advance / physics：delay、frame、速度、地面、跳跃、landing、death/respawn | FrameAdvance、FrameTick、CharacterMechanics、stage clamp | R1-SOURCE-003 | 完成（静态） | early return、整数同步、落地和 late consumer 均已查到。 |
| COV-004 | collision_collect / collision / hit：candidate、vRest、kind consume、damage、grab、weapon | snapshot、broadphase、candidate runner、interaction/damage writer | R1-SOURCE-004 | 完成（静态 source） | 创建、过滤、顺序、消费、raw field writes、清理和 fallback/optimized boundary 均已登记；runtime/joint fixture 未执行。 |
| COV-005 | cpoint / weapon / held / link / opoint / pool lifecycle | CPointWriter、WeaponSync、held/link passes、opoint queue、pool | R1-SOURCE-005 | 完成（静态 source） | T09/T14/T15/T16、关系写入、生成/可见边界和 reuse reset 均已登记；runtime/joint fixture 未执行。 |
| COV-006 | render handoff / renderer / scene visibility / sort / shadow handoff | BattlePresentation、BattleCentralRenderSystem、CentralOnly、URP command/mesh | R1-SOURCE-006 | 完成（静态 source） | C++ logical render handoff 与 Unity central presentation 的可观察合同、fail-closed gate 与 protected adaptation 均已登记；不回退 Legacy。 |
| COV-007 | cross-package fields、slot/generation、RNG、DAT/stage fixture、acceptance closure | all R1 research / profiles / fixtures | R1-SOURCE-007 | 完成（静态 source inventory closure） | 全部 coverage 状态、D-/A-条目、依赖、最小 fixture 与 R2-R7 owner 已汇总；UNKNOWN 已保留而未伪造为结论。 |

## 3. 每项都必须回答的字段合同

对每一条被审计的 C++ 子流程，研究文档必须显式回答下列问题；没有证据时填写 UNKNOWN：

| 合同字段 | 需要记录的内容 |
|---|---|
| Authority coordinate | C++ 文件、函数、调用方、字段定义和 release build 参与性。 |
| Entry / precondition | 调用发生在哪个 game_tick pass，实体类型、frame/state、link、held、team、slot、DAT 或 input 前置条件是什么。 |
| Read order | 读取 current frame、prev_frame、prev_frame2、integer / double position、rest、RNG 的顺序。 |
| Mutation order | 所有 frame、velocity、position、HP/PP、link/holder/target、candidate、pool/visibility 写入的顺序。 |
| Same-tick consumers | 每项写入在本 tick 之后被哪个 pass/helper 再读取。 |
| Unity mapping | 对应 Unity 文件、方法、writer/store；须分别说明 fallback 与 optimized/SoA writer 的实际进入边界。 |
| Difference classification | 完全映射、adapter、待处理（静态确认）、可达性待验、UNKNOWN 或不适用（已批准 Unity 扩展）。 |
| Minimal acceptance | 后续需要的初始 slots、DAT 夹具、输入 journal、tick 范围、字段断言和用户可观察结果。 |
| Dependency / owner | 需要先完成的 R1 package，以及后续修复批次和 Change ID（只有开始改代码后才填写）。 |

## 4. 已批准 Unity 适配边界

下列实现差异必须保留，不能以“与 C++ 不同”直接登记为待修复：

- CentralOnly、BattleCentralRenderSystem、central command/descriptor、Texture2DArray/atlas、
  dynamic Mesh/quad 与 URP；
- MobileExtended 的 1,050 initial slot / 1,000 active 合同，DesktopExtended 的动态增长；
- 30 Hz SimulationTickDriver、FrameInputSet、slot/generation、SoA/ECS store、object pool、
  worker 和战斗期零 GC 目标；
- T8 默认 stage.dat 的资产部署暂缓。

对这些边界的审计目标是“是否保持 C++ 的战斗逻辑真相和可观察战斗表现”，不是回退其技术实现。

## 5. 全量盘点完成门槛

R1-SOURCE-007 只有在下列条件同时成立时才可以宣告全量盘点完成：

1. COV-001 至 COV-006 均有对应 C++ source contract 和 Unity source crosswalk；
2. 每条已确认差异均已在 R1-SOURCE-ALL-DIFF-REGISTER.md 登记，具有 source 坐标、状态、
   最小验收和后续 owner；
3. 每个 UNKNOWN 都有明确缺失证据和最小补证路径，不能用历史 C# 或经验填补；
4. 所有跨模块依赖已形成有向修复批次；不会让 R2 在未关闭 producer/consumer 合同前改局部；
5. 每个 R2 代码修改能够在改动前建立 Change Record、在改动后留下验证结果；
6. 文档明确区分“静态确认”“待测试”“运行时验收”和“已对齐”，不把当前静态盘点写成完成对齐。

### 2026-08-21 closure record

上述六项已在静态 source inventory 范围内完成：

1. COV-001～006 分别有 C++ contract 与 Unity crosswalk；
2. D-/A-项统一登记在 R1-SOURCE-ALL-DIFF-REGISTER；
3. 依赖图、future repair batches 和每项分层验收矩阵已写入 SOURCE-007 artifacts；
4. R1-WP02 full trace 仍为 BLOCKED，所有 runtime / Play Mode / GPU / asset binding 的 UNKNOWN 均保留；
5. 该 closure 不授权 R2 code，须等待用户确认。

## 6. 当前明确不做的事

- 不改 C++ release source、build、binary、resource、configuration 或 authority directory；
- 不运行 C++ executable、Unity Play Mode、compile、self-check、长性能或 1000 AI；
- 不实现 trace、comparator、fixture、replay harness；
- 不改 Unity gameplay、pass order、candidate、opoint、CPoint、weapon、held/link 或 renderer；
- 不因盘点发现的一个差异立即开始修复。
