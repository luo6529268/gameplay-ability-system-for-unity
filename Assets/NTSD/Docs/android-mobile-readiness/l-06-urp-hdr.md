## 2026-10-10 当前阶段更正：L-06

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 低 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-06 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 设备候选 / 低 |
| 当前状态 | 待画面与设备决策；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-02包、M-07矩阵、视觉验收 |
| 下一步与具体方向 | 保持其他配置做HDR对照；先有GPU成本再定 |
| 完成边界 | 颜色/混合/画质通过且性能收益成立；不直接关闭 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-06 URP HDR 移动端评估方案

> 优先级：低  
> 状态：`OPEN / A_B_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 URP HDR 开启，但尚无证据证明战斗画面需要 HDR，也没有 Android 带宽、RenderTarget 内存和 GPU A/B。不能因为“移动端通常关闭 HDR”就直接改变正式表现。

## 解决方案

1. 建立 HDR on/off 的独立渲染 Profile，保持其他参数、相机、分辨率和 workload 相同。
2. 比较中央 actor、特效、透明混合、背景、颜色和截图；记录 RenderTarget 格式、内存和 GPU。
3. 若画面无必要差异且目标设备持续受益，再独立批准关闭；否则保留并记录成本。

## 验收条件

- HDR on/off 画面差异有截图或像素证据，不凭主观描述。
- CPU/GPU、RenderTarget 内存、带宽和热稳态差异完整。
- 最终选择在 Adreno/Mali 的正式场景无颜色、透明或后处理回归。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 像素 A/B | 固定相机与帧 | 差异被量化和解释 |
| 特效覆盖 | 透明/叠加/高亮场景 | 无剪裁、色带或混合异常 |
| 内存/GPU | 1000 visible/普通战斗 | RenderTarget 和 GPU 数据完整 |
| 热稳态 | 目标设备持续运行 | 选择不会后期回退 |

## 证据与留痕

- 当前证据：URP `m_SupportsHDR=1`、MSAA=1、Render Scale=1。
- 保存 URP Profile、截图/像素 diff、RenderTarget 格式、内存与 GPU 报告。
- 2026-09-06：方案建立；尚未决定关闭 HDR。
