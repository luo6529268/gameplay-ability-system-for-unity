# Q07 Menu→Battle 正式内容根门槛：当前证据

## 2026-09-25 原 Editor 双配置 Play 结论（覆盖下方待验措辞）

同一原项目 Editor `gameplay-ability-system-for-unity@b1b02287` 编译了生产根门与诊断扩展。正常正式根唯一请求 `q07-formal-root-gate-20260925` **PASS**（JSON SHA-256 `8217489BDAF57162D88DAB7E93C18232E4E262A40F20576674DC0B37AD506CFB`）：真实 Menu 组件回调预热、VS/鸣人OID2/CMC/Fight确认，正式内容三 owner key 相同，BattleRunning/World2，卸载后 Stopped、池借用0、返回Menu。诊断指纹更新为当前项目 Mode Asset 的 `27CAE01489909C46A5145A5867988CCD8C10E20FEF165D847B7AC7A6DE2DE02D`，旧结果不覆盖。

空根唯一请求 `q07-empty-root-gate-20260925` **PASS**（JSON SHA-256 `68B2C2230CCF6E00B54B24009951520BB53F598D5FC56362CD00996F3155BD45`）：仅在 Play 内将 GameConfig 的根暂设为空；旧 Menu 预热完成、VS/鸣人OID2/CMC/Fight确认、Battle Scene 加法加载，但`BattleRunning=false`、World0、Runtime Stopped。Editor.log含预期`Formal Logan battle content root is required.`，随后卸载Battle返回Menu且池借用0。`finally`已恢复原根。Editor退出Play，回到保存的Battle Scene/idle；Battle/Menu Scene及GameConfig asset SHA仍为下方原值，说明测试未写盘改变这三项。

因此本 Change 的**战斗入口根门**为 `VERIFIED`：所测空根不会进入Battle，当前正式根原路径仍可运行；Menu旧预热与选择仍按这次场景回调观察。该结论不证明所有旧读取器已退场，也不授权删除旧DAT/图片，不关闭Q07的角色技能/画面/其它资源出口。下方“尚待Play”是运行前历史记录。

当前 Battle Scene 直接测试入口已拒绝空正式根。生产 Menu 的 `LoadingPrewarmController.PrewarmOnceCoreAsync` 仍保留空根 legacy Config DAT/图片预热（非战斗菜单及作者功能不变）；但该 legacy 完成后，`CharacterAnimtorManager.ValidateConfiguredContentForBattleAsync` 可以返回 null。修改前 `AppManager.InitializeBattleAsync` 接受这个结果并继续 Battle 配置。当前 `GameConfig.asset` 序列化正式根非空，因此该问题是可达配置分支，不声称默认场景已走旧资源。

本 Change 只在 `AppManager.InitializeBattleAsync` 取得预热 manager 后、调用内容验证及 `InitializeBattleSingletons` 前，检查 `CharacterAnimtorManager.ConfiguredContentRoot.Length == 0` 并抛出明确错误。该属性对序列化根做 `Trim`，其余正式根验证和已有 catch 中的有序关闭路径不变。没有编辑 Menu 预热、manager 公共验证、DAT、图、Scene、GameConfig 或旧资源；521 个旧 DAT/图行的删除授权仍为零。

原项目当前 Unity Editor 实例 `gameplay-ability-system-for-unity@b1b02287` 在 Edit Mode/idle，`refresh_unity` 返回 ready/recovered；`Library/ScriptAssemblies/Assembly-CSharp.dll` UTC 05:36:26 晚于修改后 `AppManager.cs` UTC 05:36:03，Editor 无编译进行中/重载待处理。此为当前原 Editor 导入与编译通过的限定证据，不是 Menu Play 行为证书。Battle、Menu Scene 与 GameConfig asset SHA-256 分别为 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`，与改前相同。

仍待：在原项目 Editor 中，空根 Menu 预热成功后尝试入 Battle，应在 `BattleRunning` 前拒绝并完成关闭；恢复非空正式根的 Menu→Battle 回归应通过。现有 Menu 回调探针固定旧 `B8B1...` 语义指纹，当前项目模式指纹为 `27CAE...`，不能把它未经更新的失败当生产回归。应以独立诊断 Change 修正探针身份门并做这两种配置的聚焦 Play，再决定本 Change 是否可升为 `VERIFIED`。目前仅 `RUNTIME_PENDING`，Q07开放。
