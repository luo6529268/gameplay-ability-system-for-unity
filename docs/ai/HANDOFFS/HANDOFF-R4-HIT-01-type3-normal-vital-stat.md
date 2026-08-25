# HANDOFF — R4-HIT-01 type3 normal vital/stat writes

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-001`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入C++ authority。  

## 完成内容

- C++ release source确认 type3 target不走type6 reaction-only，而是先走normal `apply_hurt` public
  vital/stat writes，再进入type3 tail；
- Unity `ApplySpecialAttackDamage`新增最小type3 writer，写HP、HPBound、ComboCountVic与DamageStats，
  并放在`ApplySpecialObjectHurtTail`之前；
- 新self-check走真实 special target `Hit`入口，验证lethal type3的四项字段、tail读写顺序，以及
  type0-only holder/global kill/combo score不被误写；
- UnityMCP scripts refresh后Console `error CS`=0；full self-check结果文件为
  `PASS`（2026-08-22 05:26:41 +08:00）。

## 关键不变量

- **不要**把`ApplyStandardVitalAndStatWrites`直接复用到type3：它包含C++仅对type0允许的kill / holder score；
- **不要**把本次字段修复延后到type3 motion tail之后：tail会读取HP并导致同tick fall/lifecycle差异；
- 没有修改candidate、RNG、ITR、weapon、CPoint、held/link、opoint、scheduler、input、AI、render、DAT或资源。

## 未关闭 / 不得夸大

- C++ runtime trace、真实 Play Mode、type3 death/lifecycle和Karasu identity joint evidence未取得；
- 没有单独GC allocation profile；
- 不能把本包写成完整R4、完整type3 lifecycle或整体战斗对齐。

## 连续下一步

按D-009自动进入`D-HIT-002`只读 source preflight：先将C++ kind10/11、kind16和normal weapon
raw-frame writer的字段副作用拆为独立可测试子范围；不得把它混入本type3 vital/stat Record。
