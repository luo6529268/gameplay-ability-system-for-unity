# 持有路径剩余调用审计

IN_PROGRESS / READ_ONLY。初次140、释放1150、补给242及其限定回放/Play/关闭证据保持，不重做。

## 源码表述纠正

根重新读取当前BattleWorld28::settle_held_refill_objects完整函数。它只有补给holder state17门，没有child state12/18 unsupported分支。unsupported计数来自无parent或child runtime frame；无效reciprocal另有失败诊断。此前恢复游标里的“源unsupported12/18”表述错误，不能驱动实现；保留历史但以本更正覆盖。

## Unity剩余

- BattleHeldObjectWriter.SyncHeldPose：Assets与Packages全部C#搜索只有定义，无直接caller。保留原API，不为清理搜索结果修改它，也不将其视为当前生产未对齐证据。反射/外部程序集未证明，不能声称所有可能调用不存在。
- RunStep12及LF2WeaponHeldStateResolver.Act当前在child selected frame state12或10时执行额外damaged drop，使用旧随机/heldsetter；当前正式source无对应分支。已有formal330静态图只验state12/18为零，不能据此认定state10为零，正在补同图state10。
- 若state10静态域同为零，只能归为该ITR2/OPoint2静态域不触发；identity变换后仍保持关系及其它producer未穷尽。应保留明确回访依赖，不能贸然改旧兼容damaged分支或据其存在重新跑已关闭基本用例。
- 两pose实现cover2与源不一致的旧诊断已有formal330 primary cover2=0证据；不从该零域推断未来自定义内容或动态定义等价。

下一由state10域及剩余raw/identity caller矩阵确定准确任务，不批量替换getter。
