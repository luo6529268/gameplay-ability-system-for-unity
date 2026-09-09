# NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001 — derived weapon reference physics

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-DERIVED-WEAPON-REFERENCE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4DerivedWeaponReferenceEditorTests.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 type1/2/4/6 reference landing and frame suppression; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED5 / COMPILE0 / FOCUSED5 / RELATED59 / FIRST-BROAD-INFRA-LOG-POLLUTION / POLLUTED-GROUP5 / FINAL-NTSD28-BROAD491 / SELFCHECK-PASS-2026-09-05T06:21:49Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / DERIVED_TYPE1_2_4_6_REFERENCE / TYPE3_OID999_PENDING`

实施边界来自 `NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001`。只迁正常 pooled
LF2Weapon 的 derived physics/result/landing seam；shared、type3/OID999、producer、Audio/content不改。

回滚见 Task Contract；无数据迁移。

focused red job `f3b81d994bf34b199795c923722da3f4` 5/5 失败：正常 pooled
type1/2/4/6 均仍遗漏 reference-aware landing/exact-contact，且 frame cpoint kind2 未抑制
derived physics。失败与审计差异逐项一致，为有效 test-first red。

## 实际改动

- normal pooled type1/2/4/6 在保留 `WeaponFlightPhysics` specialization 后改用无分配 result core。
- in-flight callback 改读 effective-floor `Airborne`；各type landing predicate与shared路径一致。
- collision reference通过带 `landingY` 的同调用栈 virtual overload进入 `LF2Weapon` landing；旧无参 virtual仍保留为基类兼容 fallback，不新增snapshot字段。
- type1 五参落地写回使用传入 reference；type2/4/6复用同一正式落地body。
- derived frame cpoint kind2 现在在任何速度/位置写入前抑制 physics。
- type3/OID999 landing、producer、Audio/content未改。

## 验证

- focused `bfe1ff98186a49b29df424ed36ad0183`：5/5。
- related（含snapshot、pool allocation、C06及shared/derived physics）
  `bb8ded5d176f4f3ea534e2d87f69b3d6`：59/59。
- 首轮 broad `10b0618e2b92414e892b5b98e82f5a32` 的唯一失败来自本代理轮询超时后
  MCP 写入 `disposed NetworkStream` Error，被日志严格测试捕获；对应组
  `b199e327e1544aeebba1d4fa1b08c1cc` 单独复跑5/5。测试期间不再连接MCP后，最终 broad
  `735386e61b3c454d9442288899acbd64` 491/491。
- SelfCheck `2026-09-05T06:21:49.8326133Z` PASS；最终 Console 0 error。
- Scene `0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z` 未变化。

## 未关闭

type3 special-state landing、OID999 tail、collision-Y producers、type0 action tail及B10 Audio仍待；
不得将本状态扩大为F-04/B4完成。
