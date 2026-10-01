# NTSD28-336B44-Q07-C043-INVALID-HELD-TAIL-001

状态：RUNTIME_PENDING（原Battle Scene受控Play通过；正式根EXE自然同条件待）。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，G1/BATCH-04/Q07/C043。

权威：当前336B44正式 EXE 对应 playable battle_world.cpp 的 BattleWorld28::settle_held_refill_objects，失效负关系（父槽越界、父实体不存在、双向槽不匹配）在共同尾将子体 interaction_state 写0，仅此关系字段；linked_parent_slot、持有方关系、动作、运动和 RNG 不因该尾改动。当前源码 battle_world_tests.cpp 的 invalid reciprocal 用例直接断言状态0。旧 B1E13 的 B6 preserve 测试只属版本历史。

Unity 首差：SimulationQueryAndLinkModule.HeldObjectProcessAll 对三种失效负关系只记失败/trace 后 continue，保持负 LinkState；两次持有 pass 会重复报失效。已定位旧 SimulationQueryAndLinkModuleEditorTests、NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests、BattleRuntimeSelfCheck 明确期待旧行为。

范围：仅 SimulationQueryAndLinkModule.cs 的共用失效分支/诊断事件、上述三个测试文件。先将当前权威聚焦期望写入测试并取得 RED，再改生产；不修改 DAT、Scene、Prefab、项目设置、非战斗、对象生命周期正常解绑顺序或架构。不得通过角色/OID特殊条件追绿。旧历史 Record 不删除；本包以新版裁决并回链当前总表。

验收：out-of-range、inactive、reciprocal mismatch 的首扫只清 LinkState，HolderStableId/其它 sentinel/动作/运动/RNG 不动；同tick或下tick第二扫不再报同一失效；slot0、高槽、有效关系和生命周期解绑边界不回归；事件 Before/After 与 outcome 真实反映清理。原 Editor 0错、聚焦 EditMode 与定向 Play/SelfCheck 能跑到目标检查、退出池/Scene保护状态如实记录。源侧当前源码对照、根正式 EXE 在失效关系自然可达后另验，不能用旧测试或局部通过代替整个 Q07。

风险：旧 B6 测试/诊断读取方可能依赖重复失败计数与 preserved 事件；按实际引用逐项更新，不顺手改 unrelated schema。回滚仅本包行级改动，保留用户和前序任务全部脏文件；需回退/删除按仓库批准规则。

当前出口（2026-10-01）：原Editor先RED2/2，修后聚焦7/7、完整tick失效3/3、正常持有6/6；删除多余快照刷新后的最终完整tick3/3与原Battle Scene请求式Play七例通过，Editor退出idle、Battle/Menu/GameConfig/Mode Asset四SHA稳定。正式根EXE自然同条件、完整SelfCheck未验；C043限定范围通过，Q07和总目标开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-INVALID-HELD-TAIL-001/REPORT.md)。
