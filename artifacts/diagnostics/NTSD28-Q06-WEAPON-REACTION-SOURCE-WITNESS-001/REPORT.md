# 武器受击源见证

VERIFIED / SOURCE_MODEL_ONLY。正式playable调用链的诊断执行，不是正式EXE场景验收。

最终2100向量（432基础、1296水平、72攻击者post、192资源、36双方左向、72垂直），validation.json 43202断言PASS。两次执行均exit0，14,778,999 bytes逐字节相同，SHA256 28cad088cef6941c6b9c27cd4b2f0b5419cb8f47e106accd7078f9757e3eb55a。build目录Temp/NTSD28WeaponReactionFinal；入口weapon_reaction_witness.cpp复用已锁定正式playable构建闭包；不改正式发行文件。

普通kind0的effective dvx/fall保持原值，heavy不减半。武器types1/2/4/6保留reaction80；group/facing不转移，不走旧victim随机frame/self-rest尾部。低fall type2不增贡献数且跳过垂直和动作；其它目标按pendingX与朝向选择180/186。0.55为double，替换分支保留符号；dvy0加-7且不夹紧。当前state1002在rest后同步0xEE/16，raw动作写入保留counter/latch；state3000/指定3007走原post10/dvx。24个type3攻击者只产生自身broken资源音频，其余2076无音频；600次state1002各一次同步随机，其余0，所有命中CRT2。

输入明确冻结候选后设置诊断pose/impulse/counter/link。releasedHoldFinalize显式置hold0后调用原finalizer，不代表自然hold时序。覆盖raw47/3之外pending/count/rest/stats/audio/随机；三项未绑定raw字段仍MISSING。源fixture不涵盖所有armor/held/OID100/effect分支，需独立扩展，不从2100推出全武器已对齐。旧2028证据保留initial-2028/。
