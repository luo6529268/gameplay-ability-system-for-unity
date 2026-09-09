# Task Contract — NTSD28-B2-FORMAL-EXE-HEADLESS-INPUT-OBSERVATION-001

> 状态：`VERIFIED / FORMAL-EXE-HUMAN-AND-AI-HEADLESS-PASS / INPUT-RESET-OBSERVED / PER-CALL-RNG-NOT-EXPOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 EXIT / FORMAL EXE`  
> 建立日期：2026-09-04

## 目标

直接运行根目录正式`NTSD2.8-Logan.exe`的内置无窗口 smoke，取得由正式二进制自身产生的输入采样、
动作应用、无输入对照和reset清理报告，补齐B2目前只有source-model capture的正式EXE观察边界。

## Authority 与边界

- 只接受SHA-256 `1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`的根目录正式EXE。
- 使用正式`--headless-smoke`与`--headless-smoke-report`入口，resource/complete-VFS均显式指向
  authority `resources/runtime`；报告只能写workspace `Temp/NTSD28FormalExe/`。
- 分别运行`--p2-human`与`--p2-ai`，固定OID2 vs OID7、background23；human报告要求双方七键、动作、
  位置、无输入对照与reset合同通过，AI报告要求正式native-AI配置完成同一smoke。
- headless smoke报告不暴露逐次RNG call-site或exact combo10/history；它只能补正式EXE可观察输入/AI行为，
  不能替代source-model joint trace或伪称per-call RNG certificate。

## 允许操作

- 读取authority EXE/source/README/resources身份；不得写authority。
- 创建workspace `Temp/NTSD28FormalExe/`并写入报告、stdout/stderr摘要及只读前后authority清单。
- 修改本Task、Change Record、Ledger、STATE、handoff与总表。
- 不修改C#/C++/tool source、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。

## 验收

- 运行前重新计算formal EXE SHA并拒绝不匹配；记录命令、退出码和报告SHA。
- human与AI两次正式EXE均exit0、report `passed:true`；human双方七键/动作/无输入分歧/reset clean通过。
- authority根目录与runtime/source前后文件数量、总长度和max LastWriteTimeUtc摘要不变；无新增输出。
- 明确剩余per-call formal RNG/exact-field不可观察项，不以smoke报告扩大结论。

## 回滚

删除workspace Temp报告即可；authority与Unity production零修改。

## 完成证据

- formal EXE SHA重新计算为`1277B70B...DAF75`，与authority合同一致。
- `--p2-human`：exit0、schema1.1、passed；900 ticks，P1/P2 sampled mask均127、all-seven true，
  applied actions 367/307，双方均与no-input control分歧；queued input discarded，双实体full/idle clean，
  reset后双实体attack均再次sampled。报告SHA `65D4FF97...359E0`。
- `--p2-ai`：exit0、schema1.1、passed；900 ticks，`p2NativeAi:true`，P1七键/动作/对照分歧通过，
  双实体reset full/idle clean及reset后attack sampled。报告SHA `2F845CBD...DABB0`。
- authority树前后均为2809 files / 110386049 bytes / inventory SHA
  `71275342...E3D87` / max LastWriteTimeUtc `2026-08-29T01:34:09.8160543Z`，完全不变。
- smoke报告不含exact input/history/combo10或逐次RNG call-site；这些仍由source-model joint trace证明，
  本包不把不可观察字段写成formal certificate。
