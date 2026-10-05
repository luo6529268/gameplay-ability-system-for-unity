# 非角色 hit_Fa7 源深度死区：只读候选

2026-10-05，explorer `d024_noncharacter_remainder` 只读核查，root保留为平台阴影单项后的条件线索；未修改该行为、未新增Task/Change/ONE，未运行测试或Editor。

当前正式336B44闭包 `native_ai.cpp::NativeAi28::step_non_character_hit_fa` 的2/4/7/12/14共用分支以源整数position.z比较±5死区，再写native motion.z±0.4；`simulation_tick_driver.cpp`调用且build.ps1纳入native_ai.cpp。Unity `LF2Entity.RunNonCharacterHitFa7FrameLogic` 约2039行直接读投影后的Runtime.ZInt，以±5比较后写原生Runtime.Vz；后续非角色physics又将Vz分别加到SourceRuleZ和按倍率加到viewZ，因而有改变源轨迹的静态候选。

有区分力的推断初态是合法live target、HP>0、YInt=-40、Vz0、源Z380/384：identity源差4不加速，1152/730视图整数599/605差6可能加+0.4。具体共享锚点、取整、source初始化与空槽fallback仍须沿已有夹具核对；该算例未运行，不能报告自然Scene首差。

当前正式OID875/type3的 `c/ank/a/atk.dat` 与Unity暂存SHA同为FC34315BD542BC9E721419E6390103D9F165A055CFCEC45D1A66D9BAE1894CF8。安可511/512/513的OPoint生成875/action50，其hit_Fa3/next55可进入hit_Fa7/action55/56，数据链可达；自然阳性是否在源差4邻接进入这一分支尚未观测。既有 `NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001` 已留D-024域待证，不创建第二套泛化backlog。

后续若建立同态前置，只在已有 `NTSD28Q07NonCharacterHitFa7EditorTests` 的正式875/action55 live-target夹具检查identity/fixed源差4与一tick SourceRuleZ；不改空槽fallback、不改DAT、不扫自然输入矩阵。此项只读候选不计已确认生产首差，当前平台阴影的单项验收保持唯一ONE。
