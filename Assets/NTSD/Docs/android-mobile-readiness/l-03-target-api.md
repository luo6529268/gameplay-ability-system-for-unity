# L-03 Android Target API 冻结方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / RELEASE_TARGET_NOT_FROZEN`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

Target API 当前为 Automatic，实际值取决于构建环境。内部开发不一定受阻，但正式发布需要按目标商店当时要求冻结并记录实际 target/min API。由于政策会变化，实施时必须使用当时官方要求复核。

## 解决方案

1. 发布周期开始时确定 minSdk/targetSdk，并在 Build Profile 显式记录。
2. 构建前检查本机/CI SDK 可用性；构建后从 AndroidManifest 读取实际值。
3. 在最低支持 OS、主流 OS 和较新 OS 上执行安装与生命周期测试。
4. 商店要求变化时创建新记录，不静默依赖 Automatic。

## 验收条件

- Release 报告包含实际 minSdk/targetSdk 和依据日期。
- APK/AAB Manifest 与 Build Profile 一致。
- 最低支持版本和目标版本设备/模拟环境能安装并完成普通战斗。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Manifest 审计 | 解包 Release 候选 | min/target 与冻结值一致 |
| 最低系统 | minSdk 对应设备 | 安装、启动、战斗通过 |
| 较新系统 | 当前主流/较新 Android | 权限、后台恢复、存储无异常 |
| 构建环境 | 干净 CI/开发机 | SDK 缺失时前置失败信息明确 |

## 证据与留痕

- 当前证据：`AndroidTargetSdkVersion=0`（Automatic）。
- 记录官方要求来源、冻结日期、实际 Manifest 和设备矩阵。
- 2026-09-06：方案建立；发布 target 尚未选择。
