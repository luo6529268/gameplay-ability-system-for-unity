# Q09 smallb 资源发布限定验收（2026-09-24）

> **2026-09-24 后继更正：** 本包当时的测试结果真实，但选错了范围。对齐总表 P-17 明定完整原生 HUD `USER_EXCLUDED`，smallb 正是其图片源；本包代码由 `NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001` 精确撤回，不能以此历史验收继续要求 HUD 接线。保留以下记录作为审计历史。

状态：`FOCUSED_TEST_PASS / HUD_CONSUMER_AND_PLAY_PENDING`。本包在原项目 Unity 2022.3.62f3 Editor PID 10576 中完成；没有启动第二项目实例，也没有使用 computer-use。

生产改动：正式 `NativeMetadata.Bmp` 已保留 `smallb` 最后值，候选将其图片路径/字节 SHA 加入当前 visual fingerprint（正式与暂存各 1,010 图）；`CharacterAnimtorManager` 使用候选绑定的 SHA 解码额外战斗头像，连同 head/small 在同一无 await 发布点提交给 `CharacterUIResourceManager`，并由既有 operation Sprite/Texture 集合回收。新的 `GetBattlePortraitSprite(id)` 独立于选人 `GetHeadSprite`、现有 `GetSmallSprite`；仅当 `smallb` 字段缺失时回退已加载的 small Sprite。未改 DAT、Scene、Prefab、UI 布局或任何非战斗操作。

验证：原 Editor 导入/重载完成，`read_console` 为 0 error。聚焦 job `80a2799be5d0460a94b28876aa9e5f51` 5/5 PASS，包括正式/暂存同指纹、正式 1,010 图、7 型击倒图、WORDS 输入和完整 330 对象发布/回收。新增 smallb 字节变更使既有 candidate 失效的测试初轮 `95705f14c9fd450e8ac98662267156a3` FAIL（测试对 Windows 路径做直接字符串比较）；测试路径规范化修正后 job `f7046adbc8064e9cb5ccad929ce39ce6` 1/1 PASS。补充有字段/缺字段头像选择断言后，实际 330 对象发布/资源回收 job `5e53239b5cd64879bd8821b21beb2881` 1/1 PASS。候选测试类初轮 7/8，唯一旧 PNG 变更例因缺 `Temp/NTSD28PngDecode/generated/palette-8-filter-0.png` FAIL；按现有 `Tools/NTSD28PngDecode/Generate-Fixtures.py` 仅生成 Temp 夹具后，单例 job `5b44b05c4b0d4679a6a0011b94d9acbc` 1/1、整类 job `5759979d39f3409d9eb2dbd850056050` 8/8 PASS。保留所有初轮失败记录，不把 0-test 误选 job `185c4949208244059d11602342a18d08` 计为验收。

`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` PASS（783 Records，22 个当前 diff code files covered）；`git -c core.safecrlf=false diff --check` exit 0。保存的 Battle/Menu Scene SHA 仍为 `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` / `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`。

未验证：Battle Scene Play 中实际 `HeadImg` 是否从战斗实体槽读取该资源、Naruto/其他角色像素、换人、退出重进和原版同场景对照。当前保存的场景头像仍是固定 small 图；下一 Q09 consumer Task 必须使用逻辑战斗 HUD 槽/对象 ID 做显示选择，并保留用户 `HUDBg x30` 和既有 UI 布局。本包不授权旧图删除、不能关闭 Q09/R17 或总目标。
