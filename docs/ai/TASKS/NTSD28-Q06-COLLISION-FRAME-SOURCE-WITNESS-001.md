> 最新追加出口：336 source输入限定VERIFIED，expanded两遍SHA43f6713de73a76ba5f2c07c1627bf058ecf9e1706baa93701112200ef1ff7499；旧252字节前缀保留。新增84中42两端current=snapshot、42混合端点，不能称全部为正式driver输入。

> VERIFIED / SOURCE_MODEL_WITNESS_ONLY；252/3780、重复一致、原EXE/75源身份保持。下一COLLISION-FRAME-UNITY-001；完整限制与报告见同ID artifact。以下是实施前合同。

# 碰撞快照原源码对照

IN_PROGRESS / SOURCE_ONLY。Change Record 同 ID，仅新增 Tools/NTSD28AuthorityTrace/collision_frame_lookup_witness.cpp。

原 snapshot_actions 只冻结动作号；消费者从当前 definition 查询 snapshot。通过分离当前动作与快照动作、替换当前定义，观察原几何候选和 kind1 抓取读帧。implicit 0..998/declared999/null 边界由原 DAT parser 提供，runner 不自行实现目标规则。

出口为重复稳定的原结果、完整初值及 raw 输出；不是 Unity 实现或正式 EXE Play 验收。随后准确规划 LF2Entity collision getter、BruteForceSceneQuery current/Prev2 helper 和 BattleHitCandidatePairSnapshotFactory 的成组迁移，并覆盖 CPoint 与缓存/no-op 消费者。CPoint raw/throw writer 未包含在本包。
