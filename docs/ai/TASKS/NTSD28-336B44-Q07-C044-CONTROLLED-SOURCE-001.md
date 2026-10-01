# NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001

状态：`VERIFIED / CONTROLLED_SOURCE_ONLY`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C044。权威为当前正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable Core `BattleWorld28::advance_catch_relations()` 与 `finalize_horizontal_hit_impulses()`，DAT 用正式 `resources/runtime/decoded_dat/c/gaa/gaa.dat` OID16 和 `c/nar/nar.dat` OID2。自然四路径已证末端 timeout35、不跨零；见 C044 自然报告。

只新增 `Tools/NTSD28Q07Diagnostics/gaa_negative_decrease_control_probe.cpp` 和本包证据目录。用正式 DAT 的 OID16 action128 kind-1 与 OID2 action130 kind-2 建立**受控** reciprocal relation，分别将 timeout 设为2、3、4，双方 frame counter 设为7/8，motion初值0。只调用正式 World 的 `advance_catch_relations()` 一次，记录动作、timeout、帧计数、关系、pending impulse 的X/Y/count、motion与 pass计数；再单独调用 `finalize_horizontal_hit_impulses()`，记录实际速度和 pending 消费。timeout2应跨零并跳过同轮 throwvx；3等于零、4大于零均不得进入跨零分支。不能用旧 2.4/C# 或自写预期覆盖正式源码。

本包不证明正式根EXE自然发生该低timeout；LFR初态不承载抓取关系/timeout，受控样本不冒充根或原 Battle Scene。同源 `battle_world_tests.cpp` 的当前规则只作交叉检查，正式 playable Core 是规则入口。阳性后另建 Unity生产共用 writer 的Task/Change、先RED后修；禁止直接修改 DAT、Scene、GAS框架、非战斗、已批准例外或现有其它脏工作。编译、三边界、同源码复跑、账本与保护哈希如实记录。回滚只审阅本ID新诊断文件。

结果：正式 DAT 受控三边界与两次同源复跑通过，CSV 同 SHA；timeout 2 抓取 pass 只写双方 pending count 1、受害者冲量 -4/-3 而 motion 0/0，finalizer 才写 motion -4/-3；动作计数 7/8 保留。timeout 3/4 均走投掷。只关闭本源证据包，见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-CONTROLLED-SOURCE-001/REPORT.md)；Unity、正式根自然 Scene 与父 C044 仍开放。
