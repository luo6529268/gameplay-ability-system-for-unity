# R3-COMBO-01 — staggered combo-state persistence preflight

> 日期：2026-08-23
> 状态：`SOURCE-CONFIRMED DIFFERENCE / NO SCRIPT CHANGE`

## Authority closure

- release build：`J:\QQFile\NTSD2.4\ntsd_release\Makefile:35`包含
  `src/input/input_handler.cpp`；
- physical mapping：`include/input_handler.h:9-16`定义field order
  `attack(+D3), jump(+D1), defend(+D2)`，P1配置实际为L/J/K；因此Unity把物理J/K/L交叉到
  internal jump/defend/attack字段是正确适配，不是差异；
- poll：`input_handler.cpp:1555-1609`每game tick写current/prev并生成cd/history edge；
- combo：`input_handler.cpp:2758-2859`的`run_combo/advance_combo`直接接收
  `e.combo_DRA...e.combo_DJA`引用。任何step1/step2/step3、interrupt、successful trigger或DJA guard/early
  branch对combo字段的修改都已即时写入entity，不存在“先复制九字段，只有统一tail才commit”的事务。

## Unity first difference

`BattleCharacterInputActionResolver.ApplyComboFrameInput`：

1. 把九个`input.Combo*`复制到局部byte；
2. 普通wrapper与DJA只修改局部值；
3. 当`comboDja != 3`时立即`return result`，九个局部值全部丢失；
4. valid DJA frame jump、oid6 guard、missing target和Unk328分支也在统一assign-back前return；
5. 只有极窄的DJA fallthrough路径到达末尾的九字段assign-back。

因此，跨逻辑tick的L→方向→J/K组合不会保留step1/step2状态；同tick把三键一起送入则可能通过。这与
用户“按钮组合无法释放技能”的运行时现象一致，并且是独立于物理action map和worker的更早确定差异。

## Stale tests that encode the defect

- `BattleRuntimeSelfCheck.CheckComboLocalShadowCommitContracts`明确要求incomplete/DJA early-return丢弃所有
  local combo progress；该期待来自旧Unity/C# transactional shadow，不是C++ release；
- `CheckStaggeredNarutoDefendDownJumpInput`明确要求分tick物理L/S/K不能完成DDJ；这与C++ by-reference
  wrapper状态持久化相反；
- same-tick combined tests仍能通过，所以full self-check没有发现真实跨tick失败。

这些断言必须按新Change ID作correction/supersede，不能让production继续迎合陈旧测试。

## Minimal repair direction

- `ApplyComboFrameInput`直接对`ref input.ComboDra...ref input.ComboDja`执行wrapper；
- 删除local nine-field transaction和统一assign-back依赖；
- 保留既有wrapper顺序、frame refresh、cooldown clear、DJA guard、Unk328与direct-hit后续顺序；
- focused matrix至少覆盖九combo的1→2→3跨tick、interrupt、missing target、valid target、oid6 guard、Unk328、
  same-tick兼容与Naruto real-DAT L/S/K；
- 不修改physical binding、FrameInputSet、worker、DAT、opoint或技能专项代码。

## Evidence limits

这是C++ release source与Unity source的确定差异，尚未修改代码、编译或运行修复后测试；R1-WP02 full
trace仍BLOCKED，所以完成后最高先到source+Unity focused/joint/Play Mode证据，不能写成C++ runtime trace verified。

