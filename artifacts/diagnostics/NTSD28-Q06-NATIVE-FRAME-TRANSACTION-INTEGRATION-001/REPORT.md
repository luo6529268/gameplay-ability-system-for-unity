## 最终限定出口（2026-09-14 04:00Z）

VERIFIED / OFFLINE_NATIVE_FRAME_TRANSACTION_SCOPE。源450/1350连续driver、frame/lifecycle错误0、934声音事件；重复SHA e50dbee6628622e08c6deb1697f9aa83887775725b4768f82ab654989db9fce5，正式EXE与75源码身份不变。不是正式EXE/图像/全角色对局认证。

Unity实际：50a83c119aef4720b6028da0ff03217a 的Authority400/Legacy五组5/5；b8936778bbe54597a7504c1ee14b132d其余15/15，合计20组1800场景/5400完整tick，初始及每tick raw47、source生存、声音路径/次序/worldX、sound latch/两消耗统计/Native RNG一致；3MISSING明确保留。Unity C17独立流每tick1次共5400，未额外出生，已批准例外未改。

Replay首轮4本地PASS/4预分配目标转移FAIL，原32条canonical allocationEpoch2/1保留于replay-8-first.xml和replay-populated-target；不是local Generation，也没有归一化。SlotSnapshot缺epoch及shared topology restore未保留该字段，formal recovery原S0已明确未实现，独立SNAPSHOT-ALLOCATION-EPOCH-PRESERVATION-AUDIT Task继续OBSERVED。随后将同初始出生历史的新接收World场景显式命名FreshTransferredSnapshot，053c0e815f0e425b8bafffe5d62b543a 8/8 PASS、32场景/64重放tick，含epoch在内raw47/声音/完整checksum相同；这不关闭已使用World的恢复缺口，也不推广到任意跨World回滚。

真实Scene Play-224-pass.json：56代表输入×两actual factory×两frame/physics后端=224场景/672完整tick，raw47/声音/native随机全部通过，旧Scene checksum不变、Renderer2→2。Shutdown-pass.json：恢复4→4、World/slot/logic/render全部0、两帧Stopped，已正常退出Play。高动作857/998显式及implicit、正负999/编码1299、成本不足fallback/恰好支付、type3 hold与倍率都有实际完整driver范围；不认证物理按键或真实图片。

本轮只新增一个源诊断CPP与一个Unity Editor测试脚本，未修改生产逻辑；沿用同一生产实现最近完整SelfCheck（03:22:37Z PASS，OPOINT-WEAPON-HP-BIRTH目录SelfCheck-final-pass.result），本轮未重复运行。最终CS error0、Editor idle非Play/非compiling、Scene dirtyfalse/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。Ledger/diff-check交付前运行；无computer-use/非战斗/框架/正式资源/Server/Gen/Plugins/提交修改。

父frame core2676、carrier/snapshot386、已关death/state9998/fragment/state18/普通出生/上游pending证据与本次连续driver/Play联合，闭合本C25帧推进、成本、声音事件、previous078、fragment和生命周期职责。剩余Native frame reader/direct writer/input/碰撞/held/普通生成字段消费者不在此已关闭职责内，立即回NATIVE-FRAME-RUNTIME-READER-MIGRATION继续；display其它出生/post/Q07及raw3缺字段仍未完成。既有definition/fusion后继业务与Q10实际WAV边界保持，不凭这一包宣称全部B3/B1-B12完成。

以下为历史记录：

# C25帧事务实施中检查点

**IN_PROGRESS / CORE_FOCUSED_PASS / FULL_TRANSACTION_INCOMPLETE。** 不能发布为完整frame对齐，不能把当前core通过推广到真实高位动作完整tick。

## 本轮实际实现

