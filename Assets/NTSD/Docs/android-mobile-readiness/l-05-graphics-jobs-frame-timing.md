# L-05 Graphics Jobs 与 FrameTiming 评估方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / A_B_REQUIRED`  
> 最后更新：2026-09-06  
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
