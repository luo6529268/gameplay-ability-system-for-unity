<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-UNITY-001
status: VERIFIED
change-kind: TEST_ONLY_FULL_FRAME_DRIVER_JOIN
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FullFrameDriverEditorTests.cs
authority: Original450 full-driver cases/1350 ticks, formal Normal/Practice mode defaults1/1/2 and neutral input.
evidence: Source witness e50dbee6628622e08c6deb1697f9aa83887775725b4768f82ab654989db9fce5; parent2676 endpoints and state18/fragment joins already verified.
-->

# Unity完整帧driver联合验收

准确一个新Editor测试脚本；只加证据，不先改生产。复现450 fixture DAT/input/initial字段，通过已有logic factory的generic native birth入口创建type0/3/4，再施加原显式初值；原初始raw47绑定字段先比较，不把初始化首差算作tick规则。当前raw另3MISSING保留显式排除，不宣称raw50全部对齐。

两profile Authority400/MobileExtended × 两frame/physics后端Legacy/DataOriented ×5组，每例中性输入走同一NTSDBattleTickSystem三tick。每tick检查源raw47、生存与实体数、sound latch/真实HP-MP消耗统计、934原frame事件路径/顺序/worldX；源0 NativeRandom/CRT严格一致。Unity独立C17随机武器例外单独记录calls并检查未生成额外实体，不允许隐藏任何下游raw首差。cache仅不可变fixture DAT与源JSON，不能改共享frame。

若发现差异，先定位initial vs首tick再查原caller，不为通过测试而删字段；准确新增生产Record后修复，不能扩大本test-only Record为未声明代码改动。正式Scene Play用同fixture完整driver/高动作/成本fallback代表场景，并核对场景checksum/借还/关闭；不声称物理按键、真实图片或全部角色技能。无生产persistent/schema/服务/关闭阶段变化，所有新增fixture实体用既有World关闭释放。先compile/focused；无生产改变时不重复已经通过的全SelfCheck，按新差异必要性再决定。

回滚仅本测试差量且按删除规则获批；禁止非战斗/Scene/资源/Unity-GAS/Gen/Plugins/Server改动或computer-use。父frame及总目标仍IN_PROGRESS。

CODE_WRITTEN：20分组/1800场景/5400完整tick计划，source450按初值与每tick原raw47比较并保留3MISSING；所有Source音频为frame source1/channel-1，对比队列路径顺序/worldX；源Native/CRT0，C17每tick独立1且额外entity被计数拒绝。fixture使用原generic初始化，再施加显式HP/MP/bound/模式/hold/位置/动作，不修改生产。先运行Authority400/Legacy五组观察first differences，再决定是否扩展两后端/两profile。

实际首轮5/5 PASS（50a83c119aef4720b6028da0ff03217a）：Authority400/Legacy全部450场景/1350tick，初始与逐tick raw47/声音/消耗/native随机数均零差异。开始补Play probe：56代表场景×两actual factory×两frame/physics后端=224/672tick，覆盖正负next/空中999、高当前显式与implicit、6类成本fallback、type3 hold和倍率。Fixture可选Renderer分支，默认logic-only不变；仅Renderer fixture结束前回收活实体再走既有World关闭。Scene driver暂停后运行，核对checksum、pool借用恢复；不认证素材或物理按键。

剩余15/15 PASS（b8936778bbe54597a7504c1ee14b132d），合计20分组/1800场景/5400tick且初始与raw47/声音/消耗零差异。父frame明确要求snapshot/replay，现同一测试脚本追加8组（两profile×两backend×本地/转移restore）：4代表高998/成本fallback1101/编码1299，在tick1捕获、基线推进2/3，再restore重放2/3并核对原raw/声音与完整checksum。不重写旧descriptor静态244测试；此验证补连续tick恢复出口。

Replay首轮e83f9be70f2c48dd9e2f0a23066852e9：本地4PASS/预先生成目标的转移4FAIL，32条仅canonical allocationEpoch2/1，其余raw/声音/checksum相同。已核对Snapshot无epoch字段、共享Kernel topology restore未保留它；该formal recovery边界原已明确未实现。失败不删除或归一化，独立Task SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT-001留痕。当前测试改名明确FreshTransferredSnapshot，并为接收方只准备空World/catalog，不先创造额外出生历史，继续全部raw47含epoch比较；这验证帧恢复并不关闭旧World恢复缺口。生产/Server/Schema保持。

## 最终限定出口（2026-09-14 04:00Z）

VERIFIED / OFFLINE_NATIVE_FRAME_TRANSACTION_SCOPE。源450/1350连续driver、frame/lifecycle错误0、934声音事件；重复SHA e50dbee6628622e08c6deb1697f9aa83887775725b4768f82ab654989db9fce5，正式EXE与75源码身份不变。不是正式EXE/图像/全角色对局认证。

Unity实际：50a83c119aef4720b6028da0ff03217a 的Authority400/Legacy五组5/5；b8936778bbe54597a7504c1ee14b132d其余15/15，合计20组1800场景/5400完整tick，初始及每tick raw47、source生存、声音路径/次序/worldX、sound latch/两消耗统计/Native RNG一致；3MISSING明确保留。Unity C17独立流每tick1次共5400，未额外出生，已批准例外未改。

Replay首轮4本地PASS/4预分配目标转移FAIL，原32条canonical allocationEpoch2/1保留于replay-8-first.xml和replay-populated-target；不是local Generation，也没有归一化。SlotSnapshot缺epoch及shared topology restore未保留该字段，formal recovery原S0已明确未实现，独立SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT Task继续OBSERVED。随后将同初始出生历史的新接收World场景显式命名FreshTransferredSnapshot，053c0e815f0e425b8bafffe5d62b543a 8/8 PASS、32场景/64重放tick，含epoch在内raw47/声音/完整checksum相同；这不关闭已使用World的恢复缺口，也不推广到任意跨World回滚。

真实Scene Play-224-pass.json：56代表输入×两actual factory×两frame/physics后端=224场景/672完整tick，raw47/声音/native随机全部通过，旧Scene checksum不变、Renderer2→2。Shutdown-pass.json：恢复4→4、World/slot/logic/render全部0、两帧Stopped，已正常退出Play。高动作857/998显式及implicit、正负999/编码1299、成本不足fallback/恰好支付、type3 hold与倍率都有实际完整driver范围；不认证物理按键或真实图片。

本轮只新增一个源诊断CPP与一个Unity Editor测试脚本，未修改生产逻辑；沿用同一生产实现最近完整SelfCheck（03:22:37Z PASS，OPOINT-WEAPON-HP-BIRTH目录SelfCheck-final-pass.result），本轮未重复运行。最终CS error0、Editor idle非Play/非compiling、Scene dirtyfalse/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。Ledger/diff-check交付前运行；无computer-use/非战斗/框架/正式资源/Server/Gen/Plugins/提交修改。

父frame core2676、carrier/snapshot386、已关death/state9998/fragment/state18/普通出生/上游pending证据与本次连续driver/Play联合，闭合本C25帧推进、成本、声音事件、previous078、fragment和生命周期职责。剩余Native frame reader/direct writer/input/碰撞/held/普通生成字段消费者不在此已关闭职责内，立即回NATIVE-FRAME-RUNTIME-READER-MIGRATION继续；display其它出生/post/Q07及raw3缺字段仍未完成。既有definition/fusion后继业务与Q10实际WAV边界保持，不凭这一包宣称全部B3/B1-B12完成。
