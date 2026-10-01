<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001
status: VERIFIED
change-kind: SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hayate_physical_input_reach_probe.cpp
authority: 336B44 playable input_routing GameSession step and formal Hayate DAT hit_Fa action160
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001.md
-->

# C052 疾风物理键入口诊断

脚本修改前登记。Unity原状只证受控初始action160后两层OPoint链；正式DAT/action212等`hit_Fa:160`和playable水平组合路由是静态前驱，尚无初始站立按键到达证据。预计只新增上述C++诊断、只读正式资源并导出不覆盖的文件，不改生产或数据。

实际只新增上述C++脚本：mode0从站立action0于tick1按jump、首次读到action212时按defend+right+attack；mode1从受控action213于tick1按三键作路由正控制。每tick记录输入掩码、动作/源位置/MP、目标状态、417/211出生与逐hit，导出LFR；输出目录拒绝覆盖。当前`CODE_WRITTEN`，编译和正式运行未验。无正式源码／DAT／Unity生产或非战斗修改。

具体初态、输入范围、输出、有限阴性、正式根门槛和回滚见Task。风险为跳跃帧、按键边沿与三键同tick顺序不满足组合窗口；阴性只排除实测输入。脚本写入后登记实际字段和运行结果；状态仅随证据提升。

2026-10-01 v1：当前正式28Core+playable编译exit0/stderr0。mode0站立tick1只按jump、mode1受控dash213 tick1只按defend+right+attack，各60tick双跑，同模式tick/hit/LFR逐SHA相同，但均未选action160或出生417；mode0连跳跃也未触发。审查正式`simulation_tick_driver.cpp`确认本地输入只在交替的`input_update_phase_4a0b90==0`采新pending，单tick脉冲可能被跳过，v1是夹具采样不足的有界阴性，不归因Unity或正式规则。脚本v2只延长键保持3～4tick并导出phase/sampled mask，仍在原合同范围；v1原件保留不覆盖。

2026-10-01 v2限定结果：当前正式28Core+playable编译exit0/stderr0，诊断EXE SHA AC21D915BE993FAFAA5A498BBB17E091B1B91E246646495D3F555770EF581595。站立action0于tick2采到Jump进入210，tick7到212，tick8采Defend+Right+Attack进入160，MP500→300；tick11出417、tick29出211、tick31双目标HP420。受控dash正控制tick2进160、tick5/23出生子体、tick25双目标命中。两种模式各双跑，tick/hit/LFR逐SHA同。根正式336B44同站立LFR独立等待进程exit0、报告passed/failureCode0，60tick×19选定字段1140/1140零差。首次PowerShell直接调用GUI子系统根EXE的v1/v2无结果文件，保留原件、不计证据。根无法覆盖slot2初始朝向；原Unity Scene相同物理键尚未验。本Record的`VERIFIED`只指源码/根输入可达性诊断。[详细报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001/REPORT.md)。
