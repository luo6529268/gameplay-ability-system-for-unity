# R8-WP01G — certification synthesis

> 日期：2026-08-23  
> 总登记：`68 D-ID`  
> 结论：`NO UNRESOLVED SOURCE-CONFIRMED UNITY CODE DIFFERENCE FOUND`  
> 边界：`R1-WP02 FULL TRACE BLOCKED / T8 DEFERRED / IL2CPP USER-EXCLUDED`

## 1. Verdict

本轮逐项读取`R1-SOURCE-ALL-DIFF-REGISTER.md`的68个当前D-ID，并结合对应Task、Change Record、
R8-WP01C/D/E证据重新分类。结论是：

1. 当前没有一项能够同时满足“C++ release source已确认差异”“Unity尚未修复”“production可达性已闭合”
   三个条件，因此不存在可直接授权的gameplay脚本修改；
2. 已确认的AI dispatcher/RNG、effective-pic、DAT row、declared-range/partial-clip等代码差异均已关闭；
3. 剩余项主要是Unity joint/Play证据、C++ full trace、或production reachability证据缺口；
4. `UNKNOWN/INFERRED`不是“已确认没问题”，也不是“已确认要修改”；必须先做source/reachability closure；
5. CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× visual scale、fixed-world camera、扩展容量、
   30Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC均保持批准边界；
6. 用户已明确排除IL2CPP后续处理；本synthesis不将其作为gameplay gate或blocker。

该结论只说明“当前登记里没有尚未关闭的source-confirmed代码差异”，不等于完整C++ runtime对齐。

## 2. Exhaustive classification

### A. 限定范围已关闭或已取得明确Unity证据（20）

`D-CAP-001`、`D-INP-002`、`D-INP-010`、`D-TEST-001`、`D-TEST-002`、`D-TEST-003`、
`D-PERF-003`、`D-PERF-004`、`D-PERF-005`、`D-MOV-003`、`D-MOV-004`、`D-COL-001`、
`D-COL-002`、`D-COL-003`、`D-COL-004B`、`D-HIT-001`、`D-HIT-002`、`D-HIT-003`、
`D-HIT-004`、`D-LATE-001`。

边界：

- `VERIFIED`只覆盖各自实际执行或明确定义的子范围；
- `D-HIT-002/003/004`未进入live matrix的kind/type分支不因同一D-ID的部分S4而自动关闭；
- `D-INP-002`、`D-MOV-003`、`D-LATE-001`的S5仍受R1-WP02阻塞；
- test/performance/capacity证据不能单独裁决C++ gameplay。

### B. 代码差异已关闭或已取得高层Unity证据，full closure仍受trace/样本限制（20）

`D-SCHED-001`、`D-SCHED-002`、`D-SCHED-003`、`D-SCHED-004`、`D-INP-007A`、
`D-INP-007B`、`D-INP-008`、`D-INP-009`、`D-LINK-001`、`D-LINK-002`、`D-HOLD-001`、
`D-HOLD-002`、`D-HOLD-003`、`D-CPT-001`、`D-CPT-002`、`D-CPT-003`、`D-CPT-004`、
`D-CPT-005`、`D-OP-001`、`D-RENDER-006`。

下一动作：

- 不重复修改已关闭代码；
- 保留现有S4/focused/self-check证据；
- R1-WP02可用时再提升S5；
- `D-RENDER-006`的authored state8000 live witness只能来自真实数据，禁止改DAT伪造。

### C. 代码已写/已映射，但仍需Unity joint、Play或物理输入证据（19）

`D-SCHED-005`、`D-SCHED-007`、`D-SCHED-009`、`D-SCHED-010`、`D-SCHED-011`、
`D-INP-001`、`D-INP-003`、`D-INP-004`、`D-INP-005`、`D-INP-006`、`D-MOV-001`、
`D-MOV-002`、`D-COL-004`、`D-RENDER-001`、`D-RENDER-002`、`D-RENDER-003`、
`D-RENDER-004`、`D-RENDER-005`、`D-PERF-001`。

