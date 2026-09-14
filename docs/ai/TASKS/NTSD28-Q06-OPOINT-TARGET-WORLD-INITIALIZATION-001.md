> 当前状态 VERIFIED / CONFIGURED_WORLD_BINDING_ONLY；最终证据见同名Record末节。以下保留形成过程，剩余父任务继续。



# Renderer OPoint出生前绑定所属World

准确两脚本。已确认CreateLogicObject只有nativeWeaponPieceSpawn传resetWorld，普通/Multi不传；非角色Init在Register前读取config因此落到全局CharacterAnimtorManager。logic-only factory已Get(...world)。当target World确实有对应catalog config时，两renderer创建路径使用现有Get(...world)提前绑定；未配置World的编辑器/兼容预览仍保留原global fallback，避免扩大非战斗范围。late任务显式targetWorld=spawner.Match，现有parent指针/cleanup/队列/关闭阶段保持。

真实RED：C25L Play首49向量已过，renderer composite缺fragments/ordinary子frameState错误；原始报告与Scene checksum unchanged/borrowers2→2保存。加入probe FrameCache定义引用诊断以闭合后续失败定位；重跑两factory正式数据组合、前置focused、SelfCheck/关闭全0。目标World配置访问与pool API均已有，不新增服务或修改registry/Unity/GAS/Scene/资源。回滚只该条件差量且先批准；父C25L/深度lives仍待联验，不能先关。
