# M-15 本地 Battle Kernel 依赖与构建可复现性方案

> 优先级：中
> 状态：`OPEN / CERTIFICATE_INTEGRATION_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 本文为待批方案；下面的测试全部是未来验收条件，本轮未执行。

## 当前事实与边界

[已验证-配置] `Packages/manifest.json:5`将
`com.ntsd.battle-kernel`指向`file:../../NTSD_Server/packages/com.ntsd.battle-kernel`。
Client commit本身不能唯一描述相邻仓库dirty package。本项是证据/交付风险，
不是已确认运行时性能热点；不授权更换包源、修改Server或升级Kernel合同。

## 待批解决方案

1. 在M-09证书保存Kernel package.json版本、resolved来源、Server commit、
   精确package文件Manifest/SHA及dirty状态；敏感与无关文件不纳入。
2. 构建Preflight验证预期路径/版本/内容身份与实际解析包，缺失/漂移明确拒绝，
   不自动拉取、安装、覆盖或切换相邻仓库。
3. 锁定Unity/编译目标/Package解析环境；冷环境复现需要独立获批的准备方式。
   是否转版本包/交付归档另作用户选择，当前本地依赖形式可保留。
4. Kernel身份变更按受影响域使checksum/快照/AI/性能证据失效，不能以Client未改继续复用旧证书。

## 验收与未来测试

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 指纹 | 相同Client、不同Kernel或dirty内容 | 证书构建指纹不同，准确记录来源 |
| 缺失/漂移 | package不可用/版本不符 | Preflight明确失败，不偷偷改包 |
| 重复构建 | 同pinned环境及完整输入 | schema/内容/解析闭包一致，产物差异可解释 |
| ARM64闭包 | 获批H-04 Development Player | 包编译/裁剪/平台兼容通过，报告关联Kernel指纹 |
| 失效传播 | 改Kernel输入 | 相关证书标stale；不自动重跑全部历史任务 |

依赖H-02/H-04/M-09；只需证书集成即可关闭本项，不制造无证据的新资源/网络架构。
2026-10-06登记；本轮不写Packages或Server，不运行构建。
