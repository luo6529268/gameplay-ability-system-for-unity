# NTSD28-336B44-Q07-COMMON-TARGET-STALE-LIVE-001

PLANNED / 2026-10-05T16:10:25.564671+00:00。本包先执行一个必要的共用规则诊断，尚未取得本包运行首差，不称生产缺口已实测。正式336B44 paired native_ai.cpp301-389：缓存HP0/state14无效且scan无候选时保留旧3F8；仅-1杀subject，旧槽仍有实体则继续1/3等行为。Unity ResolveFrameLogicTargetByHitFa保留旧数字却bestSlot<0返回null；Fa1另有target HP0清subject HP。独立只读审阅确认当前live候选，前包缓存type的HP0控制有新候选，不覆盖本边界。

准确路径仅两既有脚本：
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs：仅ResolveFrameLogicTargetByHitFa扫描尾按最终缓存查询active target，及RunHitFa1FrameLogic移除目标HP门（必须实际首差后）；保留subject HP门、-1 sentinel、scan资格/组/相位/排序、inactive槽仍null，禁止IncludingPending/raw造目标。
- Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs：新增StaleLiveTargetSurvivesFailedScan三个参数(HP500/lying/无替代)、(HP0/standing/无替代)、(HP0/standing/有替代)；正式99/frame230 state14、902/frame0 Fa1，先实际共用AI从-1选50，再显式受控进入lying/HP0、重置source/V0，缓存50无扫描新结果应继续Vx+.85/Y-98.8/HP500，有活体替代应改0/Vx-.85。一个完整Driver CachedInvalidLiveTargetSurvivesOneDriverTick：旧scenario载体正式LoganRuntime/projectmode，subject902/frame0正HP、type0目标99/230槽50保持活跃、有同组替代99槽0；3F8=50/源500,-100,600。真实一个生产tick/CreateNew声明13字段+view/targetActive/count。全部旧行保持。

新 diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-COMMON-TARGET-STALE-LIVE-20261006/native_stale_live_target_driver_witness.cpp。当前声明28 Core CPP/C++17 O2无fastmath，全输入SHA前后。真实扫描先选50，显式受控99/230或HP0后再真实共用消费者，三direct守卫；完整Driver保留active旧目标，旧槽inactive不得恢复运动。控制输入不是自然玩家倒地/击杀，源与Unitysubject槽/部分初态不同只比声明subject字段，非根EXE/Host/全World/GPU验收。

先native实际消费者及原Editor四RED（预计三个差异失败、替代控制通过），证明首不同tick/字段后才修生产；否则停止候选并只更新条件门。GREEN同四＋前包缓存type两控制＋既有共用局部SelfCheck共七具名，不运行全套/角色矩阵。两MCPRefresh各一次、原Editor idle/nonPlay/clean/DLL新鲜、同job续查。生成编译、最终最窄独立diff/声明字段比较、Ledger和diffcheck。

不新增runtime模块；direct finally Unregister、完整wrapper原零残留保持，有序关闭11阶段不改。精确两脚本七文档九before备份见manifest；正式源码/EXE、DAT、图片、Scene/InputActions、地图/模式、Unity/GAS/非战斗/33ms/pass与旧dirty保护。回滚另获批仅本包before精确hunk，不restore/reset/clean或删文件。当前229Record/68未关：REUSE53/TRIGGER14/ONE1/P0=DEP=0；ONE是诊断，不称已证生产P0，父Q及目标开放。

2026-10-05T16:47:59.727067+00:00 RUNTIME_PENDING / SCOPED_STALE_LIVE_TARGET_DRIVER_PASS：原Editor GREEN job9d69c11e3abd48b282ea1c5b675725c9终态7/7 PASS/0fail/0skip；仅本包四例＋缓存type两控制＋局部SelfCheck。两批各一次Refresh/DLL新鲜，首查询30s超时续同job不重启。生成GREEN0error334warning，当前native v2 compile/run0、70输入前后/收尾稳。subject初态+完整tick13×2 GREEN26/26（RED22/26），direct15/15（RED11/15），view残差0；scope与原件见本包REPORT/declared-field-comparison。四保护/三DAT两端/九备份稳，原Battle clean/nonPlay/Console0error。生产仅两hunk/test222插入且旧行全保留，独立只读审阅无阻断。外部build5F400→5E85与v1 headerBC03→22D5保留更正；只接纳v2当前身份，不称旧输入或全playable闭包稳。无自然玩家/根EXE/Host/GPU整场证书，父Q/目标开放；必要ONE→REUSE，229Record/68未关REUSE54/TRIGGER14/P0=DEP=ONE=0。

2026-10-05T16:55:28.900628+00:00 最终身份更正：此前scope-pre-final-check时当前70输入稳，native v2编译/运行前后70稳定证书仍有效；随后最终guard失败实际仅README_SOURCE.md C0BA→DE936，不是新header/CPP变化。已保存当前全文post-green-current-0-README_SOURCE.md.txt。其现声明源目录为持续开发的锦标赛候选，不能把当前源快照与旧根发行EXE视为逐字节对应。根336B44/三个规则body/当前其余69输入稳，但今后不能把当前源树自动当336B44对应权威；下一先只读核冻结源码及根EXE对应关系，不用候选定新规则，不重跑七项或重编仅为README变更。已通过7/7、Core26/26/direct15/15证据保留，RUNTIME_PENDING/父Q与目标开放。最终不称70输入全稳或已对齐根EXE。
