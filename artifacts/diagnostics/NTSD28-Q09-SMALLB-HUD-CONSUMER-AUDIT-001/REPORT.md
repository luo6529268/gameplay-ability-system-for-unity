# Q09 smallb 战斗 HUD 消费链审计（2026-09-24）

> **2026-09-24 权限更正（覆盖下文实施建议）：** 用户已在对齐总表 §0.2、§1.2/P-17 排除完整原生 HUD，`docs/ai/DECISIONS.md` 也记录了该决定。本报告的正式 smallb-first 与 Unity 静态头像差异是已观察事实，但属于批准保留的排除差异，不能据此修改 Battle Scene、HUD consumer 或发布 104 张只供原生 HUD 的图片。前述发布包由 `NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001` 纠正；Q09 继续处理非例外的阴影、火花、排序等。

状态：`STATIC_FIRST_DIFFERENCE_CONFIRMED / PRODUCTION_HUD_CONSUMER_MISSING / RUNTIME_PIXEL_PENDING`。本包只读；未修改 DAT、图片、脚本、Scene、Prefab 或 ProjectSettings。

正式 playable `source/ntsd28_core/src/rendering/render_snapshot.cpp:1403-1431` 在 type0 且有 HUD 槽时、任何 body sprite gate 前产生 HUD 行；头像首先选 `bmp.last("smallb")`，只有字段不存在时才回退 `bmp.last("small")`，然后检查源文件存在。`source/ntsd28_playable/src/d3d11_renderer.cpp:1842` 起消费 HUD 纹理命令。这是战斗 HUD 的规则，不是选人头像规则。

当前 Q01 正式对象索引声明的 1,010 张不同图片均已部署且逐 SHA 一致（见 `NTSD28-Q07-INDEXED-IMAGE-COVERAGE-001/REPORT.md`）。其中 104 张独立 `smallb` 与当前生产候选的 `file/head/small` 906 张不重叠。`LoganVisualContentCandidate.Capture` 只加入 `files/head/small`；`CharacterAnimtorManager.PrepareNativeUISpritesAsync` 只准备 head/small，`CharacterUIResourceManager` 只发布两者。`GetSmallSprite` 在生产源码中没有 HUD 调用者；选人 `SelectRoleItem` 使用 `GetHeadSprite`，不得挪作战斗 HUD。

更具体的序列化首差：`Assets/NTSD/Scene/NTSD_Battle.unity:1014` 的 `HeadImg` 是静态 uGUI `Image`，`:1059` 固定引用 GUID `57bbfdb562466dc4fae25bf18a6a7fad`，其 owner 为 `Assets/NTSD/Sprite/UIPanels/BattleHud/naruto_s.png`。该文件当前 SHA-256 是 `C0402CB7F0D94C52D0D2905DA3E828D41A9A78F8E0D82C4993C8004F7D966C6F`，恰好等于正式 `sprite/small/naruto_s.png`；正式 `sprite/smallb/naruto_s.png` 及暂存同 SHA `9C12B2A2B38D2EFCDDF0986D5EE49F5B02DFC626BC0E9DD08A1C4801C270AD4A`。Scene 内 `HeadImg` 的 GameObject/Image fileID `221048552/221048554` 没有其他序列化引用；项目生产 C# 搜索未发现按名 `HeadImg` 读者。这证明当前保存场景的固定头像源与正式 Naruto HUD 源不同，但还不证明 Play 中最终像素或所有 HUD 模式结果。

后续精确实施入口：独立 Q09 Task/Change 先核对 `smallb` 原始字段在 `LoganObjectCatalog` / 配置模型中的承载，再将有 `smallb` 的正式图纳入候选哈希、预热、原子发布及清理，保留 head/small 现有选人语义；战斗 HUD consumer 从已发布战斗实体/HUD 槽与对象 ID 选择 `smallb`、缺字段才回退 `small`，不能以固定 Naruto 图代替所有角色。最后在原项目 Battle Scene 验证 Naruto 和至少一个不同角色的选择、首帧/换人/退出重进、像素可见性及旧资源租约；Scene 的用户 `HUDBg x30`、现有布局/全景相机和非战斗 UI 不在这个资源选择包内。若当前 HUD 没有实体槽发布入口，先建立准确依赖合同，不能直接写 Scene 常量或让 uGUI 状态反写战斗逻辑。

Q09/R17、完整 HUD 画面和总对齐仍开放。此报告不授权删除旧 `naruto_s.png`，也不授权批量修改 104 张图或全 HUD 布局。
