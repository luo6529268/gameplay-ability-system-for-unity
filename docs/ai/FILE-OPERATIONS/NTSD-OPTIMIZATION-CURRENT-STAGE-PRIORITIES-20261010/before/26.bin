<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-09 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 保障 / 随改动 |
| 当前状态 | 复用现有manifest／缺字段补齐；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | Client dirty/内容/Kernel实际身份；无需等待Android包才记录Windows证据 |
| 下一步与具体方向 | 沿用现有请求和报告；平台不适用字段显式N/A，不造第二套harness |
| 完成边界 | 每份声明域证据可关联输入和范围；失效按影响域传播 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-09 Android 性能证据指纹方案

> 优先级：中  
> 状态：`OPEN / SCHEMA_INTEGRATION_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

没有统一指纹时，性能报告无法证明对应哪份源码、内容、APK、设备和运行配置，也不能判断改动后证书是否过期。

## 解决方案

1. 定义版本化 `BattlePerformanceEvidenceFingerprint` schema。
2. 构建阶段写入 APK SHA-256、Unity 版本、源码 commit 或完整 dirty manifest、内容 Manifest、Scene/build profile 和 shader/资源版本。
3. 运行阶段补设备/SoC/RAM/OS/GPU/driver/API、runtime/AI/broadphase/atlas/draw、分辨率/热状态/workload；新增正式EXE与冻结源/trace provenance、Kernel package身份/dirty Manifest、插值开关/取样/FPS、worker实际资格/回退和声音预热范围。
4. 报告缺任一强制字段时标为 `UNTRACEABLE`，不能晋升证书。

## 验收条件

- 每份正式报告可唯一重建“代码+内容+构建+设备+配置+输入”。
- dirty worktree 不允许只写 commit，必须保存目标文件哈希/manifest。
- 不同 APK、内容或 workload 不会误归为同一证书。
- 指纹变化能按域使 H-05/H-07/H-09/M-08 证书失效。
- M-15相邻仓库Kernel内容变化即便Client commit不变也必须改变构建指纹；共享对象、计数口径、音频副本估算与实测RSS不得混为同一指标。

## 测试条件

| 测试 | 操作 | 通过标准 |
|---|---|---|
| Schema 完整性 | 删除一个强制字段 | 报告拒绝晋升并指出字段 |
| 稳定性 | 同一构建重复运行 | 构建指纹相同、运行实例 ID 不同 |
| 变更传播 | 改内容/配置/workload | 对应指纹必然变化 |
| Dirty manifest | 有未提交目标改动 | 能唯一列出文件与哈希 |
| 跨设备 | 同 APK 在两台设备 | APK 指纹相同，设备指纹准确区分 |

## 证据与留痕

- 最低字段：APK、Unity、源码、内容、设备、GPU/driver、API、profile、warmup/sample、场景、seed、roster。
- 保存 schema 版本、validator 结果、完整原始报告和 supersede 关系。
- 2026-09-06：方案建立；统一 schema 未实现。
- 2026-10-06：补当前Kernel/插值/音频/worker与权威来源字段；已有局部报告不等于统一生成器已实现。SCHEMA_INTEGRATION_PENDING，未运行validator或构建。
