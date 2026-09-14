# 命中火花原事务与Unity入口审计

VERIFIED / SOURCE_MODEL_ONLY。438输入、2994断言，first/repeat逐字节一致，SHA b5df61136227ffcc17f15947ca22360d1234586ca71aa94907632d5e8eece6f8。

构建实际运行Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28HitSparkSource -RunnerSource Tools/NTSD28AuthorityTrace/hit_spark_transaction_witness.cpp -ExecutableName hit_spark_transaction_witness.exe，退出0。正式EXE SHA B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033、75源manifest SHA07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F保持。两遍runner均退出0、stderr空；build-manifest、完整CRT记录及数组前后保留。

## 原结果

- selection216、armor36、geometry144、capacity36、missing6。
- 366例追加一个event并恰好消耗两次CRT；72例不追加且CRT0；全部Native synchronized调用0。435次原status applied、3次因attacker snapshot不可用而unsupported；target snapshot不可用可在后段阻止spark，不能将其与attacker前段拒绝混同。
- host按较大整数Z选择，平局较大slot；所选host满10时在取帧/RNG之前返回，另一个host是否满不能影响结果，原有记录保持不变。
- 仅itr.spark恰好-1抑制；100..199编码为0..99，99/200等走默认序号算法；fall0和60属低档，61属高档。
- 无护甲effect1把native_group改1；其余按原candidate.interaction_index。仅reduced调用允许selected armor非零spark覆盖itr.spark；feedback调用虽然有selected armor，gate=false，不能覆盖。armor.spark=-1本身不等同禁用。
- 取当前definition的snapshot中心；cover非零替代itr.y；负数平均用整数/2截向0，非右移；最后加入target Z，非attacker Z；先CRT Y再CRT X。实际CRT记录含result/stateAfter/totalCalls（本seed初始化已消费3000 CRT），不能假设首调用编号1或两次扰动相等。
- before数组完整前缀和非owner数组均保持；所有追加event的host/id/X/Y和CRT顺序已逐例比较。

范围：所有fixture itr.kind=0；kind非0的直接guard只有源阅读证据，不能称动态矩阵覆盖。位置/current/snapshot在冻结候选之后扰动属诊断初值，不等于完整driver输入复现。本轮没有改Unity生产或运行Unity测试。

## 关键入口纠正

LF2Character.Hit在当前type0时委托DatHitResolver。DatHitResolver普通kind0分支调用ApplyStandardCharacterDamage后RecordKind0Hit并立即return，因此本轮已失败完整driver实际走**LF2Entity.RecordKind0Hit**，不是文件底部SpawnSpark。后者仍有旧随机代码，但只改它不会修当前失败。weapon/special的DamageWriter尾部同样调用RecordKind0Hit。旧LF2CharacterHitResolver是否还有其他真实caller需单独确认，不能因为构造了实例就认为production调用它。

RecordKind0Hit已选raw Z/slot host并先检查容量，但仍使用旧随机流、current中心、attacker Z、右移平均、简化phase，缺少显式spark和护甲上下文。AddHitRecord有10容量并写int.MinValue lastAdvance，C01/C25生命周期既有验证保持，不修改SpriteRenderer。

## 后继实现约束

1. 必须传递原candidate index。统一SequenceRunner已持有itrIndex；旧CurrentItrIndex只由Character BeforeDispatch写，Dat BeforeDispatch为空，不能把该持久值当通用可靠上下文。明确瞬时作用域/异常恢复或显式参数方案，避免新字段进入snapshot或池复用残留；直接compat调用的索引规则单列。
2. 必须保留hit前选定的armor和分支信息。ApplyStandardCharacterDamage已拥有route，reduced/type1/bypass不能在扣血和Bdefend更新后重新匹配来推断spark上下文。
3. 公共emitter应使用现有World.NativeRandom.CrtNext和entity hit record数组；每条成功路径只发一次。在实际damage route owner接入时，必须移除/抑制对应外层重复RecordKind0Hit调用，不能把emitter放两个层级。
4. 已缺失的非角色selected armor反馈后续调用同一公共emitter，且遵守其原special-link-rest/armor选择前置，不用空return替代。
5. emitter是无独立资源的writer，使用现有记录/关闭owner；不新建队列、pool或服务，不改C17、global BattleRandInt、资源/Scene/非战斗/框架。

下一HIT-SPARK-UNITY-001从438 source oracle和精确wiring Record开始；之后反馈→武器反应/type5 plan→回BDEFEND256与完整driver。总目标ACTIVE。