下一动作：

- 先建立只认证、不改gameplay的独立Play/physical-input Work Package；
- 若出现first difference，立即退出认证包并建立新的Task/Change Record；
- `D-INP-006`需用户实体键盘/窗口焦点edge或等价真实InputSystem probe；
- `D-COL-004`需production有效geometry，不得用无效DAT frame强造结论；
- render项必须继续走CentralOnly，不恢复Legacy SpriteRenderer owner。

### D. C++ source或production reachability仍为UNKNOWN/INFERRED（7）

`D-SCHED-006`、`D-SCHED-008`、`D-STEP-001`、`D-MOV-005`、`D-COL-005`、
`D-HIT-005`、`D-LIFE-001`。

下一动作：

- 先做只读C++ live-source caller/writer/field closure和Unity静态reachability审计；
- 在authority与production route闭合前禁止修改gameplay；
- `D-COL-005`中特指05B non-character kind1 reachability；05A已有S4；
- `D-HIT-005`不能简单增加CLR/type gate，否则可能丢失C++ generic type3 route；
- `D-LIFE-001`保持批准的dormant-object adapter，除非发现battle-time low-slot writer。

### E. 批准保留的Unity adapter或未来配置决策（2）

`D-SCHED-012`、`D-PERF-002`。

下一动作：

- `D-SCHED-012`保持Authority400诊断、MobileExtended 1000和DesktopExtended扩展容量合同；
- `D-PERF-002`保持当前BruteForce默认，不因synthetic pair reduction直接切Loose Quadtree；
- 任何未来切换都必须有独立配置Record、real workload A/B、parity、fallback distribution和回滚证据。

## 3. Count reconciliation

| 分类 | 数量 |
|---|---:|
| A 限定范围关闭/明确Unity证据 | 20 |
| B 代码差异关闭/高层Unity证据，仍受trace或样本限制 | 20 |
| C 代码已写/映射，Unity joint/Play待证 | 19 |
| D source/reachability UNKNOWN/INFERRED | 7 |
| E 批准adapter/未来决策 | 2 |
| **合计** | **68** |

## 4. Recommended next Work Packages

### R8-WP01G-R01 — R2 scheduler source/reachability closure（推荐第一项）

**Goal**  
只读闭合`D-SCHED-006`两次Z clamp的对象筛选、double/int写入和newborn副作用，以及`D-SCHED-008`
candidate carrier/end-consumption cleanup时点；判断是否存在真正Unity code difference。

**Scope**  
只读C++ release `game_tick.cpp`及直接caller/callee/field定义、Makefile live参与性；只读Unity对应pass、
candidate store与lifecycle实现；不修改脚本。

**Authority / Evidence**  
C++ release live source唯一裁决；R1-SOURCE-003/004/005、R2-PASS-02与现有Unity code仅作映射证据。

**Files likely involved**  
C++ `src/entity/game_tick.cpp`及其candidate/lifecycle调用链；Unity `NTSDBattleTickSystem.cs`、
`SimulationWorld.Passes.partial.cs`、candidate store/runner与lifecycle模块。

**Unknowns**  
两次clamp是否覆盖同一实体集合；integer snapshot写入是否在clamp内；newborn/pending-free是否参与；
candidate carrier究竟在consume end、tick tail还是下一tick reset。

**Deliverables**  
source contract、caller/writer表、Unity映射、first-difference判断、D-SCHED-006/008状态更新；若确认差异，
另建修复Task/Change，不在本包修改。

**Verification**  
release build参与性、所有字段读写方、分支顺序与Unity对应pass闭合；UNKNOWN可以是合法最终结论。

**Stop conditions**  
需要运行/构建/修改C++；需要改变pass ordering或架构；发现R3+差异修复；需要用户批准的范围扩大。

