# Task Contract — NTSD28-B0-INPUT-RNG-SLOT-RAW-CONTRACT-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / DOMAIN-SELFTEST-12-OF-12 / PRODUCTION-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

建立 B0 独立 raw-domain 合同与 fail-closed validator，冻结 completed-tick 边界上的：

- 七个逻辑 held action 的逐玩家/控制 slot 输入；
- 权威 CRT、权威 synchronized 与 Unity deterministic 三个 source-native RNG stream 的可用性、状态和逐 tick 调用增量；
- 物理/运行时 slot occupant、allocation epoch；
- 由相邻 completed-tick slot snapshot 推导的 birth/death/reuse delta。

本包只定义和验证证据结构，不把 Unity 单 RNG 冒充权威任一 stream，也不为当前未暴露的
per-call trace 或 lifecycle event bus 伪造数据。下一包再接 workspace-owned authority exporter，
其后再接 Unity diagnostic exporter 与非空输入场景。

## 允许文件

- `Tools/NTSD28Parity/B0DomainRawContract.cs`
- `Tools/NTSD28Parity/B0DomainRawContractSelfTest.cs`
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/README.md`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 权威目录全程只读，不生成 build/output；不修改 Unity production/runtime/test 脚本。
- input 只记录应用到该 tick 的 held mask，合法位固定为7位；不上传按键绑定、history、cooldown或AI派生状态。
- RNG 使用三个 source-native stream；availability/missing 必须符合 producer 矩阵，null 不得伪装为0。
- per-call list 当前明确为 unavailable；仍记录 state、totalCalls、tickCallCount，并校验 total delta。
- slot capacity允许两侧不同；occupant必须严格升序、epoch正数；delta必须由相邻snapshot唯一推导。
- validator拒绝截断、额外tick、伪造availability、非法input mask、RNG delta漂移、slot乱序和伪造lifecycle delta。
- `dotnet build` 0 warning/0 error，专用 self-test 全通过，既有 self-test/raw self-test不回归，format通过。
- Change Ledger validator与scoped diff check通过。

## 回滚

删除本包新增的独立合同/自检与CLI入口，并撤销本包文档登记；不得回退先前B0实体字段成果。

## 实际结果

- 新增 `ntsd28-logan-b0-domain-raw-v1` descriptor/validator/CLI；contract SHA-256
  `7185B5D2EFFA6F51D672BADE14FF49D2105A7B4BB7E4DC3D99D83C392DCC68AD`。
- authority matrix固定为CRT/synchronized available、Unity deterministic missing；Unity matrix反之；
  三个per-call list当前都missing/null，跨stream等价未声明。
- 专用正反例12/12：两producer有效，并拒绝截断、额外tick、伪造availability、非法七动作mask、
  RNG delta、slot乱序、伪造lifecycle delta、同epoch换occupant及available-null。
- 首次专用运行暴露测试夹具直接交换已有parent的`JsonNode`异常；改用DeepClone构造恶意样本后通过。
- Release build 0 warning/0 error；既有self-test 21/21、raw self-test 5/5；format verify通过。
- Change Ledger validator 88 records / 16 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
- 未运行Unity compile/test，因为本包没有Unity程序集代码；未接双端exporter，B0阶段仍在进行。
