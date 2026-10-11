# NTSD-H01-STRATEGY-AND-SCOPE-20261010

状态：STRATEGY_REVIEW_PENDING / DOCUMENTATION_ONLY。日期：2026-10-10。
需求来源：当前方案讨论会话用户最新要求；先确认第一项策略，不开始实施。
Change：N/A，仅文档。Operation：[同ID记录](../FILE-OPERATIONS/NTSD-H01-STRATEGY-AND-SCOPE-20261010/RECORD.md)。

## 用户决定

“所有优化处理，都按照优化方案文档来处理，所有额外发现的可优化方向，一律不管”；“接下来，我们来处理第一项，然后我们先确认具体的优化策略”。

当前第一项按总清单原编号为H-01 Android生产内容部署。额外发现的方向不新增、不诊断、不修复、不派任务；原方案已有必要依赖只解释，不自动解冻。H06-ROLE-EXACT不再续办，原报告保留；Role默认、FindEffect修复、producer暂缓/OFF、95 OFF和最终门保持。

## 本轮交付与范围

1. 在决定、总清单、共同规则、进度、当前authority/state/handoff及旧提示词入口记录用户边界。
2. 只读核对当前内容根、catalog/DAT/图像/声音读取、加载owner与既有Windows复制清单；不读取活跃Q06方法体。
3. 在[H-01原独立方案](../../Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md)增加待确认策略，不创建额外优化方向或第二份平行方案。

精确已有文档清单、原字节备份和17份保护文件见Operation before-manifest.json。未批准代码/资源/Scene/Prefab/Settings/Packages修改或Unity/测试/Profiler/构建，不发送执行会话消息、不恢复自动监测或旧Goal。

## 策略草案

建议版本化Manifest + 统一内容Provider + 随包本地AssetBundle，沿用现有UniTask、加载owner、generation/取消/发布与关闭边界。先清单和Provider源对照，再接入构建期等价DAT定义及H-08已有预烘焙产物，最后Android离线/缺失/取消重进/三次生命周期验收。它是平台部署工作，不承诺FPS或整tick收益。

H-08/H-10共享资源key、依赖及Lease合同，相关资源实施仍另行明确批准；不要求两个父项所有内存优化先完成。只完成清单/Provider不关闭H-01，原方案最终门不降低。H-02/H-04的设备构建前置当前只说明，不修改设置。

## 当前待用户决定

是否采用“随包本地AssetBundle + 清单/Provider先行”的路线。确认后才制定第一子阶段准确脚本/资源写域、Task/Change、受影响最小验证和回滚。本轮不将路线提案记作已批准。

## 验证边界

文档检查：真实备份/哈希、范围外具名保护、链接、Git暂存/HEAD与diff --check。完成结果追加于Operation与本节，不提前宣称通过。
未执行：Unity、编译、测试、Profiler、性能测量、构建、Android运行。静态代码和官方API说明不等于设备验收。
