# NTSD-MENU-CHARACTER-DATA-ITEMS-001

需求：用户2026-10-06要求先理解 CharacterSelectionController / CharacterSelectionBoard，再按 data.txt 生成角色 Item，默认随机角色；仅参考 NTSD-Logan 的列表生成，其他逻辑沿用本项目。

范围：现有 UI 三脚本 CharacterSelectionController、CharacterSelectionBoard、SelectRoleItem 及一份聚焦测试。读取当前 GameDataManager 已发布的 data.txt/catalog 注册顺序和 CharacterAnimtorManager 的 DAT 元数据/资源，不增加载器，不改 Scene/Prefab/配置、输入、队伍/倒计时/战斗启动或原生随机解析规则。

参考：Logan 当前源码 game_session.cpp::visible_object_ids（type0、hidden 条件、registry_index 顺序）与 selection_flow.cpp::set_character_catalog / reset 的 Random sentinel。只作为用户指定的 UI 生成参考，不晋升当前 UI 候选为336B44战斗权威。当前两个解锁门未由本项目菜单接入，默认显示 hidden=0；不移植解锁系统。

实施：首项 -1 随机，后续 type0 且默认可见、有已加载定义的对象，缺头像不剔除；列表不按ID排序。预热完成/加入/重新打开刷新；复用原模板和池，随机小图沿用模板（缺省使用GameConfig），真实小图取 SmallSprite。保持已有选择与队伍点击处理。

风险及回滚：晚加载可能曾使槽位缓存仅随机；刷新保留仍有效的选中ID及状态，仅失效ID退回随机。对象池重开应复用而非持续扩张。脚本前10文件SHA备份在 artifacts/diagnostics/NTSD-MENU-CHARACTER-DATA-ITEMS-001/prechange-manifest.json；回滚前检查后续用户编辑，文档追加更正，禁止用HEAD覆盖已有dirty工作。

验证：生成C#编译、聚焦规则（类型/隐藏/注册顺序/无图不剔除/随机）及原Menu Play生成/点击/重开池复用、Scene SHA不变；Tools/Validate-ChangeLedger.ps1。编辑器若忙或Scene脏不抢占，未验层级明确记录。


最终状态：VERIFIED（限定角色Item生成）。见对应Change Record与 artifacts/diagnostics/NTSD-MENU-CHARACTER-DATA-ITEMS-001/REPORT.md，首轮前置失败和桥回调残留均保留，三项最终直接通过记录与实际Play截图已落盘。
