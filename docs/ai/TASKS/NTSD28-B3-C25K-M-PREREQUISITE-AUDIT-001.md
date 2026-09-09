# Task Contract — NTSD28-B3-C25K-M-PREREQUISITE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / DEPENDENCY_ORDER_CORRECTED`
> 依赖：`NTSD28-B3-C25P-HEALING-OWNER-001 / VERIFIED`

## 目标

只读闭合C25k/m所依赖的terminal pending producer和`Frame.Prev`读写边界，判断二者能否独立前移；不修改代码、内容、Scene或Authority。

## 结论

- Authority C25g在frame/post-cost结果为负action或`>=999`时写`lifecycle_resolution_pending=true`及原始`lifecycle_code`；C25k读取该pending并拒绝OPoint。
- Unity当前无对应pending/code carrier；frame body会把部分`next:999`提前规范化为0/212，后续仅看`Frame.N`无法无损恢复原terminal code。
- Unity OPoint factory除counter-zero外还有type0 `FrameDelay!=0` gate，Authority C25k本身没有该gate；删除它必须与formal pending producer和held-frame证据同包完成。
- 当前tick中hit/collision对`Frame.Prev`的读取均发生在C25之前；C25尾段内需要旧Prev的生产reader是mixed transition effect。C25l state18/19明确必须在C25m commit前执行。
- 因此不能先做C25k/m：下一安全包改为C25l/m transition-history transaction；C25k与C25g terminal pending/B7生命周期carrier合并处理。

## 不做

不创建伪pending，不用`PendingFlushDestroy`替代terminal code，不改OPoint gate、transition、RNG、DAT/资源、Scene、Input或Authority。
