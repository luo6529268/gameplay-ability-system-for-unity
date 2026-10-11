## 2026-10-10 当前阶段更正：L-02

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 低 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-02 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 发布 / 低 |
| 当前状态 | 待安全配置；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 正式密钥管理及L-01包名；不阻塞Debug包 |
| 下一步与具体方向 | 用户安全环境配置；文档只公开证书指纹，不读取私钥/口令 |
| 完成边界 | 签名和升级通过，凭据不进入仓库/日志 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-02 Android 签名与 Keystore 方案

> 优先级：低  
> 状态：`OPEN / SECURE_CREDENTIAL_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前未配置正式 keystore/key alias。Debug APK 不受阻，但没有稳定签名就无法形成可升级的外部发布链。密钥、口令和敏感路径不得提交仓库或写入报告。

## 解决方案

1. 在安全环境生成或接入正式签名，并建立离线备份、访问控制和恢复流程。
2. 本地通过安全凭据存储、CI 通过 secret 注入；仓库只保存非敏感签名配置说明。
3. 构建后记录证书公开指纹，不记录口令、私钥或完整 keystore。
4. 分离 debug/staging/release 签名并明确升级规则。

## 验收条件

- Release APK/AAB 使用正式证书签名且签名校验通过。
- 相同 application identifier 的后续版本可升级安装。
- 仓库、日志和构建产物清单中没有密钥或口令。
- 密钥丢失/轮换和 Play App Signing 策略有书面记录。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 签名验证 | Release 候选 | Android 签名工具验证通过 |
| 升级安装 | v1→v2 同签名 | 安装成功且数据保留 |
| 错签名负例 | 测试包使用不同证书 | 升级被预期拒绝 |
| Secret 扫描 | 仓库与日志 | 无口令、私钥、keystore 内容 |

## 证据与留痕

- 当前证据：`androidUseCustomKeystore=0`，keystore/key alias 为空。
- 只记录证书公开 SHA-256、签名验证结果和安全流程引用。
- 2026-09-06：方案建立；未创建或修改任何密钥。
