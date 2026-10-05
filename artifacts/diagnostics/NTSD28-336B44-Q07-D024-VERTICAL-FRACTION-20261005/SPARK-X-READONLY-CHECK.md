# Q07/D-024 同一自然火花 X 比例邻接核查（只读）

2026-10-05。当前正式根 EXE SHA-256 已重新核对为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本核查只复用前一包同一自然 C040、mode0/seed0、第25相对 tick 原件，没有新 Play、脚本改动或资源处理；用户保留固定完整背景和 1.5 倍实体显示尺寸。

正式 playable [火花原件](../NTSD28-336B44-Q01-SPARK-PLAYABLE-OFFSCREEN-001/first-positive-20261004-02/tick25-sparks.csv)中，可绘制 pic0 事件 `screen_x=559`，host slot2 同 tick 的源 X=560，故相对 host 为 -1 个源像素。Unity [修后自然 Scene 原件](../NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q07-d024-spark-green-20261005-01.json)中同 host slot2 的源 X=560、物理 X=860.3750937734434；火花命令世界 X=-3.4074652194976807。项目[地图边界资产](../../../Assets/NTSD/Map/TrainingGroundMap.asset)左缘 `xMin=-12.000725`，其 Scene BoundaryWallEditor 多边形加世界平移得到同一值；`NTSDRenderSpace.CaptureViewportTransform` 在当前可行走边界存在时以它作逻辑左缘，100 像素/世界单位。逆变换得到火花 X≈859.325978，距 host 精确视图 X≈-1.049116。正式 -1 源像素按 D-024 横向倍率 `2048/1333` 对应 -1.536384 视图像素，残差约0.4873像素；若以生产取整 host XInt=860，残差约0.8624像素。该同 tick 单例没有显示新的超出整数/浮点取整范围的水平比例首差。

边界：Unity 命令已包含 host 的渲染偏移与相机 X，原 JSON 没有单独导出这两个量，故上述是最终命令相对物理 host 锚点的画面核查，不是原始 `HitRecordX` 的逐字段证书；正式样本来自 playable D3D11 离屏而非根 EXE 实际 Present。只说明本自然火花的 X 不需要凭视觉猜测另开修复包，不证明所有火花或非例外画面都已对齐。Y 首差修复与原 Scene 验收另见 [SPARK-ACCEPTANCE](SPARK-ACCEPTANCE.md)。
