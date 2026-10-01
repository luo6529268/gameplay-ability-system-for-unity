<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-HAYATE-OPPOINT-REACH-001
status: VERIFIED
change-kind: SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hayate_fir_natural_reachability_probe.cpp
authority: selected 336B44 playable GameSession step and formal OID73 to OID417 to OID211 DAT chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-HAYATE-OPPOINT-REACH-001.md
-->

# C052 疾风到火焰子体自然链诊断

脚本前登记。正式DAT静态引用：OID73/action162生成OID417/action49，OID417后续49→50→51→52→10→11，action11生成OID211/action160；用户目标要求按当前336B44正式可达战斗入口，而不是用受控初始OID211代替自然出生。当前尚无此链的完整GameSession、根EXE或原Unity实测。预计只新增上述C++诊断脚本，不触及权威C++、Unity生产/测试、DAT、角色图、Scene或非战斗。

该脚本的职责、输入范围、输出、负例保留、验收与回滚见Task。风险为parent projectile运动/碰撞/寿命使静态next链在实际tick中断，或者出生OID211位置达不到双目标；阴性不自动说明规则错误。优先保留出生时点和坐标，再决定是否运行根/Unity。输出使用新目录、拒绝既有路径，不删除任何文件。首次编译与双跑结果必须在本Record中补记；仅在根/Unity取得独立证据后更新父C052。

2026-10-01 首轮脚本/编译：新增唯一C++诊断，使用正式28Core+playable当前编译参数链接，g++ exit0、stderr0、候选EXE SHA 0b5683ae7e5653882b32649b27e842ead4f3a17e27d9b7bc19f5c8b2a59592a5。初态OID73/action162、目标输入X1700/1730、120tick双跑均exit0，source-ticks/hits/LFR各自v1/v2 SHA同；实际目标被正式边界钳至X1330，OID417和OID211全程0。此为限定阴性，不能称动作162路径不可达。下一在同脚本增加正式action160前驱选项、改用边界内目标位置，先测自然进入162时OPoint出生；不改DAT/Unity。

2026-10-01 最终限定结果：诊断增加action160正式前驱选项，当前28Core+playable链接编译exit0/stderr0，EXE SHA `9C5A0A35889FF50709AC7126ACA736B98AF8D216D34E5A6EB7A1C59611C51031`。远距X1200/1230于tick4生成OID417、tick20生成OID211而无命中；近距X589/619于tick4生成OID417、tick22生成OID211，tick24对子体两个目标按applied/rejected-rest-no-terminate/applied顺序命中，两目标HP420。各案两次运行的tick/hit/LFR文件逐SHA相同。根正式EXE SHA本轮复核为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，同近距LFR运行exit0、报告passed/failureCode0；首次未覆盖目标1朝向，1140选定字段中193差，保留失败原件。修正目标1朝向后选定1140/1140字段零差。根LFR未覆盖目标2初始朝向，因此不主张完整初态/全World同态；原Battle Scene及物理按键进入action160未验。只新增C++诊断，没有Unity生产/DAT/图片/Scene/配置脚本变化。正式证据及下一出口见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-OPPOINT-REACH-001/REPORT.md)。
