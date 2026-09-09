# L-01 Android Application Identifier 配置方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / PRODUCT_VALUE_REQUIRED`  
> 最后更新：2026-09-06  
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
