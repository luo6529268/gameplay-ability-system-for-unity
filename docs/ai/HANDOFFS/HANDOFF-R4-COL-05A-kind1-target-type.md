# HANDOFF — R4-COL-05A kind1 target type consume

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-COL-005A`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改或写入C++ authority。

## 完成内容

- C++ source确认 kind3/8有 Character target gate、kind1无该 gate且进入common Entity `case 1` writer；
- Unity `BattleInteractionWriter.TryApplyGrab` 已将type guard从kind1/3共同限制，收窄为仅kind3；
- 新 self-check实际经 frozen candidate→character consumer→writer验证：
  - kind1 character attacker / LightWeapon-type target：1 candidate，消费后frame297/130、caught/catcher、
    duration300、fall0；
  - kind3同类 target：0 candidate，frame/relation/fall保持；
- final Unity compile：Console `error CS`=0；full self-check：
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入2026-08-22 05:05:17 +08:00。

## 首次失败及修复

首次测试代码因 `&&` 短路下 `out CollisionCandidateRange candidates` 可能未赋值，得到 CS0165。
修复只是在测试方法中先将 `candidates` 初始化为 `default`；未触及运行时 writer或battle逻辑。

## 未关闭 / 不得夸大

- C++ runtime trace仍受 R1-WP02 `BLOCKED`限制；未跑C++ executable；
- 未做真实 Play Mode / 实际DAT target验证；
- `D-COL-005B` 的 non-character attacker、weapon kind1 selector与pickup可达性没有修改；
- 不能把05A写成完整kind1、R4或战斗系统完全对齐。

## 连续下一步

按D-009进入 `D-COL-005B` 只读 source preflight，先定位 C++ non-character attacker kind1的真实 caller、输入/selector
条件与Unity weapon/pickup路径；不得复用05A的target-writer结论直接修改 weapon/special attacker路径。

