# Task Contract — NTSD28-B4-F04-STATE12-18-AIRBORNE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SELECTOR_SEAM_DEFINED`

## 目标

只读闭合type0 state12/18 airborne action selector的速度分段、environment条件、phase来源与
exact/shared production调用边界。

## 结论

- Authority使用post-gravity Vy；state12仅action<185或185<action<191进入180/186族，阈值严格为`<-8/<1/<8/else`。
- 仅180族且`EnvironmentState320<0`启用override：post-gravity Vy严格`<12`且upcoming phase12>=6选182，否则181。
- state18仅current action<205且post-gravity Vy严格`>1`选205。
- Unity exact/shared两套selector速度分段基本匹配，但错误读取`WeaponCount<0`，并以`(tickIndex-1)%12`推相位；production C06实际应读取C23提交前的`(NativeResourcePhase12+1)%12`。
- Unity使用`Runtime.Y<0`判空中，negative collision floor上的ground/contact会被误当空中；mechanics result必须显式携带`Airborne`。
- selector action只写frame，不reset counter；state12/18 landing/environment transaction不在本包。

## 下一步

建立pure selector kernel，exact/shared调用同一owner；registered world取upcoming native phase，
无world direct兼容入口用规范化tickIndex fallback但不得定义production authority。
