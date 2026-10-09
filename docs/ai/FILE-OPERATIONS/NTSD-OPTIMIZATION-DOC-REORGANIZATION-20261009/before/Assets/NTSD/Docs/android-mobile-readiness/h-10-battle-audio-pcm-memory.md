# H-10 战斗音频依赖、PCM 预处理与内存方案

> 优先级：高
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 本文为待批方案；下面的测试全部是未来验收条件，本轮未执行。

## 当前事实与问题

[已验证-代码] `App/NTSDSoundPlayer.cs:178-209` 调用
`CharacterAnimtorManager.CollectBattleSoundIds`，后者（:3923起）遍历全部已加载角色配置。
单声道转双声道在 `NTSDSoundPlayer.cs:681-713` 创建 mono/stereo 临时数组和新 AudioClip。
这是保持当前 native 每声道增益语义的适配，不是可直接删除的冗余。

[既有报告-限定] [共用声音回归报告](../../../../artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md)
:31/61记录965个播放副本，payload按样本数估算220188696字节≈209.99MiB；
退出Play副本/voice计数为0。**这不是RSS、Unity Native或峰值实测，也不是泄漏结论。**
具体声道/重触发行为遵循该任务已声明的用户来源，不把它改写成全项目新规则权威。

## 待批解决方案

1. 基于H-08保守生产者闭包计算本局声音集合，补DAT帧声音、内建/受击/破碎/武器、
   公共cue、项目KO声音等真实生产者；仅全局去重不等于按局加载。
2. 比较构建期PCM适配与现有运行时适配：保持样本、采样率、声道增益、同cue重触发、
   聚合与voice规则；交付格式/载体未冻结，不默认有损压缩或改变64-voice行为。
3. 音频Lease与prepared cue/原clip/播放副本/cache所有权合并审计；共享资源只计一次，
   声明跨局复用、上限、取消、迟到回调及销毁时点。
4. 建立整局总预算：音频steady包括原始clip及播放副本；transition包括旧/新局Lease、
   转换临时数组、解码/上传staging及回调仍持有数据。与ATLAS/renderer账本汇总避免双计；
   不擅自重定义ATLAS既有预算，归属/设备总额实施前另行批准。
5. H-01部署与M-12背压协同；内容缺失/超预算在启动前明确拒绝，战斗热路径不迟加载、
   不静默丢声音，不以降采样/减少事件偷换优化。

## 实施前决策与依赖

用户批准Task/Change范围后才测量：实际本局/全量cue数量、原clip/副本/temporary字节、
加载阶段耗时、cache持有图。确认音频依赖Manifest、载体、格式、steady/transition数值、
退出/重进释放门；未有数字不能宣称低端适配通过。H-08/M-15/M-09共享指纹。

## 验收条件

- 同seed/input/tick的模拟checksum和声音事件序列不变；逐样本/声道、增益、聚合、
  retrigger和voice行为达到当前已批准基线，设备波形/听感层级另记。
- 实际音频请求均在保守闭包内；未加载请求有明确拒绝reason，无静默漏音。
- 普通与1000实体、多角色/技能最坏集合的steady/transition音频及整局内存均满足批准预算；
  静态估算与实测差异可解释。
- 物化/播放热路径0GC；取消、连续三轮进出、应用退出无旧session回调、旧clip/voice/Lease残留。

## 未来测试矩阵

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 声音闭包 | 全catalog静态生产者 + 运行请求覆盖 | 不漏依赖，公共cue去重 |
| PCM/事件A/B | 代表单cue/多cue/持续技能/声道边界 | 样本及已声明行为不变，first difference即停 |
| 内存阶段 | 冷启动/转换/稳定/旧新局重叠/退出 | 原clip、副本、临时、cache及总峰值分别记录 |
| 1000声音洪峰 | 同cue/多cue/连续tick | 0GC，正确聚合/重触发，无异常拒绝 |
| 生命周期 | 取消/迟到/三轮重进 | generation有效，副本与借用按合同归零 |
| Android设备 | 具名ARM64/API/热状态 | 功能与内存实测，不以Editor证据替代 |

## 留痕与回滚

2026-10-06：登记待批方案，复用报告且静态重扫；没有改代码/音频或运行测试。
每个闭合行为独立Change；保留现有播放路径作A/B和批准后的回退，回退不等于低内存达标。
