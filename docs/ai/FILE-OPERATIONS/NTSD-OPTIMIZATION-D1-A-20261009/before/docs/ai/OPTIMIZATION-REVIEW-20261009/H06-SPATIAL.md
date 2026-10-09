# H06-SPATIAL：有条件的空间候选裁剪

父项H-06；状态CONDITIONAL_PROPOSAL / NOT_AUTHORIZED。现有Role-aware Direct/Sweep/tree已有代码，不默认创建新网格。
入口：Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs的ResolveFormalCollectorMode与实际Role候选路由。先查候选计数、角色过滤/重建/几何成本、fallback分布，不把1000*999/2当实测pair数。

进入条件：在明确Role比较配置下，已有数据证明仍有大量可安全排除的非相交候选，且索引维护成本小于预期收益。密集且确实相交的pairs无法靠网格消除；分散收益不能外推混战。
方法比较：优先复用现有sweep/tree与角色分组；网格只有出现明确缺口才提案。按实际ITR/BDY几何构造保守包络并覆盖所有相交格，处理多矩形、Z宽、平台、异常/超大包络及fallback。
是否需要位移包络取决于索引更新时点及复用间隔；若跨时点复用必须覆盖期间运动，不能随意加一份最大位移当正确性证明。绝不假设八邻格永远足够。
输出候选去重、稳定消费顺序、slot/generation、RNG、命中/OPoint保持；seal前预热，溢出/异常回退不漏检测。

最小验证：小规模边界与超尺寸/跨格/高速/复用负例；与Brute候选及完整Driver逐tick比较；四形状中受影响范围复用/补证；包含索引重建的双布局实际成本。
完成：采用/不采用具名算法并给出正确性、含维护成本的收益及fallback依据；不得只给局部查询加速。H06父门和D1待决不改变。依赖H-09/M-09与准确几何/索引时点合同，M-11/Burst不是自动前置。
