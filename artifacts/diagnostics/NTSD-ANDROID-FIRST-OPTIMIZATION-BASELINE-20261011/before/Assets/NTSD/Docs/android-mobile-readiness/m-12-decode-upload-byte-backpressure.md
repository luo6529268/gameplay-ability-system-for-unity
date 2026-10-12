## 2026-10-10 当前阶段更正：M-12

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P2／当前后续评审 | 4 | 中 |

第4项：核解码到上传间的字节占用与释放，存在实际高峰后才定背压。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-12 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 内存 / 中 |
| 当前状态 | 方案待容量预算；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-01/H-08所有权接口与容量；不要求视觉整个父项先通过 |
| 下一步与具体方向 | 区分待上传和仍被atlas持有的像素；保留UniTask，取消归还reservation |
| 完成边界 | 队列/峰值有界、无死锁/旧session回写，不能把队列缩小当全驻留解决 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-12 解码—上传按字节背压方案

> 优先级：中
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 本文为待批方案；下面的测试全部是未来验收条件，本轮未执行。

## 当前事实

[已验证-代码] `CharacterAnimtorManager.cs:2505-2514`解码结束后先释放cpuSemaphore，
再等待uploadSemaphore，并将processed pixels加入stagedAtlasSources。
解码并发数不等于累计完成像素字节上限；当前全量staging本身仍线性增长。
仅减少排队任务不能宣称解决H-08的全量驻留峰值。

## 待批解决方案

1. 统计每个sheet的解码前预估/实际像素字节及排队、上传、已提交staging字节。
2. 建立有界字节reservations与背压；保留已有UniTask/主线程Unity上传路线，
   不另建资源框架，不用持锁阻塞主线程等待后台释放。
3. 最大单资源超过批次上限时启动前明确拒绝或走获批单资源策略；
   失败/取消/迟到回调必须归还reservation，禁止泄露令牌和旧session发布。
4. 分别限制“待上传队列”与“因atlas构建仍必须保留”的像素；后者仅在不再被消费者
   使用时释放。最终通过H-08预烘焙移除全量组装，不偷偷在正式预印装载链增加运行时排版。
5. reservation/staging归TransitionPeak账本；峰值与吞吐折中由获批M0决定。

## 实施前门、验收与测试

依赖H-08/H-01所有权清点、用户批准容量和Task/Change；数值未测不直接设置生产默认。

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 字节背压 | 大小sheet混合、慢上传、多CPU并发 | 排队/在途字节不超批准上限，加载不死锁 |
| 全量staging | 现有atlas路径 | 单独报告保留总量，不伪称队列有界即全峰值有界 |
| 大资源/错误 | 上限+1、坏图、主线程上传失败 | 明确reason，无截断像素，reservation完整归还 |
| 取消/重进 | decode/upload各阶段取消和迟到 | 无旧发布、临时数据/令牌泄漏 |
| A/B | 相同内容/配置/指纹 | rect/UV/pixel/目录一致，加载时间与CPU/Native/GPU峰值完整 |

本项只影响加载成本，不改变逻辑结果、资源闭包、segment或fail-closed。
2026-10-06登记；本轮无实现/测量。回滚到原加载链不等于低端内存通过。
