# Q05-A2 几何内容与完整复制合同

状态FOCUSED_TEST_PASS / SOURCE_AND_ALGORITHM_INTEGRATION_PENDING。父Q05-A2同一协调窗口。CPoint27和OPoint24均已落盘并各自focused/SelfCheck通过，两个Change仍SOURCE_INTEGRATION_PENDING，保留活跃回访；不是正式内容或新版本已发布。

复用Q03几何14x3见证、JOINT-FIELD-MATRIX和OPOINT-AND-HELD-DEPTH合同。authority为playable CollisionGeometry28.decode及真实caller，不从旧经验猜默认。当前核实：decode严格读取x/y/w/h的optional int；缺失或非法不是显式零。kind=100100且几何不全时control_only=true；其余几何不全返回无box。完整0宽/高、负尺寸仍保留原值；不以w>0替代是否存在。zwidth optional/raw0及ITR z和运行时回退15分开；BDY本地z不应盲新增位移语义。

当前Unity BattleBodyBoxValue只有X/Y/W/H；Adapter只复制4项并清raw字典。BodyBox DTO尚无zwidth/geometry有效性。InteractionArea有zwidth默认15和dz等独立字段，但缺原版ITR z及几何有效性；CopyFrom逐字段复制，HitExecutionPlan有独立snapshot/copy/hash/reset，应沿实际引用检查，不能只增加模型字段。原始Raw AST保留；新native decoder必须保留严格数值成功/失败，不能把非法0与显式0合并。

修改前收敛精确code-path/符号/调用者并建Change Record。范围候选为BodyBox value/adapter、LF2FrameData BodyBox/InteractionArea/CopyFrom、LoganCombatRecordDecoder、实际已有ITR clone/snapshot/复用入口及相关测试；不是整目录修改许可。不要仅凭命名把dz/dvz当成z，或把legacy DTO无raw的手工fixture判为DAT缺字段。显式构造、default值、legacy合成数据和native缺失几何的身份/default策略必须写明并以见证验证。

在本Q05子项中完成内容值与所有必要复制/复用/identity载体，三collector的精确深度与候选算法修复归Q06；不能顺手改宽相位/时序或为通过测试过滤掉不支持的body/itr。native kind100100控制逻辑仍可消费record，不得丢整个记录或frame。weapon_strength19+index后继再接同一几何字段来源，完整source conversion最后一次接线。

验证：Q03固定几何输入逐项native解码信息对照；缺失/非法/显式零/负尺寸/非零深度/控制kind、正反方向原始值、alias/copy/cache/pool复用和相关测试。旧fallback与optimized当前几何算法未修时如实记录其旧首差，不能将数据通过写成候选parity通过。完成后完整SelfCheck/编译/Scene状态及Ledger；生产源、语义身份和Play仍同Q05出口回访。

保持Unity/GAS与非战斗/Gen/Plugins/外部包/Scene/Input Actions/33ms/十一阶段；不部署资源，不另开schema窗口。entity12→13/aggregate20→21/checksum23→24/character1→2/base1→2、source identity、OPoint快照队列guard等仍父Q05待办。旧phase断言留Q12、landing除法一ULP留Q06、默认stage.dat暂缓保持。


准确九脚本Record已建，默认策略/复制投影/数据与算法边界以Record事前合同为准，先native witness及RED。


本轮375/native88/SelfCheck通过，报告同ID；保持来源/identity/Play及Q06算法回访。下一ITR40/strength19内容Task。
