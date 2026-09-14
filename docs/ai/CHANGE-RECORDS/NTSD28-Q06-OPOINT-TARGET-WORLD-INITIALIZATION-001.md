<!-- CHANGE-RECORD
id: NTSD28-Q06-OPOINT-TARGET-WORLD-INITIALIZATION-001
status: VERIFIED
change-kind: OPOINT_TARGET_WORLD_INITIALIZATION
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
authority: BattleWorld28.spawn_at receives the caller catalog definition before initialization; current factory definition lookup already resolves target World.
evidence: C25L Play 49 passes then renderer ordinary OPoint slot50 has frameState0 vs9999 and wrong lifecycle; prior fragment Play directly proved same unbound Init fallback cause.
-->

# Renderer OPoint出生前绑定所属World

准确两脚本。已确认CreateLogicObject只有nativeWeaponPieceSpawn传resetWorld，普通/Multi不传；非角色Init在Register前读取config因此落到全局CharacterAnimtorManager。logic-only factory已Get(...world)。当target World确实有对应catalog config时，两renderer创建路径使用现有Get(...world)提前绑定；未配置World的编辑器/兼容预览仍保留原global fallback，避免扩大非战斗范围。late任务显式targetWorld=spawner.Match，现有parent指针/cleanup/队列/关闭阶段保持。

真实RED：C25L Play首49向量已过，renderer composite缺fragments/ordinary子frameState错误；原始报告与Scene checksum unchanged/borrowers2→2保存。加入probe FrameCache定义引用诊断以闭合后续失败定位；重跑两factory正式数据组合、前置focused、SelfCheck/关闭全0。目标World配置访问与pool API均已有，不新增服务或修改registry/Unity/GAS/Scene/资源。回滚只该条件差量且先批准；父C25L/深度lives仍待联验，不能先关。

正式Play96/96通过（实际Logan999/两factory/完整tick，prototype父与777明确覆写）；随机逐调用和raw47字段一致，Scene checksum保持、Renderer2→2。probe资源前置用现有诊断API为本次峰值24准备26 total，逻辑slot400不变、无配置/资产变更。最终大矩阵/回归/关机验证继续，尚不推广整个父frame或Q06已对齐。

最终回归正在同一job aa6b0c9f033e4b8b83174826936e782c执行；尚不关闭。current-test-progress.json记录当前48 chunk进度；最终SelfCheck（绑定修复后）需紧接其终态运行。

最终联合82中58PASS/24FAIL；完整3021向量执行，direct1507零差异；待处理pending motion/physics上游首差及C17例外guard范围。继续IN_PROGRESS，不能以此前17/Play96关闭整体验收。证据final-regression-82.xml/current-test-progress。

## 最终限定出口（2026-09-14 03:23Z）

VERIFIED / CONFIGURED_WORLD_BINDING_ONLY。实际证据按改动职责使用，不推广整个B或Q06完成：

- source state18 1550行、formal96，以及weaponHp210重复一致；原正式EXE SHA B1E13AE…9033与75源码manifest07CD47…778F保持。
- 原82测试58PASS/24FAIL；旧non-RNG276实际含252 pending位置和24 weaponHp（此前全归pending的说法已纠正）。pending原6中4FAIL→6PASS，补源实体位置/速度/current/latch/previous/collision/state/pending/code后仍通过；weaponHp原10FAIL→全绿。
- job5a104f86aa1147a3921e27253a5fcc8f：24组22PASS/2FAIL，1514向量只剩weaponHp；修复后jobce0f7a50c7524df990c290e4d9a12831实际30/30 PASS，含weapon10（两caller420原向量、7type真实池复用、手工fallback）、pending6、原SpawnVitals12、两失败chunk5。24组最新合计1514向量0差异/legacy546次恰为批准例外；22组来自前一终态、2组来自修复后，不能称同一run24全绿。direct1507原通过且不受普通OPoint初始化差量影响；full-matrix-final-validation.json记录来源。
- 最终真实Play：pending12含新增源状态；weaponHp84（7type×kind1/2×pointHp负/零/正×两actual factory）；正式Logan999组合96（两factory、原raw47/RNG）。全部PASS，Scene checksum保持，每组Renderer借用2→2。各目录Play-final-pass.json。synthetic父/777及旧内容Scene边界已声明，非图片/物理按键全验收。
- Shutdown-final-pass.json：4→4恢复、World/slots/logic/render borrower全0，两帧Stopped并正常退出。最后SelfCheck请求03:21:56.9990036Z、结果03:22:37Z PASS，归weaponHp目录SelfCheck-final-pass.result。CS error0；Editor idle非Play/非compiling，Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- Ledger与diff-check已通过（历史未出现在diff的声明路径WARNING不作失败），交付前再运行。无正式资源/Scene/InputActions/非战斗/Unity-GAS/Server/Gen/Plugins修改，无computer-use、commit/push或用户工作清理。

后继边界：普通OPoint的effect/reserve/join/join_reserve/join_pic及type0/5-parent credit门、defend等明确仍由OPoint remaining consumer Task承担；generic出生1/0/0不是最终所有OPoint复活字段。当前frame父事务仍需高动作/成本fallback连续完整driver与真实Play联合，不能用端点2676或本组合覆盖代替。其后回原reader/display-post/Q07。15/23/26/2/2、raw47/3及全部已批准例外保持。
