# 完整帧driver联合验收

## 最终限定出口（2026-09-14 04:00Z）

VERIFIED / OFFLINE_NATIVE_FRAME_TRANSACTION_SCOPE。源450/1350连续driver、frame/lifecycle错误0、934声音事件；重复SHA e50dbee6628622e08c6deb1697f9aa83887775725b4768f82ab654989db9fce5，正式EXE与75源码身份不变。不是正式EXE/图像/全角色对局认证。

Unity实际：50a83c119aef4720b6028da0ff03217a 的Authority400/Legacy五组5/5；b8936778bbe54597a7504c1ee14b132d其余15/15，合计20组1800场景/5400完整tick，初始及每tick raw47、source生存、声音路径/次序/worldX、sound latch/两消耗统计/Native RNG一致；3MISSING明确保留。Unity C17独立流每tick1次共5400，未额外出生，已批准例外未改。

Replay首轮4本地PASS/4预分配目标转移FAIL，原32条canonical allocationEpoch2/1保留于replay-8-first.xml和replay-populated-target；不是local Generation，也没有归一化。SlotSnapshot缺epoch及shared topology restore未保留该字段，formal recovery原S0已明确未实现，独立SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT Task继续OBSERVED。随后将同初始出生历史的新接收World场景显式命名FreshTransferredSnapshot，053c0e815f0e425b8bafffe5d62b543a 8/8 PASS、32场景/64重放tick，含epoch在内raw47/声音/完整checksum相同；这不关闭已使用World的恢复缺口，也不推广到任意跨World回滚。

真实Scene Play-224-pass.json：56代表输入×两actual factory×两frame/physics后端=224场景/672完整tick，raw47/声音/native随机全部通过，旧Scene checksum不变、Renderer2→2。Shutdown-pass.json：恢复4→4、World/slot/logic/render全部0、两帧Stopped，已正常退出Play。高动作857/998显式及implicit、正负999/编码1299、成本不足fallback/恰好支付、type3 hold与倍率都有实际完整driver范围；不认证物理按键或真实图片。

本轮只新增一个源诊断CPP与一个Unity Editor测试脚本，未修改生产逻辑；沿用同一生产实现最近完整SelfCheck（03:22:37Z PASS，OPOINT-WEAPON-HP-BIRTH目录SelfCheck-final-pass.result），本轮未重复运行。最终CS error0、Editor idle非Play/非compiling、Scene dirtyfalse/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。Ledger/diff-check交付前运行；无computer-use/非战斗/框架/正式资源/Server/Gen/Plugins/提交修改。

父frame core2676、carrier/snapshot386、已关death/state9998/fragment/state18/普通出生/上游pending证据与本次连续driver/Play联合，闭合本C25帧推进、成本、声音事件、previous078、fragment和生命周期职责。剩余Native frame reader/direct writer/input/碰撞/held/普通生成字段消费者不在此已关闭职责内，立即回NATIVE-FRAME-RUNTIME-READER-MIGRATION继续；display其它出生/post/Q07及raw3缺字段仍未完成。既有definition/fusion后继业务与Q10实际WAV边界保持，不凭这一包宣称全部B3/B1-B12完成。
