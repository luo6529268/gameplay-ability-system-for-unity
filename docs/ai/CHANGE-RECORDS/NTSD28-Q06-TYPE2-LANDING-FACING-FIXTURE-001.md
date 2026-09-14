<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE2-LANDING-FACING-FIXTURE-001
status: VERIFIED
change-kind: TYPE2_LANDING_FACING_FIXTURE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06Type2LandingFacingEditorTests.cs
authority: TYPE2-LANDING-FACING-SOURCE-WITNESS-001 original1296 physics/full outputs; all648 paired facings unchanged after physical endpoint.
evidence: Exact reported case initial right -> left in both native endpoints, frame0/vx4/vy-5/durability19; Unity current behavior matches.
-->

# type2落地翻面测试修正

准确两测试脚本，生产不改。SelfCheck CheckStateTransformLandingMatrix heavyBounce的right预期改物理翻面保留left，说明去除不存在的late state2000按vx覆盖；RunTransformedLandingPasses实际先Serial物理再Late，其已完成definition变更只是前置，不宣称验证原C25变身时序。新focused对原1296行分physics/physics+Late两入口、Weapon与Other共享CLR外壳，比较frame/facing/位置/速度/weaponHP；不以类别名称推断data type，显式sealed catalog742/type2。源端点错误/消息检查成功。

现有FAIL已保留，先新比较再修旧assert。最窄focused与完整SelfCheck验证，失败保留后继处理；不修改物理/渲染或Schema15/23/26/2/2/raw47/3。无服务/关闭职责变化，world fixture显式关闭。回滚仅两脚本差量且需批准。非战斗/Scene/资源/框架与用户例外不变，禁止computer-use。

新four focused全PASS（两外壳×两个端点，各648，共2592），生产未改。旧heavyBounce已扩展right/left初始两向，按原物理flip检查C25保持，原速度/帧/耐久断言保留。原完整SelfCheck红灯已复制保存。待完整SelfCheck新结果。

最终VERIFIED，scope按本Record及同名REPORT限定；原1296、Unity2592/4组及新完整SelfCheck PASS，生产未改。原失败与首轮native编译错误留证，下一碎片准入边缘，整体对齐未完成。
