## 2026-10-10 当前阶段更正：L-01

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 低 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-01 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 发布 / 低 |
| 当前状态 | 待产品值；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 用户正式包名决定；内部调试包不必等待Release命名 |
| 下一步与具体方向 | 冻结dev/release命名与Manifest一致性 |
| 完成边界 | 包名和升级/并存策略满足产品选择 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-01 Android Application Identifier 配置方案

> 优先级：低  
> 状态：`OPEN / PRODUCT_VALUE_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前没有观察到正式 Android application identifier。内部临时 APK 可以使用测试包名，但外部测试与发布需要稳定、唯一且不可随意更换的标识。

## 解决方案

1. 由产品/发布负责人确定反向域名格式的正式包名和可选 dev/staging 后缀。
2. 构建 Profile 显式设置并报告实际 application identifier，禁止依赖默认 `com.DefaultCompany...`。
3. 检查 deep link、provider、原生插件和签名配置是否引用包名。

## 验收条件

- Development/Staging/Release 包名唯一且规则明确。
- 正式 Release 使用用户确认的稳定 identifier，BuildReport 与 AndroidManifest 一致。
- 升级安装不会因包名变化被当成另一个应用。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Manifest 检查 | 解包 APK/AAB | package 与 Build Profile 一致 |
| 并存测试 | 安装 dev 与 release 候选 | 按产品策略可并存或明确互斥 |
| 升级测试 | 同包名版本递增安装 | 应用数据与签名链行为符合预期 |

## 证据与留痕

- 当前证据：仅观察到 Standalone 默认风格 identifier，未冻结 Android 正式值。
- 记录用户确认值、Build Profile、Manifest 检查和变更历史；不得在文档写入密钥。
- 2026-09-06：方案建立；等待产品值。
