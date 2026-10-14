# 当前图片内存预算接受与后续工作边界

Decision: NTSD-M12-IMAGE-BUDGET-ACCEPTANCE-20261014
Status: USER_ACCEPTED_IMAGE_BUDGET_BASELINE
日期：2026-10-14（Asia/Shanghai）
关联Task：NTSD-M12-MATCH-IMAGE-RESIDENCY-20261013
关联Change：NTSD-M12-MATCH-IMAGE-RESIDENCY-001（保持VERIFIED）
Operation：OP-NTSD-M12-IMAGE-BUDGET-ACCEPTANCE-20261014

## 用户决定

用户原话：

> 目前常驻和加载的内存占用，我觉得可以接受，目前这样子，这一点可以先记录下来，如果后续发现手机实测发现效果还是很大时，再做调整，那么接下来呢？是开始扩展到所有资源文件嘛？

记录为：接受当前图片驻留与分阶段加载方案及本轮报告预算，保留当前版本作为后续比较基线。暂不为继续降低静态估算而追加批量拆图、裁边、公共底图/补丁或更激进压缩。后续若手机实测仍出现明显内存压力或实际效果不符合预期，再依据具体设备、场景和测量定位后调整。该决定不阻止修复新发现的正确性、缺资源或生命周期故障。

此为用户接受，不是手机已完成实测或整进程峰值认证；不把本轮桌面RGBA观察改称手机ASTC结果。

## 接受版本的数值与口径

| 场景 | 常驻MiB | 加载MiB | 性质 |
|---|---:|---:|---|
| 固定八角色+当前场景依赖+全部203头像/公共图片 | 91.987 | 126.837 | ASTC4x4工程预算，至少4096纹理能力 |
| 同规模旧/新战斗图片重叠、头像共用 | 148.440 | 183.290 | 特定重叠包络，不是任意组合的全局最大值 |
| 原全目录预加载对照 | 472.395 | 532.243 | 同一ASTC/分页资源与相同公式的比较基线 |

实际本机Direct3D11不支持ASTC，走RGBA回退：manager所持Texture的Profiler统计两轮均为菜单26.524 ->战斗473.611 ->退出26.524 MiB；该统计非整个进程/手机GPU/RAM。330份DAT仍解析/保留，托管DAT对象、音频、字体、实例、渲染目标与驱动不在上述ASTC图片预算内。原始证据保持不变：

- [最终验收](../../../artifacts/diagnostics/M12-match-image-residency-20261013/validation-final.json)
- [完整报告](../../../artifacts/diagnostics/M12-match-image-residency-20261013/REPORT.md)
- [模型](../../../artifacts/diagnostics/M12-match-image-residency-20261013/memory-audit-02/result.json)
- [真实流程观察](../../../artifacts/diagnostics/M12-match-image-residency-20261013/scene-flow-observations-02.json)

## 当前全目录覆盖事实（本次只读刷新）

- catalog当前330个object定义，158个type0定义；其中type0/hidden0/verified的可选角色50个。八个角色是运行验收样本，不是生产支持白名单。
- PrepareBattleImagesAsync接收当前对局rootIds，选择器基于全部已发布配置、类型、fusion/kind及已配置stage依赖生成计划；没有固定八角色生产限制。
- 现有ASTC manifest为723条源映射、714份唯一压缩块；活跃分页56页/35源。主体资源压缩接入已经面向全目录，特殊多几何源仍有原RGBA兼容路径；不将这些数字说成头像、地图、字体、音频等所有资源均已转换。
- 全目录DAT解析成功与通用加载器存在，不证明全部角色、招式组合或所有设备已经运行验收。当前实际完整Scene验收仍是固定八角色。

## 下一步建议：PROPOSED_NOT_STARTED

建议把验证覆盖从固定八角色扩展到全部50个可选角色及其可达变身、分身、武器、特效对象，同时核对330定义中的公共依赖。先生成逐角色全帧闭包、实际载入资源、格式/尺寸/缺项/回退与预算矩阵；再用内存最高或依赖最复杂的角色组合，以及融合、变身、特殊共享页等边界做少量真实进出/取消检查。明确保留缺失引用，并检查必要的组合上下文；不得把单角色遍历称为所有多人组合穷举。

有清单证明确实漏接的资源，再另行声明精确补齐范围。不要把所有图片同时常驻作为“扩展到全部资源”；不要无差别重转已有ASTC、全量拆图、压缩UI/字体或自动扩大到音频。头像/公共图/地图等保持当前表示与所有权，后续分别按实测价值和兼容需求处理。

本轮问题是在讨论下一步，不是新的全资源改写授权；上述后继仅建议，未创建实施Task/Change、未运行全目录新测试或Unity。Android派生文件打包和真机兼容路径仍是未验证边界，未来需要时另行声明。

## 保持的约束

原PNG、DAT、ASTC、旧分页、全量回退与用户工作保留；不重跑P1、不改战斗规则/Scene/Settings/包，不自动恢复被暂停的优化或自动化。父M12保持OPEN、原byte admission默认OFF、H08 ON。已接受的TMP字体新增字形例外保留。
