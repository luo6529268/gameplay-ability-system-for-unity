## 2026-10-10 当前阶段更正：L-03

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 低 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-03 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 发布 / 低 |
| 当前状态 | 待发布周期；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 发布商店和时点、H-02构建环境 |
| 下一步与具体方向 | 实施时查询官方要求并冻结min/target；本轮不写未经核实的政策值 |
| 完成边界 | 实际Manifest与当时批准版本一致，目标系统可运行 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-03 Android Target API 冻结方案

> 优先级：低  
> 状态：`OPEN / RELEASE_TARGET_NOT_FROZEN / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

Target API 当前为 Automatic，实际值取决于构建环境。内部开发不一定受阻，但正式发布需要按目标商店当时要求冻结并记录实际 target/min API。由于政策会变化，实施时必须使用当时官方要求复核。

## 解决方案

1. 发布周期开始时确定 minSdk/targetSdk，并在 Build Profile 显式记录。
2. 构建前检查本机/CI SDK 可用性；构建后从 AndroidManifest 读取实际值。
3. 在最低支持 OS、主流 OS 和较新 OS 上执行安装与生命周期测试。
4. 商店要求变化时创建新记录，不静默依赖 Automatic。

## 验收条件

- Release 报告包含实际 minSdk/targetSdk 和依据日期。
- APK/AAB Manifest 与 Build Profile 一致。
- 最低支持版本和目标版本设备/模拟环境能安装并完成普通战斗。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Manifest 审计 | 解包 Release 候选 | min/target 与冻结值一致 |
| 最低系统 | minSdk 对应设备 | 安装、启动、战斗通过 |
| 较新系统 | 当前主流/较新 Android | 权限、后台恢复、存储无异常 |
| 构建环境 | 干净 CI/开发机 | SDK 缺失时前置失败信息明确 |

## 证据与留痕

- 当前证据：`AndroidTargetSdkVersion=0`（Automatic）。
- 记录官方要求来源、冻结日期、实际 Manifest 和设备矩阵。
- 2026-09-06：方案建立；发布 target 尚未选择。
