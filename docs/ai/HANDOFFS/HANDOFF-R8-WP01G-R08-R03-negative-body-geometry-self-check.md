# HANDOFF — R8-WP01G-R08-R03 negative-height body geometry self-check

> 日期：2026-08-24  
> 状态：`IN_PROGRESS / USER APPROVED / PRE-CODE`

## Why this package exists

R08 merge/split已PASS，但full `BattleRuntimeSelfCheck`在目标检查前被R-HC-01阻塞。恢复的正式DAT新增5个
`w=21/h=-999` body；旧风险分类器把它们视为未知non-positive geometry。

## Read-only conclusion

- C++与Unity production均不丢弃该body，也不把负高度取绝对值；
- 两者都计算`y2=y1+h`并使用strict AABB overlap；负高度形成倒置rect。普通小itr不命中，但跨过倒置
  两个端点的大itr仍会满足strict条件并命中；
- 5个实例来自OID58 frame75/76与OID10 frame75/76/77，形状完全相同；
- 报告中的多组`itr w=0/h=79`已属于既有zero-width line合同，不是新差异；
- 因此当前是self-check分类缺口，不是已确认production gameplay差异。

## Next action after approval

只修改`BattleRuntimeSelfCheck.cs`：增加精确negative-height body计数、保留其他unknown fail-closed，并通过
production `BruteForceSceneQuery`验证普通小itr不命中、跨端点大itr命中，并覆盖左右朝向。随后fresh compile、完整self-check和
validator；越过R-HC-01后的任何新失败都作为独立first difference停止。

## Authorization update

用户已于2026-08-24批准执行本包并恢复目标。下一步可按上述边界修改self-check；仍不得改production。

## Code written

- self-check已加入5-entry exact分类与ordinary/enclosing × right/left raw overlap矩阵；
- production/DAT/parser仍0改动；
- 当前恢复点是fresh compile，然后实际运行full self-check。任何越过R-HC-01后的新失败都必须另记first difference。

## Final result

- fresh compile0；R-HC-01日志精确为90 zero-width itr、5 known inverted body、0 unexpected/other；
- 四个raw strict-overlap矩阵通过，R-HC-01关闭，本Change VERIFIED；
- full self-check后续首次失败为旧`AnimationConfig/Mingren/naruto.dat`硬编码路径；已拆R04独立处理；
- production collision、DAT、parser均0改动。

## Protected boundaries

C++只读；DAT/parser/production collision零改动；不扩大到AI、render、T8、Android、IL2CPP、服务器或full trace。
