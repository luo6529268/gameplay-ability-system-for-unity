# H-07 当前代码 1000 AI 性能证书方案
> 第24批最终停点：PARTIAL / H07_RETRY_LIMIT_REACHED。原Dispersed120+180，实际1000AI、窄tick0B、logic mean443.8275/p95 659.97925ms，但harnessValidity=false不能推广；Combat因pool sealed未开始，四formal未跑。修复3/3到限，H07待用户是否批准独立有限恢复包；不能继续第四修、不自动切生产/解除seal。原11阶段三残留0、Scene同/Menu恢复；Goal必需交付仍未齐。详第24批REPORT/原件。
> 2026-10-07 第24批：固定入口25/25；windows-01 OID0普通OPoint拒绝，windows-02已创建1000但出生布局/seal后flag计数冲突，均零tick/失败保留，关闭三残留0/Scene同/Menu恢复。仅诊断入口适配，windows-03最后固定六窗进行中，修复3/3不增第4轮；不宣称Windows报告已交付或Android认证。生产Factory/规则/资源/settings不改，父OPEN；详同批Task/Record/REPORT。

> 优先级：高  
> 状态：`OPEN / CURRENT_CERTIFICATE_MISSING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

2026-08-23 的历史结果属于较早工作树且不是 Android；当前 AI、Host、pass、worker、checksum 和 presentation 已变化，原始 JSON 也不在现有 Temp，不能证明当前构建达标。

## 解决方案

1. 把每个压力场景冻结为版本化 request：场景、seed、roster、出生算法、profile、broadphase、renderer、分辨率、warmup/sample tick。
2. 先在可识别的当前代码状态运行 Windows/Editor 基线，再在相同内容和 workload 的 ARM64 Android Player 复测。
3. 报告统一记录logic/visible/main/render/GPU的分位数、backlog/dropped tick/GC/内存及teardown；补worker资格/回退/ack、AI查询路径/碰撞fallback、publication与alpha各自变化、物化/上传、音频预热/聚合。CPU命令与真实GPU batch分别记录。
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
- 分开报告1000 logic-only、1000实际active AI与1000可见表现；声明1000实体的类型/roster组成，不以1000 slots或同图重复实例代替完整混战。H-11完整表现热路径0GC与H-10音频账本同时覆盖。

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
- 2026-10-06：按当前插值、音频、AI/碰撞路径补充证书字段和workload有效性；M0/压力/真机均未执行，实施启动仍待用户批准。
