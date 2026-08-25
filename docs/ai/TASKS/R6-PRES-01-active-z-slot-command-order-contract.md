# R6-PRES-01 — active / Z / slot / command order 合同

> 状态：`RUNTIME_PENDING`  
> 类型：no-code certification  
> 日期：2026-08-22

## Goal

认证C++ Release `render_world` active collection、stable Z painter order和per-entity绘制顺序
在Unity CentralOnly command/index/segment链中保持，不修改production renderer。

## Scope

- 只读C++/Unity source；
- 复用并运行existing presentation order/command writer tests；
- 更新差异登记、STATE、main plan、handoff。

## Required behavior

1. active slot升序输入；
2. signed Z升序；
3. same Z runtime slot升序；
4. shadow→body→overlay→hit-record；
5. indexed order与fallback order等价；
6. dynamic mesh不跨原command流重排；
7. CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5 scale、fixed-world camera与扩展容量保持。

## Verification

- source/mapping闭合；
- BeginFrame order class既有job通过；
- command writer job `5561fce764bc4baa8804ae37ca929417` 6/6 PASS；
- 17:49:18 full self-check PASS；
- governance validator与scoped diff check通过。

## Stop / Out of scope

任何需要修改shader/renderer/order/data contract的发现必须另建Change Record。本包不处理resource fail-closed、
spark lifecycle、shadow OID、EntityVisible/ShadowVisible、camera/perspective或GPU最终像素。
