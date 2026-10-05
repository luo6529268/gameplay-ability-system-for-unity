# NTSD28-336B44-Q07-COMMON-TARGET-CACHED-TYPE-001

PLANNED / 2026-10-05T15:39:59.228862+00:00。仅新版正式EXE336B44与paired playable native_ai.cpp:301-315缓存有效性不限制object_type，333扫描才限制type0。Unity LF2Entity.ResolveFrameLogicTargetByHitFa缓存2473额外IsCharacterFrameLogicTarget会重扫并改追另一个角色。属于共用战斗目标规则候选，不因静态文字差异即称实际首差。

source生成/生命周期前置：正式Naruto2/frame193 OPoint33/149→transient50；正式clone33/frame399/state15/wait3/next1000可终止；BattleWorld.step_frame_slot/resolve_pending_lifecycle实际消除而不清3F8；正式Genma901/frame282 OPoint902/40/HP默认500复用50。当前Core新diagnostic先真实OPoint生成33并通过AI选择50，受控把clone送正式399/counter3再实际frame/lifecycle释放50，实际GenmaOPoint生成902/40复用50，再进行一个完整正常Driver tick。两阶段准备位置/V0重置明示，属于受控已声明初态，不声称自然技能输入/自然进入399、根EXE或完整World初态同态。clone terminal及OPoint调用必须实际成功并守卫，没有连续源证据则保留候选，不修代码。

准确code-path仅两已有文件：
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs，仅ResolveFrameLogicTargetByHitFa缓存bool中的type限制；RED与native首差后只移此一项，不改扫描type0、HP/lying/phase/group、source/view排名、其它行为或2F8候选。
- Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs，仅新增CachedNonCharacterTargetSurvivesSlotReuse两参数HP500/0，正式33占50先由AI缓存，注册释放与正式902/40替代，subject902/0正HP、相反侧角色99。非角色活体缓存应50/Vx+0.85，HP0必须重扫0/Vx-0.85。再新增CachedNonCharacterTargetSurvivesOneDriverTick受控同源初态，复用已有wrapper，3F8=50、subject902/0HP500、target902/40HP500、alternate99在左，一个真实Driver tick并CreateNew输出13字段/view/count。两direct加一个完整Driver共三RED；修后三GREEN加既有shared局部SelfCheck一项。保持全部旧测试行，不增加角色矩阵。

diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-COMMON-TARGET-CACHED-TYPE-20261005/native_cached_type_driver_witness.cpp。当前28Core闭包/C++17 O2无fastmath/全输入SHA前后，实际缓存/复用/完整tick消费者守卫；禁止旧authority构建器/候选EXE晋升。只现有原Editor具名EditMode/MCP Refresh两批各一次，空闲干净Battle/nonPlay/DLL新鲜，超时续查同job不重发。最终独立只读diff审阅、字段比较、最窄生成编译、ValidateChangeLedger与gitdiffcheck。

无新manager/queue/pool模块；测试shell终于Unregister/既有wrapper正常零残留，不改变有序关闭11阶段。DAT/图片/Scene/InputActions/地图模式/GAS/非战斗/33ms/F5/pass/共享2F8和历史dirty均保护。操作精确两脚本七文档9备份、逐SHA/Git状态见before-manifest。回滚另获批仅before精确hunk，无restore/reset/clean或删除。
228份同名Record/67未关闭：REUSE52/TRIGGER14/ONE1/P0=DEP=0；先证明首差再修复，父Q/目标开放。

2026-10-05T15:44:49.494361+00:00 独立只读preflight有界审阅：原native缺latch导致guard失败已更正为受控held399/latch399/c3，失败保留。三项RED运行计划为两个差异例预计失败＋HP0控制预计通过，不称三项均失败。两源码/测试全初态不同只比声明subject字段；2F8候选保持条件门，详2F8-BOUNDED-REVIEW.md，不另开包。

2026-10-05T15:58:10.417085+00:00 RUNTIME_PENDING / SCOPED_CACHED_TARGET_DRIVER_PASS：原Editor三RED两预期首差、修后4/4 PASS；native实际OPoint/held399生命周期/槽复用+完整Driver与Unity声明subject26/26同，view残差0。仅缓存type一行移除和217测试新行，全部旧行/scan/HP/state/phase/group/其它dirty保持。GREEN0错334warnings；原Battle clean/root11/nonPlay/Console0error，四保护/五权威/六DAT两端/九before备份/76native输入稳。受控399/不同World/旧scenario载体，未证明根EXE/自然完整技能/Host/GPU或父Q整场。2F8保持条件门，ONE→REUSE；228Record/67未关REUSE53/TRIGGER14/P0=DEP=ONE=0。报告回链 artifacts/diagnostics/NTSD28-336B44-Q07-COMMON-TARGET-CACHED-TYPE-20261005/REPORT.md。
