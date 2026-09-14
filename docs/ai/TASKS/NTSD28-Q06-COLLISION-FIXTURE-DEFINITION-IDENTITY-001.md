> VERIFIED / TEST_ONLY；两处同源夹具/旧collision断言纠正，完整SelfCheck05:48:49Z通过。父collision候选资格未关闭。

# 修复旧碰撞自检夹具定义

IN_PROGRESS / TEST_ONLY。准确Record同ID；仅SelfCheck.CheckLooseQuadtreeShadowBroadphaseContracts的invalidFirst用真实空frame定义创建，不只修改缓存指针。原顺序/唯一目标断言保留；之后完整SelfCheck重验，禁止恢复生产current fallback。
