# WPOINT 比例测试 RED（2026-10-02）

现有具名 `NTSD.Test.Editor.NTSD28Q07HeldWeaponDualDomainEditorTests.NaturalPickupMovingHeldWeapon_KeepsRuleAndScaledViewDomains` 已切换到当前336B44源码重核CSV，且仅把持有物理间距断言改为源整数间距乘世界视口比例。生成Editor工程编译退出0、0 error；原Editor脚本刷新完成，EditMode job `3c4b587494774c8ba039dd5db90613ac` 执行目标1例、结果 `failed`。首差为 tick2 的 `scaled WPoint X offset`：期望 `19.972993248312079 ± 1`，实际 `12.463615903976006`。与此前只读审计同向，生产两个写者尚未改；原测试后缀未被覆盖，新CSV独立保留。此为目标RED，不是当前正式源规则字段失败。
