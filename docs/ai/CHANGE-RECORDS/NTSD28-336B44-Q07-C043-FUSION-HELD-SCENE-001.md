<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C043FusionHeldBattlePlayProbeEditor.cs
authority: selected 336B44 playable fusion and held-refill natural GameSession/LFR trace
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001.md
-->

# C043 自然合体持有链原 Battle Scene 验证

脚本前记录：正式源码/根程序已证明 Chi OID8 frame256→257 自然生成并持有 OID420，Lee7/Chi8 双向输入在 tick28 融合为51，第二轮 held-refill 清子体关系而保留父槽。Unity 原生产共用失效尾已修且受控测试通过，但没有同一自然链的原 Battle Scene 完整 Driver 对照。

仅新增独立 Editor 探针及 meta，不改正式业务逻辑。入口、符号、种子、正式内容、输入、Scene 保护、限定验收与回滚见 Task。预期副作用是 Play 副本受控配置与独立诊断文件；不得修改序列化 Scene、DAT、用户资源或已有失败证据。原本脏工作区和所有历史文件不回退/清理。本 ID 状态在完成编译、Play与逐 tick比较前保持 `IN_PROGRESS`。

首轮 preflight 失败：新探针保护清单误写 `Assets/NTSD/Resources/GameConfig.asset`，实际资产在 `Assets/NTSD/Config/GameConfig/GameConfig.asset`。请求 `late-double-tap-scene-01` 被消费，但 `HashProtected` 在报告创建前抛 `FileNotFoundException`；原 Editor 未进入 Play，日志原件保留。仅修本探针路径并改为独立 v2 请求与新 runId `late-double-tap-scene-02`，不清理旧请求或覆盖结果。

第二轮真实Play：`late-double-tap-scene-02` 已正常进入原Battle Scene，前41生产tick共17字段697项仅前5tick未持有时空槽哨兵（正式0/Unity -1）不同；tick6 OID420/slot50关系-1/父1自然出生，tick28 Lee7→51、Chi休眠、关系-1→0且失败计数仅1，tick29无重复。第42tick在子体自然退场抛统一AI旧行代数异常；状态FAIL，退出Scene clean/四SHA稳但池借用5，不能以41tick局部同态称60tick/退出通过。独立共用修复Task `NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001` 已建立并取得精确RED→GREEN；本探针改为独立v3请求/结果后复验，旧结果原件保留。

第三轮限定验收：原Editor当前程序集晚于探针，生成Editor工程0错；原Battle Scene v3 60/60生产tick `NATURAL_GATE_PASS`，tick6持有、tick28融合清关系一次、tick29不重复、tick42子体正常退场；正式源码17字段1020项5个无链接哨兵raw差，严格仅该哨兵归一后1020/1020。退出Scene clean、四SHA稳、池借用0；驱动销毁后内部关闭阶段未观察（-1），不报完整十一阶段。所有旧请求和失败结果保留。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/REPORT.md)。本探针范围VERIFIED；C043父项、完整SelfCheck/全World/物理键待。

交付核验：Change Ledger validator exit0/PASSED，新增探针路径由本ID覆盖；`git diff --check` exit0，只有行尾警告。未删除文件、未启动第二Unity或使用computer-use。

2026-10-02 后继独立测试修正：原Editor当前336B44完整SelfCheck第八轮磁盘`PASS`，结果SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`；五组旧断言各有独立Task/Change/Ledger，详[逐轮报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/SELFCHECK-336B44-20261002.md)。本C043自然Scene探针原范围仍`VERIFIED`，C043父项的全World/物理键与Q07总出口未闭；上段SelfCheck待验为历史快照。
