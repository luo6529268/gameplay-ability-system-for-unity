# L-02 Android 签名与 Keystore 方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / SECURE_CREDENTIAL_REQUIRED`  
> 最后更新：2026-09-06  
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
