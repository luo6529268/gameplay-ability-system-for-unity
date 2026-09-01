# BATTLE-SPRITE-GRID-SEPARATOR-001 — Runtime BMP green gutter task

> 日期：2026-08-31  
> 状态：`VERIFIED / COMPILE_0 / FOCUSED_29_29_PASS / LIVE_GREEN_SCAN_0 / PRESENTATION_ONLY`  
> Change ID：`BATTLE-SPRITE-GRID-SEPARATOR-001`

## Goal

只清除 BMP sheet 中高覆盖率的 1px 绿色网格分隔带，消除中央渲染角色脚下绿线，同时保留角色内容中的局部合法绿色，并保证同一 BMP 不因 DAT 引用方不同而生成不同图集 source。

## Acceptance

- compile 0 error；真实 Naruto/Sasuke BMP focused tests 通过。
- Play Mode 延迟生成角色后，脚下不再出现纯绿色横线。
- 不改变 Sprite Rect/pivot、HP UI、Scene 和战斗规则。

## Result

- compile 0 error；真实 BMP、图集和资源回归 29/29 PASS。
- 延迟生成角色后的 GameView 全图无长度 >=8 的 chroma-green 横线；两名角色邻域匹配绿色像素均为 0。
- 证据：`Temp/BATTLE-SPRITE-GRID-SEPARATOR-001/fixed-game.png`。
