# C044 正式 DAT 受控逐 pass 源证据

当前权威：根 EXE SHA-256 336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；对应 playable Core 的 BattleWorld28::advance_catch_relations() 与 finalize_horizontal_hit_impulses()。输入为正式 decoded_dat/c/gaa/gaa.dat OID16 action128 和 c/nar/nar.dat OID2 action130。生产 DAT 未改。

诊断程序 Tools/NTSD28Q07Diagnostics/gaa_negative_decrease_control_probe.cpp 用当前 Core/Playable 源码编译，exit 0，compile-v1.txt 无诊断。它建立 reciprocal relation、双方动作计数7/8、初始速度0；分别给 timeout 2、3、4，记录抓取 pass 与水平冲量结算 pass。两次独立运行 exit 0，formal-dat-v1/source-pass.csv 与 formal-dat-v2/source-pass.csv 的 SHA-256 均为 1ADD4554DAD2BD1098FFF056CE0C56071288A05F02E74067AAA1C7C29B2FE0ED。

| 初始 timeout | 抓取 pass 后 | 结算 pass 后 |
| --- | --- | --- |
| 2 | timeout -1；released 1、thrown 0；动作 0/181，动作计数 7/8 保留；双方 pending count 1，受害者 X/Y=-4/-3，受害者 motion=0/0 | 双方 pending count 0；受害者 motion=-4/-3 |
| 3 | timeout 0；released 0、thrown 1；动作 249/180；受害者动作计数8，pending count 0 | 无新增冲量；受害者 motion=3/-5 |
| 4 | timeout 1；released 0、thrown 1；其余同 timeout 3 | 无新增冲量；受害者 motion=3/-5 |

这证明当前正式源码在负 decrease 跨过零时赋值待结算冲量并提前结束，且不改动作计数；等于零不进入该分支。当前 Unity BattleCpointWriter.RunKind1 同分支却立即写 Runtime.Vx/Vy、将双方 AttackingCounter 置1、没有写双方 HitCount；现有 Unity 测试把旧行为当预期。下一包先修订定向测试取得 RED，再修改共用 writer，通过原 Editor 与相邻回归。

限制：这是受控低 timeout 源码实验。正式根 LFR 初态不能承载抓取关系/timeout；四个自然近距案例到负 decrease 帧时 timeout 仍35，未跨零。因此本报告不证明根 EXE 或原 Unity Battle Scene 自然同态，C044/Q07 与总目标继续开放。
