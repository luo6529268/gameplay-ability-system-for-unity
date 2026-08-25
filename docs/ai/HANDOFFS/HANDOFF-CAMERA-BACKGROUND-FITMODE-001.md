# HANDOFF — CAMERA-BACKGROUND-FITMODE-001

> 日期：2026-08-24  
> 状态：`SUPERSEDED / USER REJECTED CROP`

用户要求在`BattleCameraSafeArea`中增加“背景填满视野”的模式，以消除上下镂空。
当前脚本只使用`ContainBackground`等价公式，完整显示背景但会留空。

已完成：新增`ContainBackground`/`CoverViewport`枚举、序列化选择字段和取尺寸分支；默认`CoverViewport`。
`CoverViewport`必须用`min(extents.y, extents.x / aspect)`，不许拉伸背景、改写 Transform、加入安全区/viewport/follow/debug逻辑，
也不许修改场景或战斗代码。

验证结果：fresh compile通过；当前`Bg (2)` / 16:9计算结果与相机`7.05703163`相同；
`backgroundFitMode=1`；相机Transform未变；短Play bootstrap和camera-filtered Console通过。仅剩本次文档落盘后的validator/diff复核。

用户确认上述Cover会舍弃背景两侧内容，不能交付。不要继续修补此裁切模式；
后续只可按`CAMERA-BACKGROUND-NOCROP-FIT-001`实现无裁切的完整显示 / 全覆盖模式。
