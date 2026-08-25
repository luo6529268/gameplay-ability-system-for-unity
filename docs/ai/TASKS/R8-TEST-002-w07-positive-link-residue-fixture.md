# R8-TEST-002 — W07 positive-link residue fixture sync

> 日期：2026-08-23
> 状态：`VERIFIED / TEST-ONLY`
> Change ID：`R8-TEST-002`

## Goal

把W07 structural witness同步到R5-LINK-001已经由C++ authority确认的合同：invalid positive relation只清
holder `LinkState`，保留holder `TargetSlotIndex/HeldWeaponStableId`和target reverse fields。

## Evidence

- full EditMode job `246be3d87338446ea7a877b13f7f88f5`：1357项，仅W07一项失败；
- fresh exact job `f453a20619b34ef0afe3716a902d7629`：1/1 FAILED，排除顺序污染；
- production `SimulationQueryAndLinkModule`与data-oriented pass已经由R5-LINK-001移除extra forward-field clears；
- W07 fixture仍要求holder target/held变为-1，因此在返回event buffer前主动抛错。

## Authority

C++ release `src/entity/game_tick.cpp:1828-1845`：positive link不匹配时只写link=0；不写forward/reverse
residue。R5-LINK-001 focused 8/8、compile/self-check已通过；本包不重新裁决production。

## Scope

- `Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs`：W07 postcondition期待holder `0/1/1`，target
  reverse仍`2/0`；
- `Assets/NTSD/Scripts/Test/Editor/BattleParityStructuralWitnessEditorTests.cs`：cleared event期待
  `AfterLinkState=0`且`AfterTargetSlot/AfterHeldWeaponSlot=1`；方法名同步为“只清link state并保留relation fields”；
- production文件不改。

## Acceptance

1. W07 exact PASS；
2. structural witness class PASS；
3. full EditMode 1357/1357 PASS；
4. same-domain及fresh full self-check PASS；
5. compile、validator、diff check PASS。

## Stop conditions / out of scope

- actual target reverse fields变化、event sink不一致或出现其他mask；
- 需要改production link writer/pass；
- Play Mode、C++ trace、held/negative-link其他合同均不属于本包。
