# Batch80：SetPass 本局共享绑定修复
结论：**SCOPED_RENDER_RUNTIME_PASS / SETPASS_1995_TO_5；H07 PERFORMANCE_FAIL 仍OPEN。** 人类批准的cold脚本修复已实现，未解冻EXT1/ATLAS/Mono或触碰Q06排序body。
## 新鲜实测
唯一成功对照是windows-02（windows-01 ON冷期失败保留），本项目既有Unity2022.3.62f3、原PID74724/MCP6402；Combat1000/OID1 Sakura、实际AI最小1000/基础roster最小1000、120warm＋180sample，各180tick逻辑统计与89完成显示帧。请求仅outputPath不同；完整末态tick300 checksum JSON两份SHA同为E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12，字节一致，包含RNG/world/slots/stats/rest/events/inputs/hashes。
| 指标 | OFF 源纹理 | ON 本局共享绑定 |
|---|---:|---:|
| 全帧Profiler SetPass平均 | 1995 | 5 |
| 全帧SetPass最大 | 2038 | 5 |
| 中央segment平均 | 1990.494382 | 1 |
| 中央submission实际Draw计数平均 | 1991.494382 | 2 |
| tick128源/解析命令 | 2026/2026 | 2026/2026 |
| tick128物理chunk/segment | 1/2024 | 1/1 |
| 每tick逻辑平均ms | 90.416156 | 87.920167 |
| 每tick逻辑P95ms | 113.763385 | 112.167190 |
| 显示帧间隔平均ms（诊断窗） | 284.669371 | 242.618169 |
| 完成帧main thread平均ms | 202.820055 | 191.659449 |
| render thread平均ms | 0.580738 | 0.582761 |
| GPU平均ms（既有记录器） | 0.457399 | 0.740765 |
| 丢弃wall-clock债务tick | 923 | 725 |
SetPass下降99.749373%，是89完成显示帧直接ProfilerRecorder计数；不是从CPU DrawMesh推导GPU batch。中央1个segment不等于整帧只1条Draw：现有RenderPass统计本窗ON平均2条中央Draw，整帧另有背景/UI等SetPass，当前不拆分归因。单chunk、排序/segment/fail-closed规则没有改；只是匹配资源的实际绑定identity成为同一个array。
ON tick128：Texture2DArray/NTSD Battle Atlas Array，AtlasTextureArray，4页/64MiB计划atlas allocation，预算仍512MiB，variant1/BattleCentralTransparentArray；源fla.png3000×800仍明确Source2D fallback。64MiB不是全驻留/transitionpeak测量，完整原源纹理、logic/UI/catalog仍保留，冷解码/staging不算已优化掉。GPU均值略增，本窗下降不是GPU耗时收益承诺。
## 实施与保护
五准确C#：CharacterAnimtorManager冷Preparing门/完整catalog源重置/选中全帧OPoint与state变身递归候选/common/验证原SHA重解码/尺寸像素一致去重/统一atlas builder/旧atlas预算扣余量/无lease资源退休；App在服务与实体出生前use玩家调用；ProductionEntityStressHarness仅实际selectedOID冷调用；同文件18新测试；Suite新固定两窗/readback/开关try-finally/各public terminal替换前精确副本。
不裁逻辑/config/catalog、不新建Sprite/源Texture、不改源PNG/DAT/Scene/Prefab/meta/importer/ProjectSettings/纹理格式/预算/bank，不改publication命令序列、透明排序、segment合并、33ms/3ms/max2或11stage关闭；无GPU Instancing。未纳候选或无法确认冷像素的资源仍合法Source2D；完整可用coverage不等于所有隐式依赖/未来技能都已优化到同一绑定。Q06只hash。
## 测试及失败留痕
- RED1 job550687af8b5b44698414c1626db1bd2a：新12/12断言缺API。
- Suite RED job31470c3e8f9b49fbb33a453be52440d4：3/3断言缺请求helper。
- GREEN1 job231f3f8e266b4d638f72df5e20548328：40/41；新增测试把combat1000误写combat，更正测试不改冻结请求。
- GREEN2 job8e67792ee2f7498eb0b90fe1fbc414ed：42/42。
- windows-01：OFF有效/1995，ON0tick遇独立Texture共用pixel.png误拒绝；原owner正常关闭回Menu；整目录保留。
- 去重RED job1568e8169573440584fa3c719f543e5e：相同像素1FAIL/不同像素1PASS。
- 最终GREEN joba39a1c0032c847a886feec3f7cdf706c：44/44、0skip（18新＋26既有回归），green-03/tests.xml；CS0。
- windows-02：MEASUREMENTS_COMPLETED/2runs；两workloadValid，未解析/unsupported0，逐窗清理exceptions/objects/entities/slots/activeGO/pool/reference皆0，最终orderedShutdown/原Scene恢复/saved Battle字节保持。Menu sole clean8/idle。
公共XML每次替换前独立备份，实际SharedResult路径为Temp/NTSD_ProductionEntityStress.result；01初manifest误记.result.txt的ABSENT更正保留，并在任何真实终态替换前补精确旧77byte副本/manifest02；每窗自动复制旧终态到唯一shared-terminal-before.txt。
## 剩余方向与未运行项
H07 ON逻辑P95仍112.167ms>33ms，drop725，不能称整体优化完成或120FPS/Android通过。该窗CandidateCollect平均57.166ms/P9580.709，CharacterInput11.383ms/P9512.919，是下一已在范围内的逻辑瓶颈证据；不再将两千SetPass当未解决现状，也不因此引入Instancing。
本轮没有完整1800/re-entry H11 0GC资格，不消除旧43的12events失败；旧Foot/H06/M13/M14有限首阶段已过不重开。实际别的两个CPU容量/中央lease/stale总计此schema未采，不编造0；Driver/central相关代码保护未变、runtime slot1050和完整source/resolved命令实测。正常role-select→App MatchConfig路径、多角色/复杂技能/Android或非array能力、GPU capture真实batch与像素A/B未运行，不用这些未来验收制造本次SetPass已测子成果的新阻断。
## 治理状态
Change NTSD-OPT-H07-SETPASS-MATCH-BINDINGS-080：RUNTIME_PENDING（正常App/mixed/device/完整视觉验收未做），**本两窗SetPass子证据通过并已接入默认冷准备**；H07/H11仍OPEN。58执行（79仅准备未执行），六项4/6、34父关闭0，Goal active且无停止目标操作。当前用户提交a10c358保留，无add/commit/push/reset/restore/clean/stash/delete。
最终[audit-final-01](audit-final-01.json)已生成：611保护路径0变化、原备份核同、五源准确after SHA登记，HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50与staged空保持；Menu/Battle原SHA核同。Tools/Validate-ChangeLedger.ps1 exit0（1375Records/5源covered/0error/4350历史warnings），git diff --check exit0。该audit在最终恢复链接追加前捕获：其中治理文档after SHA是该时点快照，不等同其随后追加收尾文字后的SHA；五C#未再修改。Operation仅文件操作审计VERIFIED，本Change仍RUNTIME_PENDING/H07H11 OPEN/Goal active。

