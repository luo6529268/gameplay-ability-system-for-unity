# 第87批：生产者 Runtime 快照重复复制候选

当前：FOCUSED_TEST_PASS / DEFAULT_OFF / PERFORMANCE_PENDING。H07未完成，阶段5/6，66已执行、34父项关闭0、Goal active。第86批H11选定阶段保持通过，不重开。依据有效有限合同第0—8节与用户继续未完成优化要求；Role-aware生产默认推广仍未授权，本包无该行为。事前PLANNED/原始65状态在Change/Operation历史保留。

## 原状与假设

85同版Role窗 ProducerSnapshotRefresh约0.95ms/1000 tick。World.RefreshCharacterInputSnapshotsAfterProducer先完整RefreshRuntimeSnapshot，再刷新必要AI可见行。LF2Entity.RefreshBaseRuntimeFromEntity绝大多数字段是Runtime读写别名；精确LF2Character.RefreshRuntimeFromEntity仅调用base，构造绑定Health/HitCounters到同一Runtime。非别名ObjType/EntityType/Frame/WaitCounter/NextFrame已有IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp检查。不能跳过后继AI row更新，不能缓存可变rawProperties；FrameCache取帧已为定长数组O(1)，本包不实施猜测解析缓存/每实体pair新缓存。

## 精确范围与操作

仅2 C#：
- Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs：默认false候选flag与每输入pass诊断计数；RefreshCharacterInputSnapshotsAfterProducer只在精确LF2Character及既有非别名完整性证明成立时省去完整自复制。producer mutation hook存在时强制原刷新；所有AI行刷新顺序和内容不变。
- Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs：新ProducerSnapshotReuse具名测试，复用原夹具；保留全部旧断言。

五治理文档及Temp/Goal18_LastTestResults.xml准确自然覆盖列于Operation；全部原始失败XML先复制保留。无其它脚本/Scene/Prefab/asset/importer/settings/input/Gen/Plugins/Server/活跃Q06正文修改。未来必要A/B冷接线需先扩展本Task/Record精确清单并保存新before，当前不修改Suite/Harness。

## 冻结验证及采用标准

test-first：defaultOFF、开启后精确且current跳过、关闭仍刷新、五非别名陈旧字段回退并修正、未知派生保留虚拟刷新、producer hook完整刷新、完整runtime别名字段及AI可见输出一致、native/legacy两输入profile16tick Extended/Lockstep/native scalar/legacy RNG/proxy一致且开启确有skip、原委托热调用预热后0分配。预计15新用例；以实际唯一测试case计数为准。

新用例RED缺API→实现→新及整CharacterInputLiveSlotLoop既有回归GREEN，编译/Console0CS。必要受影响native producer/proxy fixture按准确namespace选最窄，不重开H06或全历史。纯夹具0B不冒充正式千人0GC/规则全域证书。

本包不据source/聚焦PASS采纳默认；后继先保存准确Suitebefore并冻结同版四窗Dispersed/Combat各OFF/ON，120warm+180sample、1000真实AI、seed0x4E545344、33ms/max2、共享绑定保持、Role只owned压力窗口，可靠完整同步logicGC、完整末态/关闭/Scene恢复。若无可辨收益或skip=0不推广；不得削弱覆盖断言刷PASS。当前没有新测量，更不能称H07 <33ms门通过。

## 所有权、生命周期、预算与回滚

只World拥有一个候选bool和两int per-pass计数，无缓存/资源/lease/queue/job/异步新增；常数冷字段payload约9B未计对象padding，不是总内存实测。不改变稳态/过渡ATLAS合同、seal容量或11阶段关闭；后继冷Suite仅恢复原bool、释放World引用。默认OFF不影响现有生产分支。优化仍需同输入bitwise/权威准入，不改规则、RNG、cadence或pass。

回滚仅本包准确hunks并对比Operation的当前dirty副本；不执行恢复/删除/破坏性Git，任何实际恢复先取得批准。所有失败/未完成/工具orphan原件保留。

2026-10-09执行前计数纠正：参数展开实际16个新case，不是前述预计15；Frame为Runtime属性、FrameInfo.N独立，继续作为五非别名陈旧回退反例。尚未运行RED/测量。两C#范围不变。

2026-10-09 RED实际计数更正：新filter实际15个唯一case、0PASS/15FAIL（缺新API），故上条人工16计数作废，原预计15正确。保留历史，不删除失败。Frame属性反例保留，范围不变；实现默认OFF，不据聚焦执行宣称收益。

2026-10-09限定交付：FOCUSED_TEST_PASS / DEFAULT_OFF / PERFORMANCE_PENDING。GREEN01 63/64的唯一新夹具档位错误修正后，GREEN02实际64/64（新15/旧49）0skip，原XML/失败均保留；仅原Editor准确2C#，旧测试和AI行刷新原文同、800保护/8before核同。尚无千人实际候选收益/完整Driver逐tick准入，不推广默认、不标H07完成；66执行/阶段5of6/Goal active。下一精确Suite冷接线和同版四窗必须事前扩展清单/新before/冻结矩阵，不在本次脚本范围内偷加。
