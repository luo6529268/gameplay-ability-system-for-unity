<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-05 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 设备候选 / 低 |
| 当前状态 | 待设备A/B；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-02包、M-07候选矩阵；相关配置写入另批 |
| 下一步与具体方向 | 采集开销与Jobs性能分开；不能盲开开关 |
| 完成边界 | 设备稳定性/热态收益支持才改默认 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-05 Graphics Jobs 与 FrameTiming 评估方案

> 优先级：低  
> 状态：`OPEN / A_B_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

Graphics Jobs 和 FrameTimingStats 当前关闭，mobile multithreaded rendering 已开启。这不是已证实错误；Graphics Jobs 在不同 URP/driver/设备上可能改善、无效或回退，必须 A/B。

## 解决方案

1. Development Profile 提供可控的 FrameTiming 采集开关，不强制 Release 永久开启。
2. 在 H-05 设备矩阵上对 Graphics Jobs on/off 做相同 APK 配置、场景和温度条件 A/B。
3. 同时记录 main/render/GPU、线程利用率、崩溃、driver 异常和功耗；不能只看平均 FPS。
4. 只有跨目标设备证据支持时才修改生产默认。

## 验收条件

- FrameTiming 数据可与 Profiler/报告指纹关联，采集本身开销已量化。
- Graphics Jobs 决策基于 Adreno/Mali 与 Vulkan/GLES3 的 A/B，而不是桌面结果。
- 启用后无渲染错误、线程竞态、崩溃或热稳态回退。

## 测试条件

| 测试 | 组合 | 通过标准 |
|---|---|---|
| Timing 开销 | stats off/on | 开销可测且不污染正式结论 |
| Graphics Jobs A/B | off/on 同 workload | P50/P95/P99/Max 完整 |
| 设备矩阵 | Adreno/Mali × Vulkan/GLES3 | 结果可按设备/API 决策 |
| 持续门 | Combat1000 热稳态 | 无后期反转或崩溃 |

## 证据与留痕

- 当前证据：Android Graphics Jobs=0、FrameTimingStats=0、multithreaded rendering 已开。
- 保存完整 PlayerSettings、设备/API、原始时间序列和决策记录。
- 2026-09-06：方案建立；未建议直接打开 Graphics Jobs。
