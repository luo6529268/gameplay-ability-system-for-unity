# HANDOFF — CAMERA-PRESENTATION-REMOVE-001

> 日期：2026-08-24  
> 状态：`COMPLETE / VERIFIED`

用户要求删除`BattleCameraSafeArea`的安全区域、viewport布局/视野、follow和调试逻辑，并追加保留：
按背景 bounds 自适应正交尺寸。该脚本没有外部脚本调用，
但`NTSD_Battle`场景仍序列化挂载它。最终实现保留`[RequireComponent(typeof(Camera))]`、原字段名和这唯一的背景取景链；
不要修改scene，不要物理清理旧YAML字段，不要修改`NTSDRenderSpace`或任何战斗代码。

初版已收缩为无运行逻辑兼容标记，但被用户范围修正取代。最终最小实现只按背景 bounds
和相机 aspect 更新`orthographicSize`，不改相机位置。

已完成的验证：Unity scripts fresh compile、`NTSD_Battle`一个有效组件、`Bg (2)` bounds / 16:9 / `orthographicSize`
数值合同、短Play bootstrap、filtered Console 0条本组件错误、Ledger validator和scoped diff均PASS。
场景关闭时的MCP断连日志和既有cleanup warning未归因于本组件。无需继续操作；若日后希望删除scene YAML中的旧序列化字段，须另建scene migration Change。
