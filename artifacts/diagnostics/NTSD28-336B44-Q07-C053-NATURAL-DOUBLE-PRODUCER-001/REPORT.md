# C053 两名安科各由正式 OPoint 生成攻击者的自然双 Uj 源码见证

状态：`VERIFIED_SCOPED_SOURCE_DOUBLE_PRODUCER / C053_OPEN`（2026-10-02）。本包证明当前 336B44 对应 playable `GameSession28::step` 中，**三名受控初始角色**经正式资源/完整战斗pass自然生成目标及两名攻击者，tick7同一OID808目标受到两次`applied/effect2/Uj156`。三人初始 action511/553/511 是受控设置，不是物理按键自然选招；根正式EXE三人同初态、Unity原Scene三人、逐hit锁存值仍待。

正式身份：根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，诊断编译链接其对应 playable 构建闭包并只读正式 `resources/runtime`。新[诊断源码](../../../Tools/NTSD28Q07Diagnostics/c053_natural_double_producer_probe.cpp) SHA-256 `4BB77FB89D7C50B4BEF6ED2A384842ED1F1C92D12199892DF37C836B4675E53F`；[编译参数](compile-argv-v1.txt)沿用上一个正式闭包的g++目标并只替换诊断源码/产物路径，[编译结果](compile-result-v1.json)exit0、stderr空。新exe SHA-256 `2AAA8E6DC18B93129C6EAD73D5ECC2B5F8252CF360D216122B592595B8830D1F`，没有覆盖根正式EXE。

初态：slot0 OID65安科team1/action511/X580、610或640，slot1 OID702自来也team2/action553/X500，slot2 OID65安科team1/action511/X580、610或640；三人Y0/Z400、HP/MP500、seed682973786、mode0/background1、后续中性输入。3×3＝9组，每组12个完整tick，共108行。[首次源码CSV](source-run-01.csv)和[第二次源码CSV](source-run-02.csv)逐字节SHA均为`1133611D407D5A8AF3495902F8857A62B3D9BD841B7B9A8BD491D8C857555CBD`、各7180字节，详[复跑表](source-repeat-hashes.json)。两次运行各exit0/stderr空。

9/9组均在tick1自然生OID808/slot50、tick4自然生两名OID875/slot51与52，且两攻击者owner分别为slot0和slot2；tick5两者进入action55。tick7两条命中严格按slot51、52顺序为`51:0:2:156;52:0:2:156`，均为applied/effect2/Uj156，子体末态action156/HP450；其它tick没有双Uj。X580/580例tick8两个安科又各生成下一名OID875，说明诊断捕捉到连续正式OPoint而非手动补体。所有9条阳性已独立解析断言owner各1、命中序列及HP450。

正式根LFR CLI只提供slot0/1初始动作覆盖；本案例还要求slot2/action511。此前C053三人载体在根tick0重建slot2为action0而不是受控action55，见[旧载体更正](../NTSD28-336B44-Q07-C053-NATURAL-REACH-001/ROOT-CARRIER-CORRECTION-20261001.md)。本包没有用不等价初态的根回放宣称同态。当前源码完整tick阳性满足进入下一独立原Battle Scene三人验证的前置；须先建单独Task/Change并从原Editor非Play/Scene clean开始，不改DAT或生产逻辑。

未改 Unity C#、DAT、图片、场景或非战斗代码；本包仅离线C++诊断，不需要全量Unity测试。C053/Q07与总目标继续开放。
