# Frame-motion tail source witness

SOURCE12_TABLE61_PASS; package remains IN_PROGRESS. This is actual playable-source API diagnostic evidence, not formal EXE capture, Unity parity, or a full-tick certificate.

Build: Build-AuthoritySourceCapture.ps1 -RunnerSource Tools/NTSD28AuthorityTrace/frame_motion_tail_witness.cpp -OutputDirectory artifacts/diagnostics/NTSD28-Q06-FRAME-MOTION-TAIL-001/source -ExecutableName frame_motion_tail_witness.exe. Exit 0; log Logs/Q06-Frame-Motion-Tail-Build.log. Source manifest and formal EXE identity match CURRENT-AUTHORITY; see source/build-manifest.json.

Executed the diagnostic twice, outputs byte-identical, SHA256 2AD148A4AA3B0241DD866957C51189A7AC20A50C17F9AE0D9579D66B973F4C2F. Independent explicit expected table: 12 rows, 61 assertions PASS; source/validation.json. Assertions include whole position/motion records, not 61 individual scalar fields.

Confirmed: own velocity reads integers; fractional dv values do not become own fractional velocity. Positive delay quarters accumulated motion; negative delay does not. Nonzero float positional axes clear corresponding velocity, start from integer coordinates and round midpoint to even; facing changes X only. Linked motion precedes own displacement. Own Y displacement preserves linked collision reference. Pending lifecycle skips all; missing platform reports failure but still executes own positional displacement.

Changed only two declared diagnostic scripts. No Unity production/test edit or Unity run in this stage. Next: declare exact Unity RED fixture path, compare the same source initial/after states via existing World/factory and production entry; then declare and implement missing tail. Reuse unchanged platform evidence; batch integration/replay/Renderer/closure remain pending. Preserve prior two late-update test failures and Destroy-pool Renderer closure follow-up. Q06 incomplete; Q07 migration not started.
