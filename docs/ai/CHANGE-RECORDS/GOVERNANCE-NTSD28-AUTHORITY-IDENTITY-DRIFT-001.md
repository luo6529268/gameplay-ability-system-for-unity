# GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001 — Authority物理身份漂移

<!-- CHANGE-RECORD
id: GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001
status: SUPERSEDED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: User-confirmed fixed authority contract 1277B70B and current on-disk designated-root identity observation.
evidence: EXPECTED-EXE-1277B70B / OBSERVED-ROOT-EXE-B1E13AE1 / ROOT-ONLY-ONE-EXE / OBSERVED-SOURCE-MANIFEST-5F2E5B41 / FROZEN-SOURCE-MANIFEST-C59BD8D3 / CORE-SESSION-BUILD-HASH-DRIFT / NO-AUTHORITY-WRITE / USER-CONFIRMED-BUGFIX / SUPERSEDED-BY-GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002
-->

> 状态：`SUPERSEDED / USER-CONFIRMED-BUGFIX / PROMOTION-002`

## 事实

2026-09-04在B3下一首差只读审计中重新计算指定根身份，发现当前根EXE和source manifest均不再匹配
`docs/ai/CURRENT-AUTHORITY.md`锁定值。具体哈希、时间、大小和恢复选择见Task Contract。

`source/README_SOURCE.md`文字与SHA没有变化，但“声明对应当前发行”不能覆盖固定EXE SHA合同；当前根EXE
也不能仅因文件名、版本资源相同或位于指定路径而自动晋升。

## 影响

- `NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001`暂停在source分类前，不产生代码改动。
- `NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001`的Unity编译/test/SelfCheck/Play证据保留；若当前新artifact
  被晋升，需针对新正式身份重新核验，不把旧证据自动继承为新权威证书。
- B3入口manifest的固定旧hash和pass表继续作为“先前锁定基线记录”，但在身份裁决前不得驱动新实现。

## 等待用户裁决

1. 恢复SHA `1277B70B...DAF75`的原正式EXE及对应source；或
2. 明确把当前SHA `B1E13AE1...9033`及source manifest `5F2E5B41...5FA9`晋升为新权威，并授权
   authority governance更新与受影响阶段重新基线。

## 写入边界

authority零写入；只更新workspace治理文档。

## 关闭

用户于2026-09-04确认当前artifact是其修复Bug后的预期版本，并要求继续。等待状态由
`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`关闭；旧身份成为历史基线，当前身份晋升为正式权威。
