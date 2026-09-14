# Unity 碰撞帧读取进展

IN_PROGRESS / READER_RUNTIME_PASS_QUALIFICATION_PENDING，2026-09-14 05:51Z。不能报告整个collision包已对齐。

## 实际生产改动

三处文件：LF2Entity.GetCollisionFrameData改为当前FrameCache的Native snapshot点查；BruteForceSceneQuery两个私有current/Prev2 getter也从当前定义Native查询；BattleHitCandidatePairSnapshotFactory的current/previous点查同步迁移。共享legacy cache API、CPoint raw/throw、candidate current准入和其它非战斗接口未调整。既有Frame.Prev2D字段与序列化布局不删除；当前collision getter不再以其旧指针决定取帧。

## 原对照与自动检查

- 原source336，两遍字节一致SHA43f6713de73a76ba5f2c07c1627bf058ecf9e1706baa93701112200ef1ff7499，原252前缀保留。新增84中42两端current=snapshot、42混合端点；96有候选/144kind1推进，原EXE/75源身份不变。
- RED job576839c85fc744fd8cfd0029a0ef1e0c：四组各252，均FAIL；每组523 descriptor/raw/catch差异和122 candidate差异，before raw一致。见red目录。
- 修复后三reader job9efe00d15694402e9a63636d58cb2ea4实际12项，8PASS/4FAIL：kind2四组4800和kind3四组3200通过；collision四组descriptor/raw/catch零差异，各剩24candidate差异。见after-readers目录。观测timeout未重启job，原终态已收取。
- 既有pair carrier/group eligibility/catch顺序/catch字段job11c9f34dcd794fee94aa5614915d39e9实际64/64 PASS，legacy-64-pass.xml。
- expanded jobda257939f9d44335a8531ab24f09d46a实际四项FAIL：每组336输入，descriptor/raw/catch仍零差异，各24candidate差异不变。不是336测试全部通过；两profile×两query共1344输入中有96候选首差，全部为同24组诊断条件的四次执行。见expanded目录。
- raw比较只覆盖已绑定47字段；3MISSING及其它未覆盖字段不因PASS消失。pair state全范围/filter、完整actual+Shadow新边界仍需后继见证，不能借旧64项扩大结论。

## SelfCheck 与运行时

完整SelfCheck请求05:43:02Z，05:43:27Z FAIL：invalidFirst夹具定义仍带body，仅Frame指针被换空；改为真实空定义，保留原顺序/唯一目标断言。重跑05:46:32Z FAIL：旧DATA-01C明确要求缺失Prev2回退current；据原source纠正为450隐式零帧和-1/未声明999/1000不回退。legacy ImmediateFrame/CPoint/HasFrame测试保持。两处由独立COLLISION-FIXTURE-DEFINITION-IDENTITY-001记录，两个旧FAIL均保留。

第三请求05:47:31Z，**05:48:49Z完整SelfCheck PASS**。不将旧失败覆盖成从未失败。

真实Scene Play-168-pass.json：新增42个两端current=snapshot输入×两actual factory×普通/role-aware query=168，descriptor/raw/catch/candidate差异0，Renderer2→2，主Scene checksum保持。此为合成DAT诊断World挂在真实Scene，未使用物理键盘，未证明正式新DAT/图片或完整帧driver变身。

Shutdown-pass.json于05:50:09Z PASS：场景原地恢复4→4，World/slots/logic/render borrowers全0，两帧Stopped。Editor已正常退出Play、idle，CS错误0，Scene dirty=false/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。用户HUDBg x30保留。

## 剩余与下一步

当前资格差异明确没有豁免：source current1000且snapshot有效时普通geometry可留候选（并可同时diagnostic失败）；Unity carrier/current itr/body/null门拦截。源普通geometry由snapshot itr触发，平台另由current itr触发；完整driver的刚snapshot时点、后继consumer换帧可达性、pair state0与null合同须追踪。下一唯一Task COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001已列普通/cached/consumer相互依赖，精确子Record后处理并回跑这些失败。

脚本范围为父四脚本、独立SelfCheck一脚本、源witness一CPP。无Scene、资源、非战斗、Unity/GAS框架、Server、schema或关闭顺序变更，无computer-use、提交或推送。资源Q07未部署，epoch恢复缺口/用户例外/stage.dat暂缓保持。总目标ACTIVE。
