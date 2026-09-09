# Task Contract — NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001

> 状态：`VERIFIED / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`

## 目标

在不修改 Unity 战斗生产行为、正式 NTSD 2.8-Logan 目录或双方内容资产的前提下，建立一个可重复的
owner-slot 专项双端 trace。该 trace 只比较当前 Authority `Entity28+0x354 owner_slot` 与 Unity
`NTSDEntityRuntime.OwnerSlotIndex` 的共同可观察字段，覆盖 direct self、ordinary two-hop OPoint、F8 固定
owner 99、state9996 clone owner -1、type3 owner mutation 以及同槽释放/复用。

## Authority 与现状

- 正式 EXE SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure manifest：
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- 当前 playable live path 已分别确认：`game_session.cpp` direct/story initializer写physical self slot；
  `object_spawning.cpp -> battle_world.cpp`传播parent literal owner；F8 post-tick tail写99；state9996 clone request
  默认-1；type3 hit tail把source owner写入target；`despawn/spawn_at`使同槽新实体只拥有新request字段。
- Unity route1～4已分别完成target去混淆、direct/stage self、mode2 F8 owner99及logic/presentation ordinary
  OPoint producer，并取得focused证据；当前缺跨两端的同一规范化owner事件序列和slot-reuse出口证据。
- 现有`NTSD28UnityRawCaptureEditor`固定为两角色/三tick完整实体schema；由于Direction-B内容差异及用户保留的
  Unity随机掉武器例外，它不适合把F8的OID/数量/位置误纳为本字段出口。专项trace必须显式排除这些非owner域。

## 修改范围

- `Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1`
  - 增加可选runner source、exe名和manifest名参数；默认行为与既有capture保持不变。
- `Tools/NTSD28AuthorityTrace/owner_slot_exit_capture_main.cpp`
  - 只读链接当前正式playable source closure，生成Authority owner-only规范化trace。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0OwnerSlotProductionExitEditorTests.cs`
  - 通过Unity正式producer/writer构造同序列；输出Unity trace并对Authority trace逐字段比较。
  - 同一文件提供请求式focused runner；输出只写`Temp/`。

## 不变量与排除

- 不修改任何战斗runtime生产代码、pass顺序、content、DAT、Scene、Prefab、ProjectSettings或Authority目录。
- F8只比较owner 99与spawn visibility；候选集、spawn数量、OID、RNG派生位置属于用户保留Unity随机掉落例外，
  不得伪装为相等。
- state9996只比较五个child的owner sentinel与slot顺序；其OID/RNG/运动合同由既有C25测试继续负责。
- OPoint trace必须经双方正式materializer，不得直接给child写owner；type3必须经正式hit continuation writer。
- slot reuse必须比较同一physical slot的generation递增与新owner，不能以旧对象字段reset代替新实体。
- 专项trace是字段出口证据，不替代全战斗same-seed/input/tick parity campaign，也不自动关闭B8 physical F8。

## 验收

- build helper默认参数仍能构建原`authority_source_capture_main.cpp`，可选参数能构建本专项runner；正式EXE
  hash与authority source manifest继续写入manifest，输出目录必须留在Unity仓库内。
- Authority与Unity trace header的schema、正式EXE hash、scenario、seed、input marker、observation tick一致。
- 规范化records严格同序、同字段、同值；至少覆盖：direct self 0/1、two-hop root/child/child、F8 first spawn
  owner99、state9996 slots50..54 owner-1、type3 target owner transfer、slot50 generation 1→2/new owner。
- focused Unity测试实际通过，trace comparison报告`equal-owner-trace`且first difference为空。
- 运行相关C++现有测试、Unity route1～4 focused回归、两套compile、SelfCheck和必要Play；阻塞项如实保留。

## 回滚

只移除本专项runner、focused test以及build helper的向后兼容可选参数；不得回滚route1～4生产实现、用户工作树
或既有通用trace基础设施。

## 实际结果（2026-09-09）

- Authority runner链接当前75-file capture source子闭包，build manifest source SHA
  `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`；runner/binary SHA分别为
  `6262BD1E4D424502DBC35C24494FE7941A66B37226074B481138C55336900FAC`与
  `8C58D72F187CC162B6247CFEC691F6AF5BA37727D4F2A5934AE9249EA678F784`。
- Authority两次实际输出均`PASS records=15`且字节相等，capture SHA
  `577313C83290597E9C293194EE2A9CCE714465B46B6C31A70F8916E7AD2E1DCC`。
- Unity focused于01:05:44及随后确定性复跑均`1/1`；两次Unity trace SHA均
  `C2DCF420395A10DDAAE83AA128C4CF7E896138245A109CBFA0E153880BC3492B`；比较结果为
  `equal-owner-trace / 15 records / 135 fields / firstDifference empty`。
- route1～4新鲜回归分别`7/7、15/15、3/3、7/7`，合计`32/32`。
- runtime/editor进程外build分别`0 errors / 22 warnings`与`0 errors / 104 warnings`；Unity Editor程序集
  01:05:08刷新后无CS error。
- 01:06:52在真实`NTSD_Battle` Play环境执行同一专项trace通过；前后Scene SHA均为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`，`dirty=false`、root13、
  Play退出且目标run Console 0 error。
- full SelfCheck于00:55:48实际执行，但仍在本包检查前被既有CPoint raw throw mode0 Vz断言阻塞；该失败不被
  隐藏，也不影响专项Play/joint owner trace已经闭合的结论。
- 本包未修改战斗生产runtime、content或Scene。B0 owner producer出口已满足，B2 runtime验收恢复；这不代表
  full battle parity、B8 physical F8或整个B0历史基线以外的规则已经完成。
