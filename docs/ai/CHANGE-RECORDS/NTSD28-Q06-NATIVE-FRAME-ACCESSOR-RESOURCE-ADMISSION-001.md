<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001
status: VERIFIED
change-kind: NATIVE_FRAME_ACCESSOR_AND_RESOURCE_ADMISSION
code-path: Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeFrameAccessorEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/native_zero_frame_witness.cpp
authority: Formal Logan dat_document.cpp frame/declared_frame, parser0..999 plus catalog ok gate, BattleWorld28 resource pre-display; source EXE B1E13AE1.
evidence: Native81/63 valid cases; correctedRED6;223 plus legacy1 PASS;SelfCheck and scoped resource Play/restore/shutdown0; other readers pending.
-->

# 原版帧点查与资源资格

准确四脚本。Native getter/HasNativeFrame独立于旧getter/HasFrame/Max857，底层数组1000容纳声明999，隐式范围0..998。显式static ctor预热999个只读约定的LF2FrameData模板，id正确、wait0/next0及零数据，UsesLoganFrameNumbers=true；不把模板插入Wrapper.frames或内容hash。不创建每tick对象、不新增World carrier/schema。旧接口与GetFirstFrameByState范围保持，非战斗不被隐式迁移。

BattleRecoveryStatusWriter仅CanEnterNativeResource及HP chp/MP cmp改读Native入口；公式、mode、World phase与两caller顺序不变。原未声明7资格修正测试另立Record。原函数witness用真实DatParser/DatDocument access和BattleWorld pre-display，区分valid document与非法AST探针；不写权威或复制expected算法。新增Editor测试覆盖零/声明/高位/999/非法/未Load/Clear/旧接口保持/零热分配、HP/MP raw行为及两caller、native模板数据内容与两文档共享，快照/相关回归和真实Play。

模板只含定义数值与空集合，不持有World/entity/Renderer/UnityObject，无停止接单或队列，没有新的shutdown阶段；正常World关闭不释放进程级定义模板。所有生产reader仍应视frame为只读，不支持利用新getter修改共享零帧。fixture必须用声明帧修改字段。

回滚须批准仅本四文件差量，保留已验出生/显示/资源公式；scope中所有未提交用户文件读后追加，不恢复清理。禁止computer-use/非战斗/框架/Scene/资源/Gen/Plugins更改。大范围其他reader留父任务后继准确Record，不以新入口存在关闭全域。

原函数81行（63合法文档+18错误AST）已生成；首次用非法initial_action直接spawn被原构造器拒绝，改为先合法action0出生再设置观察action，原失败TSV留证。原初始Unity RED11FAIL：访问器缺失/直接资源implicit0拒绝已复现；其中六caller测试先设置Runtime.Frame再Register，被注册同步覆盖到0，因此得到107不是预期的implicit拒绝。已只将fixture action设置移到Register之后，原失败保留并重跑这6项RED，不按错误夹具改生产。

修正fixture后六caller RED全部以HP100/期望101失败（6d594643f30e427395f2da596f208d9e），red-callers-6.xml保留。现两生产文件已写：缓存1000存储/999预热零模板和独立Native API，旧三查询/857范围不变；资源资格与chp/cmp读改Native，公式/两caller顺序不动。原函数81行/63合法/18错误AST、重复输出一致已验证，native-validation.json有例子。当前编译及focused待，其他388检索点尚未整体迁移。

首轮新focused11/11 PASS（352fb848ccfe47dd9a1f7d0366b9fbe7），包含63合法原函数点查/资源值、六两caller及旧接口不变/零分配。已在同一声明Editor文件加入真实Scene暂停资源owner probe：857/998零帧、未声明999、两入口各三例，不把高帧号运行完整tick（其余reader尚未迁移）；finally World/checksum恢复，后续Q05清理。独立旧invalid fixture已改，当前编译/相关回归待。

联合job6fb2da5fcef048cb93197fc93c780ad0实际223/223 PASS（141.926秒），含新11、资源/出生/回放/两profile329及旧fixture修正。请求里的额外旧Legacy boundary方法误带Editor命名空间，未进入该223；已以正确namespace单独运行59958312d0d249b2ad50cddfa0674d53，1/1 PASS并归档，不冒称224单次通过。旧测试名含FormalAuthorityMaximum，本轮只证明旧接口857保持，原版Native界限由新63有效doc向量裁决。当前完整SelfCheck/Play待。

## 最终限定验证

VERIFIED / NATIVE_ACCESSOR_AND_RESOURCE_OWNER_ADMISSION_ONLY。 223/223+独立旧接口1/1、完整SelfCheck、真实Scene资源owner六例/完整恢复/有序关闭全0与两帧Stopped通过；CS0/用户Scene hash保持。证据见 artifacts/diagnostics/NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001/REPORT.md。父全部reader迁移未完成，不声称完整高位动作tick对齐。

最终账本验证PASS：517 records/25 governed code diff；父artifact/ledger-final.txt。历史Record不在当前diff的WARNING保留。下一实际reader迁移仍可推进，总目标保持ACTIVE。
