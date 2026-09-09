# NTSD28-PP-RECOVERY-LOW-THRESHOLD-INCLUSIVE-BOUNDARY-001 — Task Contract

> Goal8 / 2026-09-09 / VERIFIED / PP150_BOUNDARY_ONLY；本合同脚本修改前建立，最终证据见同ID Record。

用户授权来源：Goal8明确放行本轮首个production微小边界修正；Goal7 PartB已确认两路>=150错误，无补偿，原gate测试PP=200未覆盖等号。
Authority battle_world.cpp:2311-2321当前已建模无stats/regen_mp默认0子集，threshold_required=true，gate!=-1时current_mp<=150具备helper资格。
只修低门槛等号，不能把helper资格与所有完整资源规则的最终增量等同；stats/mode/bonus/weak/chp/cmp完整语义继续后置。

## 精确范围与不变量
- Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
测试只新增边界方法/用例；复用现有InitializeCharacter与DerivedRecoveryCharacter，不改既有测试或helper。
新增149/150/151 × gate=-1/0 × Legacy exact-character、ECS exact-character、Derived compatibility，共18组；
固定HP400/HPBound400，PP初值分别149/150/151，正常tick3、无HitStun/stepWait/negative environment，OID9850，默认stats缺省子集，预期合格时按现有公式+2。
生产仅两个PpRecoverLowLimit比较各>=改>，cap500早退、其他门控、公式、OID51/52、phase、输入、生命周期均原样保留。
附属文件仅本Task/Record、Ledger、STATE、对齐总表、Temp。无场景/资源/生成器/第三方改动，不创建其他修复包。

## 验证与硬停止
先只新增测试跑一次RED，应仅gate0/PP150三路径失败，其他15组通过；实际结果完整保留。
随后两个比较符修正，边界全绿；B5名称组与NTSD28分类完整复跑，实际计数记录（新增用例可能使原759/229增长）。
full SelfCheck保持PASS；两套Assembly build0 error；validator通过；Scene指定SHA不变。
任何修正后既有测试失败、子集内发现额外Authority资格分歧、超范围diff/Scene不符立即停止，不修不刷绿。
指定instance gameplay-ability-system-for-unity@b1b02287，2022.3.62f3/NTSD_Battle，不第二实例，不Play。
PLAY_NOT_PERFORMED_BOUNDARY_ONLY：用户明确不强制Play，改动为纯数值门槛，18组真实Editor测试及两组回归/full SelfCheck验收，非表现/输入时序修改。
VERIFIED仅关闭等号边界，不声称完整C25恢复对齐。

## 风险与回滚
只使gate非-1、PP恰为150且其他现有条件满足的实体获得一次原公式恢复；151及其他资格不变。
无新runtime模块/所有者/queue，不增加shutdown步骤；只在既有恢复pass执行，无资源/持久数据或外部不可逆副作用。
回滚必须用户明确批准，只撤销两token及本次新增tests/治理增量，不覆盖任何既有工作。
完成后GOAL9_USER_HOLD。

最终结果：RED仅gate0/PP150三路径失败；GREEN18/18，B5原759+18=777/777，NTSD28原229+18=247/247，fresh full SelfCheck13:48:37Z PASS，两套build0 error，指定Scene不变。PLAY_NOT_PERFORMED_BOUNDARY_ONLY；本包仅关闭inclusive150边界。
