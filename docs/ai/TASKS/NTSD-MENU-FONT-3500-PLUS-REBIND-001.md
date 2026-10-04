# NTSD-MENU-FONT-3500-PLUS-REBIND-001

用户需求（2026-10-04）：新建 `JifengBladeArtSC-3500-Plus SDF.asset`，修改 `MenuFont3500MigrationEditor`，把菜单字体迁移目标改为此资产。

范围：只处理 NTSD_Menu 的 3500 → 3500-Plus 字体绑定、其场景内图集材质、Plus 字形图集和菜单视觉测试的字体定位。保持原场景当前修改、旧 3500 资产、其它场景及战斗逻辑不变。迁移前按当前磁盘文件做 SHA-256 快照，编辑器须在非 Play 且目标场景干净时执行。当前原 Editor 打开 Battle，不能在 Battle 未保存修改时切场景。

风险：Plus 资产当前仅含部分字形；直接改引用会缺字。需预检菜单字形在新 TTF、图集和既有回退字体中的覆盖，并保留场景自定义材质效果。Unity 生成的图集可能触发嵌入 TMP fork 的重复字形问题，先预检、再原子式尽量少量保存。若 Editor 不满足门槛，只交付脚本和静态验证，不宣称 Scene 已迁移。

验收：脚本只指向当前 3500 输入和 Plus 输出，菜单命令可调用；生成 Editor 项目编译零错误。执行时 Menu 场景旧 3500 GUID 为零，新 Plus GUID 覆盖标签与输入，菜单显示和循环选择用例按现有测试验证。未执行的运行时步骤如实记录。

回滚：按 `artifacts/diagnostics/NTSD-MENU-FONT-3500-PLUS-REBIND-001/prechange-manifest.json` 中逐文件备份和 SHA 恢复，只在检查恢复目标未被他人继续修改后进行；治理文档使用追加更正，不做整文件覆盖。
