# HANDOFF — R7-AI-02 character decision chain inventory

> 日期：2026-08-22  
> 状态：`SOURCE-CONFIRMED DIFFERENCE / INVENTORIED / NO GAMEPLAY CHANGE`

## Current

C++ outer random gate内有39个有序helper/call positions；Unity Legacy与DataOriented只保留1–6，
缺失7–27与29–37共30 positions，并把现有28/38/39放到了gate外。optimized snapshot另缺OID11
frame290 side-effect所需的current frame `hit_j`。现有decision job 75/75 PASS只证明Unity共享缩减oracle，
不关闭C++差异。

## Registered

- `D-INP-007A` missing 30 positions；
- `D-INP-007B` three existing helpers outside gate；
- `D-INP-008` HitJ data contract；
- `D-INP-009` circular-oracle acceptance gap。

## Next

先执行`R7-AI-02A`测试/dispatcher合同；随后02B数据合同、02C～02E分组helper，最后02F统一接线。
02F前不得把部分helper激活到production默认链；脚本修改前必须分别建立Change Record。

## Stop

不得借本inventory直接整体重写AI、改C++、改profile/RNG/pass/capacity或宣称AI已完全对齐。

