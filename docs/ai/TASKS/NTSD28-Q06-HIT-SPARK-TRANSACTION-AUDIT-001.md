> 当前优先级调整：READY_NOW，需先完成Spark事务，才能正确实现96个非角色首type0的feedback-only返回；之后NONCHARACTER-ARMOR-FEEDBACK→UNARMORED-WEAPON-REACTION→回BDEFEND256/完整driver。Bdefend角色字段写入已落地，父仍进行中。下文旧“先Bdefend完成”的顺序被本依赖替代。

# 命中火花事务与随机流

READY_AFTER_UNARMORED_BDEFEND。完整driver两例×四组原Native0/CRT2，UnityNative0/CRT0/legacy3；只有其中1次legacy属于既有C17稀疏掉落例外。剩余2次不能豁免，需按原CRT顺序修正。

明确原入口 battle_world.cpp.emit_native_standard_hit_spark（约780起，858/859先Y再X调用random.crt_next()%9-4）。Unity logic-only LF2CharacterDatHitResolver.SpawnSpark约828/829仍调用_victim.BattleRandInt两次；主LF2CharacterHitResolver是否委托此resolver、两factory实际路由必须闭合后再改。不要凭2次数值就只换随机函数。

应核对整个原事务：owner选择与slot/Z平局、owner容量拒绝在RNG前、candidate itr index与effect1/显式spark编码选择、fall0含义、当前definition的snapshot几何、cover/中心/边缘/取整、先Y后X CRT、AddHitRecord与snapshot/render handoff。现Unity SpawnSpark读current.Frame.D.center而原读snapshot，也是待证差异；当前两例中心都是0，不能证明几何已对齐。避免使用未核验的Unity默认Fall去替代原显式fall0。

复用已验证NativeSpark生命周期（C01/C25）不重做，新增source命中事务见证需包含不同中心、多个itr、容量边界和完整CRT值/顺序，然后actual两factory与回放/池复用验证。保留既有图片/表现例外，不借机替换全部火花图片或重构SpriteRenderer。

准确Change Record前仅审计；不得改全局BattleRandInt/随机算法/C17例外或将表现随机反写逻辑。无框架/Scene/非战斗/Server更改，禁止computer-use。完成后回原完整driver的4个失败测试，不修改期望掩盖旧随机流。
