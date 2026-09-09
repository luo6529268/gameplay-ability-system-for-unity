# NTSD28-B4-F04-SHARED-TYPE2-4-6-REFERENCE-001 — shared type2/4/6 reference landing

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-SHARED-TYPE2-4-6-REFERENCE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4SharedType246ReferenceEditorTests.cs
authority: NTSD 2.8-Logan PhysicsIntegrator28 type2 and type4/type6 landing branches; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED6-JOB-92d08dc00ec3405189bee22d3fcbb46e / INTERMEDIATE5-OF-6-FIXTURE-ACTION40-CORRECTED / COMPILE0 / FOCUSED6 / RELATED31 / NTSD28-BROAD486 / SELFCHECK-PASS-2026-09-05T06:03:19Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / SHARED_TYPE2_4_6_REFERENCE / DERIVED_AND_OID999_PENDING`

Unity 当前 shared type2/4/6 仍由 `WeaponDynamics` 的零地面 bool 驱动，因而丢失 negative
`CollisionYReference`、exact contact 和 type-family predicate。实现只迁 shared owner；derived、
type3/OID999、producer、Audio/content 均不在本包。

回滚按 Task Contract 恢复旧 wrapper；无数据迁移。

focused red job `92d08dc00ec3405189bee22d3fcbb46e` 6/6 失败：type2/4/6 的
negative-reference clamp/落地均仍走 y=0，type4 exact-reference contact 还错误追加重力。
这些失败与正式 predicate/result 差异一致，作为本包有效 test-first red。

首次实现后 job `c20531d72d244c3682952ffc47defb7a` 为 5/6；唯一失败来自 focused
fixture 缺少 production pre-physics 会切入的 action 40，导致 state fallback 成 0。补入保持
原 state 的 action 40 后再复跑；该 fixture 更正不改变最初 6/6 red 事实。

## 实际改动

- `BattleNonCharacterMechanicsStepResult` 显式提供 effective-floor penetration 与 positive-downward predicates；type1 predicate 语义不变。
- shared type2/4/6 与 type1 共用 reference-aware mechanics；type2 只要求 strict penetration，type4/6 另要求 pre-move positive Vy。
- type2 bounce/settle 与 type4/6 bounce/settle 均把 precise Y 写到 `CollisionYReference`；type2 soft settle 还按正式规则把该 reference 写入 Vy。
- legacy 四参数 landing wrapper、derived owner、type3/OID999、producer、Audio/content 均保持。

## 验证

- Unity 编译及最终 Console：0 error。
- focused `bf2fdf0f4ef54d5b9655711cd35ff1e9`：6/6。
- related `271d1da2ac2d4ee086bb93f2eb828583`：31/31。
- NTSD28 broad `b10718cd1f5948a08eb44262f9cfb7fa`：486/486。
- SelfCheck：`2026-09-05T06:03:19Z` PASS。
- Scene：`0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`，未变化。

## 未关闭

derived `LF2WeaponBase` 仍使用 legacy bool/zero-floor owner；type3/OID999、collision-Y producers、
type0 landing actions 与 B10 Audio 仍待后续包，本记录不声明 F-04/B4 完成。
