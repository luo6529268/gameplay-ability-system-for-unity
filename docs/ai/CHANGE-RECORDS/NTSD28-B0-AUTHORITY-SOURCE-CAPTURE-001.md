# NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001 — 只读 authority source capture

<!-- CHANGE-RECORD
id: NTSD28-B0-AUTHORITY-SOURCE-CAPTURE-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/AuthorityCaptureValidator.cs
code-path: Tools/NTSD28Parity/Program.cs
code-path: Tools/NTSD28Parity/TraceComparator.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
authority: User-approved NTSD28-UNITY-BATTLE-REALIGNMENT-001 B0; fixed-SHA formal NTSD2.8-Logan.exe; corresponding playable source build closure and existing ScenarioLoader/trace_json28 discovery.
evidence: SOURCE-MODEL-CAPTURE-READY / FORMAL-EXE-SHA-1277B70B-VERIFIED / AUTHORITY-SOURCE-MANIFEST-C59BD8D3 / RUNNER-SOURCE-SHA-46B3C4CE / CAPTURE-BINARY-SHA-A731838D / CPP-BUILD-0-WARN-0-ERROR / REAL-CAPTURE-3-TICKS-6-ENTITY-SNAPSHOTS-VALID / DOTNET-BUILD-0-WARN-0-ERROR / SELF-TEST-21-OF-21-PASS / AUTHORITY-OUTPUT-GUARD-PASS / GLOBAL-LEDGER-PASS / CERTIFICATE-ELIGIBLE-FALSE / NO-AUTHORITY-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / SOURCE-MODEL-CAPTURE-READY / UNITY-EXPORTER-NEXT`

## 1. 原状

- authority source 有旧 parity runner，但没有发现预编译 runner/LFR fixture。
- 旧输出 schema 与 v2 47-field contract 不兼容。
- ScenarioLoader legacy hash 与当前正式 EXE SHA 不同，不能合并成一个含糊的 `authority` 身份。

## 2. 计划实现

- workspace-owned C++ raw capture runner，直接读取未修改的 `EntityState28`。
- workspace-owned build script，验证正式 EXE SHA、计算 source manifest、禁止 authority 内输出。
- C# raw capture validator，复用 v2 entity exact-property/type contract。
- neutral two-entity scenario 与真实运行证据。

## 3. 不可回退边界

- 不写 authority，不调用 authority build script 的 authority-local build 目录。
- 不把 source-model capture 标记为 formal EXE runtime trace 或 parity certificate。
- 不填充 Unity 数据，不静默忽略 47 字段中的 candidate/missing binding。
- 不修改 Unity runtime、Scene、DAT、资源或旧 parity 工具。

## 4. 实际改动与验证

- 新建 workspace-owned C++ runner，读取未修改 `EntityState28` 并输出 v2 contract 对应的全部
  47 个核心实体字段；输出标记 `SOURCE_MODEL_DIAGNOSTIC_ONLY`。
- build 脚本在任何创建目录前拒绝 authority 内 output，并核验正式 EXE SHA；实际 guard probe
  `before=false / rejected / after=false`。
- build/output 均位于本仓库 `Temp/NTSD28AuthorityTrace`；authority build script 与 authority
  目录未写入。
- 首次 C++ build 成功但有 1 条 `wchar_t < 0` 恒假 warning；单行修正后最终
  `-Wall -Wextra -Wpedantic / 0 warning / 0 error`。
- authority source manifest 两次稳定：
  `C59BD8D3264B5CBF15EDBCFE2BAE64BC0F3BBC41926BEF6A4723EC2F571F2D75`。
- runner source SHA：
  `46B3C4CE5E6C124AB343F3EAFBAC2D6E8F7FBB207152B964CCF3769DB77476C7`。
- 两次链接 binary SHA 不稳定，因此 raw header 新增本次实际 binary SHA；最终 capture 使用
  `A731838D9C9F408C283A5CC27B9BCE5792A51D4D08CA59214E219359DA817618`，未把非确定 binary
  错写为稳定源码身份。
- neutral scenario 实际运行：`3 completed ticks / 6 entity snapshots`；raw validator
  `valid-source-model-capture / certificateEligible=false`。
- C# build：0 warning / 0 error；self-test：21/21，含 valid/truncated/extra/formal-SHA-drift。
- C/C++ Ledger coverage 由独立 `CHANGE-LEDGER-CPP-COVERAGE-001` 建立；全局 validator
  `67 records / 9 governed diffs / PASS`。
- 未修改 Unity runtime、Scene、DAT、资源或 authority；本结果不是 formal EXE runtime trace，
  也不是 parity certificate。
