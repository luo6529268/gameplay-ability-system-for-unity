# HANDOFF — R8-WP01G-R06 P1/P2 physical input runtime

> 日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED`

## Preflight result

- G2已拆分；`D-INP-001`的自然current-type0 negative-link producer是opoint kind2 AI child，按用户AI范围决定
  不再作为非AI Play backlog，现有source-correct eligibility代码保留；
- `D-INP-004`发现production first difference：C++ P2拥有方向键+numpad3/1/2完整输入，Unity `Player_2`
  action map只有Move，导致P2三个action lookup为null；
- packet后roster routing self-check并不能覆盖physical source缺失；
- R06已补齐Player_2 physical source，并取得two-human正式输入链Play证据。

## Implemented result

- Player_2新增Attack/Jump/Defend，exact numpad1/2/3 binding；wrapper由Unity正规生成；
- crossed adapter保持：numpad1→canonical Jump、numpad2→Defend、numpad3→Attack；
- focused 2/2和input regression 47/47 PASS；
- two-human Play 11/11 press/held/release/no-cross PASS，slot0/1与stable100/101保持；
- full self-check 19:37:29 PASS，Play结束前Console error0，ledger81/96与scoped diff-check PASS；
- 证据见`RESEARCH/R8-WP01G-R06-p1p2-physical-input-runtime-evidence-20260823.md`。

## Protected boundaries

C++只读；P1 crossed mapping、8-slot extension、30Hz、FrameInputSet、worker、SoA/ECS、pool/0GC、CentralOnly、
T8、IL2CPP、Android、服务器均保持。

## Resume checkpoint

- R06不需要继续修改；下一包必须按总计划与用户批准单独启动；
- R07A/R07B/R07C和R08仍为`PLANNED / APPROVAL PENDING / NO EXECUTION`；
- R1-WP02 C++ full trace仍BLOCKED；D-INP-006人手硬件edge仍由用户验收。
