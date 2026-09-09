# NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001 — typed armor definition contract

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CONTRACT
code-path: Assets/NTSD/Scripts/Animation/LF2ArmorData.cs
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorDataContractEditorTests.cs
authority: NTSD 2.8-Logan combat_records.h ArmorRecord28 and combat_records.cpp CombatRecordDecoder28::armor; EXE B1E13AE1, closure 39DDDA15.
evidence: red 8f8b645f2e824f91bf1288a627dbad54; focused 7a9b60b9066042de9398d89d054d023f 7/7; B5 ae81f98ddb4545dab2353a0493ef8deb 279/279; broad 719a11c099ea49b1be86120731d4b4f4 755/755; SelfCheck PASS 2026-09-06T00:19:49Z; Console no C# error; Scene unchanged; Ledger PASS.
-->

> 状态：`VERIFIED / DATA_CONTRACT_READY / PRODUCTION_SELECTION_UNCONNECTED`

## 原状与边界

Unity parser AST保留`<armor>`为generic `Lf2DatBlock`，但原parser只为catch action保留双整数值，导致
Authority `frame: first last`的第二个整数丢失；`LF2CharacterData`与正式loader也没有typed转换；
`RuntimeArmorHp118/ArmorRecoveryTimer11C`只是运行时载体。当前Unity 138-DAT无armor block，Authority有18个。

本包只建立定义数据、copy与fingerprint并接正式loader；不实现selection/activation、runtime初始化或content部署。
回滚删除新model/test及meta，并移除CharacterData/converter/manager映射。

## 验收状态

实际修改：新增`LF2ArmorData/LF2ArmorFrameRange`完整字段、深拷贝和allocation-free fingerprint；
`LF2CharacterData.armors`承载有序定义；parser为数值型`frame`保留第二整数；converter实现scalar last-win、
`type`优先/`ptype`fallback、重复list/range/sound解析和stale clear；正式loader已调用typed转换。

验证：red `8f8b645f2e824f91bf1288a627dbad54`；首次focused
`ca01b7bfdcf345c083873185daa4aad6`暴露并修正frame双整数与测试checksum问题；最终focused
`7a9b60b9066042de9398d89d054d023f` 7/7、B5 `ae81f98ddb4545dab2353a0493ef8deb` 279/279、
broad `719a11c099ea49b1be86120731d4b4f4` 755/755、SelfCheck 00:19:49Z PASS。Console只有7条
SelfCheck故意触发的rest-binding拒绝日志，无C#编译错误；Scene hash/length/mtime均未变化。

边界：定义数据生产链已就绪；selection/activation/runtime HP/recovery仍未接通，正式content继续受H/B11门约束。
