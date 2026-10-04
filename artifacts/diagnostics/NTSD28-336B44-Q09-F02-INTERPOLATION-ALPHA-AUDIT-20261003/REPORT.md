# Q09/F02 武器画面位置的显示采样相位（2026-10-03）

状态：`VERIFIED_SAVED_FRAME_GEOMETRY_ONLY`。本报告只读核对已保存的正式 playable 离屏帧、原 Unity Battle Scene 中央命令和两侧生产绘制公式，没有重新运行 Editor 或修改脚本、DAT、图片、Scene。结论是：原先将 F02 相对 tick39 的正式武器中心 `x584` 与 Unity 截图中约 `x800` 直接按视口比例相比，得到的约 41 输出像素左偏，不是已证的武器位移首差。保存的 Unity 命令**精确符合**其生产插值器把武器从本 tick 位置向上一 tick 回退 30 个正式规则像素，再乘 D-024 视图比例的结果。正式 WARP 图绘制的是本 tick 未插值快照；两者不是同一显示采样相位。

## 同 tick 数据及计算

- 当前正式根 EXE 身份为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable 的 `RenderSnapshotBuilder28::build` 按 `position.x ± centerx` 求 sprite 矩形；`presentation_interpolation.cpp` 在相邻 tick 的连续实体上用源精确坐标线性插值后 `lround`，把所得整数位移加到 sprite 与阴影。本次 [WARP tick39 五行几何](../NTSD28-336B44-Q09-F02-FORMAL-OFFSCREEN-001/source-bg1-z400-20261003-02/tick39-sprites.csv) 是直接绘制本 tick 快照：slot2/OID600/pic1 左 `560`、宽 `48`、中心 `584`。该离屏工具和根 EXE 的实际 Present 不是同一证据层级。
- [原 Battle Scene 保存的 tick39 命令/样本](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-central-alpha-20261003-03/00058.json)：slot2 的规则精确 X 在 tick38 为 `543.261946777405`，tick39 为 `584.363429741788`；tick39 视图 X 为 `896.993476452499`。slot2/pic1 中央命令的 world X 为 `-3.4984`。同帧静止鸣人、Tayuya 的命令分别为 `-8.8950`、`-3.8500`。整数视图 X `896` 由现有 `SyncIntegerPosition()` 的向零转换与保存的精确 X 推得，不是 JSON 独立记录字段。
- 正式 DAT 与 Unity LoganRuntime 的 `w/6.dat` frame41 都是 `centerx:24`、pic1 宽48；鸣人 frame1 是 `centerx:38`、宽79；Tayuya frame227 是 `centerx:39`、宽79。Unity `LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels` 在三者本帧用相同公式，静止两人的整数视图 X 分别为308、814。由 `-8.8950 = L + (308 + 1.5×(79/2−38))/100` 与 `-3.8500 = L + (814 + 1.5×(79/2−39))/100` 独立求得同一个视口左端 `L=-11.9975` 世界单位。
- 武器未插值命令应为 `L+896/100=-3.0375`。D-024 水平比例 `2048/1333=1.536384096`；Unity `BattlePresentationMotionSampler` 对此连续源位置若采得 `lround(interpolated X)−lround(584.3634)=-30`，中央命令应为 `-3.0375−30×(2048/1333)/100=-3.49841523`，与保存值 `-3.4984` 在四位显示精度内相等。使源插值整数为554的 alpha 区间约 `[0.24909,0.27342)`；本保存数据没有直接记录精确 alpha，因此不把区间内某个单值称为实测。
- `BattleCentralRenderSystem` 在 `CentralOnly` 路径调用 `DisplayMotion.Prepare` 与 `ApplyToCapturedCommands`，并按 publication clock 求 alpha；这条生产路径解释为何静止角色没有回退而飞行武器有。按现有 Scene 相机 X−1.79、正交半宽10.24及1920宽，Unity 武器中心预测为 `799.84` 输出像素。源插值中心554按两视口宽比投到 Unity 是 `797.96`，差约1.88输出像素；未经插值的源中心584则是 `841.16`，与 Unity 相差约41.32输出像素。后者混合了不同采样相位，不能据此改武器逻辑或 DAT。

## 证据边界与下一步

现有保存命令的四位量化值与 `−30` 源整数位移公式精确吻合；这证明**该表面差值可以完全由现有生产插值解释**，但没有独立记录当时的 alpha，也没有正式根 EXE 同 alpha 的 GPU Present 截图。下次 F02/Q09 画面位置验收应记录中央提交的实际 `displayAlpha`，并让正式 playable/WARP 与 Unity 取相同 tick 和相同 alpha；或两边都取 alpha=1 的未插值快照。再比较实体锚点、阴影及真实像素，不能把背景/固定视口例外纳入逐像素等同要求。Q09/F02、Q12 与总目标继续开放。
