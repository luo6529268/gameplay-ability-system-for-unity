# 源矩阵设计（构建前）

计划510行：resources96、activation60、defense36、motion108、action72、gain36、rest54、effects24、death24。type1..6覆盖；源current-state、Y>reference侧独立于snapshot。motion包含奇数dvx±5，death含fall80/vx0/dvx0及两朝向、普通/2000分支，effects含22/23及target两侧；不沿用Unity旧helper的整数除法、±3或effect定位。

输出index/group/params（完整每槽实际dat[3]及全部标量参数）/before/route/status/message/after/audio/crt/native/afterReleasedHoldFinalize。状态块三槽raw/extra、3×3rest、sparks和完整RNG scalar；额外加入display四步长和已确认poison/join哨兵，type6资源跳过必须保留它们。yResolved仍source-only。

freeze前attacker action91，再snapshot/rebuild；freeze后按before设置真实current/latch/previous/snapshot和资源关系，不rebuild。入口是真实ordinary/type1 wrapper，route输出armor decision/activation/selected/defense，不能只从最终armorHP猜。reduced full raw/extra/rest由独立source规则预测；bypass/broken的完整after仍输出，validator明确分支验证边界，原非角色unarmored/source见证不修改。

预计未涵盖任意armor list多record、type2/3/4 armor选择、所有effect/资源字段组合、正式EXE输入、Unity/Play。负parent有效/无效、复制不取负均覆盖；type6不调用resource/status，仍走durability/reaction/rest/spark。当前仅设计，最终实测计数以REPORT和validation.json为准。
