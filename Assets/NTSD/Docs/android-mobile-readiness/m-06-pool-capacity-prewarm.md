## 2026-10-10 当前阶段更正：M-06

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P2／当前后续评审 | 8 | 中 |

第8项：按真实slot/command/shell峰值核预热和容量，避免无据扩大池。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-06 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 内存 / 中 |
| 当前状态 | 已有预热／峰值待评估；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 复用H-11 seal；按局资源准备依赖H-08/H-01接口 |
| 下一步与具体方向 | 按slot/command/shell分别清点峰值，不能用GameConfig旧10/200判断现容量 |
| 完成边界 | 目标工作负载seal后不增长/无拒绝；关闭quiesced且预热峰值有预算 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-06 对象池容量与预热方案

> 优先级：中  
> 状态：`OPEN / RUNTIME_CAPACITY_PREWARM_EXISTS / CAPACITY_PROFILE_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

加载流程已按runtimeSettings.InitialRuntimeSlotCapacity调用PrepareCapacityForContentAsync；MobileExtended为1050 runtime slots/1000 max active。不能只看GameConfig 10/200判容量不足。剩余风险是预热shell/逻辑slot/临时效果的数量、启动峰值、后续结构高水位和seal覆盖；不擅自改变权威实体限制。

## 解决方案

1. 从 roster、最大 active、OPoint/分身/召唤峰值和表现策略生成 `BattleCapacityPlan`。
2. 在 allocation seal 前、加载界面内分批预热必要 pool，记录耗时和内存；不需要表现的 logic-only entity 不预建 GameObject。
3. 明确各单位hard capacity、预热前增长策略、拒绝reason及峰值；allocation seal后禁止热路径动态扩容/无界Instantiate，不把“soft上限”当允许战斗中继续增长。
4. 与 H-08 资源 Lease 协同，资源准备完成后再创建依赖该资源的表现对象。

## 验收条件

- 目标 workload 开始前完成所需预热，allocation seal 后无意外扩容。
- Combat1000/OPoint burst 无 critical reject、无突发大批 Instantiate、无超预算启动峰值。
- 实际峰值与 CapacityPlan 可追溯，过量预热不会长期占用不可接受内存。
- shutdown 后 pool quiesced、active borrower=0、旧 World/Entity 绑定=0。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Plan 计算 | 多种 roster/OPoint 闭包 | 容量确定、可解释、无负值/溢出 |
| 分批预热 | 低内存 Android | 峰值和单帧耗时在预算内 |
| Seal 后运行 | Combat1000 | 无扩容与 critical reject |
| 峰值生成 | 分身/召唤/OPoint burst | 回收正确，无无界创建 |
| 关闭 | 正常/中途取消/错误退出 | pool quiesced，borrower 全归零 |

## 证据与留痕

- 2026-10-06重扫：`Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs:243-252`；`Simulation/Runtime/BattleRuntimeProfile.cs:137-164`。运行档位预热已接入，旧10/200只保留历史观察，不等于实际准备量。
- 保存 CapacityPlan、预热时长/内存、动态创建计数、拒绝计数和 teardown 诊断。
- 2026-09-06：方案建立；正式容量 Profile 尚未冻结。
