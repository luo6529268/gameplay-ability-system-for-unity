# NTSD28-R4-HIT02A-FLUTE-STAT-SENTINEL-FIXTURE-CORRECTION-001 — Task Contract

> Goal5 Part A，2026-09-09，事前建立，VERIFIED / TEST_FIXTURE_ONLY / FOUR_CASES_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_LATER_RESPAWN_STALE_INT / GOAL6_USER_HOLD。

## 依据与范围

直接采用用户/GLM独立复核结论：R4-HIT-02A唯一失败为holder.ComboCountAtk旧41期望。
退休包NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001已VERIFIED，
明确退休flute holder legacy统计，保留WeaponCount=-20/frame/motion/sound。
Authority battle_world.cpp::BattleWorld28::resolve_special_relation_hit，5453–5457字符分支
只写impact action，不写Unity legacy统计；当前EXE B1E13AE1/closure39DDDA15不变。

只修改BattleRuntimeSelfCheck.CheckKind10And11CharacterStatsWithoutDamage：
命中前const preservedComboCount=55并赋给holder.ComboCountAtk；原41期望改为该sentinel；
消息补holderComboCountAtk、expectedComboCountAtk和DamageStats[1]。
WeaponCount/HP/HPBound/DamageStats/frame/PN/attacking/wait/Frame.D条件全部原样保留。
不补两跳owner_slot源链，不宣称完整kind10对齐。

精确文件：Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs（仅上述方法）、本Task/Record、
docs/ai/CHANGE-LEDGER.md、docs/ai/STATE.md、Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md、
Temp结果。其他任何文件不改；Part B独立Record。

## 验收与停止

已有RED是Goal3 12:04:37Z R4-HIT-02A FAIL及用户确认的唯一失配，不重复Authority审计。
新鲜full SelfCheck必须越过actual/shared × tick12/13四组；后续停点只记录不修。
两套Assembly build0 error、原始validator PASS，Scene SHA保持
D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
按指定instance核验2022.3.62f3/NTSD_Battle，不启动Editor、不进Play、不清Console。
若R4-HIT-02A仍失败、需要production修改或diff越界，立即停止。
VERIFIED只关闭期望修正；完成后等待Goal6。

回滚须用户批准，仅逆向本包增量，不能整文件restore或回退退休包。无资源/schema/lifecycle改动。


## 最终结果

55 sentinel/expectation/message only. Fresh SelfCheck12:41:25Z passed four actual/shared x tick12/13 cases and stopped later at respawn stale-int:28849. Runtime47/Editor104 warnings, both0 errors; specified Scene SHA unchanged. Fixture-only VERIFIED.
完整证据与实际命令见同ID Record；停止等待Goal6。
