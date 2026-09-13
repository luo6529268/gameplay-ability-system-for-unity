# Q03 同输入碰撞几何见证

状态 VERIFIED_CAPTURE_ONLY / PARITY_DIFFERENCES_CONFIRMED。父任务NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001仍IN_PROGRESS；本子包只取得合同证据，不实施Q06命中修复。实际native14、Unity42比较15相同27不同，报告位于同ID artifacts/diagnostics目录REPORT.md；scope不含dense/cache/held strength或Play。

Authority：当前正式Logan EXE及playable build内battle_world.cpp -> HitCandidateBuilder28::append_pair_geometry -> CollisionGeometry28。用source-linked可执行程序读取共享DAT夹具并输出候选数量；它不是正式EXE运行trace。Unity用实际ParserV2/Converter、SimulationWorld.CollectCollisionCandidatesAll和BruteForce/LegacyUnion/RoleAware三种模式读取同一fixture，记录候选数量。保持场景、production、schema、内容资源不变。

准确写范围：Tools/NTSD28Q03Geometry/AuthorityGeometryWitness.cpp、Build-And-Capture.ps1、README.md；Assets/NTSD/Scripts/Test/Editor/NTSD28Q03GeometryWitnessEditorTests.cs及meta；artifacts/diagnostics/NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001下fixtures/manifest/native/Unity结果与报告；Task/Change/Ledger/STATE/handoff及Q03恢复文档。

验证：共享DAT hash、源码/编译身份；native实际编译和输出；真实Unity编译与capture test；比较每个case/mode，不把capture test通过当parity通过。覆盖±15端点/内外、缺省与零/负zwidth、BDY非零zwidth与z、ITR z偏移、2D端点及缺失/零几何。held weapon strength override本包只定位，不假装普通case覆盖held调用链。

副作用：离线工具只读source/fixture，输出workspace工件；测试只创建非Mono逻辑world和实体，finally结束候选消费并注销实体，不创建renderer/manager或进入Play。无新增battle服务，无有序关闭阶段变更。风险是fixture偏离生产上下文，故保留调用层级与缺口，必要时进一步world/Play见证。回滚为经批准仅撤销本子包新增诊断文件，不回退用户工作、不改权威EXE。
