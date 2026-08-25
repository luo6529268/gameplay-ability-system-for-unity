# R7-AI-01 — AI sensing / index conditional certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-PRODUCTION-CODE CERTIFICATION`

## Goal

重新认证 C++ `prepare_ai_input(...)` 的target sensing、cache、move-mode与special scan前半段，
以及Unity fallback/SoA/indexed adapter，不扩张到完整AI decision tree。

## Scope

- C++ `input_handler.cpp:1209-1235,1615-1898`；
- Unity `AiSensingKernel`、unified snapshot role/special indexes、cached target前半段；
- production `DataOrientedCanonical` profile接线；
- fallback/indexed差分、RNG、input facts和warmed allocation测试。

## Authority / Evidence

- C++ release source / Makefile；
- `RESEARCH/R7-AI-01-sensing-index-recertification-20260822.md`；
- jobs `8c74d8e0a76e427fac3fd7920f5ac234`、`5c6bad85dc0b43c2a6949d03cfd256fc`；
- full self-check 2026-08-22 21:04:52 PASS；
- `R7-AI-TEST-001` test-only correction。

## Deliverables

- source crosswalk；
- unknown/reopen conditions；
- focused test证据；
- register / STATE / plan / handoff同步。

## Verification

- source mapping完成；
- exact correction 2/2；AI sensing/profile 111/111；
- fresh Editor compile / Console error 0；
- full self-check与validator/diff PASS。

## Stop conditions

- first mismatch进入post-special完整decision tree；
- 需要修改production AI；
- 需要改变profile、RNG、pass order、capacity adapter或C++。

## Out of scope

`input_handler.cpp:1900+`完整decision、Play Mode、C++ runtime trace、1000 AI性能、R8、T8、服务器。

