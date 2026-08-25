# HANDOFF — R7-AI-02F full dispatcher integration

> 日期：2026-08-23
> 状态：`RUNTIME_PENDING / CODE + AUTOMATED EVIDENCE CLOSED`

## Current

Legacy与DataOriented已原子接入C++ source-derived outer-gated positions1–39；gate外28/38/39重复链已删除。
matched-position、shared/fallback rows、RNG trace、extended-slot adapter与input side-effect提交均已接通。
fixed-seed production profile-pair 1/1、final AI matrix 286/286、Unity compile 0 error、warmed full dispatcher
0 B、02:07:58 final fresh-domain full self-check PASS。同domain suite后02:03:05曾复现已登记D-TEST-001污染，
domain reload后恢复PASS。

## Protected boundaries

- C++ authority只读；
- Legacy/DataOriented必须原子完成，禁止部分默认接线；
- positions28/38/39只能从gate外移入完整chain，禁止双执行；
- common tail、input edges/cooldown、pass、render、capacity总体策略不改。

## Next

02F自动证据已闭合；后续按R7 repair sequence进入下一个独立Work Package。02F真实角色Play Mode与
C++ runtime trace保留到R8/R1-WP02，不得因自动测试通过宣称完整AI或battle已对齐。

## Stop

若后续Play Mode或可用C++ trace出现first difference，回到02F matched-position/RNG witness定位；不得用角色专项
补丁绕过dispatcher顺序，也不得恢复gate外28/38/39。
