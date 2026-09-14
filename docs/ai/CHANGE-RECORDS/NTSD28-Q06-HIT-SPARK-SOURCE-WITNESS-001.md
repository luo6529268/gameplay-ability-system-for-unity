<!-- CHANGE-RECORD
id: NTSD28-Q06-HIT-SPARK-SOURCE-WITNESS-001
status: IN_PROGRESS
change-kind: HIT_SPARK_SOURCE_TRANSACTION_WITNESS
code-path: Tools/NTSD28AuthorityTrace/hit_spark_transaction_witness.cpp
authority: Current battle_world.cpp append_confirmed_native_spark and its unarmored/selected-armor feedback/reduced callers in playable build closure.
evidence: Production normal Character returns through LF2Entity.RecordKind0Hit; old helper uses legacy RNG/current centers/attacker Z/right shift, unlike original CRT/snapshot/target Z/truncating division and encoded/index selection.
-->

# 命中火花完整事务原见证

IN_PROGRESS / SOURCE_ONLY。仅新CPP，通过原World候选与三个实际damage/feedback/reduced入口触发spark；不复制原spark算法或改正式source/EXE。

覆盖原始itr序号、effect1分支、fall0/60/61、itr.spark -1/0/99/100/199/200、所选armor.spark优先级、当前与snapshot中心分离、cover/负数取整、depth/slot owner、容量0/9/10与无效snapshot。保存spark数组和完整CRT返回值/state/调用序列；capacity拒绝前后随机不得混淆。位置/动作在候选冻结后扰动属诊断初值，不冒充完整driver可达性。

已核对真实Unity入口：LF2Character.Hit委托DatHitResolver；普通分支ApplyStandardCharacterDamage后调用RecordKind0Hit并立即return，不走底部旧SpawnSpark。weapon/special也调用RecordKind0Hit。CurrentItrIndex仅旧Character BeforeDispatch写入，generic BeforeDispatch为空；后继实现要明确瞬时index来源，不直接相信旧持久值。

验证build/run/repeat/原身份与可复用向量；后继Unity另准确Record，公共emitter和调用上下文共同闭合，已有AddHitRecord及C01/C25生命周期不重做。没有persistent字段/schema/模块/关闭变更，无Scene/资源/非战斗/Server改动；禁止computer-use。回滚仅本差量且按规则授权。
已写438源输入：selection216/armor36/geometry144/capacity36/missing6；原先冻结三candidate后扰动位置/current/snapshot，保存两host数组与完整CRT。尚未编译运行，生产未改。
