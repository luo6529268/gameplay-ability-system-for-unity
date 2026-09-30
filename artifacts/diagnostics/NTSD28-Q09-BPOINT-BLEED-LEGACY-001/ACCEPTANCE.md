# Q09/P-08 Legacy 出血标记限定验收（2026-09-28）

状态：`VERIFIED`，仅关闭本 Change 的 LegacyOnly 出血标记生产出口。原 Battle Scene 受控相机像素、相邻帧展示位移、陈旧 handle 拒绝、预备容量复用及停止/重备已通过；自然 Legacy 受击与正式 EXE 同视口对照仍属 P-08 后续，Q09/BATCH-05 不关闭。Q07/D-024 共用碰撞域取舍和 BATCH-04 优先级不变。

正式配对 `render_snapshot.cpp` 的 body 后出血标记顺序用于命令预期；这次 Play 使用正式内容 Ita OID9/frame0 的唯一 bpoint，但 HP500→166 是受控夹具，不是自然受击。保存的 GameConfig、两 Scene 和项目模式 Asset 均未写入；LegacyOnly 仅在进入 Play 前通过精确命名的内存副本选择。相机 A/B 使用原 Battle Scene 的 World camera，临时白底与 Battle layer，恢复后不保存场景。

首轮 `legacy-ita-hp-ab-20260928-01.json` 是探针前置 FAIL：`BeforeSceneLoad` 时 `GameConfig.Instance` 尚可为 null，过严身份门未选择 Legacy，生产以 CentralOnly 启动。未触及出血画面。只修新探针接受 null 或保存 Asset、仍拒绝不相关 singleton，原 Editor 重新编译 0 C# 错。首轮原始文件保留。

第二轮 `legacy-ita-hp-ab-20260928-02.json` 返回 `PASS_CONTROLLED_LEGACY_PIXEL`：backend=`LegacyOnly`，Ita frame0/bpoint1，高 HP500 无 mark、低 HP166 有且仅有一个 mark；body 命令 index1，mark index2。Legacy body SpriteRenderer 可见，生产专用标记 SpriteRenderer 活动数1，拒绝数0，位置/颜色/排序与发布命令一致。1920 宽相机 A/B 的标记投影 ROI x[1038,1043)、y[600,608) 中，高低图新增红像素3。原始高/低 PNG 在同目录，以相同 runId 加 `-high.png` / `-low.png` 命名。两图已打开复核；该标记只有数个像素，整张图缩放查看不适合判定细节，像素计数仅解释其可见贡献。

Play 清理：对象4→4、runtime槽2→2、池借用2→2，fixture 已释放、相机状态已恢复。MCP 读回 Editor idle、非 Play、原 Battle Scene `isDirty=false`。Battle/Menu/GameConfig/ProjectBattleModeConfig 四文件 SHA-256 分别保持 `2EE465D8...B48B77A`、`785F828C...1B81E13`、`0527D737...D074CB8EA7`、`B57CFEF3...C1EDD85B82`，Git 对这四个路径无修改。`git -c core.safecrlf=false diff --check` 通过。

已有聚焦测试的原 Editor RED 1/1（缺 Legacy 命令）和修后 Legacy/中央/预建清理 3/3 PASS 见同目录 `red-job.json`、`green-command-job.json`、`green-owner-job.json`。仅在第二轮受控像素通过时，这些证据仍不足以证明相邻位移、陈旧 handle 与重备；下述后继窄验补齐了这些字段。自然 Legacy 受击与正式 EXE 同视口画面仍不由本 Change 证明。

后继窄验：共享展示位移的原 Editor 精确两项 `shared-motion-job.json` 为 2/2 PASS，涵盖 BleedMark 的 body 锚点和不同代/非相邻句柄拒绝。第三轮 `legacy-ita-hp-ab-20260928-03.json` 的 400 个专用标记在同帧重复呈现与停止后重备均保持 400；Ita 释放后旧发布帧仍保留，但活动标记为 0。第四轮 `legacy-ita-hp-ab-20260928-04.json` 在相邻 tick107→108 的源规则 X 移动 20 像素、alpha=0.5 时，正式 body 位移期望 X=-0.153638407588，Legacy 标记实测 X=-0.153638422489 世界单位，绝对差约 1.49×10⁻⁸；展示时钟的两处测试性反射值在 `finally` 恢复。第五轮 `legacy-ita-hp-ab-20260928-05.json` 再通过上述门，并额外断言白纹理、命令尺寸对应的 Transform scale、颜色、Battle layer、sorting layer/order 与命令一致；受控高低 HP 相机 A/B 仍为新增红像素 3。两轮探针的 PNG/JSON 和之前所有原件均保留。

最终原 Editor 退出 Play 后 MCP 报 idle、Battle Scene `isDirty=false`；World 对象4→4、槽2→2、池借用2→2，相机已恢复，四保护文件 SHA-256 和 Git clean 状态保持。现有只读菜单 `Inspect Probe GameConfig` 于 2026-09-28T10:06:10Z 写出 singleton=保存的 CentralOnly Asset、loaded=1、probeClone=0，见 `../NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001/idle-config-inspection.txt`。最终探针修订生成 Editor 工程 0 错、原 Editor 刷新导入成功。早期相邻探针出现一次 C# `CS0165` 局部变量未赋值，已仅修探针并在后续构建/原 Editor 编译恢复 0 错；未触及生产。此结论仅覆盖声明的受控 Legacy 出口；自然命中到 Legacy GPU、正式 EXE 相同状态同视口像素、P-08/Q09 汇总出口仍待。

最终留痕检查：`Tools/Validate-ChangeLedger.ps1` 退出码 0，首行 `Change ledger validation PASSED`，其包含历史声明文件未在本次 diff 中的警告输出保存在仓库 `Temp/NTSD28-Q09-BPOINT-BLEED-LEGACY-001-ledger-final.log`；`git -c core.safecrlf=false diff --check` 退出码 0。两 Scene 与 GameConfig、项目模式 Asset 四保护路径均为 Git-clean。
