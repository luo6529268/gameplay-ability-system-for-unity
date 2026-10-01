# C040 我爱罗→大蛇丸自然分源筛选

状态：`SCOPED_NEGATIVE / C040_REBASELINE_ON_REACH`。本包只筛正式完整 GameSession 的自然可达性；没有改正式源、DAT、Unity 或场景。根正式 EXE SHA-256 本轮复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

静态候选：OID16 `gaa.dat` action365 的 kind3 可进入 action370 抓取链，action376 `injury=100/vaction=130`；OID65 `ank.dat` frame130 为 kind2，受伤动作131/132的相对挂点X比130差10。正式源码 `BattleWorld28::settle_catch_relations` 的定位分源必须在结算时保留不同的受害者当前动作且停顿非零，静态DAT差异本身不足以成立。

新增独立诊断 `Tools/NTSD28Q07Diagnostics/gaa_ank_held_position_lfr_probe.cpp`；沿用既有当前源码编译闭包，`g++` `-O1 -Wall -Wextra -Wpedantic` 编译 exit0、0诊断。初态我爱罗OID16/X500、目标大蛇丸OID65/X780或1200、Y0/Z400、HP/MP500、mode0、seed682973786、action365、中性输入，各运行120完整tick。本包编译1次，近远每例重复运行2次；各同输入的逐tick CSV、RNG CSV、LFR字节 SHA-256 均完全相同。

| 条件 | 自然抓取 | 持有伤害 | 结算后 C040 完整分源条件 |
| --- | --- | --- | --- |
| X780 | tick1 | tick28 | 0/120 |
| X1200 | 无 | 无 | 0/120 |

X780 的 tick28 前为抓取者 action376/vaction130、受害者 action130/hold0；伤害后受害者 action130/hold-3。tick29～30 仍为 action130、hold-2/-1，当前动作与 vaction 相同；tick31 抓取者切到 vaction180，受害者 hold归零，故也无完整分源。静态“130受伤可进131/132”在这条自然持有链没有发生，不能把静态挂点差当作 C040 阳性。X1200 为无抓取反例。两次运行的 `first_c040_candidate=-1` 均一致。

本批无真实分源前置，因此按任务合同不运行根 EXE LFR 同态或 Unity Play，也不修改生产。C040/Q07/总目标开放；后续只对其它正式可达初态/输入寻找阳性，不重复本批两个条件。原始 CSV/RNG/LFR 和编译参数/输出均保留在本目录对应 `a365-x780-v1/v2`、`a365-x1200-v1/v2`。
