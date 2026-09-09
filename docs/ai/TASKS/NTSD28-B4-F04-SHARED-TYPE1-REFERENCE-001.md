# Task Contract — NTSD28-B4-F04-SHARED-TYPE1-REFERENCE-001

> 状态：`VERIFIED / SHARED_TYPE1_REFERENCE / DERIVED_AND_OTHER_TYPES_PENDING`

## 目标

新增reference-aware non-character core result，并只迁`RunSharedNonCharacterDatFrameAdvance`的type1
分支：negative floor上的friction/gravity/contact与strict landing gate正确，落地Y写
`CollisionYReference`。旧`WeaponDynamics`和derived weapon保持兼容，其他type不迁。

## 不变量

- type1 threshold kernel与HP/Vx/action/facing/sound语义保持。
- type2/3/4/5/6、derived OnLanded、producer、Audio/Scene/Authority不改。
- 新core/result无托管分配；旧bool wrapper行为不变。

## 验证结论

- 初次 `30ac14f7db854011bfbce9cc4106e291` 的 2/2 失败因夹具实际落入 type5，已作废，不能作为有效红灯。
- 更正为当前 DAT type1 后，生产路径 `83453150d266427ea699b73e8f80564b` 2/2 通过；最终 focused `e5b135f1c8354c29836b96a5e3800c5a` 4/4。
- related `223bd68ec20840a99ba825d926cdee43` 48/48；NTSD28 broad `36ceb856e86c4910851544da228e7ff4` 480/480。
- `BattleRuntimeSelfCheck` 于 `2026-09-05T05:47:11Z` PASS；Console 0 error；Scene hash/length/mtime 未变化。
