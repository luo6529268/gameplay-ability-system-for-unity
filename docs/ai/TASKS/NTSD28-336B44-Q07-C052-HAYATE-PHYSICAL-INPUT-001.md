# Q07/C052 疾风物理输入进入 action160 的正式可达性

状态：`VERIFIED_SCOPED_SOURCE_ROOT_PHYSICAL_INPUT / UNITY_SCENE_PENDING`。父C052/Q07/总目标开放。正式OID73从站立action0经Jump和Defend+Right+Attack进入action160，并生成OID417/211及双目标命中；根正式336B44同录包选定1140/1140字段零差。[限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-INPUT-001/REPORT.md)。以下保留脚本前实施合同。

唯一脚本范围：新增 `Tools/NTSD28Q07Diagnostics/hayate_physical_input_reach_probe.cpp`。只用当前正式28Core+playable及正式runtime，初始OID73/action0/X500/Z400、两名OID2/action0/X589/619/Z400、mode0/difficulty0/seed682973786；按离散jump与defend+right+attack真实输入，在有界tick内记录输入、动作、子体出生、逐hit、LFR。允许一个受控初始dash帧作路由正控制，但它不能代替站立入场。每个候选输入最多60tick，阳性／阴性均保留，输出拒绝覆盖。正式源阳性后才用同LFR送336B44根；只有根可观察链成立再另包推进原Battle Scene物理输入。

不改正式源码、DAT、角色图、Unity生产／场景／测试、非战斗；不新增特殊战斗逻辑。验收为诊断编译0错、每个样本独立双跑同SHA、动作160及后续出生的真实tick、正式根原始回放报告和选定字段首差。若站立初态无阳性，只给限定阴性并按当前总表转其它可达首差，不无限重试。回滚或删除新增脚本／产物须先逐项审计并取得适用授权。
