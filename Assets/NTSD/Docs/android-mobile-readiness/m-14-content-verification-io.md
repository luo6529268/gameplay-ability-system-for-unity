> 2026-10-07 首阶段评估完成 / DEPENDENCY_NOT_READY：第25批当前真实906图、图片hash读取29,939,600B下界，Capture（含Assert）12.001s，两Assert3.830/3.656s；包含catalog解析/转换日志等，不是纯IO或cold启动。可变源文件、HashFile stream关闭、generation不等于read lease，immutable/Manifest前置未闭合，不缓存/删守卫。详[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH25-BOUNDED-ASSESSMENTS-20261007/REPORT.md)，父项开放。

# M-14 启动内容完整性校验 I/O 去重方案

> 优先级：中
> 状态：`OPEN / STARTUP_COST_CANDIDATE / PROFILING_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 本文为待批方案；下面的测试全部是未来验收条件，本轮未执行。

## 当前事实

[已验证-代码] `LoganVisualContentCandidate.AssertInputsCurrent(:363-404)`
重新读取catalog、公共输入和图片SHA；`CharacterAnimtorManager.cs:116,213,1463,1689`
有多阶段调用。文件数量/耗时未在本轮测量，不能直接称校验是最大启动瓶颈。

## 待批解决方案

1. 分别统计各阶段catalog/图片读取、hash次数/字节、缓存命中、校验耗时。
2. 如重复成本成立，为不可变内容包/固定Manifest版本建立受控validation epoch及读Lease；
   同一版本内复用校验结论，不以单纯路径/mtime/文件大小作为安全等价证明。
3. 明确内容可变时的失效点和TOCTOU边界：换包、hash、catalog/decoder/模式/公共资源/
   音频版本变化、取消与迟到回调都必须重新校验或拒绝。
4. Editor可变源文件继续保留stale-input守卫；首次/发布前/事务提交必需校验不得删除。
   实施前证明检查合并前后拒绝集合一致，不以“提高加载速度”放宽fail-closed。

## 验收与未来测试

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 正常复用 | 同一冻结包/版本多阶段加载 | 校验结果相同；重复I/O降低，报告开销 |
| 变更失效 | 内容不同但同路径/mtime/大小 | 仍被hash/包身份识别，拒绝旧候选 |
| 全输入覆盖 | DAT/图片/WORDS/SPARK/模式/decoder/音频版本 | 任一受管变化不误复用 |
| 竞态/迟到 | 校验后替换、取消、重进 | epoch/Lease正确，无旧发布 |
| A/B | 固定Manifest与内容 | 目录/行为/错误reason不变，冷启动耗时及峰值对照 |

依赖H-01部署、H-08Manifest、M-09指纹，具体受管输入必须按当前调用链重新枚举。
2026-10-06登记；缓存策略未批准、未删除守卫、未测启动收益。
回退恢复现有完整校验，不能跳过错误内容校验。
