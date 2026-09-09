# L-08 Android 发布打包方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / RELEASE_PREPARATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

Android 图标、版本策略、AAB、包体预算和发布检查尚未完成。这些不应抢占 H-01～H-08 的功能与性能门，但在外部发布前必须形成可升级、可审计的成品流程。

## 解决方案

1. 冻结 version name/code 规则、dev/release channel、图标与必要启动视觉资产。
2. 建立 Release AAB 构建，接入 L-01 包名、L-02 签名、L-03 target API 和 H-04 ARM64。
3. 设定 base/download/install/runtime content 的包体预算，报告各资源 bank、native library、shader 和重复资产。
4. 建立发布前清单：权限、调试标志、日志、符号、隐私、崩溃符号化、安装/升级/卸载和商店验证。

## 验收条件

- Release AAB 构建、签名和官方验证工具检查通过。
- version code 单调递增，升级安装和内容兼容符合策略。
- 图标、包名、版本、ABI、target API 与发布记录一致。
- 包体和运行时下载量在批准预算内，超限有归因和决定。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| AAB 构建 | Release Profile | BuildReport 成功且签名有效 |
| 分发产物 | 从 AAB 生成设备 APK | ARM64 安装和普通战斗通过 |
| 升级 | 前一正式候选→新候选 | 数据和内容迁移正确 |
| 包体审计 | 分析 AAB/APK | 资源/ABI/shader 大小可归因 |
| 发布清单 | 干净候选 | 无开发开关、敏感日志和缺失元数据 |

## 证据与留痕

- 当前证据：Android icons 为空、bundle version code=1、无 AAB 证据。
- 保存 AAB/APK SHA-256、BuildReport、签名公开指纹、版本/包体报告和发布清单。
- 2026-09-06：方案建立；发布准备未开始。
