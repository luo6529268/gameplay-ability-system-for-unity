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
