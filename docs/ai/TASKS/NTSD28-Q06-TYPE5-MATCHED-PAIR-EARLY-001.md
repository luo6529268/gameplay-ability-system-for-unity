VERIFIED / DECLARED_NO_ARMOR_TYPE5_MATCHED_SCOPE。source138/78933、双跑一致；Unity四138 before0/diff0，24/24相关回归及另旧type3四项4/4，SelfCheck15:07:54Z、Play552 15:08:59Z、关闭15:09:11Z全部PASS。最后matched本地回放job a5cb22d424fe49c8b143881be3a41db2于15:14:32Z终态2/2 PASS，40代表/配置×两配置=80场景、160重放tick。XML已归档tests-matched-replay-2-pass.xml。只在原World恢复，跨World epoch缺口未关闭；完整type5/noncharacter/Q06/资源迁移未据此关闭。

以下为历史准备与实施契约，以上最终状态优先：

> READY_NOW：普通585事务已SelfCheck/Play2340/关闭通过，本Task为下一唯一入口。源见证还需加入正常非match下prev13/snapshot12、非零CollisionYReference、正link reciprocal child rests的小向量，以覆盖本次复核已读但585未触发的分支；必须明确区分与matched早返不写damage的向量。

# Type5初始matched 3005/3006早返与native帧读取

READY_AFTER_TYPE5_ORDINARY_RUNTIME。本Task在type5领域整体关闭前必做；当前585只覆盖普通命中。完整证据由独立reviewer定位，尚无新源动态见证。

正式battle_world.cpp 6616：完成unarmored prelude/armor/live-rest/current first-body后，所有非角色target读双方当前frame state；3005/3005或3006/3006执行standard rest→target reset→attacker reset→release attacker hold并早返，位于HP/stat/Bdefend/fall/motion/音效/spark之前。后段6954 type3-only continuation保持，不能同时扩大。

reset读action_latch frame hit_Uj，缺失/0取20；raw action、frame_counter0、pending XYZ0；保留contribution_count/瞬时velocity/latch/prev/snapshot/HP/MP/Fall/Bdefend/team/link/special-latch。hold：非负link只对attacker正hold取负；负link且有效parent先复制原attacker hold到parent，再仅parent正值取负，attacker保留；无parent不fallback。

Unity现TryApplyNativeType3MatchedPairEarlyBranch事务结构可复用，但不能只加type5 gate：ApplyNativeType3PairReset仍GetFrameDataById(latch)旧上限857，latch900→hit_Uj71会误选20；reset到声明frame900旧绑定Frame.D为null也错误。入口GetState依赖旧cache，应核对native当前frame。Shadow CanProjectType3StateSyncDamageWriterEffect仍type3/旧getter，type5新普通预测也要在正确位置增加独立早返预测。

先源/Unity RED：两种match与cross-state负例；current/latch/snapshot分离、hit_Uj0、latch900及reset action900；hold正/零/负、负link有效/无效parent；live vrest和type0 feedback先返/first-body顺序guard。用非零HP/stat/fall/Bdefend/velocity/impulse/count/status哨兵证明不走普通伤害；三个RNG、声音、spark零增量；显式释放hold的finalizer保留count语义。两profile/directShadow/真实candidate，复用原type3回归，保持585和weapon2100。

新增源/test/生产前各建准确Record，候选生产DamageWriter/HitPlan；不新增schema/queue/service、不改生命周期。其他非角色武器同态组合亦属于同一native gate的全局回访线索，本Task不得凭type5通过宣称其全部已覆盖。破甲selected-route继续归NONCHARACTER-REDUCED上下文任务。禁止computer-use、Scene/资源/非战斗/Server改动。
