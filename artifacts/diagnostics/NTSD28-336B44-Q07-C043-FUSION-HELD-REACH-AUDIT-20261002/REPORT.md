# 336B44 Q07/C043 失效持有关系的自然入口只读筛选

状态：`STATIC_CANDIDATE / NATURAL_REACH_UNPROVEN`。本轮只读正式 playable 源码与正式 decoded fusion.dat；未运行正式 EXE、源码会话或 Unity，未修改 DAT、场景、战斗脚本。

当前正式 `source/ntsd28_core/src/simulation/battle_world.cpp` 的 `BattleWorld28::despawn`（约 1537～1547）先调用 `clear_entity_links` 再删除槽位。`clear_entity_links`（约 1550～1566）扫描 active 对象，清除指向该槽位的 active 关系；所以普通 despawn 不能直接当作 C043 “父槽缺失、子体仍为负关系”的自然正例。

同一 live 文件 `BattleWorld28::advance_native_fusions`（约 2744～2863）在选择伙伴并确认融合后，执行 `fusion_suspended_[partner_slot]=std::move(slots_[partner_slot])`，随后直接 `slots_[partner_slot].reset()`，所检分支没有调用 `clear_entity_links(partner_slot)`。伙伴筛选核对 object_type、存活、组别、state、HP、位置等，但所检条件未拒绝伙伴持有 kind-2 子体。正式 `decoded_dat/data/fusion.dat` 有 7+8→51、10+11→52 两条记录，因此“伙伴已持有 kind-2 子体且同 tick 融合”是有内容和源码路径的候选，不是已观察到的运行入口。

`SimulationTickDriver28::step` 先执行首轮 held-refill（约 686 行），再融合（约 708～710 行），最后执行命中后第二轮 held-refill（约 915 行）；因此这条候选若成立，有同 tick 后轮检测窗口。`BattleWorld28::settle_held_refill_objects`（约 7949～7987）在负关系子体的父槽缺失或互指不匹配时只清子体 `interaction_state`。这与既有 Unity C043 受控修复对应；但没有运行结果证明上述融合条件能在当前正式战斗中自然同 tick 成立，也没有证明 EXE 与 Unity 在该入口的先后顺序/结果一致。下一有界门是先由正式 DAT/输入形成伙伴持有 kind-2 子体和融合的同条件完整 GameSession tick；若不能成立，保持 C043 条件门并转下一个可达首差，不用手工破坏关系冒充自然正例。

权威身份：根正式 NTSD2.8-Logan.exe SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本报告没有重新计算身份哈希，沿用本轮已经核对的当前总表身份。
