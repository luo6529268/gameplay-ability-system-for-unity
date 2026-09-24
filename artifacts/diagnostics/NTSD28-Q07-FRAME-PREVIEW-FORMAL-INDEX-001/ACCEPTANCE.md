# Q07 角色帧预览窗口旧索引读取限定验收

结果：`NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001 / VERIFIED_SCOPED_PREVIEW_INDEX`。当前项目选正式 Logan 内容根时，打开 `CharacterFramePreviewWindow` 不再把旧 `Assets/NTSD/Config/data.txt` 注入共享 `GameDataManager`。窗口保留原来的帧/精灵管理器读取和“刷新所有数据”提示；后者的 Inspector 按钮已由 `NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001` 接到正式预热。空根配置仍可显式使用旧索引。

测试先修正夹具绑定：`bf21ad10b3bd4467bfe44dee4f5dbe1d` 的 `GameConfig.Instance=null` 和 `cf0aeebe05e8469dbb0dca3277409928` 的 auto-created GameDataManager 都是夹具错误，不计入行为首差。正确绑定后，**原生产代码** job `c74a38f8fd9b45fb9d753537aeac8571` 得到正式根失败/空根通过的 1/2：正式根打开窗口仍加载旧索引。只给窗口 `LoadDataFile` 加正式根判断后，原 Editor 重编译，job `2104a9446b7c4d08b01eca86cc2a4704` 两个用例 2/2 PASS；相邻 Inspector 正式预热与旧显式数据 job `e5299057e8d4401a9c6a9016dc1f6d6d` 2/2 PASS。

`NTSD_Battle.unity` 保存文件 SHA-256 `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`、`NTSD_Menu.unity` `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228` 与修改前相同；当前 Battle Scene `isDirty=false`。未运行可见预览截图或正式 EXE 逐像素对照；该包不认证它们，也不授权删旧 DAT/图片。其余空根历史链、旧夹具和逐文件退场门仍开放。
