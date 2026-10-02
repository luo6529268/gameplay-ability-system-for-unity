# Q07/D-024 武器持有 WPOINT 物理比例只读审计

状态：`CURRENT_UNITY_RATIO_FIRST_DIFFERENCE / 336B44_ROOT_PAIRED_TICK_PENDING`。此报告不把旧版 CSV 升级为新版正式 EXE 证书。当前战斗规则权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 和对应 playable live source；固定完整背景下的视口比例来自用户 D-024 例外。

2026-10-02 当前源码补证（覆盖下方“旧CSV不足”的时间快照）：用当前 `336B44` 树中的全部28个core翻译单元和playable `game_session.cpp`、`selection_flow.cpp` 重编译既有只读诊断，首轮遗漏 `-municode` 链接失败原日志保留，第二轮编译退出0；新构建探针SHA `1E45367E6753460460220C44F64B3A8E4745F35E28EC5CD554199511FF47A25C`。在当前正式 `resources/runtime` 运行 GameSession 24tick，pickup=1、持有X移动9tick、Z移动7tick；新版 `native-336b44-24.csv` 与历史 `native-24-v2.csv` **逐字节SHA相同**，均为 `1398857F6703AF7601999798CFB27D2F2E2E17657C2762DAA265328BF4C820C2`。所以该有界源规则样本已获得**当前对应源码**同态复核，仍不能把重建诊断程序冒充根正式 EXE 本体。原Editor同内容测试的24tick源字段可沿相同CSV继续核验，物理比例首差不变。

当前 336B44 `source/ntsd28_core/src/simulation/battle_world.cpp` 的 reciprocal held-object pass（约8050～8120行）从持有者源整数位置及两端 frame `centerx/centery/wpoint` 求子体源坐标，并把子体 Z 置为持有者 Z 再按 cover 调整。Unity 生产 `BattleHeldObjectWriter.SyncHeldFrameAndPosition`（约150～183行）已独立写相同种类的 `SourceRuleX/Z`，但物理 X 先通过 `holdpoint.x + raw center/wpoint`，Z 直接取持有者物理整数 Z 再加原始 cover 1；没有调用共用 `BattleSpatialProjection.SourceDeltaToViewX/Z`。`LF2WeaponHeldStateResolver.ApplyHeldWPointSync` 的 Legacy 路径也有相同形态；`BattleInteractionWriter.AlignGrabPair` 是另一个位置写者，只列待查，不在本报告宣称首差。

原 Editor 上运行既有 `NTSD.Test.Editor.NTSD28Q07HeldWeaponDualDomainEditorTests.NaturalPickupMovingHeldWeapon_KeepsRuleAndScaledViewDomains`：EditMode `1/1 PASS`，24 个完整 Driver tick，采用当前正式 `resources/runtime` 的 OID120 内容。此测试仍读取历史 `native-24-v2.csv` 并断言武器与角色物理整数间距等于源整数间距；通过只证明旧断言成立，不证明满足当前用户要求。新产生的唯一 CSV `artifacts/diagnostics/NTSD28-Q07-HELD-WEAPON-DUAL-DOMAIN-FULL-TICK-001/unity/held-weapon-20261002T022946590-442f64940cb44691bcaf0c69d2a181e8.csv` 给出如下直接读数：

| tick | 源 X 间距 | Unity 物理 X 间距 | 乘 2048/1333 应有间距 | 源 Z 间距 | Unity 物理 Z 间距 | 乘 1152/730 应有间距 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 2 | 13 | 12.463616 | 19.972993 | 1 | 1.000000 | 1.578082 |
| 8 | 21 | 20.318080 | 32.264066 | 1 | 1.000000 | 1.578082 |
| 9 | 21 | 20.172543 | 32.264066 | 1 | 1.000000 | 1.578082 |
| 17 | 8 | 7.752882 | 12.291073 | 1 | 0.687671 | 1.578082 |

此处确认为**当前 Unity 对用户 D-024 比例目标的物理位置首差**，且存在共用正式源码对应的武器持有入口；旧版 native CSV 不足以宣称已获当前336B44根 EXE同 tick证书。下一独立包先取得当前正式源/根或明确不可用的同条件出口，然后以同一个世界投影处理实际可达的 canonical 与 Legacy 武器持有写者，并更正旧测试的物理间距断言；不能修改 DAT 或将坐标比例散落为新常量。未改生产、DAT、图片、Scene、非战斗。
