# NTSD 菜单字体重绑结果（2026-10-03）

目标：将序列化 GameObject 对 `JifengBladeArtSC-0480 SDF.asset` 的引用，改为 `JifengBladeArtSC-3500 SDF.asset`。

实际修改：在现有 Unity Editor 中保存 `Assets/NTSD/Scene/NTSD_Menu.unity` 和目标 3500 SDF 资源。26 个旧字体 TMP 文本、一个输入框全局字体、六个仍指向旧图集的场景材质贴图已重绑；原先已经使用 3500 的一个文本保持原状。目标字体源文件支持的 59 个当前菜单字形已写入原 1024 图集；源字体不含的可见英文、数字和标点由项目现有 `SOURCEHANSERIFCN-BOLD SDF.asset` 回退，零宽空格仍不可见。没有修改该回退字体、旧 0480 资源或 Battle 场景。

最终序列化扫描：`Assets` 下的 `.unity`、`.prefab`、`.asset`、`.mat`、`.controller`、`.overrideController`、`.playable`、`.anim` 文件中，旧字体 GUID `325e772b4b89e0e4191bbf6bf9968e23` 的引用为 0。Menu 场景现有 27 个新字体 `m_fontAsset`、21 个新字体 `m_sharedMaterial`、六个新图集 `m_Texture` 和一个新字体 `m_GlobalFontAsset` 引用。最终 Scene SHA-256 为 `1F6586B73F45F45994A7CA3A85E73F5A84BF9F299E9DCE96B5988E6D5E571156`，目标字体 SHA-256 为 `747E4568A90DEF3DC971DC159A750CB3A6EBD6246C7F02FA0EB22F0CFB48C18B`。

验证：生成的 `Assembly-CSharp-Editor.csproj` 最终相关构建为 0 error；Unity 原菜单 Play 测试 `OriginalMenu_VisualsWrapFallbackMaterialsAndReopen` 通过，实际截图为 `../NTSD-MENU-CAROUSEL-VISUAL-001/center-vs-143133032.png` 和 `center-tournament-143135248.png`，退出后 Menu Scene SHA 不变。菜单视觉测试共 5 例，3 过、2 失败：`ReacquireDoesNotCompoundScaleOrRetainOwnedPrimaryMaterials` 和 `RuntimeFallbackPopulatesMissingGlyphsAndReleasesLastBorrower` 在本项目 TMP 动态回退字库的 `TryAddCharacters` 路径失败；隔离重跑仍可复现，和此次迁移的因果关系未确认。首次完整菜单结果见 `menu-focused-tests-first.xml`，隔离结果见 `menu-focused-tests-isolated-failures.xml`。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` 最终通过，见 `change-ledger-validation-final.txt`。Unity 自动序列化的 Scene YAML 在 Git diff check 中仍报空白尾随字符；本任务未手改或清理场景 YAML。

保护与回滚：初始和每次写入前的 Menu Scene、目标字库、受影响代码与治理文件 SHA/字节快照均保存在本目录 `before/`、`menu-scene-prepared-*.unity.txt`、`font-prepared-*.asset.txt` 和 `prechange-manifest.json`；具体时间线见 `docs/ai/FILE-OPERATIONS/NTSD-MENU-FONT-3500-REBIND-001-PREPARE/RECORD.md`。回滚必须先核对当前 SHA，不能从 HEAD 还原用户先前的菜单修改。
