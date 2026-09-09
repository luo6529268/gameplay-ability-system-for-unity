# NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: Direction-B 138-DAT raw manifest and normalized projection produced by Unity Decryptor -> ParserV2 -> Converter, current data.txt indexed subset, NTSD 2.8-Logan battle consumers; EXE B1E13AE1, closure 39DDDA15.
evidence: root cause proven as one-line-only temporary regex omitting multiline current DAT subblocks; normalized current counts are WPoint7995/CPoint1426/ITR4437 versus stale312/33 subsets; indexed kinds2/3/10/11 corrected to117/249/15/5; holder primary WPoint kind3=770, authored-DV overlap=1, nonkind3-DVX=184, terminal=28; current CPoint kind1/state9=776/760, front/back=170, positive held injury=223, throwvx=58 and positive throwinjury=48; current OID417 tree yields40 invalid post-vaction pairs; owner rules retained while dormancy/release-only labels and matrices are rebased; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / ROOT_CAUSE_MULTILINE_OMISSION / CURRENT_REACHABILITY_REBASELINED / OWNER_RULES_RETAINED / PRODUCTION_HELD`

旧current统计只匹配单行DAT subblock，漏掉大量多行WPoint/CPoint/ITR。Direction-B正式projection把current
重基线为WPoint7995、CPoint1426、ITR4437；terminal held、kind3 authored overlap、native impact与catch
post-vaction invalid均已有current-content witness，不再是dormant/release-only。

Authority/Unity code对照的规则与owner保留；所有受影响production矩阵扩大。terminal structural owner已由
`NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001`闭合，missing-action为下一只读audit。详细更正与原记录路由见
`docs/ai/TASKS/NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001.md`。本轮无code。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

本Record中的770/184/28是ITR-pickup子集；完整ITR+OPoint union由
`NTSD28-B6-HELD-RELATION-PRODUCER-DOMAIN-CORRECTION-AUDIT-001`纠正为811/186/33。
multiline根因、其他domain计数与owner结论保留。
