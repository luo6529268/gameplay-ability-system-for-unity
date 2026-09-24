# Q07 Inspector 正式资源刷新入口限定验收

`NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001 / VERIFIED_SCOPED_INSPECTOR_ENTRY`。当前 `GameConfig.BattleContentRuntimeRoot` 选中正式 Logan 根时，`CharacterAnimtorManager` Inspector 的“刷新所有数据”复用生产 `PrewarmConfiguredLoganContentAsync`；根为空时保留原显式旧内容流程。原先无条件读旧 `data.txt`/BMP 的按钮路径不再覆盖项目已选的正式资源。

在原项目已运行 Unity Editor 中，新增 actual-button 用例先按原代码运行，job `6e0333854715472dab260ba556765284` 0/1 FAIL：调用 `RefreshAllData` 后正式 `PublishedVisualContentKey` 仍为 null。生产分支修改后 Editor refresh/compile 返回 idle，job `5b37372a4480401aa9a4fb0ccf27d8d7` 2/2 PASS（Inspector 按钮实际调用、相邻 `BattleTestBootstrap` 直接调用）。按钮用例选一对象正式 fixture，等待发布并检查源键、OID56 配置名和 GameDataManager 对象登记。旧显式数据初始化 job `f9fa73b6383a4f51be1cf102d4bc91be` 1/1 PASS。

保存的 `NTSD_Battle.unity` SHA-256 为 `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`，`NTSD_Menu.unity` 为 `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`，与修改前一致。未启动第二 Editor，未用 computer-use。该测试不声称 330 对象正式整场、图片逐像素、物理按键或旧 383 张图片可删除；它只证明 Inspector 当前正式根分支调用了正确的已验证发布机制。手动空根、旧 DAT 声明及历史测试仍是 Q07 退场依赖。
