# NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan standard/reduced/type3/weapon/flute/held/input/recovery accounting fields and Unity legacy-stat/exact-accumulator writers; EXE B1E13AE1, closure 39DDDA15.
evidence: exhaustive production mutation scan found 13 ComboCountVic, 8 ComboCountAtk, 3 actual KillStat, 7 actual DamageStats and 3 actual KillStats writes. Standard/reduced lack exact KO; held lacks all exact accounting; input compat and negative recovery lack exact HP accounting. Type3/weapon/flute are independently retireable. Four prerequisite routes frozen; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / GLOBAL_RETIREMENT_BLOCKED / FOUR_PREREQUISITE_ROUTES`

全局retirement不能直接开始：删除旧writer前必须补standard/reduced exact KO、分离B5安全退休面、审计input
compat/negative recovery，并在CPoint throw runtime gate后完成B6 held accounting。完整矩阵与顺序见同ID Task。

2026-09-09进展：standard/reduced exact KO与type3/weapon/flute安全legacy stats retirement已分别由
`NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED`和
`NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001 / VERIFIED`闭合。下一严格包为
`NTSD28-B5-INPUT-HP-COST-COMPAT-STAT-RETIREMENT-AUDIT-001`；negative recovery、B6 held与schema阻塞不变。

2026-09-09后续：input compat审计已验证该路径仍可配置为production且旧cost transaction不完整，局部
writer retirement不安全。先执行
`NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001`复用既有exact action core；
negative recovery、B6 held与schema阻塞继续不变。

2026-09-09后继：input shared transaction production已验证并退休两处legacy writer。下一先完成
negative-WeaponCount recovery owner audit；B6 held与schema阻塞继续不变。

2026-09-09后继：negative recovery owner audit已证明WeaponCount是错误carrier，且完整exact consumer依赖
world rule +0x90/default9 carrier。下一按data-first顺序实施rule carrier；B6 held与schema阻塞不变。
