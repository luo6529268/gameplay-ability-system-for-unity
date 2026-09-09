# H-07 当前代码 1000 AI 性能证书方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / CURRENT_CERTIFICATE_MISSING`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

2026-08-23 的历史结果属于较早工作树且不是 Android；当前 AI、Host、pass、worker、checksum 和 presentation 已变化，原始 JSON 也不在现有 Temp，不能证明当前构建达标。

## 解决方案

1. 把每个压力场景冻结为版本化 request：场景、seed、roster、出生算法、profile、broadphase、renderer、分辨率、warmup/sample tick。
2. 先在可识别的当前代码状态运行 Windows/Editor 基线，再在相同内容和 workload 的 ARM64 Android Player 复测。
3. 报告统一记录 logic/visible/main/render/GPU 的 P50/P95/P99/Max、backlog、dropped tick、GC、内存、pair、fallback、draw、SetPass 和 teardown。
4. 每份报告使用 M-09 指纹；任何影响 tick、AI、碰撞、OPoint、presentation 或内容闭包的改动使证书失效或触发分层重测。

## 实施步骤

1. 恢复/重建可复现 request 与报告保存路径。
2. 执行 120 warmup + 180 sampled tick 冒烟门。
3. 执行 120 warmup + 1800 sampled tick 的 Dispersed1000 与 Combat1000。
4. 执行 Concentrated1000、OPoint burst 和 M-08 持续门。

## 验收条件

- 报告唯一关联当前源码/dirty manifest、内容、Unity 版本和 APK SHA-256。
- Dispersed1000、Combat1000 正式采样完成且 logic P95 `<33 ms`。
- P99 不形成持续 backlog，正常战斗 dropped tick 为 0，warmup 后 logic allocation 为 `0 B/tick`。
- capacity reject、central unresolved/stale、teardown 残留均为 0；Android 设备门另满足 H-05。

## 测试条件

| 测试 | Warmup/Sample | 通过标准 |
|---|---:|---|
| Dispersed1000 冒烟 | 120/180 | workload 有效、无 hard failure |
| Combat1000 冒烟 | 120/180 | 命中/AI/表现路径实际活跃 |
| Dispersed1000 正式 | 120/1800 | 全性能门和生命周期门通过 |
| Combat1000 正式 | 120/1800 | 全性能门和生命周期门通过 |
| 最坏/持续 | 依 H-06、M-08 | pair、热降频和 backlog 受控 |

## 证据与留痕

- 历史参考仅为 `MEASURED_HISTORICAL`，不得作为当前证书。
- 保存 request、原始 JSON、Profiler/FrameTiming、设备指纹、代码/内容 fingerprint 和完整失败原因。
- 2026-09-06：方案文档建立；当前证书仍为 `MISSING/STALE`。
