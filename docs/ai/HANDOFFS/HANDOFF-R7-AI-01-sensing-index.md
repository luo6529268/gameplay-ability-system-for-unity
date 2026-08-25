# HANDOFF — R7-AI-01 sensing / indexed target

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-PRODUCTION-CODE CERTIFICATION`

## Current

C++ `prepare_ai_input` sensing前半段与Unity fallback/SoA/indexed路径已逐段映射；未发现新的production
source-confirmed difference。fresh exact test 2/2、AI sensing/profile 111/111、21:04:52 full self-check及
validator/diff均PASS。发现的一条dead-AI旧测试合同已由`R7-AI-TEST-001 / VERIFIED / TEST-ONLY`修正，
production AI未改。

## Conditional boundary

本证书只覆盖move-mode、ground/air target、cache前半段、team summary与special scan。它不是完整AI
decision证书，也不是C++ runtime/Play Mode证书。Unity >399 slot感知仍是批准的capacity adapter语义。

## Next

`R7-AI-02`必须从C++ `input_handler.cpp:1900+`开始，逐段复核OID-specific decision、RNG和input edge，
映射`AiDecisionKernel`与canonical store direct writer。发现差异先登记/建Record，不能直接改production。

