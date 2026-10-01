# NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001

状态：`VERIFIED_SCOPED_SOURCE_DOUBLE_PRODUCER`。父项：新版336B44 G1/BATCH-04/Q07/C053/`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：在正式对应的 playable `GameSession28::step` 完整 tick 中，用两名OID65安科各从正式action511经OPoint自然生成OID875/action50→55，另一名OID702自来也从正式action553自然生成OID808/action150，寻找同一tick对OID808的两次`applied/effect2/Uj156`。这比既有人工放入两个OID875/action55的受控四对象案例更接近自然生成，但三名初始动作仍为受控设置，不代表物理按键选招。

修改范围：只新增 `Tools/NTSD28Q07Diagnostics/c053_natural_double_producer_probe.cpp` 和本Task/Change/总表/诊断。编译链接当前336B44 playable闭包，正式DAT只读；不得改正式源码、Unity生产、DAT/图片、Scene/Prefab或非战斗。有限搜索两安科初始source X的9组组合（580/610/640）与自来也X500、Y0/Z400，seed682973786、mode0、team1/1/2、各MP500、中性后续输入，12tick记录每名攻击者/目标出生与逐hit顺序；先单跑全部组合，阳性再单独双跑并核原件SHA。

验收：编译0错误、明确记录正式内容身份/编译argv/有限搜索边界、阳性命中原件或明确有界阴性；同输入阳性复跑产物逐字节一致。若找到双Uj且正式根EXE有同初态可用入口，再核根；现有LFR CLI只支持slot0/1动作覆盖，三名受控动作的根可比性须单独证明，不能预先宣称。源阳性后另建原Battle Scene Task，原Scene每次Play前核idle/clean、退出四SHA，不能借本Task修改Unity测试脚本。

风险：三名自然生成项目可能互相命中、改变动作或因槽位排序而不同于人工双攻击者；只按源实测推进。回滚只审阅本包新诊断/文档；任何删除按用户删除记录规则单独审计。脚本前须建立Change Record及Ledger/STATE/handoff，验证使用最窄案例，不重跑全量旧测试。

结果：正式源码9/9有限组自然双Uj，tick7两个独立owner的OID875按槽51/52各`applied/effect2/Uj156`，OID808 HP450；两次完整CSV逐SHA相同。仅关闭声明的源码可达性子包；根三人动作载体、Unity原Scene与物理键仍待。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/REPORT.md)。
