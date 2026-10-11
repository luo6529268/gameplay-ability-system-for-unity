<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-04 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 平台 / 中 |
| 当前状态 | 待配置授权；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 构建准备可独立；运行验收需要H-01内容和H-02可安装包 |
| 下一步与具体方向 | 盘点AOT/native/stripping并冻结Development配置；不改第三方或包源 |
| 完成边界 | ABI/AOT构建与具名平台确定性通过；不等Android千人性能才确认ABI成果 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# H-04 Android IL2CPP 与 ARM64 方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
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
