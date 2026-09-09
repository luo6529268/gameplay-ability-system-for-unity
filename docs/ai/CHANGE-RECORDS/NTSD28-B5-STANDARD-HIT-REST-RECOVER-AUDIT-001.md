# NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001 — standard hit rest/recover audit

<!-- CHANGE-RECORD
id: NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp 3854-3920 and all apply_standard_hit_rest callers; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-3854-3920-EXACT / AUTHORITY-405-DAT-REACHABILITY / UNITY-138-DAT-REACHABILITY / THREE-MISSING-CARRIERS / NONDEFAULT-TIMING-DIFFERENCE / IMPLEMENTATION-SPLIT / LEDGER-PASS / SCENE-D4266C6D-UNCHANGED / NO-CODE-NO-CONTENT-NO-SCENE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_RESOLVER_SPLIT_DEFINED`

## 审计结果

- Authority exact formula见`docs/ai/MANIFESTS/NTSD28-B5-STANDARD-HIT-REST-RECOVER.md`；world reduction
  clamp0..5，战前menu循环并由GameSession写入World，默认0。
- `InteractionArea.recover`、`LF2CharacterData` definition effect与world timing reduction在Unity均缺typed carrier；
  current DamageWriter/HitPlan重复写raw 3/-3与未reduced arest/vrest。
- 正式405 decoded DAT无显式ITR recover、无非零bmp effect；Unity冻结138 DAT同样为0。authority显式vrest
  3/10/15/20/30、Unity arest15/16/20与vrest1/5/10/17/20均在byte范围。
- 因而default reduction0/current content暂无数值首差；正式nondefault1..5会改变hold/rest，Unity缺失为
  confirmed observable difference。用户排除的是完整selection flow，不妨碍battle world carrier。
- 下一carrier包只补数据/reset/copy/parser/snapshot/checksum/parity，不接行为、不改Config/Scene；随后再做pure
  resolver和actual/HitPlan integration。
