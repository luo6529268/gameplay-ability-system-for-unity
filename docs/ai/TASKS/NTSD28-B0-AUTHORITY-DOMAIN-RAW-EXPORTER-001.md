# Task Contract — NTSD28-B0-AUTHORITY-DOMAIN-RAW-EXPORTER-001

> 状态：`FOCUSED_TEST_PASS / CPP-BUILD-0-0 / REAL-DOMAIN-VALID / AUTHORITY-READ-ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

扩展 workspace-owned `authority_source_capture_main.cpp`，在保留47字段entity raw的同时，可选输出
`ntsd28-logan-b0-domain-raw-v1`：真实applied input、权威CRT/synchronized RNG状态与调用增量、
1000物理slot occupant/normalized allocationEpoch，以及相邻completed-tick快照推导的
birth/death/reuse。新增一个两侧共有OID2/7且含非空七动作输入的三tick场景，运行并由现有
fail-closed validator验证。

## 允许文件

- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`
- `Tools/NTSD28AuthorityTrace/README.md`
- `Tools/NTSD28AuthorityTrace/Scenarios/input-common-two-entity.json`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 权威目录仅作为include/source/resource输入；build、manifest、entity raw与domain raw全部留在仓库`Temp`。
- 证据继续标记`SOURCE_MODEL_DIAGNOSTIC_ONLY`与`certificateEligible:false`，不得冒充正式EXE runtime trace。
- 输入mask按合同bit顺序显式映射：right/left/up/down/attack/jump/defend；不得依赖native enum序号巧合。
- RNG初始total在首tick前采样，tickCallCount必须由相邻total相减；table hash使用source-native 64-bit值。
- per-call trace保持missing/null；不添加diagnostic RNG调用，不改变scenario执行或现有entity raw语义。
- initial/每tick occupant必须升序；epoch沿用已验证normalizer；delta必须由相邻snapshot推导。
- C++ `-Wall -Wextra -Wpedantic` build 0 warning/0 error；真实三tickdomain raw validator通过；
  非空输入mask与双RNG/slot字段逐项核对；旧entity capture validator仍通过且确定性重跑一致。
- .NET合同12/12、既有21/21、raw5/5不回归；Ledger与scoped diff通过。

## 回滚

撤销runner可选domain输出、场景和说明；保留前一合同包及既有entity raw exporter。

## 实际结果

- runner新增可选`--domain-output`；未提供时旧47字段capture仍正常。authority source manifest保持
  `C59BD8D3264B5CBF15EDBCFE2BAE64BC0F3BBC41926BEF6A4723EC2F571F2D75`；runner SHA
  `05E352F58389856E8C5227EA53DBE634A6B529E3AAA6DC531E84870B5A9B09DC`；binary SHA
  `B928F2D813BB281D2EB04DAF1C1A195574ECCB1351915A7CD7F5C98C08908657`。
- MinGW C++17 `-Wall -Wextra -Wpedantic` build 0 warning/0 error；正式EXE SHA重验通过。
- 非空场景SHA `CF3D4D9F55CE36EE0559B390D4082284671EA3F3B7F17BA576168912EE458542`；
  三tick applied mask为`17/2`、`1/96`、`0/12`，对应D+J/A、D/K+L、空/W+S。
- initial RNG totals为CRT3000/synchronized1；三tick delta均0；tableHash64
  `A1BA1B90EA55796D`。initial occupants为slot0 OID2 epoch1、slot1 OID7 epoch1；三tick稳定，
  delta均空，未误报初始化birth。
- entity raw 3 ticks/6 entities有效，SHA
  `609D391EAE6FBA9D68F2140047D6F6529A230D4EB0F3F50BFA536B5343356C32`；domain raw有效，SHA
  `A3C3372FA600B3EACEDBBA61B2C4BA1930320B694C0534B6962AE69FE84C39A6`；两次重跑均逐字节一致。
- 旧neutral capture在不传domain输出时仍valid 3 ticks/6 entities。
- .NET build0/0、domain12/12、existing21/21、raw5/5、format通过；Ledger89/16与diff check通过。
- Unity代码/compile/test未涉及；正式EXE trace、Unity domain exporter和双端compare仍待后续包。
