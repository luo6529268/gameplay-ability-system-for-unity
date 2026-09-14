# 非角色所选护甲反馈路径

READY_AFTER_HIT_SPARK_TRANSACTION。BDEFEND字段原256中96例target types1..6、首armor type0，原保留raw/Bdefend，Unity误走普通伤害。当前完整失败在BDEFEND-FIELD-FAMILY-UNITY-001/after-field-fix。

原battle_world.cpp约6464：selected_armor存在且target.type!=0，调用append_confirmed_native_spark，然后applied return；该点在special_link_rest安装和护甲选择之后、普通伤害/rest检查之前。必须保留完整前置顺序，不能简单在所有入口最前面无条件return。

依赖HIT-SPARK-TRANSACTION-AUDIT-001的公共正确spark事务（CRT、snapshot、owner、capacity）。准确Record后接两factory实际weapon/object/other路径和对应Shadow反馈预测；不扣HP/改team/frame/weaponHP/Bdefend，不绕过原先特殊关系rest前置。复用96原向量并补必要关系/选择顺序，原失败不可豁免。非角色type1 selection是否同路须按源实际调用追踪，不从type0推断所有armor。

完成后回BDEFEND256，再处理UNARMORED-WEAPON-REACTION-001。禁止资源/Scene/非战斗/框架/Server改动和computer-use。
