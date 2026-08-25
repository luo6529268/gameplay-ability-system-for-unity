# HANDOFF — R4-COL-05B non-character attacker / weapon kind1 reachability

> 日期：2026-08-22  
> 状态：`UNKNOWN / NO GAMEPLAY CHANGE`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入 authority。  

## 本次完成的只读事实

- C++ `main.cpp:5505-5523` 的正式 post-cooldown callback 只为 active current `obj_type == 0`
  character DAT 调用 AI input 和 `apply_input`；
- C++ kind1 selector读 generic attacker `key_right/key_left`，但 kind1 consume 是 generic Entity
  `case 1` grab；C++ pickup 由 kind2/7处理；
- Unity 的 non-character key helper当前只读 `LF2Character`，weapon kind1当前进入 pickup helper；
- `data.txt` 有 non-character entries，但实际 VDC-encoded `chars/*.dat` 在本次不运行/不解码 authority
  的边界下不可静态证明 `itr kind:1` 可达。

## 结论与边界

`D-COL-005B` **未完成、未实施、未验证**。它是资产可达性与 non-character key producer 的
`UNKNOWN`，不是理由不足却继续修改 weapon logic 的任务。05A 的 kind1 target writer 修复仍维持
`RUNTIME_PENDING`，但不得据此宣称 weapon kind1 已对齐。

## 允许的未来 reopen evidence

1. 现有、可重复、只读的 C++ release slot/frame/key/candidate/consume 观察；
2. 可证明与 release loader 一致的只读 DAT asset evidence，并闭合 key producer；
3. 用户给出的 C++ release 最短场景复现。

任何 reopen 都必须新建独立 Task Contract / Change Record，不能修改本包或复用05A Record。

## 连续下一步

按 D-009，直接开始 `D-HIT-001` 的只读 source preflight：先闭合 C++ type3 normal damage 的
HP/HP-max/combo/damage-stat 写入与 Unity `ApplySpecialAttackDamage` 的对应差异，再决定是否建立
最小 implementation contract。不得因为05B的 UNKNOWN 停止 R4 主线。
