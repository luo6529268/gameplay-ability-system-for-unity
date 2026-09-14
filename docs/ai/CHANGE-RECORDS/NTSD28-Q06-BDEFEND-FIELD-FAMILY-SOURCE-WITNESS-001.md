<!-- CHANGE-RECORD
id: NTSD28-Q06-BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001
status: VERIFIED
change-kind: BDEFEND_FIELD_FAMILY_SOURCE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/bdefend_field_family_witness.cpp
authority: EntityState28.bdefend_accumulator is +0x0B8; current BattleWorld28 unarmored overwrite45, reduced armor accumulation and advance_reaction_timers_slot positive decrement with hold/link gates.
evidence: Unity full-driver raw Bdefend0/45 difference; current native C25 already owns Runtime.Bdefend, while damage writers and armor activation still read/write separate legacy HitStateCount.
-->

# Bdefend字段家族原证据

IN_PROGRESS / SOURCE_ONLY。仅新增诊断CPP；正式source/EXE、Unity生产、内容和框架不改。

以同源几何候选和实际普通命中入口测量：七类target、无armor/首type0、不同初值及itr.bdefend；另以type1 armor的既有原测试定义检测runtime armor HP条件下的累积。输出初值/结果状态、Bdefend/armorHP/action、原raw及一次接续timer恢复（持有与非持有区分）。不预设所有类型都会走相同分支，unsupported/early-return必须保留。

此为新命中→既有C25恢复的衔接证据，不重做已VERIFIED C25h所有timer矩阵。不修改DAT缺省值、不将Runtime.HitStateCount全局alias到Bdefend；最终Unity实际writer/reader/projection需独立准确Record。没有新persistent字段/schema/关闭模块。

验收：诊断编译运行成功、两次字节一致、权威EXE/75源身份保持、原不同分支结果可复用。回滚仅本CPP差量且按规则授权；禁止computer-use/非战斗/Scene/资源/Server修改。
已写单CPP：224无护甲/首type0跨七type向量+32 type1 armor向量，输出raw before/after、bdefend写入与held/free接续timer。尚待原编译与实际结果，不预设所有分支overwrite45。

## 最终源限定出口

VERIFIED / SOURCE_MODEL_ONLY。256输入/1280断言PASS，两遍一致SHA6ba2e26f9aa202453f9c2985adf1bfba9c15ad3d1f8f54713450948d9ae7f2cb，原EXE/75源身份保持。分支148覆写45/96非角色首type0反馈保留/6有符号累加/6armorHP保护，负值累加不夹0；接续既有timer门与正值-1吻合。原status全部applied/每例1candidate；完整JSON/编译manifest/validation/审计报告见同IDartifact。仅新CPP，无Unity生产改动，不把source通过写成Unity修复。下一BDEFEND-FIELD-FAMILY-UNITY-001，父完整driver失败保留。
