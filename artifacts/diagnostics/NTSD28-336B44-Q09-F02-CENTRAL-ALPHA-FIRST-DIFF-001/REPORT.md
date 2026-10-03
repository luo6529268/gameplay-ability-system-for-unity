# Q09/F02 中央绘制黑格归属（2026-10-03）

结论：原 Battle Scene 的 F02 相对 tick39 黑格不能归因为鸣人 `nar.png` 的透明度上传损坏。实际中央绘制队列在鸣人之后还有两个 OID219 特效，分别取正式 `w/e.png` 的 pic3、pic8；两个源格均全不透明，且大部分像素是纯黑。正式 336B44 根运行时在同一相对 tick 也生成这两个 OID219、相同 pic，playable 绘制源码对普通 sprite 走 source-alpha，因此这些源图的黑像素会被绘制。先前“黑格是 Unity 表现差异”的疑点在此证据下不成立；这不是生产修复，也不代表 Q09 已整体通过。尚未取得正式 EXE 同条件 GPU 截图，屏幕逐像素同态仍待验证。

## 对照证据

- [原 Unity Scene 最终运行报告](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-central-alpha-20261003-03/00058.json) 为 `CAPTURED / DONE`、空错误。相对 tick39 的冻结/显示/模拟 tick 均为全局44，中央队列有10条命令、11次提交绘制。鸣人命令 index6：slot0、stable100、visualDataId2、pic1、`SourceTexture2D`，源格 `(80,1521,79,79)`；源纹理及实际绑定纹理各自的 GPU alpha 回读均为 5,220 个零、0 个部分透明、1,021 个全不透明像素，与正式 `nar.png` pic1 内容相符。此模式没有 atlas 页，不存在已观察到的图集 alpha 丢失。
- 同一冻结队列的 index8 为 slot51/stable104/OID219/pic3，index9 为 slot50/stable109/OID219/pic8，均取 `vfs/w/e.png`，尺寸各 `81×82`、`SourceTexture2D`，排序在鸣人 index6 之后。Unity 暂存与正式 `w/e.png` SHA-256 均为 `4BC64B577309546350A0BE6F82A7E8EE5091A0A04953D8DD4B9C945BCBBEC823`。按探针记录的两个源格只读解码：pic3 的 6,642 像素全部 alpha=255，其中 5,931 个为不透明纯黑；pic8 同样全部 alpha=255，其中 6,510 个为不透明纯黑。正式 `decoded_dat/w/e.dat` 定义 `w/e.png`、单格 `81×82`，frame3/pic3 与 frame40/pic8。
- [正式根 trace](../NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/bg1-z400-root-01/root-trace.jsonl) 在相对 tick39 有 Naruto slot0/OID2/action1/pic1/x201/z400，另有 slot51/OID219/action3/pic3/x191/y-6/z401 和 slot50/OID219/action40/pic8/x197/y-6/z402；该帧 `render.sprites=5`。原先只比较三主体槽位的 F02 投影没有覆盖两个特效，不能据此认定它们是 Unity 独有。这里验证的是同一帧的对象身份与图格，不是所有位置/像素已完全同态。
- 正式 playable `source/ntsd28_playable/src/d3d11_renderer.cpp` 的普通 sprite 分支通过 `draw_quad` 读取 `frame.source_path/source_x/source_y/width/height`；`include/ntsd28_playable/d3d11_renderer.h` 的默认绘制模式为 `source_alpha`，像素着色器保留非零 alpha。由源码和正式图片可推断黑色源像素在正式渲染路径不会因黑键自动消失。此项是正式源码路径推断，不冒充正式 EXE 的 GPU 截图。
- [原 Scene Game View](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-central-alpha-20261003-03/game-view-tick39.png) 仍可见左下黑格；同一帧此前暂隐 BattleControls 后黑格仍在。图像文件本身没有独立 GPU 帧号，归属由冻结命令、源图、正式 trace 与源码联合支持。

## 画面几何补证（同日只读追加）

此项只用已保存的中央命令、现有 Scene 相机数值与 PNG 计算，不重新进入 Play，也不修改任何脚本或资源。`NTSD_Battle.unity` 的 ScenesCamera 位置为 `(-1.79,-4.8000007)`、正交半高 `5.7599998`；截图为 `1920×1080`，因此每世界单位约 `93.7500033` 输出像素。生产 `BattleDynamicMeshBackend.WriteQuad` 用 `resource.PixelSize × (1/100) × BattleVisualScale(1.5)` 生成实体宽高，两个 OID219 命令的 pivot 都是 `(0.5,0)`。

| 中央命令 | 从命令与相机推得的连续画面外接框 `(left,top,right,bottom)` | 截图中直接抽样的边界 |
| --- | --- | --- |
| index8，slot51/pic3 | `(216.328,805.781,330.234,921.094)` | `y806` 的 `x215` 是背景、`x216..329` 为 `(0,0,0)`、`x330` 恢复背景；上方 `y805` 无这条黑边。 |
| index9，slot50/pic8 | `(226.106,813.750,340.012,929.062)` | `y814` 可见右侧新增的黑色 `x330..339`，`x340` 恢复背景。 |

预测的第一个左上像素 `(216,806)` 与先前实测最大近纯黑连通区的起点完全一致，第二个外接框解释了其右侧扩展。摇杆覆盖下部，故普通 Game View 的可见连通区高度小于两个完整四边形；暂隐 UI 的旧图已显示下部黑色继续延伸。几何边界与源图全不透明黑色联合提供了直接的 Unity 画面归属证据。这里没有把正式 EXE 的实际 GPU 输出、两边纹理过滤结果或完整屏幕逐像素同态写成已验证。

## 验证边界与状态

此轮只改现有 Editor F02 探针的独立 `captureCentralAlpha` 诊断 opt-in；正式 DAT、PNG、战斗生产脚本、Scene、相机、UI 和非战斗框架均未改。生成 C# Editor 工程最终编译为 0 error/273 warning，原 Editor 已实际导入并完成最终 Play。早期 run-01 仅证明鸣人 alpha；run-02 的诊断循环误在鸣人处停止，run-03 已更正并保留全部10命令；这些历史结果未删除。

最终 run-03 的 `before`、`after`、46组 `samples`、47组 `eventRows` 与同初态无截图 run-07 序列化比较全部相同。有序关闭完成，World对象、runtime slots、池借用、活动 Sprite 均为0，回到干净 Menu，四项保护哈希不变。此 Change 关闭“F02 黑格来源诊断”子门；F02 全链、Q09 其他表现、Q12 集成与总目标均保持开放。后续若要判正式画面完全一致，仍须正式 EXE 与 Unity 同条件 GPU 图像对照，不能修改这两个正式源格来消除黑色。
