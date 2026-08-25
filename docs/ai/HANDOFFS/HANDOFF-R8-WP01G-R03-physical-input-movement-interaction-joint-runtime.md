# HANDOFF — R8-WP01G-R03 joint runtime certification

> 日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / REPEATABLE CURRENT REPORTS / C++ FULL TRACE BLOCKED`

## Scope

只执行真实物理输入→固定tick→movement/landing→interaction/hit联合运行时认证。C++ Release只读；
不处理T8、IL2CPP、Android、服务器、F1/F2 debug、性能架构或受保护Unity adapter。

## Current checkpoint

- R02四项已处理到`RUNTIME_PENDING`并停止；
- 用户已批准R03并恢复总目标；
- 工作树很脏，全部既有修改按用户工作保留；
- 现有`BattleComboPlayModeProbeEditor`已通过Input System queue走真实CharacterInputModule/
  FrameInputSet路径，可验证DDJ与DRA/DLA；
- held/grab/cpoint/collision/hit现有Play probes可复用；
- 尚未确认是否有完整movement/jump/landing end-to-end probe；若缺失，必须先建test-only Change Record。

## Active changes

`R8-JOINTINPUT-PROBE-002 / VERIFIED / TEST-ONLY`：fresh重跑DDJ和F2时一次性L/D事件均未进入
FrameInputSet；现以最多8次release→press物理脉冲修正采样可重复性。current fresh F2 attempt2/1、
DDJ attempt1/1/1、DRA attempt1/1/1均PASS；不改production。final focused257/257、17:33:15 self-check、
Console0、79/93 validator PASS。

`R8-JOINTMOVE-PROBE-001 / VERIFIED / TEST-ONLY`：Editor-only D/K movement→jump→landing逐tick探针
fresh compile0并在真实Play通过。tick1080 Right、1084 physical K/Defend、1088 airborne、1091 release、1108 landing；
DAT Vx8/Vy-16.3，首个airborne Vx7/Vy-14.6，对象数8→8。未改production；full self-check/最终治理待。

## Next action

R03 current F1/F2/F3报告、compile、focused、self-check和治理均已收口；没有production first difference。
本包完成并停止。下一R3+工作包仍需按总计划确定范围并取得用户批准。