**Out of scope**  
Unity脚本改动、Play Mode、IL2CPP、T8、Android、服务器、R1-WP02替代方案。

### R8-WP01G-R02 — input/motion real-edge joint evidence（后续，需批准）

覆盖C组中的`D-SCHED-005/010`、`D-INP-001/003/004/006`、`D-MOV-001/002`，使用真实InputSystem
edge、窗口焦点与joint landing/held链；只认证，first difference另建修复包。该包属于R3+，开始前等待用户批准。

### R8-WP01G-R03 — remaining collision/render/lifecycle reachability（后续，需批准）

在R01 source结论后处理`D-COL-004/005B`、`D-HIT-005`、`D-LIFE-001`和`D-RENDER-001～005`的
production reachability/Play证据；不得恢复Legacy owner或制造DAT数据。属于R4～R6，开始前等待用户批准。

## 5. Final boundary

- R8-WP01C 01～07已完成其Unity S4限定范围；
- R8-WP01D完成当前资源可取得证据，authored state8000与full trace仍不可得；
- R8-WP01E current-build Editor 1000实体/30FPS/0GC已验证；
- R8-WP01F按用户决定停止IL2CPP后续处理，不构成当前gameplay gate；
- R8整体仍不能宣称完整C++ runtime对齐，因为C/D分类与R1-WP02证据缺口仍存在；
- 下一项推荐为不改代码的`R8-WP01G-R01`，先闭合R2最早依赖的scheduler UNKNOWN。

## 6. 2026-08-23 R01 correction（追加更正）

`R8-WP01G-R01`完成后，上述初次分类被新source closure部分supersede：

- `D-SCHED-006`从D移到C：source合同等价，保留runtime trace/联合证据边界；
- `D-SCHED-008`从D移到新增F：F1/step-wait跳过tail时存在未修复的条件性source-confirmed difference；
- 当前计数为A20、B20、C20、D5、E2、F1，总计仍为68；
- 第1节“没有未修复source-confirmed difference”只代表R01之前的历史综合结论，不再代表当前状态；
- 下一实际修复候选是`R2-CANDIDATE-TAIL-01`，不得把它简化为只清一个count或删除一个cache-end调用。

更正证据见`R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`。

## 7. 2026-08-23 R01B correction（追加更正）

`D-STEP-001`的完整release writer/consumer closure证明A→B→C debug unlock不是不可达注释或debug-only
编译分支，而是BATTLE outer loop的release live physical edge。它从D移到F：

- 当前计数再次更正为A20、B20、C20、D4、E2、F2，总计68；
- 第二个F项是`D-STEP-001 / SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`；
- 是否移植该debug功能必须由用户决定，未批准前既不实现也不擅自标approved omission；
- `R2-CANDIDATE-TAIL-01`必须使用actual tail-skip predicate，不能把所有stepWait都retain。

证据见`R8-WP01G-R01B-d-step-debug-unlock-source-policy-20260823.md`与`R3-STEP-01`Task。

## 8. 2026-08-24 R09 final superseding reconciliation

本文件前述A～F分类是R01/R01B时点的历史快照，已由
`R8-WP01G-R09-final-evidence-reconciliation-20260824.md`取代。R05～R08-R04之后的最终分类为：

- 43项Unity S4/runtime覆盖；
- 5项exact production witness因current DAT/fixture不可得；
- 1项source等价但C++ full trace缺失；
- 9项用户排除或未来替换；
- 1项F7/F8/F9调试功能键policy；
- 3项approved adapter/config decision；
- 6项test/worker/performance事实。

总计68、missing0、extra0、duplicate0。`D-LIFE-001`和`D-RENDER-003`已由R08提升至Unity S4；
`D-SCHED-008`、`D-SCHED-010`和`D-STEP-001`按用户F1/F2决定退出正常战斗backlog但保留source差异历史。
当前没有新的正常战斗production gameplay修复包；R1-WP02 full trace仍BLOCKED。
