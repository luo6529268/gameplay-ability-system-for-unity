# H-04 Android IL2CPP 与 ARM64 方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 `AndroidTargetArchitectures: 1` 对应 ARMv7，未冻结 Android IL2CPP 配置。此状态不能覆盖 64 位-only 设备，也不能产生代表正式目标的 ARM64 性能证据。

## 解决方案

1. Android 正式基线使用 IL2CPP + ARM64；是否同时保留 ARMv7作为独立产品决定。
2. 构建 Profile 显式设置 scripting backend、ABI、stripping 和必要的反射保留规则，禁止依赖 Editor 手动残留值。
3. 构建前检查所有 native plugin 的 ARM64 闭包；构建后解包验证 `arm64-v8a` native libraries。
4. ARM64 Player 必须运行 self-check、固定输入/checksum、普通战斗和 1000 AI 工作负载。

## 实施步骤

1. 盘点 native plugin、反射、AOT generic 和 stripping 风险。
2. 建立 ARM64 Development Build，修复只属于目标闭包的 AOT/裁剪错误。
3. 加入 ABI Post-build Inspector 和构建报告字段。
4. 在 64 位-only 设备完成安装、运行和回归；随后再决定 ARMv7 双 ABI。

## 验收条件

- APK/AAB 包内存在完整 `arm64-v8a` Unity/IL2CPP/native plugin 库。
- 64 位-only 环境可以安装、冷启动并完成一局普通战斗。
- ARM64 下 self-check、固定输入、checksum 和生命周期无平台分叉。
- 报告明确区分 ARM64-only 与 ARMv7+ARM64，不能只记录 ProjectSettings 数值。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| ABI 静态检查 | 解包 APK/AAB | `arm64-v8a` 库完整，无只含 ARMv7 的生产插件 |
| IL2CPP 构建 | Development ARM64 | 0 compile/link error，BuildReport 成功 |
| 运行回归 | 64 位-only Android | 冷启动、战斗、退出重进通过 |
| 确定性 | 固定 seed/input | Editor/ARM64 强一致域 checksum 无分叉 |
| 压力入口 | MobileExtended 1000 workload | 能完成采样并产生有效报告，不因 AOT/裁剪失败 |

## 证据与留痕

- 当前证据：`ProjectSettings/ProjectSettings.asset` 中 `AndroidTargetArchitectures: 1`；当前 Unity 枚举 `ARMv7=1`、`ARM64=2`。
- 实施时保存：PlayerSettings 快照、native plugin matrix、BuildReport、ABI 清单、APK/AAB SHA-256 和设备报告。
- 2026-09-06：方案文档建立；实现未开始。
