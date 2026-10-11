## 2026-10-10 当前阶段更正：H-05

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 高 |

Android真机及GPU认证在后期平台验收处理；当前CPU渲染优化可独立评审。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-05 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 验收 / 后续 |
| 当前状态 | 设备前置未满足；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-02/H-04可运行包、H-01内容、M-07初始API矩阵；最终汇总H-07/M-08 |
| 下一步与具体方向 | 先功能与故障门，再分API/设备性能；目标设备和预算需用户确认 |
| 完成边界 | 原方案设备域全部条件有证；Windows/Editor证据不替代 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# H-05 Android 真机与 GPU 认证方案

> 优先级：高  
> 状态：`OPEN / CERTIFICATION_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前没有 Android Player 的功能、内存、FrameTiming、GPU、温度或降频报告。Editor/Windows 的驱动、线程和内存数据不能外推 Android。本项只能通过真实设备关闭，预处理和自动化不能代替真机证书。

## 解决方案

1. 建立最小设备矩阵：中端 Adreno、中端 Mali、至少一个 64 位-only 环境。
2. Vulkan 与 GLES3 分别构建或显式选择，认证 Texture2DArray、OrderedPages、动态 Mesh、URP Feature、阴影、FootSelf 与血条。
3. 使用统一认证 Runner 执行冷启动、普通战斗、生命周期、1000 AI 短测/长测和热稳定性。
4. 每份证书绑定 M-09 指纹，失败时保存 first failure、截图/录屏、Profiler/FrameTiming 和结构化日志。

## 实施步骤

1. 冻结目标设备档位、OS、图形 API 和性能门。
2. 先完成 A0～A3 功能门，再执行中央渲染 A4，最后执行 A5～A8 性能门。
3. 对差异做设备/API 分组，不用单台高端设备覆盖其他组合。
4. 所有影响渲染、worker、AI、碰撞或内容的改动按规则触发重认证。

## 验收条件

- Adreno/Mali 至少各一台完成普通战斗、中央渲染和生命周期认证。
- Vulkan/GLES3 各有一份可追溯报告，或有明确证据支持某 API 被正式排除。
- 无 unresolved/stale central command、紫材质、丢角色、GPU device loss 或驱动崩溃。
- 1000 AI 和持续热性能满足各自 H-07/M-08 门；失败不能由平均 FPS 掩盖。

## 测试条件

| Gate | 工作负载 | 必需证据 |
|---|---|---|
| A1 | 清数据、飞行模式冷启动 | 启动时间、内存、内容加载报告 |
| A2 | 2～8 角色触控普通战斗 | 操作记录、中央像素与异常计数 |
| A3 | 三轮进入/退出/重进 | worker join、pool quiesced、零残留 |
| A4 | Array/Pages × Vulkan/GLES3 × Adreno/Mali | draws、SetPass、GPU P95、unresolved=0 |
| A5～A8 | 1000 AI 短测、正式门、最坏场景、热测 | 完整性能与温度报告 |

## 证据与留痕

- 当前事实：没有可复核 Android 设备证书。
- 证书必须包含 APK SHA-256、源码/内容指纹、设备、SoC、GPU/driver、OS、API、温度和完整 workload。
- 2026-09-06：方案文档建立；真机认证未开始。
