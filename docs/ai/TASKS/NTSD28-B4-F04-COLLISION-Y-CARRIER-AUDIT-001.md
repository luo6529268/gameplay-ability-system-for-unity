# Task Contract — NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_BOUNDARY_DEFINED`

## 目标与结论

闭合 Authority `EntityState28::collision_y_reference` 的所有写入、复制、重置与主要读取边界，
并确认 Unity 当前是否存在可复用字段。

结论：Unity 没有等价独立 runtime carrier；`YInt`、`EnvironmentState320`、stage Z boundary、
`Type3VisualZOffset` 都不能复用。下一包只新增 deterministic signed int carrier，覆盖默认/完整
reset、canonical copy、snapshot/restore/checksum/parity/raw projection；不同时伪造 operation30/platform
producer，也不立即改 physics/next999/teleport/input/hit consumers。

## Authority owner

- 每次 geometric candidate rebuild 先把所有active slot reference清零。
- operation30在严格previous/current Y交叉条件后，仅当current reference更低时写入，并同时写
  platform source slot与render shadow offset。
- linked-platform frame motion若有dvy，跟随后把reference写成新integer Y。
- defusion复制primary reference到restored partner。
- spawn/reset默认0；physics只消费，不生产。

## 下游路由

- B4：physics floor/friction/landing、state400/401 teleport、next999、linked platform motion。
- B5：operation30 producer、hit-reaction above-reference predicates。
- B6/B8：fusion/held/platform相关复制与表现offset。
- B2/input：grounded/airborne predicates在carrier接通后另包迁移。

## 不变量

- carrier package不声称producer已连接，也不改变零地面现有行为。
- 不把collision reference与物理Y或environment state合并。
- 不修改Scene、DAT/资源、Authority或平台碰撞算法。

