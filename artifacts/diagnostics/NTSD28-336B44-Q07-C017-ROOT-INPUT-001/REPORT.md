# Q07/C017 正式根自然毒弹与零血输入见证

状态：`VERIFIED_SCOPED_ROOT_TRACE / UNITY_SCENE_PENDING`。正式336B44根EXE与当前对应playable源码在勘九郎自然毒弹链40 tick的8个声明字段相同（320/320），但这不是完整World parity；原Unity Battle Scene同链仍待。

受控初态：seed682973786，勘九郎OID14/action297/X500/Z650/HP与base500，鸣人OID2/action0/X610/Z650/HP与base35，二者MP500、不同team。之后不修改HP、动作或中毒字段。勘九郎正式DAT action297→298于tick6自然OPoint生成Pur OID222/action8。tick7正式根trace直接记录slot50→1 applied、damage30，鸣人HP35→5/action220。对应源码记录中毒timer330/type1/strength5；正式trace没有导出这些中毒字段，不将其当作根直接字段证据。tick19根HP0/action2，tick27回idle0；仅在这一自然零血站立条件之后发送held Attack，tick28根currentMask16、HP0、action65，input事件明确applied及native standing attack RNG0x82。

[自然链正式回放](poison-v2/root-report.json) exit0/passedtrue；[逐tick根trace](poison-v2/root-trace.jsonl)、[源码CSV](poison-v2/source-ticks.csv)、[320字段比较](poison-v2/comparison.json)保留。声明字段为双方action/HP/current Attack/source整数X，各40tick，0首差。内建nativeParityClaim=false。另有正血站立Attack控制12tick×8字段96/96、根exit0/PASS，见positive目录。

限制与失败保留：第一版源码HP0站立和HP0/state14初态各12tick正确，但正式LFR只承载base HP，播放用base初始化current HP；两例第一tick真实失败46，expected总HP500、actual1000。它们不能作为根同态或Unity差异。初始源码自然poison运行180tick曾在第164host结果冻结时被record拒绝，真实exit7/无完整LFR；poison目录保留其CSV，不能称回放PASS。revision2自然记录缩至40tick并令base35与current35一致，这才是有效根同态。不改正式载体、DAT或EXE以消除失败。

两个g++源码诊断版本均使用已声明playable闭包编译exit0（compile-argv/output及compiled-source-snapshot各版本留证）。工具只编码输入/观测，无Unity生产修复。[资源/保护哈希](artifact-hashes.json)证明根EXE仍336B44、三正式/staged DAT分别同SHA，当前工具源码等于compiled-v2快照，Battle/Menu Scene与两个config保持保护值。Original Editor零血机制已有聚焦/完整tick证据，单独不能替代当前自然场景验收。
