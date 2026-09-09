# Task Contract — NTSD28-B3-AI-RENDER-PHASE-CONSUMER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CONSUMER_CROSSWALK_COMPLETE`

## 目标

只读闭合 current Authority 中 AI 对 render phase 与真实position.y的全部相关consumer，形成可安全实施的精确清单；不修改脚本或运行行为。

## 允许

读取当前Authority `native_ai.cpp`、Unity AI snapshot/kernel/runtime和既有B2测试/记录；新增本Task/Record/manifest并更新恢复文档。

## 禁止

不修改C#、Scene、Prefab、Config、资源、Authority，不把所有Y读取机械替换为HitStop。

## 验收

Authority render-phase消费者、Unity错误consumer、已有HitStop传播链及必须保留的物理Y consumer全部列明；下一实现包范围、test-first矩阵与RNG不变量明确。
