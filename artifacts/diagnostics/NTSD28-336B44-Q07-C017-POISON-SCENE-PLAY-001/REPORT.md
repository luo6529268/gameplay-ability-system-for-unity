# Q07/C017 原 Battle Scene 自然中毒归零后攻击

状态：`VERIFIED_SCOPED`。原项目Unity2022.3.62f3 Editor唯一请求poison-zero-hp-scene-01完成`PASS/DONE/exitedPlay=true/sceneCleanAfter=true`。只验证C017共享零血输入门在本条自然DAT接触链上的出口；Q07整组、玩家物理按键选招与整场验收仍开放。

原Editor17:50经Assets/Refresh编译新Editor探针成功（Assembly-CSharp-Editor/Tundra success4.54s，既有warnings），域重载完成后核实空闲Edit/原Battle Scene，再提交唯一请求。探针在Play副本Bootstrap Start前配置勘九郎OID14/鸣人OID2；真实初始化后稳定暂停全局tick5，受控初态分别action297/X500/HP500与action0/X610/HP35，Z650/MP500，phase0和native seed682973786，source/view位置通过既有共享投影建立。没有手动设置中毒、零血或攻击动作。

40完整生产Driver tick即全局6–45中，相对tick6正式OPoint生成OID222/action8，tick7命中后HP35→5、自然poison timer330/type1/strength5，tick19 HP0，tick27回idle0；只有此时才向既有输入缓存提交外部UI Attack（现有crossed Jump bit），tick28 sampledAttack1/action65且HP0。该自然链与冻结336B44正式EXE时点一致。不是从物理按键选择毒弹开始的整场证书。

[原始Play JSON](poison-zero-hp-scene-01.json)、[源/Unity比较](source-unity-comparison.json)：40tick双方action/HP/currentAttack/sourceX共320字段320/320一致；源/Unity追加poison timer/type/strength及Pur slot/action共200字段也无差异。根trace不打印poison三字段，不能把这200全写为根直接证明；正式根同LFR的8字段320/320及tick7 applied hit、tick28 applied input在[根报告](../NTSD28-336B44-Q07-C017-ROOT-INPUT-001/REPORT.md)。全World parity未声明。

进入Play/内容初始化期间一次MCP状态读取超时，不是第二轮运行或探针失败；最终唯一报告为PASS。退出后的MCP明确非Play、idle、非编译。报告Scene前后SHA都为3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED且sceneCleanAftertrue；Menu、GameConfig、ProjectBattleModeConfig保护SHA保持。新增脚本仅Editor诊断，不接入玩家runtime，不改DAT、Scene、Prefab、input资产、配置或非战斗逻辑。已有机制测试不机械重跑。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path PASS（1054 records/shared dirty diff14 governed codefiles）；git -c core.safecrlf=false diff --check exit0。[保护四SHA前后相等](protected-hashes-after.json)。本轮没有重跑全量Unity测试，验证为新增脚本原Editor编译、唯一自然链Play及声明逐tick对照。
