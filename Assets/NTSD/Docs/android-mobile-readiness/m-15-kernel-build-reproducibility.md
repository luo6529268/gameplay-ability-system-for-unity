## 2026-10-10 当前阶段更正：M-15

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 随选定改动验证／不单独插队 | 不单独排队 | 中 |

当前修改所需Kernel身份/可复现证据随项维护；正式平台构建后置。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-15 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 保障 / 随改动 |
| 当前状态 | 证书集成待完善；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 源身份采集可独立；APK关联仅需H-02/H-04对应产物 |
| 下一步与具体方向 | 记录相邻本地package精确文件身份，复用现有工具；不写Server/更换包源 |
| 完成边界 | Client不变而Kernel变时指纹会变；不要求先取得整个平台证书 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

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
