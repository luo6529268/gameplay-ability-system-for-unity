# Q09/P-08：336B44 playable 与原 Unity Battle 血点画面回访

状态：`CURRENT_PLAYABLE_OFFSCREEN_AND_UNITY_CONTROLLED_GPU_SCOPED_PASS / SAME_STATE_ROOT_GPU_PENDING`。只关闭本次声明的新版源码离屏血点输出与当前 Unity 受控血点画面两个子门；P-08、Q09、Q12 及总目标继续开放。

## 版本与输入

- 当前根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。诊断使用其对应、参与 playable 构建闭包的当前 C++ 源码；并未启动根 EXE。旧诊断源 `ita_natural_bleed_warp_probe.cpp` SHA-256 `0C14C9104659A8852FAC8F73E650148F31B6A78721BE7864D63D656338F9404E` 未改，以旧[编译参数](../NTSD28-Q09-P08-FORMAL-NATURAL-BLEED-WARP-001/compile-argv.txt)重建到本目录，路径仍指正式发行根当前源码。[本次参数](compile-argv.txt)、[编译退出码](compile-exit.txt)为 0，[编译日志](compile.log)为空。
- 正式与 Unity 暂存 `decoded_dat/c/ita/ita.dat` 的整文件 SHA-256 均为 `2EC65F77FB74CB1106506E87FCEFB643DEDD022D395E0C4A325E1816EEDA8958`；不修改 DAT 数值。正式全 405 DAT 的 BPoint 内容默认值分支见[前置回访](../NTSD28-336B44-Q09-P08-BPOINT-CONTENT-RECHECK-20261004/REPORT.md)。
- playable 原诊断单案：鸣人 OID2/X500 对鼬 OID9/X540，同 Z650、seed2833、模式0，鼬初始 HP180/base500；前两 tick 普通攻击输入，随后释放。Unity 原保存 Battle Scene：现有受控 `NTSD28Q09BPointBleedScenePixelProbeEditor` 在正式 `LoganRuntime` 内容下给临时鼬 OID9/frame0 同一暂停 World 先 HP500 后 HP166，通过生产中央发布和原相机渲染高/低两图。Unity 案例是**受控血量对照，不是同初态的自然攻击**。

## 当前实际结果

- [playable 原始日志](run.log)退出码 0：第 8 tick 自然伤害使鼬 HP180→160，第 22 tick 站立 frame0 后有 1 条血点命令；阈值166、大小1×3、RGB 红色、1333×730 截图坐标 `(461,595)`。22 tick 轨迹及两张 [有标记图](ita-mark-on.png)、[消融图](ita-mark-off.png)与旧 B1E13 单案各自逐字节相同，但这里的两图是**用当前 336B44 源码重新编译并运行所得**。
- [修正后的独立像素分析](pixel-analysis-corrected.json)以 `1333×730` 全域逐点比较 RGBA，恰有 3 像素变化：`(461,595..597)` 从 `(106,118,121,255)` 变为 `(255,0,0,255)`。首次[分析输出](pixel-analysis.json)误用 RGBA 差图 `getbbox()`，Alpha 差值全 0 导致假零；原件保留，以上修正结果覆盖它。
- [原 Unity Editor 报告](../NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001/bpoint-ita-336b44-current-20261004-01.json)为 `PASS_CONTROLLED_CENTRAL_PIXEL`：高 HP500 时标记命令0，低 HP166时1；本体索引1、标记索引2，槽50，大小1×3。生产相机 1920×1080 的[高血图](../NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001/bpoint-ita-336b44-current-20261004-01-high.png)与[低血图](../NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001/bpoint-ita-336b44-current-20261004-01-low.png)在探针投影区域有 3 个新增纯红像素；独立全图遍历在 `(1040,474..477)` 找到 4 个新增纯红像素，其中第 4 个位于探针限定区域之外。这不改变探针的 3 像素区域断言，原图保留用于复核。
- 原 Unity Editor 的旧探针脚本未改，当前 `Assembly-CSharp-Editor.dll` 晚于脚本。临时鼬注销、相机状态恢复；World 对象4→4、槽2→2、渲染借用2→2。退出后 Editor idle/non-Play、测试未运行，Battle Scene clean，随后恢复为运行前干净 Menu。Battle/Menu/GameConfig/Mode Asset 四个磁盘 SHA 与本次运行前逐项相同，见[前](protected-before.txt)、[后](protected-after.txt)。

## 不能从本次证据推出的结论

当前 playable 与 Unity 两案使用不同初始 X、不同背景、不同视口，且一个是自然受击、另一个是暂停 World 受控血量切换；3 与 4 输出像素不能直接作对齐差异。根正式 EXE 实际 GPU Present、同初态同 tick 的根/Unity画面、自然物理键到当前 Unity Game View、Legacy 出口仍未证。旧 B1E13 结论保持旧版标签，不因新版图像字节相同而自动晋升为完整一致。

本轮只创建诊断输出与本报告，并复用原 Editor 已有探针；没有修改 C++/C# 脚本、DAT、PNG、生产配置、场景或非战斗功能。
