# Task Contract — NTSD28-B4-F04-TYPE3-OID999-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / REMAINING_NONCHAR_SEAM_DEFINED`

## 目标

只读闭合正式 type3 special-state、real OID999 override 与其余 non-character common tail，
确认 shared/derived 最后仍使用 legacy y=0 core 的对象范围。

## 结论

- type3 在 contact side 的 state 3000/3006/3007 总是 clamp 到 collision reference；仅 `hit_g!=0` 时切 action 并清 XYZ motion，且不重置 frame counter。
- real `object_id==999` 且 contactY严格 `< -9` 时，在上述 type3 helper之后覆盖为 Y/Vx/Vy=`+9`、action101、counter reset；Vz保留 helper/摩擦后的值。alias/type_sub 999 不触发。
- 非type3的其余对象也能进入相同 real-OID999 tail；普通 type5/contact common tail仍必须消费 reference-aware friction/gravity，即使不选择action。
- Unity shared普通type3/type5与derived非常规nonchar仍在legacy bool/y0 core；现有OID999写成 y0/Vx0/Vy0，与正式+9 tail相反。
- 最小包应在 `LF2Entity` 建立单一 type3/OID999 landing body，由shared和derived result owner调用；同时让所有nonchar使用result core。legacy direct wrapper暂保留，后续owner-retirement审计再裁决。

## 不变量

不改collision-Y producer、type0 action、Audio/content、Scene/Authority；不将alias999当real OID。
