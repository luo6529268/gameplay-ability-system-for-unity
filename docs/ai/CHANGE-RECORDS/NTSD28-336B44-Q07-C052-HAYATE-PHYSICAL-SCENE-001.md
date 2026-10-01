<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001
status: VERIFIED
change-kind: EDITOR_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052HayatePhysicalBattlePlayProbeEditor.cs
authority: 336B44 playable and formal root standing Hayate physical-input LFR chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001.md
-->

# 疾风站立物理输入原场景验证

脚本前登记。现有Unity只验受控初始action160后的OPoint自然链；正式源／根已证真实离散键从站立进入160，Unity同键流是未关闭首差门。预计只新增独立请求式Editor探针与meta，复用既有Play副本roster、生产Driver、空间投影和有序退出。不修改战斗生产或数据。

实际仅新增独立Editor探针与唯一meta：Play副本三人站立action0，生产Driver于相对tick1～4送Jump、8～10送Defend+Right+Attack，采36tick动作／源位置／MP／目标HP与417/211出生及提交键掩码，退出检四SHA。请求及结果拒绝覆盖。当前`CODE_WRITTEN`；原Editor导入编译、Play、逐字段比较未验。未修改生产、DAT/图片、Scene/Prefab/配置或非战斗。

初态、键流、逐tick字段、原Editor编译、四SHA、失败保留及回滚见Task。关键风险为Unity本地输入采样相位、组合键边沿或实体出生时序与正式源不同；出现首差时先核相同初态和输入相位，再决定是否需要生产修复。脚本写入后立即登记实际文件和验证状态。

2026-10-01 v1 原场景诊断实际完成36tick、退出Play且四SHA不变，但与正式源684个选定字段中有308处差，首差tick2为源action210／Unity action65。保留[v1原始结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/hayate-physical-scene-v1.json)与[逐字段比较](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/comparison-physical-scene-v1.json)。静态调用链证实探针直接提交的`SimulationInputButtons`被`SimulationFrameInputModule`写入旧`FuncKeyMask`，随后`NTSD28InputTwoPassModule.FreezeProducerState`按旧字段投到native槽；正式的物理Jump实际应送Unity缓冲Defend，物理Defend应送Attack，物理Attack应送Jump。正常本地键盘入口`CharacterInputModule.CaptureHeldSimulationButtons`已做该交叉映射。v1属于诊断夹具输入域错误，不能当生产缺陷。下一仅在本探针改为同物理键映射，使用唯一v2请求／结果／Session key，保留v1；不改生产。

2026-10-01 v2 同场景36tick/684字段仅剩29处`actor_mp`差，其余655字段逐tick一致；tick8正式300／Unity导出500，tick31正式420／Unity导出500。当前导出字段是`Runtime.MP`，而`BattleCharacterActionWriter.TryWriteNativeInputAction`从`character.Health.PP`扣成本，`LF2Health.PP`绑定`Runtime.PP`。v2的资源字段比较尚未同域；下一只扩本探针v3同时导出`Runtime.MP`、`Runtime.PP`与`InputMpConsumedTotal350`，以正式`current_mp`和Unity实际资源值对照。继续保留v1/v2原件，使用独立v3请求／结果／Session key，不动生产。

2026-10-01 最终限定验收：原Editor已导入v3，原Battle Scene生产Driver36tick，正式源19项/每tick共684/684零差；tick8 Unity`Runtime.PP`300、`InputMpConsumedTotal350`200，tick31 PP420。正常退出Play，Scene clean，四保护SHA前后相同；原始[v3报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/REPORT.md)和[独立比较](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/comparison-physical-scene-v3.json)已存。实际文件仅Editor探针及唯一meta，无生产/DAT/Scene/非战斗改动。回滚需先按删除审计和授权处理新增脚本/meta；当前保留作Q07可复跑证据。父C052逐hit内部字段、完整World、真实键盘/画面仍未关闭。
