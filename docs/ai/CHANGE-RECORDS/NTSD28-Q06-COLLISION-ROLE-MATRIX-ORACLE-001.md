<!-- CHANGE-RECORD
id: NTSD28-Q06-COLLISION-ROLE-MATRIX-ORACLE-001
status: VERIFIED
change-kind: COLLISION_ROLE_MATRIX_ORACLE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: Current BattleWorld28.scan_direction uses snapshot geometry independent of current itr/body; original480 includes current10 without geometry and snapshot0 with geometry producing accepted candidates.
evidence: Regression joba4d9cf5d247647c3850301e086f6ad92 actual69PASS/1FAIL; Formal_CachedExactMatchesLegacyForCurrentAndCollisionRoleMatrix(false,true,0) expected0 but both collectors correctly produce1.
-->

# 旧current/collision角色矩阵期望纠正

IN_PROGRESS / TEST_ONLY，唯一修改是上述TestCase的false,true期望由0改1。夹具已有不同frameId0/1、真实定义中同时包含两帧，不是缓存指针身份问题。按原source snapshot几何规则，current无itr/body不应阻止有snapshot几何的一对。

保留其余三组合、普通/缓存exact序列/RNG一致断言和零分配检查。原70项结果保留；修改后只需回跑该4组合，其他69通过且生产无新改动不重复跑。无runtime/schema/关闭副作用；回滚仅本期望差量且按规则授权。禁止修改生产迎合旧0，无Scene/资源/非战斗/框架变更。
已写唯一TestCase期望0→1，生产未改，待4组合实际运行。

VERIFIED / TEST_ONLY。job04fec11524a442048c477aee3eb2e21b实际4/4 PASS；原69PASS加旧矩阵4组合已覆盖原70项，未重跑其余生产不变的69。原FAIL的legacy-70-first.xml和新role-matrix-4-pass.xml都保留。当前qualification Play480/Scene checksum/Renderer2→2通过；本单期望修订未改production，父完整driver仍有bdefend/CRT缺口。
