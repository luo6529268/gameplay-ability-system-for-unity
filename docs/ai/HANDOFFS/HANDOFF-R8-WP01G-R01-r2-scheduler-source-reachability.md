# HANDOFF — R8-WP01G-R01 R2 scheduler source/reachability closure

> 日期：2026-08-23  
> 状态：`COMPLETE / READ-ONLY / D-SCHED-008 REPAIR PENDING`

## Completed

- 只读闭合C++ release两次character Z clamp的caller、资格、slot顺序与Z/ZInt写入；
- 只读闭合C++ candidate carrier从上一tail、collect、两次consume到本ticktail的完整读写集合；
- 对照Unity StageZ ECS pass、RuntimeSlotTable、current-DAT gate、candidate store/range/list与entity tail；
- `D-SCHED-006`更新为source等价并保留批准容量adapter；
- `D-SCHED-008`确认F1/step-wait下的条件性未修复差异；
- 建立下一修复Task `R2-CANDIDATE-TAIL-01-step-wait-carrier-retention.md`；
- 更新all-diff register、WP01G synthesis correction、STATE、总计划与R8 orchestration；
- Unity/C++ gameplay、scene、config、test脚本均未修改，未运行任何C++ executable或Unity Play/build。

## Current truth

### D-SCHED-006

`SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING`。

- C++和Unity都在相同两个scheduler位置读取current character DAT；
- 都按slot升序写double Z和截断ZInt；
- pending/dormant对应C++ inactive；
- Mobile/Desktop高槽继续执行clamp是批准的扩展容量合同，不得回退到400；
- full C++ runtime trace仍BLOCKED，因此不是runtime VERIFIED。

### D-SCHED-008

`SOURCE-CONFIRMED CONDITIONAL DIFFERENCE / UNFIXED`。

- normal completed tick：consume-end到tail无candidate reader，结果等价；
- F1/step-wait：C++ render后return并跳过tail，candidate carrier保留到下一tick；
- Unity已在object consume后EndConsumption，下一collect又ResetCandidateCollectionState，故丢失应保留carrier；
- 差异可能影响下一tick candidate count、20-cap、ordinal、nearest tie/RNG及consume结果。

### D-STEP-001（R01B后续只读closure）

`SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`。

- C++ A→B→C down-edge在release BATTLE outer loop写flag1/progress3；
- flag1+F1 wait仍skip input，但继续postprocess/late/tail；
- Unity缺flag/progress/deterministic debug command，恒为flag0行为；
- 后续`R3-STEP-01`必须由用户先选“移植”或“明确省略为approved debug policy”；
- candidate retention改用actual tail-skip predicate，避免未来flag1路径错误retain。

## Required next package

`R2-CANDIDATE-TAIL-01 — step-wait candidate carrier retention`。

只读实施预检已排除“只移动End”“只保留count”“跨tick保留旧Dictionary/List”三种不完整方案。推荐
query-owned fixed-slab retention store：pause capture当前ordered carrier，释放本tickadapter storage，下一collect
按current producer mode先seed再append，真实tail清理；slab必须由`PrepareBattleCapacity`预分配并覆盖
formal fallback/restart、store-only/oracle切换、attacker generation与target-slot current occupant。

开始任何脚本改动前必须：

1. 读取该Task Contract；
2. 新建独立Change Record并登记Ledger/STATE/current handoff；
3. 先决定store visibility与storage lifetime的最小分离方案；
4. 同时覆盖normal、single pause、multi-pause、resume、20-cap、HitConfirm2、RNG和0B；
5. 不只清`HitCandidateCount`，不只删除`EndCollisionCandidateConsumption`。

该包是跨scheduler/query/store/tail/test的多文件脚本修改，按根规则当前停在`APPROVAL PENDING`。用户若批准，
下一步先创建`R2-CANDIDATE-TAIL-001` Change Record并同步Ledger/STATE，再修改脚本。

## Protected boundaries

- C++ authority只读，不运行/构建/复制/修改/写入；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed camera不动；
- Authority400仅诊断，MobileExtended1000/DesktopExtended扩展容量不动；
- 30Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC不动；
- T8默认stage.dat继续暂缓；
- IL2CPP按用户要求不处理；
- R1-WP02 full trace继续BLOCKED。

## Evidence files

- `docs/ai/RESEARCH/R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`；
- `docs/ai/TASKS/R8-WP01G-R01-r2-scheduler-source-reachability-closure.md`；
- `docs/ai/TASKS/R2-CANDIDATE-TAIL-01-step-wait-carrier-retention.md`；
- `docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md`。
- `docs/ai/RESEARCH/R8-WP01G-R01B-d-step-debug-unlock-source-policy-20260823.md`；
- `docs/ai/TASKS/R3-STEP-01-debug-unlock-policy-and-deterministic-command.md`。
