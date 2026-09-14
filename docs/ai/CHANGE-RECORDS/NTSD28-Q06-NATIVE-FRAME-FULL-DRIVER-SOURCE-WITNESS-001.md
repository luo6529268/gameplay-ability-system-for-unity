<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001
status: VERIFIED
change-kind: SOURCE_ONLY_FULL_FRAME_DRIVER_WITNESS
code-path: Tools/NTSD28AuthorityTrace/frame_full_driver_witness.cpp
authority: Current playable GameSession28.tick_options BattleConfig28 defaults and SimulationTickDriver28.step full pass order; formal EXE B1E13AE and source closure 07CD47A.
evidence: Parent2676 endpoint evidence plus verified state18/fragment full combinations leave high action and signed cost fallback consecutive tick coverage missing.
-->

# 帧事务完整driver源码见证

准确一个新诊断CPP。已读GameSession28.tick_options:4124+，正式BattleConfig28默认Normal/Practice drop4c=2、HP28=1、MP2c=1，当前empty drop candidate表；非Casual0。构造options时从这些真实默认字段赋值，local/F6由明确case指定，native_ai关闭、controls中性无输入、无stage/碰撞形状/融合数据的隔离单实体fixture（不宣称完整正式场景）。不添加任何simulation writer，所有tick输出来自原SimulationTickDriver28.step。

计划450 cases×3连续tick：14种正负next×type0/3/4×slot0/70×ground/air；高当前857/998显式/implicit×stay/advance；HP/MP不足和恰好支付×6种原destination.next fallback×三type/两slot；recmp/mode/double/waiver/local代表组合；正负hold的type资格。使用既有FrameCase意义，不重跑2676端点。完整初始raw50、逐tick raw50+sound latch/HP和MP真实消耗统计/声音源channel/path/x/Native RNG调用、frame diagnostics及lifecycle结果落JSONL。声明动作/初始latch/counter/位置与各持久字段全部在初始行可核对；中性输入，caller不改previous078或lifecycle结果。

验证：现有Build-AuthoritySourceCapture.ps1验证正式EXE与75源closure，在Unity仓库Temp输出候选binary；两次输出字节一致、450/1350覆盖与errors检查，异常如实分析。仅SOURCE_MODEL_DIAGNOSTIC_ONLY，不能据此声明Unity已通过或覆盖根目录EXE。文件hash/命令/退出码归同ID artifact。无Unity C#/Scene/Prefab/资源/框架/Schema/Server改动；不存在新增runtime模块或关闭阶段。回滚只本新诊断文件且按删除规则先批准，保留其他任务用户工作。

出口后立即回父NATIVE-FRAME-TRANSACTION-INTEGRATION的精确Unity full-driver/Play联合Task；source证据不能替代后者。Q06、正式资源迁移及总目标仍未完成。

## 实际SOURCE_MODEL限定出口

原450 cases/1350 tick成功，frame/lifecycle错误0；934 frame声音事件，Native与CRT调用均0。34条诊断仅type0上一tick HP已为0的输入抑制，逐条前置已核对（slot0/70各17）。两次最终输出与首版字节一致，SHA e50dbee6628622e08c6deb1697f9aa83887775725b4768f82ab654989db9fce5；首版三个misleading-indentation warning已修，最终build无warning，正式EXE/75-source身份保持。header有运行时正式default合同guard。build.log/build-manifest.json/native.jsonl/native-repeat.jsonl/validation.json留证。

VERIFIED / SOURCE_MODEL_ONLY；未改变或新验Unity生产，不是整个父frame完成。下一NATIVE-FRAME-FULL-DRIVER-UNITY-001准确测试脚本先同初始raw/三tick比较，必要真实Play。
