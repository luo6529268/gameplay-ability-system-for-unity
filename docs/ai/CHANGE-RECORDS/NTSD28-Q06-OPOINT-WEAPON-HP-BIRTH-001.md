<!-- CHANGE-RECORD
id: NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001
status: VERIFIED
change-kind: OPOINT_WEAPON_HP_BIRTH_CONTRACT
code-path: Tools/NTSD28AuthorityTrace/opoint_weapon_hp_witness.cpp
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleSpawnVitalsWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointWeaponHpEditorTests.cs
authority: Formal playable BattleWorld28 spawn_at initializes weapon_hp_31c from bmp.weapon_hp for every type; materialize_spawn_intents kind2 parent link overrides with positive point.hp.
evidence: Existing state18 full-driver ordinary child slot50 weaponHp 0 vs17; old report already contains 12 per mode, not pending position differences.
-->

# 普通OPoint的weapon_hp出生字段

准确五脚本：先独立C++诊断210向量（7 DAT类型×kind1/2×pointHp负/零/正×metadata缺失/负/零/17/999），输出原完整raw和输入；不得改正式源码/EXE。Unity当前24完整tick测试结束前只写Tools CPP与文档，不改Assets触发reload。之后test-first：共用BattleSpawnVitalsWriter.Apply设置metadata weapon_hp（保留无NativeMetadata手工wrapper fallback）；两个PostInitLiving现有kind2且有parent绑定成功分支在hp>0时覆写WeaponFlightCounter。HP/MP原百分比事务保持（source实际先选HP/MP，不能凭kind2注释反向删除HP写入）；不套用native fragment固定500/500覆盖普通OPoint。

测试覆盖7类型、signed metadata、缺失fallback、kind1不被point.hp改weapon字段、kind2正值覆盖和非正保持、复用池污值重置、两factory与raw slot50。新Record只负责此字段完整出生/覆写合同，其余OPoint消费者仍归原回访Task。结构slot/RNG/位置/帧/link时序和所有非战斗/Scene/框架/资源不改；无新模块/持久字段/schema，沿用既有关闭。验收C++重复输出、Unity RED→compile/210×2 caller、真实Scene两factory含普通777组合、原失败chunk5与SelfCheck必要回归。回滚仅这些差量且按规则获批，保留用户工作。

原C++210完整出生向量已执行两遍、字节相同 SHA 0fdd60b278ea1b566a88b6927730b5327920bff6299d49e108d861b258012d82，所有weaponHp/links符合实际分支，RNG0；正式EXE和75源manifest身份保持。24回归终态22PASS/2FAIL，1514向量剩余24条weaponHp0/17，252 pending位置/546 legacy guard已消除。开始Unity测试先红：两PostInit caller共420原向量、七type真实pool复用、无metadata fallback；尚未改生产。source raw其余continuation字段明确归剩余OPoint Task，不将本单字段报告为全raw出生对齐。

实际RED job7eeca6230b7d4de6988399b8a6426dcb：16项中6 pending（含新增源状态断言）通过、10 weaponHp失败；420 PostInit保留污值，7真实factory的type0/3/5为0而weapons为legacy333，fallback未重置。原XML/JSON已归red。现在写三生产文件准确差量。

CODE_WRITTEN：共用出生writer从NativeMetadata.bmp优先取weapon_hp（缺失0、无metadata fallback既有weapon_hp）；两现有kind2绑定分支仅hp>0覆盖。无其他生产改动。新测试脚本增加请求式真实Scene probe，84次（7type×kind1/2×pointHp负/零/正×两actual factory），仅synthetic DAT字段、Renderer借还/Scene checksum，明确不认证图片/全部OPoint字段。测试前构造有效父frame/HP，沿用有序关闭。

## 最终限定出口（2026-09-14 03:23Z）

VERIFIED / OPOINT_WEAPON_HP_BIRTH_AND_KIND2_OVERRIDE_ONLY。实际证据按改动职责使用，不推广整个B或Q06完成：

- source state18 1550行、formal96，以及weaponHp210重复一致；原正式EXE SHA B1E13AE…9033与75源码manifest07CD47…778F保持。
- 原82测试58PASS/24FAIL；旧non-RNG276实际含252 pending位置和24 weaponHp（此前全归pending的说法已纠正）。pending原6中4FAIL→6PASS，补源实体位置/速度/current/latch/previous/collision/state/pending/code后仍通过；weaponHp原10FAIL→全绿。
- job5a104f86aa1147a3921e27253a5fcc8f：24组22PASS/2FAIL，1514向量只剩weaponHp；修复后jobce0f7a50c7524df990c290e4d9a12831实际30/30 PASS，含weapon10（两caller420原向量、7type真实池复用、手工fallback）、pending6、原SpawnVitals12、两失败chunk5。24组最新合计1514向量0差异/legacy546次恰为批准例外；22组来自前一终态、2组来自修复后，不能称同一run24全绿。direct1507原通过且不受普通OPoint初始化差量影响；full-matrix-final-validation.json记录来源。
- 最终真实Play：pending12含新增源状态；weaponHp84（7type×kind1/2×pointHp负/零/正×两actual factory）；正式Logan999组合96（两factory、原raw47/RNG）。全部PASS，Scene checksum保持，每组Renderer借用2→2。各目录Play-final-pass.json。synthetic父/777及旧内容Scene边界已声明，非图片/物理按键全验收。
- Shutdown-final-pass.json：4→4恢复、World/slots/logic/render borrower全0，两帧Stopped并正常退出。最后SelfCheck请求03:21:56.9990036Z、结果03:22:37Z PASS，归weaponHp目录SelfCheck-final-pass.result。CS error0；Editor idle非Play/非compiling，Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- Ledger与diff-check已通过（历史未出现在diff的声明路径WARNING不作失败），交付前再运行。无正式资源/Scene/InputActions/非战斗/Unity-GAS/Server/Gen/Plugins修改，无computer-use、commit/push或用户工作清理。

后继边界：普通OPoint的effect/reserve/join/join_reserve/join_pic及type0/5-parent credit门、defend等明确仍由OPoint remaining consumer Task承担；generic出生1/0/0不是最终所有OPoint复活字段。当前frame父事务仍需高动作/成本fallback连续完整driver与真实Play联合，不能用端点2676或本组合覆盖代替。其后回原reader/display-post/Q07。15/23/26/2/2、raw47/3及全部已批准例外保持。
