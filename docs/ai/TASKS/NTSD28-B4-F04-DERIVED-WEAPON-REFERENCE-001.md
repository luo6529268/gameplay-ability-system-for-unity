# Task Contract — NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001

> 状态：`VERIFIED / DERIVED_TYPE1_2_4_6_REFERENCE / TYPE3_OID999_PENDING`

## 目标

把正常 pooled `LF2Weapon` derived owner 迁到已验证的 reference-aware non-character mechanics，
同时保留 `WeaponFlightPhysics` specialization、virtual landing/in-flight seam 和既有 snapshot 字段。

## 不变量

- shared type1/2/4/6 已验证路径不变；type3/OID999/producer不改。
- held/link/cpoint frame suppress gates不得放宽。
- type1、type2、type4/6 各自 strict landing predicate与落地写回复用同一正式语义。
- 不新增跨 tick 状态或 snapshot schema；collision reference只通过同调用栈virtual overload传递。
- Authority、Scene、Prefab、DAT、资源、Audio/content不改。

## 验收

- test-first red 覆盖正常 pooled（type==pool type）negative-reference landing及 exact-contact gravity。
- 兼容无参 `OnLanded` virtual fallback；既有 snapshot、shared、C06 与武器测试通过。
- 编译 0 error、focused/related/NTSD28 broad/SelfCheck PASS、Scene不变。

## 回滚

删除 focused test，恢复 derived `WeaponDynamics`、absolute-Y in-flight gate与无参 landing 调用；
无数据迁移。

## 验证结论

- test-first red `f3b81d994bf34b199795c923722da3f4`：5/5 失败。
- focused `bfe1ff98186a49b29df424ed36ad0183`：5/5；related `bb8ded5d176f4f3ea534e2d87f69b3d6`：59/59。
- 首轮 broad `10b0618e2b92414e892b5b98e82f5a32` 仅因测试期间 MCP 轮询超时写入 disposed-stream error 而污染一项；被污染组 `b199e327e1544aeebba1d4fa1b08c1cc` 5/5。
- 无中途 MCP 连接的最终 broad `735386e61b3c454d9442288899acbd64`：491/491。
- SelfCheck `2026-09-05T06:21:49.8326133Z` PASS；Scene不变，最终Console 0 error，Ledger PASS。
