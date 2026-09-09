# Task Contract — NTSD28-B4-F04-NONCHARACTER-PHYSICS-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NONCHAR_SPLIT_DEFINED`

## 结论

非角色路径不能一次性把`WeaponDynamics(bool crossedGround)`改成完整Authority integrator：
negative reference下，core必须保留contact Y，type1/2/4/6 landing各自决定clamp/action，type3/OID999
还读取`contact_y < -9`。先处理不依赖该result seam的identity X extras。

当前`LF2Weapon.WeaponFlightPhysics`有confirmed difference：Authority的“type4或OID/alias120加20%”与
“OID/alias101减20%”是两个独立operation；Unity用了`if/else if`且derived path只看alias。

## 拆包

1. identity X extras：独立operation、real ObjectId和alias都参与。
2. reference-aware non-character integration result seam：contact/effective floor/strict penetration。
3. type1/2/4/6 landing坐标与写入。
4. type3 special-state与OID999发射。
5. 统一shared/derived owner并清除重复路径。

