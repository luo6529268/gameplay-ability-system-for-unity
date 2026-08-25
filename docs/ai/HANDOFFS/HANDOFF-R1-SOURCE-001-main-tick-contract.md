# Handoff — R1-SOURCE-001 C++ 主 tick 合同与 Unity Crosswalk

> 完成日期：2026-08-21  
> Work Package：R1-SOURCE-001  
> 状态：COMPLETED（仅静态主 tick source contract）。  
> C++ authority：J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick(...)。  
> 本次没有启动 C++ executable、没有修改 Unity/C++ gameplay、没有运行 Unity/trace/Play Mode。

## 完成内容

- 已确认 Makefile 将 game_tick.cpp、frame advance、physics、collision、hit、weapon、cpoint、input 与 renderer 源文件构建到 ntsd_new.exe。
- 已建立 19 个 C++ 主 tick checkpoint：tick header、cooldown/input、特殊维护、frame logic/advance、death/respawn、两轮 Z/held、candidate/character/random/object、CPoint/weapon sync/positive-link、preframe/render、postprocess/late/tail。
- 已建立 Unity NTSDBattleTickSystem 的 30 段 static pass map。
- 已新增 12 条初始差异或待盘点条目，重点是：
  - D-SCHED-001：Unity CPoint/WeaponSync 在 candidate collect 前，而 C++ 在 object collision 后；
  - D-SCHED-002：Unity positive-link validation 在 candidate 前，而 C++ 在 CPoint/weapon sync 后；
  - D-SCHED-003：Unity Held step12 在 candidate 前，而 C++ 的第二轮 held 位于 CPoint/positive-link 后；
  - D-SCHED-004：需确认 Unity 是否保留 C++ 第一轮 negative-link held loop；
  - D-SCHED-005～012：输入 gate、Z 写回、candidate adapter、camera/render、tail 和容量语义均已登记为待后续模块闭合。

## 交付物

- docs/ai/RESEARCH/R1-SOURCE-001-main-tick-contract.md
- docs/ai/RESEARCH/R1-SOURCE-001-unity-crosswalk-and-diff-inventory.md
- docs/ai/TASKS/R1-SOURCE-001-cpp-source-contract-and-unity-diff-inventory.md

## 证据等级与限制

- C++ pass 顺序和 source 坐标：VERIFIED（source）。
- Unity 当前 pass 位置：VERIFIED（source）。
- 上述 D-SCHED 条目中的“顺序不同”：静态 source 已确认；尚未获得 C++ runtime trace 或 Play Mode 行为证据。
- R1-WP02 full C++ trace 保持 BLOCKED；不影响后续 source inventory，但不能被写成已解除。

## 不得直接做的事

- 不得据此立即移动 CPoint、WeaponSync、HeldObjectProcess 或 ValidateHeldLinksAll。
- 不得启用 Legacy SpriteRenderer、降低 MobileExtended 容量、取消 DesktopExtended dynamic growth 或改动 CentralOnly 来规避差异。
- 不得把静态顺序结论宣称为运行时已验证。

## 推荐下一步

R1-SOURCE-002：只读闭合 post_cooldown_input、human/AI/组合键、NeedClearInput 与 F1/F2 gate。它是 D-SCHED-005 和 D-SCHED-010 的前置；完成前仍不允许开始 R2 脚本改动。
