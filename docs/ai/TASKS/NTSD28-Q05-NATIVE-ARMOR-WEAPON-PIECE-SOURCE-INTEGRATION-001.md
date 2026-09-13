# Q05 Native护甲与武器碎片来源

状态 IN_PROGRESS / TEST_FIRST。父NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001继续；BMP/stats实际manager由NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001交付，不重做。

下一修改前先确认准确code-path并建立Change Record。原版入口dat_parser.cpp的armor/weapon_piece context与combat_records.cpp armor decoder、battle_world.cpp武器销毁消费；确认build.ps1 $coreSources参与性。护甲沿用LF2ArmorData，保留18正式normalized匹配，补literal/strict/list/关闭语义有差异部分。weapon_piece结构必须保留root/group/variant、group amount、piece_end和原版最多5组/每组5variant规则，实际manager绑定definition载体，不能从旧扁平AST猜结构。

先新原版边界fixture及全405结构/18armor/三indexed weapon piece witness，RED后source专用实现，再330catalog与非法candidate、前BMP/stats/frame/strength和旧138回归、Unity compile/SelfCheck。记录raw/typed所有新增identity字段及Q06碎片consumer。不能宣称metadata已载入等于碎片战斗已对齐。

现schema12/20/23/1/1，目标同窗口13/21/24/2/2；identity/carrier等后继保持。Q07资源前置、Q06/Q09/Q10回访、stage.dat USER_HOLD、Unity/GAS与非战斗、Scene旧SHA/归属pending、33ms/3ms和十一阶段及用户例外不变。

限定交付：FOCUSED_TEST_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY，39/405/330/1829/SelfCheck证据在同ID REPORT。下一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，按父步骤2推进；上文TEST_FIRST为历史。
