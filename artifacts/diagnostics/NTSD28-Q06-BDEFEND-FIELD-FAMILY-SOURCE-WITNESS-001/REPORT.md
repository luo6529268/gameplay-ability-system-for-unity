# Bdefend字段家族原证据与Unity归属审计

VERIFIED / SOURCE_MODEL_ONLY。256输入、1280断言，两次输出逐字节一致，SHA6ba2e26f9aa202453f9c2985adf1bfba9c15ad3d1f8f54713450948d9ae7f2cb。

实际构建命令：Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28BdefendFieldFamily -RunnerSource Tools/NTSD28AuthorityTrace/bdefend_field_family_witness.cpp -ExecutableName bdefend_field_family_witness.exe，退出0；Python subprocess两次执行均退出0、stderr空。正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033和75-source manifest SHA07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F保持。

## 原字段与结果

EntityState28.bdefend_accumulator为+0x0B8，hit_reaction_timer为+0x0B0，motion_hold_timer为+0x0B4。Unity raw capture的Runtime.Bdefend映射正确，不应为了让测试通过改成读HitStateCount。

七target types × 无armor/首type0 × 初值-7/0/45/90 × itr.bdefend -5/0/35/100，共224；另type0首type1 armor × 同初值/raw字段 × runtimeArmorHp -1/1，共32。

| 原实际分支 | 数量 | Bdefend结果 |
|---|---:|---|
| 无护甲、角色首type0、type1绕过后无护甲 | 148 | 覆写45，不是加45，也不由DAT缺省产生 |
| 非角色首type0反馈分支 | 96 | 保留原值，不能全局写45 |
| 生效减伤分支，runtime armor HP≤0 | 6 | 初值加原始有符号itr.bdefend，-7+-5=-12 |
| 生效减伤分支，runtime armor HP>0 | 6 | 保留原值 |

256全部有一个候选且原status applied。type1其中20个绕过进入45路径；是否绕过仍由原armor matcher判定，不在witness重写判断。一次接续C25 timer验证：正值在允许body的情况下减1，非正值不变；hold2冻结非type3，type3按原例外继续，负link仍冻结。没有重做既有全部timer矩阵。

## Unity实际归属

- BattleLateEntityLifecycleModule（Passes/LateLifecycle）已有正确 `DecrementPositive(ref runtime.Bdefend)`；旧C25F-H-J Record明确HitStateCount没有当前C25h authority，生产已停止其递减。不要改这个已验证timer或把legacy -0.5恢复接回生产。
- BattleDamageWriter.ApplyStandardCharacterDamage、ApplyWeaponDamage及通用无护甲continuation仍写HitStateCount45；ApplyAlternateDamage累加和阈值读的也是HitStateCount。
- BattleOrdinaryCharacterDamageRouteResolver把target.HitStateCount传给type1 armor匹配的defenderArmorDelay；需与生产写入一起核对。
- HitPlan.CaptureWriterEffectSnapshotWithCreditOverride只捕获TargetHitStateCount；StandardObject/StandardType3/Type3D1Identity/Type3ActiveD1Identity/StandardCharacter多处分支写45，AlternateCharacter累加与判阈值。差异mask当前只比HitStateCount，解释了原raw错误而Shadow0；需要独立Bdefend观测字段，不能继续只比较两个错误实现。
- LF2HitCountersModule.Bdefend与HitStateCount是两个不同字段；AddBdefend会夹到0，不能用于原有符号累加。现有两个字段及copy/checksum保持，禁止全局alias或删除legacy字段。
- CharacterHitResolver/DatHitResolver里另有OID300特殊分支的HitStateCount45、kind7旧读取；它们不在本256 OID78向量覆盖内，须先证当前playable路由，不按grep批量替换。

减伤分支另外存在current/Prev2状态与Y参考条件差异，后续字段修复不得自动宣称完整defend response已对齐。当前原source在6743写45、7182有符号累加、7207按current状态7/70/75及collision Y reference做响应；Unity现有分支使用Prev2/地面0，需显式记录并独立处理必要耦合。

## 验收范围

本轮只新增诊断CPP和审计文档，没有修改Unity生产脚本或资源，没有运行新的Unity编译/SelfCheck/Play。前轮完整driver的bdefend0/45和spark RNG失败仍存在，不借本source通过关闭父包。first/repeat JSONL、stderr、build-manifest与validation均保存。

下一BDEFEND-FIELD-FAMILY-UNITY-001从准确caller/子Record与源256 RED开始，随后actual/armor reader/Shadow一起修改并验证两factory和既有恢复。之后HIT-SPARK-TRANSACTION-AUDIT，再回当前四个完整driver失败。总目标ACTIVE，禁止computer-use，非战斗/Unity-GAS/Scene/资源/Server边界保持。
