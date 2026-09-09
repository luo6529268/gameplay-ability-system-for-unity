# Task Contract — NTSD28-B4-F04-SHARED-TYPE2-4-6-REFERENCE-001

> 状态：`VERIFIED / SHARED_TYPE2_4_6_REFERENCE / DERIVED_AND_OID999_PENDING`

## 目标

在 shared non-character DAT frame-advance owner 中，将 type2/4/6 迁到已验证的
reference-aware mechanics result：严格按正式 `PhysicsIntegrator28` 的 contact predicate、
collision-Y clamp、type2 HP/反弹/停地和 type4/6 HP/反弹/停地分支写回。

## Authority

- 正式 EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- `source/ntsd28_core/src/simulation/physics_integrator.cpp` 的 type2、type4/type6 landing branches。

## 不变量

- type1 已验证路径不变；type3/OID999、derived `LF2WeaponBase` 路径不迁。
- type2 仅以 `contactY > effectiveFloorY` 为 landing gate；type4/6 还要求 pre-move `Vy > 0.0001`。
- Audio 仍只沿用既有 queue seam，不在本包声明 B10 完成。
- Authority、Scene、Prefab、DAT、资源与 importer 不修改。

## 验收

- focused 覆盖 type2 negative-reference bounce/settle、type4 soft/hard、type6 dead settle及 exact-contact noop。
- Unity 编译 0 error；相关测试、`.*NTSD28.*`、SelfCheck 通过；Scene 基线不变。

## 回滚

删除 focused test，恢复 shared type2/4/6 使用 legacy bool `WeaponDynamics` 与 y=0 landing wrapper；
无数据迁移。

## 验证结论

- test-first red job `92d08dc00ec3405189bee22d3fcbb46e`：6/6 失败，均命中旧零地面/reference predicate 差异。
- 首次实现 job `c20531d72d244c3682952ffc47defb7a` 5/6；唯一失败是夹具缺 production action 40，补齐后 focused job `bf2fdf0f4ef54d5b9655711cd35ff1e9` 6/6。
- related job `271d1da2ac2d4ee086bb93f2eb828583` 31/31；NTSD28 broad job `b10718cd1f5948a08eb44262f9cfb7fa` 486/486。
- `BattleRuntimeSelfCheck` 于 `2026-09-05T06:03:19Z` PASS；clear 后 Console 0 error；Scene hash/length/mtime 不变。