LF2Entity的正式nativeC25FrameTickActive分支新增RunNativeC25FrameTransaction。未带marker的direct compatibility继续既有逻辑，真实late loop的optimized/virtual均进入marker。Native getter解析当前/目标帧；原link/hold/terminal14/type3资格；负next翻面/正999二次YReference/212 advanced判断；读取源DAT wait/next且区分latch与counter；独立sound latch/head与destination前20事件；原destination signed MP/HP及fallback/消费统计/maxHP除3。跳跃读取现有NativeMetadata.Bmp binary64，无metadata旧内容用既有字段。BattleCharacterActionWriter.AdjustNativeMpCost仅private→internal复用signed调整，原输入支付算法未改。

private transient terminalPending与End(out)已写，Begin/End/Reset清零，但尚未被Module消费；此中间设计将由下一生命周期持久载体替换，不留两套真值。Module文件本轮尚未修改；它仍用旧857边界并将encoded结果写HitStun，未满足原版合同。因此尚未完成源step/lifecycle两个端点或整条driver串联。

## 新鲜证据

- 编译idle/error CS0。
- RED job3b2e788356d8456296ba2e7448a1023e：9组2676例，14项核对字段总9103差异。红色输入/输出JSON保留red/；所有组失败，非仅API缺失。
- 实施后2070be80d2314702b50cc9ee197a182f：9/9 PASS，2676例×14字段全部相同，5.705秒。字段为action/latch/counter/facing、HP/MP/maxHP/两consumed totals、三速度、sound latch/声音数。没有据此宣称status/terminal/粒子/完整driver全部相同。
- 相关job7bd2b0b4ebb44f5f9347174cac9ac991：19中18PASS/1FAIL，旧夹具假设action202自动写phase20。source唯一phase20赋值在advance_native_revivals且选择212，step_frames_range没有该writer；独立C25-RENDER-PHASE-20-FIXTURE Record将测试改为前置20→19、bare202保持0并保留new15不递减，未恢复错误production writer。
- 最终48136c7e3cc04b47aa7929dab254942c：core9+原相关19=28/28 PASS，6.216秒；9组JSON仍2676例/0差异（core-validation.json）。原19覆盖direct compatibility和两native进入方式。
- 本轮未执行完整SelfCheck、真实高位Play、完整driver trace或新的关闭验证，因为事务尾部仍未接完；之前344/SelfCheck/Play是前包数据基线，不算本轮新生产验收。
- Scene用户HUDBg x30/SHA bcd1047b…保持，无Scene/资源/非战斗/GAS/Server/Gen/Plugins变更。14/22/25/2/2仍当前版本。当前生产两个文件有新差量、一个新测试；第四个声明Module是后续接线范围。current-code-scope/diff文件保留准确差量。

## 必须接续的工作（不要求用户再确认）

下一必要Task NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / READY_FOR_EXACT_PRECHANGE_RECORD：闭合独立runtime_state_code/pending/code和snapshot/copy/checksum/raw映射，再立即返回本Task。source runtime_state_code与render_phase_008不同，battle_world.cpp:4963和native_ai_tests明确禁止别名；不能沿用HitStun写入。若新增持久三字段，按新Task实施联合版本15/23/26/2/2，并将临时private结果替换为Runtime真值，Begin/End不得清真实pending。

接线顺序必须完整：普通zero-frame OPoint被pending拒绝；state18 particles没有统一pending早退，声明999可有current frame；然后previous078提交、真正broken weapon的两类fragments、最后lifecycle。code11xx/12xx清current及collision镜像、保留latch，不修改render phase。健康负link由framebody不arm pending而存活；已arm的broken held weapon不能被旧负link保护无条件保活。旧weapon cleanup只sound/flag，fragment producer仍未实现，本Task因此保持IN_PROGRESS。

完整事务还需补判定结果/marker生命周期、source同输入全driver/OPoint/particles/fragment、snapshot replay、SelfCheck和真实高位动作完整tick/关闭全0。definition/fusion成功重置sound latch也须按已确认业务owner接通。保留Q10实际WAV效果回访及Q07资源迁移依赖；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。
