# Task Contract — NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / PHYSICS_SPLIT_DEFINED`

## 目标与结论

逐段闭合当前 NTSD 2.8-Logan `BattleWorld28::step_physics`、
`PhysicsIntegrator28::step` 与 Unity C06 生产物理路径的 gate、输入、写入和分支所有权。

审计确认 Unity 已具备 C06 升序 slot owner、`FrameDelay` 有符号倒计时、`LinkState<0`
阻断、方向碰撞标志消费、单位摩擦、主重力常量和多数零地面落地分支；但完整 F-04
不能一次性迁移。首个可独立证明且不依赖缺失 carrier 的差异是 type 1/state 1002
落地阈值：Unity 使用旧 `9.9`，正式 2.8-Logan 读取
`5.2571022450498032e120`，因此正常有限冲击应落到 action 70，而不是 action 7 反弹。

## 拆包

1. `NTSD28-B4-F04-TYPE1-LANDING-001`：只闭合 type 1/state 1002 阈值与分支。
2. collision-Y reference carrier、effective floor、friction/contact predicate 与整数同步。
3. type 0 airborne/ordinary/state12/18 landing 和 environment damage。
4. type 2、type 3/OID999、type 4/6 landing，以及 type4/OID120/OID101 独立 X extras。
5. 收敛 parallel `CharacterMechanics`/`WeaponDynamics`/derived weapon owner，并保留 direct compatibility。

## 不变量

- 不修改正式 Authority 目录、DAT/资源、Scene、Audio 或 B5/B7/B10/B11 行为。
- 不把 type 1 阈值小包写成完整 physics、完整 type 1 或 B4 已对齐。
- collision-Y carrier 未建立前，不私自用 Unity `Y==0` 推导其生产者或 OID999 行为。

