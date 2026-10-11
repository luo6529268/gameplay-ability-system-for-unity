## 2026-10-10 当前阶段更正：M-05

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P2／当前后续评审 | 5 | 中 |

第5项：核active/visible/inactive/logic-only真实shell与组件成本。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-05 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 内存/CPU / 中 |
| 当前状态 | 待归因；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 数量归因可独立；移除Core反向引用依赖M-11明确阶段 |
| 下一步与具体方向 | 分别数logic-only/active/visible/inactive shell；不假设1000AI=1000Mono |
| 完成边界 | 具名方案减少无用shell且挂点/slot/OPoint/关闭无变化；未测不承诺收益 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-05 1000 实体 GameObject Shell 成本方案

> 优先级：中  
> 状态：`OPEN / LOGIC_ONLY_PATH_EXISTS / MEASUREMENT_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前有logic-only materialization路径，中央Mesh已接管主体像素；首发角色接线与池预热仍会使用GameObject/Renderer/mount。不能笼统说每个1000实体都必须一个shell，也不能因logic-only存在宣称shell成本归零。分别统计logic-only、实际active AI、visible及预热inactive shell。

## 解决方案

1. 建立三档明确 workload：logic-only、active AI、visible presentation，分别记录对象数、组件数、Update 数、Native/Managed 内存和生命周期成本。
2. 配合 M-11 将 Renderer/Sprite 从 Core handle 中移出，建立按需 `PresentationBindingTable`。
3. 对中央渲染不需要实体 GameObject 的表现数据，评估轻量 binding/纯命令；必须保留当前逻辑实体和可观察行为。
4. 对仍需要挂点、背景或交互组件的对象保留最小 shell，并通过池复用，禁止一次性 ECS 大重写。

## 验收条件

- 三档 workload 的实体、GameObject、组件和内存数量可独立报告。
- 1000 visible 下没有每实体 SpriteRenderer 主体绘制，也没有无用 Update/LateUpdate 热点。
- shell 减少前后 slot、OPoint、挂点、武器、阴影、排序和 teardown 行为一致。
- Scene 退出后 active object、binding、pool borrower 和 World slot 全部归零。

## 测试条件

| 测试 | 规模 | 通过标准 |
|---|---:|---|
| Logic-only | 1000 | 无表现对象且 checksum 有效 |
| Active AI | 1000 | AI/碰撞实际运行，表现边界明确 |
| Visible | 1000 | 中央命令完整，GameObject/组件数可解释 |
| OPoint/武器 | 峰值生成与回收 | 挂点和生命周期无差异 |
| 两轮重进 | Play→Stop→re-enter | 零 ghost、零残留、Scene dirty unchanged |

## 证据与留痕

- 2026-10-06重扫：`Assets/NTSD/Scripts/App/AppManager.cs:320`调用SetLogicObject；`Animation/LF2Objects/LF2Character.cs:978-980`绑定Renderer；`Simulation/Host/SimulationTickDriver.cs`已有logic-only开关；`UI/LoadingPrewarmController.cs:243-252`调用池预热。实际数量、线程准入和成本待测。
- 保存每档 hierarchy/component census、CPU/内存、command 数和生命周期报告。
- 2026-09-06：方案建立；尚未决定可移除 shell 的具体对象类型。
