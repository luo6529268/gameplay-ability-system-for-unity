# Q07 千代自然链同步 RNG 首差限定验收

状态：`VERIFIED_SCOPED_RNG_FIRST_DIFFERENCE / Q07_OPEN`。本包只证明正式/Unity 同一固定输入序列的首个**同步 RNG 调用数**分叉时点和千代 tick54 的下游选招；不证明两个 World、所有实体状态、随机表初态或最终画面完全同态，也不推断生产规则缺陷。

正式权威为根正式 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，paired playable 及原 `NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001/run2/formal-root-replay-trace.jsonl`。该 trace 与正式根 LFR 已在上游子包核对。Unity 只用原项目原 Editor 的保存 Battle Scene、正式 LoganRuntime 内容及原 `NTSD28Q07ChiyoNaturalBattlePlayProbeEditor` 的 direct-canonical 固定 HP150 千代/鸣人 120 tick 输入；新增 opt-in `rngAudit` 仅读 `NativeRandom.CaptureScalarState`，不改生产随机数、DAT、资源或场景。原 Editor 程序集 UTC 2026-09-28 15:52:18 晚于探针源码15:49:26；MCP 过滤该探针编译错误为0。

原始 Unity 报告：`../NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/chiyo-canonical-rng-20260928-01.json`，SHA-256 `03765F8BBCCB650C66F10A53D1B2D7501851C11CB1B90174D18F734F690E9DE5`。报告保存120条样本、`PASS_SCOPED_NATURAL_CHAIN`：相对 tick18/27生OID419/854、56/57/68到千代420/傀儡400/407；phase、OID419数、OID854数各120/120，千代动作119/120，唯一所选动作差为tick54正式60/Unity65。报告退出 Play、有序关闭并解绑，World对象/runtime slot/pool borrower均0；MCP读回Battle Scene clean，四保护资产SHA与入场相同，探针错误Console0。

逐 tick 以正式 trace `rng.customCalls/customIndex/customCounter/lastCallSite` 对 Unity 新报告的同步 RNG 后态比较：tick1–19调用数均为0；**首个调用数差在相对tick20**，正式 `7`、Unity `6`，当 tick 两端末调用点都为十进制56（`0x38`）。tick21为14/13，tick22为28/26，差异随后扩大或缩小，不能概括成固定启动偏移。正式 tick18生OID419，tick19 trace含OID850和OID204；这只是首差附近的实体上下文，Unity本探针未捕获这两种实体的逐slot完整状态。正式 `native_ai.cpp` 的 `0x38` 属于方向门后、目标/subject合法时的同步AI随机调用，Unity `AiDecisionKernel.cs` 有对应 `RandAt(...,0x38u,...)`；缺少逐调用/资格记录，**不能断言究竟是哪一个实体或哪一个门少调用**，也不能从当前证据归咎于D-024比例坐标。

相对 tick53正式/Unity同步调用数1784/1762，tick54前态1784/1762，后态1856/1834，tick55为1927/1905；tick54两端本 tick 都增加72次，末调用点均为`0x82`。正式千代interaction0、动作60、后态index1856/counter622；Unity千代入tick LinkState0、HitConfirmEa0、动作65、后态index1834/counter600，探针按该后态表项计算`standingAttackRngResult=1`。这证明 Unity 当 tick 的 `0x82` 选择为1、正式为0；它发生在已有22次调用差之后，因此不能把这单点选招当作首个规则差异。表内容/World同初态未证，仍保留正式行为和Unity行为的分层结论。

下一精确门：只在相对 tick19→20 比较两端**完整逐调用序列**，先找第一个多/少的调用点；若归属 `0x38`，再核该AI调用的主体slot、target、方向/距离门、AI level、序号和结果，并核同 tick 子体真实出生/可见状态。两端末调用点同为 `0x38` 并不证明少的那一次也属于 `0x38`。不再重复千代tick54或扩角色/时机矩阵。若源/Unity初始World不同，先显式标明并对齐对应输入条件，不得为令随机结果相同而改DAT或注入随机调用。Q07/BATCH-04、D-024碰撞域和总目标继续开放。

治理验证：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exit0/PASSED，985 Records/18 governed diff code files；`git diff --check` exit0。未运行本包无关的全量 SelfCheck、其它角色或最终 Q12 整场验收。
